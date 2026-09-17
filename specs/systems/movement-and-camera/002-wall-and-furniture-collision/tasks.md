---
description: "Task list for Wall & Furniture Collision (Sliding Resolution)"
---

# Tasks: Wall & Furniture Collision (Sliding Resolution)

**Input**: Design documents from
`specs/systems/movement-and-camera/002-wall-and-furniture-collision/spec.md`

**Prerequisites**: `spec.md` (this feature). Depends on
`movement-and-camera/001-joystick-movement-and-sprint` (supplies the tentative per-frame movement
step this resolver corrects) and
`shared-config-and-state/002-shared-game-state-and-manager` (for `GameState`/`GameManager`, not
redefined here).

**Tests**: Included per constitution Principle IV — this entire feature is pure math and MUST be
fully covered by EditMode tests with no live-scene exception.

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1 → P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user stories (US1–US4)

## Phase 1: Setup

- [ ] T001 Add `playerRadius` (`float`, default `20`, comment noting it is a carried-over value
  pending on-device re-confirmation per this project's world scale) to the existing `GameConfig`
  `ScriptableObject` class in `Assets/Scripts/Config/GameConfig.cs`. Do not create a second config
  asset/class.

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T002 Define the `Obstacle` value type (axis-aligned bounds: `min`/`max` as `Vector2`, or
  center + half-extents — pick one consistent representation) in
  `Assets/Scripts/Systems/Collision/Obstacle.cs`.
- [ ] T003 Define the `CollisionResult` value type (corrected position `Vector2`) in
  `Assets/Scripts/Systems/Collision/CollisionResult.cs`.
- [ ] T004 Define the `CollisionResolver` plain C# class with a static/stateless method
  `ResolveSingle(Vector2 playerPosition, float playerRadius, Obstacle obstacle) : CollisionResult`
  implementing minimum circle-vs-rect push-out, in
  `Assets/Scripts/Systems/Collision/CollisionResolver.cs`. No `MonoBehaviour`/`Component`/scene
  dependency (constitution Principle III).
- [ ] T005 Add a defensive guard in `CollisionResolver.ResolveSingle` for degenerate obstacle
  bounds (zero width/height, or `min >= max` on an axis) that returns the input position unchanged
  rather than producing NaN/infinity (extends T004; covers spec Edge Cases).

**Checkpoint**: `CollisionResolver.ResolveSingle` compiles and is ready for story-specific tests.

---

## Phase 3: User Story 1 - Sliding Along a Wall Hit at an Angle (Priority: P1) 🎯 MVP

**Goal**: An oblique approach into a single obstacle preserves the tangential component of motion
while removing only the penetrating component.

**Independent Test**: Resolve a player position overlapping one obstacle at a 45° approach and
confirm the tangential component of the resulting displacement is non-zero and the penetrating
component is exactly zero.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test: for an oblique overlap (approach neither head-on nor parallel
  to the obstacle face), `ResolveSingle` returns a position with zero penetration and a non-zero
  displacement component parallel to the obstacle's nearest face, in
  `Assets/Tests/EditMode/Systems/Collision/CollisionResolverSlideTests.cs`.
- [ ] T007 [P] [US1] EditMode test: for a position and obstacle with no overlap at all (including
  an approach exactly parallel to the wall with no penetration), `ResolveSingle` returns the input
  position unchanged, in the same test file as T006.

### Implementation for User Story 1

- [ ] T008 [US1] Implement the minimum-penetration-axis push-out formula in
  `CollisionResolver.ResolveSingle` (extends T004; makes T006/T007 pass).

**Checkpoint**: Sliding along a single obstacle at an angle works correctly; User Story 1 is
independently demonstrable via EditMode tests.

---

## Phase 4: User Story 2 - Stopping Cleanly on a Head-On Wall (Priority: P1)

**Goal**: A perpendicular approach into a single obstacle stops flush at the surface, with zero
penetration and zero frame-to-frame jitter under repeated identical input.

**Independent Test**: Resolve a head-on overlap across a range of approach speeds and confirm the
corrected position always sits flush with the obstacle's face and is identical across repeated
calls with the same input.

### Tests for User Story 2

- [ ] T009 [P] [US2] EditMode test: for a purely perpendicular (head-on) overlap, `ResolveSingle`
  returns a position exactly touching the obstacle's face (zero penetration, zero gap) across a
  range of overlap depths, in
  `Assets/Tests/EditMode/Systems/Collision/CollisionResolverHeadOnTests.cs`.
- [ ] T010 [P] [US2] EditMode test: calling `ResolveSingle` repeatedly with the same head-on input
  produces an identical result every time (no jitter/drift), in the same test file as T009.

### Implementation for User Story 2

- [ ] T011 [US2] Verify/extend the T008 formula so the head-on case (penetration purely along one
  axis) is handled by the same minimum-push-out logic without a special case (extends T008; makes
  T009/T010 pass).

**Checkpoint**: Head-on stopping is clean and stable; User Stories 1–2 are both independently
demonstrable.

---

## Phase 5: User Story 3 - Resolving Cleanly in a Corner (Priority: P2)

**Goal**: Overlapping two perpendicular obstacles at once converges to one stable, zero-penetration
position within a small, fixed number of resolution passes, with no oscillation.

**Independent Test**: Resolve a position overlapping two perpendicular obstacles and confirm
convergence to a stable point with zero penetration against both, unchanged on further identical
calls.

### Tests for User Story 3

- [ ] T012 [P] [US3] EditMode test: for a position overlapping two perpendicular obstacles (a
  corner), iteratively resolving against both in a fixed order converges (within a small, fixed
  iteration cap) to a position with zero penetration against both obstacles, in
  `Assets/Tests/EditMode/Systems/Collision/CollisionResolverCornerTests.cs`.
- [ ] T013 [P] [US3] EditMode test: resolving the same corner scenario a second time from the
  converged position returns that same position unchanged (no oscillation), in the same test file
  as T012.

### Implementation for User Story 3

- [ ] T014 [US3] Implement `CollisionResolver.ResolveMany(Vector2 playerPosition, float
  playerRadius, IReadOnlyList<Obstacle> obstacles) : CollisionResult`, applying `ResolveSingle`
  against each overlapping obstacle in a fixed, repeatable order and iterating until no obstacle
  reports penetration or a small fixed iteration cap is reached, in
  `Assets/Scripts/Systems/Collision/CollisionResolver.cs` (extends T004/T008; makes T012/T013
  pass).

**Checkpoint**: Corner resolution is stable and convergent; User Stories 1–3 are independently
demonstrable.

---

## Phase 6: User Story 4 - Recovering From an Overlapping Spawn (Priority: P3)

**Goal**: A position that starts already overlapping an obstacle (e.g. respawn/checkpoint
placement error) resolves to the nearest valid point, bounded by the original penetration depth.

**Independent Test**: Place a position fully overlapping an obstacle with no input velocity and
confirm the resolved position is the nearest boundary point, at a distance no greater than the
original penetration depth.

### Tests for User Story 4

- [ ] T015 [P] [US4] EditMode test: for a position starting fully inside an obstacle's bounds,
  `ResolveSingle`/`ResolveMany` returns the nearest point outside the obstacle (plus
  `playerRadius`), at a corrective distance no greater than the original penetration depth, in
  `Assets/Tests/EditMode/Systems/Collision/CollisionResolverOverlapSpawnTests.cs`.

### Implementation for User Story 4

- [ ] T016 [US4] Verify/extend T008's minimum-push-out formula correctly handles the
  fully-overlapping (deep-penetration) case using the same nearest-boundary-point logic as a
  shallow overlap, with no special-cased "teleport out" branch (extends T008; makes T015 pass).

**Checkpoint**: All four user stories are independently functional and tested.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T017 [P] EditMode test: a degenerate obstacle (zero width/height or malformed bounds) never
  produces NaN or infinity from `ResolveSingle`, in
  `Assets/Tests/EditMode/Systems/Collision/CollisionResolverDegenerateTests.cs` (covers spec Edge
  Cases; exercises the T005 guard).
- [ ] T018 [P] EditMode test: a player wedged between two obstacles whose push-out vectors directly
  oppose each other (gap narrower than the collision diameter) returns a deterministic, bounded
  position rather than NaN or an unbounded value, in the same test file as T017.
- [ ] T019 Implement `CharacterCollisionController` `MonoBehaviour` that, each frame after
  `PlayerMovementController` (from `movement-and-camera/001`) applies its tentative movement, gathers
  the currently relevant level-authored `Obstacle` bounds and calls `CollisionResolver.ResolveMany`,
  applying the corrected position to the `PlayerCharacter`'s `Transform`, in
  `Assets/Scripts/MonoBehaviours/Player/CharacterCollisionController.cs`.
- [ ] T020 Manual quickstart-style validation pass on target hardware: walk and sprint into walls
  and furniture at varied angles across a prototype room and confirm sliding feels smooth with no
  visible clipping or jitter (documents the constitution Principle IV live-scene exception for the
  integrated, rendered result — the math itself is already fully covered by EditMode tests above).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks every user story.
- **User Story 1 (Phase 3)** depends only on Foundational — the MVP slice (single-obstacle sliding).
- **User Story 2 (Phase 4)** depends only on Foundational; shares the same formula as US1 but is
  independently testable via T009/T010.
- **User Story 3 (Phase 5)** depends on Foundational and reuses `ResolveSingle` (US1/US2's
  implementation) inside `ResolveMany`, but is independently testable via T012/T013 once
  `ResolveSingle` exists.
- **User Story 4 (Phase 6)** depends only on Foundational; independently testable via T015.
- **Polish (Phase 7)** depends on all four user stories being complete; T019 is the first point
  this feature is wired into a live scene.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them.
- All user stories converge on the same minimum-push-out formula (`ResolveSingle`); this is
  intentional — sliding, head-on stopping, corner resolution, and overlap recovery are all the same
  math observed from different starting geometry, not four separate algorithms.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- Commit after each task or logical group.
