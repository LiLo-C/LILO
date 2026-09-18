---

description: "Task list for 002-locked-door-unlock-logic"
---

# Tasks: Locked Door Unlock Logic

**Input**: Design documents from `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md`

**Prerequisites**: `spec.md` (this feature; required). Depends on
`specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md` for `Key`, `KeyState`, and
`KeyInventory` (in particular `HoldsKeyForDoor`/`TryGetKeyForDoor`, which 001 built specifically
so this feature could consume them — see 001 tasks.md T014).

**Tests**: Explicitly required — constitution Principle IV mandates EditMode tests for every
pure-logic piece. Matching, atomicity, idempotency, and reset are 100% expressible as plain C#
and MUST be covered. Door open/close animation and locked-feedback presentation are owned by
`specs/systems/keys-and-doors/004-door-visual-and-color-feedback` and audio hook assets by
`specs/systems/audio/001-sound-event-taxonomy` — neither is re-tested here; this feature only
needs to prove the rule itself and that it exposes the events those systems consume.

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
- Extensions to 001's existing pure C# classes: `Assets/Scripts/Systems/Keys/`
- MonoBehaviour adapters: `Assets/Scripts/MonoBehaviours/KeysDoors/`
- EditMode tests: `Assets/Tests/EditMode/Systems/KeysDoors/`
- Shared config asset: this feature introduces no new `GameConfig` fields — door/key matching
  uses stable IDs authored in floor/level data (owned by the floor scene specs), not tunable
  values.

---

## Phase 1: Setup

**Purpose**: Establish the folders this feature's files live in. No production logic yet.

- [ ] T001 Create the `Assets/Scripts/Systems/KeysDoors/`, `Assets/Scripts/MonoBehaviours/KeysDoors/`,
  and `Assets/Tests/EditMode/Systems/KeysDoors/` directories (via the first class/test below —
  Unity does not version empty folders).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The `Door` identity/state type and the key-consumption path this feature adds on
top of 001's `Key`/`KeyInventory`. Every US1 task depends on these.

**⚠️ CRITICAL**: T002–T005 block every task in Phase 3 onward.

- [ ] T002 [P] Modify `Assets/Scripts/Systems/Keys/KeyState.cs` (owned by 001, extended here per
  001's own note that the further state belongs to this spec): add the `Consumed` value to
  `enum KeyState { InWorld, Held, Consumed }`.
- [ ] T003 [P] Create `Assets/Scripts/Systems/KeysDoors/DoorState.cs` defining
  `enum DoorState { Locked, Unlocked }`.
- [ ] T004 Create `Assets/Scripts/Systems/KeysDoors/Door.cs`: a plain C# class (no
  `MonoBehaviour`) with a stable `string Id`, a `DoorState State` defaulting to `Locked`, and a
  `void ResetToLocked()` method that unconditionally sets `State` back to `Locked` regardless of
  its prior value (the floor-reset contract required by FR-003; the reset *trigger* itself is
  owned by `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death`) (depends on T003).
- [ ] T005 Modify `Assets/Scripts/Systems/Keys/KeyInventory.cs` (owned by 001): add
  `bool ConsumeKeyForDoor(string doorId)` — finds the held key whose `TargetDoorId == doorId` via
  the existing `TryGetKeyForDoor`, sets its `State = KeyState.Consumed`, removes it from the
  inventory (`Contains(key.Id)` becomes `false`), and returns `true`; returns `false` and mutates
  nothing if no such key is held. This is the one and only removal path 001 deliberately left out
  of its own inventory (depends on T002, and 001's `TryGetKeyForDoor`).

**Checkpoint**: `Door`, `DoorState`, and key-consumption exist and compile — US1 work can begin.

---

## Phase 3: User Story 1 - Keys Open Only Their Matching Doors (Priority: P1) 🎯 MVP

**Goal**: Pressing Unlock on a locked door opens it and consumes exactly the matching key when
held; any other input state (missing key, wrong key, already-open door, duplicate input) is
handled correctly with zero or exactly one inventory mutation and no false success.

**Independent Test**: Test missing, wrong, matching, repeated, and already-open states (spec.md's
own Independent Test).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no `DoorUnlockSystem` yet) before implementing.

- [ ] T006 [P] [US1] In `Assets/Tests/EditMode/Systems/KeysDoors/DoorUnlockMatchingTests.cs`,
  write: given a `Door` in `Locked` state and a `KeyInventory` holding the exact matching key,
  `TryUnlock` returns `true`, `door.State` becomes `Unlocked`, the key is consumed (removed from
  the inventory, its `State` is `Consumed`), and `DoorUnlocked` fires exactly once (FR-001,
  Acceptance Scenario 1).
- [ ] T007 [P] [US1] In the same file, write: given a `Locked` door and an empty (or
  unrelated-key-only) inventory — the "missing key" case — `TryUnlock` returns `false`,
  `door.State` stays `Locked`, the inventory is untouched, and an `UnlockFailed` event fires
  (Acceptance Scenario 2).
- [ ] T008 [P] [US1] In the same file, write: given a `Locked` door and an inventory holding a key
  whose `TargetDoorId` matches a *different* door — the "wrong key" case — the outcome is
  identical to T007's failure case, proving matching is by exact stable ID and never by proximity,
  ordering, or count (FR-001, SC-002).
- [ ] T009 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/DoorUnlockIdempotencyTests.cs`, write: given a door
  already in `Unlocked` state, calling `TryUnlock` again (with or without a matching key still
  held) returns `false`, raises no event, and does not re-consume or otherwise mutate the
  inventory (FR-002, Acceptance Scenario 3).
- [ ] T010 [P] [US1] In the same file, write: two rapid/duplicate `TryUnlock` calls against the
  same initially-`Locked` door with a matching key held (simulating simultaneous input in the same
  frame) unlock and consume the key exactly once — the second call sees the now-`Unlocked` door
  and is the T009 no-op (edge case: simultaneous input).
- [ ] T011 [P] [US1] In `Assets/Tests/EditMode/Systems/KeysDoors/DoorUnlockAtomicityTests.cs`,
  write: every `TryUnlock` outcome across the missing/wrong/matching/already-open matrix produces
  either exactly zero inventory mutations (any failure) or exactly one (the single successful
  unlock) — never a partial state such as a consumed key with a still-`Locked` door, or an
  `Unlocked` door with the key left un-consumed (FR-002, SC-001's "state matrix ... one or zero
  inventory mutations").
- [ ] T012 [P] [US1] In `Assets/Tests/EditMode/Systems/KeysDoors/DoorResetTests.cs`, write:
  `Door.ResetToLocked()` returns an `Unlocked` door to `Locked`, and is safe to call on an
  already-`Locked` door too (idempotent reset), modeling the floor-reset consumer contract
  (FR-003).
- [ ] T013 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/DoorUnlockSubtypeAgnosticTests.cs`, write a structural
  test asserting `DoorUnlockSystem.TryUnlock`'s signature and behavior take only `Door` and
  `KeyInventory` — no door "kind"/subtype parameter or branch — so that
  `specs/systems/keys-and-doors/003-final-door-distinct-behavior`'s final-door subtype can be
  layered on top by composition without this system being modified (edge case: "final-door subtype
  MUST be handled explicitly").

### Implementation for User Story 1

- [ ] T014 [US1] Create `Assets/Scripts/Systems/KeysDoors/DoorUnlockSystem.cs`: a plain C# class
  (no `MonoBehaviour`) exposing `event Action<Door> DoorUnlocked`, `event Action<Door> UnlockFailed`,
  and `bool TryUnlock(Door door, KeyInventory inventory)` that: no-ops and returns `false` if
  `door.State != Locked`; otherwise looks up the door's matching key via
  `inventory.TryGetKeyForDoor(door.Id, out key)`, and on a match calls
  `inventory.ConsumeKeyForDoor(door.Id)`, sets `door.State = Unlocked`, raises `DoorUnlocked` once,
  and returns `true`; on no match, raises `UnlockFailed` once and returns `false` without mutating
  anything (depends on T004, T005).
- [ ] T015 [US1] Run T006–T013 against T014's implementation; fix until green.
- [ ] T016 [US1] Create `Assets/Scripts/MonoBehaviours/KeysDoors/DoorInteractable.cs`: a thin
  `MonoBehaviour` adapter implementing the interactable contract from
  `specs/systems/interaction-and-highlight/001-nearest-interactable-detection` and
  `.../002-context-sensitive-action-button`, holding a reference to one `Door`. On the action
  button firing, it calls `DoorUnlockSystem.TryUnlock`; on success it calls the door-open
  animation/feedback hook call site (visual presentation owned by
  `specs/systems/keys-and-doors/004-door-visual-and-color-feedback`, audio owned by
  `specs/systems/audio/001-sound-event-taxonomy`); on failure it calls the "locked, key required"
  feedback hook call site (same ownership split) (depends on T014).

**Checkpoint**: User Story 1 is fully functional and independently testable — the full
missing/wrong/matching/repeated/already-open matrix behaves correctly with atomic, idempotent
mutations.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T017 [P] Add XML doc comments to `Door.cs` and `DoorUnlockSystem.cs` public members citing
  GDD 8.1 and this spec, and noting that
  `specs/systems/keys-and-doors/003-final-door-distinct-behavior` composes on top of
  `DoorUnlockSystem` without modifying it, and that
  `specs/systems/keys-and-doors/004-door-visual-and-color-feedback` is the sole intended reader of
  `Door.State` for presentation.
- [ ] T018 Run the full `Assets/Tests/EditMode/Systems/KeysDoors/` suite plus the
  `Assets/Tests/EditMode/Systems/Keys/` regression suite (since T002/T005 modified 001's files)
  and confirm 100% green before marking this feature done (mirrors SC-001/SC-002).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup and on 001's `Key`/`KeyInventory` already
  existing — BLOCKS User Story 1.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **Polish (Phase 4)**: Depends on User Story 1 being complete.

### Parallel Opportunities

- T002 and T003 can run in parallel; T004 depends on T003, T005 depends on T002.
- All [P] test-writing tasks within Phase 3 can run in parallel with each other.

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 — the entire missing/wrong/matching/repeated/already-open matrix is
   proven, which is this feature's only story and its MVP.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently.

### Incremental Delivery

1. Setup + Foundational → `Door`/`DoorState`/key-consumption shapes ready.
2. US1 → independently tested → unlock matching, atomicity, idempotency, and reset all proven
   (unblocks spec 003's eligibility/completion logic and spec 004's presentation).
3. Polish → full suite green, including the 001 regression suite.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task has a matching test task written and failing first, per constitution
  Principle IV.
- This feature introduces no new `GameConfig` fields — matching is by stable ID, not a tunable.
- T002 and T005 modify files owned by 001; re-run 001's own test suite after those edits.
- Commit after each task or logical group; stop at the checkpoint to validate the story
  independently before moving on.
