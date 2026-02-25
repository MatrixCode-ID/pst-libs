
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application;
using Emcode.Pst.Application.Abstractions;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure;
using Emcode.Pst.Infrastructure.Ltp;
using Emcode.Pst.Shared;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Implementasi writer PST berbasis NDB untuk persist ke disk (eksperimental).
/// </summary>
public sealed class PstNdbWriter : IPstWriter, IPstWriterWithContext, IPstFileBootstrapper, IDisposable
{
    private const string BlankPstResourceName = "Emcode.Pst.blank.pst";
    private const uint MessageStoreNidValue = 0x00000021;
    private const uint StoreFolderNidValue = 0x00008022;
    private const ushort PidTagDisplayName = 0x3001;
    private const ushort PidTagComment = 0x3004;
    private const ushort PidTagMessageClass = 0x001A;
    private const ushort PidTagSubject = 0x0037;
    private const ushort PidTagNormalizedSubject = 0x0E1D;
    private const ushort PidTagDeliveryTime = 0x0E06;
    private const ushort PidTagClientSubmitTime = 0x0039;
    private const ushort PidTagLastModificationTime = 0x3008;
    private const ushort PidTagMessageFlags = 0x0E07;
    private const ushort PidTagReadReceiptRequested = 0x0029;
    private const ushort PidTagDeliveryReceiptRequested = 0x0023;
    private const ushort PidTagImportance = 0x0017;
    private const ushort PidTagPriority = 0x0026;
    private const ushort PidTagSensitivity = 0x0036;
    private const ushort PidTagTransportMessageHeaders = 0x007D;
    private const ushort PidTagConversationTopic = 0x0070;
    private const ushort PidTagConversationIndex = 0x0071;
    private const ushort PidTagInternetMessageId = 0x1035;
    private const ushort PidTagSenderName = 0x0C1A;
    private const ushort PidTagSenderEmailAddress = 0x0C1F;
    private const ushort PidTagSenderSmtpAddress = 0x5D01;
    private const ushort PidTagDisplayTo = 0x0E04;
    private const ushort PidTagDisplayCc = 0x0E03;
    private const ushort PidTagDisplayBcc = 0x0E02;
    private const ushort PidTagBody = 0x1000;
    private const ushort PidTagHtml = 0x1013;
    private const ushort PidTagHasAttachments = 0x0E1B;

    private const ushort PidTagRecipientType = 0x0C15;
    private const ushort PidTagEmailAddress = 0x3003;
    private const ushort PidTagAddrType = 0x3002;
    private const ushort PidTagSmtpAddress = 0x39FE;

    private const ushort PidTagAttachNumber = 0x0E21;
    private const ushort PidTagAttachFilename = 0x3704;
    private const ushort PidTagAttachLongFilename = 0x3707;
    private const ushort PidTagAttachSize = 0x0E20;
    private const ushort PidTagAttachMimeTag = 0x370E;
    private const ushort PidTagAttachContentId = 0x3712;
    private const ushort PidTagAttachMethod = 0x3705;
    private const ushort PidTagAttachDataBinary = 0x3701;

    private const int MsgFlagUnmodified = 0x0002;
    private const int MsgFlagUnsent = 0x0008;
    private const int MsgFlagHasAttach = 0x0010;

    private readonly PstEmlParser _parser = new();
    private readonly PstBootstrapBuilder _bootstrapBuilder = new();
    private readonly Dictionary<uint, List<uint>> _tableRowCache = new();
    private LtpWriterOptions _ltpOptions;
    private PstWriteContext? _context;
    private FileStream? _stream;
    private NdbHeader? _header;
    private Dictionary<ulong, BbtEntry>? _existingBbt;
    private Dictionary<uint, NbtEntry>? _existingNbt;
    private NdbWriter? _ndbWriter;
    private NidAllocator? _nidAllocator;
    private PstFolder? _storeFolder;
    private bool _isDisposed;

    /// <summary>
    /// Membuat writer NDB dengan opsi format Unicode default.
    /// </summary>
    public PstNdbWriter()
        : this(PstFormat.Unicode)
    {
    }

    /// <summary>
    /// Membuat writer NDB dengan opsi LTP default.
    /// </summary>
    /// <param name="format">Format PST.</param>
    public PstNdbWriter(PstFormat format)
    {
        _ltpOptions = LtpWriterOptions.CreateDefault(format);
    }

    /// <summary>
    /// Menginisialisasi writer dengan konteks PST.
    /// </summary>
    /// <param name="context">Konteks PST.</param>
    public void Initialize(PstWriteContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _storeFolder = ResolveStoreFolderFromContext(context);
        EnsureWritableOptions();
        EnsureFileInitialized(context.Path, context.Options);
        _stream = new FileStream(context.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        try
        {
            var headerReader = new NdbHeaderReader();
            _header = headerReader.Read(_stream);
            EnsureAmapStateIsWritable(_header);
            _ltpOptions = LtpWriterOptions.CreateDefault(_header.HeaderInfo.Format);

            var btreeReader = new PstBTreeReader(_stream, _header.HeaderInfo.Format);
            _existingBbt = new Dictionary<ulong, BbtEntry>(btreeReader.ReadBbt(_header.BbtRoot));
            _existingNbt = new Dictionary<uint, NbtEntry>(btreeReader.ReadNbt(_header.NbtRoot));

            var maxBidCounter = ResolveMaxBidCounter(_existingBbt.Values, _header.BbtRoot.Bid, _header.NbtRoot.Bid);
            var initialBlockBidCounter = ResolveInitialBidCounter(_header.Counters.NextBlockBidRaw, maxBidCounter);
            var initialPageBidCounter = ResolveInitialBidCounter(_header.Counters.NextPageBidRaw, maxBidCounter);
            _ndbWriter = new NdbWriter(
                _stream,
                _header.HeaderInfo,
                _existingBbt.Values,
                _header.RootState.IbAMapLast,
                initialBlockBidCounter,
                initialPageBidCounter,
                enableFreeSpaceReuse: false);
            _nidAllocator = new NidAllocator(_existingNbt.Values, _header.Counters.NidCounters);
        }
        catch
        {
            _stream?.Dispose();
            _stream = null;
            throw;
        }
    }

    /// <summary>
    /// Menginisialisasi writer dengan konteks PST secara asynchronous.
    /// </summary>
    /// <param name="context">Konteks PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Initialize(context);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Menyiapkan file PST pada path target bila file belum ada.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    public void EnsureFileInitialized(string path, PstOpenOptions options)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        Guard.NotNull(options, nameof(options));

        if (File.Exists(path))
        {
            return;
        }

        if (options.ReadOnly)
        {
            throw new NotSupportedException("Tidak dapat membuat file PST baru saat opsi ReadOnly = true.");
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
        if (!TryWriteEmbeddedBlankBaseline(destination))
        {
            _bootstrapBuilder.Build(destination, PstFormat.Unicode, PstCryptMethod.Permute);
        }

        destination.Flush();
    }

    /// <summary>
    /// Menyiapkan file PST pada path target bila file belum ada secara asynchronous.
    /// </summary>
    /// <param name="path">Path file PST target.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Task representasi proses inisialisasi.</returns>
    public Task EnsureFileInitializedAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        Guard.NotNull(options, nameof(options));

        if (File.Exists(path))
        {
            return Task.CompletedTask;
        }

        if (options.ReadOnly)
        {
            throw new NotSupportedException("Tidak dapat membuat file PST baru saat opsi ReadOnly = true.");
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
        if (!TryWriteEmbeddedBlankBaseline(destination))
        {
            _bootstrapBuilder.BuildAsync(destination, PstFormat.Unicode, PstCryptMethod.Permute, cancellationToken).GetAwaiter().GetResult();
        }

        destination.Flush();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Menyalin baseline PST embedded (`blank.pst`) ke stream target bila resource tersedia.
    /// </summary>
    /// <param name="destination">Stream file tujuan.</param>
    /// <returns>True bila baseline embedded berhasil ditulis; false bila resource tidak ditemukan.</returns>
    private static bool TryWriteEmbeddedBlankBaseline(Stream destination)
    {
        using var source = typeof(PstNdbWriter).Assembly.GetManifestResourceStream(BlankPstResourceName);
        if (source is null)
        {
            return false;
        }

        destination.SetLength(0);
        destination.Seek(0, SeekOrigin.Begin);
        source.CopyTo(destination);
        destination.Seek(0, SeekOrigin.Begin);
        return true;
    }

    /// <summary>
    /// Membuat folder baru pada PST dan menulis node ke disk.
    /// </summary>
    public PstFolder CreateFolder(string name, PstFolder? parent)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        EnsureReady();

        var parentNid = ResolveParentNid(parent);
        var folderNid = _nidAllocator!.Next(NidType.NormalFolder);
        var folderNode = WriteLtpNode(BuildFolderPropertyContext(name));
        var folderEntry = new NbtEntry(folderNid, folderNode.BidData, folderNode.BidSub, parentNid);

        _ndbWriter!.UpsertNbtEntry(folderEntry);
        _existingNbt![folderNid.Value] = folderEntry;

        if (!parentNid.IsZero && _existingNbt.TryGetValue(parentNid.Value, out var parentEntry))
        {
            UpdateHierarchyTable(parentEntry, folderNid);
        }

        var folder = new PstFolder(folderNid.ToString(), name);
        AttachFolder(parent, folder);
        return folder;
    }

    /// <summary>
    /// Membuat folder baru pada PST secara asynchronous.
    /// </summary>
    public Task<PstFolder> CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateFolder(name, parent));
    }

    /// <summary>
    /// Membuat pesan baru pada folder tertentu dan menulis node ke disk.
    /// </summary>
    public PstMessage CreateMessage(PstFolder folder, PstMessageDraft draft)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNull(draft, nameof(draft));
        EnsureReady();

        var parentNid = ResolveParentNid(folder);
        if (parentNid.IsZero)
        {
            throw new InvalidOperationException("Folder parent tidak memiliki NID yang valid.");
        }

        var messageNid = _nidAllocator!.Next(NidType.NormalMessage);
        var messageNode = WriteLtpNodeData(BuildMessagePropertyContext(draft));

        var subnodeBid = BuildMessageSubnodes(draft, messageNode.Subnodes);
        var messageEntry = new NbtEntry(messageNid, messageNode.BidData, subnodeBid, parentNid);
        _ndbWriter!.UpsertNbtEntry(messageEntry);
        _existingNbt![messageNid.Value] = messageEntry;

        if (_existingNbt.TryGetValue(parentNid.Value, out var parentEntry))
        {
            UpdateContentsTable(parentEntry, messageNid);
        }

        var message = new PstMessage(messageNid.ToString());
        ApplyDraft(message, draft);
        AttachMessage(folder, message);
        return message;
    }

    /// <summary>
    /// Membuat pesan baru pada folder tertentu secara asynchronous.
    /// </summary>
    public Task<PstMessage> CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateMessage(folder, draft));
    }

    /// <summary>
    /// Mengimpor file .eml ke folder PST sebagai pesan baru.
    /// </summary>
    public PstMessage ImportEml(PstFolder folder, string emlPath)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        EnsureReady();

        var draft = _parser.Parse(emlPath);
        return CreateMessage(folder, draft);
    }

    /// <summary>
    /// Mengimpor file .eml ke folder PST sebagai pesan baru secara asynchronous.
    /// </summary>
    public async Task<PstMessage> ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(folder, nameof(folder));
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        EnsureReady();

        var draft = await _parser.ParseAsync(emlPath, cancellationToken).ConfigureAwait(false);
        return CreateMessage(folder, draft);
    }

    /// <summary>
    /// Memperbarui properti store PST (nama dan komentar data file).
    /// </summary>
    /// <param name="draft">Draft properti store.</param>
    public void UpdateStoreProperties(PstStorePropertiesDraft draft)
    {
        Guard.NotNull(draft, nameof(draft));
        EnsureReady();
        EnsureStoreDraftHasChanges(draft);

        var targetFolder = _storeFolder ?? ResolveStoreFolderFromContext(_context!);
        if (targetFolder is null)
        {
            throw new InvalidOperationException("Folder store PST tidak ditemukan untuk pembaruan properti.");
        }

        var targetNid = ParseNid(targetFolder.Id);
        if (targetNid.IsZero || _existingNbt is null || !_existingNbt.TryGetValue(targetNid.Value, out var existingEntry))
        {
            throw new InvalidOperationException("NID folder store PST tidak valid untuk pembaruan properti.");
        }

        var name = string.IsNullOrWhiteSpace(draft.DisplayName) ? targetFolder.Name : draft.DisplayName!;
        var currentDescription = targetFolder.Description ?? targetFolder.Comment;
        var description = draft.Description ?? currentDescription;
        var comment = draft.Comment ?? targetFolder.Comment;

        if (description is null && draft.Comment is not null)
        {
            // Backward compatibility: caller lama yang hanya set Comment tetap mengisi description folder seperti perilaku sebelumnya.
            description = draft.Comment;
        }

        var folderNode = WriteLtpNode(BuildStorePropertyContext(name, description));
        var updatedEntry = new NbtEntry(existingEntry.Nid, folderNode.BidData, folderNode.BidSub, existingEntry.NidParent);
        _ndbWriter!.UpsertNbtEntry(updatedEntry);
        _existingNbt[targetNid.Value] = updatedEntry;

        targetFolder.Name = name;
        if (description is not null)
        {
            targetFolder.Description = description;
        }

        if (draft.Comment is not null)
        {
            targetFolder.Comment = draft.Comment;
        }

        if (_existingNbt.TryGetValue(MessageStoreNidValue, out var messageStoreEntry))
        {
            var storeNode = WriteLtpNode(BuildStorePropertyContext(name, comment));
            var updatedStoreEntry = new NbtEntry(
                messageStoreEntry.Nid,
                storeNode.BidData,
                storeNode.BidSub,
                messageStoreEntry.NidParent);
            _ndbWriter.UpsertNbtEntry(updatedStoreEntry);
            _existingNbt[MessageStoreNidValue] = updatedStoreEntry;
        }
    }

    /// <summary>
    /// Memperbarui properti store PST secara asynchronous.
    /// </summary>
    /// <param name="draft">Draft properti store.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Task representasi operasi update.</returns>
    public Task UpdateStorePropertiesAsync(PstStorePropertiesDraft draft, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UpdateStoreProperties(draft);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Menyimpan perubahan write yang tertunda ke file PST.
    /// </summary>
    public void Save()
    {
        EnsureReady();
        CommitPendingChanges();
        _stream!.Flush();
    }

    /// <summary>
    /// Menyimpan perubahan write yang tertunda ke file PST secara asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Save();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Memperbarui pesan yang sudah ada (belum didukung).
    /// </summary>
    public void UpdateMessage(PstMessage message, PstMessageDraft draft)
    {
        throw new NotSupportedException("Update message pada PstNdbWriter belum didukung.");
    }

    /// <summary>
    /// Memperbarui pesan yang sudah ada secara asynchronous (belum didukung).
    /// </summary>
    public Task UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromException(new NotSupportedException("Update message pada PstNdbWriter belum didukung."));
    }

    /// <summary>
    /// Menghapus pesan dari PST (belum didukung).
    /// </summary>
    public void DeleteMessage(PstMessage message)
    {
        throw new NotSupportedException("Delete message pada PstNdbWriter belum didukung.");
    }

    /// <summary>
    /// Menghapus pesan dari PST secara asynchronous (belum didukung).
    /// </summary>
    public Task DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromException(new NotSupportedException("Delete message pada PstNdbWriter belum didukung."));
    }

    /// <summary>
    /// Memastikan draft update store memiliki perubahan yang valid.
    /// </summary>
    /// <param name="draft">Draft properti store.</param>
    private static void EnsureStoreDraftHasChanges(PstStorePropertiesDraft draft)
    {
        if (!string.IsNullOrWhiteSpace(draft.DisplayName) || draft.Description is not null || draft.Comment is not null)
        {
            return;
        }

        throw new ArgumentException("DisplayName, Description, atau Comment harus diisi untuk update store.", nameof(draft));
    }

    /// <summary>
    /// Menentukan folder store utama dari konteks pembacaan PST.
    /// </summary>
    /// <param name="context">Konteks write PST.</param>
    /// <returns>Folder store kandidat atau null.</returns>
    private static PstFolder? ResolveStoreFolderFromContext(PstWriteContext context)
    {
        var storeFolderId = $"0x{StoreFolderNidValue:X8}";
        var byFixedNid = context.Folders.FirstOrDefault(folder => string.Equals(folder.Id, storeFolderId, StringComparison.OrdinalIgnoreCase));
        if (byFixedNid is not null)
        {
            return byFixedNid;
        }

        var top = context.Folders.FirstOrDefault(
            folder => string.Equals(folder.Name, "Top of Outlook data file", StringComparison.OrdinalIgnoreCase));
        if (top is not null)
        {
            return top;
        }

        if (context.RootFolder is not null && context.RootFolder.SubFolders.Count > 0)
        {
            var fromRoot = context.RootFolder.SubFolders.FirstOrDefault(
                folder => string.Equals(folder.Id, storeFolderId, StringComparison.OrdinalIgnoreCase));
            if (fromRoot is not null)
            {
                return fromRoot;
            }
        }

        return context.Folders.FirstOrDefault(folder => !string.Equals(folder.Id, "root", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Melepas resource stream dan melakukan commit BBT/NBT.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        CommitPendingChanges();

        _stream?.Dispose();
        _stream = null;
        _isDisposed = true;
    }

    /// <summary>
    /// Melakukan commit BBT/NBT bila state writer sudah siap.
    /// </summary>
    private void CommitPendingChanges()
    {
        if (_stream is null || _header is null || _existingBbt is null || _existingNbt is null || _ndbWriter is null)
        {
            return;
        }

        _ndbWriter.CommitBtrees(_header, _existingBbt, _existingNbt, _nidAllocator?.SnapshotNextNids());
    }

    /// <summary>
    /// Menghitung nilai counter BID maksimum dari entri BBT.
    /// </summary>
    /// <param name="entries">Entri BBT.</param>
    /// <returns>Nilai counter BID maksimum.</returns>
    private static ulong ResolveMaxBidCounter(IEnumerable<BbtEntry> entries, params Bid[] additionalBids)
    {
        var max = 0UL;
        foreach (var entry in entries)
        {
            var counter = entry.Bid.Raw >> 2;
            if (counter > max)
            {
                max = counter;
            }
        }

        foreach (var bid in additionalBids)
        {
            var counter = bid.Raw >> 2;
            if (counter > max)
            {
                max = counter;
            }
        }

        return max;
    }

    /// <summary>
    /// Menentukan nilai counter BID awal dari header.bidNext* dengan fallback ke hasil scan BBT.
    /// </summary>
    /// <param name="nextBidRaw">Nilai raw bidNext dari header.</param>
    /// <param name="fallbackCounter">Counter fallback dari scan BBT.</param>
    /// <returns>Counter BID awal yang aman untuk writer runtime.</returns>
    private static ulong ResolveInitialBidCounter(ulong nextBidRaw, ulong fallbackCounter)
    {
        if (nextBidRaw < 4)
        {
            return fallbackCounter;
        }

        var headerCounter = (nextBidRaw >> 2) - 1;
        return headerCounter > fallbackCounter ? headerCounter : fallbackCounter;
    }


    /// <summary>
    /// Memastikan file tidak dalam state AMap invalid sebelum operasi write.
    /// </summary>
    /// <param name="header">Metadata header PST.</param>
    private static void EnsureAmapStateIsWritable(NdbHeader header)
    {
        if (header.RootState.IsAMapValid)
        {
            return;
        }

        throw new InvalidOperationException(
            "HEADER.ROOT.fAMapValid bernilai invalid. Recovery AMap belum didukung, operasi write dibatalkan.");
    }

    /// <summary>
    /// Membangun Property Context untuk folder baru.
    /// </summary>
    /// <param name="name">Nama folder.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildStorePropertyContext(string name, string? comment = null)
    {
        var ltp = new LtpWriter(_ltpOptions);
        var pc = ltp.CreatePropertyContextWriter();
        pc.SetString(PidTagDisplayName, name);
        if (comment is not null)
        {
            pc.SetString(PidTagComment, comment);
        }

        return pc.BuildResult();
    }

    /// <summary>
    /// Membangun Property Context untuk folder biasa.
    /// </summary>
    /// <param name="name">Nama folder.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildFolderPropertyContext(string name)
    {
        return BuildStorePropertyContext(name);
    }

    /// <summary>
    /// Membangun Property Context untuk message berdasarkan draft.
    /// </summary>
    /// <param name="draft">Draft message.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildMessagePropertyContext(PstMessageDraft draft)
    {
        var ltp = new LtpWriter(_ltpOptions);
        var pc = ltp.CreatePropertyContextWriter();
        var now = DateTimeOffset.UtcNow;

        var messageClass = string.IsNullOrWhiteSpace(draft.MessageClass) ? "IPM.Note" : draft.MessageClass;
        pc.SetString(PidTagMessageClass, messageClass);

        if (!string.IsNullOrWhiteSpace(draft.Subject))
        {
            pc.SetString(PidTagSubject, draft.Subject);
            pc.SetString(PidTagNormalizedSubject, draft.Subject);
        }

        if (!string.IsNullOrWhiteSpace(draft.Body))
        {
            pc.SetString(PidTagBody, draft.Body);
        }

        if (!string.IsNullOrWhiteSpace(draft.HtmlBody))
        {
            var htmlBytes = Encoding.UTF8.GetBytes(draft.HtmlBody);
            pc.SetBinary(PidTagHtml, htmlBytes);
        }

        if (!string.IsNullOrWhiteSpace(draft.FromName))
        {
            pc.SetString(PidTagSenderName, draft.FromName);
        }

        if (!string.IsNullOrWhiteSpace(draft.FromAddress))
        {
            pc.SetString(PidTagSenderEmailAddress, draft.FromAddress);
            pc.SetString(PidTagSenderSmtpAddress, draft.FromAddress);
        }

        if (!string.IsNullOrWhiteSpace(draft.MessageId))
        {
            pc.SetString(PidTagInternetMessageId, draft.MessageId);
        }

        if (draft.SentTime.HasValue)
        {
            pc.SetDateTime(PidTagDeliveryTime, draft.SentTime.Value);
        }

        pc.SetDateTime(PidTagClientSubmitTime, draft.ClientSubmitTime ?? draft.SentTime ?? now);
        pc.SetDateTime(PidTagLastModificationTime, draft.LastModificationTime ?? now);

        if (!string.IsNullOrWhiteSpace(draft.TransportMessageHeaders))
        {
            pc.SetString(PidTagTransportMessageHeaders, draft.TransportMessageHeaders);
        }

        var conversationTopic = draft.ConversationTopic ?? draft.Subject;
        if (!string.IsNullOrWhiteSpace(conversationTopic))
        {
            pc.SetString(PidTagConversationTopic, conversationTopic);
        }

        if (draft.ConversationIndex is { Length: > 0 })
        {
            pc.SetBinary(PidTagConversationIndex, draft.ConversationIndex);
        }

        var displayTo = JoinDisplay(draft.Recipients, PstRecipientType.To);
        if (!string.IsNullOrWhiteSpace(displayTo))
        {
            pc.SetString(PidTagDisplayTo, displayTo);
        }

        var displayCc = JoinDisplay(draft.Recipients, PstRecipientType.Cc);
        if (!string.IsNullOrWhiteSpace(displayCc))
        {
            pc.SetString(PidTagDisplayCc, displayCc);
        }

        var displayBcc = JoinDisplay(draft.Recipients, PstRecipientType.Bcc);
        if (!string.IsNullOrWhiteSpace(displayBcc))
        {
            pc.SetString(PidTagDisplayBcc, displayBcc);
        }

        var hasAttachments = draft.Attachments.Count > 0;
        pc.SetBoolean(PidTagHasAttachments, hasAttachments);
        if (draft.ReadReceiptRequested.HasValue)
        {
            pc.SetBoolean(PidTagReadReceiptRequested, draft.ReadReceiptRequested.Value);
        }

        if (draft.DeliveryReceiptRequested.HasValue)
        {
            pc.SetBoolean(PidTagDeliveryReceiptRequested, draft.DeliveryReceiptRequested.Value);
        }

        if (draft.Importance.HasValue)
        {
            pc.SetInt32(PidTagImportance, draft.Importance.Value);
        }

        if (draft.Priority.HasValue)
        {
            pc.SetInt32(PidTagPriority, draft.Priority.Value);
        }

        if (draft.Sensitivity.HasValue)
        {
            pc.SetInt32(PidTagSensitivity, draft.Sensitivity.Value);
        }

        pc.SetInt32(PidTagMessageFlags, ResolveMessageFlags(draft, hasAttachments));
        return pc.BuildResult();
    }
    /// <summary>
    /// Membuat subnode tree untuk recipient, attachment table, dan attachment data.
    /// </summary>
    /// <param name="draft">Draft message.</param>
    /// <returns>BID subnode atau BID nol jika tidak ada subnode.</returns>
    private Bid BuildMessageSubnodes(PstMessageDraft draft, IReadOnlyList<LtpSubnodeData> initialSubnodes)
    {
        var subnodes = new List<SubnodeEntry>();
        if (initialSubnodes.Count > 0)
        {
            foreach (var subnode in initialSubnodes)
            {
                var bidData = _ndbWriter!.WriteDataTree(subnode.Data);
                subnodes.Add(new SubnodeEntry(subnode.LocalNid, bidData, new Bid(0)));
            }
        }

        var nextLocalIndex = ResolveNextLocalIndex(subnodes);

        if (draft.Recipients.Count > 0)
        {
            var recipientNode = WriteLtpNode(BuildRecipientTable(draft.Recipients));
            var nid = CreateLocalNid(nextLocalIndex++, NidType.Ltp);
            subnodes.Add(new SubnodeEntry(nid, recipientNode.BidData, recipientNode.BidSub));
        }

        if (draft.Attachments.Count > 0)
        {
            var attachmentTableNode = WriteLtpNode(BuildAttachmentTable(draft.Attachments));
            var nid = CreateLocalNid(nextLocalIndex++, NidType.Ltp);
            subnodes.Add(new SubnodeEntry(nid, attachmentTableNode.BidData, attachmentTableNode.BidSub));

            var attachNumber = 1;
            foreach (var attachment in draft.Attachments)
            {
                if (attachment.ContentBytes is { Length: > 0 })
                {
                    var attachNode = WriteLtpNode(BuildAttachmentPropertyContext(attachment));
                    var attachNid = CreateLocalNid((uint)attachNumber, NidType.Attachment);
                    subnodes.Add(new SubnodeEntry(attachNid, attachNode.BidData, attachNode.BidSub));
                }
                attachNumber++;
            }
        }

        if (subnodes.Count == 0)
        {
            return new Bid(0);
        }

        return WriteSubnodeTree(subnodes);
    }

    /// <summary>
    /// Membuat Property Context attachment berisi data biner attachment.
    /// </summary>
    /// <param name="attachment">Draft attachment.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildAttachmentPropertyContext(PstDraftAttachment attachment)
    {
        if (attachment.ContentBytes is null || attachment.ContentBytes.Length == 0)
        {
            throw new InvalidOperationException("Konten attachment kosong tidak dapat ditulis.");
        }

        var ltp = new LtpWriter(_ltpOptions);
        var pc = ltp.CreatePropertyContextWriter();
        pc.SetBinary(PidTagAttachDataBinary, attachment.ContentBytes);
        return pc.BuildResult();
    }

    /// <summary>
    /// Membangun table recipient berdasarkan draft.
    /// </summary>
    /// <param name="recipients">Daftar penerima.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildRecipientTable(IReadOnlyList<PstDraftRecipient> recipients)
    {
        var ltp = new LtpWriter(_ltpOptions);
        var table = ltp.CreateTableRowWriter();
        table.AddColumn(PidTagRecipientType, PstPropertyType.Integer32, 4, 4, 0);
        table.AddColumn(PidTagEmailAddress, PstPropertyType.String, 8, 4, 1);
        table.AddColumn(PidTagSmtpAddress, PstPropertyType.String, 12, 4, 2);
        table.AddColumn(PidTagDisplayName, PstPropertyType.String, 16, 4, 3);
        table.AddColumn(PidTagAddrType, PstPropertyType.String, 20, 4, 4);

        var rowId = 1U;
        foreach (var recipient in recipients)
        {
            var cells = new List<TableRowWriter.TableCell>
            {
                new(PidTagRecipientType, PstPropertyType.Integer32, (int)recipient.RecipientType)
            };

            if (!string.IsNullOrWhiteSpace(recipient.EmailAddress))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagEmailAddress, PstPropertyType.String, recipient.EmailAddress));
            }

            var smtp = recipient.SmtpAddress ?? recipient.EmailAddress;
            if (!string.IsNullOrWhiteSpace(smtp))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagSmtpAddress, PstPropertyType.String, smtp));
            }

            if (!string.IsNullOrWhiteSpace(recipient.DisplayName))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagDisplayName, PstPropertyType.String, recipient.DisplayName));
            }

            if (!string.IsNullOrWhiteSpace(recipient.EmailAddress))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagAddrType, PstPropertyType.String, "SMTP"));
            }

            table.AddRow(rowId++, cells.ToArray());
        }

        return table.BuildResult();
    }

    /// <summary>
    /// Membangun table attachment berdasarkan draft.
    /// </summary>
    /// <param name="attachments">Daftar attachment.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildAttachmentTable(IReadOnlyList<PstDraftAttachment> attachments)
    {
        var ltp = new LtpWriter(_ltpOptions);
        var table = ltp.CreateTableRowWriter();
        table.AddColumn(PidTagAttachNumber, PstPropertyType.Integer32, 4, 4, 0);
        table.AddColumn(PidTagAttachFilename, PstPropertyType.String, 8, 4, 1);
        table.AddColumn(PidTagAttachLongFilename, PstPropertyType.String, 12, 4, 2);
        table.AddColumn(PidTagAttachSize, PstPropertyType.Integer32, 16, 4, 3);
        table.AddColumn(PidTagAttachMimeTag, PstPropertyType.String, 20, 4, 4);
        table.AddColumn(PidTagAttachContentId, PstPropertyType.String, 24, 4, 5);
        table.AddColumn(PidTagAttachMethod, PstPropertyType.Integer32, 28, 4, 6);

        var attachNumber = 1;
        foreach (var attachment in attachments)
        {
            var cells = new List<TableRowWriter.TableCell>
            {
                new(PidTagAttachNumber, PstPropertyType.Integer32, attachNumber)
            };

            var fileName = attachment.FileName;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagAttachFilename, PstPropertyType.String, fileName));
            }

            var longName = attachment.LongFileName ?? fileName;
            if (!string.IsNullOrWhiteSpace(longName))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagAttachLongFilename, PstPropertyType.String, longName));
            }

            if (attachment.ContentBytes is { Length: > 0 })
            {
                cells.Add(new TableRowWriter.TableCell(PidTagAttachSize, PstPropertyType.Integer32, attachment.ContentBytes.Length));
            }

            if (!string.IsNullOrWhiteSpace(attachment.ContentType))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagAttachMimeTag, PstPropertyType.String, attachment.ContentType));
            }

            if (!string.IsNullOrWhiteSpace(attachment.ContentId))
            {
                cells.Add(new TableRowWriter.TableCell(PidTagAttachContentId, PstPropertyType.String, attachment.ContentId));
            }

            cells.Add(new TableRowWriter.TableCell(PidTagAttachMethod, PstPropertyType.Integer32, 1));
            table.AddRow((uint)attachNumber, cells.ToArray());
            attachNumber++;
        }

        return table.BuildResult();
    }

    /// <summary>
    /// Menulis node LTP (PC/TC) dan mengembalikan BID data serta daftar subnode.
    /// </summary>
    /// <param name="result">Hasil penulisan LTP.</param>
    /// <returns>Hasil penulisan node.</returns>
    private LtpNodeWriteResult WriteLtpNodeData(LtpWriteResult result)
    {
        if (result.Blocks.Count == 0)
        {
            throw new InvalidOperationException("Blok data LTP kosong.");
        }

        var data = ConcatBlocks(result.Blocks);
        var bidData = _ndbWriter!.WriteDataTree(data);
        return new LtpNodeWriteResult(bidData, result.Subnodes);
    }

    /// <summary>
    /// Menulis node LTP lengkap (termasuk subnode) dan mengembalikan BID data/subnode.
    /// </summary>
    /// <param name="result">Hasil penulisan LTP.</param>
    /// <returns>Informasi node.</returns>
    private LtpNodeWriteInfo WriteLtpNode(LtpWriteResult result)
    {
        var node = WriteLtpNodeData(result);
        var bidSub = node.Subnodes.Count == 0 ? new Bid(0) : WriteSubnodeTree(node.Subnodes);
        return new LtpNodeWriteInfo(node.BidData, bidSub);
    }

    /// <summary>
    /// Menulis subnode tree dari daftar entry.
    /// </summary>
    /// <param name="entries">Daftar subnode.</param>
    /// <returns>BID subnode root.</returns>
    private Bid WriteSubnodeTree(IReadOnlyList<SubnodeEntry> entries)
    {
        var block = BuildSubnodeBlock(entries, _header!.HeaderInfo.Format);
        var allocation = _ndbWriter!.WriteInternalBlock(block);
        return allocation.Bid;
    }

    /// <summary>
    /// Menulis subnode tree dari hasil LTP subnode.
    /// </summary>
    /// <param name="subnodes">Daftar subnode LTP.</param>
    /// <returns>BID subnode root.</returns>
    private Bid WriteSubnodeTree(IReadOnlyList<LtpSubnodeData> subnodes)
    {
        if (subnodes.Count == 0)
        {
            return new Bid(0);
        }

        var entries = new List<SubnodeEntry>(subnodes.Count);
        foreach (var subnode in subnodes)
        {
            var bidData = _ndbWriter!.WriteDataTree(subnode.Data);
            entries.Add(new SubnodeEntry(subnode.LocalNid, bidData, new Bid(0)));
        }

        return WriteSubnodeTree(entries);
    }

    /// <summary>
    /// Menggabungkan blok data LTP menjadi satu buffer kontigu.
    /// </summary>
    /// <param name="blocks">Blok data LTP.</param>
    /// <returns>Buffer gabungan.</returns>
    private static ReadOnlyMemory<byte> ConcatBlocks(IReadOnlyList<PstDataBlock> blocks)
    {
        var total = 0;
        foreach (var block in blocks)
        {
            total += block.Data.Length;
        }

        var buffer = new byte[total];
        var offset = 0;
        foreach (var block in blocks)
        {
            block.Data.CopyTo(buffer.AsMemory(offset, block.Data.Length));
            offset += block.Data.Length;
        }

        return buffer;
    }

    /// <summary>
    /// Menghitung index lokal berikutnya untuk NID tipe LTP.
    /// </summary>
    /// <param name="entries">Daftar subnode existing.</param>
    /// <returns>Index berikutnya.</returns>
    private static uint ResolveNextLocalIndex(IReadOnlyList<SubnodeEntry> entries)
    {
        var max = 0U;
        foreach (var entry in entries)
        {
            if (entry.Nid.Type != NidType.Ltp)
            {
                continue;
            }

            if (entry.Nid.Index > max)
            {
                max = entry.Nid.Index;
            }
        }

        return max + 1;
    }

    /// <summary>
    /// Memperbarui hierarchy table parent agar memasukkan child baru.
    /// </summary>
    /// <param name="parentEntry">Entri parent folder.</param>
    /// <param name="childNid">NID child folder.</param>
    private void UpdateHierarchyTable(NbtEntry parentEntry, Nid childNid)
    {
        var tableNid = CreateDerivedNid(parentEntry.Nid.Index, NidType.HierarchyTable);
        var rowIds = GetTableRowIds(tableNid);
        if (!rowIds.Contains(childNid.Value))
        {
            rowIds.Add(childNid.Value);
        }

        var tableNode = WriteLtpNode(BuildRowIdTable(rowIds));
        var entry = new NbtEntry(tableNid, tableNode.BidData, tableNode.BidSub, parentEntry.Nid);
        _ndbWriter!.UpsertNbtEntry(entry);
        _existingNbt![tableNid.Value] = entry;
        _tableRowCache[tableNid.Value] = rowIds;
    }

    /// <summary>
    /// Memperbarui contents table folder agar memasukkan message baru.
    /// </summary>
    /// <param name="parentEntry">Entri folder.</param>
    /// <param name="messageNid">NID message.</param>
    private void UpdateContentsTable(NbtEntry parentEntry, Nid messageNid)
    {
        var tableNid = CreateDerivedNid(parentEntry.Nid.Index, NidType.ContentsTable);
        var rowIds = GetTableRowIds(tableNid);
        if (!rowIds.Contains(messageNid.Value))
        {
            rowIds.Add(messageNid.Value);
        }

        var tableNode = WriteLtpNode(BuildRowIdTable(rowIds));
        var entry = new NbtEntry(tableNid, tableNode.BidData, tableNode.BidSub, parentEntry.Nid);
        _ndbWriter!.UpsertNbtEntry(entry);
        _existingNbt![tableNid.Value] = entry;
        _tableRowCache[tableNid.Value] = rowIds;
    }

    /// <summary>
    /// Membangun table sederhana yang hanya menyimpan row ID.
    /// </summary>
    /// <param name="rowIds">Daftar row ID.</param>
    /// <returns>Hasil penulisan LTP.</returns>
    private LtpWriteResult BuildRowIdTable(IReadOnlyList<uint> rowIds)
    {
        var ltp = new LtpWriter(_ltpOptions);
        var table = ltp.CreateTableRowWriter();
        table.AddColumn(PidTagDisplayName, PstPropertyType.Integer32, 4, 4, 0);

        foreach (var rowId in rowIds)
        {
            table.AddRow(rowId);
        }

        return table.BuildResult();
    }
    /// <summary>
    /// Mengambil daftar row ID table dari cache atau hasil baca file.
    /// </summary>
    /// <param name="tableNid">NID table.</param>
    /// <returns>Daftar row ID.</returns>
    private List<uint> GetTableRowIds(Nid tableNid)
    {
        if (_tableRowCache.TryGetValue(tableNid.Value, out var cached))
        {
            return cached;
        }

        if (_existingNbt is null || !_existingNbt.TryGetValue(tableNid.Value, out var entry))
        {
            var empty = new List<uint>();
            _tableRowCache[tableNid.Value] = empty;
            return empty;
        }

        var rowIds = ReadTableRowIds(entry).ToList();
        _tableRowCache[tableNid.Value] = rowIds;
        return rowIds;
    }

    /// <summary>
    /// Membaca row ID dari table yang sudah ada di file.
    /// </summary>
    /// <param name="entry">Entri NBT table.</param>
    /// <returns>Daftar row ID.</returns>
    private IReadOnlyList<uint> ReadTableRowIds(NbtEntry entry)
    {
        var blockReader = new PstBlockReader(_stream!, _header!.HeaderInfo.Format, _header.HeaderInfo.CryptMethod, _existingBbt!);
        var tableBlocks = blockReader.ReadDataBlocks(entry.BidData);
        if (tableBlocks.Count == 0)
        {
            return Array.Empty<uint>();
        }

        var tableHeap = new HeapOnNode(tableBlocks);
        var tableSubnodes = new SubnodeReader(blockReader, _header.HeaderInfo.Format, entry.BidSub);
        var tableContext = new TableContext(tableHeap, tableSubnodes);
        return tableContext.ReadRowIds();
    }

    /// <summary>
    /// Menempelkan folder baru ke parent di konteks write.
    /// </summary>
    /// <param name="parent">Folder parent.</param>
    /// <param name="folder">Folder baru.</param>
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

    /// <summary>
    /// Menempelkan message baru ke folder target.
    /// </summary>
    /// <param name="folder">Folder target.</param>
    /// <param name="message">Message baru.</param>
    private static void AttachMessage(PstFolder folder, PstMessage message)
    {
        var messages = folder.Messages.ToList();
        messages.Add(message);
        folder.Messages = messages;
    }

    /// <summary>
    /// Mengisi object message dari draft.
    /// </summary>
    /// <param name="message">Message target.</param>
    /// <param name="draft">Draft message.</param>
    private static void ApplyDraft(PstMessage message, PstMessageDraft draft)
    {
        message.MessageClass = string.IsNullOrWhiteSpace(draft.MessageClass) ? "IPM.Note" : draft.MessageClass;
        message.Subject = draft.Subject;
        message.Body = draft.Body;
        message.HtmlBody = draft.HtmlBody;
        message.SenderName = draft.FromName;
        message.SenderEmailAddress = draft.FromAddress;
        message.SenderSmtpAddress = draft.FromAddress;
        message.DeliveryTime = draft.SentTime;
        message.ClientSubmitTime = draft.ClientSubmitTime ?? draft.SentTime;
        message.LastModificationTime = draft.LastModificationTime;
        message.InternetMessageId = draft.MessageId;
        message.HasAttachments = draft.Attachments.Count > 0;
        message.ReadReceiptRequested = draft.ReadReceiptRequested;
        message.DeliveryReceiptRequested = draft.DeliveryReceiptRequested;
        message.Importance = draft.Importance;
        message.Priority = draft.Priority;
        message.Sensitivity = draft.Sensitivity;
        message.TransportMessageHeaders = draft.TransportMessageHeaders;
        message.ConversationTopic = draft.ConversationTopic ?? draft.Subject;
        message.ConversationIndex = draft.ConversationIndex;
        message.MessageFlags = ResolveMessageFlags(draft, draft.Attachments.Count > 0);

        var recipients = draft.Recipients.Select(recipient => new PstRecipient
        {
            RecipientType = (int)recipient.RecipientType,
            DisplayName = recipient.DisplayName,
            EmailAddress = recipient.EmailAddress,
            AddressType = string.IsNullOrWhiteSpace(recipient.EmailAddress) ? null : "SMTP",
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

            attachments.Add(item);
        }

        message.Attachments = attachments;
    }

    /// <summary>
    /// Menggabungkan display name untuk field To/Cc/Bcc.
    /// </summary>
    /// <param name="recipients">Daftar recipient.</param>
    /// <param name="type">Jenis recipient.</param>
    /// <returns>String display gabungan.</returns>
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

    /// <summary>
    /// Menentukan nilai message flags MAPI berdasarkan draft.
    /// </summary>
    /// <param name="draft">Draft message.</param>
    /// <param name="hasAttachments">Status attachment.</param>
    /// <returns>Bitmask message flags.</returns>
    private static int ResolveMessageFlags(PstMessageDraft draft, bool hasAttachments)
    {
        if (draft.MessageFlags.HasValue)
        {
            return draft.MessageFlags.Value;
        }

        var flags = MsgFlagUnmodified;
        if (draft.IsDraft)
        {
            flags |= MsgFlagUnsent;
        }

        if (hasAttachments)
        {
            flags |= MsgFlagHasAttach;
        }

        return flags;
    }

    /// <summary>
    /// Menyusun block SLBLOCK untuk subnode.
    /// </summary>
    /// <param name="entries">Daftar entry subnode.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Buffer SLBLOCK.</returns>
    private static byte[] BuildSubnodeBlock(IReadOnlyList<SubnodeEntry> entries, PstFormat format)
    {
        var entrySize = format == PstFormat.Unicode ? 24 : 12;
        var headerSize = format == PstFormat.Unicode ? 8 : 4;
        var length = headerSize + (entries.Count * entrySize);
        var buffer = new byte[length];

        buffer[0] = 0x02;
        buffer[1] = 0x00;
        BitConverter.TryWriteBytes(buffer.AsSpan(2, 2), (ushort)entries.Count);

        var offset = headerSize;
        foreach (var entry in entries)
        {
            if (format == PstFormat.Unicode)
            {
                BitConverter.TryWriteBytes(buffer.AsSpan(offset, 8), entry.Nid.Value);
                BitConverter.TryWriteBytes(buffer.AsSpan(offset + 8, 8), entry.BidData.Raw);
                BitConverter.TryWriteBytes(buffer.AsSpan(offset + 16, 8), entry.BidSub.Raw);
            }
            else
            {
                BitConverter.TryWriteBytes(buffer.AsSpan(offset, 4), entry.Nid.Value);
                BitConverter.TryWriteBytes(buffer.AsSpan(offset + 4, 4), (uint)entry.BidData.Raw);
                BitConverter.TryWriteBytes(buffer.AsSpan(offset + 8, 4), (uint)entry.BidSub.Raw);
            }

            offset += entrySize;
        }

        return buffer;
    }

    /// <summary>
    /// Membuat NID lokal berdasarkan index dan tipe.
    /// </summary>
    /// <param name="index">Index lokal.</param>
    /// <param name="type">Tipe NID.</param>
    /// <returns>NID lokal.</returns>
    private static Nid CreateLocalNid(uint index, NidType type)
    {
        var value = (index << 5) | (uint)type;
        return new Nid(value);
    }

    /// <summary>
    /// Membuat NID turunan untuk table berdasarkan indeks folder.
    /// </summary>
    /// <param name="index">Index folder.</param>
    /// <param name="type">Jenis table.</param>
    /// <returns>NID turunan.</returns>
    private static Nid CreateDerivedNid(uint index, NidType type)
    {
        return new Nid((index << 5) | (uint)type);
    }

    /// <summary>
    /// Mengambil NID parent dari folder (atau NID nol bila root).
    /// </summary>
    /// <param name="folder">Folder target.</param>
    /// <returns>NID parent.</returns>
    private static Nid ResolveParentNid(PstFolder? folder)
    {
        if (folder is null || string.Equals(folder.Id, "root", StringComparison.OrdinalIgnoreCase))
        {
            return new Nid(0);
        }

        return ParseNid(folder.Id);
    }

    /// <summary>
    /// Mengurai string NID hex menjadi struct Nid.
    /// </summary>
    /// <param name="value">String NID.</param>
    /// <returns>NID hasil parse.</returns>
    private static Nid ParseNid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new Nid(0);
        }

        var raw = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? value[2..]
            : value;
        if (!uint.TryParse(raw, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        {
            return new Nid(0);
        }

        return new Nid(parsed);
    }

    /// <summary>
    /// Memastikan writer siap digunakan.
    /// </summary>
    private void EnsureReady()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(PstNdbWriter));
        }

        if (_context is null || _ndbWriter is null || _header is null || _existingBbt is null || _existingNbt is null || _nidAllocator is null)
        {
            throw new InvalidOperationException("Writer belum diinisialisasi dengan konteks PST.");
        }

        EnsureWritableOptions();
    }

    /// <summary>
    /// Memastikan writer berjalan dalam mode write.
    /// </summary>
    private void EnsureWritableOptions()
    {
        if (_context?.Options.ReadOnly == true)
        {
            throw new NotSupportedException("Writer membutuhkan opsi ReadOnly = false.");
        }
    }

    /// <summary>
    /// Representasi entry subnode sebelum diserialisasi ke SLBLOCK.
    /// </summary>
    private readonly struct SubnodeEntry
    {
        /// <summary>
        /// Membuat entry subnode.
        /// </summary>
        /// <param name="nid">NID subnode lokal.</param>
        /// <param name="bidData">BID data subnode.</param>
        /// <param name="bidSub">BID subnode dari subnode.</param>
        public SubnodeEntry(Nid nid, Bid bidData, Bid bidSub)
        {
            Nid = nid;
            BidData = bidData;
            BidSub = bidSub;
        }

        /// <summary>
        /// NID subnode lokal.
        /// </summary>
        public Nid Nid { get; }

        /// <summary>
        /// BID data subnode.
        /// </summary>
        public Bid BidData { get; }

        /// <summary>
        /// BID subnode dari subnode.
        /// </summary>
        public Bid BidSub { get; }
    }

    /// <summary>
    /// Hasil penulisan node LTP yang berisi BID data dan daftar subnode.
    /// </summary>
    private readonly struct LtpNodeWriteResult
    {
        /// <summary>
        /// Membuat hasil penulisan node LTP.
        /// </summary>
        /// <param name="bidData">BID data node.</param>
        /// <param name="subnodes">Daftar subnode.</param>
        public LtpNodeWriteResult(Bid bidData, IReadOnlyList<LtpSubnodeData> subnodes)
        {
            BidData = bidData;
            Subnodes = subnodes ?? Array.Empty<LtpSubnodeData>();
        }

        /// <summary>
        /// BID data node.
        /// </summary>
        public Bid BidData { get; }

        /// <summary>
        /// Daftar subnode untuk node ini.
        /// </summary>
        public IReadOnlyList<LtpSubnodeData> Subnodes { get; }
    }

    /// <summary>
    /// Informasi BID data dan BID subnode untuk node LTP.
    /// </summary>
    private readonly struct LtpNodeWriteInfo
    {
        /// <summary>
        /// Membuat info penulisan node.
        /// </summary>
        /// <param name="bidData">BID data node.</param>
        /// <param name="bidSub">BID subnode node.</param>
        public LtpNodeWriteInfo(Bid bidData, Bid bidSub)
        {
            BidData = bidData;
            BidSub = bidSub;
        }

        /// <summary>
        /// BID data node.
        /// </summary>
        public Bid BidData { get; }

        /// <summary>
        /// BID subnode node.
        /// </summary>
        public Bid BidSub { get; }
    }

    /// <summary>
    /// Allocator NID berbasis indeks maksimum per tipe.
    /// </summary>
    private sealed class NidAllocator
    {
        private readonly Dictionary<NidType, uint> _nextIndex = new();
        private readonly uint[] _headerCounters;

        /// <summary>
        /// Membuat allocator NID dari entri NBT yang sudah ada.
        /// </summary>
        /// <param name="entries">Entri NBT.</param>
        /// <param name="headerCounters">Snapshot counter rgnid[] dari header.</param>
        public NidAllocator(IEnumerable<NbtEntry> entries, IReadOnlyList<uint> headerCounters)
        {
            _headerCounters = new uint[32];
            if (headerCounters is not null)
            {
                for (var i = 0; i < _headerCounters.Length && i < headerCounters.Count; i++)
                {
                    _headerCounters[i] = headerCounters[i];
                }
            }

            foreach (var entry in entries)
            {
                var type = entry.Nid.Type;
                var index = entry.Nid.Index;
                if (_nextIndex.TryGetValue(type, out var current))
                {
                    if (index >= current)
                    {
                        _nextIndex[type] = index + 1;
                    }
                }
                else
                {
                    _nextIndex[type] = index + 1;
                }
            }

            foreach (NidType type in Enum.GetValues(typeof(NidType)))
            {
                var slot = (int)type;
                if (slot < 0 || slot >= _headerCounters.Length)
                {
                    continue;
                }

                var raw = _headerCounters[slot];
                var indexFromHeader = raw >> 5;
                if (indexFromHeader == 0)
                {
                    continue;
                }

                if (_nextIndex.TryGetValue(type, out var current))
                {
                    if (indexFromHeader > current)
                    {
                        _nextIndex[type] = indexFromHeader;
                    }
                }
                else
                {
                    _nextIndex[type] = indexFromHeader;
                }
            }
        }

        /// <summary>
        /// Mengambil NID baru berdasarkan tipe.
        /// </summary>
        /// <param name="type">Tipe NID.</param>
        /// <returns>NID baru.</returns>
        public Nid Next(NidType type)
        {
            if (!_nextIndex.TryGetValue(type, out var index))
            {
                index = 1;
                _nextIndex[type] = 2;
            }
            else
            {
                _nextIndex[type] = index + 1;
            }

            var value = (index << 5) | (uint)type;
            return new Nid(value);
        }

        /// <summary>
        /// Mengambil snapshot nilai NID berikutnya per tipe untuk dipersist ke rgnid[].
        /// </summary>
        /// <returns>Dictionary tipe NID ke nilai raw NID berikutnya.</returns>
        public IReadOnlyDictionary<NidType, uint> SnapshotNextNids()
        {
            var snapshot = new Dictionary<NidType, uint>();
            foreach (NidType type in Enum.GetValues(typeof(NidType)))
            {
                if (!_nextIndex.TryGetValue(type, out var index))
                {
                    var slot = (int)type;
                    if (slot >= 0 && slot < _headerCounters.Length && _headerCounters[slot] > 0)
                    {
                        snapshot[type] = _headerCounters[slot];
                    }

                    continue;
                }

                snapshot[type] = (index << 5) | (uint)type;
            }

            return snapshot;
        }
    }
}
