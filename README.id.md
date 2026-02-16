# Emcode.Pst

Library C# untuk membaca file Microsoft Outlook PST (Personal Storage Table) dengan fokus pada read MVP dan fondasi untuk pengembangan write.

**Kemampuan Saat Ini**
- Membuka PST ANSI/Unicode dan membaca metadata header (format, versi, ukuran, metode crypt).
- Membaca hierarki folder (root + subfolder) dari NDB.
- Membaca message per folder menggunakan Contents Table (urutan sesuai tabel bila tersedia).
- Mengambil properti message inti dan extended (mis. Subject, DeliveryTime, SenderName, Body, HtmlBody, message flags, receipt flags, importance/priority/sensitivity, conversation topic/index, transport headers).
- Membaca metadata attachment dan konten attachment sebagai Stream/byte[] bila tersedia.
- Mendukung API sinkron `PstFile.Open` dan asinkron `PstFile.OpenAsync`.
- Mendukung write draft (in-memory dan persist ke disk) untuk create folder/message, import `.eml` berbasis path file, serta mapping MAPI draft yang mencakup `PidTagMessageClass`, `PidTagMessageFlags`, timestamp utama, conversation topic/index, transport headers, receipt flags, importance/priority/sensitivity, dan properti recipient tambahan (`DisplayName`, `AddrType`).

**Quick Start (Sync)**
```csharp
using Emcode.Pst.Application;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = false
});

Console.WriteLine($"Folders: {pst.Folders.Count}");

foreach (var folder in pst.Folders)
{
    Console.WriteLine($"- {folder.Name} ({folder.Messages.Count} messages)");
    foreach (var message in folder.Messages)
    {
        Console.WriteLine($"  {message.Subject} | {message.SenderName}");
    }
}
```

**Quick Start (Async)**
```csharp
using System.Threading;
using Emcode.Pst.Application;

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var pst = await PstFile.OpenAsync(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = false
}, cancellationToken: cts.Token);
```

**Membaca Konten Attachment (Sync)**
```csharp
using System.IO;
using System.Linq;
using Emcode.Pst.Application;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = false
});

foreach (var message in pst.Folders.SelectMany(folder => folder.Messages))
{
    foreach (var attachment in message.Attachments)
    {
        var bytes = attachment.ReadContentBytes();
        if (bytes is null)
        {
            continue;
        }

        var fileName = attachment.LongFileName ?? attachment.FileName ?? "attachment.bin";
        var targetPath = Path.Combine(@"C:\export", fileName);
        File.WriteAllBytes(targetPath, bytes);
    }
}
```

**Membaca Konten Attachment (Async)**
```csharp
using System.IO;
using System.Linq;
using Emcode.Pst.Application;

using var pst = await PstFile.OpenAsync(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = false
});

foreach (var message in pst.Folders.SelectMany(folder => folder.Messages))
{
    foreach (var attachment in message.Attachments)
    {
        await using var content = await attachment.OpenContentStreamAsync();
        if (content is null)
        {
            continue;
        }

        var fileName = attachment.LongFileName ?? attachment.FileName ?? "attachment.bin";
        var targetPath = Path.Combine(@"C:\export", fileName);
        await using var file = File.Create(targetPath);
        await content.CopyToAsync(file);
    }
}
```

Catatan: konten attachment dibaca dari `PidTagAttachDataBinary`. Attachment bertipe pesan (`PidTagAttachDataObject`) belum didukung.

**Menulis Draft (In-Memory)**
```csharp
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = false,
    ValidateChecksums = false
}, writer: new PstInMemoryWriter());

var drafts = pst.Folders.FirstOrDefault(f => f.Name == "Drafts") ?? pst.RootFolder!;
var draft = new PstMessageDraft
{
    FromName = "Tester",
    FromAddress = "tester@example.com",
    Subject = "Draft Manual",
    Body = "Halo dari draft",
    HtmlBody = "<b>Halo dari draft</b>",
    Recipients = new[]
    {
        new PstDraftRecipient { RecipientType = PstRecipientType.To, EmailAddress = "target@example.com" }
    }
};

var message = pst.CreateMessage(drafts, draft);
```

**Menulis Draft (Persist ke Disk)**
```csharp
using Emcode.Pst.Application;
using Emcode.Pst.Domain;
using Emcode.Pst.Infrastructure.Ndb;

using var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = false,
    ValidateChecksums = false
}, writer: new PstNdbWriter());

var targetFolder = pst.Folders.First();
var draft = new PstMessageDraft
{
    Subject = "Draft Persist",
    Body = "Konten disimpan ke PST",
    Recipients = new[]
    {
        new PstDraftRecipient { RecipientType = PstRecipientType.To, EmailAddress = "target@example.com" }
    }
};

var message = pst.CreateMessage(targetFolder, draft);
```

**Import .eml (In-Memory)**
```csharp
using Emcode.Pst.Application;
using Emcode.Pst.Infrastructure;

var pst = PstFile.Open(@"C:\data\mail.pst", new PstOpenOptions
{
    ReadOnly = false,
    ValidateChecksums = false
}, writer: new PstInMemoryWriter());

var folder = pst.Folders.First();
var message = pst.ImportEml(folder, @"C:\data\mail.eml");
```

**Matriks Fitur**
- `Open/Read PST (sync/async)`: Supported
- `Create Folder/Message (in-memory)`: Supported
- `Create Folder/Message (persist disk via PstNdbWriter)`: Supported (eksperimental)
- `Import .eml dari path file (sync/async)`: Supported
- `Import .eml dari Stream`: Not Yet
- `Update/Delete message (in-memory writer)`: Supported
- `Update/Delete message (PstNdbWriter persist)`: Not Yet
- `Attachment content read (sync/async)`: Supported
- `Attachment message object (PidTagAttachDataObject)`: Not Yet
- `Crypt method None/Permute`: Supported
- `Crypt method Cyclic/EdpEncrypted`: Not Yet
- `Data tree write di atas XXBLOCK`: Not Yet

**Opsi Pembukaan**
- `ReadOnly` (default: true)
- `ValidateChecksums` (default: true)
- `AllowAnsi` (default: true)
- `AllowUnicode` (default: true)

**Batasan Saat Ini**
- `PstNdbWriter` sudah mendukung create folder/message dan persist ke disk, namun masih eksperimental.
- PC/TC writer masih single-block; data besar (body/attachment) bisa gagal jika melebihi ukuran block PST.
- Subnode attachment hanya disimpan bila `ContentBytes` tersedia.
- Update/delete message pada `PstNdbWriter` belum didukung (pada `PstInMemoryWriter` didukung).
- Update BBT/NBT dilakukan dengan append block baru (belum ada reuse free-space).
- Import `.eml` saat ini hanya mendukung input path file (`string emlPath`), belum mendukung input `Stream`.
- Import `.eml` via `PstNdbWriter` sudah mempersist ke disk dengan mapping MAPI draft yang lebih lengkap, namun belum mencakup seluruh properti Outlook tingkat lanjut.
- Method crypt `Cyclic` dan `EdpEncrypted` belum didukung.
- Writer data tree lebih dalam dari `XXBLOCK` belum didukung.
- Sinkronisasi folder lokal ke PST belum tersedia.

**Roadmap (Goal Project)**
- Intergrasi dengan NUGET.
- Buat pages batuan object references.
- Baca file PST (tersedia untuk read MVP).
- Buat folder import dan import file `.eml` ke folder tersebut.
- Sinkronisasi folder lokal dengan PST.

**Referensi**
- [MS-PST: Outlook Personal Folders (.pst) File Format](https://learn.microsoft.com/en-us/openspecs/office_file_formats/ms-pst/141923d5-15ab-4ef1-a524-6dce75aae546)
