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

        if (data.Length > _core.BlockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data melebihi ukuran block.");
        }

        var allocation = isInternal
            ? _core.AllocateInternalBlock((ushort)data.Length)
            : _core.AllocateExternalBlock((ushort)data.Length);

        _stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
        if (!isInternal && encode)
        {
            var encoded = data.ToArray();
            NdbCrypt.Encode(_cryptMethod, encoded);
            _stream.Write(encoded);
        }
        else
        {
            _stream.Write(data);
        }
        var padding = allocation.BlockSize - data.Length;
        if (padding > 0)
        {
            _stream.Write(new byte[padding]);
        }

        return allocation;
    }

    private async Task<NdbBlockAllocation> WriteBlockAsync(ReadOnlyMemory<byte> data, bool isInternal, bool encode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (data.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data block tidak boleh kosong.");
        }

        if (data.Length > _core.BlockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "Data melebihi ukuran block.");
        }

        var allocation = isInternal
            ? _core.AllocateInternalBlock((ushort)data.Length)
            : _core.AllocateExternalBlock((ushort)data.Length);

        _stream.Seek((long)allocation.Ib, SeekOrigin.Begin);
        if (!isInternal && encode)
        {
            var encoded = data.ToArray();
            NdbCrypt.Encode(_cryptMethod, encoded);
            await _stream.WriteAsync(encoded, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }
        var padding = allocation.BlockSize - data.Length;
        if (padding > 0)
        {
            await _stream.WriteAsync(new byte[padding], cancellationToken).ConfigureAwait(false);
        }

        return allocation;
    }
}
