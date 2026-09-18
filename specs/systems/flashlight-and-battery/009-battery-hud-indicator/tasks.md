---
description: "Task list for Battery HUD Indicator"
---

# Tasks: Battery HUD Indicator

**Input**: Design documents from `specs/systems/flashlight-and-battery/009-battery-hud-indicator/`

**Prerequisites**: [spec.md](./spec.md); `001-light-state-thresholds-and-radius` must already
provide `LightState`/`LightStateSystem`; `002-battery-real-time-drain-timer` must already provide
`Battery`; `007-battery-pickup-and-spare-slot` must already provide `SpareBatterySlot`;
`008-battery-install-and-refill` must already provide `BatteryInstallSystem.PreviewGate`

**Tests**: Included — constitution Principle IV requires EditMode coverage for every pure-logic
piece; the view-model this feature introduces is pure C# and fully EditMode-testable. Actual pixel
legibility, safe-area behavior, and rotation are rendering-dependent and validated instead via
`quickstart.md`-style manual scenarios on target hardware, per Principle IV's exception.

**Organization**: Tasks are grouped under this feature's single user story (US1, P1), split into
four independently-completable sub-groups (charge fill, state band, spare/install affordance,
edge-case safety) that all land in one `BatteryHudModel` class, followed by the UI wiring that
consumes it.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, or independent additive regions of the same new
  test file, with no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/UI/` and `Assets/Tests/EditMode/UI/` if they do not yet exist
      (this is the first feature under the `UI` category to need EditMode coverage)

---

## Phase 2: Foundational

- [ ] T002 Create `Assets/Scripts/UI/HudChargeBand.cs`: a plain C# `enum HudChargeBand` with
      exactly three values — `Normal`, `Low`, `Flickering` (FR-002) — no `UnityEngine` dependency
- [ ] T003 Create `Assets/Scripts/UI/BatteryHudModel.cs` skeleton: a plain C# class (no
      `MonoBehaviour`) exposing `public float ChargeFraction { get; private set; }`, `public
      HudChargeBand Band { get; private set; }`, `public bool HasSpareBattery { get; private
      set; }`, `public bool ShowInstallAffordance { get; private set; }`, `public string
      AccessibleText { get; private set; }`, and a single `public void Refresh(Battery
      installedBattery, SpareBatterySlot spareSlot, GameConfig config)` entry point (body filled
      in across the tasks below) — `Refresh` is a pure function of its three arguments each call,
      with no internal timer or accumulated state

**Checkpoint**: The view-model's shape exists; `Refresh` is still a no-op stub.

---

## Phase 3: User Story 1 - Read Charge and Spare State at a Glance (Priority: P1)

**Goal**: One pure view-model exposes a normalized charge fill, an accessible text value, a
more-than-color state band, spare-slot occupancy, and an install affordance gated on `008`'s
non-mutating preview — all derivable from a single `Refresh` call with no hidden state.

**Independent Test**: `BatteryHudModelTests` in EditMode, no scene running, feeding
`Battery`/`SpareBatterySlot`/`GameConfig` fixtures directly into `Refresh` and reading the
resulting properties.

### 3a. Charge fill + accessible value (FR-001)

- [ ] T004 [P] [US1] Create `Assets/Tests/EditMode/UI/BatteryHudModelTests.cs` with a case: call
      `Refresh` with an installed battery at `ChargeSeconds == config.batteryDuration` (full);
      assert `ChargeFraction == 1.0` and `AccessibleText` is non-empty (spec.md FR-001, US1 "its
      displayed value matches the state")
- [ ] T005 [P] [US1] In the same file, add a case with `ChargeSeconds = 0`; assert `ChargeFraction
      == 0.0` and `AccessibleText` differs from T004's full-charge text (FR-001)
- [ ] T006 [P] [US1] In the same file, add a case with `ChargeSeconds = 90` of a `batteryDuration
      = 180`; assert `ChargeFraction == 0.5` exactly, matching `Battery.GetChargeFraction` (`002`)
      with no independent re-derivation and no smoothing/lag applied within the same `Refresh`
      call (spec.md US1 "within one frame")
- [ ] T007 [US1] In `BatteryHudModel.Refresh`, set `ChargeFraction =
      installedBattery.GetChargeFraction(config)` and `AccessibleText` to a human-readable string
      derived from that fraction (e.g. a rounded percentage) (FR-001)

### 3b. Charge band beyond color (FR-002)

- [ ] T008 [P] [US1] In `BatteryHudModelTests.cs`, add cases reusing `001`'s exact boundaries:
      fraction `1.0` and `0.31` → `HudChargeBand.Normal`; fraction `0.30` and `0.11` →
      `Flickering`; fraction `0.10` and `0.0` → `Low` (`001`'s `Critical` and `CompactDarkness`
      both collapse into this HUD's single `Low` band) (spec.md FR-002, US1 "compare icon, fill...
      output")
- [ ] T009 [P] [US1] In the same file, add a case asserting the three sample fractions above
      produce three pairwise-distinct `HudChargeBand` values, checkable without inspecting any
      color field — the model's contract for "more than color alone" is that `Band` alone must
      already distinguish them (spec.md Edge Cases: color-vision limitations)
- [ ] T010 [US1] In `BatteryHudModel.Refresh`, set `Band` from
      `LightStateSystem.GetLightState(ChargeFraction, config)` (`001`), mapping `Normal → Normal`,
      `Flickering → Flickering`, and both `Critical` and `CompactDarkness → Low` — the one
      collapse this feature defines; it does not otherwise reinterpret `001`'s state boundaries
      (FR-002)

### 3c. Spare occupancy + install affordance (FR-003)

- [ ] T011 [P] [US1] In `BatteryHudModelTests.cs`, add a case: `Refresh` with an empty
      `SpareBatterySlot` → `HasSpareBattery == false`; `Refresh` again after a successful `007`
      `TryPickUp` → `HasSpareBattery == true` (spec.md US1 Scenario 2)
- [ ] T012 [P] [US1] In the same file, add cases covering all three `BatteryInstallSystem.
      PreviewGate` (`008`) outcomes: `Success` → `ShowInstallAffordance == true`;
      `RejectedLampNotEmpty` → `ShowInstallAffordance == false`; `RejectedNoSpare` →
      `ShowInstallAffordance == false` (spec.md FR-003 "only when the install gate is valid")
- [ ] T013 [US1] In `BatteryHudModel.Refresh`, set `HasSpareBattery = !spareSlot.IsEmpty` and
      `ShowInstallAffordance = BatteryInstallSystem.PreviewGate(installedBattery, spareSlot,
      config) == BatteryInstallResult.Success` — reusing `008`'s non-mutating preview so displaying
      the HUD can never itself trigger an install or a pickup (FR-003)

### 3d. Refresh stability + missing-state safety (Edge Cases)

- [ ] T014 [P] [US1] In the same test file, add a case calling `Refresh` twice in a row with
      unchanged inputs and asserting every property is identical both times — covers spec.md Edge
      Cases "rapid drain, paused time" by construction, since `Refresh` recomputes fully from its
      arguments each call rather than accumulating or animating internally (this is a
      verification of T007/T010/T013's design, not new logic)
- [ ] T015 [P] [US1] In the same file, add a case calling `Refresh` with `GameConfig.batterySlots
      = 0` (mirroring `007`'s misconfiguration edge case) and asserting `HasSpareBattery ==
      false`, `ShowInstallAffordance == false`, and no exception is thrown (spec.md Edge Cases:
      "Missing state")
- [ ] T016 [US1] Run T004–T006, T008–T009, T011–T012, T014–T015 and confirm all cases pass

**Checkpoint**: User Story 1 independently complete — `BatteryHudModel` is a fully tested, pure
view-model; nothing has touched `UnityEngine.UI` yet.

---

## Phase 4: UI Integration (supports the story, not itself a story)

- [ ] T017 Create `Assets/Scripts/UI/BatteryHudView.cs`: a thin `MonoBehaviour` on the HUD `Canvas`
      holding references to the fill `Image`, the label `Text`/`TMP_Text`, a band icon, and a
      spare-slot icon; each frame (or on a `GameState`-driven refresh, per `002`'s adapter
      pattern) it calls `BatteryHudModel.Refresh(...)` and writes the resulting properties onto
      those Unity UI elements — no gameplay/gate logic lives in this file (constitution
      Principle III)
- [ ] T018 In `BatteryHudView.cs`, bind `Band` to both an icon swap **and** the fill color (never
      color alone, FR-002), and bind `ShowInstallAffordance` to the install-prompt element's
      visibility, sourced only from the model — this view never re-derives the gate itself (FR-003)
- [ ] T019 In `BatteryHudView.cs`, anchor the HUD root to the Unity UI `Canvas`'s safe-area rect
      (via `Screen.safeArea`; no third-party package per constitution Principle II) so the HUD
      respects safe-area margins on notched/rounded-corner iPhone displays (FR-004)

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T020 [P] Grep `Assets/Scripts/` and `Assets/Tests/` for any hardcoded `0.30`/`0.10` boundary
      literal reintroduced by this feature's band mapping and replace any found with the
      `LightStateSystem` call from T010 rather than re-deriving thresholds (constitution
      Principle III/V — no second source of truth)
- [ ] T021 Add XML-doc comments to `BatteryHudModel.Refresh` documenting the `Critical`/
      `CompactDarkness` → `Low` collapse (T010) inline, so a later reader doesn't need to re-derive
      it from `001`
- [ ] T022 Manual on-device check (constitution Principle IV exception for legibility/safe-area
      behavior needing a live scene): verify HUD legibility in a dark scene, confirm safe-area
      margins hold on at least one notched and one non-notched supported iPhone aspect ratio, and
      run the 10-trial band/spare-identification pass from spec.md SC-002 with a tester who has
      not seen the underlying state, recording the pass count and any SC-003 aspect-ratio notes

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1**'s four sub-groups (3a–3d) all depend only on Foundational and share one file
  (`BatteryHudModel.cs`), so their tests can be drafted in parallel per the `[P]` tags, but the
  implementation tasks (T007, T010, T013) should not be edited concurrently by two people without
  coordinating, since they land in the same method.
- **UI Integration (Phase 4)** depends on User Story 1 being complete.
- **Polish (Phase 5)**: last.

```text
Setup (T001)
   ↓
Foundational (T002-T003)
   ↓
US1: 3a (T004-T007) ─┐
US1: 3b (T008-T010) ─┤
US1: 3c (T011-T013) ─┼──> T016 (run all)
US1: 3d (T014-T015) ─┘
   ↓
UI Integration (T017-T019)
   ↓
Polish (T020-T022)
```

## Notes

- [P] tasks touch different files, or independent additive regions of the same new test file.
- `BatteryHudModel` deliberately owns no timer, animation, or smoothing state of its own — per
  spec.md's "within one frame" requirement and constitution Principle II (YAGNI), it recomputes
  fully from `Battery`/`SpareBatterySlot`/`GameConfig` on every `Refresh` call rather than
  tracking history.
- `HudChargeBand` is intentionally a separate type from `001`'s `LightState` — the HUD's
  three-band presentation (`Normal`/`Low`/`Flickering`) is this feature's own display contract
  (FR-002), not a redefinition of `001`'s four-value gameplay enum; T010 is the one place the two
  are reconciled.
- `BatteryHudView` (T017) is the first place this feature touches a Unity `GameObject`/`Canvas`;
  the fill/band/affordance logic it reads remains fully scene-independent and EditMode-testable.
