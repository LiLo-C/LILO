---
description: "Task list for 002-dynamic-mix-state-machine"
---

# Tasks: Dynamic Audio Mix State Machine

**Input**: Design documents from
`specs/systems/audio/002-dynamic-mix-state-machine/spec.md`

**Prerequisites**: spec.md (this folder). GDD Ch. 14.2 defines the monster-proximity mix
characters (PATROL/far = near-silent ambient, INVESTIGATE = subtle cue, near player = heartbeat
enters mix, CHASE = full intensity); spec.md US1 adds three states GDD 14.2 does not cover
(Hiding, Pause, Ending) with explicit precedence per FR-004.

**Tests**: EditMode tests are mandatory per constitution Principle IV for every pure-logic piece.
`OnApplicationPause`/device-suspend handling is the one Unity-lifecycle exception, validated
instead via quickstart manual scenarios per Principle IV.

**Organization**: Tasks are grouped by user story (US1 from `spec.md`); Setup and Foundational
phases precede it because US1's transition logic depends on the precedence table existing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Define the `MixState` enum (`Exploration`, `Investigation`, `Chase`, `Hiding`, `Pause`,
  `Ending` — per spec.md US1) in `Assets/Scripts/Systems/Audio/MixState.cs`.
- [ ] T002 [P] Define `MixStateProfile` in `Assets/Scripts/Systems/Audio/MixStateProfile.cs`:
  plain C# struct/class holding `SnapshotName` (string reference to an `AudioMixerSnapshot`,
  resolved by the adapter — no `UnityEngine` type here) and `TransitionDurationSeconds`, one
  instance per `MixState` per GDD 14.2's characterizations (Exploration = near-silent/ambient
  only, Investigation = subtle cue, Chase = full intensity; Hiding/Pause/Ending have no GDD 14.2
  precedent and get profiles defined by this spec).

## Phase 2: Foundational — State Precedence (blocks Phase 3)

**Purpose**: User Story 1's transition logic needs an authoritative precedence order before it
can arbitrate conflicting state requests (FR-004).

### Tests for precedence (write first, confirm they fail before implementing)

- [ ] T003 [P] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/MixStatePrecedenceTests.cs`:
  `Ending_OutranksEveryOtherState`.
- [ ] T004 [P] Same file: `Hiding_OutranksChaseInvestigationExploration`.
- [ ] T005 [P] Same file: `Pause_OutranksGameplayStates_ButNotEnding`.

### Implementation for precedence

- [ ] T006 Implement `MixStatePrecedence` in
  `Assets/Scripts/Systems/Audio/MixStatePrecedence.cs`: plain C# static class with an explicit
  rank table (no magic numbers scattered elsewhere) and `IsHigherOrEqual(MixState requested,
  MixState current)`.
- [ ] T007 Run T003–T005 and confirm green.

**Checkpoint**: Precedence order is correct and independently testable.

---

## Phase 3: User Story 1 - Tension Changes With Danger (Priority: P1)

**Goal**: The mix shifts between all six states without abrupt or contradictory transitions,
honoring precedence and pause-restore.

**Independent Test**: See spec.md US1 — feed the state sequence and assert active mix, priority,
and restoration after each transition.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T008 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/DynamicMixSystemTests.cs`:
  `RequestState_ExplorationToInvestigation_UpdatesActiveStateAndProfile`.
- [ ] T009 [P] [US1] Same file: `RequestState_InvestigationToChase_UpdatesActiveStateAndProfile`.
- [ ] T010 [P] [US1] Same file: `RequestState_ChaseBackToExploration_ProvidesNonZeroTransitionDuration`
  — proves transitions are eased, not an abrupt cut (FR-002).
- [ ] T011 [P] [US1] Same file: `RequestState_HidingWhileChasing_HidingWins_PerPrecedence`.
- [ ] T012 [P] [US1] Same file: `RequestState_EndingWhileHiding_EndingWins_AsFinalState`.
- [ ] T013 [P] [US1] Same file: `EnterPause_ThenExitPause_RestoresExactPriorGameplayState` (FR-003).
- [ ] T014 [P] [US1] Same file: `RequestState_SameStateTwiceInARow_IsIdempotent_NoRetrigger` —
  covers the duplicate-state edge case.

### Implementation for User Story 1

- [ ] T015 [US1] Implement `DynamicMixSystem` core in
  `Assets/Scripts/Systems/Audio/DynamicMixSystem.cs`: plain C# class holding the current
  `MixState`, exposing `ActiveState`/`ActiveProfile`, and `RequestState(MixState requested)` gated
  by `MixStatePrecedence.IsHigherOrEqual`. No `MonoBehaviour` dependency. Depends on T006.
- [ ] T016 [US1] Implement `EnterPause()`/`ExitPause()` on `DynamicMixSystem`: stores the prior
  non-pause `MixState` on entry and restores it on exit, same file.
- [ ] T017 [US1] Implement the idempotent same-state guard in `RequestState()` (no-op, no
  duplicate transition emitted when `requested == ActiveState`), same file.
- [ ] T018 [US1] Expose `CurrentTransition` (from-state, to-state, duration/curve sourced from
  `MixStateProfile`) on `DynamicMixSystem` for the adapter to drive the real mixer, same file.
- [ ] T019 [US1] Run T008–T014 and confirm green.
- [ ] T020 [US1] Implement `DynamicMixStateAdapter` in
  `Assets/Scripts/MonoBehaviours/Audio/DynamicMixStateAdapter.cs`: `MonoBehaviour` that
  subscribes to the monster-ai, hiding, pause, and ending/win-lose signals, calls
  `DynamicMixSystem.RequestState(...)`, and drives `AudioMixerSnapshot.TransitionTo(...)` from
  `CurrentTransition`. Contains no state-selection or precedence logic itself.

**Checkpoint**: US1's independent test (full state sequence with restoration) passes.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T021 [P] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/DynamicMixSystemTests.cs`:
  `RequestState_ProfileMissingSnapshotReference_FailsSafeToPreviousState_NoException` — covers
  the missing-mixer-snapshot edge case.
- [ ] T022 [P] Same file: `MuteSettingIsOn_StateStillTracked_TransitionStillComputed` — proves the
  mute setting is orthogonal to state tracking (so unmuting resumes at the correct state), per
  the mute-settings edge case.
- [ ] T023 Implement the missing-snapshot fail-safe branch in `DynamicMixSystem.RequestState()` /
  `MixStateProfile` validation, `Assets/Scripts/Systems/Audio/DynamicMixSystem.cs`.
- [ ] T024 Run T021–T022 and confirm green.
- [ ] T025 [P] Implement `OnApplicationPause` handling in
  `Assets/Scripts/MonoBehaviours/Audio/DynamicMixStateAdapter.cs` (app-suspend edge case) —
  Unity lifecycle callback, no EditMode test per constitution Principle IV's live-scene
  exception; verify via this feature's `quickstart.md` manual scenario on device.
- [ ] T026 Code review pass: confirm zero `UnityEngine` references in `MixState.cs`,
  `MixStateProfile.cs`, `MixStatePrecedence.cs`, and `DynamicMixSystem.cs`.

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies.
- **Foundational (T003–T007)**: Depends on Setup.
- **User Story 1 (T008–T020)**: Depends on Foundational precedence table (T006).
- **Polish (T021–T026)**: Depends on User Story 1 being complete.

## Notes

- This entire feature is plain C# except `DynamicMixStateAdapter.cs`, which is the one file
  allowed to touch `AudioMixerSnapshot`/scene/lifecycle types — it forwards signals and drives the
  mixer only, no state-selection logic.
- Scope excludes the event taxonomy (see `001-sound-event-taxonomy`) and asset
  sourcing/licensing (see `003-audio-asset-sourcing-and-licensing-log`) — do not add either here.
- Commit after each checkpoint.
