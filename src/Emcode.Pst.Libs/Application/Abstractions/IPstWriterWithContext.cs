using System.Threading;
using System.Threading.Tasks;

namespace Emcode.Pst.Application.Abstractions;

/// <summary>
/// Kontrak untuk writer yang membutuhkan konteks PST sebelum operasi write.
/// </summary>
public interface IPstWriterWithContext
{
    /// <summary>
    /// Menginisialisasi writer dengan konteks PST yang sedang dibuka.
    /// </summary>
    /// <param name="context">Konteks PST untuk operasi write.</param>
    void Initialize(PstWriteContext context);

    /// <summary>
    /// Menginisialisasi writer dengan konteks PST secara asynchronous.
    /// </summary>
    /// <param name="context">Konteks PST untuk operasi write.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    Task InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default);
}
