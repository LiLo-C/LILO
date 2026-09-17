---
description: "Task list for Eased Radius Transitions"
---

# Tasks: Eased Radius Transitions

**Input**: Design documents from `specs/systems/flashlight-and-battery/003-eased-radius-transitions/`

**Prerequisites**: [spec.md](./spec.md); `001-light-state-thresholds-and-radius` and
`002-battery-real-time-drain-timer` must already exist

**Tests**: Included — constitution Principle IV.

**Organization**: Single P1 user story; tasks split between the pure ease math (testable) and the
`MonoBehaviour` integration (manual/on-device per constitution Principle IV's exception).

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/` and `Assets/Tests/EditMode/Flashlight/`
      exist (created by `001`)

---

## Phase 2: Foundational

- [ ] T002 Add `lightRadiusEaseRate` (float, default `4.0` — carried over, re-confirm on-device
      per spec.md FR-003) to `GameConfig`

---

## Phase 3: User Story 1 - The Displayed Radius Chases Its Target Smoothly, Never Jumps (Priority: P1)

**Goal**: A pure exponential-decay ease function moves a displayed radius toward a target radius
each frame, correctly for any target/rate/deltaTime combination described in spec.md.

**Independent Test**: `RadiusEaserTests` in EditMode, no scene running.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T003 [P] [US1] Create `Assets/Tests/EditMode/Flashlight/RadiusEaserTests.cs`: a test that
      starts `displayed = 220`, `target = 22`, `easeRate = 4.0`, steps forward by `deltaTime =
      1f/60f` repeatedly for 300 steps (5 simulated seconds), and asserts the sequence of returned
      values is strictly monotonically decreasing at every step and never goes below `22`
      (spec.md US1 Scenario 3, SC-001)
- [ ] T004 [P] [US1] In the same file, add a test asserting that after enough simulated time (e.g.
      5 seconds at `easeRate = 4.0`), the displayed value is within 1% of the target — and that at
      no intermediate step does the value undershoot past the target (i.e. cross below `22` when
      easing downward, or above `220` when easing upward) (spec.md SC-002)
- [ ] T005 [P] [US1] In the same file, add a test where the target changes mid-sequence (start
      easing toward `22`, then after 10 steps switch the target to `220`, e.g. simulating an
      install refill) and assert the displayed value begins moving back upward immediately from
      wherever it currently is, with no reset or restart needed (spec.md US1 Scenario 4, Edge
      Cases "target changes again before catching up")
- [ ] T006 [P] [US1] In the same file, add edge-case tests: `easeRate = 0` never changes the
      displayed value across any number of steps; `deltaTime = 0` never changes the displayed
      value; a single very large `deltaTime` (e.g. `100f`) produces a finite result equal to the
      target within floating-point tolerance, with no `NaN`/`Infinity` (spec.md FR-006, SC-005)
- [ ] T007 [P] [US1] In the same file, add a test confirming that when `target` does not change
      between two consecutive calls and `displayed` already equals `target`, the result stays
      exactly equal to `target` (spec.md US1 Scenario 1 — "nothing to ease toward")

### Implementation for User Story 1

- [ ] T008 [US1] Create `Assets/Scripts/Systems/Flashlight/RadiusEaser.cs`: a plain C# static
      class with `public static float Ease(float currentDisplayedRadius, float targetRadius,
      float easeRate, float deltaTime)` implementing `displayed + (target - displayed) * (1 -
      Mathf.Exp(-easeRate * deltaTime))` (FR-002, FR-005, FR-006, FR-007)
- [ ] T009 [US1] Run T003–T007 and confirm all cases pass

**Checkpoint**: The pure ease function is complete and independently verified — no Unity `Light`,
scene, or `Battery` involved yet.

---

## Phase 4: MonoBehaviour Integration (supports the story, not itself a separate story)

- [ ] T010 [US1] Create `Assets/Scripts/MonoBehaviours/Flashlight/FlashlightRadiusController.cs`:
      a `MonoBehaviour` holding a reference to the player's Unity `Light` component and to
      `GameState`'s installed battery; each `Update()`, it (a) reads the battery's charge fraction
      (`002`), (b) calls `LightStateSystem.GetLightState`/`GetTargetRadius` (`001`) to get the
      current target, (c) calls `RadiusEaser.Ease` with the previous frame's displayed radius,
      that target, `GameConfig.lightRadiusEaseRate`, and `Time.deltaTime`, and (d) assigns the
      result to the `Light`'s `range` (FR-001, FR-008)
- [ ] T011 [US1] In `FlashlightRadiusController.cs`, store the displayed radius as a private field
      that persists across frames (not recomputed from scratch each call) so easing is continuous
      exactly as the pure function assumes (FR-001)

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T012 [P] Manual on-device check: shorten `batteryDuration` in a debug `GameConfig` copy (or
      use a debug time-scale) and visually confirm the light shrinks smoothly through Critical
      with no visible stepping, per spec.md SC-003 — record the observation result; this is the
      constitution Principle IV exception for anything needing a live scene/rendering
- [ ] T013 Grep `Assets/Scripts/` for any hardcoded `4.0`/`4f` literal duplicating
      `GameConfig.lightRadiusEaseRate` and replace any found with a config read

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 pure logic (Phase 3)** depends only on Foundational.
- **Integration (Phase 4)** depends on Phase 3's `RadiusEaser` being final, and on `001`/`002`
  already existing.
- **Polish (Phase 5)**: last, and depends on Phase 4 for the on-device check.

```text
Setup (T001)
   ↓
Foundational (T002)
   ↓
US1 pure logic (T003-T009)
   ↓
Integration (T010-T011)
   ↓
Polish (T012-T013)
```

## Notes

- [P] tasks in Phase 3 are independent additions to the same new test file.
- `FlashlightRadiusController` (T010) is the first `MonoBehaviour` in this category to actually
  touch a Unity `Light` component — keep it a thin adapter per constitution Principle III; no
  easing math belongs inside it.
