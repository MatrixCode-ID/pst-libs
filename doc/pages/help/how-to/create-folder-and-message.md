# How-To: Create Folder and Message

## Langkah

1. Buka PST dengan writer (`PstInMemoryWriter` atau `PstNdbWriter`).
2. Buat folder dengan `CreateFolder`.
3. Buat draft pesan (`PstMessageDraft`).
4. Tulis pesan dengan `CreateMessage`.

## Contoh

```csharp
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions { ReadOnly = false }, writer: new PstInMemoryWriter());

var folder = pst.CreateFolder("Import");
var msg = pst.CreateMessage(folder, new PstMessageDraft
{
    Subject = "Test",
    Body = "Halo PST"
});
```

## API Terkait

- [PstFile](../../api/Emcode.Pst.Application/PstFile.md)
- [PstMessageDraft](../../api/Emcode.Pst.Domain/PstMessageDraft.md)
- [PstInMemoryWriter](../../api/Emcode.Pst.Infrastructure/PstInMemoryWriter.md)
