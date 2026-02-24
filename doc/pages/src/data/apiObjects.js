export const apiObjects = [
  {
    "namespace": "Emcode.Pst.Application",
    "type": "PstFile",
    "summary": "Facade utama untuk membuka, membaca, dan menulis file PST.",
    "signature": "public sealed class PstFile : IDisposable",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "Path",
        "type": "string",
        "description": "Lokasi file PST yang dibuka."
      },
      {
        "name": "Options",
        "type": "PstOpenOptions",
        "description": "Opsi pembukaan file PST yang digunakan."
      },
      {
        "name": "Folders",
        "type": "IReadOnlyList<PstFolder>",
        "description": "Daftar folder hasil pembacaan PST."
      },
      {
        "name": "RootFolder",
        "type": "PstFolder?",
        "description": "Folder root PST jika tersedia."
      },
      {
        "name": "Header",
        "type": "PstHeaderInfo?",
        "description": "Metadata header PST hasil pembacaan awal."
      }
    ],
    "methods": [
      {
        "signature": "Open(string path, PstOpenOptions? options = null, IPstReader? reader = null, IPstWriter? writer = null)",
        "returnType": "PstFile",
        "description": "Membuka file PST dengan reader dan writer opsional."
      },
      {
        "signature": "CreateFolder(string name, PstFolder? parent = null)",
        "returnType": "PstFolder",
        "description": "Membuat folder baru di PST dengan parent opsional."
      },
      {
        "signature": "CreateFolderAsync(string name, PstFolder? parent = null, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstFolder>",
        "description": "Membuat folder baru di PST secara asynchronous dengan parent opsional."
      },
      {
        "signature": "CreateMessage(PstFolder folder, PstMessageDraft draft)",
        "returnType": "PstMessage",
        "description": "Membuat pesan baru di folder tertentu."
      },
      {
        "signature": "CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Membuat pesan baru di folder tertentu secara asynchronous."
      },
      {
        "signature": "ImportEml(PstFolder folder, string emlPath)",
        "returnType": "PstMessage",
        "description": "Mengimpor file .eml ke folder tertentu sebagai pesan baru."
      },
      {
        "signature": "ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Mengimpor file .eml ke folder tertentu sebagai pesan baru secara asynchronous."
      },
      {
        "signature": "UpdateMessage(PstMessage message, PstMessageDraft draft)",
        "returnType": "void",
        "description": "Memperbarui pesan yang sudah ada dengan draft terbaru."
      },
      {
        "signature": "UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Memperbarui pesan yang sudah ada secara asynchronous."
      },
      {
        "signature": "DeleteMessage(PstMessage message)",
        "returnType": "void",
        "description": "Menghapus pesan dari PST."
      },
      {
        "signature": "DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menghapus pesan dari PST secara asynchronous."
      },
      {
        "signature": "Dispose()",
        "returnType": "void",
        "description": "Melepas resource yang digunakan oleh PST."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Application",
    "type": "PstOpenOptions",
    "summary": "Opsi pembukaan file PST untuk mengatur mode baca dan validasi.",
    "signature": "public sealed class PstOpenOptions",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "ReadOnly",
        "type": "bool",
        "description": "Menentukan apakah file dibuka dalam mode hanya-baca."
      },
      {
        "name": "ValidateChecksums",
        "type": "bool",
        "description": "Menentukan apakah checksum blok divalidasi saat membaca."
      },
      {
        "name": "AllowAnsi",
        "type": "bool",
        "description": "Mengizinkan pembacaan PST format ANSI."
      },
      {
        "name": "AllowUnicode",
        "type": "bool",
        "description": "Mengizinkan pembacaan PST format Unicode."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Application.Abstractions",
    "type": "IPstReader",
    "summary": "Kontrak untuk membaca struktur PST dari sumber penyimpanan.",
    "signature": "public interface IPstReader",
    "kind": "type",
    "constructors": [],
    "properties": [],
    "methods": [
      {
        "signature": "Read(string path, PstOpenOptions options)",
        "returnType": "PstReadResult",
        "description": "Membaca file PST dan mengembalikan hasil struktur dasarnya."
      },
      {
        "signature": "ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstReadResult>",
        "description": "Membaca file PST secara asynchronous dan mengembalikan hasil struktur dasarnya."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Application.Abstractions",
    "type": "IPstWriter",
    "summary": "Kontrak untuk operasi write pada PST.",
    "signature": "public interface IPstWriter",
    "kind": "type",
    "constructors": [],
    "properties": [],
    "methods": [
      {
        "signature": "CreateFolder(string name, PstFolder? parent)",
        "returnType": "PstFolder",
        "description": "Membuat folder baru pada PST."
      },
      {
        "signature": "CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstFolder>",
        "description": "Membuat folder baru pada PST secara asynchronous."
      },
      {
        "signature": "CreateMessage(PstFolder folder, PstMessageDraft draft)",
        "returnType": "PstMessage",
        "description": "Membuat pesan baru pada folder tertentu."
      },
      {
        "signature": "CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Membuat pesan baru pada folder tertentu secara asynchronous."
      },
      {
        "signature": "ImportEml(PstFolder folder, string emlPath)",
        "returnType": "PstMessage",
        "description": "Mengimpor file .eml ke folder PST sebagai pesan baru."
      },
      {
        "signature": "ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Mengimpor file .eml ke folder PST sebagai pesan baru secara asynchronous."
      },
      {
        "signature": "UpdateMessage(PstMessage message, PstMessageDraft draft)",
        "returnType": "void",
        "description": "Memperbarui pesan yang sudah ada."
      },
      {
        "signature": "UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Memperbarui pesan yang sudah ada secara asynchronous."
      },
      {
        "signature": "DeleteMessage(PstMessage message)",
        "returnType": "void",
        "description": "Menghapus pesan dari PST."
      },
      {
        "signature": "DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menghapus pesan dari PST secara asynchronous."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Application.Abstractions",
    "type": "IPstWriterWithContext",
    "summary": "Kontrak untuk writer yang membutuhkan konteks PST sebelum operasi write.",
    "signature": "public interface IPstWriterWithContext",
    "kind": "type",
    "constructors": [],
    "properties": [],
    "methods": [
      {
        "signature": "Initialize(PstWriteContext context)",
        "returnType": "void",
        "description": "Menginisialisasi writer dengan konteks PST yang sedang dibuka."
      },
      {
        "signature": "InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menginisialisasi writer dengan konteks PST secara asynchronous."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Application.Abstractions",
    "type": "PstReadResult",
    "summary": "Hasil pembacaan PST yang memuat metadata header, folder root, dan daftar folder.",
    "signature": "public sealed class PstReadResult",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstReadResult(PstHeaderInfo? header, PstFolder? rootFolder, IReadOnlyList<PstFolder> folders)",
        "description": "Membuat hasil pembacaan dengan metadata header, root folder, dan daftar folder."
      }
    ],
    "properties": [
      {
        "name": "Header",
        "type": "PstHeaderInfo?",
        "description": "Metadata header PST hasil pembacaan."
      },
      {
        "name": "RootFolder",
        "type": "PstFolder?",
        "description": "Folder root jika ditemukan."
      },
      {
        "name": "Folders",
        "type": "IReadOnlyList<PstFolder>",
        "description": "Daftar folder hasil pembacaan."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Application.Abstractions",
    "type": "PstWriteContext",
    "summary": "Konteks data PST yang dibutuhkan oleh writer untuk operasi write.",
    "signature": "public sealed class PstWriteContext",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstWriteContext(string path, PstOpenOptions options, PstFolder? rootFolder, List<PstFolder> folders)",
        "description": "Membuat konteks write dengan data PST yang sudah dibaca."
      }
    ],
    "properties": [
      {
        "name": "Path",
        "type": "string",
        "description": "Lokasi file PST yang sedang dibuka."
      },
      {
        "name": "Options",
        "type": "PstOpenOptions",
        "description": "Opsi pembukaan PST yang digunakan."
      },
      {
        "name": "RootFolder",
        "type": "PstFolder?",
        "description": "Folder root PST bila tersedia."
      },
      {
        "name": "Folders",
        "type": "List<PstFolder>",
        "description": "Daftar folder yang dapat diperbarui oleh writer."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstAttachment",
    "summary": "Representasi attachment pesan pada PST berdasarkan Attachment Table.",
    "signature": "public sealed class PstAttachment",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstAttachment()",
        "description": "Membuat instance attachment dengan data minimal."
      }
    ],
    "properties": [
      {
        "name": "AttachNumber",
        "type": "int?",
        "description": "Nomor attachment sesuai PidTagAttachNumber."
      },
      {
        "name": "FileName",
        "type": "string?",
        "description": "Nama file attachment singkat."
      },
      {
        "name": "LongFileName",
        "type": "string?",
        "description": "Nama file attachment lengkap (long filename)."
      },
      {
        "name": "Size",
        "type": "int?",
        "description": "Ukuran attachment dalam byte."
      },
      {
        "name": "MimeTag",
        "type": "string?",
        "description": "MIME tag attachment bila tersedia."
      },
      {
        "name": "ContentId",
        "type": "string?",
        "description": "Content-Id attachment untuk inline reference."
      },
      {
        "name": "AttachMethod",
        "type": "int?",
        "description": "Metode attachment sesuai PidTagAttachMethod."
      }
    ],
    "methods": [
      {
        "signature": "OpenContentStream()",
        "returnType": "Stream?",
        "description": "Membuka stream konten attachment secara sinkron."
      },
      {
        "signature": "OpenContentStreamAsync(CancellationToken cancellationToken = default)",
        "returnType": "Task<Stream?>",
        "description": "Membuka stream konten attachment secara asynchronous."
      },
      {
        "signature": "ReadContentBytes()",
        "returnType": "byte[]?",
        "description": "Membaca konten attachment sebagai byte array secara sinkron."
      },
      {
        "signature": "ReadContentBytesAsync(CancellationToken cancellationToken = default)",
        "returnType": "Task<byte[]?>",
        "description": "Membaca konten attachment sebagai byte array secara asynchronous."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstCryptMethod",
    "summary": "Menentukan metode enkripsi/encoding data blok pada PST.",
    "signature": "public enum PstCryptMethod",
    "kind": "enum",
    "constructors": [],
    "properties": [],
    "methods": [],
    "events": [],
    "fields": [
      {
        "name": "None",
        "value": "0x00",
        "description": "Data blok tidak dienkode atau dienkripsi."
      },
      {
        "name": "Permute",
        "value": "0x01",
        "description": "Data blok dienkode menggunakan algoritma Permutation."
      },
      {
        "name": "Cyclic",
        "value": "0x02",
        "description": "Data blok dienkode menggunakan algoritma Cyclic."
      },
      {
        "name": "EdpEncrypted",
        "value": "0x10",
        "description": "Data blok dienkripsi dengan Windows Information Protection (EDP)."
      }
    ]
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstDraftAttachment",
    "summary": "Representasi attachment untuk draft pesan sebelum ditulis ke PST.",
    "signature": "public sealed class PstDraftAttachment",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "FileName",
        "type": "string?",
        "description": "Nama file attachment."
      },
      {
        "name": "LongFileName",
        "type": "string?",
        "description": "Nama file attachment versi panjang bila tersedia."
      },
      {
        "name": "ContentType",
        "type": "string?",
        "description": "Content type attachment (MIME)."
      },
      {
        "name": "ContentId",
        "type": "string?",
        "description": "Content-Id attachment untuk inline reference."
      },
      {
        "name": "IsInline",
        "type": "bool",
        "description": "Menandakan attachment inline."
      },
      {
        "name": "ContentBytes",
        "type": "byte[]?",
        "description": "Konten attachment sebagai byte array."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstDraftRecipient",
    "summary": "Representasi penerima untuk draft pesan sebelum ditulis ke PST.",
    "signature": "public sealed class PstDraftRecipient",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "RecipientType",
        "type": "PstRecipientType",
        "description": "Jenis penerima (To/Cc/Bcc)."
      },
      {
        "name": "DisplayName",
        "type": "string?",
        "description": "Nama tampilan penerima bila tersedia."
      },
      {
        "name": "EmailAddress",
        "type": "string?",
        "description": "Alamat email penerima."
      },
      {
        "name": "SmtpAddress",
        "type": "string?",
        "description": "Alamat SMTP penerima bila tersedia."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstFolder",
    "summary": "Representasi folder di dalam PST.",
    "signature": "public sealed class PstFolder",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "Id",
        "type": "string",
        "description": "Identifier internal folder."
      },
      {
        "name": "Name",
        "type": "string",
        "description": "Nama folder."
      },
      {
        "name": "SubFolders",
        "type": "IReadOnlyList<PstFolder>",
        "description": "Subfolder di bawah folder ini."
      },
      {
        "name": "Messages",
        "type": "IReadOnlyList<PstMessage>",
        "description": "Daftar pesan yang berada di folder ini."
      }
    ],
    "methods": [
      {
        "signature": "EnumerateMessages()",
        "returnType": "IEnumerable<PstMessage>",
        "description": "Mengambil daftar pesan yang ada di folder ini."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstFormat",
    "summary": "Menentukan format PST yang terdeteksi dari header file.",
    "signature": "public enum PstFormat",
    "kind": "enum",
    "constructors": [],
    "properties": [],
    "methods": [],
    "events": [],
    "fields": [
      {
        "name": "Unknown",
        "value": "0",
        "description": "Format tidak diketahui atau belum terdeteksi."
      },
      {
        "name": "Ansi",
        "value": "1",
        "description": "PST format ANSI (versi lama)."
      },
      {
        "name": "Unicode",
        "value": "2",
        "description": "PST format Unicode (versi baru)."
      }
    ]
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstHeaderInfo",
    "summary": "Metadata header PST hasil pembacaan awal file.",
    "signature": "public sealed class PstHeaderInfo",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstHeaderInfo(uint signature, uint clientSignature, ushort version, ushort versionMinor, long fileSize, PstFormat format, PstCryptMethod cryptMethod)",
        "description": "Membuat metadata header PST."
      }
    ],
    "properties": [
      {
        "name": "Signature",
        "type": "uint",
        "description": "Signature file PST (magic number)."
      },
      {
        "name": "ClientSignature",
        "type": "uint",
        "description": "Signature client PST."
      },
      {
        "name": "Version",
        "type": "ushort",
        "description": "Versi utama file PST."
      },
      {
        "name": "VersionMinor",
        "type": "ushort",
        "description": "Versi minor file PST."
      },
      {
        "name": "FileSize",
        "type": "long",
        "description": "Ukuran file PST dalam byte."
      },
      {
        "name": "Format",
        "type": "PstFormat",
        "description": "Format PST hasil deteksi."
      },
      {
        "name": "CryptMethod",
        "type": "PstCryptMethod",
        "description": "Metode enkripsi/encoding data blok pada PST."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstMessage",
    "summary": "Representasi pesan email di dalam PST.",
    "signature": "public sealed class PstMessage",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "Id",
        "type": "string",
        "description": "Identifier internal pesan."
      },
      {
        "name": "Subject",
        "type": "string?",
        "description": "Subjek pesan."
      },
      {
        "name": "MessageClass",
        "type": "string?",
        "description": "Message class MAPI (contoh: IPM.Note)."
      },
      {
        "name": "Body",
        "type": "string?",
        "description": "Body teks biasa."
      },
      {
        "name": "HtmlBody",
        "type": "string?",
        "description": "Body HTML bila tersedia."
      },
      {
        "name": "SenderName",
        "type": "string?",
        "description": "Nama pengirim."
      },
      {
        "name": "DeliveryTime",
        "type": "DateTimeOffset?",
        "description": "Waktu pengiriman pesan."
      },
      {
        "name": "Size",
        "type": "int?",
        "description": "Ukuran pesan dalam byte bila tersedia."
      },
      {
        "name": "InternetMessageId",
        "type": "string?",
        "description": "Internet Message-Id dari header MIME."
      },
      {
        "name": "SenderEmailAddress",
        "type": "string?",
        "description": "Alamat email pengirim sesuai MAPI."
      },
      {
        "name": "SenderSmtpAddress",
        "type": "string?",
        "description": "Alamat SMTP pengirim bila tersedia."
      },
      {
        "name": "SentRepresentingName",
        "type": "string?",
        "description": "Nama yang direpresentasikan saat pengiriman (send on behalf)."
      },
      {
        "name": "SentRepresentingEmailAddress",
        "type": "string?",
        "description": "Alamat email yang direpresentasikan saat pengiriman."
      },
      {
        "name": "OriginalSenderName",
        "type": "string?",
        "description": "Nama pengirim asli sebelum perubahan/forward."
      },
      {
        "name": "OriginalSenderEmailAddress",
        "type": "string?",
        "description": "Alamat email pengirim asli sebelum perubahan/forward."
      },
      {
        "name": "DisplayTo",
        "type": "string?",
        "description": "Daftar penerima pada field To."
      },
      {
        "name": "DisplayCc",
        "type": "string?",
        "description": "Daftar penerima pada field Cc."
      },
      {
        "name": "DisplayBcc",
        "type": "string?",
        "description": "Daftar penerima pada field Bcc."
      },
      {
        "name": "ReceivedTime",
        "type": "DateTimeOffset?",
        "description": "Waktu pesan diterima (delivery time)."
      },
      {
        "name": "ClientSubmitTime",
        "type": "DateTimeOffset?",
        "description": "Waktu submit client ke transport."
      },
      {
        "name": "MessageSubmissionId",
        "type": "ReadOnlyMemory<byte>?",
        "description": "ID submit pesan untuk tracking transport."
      },
      {
        "name": "LastModificationTime",
        "type": "DateTimeOffset?",
        "description": "Waktu modifikasi terakhir pesan."
      },
      {
        "name": "MessageFlags",
        "type": "int?",
        "description": "Flag status pesan (bitmask)."
      },
      {
        "name": "ReadReceiptRequested",
        "type": "bool?",
        "description": "Menandakan permintaan read receipt."
      },
      {
        "name": "DeliveryReceiptRequested",
        "type": "bool?",
        "description": "Menandakan permintaan delivery receipt."
      },
      {
        "name": "HasAttachments",
        "type": "bool?",
        "description": "Menandakan pesan memiliki attachment."
      },
      {
        "name": "Importance",
        "type": "int?",
        "description": "Tingkat importance pesan."
      },
      {
        "name": "Priority",
        "type": "int?",
        "description": "Prioritas pesan."
      },
      {
        "name": "Sensitivity",
        "type": "int?",
        "description": "Tingkat sensitivitas pesan."
      },
      {
        "name": "TransportMessageHeaders",
        "type": "string?",
        "description": "Header transport mentah (RFC822) bila tersedia."
      },
      {
        "name": "ConversationTopic",
        "type": "string?",
        "description": "Topik percakapan (thread topic)."
      },
      {
        "name": "ConversationIndex",
        "type": "ReadOnlyMemory<byte>?",
        "description": "Indeks percakapan (thread index) dalam bentuk biner."
      },
      {
        "name": "Recipients",
        "type": "IReadOnlyList<PstRecipient>",
        "description": "Daftar penerima pesan bila tersedia."
      },
      {
        "name": "Attachments",
        "type": "IReadOnlyList<PstAttachment>",
        "description": "Daftar attachment pesan bila tersedia."
      }
    ],
    "methods": [
      {
        "signature": "Update(PstMessageDraft draft)",
        "returnType": "void",
        "description": "Memperbarui data pesan."
      },
      {
        "signature": "Delete()",
        "returnType": "void",
        "description": "Menghapus pesan dari PST."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstMessageDraft",
    "summary": "Draft data pesan yang akan dibuat atau diperbarui.",
    "signature": "public sealed class PstMessageDraft",
    "kind": "type",
    "constructors": [],
    "properties": [
      {
        "name": "MessageClass",
        "type": "string?",
        "description": "Message class MAPI (contoh: IPM.Note)."
      },
      {
        "name": "FromName",
        "type": "string?",
        "description": "Nama pengirim (display name)."
      },
      {
        "name": "FromAddress",
        "type": "string?",
        "description": "Alamat email pengirim."
      },
      {
        "name": "Subject",
        "type": "string?",
        "description": "Subjek pesan."
      },
      {
        "name": "Body",
        "type": "string?",
        "description": "Body teks biasa."
      },
      {
        "name": "HtmlBody",
        "type": "string?",
        "description": "Body HTML bila tersedia."
      },
      {
        "name": "MessageId",
        "type": "string?",
        "description": "Message-Id dari header MIME bila tersedia."
      },
      {
        "name": "SentTime",
        "type": "DateTimeOffset?",
        "description": "Waktu pengiriman pesan (tanggal pada header)."
      },
      {
        "name": "ClientSubmitTime",
        "type": "DateTimeOffset?",
        "description": "Waktu submit pesan dari client ke transport."
      },
      {
        "name": "LastModificationTime",
        "type": "DateTimeOffset?",
        "description": "Waktu modifikasi terakhir pesan."
      },
      {
        "name": "MessageFlags",
        "type": "int?",
        "description": "Flag status message dalam bentuk bitmask MAPI."
      },
      {
        "name": "IsDraft",
        "type": "bool",
        "description": "Menandakan pesan ini diperlakukan sebagai draft."
      },
      {
        "name": "ReadReceiptRequested",
        "type": "bool?",
        "description": "Menandakan permintaan read receipt."
      },
      {
        "name": "DeliveryReceiptRequested",
        "type": "bool?",
        "description": "Menandakan permintaan delivery receipt."
      },
      {
        "name": "Importance",
        "type": "int?",
        "description": "Tingkat importance pesan."
      },
      {
        "name": "Priority",
        "type": "int?",
        "description": "Prioritas pesan."
      },
      {
        "name": "Sensitivity",
        "type": "int?",
        "description": "Tingkat sensitivitas pesan."
      },
      {
        "name": "TransportMessageHeaders",
        "type": "string?",
        "description": "Header transport mentah (RFC822) bila tersedia."
      },
      {
        "name": "ConversationTopic",
        "type": "string?",
        "description": "Topik percakapan (thread topic)."
      },
      {
        "name": "ConversationIndex",
        "type": "byte[]?",
        "description": "Indeks percakapan (thread index) dalam bentuk biner."
      },
      {
        "name": "Recipients",
        "type": "IReadOnlyList<PstDraftRecipient>",
        "description": "Daftar penerima pesan (To/Cc/Bcc)."
      },
      {
        "name": "Attachments",
        "type": "IReadOnlyList<PstDraftAttachment>",
        "description": "Daftar attachment untuk pesan."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstRecipient",
    "summary": "Representasi penerima pesan pada PST berdasarkan Recipient Table.",
    "signature": "public sealed class PstRecipient",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstRecipient()",
        "description": "Membuat instance penerima dengan data minimal."
      }
    ],
    "properties": [
      {
        "name": "RecipientType",
        "type": "int?",
        "description": "Jenis penerima (To, Cc, Bcc) sesuai PidTagRecipientType."
      },
      {
        "name": "EmailAddress",
        "type": "string?",
        "description": "Alamat email penerima sesuai PidTagEmailAddress."
      },
      {
        "name": "DisplayName",
        "type": "string?",
        "description": "Nama tampilan penerima bila tersedia."
      },
      {
        "name": "AddressType",
        "type": "string?",
        "description": "Tipe alamat penerima (contoh: SMTP)."
      },
      {
        "name": "SmtpAddress",
        "type": "string?",
        "description": "Alamat SMTP penerima bila tersedia."
      }
    ],
    "methods": [],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Domain",
    "type": "PstRecipientType",
    "summary": "Jenis penerima pesan sesuai konvensi MAPI (To, Cc, Bcc).",
    "signature": "public enum PstRecipientType",
    "kind": "enum",
    "constructors": [],
    "properties": [],
    "methods": [],
    "events": [],
    "fields": [
      {
        "name": "To",
        "value": "1",
        "description": "Penerima utama (To)."
      },
      {
        "name": "Cc",
        "value": "2",
        "description": "Penerima tembusan (Cc)."
      },
      {
        "name": "Bcc",
        "value": "3",
        "description": "Penerima blind carbon copy (Bcc)."
      }
    ]
  },
  {
    "namespace": "Emcode.Pst.Infrastructure",
    "type": "PstInMemoryWriter",
    "summary": "Implementasi writer berbasis in-memory untuk membuat draft pesan tanpa menulis ke disk.",
    "signature": "public sealed class PstInMemoryWriter : IPstWriter, IPstWriterWithContext",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstInMemoryWriter()",
        "description": "Membuat writer in-memory dengan parser .eml default."
      }
    ],
    "properties": [],
    "methods": [
      {
        "signature": "Initialize(PstWriteContext context)",
        "returnType": "void",
        "description": "Menginisialisasi writer dengan konteks PST."
      },
      {
        "signature": "InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menginisialisasi writer dengan konteks PST secara asynchronous."
      },
      {
        "signature": "CreateFolder(string name, PstFolder? parent)",
        "returnType": "PstFolder",
        "description": "Membuat folder baru pada PST secara in-memory."
      },
      {
        "signature": "CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstFolder>",
        "description": "Membuat folder baru pada PST secara in-memory (async)."
      },
      {
        "signature": "CreateMessage(PstFolder folder, PstMessageDraft draft)",
        "returnType": "PstMessage",
        "description": "Membuat pesan baru pada folder tertentu secara in-memory."
      },
      {
        "signature": "CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Membuat pesan baru pada folder tertentu secara in-memory (async)."
      },
      {
        "signature": "ImportEml(PstFolder folder, string emlPath)",
        "returnType": "PstMessage",
        "description": "Mengimpor file .eml ke folder PST sebagai pesan baru (in-memory)."
      },
      {
        "signature": "ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Mengimpor file .eml ke folder PST sebagai pesan baru secara asynchronous (in-memory)."
      },
      {
        "signature": "UpdateMessage(PstMessage message, PstMessageDraft draft)",
        "returnType": "void",
        "description": "Memperbarui pesan yang sudah ada secara in-memory."
      },
      {
        "signature": "UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Memperbarui pesan yang sudah ada secara asynchronous (in-memory)."
      },
      {
        "signature": "DeleteMessage(PstMessage message)",
        "returnType": "void",
        "description": "Menghapus pesan dari folder secara in-memory."
      },
      {
        "signature": "DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menghapus pesan dari folder secara asynchronous (in-memory)."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Infrastructure",
    "type": "PstMinimalReader",
    "summary": "Reader minimal yang hanya memvalidasi header PST dan mengembalikan metadata dasar. Enumerasi folder/message masih placeholder (belum parsing NDB).",
    "signature": "public sealed class PstMinimalReader : IPstReader",
    "kind": "type",
    "constructors": [],
    "properties": [],
    "methods": [
      {
        "signature": "Read(string path, PstOpenOptions options)",
        "returnType": "PstReadResult",
        "description": "Membaca header PST dan mengembalikan metadata dasar."
      },
      {
        "signature": "ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstReadResult>",
        "description": "Membaca header PST secara asynchronous dan mengembalikan metadata dasar."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Infrastructure",
    "type": "PstNdbReader",
    "summary": "Reader PST berbasis parsing NDB untuk mengekstrak folder dan message nyata.",
    "signature": "public sealed class PstNdbReader : IPstReader",
    "kind": "type",
    "constructors": [],
    "properties": [],
    "methods": [
      {
        "signature": "Read(string path, PstOpenOptions options)",
        "returnType": "PstReadResult",
        "description": "Membaca PST menggunakan parsing NDB, BBT/NBT, dan PC."
      },
      {
        "signature": "ReadAsync(string path, PstOpenOptions options, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstReadResult>",
        "description": "Membaca PST secara asynchronous menggunakan parsing NDB, BBT/NBT, dan PC."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Infrastructure.Ltp",
    "type": "TableCell",
    "summary": "Nilai cell untuk row table.",
    "signature": "public readonly struct TableCell",
    "kind": "type",
    "constructors": [
      {
        "signature": "TableCell(ushort propertyId, PstPropertyType propType, object value)",
        "description": "Membuat cell untuk table row."
      }
    ],
    "properties": [
      {
        "name": "PropertyId",
        "type": "ushort",
        "description": "Property id."
      },
      {
        "name": "PropType",
        "type": "PstPropertyType",
        "description": "Property type."
      },
      {
        "name": "Value",
        "type": "object",
        "description": "Nilai cell."
      }
    ],
    "methods": [
      {
        "signature": "GetString()",
        "returnType": "string",
        "description": "Mengambil nilai string."
      },
      {
        "signature": "GetBinary()",
        "returnType": "ReadOnlyMemory<byte>",
        "description": "Mengambil nilai biner."
      },
      {
        "signature": "GetInt32()",
        "returnType": "int",
        "description": "Mengambil nilai integer 32-bit."
      },
      {
        "signature": "GetBoolean()",
        "returnType": "bool",
        "description": "Mengambil nilai boolean."
      },
      {
        "signature": "GetDateTime()",
        "returnType": "DateTimeOffset",
        "description": "Mengambil nilai DateTimeOffset."
      }
    ],
    "events": [],
    "fields": []
  },
  {
    "namespace": "Emcode.Pst.Infrastructure.Ndb",
    "type": "PstNdbWriter",
    "summary": "Implementasi writer PST berbasis NDB untuk persist ke disk (eksperimental).",
    "signature": "public sealed class PstNdbWriter : IPstWriter, IPstWriterWithContext, IDisposable",
    "kind": "type",
    "constructors": [
      {
        "signature": "PstNdbWriter()",
        "description": "Membuat writer NDB dengan opsi format Unicode default."
      },
      {
        "signature": "PstNdbWriter(PstFormat format)",
        "description": "Membuat writer NDB dengan opsi LTP default."
      }
    ],
    "properties": [],
    "methods": [
      {
        "signature": "Initialize(PstWriteContext context)",
        "returnType": "void",
        "description": "Menginisialisasi writer dengan konteks PST."
      },
      {
        "signature": "InitializeAsync(PstWriteContext context, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menginisialisasi writer dengan konteks PST secara asynchronous."
      },
      {
        "signature": "CreateFolder(string name, PstFolder? parent)",
        "returnType": "PstFolder",
        "description": "Membuat folder baru pada PST dan menulis node ke disk."
      },
      {
        "signature": "CreateFolderAsync(string name, PstFolder? parent, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstFolder>",
        "description": "Membuat folder baru pada PST secara asynchronous."
      },
      {
        "signature": "CreateMessage(PstFolder folder, PstMessageDraft draft)",
        "returnType": "PstMessage",
        "description": "Membuat pesan baru pada folder tertentu dan menulis node ke disk."
      },
      {
        "signature": "CreateMessageAsync(PstFolder folder, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Membuat pesan baru pada folder tertentu secara asynchronous."
      },
      {
        "signature": "ImportEml(PstFolder folder, string emlPath)",
        "returnType": "PstMessage",
        "description": "Mengimpor file .eml ke folder PST sebagai pesan baru."
      },
      {
        "signature": "ImportEmlAsync(PstFolder folder, string emlPath, CancellationToken cancellationToken = default)",
        "returnType": "Task<PstMessage>",
        "description": "Mengimpor file .eml ke folder PST sebagai pesan baru secara asynchronous."
      },
      {
        "signature": "UpdateMessage(PstMessage message, PstMessageDraft draft)",
        "returnType": "void",
        "description": "Memperbarui pesan yang sudah ada (belum didukung)."
      },
      {
        "signature": "UpdateMessageAsync(PstMessage message, PstMessageDraft draft, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Memperbarui pesan yang sudah ada secara asynchronous (belum didukung)."
      },
      {
        "signature": "DeleteMessage(PstMessage message)",
        "returnType": "void",
        "description": "Menghapus pesan dari PST (belum didukung)."
      },
      {
        "signature": "DeleteMessageAsync(PstMessage message, CancellationToken cancellationToken = default)",
        "returnType": "Task",
        "description": "Menghapus pesan dari PST secara asynchronous (belum didukung)."
      },
      {
        "signature": "Dispose()",
        "returnType": "void",
        "description": "Melepas resource stream dan melakukan commit BBT/NBT."
      }
    ],
    "events": [],
    "fields": []
  }
];

export const apiNamespaces = [
  {
    "id": "Emcode.Pst.Application",
    "types": [
      "PstFile",
      "PstOpenOptions"
    ]
  },
  {
    "id": "Emcode.Pst.Application.Abstractions",
    "types": [
      "IPstReader",
      "IPstWriter",
      "IPstWriterWithContext",
      "PstReadResult",
      "PstWriteContext"
    ]
  },
  {
    "id": "Emcode.Pst.Domain",
    "types": [
      "PstAttachment",
      "PstCryptMethod",
      "PstDraftAttachment",
      "PstDraftRecipient",
      "PstFolder",
      "PstFormat",
      "PstHeaderInfo",
      "PstMessage",
      "PstMessageDraft",
      "PstRecipient",
      "PstRecipientType"
    ]
  },
  {
    "id": "Emcode.Pst.Infrastructure",
    "types": [
      "PstInMemoryWriter",
      "PstMinimalReader",
      "PstNdbReader"
    ]
  },
  {
    "id": "Emcode.Pst.Infrastructure.Ltp",
    "types": [
      "TableCell"
    ]
  },
  {
    "id": "Emcode.Pst.Infrastructure.Ndb",
    "types": [
      "PstNdbWriter"
    ]
  }
];

export function findApiObject(namespaceName, typeName) {
  return apiObjects.find((item) => item.namespace === namespaceName && item.type === typeName);
}
