using System;
using System.Collections.Generic;
using System.IO;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer header NDB untuk pembaruan metadata dasar.
/// </summary>
internal sealed class NdbHeaderWriter
{
    private const uint SignatureMagic = 0x4E444221;
    private const ushort UnicodeClientSignature = 0x4D53;
    private const ushort UnicodeVersion = 0x0017;
    private const ushort UnicodeVersionMinor = 0x0013;
    private readonly Stream _stream;
    private const int UnicodeBidNextPOffset = 0x20;
    private const int UnicodeDwUniqueOffset = 0x28;
    private const int UnicodeBidNextBOffset = 0x204;
    private const int UnicodeDwCrcPartialOffset = 0x04;
    private const int UnicodeDwCrcFullOffset = 0x20C;
    private const int UnicodeCrcPartialDataOffset = 0x08;
    private const int UnicodeCrcPartialLength = 471;
    private const int UnicodeCrcFullDataOffset = 0x08;
    private const int UnicodeCrcFullLength = 516;
    private const int AnsiBidNextBOffset = 0x1C;
    private const int AnsiBidNextPOffset = 0x20;
    private const int AnsiDwUniqueOffset = 0x24;
    private const int AnsiDwCrcPartialOffset = 0x04;
    private const int AnsiCrcPartialDataOffset = 0x08;
    private const int AnsiCrcPartialLength = 467;
    private const int UnicodeRootOffset = 0xB4;
    private const int AnsiRootOffset = 0xA4;
    private const int UnicodeRootSize = 72;
    private const int AnsiRootSize = 40;
    private const byte InvalidAmapValue = 0x00;
    private const byte ValidAmapValue = 0x02;

    /// <summary>
    /// Membuat writer header NDB.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    public NdbHeaderWriter(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <summary>
    /// Menulis skeleton header PST baru untuk kebutuhan bootstrap file kosong.
    /// </summary>
    /// <param name="stream">Stream PST target.</param>
    /// <param name="format">Format PST yang akan dibuat.</param>
    /// <param name="cryptMethod">Metode enkripsi/encoding default.</param>
    /// <returns>Metadata header dasar hasil bootstrap.</returns>
    public static PstHeaderInfo InitializeEmptyHeader(
        Stream stream,
        PstFormat format = PstFormat.Unicode,
        PstCryptMethod cryptMethod = PstCryptMethod.Permute)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanWrite || !stream.CanSeek)
        {
            throw new NotSupportedException("Stream PST harus writable dan seekable untuk bootstrap header.");
        }

        if (format != PstFormat.Unicode && format != PstFormat.Ansi)
        {
            throw new ArgumentOutOfRangeException(nameof(format), "Format PST tidak didukung untuk bootstrap.");
        }

        var blockSize = format == PstFormat.Unicode ? 8192 : 512;
        stream.SetLength(blockSize);

        var header = new byte[512];
        BitConverter.TryWriteBytes(header.AsSpan(0, 4), SignatureMagic);
        BitConverter.TryWriteBytes(header.AsSpan(8, 2), UnicodeClientSignature);
        BitConverter.TryWriteBytes(header.AsSpan(10, 2), format == PstFormat.Unicode ? UnicodeVersion : (ushort)0x000E);
        BitConverter.TryWriteBytes(header.AsSpan(12, 2), UnicodeVersionMinor);
        header[14] = 0x01;
        header[15] = 0x01;

        stream.Seek(0, SeekOrigin.Begin);
        stream.Write(header, 0, header.Length);

        var cryptOffset = GetCryptOffset(format);
        stream.Seek(cryptOffset, SeekOrigin.Begin);
        stream.WriteByte((byte)cryptMethod);

        stream.Flush();
        return new PstHeaderInfo(
            SignatureMagic,
            clientSignature: UnicodeClientSignature,
            version: format == PstFormat.Unicode ? UnicodeVersion : (ushort)0x000E,
            versionMinor: UnicodeVersionMinor,
            fileSize: stream.Length,
            format,
            cryptMethod);
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
        var brefNbtOffset = format == PstFormat.Unicode ? rootOffset + 36 : rootOffset + 20;
        var brefBbtOffset = format == PstFormat.Unicode ? rootOffset + 52 : rootOffset + 28;

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

    /// <summary>
    /// Memperbarui nilai bidNextB dan bidNextP pada header.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="nextBlockBidRaw">Nilai BID block berikutnya.</param>
    /// <param name="nextPageBidRaw">Nilai BID page berikutnya.</param>
    public void UpdateBidCounters(PstFormat format, ulong nextBlockBidRaw, ulong nextPageBidRaw)
    {
        if (format == PstFormat.Unicode)
        {
            WriteUInt64(UnicodeBidNextBOffset, nextBlockBidRaw);
            WriteUInt64(UnicodeBidNextPOffset, nextPageBidRaw);
            IncrementDwUnique(UnicodeDwUniqueOffset);
            return;
        }

        WriteUInt32(AnsiBidNextBOffset, (uint)nextBlockBidRaw);
        WriteUInt32(AnsiBidNextPOffset, (uint)nextPageBidRaw);
        IncrementDwUnique(AnsiDwUniqueOffset);
    }

    /// <summary>
    /// Memperbarui ukuran file pada ROOT.ibFileEof di header.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="fileSize">Ukuran file terbaru.</param>
    public void UpdateFileSizeOnDisk(PstFormat format, ulong fileSize)
    {
        var rootOffset = format == PstFormat.Unicode ? UnicodeRootOffset : AnsiRootOffset;
        var ibFileEofOffset = rootOffset + 4;
        if (format == PstFormat.Unicode)
        {
            WriteUInt64(ibFileEofOffset, fileSize);
            return;
        }

        WriteUInt32(ibFileEofOffset, (uint)fileSize);
    }

    /// <summary>
    /// Memperbarui field ROOT terkait alokasi file.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="ibFileEof">Nilai ROOT.ibFileEof.</param>
    /// <param name="ibAMapLast">Nilai ROOT.ibAMapLast.</param>
    /// <param name="cbAMapFree">Nilai ROOT.cbAMapFree.</param>
    /// <param name="cbPMapFree">Nilai ROOT.cbPMapFree.</param>
    public void UpdateRootAllocationMetadata(
        PstFormat format,
        ulong ibFileEof,
        ulong ibAMapLast,
        ulong cbAMapFree,
        ulong cbPMapFree)
    {
        var rootOffset = format == PstFormat.Unicode ? UnicodeRootOffset : AnsiRootOffset;
        if (format == PstFormat.Unicode)
        {
            WriteUInt64(rootOffset + 4, ibFileEof);
            WriteUInt64(rootOffset + 12, ibAMapLast);
            WriteUInt64(rootOffset + 20, cbAMapFree);
            WriteUInt64(rootOffset + 28, cbPMapFree);
            return;
        }

        WriteUInt32(rootOffset + 4, (uint)ibFileEof);
        WriteUInt32(rootOffset + 8, (uint)ibAMapLast);
        WriteUInt32(rootOffset + 12, (uint)cbAMapFree);
        WriteUInt32(rootOffset + 16, (uint)cbPMapFree);
    }

    /// <summary>
    /// Memperbarui nilai array rgnid[] pada header.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="counters">Counter rgnid[].</param>
    public void UpdateRgnidCounters(PstFormat format, IReadOnlyList<uint> counters)
    {
        if (counters is null)
        {
            throw new ArgumentNullException(nameof(counters));
        }

        var rgnidOffset = format == PstFormat.Unicode ? 0x2C : 0x28;
        var max = Math.Min(32, counters.Count);
        for (var i = 0; i < max; i++)
        {
            WriteUInt32(rgnidOffset + (i * sizeof(uint)), counters[i]);
        }
    }

    /// <summary>
    /// Memperbarui state ROOT.fAMapValid pada header.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="isValid">True bila AMap valid; false bila invalid.</param>
    public void SetAMapValid(PstFormat format, bool isValid)
    {
        var rootOffset = format == PstFormat.Unicode ? UnicodeRootOffset : AnsiRootOffset;
        var fAmapValidOffset = format == PstFormat.Unicode ? rootOffset + 68 : rootOffset + 36;
        WriteByte(fAmapValidOffset, isValid ? ValidAmapValue : InvalidAmapValue);
    }

    /// <summary>
    /// Memperbarui nilai CRC header sesuai area checksum untuk format PST.
    /// </summary>
    /// <param name="format">Format PST.</param>
    public void UpdateHeaderCrcs(PstFormat format)
    {
        if (format == PstFormat.Unicode)
        {
            var partial = ReadBytes(UnicodeCrcPartialDataOffset, UnicodeCrcPartialLength);
            var partialCrc = NdbIntegrity.ComputeCrc(0, partial);
            WriteUInt32(UnicodeDwCrcPartialOffset, partialCrc);

            var full = ReadBytes(UnicodeCrcFullDataOffset, UnicodeCrcFullLength);
            var fullCrc = NdbIntegrity.ComputeCrc(0, full);
            WriteUInt32(UnicodeDwCrcFullOffset, fullCrc);
            return;
        }

        var ansiPartial = ReadBytes(AnsiCrcPartialDataOffset, AnsiCrcPartialLength);
        var ansiPartialCrc = NdbIntegrity.ComputeCrc(0, ansiPartial);
        WriteUInt32(AnsiDwCrcPartialOffset, ansiPartialCrc);
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

    private static int GetCryptOffset(PstFormat format)
    {
        var rootOffset = format == PstFormat.Unicode ? UnicodeRootOffset : AnsiRootOffset;
        var rootSize = format == PstFormat.Unicode ? UnicodeRootSize : AnsiRootSize;
        var align = format == PstFormat.Unicode ? 4 : 0;
        return rootOffset + rootSize + align + 128 + 128 + 1;
    }

    private byte[] ReadBytes(long offset, int length)
    {
        var buffer = new byte[length];
        _stream.Seek(offset, SeekOrigin.Begin);
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = _stream.Read(buffer, totalRead, buffer.Length - totalRead);
            if (read <= 0)
            {
                throw new EndOfStreamException("Gagal membaca data header untuk perhitungan CRC.");
            }

            totalRead += read;
        }

        return buffer;
    }

    private void IncrementDwUnique(long offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        Span<byte> buffer = stackalloc byte[4];
        var read = _stream.Read(buffer);
        if (read != 4)
        {
            throw new EndOfStreamException("Gagal membaca dwUnique pada header.");
        }

        var value = BitConverter.ToUInt32(buffer);
        value++;
        WriteUInt32(offset, value);
    }

    private void WriteUInt32(long offset, uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BitConverter.TryWriteBytes(buffer, value);
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.Write(buffer);
    }

    private void WriteUInt64(long offset, ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BitConverter.TryWriteBytes(buffer, value);
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.Write(buffer);
    }

    private void WriteByte(long offset, byte value)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.WriteByte(value);
    }
}
