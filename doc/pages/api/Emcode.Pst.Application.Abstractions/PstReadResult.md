# PstReadResult

Namespace: `Emcode.Pst.Application.Abstractions`  
Assembly: `Emcode.Pst.Libs`

Hasil pembacaan PST yang memuat metadata header, folder root, dan daftar folder.

## Definition

```csharp
public sealed class PstReadResult
```

## Constructors

- `PstReadResult(PstHeaderInfo? header, PstFolder? rootFolder, IReadOnlyList<PstFolder> folders)`

## Properties

- `PstHeaderInfo? Header`
- `PstFolder? RootFolder`
- `IReadOnlyList<PstFolder> Folders`

## Events

- Tidak ada event publik.
