# PstAttachment

Namespace: `Emcode.Pst.Domain`  
Assembly: `Emcode.Pst.Libs`

Representasi attachment pesan pada PST berdasarkan Attachment Table.

## Definition

```csharp
public sealed class PstAttachment
```

## Properties

- `int? AttachNumber`
- `string? FileName`
- `string? LongFileName`
- `int? Size`
- `string? MimeTag`
- `string? ContentId`
- `int? AttachMethod`

## Methods

- `Stream? OpenContentStream()`
- `Task<Stream?> OpenContentStreamAsync(CancellationToken cancellationToken = default)`
- `byte[]? ReadContentBytes()`
- `Task<byte[]?> ReadContentBytesAsync(CancellationToken cancellationToken = default)`

## Events

- Tidak ada event publik.
