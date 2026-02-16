using System;
using System.Collections.Generic;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Parser Table Context (TC) untuk membaca row matrix dan urutan row ID.
/// </summary>
internal sealed class TableContext
{
    private const byte TableSignature = 0x7C;
    private const int TcInfoSize = 22;
    private const int ColumnDescriptorSize = 8;
    private readonly HeapOnNode _heap;
    private readonly SubnodeReader _subnodes;
    private readonly TableContextInfo? _info;
    private readonly IReadOnlyList<TableColumn> _columns;
    private readonly Dictionary<uint, TableColumn> _columnsByTag;

    /// <summary>
    /// Membuat instance Table Context dari heap dan subnode reader.
    /// </summary>
    /// <param name="heap">Heap-on-Node untuk TC.</param>
    /// <param name="subnodes">Reader subnode untuk data row matrix.</param>
    public TableContext(HeapOnNode heap, SubnodeReader subnodes)
    {
        _heap = heap;
        _subnodes = subnodes;
        _info = ParseInfo(out _columns, out _columnsByTag);
        _columns ??= Array.Empty<TableColumn>();
        _columnsByTag ??= new Dictionary<uint, TableColumn>();
    }

    /// <summary>
    /// Mengambil daftar row ID sesuai urutan row matrix.
    /// </summary>
    /// <returns>Daftar row ID.</returns>
    public IReadOnlyList<uint> ReadRowIds()
    {
        if (!_info.HasValue)
        {
            return Array.Empty<uint>();
        }

        var info = _info.Value;
        if (info.RowSize < 4 || info.RowMatrixRaw == 0)
        {
            return Array.Empty<uint>();
        }

        if (!TryReadRowMatrixData(out var rowMatrix))
        {
            return Array.Empty<uint>();
        }

        var rowIds = new List<uint>();
        CollectRowIds(rowMatrix.Span, info.RowSize, rowIds);
        return rowIds;
    }

    /// <summary>
    /// Mengambil daftar row lengkap dari row matrix.
    /// </summary>
    /// <returns>Daftar row table.</returns>
    public IReadOnlyList<TableRow> ReadRows()
    {
        if (!_info.HasValue)
        {
            return Array.Empty<TableRow>();
        }

        var info = _info.Value;
        if (info.RowSize < 4 || info.RowMatrixRaw == 0)
        {
            return Array.Empty<TableRow>();
        }

        if (!TryReadRowMatrixData(out var rowMatrix))
        {
            return Array.Empty<TableRow>();
        }

        var rowCount = rowMatrix.Length / info.RowSize;
        var rows = new List<TableRow>(rowCount);
        var offset = 0;
        for (var i = 0; i < rowCount; i++)
        {
            var slice = rowMatrix.Slice(offset, info.RowSize);
            var rowId = BitConverter.ToUInt32(slice.Span.Slice(0, 4));
            rows.Add(new TableRow(this, rowId, slice));
            offset += info.RowSize;
        }

        return rows;
    }

    /// <summary>
    /// Mengambil daftar kolom yang didefinisikan pada TC.
    /// </summary>
    /// <returns>Daftar kolom.</returns>
    public IReadOnlyList<TableColumn> ReadColumns()
    {
        return _columns;
    }

    /// <summary>
    /// Mengurai header TCINFO dari heap untuk mengambil metadata row matrix.
    /// </summary>
    /// <returns>Info TC bila valid, atau null.</returns>
    private TableContextInfo? ParseInfo(out IReadOnlyList<TableColumn> columns, out Dictionary<uint, TableColumn> columnsByTag)
    {
        columns = Array.Empty<TableColumn>();
        columnsByTag = new Dictionary<uint, TableColumn>();
        if (!_heap.UserRoot.IsValid)
        {
            return null;
        }

        var header = _heap.ReadItem(_heap.UserRoot);
        if (header.Length < TcInfoSize)
        {
            return null;
        }

        var span = header.Span;
        var bType = span[0];
        if (bType != TableSignature)
        {
            return null;
        }

        var cCols = span[1];
        var rowSize = BitConverter.ToUInt16(span.Slice(8, 2));
        var rowMatrixRaw = BitConverter.ToUInt32(span.Slice(14, 4));
        var expectedSize = TcInfoSize + (cCols * ColumnDescriptorSize);
        if (header.Length < expectedSize || rowSize == 0)
        {
            return null;
        }

        var parsedColumns = new List<TableColumn>(cCols);
        var offset = TcInfoSize;
        for (var i = 0; i < cCols; i++)
        {
            var tag = BitConverter.ToUInt32(span.Slice(offset, 4));
            var ibData = BitConverter.ToUInt16(span.Slice(offset + 4, 2));
            var cbData = span[offset + 6];
            var iBit = span[offset + 7];
            var column = new TableColumn(tag, ibData, cbData, iBit);
            parsedColumns.Add(column);
            columnsByTag[tag] = column;
            offset += ColumnDescriptorSize;
        }

        columns = parsedColumns;
        return new TableContextInfo(cCols, rowSize, rowMatrixRaw);
    }

    /// <summary>
    /// Mengumpulkan row ID dari buffer row matrix.
    /// </summary>
    /// <param name="data">Buffer row matrix.</param>
    /// <param name="rowSize">Ukuran satu row.</param>
    /// <param name="rowIds">Koleksi output.</param>
    private static void CollectRowIds(ReadOnlySpan<byte> data, ushort rowSize, List<uint> rowIds)
    {
        var rowCount = data.Length / rowSize;
        for (var i = 0; i < rowCount; i++)
        {
            var offset = i * rowSize;
            var rowId = BitConverter.ToUInt32(data.Slice(offset, 4));
            if (rowId == 0)
            {
                continue;
            }

            rowIds.Add(rowId);
        }
    }

    /// <summary>
    /// Membaca data row matrix menjadi satu buffer kontigu.
    /// </summary>
    /// <param name="rowMatrix">Buffer row matrix hasil pembacaan.</param>
    /// <returns>True jika data tersedia.</returns>
    private bool TryReadRowMatrixData(out ReadOnlyMemory<byte> rowMatrix)
    {
        rowMatrix = ReadOnlyMemory<byte>.Empty;
        if (!_info.HasValue)
        {
            return false;
        }

        var info = _info.Value;
        var hnid = new Hnid(info.RowMatrixRaw);
        if (hnid.IsHid)
        {
            var hid = hnid.ToHid();
            if (!hid.IsValid)
            {
                return false;
            }

            rowMatrix = _heap.ReadItem(hid);
            return true;
        }

        if (_subnodes.TryGetSubnodeData(hnid.ToNid(), out var blocks, out var totalLength))
        {
            rowMatrix = ConcatBlocks(blocks, totalLength);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Menggabungkan data dari beberapa blok menjadi satu buffer.
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
    /// Representasi kolom pada Table Context.
    /// </summary>
    internal readonly struct TableColumn
    {
        /// <summary>
        /// Membuat kolom table context.
        /// </summary>
        /// <param name="tag">Property tag (propId + propType).</param>
        /// <param name="ibData">Offset data dalam row.</param>
        /// <param name="cbData">Ukuran data dalam row.</param>
        /// <param name="iBit">Index bit pada CEB.</param>
        public TableColumn(uint tag, ushort ibData, byte cbData, byte iBit)
        {
            Tag = tag;
            IbData = ibData;
            CbData = cbData;
            IBit = iBit;
        }

        /// <summary>
        /// Property tag (propId + propType).
        /// </summary>
        public uint Tag { get; }

        /// <summary>
        /// Offset data dalam row matrix.
        /// </summary>
        public ushort IbData { get; }

        /// <summary>
        /// Ukuran data dalam row matrix.
        /// </summary>
        public byte CbData { get; }

        /// <summary>
        /// Index bit pada Cell Existence Bitmap.
        /// </summary>
        public byte IBit { get; }

        /// <summary>
        /// Property id.
        /// </summary>
        public ushort PropId => (ushort)(Tag & 0xFFFF);

        /// <summary>
        /// Property type.
        /// </summary>
        public ushort PropType => (ushort)(Tag >> 16);
    }

    /// <summary>
    /// Representasi row data pada Table Context.
    /// </summary>
    internal sealed class TableRow
    {
        private readonly TableContext _context;
        private readonly ReadOnlyMemory<byte> _rowData;

        /// <summary>
        /// Membuat row table context.
        /// </summary>
        /// <param name="context">Context table.</param>
        /// <param name="rowId">Row ID.</param>
        /// <param name="rowData">Data row.</param>
        public TableRow(TableContext context, uint rowId, ReadOnlyMemory<byte> rowData)
        {
            _context = context;
            _rowData = rowData;
            RowId = rowId;
        }

        /// <summary>
        /// Row ID unik.
        /// </summary>
        public uint RowId { get; }

        /// <summary>
        /// Mengambil nilai berdasarkan property tag.
        /// </summary>
        /// <param name="propertyTag">Property tag yang dicari.</param>
        /// <param name="value">Nilai hasil.</param>
        /// <returns>True jika nilai ditemukan.</returns>
        public bool TryGetValue(uint propertyTag, out TableCellValue value)
        {
            return _context.TryGetCellValue(_rowData, propertyTag, out value);
        }
    }

    /// <summary>
    /// Nilai cell pada Table Context.
    /// </summary>
    internal readonly struct TableCellValue
    {
        /// <summary>
        /// Membuat nilai cell.
        /// </summary>
        /// <param name="propType">Tipe properti.</param>
        /// <param name="data">Data mentah.</param>
        public TableCellValue(ushort propType, ReadOnlyMemory<byte> data)
        {
            PropType = propType;
            Data = data;
        }

        /// <summary>
        /// Tipe properti.
        /// </summary>
        public ushort PropType { get; }

        /// <summary>
        /// Data mentah cell.
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; }
    }

    /// <summary>
    /// Mencoba mengambil nilai cell dari row matrix berdasarkan property tag.
    /// </summary>
    /// <param name="rowData">Data row.</param>
    /// <param name="propertyTag">Property tag.</param>
    /// <param name="value">Nilai hasil.</param>
    /// <returns>True jika nilai ditemukan.</returns>
    private bool TryGetCellValue(ReadOnlyMemory<byte> rowData, uint propertyTag, out TableCellValue value)
    {
        value = default;
        if (!_columnsByTag.TryGetValue(propertyTag, out var column))
        {
            return false;
        }

        if (!IsCellPresent(rowData.Span, column))
        {
            return false;
        }

        if (!TryGetRawValue(rowData.Span, column, out var data))
        {
            return false;
        }

        value = new TableCellValue(column.PropType, data);
        return true;
    }

    /// <summary>
    /// Menentukan apakah cell valid berdasarkan CEB.
    /// </summary>
    /// <param name="rowData">Data row.</param>
    /// <param name="column">Kolom.</param>
    /// <returns>True jika cell valid.</returns>
    private bool IsCellPresent(ReadOnlySpan<byte> rowData, TableColumn column)
    {
        if (!_info.HasValue)
        {
            return false;
        }

        var info = _info.Value;
        var cebLength = (info.ColumnCount + 7) / 8;
        if (cebLength == 0 || rowData.Length < cebLength)
        {
            return true;
        }

        var cebStart = rowData.Length - cebLength;
        var bitIndex = column.IBit;
        var byteIndex = bitIndex / 8;
        if (byteIndex >= cebLength)
        {
            return true;
        }

        var bitMask = (byte)(1 << (7 - (bitIndex % 8)));
        return (rowData[cebStart + byteIndex] & bitMask) != 0;
    }

    /// <summary>
    /// Mengambil data mentah kolom dari row matrix.
    /// </summary>
    /// <param name="rowData">Data row.</param>
    /// <param name="column">Kolom.</param>
    /// <param name="data">Data mentah hasil.</param>
    /// <returns>True jika data berhasil diambil.</returns>
    private bool TryGetRawValue(ReadOnlySpan<byte> rowData, TableColumn column, out ReadOnlyMemory<byte> data)
    {
        data = ReadOnlyMemory<byte>.Empty;
        if (column.CbData == 0)
        {
            return false;
        }

        if (!TryGetTypeInfo(column.PropType, out var typeInfo))
        {
            return false;
        }

        var offset = column.IbData;
        if (offset + column.CbData > rowData.Length)
        {
            return false;
        }

        if (!typeInfo.IsVariable && typeInfo.Size <= 8)
        {
            var buffer = new byte[typeInfo.Size];
            rowData.Slice(offset, typeInfo.Size).CopyTo(buffer);
            data = buffer;
            return true;
        }

        var hnidRaw = BitConverter.ToUInt32(rowData.Slice(offset, 4));
        var hnid = new Hnid(hnidRaw);
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

    /// <summary>
    /// Metadata minimal Table Context untuk row matrix.
    /// </summary>
    private readonly struct TableContextInfo
    {
        /// <summary>
        /// Membuat info TC.
        /// </summary>
        /// <param name="columnCount">Jumlah kolom.</param>
        /// <param name="rowSize">Ukuran satu row.</param>
        /// <param name="rowMatrixRaw">HNID row matrix.</param>
        public TableContextInfo(byte columnCount, ushort rowSize, uint rowMatrixRaw)
        {
            ColumnCount = columnCount;
            RowSize = rowSize;
            RowMatrixRaw = rowMatrixRaw;
        }

        /// <summary>
        /// Jumlah kolom pada TC.
        /// </summary>
        public byte ColumnCount { get; }

        /// <summary>
        /// Ukuran satu row dalam row matrix.
        /// </summary>
        public ushort RowSize { get; }

        /// <summary>
        /// Nilai HNID untuk row matrix.
        /// </summary>
        public uint RowMatrixRaw { get; }
    }
}
