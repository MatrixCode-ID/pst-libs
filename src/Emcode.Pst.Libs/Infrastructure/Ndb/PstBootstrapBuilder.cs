using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Builder untuk membuat baseline file PST baru langsung dari spesifikasi NDB tanpa template resource.
/// </summary>
internal sealed class PstBootstrapBuilder
{
    private const ulong FirstAmapOffset = 0x4400;
    private const ulong UnicodeMinimumFileSize = FirstAmapOffset + 512;

    /// <summary>
    /// Membuat baseline PST baru pada stream target.
    /// </summary>
    /// <param name="stream">Stream file PST target.</param>
    /// <param name="format">Format PST yang dibangun.</param>
    /// <param name="cryptMethod">Metode crypt default.</param>
    public void Build(
        Stream stream,
        PstFormat format = PstFormat.Unicode,
        PstCryptMethod cryptMethod = PstCryptMethod.None)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead || !stream.CanWrite || !stream.CanSeek)
        {
            throw new NotSupportedException("Stream bootstrap PST harus mendukung read/write/seek.");
        }

        var headerInfo = NdbHeaderWriter.InitializeEmptyHeader(stream, format, cryptMethod);
        EnsureMinimumLayoutSize(stream, format);

        var headerWriter = new NdbHeaderWriter(stream);
        headerWriter.UpdateRootAllocationMetadata(
            format,
            (ulong)stream.Length,
            FirstAmapOffset,
            cbAMapFree: 0,
            cbPMapFree: 0);
        headerWriter.SetAMapValid(format, isValid: true);
        headerWriter.UpdateHeaderCrcs(format);

        var headerReader = new NdbHeaderReader();
        var header = headerReader.Read(stream);

        var blockCounter = ResolveBidCounter(header.Counters.NextBlockBidRaw);
        var pageCounter = ResolveBidCounter(header.Counters.NextPageBidRaw);
        var writer = new NdbWriter(stream, headerInfo, blockCounter, pageCounter);
        writer.CommitBtrees(
            header,
            new Dictionary<ulong, BbtEntry>(),
            new Dictionary<uint, NbtEntry>());
    }

    /// <summary>
    /// Membuat baseline PST baru pada stream target secara asynchronous.
    /// </summary>
    /// <param name="stream">Stream file PST target.</param>
    /// <param name="format">Format PST yang dibangun.</param>
    /// <param name="cryptMethod">Metode crypt default.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task BuildAsync(
        Stream stream,
        PstFormat format = PstFormat.Unicode,
        PstCryptMethod cryptMethod = PstCryptMethod.None,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Build(stream, format, cryptMethod);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Menjaga ukuran minimal file awal agar alokasi pertama tidak menabrak halaman AMap pertama.
    /// </summary>
    /// <param name="stream">Stream target.</param>
    /// <param name="format">Format PST.</param>
    private static void EnsureMinimumLayoutSize(Stream stream, PstFormat format)
    {
        if (format != PstFormat.Unicode)
        {
            return;
        }

        if ((ulong)stream.Length >= UnicodeMinimumFileSize)
        {
            return;
        }

        stream.SetLength((long)UnicodeMinimumFileSize);
    }

    /// <summary>
    /// Mengonversi nilai next BID raw di header ke counter internal writer.
    /// </summary>
    /// <param name="nextBidRaw">Nilai next BID raw pada header.</param>
    /// <returns>Counter BID internal.</returns>
    private static ulong ResolveBidCounter(ulong nextBidRaw)
    {
        return nextBidRaw < 4 ? 0 : (nextBidRaw >> 2) - 1;
    }
}
