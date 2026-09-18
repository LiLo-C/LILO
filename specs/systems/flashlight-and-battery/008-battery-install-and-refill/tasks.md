---
description: "Task list for Battery Install and Refill"
---

# Tasks: Battery Install and Refill

**Input**: Design documents from `specs/systems/flashlight-and-battery/008-battery-install-and-refill/`

**Prerequisites**: [spec.md](./spec.md); `002-battery-real-time-drain-timer` must already provide
the `Battery` entity; `007-battery-pickup-and-spare-slot` must already provide `SpareBatterySlot`

**Tests**: Included — constitution Principle IV requires EditMode coverage for every pure-logic
piece; this feature's gate/consume/refill rules are pure logic (world button prompt rendering is
the sole rendering-dependent exception).

**Organization**: Tasks are grouped by user story (P1, P2). Both share one underlying
`BatteryInstallSystem` class.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, or independent additive regions of the same new
  test file, with no dependency on an incomplete task)
- **[Story]**: US1 or US2
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/`, `Assets/Tests/EditMode/Flashlight/`, and
      `Assets/Scripts/MonoBehaviours/Flashlight/` exist (created by `001`/`002`/`007`)

---

## Phase 2: Foundational

- [ ] T002 Add `batteryInstallChargeThreshold` (float, `ChargeSeconds` units, default `0` — install
      requires the lamp to be at or below this, i.e. "empty" per spec.md US1) to `GameConfig`; this
      is the "lamp charge threshold defined by GameConfig" FR-001 requires, kept distinct from
      `001`'s `lightStateCriticalStart` fraction (no repurposing a shipped key, constitution
      Principle V)
- [ ] T003 Create `Assets/Scripts/Systems/Flashlight/BatteryInstallResult.cs`: a plain C# `enum
      BatteryInstallResult` with three values — `Success`, `RejectedLampNotEmpty`,
      `RejectedNoSpare` (FR-004: UI feedback must tell these two rejection reasons apart)
- [ ] T004 In `Assets/Scripts/Systems/Flashlight/Battery.cs` (from `002`), add `public void
      Refill(float chargeSeconds)` setting `ChargeSeconds = chargeSeconds` directly — the
      sanctioned way any feature restores charge outside normal drain; still no `MonoBehaviour`
      dependency (FR-002)
- [ ] T005 In `Assets/Scripts/Systems/Flashlight/SpareBatterySlot.cs` (from `007`), add `public
      bool IsEmpty` (true when the internal list has zero entries) and `public Battery
      RemoveOne()` (removes and returns the first battery in the internal list) — the one
      sanctioned way this feature empties the slot, per `007`'s FR-006/Notes reserving that right
      for `008`

**Checkpoint**: The result type, the refill primitive, and the consume primitive all exist; no
gate logic wired yet.

---

## Phase 3: User Story 1 - Install a Carried Battery (Priority: P1)

**Goal**: A install attempt against an empty lamp with a carried spare atomically consumes the
spare and refills the lamp to full; any other combination is rejected with zero mutation.

**Independent Test**: `BatteryInstallSystemTests` in EditMode, no scene running, using plain
`Battery`/`SpareBatterySlot`/`GameConfig` fixtures.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T006 [P] [US1] Create `Assets/Tests/EditMode/Flashlight/BatteryInstallSystemTests.cs` with a
      case: installed `Battery.ChargeSeconds = 0`, `SpareBatterySlot` holding one battery, call
      `BatteryInstallSystem.TryInstall`, assert the result is `BatteryInstallResult.Success`, the
      installed battery's `ChargeSeconds == config.batteryDuration`, and `spareSlot.IsEmpty ==
      true` afterward (spec.md US1 Scenario 1, SC-001, SC-002)
- [ ] T007 [P] [US1] In the same file, add a boundary case: installed battery at exactly
      `config.batteryInstallChargeThreshold` installs successfully, while one unit above that
      threshold is rejected with `RejectedLampNotEmpty` (spec.md FR-001)
- [ ] T008 [P] [US1] In the same file, add a case: installed `Battery.ChargeSeconds = 0`, an
      **empty** `SpareBatterySlot` (no spare), call `TryInstall`, assert
      `BatteryInstallResult.RejectedNoSpare`, and the installed battery's `ChargeSeconds` is
      unchanged (still `0`) (spec.md US1 Scenario 2 "no spare exists")
- [ ] T009 [P] [US1] In the same file, add a case: installed battery at full charge
      (`config.batteryDuration`, representing "non-empty") with a spare present, call
      `TryInstall`, assert `BatteryInstallResult.RejectedLampNotEmpty`, the spare slot is
      unchanged (still holds its one battery, same reference), and the installed battery's charge
      is unchanged (spec.md US1 Scenario 2 "charge is non-empty"; Edge Cases "full charge")
- [ ] T010 [P] [US1] In the same file, add an atomicity case for the success path from T006:
      snapshot the spare slot's battery count and the installed battery reference before the
      call, then assert exactly one change happened after (spare count decreased by exactly `1`,
      installed battery's `ChargeSeconds` changed, nothing else) — never two mutations, never a
      partial one (spec.md SC-001 "exactly one state mutation")
- [ ] T011 [P] [US1] In the same file, add a duplicate-input case (FR-003): starting from an empty
      lamp with exactly one spare, call `TryInstall` twice back-to-back; assert the first call
      returns `Success` and the second returns `RejectedNoSpare` (the slot is already empty by
      then) — proving at most one install happens per available spare, by construction, with no
      separate debounce flag needed

### Implementation for User Story 1

- [ ] T012 [US1] Create `Assets/Scripts/Systems/Flashlight/BatteryInstallSystem.cs`: a plain C#
      static class with `public static BatteryInstallResult TryInstall(Battery installedBattery,
      SpareBatterySlot spareSlot, GameConfig config)` that checks the charge gate first
      (`installedBattery.ChargeSeconds > config.batteryInstallChargeThreshold` →
      `RejectedLampNotEmpty`), then the spare gate (`spareSlot.IsEmpty` → `RejectedNoSpare`), and
      only if both pass calls `spareSlot.RemoveOne()` followed by
      `installedBattery.Refill(config.batteryDuration)`, returning `Success` (FR-001, FR-002,
      FR-003)
- [ ] T013 [US1] Run T006–T011 and confirm all cases pass

**Checkpoint**: User Story 1 independently complete — install is atomic, gated on both conditions,
and naturally idempotent against duplicate input.

---

## Phase 4: User Story 2 - Teach the Gate Clearly (Priority: P2)

**Goal**: Whatever renders the install prompt can tell "lamp not empty" apart from "no spare"
*before* the player presses the button, without mutating any state just to check.

**Independent Test**: Extend `BatteryInstallSystemTests` with a non-mutating preview query, plus
direct pairwise-distinctness assertions across all three outcomes.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T014 [P] [US2] In `BatteryInstallSystemTests.cs`, add a case calling a new
      `BatteryInstallSystem.PreviewGate(installedBattery, spareSlot, config)` on a non-empty-lamp
      + spare-present state; assert it returns `RejectedLampNotEmpty` and that calling it twice in
      a row causes zero mutation to either the installed battery or the spare slot (spec.md US2
      "inspect prompt state for each gate condition")
- [ ] T015 [P] [US2] In the same file, add a case calling `PreviewGate` on an empty-lamp +
      no-spare state; assert `RejectedNoSpare` with zero mutation
- [ ] T016 [P] [US2] In the same file, add a case calling `PreviewGate` on an empty-lamp +
      spare-present state; assert `Success` with zero mutation, and that immediately following it
      with an actual `TryInstall` call under the same fixture also returns `Success` (the preview
      and the real attempt agree)
- [ ] T017 [P] [US2] In the same file, add a case asserting the three outcomes obtained from
      T014–T016 are pairwise distinct enum values, checkable directly (`==
      BatteryInstallResult.Success` / `RejectedLampNotEmpty` / `RejectedNoSpare`) — never inferred
      from a side effect, so a caller can render three unambiguous prompt states, never a single
      ambiguous "enabled" button (spec.md US2 "no ambiguous enabled button remains", FR-004)

### Implementation for User Story 2

- [ ] T018 [US2] In `BatteryInstallSystem.cs`, extract the charge-then-spare gate check (T012) into
      a private static helper shared by both methods, then add `public static BatteryInstallResult
      PreviewGate(Battery installedBattery, SpareBatterySlot spareSlot, GameConfig config)` that
      runs only that helper and never calls `RemoveOne()`/`Refill()`; refactor `TryInstall` to call
      the same helper and perform the mutation only when it returns `Success` (FR-004)
- [ ] T019 [US2] Run T014–T017 and confirm all cases pass

**Checkpoint**: Both user stories independently complete — the gate's three outcomes are
consistent whether queried read-only or actually attempted, and are distinguishable without
exposing which specific field of internal state caused the rejection.

---

## Phase 5: MonoBehaviour Integration (supports both stories, not itself a story)

- [ ] T020 Create `Assets/Scripts/MonoBehaviours/Flashlight/BatteryInstallAdapter.cs`: a thin
      `MonoBehaviour` that, on the context-sensitive action button's press edge (from
      `interaction-and-highlight`), reads `GameState`'s installed `Battery` and
      `SpareBatterySlot`, calls `BatteryInstallSystem.TryInstall(...)`, and exposes the latest
      `BatteryInstallResult` as a public read so `audio` and `009-battery-hud-indicator` can react
      (FR-002, FR-003, FR-004); guard the call so it is a no-op when `GameState`'s installed
      battery reference is `null` (Edge Cases: "a missing battery reference MUST not duplicate or
      lose inventory" — mirrors `002`'s `BatteryDrainAdapter` null-guard pattern)
- [ ] T021 In `BatteryInstallAdapter.cs`, drive the action button's prompt text/enabled-state every
      frame from `BatteryInstallSystem.PreviewGate(...)` — never `TryInstall` — so merely
      displaying the prompt never mutates state; call `TryInstall` only on the actual press edge
      (spec.md US2, Edge Cases "input during transition")

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T022 [P] Grep `Assets/Scripts/` for any hardcoded charge-empty literal (`0`) duplicating
      `GameConfig.batteryInstallChargeThreshold` and replace any found with a config read
      (constitution Principle III/V)
- [ ] T023 Manual on-device check (constitution Principle IV exception for input-timing/UI-binding
      behavior needing a live scene): confirm the install SFX and "cahaya kembali penuh" feedback
      (GDD Ch. 4.2) fire exactly once per real install, and that the action button's label
      reflects each of the three gate states correctly, recording the observation result

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1** depends only on Foundational (T004/T005's `Refill`/`RemoveOne` primitives).
- **User Story 2** depends on User Story 1's `TryInstall` gate ordering (T012) existing to extract
  into a shared helper — implement sequentially in `BatteryInstallSystem.cs`, but tests can be
  drafted in parallel per the `[P]` tags.
- **Integration (Phase 5)** depends on both stories being complete.
- **Polish (Phase 6)**: last.

```text
Setup (T001)
   ↓
Foundational (T002-T005)
   ↓
US1 (T006-T013)
   ↓
US2 (T014-T019)
   ↓
Integration (T020-T021)
   ↓
Polish (T022-T023)
```

## Notes

- [P] tasks touch different files, or independent additive regions of the same new test file.
- The charge gate is checked before the spare gate in every path (`TryInstall` and `PreviewGate`
  alike) so a lamp that is both non-empty and spare-less always reports `RejectedLampNotEmpty` —
  one deterministic priority order, not caller-dependent.
- `BatteryInstallSystem` never exposes a way to remove/refill outside `007`'s `RemoveOne()` and
  `002`'s new `Refill()` — no duplicate consume/refill path is added anywhere else (YAGNI,
  constitution Principle II).
- `BatteryInstallAdapter` (T020) is the first place this feature touches a Unity `GameObject`/UI
  button; the gate/consume/refill logic it calls into remains fully scene-independent and
  EditMode-testable.
