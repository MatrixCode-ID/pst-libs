# Dokumentasi Emcode.Pst.Libs.csproj

Dokumen ini menjelaskan konfigurasi package dan build pada file `src/Emcode.Pst.Libs/Emcode.Pst.Libs.csproj`.

## Identitas Project

- `Sdk`: `Microsoft.NET.Sdk`
  - Menetapkan SDK dasar untuk membangun library .NET.
- `TargetFramework`: `net10.0`
  - Menentukan target runtime/library .NET 10.
- `RootNamespace`: `Emcode.Pst`
  - Namespace akar default untuk source code.
- `ImplicitUsings`: `enable`
  - Mengaktifkan global using bawaan sesuai template SDK.
- `Nullable`: `enable`
  - Mengaktifkan nullable reference type untuk analisis null-safety.

## Metadata NuGet Package

- `PackageId`: `Emcode.Pst.Libs`
  - Nama identitas paket saat dipublish ke NuGet.
- `Version`: `1.0.0`
  - Versi paket saat proses pack.
- `Authors`: `Aan Dahliansyah`
  - Informasi author paket.
- `Company`: `MatrixCode`
  - Informasi organisasi/perusahaan.
- `Description`: `Library untuk baca/tulis PST.`
  - Deskripsi singkat paket.
- `PackageTags`: `pst outlook email`
  - Tag pencarian di NuGet.
- `PackageProjectUrl`: `https://github.com/MatrixCode-ID/pst-libs`
  - URL halaman project.
- `RepositoryUrl`: `https://github.com/MatrixCode-ID/pst-libs`
  - URL repository source code.
- `RepositoryType`: `git`
  - Tipe repository source control.
- `PackageReleaseNotes`: `Rilis awal Emcode.Pst.Libs dengan kemampuan baca PST dan fondasi API tulis/import.`
  - Catatan rilis yang tampil di package metadata.
- `PackageReadmeFile`: `README.id.md`
  - Menentukan readme yang ikut dipaketkan dan ditampilkan di NuGet.
- `PackageIcon`: `icon.png`
  - Menentukan file icon package.
- `PackageLicenseExpression`: `MIT`
  - Lisensi package menggunakan SPDX expression.
- `PackageCopyright`: `Copyright (c) 2026 MatrixCode. All rights reserved.`
  - Informasi hak cipta package.

## File Tambahan yang Ikut Dipack

Konfigurasi `ItemGroup` berikut memastikan readme dan icon dari root repository ikut masuk ke paket NuGet:

- `<None Include="..\\..\\README.id.md" Pack="true" PackagePath="\" />`
- `<None Include="..\\..\\icon.png" Pack="true" PackagePath="\" />`

Dampak konfigurasi ini:
- `README.id.md` tersedia di root package hasil `.nupkg`.
- `icon.png` tersedia di root package hasil `.nupkg`.
- Properti `PackageReadmeFile` dan `PackageIcon` dapat menemukan file saat proses pack.

## Catatan Pemeliharaan

- Jika `Version` diubah, sebaiknya sinkron dengan catatan `PackageReleaseNotes`.
- Jika nama file readme/icon diubah, update sekaligus pada:
  - `PackageReadmeFile` / `PackageIcon`
  - entry `ItemGroup` yang melakukan `Pack="true"`
- Jalankan validasi dengan:

```powershell
dotnet pack src\Emcode.Pst.Libs\Emcode.Pst.Libs.csproj -c Release -o artifacts
```
