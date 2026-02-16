using System;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer core NDB yang bertanggung jawab atas alokasi block dan BID.
/// </summary>
internal sealed class NdbWriterCore
{
    private readonly object _sync = new();
    private readonly ushort _blockSize;
    private long _nextOffset;
    private long _bidCounter;

    /// <summary>
    /// Membuat writer core berdasarkan metadata header PST.
    /// </summary>
    /// <param name="headerInfo">Metadata header PST.</param>
    /// <param name="initialOffset">Offset awal untuk alokasi block; default dari ukuran file.</param>
    /// <param name="initialBidCounter">Counter awal BID (untuk melanjutkan alokasi).</param>
    public NdbWriterCore(PstHeaderInfo headerInfo, ulong? initialOffset = null, ulong? initialBidCounter = null)
    {
        ArgumentNullException.ThrowIfNull(headerInfo);
        if (headerInfo.Format == PstFormat.Unknown)
        {
            throw new ArgumentException("Format PST belum terdeteksi.", nameof(headerInfo));
        }

        _blockSize = ResolveBlockSize(headerInfo.Format);
        var startOffset = initialOffset ?? (ulong)headerInfo.FileSize;
        _nextOffset = (long)AlignToBlock(startOffset, _blockSize);
        _bidCounter = (long)(initialBidCounter ?? 0);
    }

    /// <summary>
    /// Ukuran block PST yang digunakan untuk alokasi.
    /// </summary>
    public ushort BlockSize => _blockSize;

    /// <summary>
    /// Mengalokasikan block eksternal untuk data.
    /// </summary>
    /// <param name="dataSize">Ukuran data yang akan ditulis.</param>
    /// <returns>Metadata alokasi block.</returns>
    public NdbBlockAllocation AllocateExternalBlock(ushort dataSize)
    {
        return AllocateBlock(dataSize, isInternal: false);
    }

    /// <summary>
    /// Mengalokasikan block internal (XBLOCK/XXBLOCK).
    /// </summary>
    /// <param name="dataSize">Ukuran data yang akan ditulis.</param>
    /// <returns>Metadata alokasi block.</returns>
    public NdbBlockAllocation AllocateInternalBlock(ushort dataSize)
    {
        return AllocateBlock(dataSize, isInternal: true);
    }

    /// <summary>
    /// Mengalokasikan block eksternal secara asynchronous.
    /// </summary>
    /// <param name="dataSize">Ukuran data yang akan ditulis.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Metadata alokasi block.</returns>
    public Task<NdbBlockAllocation> AllocateExternalBlockAsync(ushort dataSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AllocateExternalBlock(dataSize));
    }

    /// <summary>
    /// Mengalokasikan block internal secara asynchronous.
    /// </summary>
    /// <param name="dataSize">Ukuran data yang akan ditulis.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Metadata alokasi block.</returns>
    public Task<NdbBlockAllocation> AllocateInternalBlockAsync(ushort dataSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AllocateInternalBlock(dataSize));
    }

    private NdbBlockAllocation AllocateBlock(ushort dataSize, bool isInternal)
    {
        if (dataSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataSize), "Ukuran data harus lebih besar dari 0.");
        }

        if (dataSize > _blockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(dataSize), "Ukuran data melebihi kapasitas block PST.");
        }

        lock (_sync)
        {
            var bid = AllocateBid(isInternal);
            var ib = (ulong)_nextOffset;
            _nextOffset += _blockSize;
            return new NdbBlockAllocation(bid, ib, dataSize, _blockSize, isInternal);
        }
    }

    private Bid AllocateBid(bool isInternal)
    {
        var counter = ++_bidCounter;
        var raw = ((ulong)counter << 2);
        if (isInternal)
        {
            raw |= 0x2;
        }

        return new Bid(raw);
    }

    private static ushort ResolveBlockSize(PstFormat format)
    {
        return format switch
        {
            PstFormat.Ansi => 512,
            PstFormat.Unicode => 8192,
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Format PST tidak didukung.")
        };
    }

    private static ulong AlignToBlock(ulong offset, ushort blockSize)
    {
        var size = (ulong)blockSize;
        var remainder = offset % size;
        return remainder == 0 ? offset : offset + (size - remainder);
    }
}
