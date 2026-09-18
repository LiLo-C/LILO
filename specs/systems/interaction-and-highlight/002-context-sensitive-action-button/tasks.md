---
description: "Task list for Context-Sensitive Action Button"
---

# Tasks: Context-Sensitive Action Button

**Input**: Design documents from
`specs/systems/interaction-and-highlight/002-context-sensitive-action-button/spec.md`

**Prerequisites**: `spec.md` (this feature; required). Depends on
`specs/systems/interaction-and-highlight/001-nearest-interactable-detection/spec.md` for the
selected target (`NearestInteractableDetector.CurrentSelection`/`SelectionChanged`) — not
redefined here.

**Tests**: Included per constitution Principle IV — label mapping, suppression, and the
one-mutation-per-press dispatch guard are pure logic and MUST be fully covered by EditMode tests.
Only touch-input plumbing, safe-area layout, and on-device pause/death behavior need a live
scene, per the Principle IV exception.

**Organization**: Tasks are grouped by user story from `spec.md` (this feature has one story,
US1), in priority order.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, or independent assertions in the same file, with
  no ordering dependency on another unfinished task in this list)
- **[Story]**: Maps the task to `spec.md`'s user story (US1), or unlabeled for
  Setup/Foundational/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Interaction/`
- MonoBehaviour/UI adapters: `Assets/Scripts/UI/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Interaction/`
- No new `GameConfig` fields are introduced by this feature — the dispatch guard is a per-press
  logical lock, not a timing value; nothing to add to `Assets/Scripts/Config/GameConfig.cs`.

---

## Phase 1: Setup

- [ ] T001 Create the `Assets/Scripts/UI/` directory (via the first file below —
  `Assets/Scripts/Systems/Interaction/` and `Assets/Tests/EditMode/Systems/Interaction/` already
  exist from `001-nearest-interactable-detection`).

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T002 [P] Define the `IInteractionAction` interface in
  `Assets/Scripts/Systems/Interaction/IInteractionAction.cs`: `string Label { get; }`,
  `string AccessibilityLabel { get; }`, `bool TryExecute()`. Concrete interactables implement this
  alongside `001`'s `IInteractionTarget` to form the full interactable contract that downstream
  features (e.g. keys-and-doors) build on (FR-001: labels come from this contract, never
  object-name string matching).
- [ ] T003 [P] Define the `ActionButtonState` readonly struct in
  `Assets/Scripts/Systems/Interaction/ActionButtonState.cs`: `bool IsVisible`, `bool IsEnabled`,
  `string Label`, `string AccessibilityLabel`.
- [ ] T004 Define the `ActionButtonSystem` plain C# static class with a stub
  `ActionButtonState BuildState(IInteractionAction target, bool executionSuppressed)` in
  `Assets/Scripts/Systems/Interaction/ActionButtonSystem.cs` (depends on T002, T003). No
  `MonoBehaviour`/scene dependency (constitution Principle III).
- [ ] T005 Define the `ActionDispatchGuard` plain C# class with stub methods `void BeginPress()`,
  `bool TryDispatch(IInteractionAction currentTarget)`, `void EndPress()` in
  `Assets/Scripts/Systems/Interaction/ActionDispatchGuard.cs` (depends on T002).

**Checkpoint**: `ActionButtonSystem` and `ActionDispatchGuard` compile and are ready for
story-specific tests.

---

## Phase 3: User Story 1 - One Obvious Action (Priority: P1) 🎯 MVP

**Goal**: The player receives a single action button whose label and enabled state match the
nearest target, and a press executes at most one action.

**Independent Test**: Feed key, door, hiding spot, battery, and empty-target fixtures; compare
label and command (spec.md's own Independent Test).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no button/dispatch logic yet) before
> implementing.

- [ ] T006 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/ActionButtonSystemNoTargetTests.cs`,
  write: `BuildState(null, false)` returns `IsVisible = false`, `IsEnabled = false` (US1
  Acceptance Scenario 2: "no valid target, then the button is hidden/disabled").
- [ ] T007 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/ActionButtonSystemFixtureMatrixTests.cs`,
  write: for a fixture matrix of stub `IInteractionAction` targets representing a key, door,
  hiding spot, and battery, `BuildState` returns exactly that target's `Label`/`AccessibilityLabel`
  with `IsVisible = true`, `IsEnabled = true` (FR-001; SC-001's fixture matrix).
- [ ] T008 [P] [US1] In the same file as T006, write: `BuildState(target, executionSuppressed: true)`
  (paused/death) reports `IsEnabled = false` for an otherwise-valid target (FR-003).
- [ ] T009 [P] [US1] In the same file as T007, write: a target whose `Label` or
  `AccessibilityLabel` is `null`/empty still yields a non-empty fallback string for both fields
  (Edge Cases: "missing label", "accessibility text").
- [ ] T010 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/ActionDispatchGuardTests.cs`,
  write: after `BeginPress()`, `TryDispatch(target)` calls `target.TryExecute()` exactly once and
  returns its result; `TryDispatch(null)` returns `false` without throwing.
- [ ] T011 [P] [US1] In the same file as T010, write: a second `TryDispatch` call within the same
  `BeginPress`/`EndPress` cycle (simulating repeated touch) does not call `TryExecute` again and
  returns `false` (SC-002: "repeated touch causes one mutation").
- [ ] T012 [P] [US1] In the same file as T010, write: `TryDispatch` always acts on whichever
  `IInteractionAction` instance is passed in at call time — a press cycle that began while target
  A was selected but is dispatched after the selection swapped to target B invokes only B's
  `TryExecute`, never A's (US1 Acceptance Scenario 3: "the target changes, then the old action
  cannot fire").

### Implementation for User Story 1

- [ ] T013 [US1] Implement `ActionButtonSystem.BuildState`: hidden and disabled when `target` is
  `null`; otherwise visible, enabled unless `executionSuppressed`, with `Label`/
  `AccessibilityLabel` copied from the target and a fallback default string substituted when
  either is null/empty (extends T004; makes T006–T009 pass).
- [ ] T014 [US1] Implement `ActionDispatchGuard`: `BeginPress` resets the per-press dispatched
  flag; `TryDispatch` calls the given target's `TryExecute` at most once per
  `BeginPress`/`EndPress` cycle, returning `false` for a `null` target or an already-dispatched
  press; `EndPress` clears state for the next press (extends T005; makes T010–T012 pass).

**Checkpoint**: User Story 1 is fully functional and independently testable via pure C# EditMode
tests — label mapping, suppression, missing-label fallback, and the one-mutation dispatch guard
all covered.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T015 [P] EditMode test: a `TryExecute` that returns `false` (action failure) still counts as
  the press's one dispatch (no retry within the same press) but is distinguishable from a
  successful dispatch by `TryDispatch`'s return value (FR-002's success/failure feedback), in
  `Assets/Tests/EditMode/Systems/Interaction/ActionDispatchGuardFailureTests.cs`.
- [ ] T016 Implement `ContextActionButton` `MonoBehaviour` in
  `Assets/Scripts/UI/ContextActionButton.cs`: subscribes to `001`'s `NearestInteractableDetector`
  `SelectionChanged`/`CurrentSelection`, queries the selected `GameObject`'s `IInteractionAction`
  component, calls `ActionButtonSystem.BuildState` (passing `GameState`'s paused/death flags as
  `executionSuppressed`) to drive the Unity UI `Button`'s visible/interactable state and
  label/accessibility text, and wires pointer-down/up to
  `ActionDispatchGuard.BeginPress`/`TryDispatch`/`EndPress`, exposing success/failure feedback
  hooks for other systems (audio/haptics, out of scope) to consume (depends on T004, T005, T013,
  T014, and `001`'s `NearestInteractableDetector`).
- [ ] T017 Manual quickstart-style validation pass on target hardware: confirm touch target size,
  safe-area placement, and pause/death suppression behavior with a real key/door/battery/hiding-
  spot fixture in a test scene (documents the constitution Principle IV live-scene exception for
  touch input and UI layout — the label/dispatch logic itself is already fully covered by
  EditMode tests above).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup and on `001-nearest-interactable-detection` already
  existing (for the eventual wiring in T016, not for T002–T005 themselves); blocks the user story.
- **User Story 1 (Phase 3)** depends only on Foundational.
- **Polish (Phase 4)** depends on User Story 1 being complete; T016 is the first point this
  feature is wired into a live scene against `001`'s detector.

## Parallel Opportunities

- T002 and T003 can run in parallel; T004 and T005 each depend on T002 but not on each other.
- All [P] test-writing tasks within Phase 3 can run in parallel with each other.

## Notes

- [P] tasks touch different files, or independent assertions with no ordering dependency on
  another unfinished task in this list.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- This feature introduces no new `GameConfig` fields — do not add any.
- Commit after each task or logical group.
