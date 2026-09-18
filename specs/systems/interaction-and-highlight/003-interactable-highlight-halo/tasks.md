---
description: "Task list for Interactable Highlight Halo"
---

# Tasks: Interactable Highlight Halo

**Input**: Design documents from
`specs/systems/interaction-and-highlight/003-interactable-highlight-halo/spec.md`

**Prerequisites**: `spec.md` (this feature; required). Depends on
`specs/systems/interaction-and-highlight/001-nearest-interactable-detection/spec.md` for the
selected target (`NearestInteractableDetector.CurrentSelection`/`SelectionChanged`) — not
redefined here. Independent of `002-context-sensitive-action-button`.

**Tests**: Included per constitution Principle IV — halo visibility and the change-detection
gate that prevents redundant re-triggering are pure logic and MUST be fully covered by EditMode
tests. Readability, non-color contrast, and low-light behavior need a live scene and are
validated by a manual quickstart pass, per the Principle IV exception.

**Organization**: Tasks are grouped by user story from `spec.md` (this feature has one story,
US1), in priority order.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, or independent assertions in the same file, with
  no ordering dependency on another unfinished task in this list)
- **[Story]**: Maps the task to `spec.md`'s user story (US1), or unlabeled for
  Setup/Foundational/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Interaction/`
- MonoBehaviour adapters: `Assets/Scripts/MonoBehaviours/Interaction/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Interaction/`
- No new `GameConfig` fields are introduced by this feature — halo appearance is per-prefab
  renderer/material data, not a tunable; nothing to add to
  `Assets/Scripts/Config/GameConfig.cs`.

---

## Phase 1: Setup

- [ ] T001 Create the `Assets/Scripts/MonoBehaviours/Interaction/` directory (via the first file
  below — `Assets/Scripts/Systems/Interaction/` and `Assets/Tests/EditMode/Systems/Interaction/`
  already exist from `001-nearest-interactable-detection`).

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T002 [P] Define the `HaloVisibilitySystem` plain C# static class with a stub
  `bool Evaluate(bool isThisTargetSelected, bool isSuppressed)` in
  `Assets/Scripts/Systems/Interaction/HaloVisibilitySystem.cs`. No `MonoBehaviour`/scene
  dependency (constitution Principle III).
- [ ] T003 [P] Define the `HaloTransition` plain C# static class with a stub
  `bool HasChanged(bool previousVisibility, bool currentVisibility)` in
  `Assets/Scripts/Systems/Interaction/HaloTransition.cs` — gates re-triggering the halo's
  enable/disable effect to only an actual state flip (supports FR-003's "avoid distracting
  flicker during stable selection").

**Checkpoint**: `HaloVisibilitySystem` and `HaloTransition` compile and are ready for
story-specific tests.

---

## Phase 3: User Story 1 - Understand What Can Be Acted On (Priority: P2) 🎯 MVP

**Goal**: The currently selected interactable receives a restrained halo that disappears the
instant selection is lost, pause/hiding/death is entered, or the target is gone.

**Independent Test**: Move selection across candidates and assert exactly one halo is active at
each step (spec.md's own Independent Test).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no visibility logic yet) before implementing.

- [ ] T004 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/HaloVisibilitySystemTests.cs`,
  write: `Evaluate(isSelected: true, isSuppressed: false)` returns `true`;
  `Evaluate(isSelected: false, isSuppressed: <any>)` returns `false` (US1 Acceptance Scenario 1:
  valid target's halo visible, prior target's halo cleared).
- [ ] T005 [P] [US1] In the same file as T004, write: `Evaluate(isSelected: true, isSuppressed: true)`
  returns `false` — pause, hiding, or death suppresses an otherwise-active halo (US1 Acceptance
  Scenario 2; FR-001).
- [ ] T006 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/HaloVisibilitySystemSelectionSequenceTests.cs`,
  write: for a simulated selection sequence across candidates A → B → none → A, evaluating every
  candidate at each step reports exactly one `true` when a target is selected and zero when none
  is — never more than one simultaneously (SC-001: "zero orphan or duplicate halos").
- [ ] T007 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/HaloTransitionTests.cs`, write:
  `HasChanged(false, false)` and `HasChanged(true, true)` both return `false` (no redundant
  re-trigger while a stable selection holds across many frames); `HasChanged(false, true)` and
  `HasChanged(true, false)` each return `true` exactly once per flip (FR-003).

### Implementation for User Story 1

- [ ] T008 [US1] Implement `HaloVisibilitySystem.Evaluate`: return `isThisTargetSelected &&
  !isSuppressed` (extends T002; makes T004–T006 pass).
- [ ] T009 [US1] Implement `HaloTransition.HasChanged`: return `previousVisibility !=
  currentVisibility` (extends T003; makes T007 pass).

**Checkpoint**: User Story 1 is fully functional and independently testable via pure C# EditMode
tests — single-halo selection tracking, suppression, and flicker-free stability all covered.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T010 [P] EditMode test: a candidate that is never the current selection reports
  `Evaluate(isSelected: false, ...)` → `false` on every call regardless of how many prior frames
  it was checked, and never reports a "changed to visible" transition via `HasChanged` (guards
  against an orphaned halo persisting via stale cached state), in
  `Assets/Tests/EditMode/Systems/Interaction/HaloVisibilityOrphanGuardTests.cs` (Edge Cases:
  destroyed target, disabled renderer, rapid swaps).
- [ ] T011 Implement `InteractableHighlightView` `MonoBehaviour` in
  `Assets/Scripts/MonoBehaviours/Interaction/InteractableHighlightView.cs`: attached to each
  interactable prefab alongside its `IInteractionTarget`/`IInteractionAction` components;
  subscribes to `001`'s `NearestInteractableDetector` `SelectionChanged` event; on each change
  (and on relevant `GameState` pause/hiding/death changes) computes
  `HaloVisibilitySystem.Evaluate` and, only when `HaloTransition.HasChanged` reports a flip,
  toggles its halo renderer/material on or off; unsubscribes in `OnDisable`/`OnDestroy` so a
  destroyed target leaves no dangling subscription or lit halo (depends on T008, T009, and `001`'s
  `NearestInteractableDetector`).
- [ ] T012 Manual quickstart-style validation pass on target hardware: verify halo readability and
  non-color contrast in both lit and low-light scenes, confirm the halo never reveals a target
  through a wall, and run a first-time-player usability check that the actionable object is
  identifiable (documents the constitution Principle IV live-scene exception for
  rendering/readability — the selection/visibility logic itself is already fully covered by
  EditMode tests above; operationalizes SC-002).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks the user story.
- **User Story 1 (Phase 3)** depends only on Foundational.
- **Polish (Phase 4)** depends on User Story 1 being complete; T011 is the first point this
  feature is wired into a live scene against `001`'s detector.

## Parallel Opportunities

- T002 and T003 can run in parallel (independent files, no shared logic).
- All [P] test-writing tasks within Phase 3 can run in parallel with each other.

## Notes

- [P] tasks touch different files, or independent assertions with no ordering dependency on
  another unfinished task in this list.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- This feature introduces no new `GameConfig` fields — do not add any.
- Commit after each task or logical group.
