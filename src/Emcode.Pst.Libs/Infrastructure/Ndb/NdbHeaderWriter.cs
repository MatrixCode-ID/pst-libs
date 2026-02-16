using System;
using System.IO;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer header NDB untuk pembaruan metadata dasar.
/// </summary>
internal sealed class NdbHeaderWriter
{
    private readonly Stream _stream;
    private const int UnicodeRootOffset = 0xB4;
    private const int AnsiRootOffset = 0xA4;
    private const int UnicodeRootSize = 72;
    private const int AnsiRootSize = 40;

    /// <summary>
    /// Membuat writer header NDB.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    public NdbHeaderWriter(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <summary>
    /// Memperbarui pointer root BBT/NBT pada header PST.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="bbtRoot">BREF root BBT baru.</param>
    /// <param name="nbtRoot">BREF root NBT baru.</param>
    public void UpdateBtreeRoots(PstFormat format, Bref bbtRoot, Bref nbtRoot)
    {
        var rootOffset = format == PstFormat.Unicode ? UnicodeRootOffset : AnsiRootOffset;
        var rootSize = format == PstFormat.Unicode ? UnicodeRootSize : AnsiRootSize;
        var brefNbtOffset = rootOffset + 36;
        var brefBbtOffset = rootOffset + 52;

        WriteBref(brefNbtOffset, format, nbtRoot);
        WriteBref(brefBbtOffset, format, bbtRoot);

        var align = format == PstFormat.Unicode ? 4 : 0;
        var trailerOffset = rootOffset + rootSize + align + 128 + 128;
        _stream.Seek(trailerOffset, SeekOrigin.Begin);
    }

    /// <summary>
    /// Memperbarui ukuran file pada metadata header (in-memory).
    /// </summary>
    /// <param name="header">Header PST.</param>
    /// <returns>Header PST dengan ukuran file terbaru.</returns>
    public PstHeaderInfo UpdateFileSize(PstHeaderInfo header)
    {
        var size = _stream.Length;
        return new PstHeaderInfo(header.Signature, header.ClientSignature, header.Version, header.VersionMinor, size, header.Format, header.CryptMethod);
    }

    private void WriteBref(long offset, PstFormat format, Bref bref)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        if (format == PstFormat.Unicode)
        {
            var buffer = new byte[16];
            BitConverter.TryWriteBytes(buffer.AsSpan(0, 8), bref.Bid.Raw);
            BitConverter.TryWriteBytes(buffer.AsSpan(8, 8), bref.Ib);
            _stream.Write(buffer, 0, buffer.Length);
            return;
        }

        var bufferAnsi = new byte[8];
        BitConverter.TryWriteBytes(bufferAnsi.AsSpan(0, 4), (uint)bref.Bid.Raw);
        BitConverter.TryWriteBytes(bufferAnsi.AsSpan(4, 4), (uint)bref.Ib);
        _stream.Write(bufferAnsi, 0, bufferAnsi.Length);
    }
}
