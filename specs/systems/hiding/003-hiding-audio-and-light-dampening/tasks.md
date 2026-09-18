---

description: "Task list for Hiding Audio and Light Dampening"
---

# Tasks: Hiding Audio and Light Dampening

**Input**: Design documents from
`specs/systems/hiding/003-hiding-audio-and-light-dampening/`

**Prerequisites**: `spec.md` (this folder; source of truth, not modified by this task list).
Depends on `hiding/001-enter-and-exit-hiding` (owns `HidingState` and its `StateChanged` event —
this feature's sole trigger) and `audio/002-dynamic-mix-state-machine` (owns the reversible
`Hiding` mix state this feature enters/exits). Integrates with, but never modifies,
`noise-and-detection/001-per-action-noise-emission`'s `NoiseEmitter.SetHiding` and
`flashlight-and-battery/003-eased-radius-transitions`'s `FlashlightRadiusController` /
`flashlight-and-battery/001-light-state-thresholds-and-radius`'s `LightStateSystem`/`Battery`.

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature's pure-logic pieces (constitution
Principle IV) — the dampening-value computation and the apply/clear-exactly-once guard are plain
C#. The `MonoBehaviour` adapter's actual `Light`/mix-system wiring is validated through a
`quickstart.md` manual scenario (Principle IV exception — needs a live scene/audio device).

**Organization**: This spec has a single user story (US1); tasks are grouped by Setup →
Foundational → US1 → Edge Cases → Polish, matching the granularity of
`specs/systems/monster-ai/002-per-floor-tuning-profile/tasks.md`.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: `US1` for every task in Phase 3; unlabeled for Setup/Foundational/Edge
  Cases/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Hiding/`
- `MonoBehaviour` adapter: `Assets/Scripts/MonoBehaviours/Hiding/`
- EditMode tests: `Assets/Tests/EditMode/Hiding/`
- Shared config asset (owned by `shared-config-and-state/001-game-config-schema`, this feature
  only adds fields): `Assets/Scripts/Config/GameConfig.cs`
- Reused type/event (owned by `hiding/001-enter-and-exit-hiding`, not this feature):
  `Assets/Scripts/Systems/Hiding/HidingState.cs`, `HidingSystem.StateChanged`
- Reused system (owned by `noise-and-detection/001-per-action-noise-emission`, not this feature,
  never modified): `Assets/Scripts/Systems/Noise/NoiseEmitter.cs`
- Reused system (owned by `audio/002-dynamic-mix-state-machine`, not this feature, never
  modified): `Assets/Scripts/Systems/Audio/DynamicMixSystem.cs`
- Reused systems this feature MUST NOT alter (owned by `flashlight-and-battery/001` and `/003`):
  `Assets/Scripts/Systems/Flashlight/LightStateSystem.cs`, `Battery.cs`,
  `Assets/Scripts/MonoBehaviours/Flashlight/FlashlightRadiusController.cs`

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Hiding/`, `Assets/Scripts/MonoBehaviours/Hiding/`, and
  `Assets/Tests/EditMode/Hiding/` exist (created by `hiding/001`) — this feature adds files
  alongside them; no new top-level folders needed.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The config field and pure value type every part of US1 depends on.

**⚠️ CRITICAL**: T002–T004 block every task in Phase 3 onward.

- [ ] T002 [P] Confirm `HidingState`/`HidingSystem.StateChanged` already exist (owned by
  `hiding/001`) — this feature's sole entry point is subscribing to that event; it does not poll
  or duplicate hiding-state tracking of its own.
- [ ] T003 Add `hidingLightVisualScale` (float, `[0,1]`, unlocked feel-based placeholder — no
  GDD-sourced number, same convention as `noisePulseDuration` in `noise-and-detection/001`) to
  `Assets/Scripts/Config/GameConfig.cs` (FR-001) — the one new tunable this feature introduces;
  noise dampening reuses `noise-and-detection/001`'s existing `noiseHiding = 0` override and needs
  no new field, and audio dampening reuses `audio/002`'s existing `Hiding` mix state.
- [ ] T004 Create `Assets/Scripts/Systems/Hiding/HidingLightDampening.cs`: a static class with
  `public static float ApplyVisualScale(float realTargetRadius, float hidingLightVisualScale,
  bool isHidden)` returning `realTargetRadius * hidingLightVisualScale` when `isHidden`, else
  `realTargetRadius` unchanged — a pure function, no `Battery`/`LightState` read or write of its
  own (FR-002; depends on T003).

**Checkpoint**: The config field and the pure dampening-value function exist and are ready to be
driven by `hiding/001`'s state-changed event.

---

## Phase 3: User Story 1 — Hiding changes the player's presence (P2)

**Goal**: Entering `Hidden` dampens noise, the flashlight's *rendered* presentation, and the
audio mix — as a reversible layer that never touches stored battery charge, light-state
thresholds, or inventory — and restores the exact pre-hide values on exit, applied/cleared
exactly once per transition.

**Independent Test**: See `spec.md` US1 — compare the same movement/light fixture before,
during, and after hiding.

### Tests for User Story 1 — Pure Computation ⚠️

> Write these first; confirm they fail (no dampening logic exists yet).

- [ ] T005 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingLightDampeningTests.cs`, write:
  `ApplyVisualScale` with `isHidden = false` returns `realTargetRadius` unchanged regardless of
  `hidingLightVisualScale`'s value.
- [ ] T006 [P] [US1] Add: `ApplyVisualScale` with `isHidden = true` returns exactly
  `realTargetRadius * hidingLightVisualScale` for varied scale values in `[0,1]` — proving the
  scale is read, not hardcoded (mirrors `MonsterTuningProfile`'s config-driven proof pattern).
- [ ] T007 [US1] Add: `ApplyVisualScale` never reads or mutates any `Battery`/`LightState` value —
  a structural check that its parameter list is exactly `(float, float, bool)` with no
  `Battery`/`LightState`-typed input (FR-002).

### Tests for User Story 1 — Apply/Clear Lifecycle ⚠️

- [ ] T008 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingPresentationModifierTests.cs`, write:
  a fresh modifier guard starts "not applied"; feeding it a simulated `Hidden` state-changed
  notification against a stub with call-counting hooks for "set noise hiding," "enter audio
  hiding mix," and "apply light scale" calls each hook exactly once (FR-003).
- [ ] T009 [P] [US1] Add: the following `Exiting`/`Visible` notification clears all three hooks
  ("clear noise hiding," "exit audio hiding mix," "restore light scale") exactly once each — never
  zero times, never twice.
- [ ] T010 [US1] Add: two consecutive `Hidden` notifications with no intervening `Visible` (a
  defensive/malformed sequence) apply the three hooks only once total, not twice — the guard flag
  prevents a double-layered modifier (FR-003's "exactly once" even under a re-fired event).
- [ ] T011 [US1] Add: the full before/during/after round trip — capture the pre-hide noise
  radius, flashlight visual scale, and audio mix state; drive `Hidden` then `Exiting`/`Visible`;
  assert the post-exit values are bit-for-bit identical to the pre-hide values (spec.md US1 third
  acceptance scenario, SC-001).
- [ ] T012 [US1] Add: throughout the entire round trip in T011, the underlying `Battery.
  ChargeSeconds` (or equivalent stored charge field) never changes by any amount — bit-for-bit
  identical before and after (SC-002).

### Implementation for User Story 1

- [ ] T013 [US1] Define `Assets/Scripts/Systems/Hiding/HidingPresentationModifier.cs`: a plain C#
  class holding a single `bool _isApplied` guard flag and three injected `Action`/`Action<bool>`
  hooks (noise, audio-mix, light-scale), exposing `OnHidingStateChanged(HidingState newState)` —
  applies all three hooks exactly once on the first transition into `Hidden`, and clears all three
  exactly once on the first transition out of `Hidden` (to `Exiting` or directly to `Visible` via
  `ForceExit`), guarded by `_isApplied` (depends on T004; makes T008–T010 pass).
- [ ] T014 [US1] Create `Assets/Scripts/MonoBehaviours/Hiding/HidingPresentationController.cs`: a
  thin `MonoBehaviour` adapter subscribing to `hiding/001`'s `HidingSystem.StateChanged`, holding
  live references to the player's `NoiseEmitter`, `audio/002`'s `DynamicMixSystem`, and
  `flashlight-and-battery/003`'s `FlashlightRadiusController`, and constructing a
  `HidingPresentationModifier` whose three hooks call, respectively: `NoiseEmitter.
  SetHiding(bool)`, `DynamicMixSystem`'s enter/exit-`Hiding`-mix-state operation, and
  `FlashlightRadiusController`'s rendered-radius override using `HidingLightDampening.
  ApplyVisualScale` (depends on T013).
- [ ] T015 [US1] Confirm (via T007's structural check plus a code-review pass) that no call path
  introduced by T013–T014 ever calls a `Battery`- or `LightState`-mutating method — the light
  dampening is applied strictly downstream of `LightStateSystem.GetTargetRadius`'s already-computed
  value, as a rendering-only override (FR-002).

**Checkpoint**: Before/during/after fixtures produce the expected noise, light-presentation, and
audio-mix outputs; stored battery state is provably untouched throughout.

---

## Phase 4: Edge Cases & Robustness

**Purpose**: Cover `spec.md`'s Edge Cases paragraph — pause, death, floor reset, low battery, and
interrupted transitions all clear temporary modifiers correctly.

- [ ] T016 [P] In `Assets/Tests/EditMode/Hiding/HidingPresentationEdgeCaseTests.cs`, write: a
  `Hidden` player who dies (death sequence starts per `lives-and-fail-state/003`, which calls
  `hiding/001`'s `HidingSystem.ForceExit()`) has all three modifiers cleared exactly once via the
  same `Exiting`/`Visible`-clearing path — no special-case death branch needed inside
  `HidingPresentationModifier` itself.
- [ ] T017 [P] Add: a floor reset (`lives-and-fail-state/002`) that forces `HidingState` back to
  `Visible` while modifiers were applied leaves no dampening applied afterward — the guard's
  `_isApplied` flag is itself reset alongside `HidingSystem`'s own state, so a fresh floor never
  inherits a stale dampened noise/light/audio value.
- [ ] T018 [P] Add: a low-battery fixture (charge fraction near 0%, `LightState.CompactDarkness`)
  entering `Hidden` still applies the visual scale on top of whatever `LightStateSystem.
  GetTargetRadius` already returned for that state — dampening composes with, and never
  overrides, the real light-state math (extends T006/T007's structural guarantee to the near-zero
  boundary).
- [ ] T019 [US1] Add: a transition interrupted before ever reaching `Hidden` (e.g., `Entering`
  cancelled by `ForceExit()` per `hiding/001`'s edge cases) never applies the modifier at all — the
  guard only triggers on an actual `Hidden` notification, never on `Entering`.
- [ ] T020 Wire `HidingPresentationController` (from T014) to also subscribe defensively to a
  pause/settings-menu-open signal from `game-shell-ui/001-pause-menu-and-time-freeze`, confirming
  (by test with a stub) that pausing while modifiers are applied leaves them applied and unchanged
  — dampening is a state-machine-driven layer, not a per-frame effect that pause needs to
  separately suspend.

**Checkpoint**: Death, floor reset, low battery, and interrupted transitions all leave the
dampening layer in a clean, non-leaking state.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T021 [P] Write `Assets/Tests/EditMode/Hiding/HidingPresentationSuccessCriteriaTests.cs`: a
  consolidated test asserting SC-001 (before/during/after fixtures produce expected noise and
  presentation outputs) and SC-002 (stored battery state is bit-for-bit unchanged) in one pass.
- [ ] T022 [P] Add XML doc comments to `HidingLightDampening` and `HidingPresentationModifier`'s
  public members citing GDD Ch. 4.3/5.2/14.2 and this spec's FR-001–FR-003, noting explicitly that
  `hidingLightVisualScale` is a feel-based placeholder pending on-device tuning, same caveat class
  as `noise-and-detection/001`'s `noiseBaseRadius`.
- [ ] T023 Write a `quickstart.md` manual scenario (device/Play Mode) covering the Principle IV
  exception: entering a real hiding-spot prefab and confirming the flashlight visibly dims and the
  audio mix audibly shifts, then both restore exactly on exit.
- [ ] T024 Run the full `Assets/Tests/EditMode/Hiding/` suite and confirm 100% green before
  marking this feature done.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks every task below; Phase 2 also depends
  on `hiding/001` (for `HidingState`/`StateChanged`) already existing.
- **US1 (Phase 3)**: Pure Computation (T005–T007, T004) has no dependency on the Apply/Clear
  Lifecycle sub-group; Apply/Clear Lifecycle (T008–T012, T013) depends on T004's pure function
  existing to wrap; the `MonoBehaviour` wiring (T014–T015) depends on T013.
- **Edge Cases (Phase 4)** depends on US1's full modifier surface (T013–T014) existing.
- **Polish (Phase 5)** depends on everything above.

## Parallel Opportunities

- T005–T007 (pure-function tests) and T008–T010 (lifecycle-guard tests) can be drafted in
  parallel — they exercise different classes (T004 vs. T013).
- All test-writing tasks marked [P] within a phase can run in parallel with each other.
- T016–T018 (edge-case tests) are independent of each other and can be staffed in parallel once
  T013–T014 land.

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another unfinished task in this list.
- This feature never adds a `noise`-side config field of its own (T003's note) — reusing
  `noise-and-detection/001`'s existing unconditional `noiseHiding = 0` override is a deliberate
  YAGNI choice (constitution Principle II), not an oversight; do not add a second noise-dampening
  number "to be safe."
- Per constitution Principle IV, do not mark any Phase 3–4 task "done" until its EditMode tests
  are written, failing first, then passing. The `MonoBehaviour` adapter's live `Light`/audio-device
  wiring is the one Principle IV exception in this feature and is covered by T023's manual
  scenario instead.
