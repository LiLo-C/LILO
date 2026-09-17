# Feature Specification: Floor State Reset on Death

**Feature Branch**: `002-floor-state-reset-on-death`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 9.2 (Yang di-reset saat mati — the explicit, exhaustive reset list), 9.1
(checkpoint = start of current floor), 3.1 (battery spawn point registry), 8.1 (keys/locked
doors per floor), 6.3 (monster preset spawn points)

**Input**: User description: "On death, every piece of that floor's state resets to its
start-of-floor condition, per GDD 9.2's explicit list: player position → floor entry point;
installed battery → 100% charge, spare slot emptied; world batteries → back to floor-start
layout; any picked-up keys → returned to original position; any opened doors → re-locked; the
monster → back to PATROL at one of its preset spawn points. This spec is the orchestrator that
triggers all of those sibling systems' own resets together, atomically — it does not redefine
any one of those systems' reset behavior itself."

## Scope Note

This spec owns exactly one thing: the moment a floor reset is triggered, it calls into each
sibling system's own reset operation, in one atomic sweep, so that **no piece of GDD 9.2's list
is ever missed and no piece is ever left half-reset**. It does not decide *when* a reset is
triggered (that is `003-death-sequence-and-outcome-branch`'s job) and it does not reimplement
*how* any individual piece of state resets — that behavior is already owned by:

- `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/spec.md` — installed
  battery charge and spare-slot contents.
- `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md` — world
  battery / spawn-point occupancy layout.
- `specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md` — held-key inventory and
  key world position.
- `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md` — door lock/open state.
- `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` — the monster's valid
  preset spawn points, and `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`
  — the `Reset()` hook that returns it to `Patrol`.

This spec's only original logic is: knowing the complete list of resets to call, calling all of
them as one unit, and reading the checkpoint (`specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md`)
to know where the player goes.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - One Call Resets Everything, With Nothing Missed (Priority: P1)

When a floor reset is triggered, every single piece of that floor's state — player position,
installed battery, world batteries, keys, doors, and the monster — returns to exactly its
start-of-floor condition in one atomic operation, with no partial reset ever observable by the
player or by any other system.

**Why this priority**: This is the entire reason the spec exists. GDD 9.2 is explicit and
exhaustive ("Semua state floor dikembalikan ke kondisi awal floor, tanpa pengecualian" — all floor
state returns to its starting condition, without exception). A reset that misses even one item
(e.g., a door stays unlocked, or the monster keeps its pre-death `Chase` target) breaks the
fairness contract the whole checkpoint system promises, and would let a bug quietly compound
across repeated deaths.

**Independent Test**: In a test harness for a fixture floor, install a battery below 100%, fill
the spare slot, deplete two world battery spawn points, pick up all of that floor's keys, unlock
all of that floor's doors, and drive the monster into `Chase`. Trigger the floor reset. Assert, in
a single post-reset check, that every one of those six systems now reports exactly its
start-of-floor value — not merely that some of them do.

**Acceptance Scenarios**:

1. **Given** a floor reset is triggered, **When** it runs, **Then** the player's position is set
   to exactly that floor's checkpoint entry point (GDD 9.2, 9.1).
2. **Given** the installed battery had less than 100% charge and the spare slot held a battery,
   **When** the floor reset runs, **Then** the installed battery reads exactly 100% and the spare
   slot is empty (GDD 9.2, delegating to `flashlight-and-battery/007`).
3. **Given** some of the floor's world battery spawn points had been depleted or their batteries
   picked up, **When** the floor reset runs, **Then** the floor's world batteries return to
   exactly their start-of-floor layout (GDD 9.2, delegating to `battery-spawn-system/001`).
4. **Given** one or more keys had been picked up, **When** the floor reset runs, **Then** each key
   is returned to its original world position and removed from the held-key inventory (GDD 9.2,
   delegating to `keys-and-doors/001`).
5. **Given** one or more doors had been unlocked and opened, **When** the floor reset runs,
   **Then** every one of those doors is locked and closed again (GDD 9.2, delegating to
   `keys-and-doors/002`).
6. **Given** the monster was in any state (`Investigate`, `Chase`, `Search`) at any position,
   **When** the floor reset runs, **Then** the monster's state machine is reset to `Patrol` and its
   world position is set to one of that floor's preset spawn points (GDD 9.2, 6.3, delegating to
   `monster-ai/001` and `monster-ai/003`).
7. **Given** the reset is in progress, **When** any other system queries floor state mid-reset,
   **Then** it never observes a state where only some of the six items above have been reset — the
   whole sweep is treated as one atomic unit from every external caller's point of view.

---

### User Story 2 - The Reset Is Idempotent and Order-Independent in Its Outcome (Priority: P2)

Triggering a floor reset a second time in a row (e.g., a defensive double-call from spec 003, or
a reset requested while the player was already at the checkpoint with nothing touched) produces
the exact same end state as triggering it once — and the specific order in which this spec calls
each sibling system's reset makes no observable difference to the final state.

**Why this priority**: This is a robustness guarantee layered on top of User Story 1's correctness
— the reset already works correctly on a normal single call; this story protects against a
double-trigger bug (e.g., two systems both deciding a death occurred) silently corrupting state,
and frees each sibling reset from needing to coordinate ordering with the others. P2 because it
does not change what a single, well-formed call does.

**Independent Test**: Trigger a floor reset twice in immediate succession on the same fixture
floor (with no player action in between) and assert the resulting state is identical to a single
trigger. Separately, call the six sibling resets in two different orders across two otherwise
identical fixture runs and assert the final state is identical either way.

**Acceptance Scenarios**:

1. **Given** a floor reset has just completed, **When** it is triggered again immediately with no
   intervening gameplay, **Then** the resulting state is identical to the state right after the
   first reset — no error, no double-decrement, no double-restore artifact.
2. **Given** two otherwise identical fixture runs that call this spec's six sibling resets in a
   different internal order, **When** both finish, **Then** the observed final state is identical
   between the two runs — no sibling reset in this list depends on another having already run.

---

### User Story 3 - The Reset Targets Only the Active Floor's State (Priority: P3)

A floor reset only ever touches the state belonging to the floor the player is currently
attempting — it never reaches back and mutates a previous, already-cleared floor's state, and it
never reads or writes a floor the player has not yet reached.

**Why this priority**: Lower priority because in the normal single-floor-at-a-time flow this is
already guaranteed by each sibling system's own per-floor scoping (e.g.,
`battery-spawn-system/001`'s registry is explicitly cleared/rebuilt per floor load). This story
exists to make that guarantee explicit at the orchestration layer too, as a safety net against a
future regression, not because today's flow can otherwise cross floor boundaries.

**Independent Test**: With fixture state established for both a "previous" floor (already passed)
and the "current" floor, trigger a reset for the current floor only, and assert the previous
floor's fixture state is completely untouched.

**Acceptance Scenarios**:

1. **Given** fixture state exists for a floor the player already completed and left, **When** a
   reset is triggered for the current floor, **Then** the previously-completed floor's state is
   unchanged.
2. **Given** no floor has been entered yet (e.g., a reset call attempted before any floor load),
   **When** the reset is invoked, **Then** it is rejected/no-ops rather than resetting an undefined
   floor — mirroring `lives-and-fail-state/001`'s own edge case for a checkpoint requested before
   any floor has loaded.

---

### Edge Cases

- **Dying mid-hiding**: the reset is completely indifferent to whether the player was hiding,
  walking, or mid-chase at the moment of death — being caught while hidden triggers the exact same
  six-part reset as being caught anywhere else. There is no seventh "un-hide the player" step
  because hiding state itself is not one of GDD 9.2's listed items and is not floor state that
  persists across the checkpoint boundary; the player simply arrives at the floor's entry point in
  the normal (not-hiding) state, the same as any other reset.
- **Dying mid-battery-install**: an in-progress "install battery from spare slot" interaction that
  is interrupted by death is not given any special partial-completion handling by this spec — the
  reset unconditionally sets the installed battery to 100% and empties the spare slot (per
  `flashlight-and-battery/007`'s own reset contract) regardless of whether an install animation or
  interaction was mid-flight. This spec does not need to know an install was in progress; the
  post-reset state is identical either way.
- **Dying at exactly 0 lives on the very first catch**: this spec's reset operation is called only
  when `lives-and-fail-state/003` has already determined lives remain (`> 0`) after the
  decrement — if the very first catch brings lives to exactly 0, spec 003 routes to the Bad Ending
  instead and this spec's reset MUST NOT run at all. This spec exposes the reset as a single
  callable operation with no opinion on whether it should be called; the zero-lives gate lives
  entirely in spec 003.
- **A sibling system has nothing to reset** (e.g., the player died before picking up any key, or
  before any world battery spawn point was ever depleted): each sibling's own reset operation MUST
  already be a safe no-op in that case (this is each sibling spec's own contract, not something
  this spec re-verifies) — calling all six resets unconditionally, every time, regardless of what
  actually changed since floor entry, is this spec's deliberate simplicity choice (constitution
  Principle II) rather than tracking a dirty-list of "what actually needs resetting."
- **The monster's preset spawn point chosen on reset need not be the same one chosen at original
  floor entry**: GDD 9.2 says "kembali ke PATROL di salah satu preset spawn point" (back to PATROL
  at one of the preset spawn points) — not necessarily the same one. This spec calls
  `monster-ai/003`'s spawn-point selection the same way floor entry does, and accepts whatever
  valid point that selection returns; it does not pin or remember the original entry's specific
  point.
- **Reset triggered with zero keys/doors/monster on the floor at all (a Floor 52-shaped floor)**:
  the reset still runs for every item that does apply (player position, installed battery, world
  batteries) and simply has nothing to do for the items that don't apply (no keys, no doors, no
  monster on Floor 52 per GDD Ch. 6.3) — this spec's six-call sweep degrades gracefully rather
  than requiring a floor to have every kind of state to reset correctly. (Floor 52 has no fail
  state to trigger this at all today, since it has no monster — this generic case is future-proofing
  the orchestrator's shape, not a currently reachable Floor 52 scenario.)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a single operation ("reset the current floor") that, once
  invoked, performs all of the following as one atomic unit, in any order, with no externally
  observable intermediate/partial state:
  1. Set the player's position to the current floor's checkpoint entry point, read from
     `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md`.
  2. Reset the installed battery to 100% charge and empty the spare slot, via
     `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/spec.md`'s reset
     operation.
  3. Restore all world batteries / battery spawn points on the current floor to their
     start-of-floor layout, via
     `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md`'s reset
     operation.
  4. Return every held key to its original world position and clear the held-key inventory, via
     `specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md`'s reset operation.
  5. Re-lock and re-close every door on the current floor, via
     `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md`'s reset operation.
  6. Reset the monster's state machine to `Patrol` and its world position to one of the current
     floor's validated preset spawn points, via
     `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`'s `Reset()` hook and
     `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`'s spawn-point selection.
- **FR-002**: This spec MUST NOT reimplement, duplicate, or diverge from any of the six sibling
  systems' own reset behavior described in FR-001. It only calls each sibling's already-owned
  reset operation; if a sibling spec changes what its own reset does, this spec's behavior changes
  automatically with it, with no code duplicated here.
- **FR-003**: The six-part reset in FR-001 MUST be treated as a single atomic unit: no other
  system may observe a state where only some of the six items have completed. If any single
  sibling reset call can fail, this spec MUST NOT leave the floor in a mixed state — the whole
  operation either completes all six parts or is treated as having completed none (fail loudly,
  do not partially apply).
- **FR-004**: Triggering the reset operation MUST be idempotent: calling it twice in immediate
  succession with no intervening gameplay MUST produce the same resulting state as calling it
  once.
- **FR-005**: The reset operation's final resulting state MUST NOT depend on the internal order in
  which this spec calls the six sibling resets — each sibling reset MUST be independent of the
  others having already run (this spec does not introduce ordering dependencies between siblings
  that don't already have one from their own specs).
- **FR-006**: The reset operation MUST scope every one of the six items to the current floor only.
  It MUST NOT read or mutate any other floor's state, whether previously completed or not yet
  reached.
- **FR-007**: The reset operation MUST NOT run if no floor has yet been entered (mirroring
  `lives-and-fail-state/001`'s "checkpoint requested before any floor entered" edge case) — callers
  (spec 003) MUST only invoke it after at least one floor entry has occurred.
- **FR-008**: This spec MUST NOT decide *when* the reset is triggered, nor decrement lives, nor
  branch on remaining lives — those decisions belong entirely to
  `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch/spec.md`. This spec
  only exposes the reset as a callable operation.
- **FR-009**: The reset operation MUST be implemented as plain C# orchestration logic under
  `Assets/Scripts/Systems/` with no `MonoBehaviour`/`Component`/scene-graph dependency
  (constitution Principle III) — it calls into each sibling system's own plain C#
  reset method directly; any Unity-lifecycle wiring needed to invoke it from a real death event is
  a thin `MonoBehaviour` adapter under `Assets/Scripts/MonoBehaviours/`, owned by spec 003's
  integration, not by this spec's own logic.
- **FR-010**: This spec introduces no new `GameConfig` tunable values of its own — every value it
  touches (battery duration/capacity, spawn point layout, key/door identities, monster tuning) is
  already owned by the sibling specs it orchestrates.

### Key Entities

- **FloorResetOrchestrator**: The plain C# entry point for this spec. Exposes one operation,
  `ResetCurrentFloor(...)`, that calls into the six sibling reset operations listed in FR-001 as
  one atomic unit and reads the checkpoint from `lives-and-fail-state/001` to place the player.
- **Floor Reset (composite concept)**: Not a new stored data type — a behavioral guarantee that,
  after `ResetCurrentFloor` returns, every one of the six items in FR-001 reads exactly its
  start-of-floor value. This spec adds no new persisted state beyond what the six sibling systems
  already own.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In an automated test that mutates all six reset targets (player position, installed
  battery charge/spare slot, world battery layout, held-key inventory, door lock states, monster
  state/position) away from their start-of-floor values and then triggers one reset call, 100% of
  the six targets read exactly their start-of-floor value in the single post-reset check.
- **SC-002**: In an automated test, triggering the reset operation twice in immediate succession
  produces byte-for-byte identical resulting state to triggering it once, in 100% of test runs.
- **SC-003**: In an automated test that calls the six sibling resets in two different internal
  orderings across two otherwise identical fixture runs, the two runs' final states are identical
  in 100% of trials.
- **SC-004**: In an automated test, resetting the current floor while a "previous, already
  completed" floor's fixture state is also populated leaves that previous floor's state completely
  unchanged, in 100% of trials.
- **SC-005**: A structural/code review confirms zero duplicated reset logic exists in this spec's
  orchestrator for any of the six items — each is a single delegating call to its owning sibling
  spec's reset operation, never a re-derived copy of that logic.

## Assumptions

- Each of the six sibling systems (`flashlight-and-battery/007`, `battery-spawn-system/001`,
  `keys-and-doors/001`, `keys-and-doors/002`, `monster-ai/001`, `monster-ai/003`) exposes its own
  reset operation as part of its own spec's contract — this spec assumes those operations exist
  and are correct in isolation (each is independently tested by its own spec's EditMode suite) and
  only tests that this spec calls all of them, together, correctly. If a sibling spec has not yet
  been implemented when this spec is implemented, its reset call site here is stubbed against that
  spec's documented contract and wired for real once that sibling lands (mirroring the same
  forward-reference pattern already used by `keys-and-doors/001`/`002` for
  `lives-and-fail-state/002` before this spec existed).
- The checkpoint (floor identifier + entry position) this spec reads to place the player is
  entirely owned and written by `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md`;
  this spec never writes the checkpoint itself, only reads it.
- "Atomic" in this spec's sense means "no external observer sees a partial result" — it does not
  imply a specific transaction/rollback mechanism; given the plain C#, single-threaded, synchronous
  nature of the systems it calls (constitution Principle III), a straightforward sequential call
  of all six operations within a single method already satisfies this guarantee in practice.
- The monster's preset spawn point selection on reset reuses the exact same selection logic
  `monster-ai/003` uses for a floor's initial entry — this spec does not define a second,
  reset-specific spawn selection rule.

## Dependencies

- **Requires**: `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md` (reads
  the checkpoint), `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/spec.md`,
  `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md`,
  `specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md`,
  `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md`,
  `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`,
  `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` (each sibling's own reset
  operation, called but not redefined here).
- **Consumed by**: `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch/spec.md`
  (the sole caller that decides *when* to invoke `ResetCurrentFloor`).

## Related

- GDD: `specs/_reference/LILO-GDD-v2-Production-Lock.md` — Ch. 9.2 (the exhaustive reset list this
  spec orchestrates), 9.1 (checkpoint = floor entry point), 3.1, 8.1, 6.3 (the sibling systems'
  own GDD sources).
- `specs/ROADMAP.md` — §2 `lives-and-fail-state` row `002-floor-state-reset-on-death` (depends on
  `001`, `battery-spawn-system/*`, `keys-and-doors/*`); §5 build order (also implicitly depends on
  `monster-ai/001`/`003` for the monster-reset call, per this spec's own FR-001 item 6 — the
  ROADMAP row lists the battery/keys categories explicitly and this spec adds the monster-ai
  dependency the row's GDD source (Ch. 9.2) itself requires).
- `specs/systems/lives-and-fail-state/003-death-sequence-and-outcome-branch/spec.md` — the sole
  trigger for this spec's reset operation.
