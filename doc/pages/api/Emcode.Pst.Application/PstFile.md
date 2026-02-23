# PstFile

Namespace: `Emcode.Pst.Application`  
Assembly: `Emcode.Pst.Libs`

Facade utama untuk membuka, membaca, dan menulis file PST.

## Definition

```csharp
public sealed class PstFile : IDisposable
```

## Properties

- `string Path` - Lokasi file PST yang dibuka.
- `PstOpenOptions Options` - Opsi pembukaan PST yang digunakan.
- `IReadOnlyList<PstFolder> Folders` - Daftar folder hasil pembacaan PST.
- `PstFolder? RootFolder` - Folder root PST jika tersedia.
- `PstHeaderInfo? Header` - Metadata header PST hasil pembacaan awal.

## Methods

- `static PstFile Open(string path, PstOpenOptions? options = null, IPstReader? reader = null, IPstWriter? writer = null)`
- `static Task<PstFile> OpenAsync(string path, PstOpenOptions? options = null, IPstReader? reader = null, IPstWriter? writer = null, CancellationToken cancellationToken = default)`
- `PstFolder CreateFolder(string name, PstFolder? parent = null)`
- `Task<PstFolder> CreateFolderAsync(string name, PstFolder? parent = null, CancellationToken cancellationToken = default)`
- `PstMessage CreateMessage(PstFolder folder, PstMessageDraft draft)`
- `Task<PstMessage> CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)`
- `PstMessage ImportEml(PstFolder folder, string emlPath)`
- `Task<PstMessage> ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)`
- `void UpdateMessage(PstMessage message, PstMessageDraft draft)`
- `Task UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)`
- `void DeleteMessage(PstMessage message)`
- `Task DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)`
- `void Dispose()`

## Events

- Tidak ada event publik.

## See Also

- [PstOpenOptions](./PstOpenOptions.md)
- [IPstReader](../Emcode.Pst.Application.Abstractions/IPstReader.md)
- [IPstWriter](../Emcode.Pst.Application.Abstractions/IPstWriter.md)
