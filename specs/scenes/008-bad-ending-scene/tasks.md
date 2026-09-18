---
description: "Task list for the Bad Ending scene"
---

# Tasks: Bad Ending Scene

**Input**: Design documents from `/specs/scenes/008-bad-ending-scene/` (`spec.md` only — this
feature has no `plan.md`/`data-model.md`/`contracts/`, per ROADMAP.md's note that scene specs
marked "single, no sub-split" own just `spec.md`/`tasks.md`/`checklists/`)

**Prerequisites**: `spec.md`. Also assumes, per `specs/ROADMAP.md` §3/§5, that
`specs/systems/narrative-content/001-eddie-character-bible`,
`specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint`,
`specs/systems/lives-and-fail-state/002-floor-state-reset-on-death`,
`specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch`, and
`specs/systems/progression-and-scene-flow/002-scene-transition-manager` already exist — this scene
only consumes their content/mechanism, it does not implement them. Also assumes
`lives-and-fail-state/003`'s zero-lives branch (its FR-008) already routes the authoritative
zero-lives signal here, and that `Prologue.unity` and `MainMenu.unity` exist as load targets for
the new-run boundary (their own content is out of scope for this task list beyond being valid
scene names to load).

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires EditMode coverage for
this scene's one piece of pure, extractable decision logic (the entry-validity gate and its
never-calls-reset-or-decrement guarantee behind FR-001/FR-002/SC-001/SC-002).

**Organization**: Tasks are grouped by user story from `spec.md` (this spec has a single story,
US1, at P1).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project: scene under `Assets/Scenes/`, pure C# decision logic under
`Assets/Scripts/Systems/`, thin Unity-lifecycle adapters under `Assets/Scripts/MonoBehaviours/`
(per `specs/ROADMAP.md` §0), EditMode tests under `Assets/Tests/EditMode/`.

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/BadEnding.unity` as a new Unity scene (`Canvas` + `EventSystem` +
  a UI-only `Camera`; no playable 3D content)
- [ ] T002 Add `Assets/Scenes/BadEnding.unity` to Build Settings immediately after
  `Assets/Scenes/GoodEnding.unity`
- [ ] T003 [P] Confirm/create the `Assets/Scripts/Systems/` and `Assets/Scripts/MonoBehaviours/`
  folders exist (per `specs/ROADMAP.md` §0 naming conventions)

**Checkpoint**: `BadEnding.unity` exists, is in the build order after the Good Ending, and has no
panel content yet.

---

## Phase 2: User Story 1 — Resolve a failed run (Priority: P1) 🎯 MVP

**Goal**: Entry is gated on the authoritative zero-lives outcome; the approved Bad Ending sequence
presents without ever calling floor reset or decrementing lives again; Replay/Menu each explicitly
start a new run.

**Independent Test**: Enter only after the final life is lost and compare state, narrative, and
return action (spec.md US1 Independent Test).

### Tests for User Story 1

- [ ] T004 [P] [US1] Create `Assets/Tests/EditMode/BadEndingEntryGateTests.cs`: a pure C# EditMode
  test asserting that (a) an authoritative zero-lives signal from
  `lives-and-fail-state/001`'s "has lives remaining" query admits entry (FR-001), (b) a positive
  (`true`) "has lives remaining" signal is rejected rather than admitted (SC-002), and (c) in
  neither case does the entry gate call `lives-and-fail-state/001`'s decrement operation or
  `lives-and-fail-state/002`'s `ResetCurrentFloor` operation — zero calls to either, every time
  (FR-002, SC-001) — write this test first and confirm it fails before T008 exists

### Content authoring for User Story 1

- [ ] T005 [P] [US1] Author Panel 1 in `Assets/Scenes/BadEnding.unity`: Eddie's death acknowledged
  as final and permanent (GDD Ch. 10.4 Bad-ending beat 1, "Eddie mati")
- [ ] T006 [P] [US1] Author Panel 2: Eddie transforming into one of the entities that haunt the
  office (GDD Ch. 10.4 beat 2, "menjadi salah satu entity")
- [ ] T007 [P] [US1] Author Panel 3: final tableau of Eddie trapped in the office forever (GDD Ch.
  10.4 beat 3, "terjebak di kantor itu selamanya")

### Implementation for User Story 1

- [ ] T008 [US1] Create `Assets/Scripts/Systems/BadEndingEntryGate.cs`: a plain C# class (no
  `MonoBehaviour`/scene dependency, per constitution Principle III) that queries
  `lives-and-fail-state/001`'s "has lives remaining" operation on entry and exposes `IsEntryValid`
  as `true` only when that query returns `false`; this class calls neither
  `lives-and-fail-state/001`'s decrement operation nor `lives-and-fail-state/002`'s
  `ResetCurrentFloor` operation on any path (FR-001, FR-002)
- [ ] T009 [US1] Create `Assets/Scripts/MonoBehaviours/BadEndingController.cs`: a thin
  `MonoBehaviour` attached to the Canvas root; on `Awake`/`Start` it calls
  `BadEndingEntryGate.IsEntryValid`; if invalid, it redirects immediately via
  `progression-and-scene-flow/002-scene-transition-manager` without showing any panel (SC-002); if
  valid, it shows panels 1–3 (T005–T007) one at a time in fixed order
- [ ] T010 [US1] Wire a tap-anywhere (or timed auto-advance, per final art pass) panel-advance
  input in `Assets/Scenes/BadEnding.unity` to `BadEndingController`'s advance handler so panels 1–3
  progress in order
- [ ] T011 [US1] Add Replay and Menu `Button`s to `Assets/Scenes/BadEnding.unity`'s Canvas,
  appearing only after panel 3 is reached
- [ ] T012 [US1] Wire the Replay button's `onClick` in `BadEndingController.cs` to explicitly start
  a new run (per `progression-and-scene-flow/003`'s new-run boundary, clearing any prior run state
  while preserving settings) and then calls the scene-transition-manager to load
  `Assets/Scenes/Prologue.unity` (FR-003)
- [ ] T013 [US1] Wire the Menu button's `onClick` in `BadEndingController.cs` to the same new-run
  boundary call as T012, then calls the scene-transition-manager to load
  `Assets/Scenes/MainMenu.unity` (FR-003)
- [ ] T014 [US1] Manual QA: enter `BadEnding.unity` only via a zero-lives fixture signal; confirm
  panels 1–3 display in order, zero floor-reset and zero decrement calls occurred, and both Replay
  and Menu each explicitly start a new run before loading their target scene (spec.md US1
  Independent Test, SC-001)
- [ ] T015 [US1] Manual QA: attempt entry with a positive-lives fixture signal; confirm the scene
  redirects away immediately without showing any panel content (spec.md SC-002)

**Checkpoint**: US1 is independently testable end to end — only an authoritative zero-lives signal
ever shows content, no reset/decrement is ever called from this scene, and both exits start a
clean new run.

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start immediately.
- **User Story 1 (Phase 2)**: Depends on Setup. This is the only user story in this spec, so it is
  also the MVP. Content tasks T005–T007 are parallelizable with each other and with T004;
  T008–T013 depend on the panel GameObjects existing; T014–T015 depend on all of T008–T013.

## Notes

- `BadEndingEntryGate.cs` is deliberately the only place decision *logic* lives (is this entry
  valid, and the standing guarantee that this scene never calls reset/decrement), so it can be
  unit-tested in EditMode without a running scene, per constitution Principle IV.
  `BadEndingController.cs` stays a thin adapter that only shows/hides panel GameObjects and calls
  the entry gate / scene-transition-manager — it does not re-implement the validity decision or
  gain any path back into `lives-and-fail-state/001`/`002`.
- Do not add a floor-reset call, a lives-decrement call, or any path that lets this scene retry the
  current floor under any task above — all explicitly out of scope per `spec.md`'s FR-002 and
  Scope; that behavior belongs exclusively to
  `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/` on the "lives remain"
  branch, which never reaches this scene.
- Final illustrated art for panels 1–3 is not this task list's job (comic-art production owns it,
  GDD Ch. 19.1); T005–T007 use placeholder static images so the entry-gating/presentation can be
  built and tested well before final art exists.
