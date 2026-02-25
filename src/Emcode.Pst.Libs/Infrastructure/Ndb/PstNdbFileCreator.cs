using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Helper untuk membuat file PST Unicode minimal yang valid sebagai titik awal operasi write.
/// </summary>
internal static class PstNdbFileCreator
{
    /// <summary>
    /// Membuat file PST Unicode minimal pada path target.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    public static void CreateMinimal(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        EnsureTargetDirectory(path);

        const long nbtOffset = 564L;
        const long bbtOffset = 1076L;
        const long fileSize = 1588L;

        var header = new byte[564];
        BitConverter.TryWriteBytes(header.AsSpan(0, 4), 0x4E444221u);
        BitConverter.TryWriteBytes(header.AsSpan(8, 2), (ushort)0x534D);
        BitConverter.TryWriteBytes(header.AsSpan(10, 2), (ushort)0x0017);
        BitConverter.TryWriteBytes(header.AsSpan(12, 2), (ushort)0x000E);
        header[14] = 0x01;
        header[15] = 0x01;
        BitConverter.TryWriteBytes(header.AsSpan(32, 8), 8UL);
        BitConverter.TryWriteBytes(header.AsSpan(40, 4), 1u);
        BitConverter.TryWriteBytes(header.AsSpan(184, 8), (ulong)fileSize);
        BitConverter.TryWriteBytes(header.AsSpan(216, 8), 0UL);
        BitConverter.TryWriteBytes(header.AsSpan(224, 8), (ulong)nbtOffset);
        BitConverter.TryWriteBytes(header.AsSpan(232, 8), 0UL);
        BitConverter.TryWriteBytes(header.AsSpan(240, 8), (ulong)bbtOffset);
        header[512] = 0xFF;
        header[513] = 0x00;
        BitConverter.TryWriteBytes(header.AsSpan(516, 8), 8UL);

        var emptyPage = new byte[512];
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(header);
        stream.Write(emptyPage);
        stream.Write(emptyPage);
    }

    /// <summary>
    /// Membuat file PST Unicode minimal pada path target secara asynchronous.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public static async Task CreateMinimalAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureTargetDirectory(path);

        const long nbtOffset = 564L;
        const long bbtOffset = 1076L;
        const long fileSize = 1588L;

        var header = new byte[564];
        BitConverter.TryWriteBytes(header.AsSpan(0, 4), 0x4E444221u);
        BitConverter.TryWriteBytes(header.AsSpan(8, 2), (ushort)0x534D);
        BitConverter.TryWriteBytes(header.AsSpan(10, 2), (ushort)0x0017);
        BitConverter.TryWriteBytes(header.AsSpan(12, 2), (ushort)0x000E);
        header[14] = 0x01;
        header[15] = 0x01;
        BitConverter.TryWriteBytes(header.AsSpan(32, 8), 8UL);
        BitConverter.TryWriteBytes(header.AsSpan(40, 4), 1u);
        BitConverter.TryWriteBytes(header.AsSpan(184, 8), (ulong)fileSize);
        BitConverter.TryWriteBytes(header.AsSpan(216, 8), 0UL);
        BitConverter.TryWriteBytes(header.AsSpan(224, 8), (ulong)nbtOffset);
        BitConverter.TryWriteBytes(header.AsSpan(232, 8), 0UL);
        BitConverter.TryWriteBytes(header.AsSpan(240, 8), (ulong)bbtOffset);
        header[512] = 0xFF;
        header[513] = 0x00;
        BitConverter.TryWriteBytes(header.AsSpan(516, 8), 8UL);

        var emptyPage = new byte[512];
        await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(emptyPage, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(emptyPage, cancellationToken).ConfigureAwait(false);
    }

    private static void EnsureTargetDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
