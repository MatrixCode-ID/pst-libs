using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure;

/// <summary>
/// Provider konten attachment berbasis memory untuk kebutuhan write sementara.
/// </summary>
internal sealed class PstInMemoryAttachmentContentProvider : IPstAttachmentContentProvider
{
    private readonly ConcurrentDictionary<ulong, byte[]> _store = new();

    /// <summary>
    /// Menyimpan konten attachment dan mengembalikan referensinya.
    /// </summary>
    /// <param name="content">Konten attachment.</param>
    /// <param name="referenceId">Identifier referensi konten.</param>
    /// <returns>Referensi konten attachment.</returns>
    public PstAttachmentContentReference CreateReference(byte[] content, ulong referenceId)
    {
        _store[referenceId] = content;
        return new PstAttachmentContentReference(referenceId, 0);
    }

    /// <summary>
    /// Membuka stream konten attachment secara sinkron dari memory.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <returns>Stream konten attachment atau null jika tidak tersedia.</returns>
    public Stream? OpenContentStream(PstAttachmentContentReference reference)
    {
        if (!_store.TryGetValue(reference.BidData, out var bytes))
        {
            return null;
        }

        return new MemoryStream(bytes, writable: false);
    }

    /// <summary>
    /// Membuka stream konten attachment secara asynchronous dari memory.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Stream konten attachment atau null jika tidak tersedia.</returns>
    public Task<Stream?> OpenContentStreamAsync(PstAttachmentContentReference reference, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OpenContentStream(reference));
    }

    /// <summary>
    /// Membaca konten attachment sebagai byte array secara sinkron dari memory.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <returns>Byte array konten attachment atau null jika tidak tersedia.</returns>
    public byte[]? ReadContentBytes(PstAttachmentContentReference reference)
    {
        if (!_store.TryGetValue(reference.BidData, out var bytes))
        {
            return null;
        }

        return bytes;
    }

    /// <summary>
    /// Membaca konten attachment sebagai byte array secara asynchronous dari memory.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Byte array konten attachment atau null jika tidak tersedia.</returns>
    public Task<byte[]?> ReadContentBytesAsync(PstAttachmentContentReference reference, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ReadContentBytes(reference));
    }
}
