---

description: "Task list for LILO Phase 1.1 — Unified 3D World & Feel Pass"

---

# Tasks: LILO Phase 1.1 — Unified 3D World & Feel Pass

**Input**: Design documents from `/specs/001-1-unified-3d-world/`

**Prerequisites**: [spec.md](./spec.md), [plan.md](./plan.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Included. Pure radius-easing, flicker-state, collision, and config-invariant tests are required before their implementation is considered complete. The 001 test suite remains a regression gate.

**Organization**: Tasks are grouped by the four user stories in `spec.md`, in priority order. The 001 gameplay logic remains the source of truth; this feature replaces the renderer and adds feel/collision behavior around it.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other tasks in the same phase when they touch different files and have no incomplete dependency.
- **[Story]**: User story from `spec.md` (`US1`–`US4`).
- File paths are exact and repo-relative.

## Path Conventions

Source is under `v2/Game/`; tests are under `v2Tests/`; project wiring is in `v2.xcodeproj/project.pbxproj`.

---

## Phase 1: Setup

- [X] T001 Confirm the working baseline is the 001 build and record the current `v2Tests` result before changing renderer files (SC-1.1-009).
- [X] T002 [P] Add all 001-1 keys from [contracts/config-additions.md](./contracts/config-additions.md) to `v2/Game/GameConfig.swift`, retaining the single-source rule and explicit `TBD` placeholders.
- [X] T003 [P] Add the new `Scenes/`, `Entities/`, `Systems/`, and `Views/` source files/groups to `v2.xcodeproj/project.pbxproj` and ensure they belong to the `v2` target.
- [X] T004 [P] Create `v2Tests/GameConfig0011Tests.swift` with shape/range checks for the new config keys, including `flashlightCriticalMinRadius == flashlightNormalRadius * compactDarknessRadiusFraction` and valid flicker/zone relationships (FR-021).

**Checkpoint**: All new config symbols have one source, the project can see the planned files, and the unmodified 001 tests are recorded as the regression baseline.

---

## Phase 2: Foundational renderer migration

**Must complete before user-story work.** This establishes one scene and keeps `GameState` renderer-independent.

- [X] T005 Implement `v2/Game/Entities/SceneNodeFactory.swift` with primitive SceneKit nodes for floor, walls, desk, batteries, door, player, and placeholder monster using the dark material baseline (FR-011, FR-020).
- [X] T006 [P] Create `v2/Game/Systems/CollisionResolver.swift` API and `v2Tests/CollisionResolverTests.swift` cases for room-boundary clamping, desk push-out, diagonal sliding, and preserving the player body radius (FR-014, SC-1.1-007).
- [X] T007 Create `v2/Game/Scenes/WorldSceneController.swift` skeleton that owns one `SCNScene`, retains the shared `GameState`, and exposes a frame update entry point with a single elapsed-time value.
- [X] T008 [P] Create `v2/Game/Views/SceneContainerView.swift` as the thin `SCNView` SwiftUI wrapper, configured for landscape rendering and HUD overlay composition (FR-018).
- [X] T009 Replace the `SpriteView` host in `v2/ContentView.swift` with `SceneContainerView`, preserving one `GameState` owner and the existing HUD state flow.
- [X] T010 Remove the playable-world dependency on `v2/Game/Scenes/TestRoomScene.swift` and the old `SK3DNode` path only after the new scene host builds; preserve 001 systems until their replacement wiring is verified.

**Checkpoint**: The app launches into a single empty SceneKit world with the existing `GameState` and HUD still connected; no gameplay behavior has changed.

---

## Phase 3: User Story 1 — The Flashlight Lights the Room (Priority: P1)

**Goal**: Every visible world surface and both characters share one real light and actual depth/shadows.

**Independent Test**: Follow [quickstart §1](./quickstart.md#1-the-flashlight-lights-the-room-user-story-1-sc-11-001) on an iPhone 17; verify lit surfaces, moving desk shadows, depth occlusion, and darkness at all four screen edges.

- [X] T011 [US1] In `WorldSceneController.swift`, build the fixed room from `TestRoom` data: floor, four solid walls, desk, two batteries, door, player, and placeholder monster at one shared 3D scale (FR-001, FR-009, FR-011).
- [X] T012 [US1] Add per-frame 2D-to-3D position synchronization in `WorldSceneController.swift` using `(x, 0, -y)` without a second gameplay coordinate system (FR-008, FR-009).
- [X] T013 [US1] Rewrite `v2/Game/Systems/CameraController.swift` to drive one orthographic SceneKit camera with fixed north-facing yaw, config tilt/scale/distance, smooth elapsed-time follow, and room-boundary clamping with no look-ahead (FR-012, FR-015).
- [X] T014 [US1] Rewrite `v2/Game/Systems/LightingController.swift` to attach one spot flashlight to the player, derive reach/cone/attenuation from the eased radius, and use the shared elapsed-time clamp (FR-001, FR-003, FR-004, FR-005, FR-015).
- [X] T015 [US1] Configure real-time wall/furniture shadows in `LightingController.swift` from `shadowsEnabled`, `shadowMapSize`, and `shadowSampleCount`; ensure shadows update as the player moves (FR-002, FR-022).
- [X] T016 [US1] Add the single config-driven readability fill in `LightingController.swift`; verify zero intensity produces black outside flashlight reach and nonzero intensity keeps wall/desk silhouettes faint (FR-007).
- [X] T017 [US1] Create `v2Tests/LightRadiusEasingTests.swift` for monotonic easing, no state-entry jump, continuous Critical narrowing, and the Critical → Compact shared boundary (FR-005).

**Checkpoint**: User Story 1 is independently demoable: the complete room is one lit 3D scene, characters are grounded/occluded by depth, and dynamic shadows are visible.

---

## Phase 4: User Story 2 — Light States Read as One Light (Priority: P1)

**Goal**: Normal, Flickering, Critical, and Compact Darkness are visually distinct but feel like one light fading over time.

**Independent Test**: Follow [quickstart §2](./quickstart.md#2-light-states-read-as-one-light-user-story-2-sc-11-004) with a shortened config battery duration.

- [X] T018 [US2] Implement the light-profile target calculation in `LightingController.swift` from `GameState.lightState`, including the Critical interpolation and Compact Darkness boundary defined in `data-model.md` (FR-005).
- [X] T019 [P] [US2] Create `v2/Game/Systems/FlickerStateMachine.swift` (or an equivalent pure helper in `LightingController.swift`) and `v2Tests/FlickerEventTests.swift` for steady/dipping timing, configured interval/duration/depth, and immediate cancellation on state exit (FR-006).
- [X] T020 [US2] Integrate discrete flicker events into `LightingController.swift`; never reroll brightness or radius every frame, and keep Normal/Critical/Compact steady except for their configured easing (FR-006).
- [X] T021 [US2] Apply `GameConfig.maxFrameDelta` once per frame to movement, radius easing, flicker timing, and camera follow; add unit coverage for a long-frame clamp (FR-015).
- [X] T022 [US2] Preserve and wire the existing `BatteryController`/`LightState` derivation without changing 001 charge thresholds, then run `BatteryControllerTests.swift` and `LightStateTests.swift` unchanged (FR-023, SC-1.1-009).

**Checkpoint**: The complete battery drain can be observed without input; four states, easing, flicker, and Compact Darkness remain playable and regression-safe.

---

## Phase 5: User Story 3 — The Room Stays Navigable in the Dark (Priority: P2)

**Goal**: Faint geometry remains readable and batteries/door remain locatable without self-lighting.

**Independent Test**: Follow [quickstart §3](./quickstart.md#3-the-room-stays-navigable-in-the-dark-user-story-3-sc-11-006) at 0% charge, then repeat with readability fill set to zero.

- [X] T023 [US3] Create `v2/Game/Systems/HighlightController.swift` with unlit outline/halo nodes around every loose battery and the closed door; drive out-of-range/in-range emission from the two config intensities and shared interaction-radius logic (FR-013).
- [X] T024 [US3] Update highlight membership and intensity every frame from `GameState`; remove a battery highlight exactly when the battery leaves `.world`, and remove the door highlight once the door is no longer interactable (FR-013).
- [X] T025 [P] [US3] Add highlight-focused unit coverage (or extend `InteractionControllerTests.swift`) for range transitions, shadow independence, battery removal, and opened-door removal (FR-013).
- [X] T026 [US3] Verify the scene materials and readability fill keep unlit wall/desk shapes distinguishable but never brighter than the dimmest lit surface, while highlights remain visible in Compact Darkness (FR-007, FR-020, SC-1.1-005).

**Checkpoint**: At zero charge, a first-time player can still navigate to the door without instructions, and important objects are marked only by their configured outline/halo.

---

## Phase 6: User Story 4 — Movement and Controls Feel Physical (Priority: P2)

**Goal**: Collision slides, camera stays grounded, controls float, and pickup/door actions animate visibly.

**Independent Test**: Follow [quickstart §4](./quickstart.md#4-movement-and-controls-feel-physical-user-story-4), including ten diagonal wall/desk trials and multi-touch joystick checks.

- [X] T027 [US4] Integrate `CollisionResolver` into `v2/Game/Systems/MovementController.swift` after velocity application, preserving walk/sprint speed and 001 movement-state rules while resolving wall/desk overlap by sliding (FR-014, FR-023).
- [X] T028 [US4] Rewrite `v2/Game/UI/JoystickView.swift` as a floating joystick: start only inside the configured left control zone, center at touch-down, track one touch through drag-out, and clear input on end/cancel (FR-019).
- [X] T029 [US4] Add config-only `batteryPickupAnimationDuration` handling in `WorldSceneController.swift`/node state so a picked-up battery grows/fades out before removal, without allowing reappearance (FR-016).
- [X] T030 [US4] Add config-only `doorOpenAnimationDuration` handling so opening the door visibly transitions its SceneKit node from closed to open and remains one-way (FR-017).
- [ ] T031 [P] [US4] Add pure animation/state tests for battery removal timing and door open completion, including interruption/long-frame behavior under `maxFrameDelta` (FR-016, FR-017).
- [X] T032 [US4] Confirm `GameHUDView`, `ActionButtonView`, `BatteryIndicatorView`, and joystick render above the SceneKit view and are unaffected by world lighting/darkness (FR-018).

**Checkpoint**: All four user stories are playable together: movement slides around geometry, joystick behavior is floating, actions animate, and HUD remains readable.

---

## Phase 7: Polish, regression, and acceptance

- [ ] T033 [P] Run all 001-1 scenarios in [quickstart.md](./quickstart.md) on a physical iPhone 17 running iOS 26; record results against SC-1.1-001 through SC-1.1-008.
- [ ] T034 [P] Run the full `v2Tests` target, including all unchanged 001 tests and new 001-1 tests; record SC-1.1-009.
- [ ] T035 [P] Measure sustained ≥60 FPS for at least 30 seconds with shadows enabled, both characters visible, and continuous movement. If needed, reduce shadow quality while keeping shadows enabled and repeat; record a separate FR-022 shadows-off fallback result (SC-1.1-002).
- [ ] T036 [P] Run the three-person side-by-side feel test and the two-person Flickering/Compact Darkness observation checks; record responses against SC-1.1-003, SC-1.1-004, and SC-1.1-005.
- [ ] T037 [P] Tune every remaining `TBD` value in `GameConfig.swift` on-device (including camera, radius, shadow, highlight, animation, and joystick values) without edits to other source files (FR-021, SC-1.1-008).
- [ ] T038 Review all changed `v2/Game/` files for duplicate tunables, stale SpriteKit renderer references, unintended gameplay-state changes, and conformance with the 001-1 contract and checklist.

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 is sequential; T004 should be written before the corresponding config implementation is considered complete.
- US1 depends on Phase 2 and is the renderer foundation for all later stories.
- US2 depends on the scene/light host from US1 but can develop its pure tests in parallel with T011–T016.
- US3 depends on the world nodes and interaction data from US1/US2.
- US4 depends on the world nodes from US1 and the interaction/highlight wiring from US3.
- Phase 7 starts only after all four story checkpoints pass.

```text
Setup (T001-T004)
   ↓
Foundation (T005-T010)
   ↓
   ├──> US1 (T011-T017) ──> US2 (T018-T022) ──┐
   │                                           ├──> US4 (T027-T032)
   └──────────────────────> US3 (T023-T026) ───┘
                                               ↓
                                      Polish (T033-T038)
```

## Implementation Strategy

**MVP**: Finish US1 + US2 first. Together they prove the central 001-1 change: one unified 3D world with meaningful flashlight/shadows and a coherent battery-light-state feel.

**Incremental delivery**: Foundation → unified world/lighting → light-state feel → dark navigation/highlights → physical movement/feedback → on-device acceptance. Every checkpoint remains buildable and demoable.

## Format Validation

All 38 tasks use the required `- [ ] T### [P?] [Story?]` format and exact repo-relative paths. New 001-1 criteria use the `SC-1.1-###` namespace; inherited 001 criteria remain referenced explicitly as `001 SC-###`.
