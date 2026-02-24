using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Metadata header NDB yang dibutuhkan untuk mengakses BBT/NBT dan blok.
/// </summary>
internal sealed class NdbHeader
{
    /// <summary>
    /// Membuat metadata header NDB.
    /// </summary>
    /// <param name="headerInfo">Metadata header yang terekspos ke domain.</param>
    /// <param name="nbtRoot">Referensi root NBT.</param>
    /// <param name="bbtRoot">Referensi root BBT.</param>
    /// <param name="counters">Counter header terkait BID/NID.</param>
    /// <param name="rootState">State ROOT terkait alokasi dan validasi AMap.</param>
    public NdbHeader(
        PstHeaderInfo headerInfo,
        Bref nbtRoot,
        Bref bbtRoot,
        NdbHeaderCounters counters,
        NdbRootState rootState)
    {
        HeaderInfo = headerInfo;
        NbtRoot = nbtRoot;
        BbtRoot = bbtRoot;
        Counters = counters;
        RootState = rootState;
    }

    /// <summary>
    /// Metadata header PST yang terekspos ke domain.
    /// </summary>
    public PstHeaderInfo HeaderInfo { get; }

    /// <summary>
    /// Referensi root Node B-Tree (NBT).
    /// </summary>
    public Bref NbtRoot { get; }

    /// <summary>
    /// Referensi root Block B-Tree (BBT).
    /// </summary>
    public Bref BbtRoot { get; }

    /// <summary>
    /// Counter header terkait alokasi BID dan NID.
    /// </summary>
    public NdbHeaderCounters Counters { get; }

    /// <summary>
    /// State ROOT terkait alokasi file dan status AMap.
    /// </summary>
    public NdbRootState RootState { get; }
}

/// <summary>
/// Snapshot counter header NDB yang dibutuhkan writer.
/// </summary>
internal sealed class NdbHeaderCounters
{
    /// <summary>
    /// Membuat snapshot counter header.
    /// </summary>
    /// <param name="nextBlockBidRaw">Nilai raw bidNextB (BID block berikutnya).</param>
    /// <param name="nextPageBidRaw">Nilai raw bidNextP (BID page berikutnya).</param>
    /// <param name="nidCounters">Array rgnid[] berisi counter NID per tipe.</param>
    public NdbHeaderCounters(ulong nextBlockBidRaw, ulong nextPageBidRaw, uint[] nidCounters)
    {
        NextBlockBidRaw = nextBlockBidRaw;
        NextPageBidRaw = nextPageBidRaw;
        NidCounters = nidCounters ?? [];
    }

    /// <summary>
    /// Nilai raw bidNextB pada header.
    /// </summary>
    public ulong NextBlockBidRaw { get; }

    /// <summary>
    /// Nilai raw bidNextP pada header.
    /// </summary>
    public ulong NextPageBidRaw { get; }

    /// <summary>
    /// Counter rgnid[] pada header.
    /// </summary>
    public uint[] NidCounters { get; }
}

/// <summary>
/// Snapshot field ROOT yang terkait state alokasi dan integritas AMap.
/// </summary>
internal sealed class NdbRootState
{
    /// <summary>
    /// Membuat snapshot state ROOT.
    /// </summary>
    /// <param name="ibFileEof">Ukuran file PST pada ROOT.ibFileEof.</param>
    /// <param name="ibAMapLast">Offset AMap terakhir pada ROOT.ibAMapLast.</param>
    /// <param name="cbAMapFree">Total free-space AMap pada ROOT.cbAMapFree.</param>
    /// <param name="cbPMapFree">Total free-space PMap pada ROOT.cbPMapFree.</param>
    /// <param name="isAMapValid">Status validitas AMap dari ROOT.fAMapValid.</param>
    public NdbRootState(ulong ibFileEof, ulong ibAMapLast, ulong cbAMapFree, ulong cbPMapFree, bool isAMapValid)
    {
        IbFileEof = ibFileEof;
        IbAMapLast = ibAMapLast;
        CbAMapFree = cbAMapFree;
        CbPMapFree = cbPMapFree;
        IsAMapValid = isAMapValid;
    }

    /// <summary>
    /// Nilai ROOT.ibFileEof.
    /// </summary>
    public ulong IbFileEof { get; }

    /// <summary>
    /// Nilai ROOT.ibAMapLast.
    /// </summary>
    public ulong IbAMapLast { get; }

    /// <summary>
    /// Nilai ROOT.cbAMapFree.
    /// </summary>
    public ulong CbAMapFree { get; }

    /// <summary>
    /// Nilai ROOT.cbPMapFree.
    /// </summary>
    public ulong CbPMapFree { get; }

    /// <summary>
    /// Menandakan ROOT.fAMapValid berada pada state valid.
    /// </summary>
    public bool IsAMapValid { get; }
}
