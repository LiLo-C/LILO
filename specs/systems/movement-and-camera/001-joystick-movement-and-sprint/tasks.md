---
description: "Task list for Joystick Movement & Sprint"
---

# Tasks: Joystick Movement & Sprint

**Input**: Design documents from `specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md`

**Prerequisites**: `spec.md` (this feature). Depends on
`specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md` for `GameState`/
`GameManager` (not redefined here).

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test.

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1 → P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user stories (US1–US4)

## Phase 1: Setup

- [ ] T001 Add player movement fields to the existing `GameConfig` `ScriptableObject` class in
  `Assets/Scripts/Config/GameConfig.cs`: `walkSpeed` (`float`, default `1.0`), `sprintMultiplier`
  (`float`, default `1.6`), `sprintJoystickThreshold` (`float`, default `0.9`, comment noting it
  is a carried-over value pending on-device re-confirmation), `joystickDeadZone` (`float`, no
  default — pending on-device tuning), `joystickDiameter` (`float`, no default — pending),
  `joystickOpacity` (`float`, no default — pending), `joystickCenterOffset` (`Vector2`, no default
  — pending). Do not create a second config asset/class — this is an addition to the single
  existing `GameConfig.asset` source.

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T002 Define the `MovementInput` value type (direction `Vector2`, magnitude `float` 0–1) in
  `Assets/Scripts/Systems/Movement/MovementInput.cs`.
- [ ] T003 Define the `MovementResult` value type (world-space velocity `Vector2`, `IsSprinting`
  `bool`) in `Assets/Scripts/Systems/Movement/MovementResult.cs`.
- [ ] T004 Define the `MovementSystem` plain C# class with a static/stateless method
  `Compute(MovementInput input, GameConfig config) : MovementResult` in
  `Assets/Scripts/Systems/Movement/MovementSystem.cs`. No `MonoBehaviour`/`Component`/scene
  dependency (constitution Principle III).
- [ ] T005 Add a `GameConfig` validation helper (e.g. `GameConfig.Validate()` or an editor-time
  check) that flags `joystickDeadZone >= sprintJoystickThreshold` as an invalid configuration, in
  `Assets/Scripts/Config/GameConfig.cs` (extends T001; covers spec FR-013).

**Checkpoint**: `MovementSystem` compiles and is ready for story-specific behavior and tests.

---

## Phase 3: User Story 1 - Walking Around With the Joystick (Priority: P1) 🎯 MVP

**Goal**: Joystick deflection above the dead zone and below the sprint threshold moves the
character at `walkSpeed` in the input direction; releasing the joystick stops movement instantly.

**Independent Test**: In an empty scene with one `PlayerCharacter`, drive joystick input in each
cardinal direction and confirm position changes at `walkSpeed`; release and confirm an immediate
stop.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test: `MovementSystem.Compute` returns a velocity of magnitude
  `walkSpeed` (direction matching input) for a deflection strictly between the dead zone and the
  sprint threshold, in
  `Assets/Tests/EditMode/Systems/Movement/MovementSystemWalkTests.cs`.
- [ ] T007 [P] [US1] EditMode test: `MovementSystem.Compute` returns zero velocity for a
  zero-magnitude `MovementInput`, in the same test file as T006.

### Implementation for User Story 1

- [ ] T008 [US1] Implement the walk-speed branch of `MovementSystem.Compute` in
  `Assets/Scripts/Systems/Movement/MovementSystem.cs` (extends T004; makes T006/T007 pass).
- [ ] T009 [US1] Implement `JoystickInputAdapter` `MonoBehaviour` that reads the Unity Input System
  drag/touch state for the on-screen joystick and produces a `MovementInput` each frame, in
  `Assets/Scripts/MonoBehaviours/Input/JoystickInputAdapter.cs`.
- [ ] T010 [US1] Implement `PlayerMovementController` `MonoBehaviour` that calls
  `MovementSystem.Compute` each `Update` with the adapter's `MovementInput` and `GameConfig`, and
  applies the resulting velocity to the `PlayerCharacter`'s `Transform`, in
  `Assets/Scripts/MonoBehaviours/Player/PlayerMovementController.cs`.

**Checkpoint**: Character walks and stops correctly; User Story 1 is independently demonstrable.

---

## Phase 4: User Story 2 - Sprinting Without a Button (Priority: P1)

**Goal**: Full (or near-full) joystick deflection produces `walkSpeed × sprintMultiplier` with no
separate input; easing back below threshold returns to walk speed on the same frame.

**Independent Test**: Push the joystick to full deflection and confirm speed equals
`walkSpeed × sprintMultiplier`; ease back and confirm an immediate return to `walkSpeed`.

### Tests for User Story 2

- [ ] T011 [P] [US2] EditMode test: `MovementSystem.Compute` returns a velocity of magnitude
  `walkSpeed × sprintMultiplier` for a deflection at or above `sprintJoystickThreshold`, in
  `Assets/Tests/EditMode/Systems/Movement/MovementSystemSprintTests.cs`.
- [ ] T012 [P] [US2] EditMode test: `MovementResult.IsSprinting` is `true` only when deflection is
  at or above `sprintJoystickThreshold`, and `false` otherwise (including exactly one tick below
  threshold), in the same test file as T011.
- [ ] T013 [P] [US2] EditMode test: sprint has no memory — feeding a below-threshold input
  immediately after an above-threshold input on the previous call returns walk speed, not sprint
  speed, in the same test file as T011 (covers spec Edge Case: sprint never "sticks").

### Implementation for User Story 2

- [ ] T014 [US2] Implement the sprint-threshold branch of `MovementSystem.Compute` (extends T008;
  makes T011–T013 pass).
- [ ] T015 [US2] Expose `IsSprinting` from `PlayerMovementController` as a read-only property for
  other systems (noise, audio) to consume, in
  `Assets/Scripts/MonoBehaviours/Player/PlayerMovementController.cs` (extends T010).

**Checkpoint**: Sprint works end-to-end and is readable by other systems; User Stories 1–2 are both
independently demonstrable.

---

## Phase 5: User Story 3 - Small, Accidental Nudges Don't Move the Character (Priority: P2)

**Goal**: Deflection below `joystickDeadZone` produces exactly zero movement, including under
sustained jitter.

**Independent Test**: Feed a sequence of small jittering deflection values below the dead zone over
several seconds and confirm zero net displacement.

### Tests for User Story 3

- [ ] T016 [P] [US3] EditMode test: `MovementSystem.Compute` returns exactly zero velocity for any
  magnitude strictly below `joystickDeadZone`, in
  `Assets/Tests/EditMode/Systems/Movement/MovementSystemDeadzoneTests.cs`.
- [ ] T017 [P] [US3] EditMode test: a magnitude exactly equal to `joystickDeadZone` produces
  non-zero movement (inclusive upper bound, per spec Edge Cases), in the same test file as T016.

### Implementation for User Story 3

- [ ] T018 [US3] Implement the dead-zone clamp as the first check in `MovementSystem.Compute`
  (extends T008/T014; makes T016–T017 pass).

**Checkpoint**: Dead zone behaves correctly at both boundaries; User Stories 1–3 are independently
demonstrable.

---

## Phase 6: User Story 4 - Diagonal Input Never Out-Runs Straight Input (Priority: P3)

**Goal**: Diagonal deflection at maximum magnitude produces the same top speed as cardinal
deflection at maximum magnitude.

**Independent Test**: Compare speed at full diagonal deflection against speed at full cardinal
deflection.

### Tests for User Story 4

- [ ] T019 [P] [US4] EditMode test: for a raw diagonal input vector (e.g. `(1, 1)`) normalized to
  magnitude `1.0`, `MovementSystem.Compute`'s resultant speed equals the resultant speed for a
  cardinal input of magnitude `1.0`, in
  `Assets/Tests/EditMode/Systems/Movement/MovementSystemNormalizationTests.cs`.

### Implementation for User Story 4

- [ ] T020 [US4] Ensure `JoystickInputAdapter` (or `MovementInput`'s construction) normalizes/clamps
  raw drag-vector magnitude to the 0–1 range before it ever reaches `MovementSystem`, in
  `Assets/Scripts/MonoBehaviours/Input/JoystickInputAdapter.cs` (extends T009; makes T019 pass by
  construction).

**Checkpoint**: All four user stories are independently functional and tested.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T021 [P] EditMode test: an invalid `GameConfig` (`joystickDeadZone >= sprintJoystickThreshold`)
  is flagged by the T005 validation check, in
  `Assets/Tests/EditMode/Systems/Config/GameConfigMovementValidationTests.cs`.
- [ ] T022 Manual on-device validation pass: tune `joystickDeadZone`, `sprintJoystickThreshold`,
  `joystickDiameter`, `joystickOpacity`, and `joystickCenterOffset` on target hardware with 2–3
  outside testers per GDD Ch. 20.2's Fase 1 test plan item, and record the confirmed values back
  into `GameConfig.asset`. Documents the constitution Principle IV live-input-device exception for
  this feature.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks every user story.
- **User Story 1 (Phase 3)** depends only on Foundational — the MVP slice.
- **User Story 2 (Phase 4)** depends only on Foundational; extends the same `MovementSystem.Compute`
  method as US1 but is independently testable via T011–T013 without US3/US4 existing yet.
- **User Story 3 (Phase 5)** depends only on Foundational; independently testable via T016–T017.
- **User Story 4 (Phase 6)** depends only on Foundational; independently testable via T019.
- **Polish (Phase 7)** depends on all four user stories being complete.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them.
- All four user stories converge on the same `MovementSystem.Compute` method; this is expected —
  each story adds one more branch/guard to a small, single-purpose function, not a new class.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- Commit after each task or logical group.
