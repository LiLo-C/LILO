---
description: "Task list for the Main Menu scene"
---

# Tasks: Main Menu Scene

**Input**: Design documents from `/specs/scenes/002-main-menu-scene/` (`spec.md` only — this
feature has no `plan.md`/`data-model.md`/`contracts/`, per ROADMAP.md's note that scene specs
marked "single, no sub-split" own just `spec.md`/`tasks.md`/`checklists/`)

**Prerequisites**: `spec.md`. Also assumes, per the dependency order in `specs/ROADMAP.md` §5,
that `specs/scenes/001-bootstrap-scene`, `specs/systems/game-shell-ui/002-settings-menu-audio-
controls`, `.../003-how-to-play-screen`, and `specs/systems/progression-and-scene-flow/002-scene-
transition-manager` have already been implemented — this scene only wires into their public
surface, it does not implement them. Exact method/class names on the How To Play overlay, the
Settings overlay, and the scene-transition mechanism are owned by those specs; where this task
list needs to call into them, it names the call in terms of *what it must do* (open the overlay,
load Prologue) rather than guessing a signature those specs haven't fixed yet.

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires EditMode coverage for
this scene's one piece of pure, extractable decision logic (the in-flight-transition guard behind
FR-006).

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1, P2, P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project: scene under `Assets/Scenes/`, pure C# decision logic under
`Assets/Scripts/Systems/`, thin UI adapters under `Assets/Scripts/UI/` (per `specs/ROADMAP.md` §0:
UI-facing MonoBehaviours live under `Assets/Scripts/UI/`, not `Assets/Scripts/MonoBehaviours/`),
EditMode tests under `Assets/Tests/EditMode/`.

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/MainMenu.unity` as a new Unity scene (`Canvas` + `EventSystem` +
  a UI-only `Camera`; no playable 3D content, per FR-009)
- [ ] T002 Add `Assets/Scenes/MainMenu.unity` to Build Settings immediately after
  `Assets/Scenes/Bootstrap.unity` (File > Build Settings > Scenes In Build)
- [ ] T003 [P] Confirm/create the `Assets/Scripts/UI/` folder exists (per `specs/ROADMAP.md` §0:
  UI-facing thin adapters live under `Assets/Scripts/UI/`, separate from
  `Assets/Scripts/MonoBehaviours/`)
- [ ] T004 [P] In Player Settings, confirm the project's orientation lock is Landscape (matches
  the constitution's Technology Stack target platform) so `MainMenu.unity` inherits it rather than
  overriding it (FR-010)

**Checkpoint**: `MainMenu.unity` exists, is in the build order after Bootstrap, and has nowhere
yet for a control to live.

---

## Phase 2: User Story 1 — Start a run from the Title screen (Priority: P1) 🎯 MVP

**Goal**: A Start control is visible on `MainMenu.unity`; tapping it transitions to
`Prologue.unity` exactly once per tap, with no double-transition possible.

**Independent Test**: From a freshly-loaded Main Menu, tap Start and confirm the scene transitions
to Prologue with no other input required.

### Tests for User Story 1

- [ ] T005 [P] [US1] Create `Assets/Tests/EditMode/MainMenuTransitionGateTests.cs`: a pure C#
  EditMode test asserting that (a) the gate reports "allow" the first time a transition is
  requested while idle, (b) it reports "block" for a second request made before the first
  transition is marked complete, and (c) it reports "allow" again once completion is signaled —
  write this test first and confirm it fails before T007 exists

### Implementation for User Story 1

- [ ] T006 [US1] Build the Title screen layout in `Assets/Scenes/MainMenu.unity`: a `Canvas` with
  a Start button as the one primary call-to-action (FR-001), plus placeholder How To Play and
  Settings buttons for later phases
- [ ] T007 [US1] Create `Assets/Scripts/Systems/MainMenuTransitionGate.cs`: a plain C# class (no
  `MonoBehaviour`/scene dependency, per constitution Principle III) exposing the in-flight-transition
  decision behind FR-006 as pure logic — e.g. `TryBeginTransition()` returning `true` only if no
  transition is already in flight (and marking one in flight), plus a way to clear that flag on
  completion
- [ ] T008 [US1] Create `Assets/Scripts/UI/MainMenuController.cs`: a `MonoBehaviour` attached to
  the Canvas root; wires the Start button's `onClick` to call
  `MainMenuTransitionGate.TryBeginTransition()`, and only if it returns `true`, disables the
  Start/How To Play/Settings controls (FR-006) and calls into
  `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`'s load mechanism
  to load `Prologue.unity` (FR-002)
- [ ] T009 [US1] Wire the Start `Button` component in `Assets/Scenes/MainMenu.unity` to
  `MainMenuController`'s start handler in the Inspector

**Checkpoint**: Start reaches Prologue in one tap, and rapid repeated taps cannot queue a second
transition. US1 is independently testable end to end.

---

## Phase 3: User Story 2 — Learn the rules before playing (Priority: P2)

**Goal**: A How To Play entry point opens the shared How To Play overlay additively, without
unloading `MainMenu.unity` underneath it.

**Independent Test**: From Main Menu, tap the How To Play entry point and confirm the shared How
To Play overlay opens; dismiss it and confirm Main Menu is unchanged underneath.

### Implementation for User Story 2

- [ ] T010 [US2] In `MainMenuController.cs`, add a how-to-play handler that calls into
  `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`'s open mechanism additively (no
  scene unload) when invoked (FR-003, FR-005)
- [ ] T011 [US2] Wire the How To Play `Button` component in `Assets/Scenes/MainMenu.unity` to
  `MainMenuController`'s how-to-play handler in the Inspector
- [ ] T012 [US2] Manual QA: from Main Menu, tap How To Play, confirm the overlay opens over
  `MainMenu.unity`; dismiss it and confirm Start, How To Play, and Settings are all still
  functional underneath (spec.md US2 Independent Test)

**Checkpoint**: How To Play is reachable and dismissable without disturbing Main Menu underneath.

---

## Phase 4: User Story 3 — Adjust audio before playing (Priority: P3)

**Goal**: A Settings entry point opens the shared Settings/audio overlay additively, without
unloading `MainMenu.unity` underneath it.

**Independent Test**: From Main Menu, tap the Settings entry point and confirm the shared Settings
overlay opens; dismiss it and confirm Main Menu is unchanged underneath.

### Implementation for User Story 3

- [ ] T013 [US3] In `MainMenuController.cs`, add a settings handler that calls into
  `specs/systems/game-shell-ui/002-settings-menu-audio-controls/spec.md`'s open mechanism
  additively (no scene unload) when invoked (FR-004, FR-005)
- [ ] T014 [US3] Wire the Settings `Button` component in `Assets/Scenes/MainMenu.unity` to
  `MainMenuController`'s settings handler in the Inspector
- [ ] T015 [US3] Manual QA: from Main Menu, tap Settings, confirm the overlay opens over
  `MainMenu.unity`; dismiss it and confirm Start, How To Play, and Settings are all still
  functional underneath (spec.md US3 Independent Test)

**Checkpoint**: All three entry points are independently reachable and non-destructive to the
Title screen underneath.

---

## Phase 5: Cross-cutting verification (FR-007, FR-008, FR-009, FR-010)

- [ ] T016 [P] Confirm no control on `Assets/Scenes/MainMenu.unity` gates behind a login, account
  creation, or network call (FR-007) — a static review of the scene's own wiring, no external call
  should exist to remove
- [ ] T017 [P] Manual QA: complete a full run to `GoodEnding.unity`, confirm its return lands on
  `MainMenu.unity` with Start, How To Play, and Settings all functional and no stale/disabled state
  left over from the previous run (FR-008, spec.md SC-004)
- [ ] T018 [P] Manual QA: repeat T017 via `BadEnding.unity`'s return path (FR-008, spec.md SC-004)
- [ ] T019 [P] Confirm `Assets/Scenes/MainMenu.unity` contains no gameplay HUD element (battery
  indicator, spare slot, or lives readout) anywhere in its Canvas hierarchy (FR-009)
- [ ] T020 [P] Manual QA: rapid repeated taps on Start confirm zero duplicate Prologue-load
  transitions (spec.md SC-003)

**Checkpoint**: All functional requirements verified; Main Menu is safe for `007-good-ending-scene`
and `008-bad-ending-scene` to build their return paths against.

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start immediately.
- **User Story 1 (Phase 2)**: Depends on Setup. Delivers the MVP (Start → Prologue).
- **User Story 2 (Phase 3)**: Depends on Setup; independent of US1's transition-gate internals
  (different button, different handler) but shares `MainMenuController.cs`.
- **User Story 3 (Phase 4)**: Depends on Setup; independent of US1/US2 for the same reason.
- **Cross-cutting verification (Phase 5)**: Depends on US1–US3 all existing; also depends on
  `specs/scenes/007-good-ending-scene` and `specs/scenes/008-bad-ending-scene` existing far enough
  to have a return path to exercise (T017/T018) — these two checks may be deferred until those
  scenes are built, without blocking sign-off on US1–US3 individually.

## Notes

- `MainMenuTransitionGate.cs` is deliberately the only place the in-flight decision *logic* lives,
  so it can be unit-tested in EditMode without a running scene, per constitution Principle IV.
  `MainMenuController.cs` stays a thin adapter that only calls Unity UI APIs and the two shared
  overlay systems — it does not re-implement the guard itself.
- Do not add a "returning player" fast path (e.g., a flag that lets Start skip Prologue) under any
  task above — `spec.md`'s Assumptions explicitly rule this out; the fast path for repeat players
  lives in `specs/scenes/003-prologue-scene/`'s own Skip control instead.
- Do not add a Quit/Continue control under any task above — out of scope per `spec.md` Assumptions.
