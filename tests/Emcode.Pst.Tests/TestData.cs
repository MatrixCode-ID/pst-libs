using System.IO;

namespace Emcode.Pst.Tests;

/// <summary>
/// Helper untuk mengakses path data uji.
/// </summary>
public static class TestData
{
    private static string ResolveDocPath(string relativePath, string missingMessage)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "doc", relativePath));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(missingMessage, path);
        }

        return path;
    }

    /// <summary>
    /// Path absolut untuk sample1.pst.
    /// </summary>
    public static string Sample1Path
    {
        get
        {
            return ResolveDocPath(Path.Combine("Samples", "sample1.pst"), "Sample PST tidak ditemukan.");
        }
    }

    /// <summary>
    /// Path absolut untuk baseline empty.pst.
    /// </summary>
    public static string EmptyBaselinePath
    {
        get
        {
            return ResolveDocPath("empty.pst", "Baseline empty.pst tidak ditemukan.");
        }
    }

    /// <summary>
    /// Path absolut untuk attachment benchmark test-doc.docx.
    /// </summary>
    public static string TestDocDocxPath => ResolveDocPath("test-doc.docx", "Fixture test-doc.docx tidak ditemukan.");

    /// <summary>
    /// Path absolut untuk attachment benchmark test-doc.pdf.
    /// </summary>
    public static string TestDocPdfPath => ResolveDocPath("test-doc.pdf", "Fixture test-doc.pdf tidak ditemukan.");
}
