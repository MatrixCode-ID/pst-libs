using System;
using System.IO;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian writer NDB untuk alokasi block dan BBT in-memory.
/// </summary>
public sealed class NdbWriterTests
{
    /// <summary>
    /// Memastikan block writer menulis data dan mengembalikan alokasi.
    /// </summary>
    [Fact]
    public void NdbBlockWriter_ShouldWriteBlock()
    {
        var temp = Path.GetTempFileName();
        try
        {
            using var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var header = new PstHeaderInfo(0, 0, 0, 0, stream.Length, PstFormat.Unicode, PstCryptMethod.None);
            var core = new NdbWriterCore(header);
            var writer = new NdbBlockWriter(stream, core, PstCryptMethod.None);

            var data = new byte[] { 1, 2, 3, 4 };
            var allocation = writer.WriteExternalBlock(data);

            Assert.False(allocation.Bid.IsInternal);
            Assert.Equal((ushort)8192, allocation.BlockSize);
            stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
            var buffer = new byte[data.Length];
            stream.Read(buffer, 0, buffer.Length);
            Assert.Equal(data, buffer);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>
    /// Memastikan NdbWriter mengembalikan snapshot BBT setelah menulis block.
    /// </summary>
    [Fact]
    public void NdbWriter_ShouldCreateBbtSnapshot()
    {
        var temp = Path.GetTempFileName();
        try
        {
            using var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var header = new PstHeaderInfo(0, 0, 0, 0, stream.Length, PstFormat.Ansi, PstCryptMethod.None);
            var writer = new NdbWriter(stream, header);
            var entry = writer.WriteExternalBlock(new byte[] { 1, 2 });

            var snapshot = writer.SnapshotBbt();
            Assert.True(snapshot.ContainsKey(entry.Bid.NormalizeForLookup()));
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
