namespace Emcode.Pst.Application;

/// <summary>
/// Opsi pembukaan file PST untuk mengatur mode baca dan validasi.
/// </summary>
public sealed class PstOpenOptions
{
    /// <summary>
    /// Menentukan apakah file dibuka dalam mode hanya-baca.
    /// </summary>
    public bool ReadOnly { get; init; } = true;

    /// <summary>
    /// Menentukan apakah checksum blok divalidasi saat membaca.
    /// </summary>
    public bool ValidateChecksums { get; init; } = true;

    /// <summary>
    /// Mengizinkan pembacaan PST format ANSI.
    /// </summary>
    public bool AllowAnsi { get; init; } = true;

    /// <summary>
    /// Mengizinkan pembacaan PST format Unicode.
    /// </summary>
    public bool AllowUnicode { get; init; } = true;
}
