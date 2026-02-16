using System;
using System.Collections.Generic;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Application.Abstractions;

/// <summary>
/// Konteks data PST yang dibutuhkan oleh writer untuk operasi write.
/// </summary>
public sealed class PstWriteContext
{
    /// <summary>
    /// Membuat konteks write dengan data PST yang sudah dibaca.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="rootFolder">Folder root jika tersedia.</param>
    /// <param name="folders">Daftar folder yang dapat dimutasi oleh writer.</param>
    public PstWriteContext(string path, PstOpenOptions options, PstFolder? rootFolder, List<PstFolder> folders)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        RootFolder = rootFolder;
        Folders = folders ?? throw new ArgumentNullException(nameof(folders));
    }

    /// <summary>
    /// Lokasi file PST yang sedang dibuka.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Opsi pembukaan PST yang digunakan.
    /// </summary>
    public PstOpenOptions Options { get; }

    /// <summary>
    /// Folder root PST bila tersedia.
    /// </summary>
    public PstFolder? RootFolder { get; }

    /// <summary>
    /// Daftar folder yang dapat diperbarui oleh writer.
    /// </summary>
    public List<PstFolder> Folders { get; }
}
