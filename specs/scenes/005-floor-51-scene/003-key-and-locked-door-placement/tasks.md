---
description: "Task list for Floor 51 Key and Locked Door Placement"
---

# Tasks: Floor 51 Key and Locked Door Placement

**Input**: Design documents from
`specs/scenes/005-floor-51-scene/003-key-and-locked-door-placement/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry` (the `KeyArea` and
`LockedDoorApproach` Areas must already exist in `Assets/Scenes/Floor51.unity`)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (pair count, ID
matching, reset-state restoration). Objective clarity/readability is level-design judgment
validated by playtest, per constitution Principle IV's live-scene exception.

**Organization**: spec.md has a single user story (US1: "Solve one readable lock"). Tasks below
break that one story into the smallest independently-placeable/verifiable units — placing each
object, wiring identity, reachability, reset, and feedback — per
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/tasks.md`'s target granularity.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/objects, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Tests/EditMode/LevelDesign/Floor51KeyDoorContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` (reuse the folder if a sibling Floor
      51 spec already created it)

**Checkpoint**: Test file exists for validation to reference.

---

## Phase 2: Foundational — Stable ID Scheme

- [ ] T002 Choose and document the stable ID pair this floor's `Key`/`Door` will share (e.g.
      `Floor51Key01` / `Floor51Door01`) so
      `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md`'s ID-matching logic
      (its FR-001: "stable IDs, never display labels") resolves correctly — record the chosen IDs
      in this task's completion note

**Checkpoint**: The ID pair this feature's objects will use is decided before either object is
placed.

---

## Phase 3: User Story 1 — Solve One Readable Lock (Priority: P1)

**Goal**: Exactly one `Key` and one `Door` exist with matching stable IDs; the key is reachable
before the door, not in danger geometry; the door and key both reset correctly; the objective is
inferable without on-screen instruction.

**Independent Test**: Walk the route, collect the key, unlock the door, and reset the floor
(spec.md US1 Independent Test).

- [ ] T003 [US1] Place one `Key` object (per
      `specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md`'s contract) inside
      `KeyArea` in `Assets/Scenes/Floor51.unity`, positioned reachable and not touching any hazard
      geometry (spec.md FR-002)
- [ ] T004 [US1] Place one `Door` object (per
      `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md`'s contract) at
      `LockedDoorApproach`'s gated boundary into `ChaseSection`, per spec 001 FR-007's "exactly one
      lock on this route"
- [ ] T005 [US1] Set the `Key`'s target-door identity and the `Door`'s own ID to the matching pair
      chosen in T002, and confirm no other Key/Door pair exists anywhere in the scene sharing
      either ID (spec.md FR-001)
- [ ] T006 [US1] Audit the `Key`'s placed position against every hazard/forbidden-geometry marker
      in `KeyArea`; reposition it if any overlap is found (spec.md FR-002 "not spawn inside danger
      geometry")
- [ ] T007 [US1] Manual walkthrough: from `Checkpoint`, confirm the `Key` is reachable via
      `ExplorationZone` → `KeyArea` without ever crossing `LockedDoorApproach`'s gated boundary
      (spec.md FR-002, reusing spec 001 US1's key-before-door topology)
- [ ] T008 [US1] Configure the `Door`'s locked-state visual/color feedback (per
      `specs/systems/keys-and-doors/004-door-visual-and-color-feedback/spec.md`) so a first-time
      player can infer "this needs a key" without on-screen text (spec.md SC-002)
- [ ] T009 [US1] Wire this floor's `Key` and `Door` instances into the floor-reset path (per
      `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`) so a reset
      returns the `Door` to `Locked` and restores the `Key` to its original `KeyArea` position and
      uncollected state (spec.md FR-003)
- [ ] T010 [US1] Manual walkthrough: after unlocking the `Door`, confirm backtracking from
      `ChaseSection` toward `KeyArea`/`LockedDoorApproach` is not blocked — unlocking only adds a
      route, it does not remove existing ones (spec 001 Edge Cases)
- [ ] T011 [US1] Add an EditMode test in `Floor51KeyDoorContentTests.cs`: open
      `Assets/Scenes/Floor51.unity` additively and assert exactly one `Key` and exactly one `Door`
      component exist, with matching stable IDs and no other pair sharing either ID (spec.md
      FR-001, SC-001)
- [ ] T012 [US1] Add an EditMode test: simulate pickup, unlock, and a floor reset, and assert the
      `Door`'s state returns to `Locked` and the `Key`'s state returns to its original in-world
      position and uncollected state (spec.md FR-003)
- [ ] T013 [US1] Playtest objective clarity: run at least 5 first-time playtesters through this
      floor's key/door beat and record whether each inferred the objective from feedback alone,
      without external instruction (spec.md SC-002)

**Checkpoint**: User Story 1 is independently complete — one readable, resettable lock exists and
is solvable in the correct order.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T014 Review the placed `Key`/`Door` pair against spec 001 FR-007 ("exactly one lock on the
      Key Area/Locked Door Approach → Chase Section route") — confirm no redundant physical
      barrier was introduced by this spec's placement
- [ ] T015 Once spec 002's Monster Spawn Presets and Patrol Waypoints have landed, confirm neither
      the `Key` nor the `Door` coincides with a patrol waypoint or spawn preset position (mirrors
      spec 002 tasks.md T016's objective-coincidence audit, run in reverse)
- [ ] T016 Record the T011/T012 test results and the T013 playtest outcome in this feature's
      `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends on Setup/Foundational and on spec 001's `KeyArea`/
  `LockedDoorApproach` Areas already existing. T003 and T004 are independent placements and could
  run in parallel, but T005 onward depends on both existing, so they are not marked `[P]`.
- **Polish (Phase 4)**: after User Story 1; T015 additionally depends on spec 002 landing.

```text
Setup (T001)
   ↓
Foundational (T002)
   ↓
US1 (T003-T013)
   ↓
Polish (T014-T016, T015 deferred on spec 002)
```

## Notes

- T015 is an explicit forward-dependency task — mark it done only once spec 002 exists, per the
  same pattern spec 001's own `tasks.md` uses for its T015 and Floor 50's parallel specs use for
  their own cross-spec re-checks.
- This spec has no new pure-logic C# — it places instances of the `Key`/`Door` contracts already
  owned by `specs/systems/keys-and-doors/001` and `002`, and is otherwise Unity Editor placement
  in `Assets/Scenes/Floor51.unity`, validated by the manual walkthroughs and playtest above and the
  two structural EditMode tests in T011–T012, per constitution Principle IV's exception.
