using System;
using System.Collections.Generic;
using System.Linq;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer B-Tree sederhana untuk BBT dan NBT (in-memory).
/// </summary>
internal sealed class NdbBtreeWriter
{
    private readonly Dictionary<ulong, BbtEntry> _bbt = new();
    private readonly Dictionary<uint, NbtEntry> _nbt = new();

    /// <summary>
    /// Menambahkan atau memperbarui entry BBT.
    /// </summary>
    /// <param name="entry">Entry BBT.</param>
    public void UpsertBbtEntry(BbtEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _bbt[entry.Bid.NormalizeForLookup()] = entry;
    }

    /// <summary>
    /// Menambahkan atau memperbarui entry NBT.
    /// </summary>
    /// <param name="entry">Entry NBT.</param>
    public void UpsertNbtEntry(NbtEntry entry)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _nbt[entry.Nid.Value] = entry;
    }

    /// <summary>
    /// Mengambil snapshot dictionary BBT saat ini.
    /// </summary>
    /// <returns>Dictionary BBT.</returns>
    public IReadOnlyDictionary<ulong, BbtEntry> SnapshotBbt()
    {
        return new Dictionary<ulong, BbtEntry>(_bbt);
    }

    /// <summary>
    /// Mengambil snapshot dictionary NBT saat ini.
    /// </summary>
    /// <returns>Dictionary NBT.</returns>
    public IReadOnlyDictionary<uint, NbtEntry> SnapshotNbt()
    {
        return new Dictionary<uint, NbtEntry>(_nbt);
    }

    /// <summary>
    /// Menulis ulang BBT ke file dan mengembalikan root BREF baru.
    /// </summary>
    /// <param name="blockWriter">Writer block.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="entries">Entry BBT.</param>
    /// <returns>BREF root BBT baru.</returns>
    public Bref WriteBbt(NdbBlockWriter blockWriter, PstFormat format, IEnumerable<BbtEntry> entries)
    {
        var ordered = entries.OrderBy(entry => entry.Bid.NormalizeForLookup()).ToList();
        return WriteBtreePages(blockWriter, format, NdbPageType.Bbt, ordered.Select(BuildBbtEntry).ToList(),
            format == PstFormat.Unicode ? 24 : 16,
            format == PstFormat.Unicode ? 24 : 12);
    }

    /// <summary>
    /// Menulis ulang NBT ke file dan mengembalikan root BREF baru.
    /// </summary>
    /// <param name="blockWriter">Writer block.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="entries">Entry NBT.</param>
    /// <returns>BREF root NBT baru.</returns>
    public Bref WriteNbt(NdbBlockWriter blockWriter, PstFormat format, IEnumerable<NbtEntry> entries)
    {
        var ordered = entries.OrderBy(entry => entry.Nid.Value).ToList();
        return WriteBtreePages(blockWriter, format, NdbPageType.Nbt, ordered.Select(BuildNbtEntry).ToList(),
            format == PstFormat.Unicode ? 32 : 16,
            format == PstFormat.Unicode ? 24 : 12);
    }

    private static Bref WriteBtreePages(
        NdbBlockWriter blockWriter,
        PstFormat format,
        byte pageType,
        IReadOnlyList<BtreeEntry> entries,
        int leafEntrySize,
        int intermediateEntrySize)
    {
        if (entries.Count == 0)
        {
            var emptyRoot = BuildEmptyLeafPage(format, leafEntrySize);
            var allocation = blockWriter.WritePage(emptyRoot, pageType);
            return new Bref(allocation.Bid, allocation.Ib);
        }

        var pages = BuildLeafPages(format, entries, leafEntrySize);
        var current = new List<PageRef>(pages.Count);
        foreach (var page in pages)
        {
            var allocation = blockWriter.WritePage(page.Data, pageType);
            current.Add(new PageRef(page.Key, allocation));
        }

        var entryAreaSize = GetEntryAreaSize(format);
        var cEntMax = entryAreaSize / intermediateEntrySize;
        if (cEntMax == 0)
        {
            throw new InvalidOperationException("Ukuran entry terlalu besar untuk halaman B-Tree.");
        }

        byte currentLevel = 1;
        while (current.Count > 1)
        {
            var next = new List<PageRef>();
            for (var i = 0; i < current.Count; i += cEntMax)
            {
                var chunk = current.Skip(i).Take(cEntMax).ToList();
                var page = BuildIntermediatePage(format, chunk, intermediateEntrySize, cEntMax, currentLevel);
                var allocation = blockWriter.WritePage(page, pageType);
                next.Add(new PageRef(chunk[0].Key, allocation));
            }

            current = next;
            checked
            {
                currentLevel++;
            }
        }

        var root = current[0];
        return new Bref(root.Allocation.Bid, root.Allocation.Ib);
    }

    private static List<PageData> BuildLeafPages(PstFormat format, IReadOnlyList<BtreeEntry> entries, int cbEnt)
    {
        var pages = new List<PageData>();
        var entryAreaSize = GetEntryAreaSize(format);
        var cEntMax = entryAreaSize / cbEnt;
        if (cEntMax == 0)
        {
            throw new InvalidOperationException("Ukuran entry terlalu besar untuk halaman B-Tree.");
        }

        for (var i = 0; i < entries.Count; i += cEntMax)
        {
            var slice = entries.Skip(i).Take(cEntMax).ToList();
            var page = new byte[512];
            var offset = 0;
            foreach (var entry in slice)
            {
                entry.WriteLeaf(page.AsSpan(offset, cbEnt), format);
                offset += cbEnt;
            }

            WritePageHeader(page, format, (byte)slice.Count, (byte)cEntMax, (byte)cbEnt, 0);
            pages.Add(new PageData(slice[0].Key, page));
        }

        return pages;
    }

    private static byte[] BuildEmptyLeafPage(PstFormat format, int cbEnt)
    {
        var page = new byte[512];
        var entryAreaSize = GetEntryAreaSize(format);
        var cEntMax = entryAreaSize / cbEnt;
        if (cEntMax == 0)
        {
            throw new InvalidOperationException("Ukuran entry terlalu besar untuk halaman B-Tree.");
        }

        WritePageHeader(page, format, cEnt: 0, (byte)cEntMax, (byte)cbEnt, cLevel: 0);
        return page;
    }

    private static byte[] BuildIntermediatePage(
        PstFormat format,
        IReadOnlyList<PageRef> children,
        int cbEnt,
        int cEntMax,
        byte cLevel)
    {
        var page = new byte[512];
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            WriteIntermediateEntry(page.AsSpan(i * cbEnt, cbEnt), format, child.Key, child.Allocation);
        }

        WritePageHeader(page, format, (byte)children.Count, (byte)cEntMax, (byte)cbEnt, cLevel);
        return page;
    }

    private static void WriteIntermediateEntry(Span<byte> buffer, PstFormat format, ulong key, NdbBlockAllocation allocation)
    {
        if (format == PstFormat.Unicode)
        {
            BitConverter.TryWriteBytes(buffer.Slice(0, 8), key);
            BitConverter.TryWriteBytes(buffer.Slice(8, 8), allocation.Bid.Raw);
            BitConverter.TryWriteBytes(buffer.Slice(16, 8), allocation.Ib);
            return;
        }

        BitConverter.TryWriteBytes(buffer.Slice(0, 4), (uint)key);
        BitConverter.TryWriteBytes(buffer.Slice(4, 4), (uint)allocation.Bid.Raw);
        BitConverter.TryWriteBytes(buffer.Slice(8, 4), (uint)allocation.Ib);
    }

    private static int GetEntryAreaSize(PstFormat format)
    {
        var trailerSize = format == PstFormat.Unicode ? 16 : 12;
        var paddingSize = format == PstFormat.Unicode ? 4 : 0;
        return 512 - trailerSize - paddingSize - 4;
    }

    private static void WritePageHeader(byte[] page, PstFormat format, byte cEnt, byte cEntMax, byte cbEnt, byte cLevel)
    {
        var trailerSize = format == PstFormat.Unicode ? 16 : 12;
        var paddingSize = format == PstFormat.Unicode ? 4 : 0;
        var entryArea = page.Length - trailerSize - paddingSize - 4;
        page[entryArea] = cEnt;
        page[entryArea + 1] = cEntMax;
        page[entryArea + 2] = cbEnt;
        page[entryArea + 3] = cLevel;
    }

    private static BtreeEntry BuildBbtEntry(BbtEntry entry)
    {
        return new BtreeEntry(entry.Bid.NormalizeForLookup(), (span, format) =>
        {
            if (format == PstFormat.Unicode)
            {
                BitConverter.TryWriteBytes(span.Slice(0, 8), entry.Bid.Raw);
                BitConverter.TryWriteBytes(span.Slice(8, 8), entry.Ib);
                BitConverter.TryWriteBytes(span.Slice(16, 2), entry.Cb);
                BitConverter.TryWriteBytes(span.Slice(18, 2), entry.CRef);
                return;
            }

            BitConverter.TryWriteBytes(span.Slice(0, 4), (uint)entry.Bid.Raw);
            BitConverter.TryWriteBytes(span.Slice(4, 4), (uint)entry.Ib);
            BitConverter.TryWriteBytes(span.Slice(8, 2), entry.Cb);
            BitConverter.TryWriteBytes(span.Slice(10, 2), entry.CRef);
        });
    }

    private static BtreeEntry BuildNbtEntry(NbtEntry entry)
    {
        return new BtreeEntry(entry.Nid.Value, (span, format) =>
        {
            if (format == PstFormat.Unicode)
            {
                BitConverter.TryWriteBytes(span.Slice(0, 8), entry.Nid.Value);
                BitConverter.TryWriteBytes(span.Slice(8, 8), entry.BidData.Raw);
                BitConverter.TryWriteBytes(span.Slice(16, 8), entry.BidSub.Raw);
                BitConverter.TryWriteBytes(span.Slice(24, 4), entry.NidParent.Value);
                return;
            }

            BitConverter.TryWriteBytes(span.Slice(0, 4), entry.Nid.Value);
            BitConverter.TryWriteBytes(span.Slice(4, 4), (uint)entry.BidData.Raw);
            BitConverter.TryWriteBytes(span.Slice(8, 4), (uint)entry.BidSub.Raw);
            BitConverter.TryWriteBytes(span.Slice(12, 4), entry.NidParent.Value);
        });
    }

    private readonly struct BtreeEntry
    {
        public BtreeEntry(ulong key, Action<Span<byte>, PstFormat> writeLeaf)
        {
            Key = key;
            _writeLeaf = writeLeaf;
        }

        public ulong Key { get; }

        private readonly Action<Span<byte>, PstFormat> _writeLeaf;

        public void WriteLeaf(Span<byte> buffer, PstFormat format)
        {
            _writeLeaf(buffer, format);
        }
    }

    private readonly struct PageData
    {
        public PageData(ulong key, byte[] data)
        {
            Key = key;
            Data = data;
        }

        public ulong Key { get; }

        public byte[] Data { get; }
    }

    private readonly struct PageRef
    {
        public PageRef(ulong key, NdbBlockAllocation allocation)
        {
            Key = key;
            Allocation = allocation;
        }

        public ulong Key { get; }

        public NdbBlockAllocation Allocation { get; }
    }
}
