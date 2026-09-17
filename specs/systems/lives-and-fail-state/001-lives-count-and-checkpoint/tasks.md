---

description: "Task list for lives-and-fail-state/001-lives-count-and-checkpoint"

---

# Tasks: Lives Count and Checkpoint

**Input**: Design documents from `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: Included — constitution Principle IV (Test-Before-Done, NON-NEGOTIABLE) requires EditMode coverage for every pure-logic piece before a story counts as done. Both systems here are plain C# with no rendering/physics/input dependency, so both are fully EditMode-testable — there is no manual-scenario exception to invoke.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P1, P2).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project. Plain C# game-rule classes under `Assets/Scripts/Systems/` (no `MonoBehaviour`/scene dependency, per constitution Principle III). Shared state under `Assets/Scripts/State/`. Config under `Assets/Scripts/Config/`. Tests under `Assets/Tests/EditMode/`.

---

## Phase 1: Foundational (blocking prerequisites)

**⚠️ MUST complete before either user story below — both stories read/write the same `GameState` fields.**

- [ ] T001 Add `public int Lives` to `Assets/Scripts/State/GameState.cs` (default/uninitialized value documented as "set by `LivesSystem.ResetLivesForNewRun` before first read" — see T005)
- [ ] T002 Add a `public struct Checkpoint { public FloorId Floor; public Vector3 EntryPosition; }` type (new file `Assets/Scripts/State/Checkpoint.cs`, or nested in `GameState.cs` if a `FloorId` enum does not already exist elsewhere — check `specs/systems/progression-and-scene-flow/001-floor-numbering-and-splash-text` first since it may already define floor identity; reuse that enum instead of inventing a second one if so) and a `public Checkpoint CurrentCheckpoint` field on `GameState.cs`
- [ ] T003 [P] Confirm/add the `lives` (int, locked default `3`) and `checkpointPerFloor` (bool, locked default `true`) fields on `Assets/Scripts/Config/GameConfig.cs` per GDD 17.5 — these may already exist from `specs/systems/shared-config-and-state/001-game-config-schema`; only add if genuinely missing, never duplicate under a different key name
- [ ] T004 [P] Create `Assets/Scripts/Systems/LivesSystem.cs`: plain C# class with `int GetLives(GameState state)`, `bool HasLivesRemaining(GameState state)` (`Lives > 0`), `void DecrementLife(GameState state)` (floors at 0, never negative), and `void ResetLivesForNewRun(GameState state, GameConfig config)` (`Lives = config.lives`) — no `MonoBehaviour`/`Component` reference anywhere in this file (constitution Principle III)
- [ ] T005 [P] Create `Assets/Scripts/Systems/CheckpointSystem.cs`: plain C# class with `void SetCheckpointForFloor(GameState state, FloorId floor, Vector3 entryPosition)` (unconditionally overwrites `state.CurrentCheckpoint` — never appends/stacks) and `Checkpoint GetCurrentCheckpoint(GameState state)`

**Checkpoint**: `GameState` carries `Lives` and `CurrentCheckpoint`; both systems compile and are ready for the user-story tests below.

---

## Phase 2: User Story 1 - Lives Persist Across a Whole Run (Priority: P1)

**Goal**: A single `Lives` counter starts at `GameConfig.lives` (3) at run start and is untouched by floor transitions.

**Independent Test**: Simulate two decrements on a fixture floor, "transition" to a new floor (mutate unrelated `GameState` fields only), and assert `Lives` still reads 1.

- [ ] T006 [US1] Wire `GameManager` (existing `MonoBehaviour` from `shared-config-and-state/002`) to call `LivesSystem.ResetLivesForNewRun` exactly once, at new-run start (e.g., when the Prologue or Floor 52 scene begins a run) — explicitly NOT on every floor load
- [ ] T007 [P] [US1] Create `Assets/Tests/EditMode/LivesSystemTests.cs`:
  - assert `ResetLivesForNewRun` sets `Lives` to exactly `GameConfig.lives` (using a `GameConfig` fixture with `lives = 3`)
  - assert `DecrementLife` reduces `Lives` by exactly 1 per call, and floors at 0 (calling it again at 0 keeps it at 0, never -1)
  - assert `HasLivesRemaining` is `true` while `Lives > 0` and `false` exactly when `Lives == 0`
  - assert `Lives` is unaffected by mutating unrelated `GameState` fields (simulating a floor transition without calling `ResetLivesForNewRun` or `DecrementLife`)
  - assert `HasLivesRemaining` transitions to `false` on the very first `DecrementLife` call when `GameConfig.lives` is fixture-configured to `1` (the "very first catch ends lives" boundary from spec.md Edge Cases)

**Checkpoint**: User Story 1 is independently complete and tested — the run-level lives counter is correct in isolation.

---

## Phase 3: User Story 2 - Exactly One Checkpoint Per Floor (Priority: P1)

**Goal**: A single checkpoint value, written once per floor entry, immune to in-floor progress.

**Independent Test**: Set the checkpoint for Floor 51, simulate key pickup / door open / battery install by mutating those unrelated `GameState` fields, and assert the checkpoint is unchanged; then set it again for Floor 50 and assert it was overwritten, not appended.

- [ ] T008 [US2] Add an integration-note doc comment in `CheckpointSystem.cs` marking where the (not-yet-specified) floor-load adapter — `specs/systems/progression-and-scene-flow/002-scene-transition-manager` — must call `SetCheckpointForFloor` exactly once per floor entry, so that call site isn't silently forgotten once that spec is implemented
- [ ] T009 [P] [US2] Create `Assets/Tests/EditMode/CheckpointSystemTests.cs`:
  - assert `SetCheckpointForFloor` stores exactly the given floor + position
  - assert calling it a second time for a later floor overwrites (never appends/stacks) the stored checkpoint — `GetCurrentCheckpoint` only ever returns the most recent call's value
  - assert no other simulated `GameState` mutation (key-held flag, door-open flag, battery charge, hiding flag) changes the stored checkpoint value, by setting those fields directly and re-reading `GetCurrentCheckpoint`
  - assert reading the checkpoint mid-floor (after movement, after a life is lost) still returns the floor's entry position, not the player's last actual position

**Checkpoint**: User Story 2 is independently complete and tested — the checkpoint contract holds under every in-floor mutation tried.

---

## Phase 4: User Story 3 - Lives Are Told Once, Then Never Shown Again (Priority: P2)

**Goal**: `Lives` is readable by exactly one UI surface (How To Play) and by nothing else.

**Independent Test**: Grep `Assets/Scripts/UI/` for any reference to `LivesSystem.GetLives`/`GameState.Lives` outside the How To Play screen's own script; manually audit a played run's screens.

- [ ] T010 [US3] Expose `LivesSystem.GetLives(GameState)` as the single call the How To Play screen will use (`specs/systems/game-shell-ui/003-how-to-play-screen`, not yet implemented) — add a doc comment on `LivesSystem.cs` naming that screen as the only intended caller for display purposes
- [ ] T011 [US3] When `specs/systems/game-shell-ui/003-how-to-play-screen` lands, wire its lives-count label to `LivesSystem.GetLives(GameManager.Instance.State)` as a one-line call site; do not add the same read anywhere else under `Assets/Scripts/UI/` (this task stays open/tracked here until that screen exists, per FR-007)
- [ ] T012 [US3] Manual audit (per constitution Principle IV's exception for UI/presentation checks): play one full run including at least two deaths on a build with placeholder UI; inspect every screen (How To Play, HUD, pause menu, death sequence, floor-transition splash) and confirm a lives indicator appears on exactly one of them — record the result against SC-003

**Checkpoint**: All three user stories are independently functional and tested.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T013 [P] Review `GameState.cs`, `Checkpoint.cs`, `LivesSystem.cs`, and `CheckpointSystem.cs` against constitution Principle III (no `MonoBehaviour`/`Component` reference inside `Assets/Scripts/Systems/`) and Principle II (no hardcoded `3` for lives anywhere outside reading `GameConfig.lives`)
- [ ] T014 Confirm `GameConfig.lives` and `GameConfig.checkpointPerFloor` are not duplicated under a second name anywhere in the codebase (single-source-of-truth check, constitution Principle V)

---

## Dependencies & Execution Order

- **Foundational (Phase 1)**: strictly before both user stories — both read/write `GameState.Lives` / `GameState.CurrentCheckpoint`.
- **User Story 1 (P1)** and **User Story 2 (P1)**: both depend only on Foundational; independent of each other (different fields, different files) and may proceed in parallel.
- **User Story 3 (P2)**: depends on US1's `LivesSystem.GetLives` existing (T004); does not depend on US2.
- **Polish (Phase 5)**: after all three stories.
