---
description: "Task list for 002-shared-game-state-and-manager"
---

# Tasks: Shared Game State and Manager

**Input**: Design documents from
`specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`

**Prerequisites**: spec.md (this folder). Depends on
`specs/systems/shared-config-and-state/001-game-config-schema/` for the `GameConfig` type
(new-run defaults) and the shared `FloorId` identifier — this feature does not redefine either.
No `plan.md`/`research.md`/`data-model.md` exist for this feature — the data shape is small
enough to specify directly in `spec.md`'s Functional Requirements.

**Tests**: EditMode tests are mandatory per constitution Principle IV for every pure-logic piece
below. `GameManager`'s Unity lifecycle (`Awake`, `DontDestroyOnLoad`, real scene loads) is the
Principle IV exception validated instead via the feature's manual scene-wiring check
(T012) — everything expressible as pure C# (the state shape, the three reset operations, the
duplicate-manager *detection logic* itself) MUST be and is covered by an EditMode test below.

**Organization**: Tasks are grouped by user story (US1/US2 from `spec.md`) so each story is
independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Create the folders `Assets/Scripts/State/`, `Assets/Scripts/MonoBehaviours/`, and
  `Assets/Tests/EditMode/State/` (no content yet — establishes the location every task below
  writes into).
- [ ] T002 [P] Confirm `Assets/Scripts/Config/FloorId.cs` (owned by
  `shared-config-and-state/001-game-config-schema`) exists and note its namespace for use in
  `GameState.currentFloor` below — this feature does not define a second floor identifier.

## Phase 2: Foundational — GameState Data Shape (blocks all user stories)

**Purpose**: The `GameState` plain-data type is read and mutated by every story below; it must
exist first.

- [ ] T003 [P] Define `Assets/Scripts/State/RunOutcome.cs`: enum `RunOutcome { InProgress,
  GoodEnding, BadEnding }`, plain C# (no `MonoBehaviour`).
- [ ] T004 [P] Define `Assets/Scripts/State/CheckpointId.cs` (or reuse an existing per-floor
  checkpoint identifier if `progression-and-scene-flow` already defines one — confirm before
  creating a duplicate per constitution Principle V): the per-floor checkpoint reference `FR-001`
  requires `GameState` to hold.
- [ ] T005 Define `Assets/Scripts/State/GameState.cs`: plain C# class (no `MonoBehaviour`, no
  `Component`) with exactly the FR-001 fields — `int lives`, `FloorId currentFloor`,
  `CheckpointId checkpoint`, a key-inventory collection, a battery-slot-occupancy field, `bool
  hidingState`, and `RunOutcome runOutcome`. No public setter that lets a consumer replace the
  whole object (FR-003). Depends on T002, T003, T004.
- [ ] T006 [P] EditMode test in `Assets/Tests/EditMode/State/GameStateTests.cs`:
  `NewGameState_AllFieldsHaveSafeInitialValue` — constructing a bare `GameState` yields a
  non-null, non-throwing value for every FR-001 field with zero scene setup.

**Checkpoint**: Data shape compiles and is usable from an EditMode test with zero scene setup.

---

## Phase 3: User Story 1 - State Survives Scene Changes (Priority: P1) 🎯 MVP

**Goal**: Exactly one manager owns `GameState` from Bootstrap onward, and its fields are
unchanged by a scene transition.

**Independent Test**: See spec.md User Story 1 — mutate a state fixture, load a second scene, and
compare every persisted field.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T007 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/State/GameManagerOwnershipTests.cs`:
  `GameManager_ExposesExactlyOneGameStateInstance` — after simulated initialization, exactly one
  `GameState` instance is reachable from `GameManager`.
- [ ] T008 [P] [US1] Same test file: `GameManager_StateFieldsUnchangedAcrossSimulatedSceneLoad` —
  mutate every FR-001 field on the owned `GameState`, invoke whatever hook simulates a scene
  transition, assert every field still holds its mutated value (SC-001).
- [ ] T009 [P] [US1] Same test file: `Consumers_CannotReplaceStateObject` — attempting to assign a
  new `GameState` instance from outside `GameManager` is prevented (compile-time via no public
  setter, or a runtime no-op/exception if a setter must exist for editor tooling) (FR-003).
- [ ] T010 [P] [US1] Same test file: `SecondManagerInstance_IsDetectedAndRejected` — constructing
  a second `GameManager`-equivalent while one already exists is detected and rejected/self-
  destructs, and the detection path itself is visible/logged (FR-005, SC-002 duplicate-manager
  audit). Exercise the detection logic as plain C# (extract it from `Awake` if needed) so it is
  EditMode-testable without a live scene.

### Implementation for User Story 1

- [ ] T011 [US1] Implement `Assets/Scripts/MonoBehaviours/GameManager.cs`: `MonoBehaviour` that
  owns a single `GameState` instance and exposes it read-only (no public replace) to consumers
  (FR-002, FR-003). Depends on T005.
- [ ] T012 [US1] Add persistent-singleton bootstrap behavior to `GameManager.cs`:
  `DontDestroyOnLoad` in `Awake`, plus the duplicate-detection guard from T010 wired into `Awake`
  so a real duplicate fails visibly in the Unity Editor/Console at development time, not only in
  a test (FR-005).
- [ ] T013 [US1] Add `GameManager` to `Assets/Scenes/Bootstrap.unity` per ROADMAP §0 ("GameManager
  ... lives in the Bootstrap scene, `DontDestroyOnLoad`"). Manual scene-wiring task — validated by
  the constitution Principle IV live-scene exception, not an EditMode test.
- [ ] T014 [US1] Run T007–T010 and confirm green.

**Checkpoint**: User Story 1 fully functional and independently testable — a single manager
persists and its state is provably unchanged by a scene transition.

---

## Phase 4: User Story 2 - Reset Boundaries Are Explicit (Priority: P2)

**Goal**: New-run, floor-restart, and ending operations each touch only their documented fields,
with deterministic ordering and no cross-contamination.

**Independent Test**: See spec.md User Story 2 — invoke each reset operation and assert its
documented field-by-field result.

### Tests for User Story 2

- [ ] T015 [P] [US2] EditMode test in `Assets/Tests/EditMode/State/GameStateResetTests.cs`:
  `StartNewRun_SetsEveryFieldToRunStartValue` — from a dirty fixture (every field mutated), invoke
  the new-run operation, assert every FR-001 field equals its documented run-start value (reading
  `lives`/`checkpointPerFloor` defaults from a `GameConfig` fixture, not a hardcoded literal,
  per `shared-config-and-state/001`'s single-source-of-truth).
- [ ] T016 [P] [US2] Same test file: `ResetForFloorRestart_ChangesOnlyFloorScopedFields` — from a
  fixture with `lives`, `currentFloor`, and `runOutcome` set, invoke the floor-restart operation,
  assert `currentFloor` and `runOutcome` are untouched while the floor-scoped fields (checkpoint,
  key inventory, battery slot, hiding state) return to their floor-start values (SC-003 "no
  unrelated field is changed"; GDD Ch. 9.2 scope).
- [ ] T017 [P] [US2] Same test file: `CompleteRun_SetsRunOutcomeAndFreezesDocumentedFields` —
  invoke the ending operation with each `RunOutcome` value, assert `runOutcome` updates and any
  field the operation's contract documents as "frozen" is provably unchanged.
- [ ] T018 [P] [US2] Same test file: `ResetForFloorRestart_BeforeAnyFloorEntered_RejectedOrIgnored`
  — invoke the floor-restart operation on a fresh, never-started `GameState`, assert it is
  rejected or safely ignored per its documented contract, never throwing an unhandled exception or
  corrupting state (Edge Case).
- [ ] T019 [P] [US2] Same test file:
  `TransitionRequestedDuringEnding_RejectedOrIgnored` — with `runOutcome` already terminal
  (`GoodEnding`/`BadEnding`), invoke a scene-transition/new-floor request, assert it is rejected or
  safely ignored per its operation contract (Edge Case).
- [ ] T020 [P] [US2] EditMode test in `Assets/Tests/EditMode/State/GameManagerOwnershipTests.cs`:
  `RepeatedBootstrapInitialization_DoesNotCreateSecondManagerOrResetInProgressRun` — invoke the
  Bootstrap-initialization path twice against an already-owned `GameManager`, assert no second
  manager is created and an in-progress run's state is not silently reset (Edge Case, ties to
  FR-005).

### Implementation for User Story 2

- [ ] T021 [US2] Implement `GameState.StartNewRun(GameConfig config)` in
  `Assets/Scripts/State/GameState.cs`: deterministic field-write ordering, sets every FR-001 field
  to its run-start value using `config` (e.g. `lives = config.lives`) rather than a literal
  (FR-004). Depends on T005 and `shared-config-and-state/001`'s `GameConfig`.
- [ ] T022 [US2] Implement `GameState.ResetForFloorRestart()` in the same file: the floor-
  restart/death contract per GDD Ch. 9.2 scope — resets only the floor-scoped fields identified in
  T016, explicitly documented in an XML doc comment listing which fields change and which do not
  (FR-004).
- [ ] T023 [US2] Implement `GameState.CompleteRun(RunOutcome outcome)` in the same file: the
  ending contract — sets `runOutcome`, with an XML doc comment listing which fields are frozen
  going forward (FR-004).
- [ ] T024 [US2] Add the guard from T018 to `ResetForFloorRestart()`: reject/ignore a call before
  any floor has been entered, per an explicit documented contract rather than an implicit silent
  success (Edge Case).
- [ ] T025 [US2] Add the guard from T019 to whatever scene-transition entry point `GameManager`
  exposes: reject/ignore a transition request while `runOutcome` is already terminal (Edge Case).
- [ ] T026 [US2] Add the guard from T020 to `GameManager`'s Bootstrap-initialization path:
  re-initialization against an already-owned manager is a no-op for state purposes, not a second
  manager and not an implicit reset (Edge Case, FR-005).
- [ ] T027 [US2] Run T015–T020 and confirm green.

**Checkpoint**: User Stories 1 AND 2 both work independently — `GameState`/`GameManager` is now a
complete ownership-and-reset contract for every downstream systems spec to consume.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T028 [P] EditMode test in `Assets/Tests/EditMode/State/GameStateCrossSceneFixtureTests.cs`:
  `CrossSceneFixture_RetainsEveryFieldAcrossEveryPlannedScene` — a single fixture simulated through
  the full scene list from ROADMAP §0 (`Bootstrap` → `MainMenu` → `Prologue` → `Floor52` →
  `Floor51` → `Floor50` → `GoodEnding`/`BadEnding`), asserting every FR-001 field survives each
  simulated hop (SC-001, full matrix).
- [ ] T029 Code review pass: confirm `GameState.cs`, `RunOutcome.cs`, and `CheckpointId.cs`
  contain zero `UnityEngine.MonoBehaviour`/`Component` references (constitution Principle III);
  confirm `GameManager.cs` contains no gameplay-rule logic of its own — only lifecycle/ownership
  plus calls into `GameState`'s named operations (Principle III thin-adapter check).
- [ ] T030 Confirm no second `GameManager` and no static/global `GameState` exists anywhere in the
  codebase (constitution Principle III "no singletons or static mutable gameplay state outside
  GameConfig" audit).
- [ ] T031 Run the full Unity EditMode test suite under `Assets/Tests/EditMode/State/` and record
  the result in the release gate.

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies.
- **Foundational (T003–T006)**: Depends on Setup and on
  `shared-config-and-state/001-game-config-schema`'s `FloorId` — blocks every user story.
- **User Story 1 (T007–T014)**: Depends on Foundational.
- **User Story 2 (T015–T027)**: Depends on User Story 1's `GameManager`/`GameState` ownership
  existing (extends the same classes) — implement after US1, though its tests can be drafted in
  parallel.
- **Polish (T028–T031)**: Depends on both user stories being complete.

## Notes

- This feature's data shape and reset operations are plain C# — fully EditMode-testable per
  constitution Principle IV. Only `GameManager`'s real Unity lifecycle (`Awake` firing in an
  actual scene load, `DontDestroyOnLoad` surviving a real scene swap) falls under Principle IV's
  live-scene exception, validated by the manual scene-wiring task (T013) and this feature's
  `quickstart.md`-equivalent manual check, not an EditMode test.
- `GameState.cs` is the one file every reset operation lives on; `GameManager.cs` is the one file
  allowed to touch `UnityEngine` scene types (`MonoBehaviour`, `DontDestroyOnLoad`), and it is
  deliberately kept free of gameplay-rule logic beyond calling `GameState`'s named operations.
- Commit after each checkpoint (T014, T027), not after every single task.
