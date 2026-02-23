# PstInMemoryWriter

Namespace: `Emcode.Pst.Infrastructure`  
Assembly: `Emcode.Pst.Libs`

Implementasi writer berbasis in-memory untuk membuat draft pesan tanpa menulis ke disk.

## Definition

```csharp
public sealed class PstInMemoryWriter : IPstWriter, IPstWriterWithContext
```

## Constructors

- `PstInMemoryWriter()`

## Methods

- `void Initialize(PstWriteContext context)`
- `Task InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)`
- `PstFolder CreateFolder(string name, PstFolder? parent)`
- `Task<PstFolder> CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default)`
- `PstMessage CreateMessage(PstFolder folder, PstMessageDraft draft)`
- `Task<PstMessage> CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)`
- `PstMessage ImportEml(PstFolder folder, string emlPath)`
- `Task<PstMessage> ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)`
- `void UpdateMessage(PstMessage message, PstMessageDraft draft)`
- `Task UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)`
- `void DeleteMessage(PstMessage message)`
- `Task DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)`

## Events

- Tidak ada event publik.
