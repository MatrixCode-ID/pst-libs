namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Menyimpan metadata hasil alokasi block pada writer NDB.
/// </summary>
internal sealed class NdbBlockAllocation
{
    /// <summary>
    /// Membuat metadata alokasi block.
    /// </summary>
    /// <param name="bid">BID hasil alokasi.</param>
    /// <param name="ib">Offset byte absolut pada file PST.</param>
    /// <param name="dataSize">Ukuran data yang akan ditulis ke block.</param>
    /// <param name="blockSize">Ukuran block PST yang dialokasikan.</param>
    /// <param name="isInternal">Menandakan block internal (XBLOCK/XXBLOCK).</param>
    public NdbBlockAllocation(Bid bid, ulong ib, ushort dataSize, ushort blockSize, bool isInternal)
    {
        Bid = bid;
        Ib = ib;
        DataSize = dataSize;
        BlockSize = blockSize;
        IsInternal = isInternal;
    }

    /// <summary>
    /// BID hasil alokasi.
    /// </summary>
    public Bid Bid { get; }

    /// <summary>
    /// Offset byte absolut pada file PST.
    /// </summary>
    public ulong Ib { get; }

    /// <summary>
    /// Ukuran data yang akan ditulis ke block.
    /// </summary>
    public ushort DataSize { get; }

    /// <summary>
    /// Ukuran block PST yang dialokasikan.
    /// </summary>
    public ushort BlockSize { get; }

    /// <summary>
    /// Menandakan block internal (XBLOCK/XXBLOCK) atau eksternal.
    /// </summary>
    public bool IsInternal { get; }
}
