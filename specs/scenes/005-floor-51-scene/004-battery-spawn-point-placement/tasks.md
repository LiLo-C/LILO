---
description: "Task list for Floor 51 Battery Spawn Point Placement"
---

# Tasks: Floor 51 Battery Spawn Point Placement

**Input**: Design documents from
`specs/scenes/005-floor-51-scene/004-battery-spawn-point-placement/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry` (its FR-009 reserved Battery
Spawn Point slots must already exist in `Assets/Scenes/Floor51.unity`)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (point count,
collision/reachability, cap/timer behavior against these scene points). Placement spread/feel is
level-design judgment validated by manual walkthrough, per constitution Principle IV's live-scene
exception.

**Organization**: spec.md has a single user story (US1: "Recover light under pressure," up to two
battery spawn points). Tasks below break that story into the smallest independently-placeable/
verifiable units — one task per point, plus the shared-system wiring and reset behavior — per
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/tasks.md`'s target granularity.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/objects, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Tests/EditMode/LevelDesign/Floor51BatteryContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` (reuse the folder if a sibling Floor
      51 spec already created it)

**Checkpoint**: Test file exists for validation to reference.

---

## Phase 2: Foundational — Confirm Shared Config Wiring

- [ ] T002 Confirm `Assets/Scenes/Floor51.unity`'s floor identifier (per
      `specs/systems/shared-config-and-state/002-shared-game-state-and-manager`) resolves
      `GameConfig.batteryMaxActiveFloor51 = 2` and `GameConfig.batteryRespawnFloor51 = 30`
      (seconds) through `specs/systems/shared-config-and-state/001-game-config-schema/spec.md` —
      no literal override in this scene (spec.md FR-002)

**Checkpoint**: Floor 51's cap and respawn duration are provably config-driven before any point
is registered against them.

---

## Phase 3: User Story 1 — Battery Spawn Points (Priority: P1)

**Goal**: Up to two reachable, separated, hazard-free Battery Spawn Points registered for Floor
51, working with the shared max-2-active/30-second respawn rule, restored on reset.

**Independent Test**: Inspect points, collect batteries, and simulate respawn/cap behavior
(spec.md US1 Independent Test).

- [ ] T003 [P] [US1] Register Battery Spawn Point "1" (per
      `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md`'s data
      shape) at one of spec 001's reserved slots in `ExplorationZone`, positioned reachable and
      outside walls/doors/unsafe spawn zones (spec.md FR-001)
- [ ] T004 [P] [US1] Register Battery Spawn Point "2", same validity rules, at a reserved slot in
      a spatially distinct Area (e.g. `KeyArea` or `ChaseSection`) so the two points are separated
      and not clustered on one side of the floor (spec.md FR-001)
- [ ] T005 [US1] Audit both spawn points against their surrounding Area's wall/door collision
      geometry, repositioning either point found overlapping (spec.md FR-001, SC-001)
- [ ] T006 [US1] Wire Floor 51's registry adapter (per battery-spawn-system/001) to expose these
      two points to the shared per-floor registry, keyed to the Floor 51 identifier confirmed in
      T002
- [ ] T007 [US1] Manual walkthrough: confirm each point is reachable from `Checkpoint` and that
      the two points sit in different Areas of the floor, not clustered (spec.md SC-001)
- [ ] T008 [US1] Add an EditMode test in `Floor51BatteryContentTests.cs`: open
      `Assets/Scenes/Floor51.unity` additively and assert exactly 2 registered Battery Spawn
      Points exist for the Floor 51 identifier, each collision-free (spec.md FR-001, SC-001)
- [ ] T009 [US1] Add an EditMode test simulating
      `specs/systems/battery-spawn-system/003-respawn-timer-and-placement-rule/spec.md` against
      these two scene points: assert the max-2-active cap is respected, a 30-second elapsed timer
      spawns a `Battery` only at an empty, out-of-view point, and no spawn ever targets an in-view
      point (spec.md FR-002, SC-002)
- [ ] T010 [US1] Wire this floor's initial battery state (both points occupied by a `Battery` at
      floor load) into the floor-reset path (per
      `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`) so a reset
      restores both points to their initial occupied state and clears any in-progress respawn
      timer (spec.md FR-003)
- [ ] T011 [US1] Add an EditMode test: simulate collecting both batteries, one respawn cycle, then
      a floor reset, and assert both points return to their original initial occupied state
      (spec.md FR-003)
- [ ] T012 [US1] Playtest pickup/recovery pacing: collect both batteries during a chase, observe
      the 30-second respawn arriving at a reasonable pace, and record pacing notes (spec.md US1
      Independent Test)

**Checkpoint**: User Story 1 is independently complete — both points are valid, wired to the
shared cap/timer system, and correctly restored on reset.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T013 Once spec 001's dead-end audit (its `tasks.md` T013) is finalized, cross-check both
      spawn points against it, confirming neither sits inside an unresolved dead-end
- [ ] T014 Once spec 002's Monster Spawn Presets and Patrol Waypoints have landed, confirm neither
      Battery Spawn Point coincides with a patrol waypoint or spawn preset position
- [ ] T015 Record the T008/T009/T011 test results and the T012 playtest outcome in this feature's
      `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends on Setup/Foundational and on spec 001's reserved slots
  already existing. T003–T004 are `[P]` (different points); T005 onward depends on both existing.
- **Polish (Phase 4)**: after User Story 1; T013/T014 additionally depend on specs 001/002's own
  forward-looking audits landing.

```text
Setup (T001)
   ↓
Foundational (T002)
   ↓
US1 points (T003-T004, parallel)
   ↓
US1 wiring & validation (T005-T012)
   ↓
Polish (T013-T015, T013/T014 deferred)
```

## Notes

- [P] tasks (T003–T004) place independent spawn points with no dependency on an incomplete task.
- T013 and T014 are explicit forward-dependency tasks — mark them done only once the referenced
  sibling spec's audit/content exists, per the same pattern spec 001's own `tasks.md` uses for its
  T015 and Floor 50's parallel specs use for their own cross-spec re-checks.
- This spec has no new pure-logic C# — it registers instances against the data shape already
  owned by `specs/systems/battery-spawn-system/001` and consumes `002`/`003`'s existing cap/timer
  logic unchanged, and is otherwise Unity Editor placement in `Assets/Scenes/Floor51.unity`,
  validated by the manual walkthroughs/playtest above and the structural EditMode tests in
  T008/T009/T011, per constitution Principle IV's exception.
