---
description: "Task list for Haptic Trigger Events"
---

# Tasks: Haptic Trigger Events

**Input**: Design documents from `specs/systems/haptics/001-haptic-trigger-events/`

**Prerequisites**: `spec.md` (source of truth for behavior) and `research.md` (source of truth
for the platform-API decision) in this folder. No `plan.md` exists — tasks are derived directly
from `spec.md`'s functional requirements/edge cases and `research.md`'s architecture decision.
Depends on `specs/systems/shared-config-and-state/001-game-config-schema/` for the additive
`GameConfig` field (T007).

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature (constitution Principle IV) — the
event→pattern mapping, every guard (settings/capability/pause/accessibility), and the debounce
timer are pure C# with zero engine dependency, so all of them MUST be covered by an EditMode test.
The native `UIFeedbackGenerator` bridge itself (T024) is the Principle IV *exception* (native code,
no engine-testable surface) and is validated instead by the manual on-device task (T028).

**Organization**: `spec.md` defines a single user story (US1). Tasks are still split into the
smallest independently-completable units — by responsibility (mapping, guards, debounce,
platform adapters, dispatcher) — rather than left as one giant task. File paths are exact.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Haptics/`, `Assets/Scripts/MonoBehaviours/Haptics/`,
  `Assets/Plugins/iOS/`, and `Assets/Tests/EditMode/Haptics/` exist; create any that are missing.
  No code in this task.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The data shapes every test and implementation task depends on.

- [ ] T002 [P] Define `HapticEventType` enum (`PlayerCaught`, `BatteryCritical`,
  `BatteryDepleted`, `ItemPickup`) in `Assets/Scripts/Systems/Haptics/HapticEventType.cs` — the
  four mandatory events named in `research.md`'s decision (FR-001).
- [ ] T003 [P] Define `HapticImpactStyle` enum (`Light`, `Heavy`, `Rigid`) and
  `HapticNotificationStyle` enum (`Warning`, `Error`) in
  `Assets/Scripts/Systems/Haptics/HapticPatternStyles.cs` — the exact `UIFeedbackGenerator` style
  vocabulary from `research.md`.
- [ ] T004 [P] Define `HapticPattern` readonly struct in
  `Assets/Scripts/Systems/Haptics/HapticPattern.cs` wrapping either one `HapticImpactStyle` or one
  `HapticNotificationStyle` (a `Kind` discriminator plus the two style fields, only one of which is
  meaningful per `Kind`) — the value type `HapticPatternCatalog` (T005) returns and
  `IHapticPlatformAdapter` (T006) consumes.
- [ ] T005 Define centralized `HapticPatternCatalog` static class in
  `Assets/Scripts/Systems/Haptics/HapticPatternCatalog.cs` mapping every `HapticEventType` to its
  `HapticPattern`: `PlayerCaught` → Impact/Heavy, `ItemPickup` → Impact/Light, `BatteryCritical` →
  Notification/Warning, `BatteryDepleted` → Notification/Error (FR-001; document in an XML comment
  that `research.md` lists Heavy/Rigid as the two acceptable intensities for `PlayerCaught` and
  this catalog is the single place that pick is recorded).
- [ ] T006 [P] Define `IHapticPlatformAdapter` interface (`void Play(HapticPattern pattern)`) in
  `Assets/Scripts/Systems/Haptics/IHapticPlatformAdapter.cs` — the seam between the pure-C#
  controller and any platform-specific call.
- [ ] T007 Add one additive `float hapticCooldownSeconds` field to the existing `GameConfig`
  `ScriptableObject` (`Assets/Scripts/Systems/Config/GameConfig.cs`, owned by
  `shared-config-and-state/001-game-config-schema`), with an initial default (e.g. `0.5f`)
  explicitly flagged in an XML/inline comment as a feel value needing on-device re-confirmation per
  `research.md`'s "Non-decisions" section (FR-003).
- [ ] T008 [P] Define `HapticGuardContext` readonly struct (`bool HapticsSettingEnabled`,
  `bool DeviceSupportsHaptics`, `bool IsPaused`, `bool AccessibilityReduceHapticsEnabled`) in
  `Assets/Scripts/Systems/Haptics/HapticGuardContext.cs` (FR-002).
- [ ] T009 Scaffold `HapticFeedbackController` plain C# class (no `MonoBehaviour`, no engine
  dependency) in `Assets/Scripts/Systems/Haptics/HapticFeedbackController.cs`: constructor takes
  `IHapticPlatformAdapter adapter` and `GameConfig config`; public no-op stub
  `bool TryTrigger(HapticEventType eventType, HapticGuardContext guardContext,
  float currentTimeSeconds)` that currently always returns `false` (FR-002, FR-003).

**Checkpoint**: Data shapes and `GameConfig` field compile; no guard/debounce/dispatch behavior
yet.

---

## Phase 3: User Story 1 — Feel important events (P2) 🎯 MVP

**Goal**: Every supported event maps to exactly one documented pattern (SC-001); disabled setting,
unsupported device, paused, or reduced-accessibility fixtures produce zero haptic calls (SC-002);
repeated source events are debounced per event type and never block gameplay (FR-003).

**Independent Test**: See `spec.md` User Story 1 — emit each event with haptics enabled/disabled
and verify one mapped pulse or no pulse.

### Tests for User Story 1

- [ ] T010 [P] [US1] EditMode test "Each of the four HapticEventType values resolves to its
  documented HapticPattern via HapticPatternCatalog" in
  `Assets/Tests/EditMode/Haptics/HapticPatternCatalogTests.cs` (SC-001).
- [ ] T011 [P] [US1] EditMode test "TryTrigger returns false and the adapter's Play is never
  called when HapticsSettingEnabled = false" in
  `Assets/Tests/EditMode/Haptics/HapticFeedbackControllerTests.cs`, using a
  `FakeHapticPlatformAdapter` test double that records every `Play` call (SC-002).
- [ ] T012 [P] [US1] EditMode test "TryTrigger returns false when DeviceSupportsHaptics = false"
  (same file, SC-002, edge case "unsupported device").
- [ ] T013 [P] [US1] EditMode test "TryTrigger returns false when IsPaused = true" (same file,
  edge case "app backgrounding" modeled as a paused guard).
- [ ] T014 [P] [US1] EditMode test "TryTrigger returns false when
  AccessibilityReduceHapticsEnabled = true" (same file, FR-002).
- [ ] T015 [P] [US1] EditMode test "TryTrigger returns true and calls adapter.Play exactly once
  with the HapticPatternCatalog-mapped pattern when every guard passes" (same file, SC-001).
- [ ] T016 [P] [US1] EditMode test "A second TryTrigger for the same HapticEventType within
  hapticCooldownSeconds of the first is debounced — adapter.Play is not called a second time" in
  `Assets/Tests/EditMode/Haptics/HapticFeedbackController_DebounceTests.cs` (FR-003, edge case
  "duplicate event").
- [ ] T017 [P] [US1] EditMode test "A second TryTrigger for the same HapticEventType after
  hapticCooldownSeconds has elapsed fires again" (same file, FR-003).
- [ ] T018 [P] [US1] EditMode test "Debounce cooldown is tracked independently per
  HapticEventType — triggering PlayerCaught does not suppress an immediately-following
  BatteryCritical" (same file, FR-003).
- [ ] T019 [P] [US1] EditMode test "TryTrigger never throws and returns synchronously even when
  every guard fails at once" (same file, FR-003's "never block gameplay").

### Implementation for User Story 1

- [ ] T020 [US1] Implement the guard short-circuit in `HapticFeedbackController.TryTrigger`:
  return `false` immediately, without calling the adapter, when any of
  `HapticsSettingEnabled = false`, `DeviceSupportsHaptics = false`, `IsPaused = true`, or
  `AccessibilityReduceHapticsEnabled = true` (FR-002).
- [ ] T021 [US1] Implement per-event debounce in `HapticFeedbackController` using a
  `Dictionary<HapticEventType, float>` of last-trigger timestamps compared against
  `config.hapticCooldownSeconds` and the passed-in `currentTimeSeconds` (FR-003).
- [ ] T022 [US1] Implement the pass-through call: when all guards pass and the debounce window has
  elapsed, look up the pattern via `HapticPatternCatalog`, call `adapter.Play(pattern)`, update the
  event's last-trigger timestamp, and return `true` (FR-001, SC-001).
- [ ] T023 [US1] Implement `HandheldVibrateFallbackAdapter : IHapticPlatformAdapter` in
  `Assets/Scripts/Systems/Haptics/HandheldVibrateFallbackAdapter.cs` whose `Play` calls
  `UnityEngine.Handheld.Vibrate()` regardless of the pattern argument — the automatic fallback
  `research.md` specifies for the Unity Editor and any non-iOS runtime.
- [ ] T024 [US1] Implement the native iOS bridge, `Assets/Plugins/iOS/LILOHaptics.mm`: an
  Objective-C file exposing four `extern "C"` functions that construct and fire
  `UIImpactFeedbackGenerator` (`.heavy` for the `PlayerCaught` function, `.light` for the
  `ItemPickup` function) and `UINotificationFeedbackGenerator` (`.warning` for `BatteryCritical`,
  `.error` for `BatteryDepleted`) — per `research.md`'s Decision section.
- [ ] T025 [US1] Implement `NativeIOSHapticAdapter : IHapticPlatformAdapter` in
  `Assets/Scripts/Systems/Haptics/NativeIOSHapticAdapter.cs`: `[DllImport("__Internal")]` extern
  declarations for T024's four functions, compiled only under `#if UNITY_IOS && !UNITY_EDITOR`;
  `Play` dispatches on `pattern.Kind`/style to the matching extern call.
- [ ] T026 [US1] Implement `HapticAdapterFactory` in
  `Assets/Scripts/Systems/Haptics/HapticAdapterFactory.cs`: a single static method returning a new
  `NativeIOSHapticAdapter` when running on-device iOS, and a `HandheldVibrateFallbackAdapter`
  otherwise (Editor, or any hypothetical non-iOS runtime) — per `research.md`'s Decision section.
- [ ] T027 [US1] Implement the thin `HapticFeedbackDispatcher` `MonoBehaviour` in
  `Assets/Scripts/MonoBehaviours/Haptics/HapticFeedbackDispatcher.cs`: builds one
  `HapticFeedbackController` from `HapticAdapterFactory`'s adapter and the scene's `GameConfig`;
  exposes a public `Trigger(HapticEventType eventType)` that other systems' `MonoBehaviour`s call,
  assembling `HapticGuardContext` from the live settings/device-capability/pause/accessibility
  state and `Time.time` as `currentTimeSeconds` on each call — contains no gameplay/guard logic of
  its own (Principle III).

**Checkpoint**: All four events are mapped, guarded, debounced, and dispatched through the correct
platform path; feature is functionally complete pending on-device confirmation.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T028 [P] Manual on-device verification (per constitution Principle IV's live-scene
  exception, recorded here since this feature has no `quickstart.md`): on a supported iPhone,
  confirm `PlayerCaught`, `ItemPickup`, `BatteryCritical`, and `BatteryDepleted` each produce a
  distinct, felt pulse; confirm zero pulse with the in-game haptics setting off, with System
  Haptics off in iOS Settings, and in Low Power Mode (SC-001, SC-002, edge cases "unsupported
  device" and "low-power mode").
- [ ] T029 [P] XML-doc comments on all public members of `HapticEventType`, `HapticPattern`,
  `HapticPatternCatalog`, `HapticGuardContext`, `IHapticPlatformAdapter`, and
  `HapticFeedbackController`, since source systems (interaction, battery, catch) integrate against
  this public surface without owning its semantics (Scope).
- [ ] T030 Code review pass: confirm `HapticFeedbackController.cs`, `HapticPatternCatalog.cs`, and
  `HapticGuardContext.cs` contain no `MonoBehaviour`/`DllImport`/native reference anywhere
  (Principle III pure-C# testability).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks all US1 work; Phase 2's `GameConfig`
  field addition (T007) depends on `shared-config-and-state/001-game-config-schema` having created
  the base `GameConfig` class.
- **US1 tests (T010–T019)** should be written and failing before **US1 implementation
  (T020–T027)**, per Principle IV.
- Within implementation: T020–T022 (controller logic) can proceed independently of T023–T026
  (adapters), but T027 (dispatcher) depends on both — it needs a working controller and a factory
  that returns a working adapter.
- **Phase 4 (Polish)** depends on everything above; T028 additionally depends on a native iOS
  build existing (T024–T026), since it exercises the real device path.

## Notes

- [P] tasks touch different files (or are read-only additions like doc comments) and can be
  parallelized.
- Every implementation task cites the exact `spec.md` FR/SC or `research.md` decision it
  satisfies — keep that traceability when this file is updated.
- Per constitution Principle IV, do not mark any Phase 3 implementation task "done" until its
  EditMode tests are written, failing first, then passing; T024's native `.mm` file has no
  EditMode surface and is validated by T028 instead.
- This feature does not build `MonsterProximityPulse` (`research.md`'s "User Story 4" /
  heartbeat-pulse stretch goal) — it is not in `spec.md`'s current scope, and per Principle II
  (YAGNI) is deferred to a future amendment rather than built ahead of need. The native bridge
  (T024) is structured so that stretch goal can extend it later without disturbing these tasks.
- This feature does not redefine the semantics of `PlayerCaught`, `BatteryCritical`,
  `BatteryDepleted`, or `ItemPickup` themselves — source systems own when those events fire; this
  file only covers the event→pattern mapping and dispatch contract (Scope).
