using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian kustom untuk membuat dan memverifikasi file PST output.
/// </summary>
public sealed class CustomTest
{
    /// <summary>
    /// Memastikan CreateAsync dapat membuat file PST baru saat path belum ada.
    /// </summary>
    [Fact]
    public void CreateOutputPstMenggunakanCreate()
    {
        var outputDirectory = Path.GetDirectoryName(TestData.OutputPath)!;
        Directory.CreateDirectory(outputDirectory);
        var outputFile = Path.Combine(outputDirectory, $"output.pst");
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        try
        {
            using PstFile pst = PstFile.Create(
                outputFile, new PstOpenOptions { 
                    ReadOnly = false, 
                    ValidateChecksums = false 
                    });

            Assert.True(true, "No Error saat membuat file PST baru dengan CreateAsync.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Pembuatan file PST gagal: {ex.Message}");
        }
    }


}
