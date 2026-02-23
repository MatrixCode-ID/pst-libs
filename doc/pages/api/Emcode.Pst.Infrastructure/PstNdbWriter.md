# PstNdbWriter

Namespace: `Emcode.Pst.Infrastructure.Ndb`  
Assembly: `Emcode.Pst.Libs`

Implementasi writer PST berbasis NDB untuk persist ke disk (eksperimental).

## Definition

```csharp
public sealed class PstNdbWriter : IPstWriter, IPstWriterWithContext, IDisposable
```

## Constructors

- `PstNdbWriter()`
- `PstNdbWriter(PstFormat format)`

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
- `void Dispose()`

## Remarks

`UpdateMessage` dan `DeleteMessage` pada implementasi ini masih menandakan belum didukung.

## Events

- Tidak ada event publik.
