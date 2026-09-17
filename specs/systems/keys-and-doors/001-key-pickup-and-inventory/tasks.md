---

description: "Task list for 001-key-pickup-and-inventory"
---

# Tasks: Key Pickup & Inventory

**Input**: Design documents from `specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md`

**Prerequisites**: `spec.md` (this feature; required). Depends on
`specs/systems/interaction-and-highlight/002-context-sensitive-action-button/spec.md` (the action
button itself, transitively 001's nearest-interactable detection) and
`specs/systems/interaction-and-highlight/003-interactable-highlight-halo/spec.md` (highlight
visual) — neither is redefined here.

**Tests**: Explicitly required — constitution Principle IV mandates EditMode tests for every
pure-logic piece. This feature's pickup/inventory rules (identity tracking, idempotency, no
drop/discard) are 100% expressible as plain C# and MUST be covered. The visual highlight and
"no floating icon" requirement (FR-002) are rendering content owned by
interaction-and-highlight/003 and are validated by that feature's own tests/quickstart, not
re-tested here — this feature only needs to prove it does not add a second, competing icon.

**Organization**: Tasks are grouped by user story (US1–US3) from `spec.md`, in priority order.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: Which user story this task belongs to (US1–US3), or unlabeled for
  Setup/Foundational/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Keys/`
- MonoBehaviour adapters: `Assets/Scripts/MonoBehaviours/Keys/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Keys/`
- Shared config asset (owned by shared-config-and-state/001-game-config-schema): no new
  `GameConfig` fields are introduced by this feature per FR-009 — nothing to add here.

---

## Phase 1: Setup

**Purpose**: Establish the folders this feature's files live in. No production logic yet.

- [ ] T001 Create the `Assets/Scripts/Systems/Keys/`, `Assets/Scripts/MonoBehaviours/Keys/`, and
  `Assets/Tests/EditMode/Systems/Keys/` directories (via the first class/test below — Unity does
  not version empty folders).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The Key identity type and inventory container every user story reads or extends.

**⚠️ CRITICAL**: T002–T004 block every task in Phase 3 onward.

- [ ] T002 [P] Create `Assets/Scripts/Systems/Keys/KeyState.cs` defining
  `enum KeyState { InWorld, Held }` (the `Consumed` value is added later by
  `specs/systems/keys-and-doors/002-locked-door-unlock-logic` — do not add it here, per spec.md's
  own note that the further state belongs to spec 002).
- [ ] T003 [P] Create `Assets/Scripts/Systems/Keys/Key.cs`: a plain C# class (no `MonoBehaviour`,
  no scene dependency) with a stable `string Id`, a `string TargetDoorId` (the one specific
  `Door` this key will unlock — established in floor/level data, out of scope here per FR-004),
  and a `KeyState State` defaulting to `InWorld` (depends on T002).
- [ ] T004 Create `Assets/Scripts/Systems/Keys/KeyInventory.cs`: a plain C# class exposing only
  `bool Add(Key key)`, `bool Contains(string keyId)`, and `int Count` for now. Deliberately expose
  **no** removal method at this stage — FR-005 requires the only ways a key leaves the inventory
  are consumption (spec 002) or a floor reset (lives-and-fail-state/002), so no `Remove`/`Drop`
  method exists in this feature's code at all, not even an unused one (depends on T003).

**Checkpoint**: `Key` and `KeyInventory` exist and compile — user story work can begin.

---

## Phase 3: User Story 1 - Pick Up a Key With the Action Button (Priority: P1) 🎯 MVP

**Goal**: Pressing the action button on an in-range key removes it from the world, adds it to the
inventory, and fires a pickup feedback hook exactly once.

**Independent Test**: In a test scene containing a single key, walk into interaction range, press
the action button, and confirm the key leaves the world, appears in the inventory, and pickup
feedback fires (spec.md's own Independent Test for this story).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no `KeyPickupSystem` yet) before implementing.

- [ ] T005 [P] [US1] In `Assets/Tests/EditMode/Systems/Keys/KeyPickupSystemTests.cs`, write: a
  `Key` in `InWorld` state, when picked up, transitions to `Held`, is added to the
  `KeyInventory` (i.e. `Contains(key.Id)` is true), and `Count` increases by exactly 1 (FR-003).
- [ ] T006 [P] [US1] In the same file, write: picking up a key raises the `KeyPickedUp` event
  exactly once, with that exact key as the event argument (FR-003's "hook fires exactly once").
- [ ] T007 [P] [US1] In the same file, write: after pickup, the key's original identity is not
  re-discoverable as `InWorld` — querying its `State` afterward returns `Held`, never `InWorld`
  again (covers spec.md Acceptance Scenario 3 — "no key exists there" — at the pure-logic level;
  the MonoBehaviour removing the GameObject from the scene is covered by T010).

### Implementation for User Story 1

- [ ] T008 [US1] Create `Assets/Scripts/Systems/Keys/KeyPickupSystem.cs`: a plain C# class (no
  `MonoBehaviour`) exposing `event Action<Key> KeyPickedUp` and
  `bool TryPickUp(Key key, KeyInventory inventory)` that, when `key.State == KeyState.InWorld`,
  sets `key.State = KeyState.Held`, calls `inventory.Add(key)`, raises `KeyPickedUp` once, and
  returns `true` (depends on T003, T004).
- [ ] T009 [US1] Run T005–T007 against T008's implementation; fix until green.
- [ ] T010 [US1] Create `Assets/Scripts/MonoBehaviours/Keys/KeyInteractable.cs`: a thin
  `MonoBehaviour` adapter implementing the interactable contract from
  `specs/systems/interaction-and-highlight/001-nearest-interactable-detection` and
  `.../002-context-sensitive-action-button`, holding a reference to one `Key`. On the action
  button firing, it calls `KeyPickupSystem.TryPickUp`; on success it deactivates/destroys the
  GameObject (removing the key from the world) and calls the pickup SFX hook call site (concrete
  asset owned by `specs/systems/audio/001-sound-event-taxonomy`, out of scope here). It adds no
  floating prompt icon of its own — the highlight-in-range visual is provided entirely by
  interaction-and-highlight/003's existing halo component already attached to the same
  GameObject (FR-002) (depends on T008).

**Checkpoint**: User Story 1 is fully functional and independently testable — a single key can be
picked up correctly.

---

## Phase 4: User Story 2 - Keys Are Tracked by Identity, Not Just a Count (Priority: P2)

**Goal**: The inventory answers "does the player hold the key for door X" correctly for any
number of distinct held keys, never just a count.

**Independent Test**: In a test scene with two keys, each authored with a different
target-door identity, pick up both and query the inventory for each door's specific key
(spec.md's own Independent Test).

### Tests for User Story 2 ⚠️

- [ ] T011 [P] [US2] In `Assets/Tests/EditMode/Systems/Keys/KeyInventoryIdentityTests.cs`, write:
  after picking up two `Key`s with different `TargetDoorId`s, the inventory holds both distinct
  entries simultaneously (`Contains` is true for both `Id`s, `Count == 2`), and neither
  overwrites the other (FR-004, Acceptance Scenario 1).
- [ ] T012 [P] [US2] In the same file, write: `HoldsKeyForDoor(doorId)` returns `true` only for
  the door whose key is actually held, `false` for a door whose key was never picked up, and
  `false` when the inventory is empty (FR-004, Acceptance Scenario 2).
- [ ] T013 [P] [US2] In the same file, write: `HoldsKeyForDoor` reflects the exact key's identity,
  not merely a nonzero count — holding one key mapped to door A must not make
  `HoldsKeyForDoor("B")` true.

### Implementation for User Story 2

- [ ] T014 [US2] Extend `KeyInventory.cs` with `bool HoldsKeyForDoor(string doorId)` and
  `bool TryGetKeyForDoor(string doorId, out Key key)` (the latter is unused within this feature
  but is the exact query surface `specs/systems/keys-and-doors/002-locked-door-unlock-logic`
  needs to find the matching key to consume — documented via an XML doc comment citing that spec)
  (depends on T004).
- [ ] T015 [US2] Run T011–T013 against T014's implementation; fix until green.

**Checkpoint**: Users Story 1–2 both work independently — multi-key identity tracking is proven
ahead of spec 002 needing it.

---

## Phase 5: User Story 3 - Keys Can Never Be Dropped, Discarded, or Picked Up Twice (Priority: P3)

**Goal**: No drop/discard action is reachable for a held key, and re-picking an already-collected
key is a safe no-op.

**Independent Test**: With a test harness, confirm no drop/discard action exists for a held key,
and confirm re-attempting a pickup at an already-collected key's former position finds nothing
there (spec.md's own Independent Test).

### Tests for User Story 3 ⚠️

- [ ] T016 [P] [US3] In `Assets/Tests/EditMode/Systems/Keys/KeyPickupIdempotencyTests.cs`, write:
  calling `TryPickUp` a second time on a key already in `Held` state returns `false`, does not
  raise `KeyPickedUp` again, does not change `KeyInventory.Count`, and throws no exception
  (FR-006, Acceptance Scenario 2).
- [ ] T017 [P] [US3] In `Assets/Tests/EditMode/Systems/Keys/KeyInventoryStructuralGuardTests.cs`,
  write a reflection-based structural test asserting that neither `Key`, `KeyState`, nor
  `KeyInventory`'s public API surface exposes any method or property named (or reasonably
  interpretable as) `Drop`, `Discard`, or `Remove` — operationalizing SC-004's "structural check,
  since no such action exists to exercise at runtime" as an automated guard against future
  regressions, per FR-005.

### Implementation for User Story 3

- [ ] T018 [US3] Run T016–T017 against the existing T008/T004 implementations. No new production
  code is expected to be required — US1/US2's design (checking `key.State` before mutating, and
  `KeyInventory` never exposing a removal method) already satisfies FR-005/FR-006 by construction;
  this phase exists to prove that guarantee explicitly and catch any future regression.
- [ ] T019 [US3] If T017 fails because a removal-shaped member exists, remove it and re-run T016–
  T017 until green (should not trigger given T004/T008's design, but keeps the guard meaningful).

**Checkpoint**: All three user stories are independently functional and tested — pickup, identity
tracking, and the no-drop/no-double-pickup guarantees are proven.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T020 [P] EditMode test: `Assets/Tests/EditMode/Systems/Keys/KeyPickupNoGatingTests.cs` —
  `TryPickUp` succeeds identically regardless of any external player-state flag passed alongside
  it (the system itself takes no such flag, so this test documents/locks in FR-007 by confirming
  `TryPickUp`'s signature has no player-state parameter to gate on).
- [ ] T021 [P] EditMode test: picking up N distinct keys for N = 1 and N = 5 always yields exactly
  N distinct inventory entries, in `Assets/Tests/EditMode/Systems/Keys/KeyInventoryScaleTests.cs`
  (operationalizes SC-001 and FR-008's "no hardcoded per-floor count").
- [ ] T022 Add XML doc comments to `KeyInventory.cs` and `KeyPickupSystem.cs` public members
  citing GDD 4.2/8.1 and this spec, and noting that
  `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md` is the sole intended future
  consumer of `TryGetKeyForDoor`/consumption behavior.
- [ ] T023 Run the full `Assets/Tests/EditMode/Systems/Keys/` suite and confirm 100% green before
  marking this feature done (mirrors SC-001/SC-002's "100% of test runs").

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS every user story below.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **User Story 2 (Phase 4)**: Depends on Foundational and on `KeyInventory` existing (T004) to
  extend; independently testable without US1's `KeyPickupSystem` even existing, though in
  practice US1 lands first as the MVP.
- **User Story 3 (Phase 5)**: Depends on US1's `KeyPickupSystem` (T008) and Foundational's
  `KeyInventory` (T004) — it only adds proof/guard tests over their existing design.
- **Polish (Phase 6)**: Depends on all three user stories being complete.

### Parallel Opportunities

- T002 and T003 can run in parallel; T004 depends on T003.
- All [P] test-writing tasks within a phase can run in parallel with each other.
- US2's tests/implementation can be staffed in parallel with US1 once Foundational is done, since
  `HoldsKeyForDoor`/`TryGetKeyForDoor` only extend `KeyInventory`, not `KeyPickupSystem`.

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 — a single key can be picked up correctly. This alone unblocks nothing
   downstream yet (spec 002 needs US2's identity query), but is already demonstrable.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently.

### Incremental Delivery

1. Setup + Foundational → `Key`/`KeyInventory` shapes ready.
2. US1 → independently tested → single-key pickup proven.
3. US2 → independently tested → multi-key identity tracking proven (unblocks spec 002).
4. US3 → independently tested → no-drop/no-double-pickup guarantees proven.
5. Polish → scale and no-gating guarantees proven; full suite green.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task has a matching test task written and failing first, per constitution
  Principle IV.
- This feature introduces no new `GameConfig` fields (FR-009) — do not add any.
- Commit after each task or logical group; stop at any checkpoint to validate a story
  independently before moving on.
