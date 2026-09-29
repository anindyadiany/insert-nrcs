# Dokumentasi Handover — INSERT NRCS

**Penulis:** Anindya
**Tanggal:** 25 September 2026
**Repo:** github.com/anindyadiany/insert-nrcs

---

## 1. Latar Belakang

**INSERT NRCS** (Newsroom Computer System) adalah sistem yang mengelola proses produksi berita dari sisi newsroom — mulai dari assignment, liputan, penulisan naskah, approval, hingga rundown siap tayang. Dibangun sebagai proyek magang/tugas akhir untuk TransTV, solo developer, mengacu longgar ke NRCS nyata (Octopus, Annova, ENPS) tapi disederhanakan sesuai skala kebutuhan.

**Prinsip utama:** *The Story is the center of the newsroom workflow* — semua fitur (assignment, media, script, approval, rundown) berputar di sekitar satu entitas Story, sehingga siapa pun yang membuka story bisa langsung tahu statusnya tanpa perlu bertanya ke orang lain.

[SS: halaman Dashboard]

---

## 2. Tech Stack

| Layer | Teknologi |
|---|---|
| Framework | ASP.NET Core, .NET 10, Blazor Web App (`InteractiveServer`) |
| Database | MySQL 8 (Docker, port 3307) |
| ORM | EF Core 9.0.0 + Pomelo.EntityFrameworkCore.MySql |
| Auth | ASP.NET Identity (cookie-based, `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`) |
| Styling | Bootstrap + custom design system (`site.css`) |
| Icon | Iconify (`<iconify-icon>`) |
| Media processing | FFmpeg/FFprobe via `Process.Start` |
| Real-time | SignalR |

---

## 3. Arsitektur — Clean Architecture, 6 Project

```
Insert.Domain          → entity + enum saja, tanpa dependency
Insert.Application     → service, interface, business logic (depend ke Domain saja)
Insert.Infrastructure  → EF Core, repository, Identity (depend ke Application + Domain)
Insert.Media           → wrapper FFmpeg
Insert.Web             → UI Blazor (depend ke semua di atas) — proses sendiri
Insert.Worker          → background service, polling ingest job — proses sendiri
```

Aturan wajib: `Insert.Application` tidak boleh mereferensikan `Insert.Infrastructure`. Kalau Application butuh sesuatu milik Infrastructure, buat interface di Application, implementasi di Infrastructure (contoh: `IUserLookupService`, `INotificationService`).

`Insert.Web` dan `Insert.Worker` adalah dua proses terpisah dengan DI container masing-masing — service yang dipakai keduanya harus didaftarkan di **kedua** `Program.cs`.

[SS: directory structure — lihat bagian 8]

---

## 4. Model Domain & Alur Status

### Story workflow (`StoryStatus`)
```
Draft → Assigned → InProgress → InReview → Approved → Published
                        ↑_______________|   (Reject dari review, bukan status Revision terpisah)
Killed — dari Draft/Assigned/InProgress/InReview/Approved
```
Ditegakkan `StoryWorkflowService.CanTransition()` — dictionary transisi hardcoded, satu choke point (`StoryService.ChangeStatusAsync`), kecuali Draft→Assigned yang lewat `AssignmentService.CreateAssignmentAsync`.

### Approval workflow
Producer di Story Detail (status `InReview`) bisa **Approve** (→`Approved`) atau **Reject** (→`InProgress`, comment wajib). Tercatat di tabel `Approvals`. Reporter melihat alert revisi amber kalau status `InProgress` dan approval terakhir `Rejected`.

### Role (5, di-seed via `IdentitySeeder.cs`)
Reporter, Producer, AssignmentDesk, IngestOperator, Administrator.

**Akun test** (password semua `Password123!`): `andi@insert.local` (Reporter), `budi@insert.local` (Producer), `desk@insert.local` (AssignmentDesk), `ingest@insert.local` (IngestOperator), `admin@insert.local` (Administrator), `rina@insert.local` (Reporter), `dian@insert.local` (Reporter), `sinta@insert.local` (Producer), `desk2@insert.local` (AssignmentDesk)

### Tabel database
`user`, `role`, `user_role` (+ tabel Identity lain), `Stories`, `Assignments`, `AuditLogs`, `Scripts`, `ScriptVersions`, `IngestJobs`, `MediaAssets`, `StoryMedias`, `Approvals`, `Rundowns` + `RundownItems`.

Enum disimpan sebagai string (`HasConversion<string>()`), bukan int, supaya inspeksi manual DB gampang dibaca.

[SS: ERD / dbdiagram]

### 4.1 Perbedaan ERD: Proposal Awal vs Implementasi Aktual

Model database sempat direvisi selama development dibanding rancangan awal di `nrcs.dbml`/dbdiagram.io. Perubahan didokumentasikan sadar, bukan kelalaian.

[SS: ERD proposal awal (screenshot dbdiagram.io versi lama)]
[SS: ERD aktual (screenshot dbdiagram.io versi `nrcs.dbml` yang sudah direkonsiliasi)]

**Hilang dari proposal awal ke skema aktual (disederhanakan):**
- Enum type asli (`story_status`, `assignment_status`, `rundown_status`, dst.) — di implementasi EF Core enum disimpan sebagai `varchar` (string conversion), bukan native enum type MySQL.
- Beberapa constraint `[not null]`/`[unique]` di level dokumen ERD (`user.email`, `role.name`, `story.slug`, `story.title`, `script.story_id unique`) — constraint-nya tetap ditegakkan lewat validasi C#/EF Core, hanya tidak seluruhnya tercermin di dbml.
- Composite primary key `user_role (user_id, role_id)` — di implementasi Identity default, `user_role` pakai struktur standar ASP.NET Identity.

**Berubah karena keputusan desain saat development (proposal awal sudah tidak akurat):**
- `rundown.status` — field ini ada di proposal awal, tapi **tidak diimplementasikan** di entity aktual (status rundown belum jadi kebutuhan nyata saat build).
- `rundown.air_time` → di implementasi jadi `rundown.start_time`.
- `rundown_item` — proposal awal masih model "slot waktu" (`start_time`, `duration_seconds`, `status`, `story_id` wajib). Implementasi aktual berubah total jadi model "tipe item" (`item_type`: Open/Story/Break/Ads/Close, `story_id` opsional, `segment_label`, `segment_duration_seconds`) — lebih fleksibel untuk item non-story (bumper, iklan, dsb).
- `script` — proposal awal hanya `story_id` + `created_at`. Implementasi aktual menambah `content`, `updated_at`, `updated_by` karena `Script` dipakai sebagai working draft yang di-autosave, terpisah dari riwayat versi final di `ScriptVersion`.
- `media_asset` — ditambah kolom `checksum` (verifikasi integritas file saat ingest), tidak ada di proposal awal.
- `ingest_job` — ditambah `source_file_path`, `original_filename`, `created_by` untuk kebutuhan tracking pipeline worker yang baru muncul saat implementasi.

**Kesimpulan:** struktur inti (Story sebagai pusat workflow, relasi Assignment/Script/Approval/Rundown ke Story) tetap konsisten dengan proposal awal. Perubahan yang ada bersifat penyempurnaan/penyesuaian kebutuhan riil saat development, terutama di area Rundown yang paling banyak berubah bentuk.

---

## 5. Fitur yang Sudah Dibangun

| Milestone | Status | Catatan |
|---|---|---|
| M1 — Scaffolding | Selesai | |
| M2 — App shell (login, role, nav) | Selesai | |
| M3 — Story Management | Selesai | Audit log jalan |
| M4 — Assignment | Selesai | Reassignment teruji, no duplicate |
| M5 — Media Ingest (upload → worker → checksum → FFmpeg metadata/thumbnail/proxy) | Selesai | End-to-end teruji |
| M6 — Script (autosave + version) | Selesai | Autosave 1.5s debounce, save version, word count (filter cue/metadata), version viewer + restore |
| M7 — Media Bin file info | Selesai | Thumbnail + durasi + resolusi + ukuran file |
| M8 — Approval Workflow | Selesai | Approve/Reject, alert revisi untuk reporter |
| M9 — Realtime (SignalR) | **Selesai** | Rundown sync, assignment/status sync, script version sync — lihat bagian 6 |
| Rundown list + detail | **Selesai** | `/rundown` (list semua rundown) → `/rundown/{id}` (board/detail) |
| Reporter filter toggle (Stories) | **Selesai** | Toggle "All Stories / My Stories" khusus role Reporter |
| Dashboard per-role | Selesai | |
| Pause/Resume/Cancel ingest job | **Selesai** | Icon-only buttons di samping status chip; pause hanya untuk job `Queued`, cancel bisa dari `Queued`/`Paused`/`Processing` (berhenti di checkpoint berikutnya, bukan mid-copy langsung) |

[SS: halaman Story List]
[SS: halaman Story Detail — script editor + approval]
[SS: halaman Assignment]
[SS: halaman Ingest]
[SS: halaman Rundown list]
[SS: halaman Rundown detail/board]

---

## 6. Real-Time Sync (SignalR) — Ditambahkan Terakhir

Satu Hub tunggal `NrcsHub`, 3 jenis group:

| Group | Isi |
|---|---|
| `rundown-{rundownId}` | Semua tab yang membuka rundown tertentu |
| `user-{userId}` | Notifikasi personal ke satu user |
| `role-{roleName}` | Broadcast ke semua user role tertentu (mis. `role-Producer`) |

Hub hanya relay "ada perubahan, tolong refetch" — semua write tetap lewat service layer (`RundownService`, `AssignmentService`, `StoryService`, `ScriptService`), yang memanggil `INotificationService` setelah `SaveChangesAsync()` sukses.

**3 area yang sudah sync real-time:**
1. **Rundown syncing** — perubahan rundown (tambah/hapus story, reorder) langsung tersinkron ke semua tab yang membuka rundown yang sama.
2. **Assignment & status syncing** — assignment baru dan perubahan status story langsung ter-push.
3. **Script version & review syncing** — saat reporter save version, producer yang membuka story yang sama otomatis lihat versi terbaru.

**Catatan teknis penting untuk maintainer:**
- Setiap `hubConnection.On<T>(...)` handler yang akses DbContext **harus** dibungkus penuh dengan `InvokeAsync(...)`, bukan cuma `StateHasChanged()`. Kalau tidak → error `Connection must be Open; current state is Closed`.
- **Self-echo reentrancy**: tab yang trigger aksi juga menerima notifikasi echo dari aksinya sendiri (karena ikut di group yang sama). Jadi handler client-side tidak perlu reload data manual lagi setelah trigger — cukup andalkan hub echo, supaya tidak race di DbContext yang sama.
- Koneksi `HubConnectionBuilder` di dalam kode Blazor Server adalah server-to-server, tidak akan muncul di tab Network/Console browser — normal.

[SS: bukti video — Assign Story Sync]
[SS: bukti video — Script Version History & Review Syncing]
[SS: bukti video — Rundown Syncing]
[SS: perbandingan page sebelum/sesudah update real-time]

**Ingest queue TIDAK pakai SignalR** — sempat dicoba (Worker connect ke hub Web sebagai `HubConnection` client, karena `Insert.Worker` adalah proses terpisah tanpa hosted Hub sendiri), tapi direvert demi mengurangi risiko/kompleksitas mendekati deadline. Ingest queue tetap pakai polling `PeriodicTimer` 2 detik di `Ingest.razor`. Keputusan ini didokumentasikan sadar — bukan keterbatasan teknis, murni trade-off waktu.

### 6.1 Auto-refresh polling ingest — bug DbContext race (ditemukan & diperbaiki 25 Sep 2026)

Auto-refresh queue ingest sempat error `A second operation was started on this context instance before a previous operation completed`. Root cause dua lapis:
1. Awalnya cuma `StateHasChanged()` yang dibungkus `InvokeAsync`, bukan seluruh tick — sudah diperbaiki sesuai pola dokumentasi project (semua akses `DbContext` dari background loop harus di dalam `InvokeAsync`).
2. Setelah itu masih ada race: upload file besar (`CopyToAsync`, I/O lambat) membuka window di mana timer tick lain — meski sudah `InvokeAsync` — tetap bisa mengakses `DbContext` scoped yang sama secara bersamaan. `InvokeAsync` cuma menjamin eksekusi di sync context yang sama, **bukan** menjamin non-overlap antar task async yang berbeda.

**Fix final:** `SemaphoreSlim(1,1)` (`_dbLock`) di `Ingest.razor` yang menyerialkan semua pemanggilan `IngestService` (refresh, upload, retry, pause, resume, cancel) — memastikan cuma satu operasi DB yang jalan di satu waktu untuk komponen itu.

**Catatan penting untuk debug ke depan:** kalau UI ingest kelihatan "nyangkut" (misal status masih `Queued` padahal DB sudah `Completed`), itu **hampir selalu bukan bug** — itu tab browser lama yang circuit Blazor-nya jadi orphan karena proses `Insert.Web` di-restart selama development (timer auto-refresh ikut mati bareng circuit lama). Solusinya reload tab, bukan ubah kode.

---

## 7. Gap & Isu Terbuka

1. `Story.ProducerId` tidak pernah di-set otomatis — masih keputusan terbuka (auto-set saat approval pertama vs assignment manual).
2. Version history belum menampilkan nama reviewer (GUID belum diterjemahkan ke nama).
3. Diff antar versi script masih side-by-side penuh, bukan highlight baris yang berubah.
4. Ada 2 baris `Console.WriteLine` debug yang masih nyangkut di `AutoRefreshLoop()` (`Ingest.razor`) — sisa diagnosa race condition di atas, belum dibersihin.

---

## 8. Struktur Direktori

```
src/
├── Insert.Domain/
│   └── Entities/
├── Insert.Application/
│   └── Stories/              ← semua service + repository interface (termasuk INotificationService)
├── Insert.Infrastructure/
│   ├── Stories/
│   ├── Identity/
│   └── Migrations/
├── Insert.Media/
├── Insert.Worker/
└── Insert.Web/
    ├── Hubs/                  ← NrcsHub.cs, NotificationService.cs
    ├── Components/
    │   ├── Pages/              ← StoryList, StoryDetail, Assignments, Ingest,
    │   │                          Rundown (detail), RundownList, Dashboard
    │   ├── Layout/
    │   ├── Shared/
    │   └── Account/Pages/
    ├── wwwroot/css/site.css    ← design system
    └── Program.cs
```

[SS: screenshot tree lengkap di terminal/IDE]

---

## 9. Pola Bug Berulang (baca sebelum debug)

- Edit kode **tidak hot-reload** — wajib stop + restart `dotnet run` (Web & Worker) setelah setiap perubahan.
- CSS scoped (`ComponentName.razor.css`) diam-diam menang lawan `site.css` untuk class yang sama. Aturan: class dipakai 2+ halaman → `site.css`; layout unik satu halaman → `.razor.css` halaman itu saja.
- DI registration harus sebelum `builder.Build()`.
- Service yang dipakai Web & Worker harus didaftarkan di **kedua** `Program.cs`.
- `double.TryParse` tanpa culture eksplisit bisa salah parsing — selalu `NumberStyles.Float, CultureInfo.InvariantCulture`.
- Cache build (`obj`/`bin`) kadang bikin error membingungkan — hapus lalu rebuild bersih kalau error tidak masuk akal.
- SignalR: lihat catatan self-echo & `InvokeAsync` di bagian 6.
- `InvokeAsync` menjamin sync context, **bukan** non-overlap — kalau ada I/O lambat (upload/copy file) berbarengan dengan polling timer, tetap bisa race di `DbContext` yang sama walau sudah `InvokeAsync`. Butuh lock eksplisit (`SemaphoreSlim`) kalau begitu — lihat bagian 6.1.
- Restart proses Web = semua circuit Blazor lama jadi orphan, tab browser lama nampilin state basi. Bukan bug, cukup reload tab.

---

## 10. Cara Menjalankan Lokal

```
docker start insert-mysql

Terminal 1: dotnet run --project src/Insert.Worker
Terminal 2: dotnet run --project src/Insert.Web
```
Buka `http://localhost:5047`, login pakai salah satu akun test di bagian 4.

---

## 11. Next Steps

- [x] Update `insert-nrcs-master-context.md` dengan progress terbaru — selesai 25 September 2026.
- [ ] Isi nama reviewer di version history.
- [ ] Putuskan logic `Story.ProducerId`.
- [ ] (opsional) `Clients.GroupExcept` di Hub untuk exclude sender dari echo-nya sendiri.
- [ ] Hapus 2 baris `Console.WriteLine` debug di `Ingest.razor` (`AutoRefreshLoop`).
- [ ] Commit + push perubahan ingest Pause/Resume/Cancel dan fix DbContext race dari mesin lokal.
- [ ] Isi semua placeholder `[SS: ...]` di dokumen ini dengan screenshot/video aktual.
