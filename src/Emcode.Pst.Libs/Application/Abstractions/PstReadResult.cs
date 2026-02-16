using Emcode.Pst.Domain;

namespace Emcode.Pst.Application.Abstractions;

/// <summary>
/// Hasil pembacaan PST yang memuat metadata header, folder root, dan daftar folder.
/// </summary>
public sealed class PstReadResult
{
    /// <summary>
    /// Membuat hasil pembacaan dengan metadata header, root folder, dan daftar folder.
    /// </summary>
    /// <param name="header">Metadata header PST jika tersedia.</param>
    /// <param name="rootFolder">Folder root jika tersedia.</param>
    /// <param name="folders">Daftar folder.</param>
    public PstReadResult(PstHeaderInfo? header, PstFolder? rootFolder, IReadOnlyList<PstFolder> folders)
    {
        Header = header;
        RootFolder = rootFolder;
        Folders = folders;
    }

    /// <summary>
    /// Metadata header PST hasil pembacaan.
    /// </summary>
    public PstHeaderInfo? Header { get; }

    /// <summary>
    /// Folder root jika ditemukan.
    /// </summary>
    public PstFolder? RootFolder { get; }

    /// <summary>
    /// Daftar folder hasil pembacaan.
    /// </summary>
    public IReadOnlyList<PstFolder> Folders { get; }
}
