namespace Emcode.Pst.Domain;

/// <summary>
/// Representasi penerima pesan pada PST berdasarkan Recipient Table.
/// </summary>
public sealed class PstRecipient
{
    /// <summary>
    /// Membuat instance penerima dengan data minimal.
    /// </summary>
    public PstRecipient()
    {
    }

    /// <summary>
    /// Jenis penerima (To, Cc, Bcc) sesuai PidTagRecipientType.
    /// </summary>
    public int? RecipientType { get; internal set; }

    /// <summary>
    /// Alamat email penerima sesuai PidTagEmailAddress.
    /// </summary>
    public string? EmailAddress { get; internal set; }

    /// <summary>
    /// Nama tampilan penerima bila tersedia.
    /// </summary>
    public string? DisplayName { get; internal set; }

    /// <summary>
    /// Tipe alamat penerima (contoh: SMTP).
    /// </summary>
    public string? AddressType { get; internal set; }

    /// <summary>
    /// Alamat SMTP penerima bila tersedia.
    /// </summary>
    public string? SmtpAddress { get; internal set; }
}
