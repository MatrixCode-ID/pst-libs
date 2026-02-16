using System;
using System.Collections.Generic;
using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Entry point writer LTP untuk membuat Property Context dan Table Context.
/// </summary>
internal sealed class LtpWriter
{
    /// <summary>
    /// Membuat writer LTP.
    /// </summary>
    /// <param name="options">Opsi writer.</param>
    public LtpWriter(LtpWriterOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Opsi writer LTP.
    /// </summary>
    public LtpWriterOptions Options { get; }

    /// <summary>
    /// Membuat writer Property Context (PC).
    /// </summary>
    /// <returns>Writer PC.</returns>
    public PropertyContextWriter CreatePropertyContextWriter()
    {
        return new PropertyContextWriter(Options);
    }

    /// <summary>
    /// Membuat writer Table Row untuk Table Context (TC).
    /// </summary>
    /// <returns>Writer Table Row.</returns>
    public TableRowWriter CreateTableRowWriter()
    {
        return new TableRowWriter(Options);
    }

    /// <summary>
    /// Builder heap sederhana untuk HN single-block.
    /// </summary>
    internal sealed class HeapWriter
    {
        private const byte HeapSignature = 0xEC;
        private readonly List<byte[]> _items = new();
        private readonly LtpWriterOptions _options;

        /// <summary>
        /// Membuat heap writer.
        /// </summary>
        /// <param name="options">Opsi writer LTP.</param>
        public HeapWriter(LtpWriterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Menambahkan item ke heap dan mengembalikan HID.
        /// </summary>
        /// <param name="data">Data item.</param>
        /// <returns>HID item.</returns>
        public Hid AddItem(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Data item heap tidak boleh kosong.");
            }

            var buffer = data.ToArray();
            _items.Add(buffer);
            var index = _items.Count;
            var raw = (uint)(index << 5);
            return new Hid(raw);
        }

        /// <summary>
        /// Membangun buffer heap beserta blok data.
        /// </summary>
        /// <param name="userRoot">HID user root.</param>
        /// <returns>Daftar blok data heap.</returns>
        public IReadOnlyList<PstDataBlock> Build(Hid userRoot)
        {
            if (!userRoot.IsValid)
            {
                throw new ArgumentException("User root HID tidak valid.", nameof(userRoot));
            }

            if (_items.Count == 0)
            {
                throw new InvalidOperationException("Heap tidak memiliki item.");
            }

            var blockSize = _options.BlockSize;
            var buffer = new byte[blockSize];
            const int headerSize = 12;
            var offset = headerSize;
            var offsets = new ushort[_items.Count + 1];

            for (var i = 0; i < _items.Count; i++)
            {
                offsets[i] = (ushort)offset;
                var item = _items[i];
                if (offset + item.Length > blockSize)
                {
                    throw new InvalidOperationException("Ukuran heap melebihi kapasitas block.");
                }

                item.CopyTo(buffer.AsSpan(offset));
                offset += item.Length;
            }

            offsets[^1] = (ushort)offset;
            var mapStart = offset;
            var mapSize = 4 + (offsets.Length * 2);
            if (mapStart + mapSize > blockSize)
            {
                throw new InvalidOperationException("HNPAGEMAP melebihi kapasitas block.");
            }

            BitConverter.TryWriteBytes(buffer.AsSpan(0, 2), (ushort)mapStart);
            buffer[2] = HeapSignature;
            buffer[3] = _options.ClientSignature;
            BitConverter.TryWriteBytes(buffer.AsSpan(4, 4), userRoot.Raw);
            BitConverter.TryWriteBytes(buffer.AsSpan(mapStart, 2), (ushort)_items.Count);
            for (var i = 0; i < offsets.Length; i++)
            {
                BitConverter.TryWriteBytes(buffer.AsSpan(mapStart + 4 + (i * 2), 2), offsets[i]);
            }

            return new[] { new PstDataBlock(new Bid(1), buffer) };
        }
    }
}
