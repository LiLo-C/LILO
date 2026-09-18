---
description: "Task list for Floor 51 Hiding Spot Placement"
---

# Tasks: Floor 51 Hiding Spot Placement

**Input**: Design documents from
`specs/scenes/005-floor-51-scene/005-hiding-spot-placement/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry` (its FR-010 reserved
enclosed-nook slots must already exist in `Assets/Scenes/Floor51.unity`)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (spot count,
collision-free entry/exit anchors, reset-state restoration). Chase usability/discoverability is
level-design judgment validated by manual walkthrough and playtest, per constitution Principle
IV's live-scene exception.

**Organization**: spec.md has a single user story (US1: "Find cover during pressure"). Tasks
below break that story into the smallest independently-placeable/verifiable units — one task per
hiding spot, plus the shared-system wiring and reset behavior — per
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/tasks.md`'s target granularity.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/objects, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Tests/EditMode/LevelDesign/Floor51HidingContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` (reuse the folder if a sibling Floor
      51 spec already created it)

**Checkpoint**: Test file exists for validation to reference.

---

## Phase 2: User Story 1 — Hiding Spots (Priority: P1)

**Goal**: Two `HidingSpot` objects placed at spec 001's reserved nooks, each with valid
entry/exit anchors and cover geometry, working with the shared immunity/dampening rules, not
clustered near `Checkpoint` or `FloorExitAlcove`, and not creating a trap.

**Independent Test**: Play a chase from nearby route segments and enter/exit each spot safely
(spec.md US1 Independent Test).

- [ ] T002 [P] [US1] Place Hiding Spot "1" (a `HidingSpot` object per
      `specs/systems/hiding/001-enter-and-exit-hiding/spec.md`'s contract) at one of spec 001's
      reserved nook slots in `ExplorationZone`, configured with a valid entry anchor, exit anchor,
      and cover geometry (spec.md FR-001)
- [ ] T003 [P] [US1] Place Hiding Spot "2", same component/contract, at the second reserved nook
      slot in a spatially distinct Area from Hiding Spot "1" (e.g. `KeyArea` or `ChaseSection`),
      so the two spots are not clustered (spec.md FR-003)
- [ ] T004 [US1] Audit both Hiding Spots' distance from `Checkpoint` and `FloorExitAlcove`,
      repositioning either spot found clustered near either (spec.md FR-003)
- [ ] T005 [US1] Confirm each Hiding Spot's entry and exit anchors are collision-free and
      navigable, with no blocked exit, per `specs/systems/hiding/001-enter-and-exit-hiding/spec.md`
      FR-003 (spec.md FR-001, SC-001)
- [ ] T006 [US1] Confirm each Hiding Spot correctly exposes itself to
      `specs/systems/hiding/002-hiding-detection-immunity-rule/spec.md`'s Hidden-state immunity
      check with no scene-local override or duplicate distance exception (spec.md FR-002)
- [ ] T007 [US1] Confirm each Hiding Spot correctly exposes itself to
      `specs/systems/hiding/003-hiding-audio-and-light-dampening/spec.md`'s entry/exit dampening
      hooks with no scene-local override (spec.md FR-002)
- [ ] T008 [US1] Manual walkthrough: from several nearby route segments, run a simulated chase
      toward each Hiding Spot and confirm entry, detection immunity while Hidden, and a safe
      (non-trapping) exit for both spots (spec.md FR-002, SC-001)
- [ ] T009 [US1] Manual walkthrough: confirm neither Hiding Spot is so hidden it is undiscoverable
      by normal exploration, nor so exposed that using it trivializes the floor's key/door route
      (spec.md SC-002)
- [ ] T010 [US1] Add an EditMode test in `Floor51HidingContentTests.cs`: open
      `Assets/Scenes/Floor51.unity` additively and assert exactly 2 `HidingSpot` components exist,
      each with a distinct, collision-free entry/exit anchor pair (spec.md FR-001, SC-001)
- [ ] T011 [US1] Wire both Hiding Spots into the floor-reset path (per
      `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`) so a reset
      returns any currently-Hidden state to Visible and clears audio/light dampening modifiers
      (mirrors hiding/001's floor-reset Edge Case and hiding/003's FR-003)
- [ ] T012 [US1] Add an EditMode test: simulate entering Hidden at each spot followed by a floor
      reset, and assert state returns to Visible with dampening modifiers cleared at both spots
      (spec.md FR-002)
- [ ] T013 [US1] Playtest chase usability: run at least one full chase scenario using each Hiding
      Spot and record whether cover felt usable without bypassing the floor's key/door objective
      (spec.md SC-002)

**Checkpoint**: User Story 1 is independently complete — both hiding spots are valid, wired to
the shared immunity/dampening system, and correctly restored on reset.

---

## Phase 3: Polish & Cross-Cutting Concerns

- [ ] T014 Once spec 001's dead-end audit (its `tasks.md` T013) is finalized, cross-check both
      Hiding Spot positions against it, confirming neither spot's entry/exit sits on an
      unresolved dead-end's single egress
- [ ] T015 Once spec 002's Monster Spawn Presets and Patrol Waypoints have landed, confirm neither
      Hiding Spot coincides with a patrol waypoint or spawn preset position
- [ ] T016 Record the T010/T012 test results and the T013 playtest outcome in this feature's
      `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **User Story 1 (Phase 2)**: sequential.
- Within User Story 1, T002–T003 are `[P]` (different `HidingSpot` instances); T004 onward
  depends on both existing.
- **Polish (Phase 3)**: after User Story 1; T014/T015 additionally depend on specs 001/002's own
  forward-looking audits landing.

```text
Setup (T001)
   ↓
US1 spots (T002-T003, parallel)
   ↓
US1 wiring & validation (T004-T013)
   ↓
Polish (T014-T016, T014/T015 deferred)
```

## Notes

- [P] tasks (T002–T003) place independent `HidingSpot` instances with no dependency on an
  incomplete task.
- T014 and T015 are explicit forward-dependency tasks — mark them done only once the referenced
  sibling spec's audit/content exists, per the same pattern spec 001's own `tasks.md` uses for its
  T015 and Floor 50's parallel specs use for their own cross-spec re-checks.
- This spec has no new pure-logic C# — it places instances of the `HidingSpot` contract already
  owned by `specs/systems/hiding/001` and consumes `002`/`003`'s existing immunity/dampening logic
  unchanged, and is otherwise Unity Editor placement in `Assets/Scenes/Floor51.unity`, validated
  by the manual walkthroughs/playtest above and the structural EditMode tests in T010/T012, per
  constitution Principle IV's exception.
