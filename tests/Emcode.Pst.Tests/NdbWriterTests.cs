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
    private const ulong FirstAmapOffset = 0x4400;
    private const ulong AmapIntervalBytes = 253_952;

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
            stream.ReadExactly(buffer, 0, buffer.Length);
            Assert.Equal(data, buffer);

            stream.Seek((long)(allocation.Ib + allocation.BlockSize - 16), SeekOrigin.Begin);
            var trailer = new byte[16];
            stream.ReadExactly(trailer, 0, trailer.Length);

            Assert.Equal((ushort)data.Length, BitConverter.ToUInt16(trailer, 0));
            Assert.Equal(
                NdbIntegrity.ComputeSignature(allocation.Ib, allocation.Bid),
                BitConverter.ToUInt16(trailer, 2));
            Assert.Equal(
                NdbIntegrity.ComputeCrc(0, data),
                BitConverter.ToUInt32(trailer, 4));
            Assert.Equal(allocation.Bid.Raw, BitConverter.ToUInt64(trailer, 8));
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

    /// <summary>
    /// Memastikan penulisan page menambahkan PAGETRAILER yang valid.
    /// </summary>
    [Fact]
    public void NdbBlockWriter_WritePage_ShouldWritePageTrailer()
    {
        var temp = Path.GetTempFileName();
        try
        {
            using var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var header = new PstHeaderInfo(0, 0, 0, 0, stream.Length, PstFormat.Unicode, PstCryptMethod.None);
            var core = new NdbWriterCore(header);
            var writer = new NdbBlockWriter(stream, core, PstCryptMethod.None);

            var page = new byte[512];
            page[0] = 0x11;
            page[100] = 0x7A;
            var allocation = writer.WritePage(page, NdbPageType.Bbt);

            stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
            var buffer = new byte[512];
            stream.ReadExactly(buffer, 0, buffer.Length);

            var trailerOffset = 512 - 16;
            Assert.Equal(NdbPageType.Bbt, buffer[trailerOffset]);
            Assert.Equal(NdbPageType.Bbt, buffer[trailerOffset + 1]);
            Assert.Equal(
                NdbIntegrity.ComputeSignature(allocation.Ib, allocation.Bid),
                BitConverter.ToUInt16(buffer, trailerOffset + 2));
            Assert.Equal(
                NdbIntegrity.ComputeCrc(0, buffer.AsSpan(0, trailerOffset)),
                BitConverter.ToUInt32(buffer, trailerOffset + 4));
            Assert.Equal(allocation.Bid.Raw, BitConverter.ToUInt64(buffer, trailerOffset + 8));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>
    /// Memastikan commit writer memperbarui bit AMap, metadata ROOT, dan transisi fAMapValid.
    /// </summary>
    [Fact]
    public void CommitBtrees_ShouldUpdateAmapAndRootMetadata()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"pst-amap-{Guid.NewGuid():N}.pst");
        new PstNdbWriter().EnsureFileInitialized(temp, new Emcode.Pst.Application.PstOpenOptions { ReadOnly = false });

        try
        {
            using var stream = new FileStream(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var headerReader = new NdbHeaderReader();
            var header = headerReader.Read(stream);
            var btreeReader = new PstBTreeReader(stream, header.HeaderInfo.Format);
            var existingBbt = btreeReader.ReadBbt(header.BbtRoot);
            var existingNbt = btreeReader.ReadNbt(header.NbtRoot);

            var blockCounter = ResolveBidCounter(header.Counters.NextBlockBidRaw);
            var pageCounter = ResolveBidCounter(header.Counters.NextPageBidRaw);
            var writer = new NdbWriter(stream, header.HeaderInfo, blockCounter, pageCounter);
            var entry = writer.WriteExternalBlock(new byte[64]);

            stream.Seek(0xF8, SeekOrigin.Begin);
            Assert.Equal(0x00, stream.ReadByte());

            writer.CommitBtrees(header, existingBbt, existingNbt);

            var updated = headerReader.Read(stream);
            Assert.True(updated.RootState.IsAMapValid);
            Assert.Equal((ulong)stream.Length, updated.RootState.IbFileEof);
            Assert.True(updated.RootState.IbAMapLast >= header.RootState.IbAMapLast);

            var amapOffset = ResolveAmapOffset(entry.Ib);
            stream.Seek((long)amapOffset, SeekOrigin.Begin);
            var amapPage = new byte[512];
            stream.ReadExactly(amapPage, 0, amapPage.Length);

            var bitIndex = (int)((entry.Ib - amapOffset) / 64);
            var expectedMask = 1 << (bitIndex % 8);
            Assert.NotEqual(0, amapPage[bitIndex / 8] & expectedMask);
        }
        finally
        {
            File.Delete(temp);
        }
    }

    private static ulong ResolveAmapOffset(ulong ib)
    {
        if (ib <= FirstAmapOffset)
        {
            return FirstAmapOffset;
        }

        return FirstAmapOffset + (((ib - FirstAmapOffset) / AmapIntervalBytes) * AmapIntervalBytes);
    }

    private static ulong ResolveBidCounter(ulong nextBidRaw)
    {
        return nextBidRaw < 4 ? 0 : (nextBidRaw >> 2) - 1;
    }
}
