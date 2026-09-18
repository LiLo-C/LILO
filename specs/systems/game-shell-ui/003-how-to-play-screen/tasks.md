---
description: "Task list for How To Play Screen"
---

# Tasks: How To Play Screen

**Input**: Design documents from `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`

**Prerequisites**: `spec.md` (this feature). Opened from the Main Menu and from
`specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/spec.md`'s Pause menu (not redefined
here).

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test.

**Organization**: `spec.md` defines a single user story (US1). Tasks are still split into
Setup/Foundational/Story/Polish phases so each unit stays small and independently completable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user story (US1)

## Phase 1: Setup

- [ ] T001 [P] Define the `HowToPlayPanel` value type (`Id` enum, `TitleKey`, `BodyKey`) in
  `Assets/Scripts/Systems/HowToPlay/HowToPlayPanel.cs` — one instance per GDD-required mechanic.
- [ ] T002 [P] Define the `HowToPlayEntryContext` enum (`MainMenu`, `Pause`) in
  `Assets/Scripts/Systems/HowToPlay/HowToPlayEntryContext.cs` — records where the screen was
  opened from so Back/Close restores the correct prior shell context (FR-003).
- [ ] T003 Create the `HowToPlayContent` static content table keyed by `HowToPlayPanel.Id` in
  `Assets/Scripts/Systems/HowToPlay/HowToPlayContent.cs`, and add the approved concise copy
  (GDD Ch. 9.1) for the **Movement** panel as its first entry (FR-001).
- [ ] T004 Add the approved concise copy for the **Light** panel to `HowToPlayContent.cs` (extends
  T003).
- [ ] T005 Add the approved concise copy for the **Batteries** panel to `HowToPlayContent.cs`
  (extends T003).
- [ ] T006 Add the approved concise copy for the **Hiding** panel (GDD Ch. 9.1, 15.4) to
  `HowToPlayContent.cs` (extends T003).
- [ ] T007 Add the approved concise copy for the **Keys** panel to `HowToPlayContent.cs` (extends
  T003).
- [ ] T008 Add the approved concise copy for the **Objective** panel to `HowToPlayContent.cs`
  (extends T003; completes the six GDD-required mechanics).

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T009 Define the `HowToPlayNavigationSystem` plain C# static class with method stubs
  `Next(int currentIndex, int panelCount) : int` and
  `Previous(int currentIndex, int panelCount) : int` in
  `Assets/Scripts/Systems/HowToPlay/HowToPlayNavigationSystem.cs`. No `MonoBehaviour`/
  `Component`/scene dependency (constitution Principle III).

**Checkpoint**: `HowToPlayContent` and `HowToPlayNavigationSystem` compile and are ready for
story-specific tests.

---

## Phase 3: User Story 1 - Understand the first action (P2) 🎯 MVP

**Goal**: Players can open a concise How To Play screen explaining movement, light, batteries,
hiding, keys, and the objective without a long popup tutorial.

**Independent Test**: Open from Main Menu and from Pause, navigate all panels, then return without
changing run state.

### Tests for User Story 1

- [ ] T010 [P] [US1] EditMode test: `HowToPlayContent` contains exactly one entry for each of the
  six GDD-required mechanics (movement, light, batteries, hiding, keys, objective) with no
  duplicate `Id`s and no missing `TitleKey`/`BodyKey` (SC-001; covers Edge Case "missing
  localization text" as a detectable missing-key failure, not a silent blank panel), in
  `Assets/Tests/EditMode/Systems/HowToPlay/HowToPlayContentCompletenessTests.cs`.
- [ ] T011 [P] [US1] EditMode test: `Next` clamps at the last panel index (does not wrap) and
  `Previous` clamps at `0`, for a fixture panel count, in
  `Assets/Tests/EditMode/Systems/HowToPlay/HowToPlayNavigationSystemBoundsTests.cs`.
- [ ] T012 [P] [US1] EditMode test: `Next`/`Previous` from a mid-range index move exactly one
  step in the expected direction, same file.
- [ ] T013 [P] [US1] EditMode test: opening with `HowToPlayEntryContext.MainMenu` and closing
  resolves back to `MainMenu`; opening with `Pause` and closing resolves back to `Pause` — no
  cross-contamination between the two entry contexts (FR-003), in
  `Assets/Tests/EditMode/Systems/HowToPlay/HowToPlayEntryContextTests.cs`.

### Implementation for User Story 1

- [ ] T014 [US1] Implement the `Next`/`Previous` bodies in `HowToPlayNavigationSystem.cs`
  (extends T009; makes T011–T012 pass).
- [ ] T015 [US1] Implement `HowToPlayView` UI (panel content display, Next/Previous/Close
  controls, current-panel indicator) reading from `HowToPlayContent` and driven by
  `HowToPlayNavigationSystem`, in `Assets/Scripts/UI/HowToPlayView.cs`.
- [ ] T016 [US1] Implement `HowToPlayController` `MonoBehaviour` that owns the current panel index
  and the active `HowToPlayEntryContext`, wires `HowToPlayView`'s Next/Previous buttons through
  `HowToPlayNavigationSystem`, and wires Close/Back to restore the prior shell context per T013's
  contract, in `Assets/Scripts/UI/HowToPlayController.cs`.
- [ ] T017 [US1] Add a Main Menu entry point that opens `HowToPlayController` with
  `HowToPlayEntryContext.MainMenu` (FR-002), in `Assets/Scripts/UI/MainMenuController.cs`.
- [ ] T018 [US1] Add a Pause menu entry point that opens `HowToPlayController` with
  `HowToPlayEntryContext.Pause` without resuming gameplay — `GameState.IsPaused` (game-shell-ui/
  001) stays `true` throughout, satisfying SC-002 — in
  `Assets/Scripts/UI/PauseMenuController.cs` (extends 001's controller).
- [ ] T019 [US1] Confirm `HowToPlayController`'s open/close path never reads or writes `GameState`/
  `GameConfig` (SC-002) and never appears as a gameplay popup outside these two explicit entry
  points (FR-002), in `Assets/Scripts/UI/HowToPlayController.cs`.

**Checkpoint**: The screen opens from both entry points, covers all six mechanics, and closing
always restores the correct prior context; User Story 1 is independently demonstrable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T020 [P] EditMode test: opening on first launch (no prior state) and after a scene reload
  both produce the same six-panel set and start at panel index `0` (covers Edge Cases "first
  launch" and "reload"), in
  `Assets/Tests/EditMode/Systems/HowToPlay/HowToPlayLaunchStateTests.cs`.
- [ ] T021 Manual device pass: open from Main Menu and from Pause, navigate all six panels
  forward and back, rotate the device mid-screen, and confirm layout remains navigable and
  legible (covers Edge Case "rotation") and that Close always returns to the correct prior
  context (spec Independent Test).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies. T004–T008 extend the same file as T003 and should be
  done in sequence, not in parallel.
- **Foundational (Phase 2)** depends on Setup; blocks the user story.
- **User Story 1 (Phase 3)** depends only on Foundational.
- **Polish (Phase 4)** depends on User Story 1 being complete.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them; T003–T008 are deliberately unmarked since they share one file.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- This feature does not implement Floor 52's own onboarding flow (a separate scene spec, per
  Scope) or the floor-transition splash text (progression-and-scene-flow/001).
- Commit after each task or logical group.
