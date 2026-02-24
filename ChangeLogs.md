# Change Logs

Semua perubahan penting pada project ini didokumentasikan pada file ini.

Format mengikuti prinsip [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) dan versioning menggunakan [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Belum ada.

### Changed
- Belum ada.

### Deprecated
- Belum ada.

### Removed
- Belum ada.

### Fixed
- Belum ada.

### Security
- Belum ada.

## [1.0.8] - 2026-02-24

### Added
- Source dokumentasi berbasis Vue + Vite di `doc/pages` (`src/`, `router`, `views`, `styles`).
- Konfigurasi build docs frontend (`package.json`, `vite.config.js`) untuk output statis GitHub Pages.

### Changed
- Entry docs `doc/pages/index.html` diubah ke SPA entry point Vue.
- Workflow GitHub Pages diupdate agar build docs terlebih dahulu (`npm ci`, `npm run build`) lalu publish `doc/pages/dist`.
- `.gitignore` diperbarui untuk mengabaikan artefak frontend lokal (`node_modules`, `doc/pages/dist`).
- Kenaikan versi build package `Emcode.Pst.Libs` dari `1.0.7` ke `1.0.8`.

## [1.0.7] - 2026-02-24

### Added
- Sidebar dokumentasi global pada seluruh halaman `doc/pages` (Home, Help, TOC, API References).
- Integrasi `Prism.js` pada semua halaman yang memiliki block code C# (`language-csharp`).

### Changed
- Format seluruh halaman object API diseragamkan ke pola referensi bergaya Microsoft Docs (`Constructors`, `Properties`, `Methods`, `Events`, `Fields` untuk enum).
- Halaman Help diperbarui dengan layout sidebar navigasi kiri yang konsisten.
- Sinkronisasi tema Prism dengan mode dokumentasi (`light`/`dark`).
- Pembaruan aturan `AGENTS.md` agar eksekusi script PowerShell wajib menggunakan PowerShell 7+ (`pwsh`).
- Kenaikan versi build package `Emcode.Pst.Libs` dari `1.0.6` ke `1.0.7`.

## [1.0.6] - 2026-02-23

### Changed
- Kenaikan versi build package `Emcode.Pst.Libs` dari `1.0.5` ke `1.0.6` untuk publikasi NuGet.
- Pembaruan `PackageReleaseNotes` pada `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

## [1.0.5] - 2026-02-23

### Changed
- Kenaikan versi build package `Emcode.Pst.Libs` dari `1.0.4` ke `1.0.5` untuk publikasi NuGet.
- Pembaruan `PackageReleaseNotes` pada `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

## [1.0.4] - 2026-02-23

### Added
- Toggle mode `Dark`/`Light` pada seluruh halaman dokumentasi `doc/pages`.
- Script terpusat `doc/pages/assets/js/theme.js` untuk persistensi tema berbasis `localStorage`.

### Changed
- Penyesuaian tema warna pada `doc/pages/assets/css/site.css` untuk dukungan light/dark yang konsisten.
- Injeksi referensi script tema ke seluruh dokumen HTML pada `doc/pages`.

## [1.0.3] - 2026-02-23

### Changed
- Restrukturisasi dokumentasi `doc/pages` dari Markdown ke HTML5 statis.
- Seluruh tautan internal dokumentasi diperbarui agar menggunakan ekstensi `.html`.
- Penyempurnaan gaya tampilan dokumentasi pada `doc/pages/assets/css/site.css` untuk konsistensi desktop dan mobile.

## [1.0.2] - 2026-02-23

### Changed
- Penyesuaian aturan `AGENTS.md` untuk permintaan commit:
  - Permintaan commit tidak perlu dicatat ke `CodexReport.md`.
  - Permintaan commit tidak wajib melalui `RefactorPlan.md`.
- Penyelarasan dokumen internal planning dan logging project.

## [1.0.1] - 2026-02-23

### Added
- Dokumentasi GitHub Pages pada `doc/pages` dengan struktur `Help` dan `API References`.
- Workflow GitHub Actions untuk deploy GitHub Pages dari folder `doc/pages`.
- Dokumentasi project file pada `doc/Emcode.Pst.Libs.csproj.md`.
- Dokumen `ChangeLogs.md` sebagai standar pencatatan rilis.

### Changed
- Aturan `AGENTS.md` diperbarui untuk alur commit/versioning:
  - Wajib tanya jenis kenaikan versi (`major`, `minor`, `build`) saat user meminta commit.
  - Wajib update `ChangeLogs.md` dan versi package sebelum commit.

## [1.0.0] - 2026-02-23

### Added
- Rilis awal `Emcode.Pst.Libs`.
- Kemampuan baca file PST (header, folder hierarchy, message metadata, attachment metadata).
- Fondasi API write/import (`CreateFolder`, `CreateMessage`, `ImportEml`) melalui abstraction writer.
- Metadata package NuGet awal untuk publikasi.
