---
description: "Task list for 002-active-battery-count-cap"
---

# Tasks: Active Battery Count Cap

**Input**: Design documents from `specs/systems/battery-spawn-system/002-active-battery-count-cap/spec.md`

**Prerequisites**: spec.md (this folder), plus
`001-per-floor-spawn-point-registry` for the shared `FloorId` type (see that feature's T001).

**Tests**: EditMode tests are mandatory per constitution Principle IV — this feature is 100%
pure C# logic.

**Organization**: Tasks are grouped by user story (US1/US2/US3 from `spec.md`).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Confirm the `FloorId` type location established by
  `001-per-floor-spawn-point-registry`'s T001 (`Assets/Scripts/Systems/Shared/FloorId.cs` or the
  shared-config-and-state equivalent, once it exists) and reuse it — do not redefine a second
  floor identifier type in this feature.
- [ ] T002 [P] Confirm `GameConfig`'s battery fields per GDD Ch. 17.2
  (`batteryMaxActiveFloor51`, `batteryMaxActiveFloor50`) exist or are stubbed in
  `Assets/Config/GameConfig.asset`'s backing `ScriptableObject` class. If
  `shared-config-and-state/001-game-config-schema` has not landed these fields yet, add the two
  fields directly to the existing `GameConfig` class as a minimal, additive change (constitution
  Principle V: additive only, never a second config source) rather than blocking this feature on
  that spec's completion.

## Phase 2: Foundational — Active Count Tracker (blocks User Story 2)

**Purpose**: User Story 2's cap check reads User Story 1's tracked count — the tracker must
exist first, so User Story 1 is implemented before User Story 2 even though both are P1.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T003 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/ActiveBatteryCountTrackerTests.cs`:
  `Increment_RaisesCountByOne` — start at 0, increment once, assert count is 1.
- [ ] T004 [P] [US1] Same file: `Decrement_LowersCountByOne` — set up a count of 2, decrement
  once, assert count is 1 immediately (no delay).
- [ ] T005 [P] [US1] Same file: `CountsAreIndependentPerFloor` — increment Floor 51's count,
  assert Floor 50's count is unaffected.

### Implementation for User Story 1

- [ ] T006 [US1] Implement `ActiveBatteryCountTracker` in
  `Assets/Scripts/Systems/BatterySpawn/ActiveBatteryCountTracker.cs`: plain C# class holding a
  per-floor count (e.g., `Dictionary<FloorId, int>`); methods `Increment(FloorId floor)`,
  `Decrement(FloorId floor)`, `GetCount(FloorId floor)`. No `MonoBehaviour` dependency.
- [ ] T007 [US1] Run T003–T005 and confirm green.
- [ ] T008 [P] [US1] Implement the adapter
  `Assets/Scripts/MonoBehaviours/BatterySpawn/BatteryActiveCountAdapter.cs`: a `MonoBehaviour`
  that subscribes to the `Battery` object's "became active" / "was removed" lifecycle signal
  (as exposed by `flashlight-and-battery/007-battery-pickup-and-spare-slot`) and calls
  `Increment`/`Decrement` synchronously on the tracker. Contains no counting logic itself.

**Checkpoint**: Active count tracker is correct and independently testable.

---

## Phase 3: User Story 2 - A Simple Cap Check (Priority: P1)

**Goal**: One method that answers "is floor F at or above its configured cap right now."

**Independent Test**: See spec.md User Story 2 — Floor 51 cap=2, count=2 → "at cap"; count=1 →
"below cap"; Floor 50 cap=1, count=1 → "at cap".

### Tests for User Story 2

- [ ] T009 [P] [US2] EditMode test in
  `Assets/Tests/EditMode/Systems/BatterySpawn/BatteryCountCapPolicyTests.cs`:
  `IsAtOrAboveCap_CountEqualsMax_ReturnsTrue` — Floor 51, max=2, count=2 → true.
- [ ] T010 [P] [US2] Same file: `IsAtOrAboveCap_CountBelowMax_ReturnsFalse` — Floor 51, max=2,
  count=1 → false.
- [ ] T011 [P] [US2] Same file: `IsAtOrAboveCap_UsesPerFloorMax_NotAGlobalNumber` — Floor 50,
  max=1, count=1 → true (proves the check reads the *floor's* max, not a shared constant).

### Implementation for User Story 2

- [ ] T012 [US2] Implement `BatteryCountCapPolicy` in
  `Assets/Scripts/Systems/BatterySpawn/BatteryCountCapPolicy.cs`: plain C# class/static class
  taking a `GameConfig` reference and an `ActiveBatteryCountTracker`; method
  `IsAtOrAboveCap(FloorId floor)` that resolves the floor's max via a `GetMaxActiveForFloor`
  lookup (see T013) and compares it against `tracker.GetCount(floor)`. Depends on T006.
- [ ] T013 [US2] Implement `GetMaxActiveForFloor(GameConfig config, FloorId floor)` (same file
  or a small dedicated lookup file
  `Assets/Scripts/Systems/BatterySpawn/BatteryCountCapPolicy.cs`) reading
  `GameConfig.batteryMaxActiveFloor51`/`batteryMaxActiveFloor50` per floor — no hardcoded
  literal for either number anywhere else in this feature's code.
- [ ] T014 [US2] Run T009–T011 and confirm green.

**Checkpoint**: User Stories 1 AND 2 both work independently — `003` now has a complete cap-check
API to call.

---

## Phase 4: User Story 3 - The Cap Number Comes From Config, Not Code (Priority: P2)

**Goal**: Prove the resolved maximum tracks `GameConfig`'s current value with no code change.

**Independent Test**: See spec.md User Story 3 — change a fixture `GameConfig`'s value between
two assertions in the same test.

### Tests for User Story 3

- [ ] T015 [P] [US3] EditMode test in `BatteryCountCapPolicyTests.cs`:
  `GetMaxActiveForFloor_ReflectsCurrentConfigValue` — read the max with config value 2, assert
  2; change the fixture config's value to 3 (no code change, no recompilation step — same test
  method, same running process), read again, assert 3.

### Implementation for User Story 3

- [ ] T016 [US3] No new production code expected — this story is a verification of T012/T013's
  design (reads happen live from the passed-in `GameConfig` reference, never cached/baked at
  construction time). If T015 fails because the policy cached the value once, fix
  `BatteryCountCapPolicy` to re-read `GameConfig` on every call rather than caching it.
- [ ] T017 [US3] Run T015 and confirm green.

**Checkpoint**: All three user stories independently functional.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T018 [P] EditMode test: `IsAtOrAboveCap_ConfiguredMaxIsZeroOrNegative_AlwaysAtCap` — covers
  the misconfiguration Edge Case.
- [ ] T019 [P] EditMode test: `IsAtOrAboveCap_UnconfiguredFloor_FailsSafeToAtCap` — covers the
  "no configured maximum for this floor" Edge Case (e.g., Floor 52 or a debug floor).
- [ ] T020 [P] EditMode test: `IsAtOrAboveCap_CountAlreadyExceedsMax_ReturnsTrueWithoutMutatingCount`
  — covers the over-cap Edge Case; assert the tracker's count is unchanged after the check call.
- [ ] T021 Code review pass: confirm zero `UnityEngine.MonoBehaviour`/`Component` references in
  `ActiveBatteryCountTracker.cs` or `BatteryCountCapPolicy.cs`.

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies (T002 may require coordinating with
  `shared-config-and-state/001-game-config-schema` if it is already in progress elsewhere —
  prefer reusing its fields once they exist rather than keeping a duplicate additive stub).
- **Foundational/User Story 1 (T003–T008)**: Depends on Setup.
- **User Story 2 (T009–T014)**: Depends on User Story 1's tracker.
- **User Story 3 (T015–T017)**: Depends on User Story 2's policy.
- **Polish (T018–T021)**: Depends on all user stories being complete.

## Notes

- This entire feature is plain C# — no MonoBehaviour-only logic exempt from EditMode testing.
- `BatteryActiveCountAdapter.cs` is the one file allowed to touch Unity scene/event-subscription
  types; it forwards signals only, no counting logic.
- Commit after each checkpoint.
