using System;
using System.Collections.Generic;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Hasil penulisan LTP yang berisi blok data dan subnode tambahan.
/// </summary>
internal sealed class LtpWriteResult
{
    /// <summary>
    /// Membuat hasil penulisan LTP.
    /// </summary>
    /// <param name="blocks">Blok data utama.</param>
    /// <param name="subnodes">Daftar subnode untuk nilai besar.</param>
    public LtpWriteResult(IReadOnlyList<PstDataBlock> blocks, IReadOnlyList<LtpSubnodeData> subnodes)
    {
        Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        Subnodes = subnodes ?? throw new ArgumentNullException(nameof(subnodes));
    }

    /// <summary>
    /// Daftar blok data utama hasil penulisan LTP.
    /// </summary>
    public IReadOnlyList<PstDataBlock> Blocks { get; }

    /// <summary>
    /// Daftar subnode yang menyimpan nilai besar.
    /// </summary>
    public IReadOnlyList<LtpSubnodeData> Subnodes { get; }
}

/// <summary>
/// Representasi subnode LTP untuk menyimpan payload besar.
/// </summary>
internal sealed class LtpSubnodeData
{
    /// <summary>
    /// Membuat subnode LTP.
    /// </summary>
    /// <param name="localNid">NID lokal subnode.</param>
    /// <param name="data">Data subnode.</param>
    public LtpSubnodeData(Nid localNid, ReadOnlyMemory<byte> data)
    {
        LocalNid = localNid;
        Data = data;
    }

    /// <summary>
    /// NID lokal subnode.
    /// </summary>
    public Nid LocalNid { get; }

    /// <summary>
    /// Data subnode.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
}
