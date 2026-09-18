---
description: "Task list for Floor 52 Exit and Transition"
---

# Tasks: Floor 52 Exit and Transition

**Input**: Design documents from
`specs/scenes/004-floor-52-scene/005-floor-exit-and-transition/`

**Prerequisites**: [spec.md](./spec.md); the `ExitDoorAlcove` Area from
`001-level-layout-and-geometry` (T008) must already exist in `Assets/Scenes/Floor52.unity`.

**Tests**: No-key behavior and idempotency under repeated/rapid input are logic-level and
EditMode-testable (Phase 3). End-to-end scene loading needs a live build/device, so per
constitution Principle IV's exception that is validated by the manual/device walkthroughs, not by
an automated assertion.

**Organization**: Tasks are grouped by the single user story from spec.md.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Confirm the `ExitDoorAlcove` Area placed by `001-level-layout-and-geometry` T008
      exists in `Assets/Scenes/Floor52.unity` — this spec places the actual `Door` object inside
      that reserved alcove, not a new location
- [ ] T002 Create `Assets/Tests/EditMode/Floor52/` folder for this spec's tests if it does not
      already exist (may already exist from a sibling Floor 52 spec)

**Checkpoint**: Alcove confirmed; ready to place and wire the Door.

---

## Phase 2: User Story 1 - Leave the Lesson Floor (Priority: P1)

**Goal**: After reaching the unkeyed exit door, the player receives clear feedback and
transitions to Floor 51 exactly once.

**Independent Test**: Approach from both sides, press with/without interaction eligibility, and
inspect one transition request.

### Implementation for User Story 1

- [ ] T003 [US1] Instantiate a `Door` object (per
      `specs/systems/keys-and-doors/002-locked-door-unlock-logic` contract) inside the
      `ExitDoorAlcove` in `Assets/Scenes/Floor52.unity`, configured with no key requirement
      (spec.md FR-001)
- [ ] T004 [US1] Apply the "pintu turun lantai" distinct color/visual variant (GDD Ch. 8.2, per
      `specs/systems/keys-and-doors/004-door-visual-and-color-feedback`) to the Door instance
      from T003, distinguishing it from an ordinary door, in `Assets/Scenes/Floor52.unity`
- [ ] T005 [US1] Create `Assets/Scripts/MonoBehaviours/Floor52/Floor52ExitDoorController.cs`: a
      thin `MonoBehaviour` on the Door object that, on a valid interaction, calls
      `specs/systems/progression-and-scene-flow/002-scene-transition-manager`'s transition
      request API targeting Floor 51, and no-ops while a load is already in progress (spec.md
      FR-002, FR-003)
- [ ] T006 [US1] Wire interaction-eligibility (approach from either side; context-sensitive action
      button per `specs/systems/interaction-and-highlight/002-context-sensitive-action-button`)
      onto the Door instance so pressing without eligibility produces no transition request
      (spec.md US1 Acceptance Scenario)
- [ ] T007 [US1] Verify run-state preservation: confirm the transition request path carries
      forward whatever `specs/systems/shared-config-and-state/002-shared-game-state-and-manager`
      defines as needing to survive a floor transition (spec.md FR-002)
- [ ] T008 [US1] Verify idempotency: rapid repeated interaction input on the Door while a
      transition is already pending produces exactly one transition request, not several
      (spec.md FR-003)
- [ ] T009 [US1] Manual walkthrough: approach the Door from both sides, interact once, and confirm
      exactly one Floor 51 load occurs with no key prompt or false locked state ever appearing
      (spec.md Independent Test, SC-001, SC-002)

**Checkpoint**: User Story 1 is independently complete — the exit is placed, unkeyed, idempotent,
and correctly wired to the shared transition manager.

---

## Phase 3: Polish & Cross-Cutting Concerns

- [ ] T010 [P] Create `Assets/Tests/EditMode/Floor52/Floor52ExitDoorTests.cs`: EditMode tests
      covering no-key interaction succeeding, repeated/rapid interaction producing exactly one
      transition request, and interaction while ineligible producing zero requests (spec.md
      FR-001, FR-003)
- [ ] T011 Verify end-to-end on device: walk the full Floor 52 critical path to the Door and
      confirm the Floor 51 scene loads correctly with run state intact (spec.md Success Criteria;
      cross-check against `progression-and-scene-flow/002-scene-transition-manager`)
- [ ] T012 Run the exit/transition validation as a single documented pass and record the result
      in this spec's `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **User Story 1 (Phase 2)**: sequential.
- Within User Story 1, T003 → T004 → T005 → T006 → T007/T008 (T007 and T008 can run in either
  order once T005–T006 land) → T009.
- **Polish (Phase 3)**: after User Story 1.

```text
Setup (T001-T002)
   ↓
US1 Door placement (T003-T004)
   ↓
US1 transition wiring (T005-T006)
   ↓
US1 state/idempotency checks (T007-T008)
   ↓
US1 walkthrough (T009)
   ↓
Polish (T010-T012)
```

## Notes

- This spec's only new C# is the thin `Floor52ExitDoorController` adapter — the transition logic
  itself lives in `progression-and-scene-flow/002-scene-transition-manager` and is not
  reimplemented here (constitution Principle II).
- T011 (device verification) depends on `Floor51.unity` existing enough to load into; if it does
  not yet exist, record T011 as deferred rather than blocking this spec's own sign-off, matching
  how `001`'s T020 handles a similar cross-spec dependency.
