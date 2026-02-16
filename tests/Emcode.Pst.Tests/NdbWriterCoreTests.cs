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
}
