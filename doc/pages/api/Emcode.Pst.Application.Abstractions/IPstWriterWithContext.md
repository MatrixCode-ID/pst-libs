# IPstWriterWithContext

Namespace: `Emcode.Pst.Application.Abstractions`  
Assembly: `Emcode.Pst.Libs`

Kontrak untuk writer yang membutuhkan konteks PST sebelum operasi write.

## Definition

```csharp
public interface IPstWriterWithContext
```

## Methods

- `void Initialize(PstWriteContext context)`
- `Task InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)`

## Events

- Tidak ada event publik.
