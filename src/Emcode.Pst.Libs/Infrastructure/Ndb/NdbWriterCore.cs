using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Writer core NDB yang bertanggung jawab atas alokasi block dan BID.
/// </summary>
internal sealed class NdbWriterCore
{
    private const ulong FirstAmapOffset = 0x4400;
    private const ulong AmapIntervalBytes = 253_952;
    private const ushort AmapPageSize = 512;
    private readonly object _sync = new();
    private readonly ushort _blockSize;
    private readonly ushort _pageSize;
    private readonly ushort _blockTrailerSize;
    private readonly List<NdbAllocationRange> _allocationRanges = new();
    private long _nextOffset;
    private long _blockBidCounter;
    private long _pageBidCounter;

    /// <summary>
    /// Membuat writer core berdasarkan metadata header PST.
    /// </summary>
    /// <param name="headerInfo">Metadata header PST.</param>
    /// <param name="initialOffset">Offset awal untuk alokasi block; default dari ukuran file.</param>
    /// <param name="initialBlockBidCounter">Counter awal BID block (untuk melanjutkan alokasi).</param>
    /// <param name="initialPageBidCounter">Counter awal BID page (untuk melanjutkan alokasi).</param>
    public NdbWriterCore(
        PstHeaderInfo headerInfo,
        ulong? initialOffset = null,
        ulong? initialBlockBidCounter = null,
        ulong? initialPageBidCounter = null)
    {
        ArgumentNullException.ThrowIfNull(headerInfo);
        if (headerInfo.Format == PstFormat.Unknown)
        {
            throw new ArgumentException("Format PST belum terdeteksi.", nameof(headerInfo));
        }

        _blockSize = ResolveBlockSize(headerInfo.Format);
        _pageSize = 512;
        _blockTrailerSize = ResolveBlockTrailerSize(headerInfo.Format);
        var startOffset = initialOffset ?? (ulong)headerInfo.FileSize;
        _nextOffset = (long)AlignToBlock(startOffset, _blockSize);
        _blockBidCounter = (long)(initialBlockBidCounter ?? 0);
        _pageBidCounter = (long)(initialPageBidCounter ?? initialBlockBidCounter ?? 0);
    }

    /// <summary>
    /// Ukuran block PST yang digunakan untuk alokasi.
    /// </summary>
    public ushort BlockSize => _blockSize;

    /// <summary>
    /// Ukuran page metadata PST.
    /// </summary>
    public ushort PageSize => _pageSize;

    /// <summary>
    /// Ukuran trailer pada data block.
    /// </summary>
    public ushort BlockTrailerSize => _blockTrailerSize;

    /// <summary>
    /// Kapasitas payload maksimum data block (tanpa trailer).
    /// </summary>
    public ushort MaxBlockDataSize => (ushort)(_blockSize - _blockTrailerSize);

    /// <summary>
    /// Snapshot range alokasi block/page yang terjadi pada sesi writer ini.
    /// </summary>
    public IReadOnlyList<NdbAllocationRange> SnapshotAllocationRanges()
    {
        lock (_sync)
        {
            return _allocationRanges.ToArray();
        }
    }

    /// <summary>
    /// Nilai BID block berikutnya yang harus dipakai.
    /// </summary>
    public ulong NextBlockBidRaw => ((ulong)(_blockBidCounter + 1) << 2);

    /// <summary>
    /// Nilai BID page berikutnya yang harus dipakai.
    /// </summary>
    public ulong NextPageBidRaw => ((ulong)(_pageBidCounter + 1) << 2);

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
    /// Mengalokasikan page metadata berukuran 512 byte.
    /// </summary>
    /// <returns>Metadata alokasi page.</returns>
    public NdbBlockAllocation AllocatePage()
    {
        lock (_sync)
        {
            var bid = AllocatePageBid();
            var alignedOffset = ResolveAllocationOffset((ulong)_nextOffset, _pageSize, _pageSize);
            var ib = alignedOffset;
            _nextOffset = (long)(alignedOffset + _pageSize);
            var allocation = new NdbBlockAllocation(bid, ib, _pageSize, _pageSize, isInternal: false);
            _allocationRanges.Add(new NdbAllocationRange(allocation.Ib, allocation.BlockSize));
            return allocation;
        }
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

        if (dataSize > MaxBlockDataSize)
        {
            throw new ArgumentOutOfRangeException(nameof(dataSize), "Ukuran data melebihi kapasitas payload block PST.");
        }

        lock (_sync)
        {
            var bid = AllocateBid(isInternal);
            var alignedOffset = ResolveAllocationOffset((ulong)_nextOffset, _blockSize, _blockSize);
            var ib = alignedOffset;
            _nextOffset = (long)(alignedOffset + _blockSize);
            var allocation = new NdbBlockAllocation(bid, ib, dataSize, _blockSize, isInternal);
            _allocationRanges.Add(new NdbAllocationRange(allocation.Ib, allocation.BlockSize));
            return allocation;
        }
    }

    private Bid AllocateBid(bool isInternal)
    {
        var counter = ++_blockBidCounter;
        var raw = ((ulong)counter << 2);
        if (isInternal)
        {
            raw |= 0x2;
        }

        return new Bid(raw);
    }

    private Bid AllocatePageBid()
    {
        var counter = ++_pageBidCounter;
        var raw = ((ulong)counter << 2);
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

    private static ushort ResolveBlockTrailerSize(PstFormat format)
    {
        return format switch
        {
            PstFormat.Ansi => 12,
            PstFormat.Unicode => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Format PST tidak didukung.")
        };
    }

    private static ulong AlignToBlock(ulong offset, ushort blockSize)
    {
        var size = (ulong)blockSize;
        var remainder = offset % size;
        return remainder == 0 ? offset : offset + (size - remainder);
    }

    private static ulong ResolveSectionStart(ulong offset)
    {
        if (offset <= FirstAmapOffset)
        {
            return FirstAmapOffset;
        }

        var delta = offset - FirstAmapOffset;
        return FirstAmapOffset + ((delta / AmapIntervalBytes) * AmapIntervalBytes);
    }

    private static bool IsAmapSection(ulong offset)
    {
        return offset >= FirstAmapOffset;
    }

    private static bool OverlapsAmapPage(ulong offset, ulong length)
    {
        if (!IsAmapSection(offset))
        {
            return false;
        }

        var sectionStart = ResolveSectionStart(offset);
        var mapEnd = sectionStart + AmapPageSize;
        return offset < mapEnd && (offset + length) > sectionStart;
    }

    private static bool OverflowsSection(ulong offset, ulong length)
    {
        if (!IsAmapSection(offset))
        {
            return false;
        }

        var sectionStart = ResolveSectionStart(offset);
        var sectionEnd = sectionStart + AmapIntervalBytes;
        return offset + length > sectionEnd;
    }

    private static ulong GetNextSectionDataStart(ulong offset)
    {
        var sectionStart = ResolveSectionStart(offset);
        var nextSectionStart = sectionStart + AmapIntervalBytes;
        return nextSectionStart + AmapPageSize;
    }

    private ulong ResolveAllocationOffset(ulong startOffset, ushort alignment, ushort allocationSize)
    {
        var candidate = AlignToBlock(startOffset, alignment);
        while (true)
        {
            if (!IsAmapSection(candidate))
            {
                return candidate;
            }

            if (OverlapsAmapPage(candidate, allocationSize))
            {
                var sectionStart = ResolveSectionStart(candidate);
                candidate = AlignToBlock(sectionStart + AmapPageSize, alignment);
                continue;
            }

            if (OverflowsSection(candidate, allocationSize))
            {
                candidate = AlignToBlock(GetNextSectionDataStart(candidate), alignment);
                continue;
            }

            return candidate;
        }
    }
}
