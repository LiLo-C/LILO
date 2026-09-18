---
description: "Task list for Pause Menu and Time Freeze"
---

# Tasks: Pause Menu and Time Freeze

**Input**: Design documents from
`specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/spec.md`

**Prerequisites**: `spec.md` (this feature). Depends on
`specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md` for `GameState`/
`GameManager` (not redefined here) and
`specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md` for transition
status. This feature is in turn depended on by
`specs/systems/audio/002-dynamic-mix-state-machine/spec.md` (FR-003, its own "pause/settings
integration" task) and `specs/systems/haptics/001-haptic-trigger-events/spec.md` (FR-002, its own
pause gate) and is the feature that
`specs/systems/flashlight-and-battery/002-battery-real-time-drain-timer/spec.md` explicitly defers
to for "whether the [real-time drain] clock freezes while paused" — this spec owns producing that
signal; it does not own those systems' consumption of it.

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test.

**Organization**: `spec.md` defines a single user story (US1). Tasks are still split into
Setup/Foundational/Story/Polish phases so each unit stays small and independently completable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user story (US1)

## Phase 1: Setup

- [ ] T001 [P] Add `IsPaused` (`bool`, default `false`) to the shared `GameState` class in
  `Assets/Scripts/State/GameState.cs` (extends shared-config-and-state/002's `GameState`
  additively, per constitution Principle V) — the one authoritative flag every time/input-owning
  system checks before advancing.
- [ ] T002 [P] Define the `PauseRequestResult` enum (`Accepted`, `RejectedAlreadyPaused`,
  `RejectedTransitionInProgress`, `RejectedRunEnded`, `RejectedNotPaused`,
  `RejectedDuplicateDispatch`) in `Assets/Scripts/Systems/Pause/PauseRequestResult.cs`.
- [ ] T003 [P] Define the `PauseGuardContext` value type (`bool IsTransitionInProgress`,
  `bool IsRunEnded`) in `Assets/Scripts/Systems/Pause/PauseGuardContext.cs` — built by the caller
  from `SceneTransitionManager` (progression-and-scene-flow/002) and `GameState`'s run outcome.
- [ ] T004 [P] Define the `PauseSessionDispatchGuard` value type (`bool HasDispatchedExitAction`)
  in `Assets/Scripts/Systems/Pause/PauseSessionDispatchGuard.cs` — one instance per pause session,
  covers FR-002's no-duplicate-dispatch rule for Resume/Settings/Quit-Restart.

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T005 Define the `PauseSystem` plain C# static class with method stubs
  `EvaluatePauseRequest(bool isPaused, PauseGuardContext ctx) : PauseRequestResult`,
  `EvaluateResumeRequest(bool isPaused) : PauseRequestResult`, and
  `EvaluateExitAction(PauseSessionDispatchGuard guard) : PauseRequestResult` in
  `Assets/Scripts/Systems/Pause/PauseSystem.cs`. No `MonoBehaviour`/`Component`/scene dependency
  (constitution Principle III).

**Checkpoint**: `PauseSystem` compiles and is ready for story-specific branches and tests.

---

## Phase 3: User Story 1 - Pause safely (P1) 🎯 MVP

**Goal**: The player can pause gameplay, see a clear menu, and resume without the monster, timers,
or input advancing behind the menu.

**Independent Test**: Pause during movement, chase, battery drain, and a transition; advance
simulation and assert gameplay state is frozen.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test: `EvaluatePauseRequest` returns `Accepted` when `isPaused` is
  `false` and `ctx` is clear, in
  `Assets/Tests/EditMode/Systems/Pause/PauseSystemPauseTests.cs`.
- [ ] T007 [P] [US1] EditMode test: `EvaluatePauseRequest` returns `RejectedAlreadyPaused` when
  `isPaused` is already `true` (covers spec Edge Case "repeated pause"), same file.
- [ ] T008 [P] [US1] EditMode test: `EvaluatePauseRequest` returns
  `RejectedTransitionInProgress` when `ctx.IsTransitionInProgress` is `true`, regardless of
  `isPaused` (covers Edge Case "transition in progress"), same file.
- [ ] T009 [P] [US1] EditMode test: `EvaluatePauseRequest` returns `RejectedRunEnded` when
  `ctx.IsRunEnded` is `true` (covers Edge Case "death/ending"), same file.
- [ ] T010 [P] [US1] EditMode test: `EvaluateResumeRequest` returns `Accepted` when `isPaused` is
  `true`, and `RejectedNotPaused` when `isPaused` is `false`, in
  `Assets/Tests/EditMode/Systems/Pause/PauseSystemResumeTests.cs`.
- [ ] T011 [P] [US1] EditMode test: a full pause→resume round trip on a `GameState` fixture
  changes only `IsPaused` and leaves lives, current floor, checkpoint, key inventory, battery
  slot, hiding state, and run outcome bit-for-bit unchanged (SC-001), in
  `Assets/Tests/EditMode/Systems/Pause/PauseSystemStateIsolationTests.cs`.
- [ ] T012 [P] [US1] EditMode test: `EvaluateExitAction` returns `Accepted` on the first call for
  a fresh `PauseSessionDispatchGuard` and `RejectedDuplicateDispatch` on every subsequent call
  within the same session (covers FR-002), in
  `Assets/Tests/EditMode/Systems/Pause/PauseSystemDispatchGuardTests.cs`.

### Implementation for User Story 1

- [ ] T013 [US1] Implement the `EvaluatePauseRequest` branches in `PauseSystem.cs` (extends T005;
  makes T006–T009 pass).
- [ ] T014 [US1] Implement the `EvaluateResumeRequest` branch in `PauseSystem.cs` (makes T010
  pass) and confirm it mutates no field beyond `IsPaused` (makes T011 pass).
- [ ] T015 [US1] Implement `EvaluateExitAction` in `PauseSystem.cs` (makes T012 pass).
- [ ] T016 [US1] Implement `PauseInputGate` `MonoBehaviour` that listens for the pause input
  action, builds a `PauseGuardContext` from `SceneTransitionManager` and `GameState`'s run
  outcome, and calls `PauseSystem.EvaluatePauseRequest`; on `Accepted`, sets
  `Time.timeScale = 0`, swaps the Input System action map to UI-only, and sets
  `GameState.IsPaused = true`, in `Assets/Scripts/MonoBehaviours/UI/PauseInputGate.cs`.
- [ ] T017 [US1] Implement `PauseMenuView` UI (Resume, Settings, Quit/Restart buttons), showing
  itself only while `GameState.IsPaused` is `true`, in `Assets/Scripts/UI/PauseMenuView.cs`.
- [ ] T018 [US1] Implement `PauseMenuController` `MonoBehaviour` that owns one
  `PauseSessionDispatchGuard` per pause session and wires `PauseMenuView`'s Resume button through
  `PauseSystem.EvaluateExitAction` + `EvaluateResumeRequest`, restoring `Time.timeScale = 1`, the
  gameplay action map, and `GameState.IsPaused = false`, in
  `Assets/Scripts/UI/PauseMenuController.cs`.
- [ ] T019 [US1] Wire the Settings button through the same dispatch guard to open the Settings
  Menu (game-shell-ui/002) as an overlay without resuming — `GameState.IsPaused` stays `true`
  while Settings is shown, in `Assets/Scripts/UI/PauseMenuController.cs` (extends T018).
- [ ] T020 [US1] Wire the Quit/Restart button through the same dispatch guard into the shared
  `GameManager`'s new-run/floor-reset operation (shared-config-and-state/002 FR-004), in
  `Assets/Scripts/UI/PauseMenuController.cs` (extends T018).
- [ ] T021 [US1] Expose a `PauseStateChanged` event (`bool IsPaused`) from `PauseInputGate`/
  `PauseMenuController` — satisfies FR-003 ("audio and haptics MUST follow the pause policy") as a
  notification contract only; the mix-state and haptic-gate logic itself is owned by audio/002 and
  haptics/001's own pause-integration tasks, in `Assets/Scripts/UI/PauseMenuController.cs`.
- [ ] T022 [US1] Document (in a code comment on `PauseInputGate`) that `GameState.IsPaused` is the
  one flag any real-time/wall-clock system not affected by `Time.timeScale = 0` — most notably the
  battery real-time drain adapter (flashlight-and-battery/002, whose spec explicitly defers
  pause-freeze behavior to this feature) — MUST check before advancing, fulfilling that deferred
  contract without this feature reaching into the drain adapter's own file, in
  `Assets/Scripts/MonoBehaviours/UI/PauseInputGate.cs`.

**Checkpoint**: Pause freezes gameplay, resumes to the exact prior mode, and dispatch is
idempotent; User Story 1 is independently demonstrable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T023 [P] EditMode test: repeated pause requests while already paused, and repeated resume
  requests while not paused, never throw and never toggle `IsPaused` a second time (exhaustive
  idempotency over spec Edge Case "repeated pause"), in
  `Assets/Tests/EditMode/Systems/Pause/PauseSystemIdempotencyTests.cs`.
- [ ] T024 Handle app backgrounding: route `OnApplicationPause(true)` through the same
  `PauseSystem.EvaluatePauseRequest` path as a manual pause rather than a second ad-hoc freeze
  path (covers Edge Case "app backgrounding"), in
  `Assets/Scripts/MonoBehaviours/UI/PauseInputGate.cs`.
- [ ] T025 Manual on-device verification pass: pause during movement, chase, battery drain, and a
  scene-transition boundary; confirm the monster, all timers (including the real-time battery
  drain), and input are unchanged across the pause (SC-001), and file a gap against that system's
  own spec — not a fix here — for anything found not checking `IsPaused` (Scope: "pause shell and
  time-freeze contract only"). Documents the constitution Principle IV live-scene exception for
  this feature.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks the user story.
- **User Story 1 (Phase 3)** depends only on Foundational.
- **Polish (Phase 4)** depends on User Story 1 being complete.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- This feature does not implement audio mixing, haptic patterns, or other systems' own
  pause-checks — it only produces and documents the `IsPaused`/`PauseStateChanged` signal they
  consume, per the spec's Scope.
- Commit after each task or logical group.
