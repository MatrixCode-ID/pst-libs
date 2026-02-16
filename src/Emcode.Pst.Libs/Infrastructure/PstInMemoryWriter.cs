using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application.Abstractions;
using Emcode.Pst.Domain;
using Emcode.Pst.Shared;

namespace Emcode.Pst.Infrastructure;

/// <summary>
/// Implementasi writer berbasis in-memory untuk membuat draft pesan tanpa menulis ke disk.
/// </summary>
public sealed class PstInMemoryWriter : IPstWriter, IPstWriterWithContext
{
    private readonly PstEmlParser _parser;
    private readonly PstInMemoryAttachmentContentProvider _attachmentProvider = new();
    private readonly Dictionary<string, PstFolder> _messageFolderIndex = new(StringComparer.OrdinalIgnoreCase);
    private long _messageCounter;
    private long _attachmentCounter;
    private PstWriteContext? _context;

    /// <summary>
    /// Membuat writer in-memory dengan parser .eml default.
    /// </summary>
    public PstInMemoryWriter()
        : this(new PstEmlParser())
    {
    }

    /// <summary>
    /// Membuat writer in-memory dengan parser .eml yang disuntikkan.
    /// </summary>
    /// <param name="parser">Parser .eml untuk import.</param>
    internal PstInMemoryWriter(PstEmlParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    /// <summary>
    /// Menginisialisasi writer dengan konteks PST.
    /// </summary>
    /// <param name="context">Konteks PST untuk operasi write.</param>
    public void Initialize(PstWriteContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        EnsureWritableOptions();
    }

    /// <summary>
    /// Menginisialisasi writer dengan konteks PST secara asynchronous.
    /// </summary>
    /// <param name="context">Konteks PST untuk operasi write.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)
    {
        Initialize(context);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Membuat folder baru pada PST secara in-memory.
    /// </summary>
    /// <param name="name">Nama folder baru.</param>
    /// <param name="parent">Folder parent jika membuat subfolder.</param>
    /// <returns>Folder yang dibuat.</returns>
    public PstFolder CreateFolder(string name, PstFolder? parent)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        EnsureReady();

        var folder = new PstFolder(NextFolderId(), name);
        AttachFolder(parent, folder);
        return folder;
    }

    /// <summary>
    /// Membuat folder baru pada PST secara in-memory (async).
    /// </summary>
    /// <param name="name">Nama folder baru.</param>
    /// <param name="parent">Folder parent jika membuat subfolder.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Folder yang dibuat.</returns>
    public Task<PstFolder> CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateFolder(name, parent));
    }

    /// <summary>
    /// Membuat pesan baru pada folder tertentu secara in-memory.
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="draft">Draft data pesan.</param>
    /// <returns>Pesan yang dibuat.</returns>
    public PstMessage CreateMessage(PstFolder folder, PstMessageDraft draft)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNull(draft, nameof(draft));
        EnsureReady();

        var message = new PstMessage(NextMessageId());
        ApplyDraft(message, draft);
        AttachMessage(folder, message);
        return message;
    }

    /// <summary>
    /// Membuat pesan baru pada folder tertentu secara in-memory (async).
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="draft">Draft data pesan.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan yang dibuat.</returns>
    public Task<PstMessage> CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateMessage(folder, draft));
    }

    /// <summary>
    /// Mengimpor file .eml ke folder PST sebagai pesan baru (in-memory).
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="emlPath">Path file .eml.</param>
    /// <returns>Pesan yang dibuat.</returns>
    public PstMessage ImportEml(PstFolder folder, string emlPath)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        EnsureReady();

        var draft = _parser.Parse(emlPath);
        return CreateMessage(folder, draft);
    }

    /// <summary>
    /// Mengimpor file .eml ke folder PST sebagai pesan baru secara asynchronous (in-memory).
    /// </summary>
    /// <param name="folder">Folder target pesan.</param>
    /// <param name="emlPath">Path file .eml.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan yang dibuat.</returns>
    public async Task<PstMessage> ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        EnsureReady();

        var draft = await _parser.ParseAsync(emlPath, cancellationToken).ConfigureAwait(false);
        return CreateMessage(folder, draft);
    }

    /// <summary>
    /// Memperbarui pesan yang sudah ada secara in-memory.
    /// </summary>
    /// <param name="message">Pesan yang akan diperbarui.</param>
    /// <param name="draft">Draft data terbaru.</param>
    public void UpdateMessage(PstMessage message, PstMessageDraft draft)
    {
        Guard.NotNull(message, nameof(message));
        Guard.NotNull(draft, nameof(draft));
        EnsureReady();

        ApplyDraft(message, draft);
    }

    /// <summary>
    /// Memperbarui pesan yang sudah ada secara asynchronous (in-memory).
    /// </summary>
    /// <param name="message">Pesan yang akan diperbarui.</param>
    /// <param name="draft">Draft data terbaru.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)
    {
        UpdateMessage(message, draft);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Menghapus pesan dari folder secara in-memory.
    /// </summary>
    /// <param name="message">Pesan yang akan dihapus.</param>
    public void DeleteMessage(PstMessage message)
    {
        Guard.NotNull(message, nameof(message));
        EnsureReady();

        if (!_messageFolderIndex.TryGetValue(message.Id, out var folder))
        {
            return;
        }

        var updated = folder.Messages.Where(item => item.Id != message.Id).ToList();
        folder.Messages = updated;
        _messageFolderIndex.Remove(message.Id);
    }

    /// <summary>
    /// Menghapus pesan dari folder secara asynchronous (in-memory).
    /// </summary>
    /// <param name="message">Pesan yang akan dihapus.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)
    {
        DeleteMessage(message);
        return Task.CompletedTask;
    }

    private void AttachFolder(PstFolder? parent, PstFolder folder)
    {
        var targetParent = parent ?? _context?.RootFolder;
        if (targetParent is not null)
        {
            var subfolders = targetParent.SubFolders.ToList();
            subfolders.Add(folder);
            targetParent.SubFolders = subfolders;
        }

        _context?.Folders.Add(folder);
    }

    private void AttachMessage(PstFolder folder, PstMessage message)
    {
        var messages = folder.Messages.ToList();
        messages.Add(message);
        folder.Messages = messages;
        _messageFolderIndex[message.Id] = folder;
    }

    private void ApplyDraft(PstMessage message, PstMessageDraft draft)
    {
        message.Subject = draft.Subject;
        message.Body = draft.Body;
        message.HtmlBody = draft.HtmlBody;
        message.SenderName = draft.FromName;
        message.SenderEmailAddress = draft.FromAddress;
        message.SenderSmtpAddress = draft.FromAddress;
        message.DeliveryTime = draft.SentTime;
        message.InternetMessageId = draft.MessageId;
        message.HasAttachments = draft.Attachments.Count > 0;

        var recipients = draft.Recipients.Select(recipient => new PstRecipient
        {
            RecipientType = (int)recipient.RecipientType,
            EmailAddress = recipient.EmailAddress,
            SmtpAddress = recipient.SmtpAddress ?? recipient.EmailAddress
        }).ToList();
        message.Recipients = recipients;

        message.DisplayTo = JoinDisplay(draft.Recipients, PstRecipientType.To);
        message.DisplayCc = JoinDisplay(draft.Recipients, PstRecipientType.Cc);
        message.DisplayBcc = JoinDisplay(draft.Recipients, PstRecipientType.Bcc);

        var attachments = new List<PstAttachment>();
        var attachNumber = 1;
        foreach (var attachment in draft.Attachments)
        {
            var item = new PstAttachment
            {
                AttachNumber = attachNumber++,
                FileName = attachment.FileName,
                LongFileName = attachment.LongFileName ?? attachment.FileName,
                Size = attachment.ContentBytes?.Length,
                MimeTag = attachment.ContentType,
                ContentId = attachment.ContentId,
                AttachMethod = 1
            };

            if (attachment.ContentBytes is { Length: > 0 })
            {
                var referenceId = (ulong)Interlocked.Increment(ref _attachmentCounter);
                var reference = _attachmentProvider.CreateReference(attachment.ContentBytes, referenceId);
                item.SetContentSource(_attachmentProvider, reference);
            }

            attachments.Add(item);
        }

        message.Attachments = attachments;
    }

    private static string? JoinDisplay(IEnumerable<PstDraftRecipient> recipients, PstRecipientType type)
    {
        var list = recipients
            .Where(recipient => recipient.RecipientType == type)
            .Select(recipient =>
                string.IsNullOrWhiteSpace(recipient.DisplayName)
                    ? recipient.EmailAddress
                    : $"{recipient.DisplayName} <{recipient.EmailAddress}>")
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        return list.Count == 0 ? null : string.Join(", ", list);
    }

    private void EnsureReady()
    {
        if (_context is null)
        {
            throw new InvalidOperationException("Writer belum diinisialisasi dengan konteks PST.");
        }

        EnsureWritableOptions();
    }

    private void EnsureWritableOptions()
    {
        if (_context?.Options.ReadOnly == true)
        {
            throw new NotSupportedException("Writer membutuhkan opsi ReadOnly = false.");
        }
    }

    private string NextMessageId()
    {
        var id = Interlocked.Increment(ref _messageCounter);
        return $"msg-{id}";
    }

    private string NextFolderId()
    {
        var id = Interlocked.Increment(ref _messageCounter);
        return $"folder-{id}";
    }
}
