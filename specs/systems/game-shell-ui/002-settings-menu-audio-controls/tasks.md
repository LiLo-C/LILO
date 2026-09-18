---
description: "Task list for Settings Menu Audio Controls"
---

# Tasks: Settings Menu Audio Controls

**Input**: Design documents from
`specs/systems/game-shell-ui/002-settings-menu-audio-controls/spec.md`

**Prerequisites**: `spec.md` (this feature). Reachable from
`specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/spec.md`'s Settings button (not
redefined here). Applies to (but does not own) the named buses driven by
`specs/systems/audio/002-dynamic-mix-state-machine/spec.md` and the haptic preference consumed by
`specs/systems/haptics/001-haptic-trigger-events/spec.md` (FR-002).

**Tests**: Included per constitution Principle IV — every pure-logic piece listed below MUST have
an EditMode test.

**Organization**: `spec.md` defines a single user story (US1). Tasks are still split into
Setup/Foundational/Story/Polish phases so each unit stays small and independently completable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Maps the task to `spec.md`'s user story (US1)

## Phase 1: Setup

- [ ] T001 [P] Define the `AudioSettings` value type (`MasterVolume`, `MusicVolume`,
  `AmbienceVolume`, `EffectsVolume`: `float`, `0`–`1`; `HapticsEnabled`: `bool`) in
  `Assets/Scripts/Systems/Settings/AudioSettings.cs`. This is a third, user-preference-only data
  class — distinct from `GameState` (run data) and `GameConfig` (design tuning) — so changes here
  can never mutate gameplay state (FR-002).
- [ ] T002 [P] Define the `SettingsValidationResult` enum (`Valid`, `ClampedOutOfRange`,
  `CorruptFallbackToDefault`) in
  `Assets/Scripts/Systems/Settings/SettingsValidationResult.cs` (covers Edge Cases "corrupt
  preference" and "unsupported control").
- [ ] T003 [P] Define the `ISettingsStore` interface (`AudioSettings Load()`,
  `void Save(AudioSettings settings)`) in `Assets/Scripts/Systems/Settings/ISettingsStore.cs` —
  abstracts persistence so pure-logic tests never touch real device storage.

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T004 Define the `AudioSettingsSystem` plain C# static class with method stubs
  `Clamp(AudioSettings raw) : AudioSettings`, `ApplyDefault() : AudioSettings`, and
  `TryParse(string raw, out AudioSettings settings, out SettingsValidationResult result) : bool`
  in `Assets/Scripts/Systems/Settings/AudioSettingsSystem.cs`. No `MonoBehaviour`/`Component`/
  scene dependency (constitution Principle III).
- [ ] T005 Implement `InMemorySettingsStore : ISettingsStore` (an `ISettingsStore` fake used only
  by EditMode tests, no real persistence) in
  `Assets/Tests/EditMode/Systems/Settings/InMemorySettingsStore.cs`.

**Checkpoint**: `AudioSettingsSystem` compiles and is ready for story-specific branches and tests.

---

## Phase 3: User Story 1 - Control audio comfortably (P2) 🎯 MVP

**Goal**: The player can adjust master, music, ambience, and effects volume and mute haptics;
settings persist across scenes and runs.

**Independent Test**: Edit each control, reload a scene, and verify the corresponding output and
saved value.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test: `Clamp` bounds each of the four volume fields to `[0, 1]`
  (values below `0` clamp to `0`, above `1` clamp to `1`) and leaves `HapticsEnabled` untouched,
  in `Assets/Tests/EditMode/Systems/Settings/AudioSettingsSystemClampTests.cs`.
- [ ] T007 [P] [US1] EditMode test: `ApplyDefault` returns the GDD-locked default `AudioSettings`
  values (covers "documented defaults" for the corrupt/unsupported/restart edge cases), same
  file.
- [ ] T008 [P] [US1] EditMode test: `TryParse` returns `true`/`Valid` and the parsed
  `AudioSettings` for a well-formed serialized settings string, in
  `Assets/Tests/EditMode/Systems/Settings/AudioSettingsSystemParseTests.cs`.
- [ ] T009 [P] [US1] EditMode test: `TryParse` returns `CorruptFallbackToDefault` and
  `ApplyDefault()`'s values for malformed/truncated input (covers Edge Case "corrupt
  preference"), same file.
- [ ] T010 [P] [US1] EditMode test: `TryParse` returns `ClampedOutOfRange` and a clamped (not
  rejected) value for a well-formed but out-of-range value (covers Edge Case "unsupported
  control" as an out-of-range value, not a crash), same file.
- [ ] T011 [P] [US1] EditMode test: `Save` then `Load` through the same `ISettingsStore` returns
  bit-for-bit identical `AudioSettings` (round-trip, SC-002), in
  `Assets/Tests/EditMode/Systems/Settings/SettingsRoundTripTests.cs`, using
  `InMemorySettingsStore` (T005).
- [ ] T012 [P] [US1] EditMode test: changing one field of `AudioSettings` (e.g. `MusicVolume`)
  and saving/loading leaves every other field unchanged (SC-001's "each control changes only its
  intended bus," expressed at the data level), same file.
- [ ] T013 [P] [US1] EditMode test: a rapid sequence of `Clamp` calls simulating slider drag
  (many values within one frame) is a pure, order-independent function of the last value only —
  no accumulation or drift (covers Edge Case "rapid slider input"), in
  `Assets/Tests/EditMode/Systems/Settings/AudioSettingsSystemRapidInputTests.cs`.

### Implementation for User Story 1

- [ ] T014 [US1] Implement the `Clamp`/`ApplyDefault`/`TryParse` bodies in
  `AudioSettingsSystem.cs` (extends T004; makes T006–T010 and T013 pass).
- [ ] T015 [US1] Implement `PlayerPrefsSettingsStore : ISettingsStore` (production adapter that
  serializes `AudioSettings` to `PlayerPrefs` keys, no validation logic duplicated from
  `AudioSettingsSystem` per constitution Principle III) in
  `Assets/Scripts/MonoBehaviours/Settings/PlayerPrefsSettingsStore.cs`.
- [ ] T016 [US1] Implement `SettingsMenuView` UI (Master/Music/Ambience/Effects sliders, one
  Haptics toggle, one Reset to Defaults button, each with an accessible label per FR-003) in
  `Assets/Scripts/UI/SettingsMenuView.cs`.
- [ ] T017 [US1] Implement `SettingsMenuController` `MonoBehaviour` that loads `AudioSettings` via
  `ISettingsStore` on open, applies each slider/toggle change immediately through
  `AudioSettingsSystem.Clamp` to the corresponding Unity `AudioMixerGroup` exposed parameter or
  haptics flag, and calls `ISettingsStore.Save` on each committed change, in
  `Assets/Scripts/UI/SettingsMenuController.cs`.
- [ ] T018 [US1] Wire the Reset to Defaults button through `AudioSettingsSystem.ApplyDefault` +
  `ISettingsStore.Save`, refreshing all four sliders and the toggle in `SettingsMenuView` in one
  pass (FR-003), in `Assets/Scripts/UI/SettingsMenuController.cs` (extends T017).
- [ ] T019 [US1] Confirm `SettingsMenuController` never reads or writes any `GameState`/
  `GameConfig` field (FR-002's "not mutate gameplay state") — it only touches
  `ISettingsStore`/`AudioSettingsSystem` and the audio mixer's own exposed parameters, in
  `Assets/Scripts/UI/SettingsMenuController.cs`.

**Checkpoint**: Every control changes only its intended bus, persists, and resets to documented
defaults; User Story 1 is independently demonstrable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T020 [P] EditMode test: loading settings when no saved preference exists yet (first launch)
  returns `ApplyDefault()`'s values without throwing, in
  `Assets/Tests/EditMode/Systems/Settings/AudioSettingsSystemFirstLaunchTests.cs`.
- [ ] T021 Manual device pass: edit each control, reload a scene, and verify the corresponding
  audio output changed and the saved value round-tripped (spec Independent Test); force-quit and
  relaunch to confirm the "app restart" edge case falls back to documented defaults only when the
  preference file is actually missing/corrupt, not on every restart.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks the user story.
- **User Story 1 (Phase 3)** depends only on Foundational.
- **Polish (Phase 4)** depends on User Story 1 being complete.

## Notes

- [P] tasks touch different files (or different, independent test methods) and have no ordering
  dependency between them.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- This feature does not implement the audio mix state machine or haptic pattern dispatch — it
  only owns the persisted preference values and the UI that edits them, per the spec's Scope.
- Commit after each task or logical group.
