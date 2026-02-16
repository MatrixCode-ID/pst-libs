using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ltp;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure;

/// <summary>
/// Provider internal untuk membaca konten attachment langsung dari file PST.
/// </summary>
internal sealed class PstAttachmentContentProvider : IPstAttachmentContentProvider
{
    /// <summary>
    /// Property id untuk data biner attachment (PidTagAttachDataBinary).
    /// </summary>
    private const ushort PidTagAttachDataBinary = 0x3701;

    private readonly string _path;
    private readonly PstFormat _format;
    private readonly PstCryptMethod _cryptMethod;
    private readonly IReadOnlyDictionary<ulong, BbtEntry> _bbtEntries;

    /// <summary>
    /// Membuat provider konten attachment dengan sumber PST dan BBT yang sudah dibaca.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cryptMethod">Metode enkripsi/encoding PST.</param>
    /// <param name="bbtEntries">Dictionary BBT hasil pembacaan.</param>
    public PstAttachmentContentProvider(
        string path,
        PstFormat format,
        PstCryptMethod cryptMethod,
        IReadOnlyDictionary<ulong, BbtEntry> bbtEntries)
    {
        _path = path;
        _format = format;
        _cryptMethod = cryptMethod;
        _bbtEntries = bbtEntries;
    }

    /// <inheritdoc />
    public Stream? OpenContentStream(PstAttachmentContentReference reference)
    {
        var data = ReadContentBytes(reference);
        if (data is null)
        {
            return null;
        }

        return new MemoryStream(data, writable: false);
    }

    /// <inheritdoc />
    public async Task<Stream?> OpenContentStreamAsync(PstAttachmentContentReference reference, CancellationToken cancellationToken = default)
    {
        var data = await ReadContentBytesAsync(reference, cancellationToken).ConfigureAwait(false);
        if (data is null)
        {
            return null;
        }

        return new MemoryStream(data, writable: false);
    }

    /// <inheritdoc />
    public byte[]? ReadContentBytes(PstAttachmentContentReference reference)
    {
        if (!reference.IsValid)
        {
            return null;
        }

        using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var blockReader = new PstBlockReader(stream, _format, _cryptMethod, _bbtEntries);
        var pc = CreateAttachmentPropertyContext(blockReader, reference);
        var data = pc.GetBinary(PidTagAttachDataBinary);
        if (!data.HasValue)
        {
            return null;
        }

        return data.Value.ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]?> ReadContentBytesAsync(
        PstAttachmentContentReference reference,
        CancellationToken cancellationToken = default)
    {
        if (!reference.IsValid)
        {
            return null;
        }

        await using var stream = new FileStream(
            _path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.RandomAccess);

        var blockReader = new PstBlockReader(stream, _format, _cryptMethod, _bbtEntries);
        var pc = await CreateAttachmentPropertyContextAsync(blockReader, reference, cancellationToken).ConfigureAwait(false);
        var data = pc.GetBinary(PidTagAttachDataBinary);
        if (!data.HasValue)
        {
            return null;
        }

        return data.Value.ToArray();
    }

    /// <summary>
    /// Membuat Property Context untuk attachment berdasarkan referensi subnode.
    /// </summary>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <returns>Property Context terisi.</returns>
    private PropertyContext CreateAttachmentPropertyContext(PstBlockReader blockReader, PstAttachmentContentReference reference)
    {
        var blocks = blockReader.ReadDataBlocks(new Bid(reference.BidData));
        var heap = new HeapOnNode(blocks);
        var subnodes = new SubnodeReader(blockReader, _format, new Bid(reference.BidSub));
        return new PropertyContext(heap, subnodes);
    }

    /// <summary>
    /// Membuat Property Context untuk attachment secara asynchronous berdasarkan referensi subnode.
    /// </summary>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Property Context terisi.</returns>
    private async Task<PropertyContext> CreateAttachmentPropertyContextAsync(
        PstBlockReader blockReader,
        PstAttachmentContentReference reference,
        CancellationToken cancellationToken)
    {
        var blocks = await blockReader.ReadDataBlocksAsync(new Bid(reference.BidData), cancellationToken).ConfigureAwait(false);
        var heap = new HeapOnNode(blocks);
        var subnodes = new SubnodeReader(blockReader, _format, new Bid(reference.BidSub));
        return new PropertyContext(heap, subnodes);
    }
}
