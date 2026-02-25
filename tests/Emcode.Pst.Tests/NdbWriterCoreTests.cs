using System.Threading.Tasks;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian alokasi block/BID pada writer core.
/// </summary>
public sealed class NdbWriterCoreTests
{
    /// <summary>
    /// Memastikan alokasi block menjaga alignment dan offset bertambah.
    /// </summary>
    [Fact]
    public void AllocateExternalBlock_ShouldAlignAndIncrement()
    {
        var header = new PstHeaderInfo(0, 0, 0, 0, 1000, PstFormat.Unicode, PstCryptMethod.None);
        var writer = new NdbWriterCore(header);

        var first = writer.AllocateExternalBlock(100);
        var second = writer.AllocateExternalBlock(200);

        Assert.Equal((ulong)8192, first.Ib);
        Assert.Equal((ulong)16384, second.Ib);
        Assert.Equal((ushort)8192, first.BlockSize);
        Assert.Equal((ushort)100, first.DataSize);
    }

    /// <summary>
    /// Memastikan alokasi internal memberi flag internal pada BID.
    /// </summary>
    [Fact]
    public void AllocateInternalBlock_ShouldSetInternalFlag()
    {
        var header = new PstHeaderInfo(0, 0, 0, 0, 0, PstFormat.Ansi, PstCryptMethod.None);
        var writer = new NdbWriterCore(header);

        var internalBlock = writer.AllocateInternalBlock(20);
        var externalBlock = writer.AllocateExternalBlock(20);

        Assert.True(internalBlock.Bid.IsInternal);
        Assert.False(externalBlock.Bid.IsInternal);
    }

    /// <summary>
    /// Memastikan alokasi async menghasilkan metadata yang sama.
    /// </summary>
    [Fact]
    public async Task AllocateExternalBlockAsync_ShouldReturnAllocation()
    {
        var header = new PstHeaderInfo(0, 0, 0, 0, 0, PstFormat.Ansi, PstCryptMethod.None);
        var writer = new NdbWriterCore(header);

        var allocation = await writer.AllocateExternalBlockAsync(64);

        Assert.False(allocation.Bid.IsInternal);
        Assert.Equal((ushort)512, allocation.BlockSize);
    }

    /// <summary>
    /// Memastikan allocator memprioritaskan free-space reuse sebelum append ke EOF.
    /// </summary>
    [Fact]
    public void AllocateExternalBlock_ShouldReuseFreeRangeBeforeEof()
    {
        var header = new PstHeaderInfo(0, 0, 0, 0, 4_194_304, PstFormat.Unicode, PstCryptMethod.None);
        var freeRanges = new[] { new NdbAllocationRange(0x0004_4000, 32_768) };
        var writer = new NdbWriterCore(header, freeRanges: freeRanges);

        var first = writer.AllocateExternalBlock(100);
        var second = writer.AllocateExternalBlock(100);

        Assert.Equal(0x0004_4000UL, first.Ib);
        Assert.Equal(0x0004_6000UL, second.Ib);
    }

    /// <summary>
    /// Memastikan range occupied tidak dipakai walaupun muncul sebagai free-range candidate.
    /// </summary>
    [Fact]
    public void AllocateExternalBlock_ShouldSkipOccupiedRange()
    {
        var header = new PstHeaderInfo(0, 0, 0, 0, 4_194_304, PstFormat.Unicode, PstCryptMethod.None);
        var freeRanges = new[] { new NdbAllocationRange(0x0004_4000, 32_768) };
        var occupiedRanges = new[] { new NdbAllocationRange(0x0004_6000, 8_192) };
        var writer = new NdbWriterCore(header, freeRanges: freeRanges, occupiedRanges: occupiedRanges);

        var allocation = writer.AllocateExternalBlock(100);

        Assert.Equal(0x0004_4000UL, allocation.Ib);
    }
}
