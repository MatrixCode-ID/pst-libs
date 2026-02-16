using System;
using Emcode.Pst.Domain;

namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Utilitas decoding data blok sesuai metode enkripsi/encoding PST.
/// </summary>
internal static class NdbCrypt
{
    private static readonly byte[] PermuteTable =
    {
        65, 54, 19, 98, 168, 33, 110, 187,
        244, 22, 204, 4, 127, 100, 232, 93,
        30, 242, 203, 42, 116, 197, 94, 53,
        210, 149, 71, 158, 150, 45, 154, 136,
        76, 125, 132, 63, 219, 172, 49, 182,
        72, 95, 246, 196, 216, 57, 139, 231,
        35, 59, 56, 142, 200, 193, 223, 37,
        177, 32, 165, 70, 96, 78, 156, 251,
        170, 211, 86, 81, 69, 124, 85, 0,
        7, 201, 43, 157, 133, 155, 9, 160,
        143, 173, 179, 15, 99, 171, 137, 75,
        215, 167, 21, 90, 113, 102, 66, 191,
        38, 74, 107, 152, 250, 234, 119, 83,
        178, 112, 5, 44, 253, 89, 58, 134,
        126, 206, 6, 235, 130, 120, 87, 199,
        141, 67, 175, 180, 28, 212, 91, 205,
        226, 233, 39, 79, 195, 8, 114, 128,
        207, 176, 239, 245, 40, 109, 190, 48,
        77, 52, 146, 213, 14, 60, 34, 50,
        229, 228, 249, 159, 194, 209, 10, 129,
        18, 225, 238, 145, 131, 118, 227, 151,
        230, 97, 138, 23, 121, 164, 183, 220,
        144, 122, 92, 140, 2, 166, 202, 105,
        222, 80, 26, 17, 147, 185, 82, 135,
        88, 252, 237, 29, 55, 73, 27, 106,
        224, 41, 51, 153, 189, 108, 217, 148,
        243, 64, 84, 111, 240, 198, 115, 184,
        214, 62, 101, 24, 68, 31, 221, 103,
        16, 241, 12, 25, 236, 174, 3, 161,
        20, 123, 169, 11, 255, 248, 163, 192,
        162, 1, 247, 46, 188, 36, 104, 117,
        13, 254, 186, 47, 181, 208, 218, 61
    };

    private static readonly byte[] InversePermuteTable = BuildInversePermuteTable();

    /// <summary>
    /// Mendekode data blok sesuai metode enkripsi/encoding.
    /// </summary>
    /// <param name="method">Metode enkripsi/encoding.</param>
    /// <param name="data">Buffer data yang akan didekode.</param>
    public static void Decode(PstCryptMethod method, Span<byte> data)
    {
        if (method == PstCryptMethod.None)
        {
            return;
        }

        if (method == PstCryptMethod.Permute)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = InversePermuteTable[data[i]];
            }

            return;
        }

        throw new NotSupportedException($"Metode enkripsi {method} belum didukung.");
    }

    /// <summary>
    /// Meng-encode data blok sesuai metode enkripsi/encoding.
    /// </summary>
    /// <param name="method">Metode enkripsi/encoding.</param>
    /// <param name="data">Buffer data yang akan di-encode.</param>
    public static void Encode(PstCryptMethod method, Span<byte> data)
    {
        if (method == PstCryptMethod.None)
        {
            return;
        }

        if (method == PstCryptMethod.Permute)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = PermuteTable[data[i]];
            }

            return;
        }

        throw new NotSupportedException($"Metode enkripsi {method} belum didukung.");
    }

    /// <summary>
    /// Membangun tabel inverse dari tabel permutasi.
    /// </summary>
    /// <returns>Tabel inverse permutasi.</returns>
    private static byte[] BuildInversePermuteTable()
    {
        var table = new byte[256];
        for (var i = 0; i < PermuteTable.Length; i++)
        {
            table[PermuteTable[i]] = (byte)i;
        }

        return table;
    }
}
