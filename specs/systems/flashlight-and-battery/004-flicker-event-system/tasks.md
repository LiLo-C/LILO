---
description: "Task list for Flicker Event System"
---

# Tasks: Flicker Event System

**Input**: Design documents from `specs/systems/flashlight-and-battery/004-flicker-event-system/`

**Prerequisites**: [spec.md](./spec.md); `001-light-state-thresholds-and-radius` and
`003-eased-radius-transitions` must already exist

**Tests**: Included — constitution Principle IV. Determinism (FR-009) is required specifically so
these tests can be written without flakiness.

**Organization**: Two P1 user stories, both implemented against one state machine class.

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/` and `Assets/Tests/EditMode/Flashlight/`
      exist (created by `001`)

---

## Phase 2: Foundational

- [ ] T002 Add `flickerIntervalMin` (float, default `0.4`), `flickerIntervalMax` (float, default
      `1.4`), `flickerEventDuration` (float, default `0.15`), and `flickerDipFraction` (float,
      default `0.45`) to `GameConfig` — all carried over/re-confirm per spec.md FR-006
- [ ] T003 Create `Assets/Scripts/Systems/Flashlight/FlickerPhase.cs`: a plain C# `enum
      FlickerPhase` with two values — `Steady`, `Dipping`
- [ ] T004 Create `Assets/Scripts/Systems/Flashlight/IRandomSource.cs`: a small interface
      (`float NextFloat(float min, float max)`) so the flicker state machine's randomization is
      injectable per spec.md FR-009, plus `Assets/Scripts/Systems/Flashlight/SystemRandomSource.cs`
      implementing it with `System.Random` for production use

**Checkpoint**: Supporting types exist; no state machine behavior yet.

---

## Phase 3: User Story 1 - Flicker Only Happens as Separated, Discrete Events (Priority: P1)

**Goal**: A state machine produces steady→dip→steady cycles with config-bounded gaps and
durations, using one random draw per cycle, not per frame.

**Independent Test**: `FlickerEventSystemTests` in EditMode, no scene running, using a seeded
fake `IRandomSource` for determinism.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T005 [P] [US1] Create `Assets/Tests/EditMode/Flashlight/FlickerEventSystemTests.cs` with a
      fake `IRandomSource` returning fixed, known values; drive the state machine forward with
      `LightState.Flickering` held constant for a simulated 60 seconds in small steps, and assert
      every recorded dip's duration equals exactly `flickerEventDuration` and every gap between
      consecutive dip starts is at least `flickerIntervalMin` (spec.md US1 Scenario 1–2, SC-001)
- [ ] T006 [P] [US1] In the same file, add an assertion over the same 60-second run that no two
      recorded dip windows overlap, and that `GetPhase()` is always exactly `Steady` or `Dipping`
      at every sampled instant, never anything else (spec.md US1 Scenario 3)
- [ ] T007 [P] [US1] In the same file, add a test proving the depth fraction returned during a
      single dip does not change between two queries taken at different points within that same
      dip's duration — i.e. it is fixed at dip-start, not resampled per query (spec.md US1
      Scenario 4, FR-004)
- [ ] T008 [P] [US1] In the same file, add a test that on the very first update after the state
      machine is constructed (or after entering `Flickering` fresh), `GetPhase()` reports `Steady`
      — never primed to dip on frame one (spec.md Edge Cases)

### Implementation for User Story 1

- [ ] T009 [US1] Create `Assets/Scripts/Systems/Flashlight/FlickerEventSystem.cs`: a plain C#
      class constructed with an `IRandomSource` and `GameConfig`, exposing `void Update(LightState
      currentState, float deltaTime)`, `FlickerPhase GetPhase()`, and `float GetDipDepth()`
      (returns `0` when `Steady`). Internally tracks a countdown timer; while `Steady` and
      `currentState == Flickering`, counts down a randomized interval
      (`random.NextFloat(flickerIntervalMin, flickerIntervalMax)`, drawn once when the countdown
      is (re)started) and transitions to `Dipping` with a depth fixed from
      `flickerDipFraction` when it elapses; while `Dipping`, counts down
      `flickerEventDuration` and transitions back to `Steady` when it elapses, drawing a fresh
      interval for the next steady wait (FR-001, FR-003, FR-004)
- [ ] T010 [US1] Run T005–T008 and confirm all cases pass

**Checkpoint**: User Story 1 independently complete for the `Flickering`-only case.

---

## Phase 4: User Story 2 - Flicker Never Happens Outside the Flickering State (Priority: P1)

**Goal**: Zero dips in any non-`Flickering` state, and immediate cancellation of an in-progress
dip the instant the state changes away from `Flickering`.

**Independent Test**: Extend `FlickerEventSystemTests` with state-forced-to-other-states runs and
a mid-dip state-change cancellation case.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T011 [P] [US2] In `FlickerEventSystemTests.cs`, add a test driving `Update` for a simulated
      60 seconds with `currentState` forced to `LightState.Normal` throughout, asserting
      `GetPhase()` is `Steady` at every sampled instant and no dip ever starts (spec.md US2
      Scenario 1, SC-002)
- [ ] T012 [P] [US2] In the same file, repeat T011's assertion for `LightState.Critical` and
      `LightState.CompactDarkness` (spec.md US2 Scenario 2–3, SC-002)
- [ ] T013 [P] [US2] In the same file, add a test that starts the state machine in `Flickering`,
      advances it until a dip is in progress (using the fake random source to force a short
      interval), then calls `Update` with `currentState` switched to `Critical` partway through
      the dip's duration, and asserts the very next `GetPhase()` call returns `Steady` with
      `GetDipDepth() == 0` — not waiting out the remaining dip duration (spec.md US2 Scenario 4,
      FR-005, SC-003)
- [ ] T014 [P] [US2] In the same file, add a randomized-trial version of T013 (at least 20 trials
      with different random mid-dip cancellation points, still using a seeded fake source for
      reproducibility) asserting the cancellation holds in every trial (spec.md SC-003)

### Implementation for User Story 2

- [ ] T015 [US2] In `FlickerEventSystem.cs`, at the start of `Update`, check whether
      `currentState != LightState.Flickering`; if so and the machine is currently `Dipping` or
      mid-countdown, force it immediately to `Steady` with a freshly-drawn interval and
      `GetDipDepth() == 0`, before any other logic runs that frame (FR-002, FR-005)
- [ ] T016 [US2] In `FlickerEventSystem.cs`, ensure that when `currentState` transitions *into*
      `Flickering` from any other state, the steady countdown restarts fresh (a new random
      interval draw) rather than resuming a stale countdown from a previous, unrelated time in
      `Flickering` (spec.md Edge Cases)
- [ ] T017 [US2] Run T011–T014 and confirm all cases pass

**Checkpoint**: Both user stories independently complete; the state machine is fully gated and
cancels correctly.

---

## Phase 5: MonoBehaviour Integration (supports both stories, not itself a story)

- [ ] T018 Create `Assets/Scripts/MonoBehaviours/Flashlight/FlashlightFlickerController.cs`: a
      `MonoBehaviour` holding a `FlickerEventSystem` instance, calling `Update(currentLightState,
      Time.deltaTime)` once per frame, and applying `GetDipDepth()` as a multiplier on top of
      `003`'s `FlashlightRadiusController` output (and/or the `Light`'s intensity) — composition
      order decided during this task per spec.md Assumptions
- [ ] T019 In `FlashlightFlickerController.cs`, expose `GetPhase()`/`GetDipDepth()` as public
      read-only accessors so `audio`/`haptics` category systems can query the same signal later
      without this feature depending on them (FR-007)

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T020 [P] Grep `Assets/Scripts/` for any hardcoded flicker-timing literal (`0.4`, `1.4`,
      `0.15`, `0.45`) duplicating a `GameConfig` value from T002 and replace any found
- [ ] T021 Manual on-device check: observe the Flickering state for at least 30 seconds and
      confirm it reads as distinct blips, not constant noise, per spec.md SC-004 — record the
      observation result

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1** depends only on Foundational.
- **User Story 2** extends the same class as User Story 1 — implement sequentially in the same
  file, but the two stories' test cases are independently meaningful and can be written in
  parallel by different people once T009 exists as a skeleton.
- **Integration (Phase 5)** depends on both stories being complete.
- **Polish (Phase 6)**: last.

```text
Setup (T001)
   ↓
Foundational (T002-T004)
   ↓
US1 (T005-T010)
   ↓
US2 (T011-T017)
   ↓
Integration (T018-T019)
   ↓
Polish (T020-T021)
```

## Notes

- [P] tasks are independent additions to the same new test file, or independent new files.
- The injectable `IRandomSource` (T004) exists solely to make FR-009's determinism requirement
  testable — production code still gets true randomness via `SystemRandomSource`.
