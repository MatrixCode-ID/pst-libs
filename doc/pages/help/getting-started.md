# Getting Started

## Prasyarat

- .NET 10 SDK
- Referensi package `Emcode.Pst.Libs`

## Quick Start (Read)

```csharp
using Emcode.Pst.Application;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = false
});

foreach (var folder in pst.Folders)
{
    Console.WriteLine($"{folder.Name}: {folder.Messages.Count}");
}
```

## Quick Start (Async)

```csharp
using Emcode.Pst.Application;
using System.Threading;

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var pst = await PstFile.OpenAsync(@"C:\data\mail.pst", cancellationToken: cts.Token);
```

## Lanjutkan

- [Konsep Arsitektur](./concepts.md)
- [API: PstFile](../api/Emcode.Pst.Application/PstFile.md)
