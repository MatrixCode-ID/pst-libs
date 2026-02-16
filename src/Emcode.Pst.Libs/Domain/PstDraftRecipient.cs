namespace Emcode.Pst.Domain;

/// <summary>
/// Representasi penerima untuk draft pesan sebelum ditulis ke PST.
/// </summary>
public sealed class PstDraftRecipient
{
    /// <summary>
    /// Jenis penerima (To/Cc/Bcc).
    /// </summary>
    public PstRecipientType RecipientType { get; init; }

    /// <summary>
    /// Nama tampilan penerima bila tersedia.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Alamat email penerima.
    /// </summary>
    public string? EmailAddress { get; init; }

    /// <summary>
    /// Alamat SMTP penerima bila tersedia.
    /// </summary>
    public string? SmtpAddress { get; init; }
}
