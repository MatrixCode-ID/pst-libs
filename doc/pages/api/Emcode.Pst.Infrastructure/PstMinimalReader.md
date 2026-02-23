# PstMinimalReader

Namespace: `Emcode.Pst.Infrastructure`  
Assembly: `Emcode.Pst.Libs`

Reader minimal yang memvalidasi header PST dan mengembalikan metadata dasar.

## Definition

```csharp
public sealed class PstMinimalReader : IPstReader
```

## Methods

- `PstReadResult Read(string path, PstOpenOptions options)`
- `Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)`

## Events

- Tidak ada event publik.
