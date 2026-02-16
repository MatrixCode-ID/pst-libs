namespace Emcode.Pst.Domain;

/// <summary>
/// Representasi attachment untuk draft pesan sebelum ditulis ke PST.
/// </summary>
public sealed class PstDraftAttachment
{
    /// <summary>
    /// Nama file attachment.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Nama file attachment versi panjang bila tersedia.
    /// </summary>
    public string? LongFileName { get; init; }

    /// <summary>
    /// Content type attachment (MIME).
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Content-Id attachment untuk inline reference.
    /// </summary>
    public string? ContentId { get; init; }

    /// <summary>
    /// Menandakan attachment inline.
    /// </summary>
    public bool IsInline { get; init; }

    /// <summary>
    /// Konten attachment sebagai byte array.
    /// </summary>
    public byte[]? ContentBytes { get; init; }
}
