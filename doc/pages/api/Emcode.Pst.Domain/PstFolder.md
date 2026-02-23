# PstFolder

Namespace: `Emcode.Pst.Domain`  
Assembly: `Emcode.Pst.Libs`

Representasi folder di dalam PST.

## Definition

```csharp
public sealed class PstFolder
```

## Properties

- `string Id`
- `string Name`
- `IReadOnlyList<PstFolder> SubFolders`
- `IReadOnlyList<PstMessage> Messages`

## Methods

- `IEnumerable<PstMessage> EnumerateMessages()`

## Events

- Tidak ada event publik.
