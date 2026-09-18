---

description: "Task list for Hiding Detection Immunity"
---

# Tasks: Hiding Detection Immunity

**Input**: Design documents from
`specs/systems/hiding/002-hiding-detection-immunity-rule/`

**Prerequisites**: `spec.md` (this folder; source of truth, not modified by this task list).
Depends on `hiding/001-enter-and-exit-hiding` (owns `HidingState` and its `StateChanged` event)
and `monster-ai/001-state-machine-core-transitions` (owns `MonsterDetectionSignal` and the
`MonsterController` adapter this feature's gate is wired into). Consumes
`noise-and-detection/002-distance-based-detection-check`'s output as the "raw" signal this feature
overrides.

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature (constitution Principle IV) — the
entire gate is a pure function of `(MonsterDetectionSignal, HidingState)` with no rendering,
physics, or input-device dependency.

**Organization**: This spec has a single user story (US1); tasks are grouped by Setup →
Foundational → US1 → Edge Cases → Polish, matching the granularity of
`specs/systems/monster-ai/002-per-floor-tuning-profile/tasks.md`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: `US1` for every task in Phase 3; unlabeled for Setup/Foundational/Edge
  Cases/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Hiding/`
- EditMode tests: `Assets/Tests/EditMode/Hiding/`
- Reused type (owned by `hiding/001-enter-and-exit-hiding`, not this feature):
  `Assets/Scripts/Systems/Hiding/HidingState.cs`
- Reused type (owned by `monster-ai/001-state-machine-core-transitions`, not this feature):
  `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs`
- Integration point (owned by `monster-ai/001`, this feature only adds a call site):
  `Assets/Scripts/MonoBehaviours/MonsterController.cs`
- Reused signal (owned by `monster-ai/004-catch-outcome-signal`, not this feature):
  `Assets/Scripts/Systems/MonsterAI/CatchSignal.cs`

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Hiding/` and `Assets/Tests/EditMode/Hiding/` exist
  (created by `hiding/001`) — this feature adds files alongside them; no new top-level folders
  needed.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Confirm the two external types this feature reads already exist and are reused, not
duplicated, and stub the gate this feature adds.

**⚠️ CRITICAL**: T002–T004 block every task in Phase 3 onward.

- [ ] T002 [P] Confirm `HidingState` (`Visible`, `Entering`, `Hidden`, `Exiting`) already exists
  at `Assets/Scripts/Systems/Hiding/HidingState.cs` (created by `hiding/001`, T002 in that spec).
  This feature MUST NOT declare a second, competing hiding-state enum.
- [ ] T003 [P] Confirm `MonsterDetectionSignal { bool IsDetected; Vector3 SourcePosition; }`
  already exists at `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs` (owned by
  `monster-ai/001`). This feature reads and returns that exact type — it does not redefine it
  (FR-002, "not a duplicated distance exception").
- [ ] T004 Create `Assets/Scripts/Systems/Hiding/HidingDetectionEligibility.cs`: a static class
  with `public static bool IsEligibleForDetection(HidingState hidingState)` returning `false` only
  for `HidingState.Hidden` and `true` for every other value (`Visible`/`Entering`/`Exiting`) — the
  single source of truth for "which states grant immunity," so `hiding/001`'s `Entering`/`Exiting`
  never accidentally count (FR-001; depends on T002).

**Checkpoint**: The reused types are confirmed and the eligibility predicate exists — gate work
can begin.

---

## Phase 3: User Story 1 — Hiding is a real tactical choice (P1)

**Goal**: While `Hidden`, every detection check the monster runs against the player is overridden
to "not detected," consumed by the detection system as an explicit result rather than a second
distance rule; normal detection resumes the instant the player is no longer canonically `Hidden`.

**Independent Test**: See `spec.md` US1 — run identical noise/line-of-sight fixtures while
Visible, Entering, Hidden, and Exiting.

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (no gate logic exists yet).

- [ ] T005 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingDetectionGateTests.cs`, write: a raw
  `MonsterDetectionSignal` with `IsDetected = true`, gated while `HidingState.Hidden` → the gated
  result's `IsDetected` is `false` and no catch signal would be derivable from it.
- [ ] T006 [P] [US1] Add: the identical raw signal gated while `HidingState.Visible` → the gated
  result is unchanged from the raw input (byte-for-byte, both fields).
- [ ] T007 [P] [US1] Add: the identical raw signal gated while `HidingState.Entering` → the gated
  result is unchanged from the raw input — an in-progress entry MUST NOT grant immunity early
  (FR-001, spec.md "invalid/interrupted hide transition" guarantee).
- [ ] T008 [P] [US1] Add: the identical raw signal gated while `HidingState.Exiting` → the gated
  result is unchanged from the raw input — immunity ends the instant the canonical `Hidden` state
  is left, not only once fully `Visible` again.
- [ ] T009 [US1] Add: a scripted sequence `Hidden → (via Exiting) → Visible` followed immediately
  by a new detection tick with `IsDetected = true` → the gated result reports detected — "the next
  eligible check uses normal rules" (spec.md US1 second acceptance scenario).
- [ ] T010 [US1] Add: when `IsDetected = false` on the raw signal, gating while `Hidden` still
  returns `IsDetected = false` (no accidental flip to `true`) — the gate only ever suppresses,
  never invents, a detection.

### Implementation for User Story 1

- [ ] T011 [US1] Create `Assets/Scripts/Systems/Hiding/HidingDetectionGate.cs`: a static class
  with `public static MonsterDetectionSignal Apply(MonsterDetectionSignal rawSignal, HidingState
  hidingState)` — if `!HidingDetectionEligibility.IsEligibleForDetection(hidingState)`, return
  `new MonsterDetectionSignal { IsDetected = false, SourcePosition = rawSignal.SourcePosition }`;
  otherwise return `rawSignal` unchanged (depends on T003, T004; makes T005–T010 pass).
- [ ] T012 [US1] Wire `monster-ai/001`'s `MonsterController`
  (`Assets/Scripts/MonoBehaviours/MonsterController.cs`) to call `HidingDetectionGate.Apply` on
  the signal returned by `noise-and-detection/002`'s `DetectionCheck.Evaluate`, passing the
  player's current `HidingState` (read from `hiding/001`'s `HidingSystem` or the shared
  `GameState`), before feeding the result into `MonsterStateMachine.Tick()` — this is the single
  call site FR-002 requires ("consumed by the detection system as an explicit result").

**Checkpoint**: Hidden fixtures produce zero detection; Visible/Entering/Exiting/post-exit
fixtures match baseline detection behavior exactly.

---

## Phase 4: Edge Cases & Robustness

**Purpose**: Cover `spec.md`'s Edge Cases paragraph — pause, floor reset, overlapping spots, and
scene unload clear immunity consistently — plus FR-003's catch-signal-in-flight guarantee.

- [ ] T013 [P] In `Assets/Tests/EditMode/Hiding/HidingDetectionGateEdgeCaseTests.cs`, write:
  `HidingDetectionGate.Apply` is a pure, stateless function of its two inputs — calling it
  repeatedly with the same arguments while "paused" (i.e., with no change to either input across
  calls) always returns the identical result, requiring no pause-specific branch of its own.
- [ ] T014 [P] Add: after a floor reset or scene unload forces `HidingState` back to `Visible`
  (via `hiding/001`'s `ForceExit()`), the very next gate call with that state returns the raw
  signal unchanged — immunity never survives a reset it wasn't explicitly re-granted for.
- [ ] T015 [P] Add: gating is entirely indifferent to which hiding spot (if any) produced the
  current `HidingState` — construct the gate call with only the enum value, proving no spot
  identity or position parameter exists for "overlapping spots" to disagree about.
- [ ] T016 [US1] Add: a `CatchSignal` (from `monster-ai/004`) that fired before the player's
  `HidingState` transitioned to `Hidden` is unaffected by this feature — this gate has no method
  that inspects or cancels an already-emitted `CatchSignal`; assert (by reflection/API-surface
  check) that `HidingDetectionGate`'s public surface takes no `CatchSignal` parameter, so an
  in-flight catch obeys only `lives-and-fail-state/003`'s death-sequence guard, per FR-003.

**Checkpoint**: Pause, floor reset, overlapping spots, and scene unload all leave immunity in the
correct, stateless-by-construction state; in-flight catches are provably untouched by this gate.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T017 [P] Write `Assets/Tests/EditMode/Hiding/HidingDetectionNoDuplicateRuleTests.cs`: a
  structural/reflection check over `Assets/Scripts/Systems/MonsterAI/` and
  `Assets/Scripts/MonoBehaviours/MonsterController.cs` confirming no second hiding-aware distance
  or range exception exists outside `HidingDetectionGate` — proves "no duplicate immunity rules
  exist in monster AI" (the original coarse task's intent), now as an automated check rather than
  a manual review note.
- [ ] T018 [P] Write `Assets/Tests/EditMode/Hiding/HidingDetectionSuccessCriteriaTests.cs`:
  a consolidated test asserting SC-001 (zero detection/catch events across 100 `Hidden` trials)
  and SC-002 (Visible and post-exit fixtures match baseline detection behavior across 100 trials)
  in one pass.
- [ ] T019 Add XML doc comments to `HidingDetectionEligibility` and `HidingDetectionGate`'s public
  members citing GDD Ch. 4.3/7.2 and this spec's FR-001–FR-003, so a future contributor does not
  accidentally add a second immunity check elsewhere.
- [ ] T020 Run the full `Assets/Tests/EditMode/Hiding/` suite and confirm 100% green before
  marking this feature done.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks every task below; Phase 2 also depends
  on `hiding/001` (for `HidingState`) and `monster-ai/001` (for `MonsterDetectionSignal`) already
  existing.
- **US1 (Phase 3)** depends on Foundational only (T004's eligibility predicate and T003's reused
  type).
- **Edge Cases (Phase 4)** depends on T011 (the gate implementation) existing to test against, and
  on `hiding/001`'s `ForceExit()` (for T014) and `monster-ai/004`'s `CatchSignal` (for T016)
  already existing.
- **Polish (Phase 5)** depends on everything above.

## Parallel Opportunities

- T002–T003 (confirming reused external types) can run in parallel.
- All test-writing tasks marked [P] within a phase can run in parallel with each other —
  `HidingDetectionGate` is a pure static function with no shared mutable state to race on.
- T012 (the `MonsterController` wiring) is the one task in this feature that touches a file owned
  by another spec (`monster-ai/001`); coordinate before landing it, but it has no ordering
  dependency on Phase 4/5 work.

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another unfinished task in this list.
- `HidingDetectionGate.Apply` never gains a third parameter during this feature's implementation —
  T016 exists to *prove* it has no `CatchSignal`-awareness, not to eventually add and then
  suppress one.
- Per constitution Principle IV, do not mark any Phase 3–4 task "done" until its EditMode tests
  are written, failing first, then passing.
