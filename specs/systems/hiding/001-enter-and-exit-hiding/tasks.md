---

description: "Task list for Enter and Exit Hiding"
---

# Tasks: Enter and Exit Hiding

**Input**: Design documents from `specs/systems/hiding/001-enter-and-exit-hiding/`

**Prerequisites**: `spec.md` (this folder; source of truth, not modified by this task list).
Depends on `movement-and-camera/001-joystick-movement-and-sprint` (movement suppression target)
and `interaction-and-highlight/002-context-sensitive-action-button` (the action contract a hiding
spot must satisfy to surface "Hide"/"Exit" as the context action). Feeds
`hiding/002-hiding-detection-immunity-rule` (the canonical `HidingState.Hidden` value) and
`hiding/003-hiding-audio-and-light-dampening` (the state-changed event).

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature's pure-logic pieces (constitution
Principle IV) — the state machine, anchor placement math, and event-emission contract are all
plain C# and MUST be covered. Collision/navigability validation against real scene geometry is the
Principle IV exception (needs a live scene) and is validated instead through a `quickstart.md`
manual scenario on target hardware.

**Organization**: This spec has a single user story (US1); tasks are grouped by Setup →
Foundational → US1 (split into Entry / Transition-Timing / Exit / Event-Emission sub-groups) →
Edge Cases → Polish, matching the granularity of
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
- Reused type (owned by `movement-and-camera/001`, not this feature):
  `Assets/Scripts/MonoBehaviours/Player/PlayerMovementController.cs`
- Reused contract (owned by `interaction-and-highlight/002`, not this feature): the action
  contract under `Assets/Scripts/Systems/Interaction/`

---

## Phase 1: Setup

- [x] T001 Create the `Assets/Scripts/Systems/Hiding/`, `Assets/Scripts/MonoBehaviours/Hiding/`,
  and `Assets/Tests/EditMode/Hiding/` directories (via a placeholder file such as the first
  class/test below — Unity does not version empty folders).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The state enum, data shapes, and config field every part of US1 depends on.

**⚠️ CRITICAL**: T002–T006 block every task in Phase 3 onward.

- [x] T002 [P] Define `enum HidingState { Visible, Entering, Hidden, Exiting }` in
  `Assets/Scripts/Systems/Hiding/HidingState.cs` — no `UnityEngine` dependency beyond the enum
  itself. `Entering`/`Exiting` are the transient states `hiding/002` and `hiding/003` key their
  own rules off of (FR-001); this feature is the sole owner of this enum.
- [x] T003 [P] Define the readonly struct `HidingSpotData { Vector3 HideAnchor; Vector3
  ExitAnchor; bool IsOccupied; }` in `Assets/Scripts/Systems/Hiding/HidingSpotData.cs` (FR-001,
  FR-003) — plain data only; collision/navigability validity of the two anchors is supplied by
  the caller (see T004), not computed here.
- [x] T004 [P] Define `HidingTransitionResult { bool Success; string FailureReason; }` in
  `Assets/Scripts/Systems/Hiding/HidingTransitionResult.cs` — the explicit success/failure shape
  for "no valid spot or a blocked exit → the action is unavailable and state does not corrupt"
  (spec.md US1 third acceptance scenario).
- [x] T005 Add `hidingTransitionDuration` (float, seconds, unlocked feel-based placeholder — no
  GDD-sourced number, same convention as `noisePulseDuration` in
  `noise-and-detection/001-per-action-noise-emission`) to `Assets/Scripts/Config/GameConfig.cs`.
  This is the duration `Entering`/`Exiting` are held before auto-resolving to `Hidden`/`Visible`
  (FR-001's "explicit state transitions").
- [x] T006 Create `Assets/Scripts/Systems/Hiding/HidingSystem.cs`: a plain C# class (no
  `MonoBehaviour`/`Component`/scene-graph dependency, constitution Principle III) constructed
  with a `GameConfig` reference, exposing `HidingState CurrentState` starting at `Visible` and a
  no-op `Tick(float deltaSeconds)` — no transition logic yet (depends on T002, T005).

**Checkpoint**: The state shape, data contracts, and config field exist; `HidingSystem` starts
correctly but does nothing yet.

---

## Phase 3: User Story 1 — Hide under a desk (P1) 🎯 MVP

**Goal**: Explicit, validated enter/exit transitions with one active hiding spot, movement
suppressed while not `Visible`, deterministic anchor placement, and events fired for the audio/
light/detection consumers named in FR-004.

**Independent Test**: See `spec.md` US1 — valid spot, invalid distance, and blocked exit
fixtures; assert transitions and player placement.

### Tests for User Story 1 — Entry ⚠️

> Write these first; confirm they fail (no comparison logic in `HidingSystem` yet).

- [x] T007 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingSystemEnterTests.cs`, write: `TryEnter`
  with a valid, unoccupied spot and valid anchors from `Visible` → returns success, `CurrentState`
  becomes `Entering` immediately (FR-001).
- [x] T008 [P] [US1] Add: `TryEnter` with no spot supplied (null/default) → returns failure with a
  named reason, `CurrentState` remains `Visible` (spec.md US1 Scenario 3, "no valid spot").
- [x] T009 [P] [US1] Add: `TryEnter` with a spot whose `IsOccupied` is already `true` → returns
  failure, `CurrentState` remains unchanged — "one active hiding spot" (FR-001) is never violated.
- [x] T010 [P] [US1] Add: `TryEnter` called a second time while already `Entering`/`Hidden` →
  returns failure (no re-entry, no double-transition), `CurrentState` unaffected.

### Tests for User Story 1 — Transition Timing ⚠️

- [x] T011 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingSystemTransitionTimingTests.cs`, write:
  after a successful `TryEnter`, `Tick` calls totalling less than `hidingTransitionDuration` keep
  `CurrentState` at `Entering`.
- [x] T012 [P] [US1] Add: once accumulated `Tick` time reaches `hidingTransitionDuration`,
  `CurrentState` becomes `Hidden` on that tick, and the player is placed at `HideAnchor` at that
  exact moment (SC-001, deterministic anchor).
- [x] T013 [US1] Add the symmetric case for `Exiting` → `Visible` at `ExitAnchor` after the same
  configured duration (depends on T011–T012 sharing the fixture pattern).

### Tests for User Story 1 — Exit ⚠️

- [x] T014 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingSystemExitTests.cs`, write: `TryExit`
  while `Hidden` with a valid exit anchor → returns success, `CurrentState` becomes `Exiting`
  immediately.
- [x] T015 [P] [US1] Add: `TryExit` while `Visible`, `Entering`, or `Exiting` → returns failure
  (exit is only legal from `Hidden`), `CurrentState` unaffected.
- [x] T016 [P] [US1] Add: `TryExit` with a blocked exit anchor (caller-supplied `anchorValid =
  false`) → returns failure, `CurrentState` remains `Hidden` — "blocked exit" leaves state
  uncorrupted (spec.md US1 Scenario 3).
- [x] T017 [US1] Add: no movement input changes the player's tracked position while `CurrentState`
  is `Entering`, `Hidden`, or `Exiting` — drive a `PlayerMovementController`-shaped fixture with
  simulated movement input during each of the three non-`Visible` states and assert position is
  unchanged except for the one deterministic anchor placement from T012/T013 (SC-002).

### Tests for User Story 1 — Event Emission ⚠️

- [x] T018 [P] [US1] In `Assets/Tests/EditMode/Hiding/HidingSystemEventsTests.cs`, write: a
  subscriber to `HidingSystem`'s state-changed event receives exactly one notification per actual
  transition (`Visible→Entering→Hidden→Exiting→Visible`), with the new `HidingState` as payload
  (FR-004 — this is the seam `hiding/002`/`hiding/003`/`audio/002` subscribe to).
- [x] T019 [US1] Add: a failed `TryEnter`/`TryExit` (from T008–T010, T015–T016) fires zero
  state-changed notifications — only successful transitions emit events.

### Implementation for User Story 1

- [x] T020 [US1] Implement `HidingSystem.TryEnter(HidingSpotData spot, bool anchorsValid)`:
  validate `spot` is non-default, `!spot.IsOccupied`, `anchorsValid`, and `CurrentState ==
  Visible`; on success set `CurrentState = Entering`, mark the spot occupied, reset the transition
  timer, and return a success `HidingTransitionResult`; otherwise return a named-reason failure
  with no state change (depends on T003, T004, T006; makes T007–T010 pass).
- [x] T021 [US1] Implement `HidingSystem.Tick(float deltaSeconds)`: advance the transition timer
  while `Entering`/`Exiting`; on reaching `hidingTransitionDuration`, resolve `Entering → Hidden`
  (placing the player at `HideAnchor`) or `Exiting → Visible` (placing the player at `ExitAnchor`
  and clearing the spot's occupied flag) (depends on T020; makes T011–T013 pass).
- [x] T022 [US1] Implement `HidingSystem.TryExit(bool anchorsValid)`: validate `CurrentState ==
  Hidden` and `anchorsValid`; on success set `CurrentState = Exiting` and reset the transition
  timer; otherwise return a named-reason failure with no state change (depends on T021; makes
  T014–T016 pass).
- [x] T023 [US1] Add `event Action<HidingState> StateChanged` to `HidingSystem`, invoked exactly
  once from within `TryEnter`'s success path, `Tick`'s two resolution branches, and `TryExit`'s
  success path — never on a failure path (depends on T020–T022; makes T018–T019 pass).
- [x] T024 [US1] Create `Assets/Scripts/MonoBehaviours/Hiding/HidingController.cs`: a thin
  `MonoBehaviour` adapter owning the `HidingSystem` instance, performing the live collision/
  navigability check on a candidate spot's anchors (Principle IV exception — needs a live scene,
  validated manually per `quickstart.md`, not by EditMode test), calling `TryEnter`/`TryExit`, and
  applying the resulting anchor position to the `PlayerCharacter`'s `Transform` (FR-003; depends
  on T020–T022).
- [x] T025 [US1] Wire `HidingController` to suppress movement input and ordinary interaction
  while `CurrentState != Visible` (FR-002) — write the actual suppression through the single
  `GameState` object's hiding field (constitution Principle III: no second source of truth), read
  by `movement-and-camera/001`'s `PlayerMovementController` before applying any `MovementInput`.
- [x] T026 [US1] Wire `HidingController` to implement the action contract from
  `interaction-and-highlight/002-context-sensitive-action-button`
  (`Assets/Scripts/Systems/Interaction/`), surfacing "Hide" when a valid spot is nearest and
  `Visible`, and "Exit" when `Hidden`, and disabled/hidden otherwise — exact label wiring per that
  spec's action-contract shape (FR-002).

**Checkpoint**: Enter/exit/transition-timing/exit-blocking/event-emission are all implemented and
independently tested — US1 is fully functional.

---

## Phase 4: Edge Cases & Robustness

**Purpose**: Cover `spec.md`'s Edge Cases paragraph — spot destroyed, monster overlap, pause/
death during transition, and scene reset all resolve to a valid `Visible` or reset state.

- [x] T027 [P] In `Assets/Tests/EditMode/Hiding/HidingSystemEdgeCaseTests.cs`, write: the active
  spot becoming unavailable (e.g. destroyed) while `Entering`/`Hidden` does not leave
  `CurrentState` stuck — an external `ForceExit()`/`Reset()` path (see T028) returns it to a valid
  `Visible` state.
- [x] T028 [US1] Implement `HidingSystem.ForceExit()`: an unconditional escape hatch (used by the
  spot-destroyed, pause/death-interrupt, and scene-reset paths) that immediately sets
  `CurrentState = Visible`, clears the occupied flag on whatever spot was active, clears the
  transition timer, and fires `StateChanged` exactly once — never leaves a corrupted intermediate
  state (depends on T020–T023).
- [x] T029 [P] Add: calling `ForceExit()` while already `Visible` is a safe no-op (defensive reset
  called with nothing active) — mirrors `MonsterStateMachine.Reset()`'s "safe even when never
  frozen" guarantee in `monster-ai/001`.
- [x] T030 [P] Add: `ForceExit()` called mid-transition (`Entering` or `Exiting`, timer partially
  elapsed) resolves to `Visible` immediately, not to whatever the timer would have produced —
  proves pause/death interrupting a transition never leaves an ambiguous state.
- [x] T031 [US1] Wire `HidingController` to call `HidingSystem.ForceExit()` on the death-sequence
  start signal (`lives-and-fail-state/003-death-sequence-and-outcome-branch`) and on floor/scene
  reset (`lives-and-fail-state/002-floor-state-reset-on-death`), so a death or floor reset while
  hidden always resolves to `Visible` before the next floor/respawn begins.
- [x] T032 [P] Add: a monster occupying/overlapping the same world position as an already-`Hidden`
  player does not itself call any `HidingSystem` method — this feature has no monster-awareness of
  its own; confirm by inspection/test that `HidingSystem`'s public surface takes no monster-related
  parameter (the actual immunity rule is `hiding/002`'s job, not this feature's).

**Checkpoint**: Spot-destroyed, pause/death mid-transition, and scene/floor reset all resolve
deterministically to `Visible` with no corrupted intermediate state.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [x] T033 [P] Write `Assets/Tests/EditMode/Hiding/HidingSystemConsumerContractTests.cs`: attach
  three independent stub subscribers to `StateChanged` (representing the audio, light, and
  detection consumers named in FR-004) and confirm all three receive every transition identically
  — proves the event is a single fan-out, not three duplicated code paths.
- [x] T034 [P] Add XML doc comments to `HidingState`, `HidingSpotData`, `HidingTransitionResult`,
  and `HidingSystem`'s public members, citing GDD Ch. 4.3/15.4 and noting that `Entering`/`Exiting`
  are the states `hiding/002` and `hiding/003` key their own rules off of.
- [ ] T035 Write a `quickstart.md` manual scenario (device/Play Mode) covering the Principle IV
  exception: walking up to a real hiding-spot prefab, confirming the actual collision/navigability
  check in `HidingController` rejects an obstructed anchor that an EditMode test cannot exercise
  without a live scene.
- [x] T036 Run the full `Assets/Tests/EditMode/Hiding/` suite and confirm 100% green before marking
  this feature done.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks every user-story task below.
- **US1 (Phase 3)**: Entry (T007–T010, T020) has no dependency on Transition Timing/Exit/Events;
  Transition Timing (T011–T013, T021) depends on Entry's `TryEnter` existing; Exit (T014–T017,
  T022) depends on Transition Timing resolving to `Hidden` first; Event Emission (T018–T019, T023)
  depends on all three transition paths existing to instrument.
- **Edge Cases (Phase 4)** depends on US1's full transition surface (T020–T023) existing to add
  `ForceExit()` alongside.
- **Polish (Phase 5)** depends on everything above.

## Parallel Opportunities

- T002–T004 (enum/data types) can run in parallel; T005/T006 depend on T002/T005 respectively.
- All test-writing tasks marked [P] within a sub-group can run in parallel with each other.
- T024–T026 (the `MonoBehaviour` adapter's three responsibilities: live validation, movement
  suppression, action-contract wiring) touch the same file but distinct methods — sequence them
  but do not block other work on them.

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another unfinished task in this list.
- Every implementation task cites the exact FR/scenario it satisfies — keep that traceability when
  this file is updated.
- Per constitution Principle IV, do not mark any Phase 3–4 task "done" until its EditMode tests
  are written, failing first, then passing. Collision/navigability validation itself (inside
  `HidingController`) is the one Principle IV exception in this feature and is covered by T035's
  manual scenario instead.
