using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ltp;
using Emcode.Pst.Infrastructure.Ndb;
using Xunit;

namespace Emcode.Pst.Tests;

/// <summary>
/// Pengujian writer LTP untuk Property Context dan Table Row.
/// </summary>
public sealed class LtpWriterTests
{
    /// <summary>
    /// Memastikan Property Context writer menghasilkan data yang bisa dibaca ulang.
    /// </summary>
    [Fact]
    public void PropertyContextWriter_ShouldRoundtripValues()
    {
        var options = LtpWriterOptions.CreateDefault(PstFormat.Unicode);
        var writer = new PropertyContextWriter(options);
        writer.SetString(0x0037, "Hello");
        writer.SetInt32(0x0E08, 42);
        writer.SetBoolean(0x0E07, true);
        writer.SetDateTime(0x0E06, DateTimeOffset.FromUnixTimeSeconds(1));
        writer.SetBinary(0x3701, new byte[] { 1, 2, 3 });

        var blocks = writer.Build();
        var pc = CreatePropertyContext(blocks, options.Format);

        Assert.Equal("Hello", pc.GetString(0x0037));
        Assert.Equal(42, pc.GetInt32(0x0E08));
        Assert.True(pc.GetBoolean(0x0E07));
        Assert.NotNull(pc.GetDateTime(0x0E06));
        Assert.True(pc.GetBinary(0x3701)?.ToArray().SequenceEqual(new byte[] { 1, 2, 3 }));
    }

    /// <summary>
    /// Memastikan Table Row writer menghasilkan row yang bisa dibaca ulang.
    /// </summary>
    [Fact]
    public void TableRowWriter_ShouldRoundtripRow()
    {
        var options = LtpWriterOptions.CreateDefault(PstFormat.Unicode);
        var writer = new TableRowWriter(options);
        writer.AddColumn(0x3001, PstPropertyType.String, 4, 4, 0);
        writer.AddColumn(0x0E08, PstPropertyType.Integer32, 8, 4, 1);

        writer.AddRow(100, new TableRowWriter.TableCell(0x3001, PstPropertyType.String, "Alice"),
            new TableRowWriter.TableCell(0x0E08, PstPropertyType.Integer32, 77));

        var blocks = writer.Build();
        var table = CreateTableContext(blocks, options.Format);
        var row = table.ReadRows().Single();
        Assert.Equal((uint)100, row.RowId);

        Assert.True(row.TryGetValue(((uint)PstPropertyType.String << 16) | 0x3001, out var nameCell));
        var name = Encoding.Unicode.GetString(nameCell.Data.Span).TrimEnd('\0');
        Assert.Equal("Alice", name);

        Assert.True(row.TryGetValue(((uint)PstPropertyType.Integer32 << 16) | 0x0E08, out var intCell));
        Assert.Equal(77, BitConverter.ToInt32(intCell.Data.Span));
    }

    /// <summary>
    /// Memastikan BuildAsync berfungsi untuk Property Context writer.
    /// </summary>
    [Fact]
    public async Task PropertyContextWriter_BuildAsync_ShouldReturnBlocks()
    {
        var options = LtpWriterOptions.CreateDefault(PstFormat.Ansi);
        var writer = new PropertyContextWriter(options);
        writer.SetString8(0x0037, "Async");

        var blocks = await writer.BuildAsync();

        Assert.NotEmpty(blocks);
    }

    /// <summary>
    /// Memastikan Property Context writer fallback ke subnode saat heap inline melebihi kapasitas block.
    /// </summary>
    [Fact]
    public void PropertyContextWriter_BuildResult_ShouldFallbackToSubnodesWhenHeapOverflows()
    {
        var options = LtpWriterOptions.CreateDefault(PstFormat.Unicode);
        var writer = new PropertyContextWriter(options);
        var text = new string('X', 1500);
        writer.SetString(0x1000, text);
        writer.SetString(0x1013, text);
        writer.SetString(0x007D, text);

        var result = writer.BuildResult();

        Assert.NotEmpty(result.Blocks);
        Assert.True(result.Subnodes.Count > 0);
    }

    private static PropertyContext CreatePropertyContext(IReadOnlyList<PstDataBlock> blocks, PstFormat format)
    {
        var heap = new HeapOnNode(blocks);
        var subnodes = CreateSubnodeReader(format);
        return new PropertyContext(heap, subnodes);
    }

    private static TableContext CreateTableContext(IReadOnlyList<PstDataBlock> blocks, PstFormat format)
    {
        var heap = new HeapOnNode(blocks);
        var subnodes = CreateSubnodeReader(format);
        return new TableContext(heap, subnodes);
    }

    private static SubnodeReader CreateSubnodeReader(PstFormat format)
    {
        var stream = new MemoryStream();
        var blockReader = new PstBlockReader(stream, format, PstCryptMethod.None, new Dictionary<ulong, BbtEntry>());
        return new SubnodeReader(blockReader, format, new Bid(0));
    }
}
