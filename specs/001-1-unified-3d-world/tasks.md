---

description: "Task list for LILO Phase 1.1 — Unified 3D World & Feel Pass"

---

# Tasks: LILO Phase 1.1 — Unified 3D World & Feel Pass

**Input**: Design documents from `/specs/001-1-unified-3d-world/`

**Prerequisites**: [spec.md](./spec.md), [plan.md](./plan.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Included. Pure radius-easing, flicker-state, collision, and config-invariant tests are required before their implementation is considered complete. The 001 test suite remains a regression gate.

**Organization**: Tasks are grouped by the four user stories in `spec.md`, in priority order. The 001 gameplay logic remains the source of truth; this feature adds feel/lighting/collision polish on top of 001's already-unified Unity scene.

**Engine migration note (2026-09-17)**: reset to unchecked for the same reason as
[001's tasks.md](../001-core-prototype/tasks.md) — the prior checked-off implementation lived
outside this repository, under a different engine. Task content is rewritten for Unity; the
tuned values it targets (see contracts/config-additions.md's *(carried over)* entries) are
already known-good starting points from that prior implementation's on-device work.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other tasks in the same phase when they touch different files and have no incomplete dependency.
- **[Story]**: User story from `spec.md` (`US1`–`US4`).
- File paths are exact and repo-relative.

## Path Conventions

Source is under `Assets/Scripts/`; tests are under `Assets/Tests/EditMode/`; the scene is
`Assets/Scenes/TestRoom.unity`; config is `Assets/Config/GameConfig.asset`.

---

## Phase 1: Setup

- [ ] T001 Confirm the working baseline is the 001 build and record the current `Assets/Tests/EditMode` result before changing lighting/collision files (SC-1.1-009).
- [ ] T002 [P] Add all 001-1 fields from [contracts/config-additions.md](./contracts/config-additions.md) to `Assets/Scripts/Config/GameConfig.cs` and the `GameConfig.asset` instance, retaining the single-source rule and the carried-over default values.
- [ ] T003 [P] Confirm `Assets/Scripts/{Systems,MonoBehaviours,UI}` can host this feature's new files without restructuring (per [plan.md](./plan.md#project-structure) — this feature adds to existing folders, it does not introduce new top-level ones).
- [ ] T004 [P] Create `Assets/Tests/EditMode/GameConfig0011Tests.cs` with shape/range checks for the new config fields, including `flashlightCriticalMinRadius == flashlightNormalRadius * compactDarknessRadiusFraction` and valid flicker/zone relationships (FR-021).

**Checkpoint**: All new config symbols have one source, the project can see the planned files, and the unmodified 001 tests are recorded as the regression baseline.

---

## Phase 2: Foundational lighting/collision groundwork

**Must complete before user-story work.** This wires the new systems in without changing `GameState`.

- [ ] T005 Create `Assets/Scripts/Systems/CollisionResolver.cs` API and `Assets/Tests/EditMode/CollisionResolverTests.cs` cases for room-boundary clamping, desk push-out, diagonal sliding, and preserving the player body radius (FR-014, SC-1.1-007).
- [ ] T006 [P] Create `Assets/Scripts/Systems/LightRadiusEasing.cs`: a plain C# easing/flicker state machine taking `LightState` and `deltaTime`, producing the current eased radius (no `MonoBehaviour`/scene dependency).
- [ ] T007 Rewrite `Assets/Scripts/MonoBehaviours/LightingRig.cs` to read `LightRadiusEasing`'s output each frame and drive `Light.range` (a `Point` light) instead of jumping directly to each state's target radius.
- [ ] T008 [P] Add the readability-fill `Directional` `Light` `GameObject` to `Assets/Scenes/TestRoom.unity`, intensity bound to `GameConfig.readabilityFillIntensity` (FR-007, FR-018).

**Checkpoint**: The scene still launches with existing gameplay unaffected; the light now eases toward its target and a fill light exists, but shadows/flicker/highlights aren't wired up yet.

---

## Phase 3: User Story 1 — The Flashlight Lights the Room (Priority: P1)

**Goal**: Every visible world surface and both characters share one real light and actual depth/shadows.

**Independent Test**: Follow [quickstart §1](./quickstart.md#1-the-flashlight-lights-the-room-user-story-1-sc-11-001) on an iPhone 17; verify lit surfaces, moving desk shadows, depth occlusion, and darkness at all four screen edges.

- [ ] T009 [US1] Confirm `Assets/Scenes/TestRoom.unity`'s existing floor/walls/desk/batteries/door/player/monster share one scene scale (FR-001, FR-009, FR-011) — this is already true from 001; add an explicit EditMode-adjacent scene-validation note if any placeholder was left at a mismatched scale.
- [ ] T010 [US1] Configure real-time shadows on the flashlight `Light` in `LightingRig.cs` from `shadowsEnabled`, `shadowMapSize`, and `shadowSampleCount`; ensure shadows update as the player moves (FR-002, FR-022). Start from the low shadow-resolution default per research.md §6's carried-over risk note.
- [ ] T011 [US1] Verify Unity's depth buffer occludes the player/monster behind the desk with no extra code (FR-010) — add a regression note/screenshot to quickstart rather than new code, since this needs no implementation.
- [ ] T012 [US1] Apply the now-supplied `cameraTiltDegrees`/`cameraOrthographicScale`/`cameraDistance` values in `Assets/Scripts/MonoBehaviours/CameraRig.cs` (FR-012).
- [ ] T013 [US1] Create `Assets/Tests/EditMode/LightRadiusEasingTests.cs` for monotonic easing, no state-entry jump, continuous Critical narrowing, and the Critical → Compact shared boundary (FR-005).

**Checkpoint**: User Story 1 is independently demoable: the room is properly shadowed, characters are grounded/occluded, and dynamic shadows are visible.

---

## Phase 4: User Story 2 — Light States Read as One Light (Priority: P1)

**Goal**: Normal, Flickering, Critical, and Compact Darkness are visually distinct but feel like one light fading over time.

**Independent Test**: Follow [quickstart §2](./quickstart.md#2-light-states-read-as-one-light-user-story-2-sc-11-004) with a shortened config battery duration.

- [ ] T014 [US2] Implement the light-profile target calculation in `LightRadiusEasing.cs` from `GameState.lightState`, including the Critical interpolation and Compact Darkness boundary defined in `data-model.md` (FR-005).
- [ ] T015 [P] [US2] Add a flicker phase (`Steady`/`Dipping`) to `LightRadiusEasing.cs` and `Assets/Tests/EditMode/FlickerEventTests.cs` for steady/dipping timing, configured interval/duration/depth, and immediate cancellation on state exit (FR-006).
- [ ] T016 [US2] Integrate discrete flicker events into `LightingRig.cs`; never reroll brightness or radius every frame, and keep Normal/Critical/Compact steady except for their configured easing (FR-006).
- [ ] T017 [US2] Apply `GameConfig.maxFrameDelta` once per frame in `GameManager.cs`, sharing the clamped delta with movement, radius easing, flicker timing, and camera follow; add unit coverage for a long-frame clamp (FR-015).
- [ ] T018 [US2] Confirm `BatteryController`/`LightState` derivation is unchanged and re-run `BatteryControllerTests.cs`/`LightStateTests.cs` unmodified (FR-023, SC-1.1-009).

**Checkpoint**: The complete battery drain can be observed without input; four states, easing, flicker, and Compact Darkness remain playable and regression-safe.

---

## Phase 5: User Story 3 — The Room Stays Navigable in the Dark (Priority: P2)

**Goal**: Faint geometry remains readable and batteries/door remain locatable without self-lighting.

**Independent Test**: Follow [quickstart §3](./quickstart.md#3-the-room-stays-navigable-in-the-dark-user-story-3-sc-11-006) at 0% charge, then repeat with readability fill set to zero.

- [ ] T019 [US3] Create `Assets/Scripts/Systems/HighlightController.cs` (plain C#) computing out-of-range/in-range intensity per loose battery and the closed door, reusing `InteractionController`'s existing distance test (FR-013).
- [ ] T020 [US3] Create `Assets/Scripts/MonoBehaviours/HighlightView.cs`: an unlit outline/halo `GameObject` per interactable, driven each frame by `HighlightController`'s intensity and `GameConfig.highlightColor`; remove a battery's highlight exactly when it leaves `World`, and the door's once it's no longer interactable (FR-013).
- [ ] T021 [P] [US3] Add highlight-focused EditMode coverage for range transitions, battery removal, and opened-door removal (FR-013).
- [ ] T022 [US3] Verify the scene's placeholder materials and the readability fill keep unlit wall/desk shapes distinguishable but never brighter than the dimmest lit surface, while highlights remain visible in Compact Darkness (FR-007, FR-020, SC-1.1-005).

**Checkpoint**: At zero charge, a first-time player can still navigate to the door without instructions, and important objects are marked only by their configured outline/halo.

---

## Phase 6: User Story 4 — Movement and Controls Feel Physical (Priority: P2)

**Goal**: Collision slides, camera stays grounded, controls float, and pickup/door actions animate visibly.

**Independent Test**: Follow [quickstart §4](./quickstart.md#4-movement-and-controls-feel-physical-user-story-4), including ten diagonal wall/desk trials and multi-touch joystick checks.

- [ ] T023 [US4] Integrate `CollisionResolver` into `Assets/Scripts/Systems/MovementController.cs` after velocity application, preserving walk/sprint speed and 001 movement-state rules while resolving wall/desk overlap by sliding (FR-014, FR-023).
- [ ] T024 [US4] Rewrite `Assets/Scripts/UI/JoystickUI.cs` as a floating joystick: start only inside the configured left control zone, center at touch-down, track one touch through drag-out, and clear input on end/cancel (FR-019).
- [ ] T025 [US4] Add config-only `batteryPickupAnimationDuration` handling in `Assets/Scripts/MonoBehaviours/BatteryPickup.cs` so a picked-up battery grows/fades out before removal, without allowing reappearance (FR-016).
- [ ] T026 [US4] Add config-only `doorOpenAnimationDuration` handling in `Assets/Scripts/MonoBehaviours/DoorController.cs` so opening the door visibly transitions from closed to open and remains one-way (FR-017).
- [ ] T027 [P] [US4] Add pure animation/state tests for battery removal timing and door open completion, including interruption/long-frame behavior under `maxFrameDelta` (FR-016, FR-017).
- [ ] T028 [US4] Confirm `GameHUD`, `ActionButtonUI`, `BatteryIndicatorUI`, and the joystick render above the 3D scene on the `Canvas` and are unaffected by world lighting/darkness (FR-018).

**Checkpoint**: All four user stories are playable together: movement slides around geometry, joystick behavior is floating, actions animate, and HUD remains readable.

---

## Phase 7: Polish, regression, and acceptance

- [ ] T029 [P] Run all 001-1 scenarios in [quickstart.md](./quickstart.md) on a physical iPhone 17 running iOS 26; record results against SC-1.1-001 through SC-1.1-008.
- [ ] T030 [P] Run the full `Assets/Tests/EditMode` suite, including all unchanged 001 tests and new 001-1 tests; record SC-1.1-009.
- [ ] T031 [P] Measure sustained ≥60 FPS for at least 30 seconds with shadows enabled, both characters visible, and continuous movement. If needed, reduce shadow quality while keeping shadows enabled and repeat; record a separate FR-022 shadows-off fallback result (SC-1.1-002).
- [ ] T032 [P] Run the three-person side-by-side feel test and the two-person Flickering/Compact Darkness observation checks; record responses against SC-1.1-003, SC-1.1-004, and SC-1.1-005.
- [ ] T033 [P] Re-tune every *(carried over)* value in `GameConfig.asset` on-device for Unity's light/shadow units (camera, radius, shadow, highlight, animation, joystick values), without edits to other source files (FR-021, SC-1.1-008).
- [ ] T034 Review all changed `Assets/Scripts/` files for duplicate tunables, stale unshadowed-light assumptions, unintended gameplay-state changes, and conformance with the 001-1 contract and checklist.

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 is sequential; T004 should be written before the corresponding config implementation is considered complete.
- US1 depends on Phase 2 and is the shadow/lighting foundation for all later stories.
- US2 depends on the lighting host from US1 but can develop its pure tests in parallel with T009–T013.
- US3 depends on the world/interaction data from US1/US2.
- US4 depends on the collision/lighting groundwork from US1 and the interaction/highlight wiring from US3.
- Phase 7 starts only after all four story checkpoints pass.

```text
Setup (T001-T004)
   ↓
Foundation (T005-T008)
   ↓
   ├──> US1 (T009-T013) ──> US2 (T014-T018) ──┐
   │                                           ├──> US4 (T023-T028)
   └──────────────────────> US3 (T019-T022) ───┘
                                               ↓
                                      Polish (T029-T034)
```

## Implementation Strategy

**MVP**: Finish US1 + US2 first. Together they prove the central 001-1 change: real-time shadows plus a coherent battery-light-state feel on top of 001's existing scene.

**Incremental delivery**: Foundation → shadowed lighting → light-state feel → dark navigation/highlights → physical movement/feedback → on-device acceptance. Every checkpoint remains buildable and demoable.

## Format Validation

All 34 tasks use the required `- [ ] T### [P?] [Story?]` format and exact repo-relative paths. New 001-1 criteria use the `SC-1.1-###` namespace; inherited 001 criteria remain referenced explicitly as `001 SC-###`.
