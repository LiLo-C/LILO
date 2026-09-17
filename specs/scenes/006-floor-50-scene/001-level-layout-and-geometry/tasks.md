---
description: "Task list for Floor 50 Level Layout & Geometry"
---

# Tasks: Floor 50 Level Layout & Geometry

**Input**: Design documents from
`specs/scenes/006-floor-50-scene/001-level-layout-and-geometry/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: Included where the content is structural and scene-graph-checkable in EditMode (marker
presence/uniqueness, connectivity by name). The qualitative content this spec is really about —
escape-route safety while chased, sightline leaks, pacing — is level-design judgment that depends
on a live scene and human play, so per constitution Principle IV's exception it is validated by the
manual walkthrough tasks in Phases 5–7, not by an automated assertion.

**Organization**: Tasks are grouped by user story from spec.md. US1/US2 are P1 and block
everything downstream (specs 002–006 place content into this geometry); US3–US5 are P2/P3
validation passes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1–US5
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/Floor50.unity` (new empty Unity Scene) if it does not yet exist,
      and add it to Build Settings in the correct run order after `Floor51.unity` (per ROADMAP §0
      scene list)
- [ ] T002 Create `Assets/Scripts/MonoBehaviours/LevelDesign/FloorAreaMarker.cs`: a thin
      `MonoBehaviour` with `public FloorAreaKind kind;` where `FloorAreaKind` is a plain C# enum —
      `Checkpoint, SafeArea, ExplorationHub, KeyLoopA, KeyLoopB, KeyLoopC, LoopConfluence,
      ChaseSection, FinalDoorAlcove` — used purely to tag each Area's root `GameObject` in the
      scene hierarchy for later structural validation (no gameplay logic; constitution Principle
      II — no more than the tagging this spec needs)
- [ ] T003 Create `Assets/Tests/EditMode/LevelDesign/` folder for this spec's structural tests

**Checkpoint**: Scene file exists and is buildable; the marker component and enum exist for
blockout to reference.

---

## Phase 2: Foundational — Checkpoint, Safe Area, Exploration Hub

**⚠️ MUST complete before any user story below.**

- [ ] T004 In `Assets/Scenes/Floor50.unity`, block out the `Checkpoint` Area (floor entry/respawn
      point per GDD Ch. 9.1) and tag its root `GameObject` with `FloorAreaMarker(kind:
      Checkpoint)`
- [ ] T005 Block out the `SafeArea` Area immediately downstream of `Checkpoint`, tag its root with
      `FloorAreaMarker(kind: SafeArea)`, and position/scale it so it sits outside the distance and
      sightline bounds spec 002's Monster Spawn Presets will later be validated against (spec.md
      FR-002) — leave a documented placeholder note in the scene (a disabled `TextMesh` or scene
      comment `GameObject` named `NOTE_SafeArea_MonsterExclusion`) for the level designer to
      confirm once spec 002 lands
- [ ] T006 Block out the `ExplorationHub` Area downstream of `SafeArea`, tag its root with
      `FloorAreaMarker(kind: ExplorationHub)`, and shape it so it has at least three distinct
      exits (one per Key Loop, T007–T009)

**Checkpoint**: A player can walk from `Checkpoint` through `SafeArea` into `ExplorationHub` in
the Editor's Play mode using placeholder movement.

---

## Phase 3: User Story 1 - Reach Every Objective Loop Without a Single Forced Path (Priority: P1)

**Goal**: All three Key Loop entrances are reachable directly from the Hub in any order, with at
least one alternate route back.

**Independent Test**: Walk from `Checkpoint` to each of the three loop entrances in turn in Play
mode; confirm none requires passing another loop's entrance area.

### Implementation for User Story 1

- [ ] T007 [P] [US1] Block out `KeyLoopA`'s entrance corridor from `ExplorationHub`, tag its root
      `GameObject` with `FloorAreaMarker(kind: KeyLoopA)`
- [ ] T008 [P] [US1] Block out `KeyLoopB`'s entrance corridor from `ExplorationHub`, tag its root
      with `FloorAreaMarker(kind: KeyLoopB)`
- [ ] T009 [P] [US1] Block out `KeyLoopC`'s entrance corridor from `ExplorationHub`, tag its root
      with `FloorAreaMarker(kind: KeyLoopC)`
- [ ] T010 [US1] Add at least one alternate corridor connecting two of the three loop-entrance
      corridors (or a loop-entrance corridor back to `ExplorationHub` by a second route), directly
      satisfying spec.md FR-004's "no single-corridor" rule
- [ ] T011 [US1] Manual walkthrough: in the Unity Editor Play mode, walk `PlayerCharacter`
      placeholder from `Checkpoint` to each of `KeyLoopA`/`B`/`C` in turn, confirming each is
      reachable without crossing another loop's entrance (spec.md Acceptance Scenario 1), then
      confirm the alternate corridor from T010 provides a second path back to a previously-visited
      area (Acceptance Scenario 2)

**Checkpoint**: User Story 1 is independently complete — the hub-and-spoke topology exists and is
walkable in any order.

---

## Phase 4: User Story 2 - Never Become Inescapably Trapped While Chased (Priority: P1)

**Goal**: Every dead-end sub-area has a second egress, or is an explicitly documented, distance-
justified exception. **This is the floor's single non-negotiable content gate — do not mark this
phase done on a partial audit.**

**Independent Test**: Walk every dead-end (each loop's innermost Key Area point, the Chase
Section's margins) and confirm a second egress exists, or find it listed in spec.md Assumptions
with a distance justification.

### Implementation for User Story 2

- [ ] T012 [P] [US2] Block out `KeyLoopA`'s innermost Key Area point with a second, distinct
      egress back toward `ExplorationHub` or an adjacent loop (spec.md FR-005(a); this loop is
      explicitly NOT listed as an FR-005(b) exception per spec.md Assumptions)
- [ ] T013 [P] [US2] Block out `KeyLoopB`'s innermost Key Area point with a second, distinct
      egress, same rule as T012
- [ ] T014 [P] [US2] Block out `KeyLoopC`'s innermost Key Area point with a second, distinct
      egress, same rule as T012
- [ ] T015 [US2] Block out the Chase Section's margins (T019) with zero unmarked single-entrance
      alcoves — audit every side-recess for a second egress or remove it from the blockout
      entirely (spec.md FR-006, Acceptance Scenario 2)
- [ ] T016 [US2] Full-floor dead-end audit: walk every corridor and sub-area not covered by
      T012–T015, list any additional dead-end found, and for each either add a second egress in
      the scene or add a named, distance-justified exception entry to spec.md's Assumptions
      section (FR-005(b)) — no silent exceptions
- [ ] T017 [US2] Cross-check the full dead-end list from T016 against spec 002's Monster Patrol
      Waypoints once that spec lands, confirming zero dead-ends coincide with a waypoint or
      connecting corridor `Monster` is known to occupy (spec.md Acceptance Scenario 3) — this task
      is a blocking re-check to perform again after spec 002 ships, not a one-time pass

**Checkpoint**: User Story 2 is independently complete and signed off — zero un-justified
inescapable dead-ends exist anywhere in the floor.

---

## Phase 5: User Story 3 - Flow Through Three Loops Into a Shared Chase Section (Priority: P2)

**Goal**: The three loops converge into one shared area with an always-open route into the Chase
Section; no redundant gate is built here (that lock lives entirely in spec 004).

### Implementation for User Story 3

- [ ] T018 [US3] Block out the `LoopConfluence` Area downstream of all three loops, tag its root
      with `FloorAreaMarker(kind: LoopConfluence)`, with three incoming connections (one per loop)
      and one outgoing connection toward the Chase Section — no gate object, trigger, or script on
      this route (spec.md FR-007)
- [ ] T019 [US3] Block out the `ChaseSection` Area downstream of `LoopConfluence` as a single long
      corridor (GDD Ch. 16.2), tag its root with `FloorAreaMarker(kind: ChaseSection)`
- [ ] T020 [US3] Manual walkthrough: confirm the route from each of `KeyLoopA`/`B`/`C` through
      `LoopConfluence` into `ChaseSection` is physically open regardless of how many loops are
      resolved (spec.md Acceptance Scenario 1) — there is nothing to unlock on this route by
      design

**Checkpoint**: User Story 3 is independently complete — the three loops share one confluence and
one Chase Section with no invented secondary lock.

---

## Phase 6: User Story 4 - Complete the Floor in About Five Minutes (Priority: P3)

**Goal**: Confirm pacing once the full floor (all six Floor 50 specs) is playable end to end.

- [ ] T021 [US4] Block out the `FinalDoorAlcove` Area at the end of `ChaseSection`, tag its root
      with `FloorAreaMarker(kind: FinalDoorAlcove)` (geometry reservation only — spec 004 places
      the actual Final Door object here)
- [ ] T022 [US4] Once specs 002–006 have landed their content into this geometry, run a
      checkpoint-to-Final-Door timed playtest with a person unfamiliar with this floor and record
      the duration against `GameConfig.targetFloorDuration` (300s) — record the observation in
      this spec's spec.md Success Criteria SC-005 notes, not as a pass/fail unit test

**Checkpoint**: Pacing observation recorded (deferred until dependent specs exist — do not block
this spec's own completion on it; see Notes).

---

## Phase 7: User Story 5 - Never See Outside the Level (Priority: P3)

- [ ] T023 [US5] Add boundary geometry (walls/skybox occluders) around the full perimeter of the
      blockout from T004–T021 so no reachable player position exposes empty space beyond the level
      (spec.md FR-012)
- [ ] T024 [US5] Manual walkthrough: walk the full perimeter, including every dead-end and each
      loop's innermost point, confirming no camera angle reveals unbuilt space (spec.md Acceptance
      Scenario 1)

**Checkpoint**: User Story 5 is independently complete — the floor has no visible seams.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Create `Assets/Tests/EditMode/LevelDesign/Floor50AreaMarkerTests.cs`: an EditMode
      test that opens `Assets/Scenes/Floor50.unity` additively, finds all `FloorAreaMarker`
      components, and asserts exactly one marker exists for each of the nine `FloorAreaKind`
      values (`Checkpoint, SafeArea, ExplorationHub, KeyLoopA, KeyLoopB, KeyLoopC, LoopConfluence,
      ChaseSection, FinalDoorAlcove`) — a structural regression guard, not a substitute for the
      manual walkthroughs above
- [ ] T026 Run the full GDD Ch. 16.3 validation checklist as a single documented pass against the
      completed blockout, and record the result (checked/not-checked per item, with the FR or
      downstream-spec reference) in this spec's `checklists/requirements.md` Notes section
- [ ] T027 Review the final blockout against spec.md's Assumptions section; confirm the loop
      topology (hub-and-spoke, no order dependency) and the dead-end exception list (if any beyond
      "none currently designated") match what was actually built, and update spec.md if the
      built geometry diverged during blockout

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** and **User Story 2 (Phase 4)** both depend only on Foundational and
  are the two P1 stories every downstream Floor 50 spec (002–006) needs — prioritize both before
  moving to Phase 5.
- **User Story 3 (Phase 5)** depends on US1's loop entrances existing (T007–T009).
- **User Story 4 (Phase 6)** depends on all of specs 002–006 existing — T022 is a deferred
  checkpoint, not a blocker for this spec's own sign-off.
- **User Story 5 (Phase 7)** depends on the full blockout (T004–T021) existing.
- **Polish (Phase 8)**: after all stories.

```text
Setup (T001-T003)
   ↓
Foundational (T004-T006)
   ↓
   ├──> US1 (T007-T011) ──> US3 (T018-T020) ──> US4 (T021-T022, T022 deferred)
   └──> US2 (T012-T017)
                           ↓
                      US5 (T023-T024)
                           ↓
                      Polish (T025-T027)
```

## Notes

- [P] tasks touch different loop corridors/areas with no dependency on an incomplete task.
- T022 (pacing timing) and T017's re-check (against spec 002) are explicitly cross-spec
  dependencies — mark them done only once the referenced spec exists; do not block this spec's own
  Phase 1–3, 5, 7 sign-off on specs that haven't landed yet.
- This spec has no pure-logic C# beyond the `FloorAreaMarker` tag component — the vast majority of
  the work here is Unity Editor blockout authored directly in `Assets/Scenes/Floor50.unity`,
  validated by manual walkthrough and the one structural EditMode test in T025, per constitution
  Principle IV's exception for scene/content that needs a live scene to judge.
