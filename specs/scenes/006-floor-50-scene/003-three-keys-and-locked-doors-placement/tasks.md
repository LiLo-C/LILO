---
description: "Task list for Floor 50 Three Keys and Locked Doors Placement"
---

# Tasks: Floor 50 Three Keys and Locked Doors Placement

**Input**: Design documents from
`specs/scenes/006-floor-50-scene/003-three-keys-and-locked-doors-placement/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` (`KeyLoopA`/`KeyLoopB`/`KeyLoopC`
Areas, each with a Key Area and Locked Door opening, must exist in `Assets/Scenes/Floor50.unity`
before this feature's content can be placed)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (count/ID
matching, reachability audit, reset behavior). Whether the full three-key objective *feels* right
under Floor 50's aggressive monster tuning is level-design judgment validated by manual
playtest, per constitution Principle IV's live-scene exception — matching spec 001's and spec
002's own testing approach.

**Organization**: This feature has a single P1 user story (US1). Tasks are grouped by Setup →
Foundational → US1, with the three key/door pairs broken into independently placeable units per
this feature's own granularity requirement, rather than one bundled placement task.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/LevelDesign/KeyMarker.cs`: a thin `MonoBehaviour`
      with `public string keyId; public string doorId;` tagging a `GameObject` in
      `Assets/Scenes/Floor50.unity` as a candidate Key placement (no gameplay logic — tagging
      only, mirroring `MonsterSpawnPresetMarker`'s pattern from spec 002), pending the real `Key`
      component owned by `specs/systems/keys-and-doors/001-key-pickup-and-inventory`
- [ ] T002 Create `Assets/Scripts/MonoBehaviours/LevelDesign/LockedDoorMarker.cs`: a thin
      `MonoBehaviour` with `public string doorId;` tagging a `GameObject` as a candidate Locked
      Door placement, pending the real `Door` component owned by
      `specs/systems/keys-and-doors/002-locked-door-unlock-logic`
- [ ] T003 Create `Assets/Tests/EditMode/LevelDesign/Floor50KeysAndDoorsContentTests.cs` (empty
      test class stub) under `Assets/Tests/EditMode/LevelDesign/` for this spec's
      structural/config tests

**Checkpoint**: Marker components and the test file exist for placement and validation to
reference.

---

## Phase 2: Foundational — Confirm Key Loop Areas Exist

**⚠️ MUST complete before any placement task below.**

- [ ] T004 In `Assets/Scenes/Floor50.unity`, confirm `KeyLoopA`, `KeyLoopB`, and `KeyLoopC` (each
      with a Key Area and a Locked Door opening, per
      `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry`) exist and are reachable
      directly from `ExplorationHub` with no loop gated behind another's door (spec 001 FR-003)

**Checkpoint**: All three loop areas exist and are order-independent, ready to receive key/door
content.

---

## Phase 3: User Story 1 - Master the Full Objective (Priority: P1)

**Goal**: Exactly three distinct matching key/door pairs exist, spatially distributed with stable
IDs, completable in any order, with escape routes and full reset preserved.

**Independent Test**: Audit counts/IDs, collect each key, unlock its matching door, and reset.

### Implementation for User Story 1

- [ ] T005 [P] [US1] Place Key A in `KeyLoopA`'s Key Area in `Assets/Scenes/Floor50.unity`, tag
      its root `GameObject` with `KeyMarker(keyId: "KeyA", doorId: "DoorA")`
- [ ] T006 [P] [US1] Place Locked Door A at `KeyLoopA`'s Locked Door opening, tag its root
      `GameObject` with `LockedDoorMarker(doorId: "DoorA")`
- [ ] T007 [P] [US1] Place Key B in `KeyLoopB`'s Key Area, tag with
      `KeyMarker(keyId: "KeyB", doorId: "DoorB")`
- [ ] T008 [P] [US1] Place Locked Door B at `KeyLoopB`'s Locked Door opening, tag with
      `LockedDoorMarker(doorId: "DoorB")`
- [ ] T009 [P] [US1] Place Key C in `KeyLoopC`'s Key Area, tag with
      `KeyMarker(keyId: "KeyC", doorId: "DoorC")`
- [ ] T010 [P] [US1] Place Locked Door C at `KeyLoopC`'s Locked Door opening, tag with
      `LockedDoorMarker(doorId: "DoorC")`
- [ ] T011 [US1] Audit all three key/door pairs for stable, unique ID matching (KeyA↔DoorA,
      KeyB↔DoorB, KeyC↔DoorC per `specs/systems/keys-and-doors/002-locked-door-unlock-logic`'s
      FR-001) and confirm no loop introduces an item-based ordering dependency on another loop
      (spec.md FR-002; spec 001's hub-and-spoke topology Assumption)
- [ ] T012 [US1] Audit every Key A/B/C and Door A/B/C placement against spec 001's escape-route
      and dead-end guarantees (spec 001 FR-005) and confirm each loop's Key/Door state resets
      fully to its initial (in-world/locked) state on a floor reset (spec.md FR-003)
- [ ] T013 [US1] Add an EditMode test in `Floor50KeysAndDoorsContentTests.cs` asserting exactly 3
      `KeyMarker` and exactly 3 `LockedDoorMarker` instances are tagged in the scene, each
      `keyId`/`doorId` pair matches its counterpart, and no duplicate `keyId` or `doorId` exists
      (spec.md SC-001)
- [ ] T014 [US1] Add an EditMode test asserting each Key Area and Locked Door position is
      reachable from `ExplorationHub` per spec 001's area graph, with no loop's Door blocking
      access to another loop's entrance (spec.md SC-001 reachability audit)
- [ ] T015 [US1] Manual playtest: collect all three keys and unlock all three doors in any order
      under Floor 50's aggressive monster tuning (spec 002), confirm no softlock occurs, and
      confirm a floor reset restores all three keys/doors to their initial state (spec.md
      SC-002)

**Checkpoint**: User Story 1 is independently complete — three validated, stably-identified
key/door pairs exist, are reachable in any order, and survive a full reset.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends only on Setup/Foundational and spec 001's three Key Loop
  Areas already existing. T005–T010 (the three placement pairs) can proceed in parallel — each
  pair is a different loop/file concern. T011–T015 depend on all six placements (T005–T010)
  being done first.

```text
Setup (T001-T003)
   ↓
Foundational (T004)
   ↓
   ├──> Place Key A + Door A (T005-T006)  [P]
   ├──> Place Key B + Door B (T007-T008)  [P]
   └──> Place Key C + Door C (T009-T010)  [P]
                           ↓
                Audits + Tests + Playtest (T011-T015)
```

## Notes

- [P] tasks (T005–T010) place independent key/door pairs in different loops with no dependency on
  an incomplete task — matching spec 002's precedent for parallel preset placement.
- This spec has no pure-logic C# beyond the two marker tag components (T001–T002) — the bulk of
  the work is Unity Editor content placement in `Assets/Scenes/Floor50.unity`, validated by
  EditMode structural tests and a manual playtest, per constitution Principle IV's exception for
  content needing a live scene to judge.
