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
    /// Memastikan file PST baru dapat dibuat di artifacts/Output.pst dengan folder dan pesan.
    /// </summary>
    [Fact]
    public void SampleTest_ShouldCreateFolderAndMessageInOutputPst()
    {
        var outputPath = TestData.OutputPath;

        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        using (PstFile.Create(outputPath, new PstOpenOptions { ReadOnly = false, ValidateChecksums = false }))
        {
        }

        var folderName = $"SampleFolder-{Guid.NewGuid():N}";
        var subject = "Sample Message";
        var body = "Sample body dari SampleTest.";
        var attachmentBytes = Encoding.UTF8.GetBytes("sample attachment content");

        using (var pst = PstFile.Open(outputPath, new PstOpenOptions { ReadOnly = false, ValidateChecksums = false }, writer: new PstNdbWriter()))
        {
            var parent = pst.Folders.First();
            var folder = pst.CreateFolder(folderName, parent);

            var draft = new PstMessageDraft
            {
                Subject = subject,
                Body = body,
                FromName = "Sample Sender",
                FromAddress = "sender@example.com",
                MessageClass = "IPM.Note",
                Recipients = new[]
                {
                    new PstDraftRecipient
                    {
                        RecipientType = PstRecipientType.To,
                        DisplayName = "Sample Recipient",
                        EmailAddress = "recipient@example.com"
                    }
                },
                Attachments = new[]
                {
                    new PstDraftAttachment
                    {
                        FileName = "sample.txt",
                        ContentType = "text/plain",
                        ContentBytes = attachmentBytes
                    }
                }
            };

            pst.CreateMessage(folder, draft);
        }

        Assert.True(File.Exists(outputPath));

        using var reopened = PstFile.Open(outputPath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
        var createdFolder = reopened.Folders.FirstOrDefault(f => f.Name == folderName);
        Assert.NotNull(createdFolder);

        var message = createdFolder!.Messages.FirstOrDefault(m => m.Subject == subject);
        Assert.NotNull(message);
        Assert.Equal(body, message!.Body);
        Assert.Equal("IPM.Note", message.MessageClass);
        Assert.True(message.HasAttachments);

        var attachment = message.Attachments.First();
        var content = attachment.ReadContentBytes();
        Assert.NotNull(content);
        Assert.Equal("sample attachment content", Encoding.UTF8.GetString(content!));
    }

    /// <summary>
    /// Memastikan CreateAsync dapat membuat file PST baru saat path belum ada.
    /// </summary>
    [Fact]
    public async Task SampleTest_ShouldCreateOutputPstViaCreateAsync_WhenMissing()
    {
        var outputDirectory = Path.GetDirectoryName(TestData.OutputPath)!;
        Directory.CreateDirectory(outputDirectory);
        var asyncOutputPath = Path.Combine(outputDirectory, $"output-async-{Guid.NewGuid():N}.pst");

        try
        {
            using (await PstFile.CreateAsync(asyncOutputPath, new PstOpenOptions { ReadOnly = false, ValidateChecksums = false }))
            {
            }

            Assert.True(File.Exists(asyncOutputPath));
        }
        finally
        {
            if (File.Exists(asyncOutputPath))
            {
                File.Delete(asyncOutputPath);
            }
        }
    }
}
