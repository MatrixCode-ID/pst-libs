using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application;

namespace Emcode.Pst.Application.Abstractions;

/// <summary>
/// Kontrak untuk komponen yang dapat menyiapkan file PST baru saat belum tersedia.
/// </summary>
public interface IPstFileBootstrapper
{
    /// <summary>
    /// Menyiapkan file PST pada path target bila file belum ada.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    void EnsureFileInitialized(string path, PstOpenOptions options);

    /// <summary>
    /// Menyiapkan file PST pada path target bila file belum ada secara asynchronous.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Task representasi proses inisialisasi.</returns>
    Task EnsureFileInitializedAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default);
}
