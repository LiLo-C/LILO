---
description: "Task list for Floor 51 Level Layout & Geometry"
---

# Tasks: Floor 51 Level Layout & Geometry

**Input**: Design documents from
`specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: Included where the content is structural and scene-graph-checkable in EditMode (marker
presence/uniqueness, connectivity by name). The qualitative content this spec is really about —
escape-route safety while chased, sightline leaks, pacing — is level-design judgment that depends on
a live scene and human play, so per constitution Principle IV's exception it is validated by the
manual walkthrough tasks in Phases 5–7, not by an automated assertion.

**Organization**: Tasks are grouped by user story from spec.md. US1/US2 are P1 and block everything
downstream (specs 002–006 place content into this geometry); US3–US5 are P2/P3 validation passes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1–US5
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/Floor51.unity` (new empty Unity Scene) if it does not yet exist,
      and add it to Build Settings in the correct run order after `Floor52.unity` and before
      `Floor50.unity` (per ROADMAP §0 scene list)
- [ ] T002 Create `Assets/Scripts/MonoBehaviours/LevelDesign/Floor51AreaMarker.cs`: a thin
      `MonoBehaviour` with `public Floor51AreaKind kind;` where `Floor51AreaKind` is a plain C# enum
      — `Checkpoint, SafeArea, ExplorationZone, KeyArea, LockedDoorApproach, ChaseSection,
      FloorExitAlcove` — used purely to tag each Area's root `GameObject` in the scene hierarchy for
      later structural validation (no gameplay logic; constitution Principle II — no more than the
      tagging this spec needs). Named `Floor51AreaMarker`/`Floor51AreaKind` (not a shared
      `FloorAreaMarker`) to avoid silently repurposing Floor 50's own scene-specific marker/enum
      pair, per constitution Principle V.
- [ ] T003 Create `Assets/Tests/EditMode/LevelDesign/` folder for this spec's structural tests (skip
      if already created by another floor's equivalent spec)

**Checkpoint**: Scene file exists and is buildable; the marker component and enum exist for
blockout to reference.

---

## Phase 2: Foundational — Checkpoint, Safe Area, Exploration Zone

**⚠️ MUST complete before any user story below.**

- [ ] T004 In `Assets/Scenes/Floor51.unity`, block out the `Checkpoint` Area (floor entry/respawn
      point per GDD Ch. 9.1) and tag its root `GameObject` with `Floor51AreaMarker(kind:
      Checkpoint)`
- [ ] T005 Block out the `SafeArea` Area immediately downstream of `Checkpoint`, tag its root with
      `Floor51AreaMarker(kind: SafeArea)`, and position/scale it so it sits outside the distance and
      sightline bounds spec 002's Monster Spawn Presets will later be validated against (spec.md
      FR-002) — leave a documented placeholder note in the scene (a disabled `TextMesh` or scene
      comment `GameObject` named `NOTE_SafeArea_MonsterExclusion`) for the level designer to confirm
      once spec 002 lands
- [ ] T006 Block out the `ExplorationZone` Area downstream of `SafeArea`, tag its root with
      `Floor51AreaMarker(kind: ExplorationZone)`, and shape it with at least three distinct
      intersections (spec.md FR-004) — points where three or more corridors meet — rather than a
      single branching corridor

**Checkpoint**: A player can walk from `Checkpoint` through `SafeArea` into `ExplorationZone` in the
Editor's Play mode using placeholder movement, and can already choose between multiple corridors at
more than one intersection.

---

## Phase 3: User Story 1 - Reach the Key Without Ever Needing the Locked Door First (Priority: P1)

**Goal**: The Key Area and the Locked Door Approach both branch directly off the Exploration Zone,
with the Key Area always reachable without crossing the Locked Door.

**Independent Test**: Walk from `Checkpoint` to the `KeyArea` in Play mode; confirm the route never
requires crossing `LockedDoorApproach`'s gated boundary.

### Implementation for User Story 1

- [ ] T007 [US1] Block out `KeyArea`'s entrance corridor from `ExplorationZone`, tag its root
      `GameObject` with `Floor51AreaMarker(kind: KeyArea)`
- [ ] T008 [US1] Block out `LockedDoorApproach`'s entrance corridor from `ExplorationZone`,
      independently of `KeyArea`'s corridor (T007), tag its root with `Floor51AreaMarker(kind:
      LockedDoorApproach)`
- [ ] T009 [US1] Add at least one alternate corridor connecting `KeyArea`'s approach and
      `LockedDoorApproach`'s approach directly (not only via `ExplorationZone`), satisfying spec.md
      FR-003/Acceptance Scenario 2 ("no single there-and-back corridor")
- [ ] T010 [US1] Manual walkthrough: in the Unity Editor Play mode, walk `PlayerCharacter`
      placeholder from `Checkpoint` to `KeyArea`, confirming it is reachable without crossing
      `LockedDoorApproach`'s gated boundary (spec.md Acceptance Scenario 1), then confirm the
      alternate corridor from T009 provides a second path between the two areas (Acceptance
      Scenario 2)

**Checkpoint**: User Story 1 is independently complete — the key-before-door topology exists and is
walkable.

---

## Phase 4: User Story 2 - Never Become Inescapably Trapped While Chased (Priority: P1)

**Goal**: Every dead-end or single-entrance sub-area has a second egress, or is an explicitly
documented, distance-justified exception. **This is the floor's single non-negotiable content
gate — do not mark this phase done on a partial audit.**

**Independent Test**: Walk every dead-end (the Key Area's innermost point, the Locked Door
Approach's innermost point, any narrow intersection spur, the Chase Section's margins) and confirm
a second egress exists, or find it listed in spec.md Assumptions with a distance justification.

### Implementation for User Story 2

- [ ] T011 [US2] Block out `KeyArea`'s innermost point with a second, distinct egress back toward
      `ExplorationZone` or `LockedDoorApproach` (spec.md FR-005(a); this area is explicitly NOT
      listed as an FR-005(b) exception per spec.md Assumptions)
- [ ] T012 [US2] Block out `LockedDoorApproach`'s innermost point with a second, distinct egress,
      same rule as T011
- [ ] T013 [US2] Audit every intersection spur created in T006/T009 for a dead end with only one
      egress; for each one found, either add a second egress in the scene or add a named,
      distance-justified exception entry to spec.md's Assumptions section (FR-005(b)) — no silent
      exceptions
- [ ] T014 [US2] Block out the Chase Section's margins (T019) with zero unmarked single-entrance
      alcoves — audit every side-recess for a second egress or remove it from the blockout entirely
      (spec.md FR-006, Acceptance Scenario 2)
- [ ] T015 [US2] Cross-check the full dead-end list from T011–T014 against spec 002's Monster
      Patrol Waypoints and Spawn Presets once that spec lands, confirming zero dead-ends coincide
      with a waypoint, preset, or connecting corridor `Monster` is known to occupy (spec.md
      Acceptance Scenario 3) — this task is a blocking re-check to perform again after spec 002
      ships, not a one-time pass

**Checkpoint**: User Story 2 is independently complete and signed off — zero un-justified
inescapable dead-ends exist anywhere in the floor.

---

## Phase 5: User Story 3 - Intersection-Heavy Zone Funneling to One Door and Chase Section (Priority: P2)

**Goal**: The Exploration Zone is genuinely intersection-rich, and both the Key Area and the Locked
Door Approach converge into one Chase Section once the door is unlocked; no redundant gate is built
here (that lock lives entirely in spec 003).

### Implementation for User Story 3

- [ ] T016 [US3] Count and confirm the Exploration Zone contains at least three distinct
      intersections (spec.md FR-004) — if T006/T009 fall short, add corridors until the count is
      met
- [ ] T017 [US3] Block out the `ChaseSection` Area downstream of `LockedDoorApproach` as a single
      corridor (GDD Ch. 16.2), tag its root with `Floor51AreaMarker(kind: ChaseSection)`, with
      exactly one incoming connection gated by the (future) Locked Door and no other physical
      blocker on that route (spec.md FR-007)
- [ ] T018 [US3] Manual walkthrough: confirm the route from `KeyArea` and from `LockedDoorApproach`
      into `ChaseSection` converges into the same corridor, with nothing to unlock on this route by
      design beyond the door itself (spec.md Acceptance Scenario 2)

**Checkpoint**: User Story 3 is independently complete — the intersection-heavy Exploration Zone and
single Chase Section both exist with no invented secondary lock.

---

## Phase 6: User Story 4 - Complete the Floor in About Five Minutes (Priority: P3)

**Goal**: Confirm pacing once the full floor (all six Floor 51 specs) is playable end to end.

- [ ] T019 [US4] Block out the `FloorExitAlcove` Area at the end of `ChaseSection`, tag its root
      with `Floor51AreaMarker(kind: FloorExitAlcove)` (geometry reservation only — spec 006 places
      the actual Floor Exit Door object here)
- [ ] T020 [US4] Once specs 002–006 have landed their content into this geometry, run a
      checkpoint-to-Floor-Exit-Door timed playtest with a person unfamiliar with this floor and
      record the duration against `GameConfig.targetFloorDuration` (300s) — record the observation
      in this spec's spec.md Success Criteria SC-005 notes, not as a pass/fail unit test

**Checkpoint**: Pacing observation recorded (deferred until dependent specs exist — do not block
this spec's own completion on it; see Notes).

---

## Phase 7: User Story 5 - Never See Outside the Level (Priority: P3)

- [ ] T021 [US5] Add boundary geometry (walls/skybox occluders) around the full perimeter of the
      blockout from T004–T019 so no reachable player position exposes empty space beyond the level
      (spec.md FR-012)
- [ ] T022 [US5] Manual walkthrough: walk the full perimeter, including every intersection spur and
      the Key Area's/Locked Door Approach's innermost points, confirming no camera angle reveals
      unbuilt space (spec.md Acceptance Scenario 1)

**Checkpoint**: User Story 5 is independently complete — the floor has no visible seams.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T023 [P] Create `Assets/Tests/EditMode/LevelDesign/Floor51AreaMarkerTests.cs`: an EditMode
      test that opens `Assets/Scenes/Floor51.unity` additively, finds all `Floor51AreaMarker`
      components, and asserts exactly one marker exists for each of the seven `Floor51AreaKind`
      values (`Checkpoint, SafeArea, ExplorationZone, KeyArea, LockedDoorApproach, ChaseSection,
      FloorExitAlcove`) — a structural regression guard, not a substitute for the manual
      walkthroughs above
- [ ] T024 Run the full GDD Ch. 16.3 validation checklist as a single documented pass against the
      completed blockout, and record the result (checked/not-checked per item, with the FR or
      downstream-spec reference) in this spec's `checklists/requirements.md` Notes section
- [ ] T025 Review the final blockout against spec.md's Assumptions section; confirm the topology
      (key reachable before the door, no order dependency) and the dead-end exception list (if any
      beyond "none currently designated") match what was actually built, and update spec.md if the
      built geometry diverged during blockout

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** and **User Story 2 (Phase 4)** both depend only on Foundational and are
  the two P1 stories every downstream Floor 51 spec (002–006) needs — prioritize both before moving
  to Phase 5.
- **User Story 3 (Phase 5)** depends on US1's Key Area/Locked Door Approach entrances existing
  (T007–T009).
- **User Story 4 (Phase 6)** depends on all of specs 002–006 existing — T020 is a deferred
  checkpoint, not a blocker for this spec's own sign-off.
- **User Story 5 (Phase 7)** depends on the full blockout (T004–T019) existing.
- **Polish (Phase 8)**: after all stories.

```text
Setup (T001-T003)
   ↓
Foundational (T004-T006)
   ↓
   ├──> US1 (T007-T010) ──> US3 (T016-T018) ──> US4 (T019-T020, T020 deferred)
   └──> US2 (T011-T015)
                           ↓
                      US5 (T021-T022)
                           ↓
                      Polish (T023-T025)
```

## Notes

- [P] tasks touch different files/areas with no dependency on an incomplete task; most tasks in
  this spec are sequential blockout steps on the same scene file and are intentionally not marked
  [P].
- T020 (pacing timing) and T015's re-check (against spec 002) are explicitly cross-spec
  dependencies — mark them done only once the referenced spec exists; do not block this spec's own
  Phase 1–3, 5, 7 sign-off on specs that haven't landed yet.
- This spec has no pure-logic C# beyond the `Floor51AreaMarker` tag component — the vast majority of
  the work here is Unity Editor blockout authored directly in `Assets/Scenes/Floor51.unity`,
  validated by manual walkthrough and the one structural EditMode test in T023, per constitution
  Principle IV's exception for scene/content that needs a live scene to judge.
