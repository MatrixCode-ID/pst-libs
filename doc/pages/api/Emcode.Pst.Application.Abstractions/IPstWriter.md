# IPstWriter

Namespace: `Emcode.Pst.Application.Abstractions`  
Assembly: `Emcode.Pst.Libs`

Kontrak untuk operasi write pada PST.

## Definition

```csharp
public interface IPstWriter
```

## Methods

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
