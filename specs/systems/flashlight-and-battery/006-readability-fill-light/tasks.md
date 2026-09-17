---
description: "Task list for Readability Fill Light"
---

# Tasks: Readability Fill Light

**Input**: Design documents from `specs/systems/flashlight-and-battery/006-readability-fill-light/`

**Prerequisites**: [spec.md](./spec.md); `001-light-state-thresholds-and-radius` must already
provide the Flashlight `Light`; `005-shadow-casting-and-quality-fallback` should already exist so
this feature's "no competing shadow source" guarantee (FR-004) has something concrete to check
against.

**Tests**: Included — constitution Principle IV. The intensity-clamping logic is pure and
EditMode-testable; whether the room actually reads as "faintly visible" or "pure black" on screen
is a rendering concern validated on-device/in-Editor per Principle IV's exception.

**Organization**: Tasks are grouped by user story (P1, P2, P2).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: US1, US2, or US3
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/`, `Assets/Scripts/MonoBehaviours/Flashlight/`,
      and `Assets/Tests/EditMode/Flashlight/` exist (created by `001`)

---

## Phase 2: Foundational

- [ ] T002 Add `fillLightIntensity` (float, `>= 0`) to `GameConfig`. Per spec.md FR-002, no
      carried-over default exists for this value — set an explicitly-commented placeholder (e.g.
      `0.15`) in the field's default, with an inline comment stating it is a placeholder for local
      iteration, not a carried-over or locked value, to be replaced by on-device playtesting
- [ ] T003 Create `Assets/Scripts/Systems/Flashlight/FillLightSystem.cs`: a plain C# static class
      with no `UnityEngine.MonoBehaviour`, `Component`, or scene dependency

**Checkpoint**: `GameConfig` carries the new field; the resolution system's skeleton exists.

---

## Phase 3: User Story 1 - The Room Stays Dimly Visible Outside the Flashlight's Reach (Priority: P1)

**Goal**: A second, independent, non-shadow-casting light illuminates the scene broadly,
unaffected by the flashlight's own state/radius/flicker.

**Independent Test**: Manual/Editor scene test per spec.md's own Independent Test — walk the
player through a test room with the fill at a positive intensity and confirm areas outside the
flashlight's radius are faintly visible.

### Implementation for User Story 1

- [ ] T004 [US1] In the Unity Editor, add one additional `Light` GameObject to the test scene
      (the Fill Light), separate from the Flashlight `Light` — configure it to NOT cast shadows
      (`Light.shadows = LightShadows.None`) and to illuminate broadly rather than follow the
      player (FR-001, FR-004)
- [ ] T005 [US1] Confirm the Fill Light's own settings (position/type/range as chosen at planning
      time) do not reference or depend on `001`'s `LightStateSystem`, `003`'s `RadiusEaser`, or
      `004`'s `FlickerEventSystem` outputs in any way (FR-005)

### Manual Validation for User Story 1

- [ ] T006 [US1] On-device or in Editor Play mode: with `fillLightIntensity` at a positive
      placeholder value, walk the player so part of the room lies outside the flashlight's current
      radius, and confirm that area is faintly but discernibly lit (spec.md SC-001) — record the
      observation result
- [ ] T007 [US1] Confirm a solid object placed to normally cast a flashlight shadow (`005`) does
      not additionally cast a second, fill-light-sourced shadow (spec.md US1 Scenario 3)

**Checkpoint**: User Story 1 independently complete — the room reads as one lit space with a
faint readability floor outside the flashlight.

---

## Phase 4: User Story 2 - Fill Brightness Is a Single Config Value (Priority: P2)

**Goal**: Changing `GameConfig.fillLightIntensity` alone visibly changes outside-of-flashlight
brightness, with no other file edited.

**Independent Test**: `FillLightSystemTests` in EditMode for the pure resolve/clamp function;
on-device/Editor observation for the visible brightness change.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T008 [P] [US2] Create `Assets/Tests/EditMode/Flashlight/FillLightSystemTests.cs` with cases:
      `fillLightIntensity = 0.15` → `ResolveIntensity` returns `0.15` unchanged; `fillLightIntensity
      = 1.0` → returns `1.0` unchanged (spec.md FR-002)

### Implementation for User Story 2

- [ ] T009 [US2] In `FillLightSystem.cs`, add `public static float ResolveIntensity(GameConfig
      config) => Mathf.Max(0f, config.fillLightIntensity);` (FR-007)
- [ ] T010 [US2] Run T008 and confirm all cases pass
- [ ] T011 [US2] Create `Assets/Scripts/MonoBehaviours/Flashlight/FillLightController.cs`: a thin
      `MonoBehaviour` holding a reference to the Fill Light `Light` component that, once per
      `Update()`, sets `light.intensity = FillLightSystem.ResolveIntensity(config)` — a live,
      per-frame read, never a one-time initialization value (FR-006)

### Manual Validation for User Story 2

- [ ] T012 [US2] On-device or in Editor: raise `fillLightIntensity`, confirm outside-of-flashlight
      areas visibly brighten; lower it (staying above `0`), confirm they visibly dim — both with no
      file other than `GameConfig` changed (spec.md SC-003)

**Checkpoint**: User Story 2 independently complete — the fill's brightness is a single, live
config lever.

---

## Phase 5: User Story 3 - Setting Fill Intensity to Exactly Zero Reproduces Pure Black (Priority: P2)

**Goal**: `fillLightIntensity = 0` produces true black outside the flashlight's radius, with no
other ambient/skybox source leaking brightness.

**Independent Test**: Extend `FillLightSystemTests` with the zero/negative clamp case; a scene
audit plus on-device/Editor pixel inspection for the "no other light source" guarantee.

### Tests for User Story 3 (write first, confirm they fail before implementing)

- [ ] T013 [P] [US3] In `FillLightSystemTests.cs`, add cases: `fillLightIntensity = 0` →
      `ResolveIntensity` returns exactly `0`; `fillLightIntensity = -2.0` (authoring error) →
      resolves to exactly `0`, never negative (spec.md FR-007, Edge Cases)

### Implementation for User Story 3

- [ ] T014 [US3] Confirm `ResolveIntensity` (from T009) already satisfies T013 by construction
      (`Mathf.Max(0f, ...)`) — this task is a verification, not new logic
- [ ] T015 [US3] Run T013 and confirm it passes
- [ ] T016 [US3] In the Unity scene/lighting settings, confirm `RenderSettings` ambient
      source/skybox contribution and any other environment lighting is disabled or set to
      contribute zero illumination to the playable scene, so the Flashlight Light and this
      feature's Fill Light are the only two light sources affecting it (FR-003, Key Entities)

### Manual Validation for User Story 3

- [ ] T017 [US3] On-device or in Editor: set `fillLightIntensity` to exactly `0`, position the
      player so part of the room is outside the flashlight's radius, and visually confirm that
      area is pure black — no walls/furniture/floor detail discernible (spec.md SC-002, US3
      Scenario 1)
- [ ] T018 [US3] With `fillLightIntensity` still at `0`, audit the scene for any other
      ambient/skybox/environment light contribution independent of this feature (T016); confirm
      none exists (spec.md US3 Scenario 2, SC-004)
- [ ] T019 [US3] Raise `fillLightIntensity` back above `0` and confirm the faint readability from
      User Story 1 resumes with no other change (spec.md US3 Scenario 3)

**Checkpoint**: All three user stories independently complete — the fill light exists, is
config-tunable, and `0` is a trustworthy true-black boundary.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T020 [P] Grep `Assets/Scripts/` for any hardcoded fill-intensity literal duplicating
      `GameConfig.fillLightIntensity` and replace any found with a config read (constitution
      Principle III/V)
- [ ] T021 Add an XML-doc comment to `FillLightSystem.ResolveIntensity` documenting that its
      `0` output is depended upon by spec.md FR-003's "pure black" guarantee, so a later reader
      does not casually change the clamp floor

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1** depends only on Foundational and `001`'s existing Flashlight Light.
- **User Story 2** depends only on Foundational; shares `FillLightSystem.cs`/
  `FillLightController.cs` with User Story 3.
- **User Story 3** depends on User Story 2's `ResolveIntensity` existing (it reuses the same
  clamp), plus `005` existing for the scene-audit task (T016) to have something concrete to check.
- **Polish (Phase 6)**: after all three stories.

```text
Setup (T001)
   ↓
Foundational (T002-T003)
   ↓
   ├──> US1 (T004-T007)
   ├──> US2 (T008-T012) ──┐
   └──> US3 (T013-T019) <─┘ (shares FillLightSystem.cs with US2)
                           ↓
                      Polish (T020-T021)
```

## Notes

- [P] tasks touch different files, or independent additive regions of the same new test file.
- `FillLightSystem.ResolveIntensity`'s clamp-to-zero-floor is the same function that satisfies
  both User Story 2 (normal tuning) and User Story 3 (the `0` boundary and negative-input guard) —
  they are tested together in one file rather than duplicated, since it is one small function with
  two spec-mandated guarantees.
- T002's placeholder default is deliberately called out as non-final in-line, per spec.md FR-002's
  explicit note that, unlike this category's other tunables, no carried-over starting value exists
  for this one.
