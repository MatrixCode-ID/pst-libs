# Concepts

## Lapisan API

- `Emcode.Pst.Application`: facade utama (`PstFile`, `PstOpenOptions`).
- `Emcode.Pst.Application.Abstractions`: kontrak reader/writer.
- `Emcode.Pst.Domain`: model data folder, message, attachment, recipient.
- `Emcode.Pst.Infrastructure`: implementasi reader/writer.

## Pola Sync dan Async

Sebagian besar operasi write/read penting tersedia dalam pasangan:
- Sync method, contoh: `Open`, `CreateMessage`, `ImportEml`.
- Async method, contoh: `OpenAsync`, `CreateMessageAsync`, `ImportEmlAsync`.

Semua API async mendukung `CancellationToken`.

## Sumber Referensi API

Referensi API pada folder `api/` disusun dari XML documentation di source code (`/// <summary>`, `param`, `returns`).

## Terkait

- [API References](../api/index.md)
- [How-To](./index.md)
