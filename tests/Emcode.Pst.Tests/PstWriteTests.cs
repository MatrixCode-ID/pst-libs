using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian operasi write in-memory untuk PST.
/// </summary>
public sealed class PstWriteTests
{
    /// <summary>
    /// Memastikan CreateMessage menambah pesan baru ke folder.
    /// </summary>
    [Fact]
    public void CreateMessage_ShouldAppendMessage()
    {
        var writer = new PstInMemoryWriter();
        var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions { ReadOnly = false }, writer: writer);
        var folder = pst.RootFolder ?? pst.Folders.First();
        var initialCount = folder.Messages.Count;

        var draft = new PstMessageDraft
        {
            Subject = "Draft Subject",
            Body = "Draft Body",
            HtmlBody = "<b>Draft Body</b>",
            FromName = "Tester",
            FromAddress = "tester@example.com",
            Recipients = new[]
            {
                new PstDraftRecipient
                {
                    RecipientType = PstRecipientType.To,
                    DisplayName = "Target",
                    EmailAddress = "target@example.com"
                }
            },
            Attachments = new[]
            {
                new PstDraftAttachment
                {
                    FileName = "note.txt",
                    ContentType = "text/plain",
                    ContentBytes = Encoding.UTF8.GetBytes("hello")
                }
            }
        };

        var message = pst.CreateMessage(folder, draft);

        Assert.Equal(initialCount + 1, folder.Messages.Count);
        Assert.Equal("Draft Subject", message.Subject);
        Assert.Equal("Tester", message.SenderName);
        Assert.True(message.HasAttachments);
        var content = message.Attachments.First().ReadContentBytes();
        Assert.Equal("hello", Encoding.UTF8.GetString(content ?? Array.Empty<byte>()));
    }

    /// <summary>
    /// Memastikan import .eml menghasilkan pesan baru dan body terisi.
    /// </summary>
    [Fact]
    public void ImportEml_ShouldCreateMessage()
    {
        var writer = new PstInMemoryWriter();
        var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions { ReadOnly = false }, writer: writer);
        var folder = pst.RootFolder ?? pst.Folders.First();

        var emlPath = CreateTempEml();
        try
        {
            var message = pst.ImportEml(folder, emlPath);
            Assert.Equal("Hello EML", message.Subject);
            Assert.Equal("Hello world", message.Body?.Trim());
            Assert.Equal("Alice", message.SenderName);
        }
        finally
        {
            File.Delete(emlPath);
        }
    }

    /// <summary>
    /// Memastikan API async create message berjalan.
    /// </summary>
    [Fact]
    public async Task CreateMessageAsync_ShouldAppendMessage()
    {
        var writer = new PstInMemoryWriter();
        var pst = await PstFile.OpenAsync(TestData.Sample1Path, new PstOpenOptions { ReadOnly = false }, writer: writer);
        var folder = pst.RootFolder ?? pst.Folders.First();
        var initialCount = folder.Messages.Count;

        var draft = new PstMessageDraft
        {
            Subject = "Async Draft",
            Body = "Async Body",
            Recipients = new[]
            {
                new PstDraftRecipient
                {
                    RecipientType = PstRecipientType.To,
                    EmailAddress = "async@example.com"
                }
            }
        };

        var message = await pst.CreateMessageAsync(folder, draft, CancellationToken.None);

        Assert.Equal(initialCount + 1, folder.Messages.Count);
        Assert.Equal("Async Draft", message.Subject);
    }

    private static string CreateTempEml()
    {
        var content = new StringBuilder()
            .AppendLine("From: Alice <alice@example.com>")
            .AppendLine("To: Bob <bob@example.com>")
            .AppendLine("Subject: Hello EML")
            .AppendLine("Content-Type: text/plain; charset=utf-8")
            .AppendLine()
            .AppendLine("Hello world")
            .ToString();

        var path = Path.Combine(Path.GetTempPath(), $"pst-eml-{Guid.NewGuid():N}.eml");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }
}
