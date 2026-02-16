using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Reader blok data berdasarkan BBT dan metode enkripsi PST.
/// </summary>
internal sealed class PstBlockReader
{
    private readonly Stream _stream;
    private readonly PstFormat _format;
    private readonly PstCryptMethod _cryptMethod;
    private readonly IReadOnlyDictionary<ulong, BbtEntry> _bbt;

    /// <summary>
    /// Membuat reader blok data.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cryptMethod">Metode enkripsi/encoding.</param>
    /// <param name="bbt">Dictionary BBT.</param>
    public PstBlockReader(Stream stream, PstFormat format, PstCryptMethod cryptMethod, IReadOnlyDictionary<ulong, BbtEntry> bbt)
    {
        _stream = stream;
        _format = format;
        _cryptMethod = cryptMethod;
        _bbt = bbt;
    }

    /// <summary>
    /// Membaca data node sebagai daftar blok data dalam urutan.
    /// </summary>
    /// <param name="bid">BID awal data node.</param>
    /// <returns>Daftar blok data.</returns>
    public IReadOnlyList<PstDataBlock> ReadDataBlocks(Bid bid)
    {
        return ReadDataBlocks(bid, out _);
    }

    /// <summary>
    /// Membaca data node secara asynchronous sebagai daftar blok data dalam urutan.
    /// </summary>
    /// <param name="bid">BID awal data node.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar blok data.</returns>
    public async Task<IReadOnlyList<PstDataBlock>> ReadDataBlocksAsync(Bid bid, CancellationToken cancellationToken = default)
    {
        var result = await ReadDataBlocksAsyncWithTotal(bid, cancellationToken).ConfigureAwait(false);
        return result.Blocks;
    }

    /// <summary>
    /// Membaca data node sebagai daftar blok data serta total ukuran dari data tree.
    /// </summary>
    /// <param name="bid">BID awal data node.</param>
    /// <param name="totalLength">Total panjang data berdasarkan lcbTotal bila tersedia.</param>
    /// <returns>Daftar blok data.</returns>
    public IReadOnlyList<PstDataBlock> ReadDataBlocks(Bid bid, out uint totalLength)
    {
        var blocks = new List<PstDataBlock>();
        var declaredTotal = AppendBlocks(bid, blocks, out var declaredLength);
        totalLength = declaredLength ?? declaredTotal;
        return blocks;
    }

    /// <summary>
    /// Membaca data node secara asynchronous sebagai daftar blok data serta total ukuran dari data tree.
    /// </summary>
    /// <param name="bid">BID awal data node.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Tuple berisi daftar blok data dan total panjang data.</returns>
    public async Task<(IReadOnlyList<PstDataBlock> Blocks, uint TotalLength)> ReadDataBlocksAsyncWithTotal(
        Bid bid,
        CancellationToken cancellationToken = default)
    {
        var blocks = new List<PstDataBlock>();
        var result = await AppendBlocksAsync(bid, blocks, cancellationToken).ConfigureAwait(false);
        var totalLength = result.DeclaredTotalLength ?? result.TotalLeafLength;
        return (blocks, totalLength);
    }

    /// <summary>
    /// Menambahkan blok data ke koleksi secara rekursif (mendukung XBLOCK/XXBLOCK).
    /// </summary>
    /// <param name="bid">BID blok.</param>
    /// <param name="blocks">Koleksi hasil.</param>
    /// <param name="declaredTotalLength">Total length yang dideklarasikan oleh XBLOCK/XXBLOCK.</param>
    /// <returns>Jumlah byte hasil penjumlahan leaf data blocks.</returns>
    private uint AppendBlocks(Bid bid, IList<PstDataBlock> blocks, out uint? declaredTotalLength)
    {
        declaredTotalLength = null;
        if (bid.IsZero)
        {
            return 0;
        }

        var entry = GetBbtEntry(bid);
        var data = ReadBlockData(bid, entry);
        if (bid.IsInternal && TryParseDataTree(data, out var totalLength, out var childBids))
        {
            declaredTotalLength = totalLength;
            uint totalLeafLength = 0;
            foreach (var childBid in childBids)
            {
                totalLeafLength += AppendBlocks(childBid, blocks, out _);
            }

            if (totalLength > totalLeafLength)
            {
                throw new InvalidDataException("lcbTotal XBLOCK/XXBLOCK melebihi ukuran data anak.");
            }

            return totalLeafLength;
        }

        blocks.Add(new PstDataBlock(bid, data));
        return (uint)data.Length;
    }

    /// <summary>
    /// Menambahkan blok data ke koleksi secara rekursif secara asynchronous (mendukung XBLOCK/XXBLOCK).
    /// </summary>
    /// <param name="bid">BID blok.</param>
    /// <param name="blocks">Koleksi hasil.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Tuple total panjang data leaf dan total length dari data tree bila ada.</returns>
    private async Task<(uint TotalLeafLength, uint? DeclaredTotalLength)> AppendBlocksAsync(
        Bid bid,
        IList<PstDataBlock> blocks,
        CancellationToken cancellationToken)
    {
        if (bid.IsZero)
        {
            return (0, null);
        }

        var entry = GetBbtEntry(bid);
        var data = await ReadBlockDataAsync(bid, entry, cancellationToken).ConfigureAwait(false);
        if (bid.IsInternal && TryParseDataTree(data, out var totalLength, out var childBids))
        {
            uint totalLeafLength = 0;
            foreach (var childBid in childBids)
            {
                var childResult = await AppendBlocksAsync(childBid, blocks, cancellationToken).ConfigureAwait(false);
                totalLeafLength += childResult.TotalLeafLength;
            }

            if (totalLength > totalLeafLength)
            {
                throw new InvalidDataException("lcbTotal XBLOCK/XXBLOCK melebihi ukuran data anak.");
            }

            return (totalLeafLength, totalLength);
        }

        blocks.Add(new PstDataBlock(bid, data));
        return ((uint)data.Length, null);
    }

    /// <summary>
    /// Mengambil entri BBT untuk BID tertentu.
    /// </summary>
    /// <param name="bid">BID yang dicari.</param>
    /// <returns>Entri BBT.</returns>
    private BbtEntry GetBbtEntry(Bid bid)
    {
        var key = bid.NormalizeForLookup();
        if (_bbt.TryGetValue(key, out var entry))
        {
            return entry;
        }

        throw new InvalidDataException($"BBT entry tidak ditemukan untuk BID {bid}.");
    }

    /// <summary>
    /// Membaca data blok berdasarkan BBT entry dan mendekode sesuai metode crypt untuk blok eksternal.
    /// </summary>
    /// <param name="bid">BID blok.</param>
    /// <param name="entry">Entri BBT.</param>
    /// <returns>Buffer data blok.</returns>
    private byte[] ReadBlockData(Bid bid, BbtEntry entry)
    {
        var buffer = new byte[entry.Cb];
        _stream.Seek((long)entry.Ib, SeekOrigin.Begin);
        var read = _stream.Read(buffer, 0, buffer.Length);
        if (read != buffer.Length)
        {
            throw new InvalidDataException("Gagal membaca data blok.");
        }

        if (!bid.IsInternal)
        {
            NdbCrypt.Decode(_cryptMethod, buffer);
        }

        return buffer;
    }

    /// <summary>
    /// Membaca data blok secara asynchronous berdasarkan BBT entry dan mendekode sesuai metode crypt untuk blok eksternal.
    /// </summary>
    /// <param name="bid">BID blok.</param>
    /// <param name="entry">Entri BBT.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Buffer data blok.</returns>
    private async Task<byte[]> ReadBlockDataAsync(Bid bid, BbtEntry entry, CancellationToken cancellationToken)
    {
        var buffer = new byte[entry.Cb];
        _stream.Seek((long)entry.Ib, SeekOrigin.Begin);
        await ReadExactlyAsync(_stream, buffer, cancellationToken).ConfigureAwait(false);

        if (!bid.IsInternal)
        {
            NdbCrypt.Decode(_cryptMethod, buffer);
        }

        return buffer;
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
                throw new InvalidDataException("Gagal membaca data blok.");
            }

            total += read;
        }
    }

    /// <summary>
    /// Mengurai struktur XBLOCK/XXBLOCK untuk mendapatkan daftar BID child dan total length.
    /// </summary>
    /// <param name="data">Buffer data internal block.</param>
    /// <param name="totalLength">Total length yang dideklarasikan pada XBLOCK/XXBLOCK.</param>
    /// <param name="childBids">Daftar BID child sesuai urutan.</param>
    /// <returns>True jika data merupakan XBLOCK/XXBLOCK.</returns>
    private bool TryParseDataTree(ReadOnlySpan<byte> data, out uint totalLength, out List<Bid> childBids)
    {
        totalLength = 0;
        childBids = new List<Bid>();

        if (data.Length < 8)
        {
            throw new InvalidDataException("Ukuran data XBLOCK/XXBLOCK tidak valid.");
        }

        var btype = data[0];
        if (btype != 0x01)
        {
            return false;
        }

        var cLevel = data[1];
        if (cLevel != 0x01 && cLevel != 0x02)
        {
            throw new InvalidDataException("Level XBLOCK/XXBLOCK tidak valid.");
        }

        var cEnt = BitConverter.ToUInt16(data.Slice(2, 2));
        totalLength = BitConverter.ToUInt32(data.Slice(4, 4));
        var bidSize = _format == PstFormat.Unicode ? 8 : 4;
        var requiredLength = 8 + (cEnt * bidSize);
        if (requiredLength > data.Length)
        {
            throw new InvalidDataException("Ukuran XBLOCK/XXBLOCK tidak cukup untuk semua entry BID.");
        }

        var offset = 8;
        for (var i = 0; i < cEnt; i++)
        {
            var bid = _format == PstFormat.Unicode
                ? new Bid(BitConverter.ToUInt64(data.Slice(offset, bidSize)))
                : new Bid(BitConverter.ToUInt32(data.Slice(offset, bidSize)));
            childBids.Add(bid);
            offset += bidSize;
        }

        return true;
    }
}

/// <summary>
/// Representasi blok data hasil pembacaan dari BBT.
/// </summary>
internal sealed class PstDataBlock
{
    /// <summary>
    /// Membuat instance blok data.
    /// </summary>
    /// <param name="bid">BID blok.</param>
    /// <param name="data">Data blok.</param>
    public PstDataBlock(Bid bid, byte[] data)
    {
        Bid = bid;
        Data = data;
    }

    /// <summary>
    /// BID blok.
    /// </summary>
    public Bid Bid { get; }

    /// <summary>
    /// Data mentah blok (tanpa trailer/padding).
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
}
