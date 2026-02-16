using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer NDB tingkat tinggi untuk alokasi block dan update BBT/NBT in-memory.
/// </summary>
internal sealed class NdbWriter
{
    private readonly Stream _stream;
    private readonly PstFormat _format;
    private readonly NdbWriterCore _core;
    private readonly NdbBlockWriter _blockWriter;
    private readonly NdbBtreeWriter _btreeWriter = new();

    /// <summary>
    /// Membuat writer NDB.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="header">Header PST.</param>
    /// <param name="initialBidCounter">Counter awal BID untuk melanjutkan alokasi.</param>
    /// <param name="initialOffset">Offset awal untuk alokasi block; default memakai ukuran file.</param>
    public NdbWriter(Stream stream, PstHeaderInfo header, ulong? initialBidCounter = null, ulong? initialOffset = null)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _format = header.Format;
        _core = new NdbWriterCore(header, initialOffset, initialBidCounter);
        _blockWriter = new NdbBlockWriter(_stream, _core, header.CryptMethod);
    }

    /// <summary>
    /// Menulis blok data eksternal dan mendaftarkan entry BBT in-memory.
    /// </summary>
    /// <param name="data">Data blok.</param>
    /// <returns>Entry BBT.</returns>
    public BbtEntry WriteExternalBlock(ReadOnlySpan<byte> data)
    {
        var allocation = _blockWriter.WriteExternalBlock(data);
        var entry = new BbtEntry(allocation.Bid, allocation.Ib, allocation.DataSize, 1);
        _btreeWriter.UpsertBbtEntry(entry);
        return entry;
    }

    /// <summary>
    /// Menulis blok data internal dan mendaftarkan entry BBT in-memory.
    /// </summary>
    /// <param name="data">Data blok.</param>
    /// <returns>Entry BBT.</returns>
    public BbtEntry WriteInternalBlock(ReadOnlySpan<byte> data)
    {
        var allocation = _blockWriter.WriteInternalBlock(data);
        var entry = new BbtEntry(allocation.Bid, allocation.Ib, allocation.DataSize, 1);
        _btreeWriter.UpsertBbtEntry(entry);
        return entry;
    }

    /// <summary>
    /// Menulis data sebagai data tree (XBLOCK/XXBLOCK) bila melebihi ukuran block.
    /// </summary>
    /// <param name="data">Data yang akan ditulis.</param>
    /// <returns>BID root data tree.</returns>
    public Bid WriteDataTree(ReadOnlyMemory<byte> data)
    {
        if (data.Length <= _core.BlockSize)
        {
            return WriteExternalBlock(data.Span).Bid;
        }

        var chunkSize = _core.BlockSize;
        var childBids = new List<Bid>();
        var offset = 0;
        while (offset < data.Length)
        {
            var size = Math.Min(chunkSize, data.Length - offset);
            var entry = WriteExternalBlock(data.Span.Slice(offset, size));
            childBids.Add(entry.Bid);
            offset += size;
        }

        return WriteDataTreeFromChildren(childBids, (uint)data.Length);
    }

    /// <summary>
    /// Menulis data tree secara asynchronous.
    /// </summary>
    /// <param name="data">Data yang akan ditulis.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>BID root data tree.</returns>
    public async Task<Bid> WriteDataTreeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (data.Length <= _core.BlockSize)
        {
            var entry = await _blockWriter.WriteExternalBlockAsync(data, cancellationToken).ConfigureAwait(false);
            _btreeWriter.UpsertBbtEntry(new BbtEntry(entry.Bid, entry.Ib, entry.DataSize, 1));
            return entry.Bid;
        }

        var chunkSize = _core.BlockSize;
        var childBids = new List<Bid>();
        var offset = 0;
        while (offset < data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var size = Math.Min(chunkSize, data.Length - offset);
            var entry = await _blockWriter.WriteExternalBlockAsync(data.Slice(offset, size), cancellationToken).ConfigureAwait(false);
            _btreeWriter.UpsertBbtEntry(new BbtEntry(entry.Bid, entry.Ib, entry.DataSize, 1));
            childBids.Add(entry.Bid);
            offset += size;
        }

        return WriteDataTreeFromChildren(childBids, (uint)data.Length);
    }

    private Bid WriteDataTreeFromChildren(IReadOnlyList<Bid> childBids, uint totalLength)
    {
        var bidSize = _format == PstFormat.Unicode ? 8 : 4;
        var cEntMax = (_core.BlockSize - 8) / bidSize;
        if (cEntMax <= 0)
        {
            throw new InvalidOperationException("Ukuran block terlalu kecil untuk XBLOCK.");
        }

        if (childBids.Count <= cEntMax)
        {
            var block = BuildXblock(childBids, totalLength, level: 1, bidSize);
            var entry = WriteInternalBlock(block);
            return entry.Bid;
        }

        var xblocks = new List<Bid>();
        for (var i = 0; i < childBids.Count; i += cEntMax)
        {
            var slice = childBids.Skip(i).Take(cEntMax).ToList();
            var block = BuildXblock(slice, totalLength, level: 1, bidSize);
            var entry = WriteInternalBlock(block);
            xblocks.Add(entry.Bid);
        }

        if (xblocks.Count > cEntMax)
        {
            throw new NotSupportedException("Data tree lebih dalam dari XXBLOCK belum didukung.");
        }

        var xxBlock = BuildXblock(xblocks, totalLength, level: 2, bidSize);
        var rootEntry = WriteInternalBlock(xxBlock);
        return rootEntry.Bid;
    }

    private byte[] BuildXblock(IReadOnlyList<Bid> bids, uint totalLength, byte level, int bidSize)
    {
        var buffer = new byte[8 + (bids.Count * bidSize)];
        buffer[0] = 0x01;
        buffer[1] = level;
        BitConverter.TryWriteBytes(buffer.AsSpan(2, 2), (ushort)bids.Count);
        BitConverter.TryWriteBytes(buffer.AsSpan(4, 4), totalLength);

        var offset = 8;
        foreach (var bid in bids)
        {
            if (_format == PstFormat.Unicode)
            {
                BitConverter.TryWriteBytes(buffer.AsSpan(offset, 8), bid.Raw);
                offset += 8;
            }
            else
            {
                BitConverter.TryWriteBytes(buffer.AsSpan(offset, 4), (uint)bid.Raw);
                offset += 4;
            }
        }

        return buffer;
    }

    /// <summary>
    /// Menambahkan atau memperbarui entry NBT in-memory.
    /// </summary>
    /// <param name="entry">Entry NBT.</param>
    public void UpsertNbtEntry(NbtEntry entry)
    {
        _btreeWriter.UpsertNbtEntry(entry);
    }

    /// <summary>
    /// Mengambil snapshot BBT in-memory.
    /// </summary>
    /// <returns>Dictionary BBT.</returns>
    public IReadOnlyDictionary<ulong, BbtEntry> SnapshotBbt()
    {
        return _btreeWriter.SnapshotBbt();
    }

    /// <summary>
    /// Mengambil snapshot NBT in-memory.
    /// </summary>
    /// <returns>Dictionary NBT.</returns>
    public IReadOnlyDictionary<uint, NbtEntry> SnapshotNbt()
    {
        return _btreeWriter.SnapshotNbt();
    }

    /// <summary>
    /// Menulis ulang BBT/NBT ke file dan memperbarui header root BREF.
    /// </summary>
    /// <param name="header">Header PST saat ini.</param>
    /// <param name="existingBbt">Entry BBT yang sudah ada.</param>
    /// <param name="existingNbt">Entry NBT yang sudah ada.</param>
    /// <returns>Header PST yang diperbarui dengan ukuran file terbaru.</returns>
    public PstHeaderInfo CommitBtrees(
        NdbHeader header,
        IReadOnlyDictionary<ulong, BbtEntry> existingBbt,
        IReadOnlyDictionary<uint, NbtEntry> existingNbt)
    {
        var mergedBbt = new Dictionary<ulong, BbtEntry>(existingBbt);
        foreach (var entry in _btreeWriter.SnapshotBbt())
        {
            mergedBbt[entry.Key] = entry.Value;
        }

        var mergedNbt = new Dictionary<uint, NbtEntry>(existingNbt);
        foreach (var entry in _btreeWriter.SnapshotNbt())
        {
            mergedNbt[entry.Key] = entry.Value;
        }

        var btreeWriter = new NdbBtreeWriter();
        var bbtRoot = btreeWriter.WriteBbt(_blockWriter, _format, mergedBbt.Values);
        var nbtRoot = btreeWriter.WriteNbt(_blockWriter, _format, mergedNbt.Values);

        var headerWriter = new NdbHeaderWriter(_stream);
        headerWriter.UpdateBtreeRoots(header.HeaderInfo.Format, bbtRoot, nbtRoot);
        return headerWriter.UpdateFileSize(header.HeaderInfo);
    }
}
