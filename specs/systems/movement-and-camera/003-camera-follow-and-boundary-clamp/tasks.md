---
description: "Task list for Camera Follow & Boundary Clamp"
---

# Tasks: Camera Follow & Boundary Clamp

**Input**: Design documents from
`specs/systems/movement-and-camera/003-camera-follow-and-boundary-clamp/spec.md`

**Prerequisites**: `spec.md` (this feature). Depends on
`specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md` for the
`PlayerCharacter` position this feature follows, and on
`specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md` for
`GameState`/`GameManager` (not redefined here). This feature's half-extent input is owned by
`specs/systems/movement-and-camera/004-camera-projection-and-framing/spec.md`, which ships after
this feature per ROADMAP.md §5's `003→004` build order — see Phase 4 below for how that forward
dependency is bridged.

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test. `CameraFollowController`'s actual `Camera`/`Transform` mutation and Unity script
execution order (FR-010) are validated instead through manual on-device/Play Mode smoke checks
(constitution Principle IV's live-scene exception), mirroring the pattern used for
`PlayerMovementController` in movement-and-camera/001.

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1 → P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user stories (US1–US4)

## Phase 1: Setup

- [ ] T001 Add camera-follow fields to the existing `GameConfig` `ScriptableObject` class in
  `Assets/Scripts/Config/GameConfig.cs`: `cameraFollowLerpFactor` (`float`, default `0.12`, comment
  noting it is a carried-over value pending on-device re-confirmation per spec Assumptions) and
  `cameraBoundsInset` (`float`, default `80`, comment noting the same). Do not create a second
  config asset/class, and do not re-declare fields already added by other specs (e.g.
  movement-and-camera/001's `walkSpeed`, movement-and-camera/004's `cameraTiltAngle`) — this task
  only adds the two fields above.

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T002 Define the plain `LevelBounds` struct (`Min` `Vector2`, `Max` `Vector2`, and an
  `IsValidOnAxis(int axisIndex)`-style helper — or equivalent per-axis validity check — that is
  `false` when `Min` exceeds `Max` on that axis) in
  `Assets/Scripts/Systems/Camera/LevelBounds.cs`. No `MonoBehaviour`/`Component`/scene dependency
  (constitution Principle III; spec FR-005).
- [ ] T003 Define the `ICameraHalfExtentProvider` interface (`Vector2 GetGroundHalfExtent()`) in
  `Assets/Scripts/Systems/Camera/ICameraHalfExtentProvider.cs` — the seam this feature's controller
  reads the camera's visible half-extent through. This lets `CameraFollowController` (Phase 3)
  consume a real implementation from movement-and-camera/004's `CameraRigController` once that
  feature exists, without this feature depending on 004's code at build time (spec FR-006,
  Assumptions; see 004's own `tasks.md` T019, which implements this interface).
- [ ] T004 Define the `CameraFollowSystem` plain C# static class with a method stub `Resolve(
  Vector2 currentPosition, Vector2 targetPosition, float lerpFactor, float deltaTime, Vector2
  halfExtent, LevelBounds bounds, float boundsInset) : Vector2` in
  `Assets/Scripts/Systems/Camera/CameraFollowSystem.cs`. No `MonoBehaviour`/`Component`/scene
  dependency (constitution Principle III/FR-008).
- [ ] T005 Implement `Lerp(Vector2 current, Vector2 target, float lerpFactor, float deltaTime) :
  Vector2` on `CameraFollowSystem` as a separately testable method, using a frame-rate-independent
  smoothing formula so `cameraFollowLerpFactor` means the same thing regardless of device frame
  rate (extends T004; FR-001). The exact formula is this task's own technical decision, not fixed
  by `spec.md` (constitution Principle I).
- [ ] T006 Implement `ClampToBounds(Vector2 position, Vector2 halfExtent, LevelBounds bounds,
  float boundsInset) : Vector2` on `CameraFollowSystem` as a separately testable method: for each
  axis independently, if the bounds are wide enough for `halfExtent*2 + boundsInset*2` on that
  axis, clamp the position so `position ± halfExtent ± boundsInset` stays within `Min`/`Max`;
  otherwise center on that axis; and if `LevelBounds.IsValidOnAxis` is `false` for an axis, skip
  clamping that axis entirely rather than producing `NaN`/garbage (extends T004; FR-004, FR-007,
  and the malformed-bounds Edge Case).
- [ ] T007 Wire `Resolve` to call `Lerp` then `ClampToBounds` in sequence (extends T004/T005/T006).
- [ ] T008 Implement `SnapTo(Vector2 targetPosition, Vector2 halfExtent, LevelBounds bounds, float
  boundsInset) : Vector2` on `CameraFollowSystem`, calling `ClampToBounds` directly on the target
  position with no lerp step (extends T006; FR-009).

**Checkpoint**: `CameraFollowSystem` compiles and is ready for story-specific behavior and tests.

---

## Phase 3: User Story 1 - The Camera Smoothly Tracks the Player (Priority: P1) 🎯 MVP

**Goal**: The camera's tracked position eases toward the player's position every frame, never
snapping instantly and never overshooting.

**Independent Test**: Move a simulated player position by a large, sudden amount and confirm the
camera's position moves toward it gradually over multiple frames, converging within a bounded
number of frames.

### Tests for User Story 1

- [ ] T009 [P] [US1] EditMode test: `Lerp` moves partway toward `target` after one call and does
  not reach it in a single step for `lerpFactor < 1`, in
  `Assets/Tests/EditMode/Systems/Camera/CameraFollowSystemLerpTests.cs`.
- [ ] T010 [P] [US1] EditMode test: repeated calls to `Lerp` (feeding each result back in as the
  next call's `current`) converge to within a small epsilon of `target` within a bounded number of
  calls, in the same test file as T009.
- [ ] T011 [P] [US1] EditMode test: `Lerp` never overshoots past `target` on either axis — the
  distance from the result to `target` never exceeds the distance from `current` to `target`
  before the step, in the same test file as T009.

### Implementation for User Story 1

- [ ] T012 [US1] Implement `CameraFollowController` `MonoBehaviour`: holds the camera's current
  tracked position, a `PlayerCharacter` `Transform` reference, and calls
  `CameraFollowSystem.Resolve` each `LateUpdate`, applying the ground-plane (X/Y) result onto its
  own `Camera`'s `Transform` position (height/depth offset from the fixed tilt setup owned by
  movement-and-camera/004 is out of this task's scope), in
  `Assets/Scripts/MonoBehaviours/Camera/CameraFollowController.cs`.

**Checkpoint**: Smooth follow works end-to-end; User Story 1 is independently demonstrable via
T009–T011 plus the manual on-device check in Phase 7.

---

## Phase 4: User Story 2 - The Camera Never Shows Outside the Level (Priority: P1)

**Goal**: The camera's resolved position never lets the visible frame extend past the current
level's authored bounds, on either or both axes at once, and resumes normal following once the
player moves back inside.

**Independent Test**: Move the simulated player position past the level's authored bounds in each
direction and confirm the camera's resulting position never lets the visible frame extend beyond
those bounds.

### Tests for User Story 2

- [ ] T013 [P] [US2] EditMode test: `ClampToBounds` keeps `position ± halfExtent ± boundsInset`
  within `bounds.Min`/`bounds.Max` for a target position placed outside the bounds on one axis, in
  `Assets/Tests/EditMode/Systems/Camera/CameraFollowSystemClampTests.cs`.
- [ ] T014 [P] [US2] EditMode test: `ClampToBounds` clamps both axes simultaneously when the
  position is outside bounds on both axes at once (the level-corner case), in the same test file
  as T013.
- [ ] T015 [P] [US2] EditMode test: a position already within the clamped range is returned
  unchanged — the clamp is not sticky, so normal following resumes as soon as the player is back
  inside — in the same test file as T013.
- [ ] T016 [P] [US2] EditMode test: a position exactly on the boundary line (inclusive) is not
  pulled further inward than the boundary itself, in the same test file as T013.
- [ ] T017 [P] [US2] EditMode test: malformed bounds (`Min > Max` on an axis) do not produce
  `NaN`/`Infinity`/wildly-out-of-level output — that axis is left unclamped instead of crashing, in
  the same test file as T013 (covers spec Edge Case: malformed level bounds).

### Implementation for User Story 2

- [ ] T018 [US2] Define `LevelBoundsAuthoring` `MonoBehaviour` exposing serialized `Min`/`Max`
  `Vector2` fields per scene and producing a `LevelBounds` value for `CameraFollowController` to
  read, in `Assets/Scripts/MonoBehaviours/Camera/LevelBoundsAuthoring.cs` (spec FR-005: per-level
  authored data, not a shared `GameConfig` value, since each floor's footprint differs).
- [ ] T019 [US2] Wire `CameraFollowController` to read the active scene's `LevelBoundsAuthoring`
  and an injected `ICameraHalfExtentProvider` reference each `LateUpdate`, passing both into
  `CameraFollowSystem.Resolve` (extends T012). Until movement-and-camera/004's `CameraRigController`
  exists to supply the real value, use a minimal interim `ICameraHalfExtentProvider` implementation
  that computes half-extent directly from `Camera.orthographicSize`/`aspect` with no
  tilt-foreshortening, clearly comment-flagged `// INTERIM — replace with
  movement-and-camera/004's CameraRigController once it exists` for the later swap.

**Checkpoint**: Boundary clamp works end-to-end (against the interim half-extent); User Stories
1–2 are both independently demonstrable.

---

## Phase 5: User Story 3 - The Player Stays Centered When Not Clamped (Priority: P2)

**Goal**: Whenever the clamp isn't holding the camera back, the resolved position keeps the
player's position exactly at the center of the frame.

**Independent Test**: Hold the player still at the center of a level larger than the viewport for
long enough for the lerp to settle, and confirm the resulting camera position keeps the player
exactly centered.

### Tests for User Story 3

- [ ] T020 [P] [US3] EditMode test: after enough repeated `Resolve` calls with a stationary
  `targetPosition` well inside the bounds, the resulting position converges to exactly
  `targetPosition` (the player is exactly centered once unclamped and settled), in
  `Assets/Tests/EditMode/Systems/Camera/CameraFollowSystemCenteringTests.cs`.

### Implementation for User Story 3

No new production code is required beyond `Resolve`'s existing `Lerp` → `ClampToBounds`
composition (T007): centering-when-unclamped is an emergent guarantee of that composition, not a
separate mechanism (spec FR-003's "Why this priority" already frames it this way). T020 exists to
lock this guarantee in as a regression test.

**Checkpoint**: Centering behavior is verified; User Stories 1–3 are independently demonstrable.

---

## Phase 6: User Story 4 - The Camera Handles a Level Smaller Than the Viewport (Priority: P3)

**Goal**: When a level's bounds are narrower than the camera's visible extent on an axis, the
camera centers on that axis instead of attempting a degenerate clamp.

**Independent Test**: Set a level's bounds narrower than the camera's visible width and confirm
the camera's horizontal position stays centered on that axis regardless of where the player stands
within it.

### Tests for User Story 4

- [ ] T021 [P] [US4] EditMode test: `ClampToBounds` centers on an axis when that axis's bounds
  width is narrower than `halfExtent*2 + boundsInset*2`, regardless of where within the level the
  target lies on that axis, in
  `Assets/Tests/EditMode/Systems/Camera/CameraFollowSystemSmallLevelTests.cs`.
- [ ] T022 [P] [US4] EditMode test: when both axes are narrower than the viewport at once, both
  axes center independently and the output is identical across repeated calls with the same input
  (no jitter/oscillation), in the same test file as T021.

### Implementation for User Story 4

- [ ] T023 [US4] Ensure `ClampToBounds`'s axis-narrower-than-viewport branch (from T006) is
  evaluated independently per axis so mixed cases (one axis clamps normally, the other centers)
  are handled correctly (extends T006; makes T021–T022 pass by construction if not already
  covered).

**Checkpoint**: All four user stories are independently functional and tested.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T024 [P] EditMode test: `SnapTo` returns the fully clamped `targetPosition` immediately, with
  no partial lerp step, in `Assets/Tests/EditMode/Systems/Camera/CameraFollowSystemSnapTests.cs`
  (FR-009).
- [ ] T025 [P] EditMode test: feeding `SnapTo`'s output back in as the `currentPosition` for a
  subsequent normal `Resolve` call does not immediately diverge or produce a visible correction
  jump (the two capabilities compose correctly), in the same test file as T024 (covers the Edge
  Case distinguishing continuous follow across a scene load from the explicit snap-to bypass).
- [ ] T026 Manual on-device/Play Mode smoke check: confirm `CameraFollowController`'s `LateUpdate`
  runs after `PlayerMovementController`'s `Update` in Unity's script execution order (FR-010), and
  confirm a checkpoint respawn/floor transition calls the `SnapTo` path (T008/T019) rather than
  showing a stale, lagging frame (SC-005). Documents the constitution Principle IV live-scene
  exception.
- [ ] T027 Manual on-device validation pass: walk the perimeter of every floor scene and confirm
  no area outside that floor's level geometry is ever visible (SC-006, GDD Ch. 16.3's checklist
  item), using the interim `ICameraHalfExtentProvider` from T019 until movement-and-camera/004
  lands.
- [ ] T028 Once movement-and-camera/004's `CameraRigController` exists, replace T019's interim
  `ICameraHalfExtentProvider` implementation with a reference to `CameraRigController` (which
  implements the same interface per 004's `tasks.md` T019), and re-run T026/T027's manual checks
  to confirm the swap changes nothing observable except half-extent accuracy across device aspect
  ratios.
- [ ] T029 Manual/on-device tuning pass: re-confirm the interim `cameraFollowLerpFactor` (`0.12`)
  and `cameraBoundsInset` (`80`) defaults on target hardware once T028's real half-extent is wired
  in, and record confirmed values back into `GameConfig.asset` (spec Assumptions; mirrors
  movement-and-camera/001's on-device tuning task).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks every user story.
- **User Story 1 (Phase 3)** depends only on Foundational — the MVP slice.
- **User Story 2 (Phase 4)** depends only on Foundational; independently testable via T013–T017
  against the pure `ClampToBounds` method, though its end-to-end controller wiring (T019) uses an
  interim half-extent source until movement-and-camera/004 exists.
- **User Story 3 (Phase 5)** depends only on Foundational (specifically T007); independently
  testable via T020.
- **User Story 4 (Phase 6)** depends only on Foundational (specifically T006); independently
  testable via T021–T022.
- **Polish (Phase 7)** depends on all four user stories being complete. T028 additionally depends
  on movement-and-camera/004 being implemented — until then, T019's interim provider stands in
  and this feature is still fully shippable and testable on its own.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them.
- `CameraFollowSystem` stays a pure, engine-lifecycle-independent class throughout; all four user
  stories add methods or assertions to it, not new classes — `CameraFollowController` is the only
  place any `UnityEngine.Camera`/`Transform` object is ever touched, keeping the constitution
  Principle III split intact.
- The `ICameraHalfExtentProvider` seam (T003) exists specifically to let this feature ship and be
  fully tested before movement-and-camera/004 does, per ROADMAP.md §5's `003→004` build order,
  without this feature's own correctness depending on unwritten code.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- Commit after each task or logical group.
