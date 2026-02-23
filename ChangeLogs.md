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
