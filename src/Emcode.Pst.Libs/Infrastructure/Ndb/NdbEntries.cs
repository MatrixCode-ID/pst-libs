namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Representasi entri leaf pada Block B-Tree (BBT).
/// </summary>
internal sealed class BbtEntry
{
    /// <summary>
    /// Membuat entri BBT.
    /// </summary>
    /// <param name="bid">BID blok.</param>
    /// <param name="ib">Offset byte absolut blok.</param>
    /// <param name="cb">Ukuran data mentah blok.</param>
    /// <param name="cref">Reference count blok.</param>
    public BbtEntry(Bid bid, ulong ib, ushort cb, ushort cref)
    {
        Bid = bid;
        Ib = ib;
        Cb = cb;
        CRef = cref;
    }

    /// <summary>
    /// BID blok yang direferensikan.
    /// </summary>
    public Bid Bid { get; }

    /// <summary>
    /// Offset byte absolut blok di file PST.
    /// </summary>
    public ulong Ib { get; }

    /// <summary>
    /// Ukuran data mentah pada blok (tanpa trailer/padding).
    /// </summary>
    public ushort Cb { get; }

    /// <summary>
    /// Reference count blok pada BBT.
    /// </summary>
    public ushort CRef { get; }
}

/// <summary>
/// Representasi entri leaf pada Node B-Tree (NBT).
/// </summary>
internal sealed class NbtEntry
{
    /// <summary>
    /// Membuat entri NBT.
    /// </summary>
    /// <param name="nid">NID node.</param>
    /// <param name="bidData">BID data node.</param>
    /// <param name="bidSub">BID subnode node.</param>
    /// <param name="nidParent">NID parent dari node.</param>
    public NbtEntry(Nid nid, Bid bidData, Bid bidSub, Nid nidParent)
    {
        Nid = nid;
        BidData = bidData;
        BidSub = bidSub;
        NidParent = nidParent;
    }

    /// <summary>
    /// NID node.
    /// </summary>
    public Nid Nid { get; }

    /// <summary>
    /// BID data node.
    /// </summary>
    public Bid BidData { get; }

    /// <summary>
    /// BID subnode node.
    /// </summary>
    public Bid BidSub { get; }

    /// <summary>
    /// NID parent node.
    /// </summary>
    public Nid NidParent { get; }
}
