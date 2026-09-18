---
description: "Task list for Floor 51 Exit and Transition"
---

# Tasks: Floor 51 Exit and Transition

**Input**: Design documents from
`specs/scenes/005-floor-51-scene/006-floor-exit-and-transition/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry` (the `FloorExitAlcove` Area must
already exist in `Assets/Scenes/Floor51.unity`),
`specs/scenes/005-floor-51-scene/003-key-and-locked-door-placement` (the `Door` whose `Unlocked`
state this feature gates on)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (precondition
matrix, transition-request idempotency, reset-state restoration). Device-level scene load
behavior is validated by manual walkthrough, per constitution Principle IV's live-scene exception.

**Organization**: spec.md has a single user story (US1: "Advance after the lock"). Tasks below
break that story into the smallest independently-placeable/verifiable units — placing the trigger,
wiring its precondition, wiring the transition request, and wiring reset — per
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/tasks.md`'s target granularity.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/objects, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Tests/EditMode/LevelDesign/Floor51ExitContentTests.cs` (empty test class
      stub) under `Assets/Tests/EditMode/LevelDesign/` (reuse the folder if a sibling Floor 51
      spec already created it)

**Checkpoint**: Test file exists for validation to reference.

---

## Phase 2: Foundational — Confirm the Scene Graph Edge

- [ ] T002 Confirm `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`'s
      explicit scene graph includes a Floor51 → Floor50 edge before wiring this floor's exit
      trigger to request it (spec.md FR-002)

**Checkpoint**: The transition target this feature requests is a valid graph edge before any
trigger is wired to it.

---

## Phase 3: User Story 1 — Advance After the Lock (Priority: P1)

**Goal**: The Floor Exit Door/trigger requires the Floor 51 objective (the `Door` from spec 003
being `Unlocked`) before it is interactable, issues exactly one Floor50 transition request on a
valid exit, preserves run state across it, and resets to its locked/objective-incomplete state on
death/reset.

**Independent Test**: Attempt exit before and after unlock, then reload Floor 50 (spec.md US1
Independent Test).

- [ ] T003 [US1] Place the Floor Exit Door/trigger `GameObject` at spec 001's `FloorExitAlcove`
      Area, at the end of `ChaseSection`, in `Assets/Scenes/Floor51.unity`
- [ ] T004 [US1] Wire the Floor Exit Door's precondition check to require the `Door` placed by
      spec 003 to be in its `Unlocked` state before the trigger is interactable (spec.md FR-001)
- [ ] T005 [US1] Add a locked-state visual/feedback presentation for the Floor Exit Door prior to
      the objective being met, distinguishing it from its unlocked/enterable presentation (mirrors
      `specs/systems/keys-and-doors/004-door-visual-and-color-feedback/spec.md`'s pattern; supports
      spec.md SC-001's "no false-positive exit")
- [ ] T006 [US1] Wire the Floor Exit Door's trigger to issue exactly one
      `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md` transition
      request targeting Floor 50 on a successful (post-unlock) entry (spec.md FR-002)
- [ ] T007 [US1] Guard the trigger so a second or duplicate entry attempt while a transition
      request is already in flight issues zero additional requests (spec.md FR-002, mirrors
      scene-transition-manager's own idempotency rule)
- [ ] T008 [US1] Wire the transition request to carry the shared run state (lives count, per
      `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md`) forward
      unmodified across the Floor51 → Floor50 load (spec.md SC-002)
- [ ] T009 [US1] Wire this floor's exit precondition/state into the floor-reset path (per
      `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`) so a death or
      reset returns the Floor Exit Door to its initial locked/objective-incomplete state
      (spec.md FR-003)
- [ ] T010 [US1] Add an EditMode test in `Floor51ExitContentTests.cs`: build a precondition matrix
      (Door locked/unlocked × exit attempted) and assert the exit trigger only succeeds in the
      unlocked case, with zero false-positive exits in every other cell (spec.md FR-001, SC-001)
- [ ] T011 [US1] Add an EditMode test: simulate a valid exit and assert exactly one Floor50
      transition request is issued and lives/run-state values are unchanged across it (spec.md
      FR-002, SC-002)
- [ ] T012 [US1] Add an EditMode test: simulate two rapid/duplicate valid-exit attempts and assert
      only one transition request is ever issued (spec.md FR-002)
- [ ] T013 [US1] Add an EditMode test: simulate a death/reset occurring after unlocking but before
      exiting, and assert the exit trigger's state returns to locked/objective-incomplete
      (spec.md FR-003)
- [ ] T014 [US1] Manual walkthrough / device verification: attempt the exit before unlock
      (must fail with the locked-state feedback from T005), unlock the `Door`, exit, and confirm
      Floor 50 loads exactly once with lives/run state preserved (spec.md Independent Test,
      SC-002)

**Checkpoint**: User Story 1 is independently complete — the exit is correctly gated, transitions
exactly once, preserves state, and resets correctly.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T015 Confirm the Floor Exit Door's precondition (T004) does not duplicate or overlap spec
      003's `Door` unlock logic — exactly one lock (spec 003's `Door`) gates entry to
      `ChaseSection`, and exactly one gate (objective completion) gates the Floor Exit Door itself,
      per spec 001 FR-007
- [ ] T016 Once spec 002's Monster Patrol Waypoints have landed, confirm the Floor Exit Door/
      trigger does not coincide with a patrol waypoint position (mirrors spec 002 tasks.md T016's
      objective-coincidence audit, run in reverse)
- [ ] T017 Record the T010–T013 test results and the T014 device-verification outcome in this
      feature's `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends on Setup/Foundational, on spec 001's `FloorExitAlcove` Area,
  and on spec 003's `Door` already existing. Tasks T003–T014 are sequential (each wires onto the
  previous), so none is marked `[P]`.
- **Polish (Phase 4)**: after User Story 1; T016 additionally depends on spec 002 landing.

```text
Setup (T001)
   ↓
Foundational (T002)
   ↓
US1 (T003-T014)
   ↓
Polish (T015-T017, T016 deferred on spec 002)
```

## Notes

- T016 is an explicit forward-dependency task — mark it done only once spec 002 exists, per the
  same pattern spec 001's own `tasks.md` uses for its T015 and Floor 50's parallel specs use for
  their own cross-spec re-checks.
- This spec has no new pure-logic C# beyond the trigger's precondition/idempotency wiring in
  T004/T006/T007 — it consumes the existing `Door` (spec 003), `progression-and-scene-flow/002`,
  and `lives-and-fail-state/001`/`002` contracts unchanged, and is otherwise Unity Editor placement
  in `Assets/Scenes/Floor51.unity`, validated by the manual walkthrough above and the structural
  EditMode tests in T010–T013, per constitution Principle IV's exception.
