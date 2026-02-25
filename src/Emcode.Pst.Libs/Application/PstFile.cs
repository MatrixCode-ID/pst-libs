using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application.Abstractions;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure;
using Emcode.Pst.Shared;

namespace Emcode.Pst.Application;

/// <summary>
/// Facade utama untuk membuka, membaca, dan menulis file PST.
/// </summary>
public sealed class PstFile : IDisposable
{
    /// <summary>
    /// Menyimpan writer untuk operasi write bila tersedia.
    /// </summary>
    private readonly IPstWriter? _writer;

    /// <summary>
    /// Membuat instance PST dengan path dan opsi yang sudah divalidasi.
    /// </summary>
    private PstFile(string path, PstOpenOptions options, IPstWriter? writer)
    {
        Path = path;
        Options = options;
        _writer = writer;
        Folders = Array.Empty<PstFolder>();
    }

    /// <summary>
    /// Lokasi file PST yang dibuka.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Opsi pembukaan file PST yang digunakan.
    /// </summary>
    public PstOpenOptions Options { get; }

    /// <summary>
    /// Daftar folder hasil pembacaan PST.
    /// </summary>
    public IReadOnlyList<PstFolder> Folders { get; internal set; }

    /// <summary>
    /// Folder root PST jika tersedia.
    /// </summary>
    public PstFolder? RootFolder { get; internal set; }

    /// <summary>
    /// Metadata header PST hasil pembacaan awal.
    /// </summary>
    public PstHeaderInfo? Header { get; internal set; }

    /// <summary>
    /// Membuka file PST dengan reader dan writer opsional.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="reader">Reader untuk memuat struktur PST.</param>
    /// <param name="writer">Writer untuk operasi write.</param>
    /// <returns>Instance <see cref="PstFile"/> yang siap digunakan.</returns>
    public static PstFile Open(string path, PstOpenOptions? options = null, IPstReader? reader = null, IPstWriter? writer = null)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        options ??= new PstOpenOptions();
        reader ??= new PstNdbReader();
        EnsureFileAvailability(path, options, writer);

        var pst = new PstFile(path, options, writer);
        var result = reader.Read(path, options);
        pst.Header = result.Header;
        pst.RootFolder = result.RootFolder;
        var folders = result.Folders.ToList();
        pst.Folders = folders;
        if (writer is IPstWriterWithContext initializer)
        {
            initializer.Initialize(new PstWriteContext(path, options, pst.RootFolder, folders));
        }
        return pst;
    }

    /// <summary>
    /// Membuka file PST secara asynchronous dengan reader dan writer opsional.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="reader">Reader untuk memuat struktur PST.</param>
    /// <param name="writer">Writer untuk operasi write.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Instance <see cref="PstFile"/> yang siap digunakan.</returns>
    public static async Task<PstFile> OpenAsync(
        string path,
        PstOpenOptions? options = null,
        IPstReader? reader = null,
        IPstWriter? writer = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        options ??= new PstOpenOptions();
        reader ??= new PstNdbReader();
        await EnsureFileAvailabilityAsync(path, options, writer, cancellationToken).ConfigureAwait(false);

        var pst = new PstFile(path, options, writer);
        var result = await reader.ReadAsync(path, options, cancellationToken).ConfigureAwait(false);
        pst.Header = result.Header;
        pst.RootFolder = result.RootFolder;
        var folders = result.Folders.ToList();
        pst.Folders = folders;
        if (writer is IPstWriterWithContext initializer)
        {
            await initializer.InitializeAsync(new PstWriteContext(path, options, pst.RootFolder, folders), cancellationToken)
                .ConfigureAwait(false);
        }
        return pst;
    }

    /// <summary>
    /// Menyimpan perubahan write ke media target secara eksplisit.
    /// </summary>
    public void Save()
    {
        if (_writer is null)
        {
            throw new NotSupportedException("Save is not available without a writer.");
        }

        _writer.Save();
    }

    /// <summary>
    /// Menyimpan perubahan write ke media target secara eksplisit dan asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (_writer is null)
        {
            throw new NotSupportedException("SaveAsync is not available without a writer.");
        }

        return _writer.SaveAsync(cancellationToken);
    }

    /// <summary>
    /// Membuat folder baru di PST dengan parent opsional.
    /// </summary>
    /// <param name="name">Nama folder baru.</param>
    /// <param name="parent">Folder parent jika membuat subfolder.</param>
    /// <returns>Folder yang baru dibuat.</returns>
    public PstFolder CreateFolder(string name, PstFolder? parent = null)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        if (_writer is null)
        {
            throw new NotSupportedException("CreateFolder is not available without a writer.");
        }

        return _writer.CreateFolder(name, parent);
    }

    /// <summary>
    /// Membuat folder baru di PST secara asynchronous dengan parent opsional.
    /// </summary>
    /// <param name="name">Nama folder baru.</param>
    /// <param name="parent">Folder parent jika membuat subfolder.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Folder yang baru dibuat.</returns>
    public Task<PstFolder> CreateFolderAsync(string name, PstFolder? parent = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        if (_writer is null)
        {
            throw new NotSupportedException("CreateFolderAsync is not available without a writer.");
        }

        return _writer.CreateFolderAsync(name, parent, cancellationToken);
    }

    /// <summary>
    /// Membuat pesan baru di folder tertentu.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="draft">Draft data pesan.</param>
    /// <returns>Pesan yang baru dibuat.</returns>
    public PstMessage CreateMessage(PstFolder folder, PstMessageDraft draft)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNull(draft, nameof(draft));
        if (_writer is null)
        {
            throw new NotSupportedException("CreateMessage is not available without a writer.");
        }

        return _writer.CreateMessage(folder, draft);
    }

    /// <summary>
    /// Membuat pesan baru di folder tertentu secara asynchronous.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="draft">Draft data pesan.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan yang baru dibuat.</returns>
    public Task<PstMessage> CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNull(draft, nameof(draft));
        if (_writer is null)
        {
            throw new NotSupportedException("CreateMessageAsync is not available without a writer.");
        }

        return _writer.CreateMessageAsync(folder, draft, cancellationToken);
    }

    /// <summary>
    /// Mengimpor file .eml ke folder tertentu sebagai pesan baru.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="emlPath">Path file .eml.</param>
    /// <returns>Pesan yang baru dibuat.</returns>
    public PstMessage ImportEml(PstFolder folder, string emlPath)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        if (_writer is null)
        {
            throw new NotSupportedException("ImportEml is not available without a writer.");
        }

        return _writer.ImportEml(folder, emlPath);
    }

    /// <summary>
    /// Mengimpor file .eml ke folder tertentu sebagai pesan baru secara asynchronous.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="emlPath">Path file .eml.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan yang baru dibuat.</returns>
    public Task<PstMessage> ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        if (_writer is null)
        {
            throw new NotSupportedException("ImportEmlAsync is not available without a writer.");
        }

        return _writer.ImportEmlAsync(folder, emlPath, cancellationToken);
    }

    /// <summary>
    /// Memperbarui properti store PST (mis. nama dan komentar data file).
    /// </summary>
    /// <param name="draft">Draft properti store.</param>
    public void UpdateStoreProperties(PstStorePropertiesDraft draft)
    {
        Guard.NotNull(draft, nameof(draft));
        if (_writer is null)
        {
            throw new NotSupportedException("UpdateStoreProperties is not available without a writer.");
        }

        _writer.UpdateStoreProperties(draft);
    }

    /// <summary>
    /// Memperbarui properti store PST secara asynchronous.
    /// </summary>
    /// <param name="draft">Draft properti store.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task UpdateStorePropertiesAsync(PstStorePropertiesDraft draft, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(draft, nameof(draft));
        if (_writer is null)
        {
            throw new NotSupportedException("UpdateStorePropertiesAsync is not available without a writer.");
        }

        return _writer.UpdateStorePropertiesAsync(draft, cancellationToken);
    }

    /// <summary>
    /// Memperbarui pesan yang sudah ada dengan draft terbaru.
    /// </summary>
    /// <param name="message">Pesan yang akan diperbarui.</param>
    /// <param name="draft">Draft data terbaru.</param>
    public void UpdateMessage(PstMessage message, PstMessageDraft draft)
    {
        Guard.NotNull(message, nameof(message));
        Guard.NotNull(draft, nameof(draft));
        if (_writer is null)
        {
            throw new NotSupportedException("UpdateMessage is not available without a writer.");
        }

        _writer.UpdateMessage(message, draft);
    }

    /// <summary>
    /// Memperbarui pesan yang sudah ada secara asynchronous.
    /// </summary>
    /// <param name="message">Pesan yang akan diperbarui.</param>
    /// <param name="draft">Draft data terbaru.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(message, nameof(message));
        Guard.NotNull(draft, nameof(draft));
        if (_writer is null)
        {
            throw new NotSupportedException("UpdateMessageAsync is not available without a writer.");
        }

        return _writer.UpdateMessageAsync(message, draft, cancellationToken);
    }

    /// <summary>
    /// Menghapus pesan dari PST.
    /// </summary>
    /// <param name="message">Pesan yang akan dihapus.</param>
    public void DeleteMessage(PstMessage message)
    {
        Guard.NotNull(message, nameof(message));
        if (_writer is null)
        {
            throw new NotSupportedException("DeleteMessage is not available without a writer.");
        }

        _writer.DeleteMessage(message);
    }

    /// <summary>
    /// Menghapus pesan dari PST secara asynchronous.
    /// </summary>
    /// <param name="message">Pesan yang akan dihapus.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(message, nameof(message));
        if (_writer is null)
        {
            throw new NotSupportedException("DeleteMessageAsync is not available without a writer.");
        }

        return _writer.DeleteMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Memastikan file PST tersedia sebelum proses baca dimulai.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="writer">Writer opsional untuk bootstrap file.</param>
    private static void EnsureFileAvailability(string path, PstOpenOptions options, IPstWriter? writer)
    {
        if (File.Exists(path))
        {
            return;
        }

        if (!options.CreateIfMissing)
        {
            throw new FileNotFoundException("File PST tidak ditemukan.", path);
        }

        if (options.ReadOnly)
        {
            throw new NotSupportedException("CreateIfMissing membutuhkan opsi ReadOnly = false.");
        }

        if (writer is not IPstFileBootstrapper bootstrapper)
        {
            throw new NotSupportedException("CreateIfMissing membutuhkan writer yang mendukung bootstrap file PST.");
        }

        bootstrapper.EnsureFileInitialized(path, options);
    }

    /// <summary>
    /// Memastikan file PST tersedia sebelum proses baca dimulai secara asynchronous.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="writer">Writer opsional untuk bootstrap file.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private static async Task EnsureFileAvailabilityAsync(string path, PstOpenOptions options, IPstWriter? writer, CancellationToken cancellationToken)
    {
        if (File.Exists(path))
        {
            return;
        }

        if (!options.CreateIfMissing)
        {
            throw new FileNotFoundException("File PST tidak ditemukan.", path);
        }

        if (options.ReadOnly)
        {
            throw new NotSupportedException("CreateIfMissing membutuhkan opsi ReadOnly = false.");
        }

        if (writer is not IPstFileBootstrapper bootstrapper)
        {
            throw new NotSupportedException("CreateIfMissing membutuhkan writer yang mendukung bootstrap file PST.");
        }

        await bootstrapper.EnsureFileInitializedAsync(path, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Melepas resource yang digunakan oleh PST.
    /// </summary>
    public void Dispose()
    {
        if (_writer is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
