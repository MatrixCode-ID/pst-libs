using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Writer untuk membangun Property Context (PC) di atas Heap-on-Node.
/// </summary>
internal sealed class PropertyContextWriter
{
    private readonly List<PropertyEntry> _entries = new();
    private readonly LtpWriterOptions _options;
    private readonly List<LtpSubnodeData> _subnodes = new();
    private uint _nextSubnodeIndex = 1;

    /// <summary>
    /// Membuat writer Property Context.
    /// </summary>
    /// <param name="options">Opsi writer LTP.</param>
    public PropertyContextWriter(LtpWriterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Menambahkan nilai string Unicode.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="value">Nilai string.</param>
    public void SetString(ushort propertyId, string value)
    {
        _entries.Add(new PropertyEntry(propertyId, PstPropertyType.String, value));
    }

    /// <summary>
    /// Menambahkan nilai string ANSI.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="value">Nilai string.</param>
    public void SetString8(ushort propertyId, string value)
    {
        _entries.Add(new PropertyEntry(propertyId, PstPropertyType.String8, value));
    }

    /// <summary>
    /// Menambahkan nilai biner.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="value">Buffer biner.</param>
    public void SetBinary(ushort propertyId, ReadOnlyMemory<byte> value)
    {
        _entries.Add(new PropertyEntry(propertyId, PstPropertyType.Binary, value));
    }

    /// <summary>
    /// Menambahkan nilai integer 32-bit.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="value">Nilai integer.</param>
    public void SetInt32(ushort propertyId, int value)
    {
        _entries.Add(new PropertyEntry(propertyId, PstPropertyType.Integer32, value));
    }

    /// <summary>
    /// Menambahkan nilai boolean.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="value">Nilai boolean.</param>
    public void SetBoolean(ushort propertyId, bool value)
    {
        _entries.Add(new PropertyEntry(propertyId, PstPropertyType.Integer32, value ? 1 : 0));
    }

    /// <summary>
    /// Menambahkan nilai DateTimeOffset.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="value">Nilai waktu.</param>
    public void SetDateTime(ushort propertyId, DateTimeOffset value)
    {
        _entries.Add(new PropertyEntry(propertyId, PstPropertyType.Time, value));
    }

    /// <summary>
    /// Membangun Property Context menjadi blok heap.
    /// </summary>
    /// <returns>Daftar blok data heap.</returns>
    public IReadOnlyList<PstDataBlock> Build()
    {
        return BuildResult().Blocks;
    }

    /// <summary>
    /// Membangun Property Context beserta subnode untuk nilai besar.
    /// </summary>
    /// <returns>Hasil penulisan LTP.</returns>
    public LtpWriteResult BuildResult()
    {
        var heap = new LtpWriter.HeapWriter(_options);
        var records = new List<PropertyRecord>();

        foreach (var entry in _entries)
        {
            var record = new PropertyRecord(entry.PropertyId, entry.PropType, BuildValue(entry, heap));
            records.Add(record);
        }

        var leaf = BuildLeafRecords(records);
        var leafHid = heap.AddItem(leaf);
        var header = BuildHeader(leafHid.Raw);
        var headerHid = heap.AddItem(header);
        return new LtpWriteResult(heap.Build(headerHid), _subnodes);
    }

    /// <summary>
    /// Membangun Property Context secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar blok data heap.</returns>
    public Task<IReadOnlyList<PstDataBlock>> BuildAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Build());
    }

    /// <summary>
    /// Membangun Property Context beserta subnode secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    public Task<LtpWriteResult> BuildResultAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(BuildResult());
    }

    private static byte[] BuildHeader(uint leafHid)
    {
        var buffer = new byte[8];
        buffer[0] = 0xB5;
        buffer[1] = 2;
        buffer[2] = 6;
        buffer[3] = 0;
        BitConverter.TryWriteBytes(buffer.AsSpan(4, 4), leafHid);
        return buffer;
    }

    private static byte[] BuildLeafRecords(IReadOnlyList<PropertyRecord> records)
    {
        var buffer = new byte[records.Count * 8];
        var offset = 0;
        foreach (var record in records)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset, 2), record.PropertyId);
            BitConverter.TryWriteBytes(buffer.AsSpan(offset + 2, 2), (ushort)record.PropType);
            BitConverter.TryWriteBytes(buffer.AsSpan(offset + 4, 4), record.ValueHnid);
            offset += 8;
        }

        return buffer;
    }

    private uint BuildValue(PropertyEntry entry, LtpWriter.HeapWriter heap)
    {
        switch (entry.PropType)
        {
            case PstPropertyType.String:
                {
                    var text = Encoding.Unicode.GetBytes(entry.GetStringWithNull());
                    return AddValue(text, heap);
                }
            case PstPropertyType.String8:
                {
                    var text = Encoding.Latin1.GetBytes(entry.GetStringWithNull());
                    return AddValue(text, heap);
                }
            case PstPropertyType.Binary:
                {
                    var data = entry.GetBinary();
                    return AddValue(data.Span, heap);
                }
            case PstPropertyType.Time:
                {
                    var bytes = new byte[8];
                    BitConverter.TryWriteBytes(bytes, entry.GetDateTime().ToFileTime());
                    var hid = heap.AddItem(bytes);
                    return hid.Raw;
                }
            case PstPropertyType.Integer32:
                return unchecked((uint)entry.GetInt32());
            case PstPropertyType.Boolean:
                return entry.GetBoolean() ? 1u : 0u;
            default:
                throw new NotSupportedException($"Property type {entry.PropType} belum didukung.");
        }
    }

    /// <summary>
    /// Menambahkan data ke heap atau subnode berdasarkan ukuran.
    /// </summary>
    /// <param name="data">Data yang akan disimpan.</param>
    /// <param name="heap">Heap target.</param>
    /// <returns>HNID hasil (HID atau NID).</returns>
    private uint AddValue(ReadOnlySpan<byte> data, LtpWriter.HeapWriter heap)
    {
        if (data.Length > _options.MaxInlineValueBytes)
        {
            var nid = AllocateSubnode(data.ToArray());
            return nid.Value;
        }

        var hid = heap.AddItem(data);
        return hid.Raw;
    }

    /// <summary>
    /// Mengalokasikan subnode untuk data besar.
    /// </summary>
    /// <param name="data">Data subnode.</param>
    /// <returns>NID subnode.</returns>
    private Nid AllocateSubnode(ReadOnlyMemory<byte> data)
    {
        var nid = new Nid((_nextSubnodeIndex << 5) | (uint)NidType.Ltp);
        _nextSubnodeIndex++;
        _subnodes.Add(new LtpSubnodeData(nid, data));
        return nid;
    }

    /// <summary>
    /// Representasi entry properti sementara sebelum ditulis ke PC.
    /// </summary>
    private readonly struct PropertyEntry
    {
        /// <summary>
        /// Membuat entry properti.
        /// </summary>
        /// <param name="propertyId">Property id.</param>
        /// <param name="propType">Property type.</param>
        /// <param name="value">Nilai properti.</param>
        public PropertyEntry(ushort propertyId, PstPropertyType propType, object value)
        {
            PropertyId = propertyId;
            PropType = propType;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Property id.
        /// </summary>
        public ushort PropertyId { get; }

        /// <summary>
        /// Property type.
        /// </summary>
        public PstPropertyType PropType { get; }

        /// <summary>
        /// Nilai properti.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Mengambil nilai string dengan terminator null.
        /// </summary>
        public string GetStringWithNull()
        {
            return $"{(string)Value}\0";
        }

        /// <summary>
        /// Mengambil nilai biner.
        /// </summary>
        public ReadOnlyMemory<byte> GetBinary()
        {
            return (ReadOnlyMemory<byte>)Value;
        }

        /// <summary>
        /// Mengambil nilai integer 32-bit.
        /// </summary>
        public int GetInt32()
        {
            return (int)Value;
        }

        /// <summary>
        /// Mengambil nilai boolean.
        /// </summary>
        public bool GetBoolean()
        {
            return (bool)Value;
        }

        /// <summary>
        /// Mengambil nilai waktu.
        /// </summary>
        public DateTimeOffset GetDateTime()
        {
            return (DateTimeOffset)Value;
        }
    }

    /// <summary>
    /// Record PC yang akan ditulis ke leaf record.
    /// </summary>
    private readonly struct PropertyRecord
    {
        /// <summary>
        /// Membuat record PC.
        /// </summary>
        /// <param name="propertyId">Property id.</param>
        /// <param name="propType">Property type.</param>
        /// <param name="valueHnid">Nilai HNID atau inline value.</param>
        public PropertyRecord(ushort propertyId, PstPropertyType propType, uint valueHnid)
        {
            PropertyId = propertyId;
            PropType = propType;
            ValueHnid = valueHnid;
        }

        /// <summary>
        /// Property id.
        /// </summary>
        public ushort PropertyId { get; }

        /// <summary>
        /// Property type.
        /// </summary>
        public PstPropertyType PropType { get; }

        /// <summary>
        /// Nilai HNID atau inline value.
        /// </summary>
        public uint ValueHnid { get; }
    }
}
