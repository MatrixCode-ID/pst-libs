using System;
using System.IO;
using System.Linq;
using System.Text;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian integrasi PstNdbWriter untuk persist folder dan message ke disk.
/// </summary>
public sealed class PstNdbWriterIntegrationTests
{
    /// <summary>
    /// Memastikan create folder + message dapat dipersist dan terbaca ulang.
    /// </summary>
    [Fact]
    public void CreateFolderAndMessage_ShouldPersistToDisk()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-write-{Guid.NewGuid():N}.pst");
        File.Copy(TestData.Sample1Path, temp, true);

        try
        {
            var folderName = $"CodexFolder-{Guid.NewGuid():N}";
            var subject = $"Persist Subject {Guid.NewGuid():N}";
            var attachmentBytes = Encoding.UTF8.GetBytes("hello");
            var sentTime = new DateTimeOffset(2026, 2, 15, 11, 43, 0, TimeSpan.Zero);
            var clientSubmitTime = sentTime.AddMinutes(1);
            var modificationTime = sentTime.AddMinutes(2);
            var conversationIndex = new byte[] { 0x01, 0x22, 0x33, 0x44 };
            const string transportHeaders = "From: Tester <tester@example.com>\r\nX-Codex: Plan27";

            using (var pst = PstFile.Open(temp, new PstOpenOptions { ReadOnly = false, ValidateChecksums = false }, writer: new PstNdbWriter()))
            {
                var parent = pst.Folders.First();
                var newFolder = pst.CreateFolder(folderName, parent);

                var draft = new PstMessageDraft
                {
                    Subject = subject,
                    Body = "Body Persist",
                    FromName = "Tester",
                    FromAddress = "tester@example.com",
                    MessageClass = "IPM.Note",
                    IsDraft = true,
                    SentTime = sentTime,
                    ClientSubmitTime = clientSubmitTime,
                    LastModificationTime = modificationTime,
                    ReadReceiptRequested = true,
                    DeliveryReceiptRequested = false,
                    Importance = 2,
                    Priority = 1,
                    Sensitivity = 0,
                    TransportMessageHeaders = transportHeaders,
                    ConversationTopic = "Thread Plan27",
                    ConversationIndex = conversationIndex,
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
                            ContentBytes = attachmentBytes
                        }
                    }
                };

                pst.CreateMessage(newFolder, draft);
            }

            var reopened = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
            var folder = reopened.Folders.FirstOrDefault(item => item.Name == folderName);
            Assert.NotNull(folder);

            var message = folder!.Messages.FirstOrDefault(item => item.Subject == subject);
            Assert.NotNull(message);
            Assert.Equal("Body Persist", message!.Body);
            Assert.Equal("IPM.Note", message.MessageClass);
            Assert.Equal(sentTime, message.DeliveryTime);
            Assert.Equal(clientSubmitTime, message.ClientSubmitTime);
            Assert.Equal(modificationTime, message.LastModificationTime);
            Assert.Equal(true, message.ReadReceiptRequested);
            Assert.Equal(false, message.DeliveryReceiptRequested);
            Assert.Equal(2, message.Importance);
            Assert.Equal(1, message.Priority);
            Assert.Equal(0, message.Sensitivity);
            Assert.Equal("Thread Plan27", message.ConversationTopic);
            Assert.Equal(transportHeaders, message.TransportMessageHeaders);
            Assert.True(message.ConversationIndex.HasValue);
            Assert.Equal(conversationIndex, message.ConversationIndex!.Value.ToArray());
            Assert.True(message.MessageFlags.HasValue);
            Assert.NotEqual(0, message.MessageFlags!.Value & 0x0008);
            Assert.NotEqual(0, message.MessageFlags!.Value & 0x0010);
            Assert.Equal("Target", message.Recipients.First().DisplayName);
            Assert.Equal("SMTP", message.Recipients.First().AddressType);
            Assert.True(message.Attachments.Count > 0);

            var attachment = message.Attachments.First();
            var content = attachment.ReadContentBytes();
            Assert.NotNull(content);
            Assert.Equal("hello", Encoding.UTF8.GetString(content!));
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
