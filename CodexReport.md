## Laporan 159 — 24 Feb 2026, 11:12
Tanggal laporan: 24 Feb 2026, 11:12

**Ringkasan Pertanyaan**
Meminta lanjut hapus/arsip file `*.html` legacy setelah migrasi Vue.

**Statement Jawaban**
Semua HTML legacy docs sudah dipindahkan ke folder arsip.

Detail:
- File yang diarsipkan: 36 file HTML legacy.
- Lokasi arsip baru: `doc/legacy/pages-html/` (struktur path dipertahankan).
- Source docs aktif `doc/pages` sekarang menyisakan:
  - `index.html` (entry Vue SPA)
  - (lokal build output) `dist/index.html` bila ada hasil build lokal.

Cakupan arsip meliputi:
- `doc/pages/api/**/*.html`
- `doc/pages/help/**/*.html`
- `doc/pages/toc.html`

**File Terkait**
- doc/legacy/pages-html/**
- doc/pages/api/**/*.html (dipindahkan)
- doc/pages/help/**/*.html (dipindahkan)
- doc/pages/toc.html (dipindahkan)
- RefactorPlan.md
- CodexReport.md

## Laporan 158 — 24 Feb 2026, 11:11
Tanggal laporan: 24 Feb 2026, 11:11

**Ringkasan Pertanyaan**
Meminta lanjut fase migrasi detail API object ke Vue secara penuh.

**Statement Jawaban**
Fase ini diselesaikan dengan mengalihkan sumber data API dari HTML legacy ke source C# langsung.

Perubahan utama:
- Generator data API diubah total:
  - `doc/pages/scripts/generate-api-data.mjs`
  - sekarang parse `src/Emcode.Pst.Libs/**/*.cs` untuk mendapatkan:
    - namespace,
    - type publik,
    - summary/signature,
    - constructors/properties/methods/events,
    - fields enum.
- Dataset output:
  - `doc/pages/src/data/apiObjects.js`
  - berisi `apiObjects` + `apiNamespaces`.
- Komponen Vue dihubungkan ke dataset baru:
  - `ApiNamespacesPage.vue` dan `ApiNamespacePage.vue` memakai `apiNamespaces`.
  - `ApiTypeDetailPage.vue` merender detail object API penuh.
- Build script tetap menjalankan generator sebelum Vite build.

Verifikasi:
- `npm run build` sukses.
- Generator menghasilkan 23 object API dari source code C#.

**File Terkait**
- doc/pages/scripts/generate-api-data.mjs
- doc/pages/src/data/apiObjects.js
- doc/pages/src/views/ApiNamespacesPage.vue
- doc/pages/src/views/ApiNamespacePage.vue
- doc/pages/src/views/ApiTypeDetailPage.vue
- doc/pages/src/router.js
- doc/pages/src/styles.css
- doc/pages/package.json
- RefactorPlan.md
- CodexReport.md

## Laporan 157 — 24 Feb 2026, 11:08
Tanggal laporan: 24 Feb 2026, 11:08

**Ringkasan Pertanyaan**
Meminta lanjut fase berikutnya: migrasi konten detail API object yang masih legacy ke komponen Vue secara penuh.

**Statement Jawaban**
Fase migrasi detail API ke Vue sudah dieksekusi.

Perubahan utama:
- Menambahkan generator data API dari halaman legacy:
  - `doc/pages/scripts/generate-api-data.mjs`
  - menghasilkan `doc/pages/src/data/apiObjects.js` (22 object API).
- Menambahkan route detail API object di Vue:
  - `/api/type/:namespace/:type`
- Memperbarui halaman namespace agar setiap type menjadi link ke halaman detail Vue.
- Menambahkan komponen detail object:
  - `doc/pages/src/views/ApiTypeDetailPage.vue`
  - menampilkan `Definition`, `Constructors`, `Properties`, `Methods`, `Events`, serta `Fields` untuk enum.
- Menambahkan style tabel untuk readability halaman detail API.
- Mengintegrasikan generator ke script build:
  - `npm run build` sekarang menjalankan `npm run generate:api-data` sebelum `vite build`.

Verifikasi:
- Build docs Vue berhasil (`npm run build` sukses).

**File Terkait**
- doc/pages/scripts/generate-api-data.mjs
- doc/pages/src/data/apiObjects.js
- doc/pages/src/router.js
- doc/pages/src/views/ApiNamespacePage.vue
- doc/pages/src/views/ApiTypeDetailPage.vue
- doc/pages/src/styles.css
- doc/pages/package.json
- RefactorPlan.md
- CodexReport.md

## Laporan 156 — 24 Feb 2026, 10:59
Tanggal laporan: 24 Feb 2026, 10:59

**Ringkasan Pertanyaan**
Meminta menjalankan plan migrasi `doc/pages` ke framework Vue.

**Statement Jawaban**
Fase awal migrasi ke Vue sudah dieksekusi.

Perubahan utama:
- Menyiapkan Vue + Vite project langsung di `doc/pages`:
  - `package.json`, `package-lock.json`, `vite.config.js`
  - source app di `doc/pages/src` (router, layout, halaman Home/Help/TOC/API)
- Mengganti entry `doc/pages/index.html` menjadi SPA entry (`#app` + `src/main.js`).
- Mengupdate workflow GitHub Pages:
  - setup Node,
  - `npm ci`,
  - `npm run build`,
  - publish artifact dari `doc/pages/dist`.
- Menambahkan ignore untuk artefak lokal build:
  - `node_modules/`
  - `doc/pages/dist/`
- Verifikasi build lokal sukses dengan `npm run build`.

Catatan:
- Migrasi ini adalah fase awal sesuai plan (struktur Vue + routing + layout docs). Konten detail API object legacy dapat dimigrasi bertahap ke komponen Vue pada iterasi berikutnya.

**File Terkait**
- .github/workflows/pages.yml
- .gitignore
- doc/pages/index.html
- doc/pages/package.json
- doc/pages/package-lock.json
- doc/pages/vite.config.js
- doc/pages/src/main.js
- doc/pages/src/App.vue
- doc/pages/src/router.js
- doc/pages/src/styles.css
- doc/pages/src/data/docs.js
- doc/pages/src/views/*.vue
- RefactorPlan.md
- CodexReport.md

## Laporan 155 — 24 Feb 2026, 10:52
Tanggal laporan: 24 Feb 2026, 10:52

**Ringkasan Pertanyaan**
Meminta dibuatkan plan untuk kemungkinan migrasi `doc/pages` ke framework Vue atau React, sekaligus menilai mana yang lebih baik.

**Statement Jawaban**
Plan migrasi sudah ditambahkan di `RefactorPlan.md` sebagai **Plan 56**.

Isi utama plan:
- Menentukan arsitektur docs berbasis framework dengan output static untuk GitHub Pages.
- Evaluasi Vue vs React berdasarkan:
  - kompleksitas setup/maintenance,
  - kecepatan implementasi fitur docs,
  - performa/bundle size,
  - kecocokan skill tim.
- Menyusun POC minimal (Home + Help + API) sebelum migrasi penuh.
- Integrasi build ke workflow Pages.
- Migrasi bertahap dari HTML statis ke source framework.

Rekomendasi awal:
- **Vue** lebih disarankan untuk kondisi saat ini karena lebih ringan dan cepat untuk kebutuhan dokumentasi statis-interaktif.
- **React** tetap opsi valid bila tim sudah dominan React.

**File Terkait**
- RefactorPlan.md
- CodexReport.md

## Laporan 154 — 24 Feb 2026, 10:50
Tanggal laporan: 24 Feb 2026, 10:50

**Ringkasan Pertanyaan**
Melaporkan Prism tidak mengikuti dark mode.

**Statement Jawaban**
Integrasi Prism sudah diperbaiki agar theme syntax highlighting mengikuti mode dokumentasi.

Perubahan:
- Menambahkan fungsi sinkronisasi Prism theme di `theme.js`.
- Saat `applyTheme` dipanggil:
  - mode `light` -> Prism menggunakan `prism.min.css`
  - mode `dark` -> Prism menggunakan `prism-okaidia.min.css`
- URL base Prism diambil dari link Prism yang sudah ada, dengan fallback ke CDN Prism 1.29.0.

Hasil:
- Toggle dark/light sekarang mengubah tampilan code block Prism sesuai mode aktif.

**File Terkait**
- doc/pages/assets/js/theme.js
- RefactorPlan.md
- CodexReport.md

## Laporan 153 — 24 Feb 2026, 10:47
Tanggal laporan: 24 Feb 2026, 10:47

**Ringkasan Pertanyaan**
Meminta memasang `Prism.js` pada semua halaman `doc/pages` yang memiliki block code C#.

**Statement Jawaban**
`Prism.js` sudah dipasang di semua halaman yang memiliki `language-csharp` (total 26 file).

Perubahan yang dilakukan:
- Menambahkan Prism stylesheet:
  - `https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism.min.css`
- Menambahkan script Prism:
  - `prism-core.min.js`
  - `prism-clike.min.js`
  - `prism-csharp.min.js`
- Script Prism disisipkan sebelum `theme.js` pada setiap halaman target.
- Melakukan validasi dan perbaikan struktur HTML pada file target agar elemen penting tetap ada:
  - `<!DOCTYPE html>`, `<html>`, `<head>`, `<meta charset>`, `<meta viewport>`, `<title>`, `site.css`, `theme.js`.

**File Terkait**
- doc/pages/help/getting-started.html
- doc/pages/help/how-to/open-and-read.html
- doc/pages/help/how-to/create-folder-and-message.html
- doc/pages/help/how-to/import-eml.html
- doc/pages/api/Emcode.Pst.Application/PstFile.html
- doc/pages/api/Emcode.Pst.Application/PstOpenOptions.html
- doc/pages/api/Emcode.Pst.Application.Abstractions/IPstReader.html
- doc/pages/api/Emcode.Pst.Application.Abstractions/IPstWriter.html
- doc/pages/api/Emcode.Pst.Application.Abstractions/IPstWriterWithContext.html
- doc/pages/api/Emcode.Pst.Application.Abstractions/PstReadResult.html
- doc/pages/api/Emcode.Pst.Application.Abstractions/PstWriteContext.html
- doc/pages/api/Emcode.Pst.Domain/PstAttachment.html
- doc/pages/api/Emcode.Pst.Domain/PstCryptMethod.html
- doc/pages/api/Emcode.Pst.Domain/PstDraftAttachment.html
- doc/pages/api/Emcode.Pst.Domain/PstDraftRecipient.html
- doc/pages/api/Emcode.Pst.Domain/PstFolder.html
- doc/pages/api/Emcode.Pst.Domain/PstFormat.html
- doc/pages/api/Emcode.Pst.Domain/PstHeaderInfo.html
- doc/pages/api/Emcode.Pst.Domain/PstMessage.html
- doc/pages/api/Emcode.Pst.Domain/PstMessageDraft.html
- doc/pages/api/Emcode.Pst.Domain/PstRecipient.html
- doc/pages/api/Emcode.Pst.Domain/PstRecipientType.html
- doc/pages/api/Emcode.Pst.Infrastructure/PstInMemoryWriter.html
- doc/pages/api/Emcode.Pst.Infrastructure/PstMinimalReader.html
- doc/pages/api/Emcode.Pst.Infrastructure/PstNdbReader.html
- doc/pages/api/Emcode.Pst.Infrastructure/PstNdbWriter.html
- RefactorPlan.md
- CodexReport.md

## Laporan 152 — 24 Feb 2026, 10:41
Tanggal laporan: 24 Feb 2026, 10:41

**Ringkasan Pertanyaan**
Meminta sidebar diterapkan ke semua halaman dokumentasi termasuk `API Reference` dan `index`.

**Statement Jawaban**
Sidebar sudah diperluas ke seluruh halaman docs secara global melalui script `theme.js`, sehingga halaman `index`, `toc`, `api/*`, dan `help/*` semuanya memiliki navigasi kiri.

Perubahan yang dilakukan:
- Menambahkan injeksi sidebar global di `theme.js` untuk semua halaman yang memuat script docs.
- Menambahkan struktur navigasi global:
  - Home, TOC
  - Help (overview, getting-started, concepts, how-to, faq)
  - API References (overview, namespaces, per-namespace index)
- Menambahkan active state otomatis berbasis URL.
- Menjaga kompatibilitas halaman Help yang sudah punya sidebar statis (tidak dibuat ganda).
- Menambahkan dukungan class layout baru `docs-layout` di CSS agar sidebar berlaku lintas halaman.

**File Terkait**
- doc/pages/assets/js/theme.js
- doc/pages/assets/css/site.css
- RefactorPlan.md
- CodexReport.md

## Laporan 151 — 24 Feb 2026, 10:39
Tanggal laporan: 24 Feb 2026, 10:39

**Ringkasan Pertanyaan**
Meminta update halaman `Help` agar punya sidebar seperti pola dokumentasi Microsoft Learn.

**Statement Jawaban**
Halaman `Help` sudah diperbarui dengan sidebar navigasi kiri yang konsisten di semua halaman help.

Perubahan yang dilakukan:
- Menambahkan layout shell `doc-shell` untuk memisahkan area `sidebar` dan `content`.
- Menambahkan komponen sidebar `doc-sidebar` berisi:
  - Overview, Getting Started, Concepts
  - Grup How-To (Open and Read, Create Folder and Message, Import EML)
  - FAQ
  - Link ke `API References` dan `TOC`
- Menambahkan active state (`is-active`) pada menu sesuai halaman yang dibuka.
- Menambahkan style CSS untuk sidebar + layout responsive mobile agar fallback ke single-column.

**File Terkait**
- doc/pages/assets/css/site.css
- doc/pages/help/index.html
- doc/pages/help/getting-started.html
- doc/pages/help/concepts.html
- doc/pages/help/faq.html
- doc/pages/help/how-to/open-and-read.html
- doc/pages/help/how-to/create-folder-and-message.html
- doc/pages/help/how-to/import-eml.html
- RefactorPlan.md
- CodexReport.md

## Laporan 150 — 24 Feb 2026, 10:34
Tanggal laporan: 24 Feb 2026, 10:34

**Ringkasan Pertanyaan**
Meminta update `AGENTS.md` agar agent menggunakan script PowerShell versi 7 (atau terbaru), dan jika belum terinstall harus meminta user install PowerShell 7 terlebih dahulu.

**Statement Jawaban**
`AGENTS.md` sudah diperbarui dengan aturan baru pada section `POWERSHELL EXECUTION RULES`.

Perubahan yang ditambahkan:
- Agent wajib menggunakan PowerShell 7 (`pwsh`) atau versi lebih baru untuk menjalankan script PowerShell.
- Jika PowerShell 7 belum terinstall, agent wajib meminta user install PowerShell 7 terlebih dahulu.
- Agent tidak boleh melanjutkan eksekusi script PowerShell sebelum instalasi PowerShell 7 selesai.

**File Terkait**
- AGENTS.md
- RefactorPlan.md
- CodexReport.md

## Laporan 149 — 24 Feb 2026, 10:32
Tanggal laporan: 24 Feb 2026, 10:32

**Ringkasan Pertanyaan**
Meminta update ke semua halaman API object lainnya agar format mengikuti style referensi API Microsoft (`properties`, `methods`, `events`, dll).

**Statement Jawaban**
Semua halaman API object di area `doc/pages/api/*/*.html` sudah diperbarui dengan format referensi yang konsisten.

Perubahan utama:
- Menstandarkan struktur halaman object menjadi:
  - `Definition`
  - `Constructors`
  - `Properties`
  - `Methods`
  - `Events`
  - `Remarks`
- Untuk object bertipe enum, menggunakan tabel `Fields` berisi nama nilai enum, value, dan keterangan.
- Mengisi keterangan member dari XML documentation source C# agar deskripsi per property/method/field konsisten.
- Mempertahankan section `See Also` pada halaman yang sebelumnya sudah memilikinya.

Halaman yang diperbarui mencakup namespace:
- `Emcode.Pst.Application`
- `Emcode.Pst.Application.Abstractions`
- `Emcode.Pst.Domain`
- `Emcode.Pst.Infrastructure`

Total halaman object API yang diperbarui: 22 file.

**File Terkait**
- doc/pages/api/Emcode.Pst.Application/*.html
- doc/pages/api/Emcode.Pst.Application.Abstractions/*.html
- doc/pages/api/Emcode.Pst.Domain/*.html
- doc/pages/api/Emcode.Pst.Infrastructure/*.html
- RefactorPlan.md
- CodexReport.md

## Laporan 148 — 24 Feb 2026, 10:22
Tanggal laporan: 24 Feb 2026, 10:22

**Ringkasan Pertanyaan**
Meminta update halaman help/API agar formatnya mirip Microsoft Docs (contoh `BinaryWriter`) dengan keterangan untuk `properties`, `methods`, `events`, dll.

**Statement Jawaban**
Halaman API `PstAttachment` sudah diperbarui ke format referensi yang lebih mirip Microsoft Docs.

Perubahan yang dilakukan:
- Menambahkan section `Constructors` dalam bentuk tabel dengan deskripsi constructor.
- Mengubah `Properties` menjadi tabel berisi nama property, tipe data, dan keterangan.
- Mengubah `Methods` menjadi tabel berisi signature, return type, dan keterangan.
- Mempertahankan section `Events` dan menegaskan tidak ada event publik.
- Menambahkan section `Remarks` untuk menjelaskan perilaku saat sumber konten attachment tidak tersedia.

**File Terkait**
- doc/pages/api/Emcode.Pst.Domain/PstAttachment.html
- RefactorPlan.md
- CodexReport.md

## Laporan 147 — 23 Feb 2026, 14:45
Tanggal laporan: 23 Feb 2026, 14:45

**Ringkasan Pertanyaan**
Meminta build NuGet artifact untuk `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

**Statement Jawaban**
Build NuGet artifact sudah dieksekusi dengan command:
- `dotnet pack .\src\Emcode.Pst.Libs\Emcode.Pst.Libs.csproj -c Release -o .\artifacts\nuget`

Hasil:
- Package berhasil dibuat di `artifacts/nuget/Emcode.Pst.Libs.1.0.4.nupkg`.

**File Terkait**
- artifacts/nuget/Emcode.Pst.Libs.1.0.4.nupkg
- CodexReport.md

## Laporan 146 — 23 Feb 2026, 14:44
Tanggal laporan: 23 Feb 2026, 14:44

**Ringkasan Pertanyaan**
Menanyakan command untuk build NuGet dan push NuGet.

**Statement Jawaban**
Command dasar yang diberikan:
- Build/pack NuGet: `dotnet pack .\src\<ProjectName>\<ProjectName>.csproj -c Release -o .\artifacts\nuget`
- Push NuGet ke nuget.org: `dotnet nuget push .\artifacts\nuget\*.nupkg --source https://api.nuget.org/v3/index.json --api-key <NUGET_API_KEY>`

Tambahan:
- Untuk skip symbol package saat push wildcard: `--skip-duplicate` dan filter file `*.nupkg` non `*.snupkg` bila perlu.

**File Terkait**
- CodexReport.md

## Laporan 145 — 23 Feb 2026, 14:38
Tanggal laporan: 23 Feb 2026, 14:38

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 48 untuk menambahkan pilihan mode `Dark` atau `Light` pada GitHub Pages.

**Statement Jawaban**
Plan 48 sudah dieksekusi.

Perubahan yang dilakukan:
- Menambahkan sistem tema `light/dark` berbasis CSS variables di `doc/pages/assets/css/site.css`.
- Menambahkan style komponen toggle tema (`doc-theme-bar` dan `#doc-theme-toggle`) agar konsisten di seluruh halaman.
- Menambahkan script tema terpusat `doc/pages/assets/js/theme.js` untuk:
  - membaca preferensi user dari `localStorage`,
  - fallback ke `prefers-color-scheme`,
  - menyimpan preferensi mode,
  - dan menambahkan tombol toggle ke dalam `<main>` secara otomatis.
- Menyisipkan referensi script tema ke seluruh halaman HTML dokumentasi di `doc/pages`.

Hasil:
- Semua halaman docs sekarang mendukung mode `Dark` dan `Light`.
- Preferensi mode persisten antar halaman.

**File Terkait**
- doc/pages/assets/css/site.css
- doc/pages/assets/js/theme.js
- doc/pages/**/*.html
- CodexReport.md

## Laporan 144 — 23 Feb 2026, 14:36
Tanggal laporan: 23 Feb 2026, 14:36

**Ringkasan Pertanyaan**
Meminta update layout GitHub Pages agar bisa memilih mode dark atau light.

**Statement Jawaban**
Sesuai aturan project untuk perubahan code, saya menyiapkan plan refactor terlebih dahulu dan menunggu instruksi eksekusi.

Tindakan yang dilakukan:
- Menambahkan **Plan 48** di `RefactorPlan.md` untuk implementasi pemilih tema `Dark/Light`.
- Plan mencakup update CSS token tema, komponen toggle, persistensi preferensi via `localStorage`, fallback `prefers-color-scheme`, dan validasi keterbacaan.

Status: menunggu perintah user untuk menjalankan Plan 48.

**File Terkait**
- RefactorPlan.md
- CodexReport.md

## Laporan 143 — 23 Feb 2026, 14:32
Tanggal laporan: 23 Feb 2026, 14:32

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 47 untuk restrukturisasi `doc/pages` agar seluruh dokumentasi menggunakan HTML5.

**Statement Jawaban**
Plan 47 sudah dieksekusi penuh.

Perubahan utama:
- Mengonversi seluruh file Markdown di `doc/pages` menjadi dokumen HTML5 statis (total 37 halaman).
- Memperbarui seluruh tautan internal dokumentasi agar menggunakan ekstensi `.html`.
- Menghapus seluruh file `.md` dari `doc/pages` agar artifact publish murni HTML5.
- Menjadikan `doc/pages/index.html` sebagai landing page hasil konversi dari konten dokumentasi utama.
- Merapikan styling global pada `doc/pages/assets/css/site.css` untuk konsistensi tampilan desktop/mobile.

Validasi akhir:
- File `.md` pada `doc/pages`: `0`.
- Referensi tautan `href` yang masih menuju `.md`: tidak ada.

**File Terkait**
- doc/pages/**/*.html
- doc/pages/assets/css/site.css
- RefactorPlan.md
- CodexReport.md

## Laporan 142 — 23 Feb 2026, 14:31
Tanggal laporan: 23 Feb 2026, 14:31

**Ringkasan Pertanyaan**
Meminta restrukturisasi `doc/pages` agar semua dokumentasi menggunakan HTML5.

**Statement Jawaban**
Sesuai aturan project, perubahan code didahului dengan penyusunan plan dan menunggu instruksi eksekusi dari user.

Tindakan yang dilakukan:
- Menyusun **Plan 47** di `RefactorPlan.md` untuk migrasi penuh konten `doc/pages` dari Markdown ke HTML5.
- Plan mencakup konversi file, perbaikan link internal ke `.html`, penyesuaian landing page, dan validasi akhir.

Status saat ini: menunggu perintah user untuk mengeksekusi Plan 47.

**File Terkait**
- RefactorPlan.md
- CodexReport.md

## Laporan 141 — 23 Feb 2026, 14:28
Tanggal laporan: 23 Feb 2026, 14:28

**Ringkasan Pertanyaan**
Melaporkan error GitHub Pages karena tidak ada file `index.html`.

**Statement Jawaban**
Perbaikan dilakukan dengan menambahkan file `doc/pages/index.html` sebagai entry page untuk artifact GitHub Pages.

Detail tindakan:
- Menyusun plan perubahan sebagai **Plan 46** di `RefactorPlan.md`.
- Menambahkan halaman landing HTML statis di root `doc/pages`.
- Menyediakan link utama ke `help/index.md`, `api/index.md`, dan `toc.md`.

Dengan perubahan ini, validasi artifact GitHub Pages tidak lagi gagal karena file `index.html` sudah tersedia.

**File Terkait**
- RefactorPlan.md
- doc/pages/index.html
- CodexReport.md

## Laporan 140 — 23 Feb 2026, 14:07
Tanggal laporan: 23 Feb 2026, 14:07

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 45 untuk update `AGENTS.md` agar saat user meminta commit tidak perlu log ke `CodexReport.md` dan tidak perlu menjalankan `RefactorPlan` terlebih dahulu.

**Statement Jawaban**
Plan 45 sudah dieksekusi.

Perubahan pada `AGENTS.md`:
- `LOGGING RULES` diperbarui menjadi pengecualian untuk `audit` dan `commit`.
- Ditambahkan aturan eksplisit bahwa saat user meminta commit, agent tidak membuat/meng-append log ke `CodexReport.md`.
- `REFACTOR RULES` diperbarui menjadi berlaku untuk perubahan code selain commit.
- Ditambahkan aturan eksplisit bahwa saat user meminta commit, tidak perlu membuat plan `RefactorPlan.md` dan tidak perlu menunggu eksekusi plan.

**File Terkait**
- AGENTS.md
- CodexReport.md
## Laporan 139 — 23 Feb 2026, 14:06
Tanggal laporan: 23 Feb 2026, 14:06

**Ringkasan Pertanyaan**
Meminta update `AGENTS.md` agar ketika user meminta commit tidak perlu log ke `CodexReport.md` dan tidak perlu menjalankan `RefactorPlan` dulu.

**Statement Jawaban**
Sesuai aturan project saat ini untuk perubahan file, plan refactor telah disiapkan sebagai **Plan 45** di `RefactorPlan.md`.

Plan 45 mencakup:
- Pengecualian logging untuk permintaan commit.
- Pengecualian refactor-plan untuk permintaan commit.
- Menjaga aturan commit/versioning yang sudah ada tetap berlaku.

Menunggu perintah user untuk eksekusi Plan 45.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 138 — 23 Feb 2026, 14:05
Tanggal laporan: 23 Feb 2026, 14:05

**Ringkasan Pertanyaan**
Meminta commit codebase dengan tipe versi `build`.

**Statement Jawaban**
Eksekusi dilakukan dengan aturan build increment (`XX3 + 1`) dari `1.0.0` menjadi `1.0.1`.

Perubahan utama sebelum commit:
- Update versi package pada `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj` menjadi `1.0.1`.
- Update `PackageReleaseNotes` agar sesuai konten rilis.
- Update `ChangeLogs.md` dengan entri rilis `1.0.1`.
- Selanjutnya commit seluruh perubahan codebase sesuai permintaan user.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- ChangeLogs.md
- AGENTS.md
- RefactorPlan.md
- .github/workflows/pages.yml
- doc/Emcode.Pst.Libs.csproj.md
- doc/pages/*
- icon.png
- CodexReport.md
## Laporan 137 — 23 Feb 2026, 14:04
Tanggal laporan: 23 Feb 2026, 14:04

**Ringkasan Pertanyaan**
Meminta commit seluruh codebase.

**Statement Jawaban**
Commit belum dijalankan karena aturan `AGENTS.md` mewajibkan konfirmasi tipe kenaikan versi terlebih dahulu (`major`, `minor`, atau `build`).

Menunggu pilihan tipe versi dari user sebelum melanjutkan:
- Update versi pada `.csproj`
- Update `ChangeLogs.md`
- Commit perubahan

**File Terkait**
- AGENTS.md
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- ChangeLogs.md
- CodexReport.md
## Laporan 136 — 23 Feb 2026, 14:03
Tanggal laporan: 23 Feb 2026, 14:03

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 44 untuk update `AGENTS.md` agar saat user minta commit wajib tanya tipe update versi (`major/minor/build`), lalu update changelog dan increment versi format `XX1.XX2.XX3`.

**Statement Jawaban**
Plan 44 sudah dieksekusi.

Perubahan pada `AGENTS.md`:
- Menambahkan section baru `### 6. COMMIT & VERSIONING RULES`.
- Menetapkan kewajiban menanyakan tipe kenaikan versi (`major`, `minor`, `build`) sebelum commit.
- Menetapkan format versi wajib `XX1.XX2.XX3` dengan mapping:
  - `XX1` = Major update
  - `XX2` = Minor update
  - `XX3` = Build update
- Menetapkan aturan increment versi:
  - `major` => `XX1+1`, reset `XX2` dan `XX3` ke `0`
  - `minor` => `XX2+1`, reset `XX3` ke `0`
  - `build` => `XX3+1`
- Menetapkan kewajiban update versi package (`.csproj`) dan `ChangeLogs.md` sebelum commit.
- Menyesuaikan penomoran `AUDIT RULE REFERENCE` dari section 6 menjadi section 7.

**File Terkait**
- AGENTS.md
- CodexReport.md
## Laporan 135 — 23 Feb 2026, 13:56
Tanggal laporan: 23 Feb 2026, 13:56

**Ringkasan Pertanyaan**
Meminta update `AGENTS.md` agar setiap permintaan commit wajib tanya jenis update versi (`major/minor/build`), lalu update changelog dan increment versi format `XX1.XX2.XX3`.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file, plan refactor sudah dibuat terlebih dahulu sebagai **Plan 44** di `RefactorPlan.md`.

Cakupan Plan 44:
- Menambah rule wajib tanya jenis update versi sebelum commit.
- Menetapkan mapping format `XX1.XX2.XX3` (major/minor/build).
- Menetapkan aturan increment versi per tipe update.
- Menetapkan kewajiban update `ChangeLogs.md` dan versi package sebelum commit.

Menunggu perintah user untuk eksekusi Plan 44.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 134 — 23 Feb 2026, 13:48
Tanggal laporan: 23 Feb 2026, 13:48

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 43 untuk membuat file `ChangeLogs.md` sesuai standar.

**Statement Jawaban**
Plan 43 sudah dieksekusi.

Perubahan yang dilakukan:
- Menambahkan file baru `ChangeLogs.md` di root repository.
- Menggunakan format standar changelog dengan referensi `Keep a Changelog` dan `Semantic Versioning`.
- Menambahkan section `Unreleased` dengan kategori perubahan standar (`Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`).
- Menambahkan baseline versi `1.0.0` dengan ringkasan rilis awal library.

**File Terkait**
- ChangeLogs.md
- CodexReport.md
## Laporan 133 — 23 Feb 2026, 13:47
Tanggal laporan: 23 Feb 2026, 13:47

**Ringkasan Pertanyaan**
Meminta dibuatkan file `ChangeLogs.md` untuk menyimpan perubahan versi sesuai standar.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file, plan refactor dibuat terlebih dahulu sebagai **Plan 43** di `RefactorPlan.md`.

Plan 43 mencakup:
- Pembuatan file `ChangeLogs.md` di root repository.
- Format standar changelog (`Keep a Changelog` + `Semantic Versioning`).
- Penambahan section `Unreleased` dan template kategori perubahan.
- Baseline entri versi awal.

Menunggu perintah user untuk eksekusi Plan 43.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 132 — 23 Feb 2026, 13:42
Tanggal laporan: 23 Feb 2026, 13:42

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 42 untuk membuat workflow GitHub Actions yang publish GitHub Pages dari folder `doc/pages`.

**Statement Jawaban**
Plan 42 sudah dieksekusi.

Perubahan yang dilakukan:
- Membuat folder `.github/workflows`.
- Menambahkan file workflow `.github/workflows/pages.yml`.
- Workflow dikonfigurasi dengan:
  - Trigger `push` ke branch `main` saat ada perubahan `doc/pages/**` atau file workflow.
  - Trigger manual `workflow_dispatch`.
  - Permissions: `contents: read`, `pages: write`, `id-token: write`.
  - Upload artifact dari `doc/pages`.
  - Deploy menggunakan `actions/deploy-pages@v4`.

Catatan:
- Setelah file ini dipush ke GitHub, aktifkan `Settings > Pages > Source: GitHub Actions`.

**File Terkait**
- .github/workflows/pages.yml
- CodexReport.md
## Laporan 131 — 23 Feb 2026, 13:41
Tanggal laporan: 23 Feb 2026, 13:41

**Ringkasan Pertanyaan**
Meminta dibuatkan workflow GitHub Actions untuk GitHub Pages dari folder `doc/pages`.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file, plan refactor sudah dibuat terlebih dahulu sebagai **Plan 42** di `RefactorPlan.md`.

Plan 42 mencakup:
- Pembuatan `.github/workflows/pages.yml`.
- Trigger `push` (path `doc/pages/**`) dan `workflow_dispatch`.
- Deploy menggunakan action resmi GitHub Pages dengan source artifact dari `doc/pages`.
- Konfigurasi permission dan concurrency.

Menunggu perintah user untuk eksekusi Plan 42.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 130 — 23 Feb 2026, 13:37
Tanggal laporan: 23 Feb 2026, 13:37

**Ringkasan Pertanyaan**
Menanyakan cara menggunakan GitHub Actions.

**Statement Jawaban**
Diberikan panduan penggunaan GitHub Actions dari nol sampai jalan:
- Membuat file workflow di `.github/workflows/*.yml`.
- Menentukan trigger (`push`, `pull_request`, `workflow_dispatch`).
- Menentukan job, runner, dan step (`checkout`, setup SDK, build/test/deploy).
- Menjalankan dan memantau eksekusi pada tab Actions.
- Menambahkan contoh workflow untuk deploy docs ke GitHub Pages.

**File Terkait**
- CodexReport.md
## Laporan 129 — 23 Feb 2026, 13:30
Tanggal laporan: 23 Feb 2026, 13:30

**Ringkasan Pertanyaan**
Meminta menjalankan publish ke branch `gh-pages` dan menghapus `doc/pages`.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file/deploy, plan refactor harus dibuat terlebih dahulu sebelum eksekusi.

Plan baru sudah ditambahkan sebagai **Plan 41** di `RefactorPlan.md` dengan cakupan:
- Publish konten dokumentasi ke branch `gh-pages`.
- Verifikasi artefak docs di branch publish.
- Hapus `doc/pages` dari branch utama.
- Commit perubahan di branch utama.

Menunggu perintah user untuk eksekusi Plan 41.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 128 — 23 Feb 2026, 13:28
Tanggal laporan: 23 Feb 2026, 13:28

**Ringkasan Pertanyaan**
Menanyakan apakah `doc/pages` boleh dihapus jika dibuat branch `gh-pages` khusus dokumentasi.

**Statement Jawaban**
Boleh, selama seluruh sumber dokumentasi memang dipindah dan dikelola penuh di branch `gh-pages`.

Catatan:
- Jika workflow deploy masih mengambil source dari `doc/pages` di branch utama, folder tersebut tidak boleh dihapus.
- Jika source Pages diubah ke branch `gh-pages` root, maka `doc/pages` di branch utama opsional.

**File Terkait**
- CodexReport.md
## Laporan 127 — 23 Feb 2026, 13:26
Tanggal laporan: 23 Feb 2026, 13:26

**Ringkasan Pertanyaan**
Menanyakan cara publish dokumentasi ke branch `gh-pages`.

**Statement Jawaban**
Diberikan panduan praktis publish ke `gh-pages` dengan dua opsi:
- Manual deploy dari folder `doc/pages`.
- Otomatis deploy memakai GitHub Actions (`actions/configure-pages`, `actions/upload-pages-artifact`, `actions/deploy-pages`).

Juga dijelaskan setting repository yang perlu diaktifkan pada menu Pages.

**File Terkait**
- CodexReport.md
## Laporan 126 — 23 Feb 2026, 13:24
Tanggal laporan: 23 Feb 2026, 13:24

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 40: restruktur dokumentasi GitHub Pages menjadi `Help` dan `API References` seperti dokumentasi Microsoft .NET Framework.

**Statement Jawaban**
Plan 40 sudah dieksekusi dengan membuat struktur dokumentasi baru pada `doc/pages`.

Hasil utama:
- Membuat area `Help` (`getting-started`, `concepts`, `how-to`, `faq`).
- Membuat area `API References` berbasis namespace dan type.
- Menyusun reference object (property, method, constructor, field enum, event status) berdasarkan XML documentation di source.
- Menambahkan `toc.md` untuk navigasi bertingkat.
- Menambahkan `_config.yml` untuk konfigurasi GitHub Pages.
- Validasi link lokal markdown: **OK, tidak ada link internal rusak**.

**File Terkait**
- doc/pages/index.md
- doc/pages/toc.md
- doc/pages/help/index.md
- doc/pages/help/getting-started.md
- doc/pages/help/concepts.md
- doc/pages/help/how-to/open-and-read.md
- doc/pages/help/how-to/create-folder-and-message.md
- doc/pages/help/how-to/import-eml.md
- doc/pages/help/faq.md
- doc/pages/api/index.md
- doc/pages/api/namespaces.md
- doc/pages/api/Emcode.Pst.Application/index.md
- doc/pages/api/Emcode.Pst.Application/PstFile.md
- doc/pages/api/Emcode.Pst.Application/PstOpenOptions.md
- doc/pages/api/Emcode.Pst.Application.Abstractions/index.md
- doc/pages/api/Emcode.Pst.Application.Abstractions/IPstReader.md
- doc/pages/api/Emcode.Pst.Application.Abstractions/IPstWriter.md
- doc/pages/api/Emcode.Pst.Application.Abstractions/IPstWriterWithContext.md
- doc/pages/api/Emcode.Pst.Application.Abstractions/PstReadResult.md
- doc/pages/api/Emcode.Pst.Application.Abstractions/PstWriteContext.md
- doc/pages/api/Emcode.Pst.Domain/index.md
- doc/pages/api/Emcode.Pst.Domain/PstAttachment.md
- doc/pages/api/Emcode.Pst.Domain/PstDraftAttachment.md
- doc/pages/api/Emcode.Pst.Domain/PstDraftRecipient.md
- doc/pages/api/Emcode.Pst.Domain/PstFolder.md
- doc/pages/api/Emcode.Pst.Domain/PstHeaderInfo.md
- doc/pages/api/Emcode.Pst.Domain/PstMessage.md
- doc/pages/api/Emcode.Pst.Domain/PstMessageDraft.md
- doc/pages/api/Emcode.Pst.Domain/PstRecipient.md
- doc/pages/api/Emcode.Pst.Domain/PstCryptMethod.md
- doc/pages/api/Emcode.Pst.Domain/PstFormat.md
- doc/pages/api/Emcode.Pst.Domain/PstRecipientType.md
- doc/pages/api/Emcode.Pst.Infrastructure/index.md
- doc/pages/api/Emcode.Pst.Infrastructure/PstMinimalReader.md
- doc/pages/api/Emcode.Pst.Infrastructure/PstNdbReader.md
- doc/pages/api/Emcode.Pst.Infrastructure/PstInMemoryWriter.md
- doc/pages/api/Emcode.Pst.Infrastructure/PstNdbWriter.md
- doc/pages/_config.yml
- doc/pages/assets/css/site.css
- CodexReport.md
## Laporan 125 — 23 Feb 2026, 13:15
Tanggal laporan: 23 Feb 2026, 13:15

**Ringkasan Pertanyaan**
Meminta restruktur plan untuk GitHub Pages agar menjadi `Help` dan `API References` seperti dokumentasi Microsoft .NET Framework.

**Statement Jawaban**
Plan baru sudah ditambahkan sebagai **Plan 40** di `RefactorPlan.md` dengan struktur dokumentasi bergaya framework docs.

Fokus Plan 40:
- Pemisahan area `Help` (konseptual) dan `API` (reference teknis).
- Navigasi bertingkat via `toc.md`.
- API references per `Namespace > Type > Members` berdasarkan XML documentation.
- Template halaman type mencakup section khas docs reference (summary, declaration, remarks, examples, see also).

Menunggu perintah user untuk eksekusi Plan 40.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 124 — 23 Feb 2026, 13:12
Tanggal laporan: 23 Feb 2026, 13:12

**Ringkasan Pertanyaan**
Meminta update Plan 39 agar fokus ke dokumentasi `Help` dan `Object References` (property, event, method, dll) sesuai XML documentation.

**Statement Jawaban**
Plan 39 di `RefactorPlan.md` sudah diperbarui.

Perubahan utama:
- Fokus ringkasan plan diubah ke `Help` + `Object References` berbasis XML documentation.
- Lingkup diperbarui dengan target `doc/pages/help/index.md` dan `doc/pages/references/objects.md`.
- Rencana prioritas ditambah langkah ekstraksi sumber XML docs dari object C#.
- Kriteria selesai diperjelas agar reference menampilkan anggota object (property, event, method, dll) sesuai XML docs.

Menunggu perintah user untuk eksekusi plan yang sudah direvisi.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 123 — 23 Feb 2026, 13:10
Tanggal laporan: 23 Feb 2026, 13:10

**Ringkasan Pertanyaan**
Meminta ubah struktur target dokumentasi menjadi `doc/pages` dan menyiapkan plan untuk membuat reference doc.

**Statement Jawaban**
Plan refactor baru sudah disiapkan sebagai **Plan 39** di `RefactorPlan.md`.

Penyesuaian utama:
- Struktur publish diarahkan ke `doc/pages`.
- Fokus deliverable ke reference documentation.
- Daftar halaman reference dirinci (`api-overview`, `pstfile`, `models`) beserta validasi link dan konsistensi API.

Menunggu perintah user untuk eksekusi Plan 39.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 122 — 23 Feb 2026, 13:09
Tanggal laporan: 23 Feb 2026, 13:09

**Ringkasan Pertanyaan**
Menanyakan apakah struktur dokumentasi GitHub Pages harus di folder `docs/`.

**Statement Jawaban**
Tidak harus di `docs/`.

Opsi GitHub Pages yang umum:
- Publish dari branch `main` folder `/docs`.
- Publish dari branch `gh-pages` (root `/`).

Untuk workflow repository ini, `docs/` di branch utama biasanya paling sederhana karena mudah dikelola bersama source code.

**File Terkait**
- CodexReport.md
## Laporan 121 — 23 Feb 2026, 13:06
Tanggal laporan: 23 Feb 2026, 13:06

**Ringkasan Pertanyaan**
Meminta dibuatkan dokumentasi `references` dan `help` untuk library project yang akan dipublish ke GitHub Pages.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file, plan refactor sudah dibuat di `RefactorPlan.md` sebagai **Plan 38**.

Rencana mencakup:
- Struktur dokumentasi `docs/` untuk GitHub Pages.
- Halaman `References` dan `Help`.
- Konfigurasi `docs/_config.yml`.
- Validasi tautan internal dan konsistensi konten.

Menunggu perintah user untuk eksekusi Plan 38.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 120 — 23 Feb 2026, 13:02
Tanggal laporan: 23 Feb 2026, 13:02

**Ringkasan Pertanyaan**
Meminta dibuatkan dokumentasi untuk file `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

**Statement Jawaban**
Dokumentasi baru berhasil dibuat pada `doc/Emcode.Pst.Libs.csproj.md`.

Isi dokumentasi mencakup:
- Penjelasan properti build utama (`TargetFramework`, `Nullable`, `ImplicitUsings`, dll).
- Penjelasan metadata NuGet (`PackageId`, `Version`, `PackageReadmeFile`, `PackageIcon`, dll).
- Penjelasan item file yang ikut dipack (`README.id.md` dan `icon.png`).
- Catatan pemeliharaan dan command validasi `dotnet pack`.

**File Terkait**
- doc/Emcode.Pst.Libs.csproj.md
- CodexReport.md
## Laporan 119 — 23 Feb 2026, 12:34
Tanggal laporan: 23 Feb 2026, 12:34

**Ringkasan Pertanyaan**
Meminta release ulang paket.

**Statement Jawaban**
Eksekusi dilakukan dengan membuat package versi patch baru tanpa mengubah file project menggunakan command:
`dotnet pack src\Emcode.Pst.Libs\Emcode.Pst.Libs.csproj -c Release -o artifacts -p:Version=1.0.1`

Hasil:
- Package baru berhasil dibuat: `artifacts/Emcode.Pst.Libs.1.0.1.nupkg`.
- Langkah publish ke `nuget.org` masih membutuhkan API key user.

**File Terkait**
- artifacts/Emcode.Pst.Libs.1.0.1.nupkg
- CodexReport.md
## Laporan 118 — 23 Feb 2026, 12:33
Tanggal laporan: 23 Feb 2026, 12:33

**Ringkasan Pertanyaan**
Meminta release ulang paket.

**Statement Jawaban**
Dijelaskan bahwa rilis ulang ke `nuget.org` tidak bisa memakai versi yang sama (`1.0.0`), sehingga perlu bump versi (mis. `1.0.1`), lalu `dotnet pack` dan `dotnet nuget push`.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- artifacts/Emcode.Pst.Libs.1.0.0.nupkg
- CodexReport.md
## Laporan 117 — 23 Feb 2026, 12:32
Tanggal laporan: 23 Feb 2026, 12:32

**Ringkasan Pertanyaan**
Menanyakan cara menambahkan copyright pada paket NuGet.

**Statement Jawaban**
Dijelaskan bahwa copyright NuGet ditambahkan melalui properti `PackageCopyright` pada file `.csproj` library, lalu divalidasi dengan `dotnet pack`.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 116 — 23 Feb 2026, 12:30
Tanggal laporan: 23 Feb 2026, 12:30

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 37 untuk mengganti readme package ke `README.id.md`.

**Statement Jawaban**
Plan 37 sudah dieksekusi.

Perubahan:
- `PackageReadmeFile` diubah menjadi `README.id.md`.
- Item pack readme diubah dari `README.md` ke `README.id.md`.

Validasi:
- `dotnet pack` berhasil dan package NuGet tetap terbentuk normal.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 115 — 23 Feb 2026, 12:28
Tanggal laporan: 23 Feb 2026, 12:28

**Ringkasan Pertanyaan**
Meminta mengganti readme package ke `README.id.md`.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file, plan refactor sudah dibuat di `RefactorPlan.md` sebagai Plan 37 untuk mengubah `PackageReadmeFile` dan item pack readme, lalu memvalidasi dengan `dotnet pack` setelah eksekusi.

**File Terkait**
- RefactorPlan.md
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 114 — 23 Feb 2026, 12:29
Tanggal laporan: 23 Feb 2026, 12:29

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 36 untuk generate icon yang sesuai prinsip project.

**Statement Jawaban**
Plan 36 sudah dieksekusi dengan mengganti `icon.png` menjadi icon baru bertema domain project (PST/email): elemen folder, envelope, dan node struktur untuk merepresentasikan hierarchy/sync.

Validasi hasil:
- Format icon: PNG
- Dimensi: 256x256
- `dotnet pack` berhasil dan package tetap terbentuk normal.

**File Terkait**
- icon.png
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 113 — 23 Feb 2026, 12:27
Tanggal laporan: 23 Feb 2026, 12:27

**Ringkasan Pertanyaan**
Meminta generate icon yang disesuaikan dengan prinsip project.

**Statement Jawaban**
Sesuai aturan project untuk perubahan file, plan refactor sudah dibuat di `RefactorPlan.md` sebagai Plan 36 (konsep visual PST/email, validasi keterbacaan, dan output `icon.png`). Menunggu perintah user untuk eksekusi pembuatan icon.

**File Terkait**
- RefactorPlan.md
- README.id.md
- CodexReport.md
## Laporan 112 — 23 Feb 2026, 12:27
Tanggal laporan: 23 Feb 2026, 12:27

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 35 untuk mengisi properti opsional NuGet.

**Statement Jawaban**
Plan 35 sudah dieksekusi.

Perubahan yang dilakukan:
- Menambahkan metadata opsional NuGet pada `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`:
  - `PackageProjectUrl`
  - `RepositoryType`
  - `PackageReleaseNotes`
  - `PackageIcon`
- Menambahkan file package icon `icon.png` di root project dan mengonfigurasi agar ikut ter-pack.
- Validasi publish readiness dengan `dotnet pack` berhasil, output package:
  - `artifacts/Emcode.Pst.Libs.1.0.0.nupkg`

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- icon.png
- CodexReport.md
## Laporan 111 — 23 Feb 2026, 12:25
Tanggal laporan: 23 Feb 2026, 12:25

**Ringkasan Pertanyaan**
Meminta mengisi properti opsional NuGet di `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

**Statement Jawaban**
Sesuai aturan project untuk perubahan kode, plan refactor sudah dibuat di `RefactorPlan.md` (Plan 35) dan menunggu perintah konfirmasi eksekusi sebelum edit file `.csproj` dilakukan.

**File Terkait**
- RefactorPlan.md
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 110 — 23 Feb 2026, 12:24
Tanggal laporan: 23 Feb 2026, 12:24

**Ringkasan Pertanyaan**
Meminta pengecekan apakah property di `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj` sudah siap untuk upload `nuget.org`.

**Statement Jawaban**
Hasil cek: property inti untuk publish NuGet sudah tersedia (`PackageId`, `Version`, `Authors`, `Description`, `PackageLicenseExpression`, `PackageReadmeFile`, `RepositoryUrl`) dan item `README.md` sudah dipack ke root package.

Catatan: masih ada properti opsional yang direkomendasikan untuk kualitas metadata (`PackageProjectUrl`, `RepositoryType`, `PackageReleaseNotes`, `PackageIcon`) namun bukan blocker untuk upload.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 109 — 23 Feb 2026, 12:23
Tanggal laporan: 23 Feb 2026, 12:23

**Ringkasan Pertanyaan**
Meminta agar `README.md` mereferensikan `README.id.md`.

**Statement Jawaban**
Sudah sesuai. File `README.md` saat ini sudah mereferensikan `README.id.md` sebagai dokumentasi utama berbahasa Indonesia, jadi tidak perlu perubahan tambahan.

**File Terkait**
- README.md
- CodexReport.md
## Laporan 108 — 23 Feb 2026, 12:19
Tanggal laporan: 23 Feb 2026, 12:19

**Ringkasan Pertanyaan**
Menanyakan langkah agar library bisa di-upload ke `nuget.org`.

**Statement Jawaban**
Diberikan checklist praktis publish NuGet: lengkapi metadata `.csproj`, build+pack, buat API key di `nuget.org`, tambah source nuget, dan jalankan `dotnet nuget push` untuk file `.nupkg`.

Status project saat dicek:
- `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj` masih minimal dan belum memuat metadata package NuGet.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
## Laporan 107 — 16 Feb 2026, 19:12
Tanggal laporan: 16 Feb 2026, 19:12

**Ringkasan Pertanyaan**
Meminta keterangan singkat project dalam satu paragraf maksimal 350 karakter.

**Statement Jawaban**
Project `Emcode.Pst` adalah library C# (.NET 10) untuk membaca file Microsoft Outlook PST, dengan kemampuan baca folder, message, properti email, dan attachment (sync/async), serta fondasi write draft/import `.eml` berbasis path; sebagian fitur write lanjutan masih bertahap.

**File Terkait**
- CodexReport.md

## Laporan 106 — 16 Feb 2026, 19:11
Tanggal laporan: 16 Feb 2026, 19:11

**Ringkasan Pertanyaan**
Meminta keterangan tentang project ini.

**Statement Jawaban**
Project `Emcode.Pst` adalah library C# (.NET 10) untuk membaca file Microsoft Outlook PST dengan fokus read MVP dan fondasi write. Fitur saat ini sudah mencakup:
- buka PST ANSI/Unicode dan baca metadata header,
- baca hierarki folder dan daftar message per folder,
- baca properti message inti/extended serta metadata dan konten attachment (sync/async),
- write draft (in-memory dan persist disk eksperimental via `PstNdbWriter`),
- import `.eml` berbasis path file (sync/async).

Batasan utama yang masih ada:
- import `.eml` dari `Stream` belum didukung,
- update/delete message pada `PstNdbWriter` belum didukung,
- crypt `Cyclic` dan `EdpEncrypted` belum didukung,
- sinkronisasi folder lokal ke PST belum tersedia.

**File Terkait**
- README.id.md
- README.md
- AGENTS.md
- CodexReport.md

## Laporan 105 — 16 Feb 2026, 17:06
Tanggal laporan: 16 Feb 2026, 17:06

**Ringkasan Pertanyaan**
Menanyakan cara persiapan upload `src\Emcode.Pst.Libs\Emcode.Pst.Libs.csproj` ke `nuget.org` dan meminta tidak membuat plan dulu.

**Statement Jawaban**
Panduan persiapan publish ke `nuget.org` diberikan tanpa membuat plan refactor. Fokus mencakup:
- penambahan metadata package pada file `.csproj`,
- validasi build/pack,
- konfigurasi API key/source NuGet,
- push paket `.nupkg` (opsional simbol `.snupkg`),
- dan checklist verifikasi pra-rilis.

Status saat ini:
- `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj` masih minimal dan belum memuat metadata NuGet.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- CodexReport.md
# Codex Review Report - PST Projects

## Laporan 104 — 16 Feb 2026, 11:36
Tanggal laporan: 16 Feb 2026, 11:36

**Ringkasan Pertanyaan**
Meminta menjalankan audit kembali.

**Statement Jawaban**
Audit ulang sudah dijalankan sesuai `AuditRules.md` dan format `doc/AuditReports/AuditReportStructure.md`.

Hasil utama:
- Report audit terbaru dibuat di `doc/AuditReports/AuditReport_0003_20260216_S065.md`.
- Total skor audit: **65/100** (Grade **C**).
- Risiko prioritas tertinggi tetap pada jalur write concurrency, parser `.eml` tanpa guard ukuran, ketiadaan `SECURITY.md`, dan belum adanya CI workflow.

Catatan test saat audit:
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release` gagal restore dengan `NU1301` (akses `api.nuget.org` terblokir).
- Sesuai instruksi project, test dijalankan ulang dengan `--no-restore`.

**File Terkait**
- AuditRules.md
- doc/AuditReports/AuditReportStructure.md
- doc/AuditReports/AuditReport_0003_20260216_S065.md
- CodexReport.md

## Laporan 103 — 16 Feb 2026, 11:25
Tanggal laporan: 16 Feb 2026, 11:25

**Ringkasan Pertanyaan**
Meminta melakukan audit kembali.

**Statement Jawaban**
Audit ulang sudah dijalankan sesuai `AuditRules.md` dan struktur `doc/AuditReports/AuditReportStructure.md`.

Hasil utama:
- Report audit baru dibuat di `doc/AuditReports/AuditReport_0002_20260216_S065.md`.
- Total skor audit terbaru: **65/100** (Grade **C**).
- Skor naik dari audit sebelumnya karena `CONTRIBUTING.md` sudah tersedia, tetapi masih ada risiko utama pada concurrency write, parser `.eml` tanpa size guard, `SECURITY.md` yang belum ada, dan belum ada CI workflow.

Catatan test saat audit:
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release` gagal restore (`NU1301`, akses `api.nuget.org` terblokir).
- Sesuai instruksi project, test dijalankan ulang dengan `--no-restore`.

**File Terkait**
- AuditRules.md
- doc/AuditReports/AuditReportStructure.md
- doc/AuditReports/AuditReport_0002_20260216_S065.md
- CodexReport.md

## Laporan 102 — 16 Feb 2026, 11:18
Tanggal laporan: 16 Feb 2026, 11:18

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 34 untuk mengubah section `Open Source Readiness Check` di audit report menjadi tabel.

**Statement Jawaban**
Plan 34 sudah dijalankan. Section `Open Source Readiness Check` pada `doc/AuditReports/AuditReport_0001_20260216_S063.md` sudah dikonversi dari checklist menjadi tabel markdown.

Detail hasil:
- Semua item readiness tetap sama.
- Status setiap item tetap sama seperti sebelum perubahan.
- Perubahan hanya pada format tampilan section (tanpa mengubah skor/temuan audit lain).

**File Terkait**
- doc/AuditReports/AuditReport_0001_20260216_S063.md
- CodexReport.md
- RefactorPlan.md

## Laporan 101 — 16 Feb 2026, 11:17
Tanggal laporan: 16 Feb 2026, 11:17

**Ringkasan Pertanyaan**
Meminta mengubah section `Open Source Readiness Check` di `doc/AuditReports/AuditReport_0001_20260216_S063.md` menjadi tabel.

**Statement Jawaban**
Sesuai aturan AGENTS, saya belum langsung mengubah file target dan sudah menyiapkan `Plan 34` di `RefactorPlan.md` untuk konversi section readiness check dari checklist ke tabel markdown, tanpa mengubah status item maupun substansi audit.

Saat ini saya menunggu perintah Anda untuk menjalankan `Plan 34`.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
- doc/AuditReports/AuditReport_0001_20260216_S063.md

## Laporan 100 — 16 Feb 2026, 11:11
Tanggal laporan: 16 Feb 2026, 11:11

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 33 untuk merapikan format markdown `AuditReport_0001_20260216_S063.md` dan menjadikannya struktur lanjutan di `AuditReportStructure.md`.

**Statement Jawaban**
Plan 33 sudah dijalankan.

Perubahan yang dilakukan:
- Merapikan `doc/AuditReports/AuditReport_0001_20260216_S063.md` menjadi format markdown yang konsisten (heading, tabel score breakdown, checklist, formatting finding, dan code block).
- Membersihkan konten noise/teks rusak yang menyisip pada executive summary.
- Menyusun ulang `doc/AuditReports/AuditReportStructure.md` menjadi template markdown reusable berbasis struktur report final.

Hasil akhir:
- Report audit markdown sekarang rapi dan mudah dibaca.
- Struktur template audit siap dipakai untuk audit berikutnya dengan placeholder field yang jelas.

**File Terkait**
- doc/AuditReports/AuditReport_0001_20260216_S063.md
- doc/AuditReports/AuditReportStructure.md
- CodexReport.md
- RefactorPlan.md
## Laporan 99 — 16 Feb 2026, 11:09
Tanggal laporan: 16 Feb 2026, 11:09

**Ringkasan Pertanyaan**
Meminta perbaikan dan perapihan format markdown pada `doc/AuditReports/AuditReport_0001_20260216_S063.md` untuk dijadikan struktur lanjutan di `doc/AuditReports/AuditReportStructure.md`.

**Statement Jawaban**
Sesuai aturan AGENTS, sebelum perubahan file saya sudah menyiapkan `Plan 33` di `RefactorPlan.md` untuk:
- merapikan format markdown report audit,
- menurunkan struktur final ke `AuditReportStructure.md` sebagai template reusable.

Saat ini saya menunggu perintah Anda untuk menjalankan `Plan 33`.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
- doc/AuditReports/AuditReport_0001_20260216_S063.md
- doc/AuditReports/AuditReportStructure.md
## Laporan 98 — 16 Feb 2026, 11:06
Tanggal laporan: 16 Feb 2026, 11:06

**Ringkasan Pertanyaan**
Meminta memperbaiki format `doc/AuditReports/AuditReport_0001_20260216_S063.txt` karena layout berantakan.

**Statement Jawaban**
Sesuai aturan AGENTS, saya belum langsung mengubah file audit. Saya sudah menyiapkan `Plan 32` di `RefactorPlan.md` untuk merapikan format report agar konsisten dengan `doc/AuditReports/AuditReportStructure.md`, tanpa mengubah substansi temuan dan skor audit.

Saat ini saya menunggu perintah Anda untuk menjalankan `Plan 32`.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
- doc/AuditReports/AuditReport_0001_20260216_S063.txt
- doc/AuditReports/AuditReportStructure.md
## Laporan 97 — 16 Feb 2026, 11:02
Tanggal laporan: 16 Feb 2026, 11:02

**Ringkasan Pertanyaan**
Meminta menjalankan audit project.

**Statement Jawaban**
Audit sudah dijalankan mengikuti `AuditRules.md` dan format `doc/AuditReports/AuditReportStructure.md`.

Hasil utama:
- Report audit baru dibuat di `doc/AuditReports/AuditReport_0001_20260216_S063.txt`.
- Total skor audit: **63/100** (Grade **C**).
- Status open-source readiness: **belum layak publik** tanpa hardening lanjutan.
- Risiko prioritas tinggi ada pada concurrency write NDB, parsing `.eml` tanpa batas ukuran, dan kelengkapan governance OSS (`SECURITY.md`, `CONTRIBUTING.md`, CI).

Catatan eksekusi test saat audit:
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release` gagal restore karena `NU1301` (akses `api.nuget.org` terblokir).
- Sesuai instruksi project, test dijalankan ulang dengan `--no-restore`.

**File Terkait**
- AuditRules.md
- doc/AuditReports/AuditReportStructure.md
- doc/AuditReports/AuditReport_0001_20260216_S063.txt
- CodexReport.md
## Laporan 96 — 16 Feb 2026, 09:42
Tanggal laporan: 16 Feb 2026, 09:42

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 31 untuk merapikan index laporan di `CodexReport.md`.

**Statement Jawaban**
Plan 31 sudah dijalankan. Saya membersihkan `CodexReport.md` dengan menghapus blok duplikasi laporan lama yang tertinggal di bagian paling bawah file (setelah `Laporan 1`), termasuk duplikasi untuk nomor `45`, `44`, `43`, `42`, `41`, dan `40`.

Hasil akhir:
- Struktur laporan kembali satu rangkaian utama.
- Nomor laporan `45` sampai `40` sekarang hanya muncul sekali pada posisi urut yang benar.
- Header ganda `# Codex Review Report - PST Projects` di bagian bawah sudah dihapus.

**File Terkait**
- CodexReport.md
- RefactorPlan.md
## Laporan 95 — 16 Feb 2026, 09:41
Tanggal laporan: 16 Feb 2026, 09:41

**Ringkasan Pertanyaan**
Meminta merapikan `CodexReport.md` karena laporan `41`, `45`, `44`, `43`, dan `40` tidak terindex dengan benar, lalu mengurutkan ulang dan membersihkan file.

**Statement Jawaban**
Saya sudah audit struktur `CodexReport.md` dan menemukan sumber masalahnya: ada blok duplikasi laporan lama (`45`, `44`, `43`, `42`, `41`, `40`) yang tertinggal di bagian paling bawah file setelah `Laporan 1`. 

Sesuai aturan project, saya sudah menyiapkan `Plan 31` di `RefactorPlan.md` untuk:
- menghapus blok duplikasi tersebut,
- mempertahankan entri utama yang sudah berada di posisi urut benar,
- dan validasi ulang index heading laporan.

Saat ini saya menunggu perintah Anda untuk menjalankan `Plan 31`.

**File Terkait**
- CodexReport.md
- RefactorPlan.md
## Laporan 94 — 16 Feb 2026, 09:36
Tanggal laporan: 16 Feb 2026, 09:36

**Ringkasan Pertanyaan**
Meminta audit kesiapan project `pst-libs` untuk dijadikan open source, mencakup kualitas kode dan compliance.

**Statement Jawaban**
Status saat ini: **belum siap rilis open source publik** tanpa hardening tambahan. Fondasi kode library sudah cukup baik (build `Emcode.Pst.Libs` sukses), tetapi ada beberapa blocker dan gap compliance yang signifikan.

Temuan utama:
1. **Blocker - Runner mengandung path lokal hardcoded**
- `src/Emcode.Pst.Runner/Program.cs` masih memuat path lokal user (`C:\Users\Aan\...`) dan output absolut (`D:\temp\output.html`), sehingga tidak portabel dan berisiko membocorkan konteks lokal.

2. **High - Belum ada baseline automation untuk repository open source**
- Tidak ada `.github/workflows` untuk CI build/test.
- Test project membutuhkan restore dari NuGet publik dan pada environment terisolasi ini gagal ke `https://api.nuget.org/v3/index.json`, sehingga status test lintas environment belum terjamin.

3. **High - Governance/compliance file untuk OSS belum tersedia**
- Tidak ditemukan `SECURITY.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SUPPORT.md`, atau `CHANGELOG.md`.

4. **High - Metadata packaging NuGet belum disiapkan**
- `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj` belum memiliki metadata publikasi penting seperti `PackageId`, `Version`, `Authors`, `Description`, `RepositoryUrl`, `PackageLicenseExpression`, dan `PackageReadmeFile`.

5. **Medium - API doc consistency belum bersih**
- Build dengan `/p:GenerateDocumentationFile=true` menghasilkan warning XML-doc `CS1573` (parameter tag tidak sinkron) di beberapa file.

6. **Medium - Reproducibility baseline belum ada**
- Belum ada `global.json` untuk pin versi SDK.
- Belum ada `Directory.Build.props` untuk standard build rule lintas project.

7. **Medium - Artefak sample binary perlu review legal/privacy**
- Repository menyertakan `doc/Samples/sample1.pst` dan `doc/Samples/sample1.rar`; sebelum publikasi perlu validasi bahwa kontennya aman dibagikan (tidak ada data sensitif/PII dan lisensi distribusi jelas).

Kesimpulan praktis:
- **Code library inti**: cukup matang untuk tahap preview internal.
- **Open-source readiness**: perlu 1 putaran hardening release (compliance docs, CI, packaging metadata, cleanup runner, legal check sample data) sebelum publish publik.

**File Terkait**
- src/Emcode.Pst.Runner/Program.cs
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
- src/Emcode.Pst.slnx
- README.md
- README.id.md
- LICENSE
- CodexReport.md
## Laporan 93 — 16 Feb 2026, 09:27
Tanggal laporan: 16 Feb 2026, 09:27

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 30 untuk sinkronisasi `README.id.md` agar merefleksikan fitur aktual.

**Statement Jawaban**
Plan 30 sudah dijalankan. `README.id.md` telah diperbarui agar selaras dengan implementasi kode saat ini.

Perubahan utama:
- Memperjelas daftar **Kemampuan Saat Ini** (termasuk properti message extended yang sudah dibaca/ditulis).
- Menambahkan section **Matriks Fitur** dengan status `Supported / Not Yet` untuk area penting (open/read, create/import, update/delete, crypt methods, data tree depth).
- Merapikan **Batasan Saat Ini** agar lebih presisi, termasuk:
  - import `.eml` masih berbasis path (belum stream),
  - update/delete pada `PstNdbWriter` belum didukung,
  - `Cyclic/EdpEncrypted` belum didukung,
  - writer data tree lebih dalam dari `XXBLOCK` belum didukung.

**File Terkait**
- README.id.md
- CodexReport.md
- RefactorPlan.md
## Laporan 92 — 16 Feb 2026, 09:25
Tanggal laporan: 16 Feb 2026, 09:25

**Ringkasan Pertanyaan**
Meminta update `README.id.md` apakah sudah merefleksikan semua fitur.

**Statement Jawaban**
Belum sepenuhnya. Dari audit cepat, ada beberapa bagian dokumentasi yang perlu disinkronkan dengan fitur aktual codebase (termasuk batasan protokol yang belum implementasi), sehingga saya menyiapkan `Plan 30` di `RefactorPlan.md`.

Fokus update plan:
- Sinkronisasi kemampuan aktual vs batasan (supported/partial/not supported).
- Koreksi poin yang berpotensi outdated.
- Tambahkan matriks fitur agar mudah diverifikasi ke implementasi.

Sesuai aturan project, saya menunggu perintah Anda untuk menjalankan Plan 30 sebelum mengubah isi `README.id.md`.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
- README.id.md
## Laporan 91 — 16 Feb 2026, 09:23
Tanggal laporan: 16 Feb 2026, 09:23

**Ringkasan Pertanyaan**
Meminta dibuatkan plan implementasi sisa protocol PST di `RefactorPlan.md`.

**Statement Jawaban**
Saya sudah menambahkan `Plan 29` di `RefactorPlan.md` untuk implementasi sisa protocol PST berdasarkan referensi `PST-241112`, dengan pendekatan bertahap (12 fase) yang mencakup:
- NDB allocation internals (`AMap/PMap/FMap/FPMap/DList`)
- recovery/rebuild
- update/delete + operasi subnode
- operasi LTP lanjutan (HN/BTH/TC)
- crypt `Cyclic`
- data tree depth > `XXBLOCK`
- Messaging advanced (`NameID/GUID stream/property lookup map`)
- attachment message object
- API import `.eml` berbasis stream
- conformance test dan pembaruan dokumentasi

Sesuai aturan project, saat ini saya hanya menyiapkan plan dan menunggu perintah Anda untuk mengeksekusi plan tersebut.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 90 — 16 Feb 2026, 09:20
Tanggal laporan: 16 Feb 2026, 09:20

**Ringkasan Pertanyaan**
Meminta validasi protocol PST apa saja yang belum diimplementasi berdasarkan referensi.

**Statement Jawaban**
Validasi terhadap referensi `doc/PST-241112.htm` menunjukkan library saat ini sudah mengimplementasi inti NDB/LTP untuk read dan sebagian write, tetapi masih ada gap penting berikut:

1. **NDB file-allocation internals belum diimplementasi penuh**
- Area referensi: AMap, PMap, FMap, FPMap, DList.
- Di codebase belum ada implementasi eksplisit pengelolaan/maintenance struktur ini; writer masih berfokus pada append block + update BBT/NBT root.

2. **NDB operations lanjutan belum lengkap**
- Referensi memuat operasi tambah/ubah/hapus subnode dan recovery.
- Di implementasi writer saat ini, `UpdateMessage`/`DeleteMessage` pada `PstNdbWriter` masih `NotSupportedException`.

3. **Crash recovery / AMap rebuilding belum ada**
- Referensi memuat topik crash recovery.
- Tidak ditemukan mekanisme recovery/rebuild map pada code saat ini.

4. **Crypt method belum lengkap**
- Enum mendefinisikan `Cyclic` dan `EdpEncrypted`.
- Implementasi `NdbCrypt`/reader hanya mendukung `None` dan `Permute`; method lain dilempar `NotSupportedException`.

5. **Batas kedalaman data tree write**
- Write data tree sudah mendukung XBLOCK/XXBLOCK.
- Kedalaman di atas XXBLOCK masih belum didukung (`NotSupportedException`).

6. **Messaging layer advanced structures belum lengkap**
- Referensi mencakup area seperti NameID/GUID Stream/property lookup map/search-related structures.
- Implementasi saat ini fokus pada folder/message/recipient/attachment core; struktur advanced tersebut belum terlihat di codebase.

7. **Attachment object type tertentu belum didukung**
- `PidTagAttachDataObject` (attachment bertipe pesan) masih belum didukung.

Kesimpulan: Implementasi sudah kuat untuk skenario inti (read PST + create/import draft dasar), tetapi belum mencakup seluruh spektrum protocol PST sesuai referensi MS-PST.

**File Terkait**
- doc/PST-241112.htm
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbCrypt.cs
- src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs
- src/Emcode.Pst.Libs/Domain/PstCryptMethod.cs
- README.id.md
- CodexReport.md
## Laporan 89 — 16 Feb 2026, 09:17
Tanggal laporan: 16 Feb 2026, 09:17

**Ringkasan Pertanyaan**
Meminta pengecekan kemampuan library (`Emcode.Pst.Libs.csproj`) untuk buka PST, import `.eml` via stream/file, buat email manual, dan ketersediaan versi async untuk method blocking.

**Statement Jawaban**
Hasil cek:

1. Bisa buka PST: **Ya**
- API tersedia: `PstFile.Open(...)` dan `PstFile.OpenAsync(...)`.
- Referensi: `src/Emcode.Pst.Libs/Application/PstFile.cs`.

2. Import `.eml` pakai file: **Ya**
- API tersedia: `ImportEml(PstFolder, string emlPath)` dan `ImportEmlAsync(...)`.
- Referensi: `src/Emcode.Pst.Libs/Application/PstFile.cs`, `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs`.

3. Import `.eml` pakai stream: **Belum**
- Kontrak dan implementasi import saat ini menerima `string emlPath`, bukan `Stream`.
- Parser internal juga dibuka via path file (`File.ReadAllText` / `File.OpenRead` by path).
- Referensi: `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs`, `src/Emcode.Pst.Libs/Infrastructure/PstEmlParser.cs`.

4. Buat email manual: **Ya**
- API tersedia: `CreateMessage(PstFolder, PstMessageDraft)` dan `CreateMessageAsync(...)`.
- Referensi: `src/Emcode.Pst.Libs/Application/PstFile.cs`.

5. Method blocking ada versi async: **Mayoritas Ya**
- Open/CreateFolder/CreateMessage/ImportEml/Update/Delete punya pasangan async di facade+interface writer.
- Attachment read/open stream juga punya versi async (`OpenContentStreamAsync`, `ReadContentBytesAsync`).
- Referensi: `src/Emcode.Pst.Libs/Application/PstFile.cs`, `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs`, `src/Emcode.Pst.Libs/Domain/PstAttachment.cs`.

**File Terkait**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj
- src/Emcode.Pst.Libs/Application/PstFile.cs
- src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/PstEmlParser.cs
- src/Emcode.Pst.Libs/Domain/PstAttachment.cs
- CodexReport.md
## Laporan 88 — 16 Feb 2026, 09:14
Tanggal laporan: 16 Feb 2026, 09:14

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 28.

**Statement Jawaban**
Plan 28 sudah dijalankan. Saya memperbarui `README.md` agar secara eksplisit menjadikan `README.id.md` sebagai dokumentasi default (bahasa Indonesia), sekaligus menambahkan tautan langsung ke file tersebut.

Perubahan ini juga menormalkan kondisi sebelumnya karena `README.md` sempat kosong.

**File Terkait**
- README.md
- README.id.md
- CodexReport.md
- RefactorPlan.md
## Laporan 87 — 16 Feb 2026, 09:13
Tanggal laporan: 16 Feb 2026, 09:13

**Ringkasan Pertanyaan**
Meminta perbaikan `README.md` agar referensi ke `README.id.md` menjadi default language.

**Statement Jawaban**
Saya sudah menyiapkan Plan 28 di `RefactorPlan.md` untuk perubahan `README.md` agar menjadikan `README.id.md` sebagai referensi bahasa default. 

Catatan penting yang saya temukan saat pengecekan: `README.md` saat ini berada dalam kondisi kosong (ukuran 0 byte), sehingga saat eksekusi plan nanti perubahan perlu sekaligus memastikan `README.md` memiliki konten pengarah yang benar.

Sesuai instruksi project, saya menunggu perintah Anda untuk menjalankan Plan 28.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
- README.md
- README.id.md
## Laporan 86 — 16 Feb 2026, 09:11
Tanggal laporan: 16 Feb 2026, 09:11

**Ringkasan Pertanyaan**
Menyampaikan bahwa `dotnet restore` di folder test berhasil.

**Statement Jawaban**
Saya verifikasi ulang dengan menjalankan test penuh. Hasilnya:

- `dotnet test src/Emcode.Pst.slnx --no-restore` masih gagal pada target `VSTest` level solution tanpa detail error eksplisit.
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj --no-restore -v normal` **berhasil penuh**.

Ringkasan hasil test:
- Total test: 26
- Passed: 26
- Failed: 0
- Durasi: ~0.63 detik

Catatan:
- Ada 1 warning analyzer (`CA2022`) di `tests/Emcode.Pst.Tests/NdbWriterTests.cs`, tetapi tidak mempengaruhi hasil test (tetap sukses).

**File Terkait**
- CodexReport.md
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
- tests/Emcode.Pst.Tests/NdbWriterTests.cs
- src/Emcode.Pst.slnx
## Laporan 85 — 16 Feb 2026, 09:08
Tanggal laporan: 16 Feb 2026, 09:08

**Ringkasan Pertanyaan**
Menanyakan kenapa test harus dengan NuGet online dan apakah bisa full local.

**Statement Jawaban**
Tidak harus online. Full local bisa, tetapi harus ada semua package dependency di mesin lokal/feed lokal.

Kenapa saat ini tetap mencoba online:
- Project test memakai `PackageReference` (`Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`).
- NuGet source aktif mencakup `nuget.org`, dan paket test belum ada di cache lokal environment ini.
- Karena paket belum tersedia lokal, restore otomatis mencoba ke endpoint online (termasuk repository signatures), lalu gagal karena network diblokir.

Agar full local:
1. Sediakan local feed/cache yang berisi semua paket yang dibutuhkan.
2. Tambahkan `NuGet.config` repo-level untuk `clear` sources lalu arahkan hanya ke local/internal feed.
3. Jalankan restore/test dengan source lokal tersebut.

**File Terkait**
- CodexReport.md
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
- src/Emcode.Pst.slnx
## Laporan 84 — 16 Feb 2026, 09:07
Tanggal laporan: 16 Feb 2026, 09:07

**Ringkasan Pertanyaan**
Meminta analisis penyebab `NU1301` dan cara resolve.

**Statement Jawaban**
Penyebab `NU1301` terkonfirmasi karena koneksi network ke NuGet HTTPS endpoint gagal, bukan karena salah konfigurasi project file.

Temuan diagnosis:
- Source NuGet aktif: `https://api.nuget.org/v3/index.json`.
- `Test-NetConnection api.nuget.org -Port 443` menghasilkan `TcpTestSucceeded: False`.
- `Invoke-WebRequest https://api.nuget.org/v3/index.json` gagal dengan pesan socket access forbidden.
- Saat restore, error juga muncul pada endpoint signature: `https://api.nuget.org/v3-index/repository-signatures/5.0.0/index.json`.

Arah resolve:
1. Buka akses firewall/proxy ke `api.nuget.org:443` (termasuk endpoint signature di domain yang sama).
2. Jika pakai proxy corporate, set proxy untuk NuGet (`http_proxy` / `https_proxy` atau konfigurasi proxy di `NuGet.Config`).
3. Jika internet dibatasi permanen, gunakan internal feed/mirror (Azure Artifacts/Nexus/Artifactory) dan arahkan source NuGet ke feed internal tersebut.
4. Setelah akses feed tersedia, jalankan ulang `dotnet restore` lalu `dotnet test`.

**File Terkait**
- CodexReport.md
- src/Emcode.Pst.slnx
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
## Laporan 83 — 16 Feb 2026, 09:04
Tanggal laporan: 16 Feb 2026, 09:04

**Ringkasan Pertanyaan**
Meminta menjalankan full test.

**Statement Jawaban**
Saya sudah menjalankan full test kembali, namun masih gagal pada tahap restore dependency test.

Perintah:
- `dotnet test src/Emcode.Pst.slnx -v minimal`
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -v minimal`

Hasil:
- Run solution gagal pada proses awal restore/test run.
- Run test project memberikan error `NU1301` saat mengakses repository signature NuGet:
  `https://api.nuget.org/v3-index/repository-signatures/5.0.0/index.json`
  dengan pesan socket access forbidden (`api.nuget.org:443`).

Kesimpulan:
- Full test masih belum bisa dieksekusi karena akses network ke endpoint NuGet yang dibutuhkan restore masih terblokir.

**File Terkait**
- CodexReport.md
- src/Emcode.Pst.slnx
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
## Laporan 82 — 16 Feb 2026, 09:00
Tanggal laporan: 16 Feb 2026, 09:00

**Ringkasan Pertanyaan**
Menanyakan kenapa restore package masih terblokir.

**Statement Jawaban**
Restore masih terblokir karena environment eksekusi saat ini membatasi akses network keluar ke NuGet (`api.nuget.org:443`). Ini terlihat dari error `NU1301` dan pesan socket access forbidden pada saat `dotnet restore`/`dotnet test`.

Secara praktis, selama host proses tidak punya akses HTTPS ke `https://api.nuget.org/v3/index.json` (atau tidak ada mirror/feed internal yang bisa diakses), proses restore package test tidak akan berhasil.

**File Terkait**
- CodexReport.md
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
- src/Emcode.Pst.slnx
## Laporan 81 — 16 Feb 2026, 08:59
Tanggal laporan: 16 Feb 2026, 08:59

**Ringkasan Pertanyaan**
Meminta mencoba menjalankan test penuh lagi.

**Statement Jawaban**
Saya sudah mencoba ulang full test, hasilnya masih gagal karena restore package test ke NuGet terblokir di environment.

Perintah yang dijalankan:
- `dotnet test src/Emcode.Pst.slnx -v minimal`
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -v minimal`

Hasil:
- Run solution: gagal saat awal eksekusi (keluar sebelum test berjalan).
- Run test project: gagal restore dependency dengan `NU1301` ke `https://api.nuget.org/v3/index.json` dan pesan socket access forbidden (`api.nuget.org:443`).

Kesimpulan:
- Full test masih **belum bisa dijalankan** sampai akses restore package tersedia.

**File Terkait**
- CodexReport.md
- src/Emcode.Pst.slnx
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
## Laporan 80 — 16 Feb 2026, 08:58
Tanggal laporan: 16 Feb 2026, 08:58

**Ringkasan Pertanyaan**
Meminta menjalankan test full.

**Statement Jawaban**
Saya sudah menjalankan perintah test full, namun test suite belum bisa dieksekusi penuh di environment ini karena restore dependency test terblokir akses jaringan ke NuGet (`api.nuget.org:443`).

Perintah yang dijalankan:
- `dotnet test src/Emcode.Pst.slnx -v minimal`
- `dotnet test src/Emcode.Pst.slnx -v normal`
- `dotnet restore src/Emcode.Pst.slnx -v diag`
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj --no-restore -v normal`

Hasil penting:
- Pada level solution, target restore gagal (Build FAILED tanpa error detail di output normal).
- Pada level test project, build/test penuh tidak benar-benar jalan karena paket test belum tersedia lokal; saat build test project terjadi error `NU1301` ke `https://api.nuget.org/v3/index.json` (socket access forbidden).

Kesimpulan:
- Full test **belum bisa divalidasi** pada sesi ini karena pembatasan network restore dependency.

**File Terkait**
- CodexReport.md
- tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj
- src/Emcode.Pst.slnx
## Laporan 79 — 16 Feb 2026, 08:55
Tanggal laporan: 16 Feb 2026, 08:55

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 27.

**Statement Jawaban**
Plan 27 sudah dijalankan pada codebase. Implementasi mencakup penambahan mapping write MAPI pada message untuk `PidTagMessageClass`, `PidTagMessageFlags`, timestamp utama (`DeliveryTime`, `ClientSubmitTime`, `LastModificationTime`), threading (`PidTagConversationTopic`, `PidTagConversationIndex`), `PidTagTransportMessageHeaders`, receipt flags, serta `Importance/Priority/Sensitivity`. 

Model domain juga diperluas agar properti tersebut bisa dibawa dari draft dan dibaca ulang (`PstMessageDraft`, `PstMessage`, `PstRecipient`). Reader (`PstNdbReader`) diperbarui untuk parse properti baru saat read-back. Integration test `PstNdbWriterIntegrationTests` diperluas untuk verifikasi persist/read-back properti Plan 27. Dokumentasi `README.md` juga diperbarui untuk status mapping MAPI write terbaru.

Verifikasi:
- `dotnet build src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj --no-restore` berhasil.
- `dotnet test` penuh belum bisa diverifikasi di environment ini karena restore package NuGet terblokir akses jaringan (`api.nuget.org:443`).

**File Terkait**
- src/Emcode.Pst.Libs/Domain/PstMessageDraft.cs
- src/Emcode.Pst.Libs/Domain/PstMessage.cs
- src/Emcode.Pst.Libs/Domain/PstRecipient.cs
- src/Emcode.Pst.Libs/Infrastructure/PstEmlParser.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs
- tests/Emcode.Pst.Tests/PstNdbWriterIntegrationTests.cs
- README.md
- CodexReport.md
## Laporan 78 — 16 Feb 2026, 08:50
Tanggal laporan: 16 Feb 2026, 08:50

**Ringkasan Pertanyaan**
Meminta pengecekan apakah codebase sudah menjalankan Plan 27.

**Statement Jawaban**
Belum. Berdasarkan pengecekan `RefactorPlan.md`, `CodexReport.md`, `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs`, `tests/Emcode.Pst.Tests/PstNdbWriterIntegrationTests.cs`, dan `README.md`, Plan 27 masih berstatus rencana (belum dieksekusi). Bukti utamanya: pada `CodexReport.md` Laporan 77 tertulis "menunggu perintah untuk menjalankan plan tersebut", writer belum menulis properti utama Plan 27 seperti message class/flags/threading/transport headers/importance/priority/sensitivity, test integrasi belum memverifikasi properti tersebut, dan README masih menyebut mapping MAPI write masih minimal.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- tests/Emcode.Pst.Tests/PstNdbWriterIntegrationTests.cs
- README.md
## Laporan 77 — 15 Feb 2026, 11:43
Tanggal laporan: 15 Feb 2026, 11:43

**Ringkasan Pertanyaan**
Meminta update plan untuk menambah write mapping MAPI yang “Outlook lengkap”.

**Statement Jawaban**
Saya menambahkan Plan 27 di RefactorPlan.md untuk mapping write MAPI lengkap (message class, flags, timestamps, threading, transport headers, recipient/attachment tambahan), termasuk test integrasi dan update README. Menunggu perintah Anda untuk menjalankan plan tersebut.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 76 — 15 Feb 2026, 11:42
Tanggal laporan: 15 Feb 2026, 11:42

**Ringkasan Pertanyaan**
Menanyakan daftar properti MAPI untuk write yang belum ada.

**Statement Jawaban**
Saya memberi daftar ringkas properti MAPI umum yang belum kita tulis (message class, flags, timestamps lengkap, conversation/threading, transport headers, recipient/address book props, dan properti folder/system tertentu), serta menegaskan bahwa daftar ini bisa diperdalam sesuai kebutuhan Outlook/interop.

**File Terkait**
- CodexReport.md
## Laporan 75 — 15 Feb 2026, 11:41
Tanggal laporan: 15 Feb 2026, 11:41

**Ringkasan Pertanyaan**
Menanyakan maksud “mapping MAPI yang kita tulis masih minimal”.

**Statement Jawaban**
Saya menjelaskan bahwa saat ini writer hanya mengisi subset properti MAPI dasar (subject, body/html, sender, recipients, attachment metadata dan binary) dan belum menulis properti penting lain seperti message class, flags, timestamps lengkap, conversation index, transport headers, dan properti folder khusus. Properti tersebut diperlukan untuk kompatibilitas penuh dengan Outlook.

**File Terkait**
- CodexReport.md
## Laporan 74 — 15 Feb 2026, 11:40
Tanggal laporan: 15 Feb 2026, 11:40

**Ringkasan Pertanyaan**
Menanyakan apakah setelah Plan 26 dijalankan bisa import file .eml ke file PST.

**Statement Jawaban**
Saya menjelaskan bahwa Plan 26 fokus pada dukungan PC/TC multi-block agar body/attachment besar bisa dipersist. Import .eml sudah bisa dilakukan via PstNdbWriter saat ini (dengan mapping minimal), dan Plan 26 membuatnya lebih stabil untuk konten besar. Namun bila di PST tertentu masih ada kendala lain (mis. mapping MAPI lanjutan), perlu tahap tambahan.

**File Terkait**
- CodexReport.md
## Laporan 73 — 15 Feb 2026, 11:39
Tanggal laporan: 15 Feb 2026, 11:39

**Ringkasan Pertanyaan**
Meminta dibuatkan plan refactor untuk dukungan PC/TC multi-block (XBLOCK/XXBLOCK).

**Statement Jawaban**
Saya menambahkan Plan 26 di RefactorPlan.md yang mencakup builder data tree XBLOCK/XXBLOCK, perluasan LTP writer untuk multi-block, integrasi ke PstNdbWriter, test integrasi body/attachment besar, dan update README. Menunggu perintah Anda untuk menjalankan plan tersebut.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 72 — 15 Feb 2026, 11:38
Tanggal laporan: 15 Feb 2026, 11:38

**Ringkasan Pertanyaan**
Menanyakan apakah tanpa dukungan multi-block berarti tidak bisa attach file 10MB.

**Statement Jawaban**
Saya mengonfirmasi bahwa tanpa dukungan PC/TC multi-block, attachment sebesar 10MB tidak bisa ditulis karena melebihi kapasitas satu block. Dukungan multi-block diperlukan untuk menyimpan data besar.

**File Terkait**
- CodexReport.md
## Laporan 71 — 15 Feb 2026, 11:37
Tanggal laporan: 15 Feb 2026, 11:37

**Ringkasan Pertanyaan**
Menanyakan maksimum ukuran body/attachment (MB) jika belum ada dukungan PC/TC multi-block.

**Statement Jawaban**
Saya menjelaskan bahwa batas maksimal kira-kira sebesar ukuran block PST karena PC/TC masih single-block. Untuk Unicode sekitar 8 KB (~0.008 MB) dan untuk ANSI sekitar 512 byte (~0.0005 MB), dengan kapasitas efektif sedikit lebih kecil karena header heap dan map. Saya menawarkan untuk memastikan format PST agar angka pastinya jelas.

**File Terkait**
- CodexReport.md
## Laporan 70 — 15 Feb 2026, 11:36
Tanggal laporan: 15 Feb 2026, 11:36

**Ringkasan Pertanyaan**
Menanyakan ukuran per block message saat ini.

**Statement Jawaban**
Saya menjelaskan bahwa ukuran block mengikuti format PST: ANSI memakai 512 byte per block, Unicode memakai 8192 byte per block. Untuk writer saat ini, PC/TC juga dibatasi satu block, jadi kapasitas efektif sekitar ukuran block tersebut.

**File Terkait**
- CodexReport.md
## Laporan 69 — 15 Feb 2026, 11:35
Tanggal laporan: 15 Feb 2026, 11:35

**Ringkasan Pertanyaan**
Menanyakan apakah update dukungan PC/TC multi-block (XBLOCK/XXBLOCK) untuk body/attachment besar termasuk perubahan code yang sangat besar.

**Statement Jawaban**
Saya menjelaskan bahwa ini perubahan menengah-besar: perlu perubahan pada writer (membangun data tree XBLOCK/XXBLOCK), kemungkinan penyesuaian writer LTP untuk heap/table yang melampaui satu block, dan penyesuaian test. Namun tidak sebesar update/delete karena tidak menyentuh logika bisnis folder/message dan mapping table secara luas.

**File Terkait**
- CodexReport.md
## Laporan 68 — 15 Feb 2026, 11:34
Tanggal laporan: 15 Feb 2026, 11:34

**Ringkasan Pertanyaan**
Menanyakan apakah update/delete message/folder pada PstNdbWriter termasuk perubahan besar.

**Statement Jawaban**
Saya menjelaskan bahwa update/delete relatif besar karena menyentuh banyak bagian: perubahan NBT/BBT, update contents/hierarchy table, referensi subnode/attachment, serta potensi free-space/reuse dan flag status. Saya menawarkan untuk memecahnya bertahap jika diinginkan.

**File Terkait**
- CodexReport.md
## Laporan 67 — 15 Feb 2026, 11:33
Tanggal laporan: 15 Feb 2026, 11:33

**Ringkasan Pertanyaan**
Menanyakan maksud “Dukungan PC/TC multi-block (XBLOCK/XXBLOCK) untuk body/attachment besar”.

**Statement Jawaban**
Saya menjelaskan bahwa PC (Property Context) dan TC (Table Context) bisa berada di data tree multi-block ketika ukurannya melebihi satu block PST. Dalam kasus itu, data node berisi XBLOCK/XXBLOCK yang merujuk ke banyak data block. “Dukungan multi-block” berarti writer/reader harus bisa menulis dan membaca struktur XBLOCK/XXBLOCK tersebut agar body/attachment besar tetap tersimpan dan terbaca dengan benar.

**File Terkait**
- CodexReport.md
## Laporan 66 — 15 Feb 2026, 11:32
Tanggal laporan: 15 Feb 2026, 11:32

**Ringkasan Pertanyaan**
Menanyakan “apa itu” tanpa konteks objek yang dimaksud.

**Statement Jawaban**
Saya meminta klarifikasi singkat tentang istilah atau bagian mana yang dimaksud agar bisa menjelaskan dengan tepat.

**File Terkait**
- CodexReport.md
## Laporan 65 — 15 Feb 2026, 11:30
Tanggal laporan: 15 Feb 2026, 11:30

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 25 (persist create folder/message ke PST).

**Statement Jawaban**
Saya mengimplementasikan create folder/message yang benar-benar menulis node + Property Context + Table Row ke PST, termasuk subnode recipient/attachment, update NBT/BBT, dan commit saat dispose. Saya menambahkan integrasi dispose pada `PstFile`, menambahkan test integrasi persist baca ulang, serta memperbarui README dengan contoh write-to-disk dan batasan. Semua test lulus.

**Test**
- dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release

**File Terkait**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Application/PstFile.cs
- tests/Emcode.Pst.Tests/NdbBtreeUpdateTests.cs
- tests/Emcode.Pst.Tests/PstNdbWriterIntegrationTests.cs
- README.md
- CodexReport.md
## Laporan 64 — 15 Feb 2026, 11:02
Tanggal laporan: 15 Feb 2026, 11:02

**Ringkasan Pertanyaan**
Meminta menjalankan semua tahapan hingga selesai save PST.

**Statement Jawaban**
Saya menambahkan Plan 25 di RefactorPlan.md untuk implementasi end-to-end create folder/message yang benar-benar menulis node + Property Context + Table Row ke PST, update NBT/BBT, test integrasi, dan update README. Sesuai instruksi, saya menunggu perintah Anda untuk menjalankan plan ini.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 63 — 15 Feb 2026, 10:59
Tanggal laporan: 15 Feb 2026, 10:59

**Ringkasan Pertanyaan**
Menanyakan sisa tahapan untuk integrasi PstNdbWriter agar bisa save PST.

**Statement Jawaban**
Masih tersisa 1 tahapan besar: implementasi create message/folder yang benar-benar menulis node + Property Context + Table Row ke PST (serta update NBT/BBT dan header). Setelah itu tahap penutup berupa test integrasi baca ulang dan dokumentasi README bila ada perubahan perilaku.

**File Terkait**
- CodexReport.md
## Laporan 62 — 15 Feb 2026, 10:57
Tanggal laporan: 15 Feb 2026, 10:57

**Ringkasan Pertanyaan**
Menanyakan apa saja yang akan diimplementasi untuk create message/folder agar benar-benar menulis node + property context + table row ke PST.

**Statement Jawaban**
Saya menjelaskan cakupan implementasi yang diperlukan: pembuatan node baru di NBT, alokasi data block/BID, penulisan Heap-on-Node + Property Context, pembuatan/penambahan row pada Table Context (contents/recipient/attachment), update hierarchy/contents table pada folder, serta update header/root BBT/NBT dan integrasi PstNdbWriter agar operasi create message/folder benar-benar persisten. Saya juga menekankan perlunya mapping MAPI minimal, penanganan subnode attachment/recipient, dan test integrasi baca ulang.

**File Terkait**
- CodexReport.md
## Laporan 61 — 15 Feb 2026, 10:55
Tanggal laporan: 15 Feb 2026, 10:55

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 24 (implementasi update BBT/NBT di file) beserta test.

**Statement Jawaban**
Saya mengimplementasikan penulisan ulang BBT/NBT ke file dengan serialisasi page (multi-level bila diperlukan), pembaruan root BREF di header, serta encoding data block untuk metode crypt. Saya menambahkan test integrasi update BBT pada salinan PST dan memastikan block baru bisa dibaca ulang lewat BBT yang diperbarui. PstNdbWriter kini memuat snapshot BBT/NBT saat init dan commit BBT/NBT saat dispose (masih terbatas pada append block). README diperbarui untuk menegaskan status write-to-disk yang masih eksperimental. Semua test lulus.

**Test**
- dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release

**File Terkait**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBtreeWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBlockWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbCrypt.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- 	ests/Emcode.Pst.Tests/NdbBtreeUpdateTests.cs
- 	ests/Emcode.Pst.Tests/NdbWriterTests.cs
- README.md
- CodexReport.md
## Laporan 60 — 15 Feb 2026, 10:41
Tanggal laporan: 15 Feb 2026, 10:41

**Ringkasan Pertanyaan**
Meminta implementasi update BBT/NBT di file agar persist benar-benar bekerja.

**Statement Jawaban**
Saya menambahkan Plan 24 di RefactorPlan.md untuk implementasi update BBT/NBT di file, termasuk serialisasi page, penulisan tree baru, update header NDB, integrasi PstNdbWriter, test integrasi, dan update README. Menunggu perintah Anda untuk menjalankan plan tersebut.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 59 — 15 Feb 2026, 10:35
Tanggal laporan: 15 Feb 2026, 10:35

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 23, menjalankan unit test, dan update README.

**Statement Jawaban**
Saya menambahkan writer NDB tahap 3 sebagai scaffold: NdbBlockWriter (menulis block), NdbBtreeWriter (update in-memory BBT/NBT), NdbWriter (orchestrator), NdbHeaderWriter, dan PstNdbWriter (eksperimental, masih belum mengubah BBT/NBT di disk). Saya menambahkan unit test untuk writer NDB dan memperbarui README agar status write-to-disk dijelaskan. Seluruh test lulus, dengan satu peringatan CA2022 pada test. Persist ke disk masih diblokir sampai implementasi update BBT/NBT di file selesai.

**Test**
- dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release

**File Terkait**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBlockWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBtreeWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- 	ests/Emcode.Pst.Tests/NdbWriterTests.cs
- README.md
- CodexReport.md
## Laporan 58 — 15 Feb 2026, 10:26
Tanggal laporan: 15 Feb 2026, 10:26

**Ringkasan Pertanyaan**
Meminta menjalankan tahap 3 (integrasi PstNdbWriter, update B-Tree/BBT/NBT, persist ke disk), lalu jalankan unit test dan update README.

**Statement Jawaban**
Saya sudah menambahkan Plan 23 di RefactorPlan.md untuk tahap 3 sesuai instruksi. Sesuai aturan, saya menunggu perintah Anda untuk mengeksekusi plan ini sebelum melakukan perubahan kode, menjalankan test, dan mengubah README.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 57 — 15 Feb 2026, 10:23
Tanggal laporan: 15 Feb 2026, 10:23

**Ringkasan Pertanyaan**
Menanyakan apakah bisa menghitung jumlah token yang dipakai per prompt dan memasukkannya ke CodexReport.

**Statement Jawaban**
Saya tidak bisa menghitung jumlah token secara akurat dari sisi lokal karena tidak ada akses ke metrik tokenisasi runtime. Jika Anda menyediakan data token dari sistem (mis. log/token usage), saya bisa masukkan ke CodexReport.md sesuai format yang Anda inginkan.

**File Terkait**
- CodexReport.md
## Laporan 56 — 15 Feb 2026, 10:22
Tanggal laporan: 15 Feb 2026, 10:22

**Ringkasan Pertanyaan**
Menanyakan sisa berapa tahap lagi.

**Statement Jawaban**
Sisa 1 tahap inti lagi untuk Plan 20, yaitu tahap 3 (integrasi PstNdbWriter untuk persist ke disk, update B-Tree/BBT/NBT dan penyimpanan data). Setelah itu masih ada pekerjaan penutup berupa test integrasi dan update README sesuai scope Plan 20.

**File Terkait**
- CodexReport.md
## Laporan 55 — 15 Feb 2026, 10:20
Tanggal laporan: 15 Feb 2026, 10:20

**Ringkasan Pertanyaan**
Meminta menjalankan test tahap 2.

**Statement Jawaban**
Saya menjalankan test tahap 2 dan memperbaiki kegagalan pada writer LTP: menambahkan CEB row untuk Table Context, memperbaiki pembacaan boolean dengan fallback integer, mengubah penulisan boolean sebagai integer32, serta menghilangkan padding heap agar binary roundtrip sesuai. Setelah perbaikan, seluruh 22 test lulus.

**Test**
- dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release

**File Terkait**
- src/Emcode.Pst.Libs/Infrastructure/Ltp/TableRowWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContextWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs
- 	ests/Emcode.Pst.Tests/LtpWriterTests.cs
- CodexReport.md
## Laporan 54 — 15 Feb 2026, 10:02
Tanggal laporan: 15 Feb 2026, 10:02

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 22 (property context & table row writer).

**Statement Jawaban**
Saya mengimplementasikan writer LTP tahap 2: opsi writer, writer Property Context, writer Table Row, serta unit test untuk roundtrip PC/TC. Implementasi ini membangun heap single-block yang dapat dibaca ulang oleh reader saat ini, termasuk dukungan sync/async dengan CancellationToken.

**Test**
- Tidak dijalankan (belum diminta).

**File Terkait**
- src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpWriterOptions.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContextWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/TableRowWriter.cs
- 	ests/Emcode.Pst.Tests/LtpWriterTests.cs
- CodexReport.md
## Laporan 53 — 15 Feb 2026, 09:49
Tanggal laporan: 15 Feb 2026, 09:49

**Ringkasan Pertanyaan**
Meminta lanjut tahap 2 (property context & table row writer).

**Statement Jawaban**
Saya menambahkan Plan 22 di RefactorPlan.md untuk tahap 2, mencakup writer Property Context dan Table Row, opsi LTP writer, API sync/async dengan CancellationToken, serta unit test untuk validasi baca ulang. Menunggu perintah Anda untuk menjalankan plan ini sesuai instruksi.

**File Terkait**
- RefactorPlan.md
- CodexReport.md
## Laporan 52 — 15 Feb 2026, 09:46
Tanggal laporan: 15 Feb 2026, 09:46

**Ringkasan Pertanyaan**
Meminta menjalankan test tahap 1.

**Statement Jawaban**
Saya menjalankan test dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release. Hasil: semua 19 test lulus (0 gagal).

**Test**
- dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release

**File Terkait**
- CodexReport.md
## Laporan 51 — 15 Feb 2026, 09:44
Tanggal laporan: 15 Feb 2026, 09:44

**Ringkasan Pertanyaan**
Meminta menjalankan tahap 1 Plan 20: writer core + alokasi block/BID.

**Statement Jawaban**
Saya menyiapkan writer core NDB untuk alokasi BID dan block (termasuk alignment offset dan flag internal/eksternal), menambahkan model metadata alokasi, serta menambahkan unit test untuk validasi alignment, flag internal, dan API async. Ini adalah fondasi tahap 1 sebelum update B-Tree/BBT/NBT dan penulisan data ke disk.

**Test**
- Tidak dijalankan (belum diminta).

**File Terkait**
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriterCore.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBlockAllocation.cs`
- `tests/Emcode.Pst.Tests/NdbWriterCoreTests.cs`
- `RefactorPlan.md`
- `CodexReport.md`
## Laporan 50 — 15 Feb 2026, 09:30
Tanggal laporan: 15 Feb 2026, 09:30

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 20 (writer NDB/LTP untuk persist ke PST di disk).

**Statement Jawaban**
Plan 20 adalah perubahan besar dan berisiko (menulis struktur NDB/LTP, update B-Tree, table, dan node di PST). Saya belum menjalankannya karena perlu konfirmasi apakah Anda ingin saya mengeksekusi semua langkah sekaligus atau memecahnya menjadi tahapan kecil (mis. tahap 1: desain writer core + alokasi block/BID, tahap 2: property context & table row, tahap 3: integrasi PstNdbWriter + Drafts). Beri tahu scope/tahap yang Anda inginkan, lalu saya akan mulai implementasi.

**File Terkait**
- CodexReport.md
## Laporan 49 — 15 Feb 2026, 09:24
Tanggal laporan: 15 Feb 2026, 09:24

**Ringkasan Pertanyaan**
Menanyakan apakah write saat ini masih belum ke PST dan meminta plan agar bisa write ke struktur PST di disk.

**Statement Jawaban**
Saat ini write masih in-memory (belum menulis ke struktur PST di disk). Saya menambahkan Plan 20 di `RefactorPlan.md` untuk implementasi writer NDB/LTP agar pesan draft, recipients, dan attachments bisa dipersist ke PST, lengkap dengan mapping MAPI, update table, API async, test integrasi, dan update README. Menunggu persetujuan Anda sebelum menjalankan plan.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 48 — 15 Feb 2026, 09:20
Tanggal laporan: 15 Feb 2026, 09:20

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 19 untuk operasi write/import message.

**Statement Jawaban**
Saya mengimplementasikan operasi write in-memory melalui `PstInMemoryWriter`, menambah parser `.eml`, memperluas model draft (From/Recipients/Attachments), serta menambahkan API sync/async pada `IPstWriter` dan facade `PstFile`. Saya juga menambahkan test create message dan import `.eml`, serta memperbarui `README.md` dengan contoh penggunaan dan batasan (write belum dipersist ke file PST). 

**Test**
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release`

**File Terkait**
- `src/Emcode.Pst.Libs/Application/PstFile.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriterWithContext.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/PstWriteContext.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessageDraft.cs`
- `src/Emcode.Pst.Libs/Domain/PstDraftRecipient.cs`
- `src/Emcode.Pst.Libs/Domain/PstDraftAttachment.cs`
- `src/Emcode.Pst.Libs/Domain/PstRecipientType.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstInMemoryWriter.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstInMemoryAttachmentContentProvider.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstEmlParser.cs`
- `tests/Emcode.Pst.Tests/PstWriteTests.cs`
- `README.md`
- `CodexReport.md`

## Laporan 47 — 15 Feb 2026, 09:15
Tanggal laporan: 15 Feb 2026, 09:15

**Ringkasan Pertanyaan**
Menanyakan apakah setelah plan berjalan bisa membuat pesan manual (from/to/cc/bcc/attachment/html/plain body) dan menyimpannya ke Drafts.

**Statement Jawaban**
Plan 19 dirancang untuk operasi write/import message. Dengan implementasi lengkapnya, Anda bisa membuat pesan baru secara manual melalui model draft dan menyimpannya ke folder Drafts, termasuk field From/To/Cc/Bcc, body text/HTML, dan attachment. Kita perlu memastikan mapping MAPI untuk pesan draft (mis. message class, flags, dan folder Drafts) ditetapkan agar statusnya tetap sebagai draft. Jika Anda setujui, saya akan eksekusi Plan 19 dan memastikan dukungan simpan ke Drafts.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`


## Laporan 46 — 15 Feb 2026, 09:12
Tanggal laporan: 15 Feb 2026, 09:12

**Ringkasan Pertanyaan**
Meminta dibuatkan plan untuk operasi write email messages.

**Statement Jawaban**
Saya menambahkan Plan 19 di `RefactorPlan.md` yang merinci rancangan operasi write/import message (parser `.eml`, mapping MAPI, pembuatan node LTP/NDB, API sync/async dengan `CancellationToken`, test integrasi, dan update README). Menunggu persetujuan Anda sebelum menjalankan plan.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`
## Laporan 45 — 15 Feb 2026, 09:03
Tanggal laporan: 15 Feb 2026, 09:03

**Ringkasan Pertanyaan**
Menanyakan cara melihat from, to, dan cc.

**Statement Jawaban**
Gunakan PstMessage.SenderName/SenderEmailAddress untuk From, serta PstMessage.DisplayTo dan DisplayCc untuk To/Cc. Jika butuh per-recipient, gunakan PstMessage.Recipients dan baca RecipientType + EmailAddress/SmtpAddress.

**File Terkait**
- src/Emcode.Pst.Libs/Domain/PstMessage.cs
- CodexReport.md

## Laporan 44 — 15 Feb 2026, 08:53
Tanggal laporan: 15 Feb 2026, 08:53

**Ringkasan Pertanyaan**
Menanyakan cara membaca body email HTML atau teks.

**Statement Jawaban**
Body email tersedia pada PstMessage.Body (text/plain) dan PstMessage.HtmlBody (HTML). Anda dapat membaca keduanya setelah membuka PST dan iterasi message; jika HtmlBody ada, gunakan itu, dan fallback ke Body bila kosong.

**File Terkait**
- src/Emcode.Pst.Libs/Domain/PstMessage.cs
- README.md
- CodexReport.md

## Laporan 43 — 15 Feb 2026, 08:50
Tanggal laporan: 15 Feb 2026, 08:50

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 18 untuk menambahkan API read Stream/byte[] attachment (sync dan async) serta update README.

**Statement Jawaban**
Saya menambahkan API pada `PstAttachment` untuk membaca konten attachment sebagai Stream/byte[] (sync dan async dengan `CancellationToken`), membuat provider internal untuk mengambil `PidTagAttachDataBinary`, menghubungkan attachment ke subnode berdasarkan `AttachNumber`, menambah test sync/async, serta memperbarui `README.md` dengan contoh penggunaan. Semua perubahan sudah diuji.

**Test**
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release`

**File Terkait**
- `src/Emcode.Pst.Libs/Domain/PstAttachment.cs`
- `src/Emcode.Pst.Libs/Domain/PstAttachmentContentReference.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstAttachmentContentProvider.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/ContentsTableTests.cs`
- `README.md`
- `CodexReport.md`

## Laporan 42 — 15 Feb 2026, 08:39
Tanggal laporan: 15 Feb 2026, 08:39

**Ringkasan Pertanyaan**
Meminta dibuatkan metode read Stream/byte[] per attachment (sync dan async) serta update README untuk cara penggunaan.

**Statement Jawaban**
Saya menambahkan Plan 18 di `RefactorPlan.md` yang mencakup penambahan API attachment stream/byte[] (sync/async dengan CancellationToken), pembacaan data attachment di reader, penambahan test, dan update `README.md`. Menunggu persetujuan Anda sebelum menjalankan plan.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 41 — 15 Feb 2026, 08:37
Tanggal laporan: 15 Feb 2026, 08:37

**Ringkasan Pertanyaan**
Menanyakan cara membuka attachment stream untuk disimpan atau diexport.

**Statement Jawaban**
Saat ini library hanya membaca metadata attachment (nama, ukuran, content-id, metode) dari Attachment Table, belum ada API untuk membuka stream data attachment. Untuk export, kita perlu menambahkan pembacaan property data attachment dari subnode attachment (mis. data binary/object) dan menyediakan method pembuka stream/byte[] di domain atau service reader. Jika Anda setuju, saya bisa menyiapkan plan perubahan untuk menambah API tersebut.

**File Terkait**
- `src/Emcode.Pst.Libs/Domain/PstAttachment.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `CodexReport.md`



## Laporan 40 — 15 Feb 2026, 08:14
Tanggal laporan: 15 Feb 2026, 08:14

**Ringkasan Pertanyaan**
Meminta dibuatkan plan untuk implementasi properti MAPI tambahan di `PstMessage`.

**Statement Jawaban**
Saya menambahkan Plan 16 di `RefactorPlan.md` yang merinci subset properti baru (sender email, received time, display fields, dll.), helper di Property Context, pengisian di reader sync/async, dan test verifikasi.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 39 — 15 Feb 2026, 08:13
Tanggal laporan: 15 Feb 2026, 08:13

**Ringkasan Pertanyaan**
Menanyakan daftar properti MAPI yang belum diimplementasi pada `PstMessage`.

**Statement Jawaban**
Saat ini `PstMessage` hanya mengekspos Subject, Body, HtmlBody, SenderName, DeliveryTime, dan Size. Properti MAPI lain yang umum di message (mis. ReceivedTime, SenderEmailAddress, DisplayTo/Cc/Bcc, MessageClass, InternetMessageId, ConversationTopic/Index, Importance, Priority, Sensitivity, HasAttachments, dan properti attachment) belum tersedia di `PstMessage`.

**File Terkait**
- `CodexReport.md`

## Laporan 38 — 15 Feb 2026, 08:12
Tanggal laporan: 15 Feb 2026, 08:12

**Ringkasan Pertanyaan**
Menanyakan apakah `PstMessage` memiliki informasi received date.

**Statement Jawaban**
Saat ini `PstMessage` hanya memiliki `DeliveryTime` dan belum ada properti khusus untuk received date. Jika Anda ingin menambahkan received date, saya bisa siapkan plan untuk membaca properti MAPI terkait (mis. PR_MESSAGE_DELIVERY_TIME atau PR_MESSAGE_RECEIVED_TIME) dan mengeksposnya di `PstMessage`.

**File Terkait**
- `CodexReport.md`

## Laporan 37 — 15 Feb 2026, 08:09
Tanggal laporan: 15 Feb 2026, 08:09

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 15 (menambahkan size pada `PstMessage`).

**Statement Jawaban**
Saya menambahkan properti `Size` pada `PstMessage`, menambah pembacaan PidTagMessageSize dari Property Context, dan menambah test untuk memastikan size terisi pada `sample1.pst`.

**Test**
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release`

**File Terkait**
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/ContentsTableTests.cs`
- `CodexReport.md`

## Laporan 36 — 15 Feb 2026, 08:07
Tanggal laporan: 15 Feb 2026, 08:07

**Ringkasan Pertanyaan**
Meminta dibuatkan plan untuk menambahkan size pada `PstMessage`.

**Statement Jawaban**
Saya menambahkan Plan 15 di `RefactorPlan.md` untuk menambah properti size pada `PstMessage`, mapping property MAPI ukuran, serta test integrasi/unit untuk validasi size.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 35 — 15 Feb 2026, 08:06
Tanggal laporan: 15 Feb 2026, 08:06

**Ringkasan Pertanyaan**
Menanyakan apakah `PstMessage` memiliki informasi ukuran (size).

**Statement Jawaban**
Saat ini `PstMessage` belum memiliki properti ukuran. Struktur domain masih fokus pada subject/sender/body/html/delivery time. Jika Anda ingin menambahkan size, saya bisa siapkan plan untuk membaca properti ukuran message dari Property Context dan expose sebagai properti baru di `PstMessage`.

**File Terkait**
- `CodexReport.md`

## Laporan 34 — 15 Feb 2026, 07:59
Tanggal laporan: 15 Feb 2026, 07:59

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 14 untuk mengisi `README.md`.

**Statement Jawaban**
Saya mengisi `README.md` dengan ringkasan kemampuan library, quick start (sync/async), opsi pembukaan, batasan saat ini, dan roadmap goal project.

**Test**
- Tidak dijalankan (perubahan dokumentasi saja).

**File Terkait**
- `README.md`
- `CodexReport.md`

## Laporan 33 — 15 Feb 2026, 07:56
Tanggal laporan: 15 Feb 2026, 07:56

**Ringkasan Pertanyaan**
Meminta dibuatkan isi `README.md` yang menjelaskan kemampuan library dan cara pakai cepat.

**Statement Jawaban**
Saya menyiapkan Plan 14 di `RefactorPlan.md` untuk menyusun isi `README.md` (ringkasan kemampuan, status fitur, quick start sync/async, dan catatan batasan/roadmap). Menunggu persetujuan Anda sebelum menerapkan perubahan pada `README.md`.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 32 — 15 Feb 2026, 07:40
Tanggal laporan: 15 Feb 2026, 07:40

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 12 (parsing Sender/Body/HtmlBody dan urutan folder berdasarkan Hierarchy Table) serta memastikan method async bila memungkinkan.

**Statement Jawaban**
Saya menjalankan Plan 12 dengan menambahkan pembacaan Sender/Body/HtmlBody dari Property Context (termasuk decoding HTML biner dan fallback ANSI/Unicode), menambahkan fallback Body dari HtmlBody saat plain text tidak ada, mengimplementasikan urutan subfolder berdasarkan Hierarchy Table (beserta jalur async), dan menambah test untuk validasi properti message serta urutan subfolder hierarchy table pada `sample1.pst`.

**Test**
- `dotnet test tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj -c Release`

**File Terkait**
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/ContentsTableTests.cs`
- `tests/Emcode.Pst.Tests/HierarchyTableTests.cs`
- `CodexReport.md`

## Laporan 31 — 15 Feb 2026, 07:30
Tanggal laporan: 15 Feb 2026, 07:30

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 13 untuk penambahan API async.

**Statement Jawaban**
Saya menambahkan API async dari `PstFile.OpenAsync` hingga reader NDB, termasuk `IPstReader.ReadAsync`, implementasi async pada `PstMinimalReader`/`PstNdbReader`, serta reader NDB (`NdbHeaderReader`, `PstBTreeReader`, `PstBlockReader`) dan menambah test `OpenAsync`.

**Test**
- `dotnet test src/Emcode.Pst.slnx -c Release`

**File Terkait**
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstReader.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`
- `src/Emcode.Pst.Libs/Application/Internal/NullPstReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBTreeReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBlockReader.cs`
- `tests/Emcode.Pst.Tests/PstFileOpenTests.cs`
- `CodexReport.md`

## Laporan 30 — 15 Feb 2026, 07:23
Tanggal laporan: 15 Feb 2026, 07:23

**Ringkasan Pertanyaan**
Meminta dibuatkan plan khusus untuk penambahan API async.

**Statement Jawaban**
Saya menambahkan Plan 13 di `RefactorPlan.md` untuk penambahan API async (OpenAsync/ReadAsync dan reader NDB async) lengkap dengan scope dan kriteria selesai. Menunggu persetujuan sebelum implementasi.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 29 — 15 Feb 2026, 07:22
Tanggal laporan: 15 Feb 2026, 07:22

**Ringkasan Pertanyaan**
Meminta daftar method synchronous di Emcode.Pst.Libs yang bisa dibuat asynchronous beserta alasannya.

**Statement Jawaban**
Saya mengidentifikasi method yang melakukan IO sinkron (File/Stream read/seek) dan merekomendasikan kandidat async (ReadAsync/OpenAsync/ReadDataBlocksAsync/ReadBbtAsync/ReadNbtAsync) beserta alasan bahwa operasi tersebut IO-bound dan dapat memanfaatkan non-blocking IO. Saya juga mencatat method yang murni CPU-bound tidak perlu async.

**File Terkait**
- `CodexReport.md`

## Laporan 28 — 15 Feb 2026, 07:19
Tanggal laporan: 15 Feb 2026, 07:19

**Ringkasan Pertanyaan**
Meminta melanjutkan ke parsing properties lain (Sender, Body, HtmlBody) dan urutan folder berdasarkan Hierarchy Table.

**Statement Jawaban**
Saya menyiapkan Plan 12 di `RefactorPlan.md` untuk menambah parsing properti message tambahan dari PC dan implementasi Hierarchy Table untuk urutan folder. Menunggu persetujuan Anda sebelum menjalankan perubahan kode.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 27 — 15 Feb 2026, 07:15
Tanggal laporan: 15 Feb 2026, 07:15

**Ringkasan Pertanyaan**
Meminta mengurutkan RefactorPlan berdasarkan nomor plan.

**Statement Jawaban**
Saya mengurutkan seluruh plan di RefactorPlan berdasarkan nomor (urutan menurun agar plan terbaru tetap berada di atas).

**File Terkait**
- `RefactorPlan.md`

## Laporan 26 — 15 Feb 2026, 07:13
Tanggal laporan: 15 Feb 2026, 07:13

**Ringkasan Pertanyaan**
Meminta mengurutkan laporan di CodexReport berdasarkan nomor.

**Statement Jawaban**
Saya mengurutkan seluruh laporan berdasarkan nomor (urutan menurun agar laporan terbaru tetap berada di atas).

**File Terkait**
- `CodexReport.md`

## Laporan 25 — 15 Feb 2026, 07:15
Tanggal laporan: 15 Feb 2026, 07:15

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 11 (aktifkan parsing PC message, penanganan penuh XBLOCK/XXBLOCK, dan Contents Table).

**Statement Jawaban**
Saya mengaktifkan parsing PC message untuk Subject/DeliveryTime, memperbaiki pembacaan data tree XBLOCK/XXBLOCK termasuk lcbTotal dan decoding internal block yang benar, memperkuat Heap-on-Node multi-block, menambahkan Table Context untuk Contents Table, serta mengubah `PstNdbReader` agar memakai contents table berdasarkan NID table pada NBT untuk urutan message per folder. Saya juga menambah tes untuk urutan contents table dan validasi Subject/DeliveryTime pada `sample1.pst`.

**File Terkait**
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBlockReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/HeapOnNode.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/SubnodeReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/TableContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbPrimitives.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/ContentsTableTests.cs`

## Laporan 24 — 15 Feb 2026, 06:39
Tanggal laporan: 15 Feb 2026, 06:39

**Ringkasan Pertanyaan**
Menanyakan langkah selanjutnya dan status kemampuan library membaca email/folder PST.

**Statement Jawaban**
Saat ini library sudah bisa membuka PST, membaca folder nyata, dan mengenumerasi message berdasarkan NBT + `nidParent`. Namun subject/delivery time belum dibaca, dan urutan message belum mengikuti Contents Table (masih urutan NBT). Langkah selanjutnya adalah menjalankan Plan 11 untuk mengaktifkan parsing PC message dengan dukungan penuh XBLOCK/XXBLOCK serta Contents Table. Saya menunggu perintah Anda untuk menjalankan Plan 11.

**File Terkait**
- `CodexReport.md`

## Laporan 23 — 15 Feb 2026, 06:38
Tanggal laporan: 15 Feb 2026, 06:38

**Ringkasan Pertanyaan**
Meminta aktivasi parsing PC message dengan dukungan penuh XBLOCK/XXBLOCK serta parsing Contents Table untuk urutan message per folder.

**Statement Jawaban**
Saya sudah menyiapkan Plan 11 di `RefactorPlan.md` yang mencakup perbaikan data tree XBLOCK/XXBLOCK, Heap‑on‑Node multi‑block, aktivasi parsing subject/delivery time, dan implementasi Contents Table untuk urutan message. Sesuai instruksi, saya menunggu perintah Anda untuk menjalankan plan tersebut.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 22 — 15 Feb 2026, 06:37
Tanggal laporan: 15 Feb 2026, 06:37

**Ringkasan Pertanyaan**
Menanyakan arti “parsing PC message” dan maksud “penanganan penuh XBLOCK/XXBLOCK”.

**Statement Jawaban**
“Parsing PC message” berarti membaca **Property Context** (PC) milik *Message object* di layer LTP: PC adalah BTree-on-Heap yang berisi pasangan *property id → value* untuk properti email (contoh: subject, delivery time, sender, dll). Reader harus membuka Heap‑on‑Node (HN), membaca BTH header, lalu menelusuri leaf record PC untuk mengambil nilai properti. 
“Penanganan penuh XBLOCK/XXBLOCK” berarti mendukung **data tree** ketika data node tidak muat dalam satu data block. Jika BID bertipe internal, data sebenarnya berada di sejumlah block yang dirujuk oleh XBLOCK/XXBLOCK; reader harus membaca struktur XBLOCK/XXBLOCK, mengumpulkan semua BID anak, lalu menggabungkan data blok tersebut agar HN/PC bisa diparse dengan benar. Tanpa dukungan penuh ini, sebagian PC message bisa gagal dibaca karena data HN tersebar di beberapa blok.

**File Terkait**
- Tidak ada perubahan file (penjelasan saja).

## Laporan 21 — 15 Feb 2026, 06:32
Tanggal laporan: 15 Feb 2026, 06:32

**Ringkasan Pertanyaan**
Meminta menjalankan Plan 10 untuk update parsing NDB agar bisa membaca folder/message nyata dari `sample1.pst`.

**Statement Jawaban**
Saya menjalankan Plan 10 dengan menambahkan parser NDB (header, BBT/NBT), reader blok dengan decoding Permute, Heap-on-Node + Property Context untuk nama folder, serta enumerasi message berbasis NBT dan `nidParent`. Reader default `PstFile.Open` kini memakai `PstNdbReader`, runner menampilkan crypt method, dan saya menambahkan project test beserta tiga test integrasi/unit yang memakai `doc/Samples/sample1.pst`. Untuk saat ini parsing subject/delivery time message belum diaktifkan (message tetap ter-enumerasi) agar pembacaan stabil di sample; fokus pada folder/message nyata sesuai permintaan.

**Test**
- `dotnet build src/Emcode.Pst.slnx -c Release`
- `dotnet test src/Emcode.Pst.slnx -c Release`

**File Terkait**
- `src/Emcode.Pst.Libs/Domain/PstCryptMethod.cs`
- `src/Emcode.Pst.Libs/Domain/PstHeaderInfo.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbPrimitives.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbEntries.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBTreeReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbCrypt.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBlockReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/SubnodeReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpPrimitives.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/HeapOnNode.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Properties/AssemblyInfo.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`
- `src/Emcode.Pst.Runner/Program.cs`
- `src/Emcode.Pst.slnx`
- `tests/Emcode.Pst.Tests/Emcode.Pst.Tests.csproj`
- `tests/Emcode.Pst.Tests/TestData.cs`
- `tests/Emcode.Pst.Tests/NdbHeaderReaderTests.cs`
- `tests/Emcode.Pst.Tests/BtreeReaderTests.cs`
- `tests/Emcode.Pst.Tests/PstFileOpenTests.cs`
- `CodexReport.md`

## Laporan 20 — 14 Feb 2026, 09:30
Tanggal laporan: 14 Feb 2026, 09:30

**Ringkasan Pertanyaan**
Meminta update project ke tahap parsing NDB agar bisa membaca folder/message nyata dari `sample1.pst`.

**Statement Jawaban**
Saya menyiapkan rencana implementasi parsing NDB (Node Database) hingga pembacaan folder/message nyata, termasuk penambahan parser BBT/NBT, LTP minimal, integrasi reader ke `PstFile`, serta test/integration test menggunakan `doc/Samples/sample1.pst`. Sesuai instruksi, saya menunggu perintah Anda sebelum menjalankan perubahan kode.

**File Terkait**
- `RefactorPlan.md`
- `CodexReport.md`

## Laporan 19 — 15 Feb 2026, 05:53
Tanggal laporan: 15 Feb 2026, 05:53

**Ringkasan Pertanyaan**
Menanyakan arti parsing NDB pada konteks PST.

**Statement Jawaban**
Parsing NDB (Node Database) adalah proses membaca struktur inti PST yang menyimpan node, block, dan B-Tree untuk memetakan data. Dari NDB, kita dapat menemukan node folder/message, lokasi data (BID), subnode, dan heap/properti sehingga bisa enumerasi folder dan pesan.

**File Terkait**
- Tidak ada perubahan file (penjelasan saja).

## Laporan 18 — 15 Feb 2026, 05:51
Tanggal laporan: 15 Feb 2026, 05:51

**Ringkasan Pertanyaan**
Meminta melanjutkan Phase 1 untuk enumerasi folder/message dan menampilkan header di console runner.

**Statement Jawaban**
Saya menambahkan reader minimal yang mengisi `RootFolder` dan `Folders`, membuat enumerasi pesan mengembalikan koleksi (awal masih kosong), serta menambahkan output metadata header di `Emcode.Pst.Runner`.

**File Terkait**
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Domain/PstFolder.cs`
- `src/Emcode.Pst.Runner/Program.cs`

## Laporan 17 — 15 Feb 2026, 05:47
Tanggal laporan: 15 Feb 2026, 05:47

**Ringkasan Pertanyaan**
Meminta dibuatkan implementasi untuk membaca PST minimal.

**Statement Jawaban**
Saya menambahkan reader minimal yang memvalidasi header PST, mendeteksi format ANSI/Unicode, dan mengisi metadata header ke `PstFile`. Default reader pada `PstFile.Open` kini menggunakan reader minimal.

**File Terkait**
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Domain/PstHeaderInfo.cs`
- `src/Emcode.Pst.Libs/Domain/PstFormat.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/PstReadResult.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`

## Laporan 16 — 15 Feb 2026, 05:41
Tanggal laporan: 15 Feb 2026, 05:41

**Ringkasan Pertanyaan**
Meminta dibuatkan implementasi pembacaan PST.

**Statement Jawaban**
Saya perlu konfirmasi scope pembacaan yang diinginkan (misalnya hanya parsing header dan validasi format, atau sampai enumerasi folder/message). Saya rekomendasikan mulai dari reader minimal: parse header, deteksi ANSI/Unicode, dan expose metadata dasar sebagai langkah pertama.

**File Terkait**
- `CodexReport.md`

## Laporan 15 — 15 Feb 2026, 05:38
Tanggal laporan: 15 Feb 2026, 05:38

**Ringkasan Pertanyaan**
Meminta validasi apakah sudah ada object untuk membaca file PST.

**Statement Jawaban**
Project `Emcode.Pst.Libs.csproj` hanya berisi konfigurasi .NET; object pembaca PST ada di kode, yaitu `IPstReader` sebagai kontrak, `NullPstReader` sebagai stub, dan `PstFile.Open` yang memakai reader untuk mengisi folder/root. Implementasi reader nyata belum dibuat.

**File Terkait**
- `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstReader.cs`
- `src/Emcode.Pst.Libs/Application/Internal/NullPstReader.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`

## Laporan 14 — 15 Feb 2026, 05:32
Tanggal laporan: 15 Feb 2026, 05:32

**Ringkasan Pertanyaan**
Meminta iterasi semua file .cs pada solution dan menambahkan XML documentation untuk setiap code object.

**Statement Jawaban**
Saya menambahkan XML documentation berbahasa Indonesia untuk class, interface, method, dan property di semua file .cs pada solution, termasuk abstraction, domain, application, dan shared helper.

**File Terkait**
- `src/Emcode.Pst.Libs/Application/PstFile.cs`
- `src/Emcode.Pst.Libs/Application/PstOpenOptions.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstReader.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/PstReadResult.cs`
- `src/Emcode.Pst.Libs/Application/Internal/NullPstReader.cs`
- `src/Emcode.Pst.Libs/Domain/PstFolder.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessageDraft.cs`
- `src/Emcode.Pst.Libs/Shared/Guard.cs`

## Laporan 13 — 15 Feb 2026, 05:27
Tanggal laporan: 15 Feb 2026, 05:27

**Ringkasan Pertanyaan**
Meminta agar format CodexReport mengikuti pola Laporan 1 dengan ringkasan pertanyaan dan statement jawaban.

**Statement Jawaban**
Saya menyesuaikan seluruh entri agar berisi Ringkasan Pertanyaan dan Statement Jawaban, serta menjaga File Terkait sebagai rujukan perubahan file.

**File Terkait**
- `CodexReport.md`

## Laporan 12 — 15 Feb 2026, 05:21
Tanggal laporan: 15 Feb 2026, 05:21

**Ringkasan Pertanyaan**
Menambahkan bagian **Jawaban** di setiap laporan CodexReport agar memuat penjelasan jawaban codex.

**Statement Jawaban**
di setiap laporan CodexReport agar memuat penjelasan jawaban codex.

**Detail**
- Menyisipkan section `Jawaban` berdasarkan isi `Detail`/`Ringkasan`.
- Menjaga `File Terkait` sebagai rujukan perubahan file.

**File Terkait**
- `CodexReport.md`

## Laporan 11 — 15 Feb 2026, 05:18
Tanggal laporan: 15 Feb 2026, 05:18

**Ringkasan Pertanyaan**
Memperbarui format `RefactorPlan.md` agar sesuai dengan template sample.

**Statement Jawaban**
- Menyusun ulang struktur laporan plan.
- Menambahkan section ringkasan, sumber, lingkup, rencana prioritas, dan kriteria selesai.

**File Terkait**
- `RefactorPlan.md`

## Laporan 10 — 15 Feb 2026, 05:17
Tanggal laporan: 15 Feb 2026, 05:17

**Ringkasan Pertanyaan**
Memperbarui format `CodexReport.md` agar mengikuti struktur template sample.

**Statement Jawaban**
- Menyesuaikan header laporan, tanggal laporan, dan section ringkasan/detail.
- Menambahkan daftar file terkait sesuai perubahan.

**File Terkait**
- `CodexReport.md`

## Laporan 9 — 15 Feb 2026, 05:12
Tanggal laporan: 15 Feb 2026, 05:12

**Ringkasan Pertanyaan**
Restruktur log dan menambahkan nomor urut serta jam di laporan.

**Statement Jawaban**
- Permintaan: merapikan posisi log dan menambah nomor urut.
- Tindakan: memastikan timestamp lengkap dan format konsisten.

**File Terkait**
- `CodexReport.md`
- `RefactorPlan.md`

## Laporan 8 — 15 Feb 2026, 05:10
Tanggal laporan: 15 Feb 2026, 05:10

**Ringkasan Pertanyaan**
Menambahkan abstraction agar proses read/write PST dapat dites dan di-mock.

**Statement Jawaban**
- Menambahkan `IPstReader`, `IPstWriter`, dan `PstReadResult`.
- Menyediakan `NullPstReader` sebagai stub default.
- Mengintegrasikan reader/writer ke `PstFile.Open`.

**File Terkait**
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstReader.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstWriter.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/PstReadResult.cs`
- `src/Emcode.Pst.Libs/Application/Internal/NullPstReader.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`

## Laporan 7 — 15 Feb 2026, 05:06
Tanggal laporan: 15 Feb 2026, 05:06

**Ringkasan Pertanyaan**
Menilai kesiapan project untuk testing dan memberikan rekomendasi awal.

**Statement Jawaban**
- Menjelaskan bahwa test sekarang masih bersifat kontrak/behavior.
- Menyarankan penambahan abstraction agar test lebih bermakna.

**File Terkait**
- Tidak ada perubahan file (penjelasan saja).

## Laporan 6 — 15 Feb 2026, 05:06
Tanggal laporan: 15 Feb 2026, 05:06

**Ringkasan Pertanyaan**
Refactor ke clean architecture berbasis folder/namespace untuk DRY.

**Statement Jawaban**
- Entities dipindah ke `Domain`.
- `PstFile` dan `PstOpenOptions` dipindah ke `Application`.
- Menambahkan helper `Guard` dan update runner.

**File Terkait**
- `src/Emcode.Pst.Libs/Domain/PstFolder.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessageDraft.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`
- `src/Emcode.Pst.Libs/Application/PstOpenOptions.cs`
- `src/Emcode.Pst.Libs/Shared/Guard.cs`
- `src/Emcode.Pst.Runner/Program.cs`

## Laporan 5 — 15 Feb 2026, 05:04
Tanggal laporan: 15 Feb 2026, 05:04

**Ringkasan Pertanyaan**
Meminta konfirmasi scope penerapan clean architecture.

**Statement Jawaban**
- Menanyakan apakah layering dipisah project atau hanya folder/namespace.

**File Terkait**
- Tidak ada perubahan file (penjelasan saja).

## Laporan 4 — 15 Feb 2026, 05:03
Tanggal laporan: 15 Feb 2026, 05:03

**Ringkasan Pertanyaan**
Membuat solution `.slnx` dan memasukkan semua project.

**Statement Jawaban**
- Membuat `Emcode.Pst.slnx` dari migrasi `.sln`.
- Menambahkan project `Emcode.Pst.Libs` dan `Emcode.Pst.Runner`.

**File Terkait**
- `src/Emcode.Pst.slnx`
- `src/Emcode.Pst.sln`

## Laporan 3 — 15 Feb 2026, 05:01
Tanggal laporan: 15 Feb 2026, 05:01

**Ringkasan Pertanyaan**
Membuat kerangka project .NET dan draft API surface.

**Statement Jawaban**
- Membuat `Emcode.Pst.Libs` (class library) dan `Emcode.Pst.Runner` (console).
- Menambahkan draft API: `PstFile`, `PstFolder`, `PstMessage`, `PstMessageDraft`, `PstOpenOptions`.

**File Terkait**
- `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`
- `src/Emcode.Pst.Runner/Emcode.Pst.Runner.csproj`
- `src/Emcode.Pst.Libs/*`
- `src/Emcode.Pst.Runner/Program.cs`

## Laporan 2 — 15 Feb 2026, 04:57
Tanggal laporan: 15 Feb 2026, 04:57

**Ringkasan Pertanyaan**
Menjelaskan dua tahap implementasi read dan write PST.

**Statement Jawaban**
- Tahap 1: read MVP (NDB, properties, enumerasi folder/message).
- Tahap 2: write MVP (CRUD minimal, konsistensi B-Tree).

**File Terkait**
- Tidak ada perubahan file (penjelasan saja).

## Laporan 1 — 15 Feb 2026, 04:54
Tanggal laporan: 15 Feb 2026, 04:54

**Ringkasan Pertanyaan**
Menjawab bisa atau tidaknya membuat library .NET C# untuk baca/tulis PST dari referensi file PST-241112.docx.

**Statement Jawaban**
- Bisa, tetapi tidak sederhana. Dokumen PST-241112.docx adalah spesifikasi format [MS-PST]; artinya Anda bisa membuat library .NET C# untuk baca/tulis PST dengan mengimplementasikan struktur NDB/Messaging layer (node, block, B‑Tree, heap, property, dll). Tantangannya: formatnya kompleks, banyak edge case, dan menulis PST jauh lebih sulit daripada membaca.

**File Terkait**
- `doc/PST-241112.docx`






































