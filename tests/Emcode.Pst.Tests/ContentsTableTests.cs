using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ltp;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian contents table dan urutan pesan.
/// </summary>
public sealed class ContentsTableTests
{
    /// <summary>
    /// Memastikan urutan message mengikuti row matrix pada contents table.
    /// </summary>
    [Fact]
    public void ContentsTable_OrderMatchesFolderMessages()
    {
        using var stream = File.OpenRead(TestData.Sample1Path);
        var header = new NdbHeaderReader().Read(stream);
        var btreeReader = new PstBTreeReader(stream, header.HeaderInfo.Format);
        var bbt = btreeReader.ReadBbt(header.BbtRoot);
        var nbt = btreeReader.ReadNbt(header.NbtRoot);
        var blockReader = new PstBlockReader(stream, header.HeaderInfo.Format, header.HeaderInfo.CryptMethod, bbt);
        NbtEntry? targetFolder = null;
        IReadOnlyList<uint> rowIds = Array.Empty<uint>();
        foreach (var entry in nbt.Values)
        {
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            var ids = ReadContentsTableRowIds(entry, nbt, blockReader, header.HeaderInfo.Format);
            if (ids.Count > 0)
            {
                targetFolder = entry;
                rowIds = ids;
                break;
            }
        }

        Assert.NotNull(targetFolder);
        Assert.NotEmpty(rowIds);

        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var folder = pst.Folders.First(f => f.Id == targetFolder!.Nid.ToString());
        var messageIds = folder.Messages.Select(message => message.Id).ToList();

        var messageEntryMap = nbt.Values
            .Where(entry => entry.Nid.Type == NidType.NormalMessage)
            .ToDictionary(entry => entry.Nid.Value, entry => entry);

        var expectedIds = rowIds
            .Where(id => messageEntryMap.ContainsKey(id))
            .Select(id => new Nid(id).ToString())
            .ToList();

        Assert.Equal(expectedIds, messageIds);
    }

    /// <summary>
    /// Memastikan subject dan delivery time terbaca dari PC message.
    /// </summary>
    [Fact]
    public void Open_Sample1_ParsesSubjectAndDeliveryTime()
    {
        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var messages = pst.Folders.SelectMany(folder => folder.Messages).ToList();
        Assert.NotEmpty(messages);
        Assert.Contains(messages, message =>
            !string.IsNullOrWhiteSpace(message.Subject) && message.DeliveryTime.HasValue);
    }

    /// <summary>
    /// Memastikan sender, body, dan html body terbaca dari PC message.
    /// </summary>
    [Fact]
    public void Open_Sample1_ParsesSenderBodyAndHtmlBody()
    {
        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var messages = pst.Folders.SelectMany(folder => folder.Messages).ToList();
        Assert.NotEmpty(messages);

        Assert.Contains(messages, message => !string.IsNullOrWhiteSpace(message.SenderName));
        Assert.Contains(messages, message => !string.IsNullOrWhiteSpace(message.Body));
        Assert.Contains(messages, message => !string.IsNullOrWhiteSpace(message.HtmlBody));
    }

    /// <summary>
    /// Memastikan ukuran pesan terbaca dari PC message.
    /// </summary>
    [Fact]
    public void Open_Sample1_ParsesMessageSize()
    {
        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var messages = pst.Folders.SelectMany(folder => folder.Messages).ToList();
        Assert.NotEmpty(messages);
        Assert.Contains(messages, message => message.Size.HasValue);
    }

    /// <summary>
    /// Memastikan properti MAPI tambahan message terbaca bila tersedia.
    /// </summary>
    [Fact]
    public void Open_Sample1_ParsesExtendedMessageProperties()
    {
        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var messages = pst.Folders.SelectMany(folder => folder.Messages).ToList();
        Assert.NotEmpty(messages);
        Assert.Contains(messages, message =>
            !string.IsNullOrWhiteSpace(message.InternetMessageId) ||
            !string.IsNullOrWhiteSpace(message.DisplayTo) ||
            !string.IsNullOrWhiteSpace(message.SenderEmailAddress) ||
            message.ClientSubmitTime.HasValue ||
            message.LastModificationTime.HasValue ||
            message.MessageFlags.HasValue ||
            message.ReadReceiptRequested.HasValue ||
            message.HasAttachments.HasValue);
    }

    /// <summary>
    /// Memastikan recipient dan attachment terbaca saat data tersedia.
    /// </summary>
    [Fact]
    public void Open_Sample1_ParsesRecipientsAndAttachmentsWhenPresent()
    {
        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var messages = pst.Folders.SelectMany(folder => folder.Messages).ToList();
        Assert.NotEmpty(messages);

        var recipients = messages.SelectMany(message => message.Recipients).ToList();
        if (recipients.Count > 0)
        {
            Assert.Contains(recipients, recipient =>
                recipient.RecipientType.HasValue ||
                !string.IsNullOrWhiteSpace(recipient.EmailAddress) ||
                !string.IsNullOrWhiteSpace(recipient.SmtpAddress));
        }

        var attachments = messages.SelectMany(message => message.Attachments).ToList();
        if (attachments.Count > 0)
        {
            Assert.Contains(attachments, attachment =>
                attachment.AttachNumber.HasValue ||
                !string.IsNullOrWhiteSpace(attachment.FileName) ||
                !string.IsNullOrWhiteSpace(attachment.LongFileName) ||
                attachment.Size.HasValue ||
                !string.IsNullOrWhiteSpace(attachment.MimeTag) ||
                !string.IsNullOrWhiteSpace(attachment.ContentId) ||
                attachment.AttachMethod.HasValue);
        }
    }

    /// <summary>
    /// Memastikan konten attachment dapat dibaca bila tersedia (sinkron).
    /// </summary>
    [Fact]
    public void Open_Sample1_CanReadAttachmentContentWhenAvailable()
    {
        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var attachments = pst.Folders.SelectMany(folder => folder.Messages)
            .SelectMany(message => message.Attachments)
            .ToList();

        if (attachments.Count == 0)
        {
            return;
        }

        byte[]? bytes = null;
        PstAttachment? target = null;
        foreach (var attachment in attachments)
        {
            bytes = attachment.ReadContentBytes();
            if (bytes is { Length: > 0 })
            {
                target = attachment;
                break;
            }
        }

        if (target is null || bytes is null)
        {
            return;
        }

        using var stream = target.OpenContentStream();
        Assert.NotNull(stream);
        Assert.Equal(bytes.Length, stream!.Length);
    }

    /// <summary>
    /// Memastikan konten attachment dapat dibaca bila tersedia (asinkron).
    /// </summary>
    [Fact]
    public async Task Open_Sample1_CanReadAttachmentContentAsyncWhenAvailable()
    {
        using var pst = await PstFile.OpenAsync(
            TestData.Sample1Path,
            new PstOpenOptions
            {
                ReadOnly = true,
                ValidateChecksums = false
            },
            cancellationToken: CancellationToken.None);

        var attachments = pst.Folders.SelectMany(folder => folder.Messages)
            .SelectMany(message => message.Attachments)
            .ToList();

        if (attachments.Count == 0)
        {
            return;
        }

        byte[]? bytes = null;
        PstAttachment? target = null;
        foreach (var attachment in attachments)
        {
            bytes = await attachment.ReadContentBytesAsync(CancellationToken.None);
            if (bytes is { Length: > 0 })
            {
                target = attachment;
                break;
            }
        }

        if (target is null || bytes is null)
        {
            return;
        }

        await using var stream = await target.OpenContentStreamAsync(CancellationToken.None);
        Assert.NotNull(stream);
        Assert.Equal(bytes.Length, stream!.Length);
    }

    /// <summary>
    /// Membaca row ID contents table untuk folder tertentu.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Daftar row ID.</returns>
    private static IReadOnlyList<uint> ReadContentsTableRowIds(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        var index = folderEntry.Nid.Index;
        var contentsNidValue = (index << 5) | (uint)NidType.ContentsTable;
        if (!nbtEntries.TryGetValue(contentsNidValue, out var tableEntry))
        {
            var assocNidValue = (index << 5) | (uint)NidType.AssocContentsTable;
            if (!nbtEntries.TryGetValue(assocNidValue, out tableEntry))
            {
                return Array.Empty<uint>();
            }
        }

        var tableBlocks = blockReader.ReadDataBlocks(tableEntry.BidData);
        if (tableBlocks.Count == 0)
        {
            return Array.Empty<uint>();
        }

        var tableHeap = new HeapOnNode(tableBlocks);
        var tableSubnodes = new SubnodeReader(blockReader, format, tableEntry.BidSub);
        var tableContext = new TableContext(tableHeap, tableSubnodes);
        return tableContext.ReadRowIds();
    }
}
