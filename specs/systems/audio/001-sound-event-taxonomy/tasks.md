---
description: "Task list for 001-sound-event-taxonomy"
---

# Tasks: Sound Event Taxonomy

**Input**: Design documents from `specs/systems/audio/001-sound-event-taxonomy/spec.md`

**Prerequisites**: spec.md (this folder). GDD Ch. 14.1 (asset table) is the source-of-truth event
list; spec.md FR-002's category grouping (player, monster, light/battery, interaction, ambience,
narrative, UI) supersedes GDD 14.1's raw "Sistem" bucket by splitting it into light/battery,
interaction, and UI.

**Tests**: EditMode tests are mandatory per constitution Principle IV — this feature is 100% pure
C# data/logic (no rendering, physics, or input dependency).

**Organization**: Tasks are grouped by user story (US1 from `spec.md`); Setup and Foundational
phases precede it because US1's category tasks all depend on the registry existing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Define the `SoundCategory` enum (`Ambient`, `Player`, `Monster`, `LightBattery`,
  `Interaction`, `Narrative`, `UI` — per spec.md FR-002) in
  `Assets/Scripts/Systems/Audio/SoundCategory.cs`.
- [ ] T002 [P] Define the `SoundEventId` enum covering every GDD Ch. 14.1 row (see Phase 3 for the
  full per-category member list) in `Assets/Scripts/Systems/Audio/SoundEventId.cs`.

## Phase 2: Foundational — Event Definition Model & Registry (blocks Phase 3 and Phase 4)

**Purpose**: Every category-definition task and the dispatch-policy tasks need a place to
register and look up events first.

### Tests for the registry (write first, confirm they fail before implementing)

- [ ] T003 [P] EditMode test in `Assets/Tests/EditMode/Systems/Audio/SoundEventRegistryTests.cs`:
  `Register_NewEvent_CanBeRetrievedByTryGetDefinition`.
- [ ] T004 [P] Same file: `Register_DuplicateEventId_ThrowsDeterministically` — covers the
  Duplicate Event edge case.
- [ ] T005 [P] Same file: `TryGetDefinition_UnknownEventId_ReturnsFalseWithoutThrowing` — lays the
  groundwork for SC-002 (unknown events never crash).

### Implementation for the registry

- [ ] T006 Implement `SoundEventDefinition` in
  `Assets/Scripts/Systems/Audio/SoundEventDefinition.cs`: plain C# struct/class with `Priority`,
  `SoundCategory Category`, `bool IsPositional`, `bool AllowOverlap` (FR-003's four required
  fields).
- [ ] T007 Implement `SoundEventRegistry` in
  `Assets/Scripts/Systems/Audio/SoundEventRegistry.cs`: plain C# class with `Register(SoundEventId,
  SoundEventDefinition)` (throws on duplicate ID), `TryGetDefinition(SoundEventId, out
  SoundEventDefinition)`, `IsRegistered(SoundEventId)`. No `MonoBehaviour` dependency. Depends on
  T006.
- [ ] T008 Run T003–T005 and confirm green.

**Checkpoint**: Registry is correct and independently testable; category work can begin.

---

## Phase 3: User Story 1 - Audio Responds Consistently (Priority: P1)

**Goal**: Every GDD Ch. 14.1 event is registered with the fields FR-003 requires, split across
spec.md's seven FR-002 categories.

**Independent Test**: See spec.md US1 — enumerate GDD Ch. 14.1 rows and dispatch each event
through the taxonomy.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T009 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/SoundEventCatalogTests.cs`:
  `AmbientEvents_MatchGdd14_1_WithCorrectCategoryAndOverlapAllowed` — covers `AcHum`,
  `ElectricBuzz`, `EmptyOfficeTone`.
- [ ] T010 [P] [US1] Same file: `PlayerEvents_MatchGdd14_1_WithPositionalFootstepsAndBreathing` —
  covers `FootstepWalk`, `FootstepSprint`, `Breathing`, `InteractGeneric`, `BatterySwap`.
- [ ] T011 [P] [US1] Same file: `MonsterEvents_MatchGdd14_1_WithEscalatingPriority` — covers
  `Patrol`, `Distant`, `InvestigationCue`, `Chase`, `AttackCatch`.
- [ ] T012 [P] [US1] Same file: `LightBatteryEvents_MatchGdd14_1` — covers `LightFlicker`.
- [ ] T013 [P] [US1] Same file: `InteractionEvents_MatchGdd14_1` — covers `Pickup`, `DoorLocked`,
  `DoorUnlock`, `SplashTextFloor`.
- [ ] T014 [P] [US1] Same file: `NarrativeEvents_MatchGdd14_1_NonPositionalAndNoOverlap` — covers
  `LiftBell`, `FootstepsBoss`, `CoworkerLaughterDistant` (GDD Ch. 10.3 foreshadowing cues).
- [ ] T015 [P] [US1] Same file: `UiEvents_MatchGdd14_1_OverlapAllowedForRapidTaps` — covers
  `UiButton`.
- [ ] T016 [US1] Same file: `AllGdd14_1Rows_MapToExactlyOneEventDefinition` — SC-001 completeness
  check that iterates every `SoundEventId` member and asserts the registry has exactly one
  definition for it. Depends on T009–T015 existing (even failing) so the enumeration is complete.

### Implementation for User Story 1

- [ ] T017 [US1] Implement the Ambient category seed entries (`AcHum`, `ElectricBuzz`,
  `EmptyOfficeTone`) in `Assets/Scripts/Systems/Audio/SoundEventCatalog.cs`: static class that
  populates a `SoundEventRegistry` via T007. Depends on T007.
- [ ] T018 [P] [US1] Implement the Player category seed entries (`FootstepWalk`,
  `FootstepSprint`, `Breathing`, `InteractGeneric`, `BatterySwap`), same file.
- [ ] T019 [P] [US1] Implement the Monster category seed entries (`Patrol`, `Distant`,
  `InvestigationCue`, `Chase`, `AttackCatch`), same file.
- [ ] T020 [P] [US1] Implement the Light/Battery category seed entries (`LightFlicker`), same
  file.
- [ ] T021 [P] [US1] Implement the Interaction category seed entries (`Pickup`, `DoorLocked`,
  `DoorUnlock`, `SplashTextFloor`), same file.
- [ ] T022 [P] [US1] Implement the Narrative category seed entries (`LiftBell`, `FootstepsBoss`,
  `CoworkerLaughterDistant`), same file.
- [ ] T023 [P] [US1] Implement the UI category seed entries (`UiButton`), same file.
- [ ] T024 [US1] Run T009–T016 and confirm green.

**Checkpoint**: US1's independent test (dispatch every GDD 14.1 row) passes.

---

## Phase 4: User Story 1 continued — Missing-Asset & Runtime-Safety Behavior (FR-004, Edge Cases)

**Goal**: Cover the remaining spec.md Edge Cases — missing clip, rapid repeated noise, paused
game, accessibility mute — deterministically, plus SC-002 (unknown events never crash or play an
arbitrary clip).

### Tests (write first, confirm they fail before implementing)

- [ ] T025 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/SoundEventDispatchPolicyTests.cs`:
  `Evaluate_UnknownEventId_ReturnsSilentUnknown_NoThrow`.
- [ ] T026 [P] [US1] Same file: `Evaluate_KnownEventMissingClip_ReturnsSilentMissingAsset_AndFlagsDevWarning`.
- [ ] T027 [P] [US1] Same file: `Evaluate_RapidRepeatOfNoOverlapEvent_SuppressesSecondCall_WhileFirstStillPlaying`
  — covers the rapid-repeated-noise edge case.
- [ ] T028 [P] [US1] Same file: `Evaluate_EventDuringPause_NonUiEventSuppressed_UiEventStillPlays`
  — covers the paused-game edge case.
- [ ] T029 [P] [US1] Same file: `Evaluate_AccessibilityMuteEnabled_AllNonEssentialEventsSuppressed`
  — covers the accessibility-mute edge case.

### Implementation

- [ ] T030 [US1] Implement `SoundEventDispatchPolicy.Evaluate(...)` core (unknown-event and
  missing-clip branches, returning a `SoundEventDispatchResult` enum: `Play`, `SilentUnknown`,
  `SilentMissingAsset`, `Suppressed`) in
  `Assets/Scripts/Systems/Audio/SoundEventDispatchPolicy.cs`. Depends on T007.
- [ ] T031 [US1] Implement the overlap-suppression branch (rapid repeated noise), same file.
- [ ] T032 [US1] Implement the pause-suppression branch (category-aware: UI events still play),
  same file.
- [ ] T033 [US1] Implement the accessibility-mute branch, same file.
- [ ] T034 [US1] Run T025–T029 and confirm green.
- [ ] T035 [P] [US1] Implement `SoundEventDevWarningLogger` in
  `Assets/Scripts/MonoBehaviours/Audio/SoundEventDevWarningLogger.cs`: `MonoBehaviour` that
  observes `SoundEventDispatchPolicy` results and calls `Debug.LogWarning` on
  `SilentMissingAsset`. Contains no decision logic itself.

**Checkpoint**: US1 fully satisfies FR-004 and all listed Edge Cases.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T036 [P] EditMode test: `TryGetDefinition_EveryRegisteredEvent_HasNonDefaultPriorityAndCategory`
  — guards against a seed entry accidentally left at a default/zero value.
- [ ] T037 Code review pass: confirm zero `UnityEngine.MonoBehaviour`/`Component` references in
  `SoundEventDefinition.cs`, `SoundEventRegistry.cs`, `SoundEventCatalog.cs`, and
  `SoundEventDispatchPolicy.cs`.

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies.
- **Foundational (T003–T008)**: Depends on Setup.
- **User Story 1 categories (T009–T024)**: Depends on Foundational registry (T007).
- **User Story 1 dispatch safety (T025–T035)**: Depends on Foundational registry (T007); may run
  in parallel with the category tasks since it tests policy behavior, not specific event content.
- **Polish (T036–T037)**: Depends on all of the above.

## Notes

- This entire feature is plain C# except `SoundEventDevWarningLogger.cs`, which is the one file
  allowed to touch `UnityEngine.Debug`/scene types — it forwards warnings only, no decision logic.
- Scope excludes actual mixing (see `002-dynamic-mix-state-machine`) and licensing/provenance (see
  `003-audio-asset-sourcing-and-licensing-log`) — do not add either here.
- Commit after each checkpoint.
