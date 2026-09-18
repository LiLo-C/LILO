---
description: "Task list for HUD Composition and Layout"
---

# Tasks: HUD Composition and Layout

**Input**: Design documents from
`specs/systems/game-shell-ui/004-hud-composition-and-layout/spec.md`

**Prerequisites**: `spec.md` (this feature). Composes, but does not redefine, the pre-owned
components from `specs/systems/flashlight-and-battery/009-battery-hud-indicator/spec.md`
(`BatteryHudView`), `specs/systems/interaction-and-highlight/002-context-sensitive-action-button/
spec.md` (`ContextActionButton`), and `specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/
spec.md` (pause access). Reads `GameState` from
`specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`.

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test.

**Organization**: `spec.md` defines a single user story (US1). Tasks are still split into
Setup/Foundational/Story/Polish phases so each unit stays small and independently completable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user story (US1)

## Phase 1: Setup

- [ ] T001 [P] Define the `HudVisibilityMode` enum (`Normal`, `SuppressedByPause`,
  `SuppressedByEnding`) in `Assets/Scripts/Systems/Hud/HudVisibilityMode.cs` — the one shared
  signal every HUD element queries instead of deriving its own pause/death check (FR-004: one
  owner).
- [ ] T002 [P] Define the `HudSafeAreaInsets` value type (`float Top`, `Bottom`, `Left`, `Right`)
  in `Assets/Scripts/Systems/Hud/HudSafeAreaInsets.cs`.
- [ ] T003 [P] Define the `HudFloorObjectiveViewModel` value type (`int FloorNumber`,
  `string ObjectiveText`) in `Assets/Scripts/Systems/Hud/HudFloorObjectiveViewModel.cs` — the
  floor/objective indicator this composition owns directly (no other spec claims it; battery and
  the action button are owned elsewhere per FR-004).

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T004 Define the `HudVisibilitySystem` plain C# static class with a method stub
  `ComputeMode(bool isPaused, bool isRunEnded) : HudVisibilityMode` in
  `Assets/Scripts/Systems/Hud/HudVisibilitySystem.cs`. No `MonoBehaviour`/`Component`/scene
  dependency (constitution Principle III).
- [ ] T005 Define the `HudSafeAreaSystem` plain C# static class with a method stub
  `Compute(Rect screenSafeArea, Vector2 screenSize) : HudSafeAreaInsets` in
  `Assets/Scripts/Systems/Hud/HudSafeAreaSystem.cs`, taking raw `Screen.safeArea`/
  `Screen.width`/`Screen.height` values as parameters rather than reading them itself (keeps it
  callable from an EditMode test with fixture rects; covers Edge Case "notch").

**Checkpoint**: `HudVisibilitySystem` and `HudSafeAreaSystem` compile and are ready for
story-specific tests.

---

## Phase 3: User Story 1 - See only vital information (P1) 🎯 MVP

**Goal**: The HUD shows charge/spare, current objective/floor, interaction prompt, and pause
access without obscuring the horror space.

**Independent Test**: Render every gameplay state and supported aspect ratio, including hidden,
chase, and low battery.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test: `ComputeMode` returns `SuppressedByPause` when `isPaused` is
  `true`, regardless of `isRunEnded`, in
  `Assets/Tests/EditMode/Systems/Hud/HudVisibilitySystemTests.cs`.
- [ ] T007 [P] [US1] EditMode test: `ComputeMode` returns `SuppressedByEnding` when `isRunEnded`
  is `true` and `isPaused` is `false`, same file.
- [ ] T008 [P] [US1] EditMode test: `ComputeMode` returns `Normal` when neither flag is set, same
  file.
- [ ] T009 [P] [US1] EditMode test: `HudSafeAreaSystem.Compute` returns zero insets for a fixture
  safe-area rect equal to the full screen (no notch), and non-zero insets matching the fixture's
  offsets for a notched fixture rect, in
  `Assets/Tests/EditMode/Systems/Hud/HudSafeAreaSystemTests.cs`.
- [ ] T010 [P] [US1] EditMode test: `HudSafeAreaSystem.Compute` never returns a negative inset
  for any fixture rect within the screen bounds, including a portrait-vs-landscape fixture pair
  representing the same notch on different sides (covers Edge Case "rotation"), same file.
- [ ] T011 [P] [US1] EditMode test: `HudFloorObjectiveViewModel`'s display text truncates rather
  than throwing for an objective string longer than the configured display budget (covers Edge
  Case "long text"), in
  `Assets/Tests/EditMode/Systems/Hud/HudFloorObjectiveViewModelTests.cs`.
- [ ] T012 [P] [US1] EditMode test: `HudFloorObjectiveViewModel` falls back to a documented
  placeholder (not a blank/null display) when floor or objective state is missing (covers Edge
  Case "missing state"), same file.

### Implementation for User Story 1

- [ ] T013 [US1] Implement the `ComputeMode` body in `HudVisibilitySystem.cs` (extends T004;
  makes T006–T008 pass).
- [ ] T014 [US1] Implement the `Compute` body in `HudSafeAreaSystem.cs` (extends T005; makes
  T009–T010 pass).
- [ ] T015 [US1] Implement the long-text-truncation and missing-state-placeholder handling in
  `HudFloorObjectiveViewModel.cs` (makes T011–T012 pass).
- [ ] T016 [US1] Implement `HudRootLayout` `MonoBehaviour` that reads `Screen.safeArea`/
  `Screen.width`/`Screen.height` each layout pass, calls `HudSafeAreaSystem.Compute`, and applies
  the resulting insets to the HUD `Canvas`'s root `RectTransform` anchors (FR-001), in
  `Assets/Scripts/UI/HudRootLayout.cs`.
- [ ] T017 [US1] Implement `HudVisibilityController` `MonoBehaviour` that reads
  `GameState.IsPaused` (game-shell-ui/001) and the run outcome each frame, calls
  `HudVisibilitySystem.ComputeMode`, and shows/hides the HUD `Canvas` group accordingly (FR-003),
  in `Assets/Scripts/UI/HudVisibilityController.cs`.
- [ ] T018 [US1] Implement `HudFloorObjectiveView` UI (floor number + objective text label,
  reading `HudFloorObjectiveViewModel`), positioned inside `HudRootLayout`'s safe-area-adjusted
  root, in `Assets/Scripts/UI/HudFloorObjectiveView.cs`.
- [ ] T019 [US1] Compose the existing `BatteryHudView` (flashlight-and-battery/009),
  `ContextActionButton` (interaction-and-highlight/002), the new `HudFloorObjectiveView` (T018),
  and a pause-access button (game-shell-ui/001) as siblings under `HudRootLayout`'s
  safe-area-adjusted root, each in its own fixed screen region with no overlapping touch targets
  (FR-001, FR-004 — this task only positions each pre-owned component; it does not re-implement
  their internal logic), in `Assets/Scripts/UI/HudComposition.cs`.
- [ ] T020 [US1] Wire the pause-access button to dispatch through `PauseSystem.
  EvaluatePauseRequest` (game-shell-ui/001) rather than a second pause entry point, in
  `Assets/Scripts/UI/HudComposition.cs` (extends T019).
- [ ] T021 [US1] Add non-color legibility cues (icon shape/fill plus text, not hue alone) to
  `HudFloorObjectiveView` (FR-002), and confirm `BatteryHudView`/`ContextActionButton` already
  satisfy FR-002 by their own specs' contracts — file a gap against that component's own spec
  rather than re-implementing it here if not, in `Assets/Scripts/UI/HudFloorObjectiveView.cs`.

**Checkpoint**: All vital indicators compose into one non-overlapping, safe-area-aware layout
that follows pause/hiding/death/ending visibility; User Story 1 is independently demonstrable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T022 [P] EditMode test: for a fixture set of simultaneous states (e.g. hiding + low battery
  + a valid interaction target), the configured anchor rects for every composed HUD element never
  require two elements to occupy the same screen region (covers Edge Case "simultaneous prompts
  ... degrade without overlap"), in
  `Assets/Tests/EditMode/Systems/Hud/HudLayoutOverlapTests.cs`.
- [ ] T023 Manual device pass: render every gameplay state (normal, hiding, chase, low battery,
  paused, dead, ending) across every supported aspect ratio/notch configuration; confirm no
  overlap or cutoff (SC-001) and that testers can identify charge, objective, and available
  action without opening a menu in a blind read-back (SC-002).

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
- This feature does not re-implement the battery indicator's or action button's own display
  rules — it positions and shows/hides them, per the spec's Scope.
- Commit after each task or logical group.
