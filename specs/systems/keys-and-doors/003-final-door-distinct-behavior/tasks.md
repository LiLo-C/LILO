---

description: "Task list for 003-final-door-distinct-behavior"
---

# Tasks: Final Door Distinct Behavior

**Input**: Design documents from
`specs/systems/keys-and-doors/003-final-door-distinct-behavior/spec.md`

**Prerequisites**: `spec.md` (this feature; required). Depends on
`specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md` (`KeyInventory.HoldsKeyForDoor`)
and `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md` (`Door`, `DoorState`,
`DoorUnlockSystem` — this feature composes on top of, and does not modify, 002's system, per 002
tasks.md T013's structural guarantee). The configured Floor 50 required-key set is authored data
owned by the Floor 50 scene spec; the Good Ending transition itself is owned by
`specs/systems/progression-and-scene-flow/002-scene-transition-manager` and
`.../003-full-run-completion-tracking` — neither is redefined here.

**Tests**: Explicitly required — constitution Principle IV mandates EditMode tests for every
pure-logic piece. Eligibility, commit-before-transition ordering, and duplicate-completion
guards are 100% expressible as plain C# and MUST be covered. The Good Ending scene content and
the actual save/reload plumbing behind "remain valid after reload" are owned elsewhere and are not
re-tested here; this feature only needs to prove the completion flag itself does not self-clear.

**Organization**: This spec has a single user story (US1); tasks are grouped Setup →
Foundational → US1 → Polish, matching the granularity of
`specs/systems/keys-and-doors/001-key-pickup-and-inventory/tasks.md`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: Which user story this task belongs to (US1), or unlabeled for
  Setup/Foundational/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/KeysDoors/`
- MonoBehaviour adapters: `Assets/Scripts/MonoBehaviours/KeysDoors/`
- EditMode tests: `Assets/Tests/EditMode/Systems/KeysDoors/`
- Shared config asset: the configured Floor 50 required-key-id set is floor/level-authored data,
  not a new `GameConfig` field — this feature introduces none.

---

## Phase 1: Setup

**Purpose**: Confirm the shared folders this feature's files live in already exist (created by
002; re-run only if starting from a clean checkout).

- [ ] T001 Confirm `Assets/Scripts/Systems/KeysDoors/`, `Assets/Scripts/MonoBehaviours/KeysDoors/`,
  and `Assets/Tests/EditMode/Systems/KeysDoors/` exist (created by
  `specs/systems/keys-and-doors/002-locked-door-unlock-logic`); create them if starting from a
  clean checkout.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The completion-flag type and the eligibility rule every US1 task reads or extends.

**⚠️ CRITICAL**: T002–T003 block every task in Phase 3 onward.

- [ ] T002 [P] Create `Assets/Scripts/Systems/KeysDoors/CompletionState.cs`: a plain C# class (no
  `MonoBehaviour`) with `bool IsCompleted` defaulting to `false` and a `void Commit()` method that
  sets `IsCompleted = true` and is safe to call more than once (idempotent — calling it again
  leaves `IsCompleted` `true` with no error) (FR-003).
- [ ] T003 [P] Create `Assets/Scripts/Systems/KeysDoors/FinalDoorEligibility.cs`: a plain C# class
  exposing `bool IsEligible(IReadOnlyCollection<string> requiredDoorKeyIds, KeyInventory inventory)`
  that returns `true` only if `requiredDoorKeyIds` is non-empty and `inventory.HoldsKeyForDoor(id)`
  is `true` for every `id` in it; returns `false` for a `null` or empty set (defensive guard
  against a misconfigured/wrong-floor call site) (FR-001, depends on 001's `KeyInventory`).

**Checkpoint**: `CompletionState` and `FinalDoorEligibility` exist and compile — US1 work can
begin.

---

## Phase 3: User Story 1 - Know the Run Is Ending (Priority: P1) 🎯 MVP

**Goal**: The final door stays locked until every Floor 50 key is held, then opens exactly once,
commits completion before requesting the Good Ending transition, and never double-fires on
repeated or overlapping input.

**Independent Test**: Vary key count and press the final door; assert locked feedback, unlock
state, and exactly one transition (spec.md's own Independent Test).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no `FinalDoorSystem` yet) before implementing.

- [ ] T004 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/FinalDoorEligibilityTests.cs`, write: given fewer than
  the required keys held, `IsEligible` returns `false` (Acceptance Scenario 1, edge case "missing
  key").
- [ ] T005 [P] [US1] In the same file, write: given exactly all required keys held, `IsEligible`
  returns `true`.
- [ ] T006 [P] [US1] In the same file, write: holding extra keys for doors outside the required
  set neither helps nor hurts eligibility — only the exact required set is checked (SC-001's "no
  false-positive completion").
- [ ] T007 [P] [US1] In the same file, write: a `null` or empty `requiredDoorKeyIds` set (the
  "wrong floor"/misconfiguration edge case) makes `IsEligible` return `false` unconditionally,
  never `true` by vacuous truth.
- [ ] T008 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/FinalDoorCompletionTests.cs`, write: given an eligible
  inventory, `TryComplete` returns `true`, and `CompletionState.IsCompleted` is already `true` at
  the moment `TransitionRequested` fires — i.e. commit happens strictly before the transition
  request (FR-003, Acceptance Scenario 2).
- [ ] T009 [P] [US1] In the same file, write: a successful `TryComplete` raises `Completed` and
  `TransitionRequested` each exactly once (SC-002).
- [ ] T010 [P] [US1] In the same file, write: given an ineligible inventory (missing keys),
  `TryComplete` returns `false`, does not call `CompletionState.Commit()`, and raises neither event
  (Acceptance Scenario 1, "no false completion").
- [ ] T011 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/FinalDoorIdempotencyTests.cs`, write: calling
  `TryComplete` again after a prior successful completion (repeated input / duplicate touch) is a
  no-op — no second `Completed`/`TransitionRequested` event, `CompletionState.IsCompleted` stays
  `true` (Acceptance Scenario 3, edge case "duplicate touch").
- [ ] T012 [P] [US1] In the same file, write: calling `TryComplete` while a transition request is
  already in flight (before any external system resolves it) is also a no-op, distinct from but
  alongside T011's already-completed guard (edge case: "transition in progress").
- [ ] T013 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/FinalDoorPersistenceTests.cs`, write: once
  `CompletionState.IsCompleted` is `true`, querying it again later (with no explicit reset call)
  still returns `true` — proving the flag does not self-clear, modeling the "remain valid after
  reload" contract at the level this feature owns (FR-003; the actual save/reload plumbing is
  owned elsewhere).

### Implementation for User Story 1

- [ ] T014 [US1] Create `Assets/Scripts/Systems/KeysDoors/FinalDoorSystem.cs`: a plain C# class (no
  `MonoBehaviour`) exposing `event Action Completed`, `event Action TransitionRequested`, and
  `bool TryComplete(IReadOnlyCollection<string> requiredDoorKeyIds, KeyInventory inventory, CompletionState state)`
  that: no-ops and returns `false` if `state.IsCompleted` is already `true` or a transition is
  already in flight; otherwise checks `FinalDoorEligibility.IsEligible`, and on success calls
  `state.Commit()`, raises `Completed`, marks the internal in-flight flag, raises
  `TransitionRequested`, and returns `true`; on ineligibility raises neither event and returns
  `false` (depends on T002, T003).
- [ ] T015 [US1] Run T004–T013 against T014's implementation; fix until green.
- [ ] T016 [US1] Create `Assets/Scripts/MonoBehaviours/KeysDoors/FinalDoorInteractable.cs`: a thin
  `MonoBehaviour` adapter implementing the interactable contract from
  `specs/systems/interaction-and-highlight/001-nearest-interactable-detection` and
  `.../002-context-sensitive-action-button` — distinct from 002's `DoorInteractable` per FR-002's
  "visibly and behaviorally distinct from ordinary doors." On the action button firing, it calls
  `FinalDoorSystem.TryComplete` with the required-key-id list read from the floor's configured
  data (owned by the Floor 50 scene spec, out of scope here); on ineligibility it calls the
  "locked, N more keys needed" feedback hook call site; on `TransitionRequested` it calls the Good
  Ending scene-transition hook call site (owned by
  `specs/systems/progression-and-scene-flow/002-scene-transition-manager` and
  `.../003-full-run-completion-tracking`) (depends on T014).

**Checkpoint**: User Story 1 is fully functional and independently testable — the eligibility
matrix and the single-completion/single-transition guarantee are proven.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T017 [P] Add XML doc comments to `FinalDoorSystem.cs` and `CompletionState.cs` public members
  citing GDD 8.1/8.2/21 and this spec, noting that the Good Ending scene content and save/reload
  plumbing are out-of-scope consumers, and that
  `specs/systems/keys-and-doors/004-door-visual-and-color-feedback` is the intended reader of this
  feature's final-door state for presentation.
- [ ] T018 Run the full `Assets/Tests/EditMode/Systems/KeysDoors/` suite (including 002's
  regression suite) and confirm 100% green before marking this feature done (mirrors SC-001/SC-002).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies (folders already exist from 002 in the common case).
- **Foundational (Phase 2)**: Depends on Setup and on 001's `KeyInventory` — BLOCKS User Story 1.
- **User Story 1 (Phase 3)**: Depends on Foundational and on 002's `Door`/`DoorState` existing
  conceptually (this feature does not modify 002's `DoorUnlockSystem`, per T013 in 002's
  tasks.md).
- **Polish (Phase 4)**: Depends on User Story 1 being complete.

### Parallel Opportunities

- T002 and T003 can run in parallel.
- All [P] test-writing tasks within Phase 3 can run in parallel with each other.

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 — the eligibility matrix and single-completion/single-transition
   guarantee are proven, which is this feature's only story and its MVP.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently.

### Incremental Delivery

1. Setup + Foundational → `CompletionState`/`FinalDoorEligibility` shapes ready.
2. US1 → independently tested → eligibility, commit-before-transition ordering, and both
   idempotency guards all proven (unblocks spec 004's final-door presentation state).
3. Polish → full suite green, including the 002 regression suite.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task has a matching test task written and failing first, per constitution
  Principle IV.
- This feature introduces no new `GameConfig` fields — the required-key set is floor-authored
  data, not a tunable.
- This feature does not modify `DoorUnlockSystem` (002) — it composes on top of the eligibility
  and completion concerns 002 deliberately left out.
- Commit after each task or logical group; stop at the checkpoint to validate the story
  independently before moving on.
