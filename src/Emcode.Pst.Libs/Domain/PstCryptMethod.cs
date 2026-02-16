namespace Emcode.Pst.Domain;

/// <summary>
/// Menentukan metode enkripsi/encoding data blok pada PST.
/// </summary>
public enum PstCryptMethod
{
    /// <summary>
    /// Data blok tidak dienkode atau dienkripsi.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Data blok dienkode menggunakan algoritma Permutation.
    /// </summary>
    Permute = 0x01,

    /// <summary>
    /// Data blok dienkode menggunakan algoritma Cyclic.
    /// </summary>
    Cyclic = 0x02,

    /// <summary>
    /// Data blok dienkripsi dengan Windows Information Protection (EDP).
    /// </summary>
    EdpEncrypted = 0x10
}
