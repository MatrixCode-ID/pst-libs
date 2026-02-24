using System;
using System.Collections.Generic;
using System.IO;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Menangani pembaruan bit Allocation Map (AMap) berdasarkan range alokasi block/page.
/// </summary>
internal sealed class NdbAllocationMapWriter
{
    private const ulong FirstAmapOffset = 0x4400;
    private const ulong AmapIntervalBytes = 253_952;
    private const int AmapPageSize = 512;
    private const int AmapDataBytes = 496;
    private const int AmapUnitBytes = 64;
    private readonly Stream _stream;
    private readonly PstFormat _format;
    private readonly ulong _initialLastAmapOffset;
    private readonly Dictionary<ulong, byte[]> _amapPages = new();
    private ulong _effectiveLastAmapOffset;

    /// <summary>
    /// Membuat writer AMap untuk stream PST tertentu.
    /// </summary>
    /// <param name="stream">Stream PST target.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="initialLastAmapOffset">Nilai awal ROOT.ibAMapLast.</param>
    public NdbAllocationMapWriter(Stream stream, PstFormat format, ulong initialLastAmapOffset)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _format = format;
        _initialLastAmapOffset = initialLastAmapOffset >= FirstAmapOffset
            ? initialLastAmapOffset
            : FirstAmapOffset;
        _effectiveLastAmapOffset = _initialLastAmapOffset;
    }

    /// <summary>
    /// Menerapkan pembaruan bit AMap berdasarkan daftar range alokasi.
    /// </summary>
    /// <param name="ranges">Range alokasi block/page.</param>
    /// <returns>Snapshot metadata AMap setelah pembaruan.</returns>
    public NdbAllocationMapState ApplyAllocatedRanges(IReadOnlyList<NdbAllocationRange> ranges)
    {
        if (ranges is not null)
        {
            foreach (var range in ranges)
            {
                ApplyRange(range);
            }
        }

        var totalFreeBytes = 0UL;
        for (var mapOffset = FirstAmapOffset; mapOffset <= _effectiveLastAmapOffset; mapOffset += AmapIntervalBytes)
        {
            var page = GetOrLoadMapPage(mapOffset);
            totalFreeBytes += CountFreeBytes(page);
        }

        return new NdbAllocationMapState(_effectiveLastAmapOffset, totalFreeBytes);
    }

    private void ApplyRange(NdbAllocationRange range)
    {
        if (range.Length == 0)
        {
            return;
        }

        var start = range.Offset;
        var remaining = range.Length;
        if (start + remaining <= FirstAmapOffset)
        {
            return;
        }

        if (start < FirstAmapOffset)
        {
            var skipped = FirstAmapOffset - start;
            start = FirstAmapOffset;
            remaining = remaining > skipped ? remaining - skipped : 0;
        }

        while (remaining > 0)
        {
            var sectionStart = ResolveSectionStart(start);
            var sectionEnd = sectionStart + AmapIntervalBytes;
            var rangeEnd = start + remaining;
            var currentEnd = Math.Min(rangeEnd, sectionEnd);
            var currentLength = currentEnd - start;
            if (currentLength == 0)
            {
                break;
            }

            var page = GetOrLoadMapPage(sectionStart);
            MarkSectionAllocation(page, sectionStart, start, currentLength);
            PersistMapPage(sectionStart, page);

            if (sectionStart > _effectiveLastAmapOffset)
            {
                _effectiveLastAmapOffset = sectionStart;
            }

            start = currentEnd;
            remaining = rangeEnd - currentEnd;
        }
    }

    private byte[] GetOrLoadMapPage(ulong mapOffset)
    {
        if (_amapPages.TryGetValue(mapOffset, out var cached))
        {
            return cached;
        }

        byte[] page;
        if (mapOffset <= _initialLastAmapOffset && _stream.Length >= (long)(mapOffset + AmapPageSize))
        {
            page = ReadMapPage(mapOffset);
        }
        else
        {
            page = CreateNewMapPage();
            PersistMapPage(mapOffset, page);
        }

        _amapPages[mapOffset] = page;
        return page;
    }

    private byte[] ReadMapPage(ulong mapOffset)
    {
        var buffer = new byte[AmapPageSize];
        _stream.Seek((long)mapOffset, SeekOrigin.Begin);
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = _stream.Read(buffer, totalRead, buffer.Length - totalRead);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        if (totalRead < buffer.Length)
        {
            Array.Clear(buffer, totalRead, buffer.Length - totalRead);
        }

        return buffer;
    }

    private static byte[] CreateNewMapPage()
    {
        var buffer = new byte[AmapPageSize];
        // Byte pertama (8 bit) wajib 1 karena AMap memetakan dirinya sendiri (512 byte awal section).
        buffer[0] = 0xFF;
        return buffer;
    }

    private void PersistMapPage(ulong mapOffset, byte[] page)
    {
        var trailerSize = _format == PstFormat.Unicode ? 16 : 12;
        var trailerOffset = AmapPageSize - trailerSize;
        var crc = NdbIntegrity.ComputeCrc(0, page.AsSpan(0, trailerOffset));
        var bid = new Bid(mapOffset);
        var signature = NdbIntegrity.ComputeSignature(mapOffset, bid);
        NdbIntegrity.WritePageTrailer(
            page.AsSpan(trailerOffset, trailerSize),
            _format,
            NdbPageType.Amap,
            crc,
            signature,
            bid);

        _stream.Seek((long)mapOffset, SeekOrigin.Begin);
        _stream.Write(page, 0, page.Length);
    }

    private static void MarkSectionAllocation(byte[] page, ulong sectionStart, ulong startOffset, ulong length)
    {
        var sectionRelativeStart = startOffset - sectionStart;
        var bitStart = (int)(sectionRelativeStart / AmapUnitBytes);
        var bitEndExclusive = (int)((sectionRelativeStart + length + (AmapUnitBytes - 1)) / AmapUnitBytes);
        var maxBits = AmapDataBytes * 8;
        if (bitStart >= maxBits)
        {
            return;
        }

        bitEndExclusive = Math.Min(bitEndExclusive, maxBits);
        for (var bit = bitStart; bit < bitEndExclusive; bit++)
        {
            var byteIndex = bit / 8;
            var bitMask = 1 << (bit % 8);
            page[byteIndex] = (byte)(page[byteIndex] | bitMask);
        }
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

    private static ulong CountFreeBytes(byte[] page)
    {
        var freeBits = 0UL;
        for (var i = 0; i < AmapDataBytes; i++)
        {
            freeBits += (ulong)(8 - CountBits(page[i]));
        }

        return freeBits * AmapUnitBytes;
    }

    private static int CountBits(byte value)
    {
        var count = 0;
        var current = value;
        while (current != 0)
        {
            count += current & 1;
            current >>= 1;
        }

        return count;
    }
}

/// <summary>
/// Representasi range alokasi file yang harus ditandai pada AMap.
/// </summary>
/// <param name="Offset">Offset absolut awal range.</param>
/// <param name="Length">Panjang range dalam byte.</param>
internal readonly record struct NdbAllocationRange(ulong Offset, ulong Length);

/// <summary>
/// Snapshot metadata AMap setelah pembaruan bit alokasi.
/// </summary>
/// <param name="IbAMapLast">Offset AMap terakhir.</param>
/// <param name="CbAMapFree">Total free-space pada seluruh AMap.</param>
internal readonly record struct NdbAllocationMapState(ulong IbAMapLast, ulong CbAMapFree);
