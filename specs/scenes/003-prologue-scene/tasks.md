---
description: "Task list for the Prologue scene"
---

# Tasks: Prologue Scene

**Input**: Design documents from `/specs/scenes/003-prologue-scene/` (`spec.md` only — this
feature has no `plan.md`/`data-model.md`/`contracts/`, per ROADMAP.md's note that scene specs
marked "single, no sub-split" own just `spec.md`/`tasks.md`/`checklists/`)

**Prerequisites**: `spec.md`. Also assumes, per the dependency order in `specs/ROADMAP.md` §5,
that `specs/systems/narrative-content/001-eddie-character-bible` and
`specs/systems/progression-and-scene-flow/002-scene-transition-manager` already exist — this scene
only wires into their content/mechanism, it does not implement them. Also assumes
`specs/scenes/002-main-menu-scene` already routes Start here, and that `Floor52.unity` exists as a
load target (its own content, owned by `specs/scenes/004-floor-52-scene/`, is out of scope for this
task list beyond being a valid scene name to load).

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires EditMode coverage for
this scene's one piece of pure, extractable decision logic (the panel-advance/rapid-tap guard
behind FR-004/FR-009).

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1, P2, P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project: scene under `Assets/Scenes/`, pure C# decision logic under
`Assets/Scripts/Systems/`, thin UI adapters under `Assets/Scripts/UI/`, EditMode tests under
`Assets/Tests/EditMode/`.

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/Prologue.unity` as a new Unity scene (`Canvas` + `EventSystem` +
  a UI-only `Camera`; no playable 3D content, per FR-010)
- [ ] T002 Add `Assets/Scenes/Prologue.unity` to Build Settings immediately after
  `Assets/Scenes/MainMenu.unity`
- [ ] T003 [P] Confirm/create the `Assets/Scripts/UI/` and `Assets/Scripts/Systems/` folders exist
  (per `specs/ROADMAP.md` §0 naming conventions)
- [ ] T004 [P] In Player Settings, confirm the project's orientation lock is Landscape so
  `Prologue.unity` inherits it (FR-011)

**Checkpoint**: `Prologue.unity` exists, is in the build order after Main Menu, and has no panel
content yet.

---

## Phase 2: User Story 1 — First-time player experiences Eddie's introduction and the horror hook (Priority: P1) 🎯 MVP

**Goal**: All 7 panels display in fixed order, one at a time, and advancing past panel 7
transitions to `Floor52.unity`.

**Independent Test**: From Main Menu, tap Start, advance through all 7 panels, confirm order and
content, confirm the transition to Floor52 fires after panel 7.

### Tests for User Story 1

- [ ] T005 [P] [US1] Create `Assets/Tests/EditMode/ProloguePanelSequenceTests.cs`: a pure C#
  EditMode test asserting that, starting at panel index 0, calling the advance decision 6 times
  lands on panel index 6 (the 7th and final panel), that the sequence correctly reports "on final
  panel" only at index 6, and that advancing once more from index 6 reports "sequence complete /
  ready to transition" rather than a nonexistent index 7 — write this test first and confirm it
  fails before T007 exists

### Content authoring for User Story 1

- [ ] T006 [P] [US1] Author Panel 1 in `Assets/Scenes/Prologue.unity`: static image placeholder +
  caption introducing Eddie as tired and chronically over-prepared (GDD 10.2 beat 1; consistent
  with Eddie bible FR-001, FR-003)
- [ ] T007 [P] [US1] Author Panel 2: static image placeholder + caption showing Eddie departing
  for and beginning his workday (GDD 10.2 beat 2)
- [ ] T008 [P] [US1] Author Panel 3: static image placeholder + caption showing Eddie growing tired
  and napping at his desk (GDD 10.2 beat 3)
- [ ] T009 [P] [US1] Author Panel 4: static image placeholder + caption showing Eddie waking to a
  totally dark building and panicking (GDD 10.2 beat 4)
- [ ] T010 [P] [US1] Author Panel 5: static image placeholder + caption showing Eddie remembering
  the emergency lamp in his desk locker (GDD 10.2 beat 5; cite Eddie bible FR-003's three-item
  locker inventory)
- [ ] T011 [P] [US1] Author Panel 6: static image placeholder + caption showing the lamp turning on
  only after several failed tries (GDD 10.2 beat 6; cite Eddie bible FR-002's clumsiness trait)
- [ ] T012 [P] [US1] Author Panel 7: static image placeholder + caption showing Eddie hearing
  strange laughter from the hallway (GDD 10.2 beat 7; see spec.md Assumptions on reusing the
  foreshadowing catalog's mocking-laughter entry)

### Implementation for User Story 1

- [ ] T013 [US1] Create `Assets/Scripts/Systems/ProloguePanelSequence.cs`: a plain C# class (no
  `MonoBehaviour`/scene dependency, per constitution Principle III) exposing the ordered 7-panel
  advance decision from FR-001/FR-003 — current panel index, `Advance()`, `IsOnFinalPanel`, and a
  "sequence complete" result once advanced past the final panel, so `PrologueController` (T014) has
  nothing to decide, only to execute
- [ ] T014 [US1] Create `Assets/Scripts/UI/PrologueController.cs`: a `MonoBehaviour` attached to
  the Canvas root; holds references to the 7 panel GameObjects (T006–T012), shows exactly one at a
  time per `ProloguePanelSequence`'s current index, handles a tap-anywhere input to call
  `ProloguePanelSequence.Advance()` (FR-004), and — when the sequence reports "complete" — calls
  into `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`'s load
  mechanism to load `Floor52.unity` (FR-006, FR-008)
- [ ] T015 [US1] Wire the tap-anywhere input region in `Assets/Scenes/Prologue.unity` (a full-screen
  invisible `Button`/raycast target behind the panel content) to `PrologueController`'s advance
  handler in the Inspector

**Checkpoint**: A first-time playthrough shows all 7 panels in order and reaches `Floor52.unity`
after the last one. US1 is independently testable end to end.

---

## Phase 3: User Story 2 — Repeat player skips a Prologue they've already seen (Priority: P2)

**Goal**: A persistent Skip control, visible from panel 1 onward, ends the sequence immediately
from any panel and transitions to `Floor52.unity`.

**Independent Test**: From Main Menu, tap Start, then tap Skip on the very first panel; confirm the
scene transitions directly to Floor52 without showing any remaining panel.

### Tests for User Story 2

- [ ] T016 [P] [US2] Extend `Assets/Tests/EditMode/ProloguePanelSequenceTests.cs` with a case
  asserting that a "skip requested" input, issued at any panel index (0 through 6), produces the
  same "sequence complete / ready to transition" result as advancing past panel 7 does — covers
  spec.md FR-007 (Skip and panel-7-completion must be indistinguishable to the transition step)

### Implementation for User Story 2

- [ ] T017 [US2] Add a Skip `Button` to `Assets/Scenes/Prologue.unity`'s Canvas, positioned so it
  remains visible and on top of every panel (FR-005)
- [ ] T018 [US2] In `PrologueController.cs`, wire the Skip button's `onClick` to a handler that
  tells `ProloguePanelSequence` to report "sequence complete" immediately, regardless of the
  current panel index, then follows the same Floor52-load path used by panel-7 completion (FR-005,
  FR-007)
- [ ] T019 [US2] Manual QA: from any of the 7 panels, tap Skip and confirm the scene transitions
  directly to `Floor52.unity` (spec.md US2 Independent Test)

**Checkpoint**: Skip reaches Floor52 in one tap from any panel, identically to reaching panel 7
naturally.

---

## Phase 4: User Story 3 — Player advances at their own pace without breaking the sequence (Priority: P3)

**Goal**: Rapid repeated taps never advance more than one panel per tap and never queue more than
one Floor52 transition.

**Independent Test**: Rapidly double-tap on a non-final panel; confirm exactly one advance. Rapidly
double-tap on panel 7 (or Skip); confirm exactly one Floor52 transition.

### Tests for User Story 3

- [ ] T020 [P] [US3] Extend `Assets/Tests/EditMode/ProloguePanelSequenceTests.cs` with a case
  asserting that calling `Advance()` twice in immediate succession while already at the "sequence
  complete" result does not report a second, distinct completion — covers spec.md FR-009's
  no-duplicate-transition guarantee at the boundary

### Implementation for User Story 3

- [ ] T021 [US3] In `PrologueController.cs`, gate both the tap-to-advance handler and the Skip
  handler behind a single "transition already triggered" flag — once either path calls the
  scene-transition-manager, no further Advance/Skip input may call it again (FR-009)
- [ ] T022 [US3] Manual QA: rapid double-tap on a non-final panel confirms exactly one advance;
  rapid double-tap on panel 7 and on Skip each confirm exactly one Floor52 transition (spec.md US3
  Independent Test, SC-003)

**Checkpoint**: All three user stories are independently verified; the Prologue is safe to treat as
the fixed, repeatable entry point every run passes through.

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start immediately.
- **User Story 1 (Phase 2)**: Depends on Setup. Delivers the MVP (7 panels → Floor52). Content
  tasks T006–T012 are parallelizable with each other and with T005; T013–T015 depend on the panel
  GameObjects existing.
- **User Story 2 (Phase 3)**: Depends on US1's `PrologueController`/`ProloguePanelSequence`
  existing; adds the Skip path on top of it.
- **User Story 3 (Phase 4)**: Depends on US1's advance path and US2's Skip path both existing;
  adds the shared no-duplicate-transition guard across both.

## Notes

- `ProloguePanelSequence.cs` is deliberately the only place decision *logic* lives (which panel is
  current, when the sequence is "complete"), so it can be unit-tested in EditMode without a running
  scene, per constitution Principle IV. `PrologueController.cs` stays a thin adapter that only
  shows/hides panel GameObjects and calls the scene-transition-manager — it does not re-implement
  the sequencing decisions itself.
- Do not add a timed auto-advance, a session-scoped "seen before" flag gating Skip's visibility, or
  any gameplay HUD element to this scene under any task above — all explicitly out of scope per
  `spec.md` Assumptions/FR-010.
- Final illustrated art for panels 1–7 is not this task list's job (comic-art production owns it,
  GDD Ch. 19.1); T006–T012 use placeholder static images so the sequencing/interaction can be built
  and tested well before final art exists.
