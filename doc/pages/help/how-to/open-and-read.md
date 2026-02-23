# How-To: Open and Read PST

## Langkah

1. Panggil `PstFile.Open` atau `PstFile.OpenAsync`.
2. Gunakan `pst.Folders` untuk iterasi folder.
3. Ambil `folder.Messages` untuk membaca pesan.

## Contoh

```csharp
using Emcode.Pst.Application;

var pst = PstFile.Open(@"C:\data\inbox.pst");

foreach (var folder in pst.Folders)
{
    foreach (var message in folder.Messages)
    {
        Console.WriteLine(message.Subject);
    }
}
```

## API Terkait

- [PstFile](../../api/Emcode.Pst.Application/PstFile.md)
- [PstFolder](../../api/Emcode.Pst.Domain/PstFolder.md)
- [PstMessage](../../api/Emcode.Pst.Domain/PstMessage.md)
