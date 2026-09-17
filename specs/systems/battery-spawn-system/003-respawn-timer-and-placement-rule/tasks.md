---
description: "Task list for 003-respawn-timer-and-placement-rule"
---

# Tasks: Respawn Timer and Placement Rule

**Input**: Design documents from `specs/systems/battery-spawn-system/003-respawn-timer-and-placement-rule/spec.md`

**Prerequisites**: spec.md (this folder). No `plan.md`/`research.md`/`data-model.md` exist for
this feature — the entities are small enough to specify directly in `spec.md`'s Key Entities.
This feature also depends on two already-complete sibling specs and MUST reuse their public APIs
rather than re-deriving them:
`battery-spawn-system/001-per-floor-spawn-point-registry` (`BatterySpawnPointRegistry`'s
`GetEmptySpawnPoints`/`MarkOccupied` and the shared `FloorId` type) and
`battery-spawn-system/002-active-battery-count-cap` (`BatteryCountCapPolicy`'s
`IsAtOrAboveCap`/`ActiveBatteryCountTracker`'s `Increment`).

**Tests**: EditMode tests are mandatory per constitution Principle IV for every pure-logic piece
below — this feature is 100% pure C# logic except its one thin `MonoBehaviour` adapter, so it is
effectively 100% covered.

**Organization**: Tasks are grouped by user story (US1/US2/US3 from `spec.md`) so each story is
independently implementable and testable, after a Foundational phase shared by all three.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Confirm the `FloorId` type location established by
  `001-per-floor-spawn-point-registry`'s T001 (`Assets/Scripts/Systems/Shared/FloorId.cs` or the
  shared-config-and-state equivalent, once it exists) and reuse it — do not define a second floor
  identifier type in this feature.
- [ ] T002 [P] Confirm `GameConfig`'s per-floor respawn duration fields exist per GDD Ch. 17.2
  (`batteryRespawnFloor51` = 30, `batteryRespawnFloor50` = 60) in `Assets/Config/GameConfig.asset`'s
  backing `ScriptableObject` class. If `shared-config-and-state/001-game-config-schema` has not
  landed these fields yet, add them directly to the existing `GameConfig` class as a minimal,
  additive change (constitution Principle V) rather than blocking this feature — mirroring how
  `002`'s T002 added the cap fields. Floor 52 MUST NOT get a respawn-duration field at all (FR-009
  — absence of config is the off-switch, not a zero/sentinel value).

## Phase 2: Foundational — Timer and Visibility Rule (blocks all user stories)

**Purpose**: `BatteryRespawnTimer` and `SpawnPointVisibilityRule` are the two pure building blocks
every story's spawn-decision path depends on; they must exist and be correct in isolation first.

### Tests for the foundation (write first, confirm they fail before implementing)

- [ ] T003 [P] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/BatteryRespawnTimerTests.cs`:
  `NotElapsed_WhenAdvancedLessThanFullDuration` — advance a timer by less than its configured
  duration, assert it reports not-elapsed (US1 AS2).
- [ ] T004 [P] Same file: `Elapsed_WhenAdvancedToExactlyFullDuration` — advance to exactly the
  configured duration, assert it reports elapsed (US1 AS3).
- [ ] T005 [P] Same file: `DoesNotAdvance_WhileAtOrAboveCap` — advance time while the "is below
  cap" input is false, assert elapsed time does not move (FR-001, US1 AS4).
- [ ] T006 [P] Same file: `ResetToFull_RestartsElapsedFromZero` — elapse the timer, call the
  reset operation, assert it reports not-elapsed again and requires the full duration to elapse
  again (FR-008).
- [ ] T007 [P] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/SpawnPointVisibilityRuleTests.cs`:
  `OutOfView_WhenDistanceStrictlyGreaterThanRadius` — plain position/radius inputs only (FR-004).
- [ ] T008 [P] Same file: `InView_WhenDistanceExactlyEqualsRadius` — the boundary case; distance
  equal to the radius MUST be treated as in-view, not eligible (US2 AS2, FR-004).
- [ ] T009 [P] Same file: `InView_WhenDistanceLessThanRadius`.

### Implementation for the foundation

- [ ] T010 [P] Implement `BatteryRespawnTimer` in
  `Assets/Scripts/Systems/BatterySpawn/BatteryRespawnTimer.cs`: plain C# class (no
  `MonoBehaviour`) holding a configured full duration, an elapsed-time accumulator, and an
  "elapsed" flag; an `Advance(float deltaTime, bool isBelowCap)` operation that only accumulates
  time when `isBelowCap` is true; and a `ResetToFull()` operation. Depends on T003–T006.
- [ ] T011 Run T003–T006 and confirm green.
- [ ] T012 [P] Implement `SpawnPointVisibilityRule` in
  `Assets/Scripts/Systems/BatterySpawn/SpawnPointVisibilityRule.cs`: a static pure function taking
  only plain position/radius values (e.g. `Vector3` spawn point position, `Vector3` player
  position, `float` view radius — `Vector3`/`float` are plain value types, not a scene dependency)
  and returning whether the point is out-of-view, using the strictly-greater-than rule from FR-004.
  No `UnityEngine.Camera`, `Light`, or rendering type appears anywhere in its signature. Depends
  on T007–T009.
- [ ] T013 Run T007–T009 and confirm green.

**Checkpoint**: The timer and the in-view test both compile and are individually correct with zero
scene setup, ready for every user story below to build on.

---

## Phase 3: User Story 1 - A New Battery Eventually Appears (Priority: P1) 🎯 MVP

**Goal**: The end-to-end timer → candidate-selection → spawn → registry/counter-mutation flow
works for the straightforward case (at least one eligible candidate available throughout).

**Independent Test**: See spec.md User Story 1 — cap = 1, exactly one empty out-of-view spawn
point, active count starts at 0, advance simulated time by the floor's configured duration, assert
exactly one spawn decision fires targeting that point.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T014 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/RespawnCandidateSelectorTests.cs`:
  `SelectsTheOnlyEligibleCandidate_WhenExactlyOneIsEmptyAndOutOfView`.
- [ ] T015 [P] [US1] Same file: `ReportsNoEligibleCandidate_WhenNoCandidateIsBothEmptyAndOutOfView`
  — feeds `003`'s own "not yet" branch (FR-005); the fuller waiting-state behavior built on top of
  this result is covered separately under User Story 3.
- [ ] T016 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/BatteryRespawnCoordinatorTests.cs`:
  `SpawnDecisionFires_WhenTimerElapsesWithOneEligibleCandidate` — the literal spec.md US1
  Independent Test (cap = 1, one empty out-of-view point, count starts at 0, advance by the
  configured duration, assert exactly one spawn decision targeting that point).
- [ ] T017 [P] [US1] Same file: `NoSpawnDecision_BeforeTimerReachesFullDuration` (US1 AS1/AS2).
- [ ] T018 [P] [US1] Same file: `TimerDoesNotAdvanceTowardSpawn_WhileFloorIsAtOrAboveCap` (US1 AS4,
  FR-001 — delegates the cap read to `002`'s `BatteryCountCapPolicy`).
- [ ] T019 [P] [US1] Same file: `SuccessfulSpawn_MarksSpawnPointOccupiedAndIncrementsCount_
  Synchronously` — asserts `001`'s registry and `002`'s tracker both reflect the change before the
  very next query (FR-007).
- [ ] T020 [P] [US1] Same file: `TimerRestartsFromFullDuration_AfterSuccessfulSpawn_WhenStillBelow
  Cap` (FR-008's restart-on-success clause).

### Implementation for User Story 1

- [ ] T021 [US1] Implement `RespawnCandidateSelector` (single/simple-candidate path) in
  `Assets/Scripts/Systems/BatterySpawn/RespawnCandidateSelector.cs`: given a floor's currently-empty
  spawn points (as returned by `001`'s `BatterySpawnPointRegistry.GetEmptySpawnPoints`) plus the
  player's current position and view radius, filters via `SpawnPointVisibilityRule` and returns
  either the single eligible candidate or a "no eligible candidate" result. Depends on T012.
- [ ] T022 [US1] Run T014–T015 and confirm green.
- [ ] T023 [US1] Implement `BatteryRespawnCoordinator` in
  `Assets/Scripts/Systems/BatterySpawn/BatteryRespawnCoordinator.cs`: plain C# class combining a
  `BatteryRespawnTimer`, a call into `002`'s `BatteryCountCapPolicy.IsAtOrAboveCap`, a call into
  `RespawnCandidateSelector`, and — on a successful spawn decision — calls `001`'s
  `MarkOccupied(floor, pointId)` and `002`'s `Increment(floor)` synchronously, then resets the
  timer per FR-008. Exposes one per-update decision method returning "spawn now at point X" or
  "not yet." Depends on T010, T021, and the completed `001`/`002` public APIs.
- [ ] T024 [US1] Run T016–T020 and confirm green.
- [ ] T025 [P] [US1] Implement the scene-side adapter
  `Assets/Scripts/MonoBehaviours/BatterySpawn/BatteryRespawnCoordinatorAdapter.cs`: a
  `MonoBehaviour` that owns the per-frame `Update` tick, reads the live player `Transform` position
  and the current light radius (sourced from
  `flashlight-and-battery/003-eased-radius-transitions`'s live value) each frame as plain inputs
  into `BatteryRespawnCoordinator`, and `Instantiate`s the `Battery` prefab in the scene only when
  the coordinator signals a spawn decision. Contains no gameplay rules of its own — pure adapter,
  per constitution Principle III. Depends on T023.

**Checkpoint**: User Story 1 fully functional and independently testable — the single-candidate
happy path spawns correctly, is cap-gated, mutates the registry/counter synchronously, and
restarts its timer only on an actual successful spawn.

---

## Phase 4: User Story 2 - Never In Front of the Player's Eyes (Priority: P2)

**Goal**: Prove the in-view exclusion and the uniform-random tie-break both hold once multiple
candidate spawn points are in play, not just the single-candidate case from User Story 1.

**Independent Test**: See spec.md User Story 2 — one in-view and one out-of-view empty spawn
point, run selection many times, assert the in-view point is never chosen.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T026 [P] [US2] EditMode test in `RespawnCandidateSelectorTests.cs`:
  `NeverSelectsTheInViewPoint_AcrossManyTrials_WithOneInViewAndOneOutOfView` — matches spec.md
  US2's Independent Test; the out-of-view point is chosen in 100% of trials (US2 AS1).
- [ ] T027 [P] [US2] Same file: `SelectionIsUniformlyRandom_AcrossManyEligibleCandidates` — with
  several simultaneously eligible candidates, over many trials no single candidate is chosen 0% or
  100% of the time (FR-006, SC-005).
- [ ] T028 [P] [US2] EditMode test in `SpawnPointVisibilityRuleTests.cs`:
  `IsOutOfView_SignatureTakesOnlyPlainPositionAndRadiusInputs` — a signature-level check confirming
  no `UnityEngine.Camera`/`Light`/rendering type is required to call the rule, so it is answerable
  from plain EditMode inputs alone (US2 AS3, FR-004).

### Implementation for User Story 2

- [ ] T029 [US2] Extend `RespawnCandidateSelector`
  (`Assets/Scripts/Systems/BatterySpawn/RespawnCandidateSelector.cs`) to filter the *entire*
  currently-empty candidate list (not just a single-candidate shortcut) through
  `SpawnPointVisibilityRule` and select uniformly at random among however many pass, using an
  injectable random source (so tests can seed/observe it deterministically) rather than calling
  `UnityEngine.Random` directly — keeping the class plain C#. Depends on T021.
- [ ] T030 [US2] Run T026–T028 and confirm green.

**Checkpoint**: User Stories 1 and 2 both hold — multi-candidate selection never leaks an in-view
point and is provably non-deterministic among the eligible set.

---

## Phase 5: User Story 3 - Every Empty Spawn Point Is Currently In View (Priority: P3)

**Goal**: Well-defined "wait and retry" behavior when zero eligible candidates exist at the moment
the timer elapses — no crash, no forced in-view spawn, no restart of already-elapsed progress, and
a clean recovery the instant a valid point appears.

**Independent Test**: See spec.md User Story 3 — every empty spawn point in view when the timer
elapses → no spawn, no error; then one point becomes out-of-view → spawns on the very next check
without the duration having restarted.

### Tests for User Story 3 (write first, confirm they fail before implementing)

- [ ] T031 [P] [US3] EditMode test in `BatteryRespawnCoordinatorTests.cs`:
  `NoSpawnAndNoError_WhenEveryEmptySpawnPointIsInViewAtElapse` (US3 AS1).
- [ ] T032 [P] [US3] Same file: `SpawnsOnNextCheck_WhenAPointBecomesOutOfView_WithoutRestartingThe
  ElapsedDuration` (US3 AS2, FR-005).
- [ ] T033 [P] [US3] Same file: `RemainsInCalmWaitingState_AcrossManyRepeatedChecksWithNoEligible
  Candidate` — no exception, no unbounded/duplicate warning state, no per-check performance
  degradation across a long run of repeated checks (US3 AS3).
- [ ] T034 [P] [US3] Same file: `NoSpawnAndNoError_WhenZeroEmptySpawnPointsExistDespiteBeingBelow
  Cap` — covers the registry/counter-inconsistency Edge Case; structurally the same "no eligible
  candidate yet" path as T031.
- [ ] T035 [P] [US3] Same file: `DoesNotRestartElapsedDuration_WhenEligibilityFluctuatesWithoutA
  SuccessfulSpawn` — covers the "player thrashes in and out of view of the only candidate" Edge
  Case; the timer's elapsed progress is untouched by eligibility flicker, only an actual spawn
  resets it (FR-008's non-restart clause).

### Implementation for User Story 3

- [ ] T036 [US3] Extend `BatteryRespawnCoordinator`
  (`Assets/Scripts/Systems/BatterySpawn/BatteryRespawnCoordinator.cs`) so that once the timer has
  elapsed with no eligible candidate, it holds the timer pinned at "elapsed" (never restarting it
  merely because eligibility fluctuated) and re-runs only the `RespawnCandidateSelector` check on
  each subsequent update, transitioning to a spawn — and only then resetting the timer per FR-008 —
  the moment an eligible candidate exists. Depends on T023, T029.
- [ ] T037 [US3] Run T031–T035 and confirm green.

**Checkpoint**: All three user stories independently functional. `BatteryRespawnCoordinator` is
feature-complete per this spec.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T038 [P] EditMode test in `BatteryRespawnCoordinatorTests.cs`:
  `NoTimerInstance_RunsForAFloorWithNoConfiguredRespawnDurationOrCap` — covers Floor 52 (and any
  other floor missing config) never running this system at all; the absence of config is the
  off-switch, not a special-cased floor check (FR-009).
- [ ] T039 [P] Code review / reference-scan task: confirm zero inbound references from
  `flashlight-and-battery/007-battery-pickup-and-spare-slot`'s pickup/install code to
  `BatteryRespawnTimer`, `SpawnPointVisibilityRule`, `RespawnCandidateSelector`, or
  `BatteryRespawnCoordinator` — record the check (e.g. a project-wide symbol search) confirming
  that system only calls generic `Battery` pickup/install APIs (SC-004, FR-010).
- [ ] T040 Code review pass: confirm zero `UnityEngine.MonoBehaviour`/`Component` references
  anywhere in `BatteryRespawnTimer.cs`, `SpawnPointVisibilityRule.cs`,
  `RespawnCandidateSelector.cs`, or `BatteryRespawnCoordinator.cs` (constitution Principle III/IV
  compliance check — these four files must be pure C#).
- [ ] T041 Confirm `BatteryRespawnCoordinatorAdapter.cs` contains no gameplay rules of its own — it
  should only read the live player position/view radius each frame, tick the coordinator, and
  `Instantiate` the `Battery` prefab on a signaled decision (FR-011).
- [ ] T042 [P] EditMode test in `BatteryRespawnCoordinatorTests.cs`:
  `RespawnDuration_IsReadFromGameConfigPerFloor_NeverAHardcodedLiteral` — swaps a fixture
  `GameConfig`'s Floor 51/Floor 50 duration values between assertions with no code change, mirroring
  `002`'s config-source-of-truth test (FR-002).
- [ ] T043 [P] EditMode test in `BatteryRespawnCoordinatorTests.cs`:
  `SpawnFiresExactlyOncePerElapsedDuration_AcrossManyRepeatedCyclesWithFloor51Defaults` — cap = 2,
  duration = 30 configured seconds, at least one eligible candidate available throughout; a spawn
  decision fires exactly once per elapsed duration across many repeated cycles, never early, never
  skipped (SC-001).

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies, but T002 should coordinate with
  `shared-config-and-state/001-game-config-schema` if it is in progress elsewhere, and confirm
  Floor 52 deliberately gets no respawn-duration field.
- **Foundational (T003–T013)**: Depends on Setup — blocks every user story.
- **User Story 1 (T014–T025)**: Depends on Foundational, and on `001`/`002`'s completed public
  APIs (already shipped).
- **User Story 2 (T026–T030)**: Depends on User Story 1's `RespawnCandidateSelector` existing —
  extends the same class.
- **User Story 3 (T031–T037)**: Depends on User Story 1's `BatteryRespawnCoordinator` and User
  Story 2's full-list selector.
- **Polish (T038–T043)**: Depends on all user stories being complete.

## Notes

- This feature is pure C# except for exactly one file
  (`BatteryRespawnCoordinatorAdapter.cs`) — every other file is fully EditMode-testable with zero
  scene setup, per constitution Principle IV.
- `BatteryRespawnCoordinatorAdapter.cs` is the one file in this feature allowed to touch
  `UnityEngine` scene/rendering types (`Transform`, the live light-radius value, `Instantiate`),
  and it is deliberately kept free of gameplay rules — it only ticks the coordinator and
  instantiates on its signal.
- This feature only ever *calls* `001`'s and `002`'s public APIs (`GetEmptySpawnPoints`,
  `MarkOccupied`, `IsAtOrAboveCap`, `Increment`) — it does not modify either of those already-shipped
  features' files.
- Commit after each checkpoint, not after every single task.
