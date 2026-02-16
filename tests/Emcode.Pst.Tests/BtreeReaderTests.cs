using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian pembacaan BBT/NBT dari sample PST.
/// </summary>
public sealed class BtreeReaderTests
{
    /// <summary>
    /// Memastikan BBT dan NBT dapat dibaca dan berisi entri.
    /// </summary>
    [Fact]
    public void ReadBtrees_Sample1_ReturnsEntries()
    {
        using var stream = File.OpenRead(TestData.Sample1Path);
        var header = new NdbHeaderReader().Read(stream);
        var reader = new PstBTreeReader(stream, header.HeaderInfo.Format);
        var bbt = reader.ReadBbt(header.BbtRoot);
        var nbt = reader.ReadNbt(header.NbtRoot);

        Assert.NotEmpty(bbt);
        Assert.NotEmpty(nbt);
    }
}
