namespace Emcode.Pst.Domain;

/// <summary>
/// Jenis penerima pesan sesuai konvensi MAPI (To, Cc, Bcc).
/// </summary>
public enum PstRecipientType
{
    /// <summary>
    /// Penerima utama (To).
    /// </summary>
    To = 1,

    /// <summary>
    /// Penerima tembusan (Cc).
    /// </summary>
    Cc = 2,

    /// <summary>
    /// Penerima blind carbon copy (Bcc).
    /// </summary>
    Bcc = 3
}
