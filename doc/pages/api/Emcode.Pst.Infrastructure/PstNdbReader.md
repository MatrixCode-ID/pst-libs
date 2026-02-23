# PstNdbReader

Namespace: `Emcode.Pst.Infrastructure`  
Assembly: `Emcode.Pst.Libs`

Reader PST berbasis parsing NDB untuk mengekstrak folder dan message nyata.

## Definition

```csharp
public sealed class PstNdbReader : IPstReader
```

## Methods

- `PstReadResult Read(string path, PstOpenOptions options)`
- `Task<PstReadResult> ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)`

## Events

- Tidak ada event publik.
