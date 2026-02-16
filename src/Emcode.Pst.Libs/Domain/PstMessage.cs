using Emcode.Pst.Shared;

namespace Emcode.Pst.Domain;

/// <summary>
/// Representasi pesan email di dalam PST.
/// </summary>
public sealed class PstMessage
{
    /// <summary>
    /// Membuat instance pesan dengan identifier internal.
    /// </summary>
    /// <param name="id">Identifier internal pesan.</param>
    internal PstMessage(string id)
    {
        Id = id;
        Recipients = Array.Empty<PstRecipient>();
        Attachments = Array.Empty<PstAttachment>();
    }

    /// <summary>
    /// Identifier internal pesan.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Subjek pesan.
    /// </summary>
    public string? Subject { get; internal set; }

    /// <summary>
    /// Message class MAPI (contoh: IPM.Note).
    /// </summary>
    public string? MessageClass { get; internal set; }

    /// <summary>
    /// Body teks biasa.
    /// </summary>
    public string? Body { get; internal set; }

    /// <summary>
    /// Body HTML bila tersedia.
    /// </summary>
    public string? HtmlBody { get; internal set; }

    /// <summary>
    /// Nama pengirim.
    /// </summary>
    public string? SenderName { get; internal set; }

    /// <summary>
    /// Waktu pengiriman pesan.
    /// </summary>
    public DateTimeOffset? DeliveryTime { get; internal set; }

    /// <summary>
    /// Ukuran pesan dalam byte bila tersedia.
    /// </summary>
    public int? Size { get; internal set; }

    /// <summary>
    /// Internet Message-Id dari header MIME.
    /// </summary>
    public string? InternetMessageId { get; internal set; }

    /// <summary>
    /// Alamat email pengirim sesuai MAPI.
    /// </summary>
    public string? SenderEmailAddress { get; internal set; }

    /// <summary>
    /// Alamat SMTP pengirim bila tersedia.
    /// </summary>
    public string? SenderSmtpAddress { get; internal set; }

    /// <summary>
    /// Nama yang direpresentasikan saat pengiriman (send on behalf).
    /// </summary>
    public string? SentRepresentingName { get; internal set; }

    /// <summary>
    /// Alamat email yang direpresentasikan saat pengiriman.
    /// </summary>
    public string? SentRepresentingEmailAddress { get; internal set; }

    /// <summary>
    /// Nama pengirim asli sebelum perubahan/forward.
    /// </summary>
    public string? OriginalSenderName { get; internal set; }

    /// <summary>
    /// Alamat email pengirim asli sebelum perubahan/forward.
    /// </summary>
    public string? OriginalSenderEmailAddress { get; internal set; }

    /// <summary>
    /// Daftar penerima pada field To.
    /// </summary>
    public string? DisplayTo { get; internal set; }

    /// <summary>
    /// Daftar penerima pada field Cc.
    /// </summary>
    public string? DisplayCc { get; internal set; }

    /// <summary>
    /// Daftar penerima pada field Bcc.
    /// </summary>
    public string? DisplayBcc { get; internal set; }

    /// <summary>
    /// Waktu pesan diterima (delivery time).
    /// </summary>
    public DateTimeOffset? ReceivedTime { get; internal set; }

    /// <summary>
    /// Waktu submit client ke transport.
    /// </summary>
    public DateTimeOffset? ClientSubmitTime { get; internal set; }

    /// <summary>
    /// ID submit pesan untuk tracking transport.
    /// </summary>
    public ReadOnlyMemory<byte>? MessageSubmissionId { get; internal set; }

    /// <summary>
    /// Waktu modifikasi terakhir pesan.
    /// </summary>
    public DateTimeOffset? LastModificationTime { get; internal set; }

    /// <summary>
    /// Flag status pesan (bitmask).
    /// </summary>
    public int? MessageFlags { get; internal set; }

    /// <summary>
    /// Menandakan permintaan read receipt.
    /// </summary>
    public bool? ReadReceiptRequested { get; internal set; }

    /// <summary>
    /// Menandakan permintaan delivery receipt.
    /// </summary>
    public bool? DeliveryReceiptRequested { get; internal set; }

    /// <summary>
    /// Menandakan pesan memiliki attachment.
    /// </summary>
    public bool? HasAttachments { get; internal set; }

    /// <summary>
    /// Tingkat importance pesan.
    /// </summary>
    public int? Importance { get; internal set; }

    /// <summary>
    /// Prioritas pesan.
    /// </summary>
    public int? Priority { get; internal set; }

    /// <summary>
    /// Tingkat sensitivitas pesan.
    /// </summary>
    public int? Sensitivity { get; internal set; }

    /// <summary>
    /// Header transport mentah (RFC822) bila tersedia.
    /// </summary>
    public string? TransportMessageHeaders { get; internal set; }

    /// <summary>
    /// Topik percakapan (thread topic).
    /// </summary>
    public string? ConversationTopic { get; internal set; }

    /// <summary>
    /// Indeks percakapan (thread index) dalam bentuk biner.
    /// </summary>
    public ReadOnlyMemory<byte>? ConversationIndex { get; internal set; }

    /// <summary>
    /// Daftar penerima pesan bila tersedia.
    /// </summary>
    public IReadOnlyList<PstRecipient> Recipients { get; internal set; }

    /// <summary>
    /// Daftar attachment pesan bila tersedia.
    /// </summary>
    public IReadOnlyList<PstAttachment> Attachments { get; internal set; }

    /// <summary>
    /// Memperbarui data pesan.
    /// </summary>
    /// <param name="draft">Draft data terbaru.</param>
    public void Update(PstMessageDraft draft)
    {
        Guard.NotNull(draft, nameof(draft));
        throw new NotSupportedException("Update is not implemented yet. Planned for Phase 2.");
    }

    /// <summary>
    /// Menghapus pesan dari PST.
    /// </summary>
    public void Delete()
    {
        throw new NotSupportedException("Delete is not implemented yet. Planned for Phase 2.");
    }
}
