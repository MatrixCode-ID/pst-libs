# PstMessage

Namespace: `Emcode.Pst.Domain`  
Assembly: `Emcode.Pst.Libs`

Representasi pesan email di dalam PST.

## Definition

```csharp
public sealed class PstMessage
```

## Properties

- `string Id`
- `string? Subject`
- `string? MessageClass`
- `string? Body`
- `string? HtmlBody`
- `string? SenderName`
- `DateTimeOffset? DeliveryTime`
- `int? Size`
- `string? InternetMessageId`
- `string? SenderEmailAddress`
- `string? SenderSmtpAddress`
- `string? SentRepresentingName`
- `string? SentRepresentingEmailAddress`
- `string? OriginalSenderName`
- `string? OriginalSenderEmailAddress`
- `string? DisplayTo`
- `string? DisplayCc`
- `string? DisplayBcc`
- `DateTimeOffset? ReceivedTime`
- `DateTimeOffset? ClientSubmitTime`
- `ReadOnlyMemory<byte>? MessageSubmissionId`
- `DateTimeOffset? LastModificationTime`
- `int? MessageFlags`
- `bool? ReadReceiptRequested`
- `bool? DeliveryReceiptRequested`
- `bool? HasAttachments`
- `int? Importance`
- `int? Priority`
- `int? Sensitivity`
- `string? TransportMessageHeaders`
- `string? ConversationTopic`
- `ReadOnlyMemory<byte>? ConversationIndex`
- `IReadOnlyList<PstRecipient> Recipients`
- `IReadOnlyList<PstAttachment> Attachments`

## Methods

- `void Update(PstMessageDraft draft)`
- `void Delete()`

## Events

- Tidak ada event publik.
