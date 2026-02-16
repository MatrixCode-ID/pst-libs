using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Application.Abstractions;

/// <summary>
/// Kontrak untuk membaca struktur PST dari sumber penyimpanan.
/// </summary>
public interface IPstReader
{
    /// <summary>
    /// Membaca file PST dan mengembalikan hasil struktur dasarnya.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <returns>Hasil pembacaan PST.</returns>
    PstReadResult Read(string path, PstOpenOptions options);

    /// <summary>
    /// Membaca file PST secara asynchronous dan mengembalikan hasil struktur dasarnya.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Hasil pembacaan PST.</returns>
    Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default);
}
