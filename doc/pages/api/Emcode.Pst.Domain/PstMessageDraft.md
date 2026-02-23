# PstMessageDraft

Namespace: `Emcode.Pst.Domain`  
Assembly: `Emcode.Pst.Libs`

Draft data pesan yang akan dibuat atau diperbarui.

## Definition

```csharp
public sealed class PstMessageDraft
```

## Properties

- `string? MessageClass`
- `string? FromName`
- `string? FromAddress`
- `string? Subject`
- `string? Body`
- `string? HtmlBody`
- `string? MessageId`
- `DateTimeOffset? SentTime`
- `DateTimeOffset? ClientSubmitTime`
- `DateTimeOffset? LastModificationTime`
- `int? MessageFlags`
- `bool IsDraft`
- `bool? ReadReceiptRequested`
- `bool? DeliveryReceiptRequested`
- `int? Importance`
- `int? Priority`
- `int? Sensitivity`
- `string? TransportMessageHeaders`
- `string? ConversationTopic`
- `byte[]? ConversationIndex`
- `IReadOnlyList<PstDraftRecipient> Recipients`
- `IReadOnlyList<PstDraftAttachment> Attachments`

## Events

- Tidak ada event publik.
