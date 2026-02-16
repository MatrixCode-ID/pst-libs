using Emcode.Pst.Infrastructure.Ndb;

namespace Emcode.Pst.Infrastructure.Ltp;

/// <summary>
/// Enumerasi tipe properti MAPI yang umum dipakai dalam PC.
/// </summary>
internal enum PstPropertyType : ushort
{
    /// <summary>
    /// String ANSI (PtypString8).
    /// </summary>
    String8 = 0x001E,

    /// <summary>
    /// String Unicode (PtypString).
    /// </summary>
    String = 0x001F,

    /// <summary>
    /// Waktu FILETIME (PtypTime).
    /// </summary>
    Time = 0x0040,

    /// <summary>
    /// Integer 32-bit (PtypInteger32).
    /// </summary>
    Integer32 = 0x0003,

    /// <summary>
    /// Boolean (PtypBoolean).
    /// </summary>
    Boolean = 0x000B,

    /// <summary>
    /// Biner variabel (PtypBinary).
    /// </summary>
    Binary = 0x0102
}

/// <summary>
/// Representasi HID (Heap ID) untuk mengakses item pada heap HN.
/// </summary>
internal readonly struct Hid
{
    /// <summary>
    /// Membuat HID dari nilai raw.
    /// </summary>
    /// <param name="raw">Nilai raw HID.</param>
    public Hid(uint raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Nilai raw HID.
    /// </summary>
    public uint Raw { get; }

    /// <summary>
    /// Indeks item heap (1-based).
    /// </summary>
    public int Index => (int)((Raw >> 5) & 0x7FF);

    /// <summary>
    /// Indeks blok data heap.
    /// </summary>
    public int BlockIndex => (int)(Raw >> 16);

    /// <summary>
    /// Menandakan HID valid (type == HID dan index > 0).
    /// </summary>
    public bool IsValid => (Raw & 0x1F) == 0 && Index > 0;
}

/// <summary>
/// HNID hybrid yang dapat mereferensikan HID atau NID.
/// </summary>
internal readonly struct Hnid
{
    /// <summary>
    /// Membuat HNID dari nilai raw.
    /// </summary>
    /// <param name="raw">Nilai raw HNID.</param>
    public Hnid(uint raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Nilai raw HNID.
    /// </summary>
    public uint Raw { get; }

    /// <summary>
    /// Menandakan HNID mengacu ke HID.
    /// </summary>
    public bool IsHid => (Raw & 0x1F) == (uint)NidType.Hid;

    /// <summary>
    /// Mengembalikan HID bila HNID mengacu ke heap.
    /// </summary>
    /// <returns>HID terurai.</returns>
    public Hid ToHid()
    {
        return new Hid(Raw);
    }

    /// <summary>
    /// Mengembalikan NID bila HNID mengacu ke subnode.
    /// </summary>
    /// <returns>NID terurai.</returns>
    public Nid ToNid()
    {
        return new Nid(Raw);
    }
}
