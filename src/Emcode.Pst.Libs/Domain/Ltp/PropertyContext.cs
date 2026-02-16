using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Property Context (PC) yang membaca properti dari Heap-on-Node.
/// </summary>
internal sealed class PropertyContext
{
    private readonly HeapOnNode _heap;
    private readonly SubnodeReader _subnodes;
    private readonly Dictionary<ushort, PropertyRecord> _records = new();
    private byte _cbKey;
    private byte _cbEnt;

    /// <summary>
    /// Membuat PC dari heap dan subnode reader.
    /// </summary>
    /// <param name="heap">Heap-on-Node.</param>
    /// <param name="subnodes">Reader subnode untuk HNID berbasis NID.</param>
    public PropertyContext(HeapOnNode heap, SubnodeReader subnodes)
    {
        _heap = heap;
        _subnodes = subnodes;
        LoadRecords();
    }

    /// <summary>
    /// Mengambil nilai string berdasarkan property id.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <returns>Nilai string atau null.</returns>
    public string? GetString(ushort propertyId)
    {
        if (!_records.TryGetValue(propertyId, out var record))
        {
            return null;
        }

        if (!TryGetRawValue(record, out var data))
        {
            return null;
        }

        var span = data.Span;
        if (record.PropType == (ushort)PstPropertyType.String)
        {
            var text = Encoding.Unicode.GetString(span);
            return text.TrimEnd('\0');
        }

        if (record.PropType == (ushort)PstPropertyType.String8)
        {
            var text = Encoding.Latin1.GetString(span);
            return text.TrimEnd('\0');
        }

        return null;
    }

    /// <summary>
    /// Mengambil data biner berdasarkan property id.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <returns>Data biner atau null.</returns>
    public ReadOnlyMemory<byte>? GetBinary(ushort propertyId)
    {
        if (!_records.TryGetValue(propertyId, out var record))
        {
            return null;
        }

        if (record.PropType != (ushort)PstPropertyType.Binary)
        {
            return null;
        }

        if (!TryGetRawValue(record, out var data))
        {
            return null;
        }

        return data;
    }

    /// <summary>
    /// Mengambil nilai waktu berdasarkan property id.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <returns>Nilai waktu atau null.</returns>
    public DateTimeOffset? GetDateTime(ushort propertyId)
    {
        if (!_records.TryGetValue(propertyId, out var record))
        {
            return null;
        }

        if (record.PropType != (ushort)PstPropertyType.Time)
        {
            return null;
        }

        if (!TryGetRawValue(record, out var data) || data.Length < 8)
        {
            return null;
        }

        var fileTime = BitConverter.ToInt64(data.Span);
        return DateTimeOffset.FromFileTime(fileTime);
    }

    /// <summary>
    /// Mengambil nilai integer 32-bit berdasarkan property id.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <returns>Nilai integer atau null.</returns>
    public int? GetInt32(ushort propertyId)
    {
        if (!_records.TryGetValue(propertyId, out var record))
        {
            return null;
        }

        if (record.PropType != (ushort)PstPropertyType.Integer32)
        {
            return null;
        }

        if (!TryGetRawValue(record, out var data) || data.Length < 4)
        {
            return null;
        }

        return BitConverter.ToInt32(data.Span);
    }

    /// <summary>
    /// Mengambil nilai boolean berdasarkan property id.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <returns>Nilai boolean atau null.</returns>
    public bool? GetBoolean(ushort propertyId)
    {
        if (!_records.TryGetValue(propertyId, out var record))
        {
            return null;
        }

        if (record.PropType == (ushort)PstPropertyType.Boolean)
        {
            if (!TryGetRawValue(record, out var data) || data.Length < 2)
            {
                return null;
            }

            return BitConverter.ToInt16(data.Span) != 0;
        }

        if (record.PropType == (ushort)PstPropertyType.Integer32)
        {
            var intValue = GetInt32(propertyId);
            return intValue.HasValue ? intValue.Value != 0 : null;
        }

        return null;
    }

    /// <summary>
    /// Memuat record PC dari BTH.
    /// </summary>
    private void LoadRecords()
    {
        if (!_heap.UserRoot.IsValid)
        {
            return;
        }

        var header = _heap.ReadItem(_heap.UserRoot);
        if (header.Length < 8)
        {
            return;
        }

        var span = header.Span;
        var bType = span[0];
        _cbKey = span[1];
        _cbEnt = span[2];
        var bIdxLevels = span[3];
        var hidRoot = BitConverter.ToUInt32(span.Slice(4, 4));

        if (bType != 0xB5 || hidRoot == 0)
        {
            return;
        }

        CollectLeafRecords(new Hid(hidRoot), bIdxLevels);
    }

    /// <summary>
    /// Mengumpulkan leaf record PC dari BTH secara rekursif.
    /// </summary>
    /// <param name="hid">HID root untuk level saat ini.</param>
    /// <param name="level">Level index yang tersisa.</param>
    private void CollectLeafRecords(Hid hid, int level)
    {
        var data = _heap.ReadItem(hid).Span;
        if (level == 0)
        {
            ParseLeafRecords(data);
            return;
        }

        var recordSize = _cbKey + 4;
        if (recordSize == 0)
        {
            return;
        }

        var count = data.Length / recordSize;
        for (var i = 0; i < count; i++)
        {
            var offset = i * recordSize;
            var hidNext = BitConverter.ToUInt32(data.Slice(offset + _cbKey, 4));
            if (hidNext == 0)
            {
                continue;
            }

            CollectLeafRecords(new Hid(hidNext), level - 1);
        }
    }

    /// <summary>
    /// Mengurai leaf record PC.
    /// </summary>
    /// <param name="data">Buffer leaf records.</param>
    private void ParseLeafRecords(ReadOnlySpan<byte> data)
    {
        if (_cbKey != 2 || _cbEnt != 6)
        {
            return;
        }

        var recordSize = _cbKey + _cbEnt;
        if (recordSize == 0)
        {
            return;
        }

        var count = data.Length / recordSize;
        for (var i = 0; i < count; i++)
        {
            var offset = i * recordSize;
            var propId = BitConverter.ToUInt16(data.Slice(offset, 2));
            var propType = BitConverter.ToUInt16(data.Slice(offset + 2, 2));
            var valueHnid = BitConverter.ToUInt32(data.Slice(offset + 4, 4));
            _records[propId] = new PropertyRecord(propId, propType, valueHnid);
        }
    }

    /// <summary>
    /// Membaca data mentah dari record properti.
    /// </summary>
    /// <param name="record">Record properti.</param>
    /// <param name="data">Data mentah hasil.</param>
    /// <returns>True jika data berhasil diambil.</returns>
    private bool TryGetRawValue(PropertyRecord record, out ReadOnlyMemory<byte> data)
    {
        data = ReadOnlyMemory<byte>.Empty;
        if (!TryGetTypeInfo(record.PropType, out var typeInfo))
        {
            return false;
        }

        if (!typeInfo.IsVariable && typeInfo.Size <= 4)
        {
            var buffer = new byte[4];
            BitConverter.TryWriteBytes(buffer, record.ValueHnid);
            data = buffer.AsMemory(0, typeInfo.Size);
            return true;
        }

        var hnid = new Hnid(record.ValueHnid);
        if (hnid.IsHid)
        {
            var hid = hnid.ToHid();
            if (!hid.IsValid)
            {
                return false;
            }

            data = _heap.ReadItem(hid);
            return true;
        }

        if (_subnodes.TryGetSubnodeData(hnid.ToNid(), out var blocks, out var totalLength))
        {
            data = ConcatBlocks(blocks, totalLength);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Menggabungkan data dari beberapa blok menjadi satu buffer.
    /// </summary>
    /// <param name="blocks">Blok data.</param>
    /// <returns>Buffer gabungan.</returns>
    private static ReadOnlyMemory<byte> ConcatBlocks(IReadOnlyList<PstDataBlock> blocks)
    {
        return ConcatBlocks(blocks, 0);
    }

    /// <summary>
    /// Menggabungkan data dari beberapa blok dengan batas panjang tertentu.
    /// </summary>
    /// <param name="blocks">Blok data.</param>
    /// <param name="totalLength">Total panjang data berdasarkan lcbTotal.</param>
    /// <returns>Buffer gabungan.</returns>
    private static ReadOnlyMemory<byte> ConcatBlocks(IReadOnlyList<PstDataBlock> blocks, uint totalLength)
    {
        var total = 0;
        foreach (var block in blocks)
        {
            total += block.Data.Length;
        }

        var targetLength = totalLength > 0 && totalLength <= total ? (int)totalLength : total;
        var buffer = new byte[targetLength];
        var offset = 0;
        foreach (var block in blocks)
        {
            if (offset >= targetLength)
            {
                break;
            }

            var toCopy = Math.Min(block.Data.Length, targetLength - offset);
            block.Data.Slice(0, toCopy).CopyTo(buffer.AsMemory(offset, toCopy));
            offset += toCopy;
        }

        return buffer;
    }

    /// <summary>
    /// Mengambil info tipe properti dasar.
    /// </summary>
    /// <param name="propType">Tipe properti.</param>
    /// <param name="info">Info tipe.</param>
    /// <returns>True jika tipe dikenali.</returns>
    private static bool TryGetTypeInfo(ushort propType, out PropertyTypeInfo info)
    {
        switch ((PstPropertyType)propType)
        {
            case PstPropertyType.String:
            case PstPropertyType.String8:
            case PstPropertyType.Binary:
                info = new PropertyTypeInfo(isVariable: true, size: 0);
                return true;
            case PstPropertyType.Time:
                info = new PropertyTypeInfo(isVariable: false, size: 8);
                return true;
            case PstPropertyType.Integer32:
                info = new PropertyTypeInfo(isVariable: false, size: 4);
                return true;
            case PstPropertyType.Boolean:
                info = new PropertyTypeInfo(isVariable: false, size: 2);
                return true;
            default:
                info = default;
                return false;
        }
    }

    /// <summary>
    /// Record properti pada PC.
    /// </summary>
    private readonly struct PropertyRecord
    {
        /// <summary>
        /// Membuat record properti.
        /// </summary>
        /// <param name="propId">Property id.</param>
        /// <param name="propType">Property type.</param>
        /// <param name="valueHnid">Nilai HNID atau inline value.</param>
        public PropertyRecord(ushort propId, ushort propType, uint valueHnid)
        {
            PropId = propId;
            PropType = propType;
            ValueHnid = valueHnid;
        }

        /// <summary>
        /// Property id.
        /// </summary>
        public ushort PropId { get; }

        /// <summary>
        /// Property type.
        /// </summary>
        public ushort PropType { get; }

        /// <summary>
        /// Nilai HNID atau inline value.
        /// </summary>
        public uint ValueHnid { get; }
    }

    /// <summary>
    /// Informasi ukuran dan sifat tipe properti.
    /// </summary>
    private readonly struct PropertyTypeInfo
    {
        /// <summary>
        /// Membuat info tipe properti.
        /// </summary>
        /// <param name="isVariable">Menandakan tipe variable length.</param>
        /// <param name="size">Ukuran fixed length (bila ada).</param>
        public PropertyTypeInfo(bool isVariable, int size)
        {
            IsVariable = isVariable;
            Size = size;
        }

        /// <summary>
        /// Menandakan tipe variable length.
        /// </summary>
        public bool IsVariable { get; }

        /// <summary>
        /// Ukuran fixed length.
        /// </summary>
        public int Size { get; }
    }
}
