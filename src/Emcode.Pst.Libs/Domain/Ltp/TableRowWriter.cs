using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Writer untuk membangun Table Context (TC) dan row matrix.
/// </summary>
internal sealed class TableRowWriter
{
    private readonly List<TableColumnDefinition> _columns = new();
    private readonly List<TableRowDefinition> _rows = new();
    private readonly LtpWriterOptions _options;
    private readonly List<LtpSubnodeData> _subnodes = new();
    private uint _nextSubnodeIndex = 1;

    /// <summary>
    /// Membuat writer table row.
    /// </summary>
    /// <param name="options">Opsi writer LTP.</param>
    public TableRowWriter(LtpWriterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Menambahkan definisi kolom pada table.
    /// </summary>
    /// <param name="propertyId">Property id.</param>
    /// <param name="propType">Property type.</param>
    /// <param name="offset">Offset data dalam row.</param>
    /// <param name="size">Ukuran data dalam row.</param>
    /// <param name="bitIndex">Index bit CEB.</param>
    public void AddColumn(ushort propertyId, PstPropertyType propType, ushort offset, byte size, byte bitIndex)
    {
        _columns.Add(new TableColumnDefinition(propertyId, propType, offset, size, bitIndex));
    }

    /// <summary>
    /// Menambahkan row ke table.
    /// </summary>
    /// <param name="rowId">Row ID unik.</param>
    /// <param name="cells">Nilai cell pada row.</param>
    public void AddRow(uint rowId, params TableCell[] cells)
    {
        _rows.Add(new TableRowDefinition(rowId, cells));
    }

    /// <summary>
    /// Membangun Table Context menjadi blok heap.
    /// </summary>
    /// <returns>Daftar blok data heap.</returns>
    public IReadOnlyList<PstDataBlock> Build()
    {
        return BuildResult().Blocks;
    }

    /// <summary>
    /// Membangun Table Context beserta subnode untuk nilai besar.
    /// </summary>
    /// <returns>Hasil penulisan LTP.</returns>
    public LtpWriteResult BuildResult()
    {
        if (_columns.Count == 0)
        {
            throw new InvalidOperationException("Kolom table belum didefinisikan.");
        }

        try
        {
            return BuildResultCore(forceVariableToSubnode: false);
        }
        catch (InvalidOperationException ex) when (IsHeapCapacityException(ex))
        {
            return BuildResultCore(forceVariableToSubnode: true);
        }
    }

    private LtpWriteResult BuildResultCore(bool forceVariableToSubnode)
    {
        _subnodes.Clear();
        _nextSubnodeIndex = 1;

        var heap = new LtpWriter.HeapWriter(_options);
        var rowSize = ResolveRowSize();
        var rowMatrixRaw = BuildRowMatrix(heap, rowSize, forceVariableToSubnode);
        var tcInfo = BuildTcInfo(rowSize, rowMatrixRaw);
        var tcInfoHid = heap.AddItem(tcInfo);
        return new LtpWriteResult(heap.Build(tcInfoHid), _subnodes);
    }

    /// <summary>
    /// Membangun Table Context secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar blok data heap.</returns>
    public Task<IReadOnlyList<PstDataBlock>> BuildAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Build());
    }

    /// <summary>
    /// Membangun Table Context beserta subnode secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    public Task<LtpWriteResult> BuildResultAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(BuildResult());
    }

    private ushort ResolveRowSize()
    {
        ushort size = 4;
        foreach (var column in _columns)
        {
            var end = (ushort)(column.Offset + column.Size);
            if (end > size)
            {
                size = end;
            }
        }

        var cebLength = (ushort)((_columns.Count + 7) / 8);
        return (ushort)(size + cebLength);
    }

    private uint BuildRowMatrix(LtpWriter.HeapWriter heap, ushort rowSize, bool forceVariableToSubnode)
    {
        var cebLength = (ushort)((_columns.Count + 7) / 8);
        var cebStart = rowSize - cebLength;
        var buffer = new byte[rowSize * _rows.Count];
        var offset = 0;
        foreach (var row in _rows)
        {
            var rowSpan = buffer.AsSpan(offset, rowSize);
            BitConverter.TryWriteBytes(rowSpan.Slice(0, 4), row.RowId);

            foreach (var cell in row.Cells)
            {
                if (!TryFindColumn(cell.PropertyId, cell.PropType, out var column))
                {
                    continue;
                }

                WriteCellValue(rowSpan, column, cell, heap, forceVariableToSubnode);
                if (cebLength > 0)
                {
                    var bitIndex = column.BitIndex;
                    var byteIndex = bitIndex / 8;
                    if (byteIndex < cebLength)
                    {
                        var bitMask = (byte)(1 << (7 - (bitIndex % 8)));
                        rowSpan[cebStart + byteIndex] |= bitMask;
                    }
                }
            }

            offset += rowSize;
        }

        if (buffer.Length > _options.MaxInlineValueBytes)
        {
            var nid = AllocateSubnode(buffer);
            return nid.Value;
        }

        var hid = heap.AddItem(buffer);
        return hid.Raw;
    }

    private byte[] BuildTcInfo(ushort rowSize, uint rowMatrixRaw)
    {
        var cCols = (byte)_columns.Count;
        var buffer = new byte[22 + (cCols * 8)];
        buffer[0] = 0x7C;
        buffer[1] = cCols;
        BitConverter.TryWriteBytes(buffer.AsSpan(8, 2), rowSize);
        BitConverter.TryWriteBytes(buffer.AsSpan(14, 4), rowMatrixRaw);

        var offset = 22;
        foreach (var column in _columns)
        {
            BitConverter.TryWriteBytes(buffer.AsSpan(offset, 4), column.Tag);
            BitConverter.TryWriteBytes(buffer.AsSpan(offset + 4, 2), column.Offset);
            buffer[offset + 6] = column.Size;
            buffer[offset + 7] = column.BitIndex;
            offset += 8;
        }

        return buffer;
    }

    private bool TryFindColumn(ushort propertyId, PstPropertyType propType, out TableColumnDefinition column)
    {
        var tag = ((uint)propType << 16) | propertyId;
        foreach (var item in _columns)
        {
            if (item.Tag == tag)
            {
                column = item;
                return true;
            }
        }

        column = default;
        return false;
    }

    private void WriteCellValue(
        Span<byte> row,
        TableColumnDefinition column,
        TableCell cell,
        LtpWriter.HeapWriter heap,
        bool forceVariableToSubnode)
    {
        if (column.Size == 0 || column.Offset + column.Size > row.Length)
        {
            return;
        }

        switch (cell.PropType)
        {
            case PstPropertyType.String:
                {
                    var value = Encoding.Unicode.GetBytes($"{cell.GetString()}\0");
                    var raw = AddValue(value, heap, forceVariableToSubnode);
                    BitConverter.TryWriteBytes(row.Slice(column.Offset, 4), raw);
                    break;
                }
            case PstPropertyType.String8:
                {
                    var value = Encoding.Latin1.GetBytes($"{cell.GetString()}\0");
                    var raw = AddValue(value, heap, forceVariableToSubnode);
                    BitConverter.TryWriteBytes(row.Slice(column.Offset, 4), raw);
                    break;
                }
            case PstPropertyType.Binary:
                {
                    var value = cell.GetBinary();
                    var raw = AddValue(value.Span, heap, forceVariableToSubnode);
                    BitConverter.TryWriteBytes(row.Slice(column.Offset, 4), raw);
                    break;
                }
            case PstPropertyType.Time:
                {
                    var bytes = new byte[8];
                    BitConverter.TryWriteBytes(bytes, cell.GetDateTime().ToFileTime());
                    bytes.CopyTo(row.Slice(column.Offset, 8));
                    break;
                }
            case PstPropertyType.Integer32:
                {
                    BitConverter.TryWriteBytes(row.Slice(column.Offset, 4), cell.GetInt32());
                    break;
                }
            case PstPropertyType.Boolean:
                {
                    BitConverter.TryWriteBytes(row.Slice(column.Offset, 2), (short)(cell.GetBoolean() ? 1 : 0));
                    break;
                }
            default:
                throw new NotSupportedException($"Property type {cell.PropType} belum didukung.");
        }
    }

    /// <summary>
    /// Menambahkan data ke heap atau subnode berdasarkan ukuran.
    /// </summary>
    /// <param name="data">Data yang akan disimpan.</param>
    /// <param name="heap">Heap target.</param>
    /// <returns>HNID hasil (HID atau NID).</returns>
    private uint AddValue(ReadOnlySpan<byte> data, LtpWriter.HeapWriter heap, bool forceVariableToSubnode)
    {
        if (forceVariableToSubnode || data.Length > _options.MaxInlineValueBytes)
        {
            var nid = AllocateSubnode(data.ToArray());
            return nid.Value;
        }

        var hid = heap.AddItem(data);
        return hid.Raw;
    }

    private static bool IsHeapCapacityException(InvalidOperationException ex)
    {
        return ex.Message.Contains("Ukuran heap melebihi kapasitas block.", StringComparison.Ordinal)
            || ex.Message.Contains("HNPAGEMAP melebihi kapasitas block.", StringComparison.Ordinal);
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
    /// Definisi kolom table.
    /// </summary>
    private readonly struct TableColumnDefinition
    {
        /// <summary>
        /// Membuat definisi kolom.
        /// </summary>
        /// <param name="propertyId">Property id.</param>
        /// <param name="propType">Property type.</param>
        /// <param name="offset">Offset data dalam row.</param>
        /// <param name="size">Ukuran data.</param>
        /// <param name="bitIndex">Index bit CEB.</param>
        public TableColumnDefinition(ushort propertyId, PstPropertyType propType, ushort offset, byte size, byte bitIndex)
        {
            PropertyId = propertyId;
            PropType = propType;
            Offset = offset;
            Size = size;
            BitIndex = bitIndex;
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
        /// Offset data pada row.
        /// </summary>
        public ushort Offset { get; }

        /// <summary>
        /// Ukuran data pada row.
        /// </summary>
        public byte Size { get; }

        /// <summary>
        /// Index bit pada CEB.
        /// </summary>
        public byte BitIndex { get; }

        /// <summary>
        /// Property tag gabungan.
        /// </summary>
        public uint Tag => ((uint)PropType << 16) | PropertyId;
    }

    /// <summary>
    /// Definisi row dan cell table.
    /// </summary>
    private readonly struct TableRowDefinition
    {
        /// <summary>
        /// Membuat definisi row.
        /// </summary>
        /// <param name="rowId">Row ID.</param>
        /// <param name="cells">Daftar cell.</param>
        public TableRowDefinition(uint rowId, TableCell[] cells)
        {
            RowId = rowId;
            Cells = cells ?? Array.Empty<TableCell>();
        }

        /// <summary>
        /// Row ID unik.
        /// </summary>
        public uint RowId { get; }

        /// <summary>
        /// Daftar cell pada row.
        /// </summary>
        public TableCell[] Cells { get; }
    }

    /// <summary>
    /// Nilai cell untuk row table.
    /// </summary>
    public readonly struct TableCell
    {
        /// <summary>
        /// Membuat cell untuk table row.
        /// </summary>
        /// <param name="propertyId">Property id.</param>
        /// <param name="propType">Property type.</param>
        /// <param name="value">Nilai cell.</param>
        public TableCell(ushort propertyId, PstPropertyType propType, object value)
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
        /// Nilai cell.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Mengambil nilai string.
        /// </summary>
        public string GetString()
        {
            return (string)Value;
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
        /// Mengambil nilai DateTimeOffset.
        /// </summary>
        public DateTimeOffset GetDateTime()
        {
            return (DateTimeOffset)Value;
        }
    }
}
