---
description: "Task list for the Good Ending scene"
---

# Tasks: Good Ending Scene

**Input**: Design documents from `/specs/scenes/007-good-ending-scene/` (`spec.md` only — this
feature has no `plan.md`/`data-model.md`/`contracts/`, per ROADMAP.md's note that scene specs
marked "single, no sub-split" own just `spec.md`/`tasks.md`/`checklists/`)

**Prerequisites**: `spec.md`. Also assumes, per `specs/ROADMAP.md` §3/§5, that
`specs/systems/narrative-content/001-eddie-character-bible`,
`specs/systems/progression-and-scene-flow/002-scene-transition-manager`, and
`specs/systems/progression-and-scene-flow/003-full-run-completion-tracking` already exist — this
scene only consumes their content/mechanism, it does not implement them. Also assumes
`specs/scenes/006-floor-50-scene`'s final-door trigger already routes a valid completion signal
here, and that `Prologue.unity` and `MainMenu.unity` exist as load targets for the new-run
boundary (their own content is out of scope for this task list beyond being valid scene names to
load).

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires EditMode coverage for
this scene's one piece of pure, extractable decision logic (the entry-validity/idempotent-record
gate behind FR-001/FR-002/SC-001/SC-002).

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

- [ ] T001 Create `Assets/Scenes/GoodEnding.unity` as a new Unity scene (`Canvas` + `EventSystem` +
  a UI-only `Camera`; no playable 3D content)
- [ ] T002 Add `Assets/Scenes/GoodEnding.unity` to Build Settings immediately after
  `Assets/Scenes/Floor50.unity`
- [ ] T003 [P] Confirm/create the `Assets/Scripts/Systems/` and `Assets/Scripts/MonoBehaviours/`
  folders exist (per `specs/ROADMAP.md` §0 naming conventions)

**Checkpoint**: `GoodEnding.unity` exists, is in the build order after Floor 50, and has no panel
content yet.

---

## Phase 2: User Story 1 — Resolve a successful run (Priority: P1) 🎯 MVP

**Goal**: Entry is gated on a valid completion signal; the approved Good Ending sequence presents
in fixed order with no gameplay reset before acknowledgment; Replay/Menu each explicitly start the
documented new-run boundary.

**Independent Test**: Enter only from a valid final-door completion signal fixture and inspect
content, completion record, and exit actions (spec.md US1 Independent Test).

### Tests for User Story 1

- [ ] T004 [P] [US1] Create `Assets/Tests/EditMode/GoodEndingEntryGateTests.cs`: a pure C# EditMode
  test asserting that (a) a valid completion signal from
  `progression-and-scene-flow/003-full-run-completion-tracking` admits entry and records exactly
  one completed run, (b) a missing/invalid completion signal is rejected rather than admitted
  (FR-001, SC-001), and (c) firing the same valid signal twice records the completion only once,
  never twice (FR-002, SC-002) — write this test first and confirm it fails before T010 exists

### Content authoring for User Story 1

- [ ] T005 [P] [US1] Author Panel 1 in `Assets/Scenes/GoodEnding.unity`: Eddie truly waking up
  (for real, not another in-dream waking) in a cafe (GDD Ch. 10.4 Good-ending beat 1)
- [ ] T006 [P] [US1] Author Panel 2: Eddie's realization that his office life has been toxic and
  everything he dreamed was a manifestation of his work trauma (GDD Ch. 10.4 beat 2; cite Eddie
  bible FR-006's dream/reality through-line — keep it felt through this realization beat, not
  named outright, per FR-007)
- [ ] T007 [P] [US1] Author Panel 3: Eddie looking at the resignation letter he already submitted
  to his old office, smiling (GDD Ch. 10.4 beat 3)
- [ ] T008 [P] [US1] Author Panel 4: Eddie seeing an interview schedule waiting for him (GDD Ch.
  10.4 beat 4)
- [ ] T009 [P] [US1] Author Panel 5: final happy-end tableau, freezing on Eddie's smile (GDD Ch.
  10.4 beat 5, "Happy end")

### Implementation for User Story 1

- [ ] T010 [US1] Create `Assets/Scripts/Systems/GoodEndingEntryGate.cs`: a plain C# class (no
  `MonoBehaviour`/scene dependency, per constitution Principle III) that queries
  `progression-and-scene-flow/003`'s completion-validity signal on entry, exposes `IsEntryValid`,
  and — only when valid — calls that spec's record-completion operation exactly once, idempotently
  (repeated calls for the same run are a no-op); this class issues no gameplay-reset call on any
  path (FR-001, FR-002)
- [ ] T011 [US1] Create `Assets/Scripts/MonoBehaviours/GoodEndingController.cs`: a thin
  `MonoBehaviour` attached to the Canvas root; on `Awake`/`Start` it calls
  `GoodEndingEntryGate.IsEntryValid`; if invalid, it redirects immediately via
  `progression-and-scene-flow/002-scene-transition-manager` without showing any panel (SC-001); if
  valid, it shows panels 1–5 (T005–T009) one at a time in fixed order
- [ ] T012 [US1] Wire a tap-anywhere (or timed auto-advance, per final art pass) panel-advance
  input in `Assets/Scenes/GoodEnding.unity` to `GoodEndingController`'s advance handler so panels
  1–5 progress in order
- [ ] T013 [US1] Add Replay and Menu `Button`s to `Assets/Scenes/GoodEnding.unity`'s Canvas,
  appearing only after panel 5 is reached
- [ ] T014 [US1] Wire the Replay button's `onClick` in `GoodEndingController.cs` to
  `progression-and-scene-flow/003`'s new-run boundary (clears the prior run's completion record
  while preserving settings) and then calls the scene-transition-manager to load
  `Assets/Scenes/Prologue.unity` (FR-003)
- [ ] T015 [US1] Wire the Menu button's `onClick` in `GoodEndingController.cs` to the same new-run
  boundary call as T014, then calls the scene-transition-manager to load
  `Assets/Scenes/MainMenu.unity` (FR-003)
- [ ] T016 [US1] Manual QA: enter `GoodEnding.unity` only via a valid final-door completion signal
  fixture; confirm panels 1–5 display in order, the completion record shows exactly one completed
  run, and both Replay and Menu each start the documented new-run boundary before loading their
  target scene (spec.md US1 Independent Test, SC-002)

**Checkpoint**: US1 is independently testable end to end — invalid entry never shows content,
valid entry shows the full approved sequence exactly once, and both exits start a clean new run.

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start immediately.
- **User Story 1 (Phase 2)**: Depends on Setup. This is the only user story in this spec, so it is
  also the MVP. Content tasks T005–T009 are parallelizable with each other and with T004;
  T010–T015 depend on the panel GameObjects existing; T016 depends on all of T010–T015.

## Notes

- `GoodEndingEntryGate.cs` is deliberately the only place decision *logic* lives (is this entry
  valid, has completion already been recorded), so it can be unit-tested in EditMode without a
  running scene, per constitution Principle IV. `GoodEndingController.cs` stays a thin adapter that
  only shows/hides panel GameObjects and calls the entry gate / scene-transition-manager — it does
  not re-implement the validity or idempotency decisions itself.
- Do not add a gameplay reset call, a skip control, or any timed auto-advance that could race the
  entry-gate check ahead of T010–T011 under any task above — all out of scope per `spec.md`'s FR-002
  and Scope.
- Final illustrated art for panels 1–5 is not this task list's job (comic-art production owns it,
  GDD Ch. 19.1); T005–T009 use placeholder static images so the entry-gating/presentation can be
  built and tested well before final art exists.
- This scene never calls `lives-and-fail-state`'s decrement or reset operations — those belong
  exclusively to the death/outcome-branch flow that leads to the *Bad* Ending
  (`specs/scenes/008-bad-ending-scene/`), not this one.
