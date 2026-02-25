using Emcode.Pst.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace Emcode.Pst.Application.Abstractions;

/// <summary>
/// Kontrak untuk operasi write pada PST.
/// </summary>
public interface IPstWriter
{
    /// <summary>
    /// Menyimpan perubahan write yang masih berada di memori ke media target.
    /// </summary>
    void Save();

    /// <summary>
    /// Menyimpan perubahan write yang masih berada di memori ke media target secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Membuat folder baru pada PST.
    /// </summary>
    /// <param name="name">Nama folder baru.</param>
    /// <param name="parent">Folder parent jika membuat subfolder.</param>
    /// <returns>Folder yang dibuat.</returns>
    PstFolder CreateFolder(string name, PstFolder? parent);

    /// <summary>
    /// Membuat folder baru pada PST secara asynchronous.
    /// </summary>
    /// <param name="name">Nama folder baru.</param>
    /// <param name="parent">Folder parent jika membuat subfolder.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Folder yang dibuat.</returns>
    Task<PstFolder> CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Membuat pesan baru pada folder tertentu.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="draft">Draft data pesan.</param>
    /// <returns>Pesan yang dibuat.</returns>
    PstMessage CreateMessage(PstFolder folder, PstMessageDraft draft);

    /// <summary>
    /// Membuat pesan baru pada folder tertentu secara asynchronous.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="draft">Draft data pesan.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan yang dibuat.</returns>
    Task<PstMessage> CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mengimpor file .eml ke folder PST sebagai pesan baru.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="emlPath">Path file .eml.</param>
    /// <returns>Pesan yang dibuat.</returns>
    PstMessage ImportEml(PstFolder folder, string emlPath);

    /// <summary>
    /// Mengimpor file .eml ke folder PST sebagai pesan baru secara asynchronous.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="emlPath">Path file .eml.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan yang dibuat.</returns>
    Task<PstMessage> ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Memperbarui properti store PST (mis. nama dan komentar data file).
    /// </summary>
    /// <param name="draft">Draft properti store yang akan diperbarui.</param>
    void UpdateStoreProperties(PstStorePropertiesDraft draft);

    /// <summary>
    /// Memperbarui properti store PST secara asynchronous.
    /// </summary>
    /// <param name="draft">Draft properti store yang akan diperbarui.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    Task UpdateStorePropertiesAsync(PstStorePropertiesDraft draft, CancellationToken cancellationToken = default);

    /// <summary>
    /// Memperbarui pesan yang sudah ada.
    /// </summary>
    /// <param name="message">Pesan yang akan diperbarui.</param>
    /// <param name="draft">Draft data terbaru.</param>
    void UpdateMessage(PstMessage message, PstMessageDraft draft);

    /// <summary>
    /// Memperbarui pesan yang sudah ada secara asynchronous.
    /// </summary>
    /// <param name="message">Pesan yang akan diperbarui.</param>
    /// <param name="draft">Draft data terbaru.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    Task UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default);

    /// <summary>
    /// Menghapus pesan dari PST.
    /// </summary>
    /// <param name="message">Pesan yang akan dihapus.</param>
    void DeleteMessage(PstMessage message);

    /// <summary>
    /// Menghapus pesan dari PST secara asynchronous.
    /// </summary>
    /// <param name="message">Pesan yang akan dihapus.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    Task DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default);
}
