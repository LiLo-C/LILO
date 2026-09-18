---

description: "Task list for 004-door-visual-and-color-feedback"
---

# Tasks: Door Visual and Color Feedback

**Input**: Design documents from
`specs/systems/keys-and-doors/004-door-visual-and-color-feedback/spec.md`

**Prerequisites**: `spec.md` (this feature; required). Depends on
`specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md` (`Door`, `DoorState`,
`DoorUnlockSystem.DoorUnlocked`) and
`specs/systems/keys-and-doors/003-final-door-distinct-behavior/spec.md`
(`FinalDoorSystem.Completed`, and the fact that a door may be a "final door") as the sole
authoritative sources of door state. This feature reads that state; it never derives or
duplicates unlock/eligibility logic of its own (spec.md Scope: "presentation only").

**Tests**: Explicitly required — constitution Principle IV mandates EditMode tests for every
pure-logic piece. The state→presentation mapping (which state maps to which icon/color/shape/text
identifiers) is 100% expressible as a pure C# function and MUST be covered. Actually rendering
those identifiers with concrete sprites/materials/colors on a live scene, and legibility under low
light or a colorblind simulation, is the one part of this feature that needs a live scene per
Principle IV's exception clause and is validated instead via `quickstart.md`-style manual device
review (T014 below), not an EditMode test.

**Organization**: This spec has a single user story (US1); tasks are grouped Setup →
Foundational → US1 → Polish, matching the granularity of
`specs/systems/keys-and-doors/001-key-pickup-and-inventory/tasks.md`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: Which user story this task belongs to (US1), or unlabeled for
  Setup/Foundational/Polish

## Path Conventions

- Pure C# view-model classes: `Assets/Scripts/Systems/KeysDoors/`
- MonoBehaviour adapters: `Assets/Scripts/MonoBehaviours/KeysDoors/`
- EditMode tests: `Assets/Tests/EditMode/Systems/KeysDoors/`
- Shared config asset: no new `GameConfig` fields — icon/color/shape/text asset identifiers are
  data owned by the art/UX system, not tunables introduced here.

---

## Phase 1: Setup

**Purpose**: Confirm the shared folders this feature's files live in already exist (created by
002/003; re-run only if starting from a clean checkout).

- [ ] T001 Confirm `Assets/Scripts/Systems/KeysDoors/`, `Assets/Scripts/MonoBehaviours/KeysDoors/`,
  and `Assets/Tests/EditMode/Systems/KeysDoors/` exist; create them if starting from a clean
  checkout.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The presentation state enum and the identifier-only descriptor every US1 task reads
or extends.

**⚠️ CRITICAL**: T002–T003 block every task in Phase 3 onward.

- [ ] T002 [P] Create `Assets/Scripts/Systems/KeysDoors/DoorVisualState.cs` defining
  `enum DoorVisualState { Locked, Unlockable, Opened, Final }` — the four states required by
  SC-001 ("four distinct ... presentations").
- [ ] T003 [P] Create `Assets/Scripts/Systems/KeysDoors/DoorVisualPresentation.cs`: a plain C#
  readonly struct/class with `string ColorId`, `string ShapeId`, `string IconId`, and
  `string TextKey` fields — identifiers only, no literal color values or asset references (the
  concrete art assets are owned by the art/UX system, out of scope here) (FR-002).

**Checkpoint**: `DoorVisualState` and `DoorVisualPresentation` exist and compile — US1 work can
begin.

---

## Phase 3: User Story 1 - Door State Is Readable (Priority: P2) 🎯 MVP

**Goal**: Locked, unlockable, opened, and final-door states each render a distinct,
non-color-only presentation that always matches the door's authoritative rule state, and a
transition animation never claims success before the rule actually commits it.

**Independent Test**: Render each state with matching/missing key and inspect icon, color, shape,
and prompt (spec.md's own Independent Test).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no `DoorVisualViewModel` yet) before
> implementing.

- [ ] T004 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/DoorVisualViewModelTests.cs`, write: `Compute`
  maps `(isFinalDoor: false, state: Locked, hasMatchingKey: false)` → `DoorVisualState.Locked`,
  `(false, Locked, true)` → `Unlockable`, `(false, Opened, *)` → `Opened` regardless of
  `hasMatchingKey`, and `(isFinalDoor: true, *, *)` → `Final` regardless of the other two inputs
  (SC-001, US1's Independent Test "render each state with matching/missing key").
- [ ] T005 [P] [US1] In the same file, write: for each of the four `DoorVisualState` values, the
  corresponding `DoorVisualPresentation` has a distinct combination of `ShapeId` + `IconId` +
  `TextKey` even when every `ColorId` is stubbed to the same value — operationalizing FR-002's
  "color MUST be paired with shape/icon/text cues" and SC-001's "non-color-only" as an automated,
  colorblind-safe fixture.
- [ ] T006 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/DoorVisualDerivationTests.cs`, write a structural test
  asserting `DoorVisualViewModel.Compute`'s signature takes only the authoritative inputs
  (`isFinalDoor`, `DoorState`, `hasMatchingKey`) and that the type holds no internal mutable
  "is animating" / "pending success" field — proving by construction that the mapping can never
  report `Opened` ahead of the rule's own committed `DoorState` (FR-001, FR-003).
- [ ] T007 [P] [US1] In the same file, write: calling `Compute` again with `DoorState.Locked`
  immediately after a call that previously returned `Opened` (simulating `Door.ResetToLocked()`
  from 002 mid-animation) returns `Locked` with no leftover `Opened`/`Final` presentation carried
  over from the prior call, since the mapping is a pure per-call function with no cached state
  (edge case: "reset during animation").
- [ ] T008 [P] [US1] In
  `Assets/Tests/EditMode/Systems/KeysDoors/DoorVisualFinalDoorTests.cs`, write: `isFinalDoor: true`
  yields the `Final` presentation for every `DoorState`/`hasMatchingKey` combination tried
  (`Locked`/`true`, `Locked`/`false`, `Opened`/`true`, `Opened`/`false`), keeping the final door
  legibly distinct in every reachable state (edge case: "final-door state MUST remain legible").

### Implementation for User Story 1

- [ ] T009 [US1] Create `Assets/Scripts/Systems/KeysDoors/DoorVisualViewModel.cs`: a plain C#
  class (no `MonoBehaviour`) exposing `DoorVisualState Compute(bool isFinalDoor, DoorState state, bool hasMatchingKey)`
  implementing the mapping from T004/T008, plus
  `DoorVisualPresentation GetPresentation(DoorVisualState visualState)` returning the fixed,
  pairwise-distinct descriptor for each of the four states (depends on T002, T003).
- [ ] T010 [US1] Run T004–T008 against T009's implementation; fix until green.
- [ ] T011 [US1] Create `Assets/Scripts/MonoBehaviours/KeysDoors/DoorVisualView.cs`: a thin
  `MonoBehaviour` adapter holding references to the owning `Door` (002), its final-door marker
  (003), and the scene's renderer/icon/text UI references. On each relevant update it reads the
  door's current `DoorState` and final-door marker, calls
  `DoorVisualViewModel.Compute`/`GetPresentation`, and applies the resulting identifiers to those
  references — guarding every renderer/icon reference for `null` before use so a missing renderer
  never throws (edge case: "missing renderer") (depends on T009).
- [ ] T012 [US1] Wire `DoorVisualView` to advance its displayed state only in response to
  `DoorUnlockSystem.DoorUnlocked` (002) / `FinalDoorSystem.Completed` (003) firing — never
  optimistically on the action-button press itself — so the open-transition animation cannot
  visually claim success before the rule commits (FR-003) (depends on T011, and 002/003's
  events).

**Checkpoint**: User Story 1 is fully functional and independently testable — all four
presentation states derive correctly from authoritative state, are pairwise distinct without
relying on color, and never precede the rule's own commit.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T013 [P] Add XML doc comments to `DoorVisualViewModel.cs` and `DoorVisualPresentation.cs`
  public members citing GDD 8.2 and this spec, noting that concrete sprite/color/icon assets are
  owned by the art/UX system and are out of scope here.
- [ ] T014 Manual quickstart on target device/dark test scene: render all four states
  (`Locked`/`Unlockable`/`Opened`/`Final`) under low light and under a colorblind simulation pass,
  confirming each remains distinguishable without relying on color alone — the one part of this
  feature not EditMode-testable per constitution Principle IV's rendering exception (SC-001,
  SC-002, edge cases "low light" and "color blindness").
- [ ] T015 Run the full `Assets/Tests/EditMode/Systems/KeysDoors/` suite and confirm 100% green
  before marking this feature done.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies (folders already exist from 002/003 in the common case).
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS User Story 1.
- **User Story 1 (Phase 3)**: Depends on Foundational, and conceptually on 002's `DoorState` and
  003's final-door completion signal existing as read-only inputs (this feature never mutates
  either).
- **Polish (Phase 4)**: Depends on User Story 1 being complete; T014 (device review) can only run
  once T009–T012 are implemented.

### Parallel Opportunities

- T002 and T003 can run in parallel.
- All [P] test-writing tasks within Phase 3 can run in parallel with each other.

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 — the four-state mapping, non-color-only distinctness, and
   commit-before-animation guarantee are proven, which is this feature's only story and its MVP.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently, then perform the T014 manual
   device review.

### Incremental Delivery

1. Setup + Foundational → `DoorVisualState`/`DoorVisualPresentation` shapes ready.
2. US1 → independently tested → state-derivation, non-color-only, and no-early-success guarantees
   all proven at the pure-logic level.
3. Polish → device-level legibility confirmed; full EditMode suite green.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task expressible as pure C# has a matching test task written and failing
  first, per constitution Principle IV; T014 is this feature's one Principle-IV-exception item.
- This feature introduces no new `GameConfig` fields — icon/color/shape/text identifiers are
  art/UX-owned data, not tunables.
- This feature never mutates `Door`/`DoorState` or `CompletionState` — it only reads them.
- Commit after each task or logical group; stop at the checkpoint to validate the story
  independently before moving on.
