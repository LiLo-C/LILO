---

description: "Task list for lives-and-fail-state/003-death-sequence-and-outcome-branch"

---

# Tasks: Death Sequence and Outcome Branch

**Input**: Design documents from `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch/`

**Prerequisites**: [spec.md](./spec.md). No `plan.md`/`research.md`/`data-model.md` — the one new
data shape (`GameConfig.deathSequenceDuration`) is small enough to specify directly in spec.md's
Key Entities/FR-012.

**Tests**: Included — constitution Principle IV (Test-Before-Done, NON-NEGOTIABLE). The
sequencing and branch-selection logic is 100% pure C# and 100% EditMode-testable. The death
sequence's actual on-screen presentation (animation/camera/audio) is the one piece that needs a
live scene and falls under Principle IV's manual-scenario exception — tracked separately in
Phase 6.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (all P1 — this
feature's four stories are one linear flow with one binary branch, so priority ties reflect that
all four must work together for the feature to be considered done).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US4)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project. Plain C# orchestration logic under `Assets/Scripts/Systems/FailState/`
(same folder introduced by `002-floor-state-reset-on-death`). Thin `MonoBehaviour` adapters under
`Assets/Scripts/MonoBehaviours/FailState/`. Tests under `Assets/Tests/EditMode/FailState/`.

**Note on sibling systems**: `monster-ai/004-catch-outcome-signal` and
`scenes/008-bad-ending-scene` may not yet be implemented when this feature's tasks are picked up.
Where a sibling's signal/operation does not yet exist, create a minimal interface/stub matching
that sibling's documented contract (per spec.md Assumptions) and leave a
`// TODO(spec-NNN): replace stub once <sibling> lands` comment at the call site — never invent a
second, competing contract in this spec's own files.

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/FailState/`, `Assets/Scripts/MonoBehaviours/FailState/`,
  and `Assets/Tests/EditMode/FailState/` exist (created by `002-floor-state-reset-on-death`); if
  not, create them.
- [ ] T002 [P] Add `deathSequenceDuration` (float, seconds) to `Assets/Scripts/Config/GameConfig.cs`
  per FR-003/FR-012, documented as a feel value with a starting default to re-confirm during
  on-device playtesting (ROADMAP §0), not a fabricated final number.

---

## Phase 2: Foundational — Signal and Result Shapes (blocks all user stories)

**Purpose**: Every user story below needs a `CatchSignal`-shaped input and a well-defined
branch-decision output before any sequencing logic can be tested.

- [ ] T003 Confirm whether `monster-ai/004-catch-outcome-signal` already defines a `CatchSignal`
  (or equivalent) type. If it exists, note its namespace/path for use below. If it does not exist
  yet, create a minimal placeholder in `Assets/Scripts/Systems/FailState/CatchSignal.cs`: a plain
  C# marker type carrying no catch-context payload (deliberately — FR-002 forbids branching on
  catch context), clearly commented as "placeholder — replace with `monster-ai/004`'s definition
  once it lands; this spec must keep consuming it as a context-free 'a catch happened' signal."
- [ ] T004 [P] Create `Assets/Scripts/Systems/FailState/DeathSequenceOutcome.cs`: a plain C#
  enum or small result type expressing exactly two possible outcomes of a resolved death
  sequence — `FloorReset` and `BadEnding` — for tests and the orchestrator below to assert
  against.

**Checkpoint**: `CatchSignal` and `DeathSequenceOutcome` compile and are usable from an EditMode
test with zero scene setup.

---

## Phase 3: User Story 1 - Every Catch Plays the Same Short Death Sequence (Priority: P1)

**Goal**: A CATCH signal, regardless of catch context, always starts the identical death sequence
with no QTE and no way to avert it.

**Independent Test**: See spec.md User Story 1 — fire an exploration-context and a chase-context
CATCH signal separately and confirm both reach the identical death-sequence entry point.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T005 [P] [US1] Create `Assets/Tests/EditMode/FailState/DeathSequenceControllerTests.cs`:
  `OnCatchSignal_AnyContext_StartsDeathSequenceImmediately` — construct a
  `DeathSequenceController` with a fixture `GameConfig`, feed it a `CatchSignal` (a
  context-free marker type per T003), assert the death sequence's "in progress" flag becomes
  true on the same call with no delay or gating (spec.md Acceptance Scenario 1).
- [ ] T006 [P] [US1] Same test file:
  `OnCatchSignal_ExplorationVsChaseContext_ProduceIdenticalSequenceStart` — because `CatchSignal`
  carries no catch-context field (T003), demonstrate this structurally: assert the controller's
  public API for handling a catch takes no context parameter at all, so exploration- and
  chase-context catches are literally the same call site by construction (spec.md Acceptance
  Scenario 2, FR-002).
- [ ] T007 [P] [US1] Same test file: `Tick_WhileDeathSequenceInProgress_NoInputCanAbortIt` — start
  a death sequence, call whatever "cancel"/"skip" surface would exist for other in-game sequences
  (if none exists, assert the controller exposes no such method at all — a structural check, per
  spec.md Acceptance Scenario 1's "no player input accepted to avert it").
- [ ] T008 [P] [US1] Same test file: `Tick_DeathSequenceDurationElapsed_TransitionsExactlyOnce` —
  advance simulated time by exactly `GameConfig.deathSequenceDuration`, assert the controller
  signals "sequence complete" exactly once (not on an earlier or later tick), matching spec.md
  Acceptance Scenario 3.

### Implementation for User Story 1

- [ ] T009 [US1] Create `Assets/Scripts/Systems/FailState/DeathSequenceController.cs`: plain C#
  class (no `MonoBehaviour`) with `void OnCatchSignal(CatchSignal signal)` (begins the sequence,
  ignoring the call if already in progress — see US1 tests and FR-009/US-guard covered fully in
  Phase 4), `void Tick(float deltaTime)` (advances an internal elapsed-time counter), a read-only
  `bool IsInProgress` flag, and an event/callback `Action OnSequenceComplete` fired exactly once
  when elapsed time reaches `GameConfig.deathSequenceDuration`. No context parameter anywhere in
  this public API (FR-002). Depends on T002–T004.
- [ ] T010 [US1] Run T005–T008 and confirm all green.

**Checkpoint**: User Story 1 independently complete — the death sequence starts identically for
any catch and completes exactly once after its configured duration, with no abort surface.

---

## Phase 4: User Story 2 - A Life Is Always Lost, Silently (Priority: P1)

**Goal**: On death-sequence completion, the lives decrement fires exactly once, never surfaced to
the player, and is immune to a second near-simultaneous CATCH signal.

**Independent Test**: See spec.md User Story 2 — complete one death sequence, confirm exactly one
decrement; fire a second CATCH signal mid-sequence, confirm it is ignored.

### Tests for User Story 2

- [ ] T011 [P] [US2] Create `Assets/Tests/EditMode/FailState/DeathOutcomeOrchestratorTests.cs`:
  `OnSequenceComplete_CallsDecrementExactlyOnce` — wire a fixture `LivesSystem`
  (`lives-and-fail-state/001`) to a `DeathOutcomeOrchestrator`, complete one death sequence,
  assert `DecrementLife` was called exactly once (spec.md Acceptance Scenario 1).
- [ ] T012 [P] [US2] Same test file: `SecondCatchSignal_WhileSequenceInProgress_IsIgnored` — start
  a death sequence, fire a second `CatchSignal` before the first completes, let the first
  complete, assert the decrement fired exactly once total (not twice) — covers spec.md FR-009 and
  the "second CATCH signal fires while a death sequence is already playing" Edge Case.
- [ ] T013 [P] [US2] Same test file: `TwoSeparateDeathsInSameRun_ProduceTwoTotalDecrements` — run
  two full, sequential (non-overlapping) death sequences to completion in the same fixture run,
  assert the lives counter reflects exactly two decrements total (spec.md Acceptance Scenario 3).
- [ ] T014 [P] [US2] Same test file (or a UI-audit-style structural test if more appropriate):
  `DeathSequence_NeverExposesLivesCountToAnyReadUiWouldUse` — grep-style structural check (mirror
  `lives-and-fail-state/001`'s T-series convention) confirming no code path in
  `DeathSequenceController`/`DeathOutcomeOrchestrator` calls any UI-rendering API with the lives
  value (spec.md Acceptance Scenario 2, SC-005).

### Implementation for User Story 2

- [ ] T015 [US2] Create `Assets/Scripts/Systems/FailState/DeathOutcomeOrchestrator.cs`: plain C#
  class that subscribes to `DeathSequenceController.OnSequenceComplete` and, on that callback,
  calls `LivesSystem.DecrementLife(state)` (from `lives-and-fail-state/001`) exactly once per
  invocation. Depends on T009.
- [ ] T016 [US2] In `DeathSequenceController.OnCatchSignal` (T009), add the in-progress guard: if
  `IsInProgress` is already `true`, the call is a no-op (FR-009) — confirms the guard used by
  T012 actually lives in the controller, not accidentally in the orchestrator.
- [ ] T017 [US2] Run T011–T014 and confirm all green.

**Checkpoint**: User Story 2 independently complete — exactly one silent decrement per completed
death sequence, immune to overlapping catches.

---

## Phase 5: User Stories 3 & 4 - The Outcome Branch (Floor Reset vs. Bad Ending) (Priority: P1)

**Goal**: Immediately after the decrement, query "has lives remaining" exactly once and call
exactly one of the two branch operations — floor reset (US3) or Bad Ending transition (US4) —
never both, never neither.

**Independent Test**: See spec.md User Stories 3 and 4 — with lives remaining > 0 after decrement,
confirm only the reset call fires; with lives remaining == 0, confirm only the Bad Ending
transition fires.

### Tests for User Stories 3 & 4 (write first, confirm they fail before implementing)

- [ ] T018 [P] [US3] Same test file as T011:
  `LivesRemainAfterDecrement_CallsFloorResetExactlyOnce_NeverBadEnding` — parameterized over
  post-decrement lives values {1, 2, 3}, assert `FloorResetOrchestrator.ResetCurrentFloor`
  (`lives-and-fail-state/002`) is called exactly once and the Bad Ending trigger is never called
  (spec.md Acceptance Scenario 1, SC-003).
- [ ] T019 [P] [US3] Same test file: `AfterFloorReset_PlayerIsAtCheckpointAndGameplayResumes` —
  assert that after the reset call returns, the returned/observed outcome is
  `DeathSequenceOutcome.FloorReset` and no Bad Ending signal was raised (spec.md Acceptance
  Scenario 2).
- [ ] T020 [P] [US4] Same test file: `LivesReachZeroAfterDecrement_TriggersBadEndingExactlyOnce_NeverFloorReset`
  — with post-decrement lives fixed at exactly 0, assert the Bad Ending transition trigger is
  called exactly once and `ResetCurrentFloor` is never called (spec.md Acceptance Scenario 1,
  SC-004).
- [ ] T021 [P] [US4] Same test file: `VeryFirstCatchWithSingleConfiguredLife_RoutesToBadEndingImmediately`
  — fixture-configure `GameConfig.lives = 1`, run exactly one death sequence to completion,
  assert it routes to `DeathSequenceOutcome.BadEnding` on this very first catch (spec.md Edge
  Case "dying at exactly 0 lives on the very first catch", mirroring
  `lives-and-fail-state/001`'s equivalent boundary test).
- [ ] T022 [P] [US4] Same test file: `BadEndingPath_NeverCallsFloorReset_FloorStateLeftAsIs` —
  after routing to the Bad Ending, assert no call was made to `ResetCurrentFloor` and (using the
  `002` fixture harness if available, or a simple spy) that the died-on floor's state was left
  untouched by this spec (spec.md Acceptance Scenario 2).
- [ ] T023 [P] [US3/US4] Same test file: `BranchDecision_DependsOnlyOnHasLivesRemaining_NoOtherSignal`
  — construct two scenarios with identical post-decrement lives values but different floor
  identifiers/catch contexts, assert the branch outcome is identical between them (spec.md
  Acceptance Scenario 3 of both US3 and US4).

### Implementation for User Stories 3 & 4

- [ ] T024 [US3/US4] Extend `DeathOutcomeOrchestrator.cs` (T015): immediately after calling
  `DecrementLife`, call `LivesSystem.HasLivesRemaining(state)` exactly once and branch: if `true`,
  call `FloorResetOrchestrator.ResetCurrentFloor(state, config)`
  (`lives-and-fail-state/002-floor-state-reset-on-death`) and report
  `DeathSequenceOutcome.FloorReset`; if `false`, call a `BadEndingTrigger` (stubbed per T003's
  convention if `scenes/008-bad-ending-scene` does not yet exist — see T025) and report
  `DeathSequenceOutcome.BadEnding`. This if/else MUST be the only place either call happens.
- [ ] T025 [US4] Create `Assets/Scripts/Systems/FailState/BadEndingTrigger.cs`: a minimal plain
  C# interface/stub (e.g. `void TriggerBadEnding()`) matching
  `specs/scenes/008-bad-ending-scene/spec.md`'s expected entry point, with a
  `// TODO(scenes/008-bad-ending-scene): replace stub once that scene spec and its transition
  hook land` comment — this spec's orchestrator calls this interface, never a concrete scene-load
  call, so swapping in the real implementation later requires no change to
  `DeathOutcomeOrchestrator.cs`.
- [ ] T026 [US3/US4] Run T018–T023 and confirm all green.

**Checkpoint**: All four user stories independently functional and tested — the full GDD 9.3
flowchart (catch → sequence → decrement → branch) is implemented end to end in plain C#.

---

## Phase 6: MonoBehaviour Integration & Manual Verification (supports all stories, not itself a story)

- [ ] T027 Create `Assets/Scripts/MonoBehaviours/FailState/DeathSequencePresentationAdapter.cs`: a
  thin `MonoBehaviour` that subscribes to `DeathSequenceController`'s start/complete events and
  drives the actual animation/camera-cut/audio-cue presentation (content itself owned by other
  systems — this adapter only calls into them), and calls `DeathSequenceController.Tick(Time.deltaTime)`
  once per `Update()`. No branch-decision or gameplay-rule logic in this file (constitution
  Principle III) — it only presents what the plain C# controller has already decided.
- [ ] T028 Wire `Assets/Scripts/MonoBehaviours/FailState/FloorResetTrigger.cs` (the placeholder
  created by `002`'s T027) to be invoked from `DeathOutcomeOrchestrator`'s `FloorReset` branch, so
  the previously-documented hook now has a real caller.
- [ ] T029 Manual on-device check (constitution Principle IV's exception for anything needing a
  live scene/animation/camera feel): play through at least one catch from plain exploration and
  one from an active chase; confirm by direct observation that the death sequence's animation,
  camera behavior, and audio are identical between the two, per spec.md SC-001's intent — record
  the result against SC-001.
- [ ] T030 Manual audit (mirroring `lives-and-fail-state/001`'s T012): play one full run including
  at least two deaths; inspect every screen active during/after each death sequence and confirm no
  lives indicator appears anywhere — record the result against SC-005.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T031 [P] Code review pass: confirm `DeathSequenceController.cs`, `DeathOutcomeOrchestrator.cs`,
  `CatchSignal.cs`, `DeathSequenceOutcome.cs`, and `BadEndingTrigger.cs` contain zero
  `UnityEngine.MonoBehaviour`/`Component` references (constitution Principle III).
- [ ] T032 [P] Confirm `deathSequenceDuration` is read from `GameConfig` everywhere it is needed
  and not duplicated as a hardcoded literal anywhere else (constitution Principle II/V).
- [ ] T033 If any stub was created for `monster-ai/004-catch-outcome-signal` (T003) or
  `scenes/008-bad-ending-scene` (T025), confirm the `// TODO(spec-NNN)` comments are present and
  accurately name the pending sibling spec.

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: depends on Setup; blocks every user story (all four consume
  `CatchSignal` and/or produce a `DeathSequenceOutcome`).
- **User Story 1 (Phase 3)**: depends only on Foundational.
- **User Story 2 (Phase 4)**: depends on User Story 1's `DeathSequenceController` existing (T009).
- **User Stories 3 & 4 (Phase 5)**: depend on User Story 2's `DeathOutcomeOrchestrator` existing
  (T015) and its decrement call being correct.
- **Phase 6 (integration/manual)**: depends on Phases 3–5 being complete.
- **Polish (Phase 7)**: last.

```text
Setup (T001-T002)
   ↓
Foundational (T003-T004)
   ↓
US1 (T005-T010)
   ↓
US2 (T011-T017)
   ↓
US3/US4 (T018-T026)
   ↓
Integration & Manual (T027-T030)
   ↓
Polish (T031-T033)
```

## Notes

- All four user stories are P1 because GDD 9.3 describes one linear flow with a single binary
  branch at the end — a "done" feature requires both branches and the sequencing between them to
  be correct together; there is no meaningful partial-credit slice below "the whole flowchart
  works."
- T021 (`VeryFirstCatchWithSingleConfiguredLife_RoutesToBadEndingImmediately`) is this spec's
  direct automated coverage of the lives-decrement/branch boundary condition called out in the
  task brief — do not consider User Story 4 done without it green.
- Where a sibling system's contract (`monster-ai/004`, `scenes/008-bad-ending-scene`) does not yet
  exist in the codebase at implementation time, stub it against that sibling's documented
  contract (never invent new behavior here) and track the stub with a `TODO` comment per T033.
- Commit after each checkpoint, not after every single task.
