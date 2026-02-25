using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Application;
using Emcode.Pst.Application.Abstractions;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ltp;
using Emcode.Pst.Infrastructure.Ndb;
using Emcode.Pst.Shared;

namespace Emcode.Pst.Infrastructure;

/// <summary>
/// Reader PST berbasis parsing NDB untuk mengekstrak folder dan message nyata.
/// </summary>
public sealed class PstNdbReader : IPstReader
{
    private const string DefaultStoreFolderName = "Top of Outlook data file";
    private const string SearchRootFolderName = "Search Root";
    private const uint MessageStoreNidValue = 0x00000021;
    private const uint StoreFolderNidValue = 0x00008022;
    private const ushort PidTagDisplayName = 0x3001;
    private const ushort PidTagComment = 0x3004;
    private const ushort PidTagMessageClass = 0x001A;
    private const ushort PidTagSubject = 0x0037;
    private const ushort PidTagNormalizedSubject = 0x0E1D;
    private const ushort PidTagConversationTopic = 0x0070;
    private const ushort PidTagConversationIndex = 0x0071;
    private const ushort PidTagTransportMessageHeaders = 0x007D;
    private const ushort PidTagDeliveryTime = 0x0E06;
    private const ushort PidTagInternetMessageId = 0x1035;
    private const ushort PidTagSenderEmailAddress = 0x0C1F;
    private const ushort PidTagSenderSmtpAddress = 0x5D01;
    private const ushort PidTagSentRepresentingName = 0x0042;
    private const ushort PidTagSentRepresentingEmailAddress = 0x0065;
    private const ushort PidTagOriginalSenderName = 0x005A;
    private const ushort PidTagOriginalSenderEmailAddress = 0x005B;
    private const ushort PidTagDisplayTo = 0x0E04;
    private const ushort PidTagDisplayCc = 0x0E03;
    private const ushort PidTagDisplayBcc = 0x0E02;
    private const ushort PidTagClientSubmitTime = 0x0039;
    private const ushort PidTagMessageSubmissionId = 0x0047;
    private const ushort PidTagLastModificationTime = 0x3008;
    private const ushort PidTagMessageFlags = 0x0E07;
    private const ushort PidTagReadReceiptRequested = 0x0029;
    private const ushort PidTagDeliveryReceiptRequested = 0x0023;
    private const ushort PidTagHasAttachments = 0x0E1B;
    private const ushort PidTagImportance = 0x0017;
    private const ushort PidTagPriority = 0x0026;
    private const ushort PidTagSensitivity = 0x0036;
    /// <summary>
    /// Property id untuk ukuran pesan (PidTagMessageSize).
    /// </summary>
    private const ushort PidTagMessageSize = 0x0E08;
    /// <summary>
    /// Property id untuk nama pengirim (PidTagSenderName).
    /// </summary>
    private const ushort PidTagSenderName = 0x0C1A;
    /// <summary>
    /// Property id untuk body teks (PidTagBody).
    /// </summary>
    private const ushort PidTagBody = 0x1000;
    /// <summary>
    /// Property id untuk body HTML (PidTagHtml).
    /// </summary>
    private const ushort PidTagHtml = 0x1013;

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

    /// <summary>
    /// Membaca PST menggunakan parsing NDB, BBT/NBT, dan PC.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <returns>Hasil pembacaan PST.</returns>
    public PstReadResult Read(string path, PstOpenOptions options)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        Guard.NotNull(options, nameof(options));

        using var stream = File.OpenRead(path);
        var header = new NdbHeaderReader().Read(stream);
        ValidateFormat(header.HeaderInfo.Format, options);
        ValidateCryptMethod(header.HeaderInfo.CryptMethod);

        var btreeReader = new PstBTreeReader(stream, header.HeaderInfo.Format);
        var bbtEntries = btreeReader.ReadBbt(header.BbtRoot);
        var nbtEntries = btreeReader.ReadNbt(header.NbtRoot);

        var blockReader = new PstBlockReader(stream, header.HeaderInfo.Format, header.HeaderInfo.CryptMethod, bbtEntries);
        var attachmentProvider = new PstAttachmentContentProvider(
            path,
            header.HeaderInfo.Format,
            header.HeaderInfo.CryptMethod,
            bbtEntries);
        var folderMap = BuildFolders(nbtEntries, blockReader, header.HeaderInfo.Format);
        var rootChildren = BuildHierarchy(folderMap, nbtEntries, blockReader, header.HeaderInfo.Format);
        ApplyStorePropertiesFromMessageStore(folderMap, rootChildren, nbtEntries, blockReader, header.HeaderInfo.Format);
        BuildMessages(nbtEntries, folderMap, blockReader, header.HeaderInfo.Format, attachmentProvider);

        var root = new PstFolder("root", "Root")
        {
            SubFolders = rootChildren,
            Messages = Array.Empty<PstMessage>()
        };

        var allFolders = new List<PstFolder> { root };
        allFolders.AddRange(folderMap.Values);

        return new PstReadResult(header.HeaderInfo, root, allFolders);
    }

    /// <summary>
    /// Membaca PST secara asynchronous menggunakan parsing NDB, BBT/NBT, dan PC.
    /// </summary>
    /// <param name="path">Path file PST.</param>
    /// <param name="options">Opsi pembukaan PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Hasil pembacaan PST.</returns>
    public async Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(path, nameof(path));
        Guard.NotNull(options, nameof(options));

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.RandomAccess);

        var header = await new NdbHeaderReader().ReadAsync(stream, cancellationToken).ConfigureAwait(false);
        ValidateFormat(header.HeaderInfo.Format, options);
        ValidateCryptMethod(header.HeaderInfo.CryptMethod);

        var btreeReader = new PstBTreeReader(stream, header.HeaderInfo.Format);
        var bbtEntries = await btreeReader.ReadBbtAsync(header.BbtRoot, cancellationToken).ConfigureAwait(false);
        var nbtEntries = await btreeReader.ReadNbtAsync(header.NbtRoot, cancellationToken).ConfigureAwait(false);

        var blockReader = new PstBlockReader(stream, header.HeaderInfo.Format, header.HeaderInfo.CryptMethod, bbtEntries);
        var attachmentProvider = new PstAttachmentContentProvider(
            path,
            header.HeaderInfo.Format,
            header.HeaderInfo.CryptMethod,
            bbtEntries);
        var folderMap = await BuildFoldersAsync(nbtEntries, blockReader, header.HeaderInfo.Format, cancellationToken).ConfigureAwait(false);
        var rootChildren = await BuildHierarchyAsync(folderMap, nbtEntries, blockReader, header.HeaderInfo.Format, cancellationToken)
            .ConfigureAwait(false);
        await ApplyStorePropertiesFromMessageStoreAsync(folderMap, rootChildren, nbtEntries, blockReader, header.HeaderInfo.Format, cancellationToken)
            .ConfigureAwait(false);
        await BuildMessagesAsync(nbtEntries, folderMap, blockReader, header.HeaderInfo.Format, attachmentProvider, cancellationToken)
            .ConfigureAwait(false);

        var root = new PstFolder("root", "Root")
        {
            SubFolders = rootChildren,
            Messages = Array.Empty<PstMessage>()
        };

        var allFolders = new List<PstFolder> { root };
        allFolders.AddRange(folderMap.Values);

        return new PstReadResult(header.HeaderInfo, root, allFolders);
    }

    /// <summary>
    /// Memvalidasi format PST terhadap opsi pembukaan.
    /// </summary>
    /// <param name="format">Format PST.</param>
    /// <param name="options">Opsi pembukaan.</param>
    private static void ValidateFormat(PstFormat format, PstOpenOptions options)
    {
        if (format == PstFormat.Ansi && !options.AllowAnsi)
        {
            throw new InvalidDataException("PST ANSI tidak diizinkan oleh opsi pembukaan.");
        }

        if (format == PstFormat.Unicode && !options.AllowUnicode)
        {
            throw new InvalidDataException("PST Unicode tidak diizinkan oleh opsi pembukaan.");
        }
    }

    /// <summary>
    /// Memvalidasi metode enkripsi/encoding yang didukung.
    /// </summary>
    /// <param name="cryptMethod">Metode enkripsi/encoding.</param>
    private static void ValidateCryptMethod(PstCryptMethod cryptMethod)
    {
        if (cryptMethod == PstCryptMethod.None || cryptMethod == PstCryptMethod.Permute)
        {
            return;
        }

        throw new NotSupportedException($"Metode enkripsi {cryptMethod} belum didukung.");
    }

    /// <summary>
    /// Membentuk folder map dari NBT.
    /// </summary>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Map NID ke folder.</returns>
    private static Dictionary<uint, PstFolder> BuildFolders(
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        var folderMap = new Dictionary<uint, PstFolder>();
        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            var folder = new PstFolder(entry.Nid.ToString(), $"Folder {entry.Nid.Value:X8}");
            var pc = CreatePropertyContext(entry, blockReader, format);
            var name = pc.GetString(PidTagDisplayName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                folder.Name = name;
            }

            var comment = pc.GetString(PidTagComment);
            if (comment is not null)
            {
                folder.Description = comment;
                folder.Comment = comment;
            }

            folderMap[entry.Nid.Value] = folder;
        }

        return folderMap;
    }

    /// <summary>
    /// Membentuk folder map dari NBT secara asynchronous.
    /// </summary>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Map NID ke folder.</returns>
    private static async Task<Dictionary<uint, PstFolder>> BuildFoldersAsync(
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        var folderMap = new Dictionary<uint, PstFolder>();
        foreach (var entry in nbtEntries.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            var folder = new PstFolder(entry.Nid.ToString(), $"Folder {entry.Nid.Value:X8}");
            var pc = await CreatePropertyContextAsync(entry, blockReader, format, cancellationToken).ConfigureAwait(false);
            var name = pc.GetString(PidTagDisplayName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                folder.Name = name;
            }

            var comment = pc.GetString(PidTagComment);
            if (comment is not null)
            {
                folder.Description = comment;
                folder.Comment = comment;
            }

            folderMap[entry.Nid.Value] = folder;
        }

        return folderMap;
    }

    /// <summary>
    /// Membangun relasi parent-child folder dengan urutan dari hierarchy table bila tersedia.
    /// </summary>
    /// <param name="folderMap">Map folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Daftar subfolder root.</returns>
    private static IReadOnlyList<PstFolder> BuildHierarchy(
        IDictionary<uint, PstFolder> folderMap,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        var childIdMap = new Dictionary<uint, List<uint>>();
        var rootChildIds = new List<uint>();

        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            if (!entry.NidParent.IsZero && folderMap.ContainsKey(entry.NidParent.Value))
            {
                if (!childIdMap.TryGetValue(entry.NidParent.Value, out var list))
                {
                    list = new List<uint>();
                    childIdMap[entry.NidParent.Value] = list;
                }

                list.Add(entry.Nid.Value);
            }
            else
            {
                rootChildIds.Add(entry.Nid.Value);
            }
        }

        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            if (!childIdMap.TryGetValue(entry.Nid.Value, out var childIds))
            {
                continue;
            }

            var orderedRowIds = TryReadHierarchyTableRowIds(entry, nbtEntries, blockReader, format);
            if (orderedRowIds.Count == 0)
            {
                continue;
            }

            childIdMap[entry.Nid.Value] = OrderChildIds(childIds, orderedRowIds);
        }

        foreach (var entry in folderMap)
        {
            if (childIdMap.TryGetValue(entry.Key, out var childIds))
            {
                var list = new List<PstFolder>(childIds.Count);
                foreach (var childId in childIds)
                {
                    if (folderMap.TryGetValue(childId, out var child))
                    {
                        list.Add(child);
                    }
                }

                entry.Value.SubFolders = list;
            }
            else
            {
                entry.Value.SubFolders = Array.Empty<PstFolder>();
            }
        }

        var rootChildren = new List<PstFolder>(rootChildIds.Count);
        foreach (var childId in rootChildIds)
        {
            if (folderMap.TryGetValue(childId, out var folder))
            {
                rootChildren.Add(folder);
            }
        }

        return rootChildren;
    }

    /// <summary>
    /// Mengaplikasikan properti store dari node internal/message-store ke folder store utama bila nilai folder masih default.
    /// </summary>
    /// <param name="folderMap">Map folder hasil parsing.</param>
    /// <param name="rootChildren">Daftar child pada root virtual.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    private static void ApplyStorePropertiesFromMessageStore(
        IDictionary<uint, PstFolder> folderMap,
        IReadOnlyList<PstFolder> rootChildren,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        var storeProperties = TryReadStorePropertiesFromMessageStore(nbtEntries, blockReader, format);
        if (!storeProperties.HasValue)
        {
            return;
        }

        var storeFolder = ResolveStoreFolderCandidate(folderMap, rootChildren, nbtEntries);
        if (storeFolder is null)
        {
            return;
        }

        var snapshot = storeProperties.Value;
        if (!string.IsNullOrWhiteSpace(snapshot.DisplayName) &&
            (string.IsNullOrWhiteSpace(storeFolder.Name) ||
             string.Equals(storeFolder.Name, DefaultStoreFolderName, StringComparison.OrdinalIgnoreCase)))
        {
            storeFolder.Name = snapshot.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(snapshot.Comment))
        {
            storeFolder.Comment = snapshot.Comment;
        }
    }

    /// <summary>
    /// Mengaplikasikan properti store dari node internal/message-store secara asynchronous.
    /// </summary>
    /// <param name="folderMap">Map folder hasil parsing.</param>
    /// <param name="rootChildren">Daftar child pada root virtual.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private static async Task ApplyStorePropertiesFromMessageStoreAsync(
        IDictionary<uint, PstFolder> folderMap,
        IReadOnlyList<PstFolder> rootChildren,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        var storeProperties = await TryReadStorePropertiesFromMessageStoreAsync(nbtEntries, blockReader, format, cancellationToken)
            .ConfigureAwait(false);
        if (!storeProperties.HasValue)
        {
            return;
        }

        var storeFolder = ResolveStoreFolderCandidate(folderMap, rootChildren, nbtEntries);
        if (storeFolder is null)
        {
            return;
        }

        var snapshot = storeProperties.Value;
        if (!string.IsNullOrWhiteSpace(snapshot.DisplayName) &&
            (string.IsNullOrWhiteSpace(storeFolder.Name) ||
             string.Equals(storeFolder.Name, DefaultStoreFolderName, StringComparison.OrdinalIgnoreCase)))
        {
            storeFolder.Name = snapshot.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(snapshot.Comment))
        {
            storeFolder.Comment = snapshot.Comment;
        }
    }

    /// <summary>
    /// Mengambil properti store dari node internal/message-store bila tersedia.
    /// </summary>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Snapshot properti store atau null.</returns>
    private static StorePropertiesSnapshot? TryReadStorePropertiesFromMessageStore(
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        if (nbtEntries.TryGetValue(MessageStoreNidValue, out var messageStoreEntry))
        {
            var direct = TryReadStoreProperties(messageStoreEntry, blockReader, format);
            if (direct.HasValue)
            {
                return direct;
            }
        }

        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.Internal || entry.Nid.Value == MessageStoreNidValue)
            {
                continue;
            }

            var candidate = TryReadStoreProperties(entry, blockReader, format);
            if (candidate.HasValue)
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// Mengambil properti store dari node internal/message-store secara asynchronous.
    /// </summary>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Snapshot properti store atau null.</returns>
    private static async Task<StorePropertiesSnapshot?> TryReadStorePropertiesFromMessageStoreAsync(
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        if (nbtEntries.TryGetValue(MessageStoreNidValue, out var messageStoreEntry))
        {
            var direct = await TryReadStorePropertiesAsync(messageStoreEntry, blockReader, format, cancellationToken).ConfigureAwait(false);
            if (direct.HasValue)
            {
                return direct;
            }
        }

        foreach (var entry in nbtEntries.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Nid.Type != NidType.Internal || entry.Nid.Value == MessageStoreNidValue)
            {
                continue;
            }

            var candidate = await TryReadStorePropertiesAsync(entry, blockReader, format, cancellationToken).ConfigureAwait(false);
            if (candidate.HasValue)
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// Mencoba membaca display name/comment dari satu entry NBT.
    /// </summary>
    /// <param name="entry">Entri NBT target.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Snapshot properti store atau null.</returns>
    private static StorePropertiesSnapshot? TryReadStoreProperties(
        NbtEntry entry,
        PstBlockReader blockReader,
        PstFormat format)
    {
        try
        {
            var pc = CreatePropertyContext(entry, blockReader, format);
            var displayName = pc.GetString(PidTagDisplayName);
            var comment = pc.GetString(PidTagComment);
            if (string.IsNullOrWhiteSpace(displayName) && string.IsNullOrWhiteSpace(comment))
            {
                return null;
            }

            return new StorePropertiesSnapshot(displayName, comment);
        }
        catch (InvalidDataException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Mencoba membaca display name/comment dari satu entry NBT secara asynchronous.
    /// </summary>
    /// <param name="entry">Entri NBT target.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Snapshot properti store atau null.</returns>
    private static async Task<StorePropertiesSnapshot?> TryReadStorePropertiesAsync(
        NbtEntry entry,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        try
        {
            var pc = await CreatePropertyContextAsync(entry, blockReader, format, cancellationToken).ConfigureAwait(false);
            var displayName = pc.GetString(PidTagDisplayName);
            var comment = pc.GetString(PidTagComment);
            if (string.IsNullOrWhiteSpace(displayName) && string.IsNullOrWhiteSpace(comment))
            {
                return null;
            }

            return new StorePropertiesSnapshot(displayName, comment);
        }
        catch (InvalidDataException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Menentukan folder store utama yang akan menerima fallback properti store.
    /// </summary>
    /// <param name="folderMap">Map folder.</param>
    /// <param name="rootChildren">Daftar child root virtual.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <returns>Folder store kandidat atau null.</returns>
    private static PstFolder? ResolveStoreFolderCandidate(
        IDictionary<uint, PstFolder> folderMap,
        IReadOnlyList<PstFolder> rootChildren,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries)
    {
        if (folderMap.TryGetValue(StoreFolderNidValue, out var storeByFixedNid))
        {
            return storeByFixedNid;
        }

        foreach (var folder in folderMap.Values)
        {
            if (string.Equals(folder.Name, DefaultStoreFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return folder;
            }
        }

        var searchRoot = folderMap.Values.FirstOrDefault(
            folder => string.Equals(folder.Name, SearchRootFolderName, StringComparison.OrdinalIgnoreCase));
        if (searchRoot is not null && TryParseFolderNid(searchRoot.Id, out var searchNid) &&
            nbtEntries.TryGetValue(searchNid, out var searchEntry) &&
            !searchEntry.NidParent.IsZero &&
            folderMap.TryGetValue(searchEntry.NidParent.Value, out var parentFolder))
        {
            return parentFolder;
        }

        foreach (var folder in folderMap.Values)
        {
            if (folder.SubFolders.Any(child => string.Equals(child.Name, SearchRootFolderName, StringComparison.OrdinalIgnoreCase)))
            {
                return folder;
            }
        }

        if (rootChildren.Count > 0)
        {
            var firstRootChild = rootChildren[0];
            if (firstRootChild.SubFolders.Count > 0)
            {
                var bySearchRoot = firstRootChild.SubFolders.FirstOrDefault(
                    child => string.Equals(child.Name, SearchRootFolderName, StringComparison.OrdinalIgnoreCase));
                if (bySearchRoot is not null && TryParseFolderNid(bySearchRoot.Id, out var searchRootNid) &&
                    nbtEntries.TryGetValue(searchRootNid, out var searchRootEntry) &&
                    !searchRootEntry.NidParent.IsZero &&
                    folderMap.TryGetValue(searchRootEntry.NidParent.Value, out var parentFromRootChild))
                {
                    return parentFromRootChild;
                }
            }
        }

        return folderMap.Values.FirstOrDefault();
    }

    /// <summary>
    /// Mengurai string NID folder dalam format hex (`0xXXXXXXXX`) menjadi uint.
    /// </summary>
    /// <param name="value">Nilai id folder.</param>
    /// <param name="nidValue">Output NID.</param>
    /// <returns>True jika parse berhasil.</returns>
    private static bool TryParseFolderNid(string value, out uint nidValue)
    {
        nidValue = 0;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return uint.TryParse(value.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out nidValue);
    }

    /// <summary>
    /// Snapshot properti store yang dibaca dari node internal/message-store.
    /// </summary>
    /// <param name="DisplayName">Nama store.</param>
    /// <param name="Comment">Komentar store.</param>
    private readonly record struct StorePropertiesSnapshot(string? DisplayName, string? Comment);

    /// <summary>
    /// Membangun relasi parent-child folder secara asynchronous dengan urutan dari hierarchy table.
    /// </summary>
    /// <param name="folderMap">Map folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar subfolder root.</returns>
    private static async Task<IReadOnlyList<PstFolder>> BuildHierarchyAsync(
        IDictionary<uint, PstFolder> folderMap,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        var childIdMap = new Dictionary<uint, List<uint>>();
        var rootChildIds = new List<uint>();

        foreach (var entry in nbtEntries.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            if (!entry.NidParent.IsZero && folderMap.ContainsKey(entry.NidParent.Value))
            {
                if (!childIdMap.TryGetValue(entry.NidParent.Value, out var list))
                {
                    list = new List<uint>();
                    childIdMap[entry.NidParent.Value] = list;
                }

                list.Add(entry.Nid.Value);
            }
            else
            {
                rootChildIds.Add(entry.Nid.Value);
            }
        }

        foreach (var entry in nbtEntries.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            if (!childIdMap.TryGetValue(entry.Nid.Value, out var childIds))
            {
                continue;
            }

            var orderedRowIds = await TryReadHierarchyTableRowIdsAsync(entry, nbtEntries, blockReader, format, cancellationToken)
                .ConfigureAwait(false);
            if (orderedRowIds.Count == 0)
            {
                continue;
            }

            childIdMap[entry.Nid.Value] = OrderChildIds(childIds, orderedRowIds);
        }

        foreach (var entry in folderMap)
        {
            if (childIdMap.TryGetValue(entry.Key, out var childIds))
            {
                var list = new List<PstFolder>(childIds.Count);
                foreach (var childId in childIds)
                {
                    if (folderMap.TryGetValue(childId, out var child))
                    {
                        list.Add(child);
                    }
                }

                entry.Value.SubFolders = list;
            }
            else
            {
                entry.Value.SubFolders = Array.Empty<PstFolder>();
            }
        }

        var rootChildren = new List<PstFolder>(rootChildIds.Count);
        foreach (var childId in rootChildIds)
        {
            if (folderMap.TryGetValue(childId, out var folder))
            {
                rootChildren.Add(folder);
            }
        }

        return rootChildren;
    }

    /// <summary>
    /// Mengurutkan daftar child NID berdasarkan row ID hierarchy table.
    /// </summary>
    /// <param name="childIds">Daftar child NID.</param>
    /// <param name="orderedRowIds">Urutan row ID dari hierarchy table.</param>
    /// <returns>Daftar child NID terurut.</returns>
    private static List<uint> OrderChildIds(IReadOnlyList<uint> childIds, IReadOnlyList<uint> orderedRowIds)
    {
        var childSet = new HashSet<uint>(childIds);
        var ordered = new List<uint>(childIds.Count);
        var seen = new HashSet<uint>();

        foreach (var rowId in orderedRowIds)
        {
            if (!childSet.Contains(rowId) || !seen.Add(rowId))
            {
                continue;
            }

            ordered.Add(rowId);
        }

        foreach (var childId in childIds)
        {
            if (seen.Contains(childId))
            {
                continue;
            }

            ordered.Add(childId);
        }

        return ordered;
    }

    /// <summary>
    /// Membaca message dan mengaitkannya ke folder parent.
    /// </summary>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="folderMap">Map folder.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    private static void BuildMessages(
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        IDictionary<uint, PstFolder> folderMap,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider)
    {
        var messageEntries = new Dictionary<uint, NbtEntry>();
        var messagesByParent = new Dictionary<uint, List<NbtEntry>>();

        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.NormalMessage)
            {
                continue;
            }

            messageEntries[entry.Nid.Value] = entry;

            if (!entry.NidParent.IsZero)
            {
                if (!messagesByParent.TryGetValue(entry.NidParent.Value, out var list))
                {
                    list = new List<NbtEntry>();
                    messagesByParent[entry.NidParent.Value] = list;
                }

                list.Add(entry);
            }
        }

        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            if (!folderMap.TryGetValue(entry.Nid.Value, out var folder))
            {
                continue;
            }

            var orderedRowIds = TryReadContentsTableRowIds(entry, nbtEntries, blockReader, format);
            if (orderedRowIds.Count > 0)
            {
                folder.Messages = BuildMessagesFromRowIds(orderedRowIds, messageEntries, blockReader, format, attachmentProvider);
                continue;
            }

            if (messagesByParent.TryGetValue(entry.Nid.Value, out var fallbackEntries))
            {
                folder.Messages = BuildMessagesFromEntries(fallbackEntries, blockReader, format, attachmentProvider);
            }
            else
            {
                folder.Messages = Array.Empty<PstMessage>();
            }
        }
    }

    /// <summary>
    /// Membaca message dan mengaitkannya ke folder parent secara asynchronous.
    /// </summary>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="folderMap">Map folder.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    private static async Task BuildMessagesAsync(
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        IDictionary<uint, PstFolder> folderMap,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider,
        CancellationToken cancellationToken)
    {
        var messageEntries = new Dictionary<uint, NbtEntry>();
        var messagesByParent = new Dictionary<uint, List<NbtEntry>>();

        foreach (var entry in nbtEntries.Values)
        {
            if (entry.Nid.Type != NidType.NormalMessage)
            {
                continue;
            }

            messageEntries[entry.Nid.Value] = entry;

            if (!entry.NidParent.IsZero)
            {
                if (!messagesByParent.TryGetValue(entry.NidParent.Value, out var list))
                {
                    list = new List<NbtEntry>();
                    messagesByParent[entry.NidParent.Value] = list;
                }

                list.Add(entry);
            }
        }

        foreach (var entry in nbtEntries.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Nid.Type != NidType.NormalFolder)
            {
                continue;
            }

            if (!folderMap.TryGetValue(entry.Nid.Value, out var folder))
            {
                continue;
            }

            var orderedRowIds = await TryReadContentsTableRowIdsAsync(entry, nbtEntries, blockReader, format, cancellationToken)
                .ConfigureAwait(false);
            if (orderedRowIds.Count > 0)
            {
                folder.Messages = await BuildMessagesFromRowIdsAsync(
                        orderedRowIds,
                        messageEntries,
                        blockReader,
                        format,
                        attachmentProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
                continue;
            }

            if (messagesByParent.TryGetValue(entry.Nid.Value, out var fallbackEntries))
            {
                folder.Messages = await BuildMessagesFromEntriesAsync(
                        fallbackEntries,
                        blockReader,
                        format,
                        attachmentProvider,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                folder.Messages = Array.Empty<PstMessage>();
            }
        }
    }

    /// <summary>
    /// Membaca urutan row ID dari hierarchy table folder.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Daftar row ID sesuai urutan table.</returns>
    private static IReadOnlyList<uint> TryReadHierarchyTableRowIds(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        try
        {
            if (!TryGetHierarchyTableEntry(folderEntry, nbtEntries, out var tableEntry) || tableEntry is null)
            {
                return Array.Empty<uint>();
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
        catch (InvalidDataException)
        {
            return Array.Empty<uint>();
        }
    }

    /// <summary>
    /// Membaca urutan row ID dari hierarchy table folder secara asynchronous.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar row ID sesuai urutan table.</returns>
    private static async Task<IReadOnlyList<uint>> TryReadHierarchyTableRowIdsAsync(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetHierarchyTableEntry(folderEntry, nbtEntries, out var tableEntry) || tableEntry is null)
            {
                return Array.Empty<uint>();
            }

            var tableBlocks = await blockReader.ReadDataBlocksAsync(tableEntry.BidData, cancellationToken).ConfigureAwait(false);
            if (tableBlocks.Count == 0)
            {
                return Array.Empty<uint>();
            }

            var tableHeap = new HeapOnNode(tableBlocks);
            var tableSubnodes = new SubnodeReader(blockReader, format, tableEntry.BidSub);
            var tableContext = new TableContext(tableHeap, tableSubnodes);
            return tableContext.ReadRowIds();
        }
        catch (InvalidDataException)
        {
            return Array.Empty<uint>();
        }
    }

    /// <summary>
    /// Mencari entry NBT hierarchy table berdasarkan indeks folder.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="tableEntry">Entri table hasil.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetHierarchyTableEntry(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        out NbtEntry? tableEntry)
    {
        var index = folderEntry.Nid.Index;
        var hierarchyNidValue = (index << 5) | (uint)NidType.HierarchyTable;
        if (nbtEntries.TryGetValue(hierarchyNidValue, out tableEntry))
        {
            return true;
        }

        tableEntry = null;
        return false;
    }

    /// <summary>
    /// Membaca urutan row ID dari contents table folder.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Daftar row ID sesuai urutan table.</returns>
    private static IReadOnlyList<uint> TryReadContentsTableRowIds(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        try
        {
            if (!TryGetContentsTableEntry(folderEntry, nbtEntries, out var tableEntry) || tableEntry is null)
            {
                return Array.Empty<uint>();
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
        catch (InvalidDataException)
        {
            return Array.Empty<uint>();
        }
    }

    /// <summary>
    /// Membaca urutan row ID dari contents table folder secara asynchronous.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar row ID sesuai urutan table.</returns>
    private static async Task<IReadOnlyList<uint>> TryReadContentsTableRowIdsAsync(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryGetContentsTableEntry(folderEntry, nbtEntries, out var tableEntry) || tableEntry is null)
            {
                return Array.Empty<uint>();
            }

            var tableBlocks = await blockReader.ReadDataBlocksAsync(tableEntry.BidData, cancellationToken).ConfigureAwait(false);
            if (tableBlocks.Count == 0)
            {
                return Array.Empty<uint>();
            }

            var tableHeap = new HeapOnNode(tableBlocks);
            var tableSubnodes = new SubnodeReader(blockReader, format, tableEntry.BidSub);
            var tableContext = new TableContext(tableHeap, tableSubnodes);
            return tableContext.ReadRowIds();
        }
        catch (InvalidDataException)
        {
            return Array.Empty<uint>();
        }
    }

    /// <summary>
    /// Mencari entry NBT contents table berdasarkan indeks folder.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="tableEntry">Entri table hasil.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetContentsTableEntry(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        out NbtEntry? tableEntry)
    {
        var index = folderEntry.Nid.Index;
        var contentsNidValue = (index << 5) | (uint)NidType.ContentsTable;
        if (nbtEntries.TryGetValue(contentsNidValue, out tableEntry))
        {
            return true;
        }

        var assocNidValue = (index << 5) | (uint)NidType.AssocContentsTable;
        if (nbtEntries.TryGetValue(assocNidValue, out tableEntry))
        {
            return true;
        }

        tableEntry = null;
        return false;
    }

    /// <summary>
    /// Membentuk daftar pesan dari urutan row ID contents table.
    /// </summary>
    /// <param name="rowIds">Urutan row ID.</param>
    /// <param name="messageEntries">Map entri message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <returns>Daftar pesan.</returns>
    private static IReadOnlyList<PstMessage> BuildMessagesFromRowIds(
        IReadOnlyList<uint> rowIds,
        IReadOnlyDictionary<uint, NbtEntry> messageEntries,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider)
    {
        var messages = new List<PstMessage>(rowIds.Count);
        foreach (var rowId in rowIds)
        {
            if (!messageEntries.TryGetValue(rowId, out var entry))
            {
                continue;
            }

            messages.Add(CreateMessage(entry, blockReader, format, attachmentProvider));
        }

        return messages;
    }

    /// <summary>
    /// Membentuk daftar pesan dari urutan row ID contents table secara asynchronous.
    /// </summary>
    /// <param name="rowIds">Urutan row ID.</param>
    /// <param name="messageEntries">Map entri message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar pesan.</returns>
    private static async Task<IReadOnlyList<PstMessage>> BuildMessagesFromRowIdsAsync(
        IReadOnlyList<uint> rowIds,
        IReadOnlyDictionary<uint, NbtEntry> messageEntries,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider,
        CancellationToken cancellationToken)
    {
        var messages = new List<PstMessage>(rowIds.Count);
        foreach (var rowId in rowIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!messageEntries.TryGetValue(rowId, out var entry))
            {
                continue;
            }

            messages.Add(await CreateMessageAsync(entry, blockReader, format, attachmentProvider, cancellationToken).ConfigureAwait(false));
        }

        return messages;
    }

    /// <summary>
    /// Membentuk daftar pesan dari daftar entry message.
    /// </summary>
    /// <param name="entries">Daftar entry message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <returns>Daftar pesan.</returns>
    private static IReadOnlyList<PstMessage> BuildMessagesFromEntries(
        IReadOnlyList<NbtEntry> entries,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider)
    {
        var messages = new List<PstMessage>(entries.Count);
        foreach (var entry in entries)
        {
            messages.Add(CreateMessage(entry, blockReader, format, attachmentProvider));
        }

        return messages;
    }

    /// <summary>
    /// Membentuk daftar pesan dari daftar entry message secara asynchronous.
    /// </summary>
    /// <param name="entries">Daftar entry message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Daftar pesan.</returns>
    private static async Task<IReadOnlyList<PstMessage>> BuildMessagesFromEntriesAsync(
        IReadOnlyList<NbtEntry> entries,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider,
        CancellationToken cancellationToken)
    {
        var messages = new List<PstMessage>(entries.Count);
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            messages.Add(await CreateMessageAsync(entry, blockReader, format, attachmentProvider, cancellationToken).ConfigureAwait(false));
        }

        return messages;
    }

    /// <summary>
    /// Membuat pesan dari entry NBT dan mengisi properti minimum.
    /// </summary>
    /// <param name="entry">Entri message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <returns>Pesan terisi minimum.</returns>
    private static PstMessage CreateMessage(
        NbtEntry entry,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider)
    {
        var message = new PstMessage(entry.Nid.ToString());
        try
        {
            var blocks = blockReader.ReadDataBlocks(entry.BidData);
            var heap = new HeapOnNode(blocks);
            var subnodes = new SubnodeReader(blockReader, format, entry.BidSub);
            var pc = new PropertyContext(heap, subnodes);
            var subject = pc.GetString(PidTagSubject) ?? pc.GetString(PidTagNormalizedSubject);
            if (!string.IsNullOrWhiteSpace(subject))
            {
                message.Subject = NormalizeSubject(subject);
            }

            var messageClass = pc.GetString(PidTagMessageClass);
            if (!string.IsNullOrWhiteSpace(messageClass))
            {
                message.MessageClass = messageClass;
            }

            var senderName = pc.GetString(PidTagSenderName);
            if (!string.IsNullOrWhiteSpace(senderName))
            {
                message.SenderName = senderName;
            }

            var body = pc.GetString(PidTagBody);
            if (!string.IsNullOrWhiteSpace(body))
            {
                message.Body = body;
            }

            var htmlBody = TryReadHtmlBody(pc);
            if (!string.IsNullOrWhiteSpace(htmlBody))
            {
                message.HtmlBody = htmlBody;
                if (string.IsNullOrWhiteSpace(message.Body))
                {
                    message.Body = htmlBody;
                }
            }

            var delivery = pc.GetDateTime(PidTagDeliveryTime);
            if (delivery.HasValue)
            {
                message.DeliveryTime = delivery;
                message.ReceivedTime = delivery;
            }

            var size = pc.GetInt32(PidTagMessageSize);
            if (size.HasValue && size.Value >= 0)
            {
                message.Size = size.Value;
            }

            var internetMessageId = pc.GetString(PidTagInternetMessageId);
            if (!string.IsNullOrWhiteSpace(internetMessageId))
            {
                message.InternetMessageId = internetMessageId;
            }

            var senderEmail = pc.GetString(PidTagSenderEmailAddress);
            if (!string.IsNullOrWhiteSpace(senderEmail))
            {
                message.SenderEmailAddress = senderEmail;
            }

            var senderSmtp = pc.GetString(PidTagSenderSmtpAddress);
            if (!string.IsNullOrWhiteSpace(senderSmtp))
            {
                message.SenderSmtpAddress = senderSmtp;
            }

            var sentRepresentingName = pc.GetString(PidTagSentRepresentingName);
            if (!string.IsNullOrWhiteSpace(sentRepresentingName))
            {
                message.SentRepresentingName = sentRepresentingName;
            }

            var sentRepresentingEmail = pc.GetString(PidTagSentRepresentingEmailAddress);
            if (!string.IsNullOrWhiteSpace(sentRepresentingEmail))
            {
                message.SentRepresentingEmailAddress = sentRepresentingEmail;
            }

            var originalSenderName = pc.GetString(PidTagOriginalSenderName);
            if (!string.IsNullOrWhiteSpace(originalSenderName))
            {
                message.OriginalSenderName = originalSenderName;
            }

            var originalSenderEmail = pc.GetString(PidTagOriginalSenderEmailAddress);
            if (!string.IsNullOrWhiteSpace(originalSenderEmail))
            {
                message.OriginalSenderEmailAddress = originalSenderEmail;
            }

            var displayTo = pc.GetString(PidTagDisplayTo);
            if (!string.IsNullOrWhiteSpace(displayTo))
            {
                message.DisplayTo = displayTo;
            }

            var displayCc = pc.GetString(PidTagDisplayCc);
            if (!string.IsNullOrWhiteSpace(displayCc))
            {
                message.DisplayCc = displayCc;
            }

            var displayBcc = pc.GetString(PidTagDisplayBcc);
            if (!string.IsNullOrWhiteSpace(displayBcc))
            {
                message.DisplayBcc = displayBcc;
            }

            var clientSubmit = pc.GetDateTime(PidTagClientSubmitTime);
            if (clientSubmit.HasValue)
            {
                message.ClientSubmitTime = clientSubmit;
            }

            var submissionId = pc.GetBinary(PidTagMessageSubmissionId);
            if (submissionId.HasValue && !submissionId.Value.IsEmpty)
            {
                message.MessageSubmissionId = submissionId.Value;
            }

            var lastModification = pc.GetDateTime(PidTagLastModificationTime);
            if (lastModification.HasValue)
            {
                message.LastModificationTime = lastModification;
            }

            var flags = pc.GetInt32(PidTagMessageFlags);
            if (flags.HasValue)
            {
                message.MessageFlags = flags.Value;
            }

            var readReceipt = pc.GetBoolean(PidTagReadReceiptRequested);
            if (readReceipt.HasValue)
            {
                message.ReadReceiptRequested = readReceipt.Value;
            }

            var deliveryReceipt = pc.GetBoolean(PidTagDeliveryReceiptRequested);
            if (deliveryReceipt.HasValue)
            {
                message.DeliveryReceiptRequested = deliveryReceipt.Value;
            }

            var hasAttachments = pc.GetBoolean(PidTagHasAttachments);
            if (hasAttachments.HasValue)
            {
                message.HasAttachments = hasAttachments.Value;
            }

            var importance = pc.GetInt32(PidTagImportance);
            if (importance.HasValue)
            {
                message.Importance = importance.Value;
            }

            var priority = pc.GetInt32(PidTagPriority);
            if (priority.HasValue)
            {
                message.Priority = priority.Value;
            }

            var sensitivity = pc.GetInt32(PidTagSensitivity);
            if (sensitivity.HasValue)
            {
                message.Sensitivity = sensitivity.Value;
            }

            var transportHeaders = pc.GetString(PidTagTransportMessageHeaders);
            if (!string.IsNullOrWhiteSpace(transportHeaders))
            {
                message.TransportMessageHeaders = transportHeaders;
            }

            var conversationTopic = pc.GetString(PidTagConversationTopic);
            if (!string.IsNullOrWhiteSpace(conversationTopic))
            {
                message.ConversationTopic = conversationTopic;
            }

            var conversationIndex = pc.GetBinary(PidTagConversationIndex);
            if (conversationIndex.HasValue && !conversationIndex.Value.IsEmpty)
            {
                message.ConversationIndex = conversationIndex.Value;
            }

            if (string.IsNullOrWhiteSpace(message.SenderEmailAddress))
            {
                message.SenderEmailAddress = message.SentRepresentingEmailAddress;
            }

            if (string.IsNullOrWhiteSpace(message.SenderSmtpAddress))
            {
                message.SenderSmtpAddress = message.SentRepresentingEmailAddress;
            }

            if (string.IsNullOrWhiteSpace(message.SenderName))
            {
                message.SenderName = message.SentRepresentingName ?? message.SenderEmailAddress;
            }

            PopulateRecipientsAndAttachments(message, subnodes, blockReader, format, attachmentProvider);
            PopulateRecipientsFallbackFromDisplayFields(message);
        }
        catch (InvalidDataException)
        {
            // Abaikan error PC untuk menjaga pesan tetap dapat dibaca.
        }

        return message;
    }

    /// <summary>
    /// Membuat pesan dari entry NBT secara asynchronous dan mengisi properti minimum.
    /// </summary>
    /// <param name="entry">Entri message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Pesan terisi minimum.</returns>
    private static async Task<PstMessage> CreateMessageAsync(
        NbtEntry entry,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider,
        CancellationToken cancellationToken)
    {
        var message = new PstMessage(entry.Nid.ToString());
        try
        {
            var blocks = await blockReader.ReadDataBlocksAsync(entry.BidData, cancellationToken).ConfigureAwait(false);
            var heap = new HeapOnNode(blocks);
            var subnodes = new SubnodeReader(blockReader, format, entry.BidSub);
            var pc = new PropertyContext(heap, subnodes);
            var subject = pc.GetString(PidTagSubject) ?? pc.GetString(PidTagNormalizedSubject);
            if (!string.IsNullOrWhiteSpace(subject))
            {
                message.Subject = NormalizeSubject(subject);
            }

            var messageClass = pc.GetString(PidTagMessageClass);
            if (!string.IsNullOrWhiteSpace(messageClass))
            {
                message.MessageClass = messageClass;
            }

            var senderName = pc.GetString(PidTagSenderName);
            if (!string.IsNullOrWhiteSpace(senderName))
            {
                message.SenderName = senderName;
            }

            var body = pc.GetString(PidTagBody);
            if (!string.IsNullOrWhiteSpace(body))
            {
                message.Body = body;
            }

            var htmlBody = TryReadHtmlBody(pc);
            if (!string.IsNullOrWhiteSpace(htmlBody))
            {
                message.HtmlBody = htmlBody;
                if (string.IsNullOrWhiteSpace(message.Body))
                {
                    message.Body = htmlBody;
                }
            }

            var delivery = pc.GetDateTime(PidTagDeliveryTime);
            if (delivery.HasValue)
            {
                message.DeliveryTime = delivery;
                message.ReceivedTime = delivery;
            }

            var size = pc.GetInt32(PidTagMessageSize);
            if (size.HasValue && size.Value >= 0)
            {
                message.Size = size.Value;
            }

            var internetMessageId = pc.GetString(PidTagInternetMessageId);
            if (!string.IsNullOrWhiteSpace(internetMessageId))
            {
                message.InternetMessageId = internetMessageId;
            }

            var senderEmail = pc.GetString(PidTagSenderEmailAddress);
            if (!string.IsNullOrWhiteSpace(senderEmail))
            {
                message.SenderEmailAddress = senderEmail;
            }

            var senderSmtp = pc.GetString(PidTagSenderSmtpAddress);
            if (!string.IsNullOrWhiteSpace(senderSmtp))
            {
                message.SenderSmtpAddress = senderSmtp;
            }

            var sentRepresentingName = pc.GetString(PidTagSentRepresentingName);
            if (!string.IsNullOrWhiteSpace(sentRepresentingName))
            {
                message.SentRepresentingName = sentRepresentingName;
            }

            var sentRepresentingEmail = pc.GetString(PidTagSentRepresentingEmailAddress);
            if (!string.IsNullOrWhiteSpace(sentRepresentingEmail))
            {
                message.SentRepresentingEmailAddress = sentRepresentingEmail;
            }

            var originalSenderName = pc.GetString(PidTagOriginalSenderName);
            if (!string.IsNullOrWhiteSpace(originalSenderName))
            {
                message.OriginalSenderName = originalSenderName;
            }

            var originalSenderEmail = pc.GetString(PidTagOriginalSenderEmailAddress);
            if (!string.IsNullOrWhiteSpace(originalSenderEmail))
            {
                message.OriginalSenderEmailAddress = originalSenderEmail;
            }

            var displayTo = pc.GetString(PidTagDisplayTo);
            if (!string.IsNullOrWhiteSpace(displayTo))
            {
                message.DisplayTo = displayTo;
            }

            var displayCc = pc.GetString(PidTagDisplayCc);
            if (!string.IsNullOrWhiteSpace(displayCc))
            {
                message.DisplayCc = displayCc;
            }

            var displayBcc = pc.GetString(PidTagDisplayBcc);
            if (!string.IsNullOrWhiteSpace(displayBcc))
            {
                message.DisplayBcc = displayBcc;
            }

            var clientSubmit = pc.GetDateTime(PidTagClientSubmitTime);
            if (clientSubmit.HasValue)
            {
                message.ClientSubmitTime = clientSubmit;
            }

            var submissionId = pc.GetBinary(PidTagMessageSubmissionId);
            if (submissionId.HasValue && !submissionId.Value.IsEmpty)
            {
                message.MessageSubmissionId = submissionId.Value;
            }

            var lastModification = pc.GetDateTime(PidTagLastModificationTime);
            if (lastModification.HasValue)
            {
                message.LastModificationTime = lastModification;
            }

            var flags = pc.GetInt32(PidTagMessageFlags);
            if (flags.HasValue)
            {
                message.MessageFlags = flags.Value;
            }

            var readReceipt = pc.GetBoolean(PidTagReadReceiptRequested);
            if (readReceipt.HasValue)
            {
                message.ReadReceiptRequested = readReceipt.Value;
            }

            var deliveryReceipt = pc.GetBoolean(PidTagDeliveryReceiptRequested);
            if (deliveryReceipt.HasValue)
            {
                message.DeliveryReceiptRequested = deliveryReceipt.Value;
            }

            var hasAttachments = pc.GetBoolean(PidTagHasAttachments);
            if (hasAttachments.HasValue)
            {
                message.HasAttachments = hasAttachments.Value;
            }

            var importance = pc.GetInt32(PidTagImportance);
            if (importance.HasValue)
            {
                message.Importance = importance.Value;
            }

            var priority = pc.GetInt32(PidTagPriority);
            if (priority.HasValue)
            {
                message.Priority = priority.Value;
            }

            var sensitivity = pc.GetInt32(PidTagSensitivity);
            if (sensitivity.HasValue)
            {
                message.Sensitivity = sensitivity.Value;
            }

            var transportHeaders = pc.GetString(PidTagTransportMessageHeaders);
            if (!string.IsNullOrWhiteSpace(transportHeaders))
            {
                message.TransportMessageHeaders = transportHeaders;
            }

            var conversationTopic = pc.GetString(PidTagConversationTopic);
            if (!string.IsNullOrWhiteSpace(conversationTopic))
            {
                message.ConversationTopic = conversationTopic;
            }

            var conversationIndex = pc.GetBinary(PidTagConversationIndex);
            if (conversationIndex.HasValue && !conversationIndex.Value.IsEmpty)
            {
                message.ConversationIndex = conversationIndex.Value;
            }

            if (string.IsNullOrWhiteSpace(message.SenderEmailAddress))
            {
                message.SenderEmailAddress = message.SentRepresentingEmailAddress;
            }

            if (string.IsNullOrWhiteSpace(message.SenderSmtpAddress))
            {
                message.SenderSmtpAddress = message.SentRepresentingEmailAddress;
            }

            if (string.IsNullOrWhiteSpace(message.SenderName))
            {
                message.SenderName = message.SentRepresentingName ?? message.SenderEmailAddress;
            }

            PopulateRecipientsAndAttachments(message, subnodes, blockReader, format, attachmentProvider);
            PopulateRecipientsFallbackFromDisplayFields(message);
        }
        catch (InvalidDataException)
        {
            // Abaikan error PC untuk menjaga pesan tetap dapat dibaca.
        }

        return message;
    }

    /// <summary>
    /// Mengambil body HTML dari Property Context dengan fallback ANSI/Unicode.
    /// </summary>
    /// <param name="pc">Property Context message.</param>
    /// <returns>Body HTML atau null.</returns>
    private static string? TryReadHtmlBody(PropertyContext pc)
    {
        var raw = pc.GetBinary(PidTagHtml);
        if (!raw.HasValue || raw.Value.IsEmpty)
        {
            return null;
        }

        return DecodeHtmlBody(raw.Value.Span);
    }

    /// <summary>
    /// Mendeteksi encoding HTML biner dan mengubahnya menjadi string.
    /// </summary>
    /// <param name="data">Data biner HTML.</param>
    /// <returns>String HTML atau null.</returns>
    private static string? DecodeHtmlBody(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return null;
        }

        if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
        {
            return Encoding.Unicode.GetString(data).TrimEnd('\0');
        }

        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(data).TrimEnd('\0');
        }

        var sampleLength = Math.Min(data.Length, 128);
        var zeroCount = 0;
        var pairCount = sampleLength / 2;
        for (var i = 1; i < sampleLength; i += 2)
        {
            if (data[i] == 0)
            {
                zeroCount++;
            }
        }

        if (pairCount > 0 && zeroCount >= pairCount / 2)
        {
            return Encoding.Unicode.GetString(data).TrimEnd('\0');
        }

        var utf8 = Encoding.UTF8.GetString(data);
        if (utf8.Contains('\uFFFD'))
        {
            return Encoding.Latin1.GetString(data).TrimEnd('\0');
        }

        return utf8.TrimEnd('\0');
    }

    /// <summary>
    /// Mengisi daftar recipient dan attachment dari subnode message bila tersedia.
    /// </summary>
    /// <param name="message">Message target.</param>
    /// <param name="subnodes">Subnode reader message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="attachmentProvider">Provider konten attachment bila tersedia.</param>
    private static void PopulateRecipientsAndAttachments(
        PstMessage message,
        SubnodeReader subnodes,
        PstBlockReader blockReader,
        PstFormat format,
        IPstAttachmentContentProvider? attachmentProvider)
    {
        var recipients = new List<PstRecipient>();
        var attachments = new List<PstAttachment>();

        foreach (var entry in subnodes.EnumerateSubnodes())
        {
            try
            {
                var blocks = blockReader.ReadDataBlocks(entry.Value.BidData);
                if (blocks.Count == 0)
                {
                    continue;
                }

                var tableHeap = new HeapOnNode(blocks);
                var tableSubnodes = new SubnodeReader(blockReader, format, entry.Value.BidSub);
                var tableContext = new TableContext(tableHeap, tableSubnodes);
                var columns = tableContext.ReadColumns();
                if (columns.Count == 0)
                {
                    continue;
                }

                if (IsRecipientTable(columns))
                {
                    recipients.AddRange(ReadRecipients(tableContext));
                    continue;
                }

                if (IsAttachmentTable(columns))
                {
                    attachments.AddRange(ReadAttachments(tableContext));
                }
            }
            catch (InvalidDataException)
            {
                // Abaikan subnode non-table agar parsing recipient/attachment lain tetap berjalan.
            }
        }

        if (attachments.Count == 0)
        {
            attachments.AddRange(ReadAttachmentsFromAttachmentSubnodes(subnodes, blockReader, format));
        }

        if (attachmentProvider is not null)
        {
            BindAttachmentContentSources(attachments, subnodes, attachmentProvider);
        }

        message.Recipients = recipients;
        message.Attachments = attachments;
    }

    /// <summary>
    /// Membaca fallback attachment langsung dari subnode bertipe Attachment bila attachment table tidak tersedia/invalid.
    /// </summary>
    /// <param name="subnodes">Subnode reader message.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Daftar attachment fallback.</returns>
    private static IReadOnlyList<PstAttachment> ReadAttachmentsFromAttachmentSubnodes(
        SubnodeReader subnodes,
        PstBlockReader blockReader,
        PstFormat format)
    {
        var attachments = new List<PstAttachment>();

        foreach (var entry in subnodes.EnumerateSubnodes())
        {
            if (entry.Key.Type != NidType.Attachment || entry.Value.BidData.IsZero)
            {
                continue;
            }

            try
            {
                var blocks = blockReader.ReadDataBlocks(entry.Value.BidData);
                if (blocks.Count == 0)
                {
                    continue;
                }

                var heap = new HeapOnNode(blocks);
                var nestedSubnodes = new SubnodeReader(blockReader, format, entry.Value.BidSub);
                var pc = new PropertyContext(heap, nestedSubnodes);

                var attachment = new PstAttachment
                {
                    AttachNumber = pc.GetInt32(PidTagAttachNumber) ?? (int)entry.Key.Index,
                    FileName = pc.GetString(PidTagAttachFilename),
                    LongFileName = pc.GetString(PidTagAttachLongFilename),
                    Size = pc.GetInt32(PidTagAttachSize),
                    MimeTag = pc.GetString(PidTagAttachMimeTag),
                    ContentId = pc.GetString(PidTagAttachContentId),
                    AttachMethod = pc.GetInt32(PidTagAttachMethod)
                };

                if (attachment.AttachNumber.HasValue || !string.IsNullOrWhiteSpace(attachment.FileName) ||
                    !string.IsNullOrWhiteSpace(attachment.LongFileName) || attachment.Size.HasValue ||
                    !string.IsNullOrWhiteSpace(attachment.MimeTag) || !string.IsNullOrWhiteSpace(attachment.ContentId) ||
                    attachment.AttachMethod.HasValue)
                {
                    attachments.Add(attachment);
                }
            }
            catch (InvalidDataException)
            {
                // Abaikan subnode attachment yang tidak valid.
            }
        }

        return attachments;
    }

    /// <summary>
    /// Menentukan apakah table context adalah recipient table.
    /// </summary>
    /// <param name="columns">Daftar kolom table.</param>
    /// <returns>True jika recipient table.</returns>
    private static bool IsRecipientTable(IReadOnlyList<TableContext.TableColumn> columns)
    {
        foreach (var column in columns)
        {
            if (ColumnMatchesPropertyId(column, PidTagRecipientType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Menentukan apakah table context adalah attachment table.
    /// </summary>
    /// <param name="columns">Daftar kolom table.</param>
    /// <returns>True jika attachment table.</returns>
    private static bool IsAttachmentTable(IReadOnlyList<TableContext.TableColumn> columns)
    {
        foreach (var column in columns)
        {
            if (ColumnMatchesPropertyId(column, PidTagAttachMethod) || ColumnMatchesPropertyId(column, PidTagAttachNumber))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Membaca daftar recipient dari table context.
    /// </summary>
    /// <param name="tableContext">Table context recipient.</param>
    /// <returns>Daftar recipient.</returns>
    private static IReadOnlyList<PstRecipient> ReadRecipients(TableContext tableContext)
    {
        var rows = tableContext.ReadRows();
        if (rows.Count == 0)
        {
            return Array.Empty<PstRecipient>();
        }

        var recipients = new List<PstRecipient>(rows.Count);
        foreach (var row in rows)
        {
            var recipient = new PstRecipient();
            if (TryGetRowInt32(row, PidTagRecipientType, out var recipientType))
            {
                recipient.RecipientType = recipientType;
            }

            if (TryGetRowString(row, PidTagEmailAddress, out var emailAddress))
            {
                recipient.EmailAddress = emailAddress;
            }

            if (TryGetRowString(row, PidTagDisplayName, out var displayName))
            {
                recipient.DisplayName = displayName;
            }

            if (TryGetRowString(row, PidTagAddrType, out var addrType))
            {
                recipient.AddressType = addrType;
            }

            if (TryGetRowString(row, PidTagSmtpAddress, out var smtpAddress))
            {
                recipient.SmtpAddress = smtpAddress;
            }

            if (recipient.RecipientType.HasValue || !string.IsNullOrWhiteSpace(recipient.EmailAddress) ||
                !string.IsNullOrWhiteSpace(recipient.DisplayName) || !string.IsNullOrWhiteSpace(recipient.AddressType) ||
                !string.IsNullOrWhiteSpace(recipient.SmtpAddress))
            {
                recipients.Add(recipient);
            }
        }

        return recipients;
    }

    /// <summary>
    /// Membaca daftar attachment dari table context.
    /// </summary>
    /// <param name="tableContext">Table context attachment.</param>
    /// <returns>Daftar attachment.</returns>
    private static IReadOnlyList<PstAttachment> ReadAttachments(TableContext tableContext)
    {
        var rows = tableContext.ReadRows();
        if (rows.Count == 0)
        {
            return Array.Empty<PstAttachment>();
        }

        var attachments = new List<PstAttachment>(rows.Count);
        foreach (var row in rows)
        {
            var attachment = new PstAttachment();
            if (TryGetRowInt32(row, PidTagAttachNumber, out var attachNumber))
            {
                attachment.AttachNumber = attachNumber;
            }

            if (TryGetRowString(row, PidTagAttachFilename, out var fileName))
            {
                attachment.FileName = fileName;
            }

            if (TryGetRowString(row, PidTagAttachLongFilename, out var longFileName))
            {
                attachment.LongFileName = longFileName;
            }

            if (TryGetRowInt32(row, PidTagAttachSize, out var size))
            {
                attachment.Size = size;
            }

            if (TryGetRowString(row, PidTagAttachMimeTag, out var mimeTag))
            {
                attachment.MimeTag = mimeTag;
            }

            if (TryGetRowString(row, PidTagAttachContentId, out var contentId))
            {
                attachment.ContentId = contentId;
            }

            if (TryGetRowInt32(row, PidTagAttachMethod, out var attachMethod))
            {
                attachment.AttachMethod = attachMethod;
            }

            if (attachment.AttachNumber.HasValue || !string.IsNullOrWhiteSpace(attachment.FileName) ||
                !string.IsNullOrWhiteSpace(attachment.LongFileName) || attachment.Size.HasValue ||
                !string.IsNullOrWhiteSpace(attachment.MimeTag) || !string.IsNullOrWhiteSpace(attachment.ContentId) ||
                attachment.AttachMethod.HasValue)
            {
                attachments.Add(attachment);
            }
        }

        return attachments;
    }

    /// <summary>
    /// Menghubungkan attachment dengan sumber konten berdasarkan subnode attachment.
    /// </summary>
    /// <param name="attachments">Daftar attachment message.</param>
    /// <param name="subnodes">Subnode reader message.</param>
    /// <param name="attachmentProvider">Provider konten attachment.</param>
    private static void BindAttachmentContentSources(
        List<PstAttachment> attachments,
        SubnodeReader subnodes,
        IPstAttachmentContentProvider attachmentProvider)
    {
        foreach (var attachment in attachments)
        {
            if (!attachment.AttachNumber.HasValue)
            {
                continue;
            }

            var attachNumber = attachment.AttachNumber.Value;
            if (attachNumber < 0)
            {
                continue;
            }

            var nidValue = ((uint)attachNumber << 5) | (uint)NidType.Attachment;
            var nid = new Nid(nidValue);
            if (!subnodes.TryGetSubnodeInfo(nid, out var info) || info.BidData.IsZero)
            {
                continue;
            }

            var reference = new PstAttachmentContentReference(info.BidData.Raw, info.BidSub.Raw);
            attachment.SetContentSource(attachmentProvider, reference);
        }
    }

    /// <summary>
    /// Membuat property tag dari prop id dan type.
    /// </summary>
    /// <param name="propId">Property id.</param>
    /// <param name="propType">Property type.</param>
    /// <returns>Property tag.</returns>
    private static uint MakePropertyTag(ushort propId, PstPropertyType propType)
    {
        return ((uint)propType << 16) | propId;
    }

    /// <summary>
    /// Mengambil string dari row berdasarkan prop id.
    /// </summary>
    /// <param name="row">Row table context.</param>
    /// <param name="propId">Property id.</param>
    /// <param name="value">Nilai string.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetRowString(TableContext.TableRow row, ushort propId, out string value)
    {
        value = string.Empty;
        if (TryGetRowCell(row, propId, PstPropertyType.String, out var cell) ||
            TryGetRowCell(row, propId, PstPropertyType.String8, out cell))
        {
            value = DecodeString(cell.PropType, cell.Data);
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    /// <summary>
    /// Mengambil integer dari row berdasarkan prop id.
    /// </summary>
    /// <param name="row">Row table context.</param>
    /// <param name="propId">Property id.</param>
    /// <param name="value">Nilai integer.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetRowInt32(TableContext.TableRow row, ushort propId, out int value)
    {
        value = 0;
        if (!TryGetRowCell(row, propId, PstPropertyType.Integer32, out var cell))
        {
            return false;
        }

        if (cell.Data.Length < 4)
        {
            return false;
        }

        value = BitConverter.ToInt32(cell.Data.Span);
        return true;
    }

    /// <summary>
    /// Mengambil boolean dari row berdasarkan prop id.
    /// </summary>
    /// <param name="row">Row table context.</param>
    /// <param name="propId">Property id.</param>
    /// <param name="value">Nilai boolean.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetRowBoolean(TableContext.TableRow row, ushort propId, out bool value)
    {
        value = false;
        if (!TryGetRowCell(row, propId, PstPropertyType.Boolean, out var cell))
        {
            return false;
        }

        if (cell.Data.Length < 2)
        {
            return false;
        }

        value = BitConverter.ToInt16(cell.Data.Span) != 0;
        return true;
    }

    /// <summary>
    /// Mengambil waktu dari row berdasarkan prop id.
    /// </summary>
    /// <param name="row">Row table context.</param>
    /// <param name="propId">Property id.</param>
    /// <param name="value">Nilai waktu.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetRowDateTime(TableContext.TableRow row, ushort propId, out DateTimeOffset value)
    {
        value = default;
        if (!TryGetRowCell(row, propId, PstPropertyType.Time, out var cell))
        {
            return false;
        }

        if (cell.Data.Length < 8)
        {
            return false;
        }

        var fileTime = BitConverter.ToInt64(cell.Data.Span);
        value = DateTimeOffset.FromFileTime(fileTime);
        return true;
    }

    /// <summary>
    /// Mengambil data biner dari row berdasarkan prop id.
    /// </summary>
    /// <param name="row">Row table context.</param>
    /// <param name="propId">Property id.</param>
    /// <param name="value">Data biner.</param>
    /// <returns>True jika ditemukan.</returns>
    private static bool TryGetRowBinary(TableContext.TableRow row, ushort propId, out ReadOnlyMemory<byte> value)
    {
        value = ReadOnlyMemory<byte>.Empty;
        if (!TryGetRowCell(row, propId, PstPropertyType.Binary, out var cell))
        {
            return false;
        }

        value = cell.Data;
        return !value.IsEmpty;
    }

    /// <summary>
    /// Mengubah data string dari table row sesuai tipe properti.
    /// </summary>
    /// <param name="propType">Tipe properti.</param>
    /// <param name="data">Data mentah.</param>
    /// <returns>String hasil decoding.</returns>
    private static string DecodeString(ushort propType, ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
        {
            return string.Empty;
        }

        if (propType == (ushort)PstPropertyType.String)
        {
            return Encoding.Unicode.GetString(data.Span).TrimEnd('\0');
        }

        return Encoding.Latin1.GetString(data.Span).TrimEnd('\0');
    }

    /// <summary>
    /// Mengambil cell row dengan dukungan dua orientasi property tag (standar writer internal dan baseline Outlook).
    /// </summary>
    /// <param name="row">Row table context.</param>
    /// <param name="propId">Property id.</param>
    /// <param name="propType">Property type.</param>
    /// <param name="cell">Cell hasil.</param>
    /// <returns>True jika cell ditemukan.</returns>
    private static bool TryGetRowCell(TableContext.TableRow row, ushort propId, PstPropertyType propType, out TableContext.TableCellValue cell)
    {
        return row.TryGetValue(MakePropertyTag(propId, propType), out cell) ||
               row.TryGetValue(MakePropertyTagAlt(propId, propType), out cell);
    }

    /// <summary>
    /// Membuat property tag orientasi alternatif (propId pada 16-bit atas) untuk kompatibilitas baseline Outlook.
    /// </summary>
    /// <param name="propId">Property id.</param>
    /// <param name="propType">Property type.</param>
    /// <returns>Property tag alternatif.</returns>
    private static uint MakePropertyTagAlt(ushort propId, PstPropertyType propType)
    {
        return ((uint)propId << 16) | (ushort)propType;
    }

    /// <summary>
    /// Menentukan apakah kolom table mereferensikan property id tertentu pada salah satu orientasi tag.
    /// </summary>
    /// <param name="column">Kolom table.</param>
    /// <param name="propId">Property id target.</param>
    /// <returns>True jika cocok.</returns>
    private static bool ColumnMatchesPropertyId(TableContext.TableColumn column, ushort propId)
    {
        return column.PropId == propId || column.PropType == propId;
    }

    /// <summary>
    /// Menormalkan subject dengan menghapus prefix karakter kontrol non-printable di awal string.
    /// </summary>
    /// <param name="subject">Nilai subject mentah.</param>
    /// <returns>Subject yang sudah dinormalkan.</returns>
    private static string NormalizeSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject))
        {
            return subject;
        }

        var index = 0;
        while (index < subject.Length && char.IsControl(subject[index]))
        {
            index++;
        }

        return index == 0 ? subject : subject[index..];
    }

    /// <summary>
    /// Membuat fallback recipient dari display fields (To/Cc/Bcc) bila recipient table tidak tersedia.
    /// </summary>
    /// <param name="message">Message target.</param>
    private static void PopulateRecipientsFallbackFromDisplayFields(PstMessage message)
    {
        if (message.Recipients.Count > 0)
        {
            return;
        }

        var fallback = new List<PstRecipient>();
        AppendRecipientsFromDisplay(fallback, message.DisplayTo, (int)PstRecipientType.To);
        AppendRecipientsFromDisplay(fallback, message.DisplayCc, (int)PstRecipientType.Cc);
        AppendRecipientsFromDisplay(fallback, message.DisplayBcc, (int)PstRecipientType.Bcc);

        if (fallback.Count > 0)
        {
            message.Recipients = fallback;
        }
    }

    /// <summary>
    /// Menambahkan recipient hasil parsing display string ke daftar fallback.
    /// </summary>
    /// <param name="target">Daftar recipient fallback.</param>
    /// <param name="displayValue">String display recipient.</param>
    /// <param name="recipientType">Jenis recipient (To/Cc/Bcc).</param>
    private static void AppendRecipientsFromDisplay(List<PstRecipient> target, string? displayValue, int recipientType)
    {
        if (string.IsNullOrWhiteSpace(displayValue))
        {
            return;
        }

        var tokens = displayValue.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            target.Add(new PstRecipient
            {
                RecipientType = recipientType,
                EmailAddress = token,
                SmtpAddress = token,
                DisplayName = token,
                AddressType = "SMTP"
            });
        }
    }

    /// <summary>
    /// Membuat Property Context dari NBT entry.
    /// </summary>
    /// <param name="entry">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Property Context terisi.</returns>
    private static PropertyContext CreatePropertyContext(NbtEntry entry, PstBlockReader blockReader, PstFormat format)
    {
        var blocks = blockReader.ReadDataBlocks(entry.BidData);
        var heap = new HeapOnNode(blocks);
        var subnodes = new SubnodeReader(blockReader, format, entry.BidSub);
        return new PropertyContext(heap, subnodes);
    }

    /// <summary>
    /// Membuat Property Context secara asynchronous dari NBT entry.
    /// </summary>
    /// <param name="entry">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Property Context terisi.</returns>
    private static async Task<PropertyContext> CreatePropertyContextAsync(
        NbtEntry entry,
        PstBlockReader blockReader,
        PstFormat format,
        CancellationToken cancellationToken)
    {
        var blocks = await blockReader.ReadDataBlocksAsync(entry.BidData, cancellationToken).ConfigureAwait(false);
        var heap = new HeapOnNode(blocks);
        var subnodes = new SubnodeReader(blockReader, format, entry.BidSub);
        return new PropertyContext(heap, subnodes);
    }
}
