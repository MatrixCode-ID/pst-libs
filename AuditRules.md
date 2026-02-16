# PROJECT AUDIT INSTRUCTION

## 1. OBJECTIVE

Lakukan audit menyeluruh pada project dengan fokus:

- Internal Code Quality
- Persiapan Open Source

Audit harus objektif, berbasis temuan nyata dalam kode, dan menghasilkan skor kuantitatif.

---

## 2. AUDIT CATEGORIES

Gunakan kategori berikut beserta bobotnya:

| Category              | Max Score |
|-----------------------|-----------|
| Architecture          | 15        |
| Code Quality          | 20        |
| Security              | 20        |
| Performance           | 15        |
| Concurrency           | 10        |
| Repository Hygiene    | 10        |
| Documentation         | 10        |

Total Maximum Score: 100

Score harus realistis dan konsisten dengan temuan.

---

## 3. OUTPUT FILE REQUIREMENT

### 3.1 Folder

Simpan hasil audit di folder `doc/AuditReports`

### 3.2 File Naming Convention

Gunakan format `AuditReport_nnnn_yyyymmdd_Snnn.txt`

Dimana:

- `nnnn` = nomor urut 4 digit dengan leading zero (0001, 0002, dst)
- `yyyymmdd` = tanggal audit dilakukan
- `Snnn` = total score audit (3 digit, leading zero)

Contoh:
`AuditReport_0001_20260216_S082.txt`
`AuditReport_0002_20260220_S091.txt`

### 3.3 Sequential Number Rule

1. Periksa folder `audit/`
2. Ambil nomor terbesar yang sudah ada
3. Tambahkan +1
4. Jika belum ada file → mulai dari `0001`

---

### 3.4 Score in Filename Rule

- Hitung total score terlebih dahulu
- Pastikan konsisten dengan breakdown
- Format score menjadi 3 digit (leading zero)
  - 7   → S007
  - 85  → S085
  - 100 → S100

---

## 4. REPORT FORMAT (Markdown)

File harus berupa `.md` dengan format struktur [AuditReportStructure.md](doc/AuditReports/AuditReportStructure.md)

## 5. STRICT RULES FOR AGENT

- Jangan berikan saran generik.
- Fokus hanya pada temuan nyata dalam kode.
- Sertakan file dan line number spesifik.
- Prioritaskan security dan concurrency.
- Gunakan severity secara objektif.
- Pastikan score mencerminkan kondisi aktual project.
- Jangan mengarang file atau line yang tidak ada.
- Jangan mengubah format laporan.
