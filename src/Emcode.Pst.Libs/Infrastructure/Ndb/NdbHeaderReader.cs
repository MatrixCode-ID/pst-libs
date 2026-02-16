using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Parser header PST untuk mendapatkan root BBT/NBT dan metadata dasar.
/// </summary>
internal sealed class NdbHeaderReader
{
    private const uint SignatureMagic = 0x4E444221; // "!BDN" dalam little-endian.
    private const int UnicodeRootOffset = 0xB4;
    private const int AnsiRootOffset = 0xA4;
    private const int UnicodeRootSize = 72;
    private const int AnsiRootSize = 40;

    /// <summary>
    /// Membaca header PST dari stream.
    /// </summary>
    /// <param name="stream">Stream file PST.</param>
    /// <returns>Metadata header NDB.</returns>
    public NdbHeader Read(Stream stream)
    {
        if (stream is null)
        {
            throw new InvalidDataException("Stream PST tidak tersedia.");
        }

        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        stream.Seek(0, SeekOrigin.Begin);
        var signature = reader.ReadUInt32();
        if (signature != SignatureMagic)
        {
            throw new InvalidDataException("Signature PST tidak valid.");
        }

        stream.Seek(8, SeekOrigin.Begin);
        var clientSignature = reader.ReadUInt16();

        stream.Seek(10, SeekOrigin.Begin);
        var version = reader.ReadUInt16();
        var versionMinor = reader.ReadUInt16();

        var format = version >= 0x0017 ? PstFormat.Unicode : PstFormat.Ansi;
        var rootOffset = GetRootOffset(format);
        var rootBrefNbtOffset = rootOffset + 36;
        var rootBrefBbtOffset = rootOffset + 52;

        var brefNbt = ReadBref(stream, rootBrefNbtOffset, format);
        var brefBbt = ReadBref(stream, rootBrefBbtOffset, format);
        var cryptMethod = ReadCryptMethod(stream, format);

        var headerInfo = new PstHeaderInfo(signature, clientSignature, version, versionMinor, stream.Length, format, cryptMethod);
        return new NdbHeader(headerInfo, brefNbt, brefBbt);
    }

    /// <summary>
    /// Membaca header PST secara asynchronous dari stream.
    /// </summary>
    /// <param name="stream">Stream file PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Metadata header NDB.</returns>
    public async Task<NdbHeader> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new InvalidDataException("Stream PST tidak tersedia.");
        }

        var headerBuffer = new byte[512];
        stream.Seek(0, SeekOrigin.Begin);
        await ReadExactlyAsync(stream, headerBuffer, cancellationToken).ConfigureAwait(false);

        var signature = BitConverter.ToUInt32(headerBuffer, 0);
        if (signature != SignatureMagic)
        {
            throw new InvalidDataException("Signature PST tidak valid.");
        }

        var clientSignature = BitConverter.ToUInt16(headerBuffer, 8);
        var version = BitConverter.ToUInt16(headerBuffer, 10);
        var versionMinor = BitConverter.ToUInt16(headerBuffer, 12);

        var format = version >= 0x0017 ? PstFormat.Unicode : PstFormat.Ansi;
        var rootOffset = GetRootOffset(format);
        var rootBrefNbtOffset = rootOffset + 36;
        var rootBrefBbtOffset = rootOffset + 52;

        var brefNbt = ReadBref(headerBuffer, rootBrefNbtOffset, format);
        var brefBbt = ReadBref(headerBuffer, rootBrefBbtOffset, format);
        var cryptMethod = await ReadCryptMethodAsync(stream, headerBuffer, format, cancellationToken).ConfigureAwait(false);

        var headerInfo = new PstHeaderInfo(signature, clientSignature, version, versionMinor, stream.Length, format, cryptMethod);
        return new NdbHeader(headerInfo, brefNbt, brefBbt);
    }

    /// <summary>
    /// Menghitung offset ROOT berdasarkan format PST.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <returns>Offset ROOT dalam header.</returns>
    private static int GetRootOffset(PstFormat format)
    {
        return format == PstFormat.Unicode ? UnicodeRootOffset : AnsiRootOffset;
    }

    /// <summary>
    /// Membaca BREF dari offset tertentu.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="offset">Offset awal BREF.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>BREF terparse.</returns>
    private static Bref ReadBref(Stream stream, long offset, PstFormat format)
    {
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        stream.Seek(offset, SeekOrigin.Begin);
        if (format == PstFormat.Unicode)
        {
            var bid = reader.ReadUInt64();
            var ib = reader.ReadUInt64();
            return new Bref(new Bid(bid), ib);
        }

        var bidAnsi = reader.ReadUInt32();
        var ibAnsi = reader.ReadUInt32();
        return new Bref(new Bid(bidAnsi), ibAnsi);
    }

    /// <summary>
    /// Membaca BREF dari buffer header berdasarkan offset.
    /// </summary>
    /// <param name="buffer">Buffer header PST.</param>
    /// <param name="offset">Offset awal BREF.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>BREF terparse.</returns>
    private static Bref ReadBref(byte[] buffer, int offset, PstFormat format)
    {
        if (format == PstFormat.Unicode)
        {
            var bid = BitConverter.ToUInt64(buffer, offset);
            var ib = BitConverter.ToUInt64(buffer, offset + 8);
            return new Bref(new Bid(bid), ib);
        }

        var bidAnsi = BitConverter.ToUInt32(buffer, offset);
        var ibAnsi = BitConverter.ToUInt32(buffer, offset + 4);
        return new Bref(new Bid(bidAnsi), ibAnsi);
    }

    /// <summary>
    /// Membaca metode enkripsi/encoding dari header.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Metode enkripsi/encoding.</returns>
    private static PstCryptMethod ReadCryptMethod(Stream stream, PstFormat format)
    {
        var rootOffset = GetRootOffset(format);
        var rootSize = format == PstFormat.Unicode ? UnicodeRootSize : AnsiRootSize;
        var align = format == PstFormat.Unicode ? 4 : 0;
        var cryptOffset = rootOffset + rootSize + align + 128 + 128 + 1;
        stream.Seek(cryptOffset, SeekOrigin.Begin);
        var value = stream.ReadByte();
        if (value < 0)
        {
            return PstCryptMethod.None;
        }

        return (PstCryptMethod)value;
    }

    /// <summary>
    /// Membaca metode enkripsi/encoding dari header secara asynchronous.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="headerBuffer">Buffer header PST.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Metode enkripsi/encoding.</returns>
    private static async Task<PstCryptMethod> ReadCryptMethodAsync(
        Stream stream,
        byte[] headerBuffer,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        var rootOffset = GetRootOffset(format);
        var rootSize = format == PstFormat.Unicode ? UnicodeRootSize : AnsiRootSize;
        var align = format == PstFormat.Unicode ? 4 : 0;
        var cryptOffset = rootOffset + rootSize + align + 128 + 128 + 1;

        if (cryptOffset < headerBuffer.Length)
        {
            return (PstCryptMethod)headerBuffer[cryptOffset];
        }

        stream.Seek(cryptOffset, SeekOrigin.Begin);
        var buffer = new byte[1];
        var read = await stream.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false);
        if (read == 0)
        {
            return PstCryptMethod.None;
        }

        return (PstCryptMethod)buffer[0];
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
                throw new EndOfStreamException("Gagal membaca header PST secara penuh.");
            }

            total += read;
        }
    }
}
