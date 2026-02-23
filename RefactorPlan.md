## Plan 48 — 23 Feb 2026, 14:36
Tanggal plan: 23 Feb 2026, 14:36

**Ringkasan**
Memperbarui layout GitHub Pages agar pengguna bisa memilih mode `Dark` atau `Light`.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- doc/pages/assets/css/site.css
- doc/pages/index.html
- doc/pages/api/**/*.html
- doc/pages/help/**/*.html
- doc/pages/toc.html

**Rencana Prioritas**
1. Tambahkan token warna untuk dua tema (`light` dan `dark`) di CSS menggunakan custom properties.
2. Tambahkan komponen toggle mode pada layout halaman (button/switch) yang konsisten di seluruh halaman dokumentasi.
3. Simpan preferensi mode user di `localStorage` dan terapkan saat halaman dimuat.
4. Gunakan fallback ke preferensi sistem (`prefers-color-scheme`) jika user belum memilih mode.
5. Verifikasi kontras warna, keterbacaan, dan konsistensi elemen utama (heading, link, code block, table) pada kedua mode.

**Kriteria Selesai**
- Halaman docs memiliki kontrol untuk memilih `Dark`/`Light`.
- Preferensi tema tersimpan dan persisten antar halaman.
- Tampilan kedua mode tetap rapi dan readable di desktop/mobile.

## Plan 47 — 23 Feb 2026, 14:31
Tanggal plan: 23 Feb 2026, 14:31

**Ringkasan**
Restrukturisasi `doc/pages` agar seluruh konten dokumentasi menggunakan format HTML5.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- doc/pages/index.md
- doc/pages/toc.md
- doc/pages/help/**/*.md
- doc/pages/api/**/*.md
- doc/pages/index.html
- doc/pages/assets/css/site.css

**Rencana Prioritas**
1. Inventarisasi semua file `.md` di bawah `doc/pages` dan tentukan pasangan target `.html` dengan struktur path yang sama.
2. Konversi konten Markdown ke HTML5 statis (heading, paragraf, list, link, kode) sambil menjaga tautan antar halaman agar memakai ekstensi `.html`.
3. Pastikan halaman root `doc/pages/index.html` menjadi landing utama dan menaut ke dokumen HTML hasil konversi.
4. Rapikan styling di `doc/pages/assets/css/site.css` agar seluruh halaman HTML5 konsisten dan mudah dibaca di desktop/mobile.
5. Hapus atau arsip file Markdown dari alur publish setelah semua tautan valid, sehingga artifact Pages sepenuhnya HTML5.
6. Validasi cepat integritas link internal agar tidak ada referensi `.md` tersisa.

**Kriteria Selesai**
- Seluruh dokumen publish di `doc/pages` menggunakan file `.html`.
- Tidak ada tautan internal yang masih mengarah ke `.md`.
- Navigasi Help/API/TOC tetap berfungsi dari halaman landing.

## Plan 46 — 23 Feb 2026, 14:28
Tanggal plan: 23 Feb 2026, 14:28

**Ringkasan**
Memperbaiki error GitHub Pages karena artifact tidak memiliki `index.html` pada root `doc/pages`.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- doc/pages/index.html

**Rencana Prioritas**
1. Tambahkan file `doc/pages/index.html` sebagai halaman landing statis agar valid untuk deployment GitHub Pages berbasis artifact.
2. Isi halaman dengan navigasi ringkas ke dokumentasi Help, API, dan TOC yang sudah ada di `doc/pages`.
3. Pertahankan struktur deployment existing (`.github/workflows/pages.yml`) tanpa perubahan karena akar masalah ada pada entry file yang hilang.

**Kriteria Selesai**
- `doc/pages/index.html` tersedia di root artifact.
- Link utama dokumentasi dapat diakses dari halaman landing.

## Plan 45 — 23 Feb 2026, 14:06
Tanggal plan: 23 Feb 2026, 14:06

**Ringkasan**
Memperbarui `AGENTS.md` agar saat user meminta commit: tidak perlu log ke `CodexReport.md` dan tidak perlu menjalankan `RefactorPlan` terlebih dahulu.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- AGENTS.md

**Rencana Prioritas**
1. Tambahkan pengecualian eksplisit pada `LOGGING RULES` bahwa permintaan commit tidak perlu dicatat ke `CodexReport.md`.
2. Tambahkan pengecualian eksplisit pada `REFACTOR RULES` bahwa permintaan commit tidak wajib membuat plan di `RefactorPlan.md`.
3. Pastikan pengecualian commit tetap konsisten dengan `COMMIT & VERSIONING RULES` yang sudah ada.
4. Validasi redaksi rule agar jelas dan tidak ambigu.

**Kriteria Selesai**
- `AGENTS.md` memuat pengecualian commit untuk logging.
- `AGENTS.md` memuat pengecualian commit untuk keharusan refactor plan.
- Aturan commit/versioning tetap berlaku untuk permintaan commit.
## Plan 44 — 23 Feb 2026, 13:56
Tanggal plan: 23 Feb 2026, 13:56

**Ringkasan**
Memperbarui `AGENTS.md` agar saat user meminta commit, agent wajib menanyakan jenis kenaikan versi (`major`, `minor`, atau `build`), lalu memperbarui `ChangeLogs.md` dan increment versi package sesuai format `XX1.XX2.XX3`.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- AGENTS.md
- (referensi perilaku) ChangeLogs.md
- (referensi perilaku) src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj

**Rencana Prioritas**
1. Tambahkan rule baru pada `AGENTS.md` di section aturan kerja agar sebelum commit agent wajib meminta klasifikasi update versi: `major`, `minor`, atau `build`.
2. Definisikan mapping format versi `XX1.XX2.XX3`:
   - `XX1` = Major update.
   - `XX2` = Minor update.
   - `XX3` = Build update.
3. Tambahkan rule perilaku update versi:
   - `major`: naikkan `XX1`, reset `XX2` dan `XX3` ke `0`.
   - `minor`: naikkan `XX2`, reset `XX3` ke `0`.
   - `build`: naikkan `XX3`.
4. Tambahkan rule bahwa setiap permintaan commit harus disertai update `ChangeLogs.md` dan update versi pada file project yang relevan sebelum commit.
5. Pastikan aturan baru tidak bertentangan dengan prioritas rule yang sudah ada di `AGENTS.md`.

**Kriteria Selesai**
- `AGENTS.md` memuat aturan eksplisit pertanyaan jenis update versi sebelum commit.
- `AGENTS.md` memuat aturan update changelog + increment versi dengan format `XX1.XX2.XX3`.
- Mapping major/minor/build terdokumentasi jelas.
## Plan 43 — 23 Feb 2026, 13:47
Tanggal plan: 23 Feb 2026, 13:47

**Ringkasan**
Membuat file `ChangeLogs.md` untuk menyimpan histori perubahan versi dengan format standar dokumentasi rilis.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- ChangeLogs.md

**Rencana Prioritas**
1. Buat file `ChangeLogs.md` di root repository.
2. Terapkan struktur standar changelog yang konsisten:
   - Header dan tujuan dokumen.
   - Referensi format `Keep a Changelog`.
   - Referensi versioning `Semantic Versioning`.
   - Section `[Unreleased]`.
   - Template section versi rilis (`Added`, `Changed`, `Fixed`, `Removed`, `Security`, dll).
3. Tambahkan entri awal untuk versi terbaru yang sudah ada agar baseline terdokumentasi.
4. Pastikan gaya penulisan konsisten dengan dokumen lain dalam repo.

**Kriteria Selesai**
- `ChangeLogs.md` tersedia di root project.
- Format changelog siap dipakai untuk update versi berikutnya.
- Baseline versi awal sudah tercatat.
## Plan 42 — 23 Feb 2026, 13:41
Tanggal plan: 23 Feb 2026, 13:41

**Ringkasan**
Membuat workflow GitHub Actions untuk publish GitHub Pages dari folder `doc/pages`.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- .github/workflows/pages.yml
- (opsional) doc/pages/_config.yml (hanya bila perlu penyesuaian ringan)

**Rencana Prioritas**
1. Buat workflow `.github/workflows/pages.yml` dengan trigger `push` pada perubahan `doc/pages/**` dan `workflow_dispatch`.
2. Konfigurasi permission minimum untuk deployment Pages: `contents: read`, `pages: write`, `id-token: write`.
3. Gunakan action resmi GitHub Pages: `actions/checkout`, `actions/configure-pages`, `actions/upload-pages-artifact` dengan `path: doc/pages`, dan `actions/deploy-pages`.
4. Tambahkan `concurrency` untuk mencegah deploy bertumpuk.
5. Validasi sintaks YAML dan struktur workflow agar siap dijalankan di repository.

**Kriteria Selesai**
- File `.github/workflows/pages.yml` tersedia dan valid.
- Deploy GitHub Pages mengambil artefak dari `doc/pages`.
- Workflow dapat dipicu otomatis via push dan manual via Actions tab.
## Plan 41 — 23 Feb 2026, 13:30
Tanggal plan: 23 Feb 2026, 13:30

**Ringkasan**
Publish dokumentasi ke branch `gh-pages` sebagai source GitHub Pages, lalu hapus folder `doc/pages` dari branch utama.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- Branch `gh-pages` (konten dokumentasi root)
- doc/pages/* (penghapusan dari branch utama)
- (opsional) .github/workflows/pages.yml (jika diperlukan untuk fallback deploy)

**Rencana Prioritas**
1. Validasi status git lokal agar perubahan dokumentasi `doc/pages` terbaru siap dipublish.
2. Buat/perbarui branch `gh-pages` dari konten `doc/pages` (copy ke root branch), commit, lalu push ke remote `origin/gh-pages`.
3. Verifikasi branch `gh-pages` berisi artefak dokumentasi (`index.md`, `api/`, `help/`, `_config.yml`, assets).
4. Kembali ke branch kerja utama, hapus folder `doc/pages` dari tracking git.
5. Commit perubahan penghapusan `doc/pages` di branch utama.
6. Catat langkah setting repository GitHub Pages: source dari branch `gh-pages` root.

**Kriteria Selesai**
- Branch `gh-pages` terupdate dengan konten dokumentasi siap publish.
- Folder `doc/pages` sudah terhapus di branch utama.
- Struktur docs tetap dapat diakses melalui source `gh-pages` root.
## Plan 40 — 23 Feb 2026, 13:15
Tanggal plan: 23 Feb 2026, 13:15

**Ringkasan**
Restruktur dokumentasi GitHub Pages agar menyerupai pola Microsoft .NET Help: dokumentasi konseptual (`Help`) dan `API References` terstruktur dari XML documentation (namespace, type, property, event, method, dll).

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- doc/pages/index.md
- doc/pages/toc.md
- doc/pages/help/index.md
- doc/pages/help/getting-started.md
- doc/pages/help/concepts.md
- doc/pages/help/how-to/*.md
- doc/pages/help/faq.md
- doc/pages/api/index.md
- doc/pages/api/namespaces.md
- doc/pages/api/<namespace>/index.md
- doc/pages/api/<namespace>/<type>.md
- doc/pages/_config.yml
- doc/pages/assets/css/site.css (opsional, untuk layout navigasi ala docs)

**Rencana Prioritas**
1. Definisikan information architecture ala Microsoft Docs di `doc/pages`: `Help` (konseptual) terpisah dari `API Reference` (referensi teknis).
2. Siapkan `doc/pages/toc.md` sebagai navigasi sidebar bertingkat (Home > Help > API) agar pola jelajah serupa documentation framework.
3. Susun halaman Help: `getting-started`, `concepts`, `how-to`, `faq`, dengan tautan silang ke halaman API terkait.
4. Kumpulkan XML documentation dari build output/library source lalu petakan ke hirarki `Namespace > Type > Members`.
5. Susun `API References`:
   - `api/index.md` sebagai portal reference.
   - `api/namespaces.md` sebagai daftar namespace.
   - halaman namespace berisi daftar type.
   - halaman type berisi signature + deskripsi + tabel member (`Properties`, `Events`, `Methods`, `Constructors`, dan anggota lain jika ada).
6. Standarkan template halaman type agar konsisten (summary, declaration, remarks, exceptions, examples, see also) mengikuti gaya referensi Microsoft.
7. Tambahkan konfigurasi GitHub Pages pada `doc/pages/_config.yml` dan styling minimal agar layout docs rapi di desktop/mobile.
8. Validasi seluruh link internal, anchor member, dan konsistensi isi dengan XML docs serta API aktual di `src/Emcode.Pst.Libs`.

**Kriteria Selesai**
- Struktur `Help` dan `API References` tersedia di `doc/pages` dan siap publish ke GitHub Pages.
- Navigasi bertingkat (`toc`) memudahkan browsing seperti docs framework.
- Setiap object API yang didokumentasikan memiliki daftar anggota (property/event/method/dll) bersumber dari XML documentation.
- Tidak ada tautan internal/anchor yang rusak pada halaman yang dibuat.
## Plan 39 — 23 Feb 2026, 13:10
Tanggal plan: 23 Feb 2026, 13:10

**Ringkasan**
Merevisi rencana dokumentasi GitHub Pages ke struktur `doc/pages` dengan fokus pada halaman `Help` dan `Object References` (property, event, method, dll) berdasarkan XML documentation library.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- doc/pages/index.md
- doc/pages/references/index.md
- doc/pages/references/objects.md
- doc/pages/references/<namespace-or-type>.md (per object group, jika diperlukan)
- doc/pages/help/index.md
- doc/pages/_config.yml

**Rencana Prioritas**
1. Ubah struktur target dari `docs/` menjadi `doc/pages/` sebagai source publish GitHub Pages.
2. Buat landing page `doc/pages/index.md` dengan navigasi utama ke `Help` dan `References`.
3. Kumpulkan sumber XML documentation dari object C# di `src/Emcode.Pst.Libs` sebagai basis konten reference.
4. Susun halaman `doc/pages/references/index.md` dan `doc/pages/references/objects.md` berisi daftar object + anggota (constructor, property, method, event, field bila ada), signature, dan ringkasan dari XML docs.
5. Tambahkan halaman `doc/pages/help/index.md` berisi quick-start, pola penggunaan umum, dan troubleshooting dasar yang merujuk ke halaman object reference.
6. Tambahkan `doc/pages/_config.yml` untuk konfigurasi site (title, description, theme, permalink, navigation baseline).
7. Validasi tautan internal antardokumen serta konsistensi antara XML docs, `README.id.md`, dan API aktual source code.

**Kriteria Selesai**
- Struktur dokumentasi siap dipublish dari `doc/pages`.
- Halaman `Help` dan `References` tersedia dan saling terhubung.
- Object references menampilkan anggota object (property, event, method, dll) sesuai XML documentation.
- Tidak ada tautan internal rusak pada dokumen yang dibuat.
## Plan 38 — 23 Feb 2026, 13:06
Tanggal plan: 23 Feb 2026, 13:06

**Ringkasan**
Menyusun dokumentasi `References` dan `Help` untuk library `Emcode.Pst.Libs` yang siap dipublish ke GitHub Pages.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- docs/index.md
- docs/references/index.md
- docs/help/index.md
- docs/_config.yml
- docs/assets/* (jika diperlukan untuk styling ringan)

**Rencana Prioritas**
1. Definisikan struktur dokumentasi GitHub Pages berbasis folder `docs/` dengan navigasi utama: Home, References, dan Help.
2. Susun konten `References` berisi API surface utama library (konsep, class inti, opsi, dan contoh penggunaan sync/async).
3. Susun konten `Help` berisi troubleshooting, FAQ, dan panduan langkah cepat untuk skenario umum.
4. Tambahkan konfigurasi GitHub Pages (`docs/_config.yml`) agar site dapat dirender konsisten (judul, deskripsi, theme, permalink).
5. Validasi konsistensi tautan internal antardokumen dan kesesuaian istilah dengan `README.id.md` serta `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

**Kriteria Selesai**
- Struktur dokumentasi siap dipublish melalui GitHub Pages dari folder `docs/`.
- Halaman `References` dan `Help` tersedia dan saling terhubung.
- Konten relevan dengan fitur library saat ini dan mudah dipakai pengguna.
- Tidak ada tautan internal yang rusak pada dokumen yang ditambahkan.
# Refactor Plan - PST Projects

## Plan 37 — 23 Feb 2026, 12:28
Tanggal plan: 23 Feb 2026, 12:28

**Ringkasan**
Mengganti konfigurasi NuGet package readme dari `README.md` menjadi `README.id.md` pada project library.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj

**Rencana Prioritas**
1. Ubah properti `PackageReadmeFile` dari `README.md` ke `README.id.md`.
2. Sesuaikan item `None Include` yang di-pack agar menyertakan `README.id.md` ke root package.
3. Hapus referensi pack `README.md` jika sudah tidak dipakai sebagai package readme.
4. Validasi dengan `dotnet pack` untuk memastikan metadata/readme package valid.

**Kriteria Selesai**
- NuGet package readme mengarah ke `README.id.md`.
- File readme yang direferensikan benar-benar ikut di-pack.
- `dotnet pack` berhasil tanpa error metadata readme.
## Plan 36 — 23 Feb 2026, 12:27
Tanggal plan: 23 Feb 2026, 12:27

**Ringkasan**
Membuat icon package NuGet yang selaras dengan prinsip project `Emcode.Pst` (fokus PST/email, struktur folder, dan nuansa engineering library).

**Sumber**
- Permintaan user — 23 Feb 2026
- Konteks produk — `README.id.md`

**Lingkup**
- icon.png
- (opsional turunan) icon.svg untuk sumber desain

**Rencana Prioritas**
1. Definisikan konsep visual icon berdasarkan domain PST: envelope + folder/tree motif dengan gaya minimal, tajam, dan mudah dikenali di ukuran kecil.
2. Generate aset icon resolusi NuGet-friendly (minimal 128x128, target 256x256) dengan kontras warna yang jelas.
3. Validasi keterbacaan icon pada ukuran kecil (mis. 32x32) agar tetap terbaca sebagai identitas package.
4. Simpan hasil final ke `icon.png` (dan `icon.svg` bila dipakai sebagai source).

**Kriteria Selesai**
- Icon final mencerminkan konteks PST/email secara konsisten.
- File `icon.png` valid untuk metadata `PackageIcon` NuGet.
- Visual tetap jelas pada skala kecil.
## Plan 35 — 23 Feb 2026, 12:25
Tanggal plan: 23 Feb 2026, 12:25

**Ringkasan**
Menambahkan properti metadata NuGet opsional pada `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj` agar package lebih lengkap saat dipublikasikan.

**Sumber**
- Permintaan user — 23 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj

**Rencana Prioritas**
1. Tambahkan `PackageProjectUrl` dan `RepositoryType` untuk melengkapi metadata repository/package.
2. Tambahkan `PackageReleaseNotes` dengan catatan rilis awal.
3. Tambahkan `PackageIcon` dan item file icon agar ikut ter-pack di root package.
4. Validasi struktur `.csproj` tetap valid dan konsisten dengan properti yang sudah ada.

**Kriteria Selesai**
- Properti metadata opsional NuGet terisi di `.csproj`.
- File icon terkonfigurasi untuk dipack (jika file disediakan).
- `dotnet pack` dapat menghasilkan `.nupkg` tanpa error metadata.
## Plan 34 — 16 Feb 2026, 11:17
Tanggal plan: 16 Feb 2026, 11:17

**Ringkasan**
Mengubah section `Open Source Readiness Check` di `doc/AuditReports/AuditReport_0001_20260216_S063.md` dari checklist bullet menjadi tabel markdown agar lebih ringkas dan mudah dibaca.

**Sumber**
- Permintaan user — 16 Feb 2026

**Lingkup**
- doc/AuditReports/AuditReport_0001_20260216_S063.md

**Rencana Prioritas**
1. Identifikasi seluruh item readiness check yang saat ini berbentuk checklist.
2. Konversi item tersebut ke tabel markdown dengan kolom status dan item.
3. Pastikan status existing (`checked/unchecked`) tetap sama tanpa mengubah substansi audit.
4. Validasi format markdown tetap rapi dan konsisten dengan section lain.

**Kriteria Selesai**
- Section `Open Source Readiness Check` menggunakan format tabel markdown.
- Semua item readiness tetap lengkap dan statusnya tidak berubah.
- Tidak ada perubahan skor atau temuan audit lain.

## Plan 33 — 16 Feb 2026, 11:09
Tanggal plan: 16 Feb 2026, 11:09

**Ringkasan**
Merapikan format markdown di `doc/AuditReports/AuditReport_0001_20260216_S063.md` lalu menjadikannya basis struktur baku di `doc/AuditReports/AuditReportStructure.md`.

**Sumber**
- Permintaan user — 16 Feb 2026
- Existing audit report — `doc/AuditReports/AuditReport_0001_20260216_S063.md`

**Lingkup**
- doc/AuditReports/AuditReport_0001_20260216_S063.md
- doc/AuditReports/AuditReportStructure.md

**Rencana Prioritas**
1. Bersihkan noise/layout rusak di `AuditReport_0001_20260216_S063.md` (spacing, heading, separator, bullet/numbering, tabel score).
2. Standarkan struktur section menjadi markdown yang konsisten dan mudah dijadikan template.
3. Turunkan struktur final ke `AuditReportStructure.md` sebagai template reusable (placeholder terarah per section/finding).
4. Validasi kesesuaian antara report aktual dan template baru agar field wajib audit tetap lengkap.

**Kriteria Selesai**
- `AuditReport_0001_20260216_S063.md` rapi, konsisten, dan valid markdown.
- `AuditReportStructure.md` menjadi template markdown yang jelas untuk report audit berikutnya.
- Struktur inti audit (7 section + field finding wajib) tetap terjaga.
## Plan 32 — 16 Feb 2026, 11:06
Tanggal plan: 16 Feb 2026, 11:06

**Ringkasan**
Merapikan format file audit `doc/AuditReports/AuditReport_0001_20260216_S063.txt` agar konsisten dengan template `doc/AuditReports/AuditReportStructure.md` dan mudah dibaca.

**Sumber**
- Permintaan user — 16 Feb 2026
- Template audit — `doc/AuditReports/AuditReportStructure.md`

**Lingkup**
- doc/AuditReports/AuditReport_0001_20260216_S063.txt

**Rencana Prioritas**
1. Audit struktur laporan saat ini dan identifikasi bagian yang melenceng dari template (header, separator, spacing, label field).
2. Normalisasi layout per section agar konsisten: Executive Summary, Score Breakdown, Detailed Findings, Security Risk Analysis, Open Source Readiness, Technical Debt, Final Recommendation.
3. Rapikan format entri finding agar setiap field tampil stabil (`Severity`, `File`, `Line`, `Issue`, `Technical Explanation`, `Impact`, `Risk If Ignored`, `Recommendation`, `Suggested Refactor Code`).
4. Validasi ulang keterbacaan plaintext tanpa mengubah substansi hasil audit dan skor total (`63/100`).

**Kriteria Selesai**
- Struktur file audit mengikuti template yang ditentukan.
- Layout rapi dan konsisten antar section/finding.
- Tidak ada perubahan isi substansi audit (temuan, severity, skor, rekomendasi inti).
## Plan 31 — 16 Feb 2026, 09:41
Tanggal plan: 16 Feb 2026, 09:41

**Ringkasan**
Merapikan `CodexReport.md` agar index laporan kembali valid dengan menghapus blok duplikasi laporan lama (`45`, `44`, `43`, `42`, `41`, `40`) yang tertinggal di bagian bawah file.

**Sumber**
- Permintaan user — 16 Feb 2026
- Hasil audit struktur heading `## Laporan` pada `CodexReport.md`

**Lingkup**
- CodexReport.md

**Rencana Prioritas**
1. Identifikasi blok laporan duplikat di akhir `CodexReport.md` yang berada setelah `Laporan 1`.
2. Hapus blok duplikat tersebut beserta header ganda yang ikut tertinggal.
3. Pertahankan blok laporan yang sudah berada di posisi urut benar (descending) pada bagian utama dokumen.
4. Validasi ulang urutan index dengan pencarian heading `## Laporan` agar tidak ada nomor yang terduplikasi di luar struktur utama.

**Kriteria Selesai**
- `CodexReport.md` hanya memiliki satu rangkaian laporan utama terurut descending.
- Entri `Laporan 45, 44, 43, 42, 41, 40` muncul sekali pada posisi yang benar.
- Tidak ada blok laporan tertinggal setelah `Laporan 1`.

## Plan 30 — 16 Feb 2026, 09:25
Tanggal plan: 16 Feb 2026, 09:25

**Ringkasan**
Sinkronisasi `README.id.md` agar merefleksikan fitur aktual codebase (status supported/partial/not supported) dan menghapus informasi yang sudah tidak akurat.

**Sumber**
- Permintaan user — 16 Feb 2026
- Hasil audit codebase — 16 Feb 2026

**Lingkup**
- README.id.md
- README.md (jika perlu menyesuaikan tautan/ringkasan)

**Rencana Prioritas**
1. Audit poin fitur pada README terhadap API/fungsi aktual (`PstFile`, `IPstWriter`, `PstNdbReader`, `PstNdbWriter`, `NdbWriter`, `NdbCrypt`).
2. Perbarui bagian **Kemampuan Saat Ini** dengan fitur yang benar-benar tersedia:
   - Open/read sync-async
   - write draft (in-memory + persist)
   - mapping MAPI tambahan yang sudah diimplementasi
   - attachment read sync-async
3. Perbarui bagian **Batasan Saat Ini** agar akurat:
   - import `.eml` berbasis path (belum stream API)
   - update/delete pada `PstNdbWriter` belum didukung
   - crypt method `Cyclic`/`EdpEncrypted` belum didukung
   - batas data tree write di atas `XXBLOCK`
   - status attachment message object (`PidTagAttachDataObject`) belum didukung
4. Tambahkan section ringkas **Matriks Fitur** (Supported / Partial / Not Yet) untuk memudahkan tracking.
5. Periksa contoh kode agar konsisten dengan API aktual dan tambahkan catatan CancellationToken pada async.

**Kriteria Selesai**
- Isi `README.id.md` selaras dengan implementasi code saat ini.
- Tidak ada klaim fitur yang bertentangan dengan kode.
- Batasan protokol utama yang belum implementasi tercantum jelas.

## Plan 29 — 16 Feb 2026, 09:23
Tanggal plan: 16 Feb 2026, 09:23

**Ringkasan**
Implementasi sisa protocol PST berdasarkan referensi `PST-241112`, mencakup NDB allocation internals, operasi NDB/LTP lanjutan, crypt method tambahan, Messaging layer advanced structures, dan hardening recovery.

**Sumber**
- Permintaan user — 16 Feb 2026
- Referensi — `doc/PST-241112.htm`

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/*
- src/Emcode.Pst.Libs/Infrastructure/*
- src/Emcode.Pst.Libs/Domain/*
- src/Emcode.Pst.Libs/Application/*
- tests/Emcode.Pst.Tests/*
- README.id.md
- README.md

**Rencana Prioritas**
1. Fase 1 - NDB Allocation Map:
Implementasi struktur AMap/PMap/FMap/FPMap/DList minimal viable untuk path write (alokasi, update, dan persist state), termasuk abstraction allocator agar tidak hanya append-only.
2. Fase 2 - NDB Maintenance dan Recovery:
Tambahkan mekanisme validasi integritas page-map saat open/write, plus flow recovery/rebuild metadata allocation (minimal safe mode) ketika ditemukan inkonsistensi.
3. Fase 3 - NDB Ops Lanjutan:
Implementasi update/delete untuk message/folder dan operasi subnode (create/modify/delete) dengan konsistensi NBT/BBT + table hierarchy/contents.
4. Fase 4 - LTP Ops Lanjutan:
Lengkapi operasi HN/BTH lanjutan (insert/update/delete entry), termasuk pengelolaan free slot internal HN dan update row matrix/index TC.
5. Fase 5 - Crypt Method:
Tambahkan dukungan `PstCryptMethod.Cyclic` untuk read/write block path; untuk `EdpEncrypted` tetapkan strategi eksplisit (detect + graceful failure atau dukungan terbatas jika feasible).
6. Fase 6 - Data Tree Depth:
Perluas writer data tree agar mendukung kedalaman lebih dari XXBLOCK (multi-level tree builder) dengan pembacaan tetap kompatibel.
7. Fase 7 - Messaging Layer Advanced:
Implementasi NameID/GUID stream/property lookup map dasar untuk named properties, agar mapping properti non-standar bisa ditulis/dibaca konsisten.
8. Fase 8 - Attachment Object:
Tambahkan dukungan attachment bertipe message object (`PidTagAttachDataObject`) untuk read dulu, lalu write jika struktur subnode sudah stabil.
9. Fase 9 - EML Import Stream API:
Tambahkan API import `.eml` berbasis `Stream` (sync/async + CancellationToken) tanpa menghapus API berbasis path.
10. Fase 10 - Conformance & Compatibility Tests:
Tambah test unit/integration per fase (golden PST sample + roundtrip), termasuk scenario corruption/recovery dan format ANSI/Unicode.
11. Fase 11 - Performance & Reliability:
Optimasi I/O path, kurangi alokasi memori besar, dan tambahkan guard untuk operasi berisiko (timeout/cancellation/error context).
12. Fase 12 - Dokumentasi:
Perbarui README (ID/EN) dengan matriks fitur protocol (supported/partial/not supported), batasan, dan contoh penggunaan untuk fitur baru.

**Kriteria Selesai**
- Gap utama terhadap referensi `PST-241112` pada area NDB/LTP/Messaging yang teridentifikasi sudah tertutup atau memiliki fallback resmi yang terdokumentasi.
- Update/delete + operasi subnode stabil dan lolos test integrasi.
- Dukungan crypt `Cyclic` tersedia untuk read/write.
- Named properties dasar (NameID/GUID stream/property lookup map) dapat dibaca/ditulis.
- API import `.eml` mendukung path dan stream (sync/async).
- Dokumentasi fitur protocol terbarui dan dapat ditrace ke test.

## Plan 28 — 16 Feb 2026, 09:13
Tanggal plan: 16 Feb 2026, 09:13

**Ringkasan**
Menjadikan `README.id.md` sebagai bahasa default dari `README.md` dengan menambahkan referensi/penjelasan yang jelas di `README.md`.

**Sumber**
- Permintaan user — 16 Feb 2026

**Lingkup**
- README.md
- README.id.md

**Rencana Prioritas**
1. Verifikasi kondisi akhir `README.md` dan tentukan format referensi default language ke `README.id.md`.
2. Perbarui `README.md` agar menampilkan informasi singkat dan mengarahkan pengguna ke `README.id.md` sebagai dokumentasi utama/default.
3. Pastikan tautan relatif valid dan mudah dipahami pada viewer GitHub.
4. Review cepat konsistensi isi antara `README.md` dan `README.id.md`.

**Kriteria Selesai**
- `README.md` secara eksplisit menyatakan bahwa bahasa default adalah Indonesia dan merujuk ke `README.id.md`.
- Tautan dokumentasi berfungsi dengan benar.

## Plan 27 — 15 Feb 2026, 11:43
Tanggal plan: 15 Feb 2026, 11:43

**Ringkasan**
Menambahkan mapping write MAPI agar pesan/folder lebih “Outlook lengkap”, meliputi message class, flags, timestamps, threading, transport headers, dan properti recipient/attachment tambahan.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContextWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/TableRowWriter.cs
- src/Emcode.Pst.Libs/Domain/*
- tests/Emcode.Pst.Tests/*
- README.md

**Rencana Prioritas**
1. Definisikan daftar properti MAPI write minimal untuk kompatibilitas Outlook (message class, flags, timestamps, conversation, transport headers, has attachments, importance/priority/sensitivity).
2. Tambahkan mapping properti tersebut ke writer PC message (PropertyContextWriter) dan table recipient/attachment bila perlu.
3. Tambahkan property tambahan pada model domain bila diperlukan (mis. header raw, conversation index).
4. Perbarui test integrasi: buat message dengan properti tambahan dan verifikasi read-back.
5. Update README untuk dokumentasi mapping MAPI write yang sudah didukung.

**Kriteria Selesai**
- Message yang ditulis memiliki message class, flags, timestamp, dan threading minimal sehingga terbaca normal di Outlook.
- Test integrasi lulus dan dokumentasi diperbarui.
## Plan 26 — 15 Feb 2026, 11:39
Tanggal plan: 15 Feb 2026, 11:39

**Ringkasan**
Refactor writer agar mendukung PC/TC multi-block dengan data tree XBLOCK/XXBLOCK sehingga body/attachment besar bisa ditulis dan dibaca konsisten.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriterCore.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBlockWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContextWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/TableRowWriter.cs
- tests/Emcode.Pst.Tests/*
- README.md

**Rencana Prioritas**
1. Tambahkan builder data tree (XBLOCK/XXBLOCK) pada writer NDB untuk data di atas ukuran block tunggal, termasuk API sync/async.
2. Perluas LTP writer (Heap/PC/TC) agar bisa menghasilkan buffer multi-block dan mengembalikan struktur data tree yang sesuai.
3. Integrasikan ke `PstNdbWriter` untuk body/attachment besar (PC/TC dan subnode attachment).
4. Tambahkan test integrasi: write body besar + attachment besar ke PST copy dan baca ulang memastikan konten utuh.
5. Update README dengan batasan baru dan contoh ukuran besar.

**Kriteria Selesai**
- Body/attachment besar (> ukuran block) berhasil ditulis dan terbaca ulang.
- XBLOCK/XXBLOCK terbentuk valid dan lolos test integrasi.
- Dokumentasi README diperbarui.
## Plan 25 — 15 Feb 2026, 11:01
Tanggal plan: 15 Feb 2026, 11:01

**Ringkasan**
Implementasi end-to-end create message/folder yang benar-benar menulis node + Property Context + Table Row ke PST hingga save berhasil.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/* (writer PC/TC)
- src/Emcode.Pst.Libs/Domain/* (mapping MAPI draft)
- 	ests/Emcode.Pst.Tests/*
- README.md

**Rencana Prioritas**
1. Implementasi create folder/message yang menulis data node (HN/PC) dan row table (Hierarchy/Contents) ke PST.
2. Implementasi subnode untuk recipient/attachment dan mapping MAPI minimal.
3. Update NBT/BBT dan header roots pada setiap commit write.
4. Tambahkan test integrasi: create folder + message pada PST copy dan verifikasi baca ulang.
5. Update README untuk status save-to-disk.

**Kriteria Selesai**
- Folder/message baru muncul saat PST dibaca ulang.
- Data message (subject/body/recipients/attachments) terbaca sesuai mapping.
- Test integrasi lulus.
## Plan 24 — 15 Feb 2026, 10:40
Tanggal plan: 15 Feb 2026, 10:40

**Ringkasan**
Implementasi update BBT/NBT di file agar persist write benar-benar bekerja.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBtreeWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBTreeReader.cs (jika perlu helper serialisasi page)
- 	ests/Emcode.Pst.Tests/*
- README.md

**Rencana Prioritas**
1. Implementasi serialisasi BBT/NBT page (ANSI/Unicode) termasuk header page, checksum/trailer sesuai format.
2. Implementasi writer untuk membuat BBT/NBT tree baru (minimal single-level) dan menulis ke block baru.
3. Update header NDB untuk menunjuk root BBT/NBT terbaru dan memperbarui ukuran file.
4. Integrasi PstNdbWriter agar create message/folder menulis block data + update BBT/NBT di disk.
5. Tambahkan test integrasi: tulis message ke salinan PST dan baca ulang memastikan entry muncul.
6. Update README untuk menandai write-to-disk sudah aktif (batasan jelas jika masih ada).

**Kriteria Selesai**
- BBT/NBT di file terupdate dan dapat dibaca ulang oleh reader.
- Message/folder yang ditulis muncul saat dibaca ulang.
- Test integrasi lulus.
## Plan 23 — 15 Feb 2026, 10:25
Tanggal plan: 15 Feb 2026, 10:25

**Ringkasan**
Tahap 3 untuk Plan 20: integrasi writer NDB/LTP agar persist ke disk, update B-Tree (BBT/NBT), dan menyimpan data node.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBtreeWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBlockWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/PstNdbWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/* (integrasi output PC/TC)
- src/Emcode.Pst.Libs/Application/PstFile.cs
- 	ests/Emcode.Pst.Tests/*
- README.md

**Rencana Prioritas**
1. Implementasi writer BBT/NBT untuk insert/update entry sesuai format ANSI/Unicode.
2. Implementasi writer block untuk menulis data block dan update trailer/checksum.
3. Implementasi PstNdbWriter untuk create folder/message, update hierarchy/contents table, dan persisten data ke disk.
4. Integrasi writer dengan facade PstFile dan opsi ReadOnly.
5. Tambahkan unit/integration test untuk persist message draft dan baca ulang.
6. Jalankan test dan update README.md dengan dukungan write-to-disk.

**Kriteria Selesai**
- Message draft tersimpan di PST di disk dan bisa dibaca ulang.
- BBT/NBT konsisten setelah operasi write.
- Test lulus dan README diperbarui.
## Plan 22 — 15 Feb 2026, 09:48
Tanggal plan: 15 Feb 2026, 09:48

**Ringkasan**
Tahap 2 untuk Plan 20: menyiapkan writer Property Context dan Table Row untuk message, recipient, dan attachment.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContextWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/TableRowWriter.cs
- src/Emcode.Pst.Libs/Infrastructure/Ltp/LtpWriterOptions.cs
- 	ests/Emcode.Pst.Tests/LtpWriterTests.cs

**Rencana Prioritas**
1. Definisikan opsi writer LTP (ukuran heap, alignment, dan batas ukuran entry) agar konsisten dengan reader.
2. Implementasi writer Property Context (PC) untuk menulis pasangan property id/value (string/int/bool/datetime/binary) ke Heap-on-Node.
3. Implementasi writer Table Row minimal untuk Contents/Recipient/Attachment table dengan dukungan kolom dasar.
4. Sediakan API sync/async dengan CancellationToken untuk operasi tulis PC dan row.
5. Tambahkan unit test untuk memastikan PC dan Table Row dapat ditulis dan dibaca ulang oleh reader.

**Kriteria Selesai**
- Property Context writer menghasilkan buffer PC yang valid dan terbaca oleh reader.
- Table Row writer menghasilkan row minimal yang terbaca ulang.
- Unit test tahap 2 lulus.
## Plan 21 — 15 Feb 2026, 09:40
Tanggal plan: 15 Feb 2026, 09:40

**Ringkasan**
Tahap 1 untuk Plan 20: menyiapkan writer core dan alokasi block/BID sebagai fondasi write ke PST.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbWriterCore.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbBlockAllocation.cs`
- `tests/Emcode.Pst.Tests/NdbWriterCoreTests.cs`

**Rencana Prioritas**
1. Definisikan model alokasi block/BID untuk writer core (metadata BID, IB, ukuran, dan tipe block).
2. Implementasi allocator BID (internal/eksternal) dengan aturan bit flag yang konsisten.
3. Implementasi allocator block yang menjaga alignment terhadap ukuran block PST (ANSI/Unicode) dan validasi ukuran data.
4. Sediakan API sync/async dengan CancellationToken sesuai kebijakan async project.
5. Tambahkan unit test untuk memastikan BID dan IB teralokasi konsisten serta alignment benar.

**Kriteria Selesai**
- Writer core menghasilkan alokasi BID/IB yang konsisten dan teruji.
- Unit test tahap 1 lulus.
## Plan 20 — 15 Feb 2026, 09:24
Tanggal plan: 15 Feb 2026, 09:24

**Ringkasan**
Menambahkan kemampuan write ke struktur PST di disk (NDB/LTP) untuk membuat pesan baru, termasuk recipients dan attachments, dengan dukungan draft/save ke folder Drafts.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Application`
- `src/Emcode.Pst.Libs/Domain`
- `src/Emcode.Pst.Libs/Infrastructure`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Rancang layer writer NDB/LTP:
   - Struktur API untuk membuat node baru (BTree, NBT/BBT), heap on node, property context, dan table row.
   - Identifikasi minimal set MAPI property untuk message draft (subject, body, html, sender, recipients, flags, message class).
2. Implementasi storage writer:
   - `NdbWriter` untuk alokasi block/BID dan update BTree.
   - `LtpWriter` untuk membuat property context dan table row (contents/recipient/attachment tables).
3. Implementasi `PstNdbWriter` yang mengubah `PstInMemoryWriter` menjadi persist ke file:
   - Create folder/message di PST, update hierarchy/contents table.
   - Simpan recipients dan attachments ke subnode yang sesuai.
4. Tambahkan mapping draft -> MAPI untuk draft:
   - Set message class ke `IPM.Note` dan flags draft/unsent sesuai kebutuhan.
   - Pastikan folder Drafts terdeteksi/diupdate.
5. Tambahkan API async lengkap dengan `CancellationToken` untuk seluruh path write.
6. Tambahkan test integrasi:
   - Buat pesan draft ke folder Drafts pada sample PST copy.
   - Baca kembali dan verifikasi property utama + attachment.
7. Update `README.md`:
   - Dokumentasikan write-to-disk, batasan, dan contoh penggunaan.

**Kriteria Selesai**
- Pesan draft dapat disimpan ke PST di disk (termasuk recipients + attachments).
- Reader bisa membaca kembali pesan yang baru dibuat.
- Test integrasi lulus untuk sync/async.



## Plan 19 — 15 Feb 2026, 09:12
Tanggal plan: 15 Feb 2026, 09:12

**Ringkasan**
Merancang operasi write untuk membuat message baru di folder PST (import `.eml`) lengkap dengan pipeline parsing, mapping properti MAPI, dan penyimpanan node NDB/LTP.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Application`
- `src/Emcode.Pst.Libs/Domain`
- `src/Emcode.Pst.Libs/Infrastructure`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Definisikan kontrak aplikasi untuk operasi write/import message:
   - `IPstWriter` dengan method sync/async untuk `ImportEml` dan `CreateMessage`.
   - Setiap method async menerima `CancellationToken`.
   - Semua object/DTO yang diperlukan diberi XML documentation berbahasa Indonesia.
2. Rancang model domain untuk message write (mis. `PstMessageDraft`/`PstMessageContent`) berisi:
   - Header (From/To/Cc/Bcc/Subject/MessageId/Date), body text, html body, dan attachment metadata.
   - Dokumenkan mapping konteks MAPI (PidTag*).
3. Implementasi parser `.eml` (sync/async) untuk menghasilkan model domain write:
   - Normalisasi encoding, line endings, dan MIME multipart.
   - Pastikan attachment binary dan inline content-id terpetakan.
4. Implementasi builder LTP/NDB untuk membuat message:
   - Buat node baru, property context, dan subnode attachment/recipient.
   - Mapping property MAPI wajib konsisten dengan reader.
5. Integrasi ke facade `PstFile`:
   - `ImportEml` dan `ImportEmlAsync` pada folder target.
   - Validasi folder tujuan dan penanganan error yang terdefinisi.
6. Tambahkan test integrasi:
   - Import 1 `.eml` ke folder import dan verifikasi message baru terbaca kembali.
   - Uji sync/async + cancellation.
7. Update `README.md`:
   - Contoh penggunaan write/import `.eml`, batasan, dan best practice.

**Kriteria Selesai**
- Tersedia API write/import message sync/async dengan `CancellationToken`.
- `.eml` berhasil diimport dan terbaca kembali dari PST.
- Mapping property MAPI terdokumentasi dan tervalidasi oleh test.



## Plan 18 — 15 Feb 2026, 08:39
Tanggal plan: 15 Feb 2026, 08:39

**Ringkasan**
Menambahkan API untuk membaca data attachment sebagai Stream/byte[] (sync dan async dengan CancellationToken), serta dokumentasi penggunaan di `README.md`.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Domain/PstAttachment.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure` (jika perlu helper stream/binary)
- `tests/Emcode.Pst.Tests/*`
- `README.md`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Identifikasi dan dokumentasikan property MAPI untuk data attachment (mis. PidTagAttachDataBinary/PidTagAttachDataObject) dan cara akses ke subnode attachment di PST.
2. Tambahkan API di domain untuk membuka data attachment:
   - Sync: `OpenContentStream(...)` dan `ReadContentBytes()`.
   - Async: `OpenContentStreamAsync(CancellationToken)` dan `ReadContentBytesAsync(CancellationToken)`.
   Semua method wajib ber-XML doc (bahasa Indonesia) dan menjelaskan konteksnya.
3. Implementasi reader untuk mengambil data attachment melalui NDB/LTP:
   - Parsing subnode attachment dan mengambil property data.
   - Pastikan tidak memuat semua data ke memory jika user memilih stream.
4. Tambahkan opsi cancellation pada seluruh path async.
5. Tambahkan test untuk minimal satu attachment pada `sample1.pst`:
   - Memastikan stream/byte[] tidak kosong dan ukuran konsisten dengan metadata.
6. Update `README.md` dengan contoh penggunaan sync/async export attachment.

**Kriteria Selesai**
- Tersedia method sync/async untuk stream dan byte[] per attachment dengan CancellationToken.
- Data attachment bisa diambil dari PST dan tervalidasi dengan test.
- README berisi cara penggunaan.





## Plan 17 — 15 Feb 2026, 08:23
Tanggal plan: 15 Feb 2026, 08:23

**Ringkasan**
Memperluas Plan 16 dengan daftar properti MAPI spesifik yang diminta untuk diexpose di `PstMessage`.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Tambahkan properti baru pada `PstMessage` untuk:
`PidTagInternetMessageId`, `PidTagSenderEmailAddress`, `PidTagSenderSmtpAddress`, `PidTagSentRepresentingName`, `PidTagSentRepresentingEmailAddress`, `PidTagOriginalSenderName`, `PidTagOriginalSenderEmailAddress`, `PidTagDisplayTo`, `PidTagDisplayCc`, `PidTagDisplayBcc`, `PidTagMessageDeliveryTime`, `PidTagClientSubmitTime`, `PidTagMessageSubmissionId`, `PidTagLastModificationTime`, `PidTagMessageFlags`, `PidTagReadReceiptRequested`, `PidTagDeliveryReceiptRequested`, `PidTagHasAttachments`, `PidTagImportance`, `PidTagPriority`, `PidTagSensitivity`.
2. Siapkan model recipient (per-recipient) untuk `PidTagRecipientType`, `PidTagEmailAddress`, `PidTagSmtpAddress` (ditaruh di object baru, mis. `PstRecipient`), lengkap XML doc.
3. Siapkan model attachment (per-attachment) untuk `PidTagAttachNumber`, `PidTagAttachFilename`, `PidTagAttachLongFilename`, `PidTagAttachSize`, `PidTagAttachMimeTag`, `PidTagAttachContentId`, `PidTagAttachMethod` (ditaruh di object baru, mis. `PstAttachment`), lengkap XML doc.
4. Tambahkan helper di `PropertyContext` untuk tipe data yang diperlukan (string, int, bool, datetime, binary).
5. Map properti di `PstNdbReader` sync/async, termasuk parsing subnode untuk recipients/attachments.
6. Tambahkan unit/integration test yang memverifikasi minimal satu properti baru dan satu attachment/recipient pada `sample1.pst` (jika data tersedia).

**Kriteria Selesai**
- `PstMessage` mengekspos semua properti MAPI yang diminta.
- Reader sync/async memetakan properti message, recipient, dan attachment dengan aman.
- Test verifikasi minimal satu properti baru lulus.

## Plan 16 — 15 Feb 2026, 08:14
Tanggal plan: 15 Feb 2026, 08:14

**Ringkasan**
Menambah properti MAPI tambahan pada `PstMessage` dan mapping-nya dari Property Context agar domain lebih kaya informasi.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Tentukan subset properti MAPI yang paling berguna (mis. `SenderEmailAddress`, `ReceivedTime`, `DisplayTo`, `MessageClass`, `ConversationIndex`, `HasAttachments`, `Importance`) dan dokumentasikan konteksnya dalam XML doc.
2. Tambahkan properti baru pada `PstMessage` dengan komentar Indonesia yang menjelaskan konteks MAPI-nya (sebut PidTag).
3. Ekspose helper baru di `PropertyContext` (string/int/binary/datetime) untuk mempermudah pembacaan jenis data lain.
4. Isi properti tambahan tersebut di `CreateMessage`/`CreateMessageAsync` dari `PstNdbReader`, dengan fallback aman bila property tidak ada.
5. Tambahkan unit atau integration test untuk minimal satu properti (mis. `SenderEmailAddress` dan `HasAttachments`) pada `sample1.pst`, jika data tersedia.

**Kriteria Selesai**
- `PstMessage` memiliki properti tambahan sesuai subset yang dipilih dengan XML doc konteks MAPI.
- Property Context mendukung pengambilan tipe data yang diperlukan dan digunakan oleh reader sync/async.
- Test memverifikasi setidaknya satu properti baru terisi.

## Plan 15 — 15 Feb 2026, 08:07
Tanggal plan: 15 Feb 2026, 08:07

**Ringkasan**
Menambahkan informasi ukuran message pada `PstMessage` dan membaca nilainya dari Property Context.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Tambahkan properti ukuran (byte) pada `PstMessage` dengan XML documentation berbahasa Indonesia.
2. Identifikasi property MAPI untuk ukuran pesan (mis. PR_MESSAGE_SIZE) dan mapping-nya di Property Context.
3. Isi properti ukuran saat mapping PC message ke `PstMessage` (sync + async path).
4. Tambahkan unit/integration test untuk memastikan size terisi pada `sample1.pst`.

**Kriteria Selesai**
- `PstMessage` memiliki properti ukuran yang terisi dari Property Context.
- Test terkait size lulus.

## Plan 14 — 15 Feb 2026, 07:56
Tanggal plan: 15 Feb 2026, 07:56

**Ringkasan**
Mengisi `README.md` dengan ringkasan kemampuan library dan panduan penggunaan cepat.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `README.md`

**Rencana Prioritas**
1. Ringkas kemampuan baca PST yang sudah tersedia (header, folder, message, subject, sender, body, html, delivery time) dan opsi pembukaan (ANSI/Unicode, checksum).
2. Tambahkan contoh Quick Start sync/async menggunakan `PstFile.Open` dan `PstFile.OpenAsync`.
3. Cantumkan batasan saat ini (write/import eml belum tersedia) serta arah roadmap sesuai goal proyek.

**Kriteria Selesai**
- `README.md` berisi ringkasan kemampuan, quick start, dan batasan/roadmap yang akurat.

## Plan 13 — 15 Feb 2026, 07:23
Tanggal plan: 15 Feb 2026, 07:23

**Ringkasan**
Menambahkan API async untuk operasi baca PST (reader dan facade) dengan alur IO non-blocking.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Application/PstFile.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/IPstReader.cs`
- `src/Emcode.Pst.Libs/Application/Internal/NullPstReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/NdbHeaderReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBTreeReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBlockReader.cs`
- `tests/Emcode.Pst.Tests/*`

**Rencana Prioritas**
1. Tambahkan kontrak async pada `IPstReader` (mis. `ReadAsync`) dengan `CancellationToken` dan XML doc.
2. Tambahkan `OpenAsync` di `PstFile` yang menggunakan `ReadAsync`.
3. Implementasi async pada `PstMinimalReader` dan `PstNdbReader` (gunakan `FileStream` `useAsync: true`, `ReadAsync`).
4. Tambahkan method async pada reader level NDB (`NdbHeaderReader`, `PstBTreeReader`, `PstBlockReader`) untuk membaca stream secara non-blocking.
5. Update `NullPstReader` untuk mendukung async.
6. Tambahkan/ubah test async untuk memastikan `OpenAsync` mengembalikan hasil yang setara dengan `Open`.
7. Untuk method async, diberikan opsi untuk tidak menyertakan `CancellationToken`.
**Kriteria Selesai**
- API async tersedia dan terintegrasi dari `PstFile.OpenAsync` hingga reader NDB.
- Semua method async memiliki XML documentation dan menggunakan `CancellationToken`.
- Test async lulus.

## Plan 12 — 15 Feb 2026, 07:19
Tanggal plan: 15 Feb 2026, 07:19

**Ringkasan**
Menambahkan parsing properti message tambahan (Sender, Body, HtmlBody) dan urutan folder berdasarkan Hierarchy Table.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/TableContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Libs/Domain/PstFolder.cs`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Tambahkan mapping property MAPI untuk `SenderName`, `Body`, dan `HtmlBody` (termasuk fallback ANSI/Unicode), lalu isi pada `PstMessage` dari PC message.
2. Implement parsing Hierarchy Table (TC) untuk membaca urutan folder per parent berdasarkan row matrix, dengan fallback ke urutan NBT bila table tidak tersedia.
3. Integrasikan urutan folder dari Hierarchy Table ke `PstNdbReader` saat membangun `SubFolders`.
4. Tambahkan/ubah unit + integration test untuk memverifikasi minimal satu message memiliki Sender/Body/HtmlBody dan urutan folder mengikuti Hierarchy Table di `sample1.pst`.

**Kriteria Selesai**
- Sender, Body, dan HtmlBody terbaca dari PC message pada `sample1.pst`.
- Urutan folder mengikuti Hierarchy Table (TC) bila tersedia.
- Semua test lulus.

## Plan 11 — 15 Feb 2026, 06:38
Tanggal plan: 15 Feb 2026, 06:38

**Ringkasan**
Aktifkan parsing PC message (subject/delivery time) dengan penanganan penuh XBLOCK/XXBLOCK, serta parsing Contents Table agar urutan message sesuai folder.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/PstBlockReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/HeapOnNode.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/PropertyContext.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/*` (Table Context / Contents Table)
- `src/Emcode.Pst.Libs/Infrastructure/PstNdbReader.cs`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)

**Rencana Prioritas**
1. Lengkapi reader data tree: dukung penuh XBLOCK/XXBLOCK termasuk validasi `btype`, `cLevel`, `cEnt`, `lcbTotal` dan penggabungan data blok sesuai urutan BID.
2. Perbaiki Heap‑on‑Node untuk multi‑block (HNHDR/HNPAGEHDR/HNBITMAPHDR) agar item heap dari data tree dapat dibaca konsisten.
3. Aktifkan kembali parsing PC message (Subject & Delivery Time) dengan fallback aman bila properti tidak ada.
4. Implement Table Context minimal untuk Contents Table (TC) dan mapping urutan message per folder berdasarkan table rows.
5. Update `PstNdbReader` agar mengisi `Messages` sesuai urutan Contents Table.
6. Tambahkan/ubah unit + integration test untuk validasi: subject/delivery time terbaca dan urutan message konsisten dengan Contents Table.

**Kriteria Selesai**
- Subject dan Delivery Time terbaca dari PC message pada `sample1.pst`.
- Urutan message di folder mengikuti Contents Table.
- Semua test lulus.

## Plan 10 — 14 Feb 2026, 09:30
Tanggal plan: 14 Feb 2026, 09:30

**Ringkasan**
Implementasi parsing NDB (Node Database) agar bisa membaca folder/message nyata dari `doc/Samples/sample1.pst`.

**Sumber**
- Permintaan user — 14 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Infrastructure/Ndb/*`
- `src/Emcode.Pst.Libs/Infrastructure/Ltp/*`
- `src/Emcode.Pst.Libs/Application/Abstractions/*`
- `src/Emcode.Pst.Libs/Domain/PstFolder.cs`
- `src/Emcode.Pst.Libs/Domain/PstMessage.cs`
- `src/Emcode.Pst.Runner/Program.cs`
- `tests/Emcode.Pst.Tests/*`
- `doc/PST-241112.docx` (referensi)
- `doc/Samples/sample1.pst` (data uji)

**Rencana Prioritas**
1. Tambahkan model NDB dasar (header, BID, BREF, NID) dan parser header NDB untuk membaca pointer ke BBT/NBT. Semua object baru dilengkapi XML documentation dan unit test parser header.
2. Implementasikan reader block dengan lookup BBT (Block B-Tree), termasuk verifikasi ukuran/block trailer dan caching sederhana; tambah unit test untuk lookup block dan validasi BID.
3. Implementasikan lookup NBT (Node B-Tree) untuk menemukan node folder/message, termasuk pemetaan NID ke data block; tambah unit test untuk lookup node.
4. Implementasikan LTP minimal (Heap-on-Node + Table Context) untuk membaca tabel folder dan message, lalu mapping ke `PstFolder`/`PstMessage` dengan properti minimum (nama folder, subject, delivery time).
5. Integrasikan reader NDB ke flow `PstFile.Open` (opsi default menggantikan reader minimal) dan pastikan folder/message bisa diekspose.
6. Tambahkan integration test yang membaca `doc/Samples/sample1.pst` dan memastikan folder/message terisi, serta update runner untuk menampilkan ringkasan hasil parsing.

**Kriteria Selesai**
- Parsing NDB menghasilkan folder/message nyata dari `sample1.pst`.
- Semua object baru memiliki XML documentation dan test yang dapat dijalankan.
- Runner menampilkan jumlah folder/message tanpa exception.

## Plan 9 — 15 Feb 2026, 05:50
Tanggal plan: 15 Feb 2026, 05:50

**Ringkasan**
Melanjutkan Phase 1: menyiapkan enumerasi folder/message minimal dan menampilkan header di runner.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Domain/PstFolder.cs`
- `src/Emcode.Pst.Runner/Program.cs`

**Rencana Prioritas**
1. Isi `RootFolder` dan `Folders` pada reader minimal.
2. Ubah enumerator pesan agar mengembalikan koleksi (awal: kosong).
3. Tampilkan metadata header di console runner.

**Kriteria Selesai**
- Runner menampilkan header PST dan enumerasi folder/messages tidak melempar exception.

## Plan 8 — 15 Feb 2026, 05:47
Tanggal plan: 15 Feb 2026, 05:47

**Ringkasan**
Menambahkan implementasi reader PST minimal (parse header dan deteksi format).

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Infrastructure/PstMinimalReader.cs`
- `src/Emcode.Pst.Libs/Domain/PstHeaderInfo.cs`
- `src/Emcode.Pst.Libs/Domain/PstFormat.cs`
- `src/Emcode.Pst.Libs/Application/Abstractions/PstReadResult.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`

**Rencana Prioritas**
1. Buat reader minimal yang memvalidasi signature PST dan baca versi.
2. Tambahkan model metadata header dan format PST.
3. Integrasikan hasil header ke `PstFile`.

**Kriteria Selesai**
- `PstFile.Open` mampu membaca header PST dan mengisi metadata dasar.

## Plan 7 — 15 Feb 2026, 05:30
Tanggal plan: 15 Feb 2026, 05:30

**Ringkasan**
Menambahkan XML documentation untuk semua code object di seluruh file .cs pada solution.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- Semua file .cs di src/Emcode.Pst.Libs/*

**Rencana Prioritas**
1. Tambah XML doc untuk class, interface, method, property, dan field yang relevan.
2. Pastikan dokumentasi menjelaskan konteks masing-masing object.
3. Menjaga konsistensi bahasa dan format.

**Kriteria Selesai**
- Semua code object di file .cs memiliki XML documentation.

## Plan 6 — 15 Feb 2026, 05:17
Tanggal plan: 15 Feb 2026, 05:17

**Ringkasan**
Memperbarui format `RefactorPlan.md` agar mengikuti struktur template sample.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `RefactorPlan.md`

**Rencana Prioritas**
1. Menyesuaikan header dan format entri sesuai template.
2. Menormalisasi tanggal dan nomor plan.
3. Menyusun ringkasan dan lingkup per entri.

**Kriteria Selesai**
- Format `RefactorPlan.md` konsisten dengan sample.

## Plan 5 — 15 Feb 2026, 05:09
Tanggal plan: 15 Feb 2026, 05:09

**Ringkasan**
Menambahkan interface/abstraction untuk storage/parser agar testable.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Application/Abstractions/*`
- `src/Emcode.Pst.Libs/Application/Internal/NullPstReader.cs`
- `src/Emcode.Pst.Libs/Application/PstFile.cs`

**Rencana Prioritas**
1. Buat `IPstReader`, `IPstWriter`, dan `PstReadResult`.
2. Tambahkan `NullPstReader` sebagai stub.
3. Update `PstFile.Open` agar menerima reader/writer.

**Kriteria Selesai**
- Abstraction tersedia dan `PstFile.Open` terintegrasi.

## Plan 4 — 15 Feb 2026, 05:05
Tanggal plan: 15 Feb 2026, 05:05

**Ringkasan**
Menerapkan Clean Architecture via folder/namespace di `Emcode.Pst.Libs`.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/Domain/*`
- `src/Emcode.Pst.Libs/Application/*`
- `src/Emcode.Pst.Libs/Shared/*`
- `src/Emcode.Pst.Runner/Program.cs`

**Rencana Prioritas**
1. Buat folder `Domain`, `Application`, `Shared`.
2. Pindahkan entity ke `Domain` dan facade/options ke `Application`.
3. Tambahkan helper `Guard` dan update namespace.

**Kriteria Selesai**
- Struktur folder dan namespace baru terpasang.

## Plan 3 — 15 Feb 2026, 05:03
Tanggal plan: 15 Feb 2026, 05:03

**Ringkasan**
Menambahkan solution `.slnx` dan memasukkan semua project.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.slnx`
- `src/Emcode.Pst.sln`

**Rencana Prioritas**
1. Buat solution dan migrasi ke `.slnx`.
2. Tambahkan `Emcode.Pst.Libs` dan `Emcode.Pst.Runner`.

**Kriteria Selesai**
- Solution berisi seluruh project.

## Plan 2 — 15 Feb 2026, 05:00
Tanggal plan: 15 Feb 2026, 05:00

**Ringkasan**
Membuat kerangka project .NET dan draft API surface.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- `src/Emcode.Pst.Libs/*`
- `src/Emcode.Pst.Runner/*`

**Rencana Prioritas**
1. Buat library dan console runner.
2. Tambah draft API surface dasar.

**Kriteria Selesai**
- Struktur project dan API draft terbentuk.

## Plan 1 — 15 Feb 2026, 04:57
Tanggal plan: 15 Feb 2026, 04:57

**Ringkasan**
Menyusun rencana dua tahap implementasi read dan write PST.

**Sumber**
- Permintaan user — 15 Feb 2026

**Lingkup**
- Tidak ada perubahan file; rencana implementasi.

**Rencana Prioritas**
1. Tahap 1: read MVP.
2. Tahap 2: write MVP.

**Kriteria Selesai**
- Rencana dua tahap terdokumentasi.


















