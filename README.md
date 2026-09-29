# INSERT NRCS Documentation

**Penulis:** Anindya Diany Putri
**Tanggal:** 25 September 2026
**Repo:** [github.com/anindyadiany/insert-nrcs](https://github.com/anindyadiany/insert-nrcs)

## 1. Background

INSERT NRCS (Newsroom Computer System) adalah sistem yang mengelola proses produksi berita dari sisi newsroom, mulai dari assignment, liputan, penulisan naskah, approval, hingga rundown siap tayang. Dibangun sebagai proyek magang untuk TransTV, solo developer, mengacu longgar ke NRCS nyata (Octopus, Annova, ENPS) tapi disederhanakan sesuai skala kebutuhan.

*The Story is the center of the newsroom workflow* — semua fitur (assignment, media, script, approval, rundown) berputar di sekitar satu entitas Story, sehingga siapa pun yang membuka story bisa langsung tahu statusnya tanpa perlu bertanya ke orang lain.

<p align="center">
  <img width="500" alt="Preview Dashboard INSERT NRCS" src="https://github.com/user-attachments/assets/180997fa-033f-42e0-81d0-a2f94e8cb5b5" /><br>
  <em>Gambar 1.1. Preview Dashboard INSERT NRCS</em>
</p>

<p align="center">
  <img width="420" alt="Alur kerja newsroom yang dikelola NRCS" src="https://github.com/user-attachments/assets/9db26c62-2582-46b8-b1fe-1fa81ab8a851" /><br>
  <em>Gambar 1.2. Alur kerja newsroom yang dikelola NRCS, dari perencanaan hingga tayang.</em>
</p>

## 2. Tech Stack

| Layer | Teknologi |
|---|---|
| Framework | ASP.NET Core, .NET 10, Blazor Web App |
| Database | MySQL 8 |
| ORM | EF Core 9.0.0 + Pomelo.EntityFrameworkCore.MySql |
| Auth | ASP.NET Identity |
| Styling | Bootstrap + custom design system |
| Icon | Iconify |
| Media processing | FFmpeg / FFprobe |
| Real-time | SignalR |

## 3. Architecture

INSERT NRCS memakai pendekatan Clean Architecture: kode dipisah menjadi enam project dalam empat layer. Aturan intinya, ketergantungan selalu mengarah ke dalam — lapisan luar seperti UI dan database bergantung pada domain, tidak pernah sebaliknya — sehingga komponen teknis bisa diganti tanpa merombak logika inti.

<p align="center">
  <img width="560" alt="Lapisan Architecture" src="https://github.com/user-attachments/assets/e5c104be-6005-4ad9-ba4d-323ff2b734d2" /><br>
  <em>Gambar 3.1. Lapisan Architecture</em>
</p>

| Project | Tanggung Jawab | Bergantung pada |
|---|---|---|
| **Insert.Domain** | Entity dan enum murni beserta aturan domain. Tidak tahu apa pun soal database maupun UI. | — |
| **Insert.Application** | Service, interface, dan business logic. | Domain |
| **Insert.Infrastructure** | Implementasi EF Core, repository, dan ASP.NET Identity — jembatan ke database MySQL. | Application, Domain |
| **Insert.Media** | Pembungkus pemanggilan FFmpeg/FFprobe untuk memproses berkas media saat ingest. | Application, Domain |
| **Insert.Web** | Antarmuka Blazor Server, sekaligus meng-host SignalR Hub untuk sinkronisasi real-time. | semua lapisan di atas |
| **Insert.Worker** | Background service yang mem-polling antrean ingest dan menjalankan pipeline media. | Application, Infrastructure, Media |

## 4. Domain Model & Flow Status

<p align="center">
  <img width="820" alt="State machine status Story" src="https://github.com/user-attachments/assets/4f74d595-1385-42fd-a213-edef9288bbb6" /><br>
  <em>Gambar 4.1. State machine status Story</em>
</p>

Perpindahan status Story mengikuti jalur tetap yang dicek lewat `StoryWorkflowService.CanTransition()`, yaitu dari Draft sampai Published. Selain jalur normal ini, ada Reject (Story balik ke pengerjaan) dan Kill (Story dibatalkan sebelum tayang).

Transisi `Draft → Assigned` jalan lewat `AssignmentService.CreateAssignmentAsync`, sisanya lewat `StoryService.ChangeStatusAsync`. Tidak ada status "Revision" khusus; Reject langsung membalikkan Story ke `InProgress`.

### 4.1 Penegakan Transisi

Aturan perpindahan status disimpan sebagai *dictionary* di dalam `StoryWorkflowService.CanTransition()`, sehingga hanya transisi yang terdaftar yang dapat dijalankan. Seluruh perubahan status dilakukan melalui satu titik terpusat, yaitu `StoryService.ChangeStatusAsync`. Pengecualiannya adalah transisi `Draft → Assigned`, yang berlangsung saat story di-assign melalui `AssignmentService.CreateAssignmentAsync`.

### 4.2 Approval Workflow

Producer membuka Story Detail saat status `InReview`, lalu memilih salah satu:

- **Approve** → status menjadi `Approved`.
- **Reject** → status kembali ke `InProgress`, dengan comment wajib diisi.

Setiap keputusan tercatat di tabel `Approvals` (reviewer, comment, timestamp, versi naskah yang direview). Kalau status `InProgress` dan approval terakhir `Rejected`, Reporter melihat alert revisi berwarna amber di halaman story.

### 4.3 Role

Ada 5 role, di-seed lewat `IdentitySeeder.cs`. Tiap role menentukan aksi apa yang boleh dilakukan di dalam workflow di atas.

| Role | Tanggung Jawab |
|---|---|
| Reporter | Mengerjakan liputan dan menulis naskah untuk story yang di-assign; submit naskah untuk direview. |
| Producer | Me-review naskah: Approve, Reject, publish story, dan bisa mem-Kill story. |
| Assignment Desk | Membuat story baru dan meng-assign-nya ke reporter. |
| Ingest Operator | Meng-upload dan mengelola berkas media di modul ingest. |
| Administrator | Mengelola user dan role; akses penuh ke sistem. |

### 4.4 Database Table

Tabel dikelompokkan berdasarkan fungsinya:

| Kelompok | Tabel |
|---|---|
| Identity & user | `user`, `role`, `user_role` (+ tabel Identity lainnya) |
| Story & alur | `Stories`, `Assignments`, `Approvals`, `AuditLogs` |
| Naskah | `Scripts`, `ScriptVersions` |
| Media | `IngestJobs`, `MediaAssets`, `StoryMedias` |
| Rundown | `Rundowns`, `RundownItems` |

Keseluruhan relasi antar tabel digambarkan pada ERD berikut, dengan Story sebagai entitas pusat yang menghubungkan assignment, naskah, media, dan rundown.

<p align="center">
  <img width="820" alt="Struktur ERD DBML" src="https://github.com/user-attachments/assets/71fe7e7c-ce7e-448c-951e-b48157e75f3f" /><br>
  <em>Gambar 4.1. Struktur ERD DBML</em>
</p>

## 5. Features

| Milestone | Status | Note |
|---|---|---|
| M1: Scaffolding | Selesai | |
| M2: App shell (login, role, nav) | Selesai | |
| M3: Story Management | Selesai | Audit log jalan |
| M4: Assignment | Selesai | Reassignment teruji, no duplicate |
| M5: Media Ingest (upload → worker → checksum → FFmpeg metadata/thumbnail/proxy) | Selesai | End-to-end teruji |
| M6: Script (autosave + version) | Selesai | Autosave 1.5s debounce, save version, word count (filter cue/metadata), version viewer + restore |
| M7: Media Bin file info | Selesai | Thumbnail + durasi + resolusi + ukuran file |
| M8: Approval Workflow | Selesai | Approve/Reject, alert revisi untuk reporter |
| M9: Realtime (SignalR) | Selesai | Rundown sync, assignment/status sync, script version sync — lihat bagian 6 |
| M10: Dashboard per-role | Selesai | |

10 dari 10 milestone selesai. Seluruh alur inti newsroom — dari pembuatan story, penugasan, ingest media, penulisan naskah, approval, hingga rundown — sudah berjalan dan dapat digunakan.

### 5.1 Potential Improvements

Beberapa hal di luar cakupan milestone awal yang bisa dikembangkan untuk melengkapi sistem:

| Plan | Description |
|---|---|
| Penetapan Producer otomatis | Menghubungkan setiap story ke Producer secara otomatis (mis. saat approval pertama), sehingga tidak perlu ditugaskan manual. |
| Nama reviewer di riwayat versi | Menampilkan nama reviewer di version history, bukan sekadar menyimpan identitasnya secara internal. |
| Highlight perubahan antar versi naskah | Menyorot baris yang berubah saat membandingkan dua versi, bukan menampilkan keduanya secara utuh berdampingan. |
| Validasi tipe file saat ingest | Menolak berkas non-video sebelum diproses, agar job ingest tidak gagal di tahap FFmpeg. |
| Status & alur rundown | Menambah status pada rundown (mis. draft → siap tayang → tayang) untuk menandai kesiapan setiap rundown. |
| Ekspor rundown | Mengekspor susunan rundown ke format yang bisa dipakai untuk kebutuhan siaran atau dokumentasi. |

### 5.2 UI

Berikut tampilan halaman-halaman utama aplikasi.

<p align="center">
  <img width="760" alt="Halaman Story List" src="https://github.com/user-attachments/assets/3d8f5edf-6ff9-46c9-a5fe-1962bd3f25d3" /><br>
  <em>Gambar 5.2.1. Halaman Story List. Daftar seluruh story beserta status dan prioritasnya. Reporter punya toggle "All Stories / My Stories".</em>
</p>

<p align="center">
  <img width="760" alt="Halaman Story Detail" src="https://github.com/user-attachments/assets/d7ab0dae-e883-469d-a126-93c1fdd9bacd" /><br>
  <em>Gambar 5.2.2. Halaman Story Detail. Editor naskah dengan autosave dan version history, dilengkapi panel approval untuk Producer.</em>
</p>

<p align="center">
  <img width="760" alt="Halaman Assignment" src="https://github.com/user-attachments/assets/23708a6b-3764-468d-afa3-b3300d6b3d84" /><br>
  <em>Gambar 5.2.3. Halaman Assignmen. Pembuatan dan penugasan story ke reporter oleh Assignment Desk.</em>
</p>

<p align="center">
  <img width="760" alt="Halaman Ingest" src="https://github.com/user-attachments/assets/09a3dc73-1a8b-44af-b58f-2af9496cf8fb" /><br>
  <em>Gambar 5.2.4. Halaman Ingest. Upload berkas media dan antrean pemrosesan (queue) beserta status tiap job.</em>
</p>

<p align="center">
  <img width="760" alt="Halaman Rundown Detail / Board" src="https://github.com/user-attachments/assets/051f9b7b-d54d-4794-8706-760348ca5dc0" /><br>
  <em>Gambar 5.2.5. Halaman Rundown Detail / Board. Susunan item rundown (story, bumper, iklan, dsb) untuk satu rundown.</em>
</p>

## 6. Gap

1. **Penetapan Producer belum otomatis**: setiap story belum otomatis terhubung ke seorang Producer. Perlu diputuskan apakah Producer ditetapkan otomatis saat approval pertama, atau ditugaskan manual.
2. **Nama reviewer belum tampil di riwayat versi**: riwayat versi naskah sudah menyimpan siapa yang me-review, tapi di tampilan belum diterjemahkan menjadi nama pengguna.
3. **Perbandingan versi belum menyorot perubahan**: saat membandingkan dua versi naskah, keduanya ditampilkan utuh berdampingan; bagian yang berubah belum di-highlight secara spesifik.

## 7. Directory Structure

Berikut susunan folder proyek. Tiap project menempati satu direktori di bawah `src/`, sesuai pembagian lapisan pada arsitektur.

```
src/
├── Insert.Domain/
│   └── Entities/
├── Insert.Application/
│   └── Stories/
├── Insert.Infrastructure/
│   ├── Stories/
│   ├── Identity/
│   └── Migrations/
├── Insert.Media/
├── Insert.Worker/
└── Insert.Web/
    ├── Hubs/
    ├── Components/
    │   ├── Pages/
    │   ├── Layout/
    │   ├── Shared/
    │   └── Account/Pages/
    ├── wwwroot/css/
    └── Program.cs
```

## 8. How to Run Locally

Bagian ini menjelaskan langkah menjalankan INSERT NRCS secara lokal. Aplikasi terdiri dari tiga komponen yang harus berjalan bersamaan: database, background worker, dan web server.

**Prerequisites:** .NET 10 SDK, Docker, Git, dan FFmpeg sudah terpasang di komputer.

### 8.1 Clone the repository

```bash
git clone https://github.com/anindyadiany/insert-nrcs.git
cd insert-nrcs
```

### 8.2 Start the database

```bash
docker start insert-mysql
```

Menyalakan kontainer MySQL (database `insert_nrcs`, dipetakan ke port `3307` di host).

### 8.3 Create the database schema (migration)

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/Insert.Infrastructure --startup-project src/Insert.Web
```

Aplikasi tidak melakukan migrasi otomatis saat dijalankan, sehingga skema tabel harus dibuat lebih dulu melalui migration. Baris pertama memasang tool `dotnet-ef` (cukup sekali), baris kedua menerapkan seluruh migration yang tersedia. Data role dan akun test akan terisi otomatis (*seeding*) saat web server pertama kali dijalankan.

Pastikan kontainer database sudah menyala, lalu jalankan kedua komponen berikut di terminal terpisah.

### 8.4 Start the background worker *(Terminal 1)*

```bash
dotnet run --project src/Insert.Worker
```

Background process yang memantau queue ingest dan memproses media (checksum, thumbnail, proxy via FFmpeg).

### 8.5 Start the web server *(Terminal 2)*

```bash
dotnet run --project src/Insert.Web
```

Menjalankan aplikasi web utama. Setelah siap, buka `http://localhost:5047` di browser dan login menggunakan salah satu akun test.
