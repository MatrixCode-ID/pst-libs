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
    public NdbHeader(PstHeaderInfo headerInfo, Bref nbtRoot, Bref bbtRoot)
    {
        HeaderInfo = headerInfo;
        NbtRoot = nbtRoot;
        BbtRoot = bbtRoot;
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
}
