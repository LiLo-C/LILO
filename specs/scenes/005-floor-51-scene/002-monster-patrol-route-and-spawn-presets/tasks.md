---
description: "Task list for Floor 51 Monster Patrol Route and Spawn Presets"
---

# Tasks: Floor 51 Monster Patrol Route and Spawn Presets

**Input**: Design documents from
`specs/scenes/005-floor-51-scene/002-monster-patrol-route-and-spawn-presets/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry` (the `Checkpoint`, `SafeArea`,
`ExplorationZone`, `KeyArea`, `LockedDoorApproach`, `ChaseSection` Areas must already exist in
`Assets/Scenes/Floor51.unity`)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (spawn-preset
selection distribution, resolved tuning values, deterministic reset behavior). Patrol-route quality
(does it apply pressure without cornering the player) is level-design judgment validated by manual
walkthrough, per constitution Principle IV's live-scene exception — matching spec 001's own
testing approach.

**Organization**: spec.md has a single user story (US1). Tasks below are grouped into that one
story's constituent content clusters — spawn presets, patrol route, tuning-profile wiring, and
reset behavior — each broken into the smallest independently-placeable/verifiable unit, per
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/tasks.md`'s target granularity.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/objects, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/LevelDesign/MonsterSpawnPresetMarker.cs`: a thin
      `MonoBehaviour` with `public string presetId;` tagging a `GameObject` in
      `Assets/Scenes/Floor51.unity` as a candidate Monster Spawn Preset (no gameplay logic —
      tagging only, mirroring `Floor51AreaMarker`'s pattern from spec 001)
- [ ] T002 Create `Assets/Scripts/MonoBehaviours/LevelDesign/MonsterPatrolWaypointMarker.cs`: a
      thin `MonoBehaviour` with `public int sequenceIndex;` tagging a `GameObject` as one node in
      the fixed patrol route, ordered by `sequenceIndex`
- [ ] T003 Create `Assets/Tests/EditMode/LevelDesign/Floor51MonsterContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` (reuse the folder if spec 001 already
      created it)

**Checkpoint**: Marker components and the test file exist for blockout and validation to
reference.

---

## Phase 2: Foundational — Confirm Floor Identifier Wiring

**⚠️ MUST complete before the tuning-profile tasks below.**

- [ ] T004 In `Assets/Scenes/Floor51.unity`, confirm the scene's `GameManager`/floor-identifier
      configuration (per `specs/systems/shared-config-and-state/002-shared-game-state-and-manager`)
      is set to the Floor 51 identifier, not Floor 50's or an unset default — this is what makes
      spec.md FR-001 resolvable at all

**Checkpoint**: Floor 51's scene correctly reports its own floor identifier.

---

## Phase 3: User Story 1 — Monster Spawn Presets (Priority: P1)

**Goal**: At least two validated Monster Spawn Presets exist; floor load/respawn picks one.

**Independent Test**: Reload the floor repeatedly in the Editor and confirm the chosen preset is
always one of the authored set and is reachable, separated from `Checkpoint`, and outside forbidden
geometry (spec.md FR-002).

- [ ] T005 [P] [US1] Place Monster Spawn Preset "A" in `Assets/Scenes/Floor51.unity` (reachable,
      separated from `Checkpoint`, outside any forbidden/danger geometry, per
      `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`'s rules), tag its root
      `GameObject` with `MonsterSpawnPresetMarker(presetId: "A")`
- [ ] T006 [P] [US1] Place Monster Spawn Preset "B", same validity rules, in a location spatially
      distinct from Preset "A", tag with `MonsterSpawnPresetMarker(presetId: "B")`
- [ ] T007 [US1] Wire the Floor 51 `MonsterController` adapter's floor-load and
      respawn-after-death startup path to select one of the tagged `MonsterSpawnPresetMarker`
      `GameObject`s and set it as `Monster`'s starting `Patrol` position (spec.md FR-002)
- [ ] T008 [US1] Create the `Assets/Tests/EditMode/LevelDesign/Floor51MonsterContentTests.cs` test:
      simulate 100+ floor-load/reset selections against the two tagged presets and assert every
      selection is one of the authored set and passes the minimum-distance/visibility rules
      (spec.md SC-001)
- [ ] T009 [US1] Manual walkthrough: once
      `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` exists, re-verify Presets
      "A" and "B" against its rules — mark this task done only after that system spec lands; do not
      block T005–T008 on it

**Checkpoint**: Monster Spawn Presets exist and selection is provably valid.

---

## Phase 4: User Story 1 — Patrol Route (Priority: P1)

**Goal**: One fixed, hand-authored patrol sequence with multiple waypoints, no trap dead-end,
touching the `ExplorationZone`'s intersections and the vicinity of `KeyArea` and
`LockedDoorApproach` without idling on an objective.

**Independent Test**: Run the floor in Play mode, let `Monster` patrol several full circuits, and
confirm no waypoint traps the player and none coincides with an objective.

- [ ] T010 [US1] Author Patrol Waypoint 1 (`MonsterPatrolWaypointMarker(sequenceIndex: 0)`) in
      `ExplorationZone`, near its entrance from `SafeArea`
- [ ] T011 [US1] Author Patrol Waypoint 2 (`sequenceIndex: 1`) at one of `ExplorationZone`'s
      interior intersections
- [ ] T012 [US1] Author Patrol Waypoint 3 (`sequenceIndex: 2`) near `KeyArea`'s entrance/vicinity
      (not its innermost point, per spec 001's dead-end designation for that point)
- [ ] T013 [US1] Author Patrol Waypoint 4 (`sequenceIndex: 3`) back at an `ExplorationZone`
      intersection distinct from Waypoint 2's
- [ ] T014 [US1] Author Patrol Waypoint 5 (`sequenceIndex: 4`) near `LockedDoorApproach`'s
      entrance/vicinity (not its innermost point)
- [ ] T015 [US1] Author Patrol Waypoint 6 (`sequenceIndex: 5`) closing the loop back toward
      Waypoint 1, completing one full circuit with no dead-end along the route
- [ ] T016 [US1] Audit Waypoints 1–6 against `Key`, `Door` (spec 003), and `FloorExitAlcove`
      (spec 006)'s reserved locations; reposition any waypoint found coinciding with one (spec.md
      FR-003 "no trap dead-end")
- [ ] T017 [US1] Audit Waypoints 1–6 against `SafeArea`'s boundary; reposition any waypoint found
      inside it or requiring a path through it
- [ ] T018 [US1] Manual walkthrough: in Play mode, let `Monster` complete at least 3 full patrol
      circuits and confirm it touches `ExplorationZone`'s intersections and the vicinity of
      `KeyArea`/`LockedDoorApproach`, with zero stops on an objective and no dead-end trap
      (spec.md FR-003)

**Checkpoint**: Patrol route exists, is trap-free, and touches every intersection-adjacent area.

---

## Phase 5: User Story 1 — Tuning Profile and Deterministic Reset (Priority: P1)

**Goal**: Exactly one `Monster` resolves the Floor 51 tuning profile, and both the spawn selection
and the patrol-route position reset deterministically on floor reset.

- [ ] T019 [US1] Confirm the Floor 51 `MonsterController` adapter reads
      `investigateDuration`/`chaseHoldDuration`/`searchDuration`/`patrolSpeed`/`chaseSpeed`
      exclusively through `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`'s
      resolved profile for the floor identifier confirmed in T004 — zero literal constants in this
      scene's own scripts (spec.md FR-001)
- [ ] T020 [US1] Add an EditMode test in `Floor51MonsterContentTests.cs` asserting exactly one
      `Monster` `GameObject` exists in `Assets/Scenes/Floor51.unity` and its resolved tuning equals
      exactly `investigateDuration = 4s, chaseHoldDuration = 3s, searchDuration = 6s, patrolSpeed =
      1.0×, chaseSpeed = 1.4×`, and does NOT equal Floor 50's five values (spec.md FR-001)
- [ ] T021 [US1] Wire the patrol route's reset behavior so that, on floor reset (death or reload),
      `Monster`'s current waypoint index resets deterministically to Waypoint 1
      (`sequenceIndex: 0`) rather than resuming mid-route (spec.md FR-003 "deterministic reset
      behavior")
- [ ] T022 [US1] Add an EditMode test simulating a floor reset mid-patrol and asserting the
      waypoint index returns to `sequenceIndex: 0` and `Monster`'s position returns to one of the
      validated Spawn Presets from Phase 3 (spec.md SC-002)

**Checkpoint**: Tuning is provably correct for Floor 51 specifically, and reset is deterministic
for both spawn and route.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T023 Once spec 003's `Key`/`Door` and spec 006's `FloorExitAlcove` content have landed,
      re-run the T016 objective-coincidence audit against their actual placed positions (not just
      spec 001's reserved locations) — mark done only once those specs exist
- [ ] T024 Once spec 001's dead-end audit (its `tasks.md` T013) is finalized, cross-check every
      `MonsterPatrolWaypointMarker` and `MonsterSpawnPresetMarker` position against it, confirming
      zero coincidences with an audited dead-end's single egress (spec 001 tasks.md T015)
- [ ] T025 Playtest route fairness and detection pressure: play several full runs and confirm the
      patrol creates pressure without an unfair instant catch from `Checkpoint`/`SafeArea`
      (spec.md US1 Independent Test)
- [ ] T026 Record the T008/T009/T020/T022 test results, the T023/T024 cross-checks, and the T025
      playtest outcome in this feature's `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **Spawn Presets (Phase 3)**, **Patrol Route (Phase 4)**, and **Tuning/Reset (Phase 5)** all
  depend only on Setup/Foundational and spec 001's Areas already existing — T005–T009,
  T010–T018, and T019–T022 can proceed in parallel once Phase 2 is done (different concerns), but
  T021–T022 also depend on the waypoint sequence from Phase 4 existing.
- **Polish (Phase 6)** depends on all of Phases 3–5, and T023/T024 additionally depend on specs
  003/006/001's own forward-looking audits landing.

```text
Setup (T001-T003)
   ↓
Foundational (T004)
   ↓
   ├──> Spawn Presets (T005-T009)
   ├──> Patrol Route (T010-T018)
   └──> Tuning/Reset (T019-T022, depends on T010-T018 for waypoint sequence)
                           ↓
                      Polish (T023-T026)
```

## Notes

- [P] tasks (T005–T006) place independent preset markers with no dependency on an incomplete task.
- T009, T023, and T024 are explicit forward-dependency tasks — mark them done only once the
  referenced system/sibling spec exists, per the same pattern spec 001's own `tasks.md` uses for
  its T017/T022 and Floor 50's parallel spec uses for its T010/T019.
- This spec has no pure-logic C# beyond the two marker tag components (T001–T002) and the
  wiring/tests in T007–T008, T019–T022 — the bulk of the work is Unity Editor content placement in
  `Assets/Scenes/Floor51.unity`, validated by manual walkthrough per constitution Principle IV's
  exception for content needing a live scene to judge.
