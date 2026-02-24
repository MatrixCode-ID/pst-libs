using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer blok data NDB yang mengalokasikan block dan menulis ke stream.
/// </summary>
internal sealed class NdbBlockWriter
{
    private readonly Stream _stream;
    private readonly NdbWriterCore _core;
    private readonly PstCryptMethod _cryptMethod;

    /// <summary>
    /// Membuat writer blok NDB.
    /// </summary>
    /// <param name="stream">Stream PST.</param>
    /// <param name="core">Writer core untuk alokasi BID/IB.</param>
    public NdbBlockWriter(Stream stream, NdbWriterCore core, PstCryptMethod cryptMethod)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _core = core ?? throw new ArgumentNullException(nameof(core));
        _cryptMethod = cryptMethod;
    }

    /// <summary>
    /// Stream PST yang digunakan writer.
    /// </summary>
    internal Stream Stream => _stream;

    /// <summary>
    /// Menulis data ke block eksternal.
    /// </summary>
    /// <param name="data">Data yang akan ditulis.</param>
    /// <returns>Metadata alokasi block.</returns>
    public NdbBlockAllocation WriteExternalBlock(ReadOnlySpan<byte> data, bool encode = true)
    {
        return WriteBlock(data, isInternal: false, encode);
    }

    /// <summary>
    /// Menulis data ke block internal.
    /// </summary>
    /// <param name="data">Data yang akan ditulis.</param>
    /// <returns>Metadata alokasi block.</returns>
    public NdbBlockAllocation WriteInternalBlock(ReadOnlySpan<byte> data)
    {
        return WriteBlock(data, isInternal: true, encode: false);
    }

    /// <summary>
    /// Menulis page metadata berukuran 512 byte dengan PAGETRAILER terinisialisasi.
    /// </summary>
    /// <param name="page">Buffer page 512 byte.</param>
    /// <param name="pageType">Nilai ptype page.</param>
    /// <returns>Metadata alokasi page.</returns>
    public NdbBlockAllocation WritePage(ReadOnlySpan<byte> page, byte pageType)
    {
        if (page.Length != _core.PageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Ukuran page harus 512 byte.");
        }

        var allocation = _core.AllocatePage();
        var buffer = page.ToArray();
        var trailerSize = _core.BlockTrailerSize;
        var trailerOffset = buffer.Length - trailerSize;
        var crc = NdbIntegrity.ComputeCrc(0, buffer.AsSpan(0, trailerOffset));
        var signature = NdbIntegrity.ComputeSignature(allocation.Ib, allocation.Bid);
        NdbIntegrity.WritePageTrailer(
            buffer.AsSpan(trailerOffset, trailerSize),
            ResolveFormat(),
            pageType,
            crc,
            signature,
            allocation.Bid);

        _stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
        _stream.Write(buffer, 0, buffer.Length);
        return allocation;
    }

    /// <summary>
    /// Menulis data ke block eksternal secara asynchronous.
    /// </summary>
    /// <param name="data">Data yang akan ditulis.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Metadata alokasi block.</returns>
    public Task<NdbBlockAllocation> WriteExternalBlockAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        return WriteBlockAsync(data, isInternal: false, encode: true, cancellationToken);
    }

    /// <summary>
    /// Menulis data ke block internal secara asynchronous.
    /// </summary>
    /// <param name="data">Data yang akan ditulis.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Metadata alokasi block.</returns>
    public Task<NdbBlockAllocation> WriteInternalBlockAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        return WriteBlockAsync(data, isInternal: true, encode: false, cancellationToken);
    }

    private NdbBlockAllocation WriteBlock(ReadOnlySpan<byte> data, bool isInternal, bool encode)
    {
        if (data.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data block tidak boleh kosong.");
        }

        if (data.Length > _core.MaxBlockDataSize)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data melebihi kapasitas payload block.");
        }

        var allocation = isInternal
            ? _core.AllocateInternalBlock((ushort)data.Length)
            : _core.AllocateExternalBlock((ushort)data.Length);

        var format = ResolveFormat();
        var trailerSize = _core.BlockTrailerSize;
        var blockBuffer = new byte[allocation.BlockSize];
        data.CopyTo(blockBuffer.AsSpan(0, data.Length));
        if (!isInternal && encode)
        {
            NdbCrypt.Encode(_cryptMethod, blockBuffer.AsSpan(0, data.Length));
        }

        var signature = NdbIntegrity.ComputeSignature(allocation.Ib, allocation.Bid);
        var crc = NdbIntegrity.ComputeCrc(0, data);
        var trailerOffset = allocation.BlockSize - trailerSize;
        NdbIntegrity.WriteBlockTrailer(
            blockBuffer.AsSpan(trailerOffset, trailerSize),
            format,
            (ushort)data.Length,
            crc,
            signature,
            allocation.Bid);

        _stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
        _stream.Write(blockBuffer, 0, blockBuffer.Length);

        return allocation;
    }

    private async Task<NdbBlockAllocation> WriteBlockAsync(ReadOnlyMemory<byte> data, bool isInternal, bool encode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (data.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data block tidak boleh kosong.");
        }

        if (data.Length > _core.MaxBlockDataSize)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data melebihi kapasitas payload block.");
        }

        var allocation = isInternal
            ? _core.AllocateInternalBlock((ushort)data.Length)
            : _core.AllocateExternalBlock((ushort)data.Length);

        var format = ResolveFormat();
        var trailerSize = _core.BlockTrailerSize;
        var blockBuffer = new byte[allocation.BlockSize];
        data.Span.CopyTo(blockBuffer.AsSpan(0, data.Length));
        if (!isInternal && encode)
        {
            NdbCrypt.Encode(_cryptMethod, blockBuffer.AsSpan(0, data.Length));
        }

        var signature = NdbIntegrity.ComputeSignature(allocation.Ib, allocation.Bid);
        var crc = NdbIntegrity.ComputeCrc(0, data.Span);
        var trailerOffset = allocation.BlockSize - trailerSize;
        NdbIntegrity.WriteBlockTrailer(
            blockBuffer.AsSpan(trailerOffset, trailerSize),
            format,
            (ushort)data.Length,
            crc,
            signature,
            allocation.Bid);

        _stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
        await _stream.WriteAsync(blockBuffer, cancellationToken).ConfigureAwait(false);

        return allocation;
    }

    private PstFormat ResolveFormat()
    {
        return _core.BlockSize == 8192 ? PstFormat.Unicode : PstFormat.Ansi;
    }
}
