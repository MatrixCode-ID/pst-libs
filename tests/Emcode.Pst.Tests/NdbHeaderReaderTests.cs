using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian parser header NDB pada sample PST.
/// </summary>
public sealed class NdbHeaderReaderTests
{
    /// <summary>
    /// Memastikan parser header membaca format dan crypt method dengan benar.
    /// </summary>
    [Fact]
    public void Read_Sample1_ReturnsExpectedMetadata()
    {
        using var stream = File.OpenRead(TestData.Sample1Path);
        var header = new NdbHeaderReader().Read(stream);

        Assert.Equal(PstFormat.Unicode, header.HeaderInfo.Format);
        Assert.Equal(PstCryptMethod.Permute, header.HeaderInfo.CryptMethod);
        Assert.True(header.BbtRoot.Ib > 0);
        Assert.True(header.NbtRoot.Ib > 0);
    }
}
