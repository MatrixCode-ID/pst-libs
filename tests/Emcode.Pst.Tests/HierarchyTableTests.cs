using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ltp;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian hierarchy table dan urutan subfolder.
/// </summary>
public sealed class HierarchyTableTests
{
    /// <summary>
    /// Memastikan urutan subfolder mengikuti row matrix pada hierarchy table.
    /// </summary>
    [Fact]
    public void HierarchyTable_OrderMatchesFolderSubfolders()
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

            var ids = ReadHierarchyTableRowIds(entry, nbt, blockReader, header.HeaderInfo.Format);
            if (ids.Count == 0)
            {
                continue;
            }

            var expected = ids
                .Where(id => nbt.TryGetValue(id, out var child)
                    && child.Nid.Type == NidType.NormalFolder
                    && child.NidParent.Value == entry.Nid.Value)
                .ToList();

            if (expected.Count == 0)
            {
                continue;
            }

            targetFolder = entry;
            rowIds = expected;
            break;
        }

        Assert.NotNull(targetFolder);
        Assert.NotEmpty(rowIds);

        using var pst = PstFile.Open(TestData.Sample1Path, new PstOpenOptions
        {
            ReadOnly = true,
            ValidateChecksums = false
        });

        var folder = pst.Folders.First(f => f.Id == targetFolder!.Nid.ToString());
        var actualIds = folder.SubFolders.Select(subfolder => subfolder.Id).ToList();

        var expectedIds = rowIds.Select(id => new Nid(id).ToString()).ToList();
        var expectedSet = new HashSet<string>(expectedIds);
        var filteredActual = actualIds.Where(id => expectedSet.Contains(id)).ToList();

        Assert.Equal(expectedIds, filteredActual);
    }

    /// <summary>
    /// Membaca row ID hierarchy table untuk folder tertentu.
    /// </summary>
    /// <param name="folderEntry">Entri folder.</param>
    /// <param name="nbtEntries">Entri NBT.</param>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <returns>Daftar row ID.</returns>
    private static IReadOnlyList<uint> ReadHierarchyTableRowIds(
        NbtEntry folderEntry,
        IReadOnlyDictionary<uint, NbtEntry> nbtEntries,
        PstBlockReader blockReader,
        PstFormat format)
    {
        var index = folderEntry.Nid.Index;
        var hierarchyNidValue = (index << 5) | (uint)NidType.HierarchyTable;
        if (!nbtEntries.TryGetValue(hierarchyNidValue, out var tableEntry))
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
}
