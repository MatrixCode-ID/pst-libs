using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Emcode.Pst.Domain;

/// <summary>
/// Referensi internal untuk data attachment yang mengarah ke node data di PST.
/// </summary>
internal readonly struct PstAttachmentContentReference
{
    /// <summary>
    /// Membuat referensi konten attachment berdasarkan BID data dan BID subnode.
    /// </summary>
    /// <param name="bidData">BID data attachment.</param>
    /// <param name="bidSub">BID subnode attachment.</param>
    public PstAttachmentContentReference(ulong bidData, ulong bidSub)
    {
        BidData = bidData;
        BidSub = bidSub;
    }

    /// <summary>
    /// BID data attachment.
    /// </summary>
    public ulong BidData { get; }

    /// <summary>
    /// BID subnode attachment.
    /// </summary>
    public ulong BidSub { get; }

    /// <summary>
    /// Menandakan referensi memiliki BID data yang valid.
    /// </summary>
    public bool IsValid => BidData != 0;
}

/// <summary>
/// Kontrak internal untuk mengambil data attachment dari sumber PST.
/// </summary>
internal interface IPstAttachmentContentProvider
{
    /// <summary>
    /// Membuka stream konten attachment secara sinkron.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <returns>Stream konten attachment atau null jika tidak tersedia.</returns>
    Stream? OpenContentStream(PstAttachmentContentReference reference);

    /// <summary>
    /// Membuka stream konten attachment secara asynchronous.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Stream konten attachment atau null jika tidak tersedia.</returns>
    Task<Stream?> OpenContentStreamAsync(PstAttachmentContentReference reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Membaca konten attachment sebagai byte array secara sinkron.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <returns>Byte array konten attachment atau null jika tidak tersedia.</returns>
    byte[]? ReadContentBytes(PstAttachmentContentReference reference);

    /// <summary>
    /// Membaca konten attachment sebagai byte array secara asynchronous.
    /// </summary>
    /// <param name="reference">Referensi konten attachment.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Byte array konten attachment atau null jika tidak tersedia.</returns>
    Task<byte[]?> ReadContentBytesAsync(PstAttachmentContentReference reference, CancellationToken cancellationToken = default);
}
