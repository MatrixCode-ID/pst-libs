namespace Emcode.Pst.Infrastructure.Ndb;

/// <summary>
/// Enumerasi jenis NID sesuai tabel NID_TYPE di spesifikasi PST.
/// </summary>
internal enum NidType : uint
{
    /// <summary>
    /// NID yang merepresentasikan heap item (HID).
    /// </summary>
    Hid = 0x00,

    /// <summary>
    /// NID internal untuk objek khusus (misalnya message store).
    /// </summary>
    Internal = 0x01,

    /// <summary>
    /// NID untuk Folder object normal.
    /// </summary>
    NormalFolder = 0x02,

    /// <summary>
    /// NID untuk Search Folder object.
    /// </summary>
    SearchFolder = 0x03,

    /// <summary>
    /// NID untuk Message object normal.
    /// </summary>
    NormalMessage = 0x04,

    /// <summary>
    /// NID untuk Attachment object.
    /// </summary>
    Attachment = 0x05,

    /// <summary>
    /// NID untuk FAI Message object.
    /// </summary>
    AssocMessage = 0x08,

    /// <summary>
    /// NID untuk hierarchy table (TC).
    /// </summary>
    HierarchyTable = 0x0D,

    /// <summary>
    /// NID untuk contents table (TC).
    /// </summary>
    ContentsTable = 0x0E,

    /// <summary>
    /// NID untuk FAI contents table (TC).
    /// </summary>
    AssocContentsTable = 0x0F,

    /// <summary>
    /// NID untuk LTP internal.
    /// </summary>
    Ltp = 0x1F
}

/// <summary>
/// Representasi NID (Node ID) yang menyimpan tipe dan indeks.
/// </summary>
internal readonly struct Nid
{
    /// <summary>
    /// Membuat instance NID dari nilai 32-bit.
    /// </summary>
    /// <param name="value">Nilai raw NID.</param>
    public Nid(uint value)
    {
        Value = value;
    }

    /// <summary>
    /// Nilai raw NID.
    /// </summary>
    public uint Value { get; }

    /// <summary>
    /// Jenis NID berdasarkan 5 bit terbawah.
    /// </summary>
    public NidType Type => (NidType)(Value & 0x1F);

    /// <summary>
    /// Indeks NID (27 bit teratas).
    /// </summary>
    public uint Index => Value >> 5;

    /// <summary>
    /// Menandakan NID bernilai nol.
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// Mengembalikan representasi string NID untuk logging.
    /// </summary>
    /// <returns>String hex dari NID.</returns>
    public override string ToString()
    {
        return $"0x{Value:X8}";
    }
}

/// <summary>
/// Representasi BID (Block ID) pada PST.
/// </summary>
internal readonly struct Bid
{
    /// <summary>
    /// Membuat instance BID dari nilai 64-bit.
    /// </summary>
    /// <param name="raw">Nilai raw BID.</param>
    public Bid(ulong raw)
    {
        Raw = raw;
    }

    /// <summary>
    /// Nilai raw BID.
    /// </summary>
    public ulong Raw { get; }

    /// <summary>
    /// Menandakan BID bernilai nol.
    /// </summary>
    public bool IsZero => Raw == 0;

    /// <summary>
    /// Menandakan BID bertipe internal (bit i).
    /// </summary>
    public bool IsInternal => (Raw & 0x2UL) != 0;

    /// <summary>
    /// Menghapus reserved bit sebelum lookup ke BBT.
    /// </summary>
    /// <returns>Nilai BID dengan reserved bit dinormalisasi.</returns>
    public ulong NormalizeForLookup()
    {
        return Raw & ~1UL;
    }

    /// <summary>
    /// Mengembalikan representasi string BID untuk logging.
    /// </summary>
    /// <returns>String hex dari BID.</returns>
    public override string ToString()
    {
        return $"0x{Raw:X}";
    }
}

/// <summary>
/// Referensi blok atau page yang terdiri dari BID dan IB.
/// </summary>
internal readonly struct Bref
{
    /// <summary>
    /// Membuat instance BREF.
    /// </summary>
    /// <param name="bid">BID yang direferensikan.</param>
    /// <param name="ib">Offset byte absolut di file PST.</param>
    public Bref(Bid bid, ulong ib)
    {
        Bid = bid;
        Ib = ib;
    }

    /// <summary>
    /// BID yang direferensikan.
    /// </summary>
    public Bid Bid { get; }

    /// <summary>
    /// Offset byte absolut di file PST.
    /// </summary>
    public ulong Ib { get; }
}
