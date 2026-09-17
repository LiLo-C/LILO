---
description: "Task list for Battery Real-Time Drain Timer"
---

# Tasks: Battery Real-Time Drain Timer

**Input**: Design documents from `specs/systems/flashlight-and-battery/002-battery-real-time-drain-timer/`

**Prerequisites**: [spec.md](./spec.md); `001-light-state-thresholds-and-radius` must already
provide `LightState`/`LightStateSystem`

**Tests**: Included — constitution Principle IV.

**Organization**: Tasks are grouped by user story (both P1).

## Format: `[ID] [P?] [Story] Description`

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/` and `Assets/Tests/EditMode/Flashlight/`
      exist (created by `001`); if not, create them

---

## Phase 2: Foundational

- [ ] T002 Add `batteryDuration` (float, default `180`) to `GameConfig`
- [ ] T003 Create `Assets/Scripts/Systems/Flashlight/Battery.cs`: a plain C# class with
      `public float ChargeSeconds { get; private set; }`, a constructor/factory that initializes
      it to a given value, and a read-only `public float GetChargeFraction(GameConfig config) =>
      Mathf.Clamp01(ChargeSeconds / config.batteryDuration)` (FR-009). No `MonoBehaviour`
      dependency.

**Checkpoint**: `Battery` entity exists with a charge value and a derived fraction; nothing drains
it yet.

---

## Phase 3: User Story 1 - A Full Battery Drains to Empty in Exactly 180 Seconds (Priority: P1)

**Goal**: `Battery` charge decreases by exactly elapsed time, reaching `0` at exactly
`batteryDuration` regardless of step decomposition.

**Independent Test**: `BatteryDrainTests` in EditMode, no scene running, summing arbitrary
`deltaTime` sequences to 180s and checking the final charge is exactly `0`.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T004 [P] [US1] Create `Assets/Tests/EditMode/Flashlight/BatteryDrainTests.cs`: a test that
      starts a `Battery` at `ChargeSeconds = 180` and applies 10,800 steps of `1f/60f` (simulating
      60 FPS), asserting final `ChargeSeconds == 0` (spec.md US1 Scenario 1)
- [ ] T005 [P] [US1] In the same file, add a test that starts a fresh `Battery` at
      `ChargeSeconds = 180` and applies 600 steps of `0.3f` (simulating a stuttering ~3.3 FPS),
      asserting the final `ChargeSeconds` is identical to T004's result (spec.md US1 Scenario 2,
      SC-001)
- [ ] T006 [P] [US1] In the same file, add a test that applies a single drain step of `181f`
      (elapsed time exceeding remaining charge) to a `Battery` at `ChargeSeconds = 180`, asserting
      the result clamps to exactly `0`, never negative (spec.md FR-007, Edge Cases, SC-003)
- [ ] T007 [P] [US1] In the same file, add a test that applies a drain step of `0f` elapsed time
      and asserts `ChargeSeconds` is unchanged (spec.md US1 Scenario 3's "no elapsed time passed
      → no drain" guarantee)

### Implementation for User Story 1

- [ ] T008 [US1] Add `public void Drain(float elapsedSeconds)` to `Battery.cs`:
      `ChargeSeconds = Mathf.Max(0f, ChargeSeconds - elapsedSeconds)` — a pure function of current
      charge and elapsed time only (FR-001, FR-002, FR-007)
- [ ] T009 [US1] Run T004–T007 and confirm all cases pass

**Checkpoint**: User Story 1 independently complete — drain timing is frame-rate-independent and
correct at every tested decomposition.

---

## Phase 4: User Story 2 - Drain Rate Is Identical Regardless of Player Action (Priority: P1)

**Goal**: Prove by construction (no such parameter exists) that drain never varies by player
action.

**Independent Test**: `BatteryDrainActionIndependenceTests` in EditMode — drain twice by the same
elapsed time under different narrative labels, assert identical results.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T010 [P] [US2] Create
      `Assets/Tests/EditMode/Flashlight/BatteryDrainActionIndependenceTests.cs`: create two
      `Battery` instances both at `ChargeSeconds = 100`, call `Drain(30f)` on both (one comment
      labeled "idle", one labeled "sprinting" — the call signature itself takes no such
      parameter), assert both end at the same `ChargeSeconds` (spec.md US2 Scenario 1)
- [ ] T011 [P] [US2] In the same file, add a case simulating "hiding": call `Drain(45f)` on a
      battery and assert the result matches a plain `Drain(45f)` call with no other state
      involved, documenting that hiding's zero-noise rule (from `hiding` category) has no special
      drain path here (spec.md US2 Scenario 2, FR-006)
- [ ] T012 [P] [US2] In the same file, add a case for an "interaction" label (e.g. door open)
      applying `Drain(2f)` for a 2-second interaction, asserting the same `-2` charge as a plain
      idle 2 seconds (spec.md US2 Scenario 3)

### Implementation for User Story 2

- [ ] T013 [US2] Confirm `Battery.Drain(float elapsedSeconds)` (from T008) has no movement-state,
      action, or interaction-type parameter of any kind — if any was added, remove it (FR-004);
      this task is a verification/refactor task, not new logic
- [ ] T014 [US2] Run T010–T012 and confirm all cases pass

**Checkpoint**: Both user stories independently complete; `Battery.Drain` is proven
action-independent by its own signature, not just by test coverage.

---

## Phase 5: MonoBehaviour Integration (supports both stories, not itself a story)

- [ ] T015 Create `Assets/Scripts/MonoBehaviours/Flashlight/BatteryDrainAdapter.cs`: a thin
      `MonoBehaviour` holding a reference to the installed `Battery` (via `GameState`, per
      `specs/systems/shared-config-and-state/002-shared-game-state-and-manager`) and calling
      `battery.Drain(Time.deltaTime)` once per `Update()` — no gameplay rule logic in this file,
      only the Unity lifecycle wiring (constitution Principle III)
- [ ] T016 In `BatteryDrainAdapter.cs`, guard the `Update()` call so it is a no-op when
      `GameState`'s installed battery reference is `null` (spec.md FR-008)

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T017 [P] Grep `Assets/Scripts/` for any hardcoded `180` literal duplicating
      `GameConfig.batteryDuration` and replace any found with a config read
- [ ] T018 Manual on-device check (constitution Principle IV exception for anything needing a live
      scene/frame-rate variance): let a battery drain fully on target hardware and confirm by
      stopwatch it takes approximately 180 real seconds, recording the result for spec.md SC-004

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1** depends only on Foundational.
- **User Story 2** depends on User Story 1's `Drain` method existing (T008) to verify it has no
  extra parameter — practically, implement/verify sequentially, but the two stories' *tests* can
  be written in parallel per the `[P]` tags since they're different files.
- **Phase 5 (integration)** depends on both stories' `Battery.Drain` being final.
- **Polish (Phase 6)**: last.

```text
Setup (T001)
   ↓
Foundational (T002-T003)
   ↓
US1 (T004-T009)
   ↓
US2 (T010-T014)
   ↓
Integration (T015-T016)
   ↓
Polish (T017-T018)
```

## Notes

- [P] tasks touch different files or independent regions of the same new test file.
- `Battery.cs` is intentionally minimal in this feature — no world/spare/installed distinction yet
  (that's `007`/`008`). Do not add those fields here; keep this feature's `Battery` to exactly
  what draining needs (YAGNI, constitution Principle II).
