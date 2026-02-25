using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    private const string ImportEnabledEnvName = "PST_IMPORT_ENABLED";
    private const string SourceDirectoryEnvName = "PST_IMPORT_SOURCE_DIR";
    private const string TargetPstPathEnvName = "PST_IMPORT_TARGET_PATH";
    private const string BaselineAttachmentDocxName = "test-doc.docx";
    private const string BaselineAttachmentPdfName = "test-doc.pdf";
    private const string AppendedFolderName = "appended-folder";
    private const string AppendedSubject = "Appended from code";
    private const string AppendedBody = "This text appended from benchmark test.";
    private const string AppendedTo = "email3@contoso.com";
    private static readonly object ArtifactOutputSync = new();

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

    /// <summary>
    /// Memastikan Save() dapat flush perubahan ke disk sebelum Dispose dipanggil.
    /// </summary>
    [Fact]
    public void Save_ShouldPersistChangesBeforeDispose()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-save-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        PstFile? writable = null;
        try
        {
            var bootstrapper = new PstNdbWriter();
            bootstrapper.EnsureFileInitialized(temp, new PstOpenOptions { ReadOnly = false });

            var headerReader = new NdbHeaderReader();
            NdbHeader baselineHeader;
            using (var baselineStream = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                baselineHeader = headerReader.Read(baselineStream);
            }

            writable = PstFile.Open(
                temp,
                new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
                writer: new PstNdbWriter());

            writable.CreateFolder($"SaveFolder-{Guid.NewGuid():N}");
            writable.Save();

            NdbHeader updatedHeader;
            using (var updatedStream = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                updatedHeader = headerReader.Read(updatedStream);
            }

            Assert.True(
                updatedHeader.Counters.NidCounters[(int)NidType.NormalFolder] >
                baselineHeader.Counters.NidCounters[(int)NidType.NormalFolder]);
        }
        finally
        {
            writable?.Dispose();
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan SaveAsync() dapat flush perubahan ke disk sebelum Dispose dipanggil.
    /// </summary>
    [Fact]
    public async Task SaveAsync_ShouldPersistChangesBeforeDispose()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-save-async-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        PstFile? writable = null;
        try
        {
            var bootstrapper = new PstNdbWriter();
            bootstrapper.EnsureFileInitialized(temp, new PstOpenOptions { ReadOnly = false });

            var headerReader = new NdbHeaderReader();
            NdbHeader baselineHeader;
            using (var baselineStream = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                baselineHeader = headerReader.Read(baselineStream);
            }

            writable = PstFile.Open(
                temp,
                new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
                writer: new PstNdbWriter());

            writable.CreateFolder($"SaveAsyncFolder-{Guid.NewGuid():N}");
            await writable.SaveAsync(CancellationToken.None);

            NdbHeader updatedHeader;
            using (var updatedStream = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                updatedHeader = headerReader.Read(updatedStream);
            }

            Assert.True(
                updatedHeader.Counters.NidCounters[(int)NidType.NormalFolder] >
                baselineHeader.Counters.NidCounters[(int)NidType.NormalFolder]);
        }
        finally
        {
            writable?.Dispose();
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan PST baru dapat dibuat dari nol saat file target belum ada.
    /// </summary>
    [Fact]
    public void Open_WithCreateIfMissing_ShouldCreateNewPstAndPersistData()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-new-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            const string folderName = "InboxLocal";
            var subject = $"Fresh Subject {Guid.NewGuid():N}";

            using (var pst = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
                       writer: new PstNdbWriter()))
            {
                Assert.True(File.Exists(temp));
                var folder = pst.CreateFolder(folderName);
                pst.CreateMessage(folder, new PstMessageDraft
                {
                    Subject = subject,
                    Body = "Body Fresh",
                    FromName = "Tester",
                    FromAddress = "tester@example.com"
                });
            }

            var reopened = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
            var folderAfter = reopened.Folders.FirstOrDefault(item => item.Name == folderName);
            Assert.NotNull(folderAfter);

            var messageAfter = folderAfter!.Messages.FirstOrDefault(item => item.Subject == subject);
            Assert.NotNull(messageAfter);
            Assert.Equal("Body Fresh", messageAfter!.Body);
        }
        finally
        {
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan file hasil write dapat dibuka ulang dengan validasi checksum aktif.
    /// </summary>
    [Fact]
    public void CreateIfMissing_Result_ShouldOpenWithChecksumValidation()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-checksum-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            using (var pst = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
                       writer: new PstNdbWriter()))
            {
                var folder = pst.CreateFolder("ChecksumFolder");
                pst.CreateMessage(folder, new PstMessageDraft
                {
                    Subject = "Checksum Subject",
                    Body = "Checksum Body",
                    FromName = "Tester",
                    FromAddress = "tester@example.com"
                });
            }

            using var reopened = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true });
            Assert.Contains(reopened.Folders, folder => folder.Name == "ChecksumFolder");
        }
        finally
        {
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan test permanen dapat menghasilkan `artifacts/output.pst` untuk pembanding terhadap `doc/Empty.pst`.
    /// </summary>
    [Fact]
    public void CreateBenchmarkOutputPst_ShouldWriteArtifactsOutputAndProvideBaselineComparison()
    {
        lock (ArtifactOutputSync)
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
            }

            var baselineStore = ResolveBenchmarkStoreFolder(baseline);
            writable.UpdateStoreProperties(new PstStorePropertiesDraft
            {
                DisplayName = baselineStore.Name,
                Description = baselineStore.Description,
                Comment = baselineStore.Comment
            });

            var parent = writable.Folders.FirstOrDefault(folder =>
                             string.Equals(folder.Name, baselineStore.Name, StringComparison.OrdinalIgnoreCase))
                         ?? writable.Folders.FirstOrDefault(folder =>
                             string.Equals(folder.Name, "Top of Outlook data file", StringComparison.OrdinalIgnoreCase))
                         ?? writable.RootFolder
                         ?? writable.Folders.First();
            CopyFolderTreeAndMessagesFromBaseline(baselineStore, parent, writable);

            using var baselineReopened = PstFile.Open(baselinePath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true });
            using (var reopened = PstFile.Open(outputPath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true }))
            {
                Assert.True(reopened.Folders.Count > 0);
                AssertBenchmarkContentMatchesBaseline(baselineReopened, reopened);
            }

            var generatedHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(outputPath)));
            var baselineHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(baselinePath)));
            Assert.False(string.IsNullOrWhiteSpace(generatedHash));
            Assert.False(string.IsNullOrWhiteSpace(baselineHash));
            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);
            Assert.Equal(baselineHash, generatedHash);
        }
    }

    /// <summary>
    /// Memastikan benchmark append dapat membuat `artifacts/output2.pst` dari baseline lalu menambah folder/message baru.
    /// </summary>
    [Fact]
    public void CreateBenchmarkOutput2Pst_ShouldAppendFolderAndMessageWithAttachment()
    {
        lock (ArtifactOutputSync)
        {
            var outputPath = ResolveArtifactsOutput2Path();
            var baselinePath = TestData.EmptyBaselinePath;
            var outputDirectory = Path.GetDirectoryName(outputPath);
            Assert.False(string.IsNullOrWhiteSpace(outputDirectory), "Direktori artifacts tidak valid.");
            Directory.CreateDirectory(outputDirectory!);
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            File.Copy(baselinePath, outputPath, overwrite: true);

            using (var writable = PstFile.Open(
                       outputPath,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
                       writer: new PstNdbWriter()))
            {
                var parent = writable.Folders.FirstOrDefault(folder =>
                                 string.Equals(folder.Name, "empty@contoso.com", StringComparison.OrdinalIgnoreCase))
                             ?? writable.Folders.FirstOrDefault(folder =>
                                 string.Equals(folder.Name, "Top of Outlook data file", StringComparison.OrdinalIgnoreCase))
                             ?? writable.RootFolder
                             ?? writable.Folders.First();
                var appendedFolder = writable.CreateFolder(AppendedFolderName, parent);
                writable.CreateMessage(appendedFolder, new PstMessageDraft
                {
                    MessageClass = "IPM.Note",
                    FromName = "email@contoso.com",
                    FromAddress = "email@contoso.com",
                    Subject = AppendedSubject,
                    HtmlBody = $"<html><body><p>{AppendedBody}</p></body></html>",
                    Recipients = new[]
                    {
                        new PstDraftRecipient
                        {
                            RecipientType = PstRecipientType.To,
                            DisplayName = AppendedTo,
                            EmailAddress = AppendedTo,
                            SmtpAddress = AppendedTo
                        }
                    },
                    Attachments = new[]
                    {
                        new PstDraftAttachment
                        {
                            FileName = BaselineAttachmentPdfName,
                            LongFileName = BaselineAttachmentPdfName,
                            ContentType = "application/pdf",
                            ContentBytes = File.ReadAllBytes(TestData.TestDocPdfPath)
                        }
                    }
                });
            }

            using var reopened = PstFile.Open(outputPath, new PstOpenOptions { ReadOnly = true, ValidateChecksums = true });
            var appended = reopened.Folders.FirstOrDefault(folder =>
                string.Equals(folder.Name, AppendedFolderName, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(appended);
            Assert.Single(appended!.Messages);
            var message = appended.Messages[0];
            Assert.Equal(AppendedSubject, message.Subject);
            var sender = message.SenderEmailAddress ?? message.SenderSmtpAddress ?? message.SenderName;
            Assert.Equal("email@contoso.com", sender);
            var toValue = message.Recipients.FirstOrDefault(item => item.RecipientType == (int)PstRecipientType.To)?.SmtpAddress
                ?? message.Recipients.FirstOrDefault(item => item.RecipientType == (int)PstRecipientType.To)?.EmailAddress
                ?? message.DisplayTo;
            Assert.Equal(AppendedTo, toValue);
            Assert.True(
                (!string.IsNullOrWhiteSpace(message.Body) && message.Body.Contains(AppendedBody, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(message.HtmlBody) && message.HtmlBody.Contains(AppendedBody, StringComparison.OrdinalIgnoreCase)));
            AssertAttachmentMatchesFixture(message, BaselineAttachmentPdfName, TestData.TestDocPdfPath);
        }
    }

    /// <summary>
    /// Memastikan properti store (name/description/comment) bisa di-set saat membuat PST baru.
    /// </summary>
    [Fact]
    public void CreateIfMissing_WithStoreProperties_ShouldPersistStoreNameDescriptionAndComment()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-store-create-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            using (var pst = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
                       writer: new PstNdbWriter()))
            {
                pst.UpdateStoreProperties(new PstStorePropertiesDraft
                {
                    DisplayName = "aan@connusa.com",
                    Description = "Deskripsi sinkronisasi.",
                    Comment = "PST untuk sinkronisasi."
                });
            }

            using var reopened = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
            var storeFolder = reopened.Folders.FirstOrDefault(folder => folder.Name == "aan@connusa.com");
            Assert.NotNull(storeFolder);
            Assert.Equal("Deskripsi sinkronisasi.", storeFolder!.Description);
            Assert.Equal("PST untuk sinkronisasi.", storeFolder!.Comment);
        }
        finally
        {
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan properti store (name/description/comment) bisa diupdate pada PST existing.
    /// </summary>
    [Fact]
    public void OpenExisting_WithStorePropertiesUpdate_ShouldPersistLatestValues()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-store-update-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            using (var create = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
                       writer: new PstNdbWriter()))
            {
                create.UpdateStoreProperties(new PstStorePropertiesDraft
                {
                    DisplayName = "Store Awal",
                    Description = "Deskripsi awal",
                    Comment = "Komentar awal"
                });
            }

            using (var update = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
                       writer: new PstNdbWriter()))
            {
                update.UpdateStoreProperties(new PstStorePropertiesDraft
                {
                    DisplayName = "Store Final",
                    Description = "Deskripsi final",
                    Comment = "Komentar final"
                });
            }

            using var reopened = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
            var storeFolder = reopened.Folders.FirstOrDefault(folder => folder.Name == "Store Final");
            Assert.NotNull(storeFolder);
            Assert.Equal("Deskripsi final", storeFolder!.Description);
            Assert.Equal("Komentar final", storeFolder!.Comment);
        }
        finally
        {
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan bootstrap file PST baru dibangun dari builder spesifikasi tanpa resource template.
    /// </summary>
    [Fact]
    public void EnsureFileInitialized_ShouldBuildSpecificationBasedBaseline()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-resource-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            var writer = new PstNdbWriter();
            writer.EnsureFileInitialized(temp, new PstOpenOptions { ReadOnly = false });

            Assert.True(File.Exists(temp));
            using var target = File.OpenRead(temp);
            var header = new NdbHeaderReader().Read(target);
            Assert.Equal(PstFormat.Unicode, header.HeaderInfo.Format);
            Assert.Equal((uint)0x4D53, header.HeaderInfo.ClientSignature);
            Assert.Equal((ushort)0x0013, header.HeaderInfo.VersionMinor);
            Assert.Equal(PstCryptMethod.Permute, header.HeaderInfo.CryptMethod);
            Assert.True(header.RootState.IsAMapValid);
            Assert.True(header.BbtRoot.Ib > 0);
            Assert.True(header.NbtRoot.Ib > 0);
            Assert.True(header.BbtRoot.Bid.Raw > 0);
            Assert.True(header.NbtRoot.Bid.Raw > 0);

            using var pst = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
            Assert.Contains(pst.Folders, folder => folder.Name == "Root");
            Assert.Contains(pst.Folders, folder => folder.Name == "Top of Outlook data file");
            Assert.Contains(pst.Folders, folder => folder.Name == "Search Root");
            Assert.Contains(pst.Folders, folder => folder.Name == "Deleted Items");
        }
        finally
        {
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan writer gagal cepat ketika HEADER.ROOT.fAMapValid bernilai invalid.
    /// </summary>
    [Fact]
    public void Open_WhenAmapInvalid_ShouldFailFast()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-invalid-amap-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            var bootstrapper = new PstNdbWriter();
            bootstrapper.EnsureFileInitialized(temp, new PstOpenOptions { ReadOnly = false });

            using (var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                stream.Seek(0xF8, SeekOrigin.Begin);
                stream.WriteByte(0x00);
            }

            var exception = Assert.Throws<InvalidOperationException>(() =>
                PstFile.Open(
                    temp,
                    new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
                    writer: new PstNdbWriter()));

            Assert.Contains("fAMapValid", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteFileIfExists(temp);
        }
    }

    /// <summary>
    /// Memastikan counter rgnid pada header meningkat setelah alokasi folder dan message.
    /// </summary>
    [Fact]
    public void CreateFolderAndMessage_ShouldIncrementRgnidCounters()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-rgnid-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            var bootstrapper = new PstNdbWriter();
            bootstrapper.EnsureFileInitialized(temp, new PstOpenOptions { ReadOnly = false });

            var headerReader = new NdbHeaderReader();
            NdbHeader baselineHeader;
            using (var baselineStream = File.OpenRead(temp))
            {
                baselineHeader = headerReader.Read(baselineStream);
            }

            using (var pst = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
                       writer: new PstNdbWriter()))
            {
                var folder = pst.CreateFolder($"Rgnid-{Guid.NewGuid():N}");
                pst.CreateMessage(folder, new PstMessageDraft
                {
                    Subject = "Rgnid Counter",
                    Body = "Counter update",
                    FromName = "Tester",
                    FromAddress = "tester@example.com"
                });
            }

            NdbHeader updatedHeader;
            using (var updatedStream = File.OpenRead(temp))
            {
                updatedHeader = headerReader.Read(updatedStream);
            }

            Assert.True(
                updatedHeader.Counters.NidCounters[(int)NidType.NormalFolder] >
                baselineHeader.Counters.NidCounters[(int)NidType.NormalFolder]);
            Assert.True(
                updatedHeader.Counters.NidCounters[(int)NidType.NormalMessage] >
                baselineHeader.Counters.NidCounters[(int)NidType.NormalMessage]);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>
    /// Memastikan message dengan field variable-length besar tetap bisa dipersist tanpa overflow heap.
    /// </summary>
    [Fact]
    public void CreateMessage_WithLargeVariableFields_ShouldPersistWithoutHeapOverflow()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-large-{Guid.NewGuid():N}.pst");
        if (File.Exists(temp))
        {
            File.Delete(temp);
        }

        try
        {
            var subject = $"Large Subject {Guid.NewGuid():N}";
            var body = new string('B', 1500);
            var html = new string('H', 1500);
            var headers = string.Join("\r\n", Enumerable.Range(0, 180).Select(i => $"X-Large-{i:000}: {new string('Z', 8)}"));

            using (var pst = PstFile.Open(
                       temp,
                       new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
                       writer: new PstNdbWriter()))
            {
                var folder = pst.CreateFolder("LargeMails");
                pst.CreateMessage(folder, new PstMessageDraft
                {
                    Subject = subject,
                    Body = body,
                    HtmlBody = html,
                    FromName = "Tester",
                    FromAddress = "tester@example.com",
                    TransportMessageHeaders = headers
                });
            }

            var reopened = PstFile.Open(temp, new PstOpenOptions { ReadOnly = true, ValidateChecksums = false });
            var folderAfter = reopened.Folders.FirstOrDefault(item => item.Name == "LargeMails");
            Assert.NotNull(folderAfter);

            var message = folderAfter!.Messages.FirstOrDefault(item => item.Subject == subject);
            Assert.NotNull(message);
            Assert.Equal(body, message!.Body);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>
    /// Memastikan import folder .eml dari local filesystem ke PST bisa menjaga struktur hierarchy.
    /// </summary>
    [Fact]
    public void ImportEmlDirectoryTree_FromEnvironmentVariables_ShouldPreserveHierarchy()
    {
        if (!IsImportScenarioEnabled())
        {
            return;
        }

        var sourceDirectory = GetRequiredDirectoryFromEnvironmentVariable(SourceDirectoryEnvName);
        var targetPstPath = GetRequiredPathFromEnvironmentVariable(TargetPstPathEnvName);
        EnsureTargetPstExists(targetPstPath);

        using var pst = PstFile.Open(
            targetPstPath,
            new PstOpenOptions { ReadOnly = false, ValidateChecksums = false },
            writer: new PstNdbWriter());

        var parentFolder = pst.RootFolder ?? pst.Folders.First();
        var importRoot = pst.CreateFolder(Path.GetFileName(sourceDirectory), parentFolder);

        var (importedFolderCount, importedMessageCount) = ImportDirectoryTree(pst, importRoot, sourceDirectory);
        Assert.True(importedFolderCount > 0, "Import hierarchy folder tidak terbuat.");
        Assert.True(importedMessageCount > 0, "Tidak ada file .eml yang berhasil diimport.");
    }

    /// <summary>
    /// Melakukan import rekursif file .eml dari folder local ke folder PST.
    /// </summary>
    /// <param name="pst">Instance PST yang terbuka dalam mode write.</param>
    /// <param name="destinationFolder">Folder PST tujuan untuk folder local saat ini.</param>
    /// <param name="sourceDirectory">Path folder source local.</param>
    /// <returns>Jumlah folder dibuat dan jumlah pesan yang diimport.</returns>
    private static (int FolderCount, int MessageCount) ImportDirectoryTree(
        PstFile pst,
        PstFolder destinationFolder,
        string sourceDirectory)
    {
        var folderCount = 1;
        var messageCount = 0;

        foreach (var emlPath in Directory.EnumerateFiles(sourceDirectory, "*.eml", SearchOption.TopDirectoryOnly))
        {
            pst.ImportEml(destinationFolder, emlPath);
            messageCount++;
        }

        foreach (var childDirectory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.TopDirectoryOnly))
        {
            var childFolder = pst.CreateFolder(Path.GetFileName(childDirectory), destinationFolder);
            var (childFolderCount, childMessageCount) = ImportDirectoryTree(pst, childFolder, childDirectory);
            folderCount += childFolderCount;
            messageCount += childMessageCount;
        }

        return (folderCount, messageCount);
    }

    /// <summary>
    /// Memastikan skenario import lokal diaktifkan oleh environment variable.
    /// </summary>
    /// <returns>True jika skenario import diaktifkan.</returns>
    private static bool IsImportScenarioEnabled()
    {
        var enabledRaw = Environment.GetEnvironmentVariable(ImportEnabledEnvName);
        return string.Equals(enabledRaw, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(enabledRaw, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Mengambil path file dari environment variable dan memastikan nilainya tersedia.
    /// </summary>
    /// <param name="envName">Nama environment variable.</param>
    /// <returns>Path absolut.</returns>
    private static string GetRequiredPathFromEnvironmentVariable(string envName)
    {
        var raw = Environment.GetEnvironmentVariable(envName);
        Assert.False(string.IsNullOrWhiteSpace(raw), $"Environment variable '{envName}' wajib diisi.");
        return Path.GetFullPath(raw!);
    }

    /// <summary>
    /// Mengambil path direktori dari environment variable dan memastikan direktorinya tersedia.
    /// </summary>
    /// <param name="envName">Nama environment variable.</param>
    /// <returns>Path absolut direktori.</returns>
    private static string GetRequiredDirectoryFromEnvironmentVariable(string envName)
    {
        var directoryPath = GetRequiredPathFromEnvironmentVariable(envName);
        Assert.True(Directory.Exists(directoryPath), $"Direktori '{directoryPath}' dari env '{envName}' tidak ditemukan.");
        return directoryPath;
    }

    /// <summary>
    /// Memastikan file PST target tersedia; jika belum ada, buat menggunakan flow CreateIfMissing.
    /// </summary>
    /// <param name="targetPstPath">Path PST target.</param>
    private static void EnsureTargetPstExists(string targetPstPath)
    {
        if (File.Exists(targetPstPath))
        {
            return;
        }

        var targetDirectory = Path.GetDirectoryName(targetPstPath);
        Assert.False(string.IsNullOrWhiteSpace(targetDirectory), $"Path target PST '{targetPstPath}' tidak valid.");
        Directory.CreateDirectory(targetDirectory!);

        using var pst = PstFile.Open(
            targetPstPath,
            new PstOpenOptions { ReadOnly = false, ValidateChecksums = false, CreateIfMissing = true },
            writer: new PstNdbWriter());

        Assert.True(File.Exists(targetPstPath), $"Gagal membuat file target PST '{targetPstPath}' dengan CreateIfMissing.");
    }

    /// <summary>
    /// Menghapus file bila ada dengan retry singkat saat handle belum terlepas.
    /// </summary>
    /// <param name="path">Path file.</param>
    private static void DeleteFileIfExists(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(30);
            }
        }
    }

    /// <summary>
    /// Menentukan path output benchmark permanen pada folder artifacts repository.
    /// </summary>
    /// <returns>Path absolut `artifacts/output.pst`.</returns>
    private static string ResolveArtifactsOutputPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var repositoryRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        return Path.Combine(repositoryRoot, "artifacts", "output.pst");
    }

    /// <summary>
    /// Menentukan path output benchmark append permanen pada folder artifacts repository.
    /// </summary>
    /// <returns>Path absolut `artifacts/output2.pst`.</returns>
    private static string ResolveArtifactsOutput2Path()
    {
        var baseDir = AppContext.BaseDirectory;
        var repositoryRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        return Path.Combine(repositoryRoot, "artifacts", "output2.pst");
    }

    /// <summary>
    /// Menyalin seluruh folder dan message baseline ke PST target secara rekursif.
    /// </summary>
    /// <param name="baselineFolder">Folder baseline sumber.</param>
    /// <param name="targetParent">Folder parent pada PST target.</param>
    /// <param name="writable">Instance PST target dalam mode tulis.</param>
    private static void CopyFolderTreeAndMessagesFromBaseline(PstFolder baselineFolder, PstFolder targetParent, PstFile writable)
    {
        foreach (var baselineMessage in baselineFolder.Messages)
        {
            writable.CreateMessage(targetParent, BuildMessageDraftFromBaseline(baselineMessage));
        }

        foreach (var baselineChild in baselineFolder.SubFolders)
        {
            var targetChild = targetParent.SubFolders.FirstOrDefault(folder =>
                string.Equals(folder.Name, baselineChild.Name, StringComparison.OrdinalIgnoreCase))
                ?? writable.CreateFolder(baselineChild.Name, targetParent);
            CopyFolderTreeAndMessagesFromBaseline(baselineChild, targetChild, writable);
        }
    }

    /// <summary>
    /// Membangun draft message target dari object message baseline.
    /// </summary>
    /// <param name="source">Message baseline sumber.</param>
    /// <returns>Draft message untuk ditulis ke PST target.</returns>
    private static PstMessageDraft BuildMessageDraftFromBaseline(PstMessage source)
    {
        var fromAddress = source.SenderEmailAddress ?? source.SenderSmtpAddress ?? source.SenderName;
        var recipients = BuildRecipientsFromBaseline(source);
        var attachments = source.Attachments
            .Select(attachment => new PstDraftAttachment
            {
                FileName = attachment.FileName ?? attachment.LongFileName,
                LongFileName = attachment.LongFileName ?? attachment.FileName,
                ContentType = attachment.MimeTag,
                ContentId = attachment.ContentId,
                IsInline = !string.IsNullOrWhiteSpace(attachment.ContentId),
                ContentBytes = attachment.ReadContentBytes()
            })
            .ToArray();

        var fallbackBody = !string.IsNullOrWhiteSpace(source.HtmlBody) ? null : source.Body;
        var isDraft = source.MessageFlags.HasValue && (source.MessageFlags.Value & 0x0008) != 0;

        return new PstMessageDraft
        {
            MessageClass = source.MessageClass,
            FromName = source.SenderName ?? fromAddress,
            FromAddress = fromAddress,
            Subject = source.Subject,
            Body = fallbackBody,
            HtmlBody = source.HtmlBody,
            MessageId = source.InternetMessageId,
            SentTime = source.DeliveryTime,
            ClientSubmitTime = source.ClientSubmitTime,
            LastModificationTime = source.LastModificationTime,
            MessageFlags = source.MessageFlags,
            IsDraft = isDraft,
            ReadReceiptRequested = source.ReadReceiptRequested,
            DeliveryReceiptRequested = source.DeliveryReceiptRequested,
            Importance = source.Importance,
            Priority = source.Priority,
            Sensitivity = source.Sensitivity,
            TransportMessageHeaders = source.TransportMessageHeaders,
            ConversationTopic = source.ConversationTopic,
            ConversationIndex = source.ConversationIndex?.ToArray(),
            Recipients = recipients,
            Attachments = attachments
        };
    }

    /// <summary>
    /// Membangun daftar penerima draft dari data baseline.
    /// </summary>
    /// <param name="source">Message baseline sumber.</param>
    /// <returns>Daftar penerima untuk draft message.</returns>
    private static PstDraftRecipient[] BuildRecipientsFromBaseline(PstMessage source)
    {
        if (source.Recipients.Count > 0)
        {
            return source.Recipients
                .Select(recipient => new PstDraftRecipient
                {
                    RecipientType = ConvertRecipientType(recipient.RecipientType),
                    DisplayName = recipient.DisplayName ?? recipient.SmtpAddress ?? recipient.EmailAddress,
                    EmailAddress = recipient.EmailAddress ?? recipient.SmtpAddress,
                    SmtpAddress = recipient.SmtpAddress ?? recipient.EmailAddress
                })
                .ToArray();
        }

        var displayTo = source.DisplayTo;
        if (!string.IsNullOrWhiteSpace(displayTo))
        {
            return new[]
            {
                new PstDraftRecipient
                {
                    RecipientType = PstRecipientType.To,
                    DisplayName = displayTo,
                    EmailAddress = displayTo,
                    SmtpAddress = displayTo
                }
            };
        }

        return Array.Empty<PstDraftRecipient>();
    }

    /// <summary>
    /// Mengonversi numeric recipient type baseline ke enum draft recipient type.
    /// </summary>
    /// <param name="recipientType">Nilai recipient type pada baseline.</param>
    /// <returns>Enum recipient type untuk draft.</returns>
    private static PstRecipientType ConvertRecipientType(int? recipientType)
    {
        return recipientType switch
        {
            (int)PstRecipientType.Cc => PstRecipientType.Cc,
            (int)PstRecipientType.Bcc => PstRecipientType.Bcc,
            _ => PstRecipientType.To
        };
    }

    /// <summary>
    /// Mengambil folder store baseline utama untuk benchmark.
    /// </summary>
    /// <param name="baseline">Instance baseline PST.</param>
    /// <returns>Folder store baseline.</returns>
    private static PstFolder ResolveBenchmarkStoreFolder(PstFile baseline)
    {
        var store = baseline.Folders.FirstOrDefault(folder =>
                        !string.IsNullOrWhiteSpace(folder.Description) ||
                        !string.IsNullOrWhiteSpace(folder.Comment))
                    ?? baseline.Folders.FirstOrDefault(folder =>
                        !string.Equals(folder.Id, "root", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException("Store folder baseline tidak ditemukan.");
        Assert.False(string.IsNullOrWhiteSpace(store.Name), "Nama store baseline kosong.");
        return store;
    }

    /// <summary>
    /// Memastikan konten benchmark output sesuai baseline terbaru berbasis perbandingan object.
    /// </summary>
    /// <param name="baseline">PST baseline pembanding.</param>
    /// <param name="actual">PST hasil output benchmark.</param>
    private static void AssertBenchmarkContentMatchesBaseline(PstFile baseline, PstFile actual)
    {
        var baselineStore = ResolveBenchmarkStoreFolder(baseline);
        var actualStore = actual.Folders.FirstOrDefault(folder =>
            string.Equals(folder.Name, baselineStore.Name, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(actualStore);
        Assert.Equal(baselineStore.Description, actualStore!.Description);
        Assert.Equal(baselineStore.Comment, actualStore.Comment);

        AssertFolderTreeMatches(baselineStore, actualStore);
    }

    /// <summary>
    /// Memastikan tree folder dan message antara baseline dan output identik berdasarkan object.
    /// </summary>
    /// <param name="expectedFolder">Folder baseline expected.</param>
    /// <param name="actualFolder">Folder output actual.</param>
    private static void AssertFolderTreeMatches(PstFolder expectedFolder, PstFolder actualFolder)
    {
        Assert.Equal(expectedFolder.Messages.Count, actualFolder.Messages.Count);
        for (var index = 0; index < expectedFolder.Messages.Count; index++)
        {
            AssertMessageMatches(expectedFolder.Messages[index], actualFolder.Messages[index]);
        }

        Assert.Equal(expectedFolder.SubFolders.Count, actualFolder.SubFolders.Count);
        foreach (var expectedChild in expectedFolder.SubFolders)
        {
            var actualChild = actualFolder.SubFolders.FirstOrDefault(folder =>
                string.Equals(folder.Name, expectedChild.Name, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(actualChild);
            AssertFolderTreeMatches(expectedChild, actualChild!);
        }
    }

    /// <summary>
    /// Memastikan message actual identik dengan message baseline.
    /// </summary>
    /// <param name="expected">Message baseline expected.</param>
    /// <param name="actual">Message output actual.</param>
    private static void AssertMessageMatches(PstMessage expected, PstMessage actual)
    {
        Assert.Equal(expected.Subject, actual.Subject);
        Assert.Equal(expected.MessageClass, actual.MessageClass);
        Assert.Equal(expected.Body, actual.Body);
        Assert.Equal(expected.HtmlBody, actual.HtmlBody);

        var expectedSender = expected.SenderEmailAddress ?? expected.SenderSmtpAddress ?? expected.SenderName;
        var actualSender = actual.SenderEmailAddress ?? actual.SenderSmtpAddress ?? actual.SenderName;
        Assert.Equal(expectedSender, actualSender);

        var expectedTo = expected.Recipients.FirstOrDefault(item => item.RecipientType == (int)PstRecipientType.To)?.SmtpAddress
            ?? expected.Recipients.FirstOrDefault(item => item.RecipientType == (int)PstRecipientType.To)?.EmailAddress
            ?? expected.DisplayTo;
        var actualTo = actual.Recipients.FirstOrDefault(item => item.RecipientType == (int)PstRecipientType.To)?.SmtpAddress
            ?? actual.Recipients.FirstOrDefault(item => item.RecipientType == (int)PstRecipientType.To)?.EmailAddress
            ?? actual.DisplayTo;
        Assert.Equal(expectedTo, actualTo);

        Assert.Equal(expected.Attachments.Count, actual.Attachments.Count);
        foreach (var expectedAttachment in expected.Attachments)
        {
            var expectedFileName = expectedAttachment.LongFileName ?? expectedAttachment.FileName;
            Assert.False(string.IsNullOrWhiteSpace(expectedFileName), "Nama attachment baseline kosong.");
            var actualAttachment = actual.Attachments.FirstOrDefault(item =>
                string.Equals(item.LongFileName, expectedFileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.FileName, expectedFileName, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(actualAttachment);

            var expectedBytes = expectedAttachment.ReadContentBytes();
            var actualBytes = actualAttachment!.ReadContentBytes();
            Assert.NotNull(expectedBytes);
            Assert.NotNull(actualBytes);
            Assert.Equal(expectedBytes!.Length, actualBytes!.Length);
            Assert.Equal(
                Convert.ToHexString(SHA256.HashData(expectedBytes)),
                Convert.ToHexString(SHA256.HashData(actualBytes)));
        }
    }

    /// <summary>
    /// Memastikan attachment pada message sama persis dengan fixture file fisik.
    /// </summary>
    /// <param name="message">Message benchmark.</param>
    /// <param name="fileName">Nama attachment.</param>
    /// <param name="fixturePath">Path fixture source.</param>
    private static void AssertAttachmentMatchesFixture(PstMessage message, string fileName, string fixturePath)
    {
        var expectedBytes = File.ReadAllBytes(fixturePath);
        var attachment = message.Attachments.FirstOrDefault(item =>
            string.Equals(item.LongFileName, fileName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(attachment);

        var actualBytes = attachment!.ReadContentBytes();
        Assert.NotNull(actualBytes);
        Assert.Equal(expectedBytes.Length, actualBytes!.Length);
        Assert.Equal(
            Convert.ToHexString(SHA256.HashData(expectedBytes)),
            Convert.ToHexString(SHA256.HashData(actualBytes)));
    }
}
