---
description: "Task list for Light State Thresholds & Radius"
---

# Tasks: Light State Thresholds & Radius

**Input**: Design documents from `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: Included — constitution Principle IV requires EditMode coverage for every pure-logic
piece before a story is "done"; this entire feature is pure logic.

**Organization**: Tasks are grouped by user story from spec.md (both P1). Both stories share one
underlying pure-logic file, so they are implemented together in Phase 3, with tests kept in
separate files per concern (state derivation vs. radius derivation) for independent traceability.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: US1 or US2
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create the `Assets/Scripts/Systems/Flashlight/` folder if it does not yet exist
- [ ] T002 Create the `Assets/Tests/EditMode/Flashlight/` folder if it does not yet exist, and
      confirm an EditMode assembly definition referencing `Assets/Scripts/Systems/` exists (create
      one at `Assets/Tests/EditMode/LILO.Tests.EditMode.asmdef` if this is the first EditMode test
      in the project)

**Checkpoint**: Folders and test assembly exist; no code yet.

---

## Phase 2: Foundational

**⚠️ MUST complete before either user story below.**

- [ ] T003 Add `lightStateFlickerStart` (float, default `0.30`), `lightStateCriticalStart` (float,
      default `0.10`), `compactDarknessRadiusFraction` (float, default `0.10`), and
      `flashlightNormalRadius` (float, default `220` — carried over, re-confirm on-device per
      spec.md FR-008) fields to the `GameConfig` `ScriptableObject` (per
      `specs/systems/shared-config-and-state/001-game-config-schema`; add fields to the single
      existing asset, do not create a new asset)
- [ ] T004 Create `Assets/Scripts/Systems/Flashlight/LightState.cs`: a plain C# `enum LightState`
      with exactly four values — `Normal`, `Flickering`, `Critical`, `CompactDarkness` — no
      namespace dependency on `UnityEngine`

**Checkpoint**: `GameConfig` carries every field this feature needs; the enum type exists for both
stories to build on.

---

## Phase 3: User Story 1 - Identify the Current Light State from Charge (Priority: P1)

**Goal**: A pure function maps any charge fraction in `[0.0, 1.0]` to exactly one `LightState`,
using the exact boundaries in spec.md FR-002.

**Independent Test**: Run `LightStateSystemTests` in EditMode with no scene open; every asserted
charge value (including exact boundaries) resolves to the documented state.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T005 [P] [US1] Create `Assets/Tests/EditMode/Flashlight/LightStateSystemTests.cs` with cases
      for: charge `1.0` → `Normal`; charge `0.31` → `Normal`; charge exactly `0.30` → `Flickering`
      (not `Normal`); charge `0.11` → `Flickering`; charge exactly `0.10` → `Critical` (not
      `Flickering`); charge `0.01` → `Critical`; charge exactly `0.0` → `CompactDarkness`
      (spec.md US1 Acceptance Scenarios 1–6)
- [ ] T006 [P] [US1] In the same test file, add cases for out-of-range input clamping: charge
      `-0.5` resolves as if `0.0`; charge `1.5` resolves as if `1.0` (spec.md Edge Cases)

### Implementation for User Story 1

- [ ] T007 [US1] Create `Assets/Scripts/Systems/Flashlight/LightStateSystem.cs`: a plain C#
      static class with `public static LightState GetLightState(float chargeFraction, GameConfig
      config)`, clamping the input to `[0.0, 1.0]` first, then comparing against
      `config.lightStateFlickerStart` and `config.lightStateCriticalStart` in the fixed order
      Normal → Flickering → Critical → CompactDarkness (FR-001, FR-002, FR-009)
- [ ] T008 [US1] Run T005–T006 and confirm all cases pass

**Checkpoint**: User Story 1 is independently complete — any caller can get an unambiguous
`LightState` from a charge fraction alone.

---

## Phase 4: User Story 2 - Derive the Target Lit Radius for the Current State (Priority: P1)

**Goal**: A pure function maps the same charge fraction to a target lit radius, continuous through
Critical, with no animation/memory.

**Independent Test**: Run `LightRadiusTargetTests` in EditMode with no scene open; verify the
target radius at each state's boundary and the continuous interpolation inside Critical.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T009 [P] [US2] Create `Assets/Tests/EditMode/Flashlight/LightRadiusTargetTests.cs` with
      cases for: charge `1.0` and charge `0.31` (both `Normal`) → `flashlightNormalRadius`; charge
      `0.20` (`Flickering`) → `flashlightNormalRadius` (spec.md US2 Scenario 1)
- [ ] T010 [P] [US2] In the same test file, add cases for the Critical band: charge exactly
      `lightStateCriticalStart` (`0.10`) → `flashlightNormalRadius` (US2 Scenario 2); charge
      approaching `0.0` from above → approaching `flashlightNormalRadius *
      compactDarknessRadiusFraction` (US2 Scenario 3); charge at the exact midpoint of the
      Critical band → a value exactly midway between the two endpoints, asserting monotonic
      decrease across at least 5 sample points (US2 Scenario 4, spec.md SC-003)
- [ ] T011 [P] [US2] In the same test file, add a case for charge exactly `0.0`
      (`CompactDarkness`) → exactly `flashlightNormalRadius * compactDarknessRadiusFraction` (US2
      Scenario 5)

### Implementation for User Story 2

- [ ] T012 [US2] In `Assets/Scripts/Systems/Flashlight/LightStateSystem.cs`, add `public static
      float GetTargetRadius(float chargeFraction, LightState state, GameConfig config)`: returns
      `config.flashlightNormalRadius` for `Normal`/`Flickering` (FR-005); for `Critical`, linearly
      interpolates between `config.flashlightNormalRadius` (at `chargeFraction ==
      config.lightStateCriticalStart`) and `config.flashlightNormalRadius *
      config.compactDarknessRadiusFraction` (as `chargeFraction → 0`) per FR-006; for
      `CompactDarkness`, returns exactly `config.flashlightNormalRadius *
      config.compactDarknessRadiusFraction` (FR-007)
- [ ] T013 [US2] Run T009–T011 and confirm all cases pass

**Checkpoint**: Both user stories are independently complete and covered by tests; every other
flashlight-and-battery feature (002–009) can now call `LightStateSystem`.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T014 [P] Add XML-doc comments to `LightStateSystem.GetLightState` and `GetTargetRadius`
      documenting the exact boundary rule (upper bound belongs to the lower band) inline, so a
      later reader doesn't need to re-derive it from the GDD
- [ ] T015 Grep `Assets/Scripts/` and `Assets/Tests/` for any hardcoded `0.30`, `0.10`, or `220`
      literal duplicating a `GameConfig` value introduced in T003, and replace any found with a
      `GameConfig` read (constitution Principle III/V — no second source of truth)

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential; Foundational blocks both stories.
- **User Story 1** and **User Story 2** both depend only on Foundational and can be built in
  parallel by different people, since `GetLightState` and `GetTargetRadius` are independent pure
  functions sharing only the `LightState` enum and `GameConfig` fields from Phase 2 — but both
  land in the same `LightStateSystem.cs` file, so the two implementation tasks (T007, T012) should
  not be edited concurrently by two people without coordinating.
- **Polish (Phase 5)**: after both stories.

```text
Setup (T001-T002)
   ↓
Foundational (T003-T004)
   ↓
   ├──> US1 (T005-T008) ──┐
   └──> US2 (T009-T013) ──┤
                           ↓
                      Polish (T014-T015)
```

## Notes

- [P] tasks touch different files (or different, additive regions of the same new test file) with
  no dependency on an incomplete task.
- Every task in this feature is pure C#; there is no `MonoBehaviour` adapter in this spec — the
  first adapter that calls `LightStateSystem` each frame belongs to
  `003-eased-radius-transitions`.
- Commit after each task or logical group; verify EditMode tests fail before their implementation
  task lands, then pass after.
