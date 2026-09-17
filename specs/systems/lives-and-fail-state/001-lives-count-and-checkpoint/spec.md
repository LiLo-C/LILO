# Feature Specification: Lives Count and Checkpoint

**Feature Branch**: `001-lives-count-and-checkpoint`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 9.1 (Aturan), 17.5 (Progression config: `lives`, `checkpointPerFloor`), 15.4 (How To Play mention)

**Input**: User description: "Player has 3 lives for the whole run, hidden from the HUD during gameplay — the only time the count is ever shown is once, on the How To Play screen before a run starts. The checkpoint is the start of the current floor: exactly one checkpoint per floor, with no mid-floor checkpoints. This spec owns the lives counter's storage and rules and the checkpoint's storage and rules; it does not itself decide when a life is lost or what a reset restores — those belong to sibling specs 002 and 003."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Lives Persist Across a Whole Run (Priority: P1)

The game tracks how many lives the player has left across the entire run (all three floors), starting at the locked value of 3 and never resetting mid-run — only a brand new run brings it back to 3.

**Why this priority**: Every other fail-state spec (002, 003) and the Bad Ending trigger depend on there being a single, trustworthy lives counter that survives floor transitions. Without this, "lives habis = Bad Ending" (GDD 9.1) has nothing to check.

**Independent Test**: Start a run, simulate two life losses on Floor 51, transition to Floor 50, and confirm the counter still reads 1 (not reset to 3) until a brand new run begins.

**Acceptance Scenarios**:

1. **Given** a brand new run is starting, **When** the run begins, **Then** the lives counter is initialized to exactly `GameConfig.lives` (locked at 3 per GDD 17.5).
2. **Given** the player has lost one life on Floor 51, **When** the player descends to Floor 50, **Then** the lives counter still reads 2 — floor transitions never touch it.
3. **Given** a run has ended (Good Ending or Bad Ending) and a new run starts, **When** the new run begins, **Then** the counter is reinitialized to `GameConfig.lives`, independent of how the previous run ended.

---

### User Story 2 - Exactly One Checkpoint Per Floor (Priority: P1)

The game remembers exactly one place to respawn the player: the entry point of whichever floor is currently active. That checkpoint is written once, when the floor is entered, and never updated again until the next floor is entered.

**Why this priority**: Spec 002 (floor-state-reset-on-death) cannot place the player anywhere sensible after a reset without a single, unambiguous checkpoint value to read. GDD 9.1 is explicit that there is no mid-floor checkpoint — this is the rule that keeps that true.

**Independent Test**: Enter Floor 51 at its entry point, walk to the far end of the map, pick up the key, and confirm the stored checkpoint still equals Floor 51's entry point (not the player's current position) — then enter Floor 50 and confirm the checkpoint is overwritten to Floor 50's entry point.

**Acceptance Scenarios**:

1. **Given** a floor becomes the active floor, **When** it is entered, **Then** the checkpoint is set to exactly that floor's entry point.
2. **Given** the player has moved away from the entry point, picked up a key, opened a door, or installed a battery, **When** any of those actions occur, **Then** the stored checkpoint is unchanged.
3. **Given** the player descends to a new floor, **When** that new floor is entered, **Then** the previous floor's checkpoint value is discarded and replaced — the system never holds more than one checkpoint at a time.

---

### User Story 3 - Lives Are Told Once, Then Never Shown Again (Priority: P2)

The player learns the number of lives exactly once, on the How To Play screen before a run starts, and the number never appears anywhere else — not in the HUD, not at the moment of death, not on the pause menu.

**Why this priority**: This is a deliberate tension choice in the GDD (9.1: "Disembunyikan dari HUD selama gameplay") — the risk is that a future HUD or death-feedback feature quietly reads this value without realizing it's supposed to stay hidden. Lower priority than US1/US2 because it's a display constraint layered on data that already has to exist correctly first.

**Independent Test**: Play a full run that includes at least two deaths, and screen-by-screen audit every UI surface (How To Play, HUD, pause menu, death sequence, floor splash text) for a lives indicator.

**Acceptance Scenarios**:

1. **Given** the How To Play screen is shown before a run starts, **When** it renders, **Then** it displays the lives count exactly once (per GDD 15.4, this is the only place it appears — see `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`).
2. **Given** any moment during gameplay — HUD, death sequence, pause menu, or floor-transition splash text — **When** that screen is inspected, **Then** no lives count, heart icon, or "lives remaining" text is present.

---

### Edge Cases

- **Dying mid-hiding**: the checkpoint is defined purely by which floor is active and that floor's entry point — it is completely indifferent to whether the player was hiding, walking, or mid-chase at the moment of death. Being caught while hidden does not create, move, or invalidate a checkpoint; the checkpoint that spec 002 will read back is exactly the same value it was before the catch.
- **Dying mid-battery-install**: the lives counter and checkpoint are read/written independently of any in-progress interaction. An interrupted install does not block, delay, or partially apply a life decrement or a checkpoint read — this spec exposes plain, synchronous queries/operations with no notion of "waiting for an interaction to finish."
- **Dying at exactly 0 lives on the very first catch**: this spec's job is only to expose a correct, generic "lives remaining" query (`lives > 0`) and a single decrement operation — it must never assume that reaching zero requires more than one call. If `GameConfig.lives` were ever configured to `1` (a non-default/test configuration; GDD 9.1/17.5 locks the shipped value at 3), the very first decrement must correctly report zero lives remaining. This spec does not itself decide what happens next (that branch is spec 003's) — it only guarantees the query is correct at the boundary.
- **Checkpoint requested before any floor has been entered** (e.g., during Bootstrap or the Prologue, before Floor 52 loads): there is no valid checkpoint yet. Reading the checkpoint in this state is out of scope for this spec to resolve gracefully — callers (spec 002, scene-flow) must not invoke a floor reset before a floor has been entered at least once.
- **Lives counter queried or decremented outside of a run** (e.g., at the Main Menu, before a new run has initialized it): reads MUST return the last-initialized value, never an uninitialized/garbage value — the counter is always in a well-defined state once `GameManager` exists, even before the first `ResetLivesForNewRun` call, per FR-001's ScriptableObject-backed default.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `GameState` MUST expose a single integer `Lives` value. At the start of a new run, it MUST be initialized to exactly `GameConfig.lives`, the value locked by the GDD at 3 (Ch. 9.1, 17.5). This spec MUST NOT hardcode `3` anywhere outside reading `GameConfig.lives`.
- **FR-002**: `Lives` MUST remain unchanged across floor-to-floor transitions within the same run. Only a decrement performed by spec 003 (death-sequence-and-outcome-branch), or a new-run initialization (FR-001), may change it — no other system in this spec or elsewhere may write to it directly.
- **FR-003**: This spec MUST expose exactly three operations for `Lives`, for exclusive use by other specs: a read query (current value), a boolean query for "has lives remaining" (`Lives > 0`), and a single decrement operation (`Lives - 1`, floored at 0, never negative). No other read/write surface for `Lives` may exist.
- **FR-004**: `GameState` MUST expose a single `Checkpoint` value (floor identifier + entry position) representing where the player respawns on the current floor. There MUST NOT be a list, stack, or history of checkpoints — only ever one, for the currently active floor.
- **FR-005**: The checkpoint MUST be written exactly once per floor, at the moment that floor becomes the active floor. It MUST NOT be written at any other time during that floor's play — no autosave, no partial-progress checkpoint, no update triggered by picking up a key, opening a door, installing a battery, or any other in-floor action (GDD 9.1: "Satu checkpoint per floor. Tidak ada checkpoint tengah floor.").
- **FR-006**: The `Lives` value MUST NOT be rendered or otherwise exposed by any UI element during gameplay, the death sequence, the pause menu, or floor-transition splash text.
- **FR-007**: The `Lives` value MUST be readable by, and MUST only be intentionally surfaced through, the How To Play screen (`specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`), shown once before a run starts (GDD 15.4: "Layar How To Play singkat sebelum game dimulai boleh ada, dan di situlah satu-satunya tempat jumlah lives disebutkan."). No other screen may read `Lives` for display purposes.
- **FR-008**: `GameConfig` MUST carry the `lives` (locked at 3) and `checkpointPerFloor` (locked `true`) fields already named in GDD 17.5. This spec does not introduce any additional tunable beyond what FR-001–FR-007 require.

### Key Entities

- **Lives Counter**: a single integer, part of `GameState`, bounded to `[0, GameConfig.lives]`, persists unchanged across floors within one run, reinitialized only at new-run start.
- **Checkpoint**: a single `(floor identifier, entry position)` pair, part of `GameState`, overwritten wholesale exactly once per floor entry, otherwise immutable for the rest of that floor.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of automated tests, a new-run initialization sets `Lives` to exactly `GameConfig.lives`, regardless of how many lives were lost or floors played in any prior run.
- **SC-002**: In 100% of automated tests, no in-floor action (key pickup, door open, battery install, hiding enter/exit) changes the stored checkpoint value; only a floor-entry event does.
- **SC-003**: In a screen-by-screen audit of one full run containing at least two deaths, a lives indicator appears on exactly one screen (How To Play) and zero other screens.
- **SC-004**: In 100% of automated tests, the "has lives remaining" query correctly returns `false` the moment `Lives` reaches `0` via a single decrement from `1`, with no dependency on how many prior decrements occurred in the run.

## Assumptions

- Actual layout/rendering of the How To Play screen is owned by `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`; this spec only requires that screen be able to read `Lives`.
- The floor's authored entry position (the coordinate the checkpoint stores) is supplied by that floor's own level-layout content (e.g., `specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/spec.md`); this spec defines only the rule for when that position is captured and how it's stored, not the level design that produces it.
- The mechanism for persisting `GameState` across scene loads (a `GameManager` `MonoBehaviour` with `DontDestroyOnLoad`) is already fixed by `specs/systems/shared-config-and-state/002-shared-game-state-and-manager` and the constitution (Principle III); this spec does not re-decide that architecture.

## Dependencies

- **Requires**: `specs/systems/shared-config-and-state/002-shared-game-state-and-manager` (for `GameState`/`GameManager` to exist at all).
- **Consumed by**: `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death` (reads the checkpoint to place the player), `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch` (calls the decrement and reads "has lives remaining"), `specs/systems/game-shell-ui/003-how-to-play-screen` (reads `Lives` for its one-time display).

## Related

- GDD: `specs/_reference/LILO-GDD-v2-Production-Lock.md` — Ch. 9.1, 17.5, 15.4.
- `specs/ROADMAP.md` — §2 `lives-and-fail-state` row `001-lives-count-and-checkpoint`; §5 build order (depends only on `shared-config-and-state`).
- `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md` — the sole display surface for `Lives`.
