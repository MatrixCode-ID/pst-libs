using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Reader untuk B-Tree pages (BBT/NBT) pada NDB layer.
/// </summary>
internal sealed class PstBTreeReader
{
    private readonly Stream _stream;
    private readonly PstFormat _format;

    /// <summary>
    /// Membuat reader B-Tree dengan stream PST dan format.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="format">Format PST.</param>
    public PstBTreeReader(Stream stream, PstFormat format)
    {
        _stream = stream;
        _format = format;
    }

    /// <summary>
    /// Membaca seluruh entri BBT dari root.
    /// </summary>
    /// <param name="root">BREF root BBT.</param>
    /// <returns>Dictionary BID ke entri BBT.</returns>
    public IReadOnlyDictionary<ulong, BbtEntry> ReadBbt(Bref root)
    {
        var entries = new Dictionary<ulong, BbtEntry>();
        var visited = new HashSet<ulong>();
        ReadBbtPage(root, entries, visited);
        return entries;
    }

    /// <summary>
    /// Membaca seluruh entri BBT dari root secara asynchronous.
    /// </summary>
    /// <param name="root">BREF root BBT.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Dictionary BID ke entri BBT.</returns>
    public async Task<IReadOnlyDictionary<ulong, BbtEntry>> ReadBbtAsync(Bref root, CancellationToken cancellationToken = default)
    {
        var entries = new Dictionary<ulong, BbtEntry>();
        var visited = new HashSet<ulong>();
        await ReadBbtPageAsync(root, entries, visited, cancellationToken).ConfigureAwait(false);
        return entries;
    }

    /// <summary>
    /// Membaca seluruh entri NBT dari root.
    /// </summary>
    /// <param name="root">BREF root NBT.</param>
    /// <returns>Dictionary NID ke entri NBT.</returns>
    public IReadOnlyDictionary<uint, NbtEntry> ReadNbt(Bref root)
    {
        var entries = new Dictionary<uint, NbtEntry>();
        var visited = new HashSet<ulong>();
        ReadNbtPage(root, entries, visited);
        return entries;
    }

    /// <summary>
    /// Membaca seluruh entri NBT dari root secara asynchronous.
    /// </summary>
    /// <param name="root">BREF root NBT.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Dictionary NID ke entri NBT.</returns>
    public async Task<IReadOnlyDictionary<uint, NbtEntry>> ReadNbtAsync(Bref root, CancellationToken cancellationToken = default)
    {
        var entries = new Dictionary<uint, NbtEntry>();
        var visited = new HashSet<ulong>();
        await ReadNbtPageAsync(root, entries, visited, cancellationToken).ConfigureAwait(false);
        return entries;
    }

    /// <summary>
    /// Membaca page BBT secara rekursif.
    /// </summary>
    /// <param name="root">BREF halaman.</param>
    /// <param name="entries">Dictionary hasil.</param>
    /// <param name="visited">Set halaman yang sudah dibaca.</param>
    private void ReadBbtPage(Bref root, IDictionary<ulong, BbtEntry> entries, ISet<ulong> visited)
    {
        if (!visited.Add(root.Ib))
        {
            return;
        }

        var page = ReadPage(root.Ib);
        var header = ParsePageHeader(page);
        if (header.CLevel > 0)
        {
            ReadIntermediateEntries(page, header, bref => ReadBbtPage(bref, entries, visited));
            return;
        }

        for (var i = 0; i < header.CEnt; i++)
        {
            var offset = i * header.CbEnt;
            var entry = ParseBbtEntry(page, offset);
            var key = entry.Bid.NormalizeForLookup();
            entries[key] = entry;
        }
    }

    /// <summary>
    /// Membaca page BBT secara rekursif secara asynchronous.
    /// </summary>
    /// <param name="root">BREF halaman.</param>
    /// <param name="entries">Dictionary hasil.</param>
    /// <param name="visited">Set halaman yang sudah dibaca.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private async Task ReadBbtPageAsync(
        Bref root,
        IDictionary<ulong, BbtEntry> entries,
        ISet<ulong> visited,
        CancellationToken cancellationToken)
    {
        if (!visited.Add(root.Ib))
        {
            return;
        }

        var page = await ReadPageAsync(root.Ib, cancellationToken).ConfigureAwait(false);
        var header = ParsePageHeader(page);
        if (header.CLevel > 0)
        {
            await ReadIntermediateEntriesAsync(page, header, bref => ReadBbtPageAsync(bref, entries, visited, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        for (var i = 0; i < header.CEnt; i++)
        {
            var offset = i * header.CbEnt;
            var entry = ParseBbtEntry(page, offset);
            var key = entry.Bid.NormalizeForLookup();
            entries[key] = entry;
        }
    }

    /// <summary>
    /// Membaca page NBT secara rekursif.
    /// </summary>
    /// <param name="root">BREF halaman.</param>
    /// <param name="entries">Dictionary hasil.</param>
    /// <param name="visited">Set halaman yang sudah dibaca.</param>
    private void ReadNbtPage(Bref root, IDictionary<uint, NbtEntry> entries, ISet<ulong> visited)
    {
        if (!visited.Add(root.Ib))
        {
            return;
        }

        var page = ReadPage(root.Ib);
        var header = ParsePageHeader(page);
        if (header.CLevel > 0)
        {
            ReadIntermediateEntries(page, header, bref => ReadNbtPage(bref, entries, visited));
            return;
        }

        for (var i = 0; i < header.CEnt; i++)
        {
            var offset = i * header.CbEnt;
            var entry = ParseNbtEntry(page, offset);
            entries[entry.Nid.Value] = entry;
        }
    }

    /// <summary>
    /// Membaca page NBT secara rekursif secara asynchronous.
    /// </summary>
    /// <param name="root">BREF halaman.</param>
    /// <param name="entries">Dictionary hasil.</param>
    /// <param name="visited">Set halaman yang sudah dibaca.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private async Task ReadNbtPageAsync(
        Bref root,
        IDictionary<uint, NbtEntry> entries,
        ISet<ulong> visited,
        CancellationToken cancellationToken)
    {
        if (!visited.Add(root.Ib))
        {
            return;
        }

        var page = await ReadPageAsync(root.Ib, cancellationToken).ConfigureAwait(false);
        var header = ParsePageHeader(page);
        if (header.CLevel > 0)
        {
            await ReadIntermediateEntriesAsync(page, header, bref => ReadNbtPageAsync(bref, entries, visited, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        for (var i = 0; i < header.CEnt; i++)
        {
            var offset = i * header.CbEnt;
            var entry = ParseNbtEntry(page, offset);
            entries[entry.Nid.Value] = entry;
        }
    }

    /// <summary>
    /// Membaca halaman B-Tree ukuran 512 byte.
    /// </summary>
    /// <param name="ib">Offset byte absolut halaman.</param>
    /// <returns>Buffer halaman.</returns>
    private byte[] ReadPage(ulong ib)
    {
        var buffer = new byte[512];
        _stream.Seek((long)ib, SeekOrigin.Begin);
        var read = _stream.Read(buffer, 0, buffer.Length);
        if (read != buffer.Length)
        {
            throw new InvalidDataException("Gagal membaca halaman B-Tree.");
        }

        return buffer;
    }

    /// <summary>
    /// Membaca halaman B-Tree ukuran 512 byte secara asynchronous.
    /// </summary>
    /// <param name="ib">Offset byte absolut halaman.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Buffer halaman.</returns>
    private async Task<byte[]> ReadPageAsync(ulong ib, CancellationToken cancellationToken)
    {
        var buffer = new byte[512];
        _stream.Seek((long)ib, SeekOrigin.Begin);
        await ReadExactlyAsync(_stream, buffer, cancellationToken).ConfigureAwait(false);
        return buffer;
    }

    /// <summary>
    /// Mengurai header BTPAGE.
    /// </summary>
    /// <param name="page">Buffer halaman.</param>
    /// <returns>Informasi header halaman.</returns>
    private PageHeader ParsePageHeader(byte[] page)
    {
        var trailerSize = _format == PstFormat.Unicode ? 16 : 12;
        var paddingSize = _format == PstFormat.Unicode ? 4 : 0;
        var entryArea = page.Length - trailerSize - paddingSize - 4;
        var cEnt = page[entryArea];
        var cEntMax = page[entryArea + 1];
        var cbEnt = page[entryArea + 2];
        var cLevel = page[entryArea + 3];
        return new PageHeader(entryArea, cEnt, cEntMax, cbEnt, cLevel);
    }

    /// <summary>
    /// Membaca entri intermediate dan melakukan traversal rekursif.
    /// </summary>
    /// <param name="page">Buffer halaman.</param>
    /// <param name="header">Header halaman.</param>
    /// <param name="onChild">Aksi untuk child BREF.</param>
    private void ReadIntermediateEntries(byte[] page, PageHeader header, Action<Bref> onChild)
    {
        for (var i = 0; i < header.CEnt; i++)
        {
            var offset = i * header.CbEnt;
            var bref = ParseBtEntryBref(page, offset);
            onChild(bref);
        }
    }

    /// <summary>
    /// Membaca entri intermediate dan melakukan traversal rekursif secara asynchronous.
    /// </summary>
    /// <param name="page">Buffer halaman.</param>
    /// <param name="header">Header halaman.</param>
    /// <param name="onChild">Aksi async untuk child BREF.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private async Task ReadIntermediateEntriesAsync(
        byte[] page,
        PageHeader header,
        Func<Bref, Task> onChild,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < header.CEnt; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var offset = i * header.CbEnt;
            var bref = ParseBtEntryBref(page, offset);
            await onChild(bref).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Membaca buffer hingga penuh dari stream.
    /// </summary>
    /// <param name="stream">Stream sumber.</param>
    /// <param name="buffer">Buffer tujuan.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private static async Task ReadExactlyAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.Slice(total), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new InvalidDataException("Gagal membaca halaman B-Tree.");
            }

            total += read;
        }
    }

    /// <summary>
    /// Mengurai BREF dari BTENTRY (intermediate entry).
    /// </summary>
    /// <param name="page">Buffer halaman.</param>
    /// <param name="offset">Offset entry.</param>
    /// <returns>BREF anak.</returns>
    private Bref ParseBtEntryBref(byte[] page, int offset)
    {
        if (_format == PstFormat.Unicode)
        {
            var brefOffset = offset + 8;
            var bid = BitConverter.ToUInt64(page, brefOffset);
            var ib = BitConverter.ToUInt64(page, brefOffset + 8);
            return new Bref(new Bid(bid), ib);
        }

        var brefOffsetAnsi = offset + 4;
        var bidAnsi = BitConverter.ToUInt32(page, brefOffsetAnsi);
        var ibAnsi = BitConverter.ToUInt32(page, brefOffsetAnsi + 4);
        return new Bref(new Bid(bidAnsi), ibAnsi);
    }

    /// <summary>
    /// Mengurai entri BBT pada halaman leaf.
    /// </summary>
    /// <param name="page">Buffer halaman.</param>
    /// <param name="offset">Offset entry.</param>
    /// <returns>Entri BBT.</returns>
    private BbtEntry ParseBbtEntry(byte[] page, int offset)
    {
        if (_format == PstFormat.Unicode)
        {
            var bid = BitConverter.ToUInt64(page, offset);
            var ib = BitConverter.ToUInt64(page, offset + 8);
            var cb = BitConverter.ToUInt16(page, offset + 16);
            var cref = BitConverter.ToUInt16(page, offset + 18);
            return new BbtEntry(new Bid(bid), ib, cb, cref);
        }

        var bidAnsi = BitConverter.ToUInt32(page, offset);
        var ibAnsi = BitConverter.ToUInt32(page, offset + 4);
        var cbAnsi = BitConverter.ToUInt16(page, offset + 8);
        var crefAnsi = BitConverter.ToUInt16(page, offset + 10);
        return new BbtEntry(new Bid(bidAnsi), ibAnsi, cbAnsi, crefAnsi);
    }

    /// <summary>
    /// Mengurai entri NBT pada halaman leaf.
    /// </summary>
    /// <param name="page">Buffer halaman.</param>
    /// <param name="offset">Offset entry.</param>
    /// <returns>Entri NBT.</returns>
    private NbtEntry ParseNbtEntry(byte[] page, int offset)
    {
        if (_format == PstFormat.Unicode)
        {
            var nidRaw = BitConverter.ToUInt64(page, offset);
            var bidData = BitConverter.ToUInt64(page, offset + 8);
            var bidSub = BitConverter.ToUInt64(page, offset + 16);
            var nidParent = BitConverter.ToUInt32(page, offset + 24);
            return new NbtEntry(new Nid((uint)nidRaw), new Bid(bidData), new Bid(bidSub), new Nid(nidParent));
        }

        var nidAnsi = BitConverter.ToUInt32(page, offset);
        var bidDataAnsi = BitConverter.ToUInt32(page, offset + 4);
        var bidSubAnsi = BitConverter.ToUInt32(page, offset + 8);
        var nidParentAnsi = BitConverter.ToUInt32(page, offset + 12);
        return new NbtEntry(new Nid(nidAnsi), new Bid(bidDataAnsi), new Bid(bidSubAnsi), new Nid(nidParentAnsi));
    }

    /// <summary>
    /// Metadata header halaman BTPAGE.
    /// </summary>
    private readonly struct PageHeader
    {
        /// <summary>
        /// Membuat metadata header halaman.
        /// </summary>
        /// <param name="entryAreaSize">Ukuran area entry.</param>
        /// <param name="cEnt">Jumlah entry.</param>
        /// <param name="cEntMax">Kapasitas entry.</param>
        /// <param name="cbEnt">Ukuran entry.</param>
        /// <param name="cLevel">Level halaman.</param>
        public PageHeader(int entryAreaSize, byte cEnt, byte cEntMax, byte cbEnt, byte cLevel)
        {
            EntryAreaSize = entryAreaSize;
            CEnt = cEnt;
            CEntMax = cEntMax;
            CbEnt = cbEnt;
            CLevel = cLevel;
        }

        /// <summary>
        /// Ukuran area entry pada halaman.
        /// </summary>
        public int EntryAreaSize { get; }

        /// <summary>
        /// Jumlah entry pada halaman.
        /// </summary>
        public byte CEnt { get; }

        /// <summary>
        /// Kapasitas maksimal entry.
        /// </summary>
        public byte CEntMax { get; }

        /// <summary>
        /// Ukuran satu entry.
        /// </summary>
        public byte CbEnt { get; }

        /// <summary>
        /// Level halaman BTree.
        /// </summary>
        public byte CLevel { get; }
    }
}
