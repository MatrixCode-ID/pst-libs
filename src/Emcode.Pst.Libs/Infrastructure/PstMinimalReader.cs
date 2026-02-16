using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application;
using Emcode.Pst.Application.Abstractions;
using Emcode.Pst.Domain;
using Emcode.Pst.Shared;

namespace Emcode.Pst.Infrastructure;

/// <summary>
/// Reader minimal yang hanya memvalidasi header PST dan mengembalikan metadata dasar.
/// Enumerasi folder/message masih placeholder (belum parsing NDB).
/// </summary>
public sealed class PstMinimalReader : IPstReader
{
    private const int HeaderSize = 512;
    private const uint SignatureMagic = 0x4E444221; // "!BDN" dalam little-endian.

    /// <summary>
    /// Membaca header PST dan mengembalikan metadata dasar.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <returns>Hasil pembacaan minimal.</returns>
    public PstReadResult Read(string path, PstOpenOptions options)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        Guard.NotNull(options, nameof(options));

        using var stream = File.OpenRead(path);
        if (stream.Length < HeaderSize)
        {
            throw new InvalidDataException("Ukuran file terlalu kecil untuk header PST.");
        }

        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, leaveOpen: true);
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

        var format = DetectFormat(version, options);
        var header = new PstHeaderInfo(signature, clientSignature, version, versionMinor, stream.Length, format, PstCryptMethod.None);

        var root = new PstFolder("root", "Root");
        var folders = new[] { root };
        return new PstReadResult(header, rootFolder: root, folders: folders);
    }

    /// <summary>
    /// Membaca header PST secara asynchronous dan mengembalikan metadata dasar.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Hasil pembacaan minimal.</returns>
    public async Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        Guard.NotNull(options, nameof(options));

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (stream.Length < HeaderSize)
        {
            throw new InvalidDataException("Ukuran file terlalu kecil untuk header PST.");
        }

        var headerBuffer = new byte[HeaderSize];
        await ReadExactlyAsync(stream, headerBuffer, cancellationToken).ConfigureAwait(false);

        var signature = BitConverter.ToUInt32(headerBuffer, 0);
        if (signature != SignatureMagic)
        {
            throw new InvalidDataException("Signature PST tidak valid.");
        }

        var clientSignature = BitConverter.ToUInt16(headerBuffer, 8);
        var version = BitConverter.ToUInt16(headerBuffer, 10);
        var versionMinor = BitConverter.ToUInt16(headerBuffer, 12);

        var format = DetectFormat(version, options);
        var header = new PstHeaderInfo(signature, clientSignature, version, versionMinor, stream.Length, format, PstCryptMethod.None);

        var root = new PstFolder("root", "Root");
        var folders = new[] { root };
        return new PstReadResult(header, rootFolder: root, folders: folders);
    }

    /// <summary>
    /// Menentukan format PST berdasarkan versi dan opsi pembukaan.
    /// </summary>
    /// <param name="version">Versi file PST.</param>
    /// <param name="options">Opsi pembukaan.</param>
    /// <returns>Format PST terdeteksi.</returns>
    private static PstFormat DetectFormat(ushort version, PstOpenOptions options)
    {
        if (version == 0)
        {
            return PstFormat.Unknown;
        }

        var format = version >= 0x0017 ? PstFormat.Unicode : PstFormat.Ansi;
        if (format == PstFormat.Ansi && !options.AllowAnsi)
        {
            throw new InvalidDataException("PST ANSI tidak diizinkan oleh opsi pembukaan.");
        }

        if (format == PstFormat.Unicode && !options.AllowUnicode)
        {
            throw new InvalidDataException("PST Unicode tidak diizinkan oleh opsi pembukaan.");
        }

        return format;
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
