namespace Emcode.Pst.Domain;

/// <summary>
/// Representasi folder di dalam PST.
/// </summary>
public sealed class PstFolder
{
    /// <summary>
    /// Membuat instance folder dengan identifier dan nama.
    /// </summary>
    /// <param name="id">Identifier internal folder.</param>
    /// <param name="name">Nama folder.</param>
    internal PstFolder(string id, string name)
    {
        Id = id;
        Name = name;
        SubFolders = Array.Empty<PstFolder>();
        Messages = Array.Empty<PstMessage>();
    }

    /// <summary>
    /// Identifier internal folder.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Nama folder.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Komentar folder jika tersedia.
    /// </summary>
    public string? Comment { get; internal set; }

    /// <summary>
    /// Deskripsi folder/store jika tersedia.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Subfolder di bawah folder ini.
    /// </summary>
    public IReadOnlyList<PstFolder> SubFolders { get; internal set; }

    /// <summary>
    /// Daftar pesan yang berada di folder ini.
    /// </summary>
    public IReadOnlyList<PstMessage> Messages { get; internal set; }

    /// <summary>
    /// Mengambil daftar pesan yang ada di folder ini.
    /// </summary>
    /// <returns>Enumerasi pesan di folder.</returns>
    public IEnumerable<PstMessage> EnumerateMessages()
    {
        return Messages;
    }
}
