using System;
using System.Collections.Generic;
using System.IO;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian update BBT/NBT pada file PST.
/// </summary>
public sealed class NdbBtreeUpdateTests
{
    /// <summary>
    /// Memastikan BBT dapat diupdate dan entry baru terbaca ulang.
    /// </summary>
    [Fact]
    public void UpdateBbt_ShouldPersistNewEntry()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-bbt-{Guid.NewGuid():N}.pst");
        File.Copy(TestData.Sample1Path, temp, true);

        try
        {
            using var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var headerReader = new NdbHeaderReader();
            var header = headerReader.Read(stream);

            var btreeReader = new PstBTreeReader(stream, header.HeaderInfo.Format);
            var existingBbt = btreeReader.ReadBbt(header.BbtRoot);
            var existingNbt = btreeReader.ReadNbt(header.NbtRoot);

            var maxBidCounter = 0UL;
            foreach (var item in existingBbt.Values)
            {
                var counter = item.Bid.Raw >> 2;
                if (counter > maxBidCounter)
                {
                    maxBidCounter = counter;
                }
            }

            var writer = new NdbWriter(stream, header.HeaderInfo, maxBidCounter);
            var entry = writer.WriteExternalBlock(new byte[] { 9, 9, 9 });
            writer.CommitBtrees(header, existingBbt, existingNbt);

            var updatedHeader = headerReader.Read(stream);
            var updatedBbt = btreeReader.ReadBbt(updatedHeader.BbtRoot);
            Assert.True(updatedBbt.ContainsKey(entry.Bid.NormalizeForLookup()));
            var updatedEntry = updatedBbt[entry.Bid.NormalizeForLookup()];
            Assert.Equal(entry.Ib, updatedEntry.Ib);
            Assert.Equal(entry.Cb, updatedEntry.Cb);

            var blockReader = new PstBlockReader(stream, updatedHeader.HeaderInfo.Format, updatedHeader.HeaderInfo.CryptMethod, updatedBbt);
            var blocks = blockReader.ReadDataBlocks(entry.Bid);
            Assert.Equal(9, blocks[0].Data.Span[0]);
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
