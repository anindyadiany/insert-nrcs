# Background Story — INSERT NRCS

## Understanding the Newsroom

Sebuah program berita televisi terlihat sederhana ketika ditonton oleh pemirsa.

Di layar hanya terlihat presenter, video, graphics, dan informasi yang disampaikan kepada penonton. Namun di belakang layar terdapat sebuah organisasi produksi yang bekerja secara simultan untuk mengubah informasi mentah menjadi sebuah program yang siap ditayangkan.

Ada assignment desk yang mencari dan menentukan apa yang perlu diliput.

Ada reporter yang berada di lapangan.

Ada cameraman yang mengumpulkan gambar.

Ada producer yang menentukan angle dan editorial direction.

Ada writer yang menyusun naskah.

Ada editor yang mengolah materi video.

Ada personel yang melakukan review dan approval.

Ada rundown producer yang mengatur urutan berita dan durasi program.

Dan akhirnya ada tim yang membawa seluruh hasil produksi tersebut ke ruang kontrol untuk ditayangkan.

Semua bagian tersebut harus bekerja dengan informasi yang sama, dalam waktu yang sama, dengan perubahan yang dapat terjadi setiap saat.

Inilah lingkungan yang menjadi dasar keberadaan sebuah **NRCS**.

---

# Apa Itu NRCS?

**NRCS (Newsroom Computer System)** adalah sistem komputer yang digunakan untuk mengelola **proses produksi berita dari sisi newsroom**, mulai dari perencanaan dan assignment hingga story siap ditayangkan.

NRCS bukan sekadar aplikasi untuk menulis berita.

NRCS juga bukan sekadar aplikasi rundown.

Dan NRCS bukan MAM.

NRCS merupakan **pusat workflow editorial dan produksi newsroom**.

Secara sederhana, NRCS menghubungkan orang, story, naskah, media, approval, dan rundown dalam satu alur kerja.

```text id="r9f3q1"
             NEWSROOM
                 │
      ┌──────────┼──────────┐
      │          │          │
 Assignment    Story      Planning
      │          │          │
      └──────────┼──────────┘
                 │
        ┌────────┼────────┐
        │        │        │
      Script   Media   Approval
        │        │        │
        └────────┼────────┘
                 │
              Rundown
                 │
              On Air
```

Dengan demikian, NRCS dapat dianggap sebagai **operating system untuk workflow newsroom**.

Ia tidak menggantikan semua sistem broadcast lainnya.

Sebaliknya, NRCS menjadi tempat newsroom mengatur **apa yang sedang diproduksi, siapa yang mengerjakan, bagaimana perkembangannya, materi apa yang digunakan, dan kapan hasilnya harus ditayangkan.**

---

# NRCS Dalam Ekosistem Broadcast

Penting untuk membedakan NRCS dengan sistem lain yang terdapat dalam lingkungan televisi.

Sebuah stasiun televisi biasanya memiliki berbagai sistem dengan fungsi berbeda.

Secara sederhana:

```text id="8sl4gq"
                   BROADCAST ECOSYSTEM

                        NRCS
                         │
              Editorial / Production
                         │
          ┌──────────────┼──────────────┐
          │              │              │
         PAM            MAM            MOS
          │              │              │
     Production       Archive        Control /
       Media           Media         Automation
```

### NRCS

Mengelola:

* Story
* Assignment
* Planning
* Script
* Editorial workflow
* Approval
* Rundown
* Newsroom collaboration

### PAM — Production Asset Management

Mengelola media yang sedang digunakan dalam proses produksi.

Contohnya:

* Camera footage
* Working media
* Proxy
* Editing project
* Production assets

### MAM — Media Asset Management

Mengelola aset media yang memiliki nilai untuk disimpan dan digunakan kembali.

Contohnya:

* Master video
* Historical footage
* Program archive
* Metadata
* Long-term storage

### MOS

Digunakan untuk menghubungkan NRCS dengan perangkat dan sistem produksi seperti:

* Video server
* Graphics
* Teleprompter
* Automation
* Playout

Batas antara sistem-sistem tersebut dapat berbeda tergantung implementasi sebuah broadcaster. Namun secara konseptual, NRCS berfokus pada **workflow newsroom dan editorial production**.

INSERT mengikuti konsep tersebut.

---

# Mengapa NRCS Dibutuhkan?

Tanpa NRCS, sebuah newsroom tetap dapat bekerja.

Bahkan banyak newsroom memulai dengan cara yang sangat sederhana.

Assignment dapat disampaikan melalui chat.

Script dapat dibuat menggunakan word processor.

Media dapat disimpan di network folder.

Rundown dapat dibuat menggunakan spreadsheet.

Approval dapat dilakukan melalui chat atau komunikasi langsung.

Status pekerjaan dapat diketahui dengan bertanya kepada orang yang mengerjakannya.

Untuk volume pekerjaan yang kecil, pendekatan ini mungkin masih berjalan.

Masalah muncul ketika newsroom semakin sibuk.

---

# Newsroom Tanpa NRCS

Bayangkan satu hari produksi entertainment news.

Pagi hari, assignment desk menerima informasi mengenai seorang artis yang sedang menghadiri sebuah acara.

Assignment diberikan kepada reporter melalui komunikasi internal.

Reporter pergi ke lokasi.

Sementara itu producer sudah mulai membuat rencana story.

Reporter mengambil beberapa footage.

Footage kemudian dikirim ke newsroom.

Pada saat yang sama reporter mulai menulis script.

Producer menunggu footage.

Editor menunggu media.

Rundown producer menunggu kepastian durasi.

Kemudian muncul pertanyaan:

```text id="f9f0sx"
"Reporter sudah sampai?"

"Footage-nya sudah masuk?"

"File yang terbaru yang mana?"

"Script-nya sudah?"

"Producer sudah review?"

"Masih ada revisi?"

"Story ini sudah approved?"

"Sudah masuk rundown?"

"Durasi akhirnya berapa?"
```

Tidak ada satu sistem yang dapat memberikan jawaban langsung.

Informasi tersebar.

Orang menjadi sumber informasi.

Dan ketika orang yang mengetahui status tersebut sedang sibuk, proses ikut terhambat.

---

# Masalah Utama: Fragmented Workflow

Masalah sebenarnya bukan karena newsroom menggunakan banyak aplikasi.

Masalahnya adalah **informasi tidak memiliki satu pusat konteks**.

Misalnya terdapat file:

```text id="b5v7rf"
CAM_0034.MXF
```

Nama file tersebut tidak memberi tahu newsroom:

* Siapa yang mengambil?
* Untuk story apa?
* Kapan diambil?
* Apakah ini footage utama?
* Apakah sudah diverifikasi?
* Apakah sudah dibuat proxy?
* Apakah sudah digunakan editor?
* Apakah sudah tayang?
* Apakah masih diperlukan?

File tersebut hanya merupakan file.

NRCS mengubah file menjadi bagian dari workflow.

---

# Story Sebagai Pusat Workflow

Dalam INSERT NRCS, konsep utama adalah:

> **The Story is the center of the newsroom workflow.**

Misalnya:

**Story: Artis X Menghadiri Premiere Film**

Story tersebut memiliki hubungan dengan:

```text id="q8f9t2"
Story
│
├── Assignment
│    ├── Reporter
│    ├── Location
│    └── Deadline
│
├── Media
│    ├── Interview
│    ├── B-Roll
│    └── Standup
│
├── Script
│    ├── Draft 1
│    ├── Draft 2
│    └── Approved
│
├── Approval
│
└── Rundown
```

Ketika producer membuka story tersebut, producer dapat memahami keseluruhan status pekerjaan tanpa harus mencari informasi dari berbagai tempat.

---

# Contoh Workflow Dengan INSERT

Dengan INSERT, workflow dapat menjadi:

```text id="x3v2z8"
Story Idea
    ↓
Assignment
    ↓
Reporter
    ↓
Media Ingest
    ↓
Story Media
    ↓
Script
    ↓
Review
    ↓
Approval
    ↓
Rundown
    ↓
Ready for Broadcast
```

Setiap tahap memiliki status yang jelas.

Misalnya:

```text id="1ylz9s"
STORY: ARTIS X

Assignment      ✓ Completed
Media Ingest    ✓ Completed
Script          ✓ Draft
Review          ○ Waiting
Approval        ○ Waiting
Rundown         ○ Not Added
```

Producer tidak perlu bertanya kepada reporter.

Status pekerjaan terlihat dari sistem.

---

# Media Ingest Dalam NRCS

Media Ingest merupakan bagian penting dari INSERT NRCS.

Namun ingest bukan berarti semua file langsung masuk ke archive.

Media yang baru diterima harus melalui area produksi sementara.

```text id="m8y4tu"
Camera / Reporter
        ↓
   Ingest Queue
        ↓
      Copy
        ↓
     Verify
        ↓
Media Inspection
        ↓
      Proxy
        ↓
    Story Media
```

Media yang belum memiliki nilai untuk disimpan secara permanen tidak perlu langsung masuk MAM.

Hal ini penting karena newsroom dapat menerima banyak material yang akhirnya tidak digunakan.

Misalnya:

* Take yang gagal
* Footage blur
* Duplicate footage
* Footage test
* Footage yang tidak relevan
* Material yang akhirnya tidak dipakai

INSERT harus memungkinkan newsroom bekerja dengan media tersebut tanpa menjadikannya arsip permanen.

---

# NRCS Tidak Sama Dengan MAM

Ini merupakan prinsip penting dalam desain INSERT.

**NRCS menjawab:**

> Apa yang sedang diproduksi oleh newsroom?

**PAM menjawab:**

> Media apa yang sedang digunakan dalam proses produksi?

**MAM menjawab:**

> Media apa yang harus dipertahankan dan dapat digunakan kembali?

Ketiganya dapat saling terintegrasi, tetapi bukan merupakan sistem yang sama.

Untuk Version 1, INSERT hanya berfokus pada NRCS dan fungsi media ingest yang diperlukan untuk mendukung workflow tersebut.

Integrasi PAM dan MAM dapat dilakukan kemudian.

---

# Keuntungan Operasional INSERT

Dengan NRCS, newsroom tidak lagi bergantung sepenuhnya pada komunikasi manual untuk mengetahui status produksi.

### Sebelum

```text id="s7b2d1"
Producer
   ↓
Tanya Reporter
   ↓
Tanya Editor
   ↓
Tanya Ingest
   ↓
Tanya Rundown
```

### Dengan INSERT

```text id="7x0m4s"
Producer
   ↓
Open Story
   ↓
See Current Status
```

Satu story dapat menunjukkan:

```text id="g7j2v5"
Reporter       : Andi
Producer       : Budi
Media          : 8 assets
Proxy          : Ready
Script         : Version 3
Approval       : Approved
Rundown        : 18:15
Duration       : 02:35
```

Informasi yang sebelumnya membutuhkan beberapa percakapan kini menjadi informasi yang tersedia di dalam workflow.

---

# Ketika Breaking News Terjadi

Perbedaan antara newsroom dengan dan tanpa NRCS semakin terlihat ketika waktu menjadi sangat terbatas.

Misalnya sebuah informasi penting muncul pukul 17:40 dan program harus tayang pukul 18:00.

Tanpa workflow terintegrasi:

```text id="v2m3fz"
17:40
Informasi diterima

17:42
Producer mencari reporter

17:44
Reporter mulai bekerja

17:48
Footage dikirim

17:51
Mencari file

17:53
Script selesai

17:55
Producer review

17:57
Rundown diperbarui

18:00
On Air
```

Setiap perpindahan informasi berpotensi menjadi bottleneck.

Dengan INSERT:

```text id="i1h7kt"
17:40
Story Created

17:41
Assignment Created

17:42
Reporter Assigned

17:47
Media Ingested

17:50
Script Draft

17:53
Producer Review

17:56
Approved

17:57
Added to Rundown

18:00
On Air
```

Bukan berarti NRCS membuat pekerjaan manusia menjadi lebih sedikit.

NRCS membuat **koordinasi antarpekerjaan menjadi lebih cepat dan lebih terukur**.

---

# INSERT Sebagai Newsroom Operating System

Karena itu, INSERT NRCS tidak boleh dipandang sebagai sekumpulan menu:

```text id="q1n2cx"
Story
Assignment
Script
Rundown
Ingest
```

Masing-masing fitur harus menjadi bagian dari satu workflow.

Story dibuat.

Assignment diberikan.

Reporter bekerja.

Media masuk.

Script dibuat.

Producer melakukan review.

Story disetujui.

Story ditempatkan di rundown.

Rundown berubah.

Semua pihak mengetahui perubahan tersebut.

Inilah fungsi utama NRCS.

---

# Fokus INSERT Versi Pertama

INSERT tidak akan mencoba menjadi seluruh ekosistem broadcast sejak hari pertama.

Versi pertama berfokus pada workflow inti:

```text id="p8h6sa"
             INSERT NRCS

              Story
                │
        ┌───────┼───────┐
        │       │       │
   Assignment Media   Script
        │       │       │
        └───────┼───────┘
                │
             Approval
                │
             Rundown
                │
             Broadcast
```

Media Ingest menjadi fasilitas penting untuk memasukkan media ke dalam workflow story, tetapi bukan berarti INSERT menjadi MAM.

PAM, MAM, MOS, automation, dan AI merupakan kemungkinan pengembangan berikutnya.

---

# Prinsip Dasar INSERT

INSERT harus mengikuti satu prinsip sederhana:

> **The newsroom should work through the system, not around it.**

Sistem tidak boleh menjadi tambahan pekerjaan bagi reporter dan producer.

Sistem harus mengurangi pekerjaan administratif yang tidak perlu.

Jika reporter harus terus-menerus membuka spreadsheet, chat, folder, dan aplikasi berbeda hanya untuk mengetahui status sebuah story, maka sistem belum berhasil.

Jika producer harus bertanya kepada lima orang untuk mengetahui apakah sebuah story siap ditayangkan, maka workflow belum terintegrasi.

Jika operator harus mencari file secara manual untuk mengetahui media mana yang terkait dengan sebuah story, maka context belum tersedia.

INSERT harus menghilangkan friksi tersebut.

Tujuan akhirnya sederhana:

> Ketika sebuah story masuk ke newsroom, setiap orang yang terlibat harus dapat mengetahui **apa yang harus dilakukan, siapa yang bertanggung jawab, apa yang sudah selesai, apa yang masih kurang, dan kapan story tersebut harus siap ditayangkan.**

Itulah alasan INSERT NRCS dibangun.
