using System;
using System.Collections.Generic;
using System.IO;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Implementasi Heap-on-Node untuk mengakses item heap dalam data node.
/// </summary>
internal sealed class HeapOnNode
{
    private const byte HeapSignature = 0xEC;
    private readonly IReadOnlyList<PstDataBlock> _blocks;
    private readonly Dictionary<int, PageMap> _pageMaps = new();

    /// <summary>
    /// Membuat heap dari daftar blok data node.
    /// </summary>
    /// <param name="blocks">Blok data node.</param>
    public HeapOnNode(IReadOnlyList<PstDataBlock> blocks)
    {
        _blocks = blocks;
        if (_blocks.Count == 0)
        {
            throw new InvalidDataException("Heap tidak memiliki blok data.");
        }

        ParseRootHeader(_blocks[0].Data.Span);
    }

    /// <summary>
    /// HID root untuk BTH/PC pada heap.
    /// </summary>
    public Hid UserRoot { get; private set; }

    /// <summary>
    /// Client signature pada HN untuk mengindikasikan struktur di atasnya.
    /// </summary>
    public byte ClientSignature { get; private set; }

    /// <summary>
    /// Mengambil data heap berdasarkan HID.
    /// </summary>
    /// <param name="hid">HID item.</param>
    /// <returns>Data heap item.</returns>
    public ReadOnlyMemory<byte> ReadItem(Hid hid)
    {
        if (!hid.IsValid)
        {
            throw new InvalidDataException("HID tidak valid.");
        }

        if (hid.BlockIndex >= _blocks.Count)
        {
            throw new InvalidDataException("HID mengacu ke blok yang tidak tersedia.");
        }

        var map = GetPageMap(hid.BlockIndex);
        var index = hid.Index - 1;
        if (index < 0 || index >= map.Offsets.Length - 1)
        {
            throw new InvalidDataException("HID index di luar batas alokasi.");
        }

        var start = map.Offsets[index];
        var end = map.Offsets[index + 1];
        if (end < start || end > _blocks[hid.BlockIndex].Data.Length)
        {
            throw new InvalidDataException("Offset alokasi heap tidak valid.");
        }

        return _blocks[hid.BlockIndex].Data.Slice(start, end - start);
    }

    /// <summary>
    /// Mengurai header HNHDR pada blok pertama.
    /// </summary>
    /// <param name="data">Data blok pertama.</param>
    private void ParseRootHeader(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12)
        {
            throw new InvalidDataException("HNHDR terlalu kecil.");
        }

        var ibHnpm = BitConverter.ToUInt16(data.Slice(0, 2));
        var bSig = data[2];
        if (bSig != HeapSignature)
        {
            throw new InvalidDataException($"Signature HNHDR tidak valid (0x{bSig:X2}).");
        }

        ClientSignature = data[3];
        var hidUserRoot = BitConverter.ToUInt32(data.Slice(4, 4));
        UserRoot = new Hid(hidUserRoot);
        _pageMaps[0] = ParsePageMap(data, ibHnpm);
    }

    /// <summary>
    /// Mengambil peta alokasi heap untuk blok tertentu.
    /// </summary>
    /// <param name="blockIndex">Indeks blok heap.</param>
    /// <returns>Peta alokasi.</returns>
    private PageMap GetPageMap(int blockIndex)
    {
        if (_pageMaps.TryGetValue(blockIndex, out var map))
        {
            return map;
        }

        var data = _blocks[blockIndex].Data.Span;
        var ibHnpm = ReadIbHnpm(blockIndex, data);
        map = ParsePageMap(data, ibHnpm);
        _pageMaps[blockIndex] = map;
        return map;
    }

    /// <summary>
    /// Menentukan offset HNPAGEMAP berdasarkan tipe header blok heap.
    /// </summary>
    /// <param name="blockIndex">Indeks blok heap.</param>
    /// <param name="data">Buffer data blok.</param>
    /// <returns>Offset HNPAGEMAP.</returns>
    private static ushort ReadIbHnpm(int blockIndex, ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
        {
            throw new InvalidDataException("Header blok heap tidak valid.");
        }

        if (blockIndex == 0)
        {
            return BitConverter.ToUInt16(data.Slice(0, 2));
        }

        if (IsBitmapHeaderIndex(blockIndex))
        {
            if (data.Length < 66)
            {
                throw new InvalidDataException("HNBITMAPHDR terlalu kecil.");
            }

            return BitConverter.ToUInt16(data.Slice(0, 2));
        }

        return BitConverter.ToUInt16(data.Slice(0, 2));
    }

    /// <summary>
    /// Menentukan apakah blok memakai HNBITMAPHDR berdasarkan indeks blok.
    /// </summary>
    /// <param name="blockIndex">Indeks blok heap.</param>
    /// <returns>True jika blok menggunakan HNBITMAPHDR.</returns>
    private static bool IsBitmapHeaderIndex(int blockIndex)
    {
        if (blockIndex < 8)
        {
            return false;
        }

        return (blockIndex - 8) % 128 == 0;
    }

    /// <summary>
    /// Mengurai HNPAGEMAP dari blok heap.
    /// </summary>
    /// <param name="data">Data blok heap.</param>
    /// <param name="ibHnpm">Offset HNPAGEMAP.</param>
    /// <returns>Peta alokasi heap.</returns>
    private static PageMap ParsePageMap(ReadOnlySpan<byte> data, ushort ibHnpm)
    {
        if (ibHnpm + 4 > data.Length)
        {
            throw new InvalidDataException("Offset HNPAGEMAP tidak valid.");
        }

        var cAlloc = BitConverter.ToUInt16(data.Slice(ibHnpm, 2));
        var offsets = new ushort[cAlloc + 1];
        var offsetStart = ibHnpm + 4;
        for (var i = 0; i < offsets.Length; i++)
        {
            var pos = offsetStart + (i * 2);
            if (pos + 2 > data.Length)
            {
                throw new InvalidDataException("Data HNPAGEMAP tidak lengkap.");
            }

            offsets[i] = BitConverter.ToUInt16(data.Slice(pos, 2));
        }

        return new PageMap(offsets);
    }

    /// <summary>
    /// Peta alokasi heap pada satu blok.
    /// </summary>
    private readonly struct PageMap
    {
        /// <summary>
        /// Membuat peta alokasi heap.
        /// </summary>
        /// <param name="offsets">Daftar offset alokasi.</param>
        public PageMap(ushort[] offsets)
        {
            Offsets = offsets;
        }

        /// <summary>
        /// Daftar offset alokasi termasuk slot akhir.
        /// </summary>
        public ushort[] Offsets { get; }
    }
}
