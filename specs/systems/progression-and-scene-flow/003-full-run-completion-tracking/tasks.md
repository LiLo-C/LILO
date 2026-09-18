---

description: "Task list for 003-full-run-completion-tracking"

---

# Tasks: Full Run Completion Tracking

**Input**: Design documents from
`/specs/systems/progression-and-scene-flow/003-full-run-completion-tracking/spec.md`

**Prerequisites**: spec.md (this feature's; required)

**Tests**: Explicitly required — constitution Principle IV (Test-Before-Done) mandates EditMode
tests for every pure-logic piece, and this entire feature is pure C# milestone/outcome-record
logic with no rendering, physics, or input-device dependency, so nothing here qualifies for the
manual-quickstart exception.

**Organization**: This spec defines a single user story (US1). Tasks are still split into the
smallest independently-completable units within that story — tests are written first and MUST
fail before the matching implementation task closes them out.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: Which user story this task belongs to (US1), or unlabeled for
  Setup/Foundational/Polish
- File paths are exact and relative to the repository root

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Progression/`
- Thin `MonoBehaviour` adapter: `Assets/Scripts/MonoBehaviours/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Progression/`
- Reused signal (owned by keys-and-doors/003-final-door-distinct-behavior, not this feature —
  this feature only records the resulting completion, it does not decide final-door eligibility):
  the final-door success signal
- Reused signal (owned by lives-and-fail-state/003-death-sequence-and-outcome-branch FR-008, not
  this feature): the Bad Ending trigger
- Reused type (owned by shared-config-and-state/002, not this feature): `GameState`
  at `Assets/Scripts/State/GameState.cs`, including its new-run reset operation which this
  feature's clear attaches to

---

## Phase 1: Setup

- [ ] T001 Confirm/create the `Assets/Scripts/Systems/Progression/`,
  `Assets/Scripts/MonoBehaviours/`, and `Assets/Tests/EditMode/Systems/Progression/` directories.
  These may already exist from a sibling feature in this same folder group; reuse them, do not
  create duplicates.

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: T002–T006 block every US1 task in Phase 3.

- [ ] T002 [P] Confirm the final-door success signal this feature consumes is owned by
  `specs/systems/keys-and-doors/003-final-door-distinct-behavior/spec.md` (its FR-003: completion
  is committed before transition) — this feature defines the run-level completion *record*; it
  does not re-decide final-door eligibility.
- [ ] T003 [P] Confirm the Bad Ending trigger this feature consumes is owned by
  `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch/spec.md` (its
  FR-008) — this feature only records the resulting failure, it does not decide when lives are
  exhausted.
- [ ] T004 [P] Confirm `GameState` (`Assets/Scripts/State/GameState.cs`,
  shared-config-and-state/002) is where `RunCompletionState` is stored/exposed, and that
  `GameState`'s existing new-run reset operation (shared-config-and-state/002 FR-004) is the
  single hook this feature's new-run clear attaches to — no second reset path is created.
- [ ] T005 Define `Assets/Scripts/Systems/Progression/RunCompletionState.cs`: a plain data type
  with ordered milestone flags (`Floor52Reached`, `Floor51Reached`, `Floor50Reached`) and a
  `RunOutcome` enum (`InProgress`, `Completed`, `Failed`), defaulting to no milestones reached and
  `InProgress`.
- [ ] T006 Create `Assets/Scripts/Systems/Progression/RunCompletionTracker.cs` stub: static
  methods `RecordFloorReached(RunCompletionState, int floorId)`,
  `RecordFinalDoorSuccess(RunCompletionState)`, `RecordBadEnding(RunCompletionState)`, and
  `ResetForNewRun(RunCompletionState)` — all no-ops for now (depends on T005).

**Checkpoint**: The state type and tracker stub exist — US1 work can begin.

---

## Phase 3: User Story 1 - Complete One Coherent Run (Priority: P1) 🎯 MVP

**Goal**: Completion is recorded only after the full ordered Floor 52→51→50 progression plus a
final-door success; a Bad Ending records failure without ever falsely claiming success; the two
outcomes are mutually exclusive and idempotent; and a new run clears prior completion while
leaving unrelated fixture/settings data untouched.

**Independent Test**: Execute successful, failed, reset, and reload fixtures and inspect the
resulting `RunCompletionState` after each.

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (the tracker stub is all no-ops) before starting
> implementation.

- [ ] T007 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerValidPathTests.cs`, write:
  recording Floor52→Floor51→Floor50 reached in order, then `RecordFinalDoorSuccess`, sets
  `RunOutcome.Completed` and `RunOutcome.Failed` is never set.
- [ ] T008 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerOutOfOrderTests.cs`, write:
  calling `RecordFinalDoorSuccess` when floors were reached out of order (52→50 skipping 51) or
  incompletely (only 52) MUST NOT set `Completed` — proves FR-001's *ordered* progression
  requirement, not just "all three eventually true."
- [ ] T009 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerBadEndingTests.cs`, write:
  `RecordBadEnding` at any milestone state sets `RunOutcome.Failed` and never sets `Completed` —
  a Bad Ending never falsely claims success.
- [ ] T010 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerMutualExclusivityTests.cs`,
  write: once `Completed` is set, `RecordBadEnding` cannot flip it to `Failed`; once `Failed` is
  set, `RecordFinalDoorSuccess` cannot flip it to `Completed` — the first outcome recorded wins
  (FR-002).
- [ ] T011 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerIdempotencyTests.cs`, write:
  calling `RecordFinalDoorSuccess` twice in a row (duplicate final-door input) results in
  `Completed` set exactly once with no observable second write; calling `RecordBadEnding` twice
  similarly results in `Failed` set exactly once (FR-002, SC-002).
- [ ] T012 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerNewRunResetTests.cs`, write:
  `ResetForNewRun` clears milestone flags and `RunOutcome` back to `InProgress`/none-reached
  regardless of prior outcome, while a separate settings-fixture value passed alongside it is
  untouched (FR-003).
- [ ] T013 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerReloadDuringEndingTests.cs`,
  write: simulate a reload event arriving after `RecordFinalDoorSuccess`/`RecordBadEnding` has
  already been recorded but before the ending scene finishes loading — the recorded outcome is
  preserved (not reset, not double-recorded) across the reload.
- [ ] T014 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerAbandonedRunTests.cs`, write: a
  run left `InProgress` (no final door, no Bad Ending ever recorded) records neither `Completed`
  nor `Failed` — an abandoned run is never silently marked as either outcome.
- [ ] T015 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerCorruptedProgressTests.cs`,
  write: milestone data manually set to an impossible combination (`Floor50Reached = true` while
  `Floor52Reached = false`) MUST NOT allow `RecordFinalDoorSuccess` to set `Completed` —
  corrupted/impossible progress is treated as incomplete, never as a false success.

### Implementation for User Story 1

- [ ] T016 [US1] Implement `RecordFloorReached` to set milestone flags only in the fixed forward
  order (52 before 51 before 50) — reaching 51 or 50 out of order does not silently "catch up"
  earlier flags (depends on T006; supports T008).
- [ ] T017 [US1] Implement `RecordFinalDoorSuccess` to check the full ordered milestone set (52
  AND 51 AND 50, consistent with T016's forward-order guarantee) before setting
  `RunOutcome.Completed`; otherwise it is a no-op (depends on T016; closes T007–T008).
- [ ] T018 [US1] Implement `RecordBadEnding` to set `RunOutcome.Failed` unconditionally at any
  milestone state, with a guard that does nothing if `RunOutcome` is already `Completed` or
  `Failed` (mutual exclusion, first-wins) (depends on T005; closes T009–T010).
- [ ] T019 [US1] Add the matching first-wins guard to `RecordFinalDoorSuccess` so it does nothing
  once `RunOutcome` is already `Completed` or `Failed` (depends on T017–T018; closes T010–T011).
- [ ] T020 [US1] Implement `ResetForNewRun` to clear milestone flags and `RunOutcome`, with no
  dependency on and no write to any settings-fixture field passed alongside it in tests (depends
  on T005; closes T012).
- [ ] T021 [US1] Verify `RunCompletionTracker`'s methods are pure functions of the passed-in
  `RunCompletionState` (no static/global mutable state), so a reload that reconstructs
  `RunCompletionState` from persisted data and replays no further ending signal preserves the
  last-recorded outcome by construction (depends on T017–T018; closes T013).
- [ ] T022 [US1] Add the corrupted-progress guard to `RecordFinalDoorSuccess`: treat any milestone
  combination that violates the fixed forward order (a later floor flagged `true` while an
  earlier one is `false`) as incomplete, never as satisfying the completion check (depends on
  T017; closes T015).
- [ ] T023 [US1] Create `Assets/Scripts/MonoBehaviours/RunCompletionListener.cs`: a thin adapter
  subscribing to the final-door success event (keys-and-doors/003), the Bad Ending trigger
  (lives-and-fail-state/003), and per-floor scene-load events (progression-and-scene-flow/002),
  forwarding each into `RunCompletionTracker`'s methods against the `GameState`-held
  `RunCompletionState` — it contains no completion-decision logic of its own (depends on
  T016–T022).
- [ ] T024 [US1] Run T007–T015 against T016–T022's implementation; fix `RunCompletionTracker`
  until every case is green.

**Checkpoint**: User Story 1 is fully functional and independently testable — every SC-001/SC-002
guarantee is proven.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Write
  `Assets/Tests/EditMode/Systems/Progression/RunCompletionTrackerGameStateIntegrationTests.cs`:
  confirm `RunCompletionState` round-trips through `GameState`'s new-run reset operation
  (shared-config-and-state/002) with zero adapter/mapping code required.
- [ ] T026 [P] Add XML doc comments to `RunCompletionState.cs` and `RunCompletionTracker.cs`
  citing GDD Ch. 2.3/10.4, the mutual-exclusivity/first-wins rule, and explicitly noting that
  corrupted or out-of-order milestone data must never satisfy completion.
- [ ] T027 Run the full `Assets/Tests/EditMode/Systems/Progression/` suite covering this feature
  and confirm 100% green before marking this feature done (SC-001/SC-002).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS User Story 1.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **Polish (Phase 4)**: Depends on User Story 1 being complete.

### Parallel Opportunities

- T002–T004 (confirming reused external signals/state) can run in parallel.
- All test-writing tasks marked [P] in Phase 3 can run in parallel with each other (different
  files, independent `RunCompletionState` fixture instances — no shared mutable state).

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 in full — this is the entire scope of this feature.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently; all green.

### Incremental Delivery

1. Setup + Foundational → state type and tracker stub in place, reused signals confirmed.
2. US1 valid/ordered path (T007–T008, T016–T017) → completion-requires-ordered-progression
   guarantee proven.
3. US1 failure path (T009, T018) → Bad Ending never falsely claims success.
4. US1 exclusivity/idempotency (T010–T011, T019) → first-wins and duplicate-signal guarantees
   proven.
5. US1 reset + reload/abandoned/corrupted edge cases (T012–T015, T020–T022) → new-run clear and
   every listed edge case proven.
6. US1 adapter (T023) → wired to the real final-door/Bad Ending/floor-load events.
7. Polish → `GameState` round-trip proven, documentation, full suite green.

---

## Notes

- [P] tasks touch different files or independent `RunCompletionState` fixture instances with no
  ordering dependency on another *unfinished* task in this list.
- Every implementation task in Phase 3 has a matching test task that must be written and failing
  first, per constitution Principle IV.
- `RunCompletionTracker` never gains a "trust the caller" shortcut around the ordered-milestone
  or first-wins checks during this feature's implementation — T008/T010/T015's tasks exist to
  *prove* those guards hold, not to add and later relax them.
- Commit after each task or logical group; stop at the Phase 3 checkpoint to validate the story
  independently before moving to Polish.
