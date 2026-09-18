---
description: "Task list for Floor 50 Final Door Placement and Trigger"
---

# Tasks: Floor 50 Final Door Placement and Trigger

**Input**: Design documents from
`specs/scenes/006-floor-50-scene/004-final-door-placement-and-trigger/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` (the `FinalDoorAlcove` Area at the
end of the `ChaseSection` must exist in `Assets/Scenes/Floor50.unity` before this feature's
content can be placed), `specs/scenes/006-floor-50-scene/003-three-keys-and-locked-doors-placement`
(the three Locked Doors this feature's trigger gates on)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (eligibility
matrix across 0–3 resolved objectives, one-shot transition guarantee, reset behavior). Whether the
final approach *feels* right under Chase Section pressure is level-design judgment validated by
manual playtest, per constitution Principle IV's live-scene exception.

**Organization**: This feature has a single P1 user story (US1). Tasks are grouped by Setup →
Foundational → US1, separating door placement, approach authoring, trigger wiring, and the ending
handoff into independently completable units.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/LevelDesign/FinalDoorMarker.cs`: a thin
      `MonoBehaviour` tagging the Final Door `GameObject` in `Assets/Scenes/Floor50.unity` (no
      gameplay logic — tagging only, mirroring `MonsterSpawnPresetMarker`'s pattern from spec
      002), pending the real `FinalDoor` component owned by
      `specs/systems/keys-and-doors/003-final-door-distinct-behavior`
- [ ] T002 Create `Assets/Tests/EditMode/LevelDesign/Floor50FinalDoorContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` for this spec's structural/config
      tests

**Checkpoint**: Marker component and the test file exist for placement and validation to
reference.

---

## Phase 2: Foundational — Confirm the Final Door Alcove Exists

**⚠️ MUST complete before any placement task below.**

- [ ] T003 In `Assets/Scenes/Floor50.unity`, confirm the `FinalDoorAlcove` Area and the
      `ChaseSection` leading into it (both reserved by
      `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` FR-006/FR-008) exist before
      placing Final Door content
- [ ] T004 Confirm the three Locked Doors from
      `specs/scenes/006-floor-50-scene/003-three-keys-and-locked-doors-placement` are placed and
      their `doorId`s (`DoorA`/`DoorB`/`DoorC`) are stable — this feature's trigger gate reads
      their resolved state

**Checkpoint**: The Final Door's area exists and the objective set it gates on is stable.

---

## Phase 3: User Story 1 - Reach the Escape (Priority: P1)

**Goal**: The Final Door is visibly/behaviorally distinct from the three ordinary doors,
reachable after all three objectives, and triggers the Good Ending exactly once.

**Independent Test**: Attempt with 0–2 keys, then with all keys, including repeated input and
reset.

### Implementation for User Story 1

- [ ] T005 [US1] Place the Final Door `GameObject` in `FinalDoorAlcove`, tag it with
      `FinalDoorMarker`, and give it visually and behaviorally distinct dressing/interaction
      feedback from the three ordinary Locked Doors (spec.md FR-001, FR-002)
- [ ] T006 [US1] Author the readable approach corridor from `ChaseSection` into `FinalDoorAlcove`,
      auditing its margins against spec 001's "no unmarked single-entrance alcove" rule (spec 001
      FR-006) and confirming no unsafe dead-end exists along the approach (spec.md FR-003)
- [ ] T007 [US1] Place and configure the Final Door's trigger volume, wiring it to
      `specs/systems/keys-and-doors/003-final-door-distinct-behavior`'s eligibility check so it
      requires `DoorA`, `DoorB`, and `DoorC` all resolved before the final-door interaction is
      offered (spec.md FR-001, FR-002)
- [ ] T008 [US1] Wire the Final Door's successful-trigger path to commit completion and request
      the Good Ending transition exactly once, via
      `specs/systems/progression-and-scene-flow/002-scene-transition-manager` (spec.md FR-002,
      citing `specs/systems/keys-and-doors/003-final-door-distinct-behavior` FR-003)
- [ ] T009 [US1] Add an EditMode test in `Floor50FinalDoorContentTests.cs` simulating attempts
      with 0, 1, and 2 of the 3 doors resolved and asserting the final-door trigger produces no
      completion and no transition request in any case (spec.md SC-001)
- [ ] T010 [US1] Add an EditMode test simulating an attempt with all 3 doors resolved and
      asserting exactly one Good Ending transition request fires, and that a repeated/duplicate
      trigger input in the same already-completed state produces no second request (spec.md
      SC-002)
- [ ] T011 [US1] Add an EditMode test simulating a floor reset mid-approach (before and during the
      trigger animation) and confirming the Final Door's state (locked, no false completion, no
      stray transition request) returns to its initial state (spec.md FR-003; system spec's
      reset-during-animation edge case)
- [ ] T012 [US1] Manual playtest: approach the Final Door through the `ChaseSection` under active
      pursuit with all three objectives resolved, confirming the approach reads clearly, the
      trigger fires once, and the Good Ending request feels correct (spec.md US1 Independent
      Test)

**Checkpoint**: User Story 1 is independently complete — the Final Door is distinct, correctly
gated, and produces exactly one Good Ending request under valid conditions.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends on Setup/Foundational, including spec 003's three Locked
  Doors existing with stable IDs (T004). T005–T006 (placement/approach) can proceed in parallel
  with each other but must both complete before T007 (trigger wiring, which depends on the door
  object and approach existing). T008 depends on T007. T009–T012 depend on T005–T008.

```text
Setup (T001-T002)
   ↓
Foundational (T003-T004)
   ↓
   ├──> Place Final Door (T005)      ─┐
   └──> Author approach (T006)       ─┴──> Wire trigger gate (T007) ──> Wire ending handoff (T008)
                                                                              ↓
                                                              Tests + Playtest (T009-T012)
```

## Notes

- This spec has no pure-logic C# beyond the one marker tag component (T001) and the wiring/tests
  in T007–T011 — the bulk of the work is Unity Editor content placement in
  `Assets/Scenes/Floor50.unity`, validated by EditMode structural tests and a manual playtest,
  per constitution Principle IV's exception for content needing a live scene to judge.
- T004 is a forward-dependency confirmation on spec 003's output, matching spec 002's precedent
  (its T004) for confirming a prerequisite spec's state before proceeding.
