namespace Emcode.Pst.Domain;

/// <summary>
/// Draft properti store PST yang dapat diperbarui.
/// </summary>
public sealed class PstStorePropertiesDraft
{
    /// <summary>
    /// Nama tampilan store PST (nama data file di Outlook).
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Deskripsi store PST (disimpan pada properti komentar folder store).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Komentar store PST (disimpan pada message-store/internal node).
    /// </summary>
    public string? Comment { get; init; }
}
