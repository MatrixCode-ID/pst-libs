# IPstReader

Namespace: `Emcode.Pst.Application.Abstractions`  
Assembly: `Emcode.Pst.Libs`

Kontrak untuk membaca struktur PST dari sumber penyimpanan.

## Definition

```csharp
public interface IPstReader
```

## Methods

- `PstReadResult Read(string path, PstOpenOptions options)`
- `Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)`

## Events

- Tidak ada event publik.
