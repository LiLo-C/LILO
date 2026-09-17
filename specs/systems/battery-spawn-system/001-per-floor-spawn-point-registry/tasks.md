---
description: "Task list for 001-per-floor-spawn-point-registry"
---

# Tasks: Per-Floor Battery Spawn Point Registry

**Input**: Design documents from `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md`

**Prerequisites**: spec.md (this folder). No `plan.md`/`research.md`/`data-model.md` exist for
this feature — the data shape is small enough to specify directly in `spec.md`'s Key Entities.

**Tests**: EditMode tests are mandatory per constitution Principle IV for every pure-logic piece
below — this feature is 100% pure C# logic, so it is 100% covered.

**Organization**: Tasks are grouped by user story (US1/US2/US3 from `spec.md`) so each story is
independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Confirm whether a shared floor identifier type already exists (owned by
  `shared-config-and-state` or `progression-and-scene-flow/001-floor-numbering-and-splash-text`
  per this spec's Assumptions). If it exists, note its namespace/path for use in T002+. If it
  does not exist yet, create a minimal placeholder `FloorId` enum (values: `Floor52`, `Floor51`,
  `Floor50`) in `Assets/Scripts/Systems/Shared/FloorId.cs`, clearly commented as
  "placeholder — replace with the shared-config-and-state definition once it lands."
- [ ] T002 [P] Create the folder `Assets/Scripts/Systems/BatterySpawn/` and
  `Assets/Tests/EditMode/Systems/BatterySpawn/` (no content yet — just establishes the
  location every task below writes into).

## Phase 2: Foundational — Data Shape (blocks all user stories)

**Purpose**: The `BatterySpawnPoint` struct/class is read and written by every story below; it
must exist first.

- [ ] T003 [US1] Implement `BatterySpawnPoint` in
  `Assets/Scripts/Systems/BatterySpawn/BatterySpawnPoint.cs`: plain C# type (no
  `MonoBehaviour`) with fields for identifier (string), `FloorId`, world position (`Vector3` is
  acceptable here — it is a plain value type, not a scene dependency), and an occupancy
  enum/bool (empty vs. occupied) defaulting to empty. No Unity lifecycle, no `Component`.

**Checkpoint**: Data shape compiles and is usable from an EditMode test with zero scene setup.

---

## Phase 3: User Story 1 - A Reliable List of Candidate Locations (Priority: P1) 🎯 MVP

**Goal**: A per-floor registry that returns exactly the hand-authored points for a floor, and
nothing for any other floor.

**Independent Test**: See spec.md User Story 1 — register 3 points for floor F, query F (get 3),
query an unrelated floor (get 0).

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T004 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/BatterySpawnPointRegistryTests.cs`:
  `GetSpawnPoints_ReturnsOnlyPointsRegisteredForThatFloor` — register points for two different
  floors, assert each floor's query returns only its own points.
- [ ] T005 [P] [US1] Same test file: `GetSpawnPoints_UnregisteredFloor_ReturnsEmptyList` —
  query a floor with nothing registered, assert an empty (not null, not throwing) list.
- [ ] T006 [P] [US1] Same test file: `ClearFloor_RemovesOnlyThatFloorsEntries` — register points
  for floors A and B, clear/rebuild floor A, assert floor A is now empty while floor B is
  untouched (covers Edge Case: no cross-floor leakage on floor load).

### Implementation for User Story 1

- [ ] T007 [US1] Implement `BatterySpawnPointRegistry` in
  `Assets/Scripts/Systems/BatterySpawn/BatterySpawnPointRegistry.cs`: plain C# class holding a
  per-floor collection (e.g., `Dictionary<FloorId, List<BatterySpawnPoint>>`); methods
  `Register(BatterySpawnPoint point)`, `GetSpawnPoints(FloorId floor)`, and
  `ClearFloor(FloorId floor)` (rebuild that floor's list to empty). Depends on T003.
- [ ] T008 [US1] Run T004–T006 and confirm green.
- [ ] T009 [P] [US1] Implement the scene-side adapter
  `Assets/Scripts/MonoBehaviours/BatterySpawn/BatterySpawnPointMarker.cs`: a `MonoBehaviour`
  placed on each hand-authored spawn point `GameObject` in a floor scene. On `Awake`/`OnEnable`
  it reads its own `Transform.position`, its assigned identifier and `FloorId` (serialized
  fields set by the level designer in the Inspector), and calls
  `BatterySpawnPointRegistry.Register(...)`. Contains no gameplay logic itself — pure adapter,
  per constitution Principle III.

**Checkpoint**: User Story 1 fully functional and independently testable — a floor scene with
markers populates the registry correctly, and unrelated floors are unaffected.

---

## Phase 4: User Story 2 - Tracking Which Points Are Currently Free (Priority: P2)

**Goal**: Occupancy state per spawn point, and a query for only the empty ones.

**Independent Test**: See spec.md User Story 2 — mark one of two points occupied, confirm the
"empty" query excludes it; mark it empty again, confirm it reappears.

### Tests for User Story 2

- [ ] T010 [P] [US2] EditMode test, same test file as T004:
  `NewlyRegisteredPoint_DefaultsToEmpty` — register a point, assert it appears in the "empty
  spawn points" query without any explicit occupancy call.
- [ ] T011 [P] [US2] `MarkOccupied_ExcludesPointFromEmptyQuery` — mark a registered point
  occupied, assert `GetEmptySpawnPoints(floor)` no longer includes it (but `GetSpawnPoints`
  still does).
- [ ] T012 [P] [US2] `MarkEmpty_ReturnsPointToEmptyQuery` — mark a previously-occupied point
  empty again, assert it reappears in the "empty" query.

### Implementation for User Story 2

- [ ] T013 [US2] Extend `BatterySpawnPointRegistry`
  (`Assets/Scripts/Systems/BatterySpawn/BatterySpawnPointRegistry.cs`) with
  `GetEmptySpawnPoints(FloorId floor)`, `MarkOccupied(FloorId floor, string pointId)`, and
  `MarkEmpty(FloorId floor, string pointId)`. Depends on T007.
- [ ] T014 [US2] Run T010–T012 and confirm green.

**Checkpoint**: User Stories 1 AND 2 both work independently — the registry is now a complete
read/write data contract for `002` and `003` to consume.

---

## Phase 5: User Story 3 - Authoring Mistakes Fail Loudly (Priority: P3)

**Goal**: Duplicate identifiers on the same floor are rejected, not silently duplicated.

**Independent Test**: See spec.md User Story 3 — register identifier "A" twice on floor F,
confirm the registry keeps exactly one live entry and logs/flags the conflict.

### Tests for User Story 3

- [ ] T015 [P] [US3] EditMode test, same test file:
  `Register_DuplicateIdOnSameFloor_RejectsSecondEntry` — register id "A" on floor F twice,
  assert floor F's list still contains exactly one entry for "A" after both calls.
- [ ] T016 [P] [US3] `Register_SameIdOnDifferentFloors_BothAccepted` — register id "A" on floor
  F and floor G, assert both are present (uniqueness is per-floor, not global).

### Implementation for User Story 3

- [ ] T017 [US3] Update `Register` in `BatterySpawnPointRegistry.cs` to check for an existing
  entry with the same identifier on the same floor before adding; if found, log a clear
  authoring-error message (e.g., via a simple `Debug.LogError`-equivalent injected logger so the
  plain C# class stays testable without `UnityEngine` in EditMode — or `Debug.LogError` directly
  if the project's EditMode test setup already tolerates that; confirm against existing test
  conventions before choosing) and return without adding a duplicate. Depends on T013.
- [ ] T018 [US3] Run T015–T016 and confirm green.

**Checkpoint**: All three user stories independently functional. Registry is feature-complete
per this spec.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T019 [P] EditMode test: `GetSpawnPoints_FloorWithZeroPoints_ReturnsEmptyNotError` —
  explicitly covers the Edge Case that a floor with zero authored points (e.g., Floor 52's
  intentional non-use of this registry) never throws.
- [ ] T020 Code review pass: confirm zero `UnityEngine.MonoBehaviour`/`Component` references
  anywhere in `BatterySpawnPoint.cs` or `BatterySpawnPointRegistry.cs` (constitution Principle
  III/IV compliance check — these two files must be pure C#).
- [ ] T021 Confirm `BatterySpawnPointMarker.cs` contains no gameplay rules of its own — it should
  only read its `Transform`/serialized fields and call into the registry.

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies.
- **Foundational (T003)**: Depends on Setup — blocks every user story.
- **User Story 1 (T004–T009)**: Depends on Foundational.
- **User Story 2 (T010–T014)**: Depends on User Story 1's registry existing (extends the same
  class) — implement after US1, though its tests can be drafted in parallel.
- **User Story 3 (T015–T018)**: Depends on User Story 1's `Register` method existing.
- **Polish (T019–T021)**: Depends on all user stories being complete.

## Notes

- This entire feature is plain C# — there is no MonoBehaviour-only logic exempt from EditMode
  testing per constitution Principle IV.
- `BatterySpawnPointMarker.cs` is the one file in this feature allowed to touch `UnityEngine`
  scene types (`Transform`, `MonoBehaviour`), and it is deliberately kept free of gameplay rules.
- Commit after each checkpoint, not after every single task.
