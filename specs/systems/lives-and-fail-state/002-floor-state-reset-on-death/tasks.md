---

description: "Task list for lives-and-fail-state/002-floor-state-reset-on-death"

---

# Tasks: Floor State Reset on Death

**Input**: Design documents from `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/`

**Prerequisites**: [spec.md](./spec.md). No `plan.md`/`research.md`/`data-model.md` — this
feature has no new data shape of its own; it is pure orchestration over six sibling systems'
already-owned reset operations.

**Tests**: Included — constitution Principle IV (Test-Before-Done, NON-NEGOTIABLE). This entire
feature is plain C# orchestration with no rendering/physics/input dependency, so it is 100%
EditMode-testable. The single most important test list for this spec is the "everything listed
actually resets, nothing missed" itemized check in Phase 3 below — it is written before any
implementation, per spec.md User Story 1.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P2, P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project. Plain C# orchestration logic under `Assets/Scripts/Systems/FailState/` (no
`MonoBehaviour`/scene dependency, per constitution Principle III). Thin `MonoBehaviour` adapters
under `Assets/Scripts/MonoBehaviours/FailState/`. Tests under
`Assets/Tests/EditMode/FailState/`.

**Note on sibling systems**: several of this spec's dependencies
(`flashlight-and-battery/007-battery-pickup-and-spare-slot`,
`battery-spawn-system/001-per-floor-spawn-point-registry`,
`keys-and-doors/001-key-pickup-and-inventory`, `keys-and-doors/002-locked-door-unlock-logic`,
`monster-ai/001-state-machine-core-transitions`, `monster-ai/003-spawn-point-validation-rules`)
may not yet be implemented when this feature's tasks are picked up. Where a sibling's reset
method does not yet exist, create a minimal interface/stub matching that sibling's documented
contract (per spec.md Assumptions) and leave a `// TODO(spec-NNN): replace stub once <sibling>
lands` comment at the call site — never invent a second, competing reset behavior in this
spec's own files.

---

## Phase 1: Setup

- [ ] T001 Create the folders `Assets/Scripts/Systems/FailState/`,
  `Assets/Scripts/MonoBehaviours/FailState/`, and `Assets/Tests/EditMode/FailState/` (no content
  yet — establishes the location every task below writes into).
- [ ] T002 For each of the six sibling reset dependencies listed in spec.md's Requires section,
  confirm whether its reset operation already exists in the codebase. Record findings in a doc
  comment at the top of `Assets/Scripts/Systems/FailState/FloorResetOrchestrator.cs` (created in
  T006) noting which of the six are real calls vs. stubs as of this implementation pass.

---

## Phase 2: Foundational — Fixture Test Harness (blocks all user stories)

**Purpose**: Every user story below needs a way to mutate all six reset targets away from their
start-of-floor values and read them back, without a running scene.

- [ ] T003 Create `Assets/Tests/EditMode/FailState/FloorResetFixture.cs`: a test-only helper that
  constructs a minimal `GameState` + fixture instances of the six reset targets (installed
  `Battery`, a `BatterySpawnPointRegistry` with at least 2 points for a fixture floor, a
  held-key inventory with at least 2 keys, at least 2 `Door` instances, and a
  `MonsterStateMachine` + preset spawn point list for the fixture floor), each seeded with a
  known start-of-floor state and each independently mutable by test code.
- [ ] T004 [P] In `FloorResetFixture.cs`, add a `MutateAllAwayFromStartOfFloor()` helper that:
  drains the installed battery below 100% and fills the spare slot; marks two world battery
  spawn points occupied/depleted; picks up all fixture keys into the held-key inventory; unlocks
  and opens all fixture doors; and drives the `MonsterStateMachine` into `Chase` at a non-spawn
  position — leaving the player's tracked position away from the floor's checkpoint entry point
  too.
- [ ] T005 [P] In `FloorResetFixture.cs`, add an `AssertAllAtStartOfFloor()` helper that asserts,
  in one pass, that all six targets (player position, installed battery charge/spare slot, world
  battery layout, held-key inventory, door lock states, monster state/position) read exactly
  their start-of-floor value — this is the itemized "nothing missed" check every story below
  reuses.

**Checkpoint**: The fixture harness can mutate every reset target away from start-of-floor and
verify all six back to start-of-floor in one call; nothing under test yet.

---

## Phase 3: User Story 1 - One Call Resets Everything, With Nothing Missed (Priority: P1) 🎯 MVP

**Goal**: A single `ResetCurrentFloor` call performs all six resets from spec.md FR-001 as one
atomic unit.

**Independent Test**: See spec.md User Story 1 — mutate all six targets via the fixture, call
`ResetCurrentFloor`, assert all six read start-of-floor in one check.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T006 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/FailState/FloorResetOrchestratorTests.cs`:
  `ResetCurrentFloor_PlayerPosition_ReturnsToCheckpointEntryPoint` — move the fixture player away
  from the checkpoint, reset, assert position equals the stored checkpoint entry point (spec.md
  Acceptance Scenario 1).
- [ ] T007 [P] [US1] Same test file:
  `ResetCurrentFloor_InstalledBattery_ReturnsTo100PercentAndEmptySpareSlot` — drain the battery
  and fill the spare slot, reset, assert 100% charge and empty spare slot (Acceptance Scenario 2).
- [ ] T008 [P] [US1] Same test file: `ResetCurrentFloor_WorldBatteries_ReturnToStartOfFloorLayout`
  — deplete/occupy fixture spawn points, reset, assert the registry's occupancy matches the
  floor's start-of-floor layout (Acceptance Scenario 3).
- [ ] T009 [P] [US1] Same test file: `ResetCurrentFloor_Keys_ReturnToOriginalPositionsAndClearInventory`
  — pick up all fixture keys, reset, assert each key is back at its authored world position and
  the held-key inventory is empty (Acceptance Scenario 4).
- [ ] T010 [P] [US1] Same test file: `ResetCurrentFloor_Doors_RelockAndReclose` — unlock/open all
  fixture doors, reset, assert every door is locked and closed (Acceptance Scenario 5).
- [ ] T011 [P] [US1] Same test file: `ResetCurrentFloor_Monster_ReturnsToPatrolAtAPresetSpawnPoint`
  — drive the monster into `Chase` away from any spawn point, reset, assert
  `CurrentState == Patrol` and the monster's position is one of the floor's validated preset
  spawn points (Acceptance Scenario 6).
- [ ] T012 [US1] Same test file, using `FloorResetFixture.MutateAllAwayFromStartOfFloor()` +
  `AssertAllAtStartOfFloor()`: `ResetCurrentFloor_AllSixTargets_AllResetInOneCall` — the single
  itemized "everything listed actually resets, nothing missed" test combining all six checks in
  one assertion pass after one `ResetCurrentFloor()` call (spec.md SC-001). This is the spec's
  headline test.
- [ ] T013 [P] [US1] Same test file: `ResetCurrentFloor_NoOtherSystemObservesPartialState` — using
  a spy/callback hook on each of the six sibling reset calls (or, if no such hook is practical,
  a sequential-call-order assertion), confirm there is no intermediate point at which some
  targets are reset and others are not observable as still-mutated by a concurrent read (spec.md
  Acceptance Scenario 7). Given the synchronous single-threaded nature of these systems
  (constitution Principle III), this may reduce to asserting `ResetCurrentFloor` is a single
  non-yielding method call with no intermediate return point — document that reasoning in the
  test's comment if so.

### Implementation for User Story 1

- [ ] T014 [US1] Create `Assets/Scripts/Systems/FailState/FloorResetOrchestrator.cs`: plain C#
  class (no `MonoBehaviour`) with one public method,
  `ResetCurrentFloor(GameState state, GameConfig config)`, that:
  (a) reads the checkpoint via `CheckpointSystem.GetCurrentCheckpoint(state)` (from
  `lives-and-fail-state/001`) and sets the player's tracked position to it;
  (b) calls the installed-battery reset from `flashlight-and-battery/007`'s `Battery`/spare-slot
  system;
  (c) calls the world-battery-layout reset from `battery-spawn-system/001`'s
  `BatterySpawnPointRegistry`;
  (d) calls the key-inventory-and-world-position reset from `keys-and-doors/001`;
  (e) calls the door-relock reset from `keys-and-doors/002`;
  (f) calls `MonsterStateMachine.Reset()` (from `monster-ai/001`) and places the monster at a
  spawn point chosen by `monster-ai/003`'s selection logic.
  Each of (b)–(f) is a single delegating call — no reset logic is re-derived in this file
  (spec.md FR-002, SC-005). Depends on T003.
- [ ] T015 [US1] Run T006–T013 and confirm all green.

**Checkpoint**: User Story 1 is independently complete and tested — a single call correctly
resets all six GDD 9.2 items, verified individually and in the combined itemized check.

---

## Phase 4: User Story 2 - The Reset Is Idempotent and Order-Independent (Priority: P2)

**Goal**: Calling `ResetCurrentFloor` twice in a row is safe and produces the same result as once;
the internal call order across the six siblings does not affect the outcome.

**Independent Test**: See spec.md User Story 2 — double-call and compare state; call siblings in
two different orders across two fixture runs and compare state.

### Tests for User Story 2

- [ ] T016 [P] [US2] EditMode test, same test file as T006:
  `ResetCurrentFloor_CalledTwiceInARow_ProducesIdenticalState` — call `ResetCurrentFloor` once,
  snapshot all six targets, call it again immediately with no intervening mutation, snapshot
  again, assert the two snapshots are identical (spec.md Acceptance Scenario 1, SC-002).
- [ ] T017 [P] [US2] Same test file:
  `ResetCurrentFloor_SiblingCallOrderDoesNotAffectFinalState` — using two separately constructed
  fixtures mutated identically, invoke the six sibling resets in two different orders (e.g., via
  two orchestrator instances configured with reversed internal call sequences, or by directly
  invoking the six sibling reset calls in each order outside the orchestrator) and assert the
  resulting state is identical between the two orderings (spec.md Acceptance Scenario 2, SC-003).

### Implementation for User Story 2

- [ ] T018 [US2] Review `FloorResetOrchestrator.ResetCurrentFloor` (T014) and confirm none of the
  six delegating calls reads state written by another of the six within the same
  `ResetCurrentFloor` invocation — if any accidental ordering dependency is found, remove it so
  each call only depends on pre-reset state and its own sibling's contract.
- [ ] T019 [US2] Run T016–T017 and confirm green.

**Checkpoint**: User Story 2 independently complete — repeated/differently-ordered resets are
provably safe.

---

## Phase 5: User Story 3 - The Reset Targets Only the Active Floor's State (Priority: P3)

**Goal**: A reset never mutates a different floor's state, and never runs before any floor has
been entered.

**Independent Test**: See spec.md User Story 3 — populate fixture state for a "previous" floor and
the "current" floor, reset the current floor, assert the previous floor's state is untouched.

### Tests for User Story 3

- [ ] T020 [P] [US3] EditMode test, same test file:
  `ResetCurrentFloor_DoesNotMutatePreviouslyCompletedFloorsState` — populate fixture state for
  floor A ("previous, completed") and floor B ("current"), reset floor B, assert floor A's
  fixture state (world batteries, keys, doors, monster) is byte-for-byte unchanged (spec.md
  Acceptance Scenario 1, SC-004).
- [ ] T021 [P] [US3] Same test file: `ResetCurrentFloor_BeforeAnyFloorEntered_IsRejectedOrNoOp` —
  call `ResetCurrentFloor` on a fresh `GameState` that has never had a checkpoint set, assert it
  is rejected/no-ops rather than resetting an undefined floor (spec.md Acceptance Scenario 2,
  mirroring `lives-and-fail-state/001`'s checkpoint-before-floor-entry edge case).

### Implementation for User Story 3

- [ ] T022 [US3] In `FloorResetOrchestrator.ResetCurrentFloor`, add a guard: if
  `CheckpointSystem.GetCurrentCheckpoint(state)` has no valid floor set yet (no floor entered),
  return without performing any of the six resets (spec.md FR-007).
- [ ] T023 [US3] Confirm each of the six sibling reset calls in T014 is already scoped to the
  floor identifier read from the checkpoint (not a global/all-floors reset) — this should already
  be true by construction since each sibling system scopes its own state per floor (per their own
  specs), but explicitly verify by reading each sibling's reset method signature.
- [ ] T024 [US3] Run T020–T021 and confirm green.

**Checkpoint**: All three user stories independently functional and tested.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Code review pass: confirm `FloorResetOrchestrator.cs` contains zero
  `UnityEngine.MonoBehaviour`/`Component` references (constitution Principle III) and zero
  duplicated reset logic for any of the six items (constitution Principle II/V, spec.md SC-005) —
  every branch is a single delegating call into a sibling system.
- [ ] T026 [P] If any stub was created in T002/T014 for a sibling reset that did not yet exist,
  confirm the `// TODO(spec-NNN): replace stub once <sibling> lands` comment is present and
  accurately names the pending sibling spec.
- [ ] T027 Create the thin `MonoBehaviour` adapter placeholder
  `Assets/Scripts/MonoBehaviours/FailState/FloorResetTrigger.cs` with a doc comment marking it as
  the call site `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch/spec.md`
  will wire up to invoke `FloorResetOrchestrator.ResetCurrentFloor` on an actual death event —
  no gameplay logic in this file, just the documented hook (constitution Principle III).

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: depends on Setup; blocks every user story (all three read/write the
  same fixture harness).
- **User Story 1 (P1)**: depends only on Foundational. This is the MVP — the other two stories
  are safety/scoping guarantees layered on top of a working single reset.
- **User Story 2 (P2)**: depends on User Story 1's `FloorResetOrchestrator` existing (T014).
- **User Story 3 (P3)**: depends on User Story 1's `FloorResetOrchestrator` existing (T014);
  independent of User Story 2.
- **Polish (Phase 6)**: after all three stories.

## Notes

- T012 (`ResetCurrentFloor_AllSixTargets_AllResetInOneCall`) is this spec's single most important
  test — it is the direct automated expression of "everything listed actually resets, nothing
  missed" from the task brief, and of spec.md SC-001. Do not consider User Story 1 done without it
  green.
- Where a sibling system's reset method does not yet exist in the codebase at implementation
  time, stub it against that sibling's own documented spec contract (never invent new reset
  behavior here) and track the stub with a `TODO` comment per T026.
- Commit after each checkpoint, not after every single task.
