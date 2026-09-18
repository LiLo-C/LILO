---
description: "Task list for Floor 50 Hiding Spot Placement"
---

# Tasks: Floor 50 Hiding Spot Placement

**Input**: Design documents from
`specs/scenes/006-floor-50-scene/006-hiding-spot-placement/`

**Prerequisites**: [spec.md](./spec.md),
`specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` (at least one office-appropriate
enclosed nook reserved by its FR-010 must exist in `Assets/Scenes/Floor50.unity` before this
feature's content can be placed), `specs/systems/hiding/001-enter-and-exit-hiding` and
`002-hiding-detection-immunity-rule` (the entry/exit/immunity behavior each placed spot must
satisfy)

**Tests**: Included for the parts that are structural/config-checkable in EditMode (spot count,
anchor collision/no-trap audit, reset behavior). Whether cover *feels* tactically valuable without
trivializing the three-door objective is level-design judgment validated by manual playtest, per
constitution Principle IV's live-scene exception.

**Organization**: This feature has a single P1 user story (US1). Floor 50 is deliberately sparser
on hiding cover than Floor 51/52 (GDD Ch. 3: "Ada, tapi lebih jarang"). Tasks are grouped by Setup
→ Foundational → US1, with the count decision and each individual hiding spot broken into
independently placeable units, per this feature's own granularity requirement, rather than one
bundled placement task.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/LevelDesign/HidingSpotMarker.cs`: a thin
      `MonoBehaviour` with `public string spotId;` tagging a `GameObject` in
      `Assets/Scenes/Floor50.unity` as a candidate Hiding Spot (no gameplay logic — tagging
      only, mirroring `MonsterSpawnPresetMarker`'s pattern from spec 002), pending the real
      hide-anchor components owned by `specs/systems/hiding/001-enter-and-exit-hiding`
- [ ] T002 Create `Assets/Tests/EditMode/LevelDesign/Floor50HidingSpotContentTests.cs` (empty test
      class stub) under `Assets/Tests/EditMode/LevelDesign/` for this spec's structural/config
      tests

**Checkpoint**: Marker component and the test file exist for placement and validation to
reference.

---

## Phase 2: Foundational — Decide the Spot Count and Confirm Reserved Nooks

**⚠️ MUST complete before any placement task below.**

- [ ] T003 Decide and record Floor 50's hiding spot count as this spec's design decision: **2**
      spots — deliberately fewer than Floor 51/52 per GDD Ch. 3 ("Hiding spot: Ada, tapi lebih
      jarang" — rarer, not absent) — satisfying spec.md FR-001's "must match the Floor 50 design
      decision"; record this count and its rationale in this file's Notes section below (spec.md
      requires validation in scene review, not a fabricated exact GDD number)
- [ ] T004 In `Assets/Scenes/Floor50.unity`, confirm at least 2 office-appropriate enclosed nooks
      reserved by `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` FR-010 exist
      along a plausible traversal path before placing hiding spot content

**Checkpoint**: The spot count is decided and enough reserved nooks exist to hold it.

---

## Phase 3: User Story 1 - Use Rare Cover Wisely (Priority: P1)

**Goal**: Floor 50's 2 hiding spots have safe, collision-free entry/exit anchors, connect to the
traversal route without becoming a trap, preserve pressure, and remain resettable.

**Independent Test**: Chase the player through each spot and verify entry/exit, immunity, and
objective access.

### Implementation for User Story 1

- [ ] T005 [P] [US1] Place Hiding Spot 1 (under-desk nook) in its reserved slot, tag its root
      `GameObject` with `HidingSpotMarker(spotId: "HideA")`, and configure its entry, hide, and
      exit anchors per `specs/systems/hiding/001-enter-and-exit-hiding` FR-003 (spec.md FR-001,
      FR-002)
- [ ] T006 [P] [US1] Place Hiding Spot 2 in its reserved slot, tag with
      `HidingSpotMarker(spotId: "HideB")`, and configure its entry, hide, and exit anchors (spec.md
      FR-001, FR-002)
- [ ] T007 [US1] Audit both spots' anchors for collision-free entry/exit and confirm each connects
      back to the main traversal route without becoming a single-entrance trap (spec.md FR-002;
      spec 001's dead-end-while-chased rule)
- [ ] T008 [US1] Audit both spots' placement to confirm using either does not trivialize or bypass
      the three-Locked-Door objective (e.g., neither overlooks a Key or Door from within
      detection-immunity range) and that using cover preserves rather than releases chase pressure
      (spec.md FR-003)
- [ ] T009 [US1] Confirm both spots' entering/exiting state and occupancy fully return to their
      initial (unoccupied, `Visible`) condition on a floor reset (spec.md FR-003)
- [ ] T010 [US1] Add an EditMode test in `Floor50HidingSpotContentTests.cs` asserting exactly 2
      `HidingSpotMarker` instances are tagged in the scene, with distinct `spotId`s and valid,
      non-colliding entry/hide/exit anchor transforms (spec.md SC-001)
- [ ] T011 [US1] Add an EditMode test asserting neither spot's anchors overlap or trap per
      `specs/systems/hiding/001-enter-and-exit-hiding`'s collision/navigability validation rule
      (spec.md SC-001 no-trap audit)
- [ ] T012 [US1] Manual playtest: have `Monster` chase the player through each of the 2 hiding
      spots and verify entry/exit feel, detection immunity while Hidden (per
      `specs/systems/hiding/002-hiding-detection-immunity-rule`), and confirm testers use cover
      tactically without bypassing the three-door objective (spec.md SC-002)

**Checkpoint**: User Story 1 is independently complete — both hiding spots are validated, safe,
non-trivializing, and survive a full reset.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends only on Setup/Foundational and the 2 reserved nooks existing.
  T005–T006 (the two spot placements) can proceed in parallel — different nooks, no dependency on
  an incomplete task. T007–T012 depend on both placements being done first.

```text
Setup (T001-T002)
   ↓
Foundational (T003-T004)
   ↓
   ├──> Place Hiding Spot 1 (T005)  [P]
   └──> Place Hiding Spot 2 (T006)  [P]
                    ↓
        Audits + Tests + Playtest (T007-T012)
```

## Notes

- **Spot count decision (T003)**: 2 hiding spots for Floor 50. Rationale: GDD Ch. 3's Floor 50
  column states hiding is present but rarer than Floor 51/52 ("Ada, tapi lebih jarang"); no exact
  GDD number is given, so per ROADMAP §0's rule against fabricating undefined-precision values,
  this task fixes the count as this spec's own recorded design decision (validated in scene
  review, per spec.md FR-001) rather than inventing a false GDD citation for it. If review finds 2
  is wrong for pacing, revise this Notes entry and T005/T006 rather than reopening spec.md.
- [P] tasks (T005–T006) place independent hiding spots in different reserved nooks with no
  dependency on an incomplete task — matching spec 002's precedent for parallel content
  placement.
- This spec has no pure-logic C# beyond the one marker tag component (T001) — the bulk of the
  work is Unity Editor content placement in `Assets/Scenes/Floor50.unity`, validated by EditMode
  structural tests and a manual playtest, per constitution Principle IV's exception for content
  needing a live scene to judge.
