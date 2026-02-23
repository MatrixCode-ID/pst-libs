# AGENTS RULE CONFIGURATION

Dokumen ini mendefinisikan aturan kerja AI Agent pada project ini.

## PRIORITY ORDER (WAJIB DIPATUHI)

Jika terjadi konflik aturan, gunakan prioritas berikut:

1. PRIORITY OVERRIDE RULES
2. Audit Rules
3. Refactor Rules
4. Logging Rules
5. General Coding Rules

Rule dengan prioritas lebih tinggi SELALU meng-override rule di bawahnya.

### 1. PRIORITY OVERRIDE RULES

ATURAN DI BAGIAN INI MENG-OVERRIDE SEMUA ATURAN LAIN.

#### AUDIT OVERRIDE RULE

Jika user meminta audit:

- Jalankan audit sesuai dengan [AuditRules.md](AuditRules.md)
- Eksekusi langsung tanpa konfirmasi
- JANGAN buat log pada CodexReport.md
- JANGAN append apapun ke CodexReport.md
- JANGAN buat plan ke [RefactorPlan.md](RefactorPlan.md)
- JANGAN append apapun ke [RefactorPlan.md](RefactorPlan.md)
- Hanya simpan hasil audit sesuai aturan audit

Aturan ini bersifat ABSOLUT dan tidak boleh dilanggar.

### 2. GENERAL PROJECT RULES

- Project menggunakan .NET 10 dan C#
- Project untuk baca dan tulis file Microsoft Outlook PST
- Referensi ada di:
  - doc/PST-241112.docx
  - doc/PST-241112.htm

#### Goal Project:
- Baca File PST
- Buat Folder Import dan import file .eml
- Sync folder lokal dengan PST

#### Language Rules:
- Bahasa komunikasi: Indonesia
- Bahasa object C# (parameter, variable, method, dll): English
- Semua object harus ada XML-Documentation dalam Bahasa Indonesia

#### Coding Rules:
- Gunakan DRY
- Gunakan Clean Architecture
- Setiap object harus bisa di test
- Jika membuat method sync → buat juga async version jika memungkinkan
- Async method harus support CancellationToken

### 3. LOGGING RULES

Berlaku untuk SEMUA permintaan user KECUALI audit.

- Setiap jawaban user harus di log pada [CodexReport.md](CodexReport.md)
- Append hasil di paling atas
- Gunakan proper format layout
- Berikan nomor urut
- Nomor harus descending
- Jelaskan file yang dibuat / diubah

### 4. REFACTOR RULES

Jika ada permintaan perubahan code:

- Siapkan plan di [RefactorPlan.md](RefactorPlan.md)
- Append di paling atas
- Nomor urut descending
- Tunggu perintah user sebelum eksekusi

### 5. TESTING RULES

- Setiap object harus bisa di test
- Jika dotnet restore gagal saat test:
  - Jalankan test tanpa restore
  - Informasikan ke user bahwa test dijalankan tanpa restore

### 6. COMMIT & VERSIONING RULES

Berlaku saat user meminta commit.

- WAJIB tanya user tipe kenaikan versi: `major`, `minor`, atau `build`
- Format versi WAJIB: `XX1.XX2.XX3`
  - `XX1` = Major update
  - `XX2` = Minor update
  - `XX3` = Build update
- Aturan increment versi:
  - `major` → naikkan `XX1` +1, lalu reset `XX2` = 0 dan `XX3` = 0
  - `minor` → naikkan `XX2` +1, lalu reset `XX3` = 0
  - `build` → naikkan `XX3` +1
- Setiap commit WAJIB:
  - Update versi package di file project terkait (contoh: `.csproj`)
  - Update [ChangeLogs.md](ChangeLogs.md) sesuai perubahan versi dan ringkasan perubahan
  - Baru lanjut proses commit setelah dua update di atas selesai

### 7. AUDIT RULE REFERENCE

- Jika diminta audit → jalankan sesuai [AuditRules.md](AuditRules.md)
- Audit TIDAK boleh membuat log
- Audit TIDAK boleh membuat plan
