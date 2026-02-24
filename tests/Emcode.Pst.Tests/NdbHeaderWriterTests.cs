using System;
using System.IO;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian pembaruan metadata header NDB.
/// </summary>
public sealed class NdbHeaderWriterTests
{
    /// <summary>
    /// Memastikan update counter BID dan CRC header menulis nilai yang konsisten.
    /// </summary>
    [Fact]
    public void UpdateBidCountersAndCrc_ShouldWriteExpectedValues()
    {
        var temp = Path.GetTempFileName();
        try
        {
            using var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            stream.SetLength(8192);

            var writer = new NdbHeaderWriter(stream);
            const ulong nextBlockBid = 0x0000000000000300;
            const ulong nextPageBid = 0x0000000000000200;
            const ulong fileSize = 0x0000000000004000;

            writer.UpdateBidCounters(PstFormat.Unicode, nextBlockBid, nextPageBid);
            writer.UpdateFileSizeOnDisk(PstFormat.Unicode, fileSize);
            writer.UpdateHeaderCrcs(PstFormat.Unicode);

            stream.Seek(0, SeekOrigin.Begin);
            var header = new byte[0x220];
            stream.ReadExactly(header, 0, header.Length);

            Assert.Equal(nextPageBid, BitConverter.ToUInt64(header, 0x20));
            Assert.Equal((uint)1, BitConverter.ToUInt32(header, 0x28));
            Assert.Equal(fileSize, BitConverter.ToUInt64(header, 0xB8));
            Assert.Equal(nextBlockBid, BitConverter.ToUInt64(header, 0x204));

            var partialExpected = NdbIntegrity.ComputeCrc(0, header.AsSpan(0x08, 471));
            var fullExpected = NdbIntegrity.ComputeCrc(0, header.AsSpan(0x08, 516));
            Assert.Equal(partialExpected, BitConverter.ToUInt32(header, 0x04));
            Assert.Equal(fullExpected, BitConverter.ToUInt32(header, 0x20C));
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
