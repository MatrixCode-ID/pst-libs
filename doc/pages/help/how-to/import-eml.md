# How-To: Import EML

## Langkah

1. Buka PST dengan writer aktif.
2. Panggil `ImportEml(folder, emlPath)`.
3. Gunakan versi async untuk workflow I/O berat.

## Contoh

```csharp
using Emcode.Pst.Application;
using Emcode.Pst.Infrastructure;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions { ReadOnly = false }, writer: new PstInMemoryWriter());
var target = pst.CreateFolder("EML Import");
var message = pst.ImportEml(target, @"C:\temp\sample.eml");
```

## API Terkait

- [PstFile](../../api/Emcode.Pst.Application/PstFile.md)
- [IPstWriter](../../api/Emcode.Pst.Application.Abstractions/IPstWriter.md)
- [PstNdbWriter](../../api/Emcode.Pst.Infrastructure/PstNdbWriter.md)
