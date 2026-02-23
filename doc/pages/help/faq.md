# FAQ

## Apakah library ini hanya read-only?

Tidak. Read sudah tersedia, dan write tersedia melalui kontrak writer (`IPstWriter`) dengan implementasi `PstInMemoryWriter` dan `PstNdbWriter`.

## Bedanya `PstMinimalReader` dan `PstNdbReader`?

- `PstMinimalReader`: validasi header + metadata dasar.
- `PstNdbReader`: parsing NDB untuk folder/message nyata.

## Apakah semua operasi punya async?

Untuk operasi utama read/write, tersedia pasangan async yang mendukung `CancellationToken`.

## Di mana referensi object lengkap?

Lihat [API References](../api/index.md), disusun per namespace dan type.
