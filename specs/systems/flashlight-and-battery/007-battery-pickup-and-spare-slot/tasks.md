---
description: "Task list for Battery Pickup & Spare Slot"
---

# Tasks: Battery Pickup & Spare Slot

**Input**: Design documents from `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/`

**Prerequisites**: [spec.md](./spec.md); `002-battery-real-time-drain-timer` must already
provide the `Battery` entity

**Tests**: Included — constitution Principle IV requires EditMode coverage for every pure-logic
piece; this entire feature is pure logic (world object presence/placement is out of scope, per
spec.md Assumptions).

**Organization**: Tasks are grouped by user story (P1, P1, P2). All three share one underlying
`SpareBatterySlot` class.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: US1, US2, or US3
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/` and `Assets/Tests/EditMode/Flashlight/`
      exist (created by `001`); confirm `Assets/Scripts/MonoBehaviours/Flashlight/` exists
      (created by `003`)

---

## Phase 2: Foundational

- [ ] T002 Add `batterySlots` (int, locked default `1` per GDD Ch. 17.2) to `GameConfig`
- [ ] T003 Create `Assets/Scripts/Systems/Flashlight/BatteryPickupResult.cs`: a plain C# `enum
      BatteryPickupResult` with two values — `Success`, `RejectedSlotFull` (FR-004)

**Checkpoint**: `GameConfig` carries the capacity field; the result type both stories return
exists.

---

## Phase 3: User Story 1 - Player Picks Up a Loose Battery Into an Empty Spare Slot (Priority: P1)

**Goal**: A pickup attempt against a slot with room succeeds, placing a new full-charge battery
into it.

**Independent Test**: `SpareBatterySlotTests` in EditMode, no scene running.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T004 [P] [US1] Create `Assets/Tests/EditMode/Flashlight/SpareBatterySlotTests.cs` with a
      case: construct an empty `SpareBatterySlot` with `GameConfig.batterySlots = 1`, call
      `TryPickUp`, assert the result is `BatteryPickupResult.Success` and the slot now holds
      exactly one battery whose `ChargeSeconds == config.batteryDuration` (spec.md US1 Scenario 1,
      SC-001)
- [ ] T005 [P] [US1] In the same file, add a case asserting a monster-proximity/chase label
      associated with the call (a comment only — the method takes no such parameter) makes no
      difference to the outcome, mirroring `002`'s action-independence test pattern (spec.md US1
      Scenario 3, FR-007)

### Implementation for User Story 1

- [ ] T006 [US1] Create `Assets/Scripts/Systems/Flashlight/SpareBatterySlot.cs`: a plain C# class
      holding a private `List<Battery>` of at most `GameConfig.batterySlots` entries (read from
      config, never hardcoded `1`), exposing `public bool HasRoom(GameConfig config)` and
      `public BatteryPickupResult TryPickUp(GameConfig config)` which, when there is room,
      constructs a new full-charge `Battery` (`ChargeSeconds = config.batteryDuration`), adds it to
      the internal list, and returns `Success` (FR-001, FR-002, FR-008)
- [ ] T007 [US1] Run T004–T005 and confirm all cases pass

**Checkpoint**: User Story 1 independently complete — a pickup into an empty slot works and
produces a full-charge battery.

---

## Phase 4: User Story 2 - Pickup Is Rejected When the Spare Slot Is Already Full (Priority: P1)

**Goal**: A pickup attempt against a full slot is rejected, leaves the slot's contents untouched,
and creates no new battery.

**Independent Test**: Extend `SpareBatterySlotTests` with full-slot rejection cases.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T008 [P] [US2] In `SpareBatterySlotTests.cs`, add a case: construct a `SpareBatterySlot`,
      call `TryPickUp` once to fill it, then call `TryPickUp` a second time; assert the second
      call returns `BatteryPickupResult.RejectedSlotFull`, the slot still holds exactly one
      battery, and it is reference-equal to the one from the first call (not replaced) (spec.md
      US2 Scenario 1, SC-002)
- [ ] T009 [P] [US2] In the same file, add a case that calls `TryPickUp` five more times against
      the already-full slot from T008 and asserts every call returns `RejectedSlotFull` with the
      slot's single battery reference unchanged across all five calls (spec.md US2 Scenario 3,
      Edge Cases — idempotent rejection)
- [ ] T010 [P] [US2] In the same file, add a case with `GameConfig.batterySlots` set to `0`
      (misconfiguration) and assert every `TryPickUp` call against a freshly constructed slot
      returns `RejectedSlotFull`, never `Success` and never a crash (spec.md Edge Cases)

### Implementation for User Story 2

- [ ] T011 [US2] In `SpareBatterySlot.TryPickUp`, when `HasRoom(config)` is `false`, return
      `BatteryPickupResult.RejectedSlotFull` immediately with no mutation to the internal list
      (FR-003)
- [ ] T012 [US2] Run T008–T010 and confirm all cases pass

**Checkpoint**: User Story 2 independently complete — capacity is enforced and rejection has zero
side effects.

---

## Phase 5: User Story 3 - A Rejected Pickup Produces a Distinguishable Outcome (Priority: P2)

**Goal**: Verify success and rejection outcomes are checkable as distinct values, not inferred
from side effects alone.

**Independent Test**: Extend `SpareBatterySlotTests` with a direct outcome-comparison case.

### Tests for User Story 3 (write first, confirm they fail before implementing)

- [ ] T013 [P] [US3] In `SpareBatterySlotTests.cs`, add a case that captures the `
      BatteryPickupResult` from a successful call and from a rejected call and asserts they are
      not equal to each other, and each equals its own named enum value directly (`==
      BatteryPickupResult.Success` / `== BatteryPickupResult.RejectedSlotFull`) — not merely
      inferred from slot contents (spec.md US3 Scenario 1–2, SC-003)

### Implementation for User Story 3

- [ ] T014 [US3] Confirm `BatteryPickupResult` (T003) and `TryPickUp` (T006/T011) already satisfy
      T013 by construction — this task is a verification, not new logic

**Checkpoint**: All three user stories independently complete; the slot's capacity, rejection, and
outcome contract are all covered.

---

## Phase 6: MonoBehaviour Integration (supports all stories, not itself a story)

- [ ] T015 Create `Assets/Scripts/MonoBehaviours/Flashlight/WorldBatteryPickup.cs`: a thin
      `MonoBehaviour` placed on a world battery prefab that, when the player's interaction system
      (`interaction-and-highlight`) signals a pickup attempt on this object, calls `GameState`'s
      `SpareBatterySlot.TryPickUp(config)` and, only on `Success`, deactivates/removes this
      `GameObject` from the world; on `RejectedSlotFull`, leaves the `GameObject` fully active and
      unmodified (FR-002, FR-003, FR-005)
- [ ] T016 In `WorldBatteryPickup.cs`, expose the `BatteryPickupResult` of the most recent attempt
      as a public read so `audio`/`interaction-and-highlight`/`009-battery-hud-indicator` can react
      to success vs. rejection differently without re-deriving the capacity check (FR-004)

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T017 [P] Grep `Assets/Scripts/` for any hardcoded `1` literal duplicating
      `GameConfig.batterySlots` and replace any found with a config read (constitution Principle
      III/V)
- [ ] T018 Add XML-doc comments to `SpareBatterySlot.TryPickUp` documenting that a rejection MUST
      leave the world battery object untouched (FR-005) — a note for whoever writes `008`'s
      install logic and `WorldBatteryPickup`'s caller

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1** depends only on Foundational.
- **User Story 2** depends on User Story 1's `TryPickUp`/`HasRoom` existing (extends the same
  method) — implement sequentially in `SpareBatterySlot.cs`, but tests can be drafted in parallel.
- **User Story 3** depends on both US1 and US2's outcomes existing to compare.
- **Integration (Phase 6)** depends on all three stories being complete.
- **Polish (Phase 7)**: last.

```text
Setup (T001)
   ↓
Foundational (T002-T003)
   ↓
US1 (T004-T007)
   ↓
US2 (T008-T012)
   ↓
US3 (T013-T014)
   ↓
Integration (T015-T016)
   ↓
Polish (T017-T018)
```

## Notes

- [P] tasks touch different files, or independent additive regions of the same new test file.
- `SpareBatterySlot` deliberately does not expose a "drop" or "remove" method beyond the
  install-time `008` consumes — FR-006 is enforced by simply never writing that method (YAGNI,
  constitution Principle II), not by a runtime guard against a call path that doesn't exist.
- `WorldBatteryPickup` (T015) is the first place this feature touches a Unity `GameObject`; the
  capacity/outcome logic it calls into remains fully scene-independent and EditMode-testable.
