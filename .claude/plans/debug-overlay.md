# Debug Overlay di Layar (light / battery / monster / noise)

## Tujuan

Satu overlay debug di layar yang menampilkan **level cahaya, baterai, status monster, dan noise
level** — bisa di-toggle saat main, dan otomatis hilang di release build.

## Keputusan (dari user)

- **Cara disable**: toggle runtime (tombol di HUD) **+** auto-strip di release build
- **Isi**: cahaya (state/intensitas/radius), baterai (% + spare), monster (state + jarak),
  noise (radius + sumbernya)
- **Struktur**: evolusi `LightDebugHud` → `DebugOverlay`

## Arsitektur (sesuai constitution Principle III)

- **Pure logic** → `Assets/Scripts/Systems/Debug/` — tanpa `MonoBehaviour`, EditMode-testable
- **Thin adapter (UI)** → `Assets/Scripts/UI/DebugOverlay.cs` — baca Unity object, panggil pure system
- **Test** → `Assets/Tests/Editor/DebugOverlaySystemTests.cs`
- Semua angka tetap dibaca dari `GameConfig` — tidak ada sumber config kedua

## Perubahan file

### 1. BARU — `Assets/Scripts/Systems/Debug/NoiseSource.cs`

```csharp
public enum NoiseSource { Silent, Walk, Sprint, Pulse, Hiding }
```

### 2. BARU — `Assets/Scripts/Systems/Debug/DebugOverlaySystem.cs`

- `struct DebugOverlaySnapshot` — `LightState`, `LightRadius`, `LightIntensity`,
  `BatteryFraction`, `SpareOccupied`, `SpareChargeFraction`, `MonsterAvailable`, `MonsterState`,
  `MonsterDistance`, `NoiseRadius`, `NoiseSource`, `IsHiding`
- `static NoiseReadout ClassifyNoise(GameConfig, isHiding, isMoving, isSprinting,
  largestPulseRadius)` → `(radius, source)`. Memakai `MonsterNoise.MovementRadius` untuk
  walk/sprint supaya angkanya identik dengan yang dipakai brain — bukan rumus kedua.
- `static string Format(DebugOverlaySnapshot)` → teks multi-baris:

```
LIGHT    Normal  r=220.0  i=1.00
BATTERY  84%  spare 100%
MONSTER  Chase  d=6.2
NOISE    3.0  sprint
```

### 3. BARU — `Assets/Scripts/UI/DebugOverlay.cs`

Menggantikan `LightDebugHud`. `sealed`, `[SerializeField] Text label`,
`[SerializeField] bool visibleOnStart = true`.

- `Awake()` — `#if !UNITY_EDITOR && !DEVELOPMENT_BUILD` → `gameObject.SetActive(false); return;`
  (kelas tetap dikompilasi di semua build supaya referensi scene tidak putus, tapi tidak jalan)
- `Update()` — ambil `LightingRig` + `MonsterAIController`, susun snapshot, tulis `label.text`
- `public void Toggle()` — bolak-balik `label.enabled`, dipakai tombol HUD
- Monster tidak ada / tidak aktif → baris monster tampil `—`, bukan angka menyesatkan

### 4. DIHAPUS — `Assets/Scripts/MonoBehaviours/Flashlight/LightDebugHud.cs` (+ `.meta`)

GUID dipertahankan saat rename supaya referensi di scene `OfficeLevel1` tidak jadi
*missing script*.

### 5. UBAH — `Assets/Scripts/MonoBehaviours/Monster/MonsterAIController.cs`

Tambah 2 properti read-only, **tanpa mengubah perilaku**:

- `public float CurrentNoiseRadius { get; private set; }`
- `public NoiseSource CurrentNoiseSource { get; private set; }`

Diisi di `Update()` tepat setelah `moveRadius` dihitung (baris ~307).

### 6. UBAH — `Assets/Editor/SetupOfficeGameplay.cs`

- `EnsureDebugHud()` → `EnsureDebugOverlay()`: `sizeDelta` naik ke ~320x140 (4 baris),
  komponen `DebugOverlay`, plus tombol kecil `DebugToggleButton` di kanan atas yang memanggil
  `Toggle()` — pola sama dengan `JumpButton` di `SetupOfficeLevel1Mobile.cs`
- `Validate()`: `"LightDebugText"` → `"DebugOverlayText"`, tambah `"DebugToggleButton"`

### 7. BARU — `Assets/Tests/Editor/DebugOverlaySystemTests.cs`

EditMode test (constitution Principle IV) untuk:

- `ClassifyNoise`: silent saat diam, walk, sprint, hiding menang atas semua, pulse,
  largest-wins saat pulse > movement
- `Format`: tiap baris terisi benar, dan kasus monster unavailable

## Urutan kerja

1. Tambah `NoiseSource` + `DebugOverlaySystem` + test → jalankan EditMode test
2. Expose noise di `MonsterAIController`
3. Tambah `DebugOverlay` (UI) dengan guard release build
4. Hapus `LightDebugHud` (pertahankan GUID)
5. Update `SetupOfficeGameplay` (overlay + tombol toggle)
6. Jalankan menu `LILO/Setup Office Gameplay Slice` di Unity untuk rewire scene

## Verifikasi

- EditMode test hijau, termasuk `MonsterBrainTests` yang lama (bukti perilaku monster tidak berubah)
- Di Editor: overlay muncul, keempat baris terisi, tombol toggle mematikan/menyalakan
- Cek build release: overlay tidak muncul

## Catatan

- Scene `OfficeLevel1` mereferensikan komponen ini lewat GUID — itu sebabnya langkah 4
  mempertahankan GUID dan langkah 6 tetap dijalankan (setup-nya idempoten, akan menambah
  komponen kalau hilang).
- Overlay ini **debug-only**, bukan bagian dari HUD produksi (`game-shell-ui/004`).
