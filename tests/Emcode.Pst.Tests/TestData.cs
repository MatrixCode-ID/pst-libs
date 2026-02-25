using System.IO;

namespace Emcode.Pst.Tests;

/// <summary>
/// Helper untuk mengakses path data uji.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Path absolut untuk sample1.pst.
    /// </summary>
    public static string Sample1Path
    {
        get
        {
            var baseDir = AppContext.BaseDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "doc", "Samples", "sample1.pst"));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Sample PST tidak ditemukan.", path);
            }

            return path;
        }
    }

    /// <summary>
    /// Path absolut untuk artifacts/Output.pst sebagai output uji kustom.
    /// </summary>
    public static string OutputPath
    {
        get
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "artifacts", "Output.pst"));
        }
    }
}
