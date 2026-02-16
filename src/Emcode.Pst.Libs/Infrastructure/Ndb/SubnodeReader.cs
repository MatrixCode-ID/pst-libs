using System;
using System.Collections.Generic;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Reader subnode B-Tree untuk mengambil data subnode berdasarkan NID lokal.
/// </summary>
internal sealed class SubnodeReader
{
    private readonly PstBlockReader _blockReader;
    private readonly PstFormat _format;
    private readonly Bid _bidSub;
    private Dictionary<uint, SubnodeInfo>? _entries;

    /// <summary>
    /// Membuat reader subnode untuk node tertentu.
    /// </summary>
    /// <param name="blockReader">Reader blok data.</param>
    /// <param name="format">Format PST.</param>
    /// <param name="bidSub">BID subnode node.</param>
    public SubnodeReader(PstBlockReader blockReader, PstFormat format, Bid bidSub)
    {
        _blockReader = blockReader;
        _format = format;
        _bidSub = bidSub;
    }

    /// <summary>
    /// Mengambil data subnode berdasarkan NID lokal.
    /// </summary>
    /// <param name="localNid">NID lokal subnode.</param>
    /// <param name="blocks">Blok data subnode.</param>
    /// <returns>True jika subnode ditemukan.</returns>
    public bool TryGetSubnodeData(Nid localNid, out IReadOnlyList<PstDataBlock> blocks)
    {
        return TryGetSubnodeData(localNid, out blocks, out _);
    }

    /// <summary>
    /// Mengambil data subnode berdasarkan NID lokal beserta total length data tree.
    /// </summary>
    /// <param name="localNid">NID lokal subnode.</param>
    /// <param name="blocks">Blok data subnode.</param>
    /// <param name="totalLength">Total panjang data subnode bila tersedia.</param>
    /// <returns>True jika subnode ditemukan.</returns>
    public bool TryGetSubnodeData(Nid localNid, out IReadOnlyList<PstDataBlock> blocks, out uint totalLength)
    {
        blocks = Array.Empty<PstDataBlock>();
        totalLength = 0;
        if (!TryGetSubnodeInfo(localNid, out var info))
        {
            return false;
        }

        blocks = _blockReader.ReadDataBlocks(info.BidData, out totalLength);
        return true;
    }

    /// <summary>
    /// Mengambil informasi subnode (BID data/sub) berdasarkan NID lokal.
    /// </summary>
    /// <param name="localNid">NID lokal subnode.</param>
    /// <param name="info">Informasi subnode hasil.</param>
    /// <returns>True jika subnode ditemukan.</returns>
    public bool TryGetSubnodeInfo(Nid localNid, out SubnodeInfo info)
    {
        EnsureParsed();
        info = default;
        if (_entries is null)
        {
            return false;
        }

        return _entries.TryGetValue(localNid.Value, out info);
    }

    /// <summary>
    /// Mengambil informasi subnode pertama berdasarkan tipe NID.
    /// </summary>
    /// <param name="type">Tipe NID yang dicari.</param>
    /// <param name="localNid">NID lokal yang ditemukan.</param>
    /// <param name="info">Informasi subnode.</param>
    /// <returns>True jika ditemukan subnode dengan tipe tersebut.</returns>
    public bool TryGetSubnodeInfoByType(NidType type, out Nid localNid, out SubnodeInfo info)
    {
        EnsureParsed();
        localNid = default;
        info = default;
        if (_entries is null)
        {
            return false;
        }

        foreach (var entry in _entries)
        {
            var nid = new Nid(entry.Key);
            if (nid.Type != type)
            {
                continue;
            }

            localNid = nid;
            info = entry.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Mengambil semua subnode yang terurai pada node ini.
    /// </summary>
    /// <returns>Enumerasi subnode (NID lokal dan info).</returns>
    public IEnumerable<KeyValuePair<Nid, SubnodeInfo>> EnumerateSubnodes()
    {
        EnsureParsed();
        if (_entries is null)
        {
            yield break;
        }

        foreach (var entry in _entries)
        {
            yield return new KeyValuePair<Nid, SubnodeInfo>(new Nid(entry.Key), entry.Value);
        }
    }


    /// <summary>
    /// Memastikan subnode tree sudah terurai ke dictionary.
    /// </summary>
    private void EnsureParsed()
    {
        if (_entries is not null)
        {
            return;
        }

        _entries = new Dictionary<uint, SubnodeInfo>();
        if (_bidSub.IsZero)
        {
            return;
        }

        var blocks = _blockReader.ReadDataBlocks(_bidSub);
        if (blocks.Count == 0)
        {
            return;
        }

        ParseSubnodeBlock(blocks[0].Data.Span);
    }

    /// <summary>
    /// Mengurai block subnode root (SLBLOCK atau SIBLOCK).
    /// </summary>
    /// <param name="data">Data block subnode.</param>
    private void ParseSubnodeBlock(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
        {
            return;
        }

        var btype = data[0];
        var cLevel = data[1];
        var cEnt = BitConverter.ToUInt16(data.Slice(2, 2));
        if (btype != 0x02)
        {
            return;
        }

        if (cLevel == 0x00)
        {
            ParseSlBlockEntries(data, cEnt);
            return;
        }

        if (cLevel == 0x01)
        {
            ParseSiBlockEntries(data, cEnt);
        }
    }

    /// <summary>
    /// Mengurai SLBLOCK dan menambahkan SLENTRY ke dictionary.
    /// </summary>
    /// <param name="data">Data SLBLOCK.</param>
    /// <param name="cEnt">Jumlah entry.</param>
    private void ParseSlBlockEntries(ReadOnlySpan<byte> data, ushort cEnt)
    {
        var entrySize = _format == PstFormat.Unicode ? 24 : 12;
        var offset = _format == PstFormat.Unicode ? 8 : 4;
        for (var i = 0; i < cEnt; i++)
        {
            var entryOffset = offset + (i * entrySize);
            if (entryOffset + entrySize > data.Length)
            {
                break;
            }

            if (_format == PstFormat.Unicode)
            {
                var nidRaw = BitConverter.ToUInt64(data.Slice(entryOffset, 8));
                var bidData = BitConverter.ToUInt64(data.Slice(entryOffset + 8, 8));
                var bidSub = BitConverter.ToUInt64(data.Slice(entryOffset + 16, 8));
                _entries![unchecked((uint)nidRaw)] = new SubnodeInfo(new Bid(bidData), new Bid(bidSub));
            }
            else
            {
                var nidRaw = BitConverter.ToUInt32(data.Slice(entryOffset, 4));
                var bidData = BitConverter.ToUInt32(data.Slice(entryOffset + 4, 4));
                var bidSub = BitConverter.ToUInt32(data.Slice(entryOffset + 8, 4));
                _entries![nidRaw] = new SubnodeInfo(new Bid(bidData), new Bid(bidSub));
            }
        }
    }

    /// <summary>
    /// Mengurai SIBLOCK dan menelusuri SLBLOCK yang direferensikan.
    /// </summary>
    /// <param name="data">Data SIBLOCK.</param>
    /// <param name="cEnt">Jumlah entry.</param>
    private void ParseSiBlockEntries(ReadOnlySpan<byte> data, ushort cEnt)
    {
        var entrySize = _format == PstFormat.Unicode ? 16 : 8;
        var offset = _format == PstFormat.Unicode ? 8 : 4;
        for (var i = 0; i < cEnt; i++)
        {
            var entryOffset = offset + (i * entrySize);
            if (entryOffset + entrySize > data.Length)
            {
                break;
            }

            Bid bid;
            if (_format == PstFormat.Unicode)
            {
                var bidRaw = BitConverter.ToUInt64(data.Slice(entryOffset + 8, 8));
                bid = new Bid(bidRaw);
            }
            else
            {
                var bidRaw = BitConverter.ToUInt32(data.Slice(entryOffset + 4, 4));
                bid = new Bid(bidRaw);
            }

            var slBlocks = _blockReader.ReadDataBlocks(bid);
            if (slBlocks.Count > 0)
            {
                ParseSubnodeBlock(slBlocks[0].Data.Span);
            }
        }
    }

    /// <summary>
    /// Representasi entry subnode dari SLBLOCK.
    /// </summary>
    internal readonly struct SubnodeInfo
    {
        /// <summary>
        /// Membuat info subnode.
        /// </summary>
        /// <param name="bidData">BID data subnode.</param>
        /// <param name="bidSub">BID subnode dari subnode.</param>
        public SubnodeInfo(Bid bidData, Bid bidSub)
        {
            BidData = bidData;
            BidSub = bidSub;
        }

        /// <summary>
        /// BID data subnode.
        /// </summary>
        public Bid BidData { get; }

        /// <summary>
        /// BID subnode dari subnode (jika ada).
        /// </summary>
        public Bid BidSub { get; }
    }
}
