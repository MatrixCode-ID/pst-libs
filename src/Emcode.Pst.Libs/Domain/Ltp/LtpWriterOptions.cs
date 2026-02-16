using System;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Opsi konfigurasi writer LTP untuk pembuatan heap/PC/TC.
/// </summary>
internal sealed class LtpWriterOptions
{
    /// <summary>
    /// Membuat opsi writer LTP.
    /// </summary>
    /// <param name="format">Format PST (ANSI/Unicode).</param>
    /// <param name="blockSize">Ukuran block heap yang digunakan.</param>
    /// <param name="clientSignature">Client signature HN.</param>
    /// <param name="maxInlineValueBytes">Batas maksimum inline value sebelum dialihkan ke subnode.</param>
    public LtpWriterOptions(PstFormat format, ushort blockSize, byte clientSignature = 0, int? maxInlineValueBytes = null)
    {
        if (format == PstFormat.Unknown)
        {
            throw new ArgumentException("Format PST belum terdeteksi.", nameof(format));
        }

        Format = format;
        BlockSize = blockSize;
        ClientSignature = clientSignature;
        MaxInlineValueBytes = maxInlineValueBytes ?? blockSize / 2;
    }

    /// <summary>
    /// Format PST yang dipakai writer.
    /// </summary>
    public PstFormat Format { get; }

    /// <summary>
    /// Ukuran block heap yang akan digunakan.
    /// </summary>
    public ushort BlockSize { get; }

    /// <summary>
    /// Client signature untuk HNHDR.
    /// </summary>
    public byte ClientSignature { get; }

    /// <summary>
    /// Ukuran maksimum nilai variabel yang disimpan inline pada heap sebelum dialihkan ke subnode.
    /// </summary>
    public int MaxInlineValueBytes { get; }

    /// <summary>
    /// Membuat opsi writer berdasarkan format PST.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <returns>Opsi writer default.</returns>
    public static LtpWriterOptions CreateDefault(PstFormat format)
    {
        var blockSize = format switch
        {
            PstFormat.Ansi => (ushort)512,
            PstFormat.Unicode => (ushort)8192,
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Format PST tidak didukung.")
        };

        return new LtpWriterOptions(format, blockSize);
    }
}
