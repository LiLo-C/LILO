---
description: "Task list for Camera Projection & Framing"
---

# Tasks: Camera Projection & Framing

**Input**: Design documents from
`specs/systems/movement-and-camera/004-camera-projection-and-framing/spec.md`

**Prerequisites**: `spec.md` (this feature). Depends on
`specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md` for `GameState`/
`GameManager` (not redefined here). ROADMAP.md lists this feature's build-order dependency as
`movement-and-camera/003-camera-follow-and-boundary-clamp`; functionally, 003's boundary-clamp
consumes this feature's ground-plane half-extent output (see `spec.md`'s Related section).

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test. `CameraRigController`'s actual `Camera` component mutation is validated instead
through manual on-device/Play Mode smoke checks (constitution Principle IV's live-scene exception),
mirroring the pattern used for `PlayerMovementController` in movement-and-camera/001.

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1 → P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user stories (US1–US4)

## Phase 1: Setup

- [ ] T001 Add camera projection fields to the existing `GameConfig` `ScriptableObject` class in
  `Assets/Scripts/Config/GameConfig.cs`: `cameraTiltAngle` (`float`, default `45`, comment noting
  this is the one GDD-locked camera feel value — GDD Ch. 13 — used as-is, not pending) and
  `cameraOrthographicSize` (`float`, no default, comment noting it is pending on-device tuning per
  GDD Ch. 15.3/21, once a floor layout exists to tune against). Do not create a second config
  asset/class — this is an addition to the single existing `GameConfig.asset` source.

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T002 Define the plain `CameraRigSettings` struct (`Orthographic` `bool`, `Yaw` `float`,
  `Tilt` `float`, `OrthographicSize` `float`) in
  `Assets/Scripts/Systems/Camera/CameraRigSettings.cs`. No `MonoBehaviour`/`Component`/scene
  dependency (constitution Principle III).
- [ ] T003 Define the `CameraProjectionSystem` plain C# static class with a public constant
  `NorthFacingYaw = 0f` and a static method `BuildRigSettings(GameConfig config) : CameraRigSettings`
  that always sets `Orthographic = true`, `Yaw = NorthFacingYaw`, `Tilt = config.cameraTiltAngle`,
  `OrthographicSize = config.cameraOrthographicSize`, in
  `Assets/Scripts/Systems/Camera/CameraProjectionSystem.cs`. No `MonoBehaviour`/`Component`/scene
  dependency (constitution Principle III) — `GameConfig` as input is the sanctioned exception.
- [ ] T004 Add a static method `ComputeGroundHalfExtent(float orthographicSize, float aspectRatio,
  float tiltAngleDegrees) : Vector2` to `CameraProjectionSystem` (extends T003; FR-006, FR-009).
  Leave the tilt-foreshortening formula itself for T018 — this task only establishes the method
  signature and a placeholder pass-through so the class compiles.
- [ ] T005 Add a `GameConfig` validation check that flags `cameraTiltAngle <= 0f ||
  cameraTiltAngle >= 90f` as invalid, extending the same `Validate()`/editor-time check helper
  introduced in movement-and-camera/001 (do not add a second validation entry point), in
  `Assets/Scripts/Config/GameConfig.cs` (extends T001; covers spec FR-008).

**Checkpoint**: `CameraProjectionSystem` compiles and is ready for story-specific behavior and
tests.

---

## Phase 3: User Story 1 - The World Always Reads as Orthographic, North-Facing, and Tilted (Priority: P1) 🎯 MVP

**Goal**: The camera's projection, yaw, and tilt are fixed values derived only from `GameConfig`,
never from player facing or movement.

**Independent Test**: Query the camera's projection mode and rotation at scene start and after the
player has moved in every direction, and confirm none of the three values ever changed.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test: `CameraProjectionSystem.BuildRigSettings(config).Orthographic`
  is `true` for any `GameConfig` values, in
  `Assets/Tests/EditMode/Systems/Camera/CameraProjectionSystemOrthographicTests.cs`.
- [ ] T007 [P] [US1] EditMode test: `BuildRigSettings(config).Yaw` equals
  `CameraProjectionSystem.NorthFacingYaw` for any `GameConfig` values — the method takes no
  player-facing/movement input at all, so this also documents by construction that yaw cannot vary
  with them, in the same test file as T006.
- [ ] T008 [P] [US1] EditMode test: `BuildRigSettings(config).Tilt` equals
  `config.cameraTiltAngle`, including the default `45` case, in the same test file as T006.

### Implementation for User Story 1

- [ ] T009 [US1] Implement `CameraRigController` `MonoBehaviour` with a serialized `Camera`
  reference and a `GameConfig` reference; on `Awake`, call
  `CameraProjectionSystem.BuildRigSettings(gameConfig)` and apply `Orthographic`/`Yaw`/`Tilt` onto
  the `Camera` (`camera.orthographic = settings.Orthographic`;
  `camera.transform.rotation = Quaternion.Euler(settings.Tilt, settings.Yaw, 0f)`), in
  `Assets/Scripts/MonoBehaviours/Camera/CameraRigController.cs`.

**Checkpoint**: Camera projection/yaw/tilt are correctly computed and applied; User Story 1 is
independently demonstrable via T006–T008 plus the manual on-device check in Phase 6/7.

---

## Phase 4: User Story 2 - Zoom Never Changes During Play (Priority: P1)

**Goal**: The camera's orthographic size is a pure function of `GameConfig` alone, with no runtime
modifier applied by this feature or any coupling to other tunables.

**Independent Test**: Sample orthographic size across simulated Light State transitions, sprinting,
and hiding, and confirm it never changes.

### Tests for User Story 2

- [ ] T010 [P] [US2] EditMode test: `BuildRigSettings(config).OrthographicSize` equals
  `config.cameraOrthographicSize`, in
  `Assets/Tests/EditMode/Systems/Camera/CameraProjectionSystemZoomTests.cs`.
- [ ] T011 [P] [US2] EditMode test: calling `BuildRigSettings` twice with the same `GameConfig`
  instance returns an identical `OrthographicSize` both times (no hidden state, no drift across
  calls), in the same test file as T010.
- [ ] T012 [P] [US2] EditMode test: `BuildRigSettings(config).OrthographicSize` is unaffected by
  varying any other `GameConfig` field (e.g. `cameraTiltAngle`, `walkSpeed`) while holding
  `cameraOrthographicSize` fixed — proves camera zoom cannot accidentally couple to any other
  tunable, including feel-adjacent movement/light fields — in the same test file as T010.

### Implementation for User Story 2

- [ ] T013 [US2] Apply `settings.OrthographicSize` onto the `Camera` component's `orthographicSize`
  field inside `CameraRigController.Awake` (extends T009). `CameraRigController` MUST NOT define an
  `Update()`/`LateUpdate()` method that touches `orthographicSize` — this makes the FR-005 "no
  dynamic zoom" guarantee true by construction, not by convention.

**Checkpoint**: Zoom is fixed and config-driven; User Stories 1–2 are both independently
demonstrable.

---

## Phase 5: User Story 3 - Other Systems Can Get a Correct Visible Ground Footprint (Priority: P2)

**Goal**: `ComputeGroundHalfExtent` correctly derives the ground-plane visible half-extent from
orthographic size, aspect ratio, and tilt angle, reflecting the tilt's foreshortening — the value
movement-and-camera/003's boundary clamp consumes.

**Independent Test**: Compute the half-extent at two or more different aspect ratios for a fixed
orthographic size/tilt and confirm the horizontal component scales with aspect ratio while the
tilt-axis component reflects the configured tilt.

### Tests for User Story 3

- [ ] T014 [P] [US3] EditMode test: `ComputeGroundHalfExtent`'s horizontal (X) component scales
  proportionally with aspect ratio for a fixed orthographic size and tilt angle (e.g. doubling the
  aspect ratio doubles X), in
  `Assets/Tests/EditMode/Systems/Camera/CameraProjectionSystemHalfExtentTests.cs`.
- [ ] T015 [P] [US3] EditMode test: `ComputeGroundHalfExtent`'s X component is unaffected by
  changes to tilt angle alone, holding orthographic size and aspect ratio fixed, in the same test
  file as T014.
- [ ] T016 [P] [US3] EditMode test: `ComputeGroundHalfExtent`'s tilt-axis (Y) component at a 45°
  tilt is strictly greater than the raw orthographic size — proves the value is foreshortened/
  derived from tilt, not a pass-through of the untilted size (spec FR-006) — in the same test file
  as T014.
- [ ] T017 [P] [US3] EditMode test: `ComputeGroundHalfExtent`'s Y component increases
  monotonically as tilt angle increases across the valid `(0°, 90°)` range for a fixed orthographic
  size (e.g. `Y(60°) > Y(45°) > Y(30°)`), in the same test file as T014.

### Implementation for User Story 3

- [ ] T018 [US3] Implement the tilt-foreshortening relationship inside `ComputeGroundHalfExtent`
  (extends T004; makes T014–T017 pass). The exact trigonometric formula is this task's own
  technical decision, not fixed by `spec.md` (constitution Principle I: specs describe observable
  behavior, not implementation).
- [ ] T019 [US3] Have `CameraRigController` implement movement-and-camera/003's
  `ICameraHalfExtentProvider` interface (`Assets/Scripts/Systems/Camera/
  ICameraHalfExtentProvider.cs`, defined by that feature) via `public Vector2
  GetGroundHalfExtent()`, computing it on demand from the live `Camera`'s current
  `orthographicSize`/`aspect` and `gameConfig.cameraTiltAngle` via
  `CameraProjectionSystem.ComputeGroundHalfExtent` (extends T009/T013). Once this exists,
  `CameraFollowController` wires this feature's `CameraRigController` in as its provider in place
  of 003's interim placeholder (see 003's `tasks.md`), per this spec's FR-006 and Related section.

**Checkpoint**: Half-extent is correct and consumable by movement-and-camera/003; User Stories 1–3
are independently demonstrable.

---

## Phase 6: User Story 4 - Tilt and Zoom Are Tunable Without Touching Code (Priority: P3)

**Goal**: Changing `cameraTiltAngle`/`cameraOrthographicSize` on the `GameConfig` asset alone
changes the camera's behavior, with no hardcoded duplicate value anywhere else.

**Independent Test**: Edit both fields on the `GameConfig` asset with no code changes, reload the
scene, and confirm the running camera reflects the new values.

### Tests for User Story 4

- [ ] T020 [P] [US4] EditMode test: two `GameConfig` instances differing only in
  `cameraTiltAngle` produce different `BuildRigSettings().Tilt` values — proves the value is read
  from the asset, not a hardcoded constant anywhere in `CameraProjectionSystem` — in
  `Assets/Tests/EditMode/Systems/Camera/CameraProjectionSystemConfigDrivenTests.cs`.
- [ ] T021 [P] [US4] EditMode test: two `GameConfig` instances differing only in
  `cameraOrthographicSize` produce different `BuildRigSettings().OrthographicSize` values, in the
  same test file as T020.

### Implementation for User Story 4

- [ ] T022 [US4] Manual on-device validation pass: edit `cameraTiltAngle` and
  `cameraOrthographicSize` directly on `GameConfig.asset` (no code changes), reload a floor scene,
  and confirm the running camera's actual tilt/orthographic size match the edited values (validates
  SC-004; documents the constitution Principle IV live-scene exception for this feature, mirroring
  movement-and-camera/001's on-device task).

**Checkpoint**: All four user stories are independently functional and tested.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T023 [P] EditMode test: an invalid `GameConfig` (`cameraTiltAngle <= 0` or `>= 90`) is
  flagged by the T005 validation check, in
  `Assets/Tests/EditMode/Systems/Config/GameConfigCameraValidationTests.cs`.
- [ ] T024 [P] EditMode test: `ComputeGroundHalfExtent` returns finite (non-`NaN`, non-infinite)
  values across a sweep of extreme-but-valid aspect ratios spanning the supported iPhone landscape
  range, in
  `Assets/Tests/EditMode/Systems/Camera/CameraProjectionSystemHalfExtentEdgeCaseTests.cs` (covers
  spec Edge Cases: aspect-ratio extremes).
- [ ] T025 [P] EditMode test: repeatedly calling `CameraProjectionSystem.BuildRigSettings` and
  `ComputeGroundHalfExtent` with the same `GameConfig` (simulating repeated scene loads) never
  produces drifting output, in the same test file as T024 (covers FR-007 idempotency at the
  pure-system level).
- [ ] T026 Manual on-device/Play Mode smoke check: force a `Camera` component into perspective mode
  via the Editor, then confirm `CameraRigController.Awake` overrides it to orthographic on scene
  load (covers spec Edge Case: scene misconfiguration), and confirm repeated floor-to-floor scene
  loads never drift the resulting projection/yaw/tilt on the real `Camera` (covers FR-007 for the
  adapter itself, complementing T025's system-level check). Documents the constitution Principle IV
  live-scene exception.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks every user story.
- **User Story 1 (Phase 3)** depends only on Foundational — the MVP slice.
- **User Story 2 (Phase 4)** depends only on Foundational; extends the same
  `CameraRigController`/`CameraProjectionSystem` as US1 but is independently testable via
  T010–T012.
- **User Story 3 (Phase 5)** depends only on Foundational (specifically T004's method signature);
  independently testable via T014–T017; its output is what movement-and-camera/003 consumes, but
  003 is not a dependency of this feature's own tasks.
- **User Story 4 (Phase 6)** depends only on Foundational; independently testable via T020–T021.
- **Polish (Phase 7)** depends on all four user stories being complete.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them.
- `CameraProjectionSystem` stays a pure, `GameConfig`-in/plain-data-out class throughout; all four
  user stories add methods or assertions to it, not new classes — `CameraRigController` is the only
  place any `UnityEngine.Camera` object is ever touched, keeping the constitution Principle III
  split intact.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- Commit after each task or logical group.
