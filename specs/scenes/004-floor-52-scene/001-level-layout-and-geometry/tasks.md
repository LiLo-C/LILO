---
description: "Task list for Floor 52 Level Layout & Geometry"
---

# Tasks: Floor 52 Level Layout & Geometry

**Input**: Design documents from
`specs/scenes/004-floor-52-scene/001-level-layout-and-geometry/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: Included where the content is structural and scene-graph-checkable in EditMode (marker
presence/uniqueness, connectivity by name). The qualitative content this spec is really about —
branching feel, dead-end hygiene, pacing, sightline leaks — is level-design judgment that depends
on a live scene and human play, so per constitution Principle IV's exception it is validated by the
manual walkthrough tasks in Phases 3–8, not by an automated assertion.

**Organization**: Tasks are grouped by user story from spec.md. US1–US3 are P1 and block
everything downstream (specs 002–005 place content into this geometry); US4–US6 are P3 validation
passes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1–US6
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/Floor52.unity` (new empty Unity Scene) if it does not yet exist,
      and add it to Build Settings directly after `Prologue.unity` and before `Floor51.unity` (per
      ROADMAP.md §0 scene list)
- [ ] T002 Create `Assets/Scripts/MonoBehaviours/LevelDesign/FloorAreaMarker.cs`: a thin
      `MonoBehaviour` with `public FloorAreaKind kind;` where `FloorAreaKind` is a plain C# enum —
      `Checkpoint, SafeArea, ExplorationZone, DistantZone, ChaseSection, ExitDoorAlcove` — used
      purely to tag each Area's root `GameObject` in the scene hierarchy for later structural
      validation (no gameplay logic; constitution Principle II — no more than the tagging this
      spec needs). Reuse the existing type if a prior Floor 50/51 spec already created this file —
      only add the new enum values this spec introduces.
- [ ] T003 Create `Assets/Tests/EditMode/LevelDesign/` folder for this spec's structural tests if
      it does not already exist

**Checkpoint**: Scene file exists and is buildable; the marker component and enum exist for
blockout to reference.

---

## Phase 2: Foundational — Checkpoint, Safe Area, Exploration Zone

**⚠️ MUST complete before any user story below.**

- [ ] T004 In `Assets/Scenes/Floor52.unity`, block out the `Checkpoint` Area (floor entry/respawn
      point per GDD Ch. 9.1) and tag its root `GameObject` with `FloorAreaMarker(kind:
      Checkpoint)`
- [ ] T005 Block out the `SafeArea` Area immediately downstream of `Checkpoint`, tag its root with
      `FloorAreaMarker(kind: SafeArea)`, and dress it with full ambient light and zero
      monster-related audio sources (spec.md FR-002) — this is Onboarding Beat 1's room
- [ ] T006 Block out the `ExplorationZone` Area downstream of `SafeArea`, tag its root with
      `FloorAreaMarker(kind: ExplorationZone)`, and shape it as a small number of wide corridors
      (spec.md FR-003) rather than a maze

**Checkpoint**: A player can walk from `Checkpoint` through `SafeArea` into `ExplorationZone` in
the Editor's Play mode using placeholder movement.

---

## Phase 3: User Story 1 - Walk the Full Blueprint With No Locked Door or Key Area (Priority: P1)

**Goal**: The floor's top-level area graph matches GDD Ch. 16.2 with the Locked Door → Key Area
section omitted.

**Independent Test**: Walk `Checkpoint` → `SafeArea` → `ExplorationZone` → `ChaseSection` →
`ExitDoorAlcove` and confirm no Locked Door or Key Area object exists anywhere in the scene.

### Implementation for User Story 1

- [ ] T007 [US1] Block out the `ChaseSection` Area downstream of `ExplorationZone` as a single long
      corridor (GDD Ch. 16.2, Ch. 15.4 Beat 5), tag its root with `FloorAreaMarker(kind:
      ChaseSection)`, and confirm it has no side branches (spec.md FR-007)
- [ ] T008 [US1] Block out the `ExitDoorAlcove` Area at the end of `ChaseSection`, tag its root
      with `FloorAreaMarker(kind: ExitDoorAlcove)` (geometry reservation only — spec 005 places the
      actual `Door` object here, spec.md FR-008)
- [ ] T009 [US1] Manual walkthrough: in the Unity Editor Play mode, walk `PlayerCharacter`
      placeholder from `Checkpoint` to `ExitDoorAlcove` and confirm the path never requires a key
      and never passes through a locked-door object (spec.md Acceptance Scenario 1)
- [ ] T010 [US1] Review the completed area graph against the generic GDD Ch. 16.2 blueprint and
      confirm it matches exactly except for the deliberate Locked Door → Key Area omission
      (spec.md Acceptance Scenario 2)

**Checkpoint**: User Story 1 is independently complete — the full backbone exists with zero locked
-door content.

---

## Phase 4: User Story 2 - Wide, Low-Branching Corridors That Still Offer One Real Choice (Priority: P1)

**Goal**: A visibly low branch-point count overall, with at least one genuine alternate route that
never bypasses a later onboarding beat.

**Independent Test**: Count `ExplorationZone`'s branch points (should be a handful, not many);
confirm at least one alternate route exists and stays local to the zone.

### Implementation for User Story 2

- [ ] T011 [US2] Add exactly one short alternate-route loop within `ExplorationZone` (e.g., two
      parallel corridors around one central room) satisfying spec.md FR-003's "at least one
      alternate route" without turning the zone into a maze
- [ ] T012 [US2] Confirm the loop from T011 stays entirely within `ExplorationZone`'s tagged root
      and does not connect directly into `ChaseSection` or `ExitDoorAlcove` — it must not let a
      player skip ahead of a later onboarding beat (spec.md FR-011, Acceptance Scenario 3)
- [ ] T013 [US2] Manual walkthrough: count and record `ExplorationZone`'s branch points (target: a
      handful, consistent with GDD Ch. 3 "sedikit percabangan"), and walk the T011 loop to confirm
      it offers a genuine second route between two already-visited points (spec.md Acceptance
      Scenario 1–2)

**Checkpoint**: User Story 2 is independently complete — the branching-feel tension (few branches
vs. at least one alternate route) is resolved and verified in the actual blockout.

---

## Phase 5: User Story 3 - Reserve Every Slot the Onboarding Beats and Content Specs Need (Priority: P1)

**Goal**: 3–5 spread-out Battery candidate slots, one hiding-spot nook on the critical path, and
one `DistantZone` dressing element for the audio hint — all reserved without any downstream spec
needing to alter this spec's geometry.

**Independent Test**: Inspect the blockout and count Battery candidate slots, confirm the hiding
nook sits on the critical path, and confirm a `DistantZone` element exists.

### Implementation for User Story 3

- [ ] T014 [P] [US3] Mark between 3 and 5 distinct, spatially separated Battery candidate
      locations inside `ExplorationZone` and along `ChaseSection`, spread across more than one side
      of the map (spec.md FR-004), each as an empty placeholder `GameObject` named
      `BatteryCandidate_<n>` (no `Battery` component yet — spec 003 instantiates the real object)
- [ ] T015 [US3] Mark exactly one under-desk-sized nook directly on the floor's critical path
      (inside `ExplorationZone`, not inside the T011 optional loop) as an empty placeholder
      `GameObject` named `HidingSpotCandidate` (spec.md FR-005)
- [ ] T016 [US3] Block out a `DistantZone` dressing element (an unopenable door, unlit side
      corridor, or duct/vent) reachable-by-sightline-only from somewhere in `ExplorationZone` or
      `ChaseSection`, tag its root with `FloorAreaMarker(kind: DistantZone)` (spec.md FR-006) — no
      `Monster` GameObject is placed here or anywhere else in this scene
- [ ] T017 [US3] Manual walkthrough: confirm the Battery candidate count is between 3 and 5 and
      spread across more than one side of the map, the hiding nook sits on the unavoidable critical
      path, and the `DistantZone` reads spatially as "elsewhere in the building" (spec.md
      Acceptance Scenarios 1–3)

**Checkpoint**: User Story 3 is independently complete — every downstream Floor 52 content spec
(002–005) has a valid, reserved place to put its content.

---

## Phase 6: User Story 4 - No Dead-End Ever Reads as a Trap (Priority: P3)

**Goal**: Every dead-end pocket either has a second egress or is shallow enough to be a short
backtrack, applied as design hygiene even though no monster exists on this floor.

### Implementation for User Story 4

- [ ] T018 [US4] Full-zone dead-end audit: walk every side alcove and the T011 loop's innermost
      point, and for each confirm either a second egress exists or the pocket is shallow enough
      that rejoining the critical path never takes more than a short backtrack (spec.md FR-010)
- [ ] T019 [US4] Record the audit result in this spec's `checklists/requirements.md` Notes,
      explicitly noting Ch. 16.3 checklist item 2 is satisfied vacuously (no monster exists to
      chase the player on Floor 52) while the hygiene standard is still held (spec.md Acceptance
      Scenario 2)

**Checkpoint**: User Story 4 is independently complete.

---

## Phase 7: User Story 5 - Finish in About Five Minutes (Priority: P3)

- [ ] T020 [US5] Once specs 002–005 have landed their content into this geometry, run a
      checkpoint-to-exit-door timed playtest with a person unfamiliar with LILO and record the
      duration against `GameConfig.targetFloorDuration` (300s) in this spec's spec.md Success
      Criteria SC-005 notes, not as a pass/fail unit test

**Checkpoint**: Pacing observation recorded (deferred until dependent specs exist — do not block
this spec's own completion on it; see Notes).

---

## Phase 8: User Story 6 - Never See Outside the Level (Priority: P3)

- [ ] T021 [US6] Add boundary geometry (walls/skybox occluders) around the full perimeter of the
      blockout from T004–T016 so no reachable player position exposes empty space beyond the level
      (spec.md FR-012)
- [ ] T022 [US6] Manual walkthrough: walk the full perimeter, including every dead-end and the
      alternate-route loop's innermost point, confirming no camera angle reveals unbuilt space
      (spec.md Acceptance Scenario 1) — cite
      `specs/systems/movement-and-camera/003-camera-follow-and-boundary-clamp/spec.md` for the
      clamp mechanism this depends on

**Checkpoint**: User Story 6 is independently complete — the floor has no visible seams.

---

## Phase 9: Polish & Cross-Cutting Concerns

- [ ] T023 [P] Create `Assets/Tests/EditMode/LevelDesign/Floor52AreaMarkerTests.cs`: an EditMode
      test that opens `Assets/Scenes/Floor52.unity` additively, finds all `FloorAreaMarker`
      components, and asserts exactly one marker exists for each of the six `FloorAreaKind` values
      used by this spec (`Checkpoint, SafeArea, ExplorationZone, DistantZone, ChaseSection,
      ExitDoorAlcove`) — a structural regression guard, not a substitute for the manual
      walkthroughs above
- [ ] T024 Run the full GDD Ch. 16.3 validation checklist as a single documented pass against the
      completed blockout, and record the result (checked/not-checked/N/A-by-design per item, with
      the FR or downstream-spec reference) in this spec's `checklists/requirements.md` Notes
      section
- [ ] T025 Review the final blockout against spec.md's Assumptions section; confirm the
      alternate-route loop shape and the dead-end exception list (if any beyond "audited under
      FR-010") match what was actually built, and update spec.md if the built geometry diverged
      during blockout

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)**, **User Story 2 (Phase 4)**, and **User Story 3 (Phase 5)** all
  depend only on Foundational and are the three P1 stories every downstream Floor 52 spec
  (002–005) needs — prioritize all three before moving to Phase 6.
- **User Story 4 (Phase 6)** depends on the full blockout from Phases 3–5 existing (it audits
  dead-ends across the whole zone, including the US2 loop and US3's candidate slots).
- **User Story 5 (Phase 7)** depends on specs 002–005 existing — T020 is a deferred checkpoint, not
  a blocker for this spec's own sign-off.
- **User Story 6 (Phase 8)** depends on the full blockout (T004–T016) existing.
- **Polish (Phase 9)**: after all stories.

```text
Setup (T001-T003)
   ↓
Foundational (T004-T006)
   ↓
   ├──> US1 (T007-T010) ─┐
   ├──> US2 (T011-T013) ─┼──> US4 (T018-T019)
   └──> US3 (T014-T017) ─┘         ↓
                                US6 (T021-T022)
                                   ↓
                              US5 (T020, deferred)
                                   ↓
                              Polish (T023-T025)
```

## Notes

- [P] tasks touch different areas/candidate slots with no dependency on an incomplete task.
- T020 (pacing timing) is an explicitly cross-spec dependency — mark it done only once specs
  002–005 exist; do not block this spec's own Phase 1–6, 8 sign-off on specs that haven't landed
  yet.
- This spec has no pure-logic C# beyond the `FloorAreaMarker` tag component — the vast majority of
  the work here is Unity Editor blockout authored directly in `Assets/Scenes/Floor52.unity`,
  validated by manual walkthrough and the one structural EditMode test in T023, per constitution
  Principle IV's exception for scene/content that needs a live scene to judge.
- If `FloorAreaMarker.cs` and its `FloorAreaKind` enum already exist from a Floor 50/51 spec by the
  time this spec is implemented, extend the enum rather than creating a second marker type
  (constitution Principle II).
