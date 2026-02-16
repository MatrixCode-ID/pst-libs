using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Emcode.Pst.Domain;

/// <summary>
/// Representasi attachment pesan pada PST berdasarkan Attachment Table.
/// </summary>
public sealed class PstAttachment
{
    /// <summary>
    /// Provider internal untuk membaca konten attachment.
    /// </summary>
    internal IPstAttachmentContentProvider? ContentProvider { get; set; }

    /// <summary>
    /// Referensi internal konten attachment.
    /// </summary>
    internal PstAttachmentContentReference? ContentReference { get; set; }

    /// <summary>
    /// Membuat instance attachment dengan data minimal.
    /// </summary>
    public PstAttachment()
    {
    }

    /// <summary>
    /// Nomor attachment sesuai PidTagAttachNumber.
    /// </summary>
    public int? AttachNumber { get; internal set; }

    /// <summary>
    /// Nama file attachment singkat.
    /// </summary>
    public string? FileName { get; internal set; }

    /// <summary>
    /// Nama file attachment lengkap (long filename).
    /// </summary>
    public string? LongFileName { get; internal set; }

    /// <summary>
    /// Ukuran attachment dalam byte.
    /// </summary>
    public int? Size { get; internal set; }

    /// <summary>
    /// MIME tag attachment bila tersedia.
    /// </summary>
    public string? MimeTag { get; internal set; }

    /// <summary>
    /// Content-Id attachment untuk inline reference.
    /// </summary>
    public string? ContentId { get; internal set; }

    /// <summary>
    /// Metode attachment sesuai PidTagAttachMethod.
    /// </summary>
    public int? AttachMethod { get; internal set; }

    /// <summary>
    /// Menghubungkan attachment dengan sumber konten internal.
    /// </summary>
    /// <param name="provider">Provider konten attachment.</param>
    /// <param name="reference">Referensi konten attachment.</param>
    internal void SetContentSource(IPstAttachmentContentProvider provider, PstAttachmentContentReference reference)
    {
        ContentProvider = provider;
        ContentReference = reference;
    }

    /// <summary>
    /// Membuka stream konten attachment secara sinkron.
    /// </summary>
    /// <returns>Stream konten attachment atau null jika tidak tersedia.</returns>
    public Stream? OpenContentStream()
    {
        if (ContentProvider is null || ContentReference is null)
        {
            return null;
        }

        return ContentProvider.OpenContentStream(ContentReference.Value);
    }

    /// <summary>
    /// Membuka stream konten attachment secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Stream konten attachment atau null jika tidak tersedia.</returns>
    public Task<Stream?> OpenContentStreamAsync(CancellationToken cancellationToken = default)
    {
        if (ContentProvider is null || ContentReference is null)
        {
            return Task.FromResult<Stream?>(null);
        }

        return ContentProvider.OpenContentStreamAsync(ContentReference.Value, cancellationToken);
    }

    /// <summary>
    /// Membaca konten attachment sebagai byte array secara sinkron.
    /// </summary>
    /// <returns>Byte array konten attachment atau null jika tidak tersedia.</returns>
    public byte[]? ReadContentBytes()
    {
        if (ContentProvider is null || ContentReference is null)
        {
            return null;
        }

        return ContentProvider.ReadContentBytes(ContentReference.Value);
    }

    /// <summary>
    /// Membaca konten attachment sebagai byte array secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Byte array konten attachment atau null jika tidak tersedia.</returns>
    public Task<byte[]?> ReadContentBytesAsync(CancellationToken cancellationToken = default)
    {
        if (ContentProvider is null || ContentReference is null)
        {
            return Task.FromResult<byte[]?>(null);
        }

        return ContentProvider.ReadContentBytesAsync(ContentReference.Value, cancellationToken);
    }
}
