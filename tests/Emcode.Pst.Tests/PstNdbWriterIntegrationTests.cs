using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            Assert.True(header.RootState.IsAMapValid);
            Assert.True(header.BbtRoot.Ib > 0);
            Assert.True(header.NbtRoot.Ib > 0);
            Assert.True(header.BbtRoot.Bid.Raw > 0);
            Assert.True(header.NbtRoot.Bid.Raw > 0);
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
}
