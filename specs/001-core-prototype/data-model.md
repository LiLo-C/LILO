# Phase 1 Data Model: LILO Phase 1 — Core Movement & Light System Prototype

All entities below are owned, directly or indirectly, by the single `GameState` root described
in [research.md](./research.md#2-shared-game-state) (constitution Principle III). None of this
is persisted — the whole model resets when the app process restarts (no Storage layer this
phase, see [plan.md](./plan.md#technical-context)). `GameState` and its nested types are plain
C# classes/structs with no `UnityEngine.MonoBehaviour`/`Component` dependency, per Principle III.

## GameState (root aggregate)

| Field | Type | Notes |
|---|---|---|
| `player` | `PlayerCharacter` | single instance, this phase has one player |
| `installedBattery` | `Battery?` | the battery currently powering the flashlight; present from room start (spec Assumptions) |
| `spareBattery` | `Battery?` | `null` until the room's loose battery is picked up |
| `worldBatteries` | `List<Battery>` | the two loose batteries lying in the room; each is removed on pickup and never repopulates (FR-016) |
| `placeholderMonster` | `PlaceholderMonsterFigure` | static, behavior-less figure for perf/lighting checks (FR-019, FR-021) |
| `door` | `Door` | single instance |
| `room` | `TestRoom` | static bounds/spawn data, effectively read-only after load |

## PlayerCharacter

| Field | Type | Notes |
|---|---|---|
| `position` | `Vector2` (world ground-plane position; a `PlayerRig` `MonoBehaviour` maps this to the `Transform`'s `(x, 0, y)` each frame) | see research.md §5 |
| `movementState` | `enum { Idle, Walking, Sprinting }` | derived each frame from joystick deflection (FR-001, FR-002) |
| `facingDirection` | normalized `Vector2` | drives which way the 3D character model faces |

No persisted identity needed — exactly one player exists this phase.

## Battery

| Field | Type | Notes |
|---|---|---|
| `charge` | `float`, seconds remaining, `0...180` | drains in real time per FR-004 when this is the *installed* battery; frozen while spare or in-world |
| `location` | `enum { World, Spare, Installed }` | see state transitions below |

### State transitions

```text
World --(picked up, FR-007/FR-008)--> Spare --(installed, FR-009)--> Installed
```

- `World → Spare` is legal once per loose battery (the room has exactly two, and FR-016 forbids
  them ever repopulating). There is no transition back into `World`.
- `Spare → Installed` is only legal while `installedBattery.charge ≤ lightStateCriticalStart ×
  batteryDuration` (≤18s / ≤10%, FR-009 — GDD 4.2/5.2). When legal, it always sets `charge = 180`
  (full refill) on the *new* installed battery and discards whatever was previously installed
  (FR-009) — the old installed battery is not moved to any other state, it is simply removed from
  the model.
- Pickup (`World → Spare`) is only legal when `spareBattery == null`; otherwise the attempt is
  rejected per FR-008 and that battery stays in `worldBatteries` unchanged.

## LightState (derived, not stored)

Computed each frame from `installedBattery.charge`, never stored independently — this keeps a
single source of truth and makes the four states impossible to desync from the charge value
(FR-005):

| Charge range | State | Visual effect |
|---|---|---|
| 180–54s (100–30%) | `Normal` | full radius |
| 54–18s (30–10%) | `Flickering` | visible flicker |
| 18–0s (10–0%, exclusive of 0) | `Critical` | radius visibly narrowing |
| 0s (0%) | `CompactDarkness` | radius fixed at ~10% of normal (FR-006) |

## Derived HUD values (not stored)

Read by `BatteryIndicatorUI` (FR-017), computed each frame, never stored:

| Value | Formula | Notes |
|---|---|---|
| `batteryChargeFraction` | `installedBattery.charge / GameConfig.batteryDuration` | drives the bar's fill amount, `0.0...1.0` |
| `spareSlotOccupied` | `spareBattery != null` | drives the spare-slot indicator's empty/full display |

## PlaceholderMonsterFigure

| Field | Type | Notes |
|---|---|---|
| `position` | `Vector2` | fixed; solid scenery only, no AI, no catch (FR-019) |

## Door

| Field | Type | Notes |
|---|---|---|
| `position` | `Vector2` | fixed, set by room layout |
| `isOpen` | `bool`, default `false` | one-way transition to `true` via FR-010; no close action exists this phase |

## TestRoom

| Field | Type | Notes |
|---|---|---|
| `bounds` | `Rect` in world space | drives camera clamping (FR-012) and player/character collision with walls |
| `playerSpawn` | `Vector2` | player starting position |
| `batterySpawns` | `Vector2[]` (2 entries) | fixed spawn points for `worldBatteries` |
| `deskFrame` | `Rect` | placeholder desk: occludes the player (FR-014) and blocks movement (FR-020) |
| `monsterFigurePosition` | `Vector2` | placement for the placeholder monster figure (FR-019) |
| `doorPosition` | `Vector2` | placement for `Door` |

Static for this phase — one hardcoded room, no level-loading system (Simplicity/YAGNI). Concrete
values (carried over from the equivalent native-iOS prototype's on-device tuning, since the room
layout itself is engine-independent): `bounds = (-600, -400, 1200, 800)`, `playerSpawn = (0,
-300)`, `batterySpawns = [(220, 60), (-220, 60)]`, `doorPosition = (0, 330)`, `deskFrame = (-5,
-205, 90, 50)`, `monsterFigurePosition = (-320, 220)`.

## Validation rules summary

- A pickup of any `worldBatteries` entry MUST be rejected whenever `spareBattery != null` (FR-008).
- An install MUST be rejected (and not offered) while installed charge > 10% (FR-009).
- `installedBattery` MUST never be `null` after room start (flashlight is always on per FR-004's
  "regardless of player action").
- A battery removed from `worldBatteries` MUST NOT be re-added (FR-016).
