namespace Emcode.Pst.Domain;

/// <summary>
/// Menentukan format PST yang terdeteksi dari header file.
/// </summary>
public enum PstFormat
{
    /// <summary>
    /// Format tidak diketahui atau belum terdeteksi.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// PST format ANSI (versi lama).
    /// </summary>
    Ansi = 1,

    /// <summary>
    /// PST format Unicode (versi baru).
    /// </summary>
    Unicode = 2
}
