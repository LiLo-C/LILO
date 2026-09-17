# Phase 1 Data Model: LILO Phase 1 — Core Movement & Light System Prototype

All entities below are owned, directly or indirectly, by the single `GameState` root described
in [research.md](./research.md#2-shared-game-state) (constitution Principle III). None of this
is persisted — the whole model resets when the app process restarts (no Storage layer this
phase, see [plan.md](./plan.md#technical-context)).

## GameState (root aggregate)

| Field | Type | Notes |
|---|---|---|
| `player` | `PlayerCharacter` | single instance, this phase has one player |
| `installedBattery` | `Battery?` | the battery currently powering the flashlight; present from room start (spec Assumptions) |
| `spareBattery` | `Battery?` | `nil` until the room's loose battery is picked up |
| `worldBatteries` | `[Battery]` | the two loose batteries lying in the room; each is removed on pickup and never repopulates (FR-016). *(Alignment pass: was a single `worldBattery: Battery?`.)* |
| `placeholderMonster` | `PlaceholderMonsterFigure` | static, behavior-less 3D figure for perf/lighting checks (FR-019, FR-021) |
| `door` | `Door` | single instance |
| `room` | `TestRoom` | static bounds/spawn data, effectively read-only after load |

## PlayerCharacter

| Field | Type | Notes |
|---|---|---|
| `position` | 2D world position (drives both the `SKNode` and the derived `SK3DNode`/`SCNNode` transform) | see research.md §5 |
| `movementState` | `idle \| walking \| sprinting` | derived each frame from joystick deflection (FR-001, FR-002) |
| `facingDirection` | normalized 2D vector | drives which way the 3D character model faces |

No persisted identity needed — exactly one player exists this phase.

## Battery

| Field | Type | Notes |
|---|---|---|
| `charge` | `Double`, seconds remaining, `0...180` | drains in real time per FR-004 when this is the *installed* battery; frozen while spare or in-world |
| `location` | `.world \| .spare \| .installed` | see state transitions below |

### State transitions

```text
.world --(picked up, FR-007/FR-008)--> .spare --(installed, FR-009)--> .installed
```

- `.world → .spare` is legal once per loose battery (the room has exactly two, and FR-016
  forbids them ever repopulating). There is no transition back into `.world`.
- `.spare → .installed` is only legal while `installedBattery.charge ≤ lightStateCriticalStart ×
  batteryDuration` (≤18s / ≤10%, FR-009 — GDD 4.2/5.2). When legal, it always sets `charge = 180` (full refill) on the *new* installed battery
  and discards whatever was previously installed (FR-009) — the old installed battery is not
  moved to any other state, it is simply removed from the model.
- Pickup (`.world → .spare`) is only legal when `spareBattery == nil`; otherwise the attempt is
  rejected per FR-008 and that battery stays in `worldBatteries` unchanged.

## LightState (derived, not stored)

Computed each frame from `installedBattery.charge`, never stored independently — this keeps a
single source of truth and makes the four states impossible to desync from the charge value
(FR-005):

| Charge range | State | Visual effect |
|---|---|---|
| 180–54s (100–30%) | `.normal` | full radius |
| 54–18s (30–10%) | `.flickering` | visible flicker |
| 18–0s (10–0%, exclusive of 0) | `.critical` | radius visibly narrowing |
| 0s (0%) | `.compactDarkness` | radius fixed at ~10% of normal (FR-006) |

## Derived HUD values (not stored)

Read by `BatteryIndicatorView` (FR-017), computed each frame, never stored:

| Value | Formula | Notes |
|---|---|---|
| `batteryChargeFraction` | `installedBattery.charge / GameConfig.batteryDuration` | drives the bar's fill amount, `0.0...1.0` |
| `spareSlotOccupied` | `spareBattery != nil` | drives the spare-slot indicator's empty/full display |

## PlaceholderMonsterFigure

| Field | Type | Notes |
|---|---|---|
| `position` | 2D world position | fixed; solid scenery only, no AI, no catch (FR-019) |

## Door

| Field | Type | Notes |
|---|---|---|
| `position` | 2D world position | fixed, set by room layout |
| `isOpen` | `Bool`, default `false` | one-way transition to `true` via FR-010; no close action exists this phase |

## TestRoom

| Field | Type | Notes |
|---|---|---|
| `bounds` | rectangle in world space | drives camera clamping (FR-012) and player/character collision with walls |
| `playerSpawn` | 2D position | player starting position |
| `batterySpawns` | `[2D position]` (2 entries) | fixed spawn points for `worldBatteries` |
| `deskFrame` | rectangle | placeholder desk: occludes the player (FR-014) and blocks movement (FR-020) |
| `monsterFigurePosition` | 2D position | placement for the placeholder monster figure (FR-019) |
| `doorPosition` | 2D position | placement for `Door` |

Static for this phase — one hardcoded room, no level-loading system (Simplicity/YAGNI).

## Validation rules summary

- A pickup of any `worldBatteries` entry MUST be rejected whenever `spareBattery != nil` (FR-008).
- An install MUST be rejected (and not offered) while installed charge > 10% (FR-009).
- `installedBattery` MUST never be `nil` after room start (flashlight is always on per FR-004's
  "regardless of player action").
- A battery removed from `worldBatteries` MUST NOT be re-added (FR-016).
