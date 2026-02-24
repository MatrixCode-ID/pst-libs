using System;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Utilitas perhitungan checksum dan signature untuk struktur NDB.
/// </summary>
internal static class NdbIntegrity
{
    private static readonly uint[] CrcTable = BuildCrcTable();

    /// <summary>
    /// Menghitung CRC-32 untuk buffer dengan seed awal tertentu.
    /// </summary>
    /// <param name="seed">Seed CRC awal.</param>
    /// <param name="data">Data yang dihitung.</param>
    /// <returns>Nilai CRC hasil perhitungan.</returns>
    public static uint ComputeCrc(uint seed, ReadOnlySpan<byte> data)
    {
        var crc = seed;
        foreach (var value in data)
        {
            crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    /// <summary>
    /// Menghitung signature block/page sesuai rumus pada spesifikasi PST.
    /// </summary>
    /// <param name="ib">Offset absolut block/page.</param>
    /// <param name="bid">BID block/page.</param>
    /// <returns>Nilai signature 16-bit.</returns>
    public static ushort ComputeSignature(ulong ib, Bid bid)
    {
        var value = ib ^ bid.Raw;
        return (ushort)(((value >> 16) & 0xFFFF) ^ (value & 0xFFFF));
    }

    /// <summary>
    /// Menulis BLOCKTRAILER ke posisi akhir block.
    /// </summary>
    /// <param name="target">Buffer block target.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cb">Ukuran data mentah.</param>
    /// <param name="crc">CRC data mentah.</param>
    /// <param name="signature">Nilai signature block.</param>
    /// <param name="bid">BID block.</param>
    public static void WriteBlockTrailer(
        Span<byte> target,
        PstFormat format,
        ushort cb,
        uint crc,
        ushort signature,
        Bid bid)
    {
        if (format == PstFormat.Unicode)
        {
            // Unicode: cb(2), wSig(2), dwCRC(4), bid(8)
            BitConverter.TryWriteBytes(target.Slice(0, 2), cb);
            BitConverter.TryWriteBytes(target.Slice(2, 2), signature);
            BitConverter.TryWriteBytes(target.Slice(4, 4), crc);
            BitConverter.TryWriteBytes(target.Slice(8, 8), bid.Raw);
            return;
        }

        // ANSI: cb(2), wSig(2), bid(4), dwCRC(4)
        BitConverter.TryWriteBytes(target.Slice(0, 2), cb);
        BitConverter.TryWriteBytes(target.Slice(2, 2), signature);
        BitConverter.TryWriteBytes(target.Slice(4, 4), (uint)bid.Raw);
        BitConverter.TryWriteBytes(target.Slice(8, 4), crc);
    }

    /// <summary>
    /// Menulis PAGETRAILER ke posisi akhir page.
    /// </summary>
    /// <param name="target">Buffer page target.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="ptype">Jenis page.</param>
    /// <param name="crc">CRC data page tanpa trailer.</param>
    /// <param name="signature">Nilai signature page.</param>
    /// <param name="bid">BID page.</param>
    public static void WritePageTrailer(
        Span<byte> target,
        PstFormat format,
        byte ptype,
        uint crc,
        ushort signature,
        Bid bid)
    {
        if (format == PstFormat.Unicode)
        {
            // Unicode: ptype(1), ptypeRepeat(1), wSig(2), dwCRC(4), bid(8)
            target[0] = ptype;
            target[1] = ptype;
            BitConverter.TryWriteBytes(target.Slice(2, 2), signature);
            BitConverter.TryWriteBytes(target.Slice(4, 4), crc);
            BitConverter.TryWriteBytes(target.Slice(8, 8), bid.Raw);
            return;
        }

        // ANSI: ptype(1), ptypeRepeat(1), wSig(2), bid(4), dwCRC(4)
        target[0] = ptype;
        target[1] = ptype;
        BitConverter.TryWriteBytes(target.Slice(2, 2), signature);
        BitConverter.TryWriteBytes(target.Slice(4, 4), (uint)bid.Raw);
        BitConverter.TryWriteBytes(target.Slice(8, 4), crc);
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < table.Length; i++)
        {
            var crc = i;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0
                    ? 0xEDB88320U ^ (crc >> 1)
                    : crc >> 1;
            }

            table[i] = crc;
        }

        return table;
    }
}

/// <summary>
/// Nilai ptype yang digunakan pada PAGETRAILER.
/// </summary>
internal static class NdbPageType
{
    /// <summary>
    /// Block B-Tree page.
    /// </summary>
    public const byte Bbt = 0x80;

    /// <summary>
    /// Node B-Tree page.
    /// </summary>
    public const byte Nbt = 0x81;

    /// <summary>
    /// Allocation Map (AMap) page.
    /// </summary>
    public const byte Amap = 0x84;
}
