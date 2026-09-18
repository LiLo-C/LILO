---
description: "Task list for Floor 50 Battery Spawn Point Placement"
---

# Tasks: Floor 50 Battery Spawn Point Placement

**Input**: Design documents from
`specs/scenes/006-floor-50-scene/005-battery-spawn-point-placement/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` (reserved Battery Spawn Point
slots across the Exploration Hub and the three loops, per its FR-009, must exist before this
feature's content can be placed), `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry`,
`002-active-battery-count-cap`, and `003-respawn-timer-and-placement-rule` (the shared systems
this scene's one spawn point is registered against)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (count audit,
cap/timer resolution against this scene's data, reset behavior). Whether the single spawn point's
position *feels* like meaningful risk/reward is level-design judgment validated by manual
playtest, per constitution Principle IV's live-scene exception.

**Organization**: This feature has a single P1 user story (US1). Because Floor 50 authors exactly
one Battery Spawn Point (GDD Ch. 3: max 1 active, rarer than Floor 51's 2), tasks are grouped by
Setup → Foundational → US1 rather than split per-point, but the point's placement, forbidden-zone
audit, registry wiring, and cap/timer confirmation are still separated into independently
completable units.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/LevelDesign/BatterySpawnPointMarker.cs`: a thin
      `MonoBehaviour` with `public string spawnPointId;` tagging a `GameObject` in
      `Assets/Scenes/Floor50.unity` as a candidate Battery Spawn Point (no gameplay logic —
      tagging only, mirroring `MonsterSpawnPresetMarker`'s pattern from spec 002), feeding
      `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry`'s scene-side
      adapter (its FR-009)
- [ ] T002 Create `Assets/Tests/EditMode/LevelDesign/Floor50BatteryContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` for this spec's structural/config
      tests

**Checkpoint**: Marker component and the test file exist for placement and validation to
reference.

---

## Phase 2: Foundational — Confirm Floor Identifier and Reserved Slots

**⚠️ MUST complete before the placement and config tasks below.**

- [ ] T003 In `Assets/Scenes/Floor50.unity`, confirm the scene's `GameManager`/floor-identifier
      configuration (per `shared-config-and-state/002-shared-game-state-and-manager`) is set to
      the Floor 50 identifier — this is what makes `GameConfig.batteryMaxActiveFloor50` and
      `batteryRespawnFloor50` resolve correctly for this scene (reuse spec 002's T004 confirmation
      if already done for this scene)
- [ ] T004 Confirm at least one Battery Spawn Point slot reserved by
      `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` FR-009 exists in the
      Exploration Hub or one of the three loops before placing content

**Checkpoint**: Floor 50's identifier resolves correctly and a reserved slot is available.

---

## Phase 3: User Story 1 - Manage Scarce Light (Priority: P1)

**Goal**: Exactly one valid Battery Spawn Point exists for Floor 50, registered against the
shared one-active/60-second-respawn rule, reachable but exposing meaningful risk.

**Independent Test**: Collect the battery, simulate time, and verify the cap, respawn, and reset.

### Implementation for User Story 1

- [ ] T005 [US1] Place the single Floor 50 Battery Spawn Point anchor in a reserved slot from
      spec 001 FR-009, tag its root `GameObject` with
      `BatterySpawnPointMarker(spawnPointId: "BatteryFloor50A")`
- [ ] T006 [US1] Audit the anchor's position against forbidden zones — it MUST NOT sit on or
      block a Key, Locked Door, the Final Door, or the Safe Area — and confirm it is reachable
      while exposing meaningful risk (not trivially safe), per spec.md FR-003
- [ ] T007 [US1] Confirm no second `BatterySpawnPointMarker` exists anywhere in
      `Assets/Scenes/Floor50.unity` (spec.md FR-001 — exactly one valid spawn point, no second
      active point)
- [ ] T008 [US1] Wire the scene's registry adapter to register this one marker under the Floor 50
      identifier via `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry`
- [ ] T009 [US1] Confirm the Floor 50 respawn coordinator resolves
      `GameConfig.batteryMaxActiveFloor50 = 1` (`002-active-battery-count-cap`) and
      `GameConfig.batteryRespawnFloor50 = 60` seconds (`003-respawn-timer-and-placement-rule`) for
      this scene's floor identifier — zero literal constants in this scene's own scripts (spec.md
      FR-002)
- [ ] T010 [US1] Add an EditMode test in `Floor50BatteryContentTests.cs` asserting exactly one
      `BatterySpawnPointMarker` is registered for the Floor 50 identifier and the registry query
      returns exactly 1 entry matching its authored position (spec.md SC-001 count audit)
- [ ] T011 [US1] Add an EditMode test simulating collecting the battery, advancing simulated time
      to less than 60s (asserting no respawn yet) and then to exactly 60s (asserting the one
      spawn point becomes eligible and the cap/timer fixtures pass with this scene's data, spec.md
      SC-002)
- [ ] T012 [US1] Add an EditMode test simulating a floor reset and confirming the spawn point's
      occupancy and the battery-in-map state fully return to their initial condition (spec.md
      FR-003, "reset to initial state")
- [ ] T013 [US1] Manual playtest: collect the battery, explore Floor 50 through a full 60-second
      respawn cycle, and confirm the scarcity/risk-reward pacing feels correct under Floor 50's
      aggressive monster tuning (spec.md US1 Independent Test)

**Checkpoint**: User Story 1 is independently complete — one validated spawn point exists,
correctly registered and cap/timer-wired, and survives a full reset.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends only on Setup/Foundational. T005 (placement) must precede
  T006–T009 (audit/registration/config confirmation), which must precede T010–T012 (tests), which
  must precede T013 (playtest).

```text
Setup (T001-T002)
   ↓
Foundational (T003-T004)
   ↓
Place anchor (T005)
   ↓
Audit + register + confirm config (T006-T009)
   ↓
Tests (T010-T012)
   ↓
Playtest (T013)
```

## Notes

- Unlike Floor 50's keys/doors (spec 003, three independent pairs) or hiding spots (spec 006,
  multiple spots), this feature places exactly one spawn point per GDD Ch. 3's Floor 50 column
  (max 1 active), so its tasks are separated by concern (placement, forbidden-zone audit,
  registry wiring, cap/timer confirmation) rather than by count.
- This spec has no pure-logic C# beyond the one marker tag component (T001) — the cap/timer logic
  itself belongs to `specs/systems/battery-spawn-system/002` and `003`; this scene only supplies
  data and confirms correct resolution (T009, T011).
