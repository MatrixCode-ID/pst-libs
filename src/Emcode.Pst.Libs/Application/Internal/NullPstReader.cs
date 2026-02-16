using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application.Abstractions;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Application.Internal;

/// <summary>
/// Reader default yang mengembalikan hasil kosong untuk stub/testing.
/// </summary>
internal sealed class NullPstReader : IPstReader
{
    /// <summary>
    /// Mengembalikan hasil pembacaan kosong tanpa membaca file fisik.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <returns>Hasil pembacaan kosong.</returns>
    public PstReadResult Read(string path, PstOpenOptions options)
    {
        _ = path;
        _ = options;
        return new PstReadResult(header: null, rootFolder: null, folders: Array.Empty<PstFolder>());
    }

    /// <summary>
    /// Mengembalikan hasil pembacaan kosong secara asynchronous.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Hasil pembacaan kosong.</returns>
    public Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return Task.FromResult(Read(path, options));
    }
}
