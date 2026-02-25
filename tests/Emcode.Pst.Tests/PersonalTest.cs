using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

public class PersonalTest
{
    [Fact]
    public void TestWrite_CreateOutput()
    {
        var outputPath = ResolveArtifactsOutputPath();
        var baselinePath = TestData.EmptyBaselinePath;
        var outputDirectory = Path.GetDirectoryName(outputPath);
        Assert.False(string.IsNullOrWhiteSpace(outputDirectory), "Direktori artifacts tidak valid.");
        Directory.CreateDirectory(outputDirectory!);
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        using var baseline = PstFile.Open(baselinePath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true });
        using var writable = PstFile.Open(
                   outputPath,
                   new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
                   writer: new PstNdbWriter());

        foreach (var folder in baseline.Folders)
        {
            var newWriteableFolder = writable.CreateFolder(folder.Name);
            newWriteableFolder.Comment = folder.Comment;
            newWriteableFolder.Description = folder.Description;
            newWriteableFolder.Name = folder.Name;            

            foreach (var message in folder.Messages)
            {
                List<PstDraftRecipient> recipients =
                [
                    .. message.Recipients.Select(recipient => new PstDraftRecipient
                    {
                        DisplayName = recipient.DisplayName,
                        EmailAddress = recipient.EmailAddress,
                        SmtpAddress = recipient.SmtpAddress,
                        RecipientType = (PstRecipientType)(recipient.RecipientType ?? 0)
                    }),
                ];


                var newMessageDraft = new PstMessageDraft
                {
                    Subject = message.Subject,
                    Body = message.Body,
                    FromAddress = message.SenderEmailAddress,
                    FromName = message.SenderName,
                    Recipients = recipients
                };
                writable.CreateMessage(newWriteableFolder, newMessageDraft);
            }
        }
        writable.Save();

        // var baselineStore = ResolveBenchmarkStoreFolder(baseline);
        // writable.UpdateStoreProperties(new PstStorePropertiesDraft
        // {
        //     DisplayName = baselineStore.Name,
        //     Description = baselineStore.Description,
        //     Comment = baselineStore.Comment
        // });

        // var parent = writable.Folders.FirstOrDefault(folder =>
        //                  string.Equals(folder.Name, baselineStore.Name, StringComparison.OrdinalIgnoreCase))
        //              ?? writable.Folders.FirstOrDefault(folder =>
        //                  string.Equals(folder.Name, "Top of Outlook data file", StringComparison.OrdinalIgnoreCase))
        //              ?? writable.RootFolder
        //              ?? writable.Folders.First();
        // CopyFolderTreeAndMessagesFromBaseline(baselineStore, parent, writable);

        // using var baselineReopened = PstFile.Open(baselinePath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true });
        // using (var reopened = PstFile.Open(outputPath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true }))
        // {
        //     Assert.True(reopened.Folders.Count > 0);
        //     AssertBenchmarkContentMatchesBaseline(baselineReopened, reopened);
        // }

        // var generatedHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(outputPath)));
        // var baselineHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(baselinePath)));
        // Assert.False(string.IsNullOrWhiteSpace(generatedHash));
        // Assert.False(string.IsNullOrWhiteSpace(baselineHash));
        // Assert.True(File.Exists(outputPath));
        // Assert.True(new FileInfo(outputPath).Length > 0);
        // Assert.Equal(baselineHash, generatedHash);
    }

    private static string ResolveArtifactsOutputPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var repositoryRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        return Path.Combine(repositoryRoot, "artifacts", "output.pst");
    }

}