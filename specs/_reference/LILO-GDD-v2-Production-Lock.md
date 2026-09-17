**LILO**

Lights In, Lights Out

**Game Design Document v2 --- Production Lock**

*Siap dipakai sebagai source of truth untuk mulai development Fase 1
(Core Prototype & Testing)*

Tim: Calzy (Tech Lead) · Radit · Fathia (Design Lead) · Eca · Eileen ·
Salwa

Platform: iOS (iPhone, landscape) · Engine: SpriteKit + SceneKit
(SK3DNode) · Bahasa: Swift

Versi: 2.0 --- 16 September 2026

# Daftar Isi

*Struktur dokumen. Bab 17 (GameConfig) dan Bab 20 (Rencana Testing
Fase 1) adalah dua bab yang paling langsung dipakai minggu ini.*

  --------------------------------------------------------------------------
  **Bab**   **Judul**                  **Isi**
  --------- -------------------------- -------------------------------------
  **1**     Vision & Core Experience   Premis, design pillars, target durasi

  **2**     Core Gameplay Loop         Loop 30 detik / 5 menit / satu run

  **3**     Struktur Floor &           Floor 52 / 51 / 50 dan aturan spawn
            Difficulty Curve           battery

  **4**     Player Systems             Movement, sprint, interaction, hiding

  **5**     Light & Battery System     Light state, Compact Darkness, sumber
                                       battery

  **6**     Monster AI Specification   State machine, nilai per floor, spawn
                                       rules

  **7**     Noise & Detection System   Noise radius per aksi, aturan deteksi

  **8**     Objective, Pintu &         Tipe objective per floor, feedback
            Progression                

  **9**     Lives, Checkpoint & Fail   Reset rules, flowchart kekalahan
            State                      

  **10**    Narrative & Endings        Eddie, prologue, foreshadowing,
                                       ending

  **11**    Visual & Art Pipeline      Hybrid 2D + 3D lewat SK3DNode

  **12**    Lighting System            Vignette 2D + SCNLight 3D

  **13**    Camera Specification       Orthographic, smooth follow, clamping

  **14**    Audio Design               Daftar asset, dynamic mixing

  **15**    Controls & UX              Joystick, haptic, onboarding

  **16**    Level Design Guidelines    Blueprint template, checklist
                                       validasi

  **17**    GameConfig --- Master      SEMUA angka tuning di satu tempat
            Tuning Table               

  **18**    Scope Lock                 Must have, cut list, urutan potong

  **19**    Production Plan            Roles, fase, kebijakan AI & CC0

  **20**    Fase 1 --- Rencana Testing Yang harus dibuktikan minggu ini

  **21**    Open Items                 Belum dikunci, tapi tidak memblokir
                                       Fase 1
  --------------------------------------------------------------------------

# 1. Vision & Core Experience

## 1.1 Premis

Eddie, seorang salaryman biasa, ketiduran di meja kerjanya. Waktu
bangun, seluruh gedung sudah gelap total. Dengan hanya sebuah handheld
emergency lamp yang selalu dia simpan di loker mejanya, Eddie harus
turun tiga lantai dan keluar dari gedung --- sementara sesuatu
berkeliling di kantor itu, mencarinya lewat suara.

## 1.2 Core Experience

Immersive horror dengan tension tinggi. Rasa takut datang dari
keterbatasan penglihatan dan ketidaktahuan posisi monster, bukan dari
jumpscare atau kekerasan.

## 1.3 Design Pillars

*Setiap keputusan desain baru harus lolos minimal satu pillar di bawah
ini. Kalau tidak lolos satu pun, jangan dikerjakan.*

  -----------------------------------------------------------------------
  **Pillar**       **Artinya**                **Konsekuensi praktis**
  ---------------- -------------------------- ---------------------------
  **Limited        Player hanya melihat apa   Tidak ada minimap, tidak
  Vision**         yang disinari senter.      ada indikator posisi
                   Sisanya gelap.             monster, tidak ada outline
                                              musuh menembus dinding.

  **Uncertainty    Player dan monster         Monster hanya tahu 'ada
  over             sama-sama tidak punya      suara di area ini'. Player
  Information**    informasi sempurna.        hanya tahu 'ada sesuatu di
                                              dekat sini' lewat audio.

  **Meaningful     Setiap battery adalah      Jumlah battery makin
  Resource**       keputusan, bukan sekadar   sedikit tiap floor.
                   pickup.                    Mengambil battery
                                              menghasilkan suara.

  **Compact        Tekanan diekspresikan      Battery habis = lingkaran
  Pressure**       lewat ruang pandang yang   pandang mengecil drastis,
                   menyempit.                 bukan game over.
  -----------------------------------------------------------------------

## 1.4 Target Durasi

  ------------------------------------------------------------------------
  **Metrik**           **Target**        **Catatan**
  -------------------- ----------------- ---------------------------------
  **Durasi satu        ± 5 menit         Diukur dari spawn player sampai
  floor**                                masuk pintu turun lantai.

  **Durasi satu full   ± 15 menit        3 floor. Bisa diperpanjang
  run**                                  belakangan lewat config, bukan
                                         lewat menambah floor baru.

  **Worst-case retry** ± 5 menit         Karena checkpoint ada di awal
                                         tiap floor.
  ------------------------------------------------------------------------

*Cara memperpanjang durasi kalau ternyata terlalu pendek: kurangi jumlah
battery, perlambat spawn battery, perbesar map. JANGAN tambah floor
ke-4.*

# 2. Core Gameplay Loop

## 2.1 Loop 30 detik (moment-to-moment)

explore → lihat sekitar → putuskan rute → cari resource → dengar cue
monster → hide/evade → lanjut

## 2.2 Loop 5 menit (satu floor)

masuk floor → identifikasi objective → explore → tekanan baterai naik →

encounter monster → selesaikan obstacle → dapat key → buka pintu turun →
progress

## 2.3 Loop penuh (satu run)

Prologue → Floor 52 → Floor 51 → Floor 50 → Final Door → Ending

Player punya 3 lives (tersembunyi dari HUD). Mati = kembali ke awal
floor yang sedang dijalani. Lives habis = Bad Ending.

## 2.4 Aturan penomoran lantai

Lantai dihitung mundur untuk memperkuat rasa 'turun menuju keluar':
Floor 52 → Floor 51 → Floor 50. Setiap kali player turun, muncul splash
text singkat berisi nomor lantai baru.

# 3. Struktur Floor & Difficulty Curve

Tiga floor = tiga tingkat difficulty. Yang membedakan bukan cuma ukuran
map, tapi perilaku monster, kelangkaan battery, dan jumlah pintu
terkunci.

  ---------------------------------------------------------------------------
                  **Floor 52 (Easy)** **Floor 51          **Floor 50 (Hard)**
                                      (Medium)**          
  --------------- ------------------- ------------------- -------------------
  **Peran**       Learn               Pressure            Mastery

  **Monster**     TIDAK ADA. Hanya    Aktif. Patrol +     Aktif & agresif.
                  SFX dari kejauhan   investigate + chase Patrol lebih cepat,
                  sebagai hint.       normal.             investigate & chase
                                                          lebih lama.

  **Objective**   Cari jalan +        Cari 1 key untuk    Cari key untuk 3
                  belajar sistem      membuka pintu       pintu terkunci,
                  battery. Tanpa key. turun.              lalu Final Door.

  **Battery di    3--5 sekaligus      Maks 2 sekaligus,   Maks 1 sekaligus,
  map**           (melimpah), statis. respawn tiap 30     respawn tiap 60
                                      detik.              detik.

  **Layout**      Lorong lebar,       Banyak              Banyak dead-end,
                  sedikit             persimpangan.       rute sempit.
                  percabangan.                            

  **Hiding spot** Ada (dipakai untuk  Ada.                Ada, tapi lebih
                  mengajarkan                             jarang.
                  mekanik).                               
  ---------------------------------------------------------------------------

## 3.1 Aturan spawn battery (penting untuk programmer)

Floor 51 dan 50 memakai sistem respawn, bukan battery statis. Aturannya:

- Setiap floor punya daftar Battery Spawn Point yang sudah ditentukan
  manual di level design.

- Sistem menjaga jumlah battery aktif di map tidak melebihi batas floor
  tersebut (Floor 51 = 2, Floor 50 = 1).

- Kalau jumlah battery aktif di bawah batas, timer respawn berjalan (30
  detik / 60 detik). Saat timer habis, satu battery muncul di spawn
  point acak yang saat itu kosong dan tidak berada di dalam radius
  pandang player.

- Battery tidak pernah muncul di depan mata player --- ini memutus ilusi
  dan bikin terasa artifisial.

- Floor 52 tidak memakai respawn sama sekali: 3--5 battery ditaruh
  statis sejak awal.

# 4. Player Systems

## 4.1 Movement

  ------------------------------------------------------------------------
  **Parameter**        **Nilai**      **Catatan**
  -------------------- -------------- ------------------------------------
  **Walk speed**       1.0 (base)     Unit dasar. Semua kecepatan lain
                                      adalah multiplier dari nilai ini.

  **Sprint speed**     1.6× walk      Tunable lewat config.

  **Stamina**          TIDAK ADA      Noise sudah cukup jadi pembatas
                                      alami. Jangan tambahkan stamina bar.

  **Sprint drain       TIDAK ADA      Sengaja ditolak --- tidak ada
  battery**                           hubungan logis antara lari dan
                                      baterai senter.

  **Trigger sprint**   Joystick       Tanpa tombol terpisah. Threshold
                       didorong penuh sprint diatur di config.
  ------------------------------------------------------------------------

## 4.2 Interaction

Satu tombol aksi context-sensitive di sisi kanan layar. Tombol yang sama
dipakai untuk semua interaksi, dan label/ikonnya berubah mengikuti objek
terdekat yang bisa di-interact.

  ------------------------------------------------------------------------
  **Konteks**          **Aksi tombol**      **Feedback**
  -------------------- -------------------- ------------------------------
  **Dekat battery**    Ambil battery        Objek di-highlight sebelum
                                            diambil + SFX pickup

  **Senter \< 10% /    Pasang battery dari  SFX pemasangan + cahaya
  kosong**             slot                 kembali penuh

  **Dekat key**        Ambil key            Highlight + SFX pickup

  **Dekat pintu        Buka pintu           SFX unlock + animasi pintu
  terkunci (punya                           
  key)**                                    

  **Dekat pintu        Gagal / tidak aktif  SFX 'terkunci' pendek
  terkunci (tanpa                           
  key)**                                    

  **Dekat kolong       Masuk / keluar       Transisi kamera + perubahan
  meja**               hiding               audio (teredam)
  ------------------------------------------------------------------------

Feedback interactable: highlight warna pada objek (visual), dan SFX
hanya saat objek benar-benar diambil/dipakai. Tidak pakai ikon prompt
melayang.

## 4.3 Hiding (NEW - OPTIONAL)

Player bisa bersembunyi HANYA di kolong meja (under desk). Tidak ada
locker, tidak ada kamar mandi, tidak ada lemari.

- Saat hiding: player tidak bisa bergerak, noise player = 0, audio jadi
  teredam (low-pass).

- Monster yang sedang SEARCH tidak bisa menemukan player yang sedang
  hiding, kecuali player masuk hiding saat monster sudah dalam state
  CHASE dan melihat langsung.

- Senter otomatis diredupkan/disembunyikan saat hiding (secara fiction:
  Eddie menutupi lampunya).

- Battery tetap berkurang saat hiding --- supaya hiding bukan tempat
  aman tak terbatas.

# 5. Light & Battery System

Ini sistem inti game. Namanya saja Lights In, Lights Out.

## 5.1 Aturan dasar

  ------------------------------------------------------------------------
  **Parameter**          **Nilai**       **Catatan**
  ---------------------- --------------- ---------------------------------
  **Durasi satu          180 detik       Real-time, bukan game-time.
  battery**                              

  **Pengisian**          Full refill     Memasang battery selalu
                                         mengembalikan ke 100%, sisa lama
                                         dibuang.

  **Slot cadangan**      1 slot          Total bawaan: 1 terpasang + 1
                                         cadangan.

  **Drain saat idle**    Tetap berjalan  Waktu berjalan absolut. Diam
                                         tidak menghemat battery.

  **Drain saat hiding**  Tetap berjalan  Hiding tidak menghentikan tekanan
                                         waktu.

  **Bisa dibuang/ditaruh TIDAK           Tidak menambah value gameplay.
  ulang**                                

  **Ambil saat dikejar** BOLEH           Tanpa syarat tambahan --- ini
                                         bagian dari tension.

  **Slot penuh + nemu    Tidak bisa      UI slot menunjukkan kapasitas 1,
  battery**              diambil         jadi player paham tanpa perlu
                                         dijelaskan.
  ------------------------------------------------------------------------

## 5.2 Light State (tiered)

  -------------------------------------------------------------------------
  **Sisa         **State**     **Efek visual**         **Efek lain**
  battery**                                            
  -------------- ------------- ----------------------- --------------------
  **100--30%**   Normal        Radius senter penuh     ---

  **30--10%**    Flickering    Cahaya berkedip         SFX kedip. Ini
                               sesekali                sinyal peringatan
                                                       buat player.

  **10--0%**     Critical      Radius menyempit        Tombol aksi mulai
                               bertahap                menampilkan opsi
                                                       pasang battery.

  **0%**         Compact       Vignette kecil (±10%    TIDAK mati. Player
                 Darkness      ukuran normal), mirip   tetap bisa bergerak,
                               Among Us saat listrik   mencari, dan lolos.
                               mati                    
  -------------------------------------------------------------------------

## 5.3 Sumber Battery (environmental storytelling)

Battery didapat dari barang elektronik di kantor. Sebagian barang
sengaja TIDAK memberi battery --- ini bikin eksplorasi terasa seperti
mencari, bukan menyapu.

  -----------------------------------------------------------------------
  **Memberi battery**                 **Tidak memberi battery**
  ----------------------------------- -----------------------------------
  Emergency radio                     Keyboard

  Smoke detector                      Monitor mati

  Remote control                      Printer

  Senter cadangan di loker meja       Telepon kabel
  -----------------------------------------------------------------------

Barang-barang ini juga jadi kendaraan cerita: Eddie adalah orang yang
selalu siap siaga, jadi masuk akal kalau di mejanya ada P3K, makanan
darurat, dan lampu emergency. Penempatan battery sebaiknya mendukung
hint naratif di Bab 8.

# 6. Monster AI Specification

Satu jenis monster. Tidak ada multiple enemy types.

## 6.1 State Machine

PATROL → INVESTIGATE → CHASE → SEARCH → PATROL

  ---------------------------------------------------------------------------
  **State**         **Perilaku**                  **Transisi keluar**
  ----------------- ----------------------------- ---------------------------
  **PATROL**        Bergerak antar waypoint tetap → INVESTIGATE saat
                    yang sudah ditentukan di      mendeteksi noise player di
                    level design. Kecepatan       dalam noise radius.
                    normal. SFX nyaris hening.    

  **INVESTIGATE**   Bergerak menuju titik sumber  → CHASE kalau player
                    noise. Belum tahu posisi      terdeteksi lagi di jarak
                    player sebenarnya. SFX cue    dekat. → PATROL setelah
                    halus.                        durasi investigate habis
                                                  tanpa deteksi baru.

  **CHASE**         Mengejar posisi terakhir      → SEARCH setelah kehilangan
                    player yang diketahui (last   jejak selama durasi chase.
                    known position), BUKAN posisi 
                    aktual real-time. SFX         
                    intensitas penuh.             

  **SEARCH**        Berkeliling di radius kecil   → CHASE kalau player
                    sekitar last known position.  terdeteksi lagi. → PATROL
                                                  setelah durasi search
                                                  habis.

  **CATCH**         Monster menyentuh player.     → Death sequence → −1 life
                    Langsung tertangkap, tanpa    → respawn di awal floor.
                    QTE, tanpa kesempatan lolos.  
  ---------------------------------------------------------------------------

## 6.2 Nilai per state

  ------------------------------------------------------------------------
  **Parameter**        **Floor 51** **Floor 50** **Catatan**
  -------------------- ------------ ------------ -------------------------
  **Investigate        4 detik      6 detik      Lama monster memeriksa
  duration**                                     sumber suara.

  **Chase hold (last   3 detik      5 detik      Lama monster terus
  known)**                                       mengejar setelah
                                                 kehilangan jejak.

  **Search duration**  6 detik      8 detik      Sebelum kembali ke
                                                 PATROL.

  **Patrol speed**     1.0×         1.2×         Relatif terhadap walk
                                                 speed player.

  **Chase speed**      1.4×         1.5×         Harus di bawah sprint
                                                 player (1.6×) supaya lari
                                                 selalu punya harapan.
  ------------------------------------------------------------------------

**Aturan keras: kecepatan chase monster TIDAK BOLEH melebihi sprint
speed player. Kalau monster lebih cepat dari sprint, chase berubah dari
menegangkan jadi hukuman yang tidak bisa dihindari.**

## 6.3 Presence & Spawn

- Monster selalu eksis di map sejak floor dimulai (Floor 51 & 50). Tidak
  pernah 'muncul tiba-tiba' di dekat player.

- Floor 52 tidak punya monster sama sekali --- hanya SFX dari kejauhan
  sebagai hint dan pembangun tension.

- Posisi awal monster diambil acak dari beberapa preset spawn point yang
  sudah divalidasi, bukan acak bebas.

**Syarat valid sebuah Monster Spawn Point:**

- Terjangkau secara navigasi (tidak terjebak di ruang tertutup)

- Cukup jauh dari titik spawn player

- Di luar garis pandang awal player

- Tidak menempel pada objective (key, pintu turun)

- Tidak menciptakan instant-death atau situasi yang mustahil dihindari

# 7. Noise & Detection System

Tidak ada radar. Tidak ada minimap. Tidak ada indikator posisi monster.
Deteksi sepenuhnya berbasis suara.

## 7.1 Noise radius per aksi

  ------------------------------------------------------------------------
  **Aksi player**        **Noise         **Catatan**
                         radius**        
  ---------------------- --------------- ---------------------------------
  **Diam / hiding**      0               Sama sekali tidak terdeteksi.

  **Jalan**              1.0× (base)     Unit dasar noise.

  **Sprint**             3.0×            Sprint adalah alat darurat, bukan
                                         gerakan default.

  **Interact objek**     2.0×            Membuka pintu, mengambil barang.

  **Pasang battery**     1.5×            Kecil-sedang. Bikin isi ulang di
                                         dekat monster terasa berisiko.
  ------------------------------------------------------------------------

## 7.2 Aturan deteksi

- Monster hanya menerima informasi berupa titik lokasi suara, bukan
  identitas atau posisi real-time player.

- Deteksi terjadi kalau jarak monster ke player lebih kecil dari noise
  radius aksi yang sedang dilakukan player.

- Cahaya senter TIDAK memicu deteksi di versi ini. (Bisa ditambahkan
  pasca-jam kalau ternyata dibutuhkan.)

- Noise radius tidak pernah digambar di layar. Player merasakannya lewat
  audio cue monster saja.

# 8. Objective, Pintu & Progression

## 8.1 Tipe objective

  ------------------------------------------------------------------------
  **Floor**     **Objective**                       **Jumlah pintu
                                                    terkunci**
  ------------- ----------------------------------- ----------------------
  **Floor 52**  Survive & navigate. Cari battery,   0
                temukan pintu turun. Tanpa key.     

  **Floor 51**  Cari 1 key, buka pintu turun.       1

  **Floor 50**  Cari key untuk 3 pintu terkunci,    3 + Final Door
                lalu buka Final Door.               
  ------------------------------------------------------------------------

## 8.2 Feedback progression

Sengaja dibuat minimal. Tidak ada UI objective tracker, tidak ada quest
log.

- Pintu turun lantai dibedakan lewat WARNA yang berbeda dari pintu biasa
  --- ini satu-satunya penanda visual yang dibutuhkan.

- Saat player turun lantai, muncul splash text singkat: "Floor 51",
  "Floor 50".

- Di Floor 50 ada Final Door yang secara visual jelas berbeda dari pintu
  turun biasa.

- Tidak ada feedback berbeda antar tipe objective --- semua pakai SFX
  interaksi yang sama.

# 9. Lives, Checkpoint & Fail State

## 9.1 Aturan

  ------------------------------------------------------------------------
  **Parameter**        **Nilai**         **Catatan**
  -------------------- ----------------- ---------------------------------
  **Jumlah lives**     3                 Disembunyikan dari HUD selama
                                         gameplay.

  **Player diberi      Ya, sekali        Hanya di layar How To Play
  tahu?**                                sebelum game dimulai. Tidak
                                         pernah muncul lagi di HUD.

  **Checkpoint**       Awal tiap floor   Satu checkpoint per floor. Tidak
                                         ada checkpoint tengah floor.

  **Lives habis**      Bad Ending        Langsung memutar epilogue Bad
                                         Ending.
  ------------------------------------------------------------------------

## 9.2 Yang di-reset saat mati

Semua state floor dikembalikan ke kondisi awal floor, tanpa
pengecualian:

- Posisi player → kembali ke titik masuk floor

- Battery terpasang → reset ke 100%, slot cadangan dikosongkan

- Battery di map → kembali ke kondisi awal floor

- Key yang sudah diambil → dikembalikan ke posisi semula

- Pintu yang sudah dibuka → terkunci lagi

- Monster → kembali ke PATROL di salah satu preset spawn point

## 9.3 Fail State Flow

Monster menyentuh player

↓

Death sequence (animasi singkat, tanpa QTE)

↓

lives − 1 (tidak ditampilkan ke player)

↓

lives \> 0 ? ── ya → reset floor → respawn di awal floor

│

└──────── tidak → Bad Ending

Tidak ada perbedaan animasi/kamera antara tertangkap saat eksplorasi dan
tertangkap saat chase. Satu death sequence untuk semua kasus.

# 10. Narrative & Endings

## 10.1 Karakter: Eddie

Eddie adalah pekerja kantoran bergaji kecil dengan beban kerja yang
tidak masuk akal. Dia selalu dikejar pekerjaan sampai merasa tidak punya
ruang bebas untuk dirinya sendiri. Dia sampai rutin mendatangi psikolog.
Sifatnya: clumsy, sangat berhati-hati, tapi punya fokus tajam untuk
pekerjaannya --- saking hati-hatinya, dia menyimpan P3K, makanan
darurat, dan handheld emergency lamp di loker mejanya.

**Benang merah yang harus terasa: kantor yang mengejar Eddie dalam mimpi
adalah versi harfiah dari kantor yang mengejar Eddie dalam hidup nyata.
Ruang pandang yang menyempit = ruang hidup yang menyempit.**

## 10.2 Prologue (comic-style scene)

- Eddie diperkenalkan: lelah, selalu lembur, sangat siap siaga

- Eddie berangkat dan mulai bekerja

- Eddie kelelahan, tidur sebentar di meja

- Eddie bangun --- seluruh gedung gelap, dia panik

- Eddie ingat handheld emergency lamp di lokernya

- Setelah beberapa percobaan, lampunya menyala

- Eddie mendengar suara tawa aneh dari arah lorong → masuk gameplay

## 10.3 Foreshadowing (tersirat, tidak pernah dijelaskan eksplisit)

Sebar sebagian dari daftar ini di ketiga floor. Tidak perlu semua
dipakai --- yang penting konsisten dan tidak pernah dijelaskan lewat
teks.

- Suara monster menyerupai bel lift kantor

- Langkah kaki monster menyerupai suara atasan

- Tertawaan rekan kerja yang seolah mengejek Eddie, terdengar samar dari
  ruangan kosong

- Meeting room berisi kursi kosong semua

- Jam dinding selalu menunjuk waktu yang sama

- Komputer menyala menampilkan task yang belum selesai

- Printer mencetak surat resign berulang-ulang

- Office directory menampilkan nama Eddie yang berubah-ubah

## 10.4 Endings

  -------------------------------------------------------------------------
  **Ending**   **Trigger**     **Isi**                       **Status**
  ------------ --------------- ----------------------------- --------------
  **Good**     Berhasil keluar Eddie terbangun (benar-benar  FINAL
               lewat Final     terbangun) di sebuah kafe.    
               Door            Dia menyadari kehidupan       
                               kantornya selama ini toxic    
                               dan semua yang dia mimpikan   
                               adalah trauma kerjanya. Dia   
                               melihat surat resign yang     
                               sudah diajukan ke kantor lama 
                               sambil tersenyum, dan ada     
                               jadwal interview yang         
                               menunggu. Happy end.          

  **Bad**      Lives habis     Eddie mati dan menjadi salah  FINAL
                               satu entity --- terjebak di   
                               kantor itu selamanya.         

  **Secret**   ---             Belum dibahas. Tetap dibuka   DI LUAR SCOPE
                               sebagai kemungkinan           
                               pasca-jam.                    
  -------------------------------------------------------------------------

**Secret Ending tidak boleh mengambil waktu development sebelum Good dan
Bad ending selesai dan stabil.**

# 11. Visual & Art Pipeline

## 11.1 Arsitektur render: hybrid 2D + 3D

LILO memakai dua renderer sekaligus, dan ini keputusan yang disengaja:

  -----------------------------------------------------------------------
  **Elemen**         **Teknologi**        **Alasan**
  ------------------ -------------------- -------------------------------
  **Background &     SpriteKit 2D         Murah, cepat dibuat oleh tim
  environment**      (SKSpriteNode /      illustrator, gampang
                     tilemap)             di-iterate.

  **Karakter &       SceneKit 3D via      Player harus bisa tahu
  monster**          SK3DNode             karakternya menghadap ke arah
                                          mana. Model 3D menyelesaikan
                                          ini tanpa perlu membuat sprite
                                          untuk banyak arah.
  -----------------------------------------------------------------------

**Penanggung jawab model 3D: Fathia.**

Referensi visual: Playdead's Inside (gelap, vignette, satu sumber
cahaya) untuk mood; Sneaky Sasquatch untuk gaya kamera dan gerak
karakter.

## 11.2 Risiko teknis yang harus diuji di Fase 1

SK3DNode me-render scene SceneKit penuh setiap frame lalu
meng-compositing hasilnya ke SpriteKit. Ini bukan hal yang gratis, dan
ada dua hal yang secara historis merepotkan:

- Overhead performa di device target --- harus diukur langsung di
  iPhone, bukan di simulator.

- Sinkronisasi kamera 3D dan kamera 2D --- posisi, sudut, dan skala
  harus dijaga cocok secara manual supaya karakter tidak 'melayang' di
  atas background.

- zPosition / layering --- mengatur sprite 2D supaya bisa tampil di
  depan konten SK3DNode butuh perhatian khusus.

**Fallback yang sudah disepakati kalau SK3DNode ternyata terlalu mahal:
turun ke sprite 2D dengan jumlah arah paling sedikit yang masih bisa
mengkomunikasikan facing --- mulai dari 4 arah, atau bahkan 1 sprite
dengan rotate/flip.**

## 11.3 Aturan asset

- Tidak ada AI-generated asset --- berlaku untuk sprite 2D maupun model
  3D.

- Model 3D dibuat low-poly. Target minimal: 1 karakter (Eddie) + 1
  monster.

- Asset CC0 dari luar boleh dipakai untuk audio, dengan pencatatan
  lisensi (lihat Bab 16).

# 12. Lighting System

Karena game ini punya dua renderer, sistem pencahayaannya juga dua
pendekatan berbeda --- dan ini disengaja, bukan kelupaan.

  ---------------------------------------------------------------------------
  **Layer**         **Teknik**             **Detail**
  ----------------- ---------------------- ----------------------------------
  **2D              Fake vignette mask     SKCropNode + hole texture. Statis,
  (environment)**                          murah, predictable. Tidak ada
                                           perhitungan lighting sungguhan.

  **3D (karakter &  Lighting engine        SCNLight tipe spot + shadow map
  monster)**        sungguhan              dinamis. Memberi bayangan asli
                                           pada karakter dan monster.
  ---------------------------------------------------------------------------

## 12.1 Hal yang harus dijaga

- Radius vignette 2D dan jangkauan spotlight 3D harus terlihat menyatu.
  Kalau tidak dijaga, karakter akan terlihat seperti tempelan di atas
  background.

- Radius vignette berubah mengikuti Light State (lihat Bab 5.2).
  Spotlight 3D harus ikut menyusut bersamaan.

- Efek flicker HANYA muncul di state Flickering (30--10%). Jangan
  dipakai terus-menerus --- flicker konstan berubah jadi noise visual
  yang bikin lelah, bukan sinyal.

- Pada state Compact Darkness (0%), vignette menyusut ke sekitar 10%
  ukuran normal --- acuan visualnya: Among Us saat listrik mati.

# 13. Camera Specification

  -----------------------------------------------------------------------
  **Parameter**           **Keputusan**
  ----------------------- -----------------------------------------------
  **Proyeksi**            Orthographic

  **Sudut**               North facing, tilt 45 derajat (mengikuti
                          referensi Alien Shooter / Sneaky Sasquatch)

  **Follow**              Smooth follow (lerp), bukan fixed-snap

  **Posisi player**       Selalu di tengah layar

  **Boundary**            Clamping di tepi map --- kamera berhenti supaya
                          player tidak melihat area kosong di luar level

  **Zoom**                Tetap, tidak ada zoom dinamis di versi ini

  **Orientasi device**    Landscape
  -----------------------------------------------------------------------

# 14. Audio Design

Audio bukan pelengkap di LILO --- karena deteksi monster sepenuhnya
berbasis suara, audio adalah salah satu core mechanic. Silence adalah
senjata utama horror; jangan semua suara dibuat keras.

## 14.1 Daftar asset audio

  -----------------------------------------------------------------------
  **Kategori**      **Asset yang dibutuhkan**
  ----------------- -----------------------------------------------------
  **Ambient**       AC hum, electric buzz, nada kantor kosong

  **Player**        Footsteps walk, footsteps sprint (berbeda jelas),
                    breathing, SFX interaksi, SFX ganti battery

  **Monster**       Patrol sound, distant sound, investigation cue, chase
                    sound, attack/catch sound

  **Sistem**        SFX pickup, SFX pintu terkunci, SFX unlock, SFX kedip
                    lampu, splash text floor, UI button

  **Naratif**       Bel lift, langkah kaki menyerupai atasan, tertawaan
                    rekan kerja samar (lihat Bab 10.3)
  -----------------------------------------------------------------------

## 14.2 Dynamic mixing berdasarkan state monster

  -----------------------------------------------------------------------
  **State monster**    **Karakter audio**
  -------------------- --------------------------------------------------
  **PATROL / jauh**    Nyaris hening. Hanya ambient.

  **INVESTIGATE**      Cue halus --- cukup untuk membuat player waspada,
                       tidak cukup untuk memberi tahu posisi.

  **Dekat player**     Heartbeat / breathing masuk ke mix.

  **CHASE**            Intensitas penuh.
  -----------------------------------------------------------------------

## 14.3 Sumber audio

Kombinasi CC0 audio library dan asset yang dibuat sendiri oleh tim
designer. Setiap asset CC0 wajib dicatat (lihat Bab 16).

# 15. Controls & UX

## 15.1 Layout (landscape)

  ------------------------------------------------------------------------
  **Posisi**     **Kontrol**          **Fungsi**
  -------------- -------------------- ------------------------------------
  **Kiri**       Virtual joystick     Gerak. Didorong penuh = sprint.
                 360°                 

  **Kanan**      Tombol aksi          Semua interaksi (lihat Bab 4.2).
                 context-sensitive    

  **Pojok atas** Tombol pause         Membuka menu pause.

  **HUD**        Indikator battery +  Tidak ada indikator lives. Tidak ada
                 slot cadangan        minimap.
  ------------------------------------------------------------------------

**Keputusan: pakai virtual joystick, bukan Core Motion tilt. Tilt
ditolak karena kurang presisi untuk gerakan halus dan sprint mendadak
yang dibutuhkan game ini.**

## 15.2 Haptic feedback

Dipakai untuk memperkuat tension, memanfaatkan kekuatan platform iOS:

- Saat tertangkap monster

- Saat battery masuk state Critical / habis

- Saat pickup item

- Opsional: haptic mengikuti pola detak jantung saat monster mendekat

## 15.3 Parameter yang ditentukan saat development

Nilai berikut sengaja belum dikunci dan akan ditentukan sambil dicoba
langsung di device --- semuanya WAJIB dibaca dari config file, bukan
hardcoded:

- Diameter joystick, dead zone, opacity, posisi di layar

- Threshold dorongan joystick untuk memicu sprint

- Ukuran, posisi, dan radius sentuh tombol aksi

- Radius interaksi player terhadap objek

## 15.4 Onboarding

Tidak ada tutorial popup panjang. Floor 52 adalah tutorialnya, lewat
level design. Urutan pengenalan mekanik:

  ------------------------------------------------------------------------
  **Segmen Floor    **Yang dipelajari      **Caranya**
  52**              player**               
  ----------------- ---------------------- -------------------------------
  **Awal**          Gerak + senter         Ruangan aman, tidak ada
                                           ancaman, cahaya masih penuh.

  **Berikutnya**    Battery & resource     Battery pertama muncul, senter
                                           mulai turun ke state
                                           Flickering.

  **Berikutnya**    Ada sesuatu di gedung  SFX monster dari kejauhan.
                    ini                    Monster tidak pernah
                                           benar-benar muncul di floor
                                           ini.

  **Berikutnya**    Hiding                 Kolong meja ditempatkan di
                                           jalur yang pasti dilewati.

  **Menjelang       Noise & sprint         Lorong panjang yang mendorong
  akhir**                                  player untuk lari.
  ------------------------------------------------------------------------

Layar How To Play singkat sebelum game dimulai boleh ada, dan di situlah
satu-satunya tempat jumlah lives disebutkan.

# 16. Level Design Guidelines

## 16.1 Aturan dasar

- Map FIXED, bukan procedural. Keputusan ini final untuk versi jam.

- Setiap floor digambar manual di kertas dulu, sebelum dibangun di
  engine.

- Elemen random satu-satunya adalah posisi awal monster (dari preset).
  Layout, key, dan pintu selalu di tempat yang sama.

## 16.2 Blueprint template per floor

START (titik masuk / checkpoint)

↓

Safe Area ─ tanpa monster, tempat player orientasi

↓

Exploration Zone ─┐

│ ├─ Battery spawn points

│ └─ Monster patrol route

↓

Locked Door ← butuh key (Floor 51 & 50)

↓

Key Area

↓

Chase Section ─ lorong panjang, mendorong sprint

↓

Pintu turun lantai (warna berbeda) → floor berikutnya

## 16.3 Checklist validasi tiap floor

☐ Ada minimal satu rute alternatif --- jangan cuma satu jalur lurus

☐ Tidak ada dead-end yang bisa membuat player terjebak tanpa jalan
keluar saat dikejar

☐ Battery spawn point tersebar, tidak menumpuk di satu sisi map

☐ Patrol route monster melewati area objective, tapi tidak berdiri diam
di atasnya

☐ Hiding spot (kolong meja) ada di jalur yang masuk akal secara tata
ruang kantor

☐ Player bisa menyelesaikan floor dalam ±5 menit saat dimainkan orang
yang belum pernah main

☐ Tidak ada titik di mana player bisa melihat area kosong di luar level

# 17. GameConfig --- Master Tuning Table

**Keputusan tim: SEMUA angka di bawah ini wajib berada di satu file
config (GameConfig.swift atau .plist), tidak boleh hardcoded tersebar di
banyak file. Balancing dilakukan dengan mengubah file ini, bukan dengan
mencari-cari angka di seluruh codebase.**

## 17.1 Player

  ------------------------------------------------------------------------------
  **Key**                       **Nilai**      **Keterangan**
  ----------------------------- -------------- ---------------------------------
  **walkSpeed**                 1.0            Base unit

  **sprintMultiplier**          1.6            × walkSpeed

  **sprintJoystickThreshold**   TBD di device  Seberapa penuh joystick harus
                                               didorong

  **interactionRadius**         TBD di device  Jarak player ke objek agar bisa
                                               di-interact
  ------------------------------------------------------------------------------

## 17.2 Light & Battery

  ------------------------------------------------------------------------------
  **Key**                       **Nilai**      **Keterangan**
  ----------------------------- -------------- ---------------------------------
  **batteryDuration**           180            Detik, real-time

  **batterySlots**              1              Slot cadangan (total bawa 2)

  **lightStateFlickerStart**    30%            Mulai berkedip

  **lightStateCriticalStart**   10%            Mulai menyempit

  **compactDarknessRadius**     ±10% radius    Vignette saat 0% --- tidak
                                normal         mematikan player

  **batteryCountFloor52**       3--5           Statis, tanpa respawn

  **batteryMaxActiveFloor51**   2              Dengan respawn

  **batteryRespawnFloor51**     30             Detik

  **batteryMaxActiveFloor50**   1              Dengan respawn

  **batteryRespawnFloor50**     60             Detik
  ------------------------------------------------------------------------------

## 17.3 Noise

  ------------------------------------------------------------------------
  **Key**                 **Nilai**      **Keterangan**
  ----------------------- -------------- ---------------------------------
  **noiseBaseRadius**     TBD di device  Radius dasar untuk jalan

  **noiseWalk**           1.0×           

  **noiseSprint**         3.0×           

  **noiseInteract**       2.0×           

  **noiseBatterySwap**    1.5×           

  **noiseHiding**         0              Tidak terdeteksi
  ------------------------------------------------------------------------

## 17.4 Monster (per floor)

  ---------------------------------------------------------------------------
  **Key**                   **Floor     **Floor     **Keterangan**
                            51**        50**        
  ------------------------- ----------- ----------- -------------------------
  **monsterActive**         true        true        Floor 52 = false

  **patrolSpeed**           1.0×        1.2×        × walkSpeed player

  **chaseSpeed**            1.4×        1.5×        WAJIB \< sprintMultiplier
                                                    (1.6)

  **investigateDuration**   4           6           Detik

  **chaseHoldDuration**     3           5           Detik

  **searchDuration**        6           8           Detik
  ---------------------------------------------------------------------------

## 17.5 Progression

  --------------------------------------------------------------------------
  **Key**                   **Nilai**      **Keterangan**
  ------------------------- -------------- ---------------------------------
  **lives**                 3              Disembunyikan dari HUD

  **floorCount**            3              Floor 52, 51, 50

  **checkpointPerFloor**    true           Di awal floor

  **lockedDoorsFloor52**    0              

  **lockedDoorsFloor51**    1              

  **lockedDoorsFloor50**    3              \+ Final Door

  **targetFloorDuration**   300            Detik (±5 menit)
  --------------------------------------------------------------------------

# 18. Scope Lock

## 18.1 Must Have

- Movement + sprint + flashlight + interaction context-sensitive

- Battery system lengkap (1 terpasang + 1 cadangan, tiered light state,
  Compact Darkness)

- Battery respawn system untuk Floor 51 & 50

- 3 floor + doors + keys + checkpoint

- 1 monster dengan state PATROL, INVESTIGATE, CHASE, SEARCH, CATCH

- Noise radius detection system

- Hiding terbatas (kolong meja saja)

- Progression Floor 52 → 51 → 50 → Final Door

- Narrative: prologue + Good ending + Bad ending

- UX: pause, restart, audio settings, How To Play, feedback interaksi

- Audio: ambience, player SFX, monster SFX dinamis, chase SFX

- Haptic feedback

## 18.2 Cut List --- jangan dikerjakan

*Kalau ada yang mengusulkan salah satu dari ini di tengah development,
jawabannya sudah tertulis di sini: tidak, kecuali seluruh Must Have
sudah selesai dan stabil.*

- Inventory system yang kompleks

- Elaborate puzzle system

- Multiple monster / multiple enemy types

- Procedural map

- Hiding selain kolong meja (locker, kamar mandi, lemari)

- Combat, skill system, crafting

- Secret ending

- Branching narrative besar

- Multiple weapons/tools

- Deteksi lewat cahaya senter

- Stamina system

- Sprint yang menguras battery

- Floor ke-4

## 18.3 Kandidat potong pertama kalau waktu menipis

Urutan ini sudah disepakati di depan supaya keputusan sulit tidak
diambil dalam keadaan panik:

  -------------------------------------------------------------------------
  **Urutan**   **Yang dipotong**         **Dampak ke game**
  ------------ ------------------------- ----------------------------------
  **1**        Hiding system (kolong     Game tetap utuh. LILO jadi murni
               meja)                     chase/evasion.

  **2**        Battery respawn → ganti   Balancing jadi lebih kasar tapi
               statis                    tetap main.

  **3**        Floor 50 → game jadi 2    Durasi turun ke ±10 menit. Ending
               floor                     tetap jalan.

  **4**        Model 3D → sprite 2D 4    Visual kurang menarik, tapi
               arah                      gameplay tidak berubah.
  -------------------------------------------------------------------------

**Yang TIDAK boleh dipotong dalam kondisi apa pun: satu floor yang
benar-benar selesai dari start sampai exit, monster yang berfungsi, dan
kedua ending.**

# 19. Production Plan

## 19.1 Roles

  -----------------------------------------------------------------------
  **Nama**       **Role**           **Ownership sistem (usulan)**
  -------------- ------------------ -------------------------------------
  **Calzy**      Tech Lead ---      Arsitektur, GameConfig, integrasi
                 Coding             SK3DNode + SpriteKit, state
                                    management

  **Radit**      Coding             Monster AI state machine, noise
                                    detection

  **Fathia**     Design Lead ---    Model 3D (Eddie + monster), level
                 Coding &           design, design review
                 Illustrator        

  **Eca**        Coding &           Light/battery system, UI/HUD
                 Illustrator        

  **Eileen**     Illustrator        Environment art 2D, tileset kantor

  **Salwa**      Illustrator        Prologue & ending comic scene, UI art
  -----------------------------------------------------------------------

*Ownership di atas adalah usulan awal supaya tidak ada sistem yang
'tidak ada penanggung jawabnya'. Silakan disesuaikan di kickoff, tapi
pastikan setiap baris punya satu nama.*

## 19.2 Fase development

  ------------------------------------------------------------------------
  **Fase**        **Target**                **Definition of Done**
  --------------- ------------------------- ------------------------------
  **Fase 1 ---    Movement, kamera, senter, Player bisa berjalan di satu
  Core            battery, interaksi,       ruangan, senter menyala dan
  Prototype**     collision. Semua visual   habis, battery bisa diambil
                  boleh placeholder         dan dipasang. SK3DNode sudah
                  (kotak/kapsul).           diuji di iPhone asli.

  **Fase 2 ---    State machine lengkap +   Monster bisa patrol, mendengar
  Monster         noise detection di satu   sprint, investigate, chase,
  Prototype**     map kecil.                kehilangan jejak, lalu kembali
                                            patrol. Hiding sudah bisa
                                            diuji.

  **Fase 3 ---    Satu floor utuh dari      Orang yang belum pernah main
  Floor 52        start sampai pintu turun, bisa menyelesaikan Floor 52
  Lengkap**       termasuk onboarding       tanpa dijelaskan apa pun.
                  beats.                    

  **Fase 4 ---    Sisa floor, key & locked  Full run 3 floor bisa
  Floor 51 & 50** door, battery respawn,    diselesaikan tanpa crash.
                  monster agresif.          

  **Fase 5 ---    Prologue comic, Good      Kedua ending bisa dicapai dari
  Narrative &     ending, Bad ending,       gameplay, bukan lewat debug
  Ending**        splash text floor.        menu.

  **Fase 6 ---    Model 3D final,           Tidak ada lagi placeholder
  Art & Audio     environment art, semua    yang terlihat player.
  Pass**          SFX, lighting polish,     
                  haptic.                   

  **Fase 7 ---    Playtest eksternal, bug   Minimal 5 orang di luar tim
  Playtest &      fix berdasarkan           menyelesaikan satu full run.
  Polish**        prioritas.                

  **Fase 8 ---    Tidak ada fitur baru.     Satu full run diuji dari awal
  Submission      Build final.              sampai ending di build
  Lock**                                    terakhir.
  ------------------------------------------------------------------------

## 19.3 Prioritas bug fix

Crash → Softlock → Impossible state → Bad collision →

Monster bug → UI bug → Audio → Visual polish

## 19.4 Kebijakan AI & CC0

*Menggantikan kebijakan di GDD v1 ("AI-generated code to part that we
could not understand"), yang tidak aman dipakai saat harus debug cepat
menjelang deadline.*

- AI boleh membantu menghasilkan code, TAPI setiap code yang masuk ke
  main branch harus dipahami, diuji, dan bisa dijelaskan oleh anggota
  yang bertanggung jawab atas sistem tersebut.

- Tidak ada AI-generated asset (visual maupun 3D), sesuai constraint
  jam.

- Setiap asset CC0 yang dipakai dicatat di satu spreadsheet: nama asset,
  sumber, lisensi, status modifikasi, penanggung jawab integrasi, dan
  status penggunaan komersial.

# 20. Fase 1 --- Rencana Testing

Bagian ini adalah yang paling langsung dipakai minggu ini. Tujuan Fase 1
bukan membuat game yang seru --- tapi membuktikan bahwa fondasi
teknisnya berdiri.

## 20.1 Prototype scope (satu ruangan saja)

Bangun satu ruangan kotak, satu karakter, satu senter, satu battery,
satu pintu. Tidak ada monster, tidak ada art, tidak ada audio.

## 20.2 Yang WAJIB dibuktikan di Fase 1

  ------------------------------------------------------------------------
  **Pertanyaan teknis**  **Cara mengujinya**       **Lolos kalau...**
  ---------------------- ------------------------- -----------------------
  **Apakah SK3DNode      Taruh 1 karakter 3D + 1   Stabil di 60 FPS tanpa
  cukup cepat di iPhone  monster 3D di scene,      frame drop saat
  asli?**                jalankan di device (BUKAN bergerak.
                         simulator), pantau FPS.   

  **Apakah kamera 3D dan Gerakkan player keliling  Karakter tidak
  kamera 2D bisa         ruangan, perhatikan       melayang, tidak geser,
  disinkronkan?**        apakah karakter tetap     tidak berubah skala
                         'menempel' pada lantai    saat bergerak.
                         background.               

  **Apakah layering 2D   Taruh satu sprite 2D      Sprite bisa menutupi
  di atas SK3DNode bisa  (misal meja) yang harus   karakter dengan benar
  diatur?**              tampil di DEPAN karakter. lewat pengaturan
                                                   zPosition.

  **Apakah vignette 2D   Kecilkan radius vignette  Tidak ada karakter yang
  dan spotlight 3D       sampai state Compact      tetap terang di tengah
  terlihat menyatu?**    Darkness, lihat apakah    layar yang sudah gelap.
                         karakter ikut gelap.      

  **Apakah joystick      Minta 2--3 orang di luar  Tidak ada yang tidak
  terasa enak untuk      tim mencoba berjalan      sengaja sprint saat mau
  gerak halus + sprint   pelan lalu mendadak lari. jalan pelan.
  mendadak?**                                      

  **Apakah battery drain Jalan keliling ruangan    Tim merasa 180 detik
  terasa masuk akal di   sampai battery habis      terasa cukup menekan,
  180 detik?**           tanpa mengisi ulang.      tidak terlalu longgar.
  ------------------------------------------------------------------------

## 20.3 Definition of Done --- Fase 1

☐ Player bisa berjalan dan sprint dengan joystick di iPhone asli

☐ Karakter 3D tampil di atas background 2D dengan benar (posisi, skala,
layering)

☐ Senter menyala dan berkurang 180 detik secara real-time

☐ Keempat Light State (Normal / Flickering / Critical / Compact
Darkness) bisa terlihat perbedaannya

☐ Battery bisa diambil, masuk slot, dan dipasang lewat tombol aksi

☐ Slot cadangan menolak battery kedua saat sudah penuh, dan player paham
kenapa

☐ Pintu bisa dibuka lewat tombol aksi yang sama

☐ Semua angka tuning sudah dibaca dari GameConfig, bukan hardcoded

☐ FPS stabil di device target

## 20.4 Yang JANGAN dikerjakan di Fase 1

- Art asli --- semua tetap placeholder

- Audio --- belum dibutuhkan untuk membuktikan fondasi

- Monster --- itu Fase 2

- Level design beneran --- satu ruangan kotak sudah cukup

- Prologue / ending --- itu Fase 5

# 21. Open Items

Hal-hal yang belum dikunci, tapi TIDAK memblokir Fase 1. Perlu
diputuskan sebelum fase yang disebut di kolom kanan.

  -----------------------------------------------------------------------
  **Item**             **Kenapa belum             **Harus selesai
                       diputuskan**               sebelum**
  -------------------- -------------------------- -----------------------
  **Bagaimana Final    Butuh satu kalimat         Fase 5 (Narrative)
  Door di Floor 50     penghubung di prologue     
  menjelaskan Eddie    atau epilogue              
  akhirnya keluar                                 
  gedung**                                        

  **Parameter joystick Harus dirasakan langsung   Akhir Fase 1
  (diameter, dead      di device, tidak bisa      
  zone, opacity,       ditentukan di atas kertas  
  posisi)**                                       

  **Nilai              Bergantung pada skala map  Fase 2 (Monster)
  noiseBaseRadius**    yang belum dibuat          

  **Apakah hiding      Bergantung pada seberapa   Akhir Fase 2
  tetap masuk final    rapi state SEARCH bisa     
  build**              dibuat                     

  **Berapa banyak hint Bergantung pada kapasitas  Fase 6 (Art & Audio)
  naratif yang         illustrator setelah asset  
  benar-benar          utama selesai              
  dipasang**                                      

  **Secret ending**    Sengaja ditunda --- di     Pasca-jam
                       luar scope jam             
  -----------------------------------------------------------------------
