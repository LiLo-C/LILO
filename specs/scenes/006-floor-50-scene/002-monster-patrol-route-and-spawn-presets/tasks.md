---
description: "Task list for Floor 50 Monster Patrol Route & Spawn Presets"
---

# Tasks: Floor 50 Monster Patrol Route & Spawn Presets

**Input**: Design documents from
`specs/scenes/006-floor-50-scene/002-monster-patrol-route-and-spawn-presets/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` (Areas must exist in
`Assets/Scenes/Floor50.unity` before this feature's content can be placed)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (random
selection distribution, resolved tuning values, the chase-vs-sprint hard rule at Floor 50's
resolved configuration). Patrol-route quality (does it feel like it touches every loop without
idling on an objective) is level-design judgment validated by manual walkthrough, per constitution
Principle IV's live-scene exception — matching spec 001's own testing approach.

**Organization**: Tasks are grouped by user story from spec.md. US1–US3 are P1 and must all be
complete before this feature is considered playable; US4 is a P2 cross-check that can only run
once spec 001's dead-end audit exists.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1–US4
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/LevelDesign/MonsterSpawnPresetMarker.cs`: a thin
      `MonoBehaviour` with `public string presetId;` tagging a `GameObject` in
      `Assets/Scenes/Floor50.unity` as a candidate Monster Spawn Preset (no gameplay logic —
      tagging only, mirroring `FloorAreaMarker`'s pattern from spec 001)
- [ ] T002 Create `Assets/Scripts/MonoBehaviours/LevelDesign/MonsterPatrolWaypointMarker.cs`: a
      thin `MonoBehaviour` with `public int sequenceIndex;` tagging a `GameObject` as one node in
      the fixed patrol route, ordered by `sequenceIndex`
- [ ] T003 Create `Assets/Tests/EditMode/LevelDesign/Floor50MonsterContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` for this spec's structural/config
      tests

**Checkpoint**: Marker components and the test file exist for blockout and validation to
reference.

---

## Phase 2: Foundational — Confirm Floor Identifier Wiring

**⚠️ MUST complete before User Story 3's tuning checks below.**

- [ ] T004 In `Assets/Scenes/Floor50.unity`, confirm the scene's `GameManager`/floor-identifier
      configuration (per `shared-config-and-state/002-shared-game-state-and-manager`) is set to the
      Floor 50 identifier, not Floor 51's or an unset default — this is what makes spec.md FR-006
      resolvable at all

**Checkpoint**: Floor 50's scene correctly reports its own floor identifier.

---

## Phase 3: User Story 1 - Monster Starts at One of Several Validated Presets (Priority: P1)

**Goal**: At least three validated Monster Spawn Presets exist; floor load picks one uniformly at
random.

**Independent Test**: Reload the floor repeatedly in the Editor and confirm the chosen preset
varies and is always one of the authored set.

### Implementation for User Story 1

- [ ] T005 [P] [US1] Place Monster Spawn Preset "A" in `Assets/Scenes/Floor50.unity` (in a location
      satisfying `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`'s rules:
      reachable, far from Checkpoint, outside initial player sightline, off any objective, no
      instant-death), tag its root `GameObject` with `MonsterSpawnPresetMarker(presetId: "A")`
- [ ] T006 [P] [US1] Place Monster Spawn Preset "B", same validity rules, tag with
      `MonsterSpawnPresetMarker(presetId: "B")`
- [ ] T007 [P] [US1] Place Monster Spawn Preset "C", same validity rules, tag with
      `MonsterSpawnPresetMarker(presetId: "C")`
- [ ] T008 [US1] Wire the Floor 50 `MonsterController` adapter's floor-load/respawn-after-death
      startup path to select one of the tagged `MonsterSpawnPresetMarker` `GameObject`s uniformly
      at random and set it as `Monster`'s starting `Patrol` position (spec.md FR-002)
- [ ] T009 [US1] Create `Assets/Tests/EditMode/LevelDesign/Floor50MonsterContentTests.cs` test:
      simulate 100+ floor-load selections against the three tagged presets and assert every preset
      is chosen at least once and none is chosen 100% of the time (spec.md SC-002)
- [ ] T010 [US1] Manual walkthrough: once
      `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` exists, re-verify each of
      presets A/B/C against its five rules (spec.md Acceptance Scenario 3) — mark this task done
      only after that system spec lands; do not block T005–T009 on it

**Checkpoint**: User Story 1 is independently complete — three validated presets exist and
selection is provably random.

---

## Phase 4: User Story 2 - A Single Fixed Patrol Route Touches Every Loop Without Idling on an Objective (Priority: P1)

**Goal**: One fixed, hand-authored patrol sequence passes near the Exploration Hub and all three
Key Loops, never idling on a Key, Door, or the Final Door alcove, and never entering the Safe Area.

**Independent Test**: Run the floor in Play mode, let `Monster` patrol several full circuits, and
confirm no waypoint coincides with an objective or the Safe Area.

### Implementation for User Story 2

- [ ] T011 [US2] Author a sequence of `MonsterPatrolWaypointMarker`-tagged `GameObject`s (ordered
      by `sequenceIndex`) starting in `ExplorationHub`, passing near `KeyLoopA`'s entrance/vicinity
      (not its innermost Key Area point)
- [ ] T012 [US2] Continue the sequence through `KeyLoopB`'s vicinity, then `KeyLoopC`'s vicinity,
      looping back to `ExplorationHub` to close the circuit
- [ ] T013 [US2] Audit every waypoint placed in T011–T012 against every `Key`, `Door`, and the
      `FinalDoorAlcove`'s reserved location from spec 001; reposition any waypoint found to
      coincide with one (spec.md FR-004)
- [ ] T014 [US2] Audit every waypoint against spec 001's `SafeArea` boundary; reposition any
      waypoint found inside it or requiring a path through it (spec.md FR-005)
- [ ] T015 [US2] Manual walkthrough: in Play mode, let `Monster` complete at least 3 full patrol
      circuits and confirm it passes through/near all three Key Loops and the Exploration Hub each
      time, with zero stops on an objective (spec.md Acceptance Scenario 1–2)

**Checkpoint**: User Story 2 is independently complete — the patrol route exists, touches every
loop, and never idles on an objective or enters the Safe Area.

---

## Phase 5: User Story 3 - Floor 50 Resolves the Aggressive Tuning Profile (Priority: P1)

**Goal**: `Assets/Scenes/Floor50.unity` provably resolves the Floor 50 `MonsterTuningProfile`
(investigateDuration 6s, chaseHoldDuration 5s, searchDuration 8s, patrolSpeed 1.2×, chaseSpeed
1.5×), and the chase-vs-sprint hard rule holds at this floor's resolved configuration.

### Implementation for User Story 3

- [ ] T016 [US3] Confirm the Floor 50 `MonsterController` adapter reads
      `investigateDuration`/`chaseHoldDuration`/`searchDuration`/`patrolSpeed`/`chaseSpeed`
      exclusively through `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`'s
      resolved profile for the floor identifier confirmed in T004 — zero literal constants in this
      scene's own scripts (spec.md FR-006, FR-007)
- [ ] T017 [US3] Add an EditMode test in `Assets/Tests/EditMode/LevelDesign/Floor50MonsterContentTests.cs`
      that loads/queries Floor 50's resolved `MonsterTuningProfile` and asserts it equals exactly
      `investigateDuration = 6, chaseHoldDuration = 5, searchDuration = 8, patrolSpeed = 1.2,
      chaseSpeed = 1.5`, and separately asserts it does NOT equal Floor 51's five values (spec.md
      SC-004)
- [ ] T018 [US3] Add an EditMode test asserting `chaseSpeed (1.5) < GameConfig.sprintMultiplier
      (1.6)` holds for Floor 50's resolved configuration, reusing
      `specs/systems/monster-ai/002-per-floor-tuning-profile`'s existing hard-rule validator rather
      than writing a second comparison (spec.md FR-008, SC-005)

**Checkpoint**: User Story 3 is independently complete — Floor 50's tuning wiring is proven
correct and the hard rule is confirmed at this floor's tightest-in-the-game margin.

---

## Phase 6: User Story 4 - Cross-Check Against Spec 001's Dead-End Audit (Priority: P2)

**Goal**: Once spec 001's dead-end audit (its `tasks.md` T016/T017) is finalized, confirm zero
presets or waypoints from this spec coincide with an audited dead-end.

- [ ] T019 [US4] Once `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry`'s dead-end
      list (its `tasks.md` T016) is finalized, cross-check every `MonsterPatrolWaypointMarker` and
      `MonsterSpawnPresetMarker` position in this scene against it (spec.md FR-009, SC-006) — mark
      done only once that list exists; this task is a mandatory forward re-check, not optional
- [ ] T020 [US4] Record the result of T019 (pass/fail per coincidence found, with fixes applied) in
      this feature's `checklists/requirements.md` Notes section

**Checkpoint**: User Story 4 is independently complete — the bidirectional dead-end guarantee
between specs 001 and 002 is closed.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)**, **User Story 2 (Phase 4)**, and **User Story 3 (Phase 5)** all depend
  only on Setup/Foundational and spec 001's Areas already existing — all three are P1 and should be
  completed before this feature is considered playable, but T005–T010, T011–T015, and T016–T018 can
  proceed in parallel once Phase 2 is done (different files/concerns).
- **User Story 4 (Phase 6)** depends on spec 001's dead-end audit landing — it cannot start before
  that, regardless of how complete Phases 3–5 are.

```text
Setup (T001-T003)
   ↓
Foundational (T004)
   ↓
   ├──> US1 (T005-T010)
   ├──> US2 (T011-T015)
   └──> US3 (T016-T018)
                           ↓
                      US4 (T019-T020, deferred on spec 001's dead-end audit)
```

## Notes

- [P] tasks (T005–T007) place independent preset markers with no dependency on an incomplete task.
- T010 and T019/T020 are explicit forward-dependency tasks — mark them done only once
  `specs/systems/monster-ai/003-spawn-point-validation-rules` and spec 001's dead-end audit
  respectively exist, per the same pattern spec 001's own `tasks.md` uses for its T017/T022.
- This spec has no pure-logic C# beyond the two marker tag components (T001–T002) and the
  wiring/tests in T008–T009 and T016–T018 — the bulk of the work is Unity Editor content placement
  in `Assets/Scenes/Floor50.unity`, validated by manual walkthrough per constitution Principle IV's
  exception for content needing a live scene to judge.
