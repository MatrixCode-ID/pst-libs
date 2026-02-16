namespace Emcode.Pst.Domain;

/// <summary>
/// Metadata header PST hasil pembacaan awal file.
/// </summary>
public sealed class PstHeaderInfo
{
    /// <summary>
    /// Membuat metadata header PST.
    /// </summary>
    /// <param name="signature">Signature file PST.</param>
    /// <param name="clientSignature">Signature client.</param>
    /// <param name="version">Versi file PST.</param>
    /// <param name="versionMinor">Versi minor file PST.</param>
    /// <param name="fileSize">Ukuran file dalam byte.</param>
    /// <param name="format">Format PST terdeteksi.</param>
    /// <param name="cryptMethod">Metode enkripsi/encoding data blok.</param>
    public PstHeaderInfo(uint signature, uint clientSignature, ushort version, ushort versionMinor, long fileSize, PstFormat format, PstCryptMethod cryptMethod)
    {
        Signature = signature;
        ClientSignature = clientSignature;
        Version = version;
        VersionMinor = versionMinor;
        FileSize = fileSize;
        Format = format;
        CryptMethod = cryptMethod;
    }

    /// <summary>
    /// Signature file PST (magic number).
    /// </summary>
    public uint Signature { get; }

    /// <summary>
    /// Signature client PST.
    /// </summary>
    public uint ClientSignature { get; }

    /// <summary>
    /// Versi utama file PST.
    /// </summary>
    public ushort Version { get; }

    /// <summary>
    /// Versi minor file PST.
    /// </summary>
    public ushort VersionMinor { get; }

    /// <summary>
    /// Ukuran file PST dalam byte.
    /// </summary>
    public long FileSize { get; }

    /// <summary>
    /// Format PST hasil deteksi.
    /// </summary>
    public PstFormat Format { get; }

    /// <summary>
    /// Metode enkripsi/encoding data blok pada PST.
    /// </summary>
    public PstCryptMethod CryptMethod { get; }
}
