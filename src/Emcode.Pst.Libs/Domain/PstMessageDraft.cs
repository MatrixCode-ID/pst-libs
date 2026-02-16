using System;
using System.Collections.Generic;

namespace Emcode.Pst.Domain;

/// <summary>
/// Draft data pesan yang akan dibuat atau diperbarui.
/// </summary>
public sealed class PstMessageDraft
{
    /// <summary>
    /// Message class MAPI (contoh: IPM.Note).
    /// </summary>
    public string? MessageClass { get; init; }

    /// <summary>
    /// Nama pengirim (display name).
    /// </summary>
    public string? FromName { get; init; }

    /// <summary>
    /// Alamat email pengirim.
    /// </summary>
    public string? FromAddress { get; init; }

    /// <summary>
    /// Subjek pesan.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Body teks biasa.
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Body HTML bila tersedia.
    /// </summary>
    public string? HtmlBody { get; init; }

    /// <summary>
    /// Message-Id dari header MIME bila tersedia.
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// Waktu pengiriman pesan (tanggal pada header).
    /// </summary>
    public DateTimeOffset? SentTime { get; init; }

    /// <summary>
    /// Waktu submit pesan dari client ke transport.
    /// </summary>
    public DateTimeOffset? ClientSubmitTime { get; init; }

    /// <summary>
    /// Waktu modifikasi terakhir pesan.
    /// </summary>
    public DateTimeOffset? LastModificationTime { get; init; }

    /// <summary>
    /// Flag status message dalam bentuk bitmask MAPI.
    /// </summary>
    public int? MessageFlags { get; init; }

    /// <summary>
    /// Menandakan pesan ini diperlakukan sebagai draft.
    /// </summary>
    public bool IsDraft { get; init; } = true;

    /// <summary>
    /// Menandakan permintaan read receipt.
    /// </summary>
    public bool? ReadReceiptRequested { get; init; }

    /// <summary>
    /// Menandakan permintaan delivery receipt.
    /// </summary>
    public bool? DeliveryReceiptRequested { get; init; }

    /// <summary>
    /// Tingkat importance pesan.
    /// </summary>
    public int? Importance { get; init; }

    /// <summary>
    /// Prioritas pesan.
    /// </summary>
    public int? Priority { get; init; }

    /// <summary>
    /// Tingkat sensitivitas pesan.
    /// </summary>
    public int? Sensitivity { get; init; }

    /// <summary>
    /// Header transport mentah (RFC822) bila tersedia.
    /// </summary>
    public string? TransportMessageHeaders { get; init; }

    /// <summary>
    /// Topik percakapan (thread topic).
    /// </summary>
    public string? ConversationTopic { get; init; }

    /// <summary>
    /// Indeks percakapan (thread index) dalam bentuk biner.
    /// </summary>
    public byte[]? ConversationIndex { get; init; }

    /// <summary>
    /// Daftar penerima pesan (To/Cc/Bcc).
    /// </summary>
    public IReadOnlyList<PstDraftRecipient> Recipients { get; init; } = Array.Empty<PstDraftRecipient>();

    /// <summary>
    /// Daftar attachment untuk pesan.
    /// </summary>
    public IReadOnlyList<PstDraftAttachment> Attachments { get; init; } = Array.Empty<PstDraftAttachment>();
}
