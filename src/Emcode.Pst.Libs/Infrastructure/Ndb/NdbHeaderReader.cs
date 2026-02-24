using System.IO;
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
    private const int HeaderReadSize = 0x220;
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

        var headerBuffer = new byte[HeaderReadSize];
        stream.Seek(0, SeekOrigin.Begin);
        ReadExactly(stream, headerBuffer);

        return ParseHeader(headerBuffer, stream.Length);
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

        var headerBuffer = new byte[HeaderReadSize];
        stream.Seek(0, SeekOrigin.Begin);
        await ReadExactlyAsync(stream, headerBuffer, cancellationToken).ConfigureAwait(false);
        return ParseHeader(headerBuffer, stream.Length);
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
    /// Melakukan parse header buffer menjadi model NdbHeader lengkap.
    /// </summary>
    /// <param name="headerBuffer">Buffer header PST (minimal 0x220 byte).</param>
    /// <param name="streamLength">Ukuran stream saat ini.</param>
    /// <returns>Model header terparse.</returns>
    private static NdbHeader ParseHeader(byte[] headerBuffer, long streamLength)
    {
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
        var rootBrefNbtOffset = GetRootBrefNbtOffset(format, rootOffset);
        var rootBrefBbtOffset = GetRootBrefBbtOffset(format, rootOffset);

        var brefNbt = ReadBref(headerBuffer, rootBrefNbtOffset, format);
        var brefBbt = ReadBref(headerBuffer, rootBrefBbtOffset, format);
        var cryptMethod = ReadCryptMethod(headerBuffer, format);

        var headerInfo = new PstHeaderInfo(signature, clientSignature, version, versionMinor, streamLength, format, cryptMethod);
        var counters = ReadCounters(headerBuffer, format);
        var rootState = ReadRootState(headerBuffer, format, rootOffset);
        return new NdbHeader(headerInfo, brefNbt, brefBbt, counters, rootState);
    }

    /// <summary>
    /// Membaca metode enkripsi/encoding dari buffer header.
    /// </summary>
    /// <param name="headerBuffer">Buffer header PST.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Metode enkripsi/encoding.</returns>
    private static PstCryptMethod ReadCryptMethod(byte[] headerBuffer, PstFormat format)
    {
        var rootOffset = GetRootOffset(format);
        var rootSize = format == PstFormat.Unicode ? UnicodeRootSize : AnsiRootSize;
        var align = format == PstFormat.Unicode ? 4 : 0;
        var cryptOffset = rootOffset + rootSize + align + 128 + 128 + 1;
        if (cryptOffset >= headerBuffer.Length)
        {
            return PstCryptMethod.None;
        }

        return (PstCryptMethod)headerBuffer[cryptOffset];
    }

    /// <summary>
    /// Membaca counter BID/NID dari header.
    /// </summary>
    /// <param name="buffer">Buffer header PST.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Snapshot counter header.</returns>
    private static NdbHeaderCounters ReadCounters(byte[] buffer, PstFormat format)
    {
        var nextPageBidRaw = format == PstFormat.Unicode
            ? BitConverter.ToUInt64(buffer, 0x20)
            : BitConverter.ToUInt32(buffer, 0x20);
        var nextBlockBidRaw = format == PstFormat.Unicode
            ? BitConverter.ToUInt64(buffer, 0x204)
            : BitConverter.ToUInt32(buffer, 0x1C);

        var rgnidOffset = format == PstFormat.Unicode ? 0x2C : 0x28;
        var nidCounters = new uint[32];
        for (var i = 0; i < nidCounters.Length; i++)
        {
            var offset = rgnidOffset + (i * sizeof(uint));
            if (offset + sizeof(uint) > buffer.Length)
            {
                break;
            }

            nidCounters[i] = BitConverter.ToUInt32(buffer, offset);
        }

        return new NdbHeaderCounters(nextBlockBidRaw, nextPageBidRaw, nidCounters);
    }

    /// <summary>
    /// Membaca state ROOT dari buffer header.
    /// </summary>
    /// <param name="buffer">Buffer header PST.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="rootOffset">Offset ROOT dalam header.</param>
    /// <returns>Snapshot state ROOT.</returns>
    private static NdbRootState ReadRootState(byte[] buffer, PstFormat format, int rootOffset)
    {
        var dataSize = format == PstFormat.Unicode ? sizeof(ulong) : sizeof(uint);
        var ibFileEofOffset = rootOffset + sizeof(uint);
        var ibAMapLastOffset = ibFileEofOffset + dataSize;
        var cbAMapFreeOffset = ibAMapLastOffset + dataSize;
        var cbPMapFreeOffset = cbAMapFreeOffset + dataSize;
        var fAMapValidOffset = format == PstFormat.Unicode ? rootOffset + 68 : rootOffset + 36;

        var ibFileEof = ReadInteger(buffer, ibFileEofOffset, dataSize);
        var ibAMapLast = ReadInteger(buffer, ibAMapLastOffset, dataSize);
        var cbAMapFree = ReadInteger(buffer, cbAMapFreeOffset, dataSize);
        var cbPMapFree = ReadInteger(buffer, cbPMapFreeOffset, dataSize);

        var fAMapValid = fAMapValidOffset < buffer.Length ? buffer[fAMapValidOffset] : (byte)0;
        var isAmapValid = fAMapValid is 0x01 or 0x02;

        return new NdbRootState(ibFileEof, ibAMapLast, cbAMapFree, cbPMapFree, isAmapValid);
    }

    /// <summary>
    /// Mengambil offset BREF NBT pada struktur ROOT.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="rootOffset">Offset ROOT.</param>
    /// <returns>Offset absolut BREF NBT.</returns>
    private static int GetRootBrefNbtOffset(PstFormat format, int rootOffset)
    {
        return format == PstFormat.Unicode ? rootOffset + 36 : rootOffset + 20;
    }

    /// <summary>
    /// Mengambil offset BREF BBT pada struktur ROOT.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="rootOffset">Offset ROOT.</param>
    /// <returns>Offset absolut BREF BBT.</returns>
    private static int GetRootBrefBbtOffset(PstFormat format, int rootOffset)
    {
        return format == PstFormat.Unicode ? rootOffset + 52 : rootOffset + 28;
    }

    /// <summary>
    /// Membaca integer little-endian dari buffer dengan ukuran 4/8 byte.
    /// </summary>
    /// <param name="buffer">Buffer sumber.</param>
    /// <param name="offset">Offset baca.</param>
    /// <param name="size">Ukuran integer (4 atau 8).</param>
    /// <returns>Nilai integer dalam bentuk ulong.</returns>
    private static ulong ReadInteger(byte[] buffer, int offset, int size)
    {
        if (offset + size > buffer.Length)
        {
            return 0;
        }

        return size == sizeof(ulong)
            ? BitConverter.ToUInt64(buffer, offset)
            : BitConverter.ToUInt32(buffer, offset);
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

    /// <summary>
    /// Membaca buffer hingga penuh dari stream secara sinkron.
    /// </summary>
    /// <param name="stream">Stream sumber.</param>
    /// <param name="buffer">Buffer tujuan.</param>
    private static void ReadExactly(Stream stream, Span<byte> buffer)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var read = stream.Read(buffer.Slice(total));
            if (read == 0)
            {
                throw new EndOfStreamException("Gagal membaca header PST secara penuh.");
            }

            total += read;
        }
    }
}
