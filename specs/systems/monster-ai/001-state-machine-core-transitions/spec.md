# Feature Specification: Monster State Machine Core Transitions

**Feature Branch**: `001-state-machine-core-transitions`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The PATROL → INVESTIGATE → CHASE → SEARCH → PATROL monster state
machine per GDD Ch. 6.1's exact transition table. Patrol follows fixed waypoints. Investigate
moves to the noise source without knowing the player's true position. Chase pursues the last-known
position, NOT the player's real-time position. Search circles the last-known position briefly
before giving up. Consumes a detection signal from the noise/detection system as input; does not
redefine that system's detection math."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Monster investigates a noise it hears (Priority: P1)

While patrolling its fixed waypoint route, the `Monster` hears the player (a detection signal
arrives) and breaks off patrol to move toward the point the noise came from — not toward the
player's actual position, which the monster never has direct access to.

**Why this priority**: This is the smallest slice that makes the monster feel alive and makes the
"Uncertainty over Information" design pillar (GDD Ch. 1.3) real in code. Without this reaction,
there is no AI at all — every other transition in the ring builds on top of it.

**Independent Test**: In a debug test arena (a bare NavMesh-baked room with one `Monster` and one
scripted detection signal — not a real floor), feed the state machine a single synthetic
detection tick while it is `Patrol`ling. Assert it transitions to `Investigate` and that its
target position equals the detection signal's source point, not the (separately tracked) real
player position.

**Acceptance Scenarios**:

1. **Given** the monster is in `Patrol` and no detection signal has ever arrived, **When** the
   state machine is ticked repeatedly, **Then** it remains in `Patrol` indefinitely.
2. **Given** the monster is in `Patrol`, **When** a detection signal with `IsDetected = true`
   arrives, **Then** the monster transitions to `Investigate` on that same tick and its
   investigate-target is set to the signal's source position.
3. **Given** the monster is `Investigate`ing and no further detection arrives, **When**
   `investigateDuration` (supplied externally, per floor — see spec 002) elapses, **Then** the
   monster transitions back to `Patrol`.

---

### User Story 2 - Monster escalates to a full chase (Priority: P1)

While `Investigate`ing (or `Search`ing), the monster is detected again and escalates to `Chase`,
pursuing the last point the player was heard at — never a live, continuously-updating player
position.

**Why this priority**: This is the mechanic that turns "something is looking around" into real
threat. Without escalation, the monster can never meaningfully endanger the player, and the
Compact Pressure / Uncertainty pillars have nothing to bite on.

**Independent Test**: In the same debug arena, script two detection ticks a few seconds apart
while the monster is `Investigate`ing. Assert the second tick causes a transition to `Chase`, and
that the chase target only ever updates on a detection tick (never silently tracks the player
between ticks).

**Acceptance Scenarios**:

1. **Given** the monster is `Investigate`ing, **When** a new detection signal arrives before
   `investigateDuration` elapses, **Then** the monster transitions to `Chase` and its target
   updates to the new source position.
2. **Given** the monster is `Chase`ing and detection signals keep arriving, **When** each new
   signal is ticked, **Then** the chase target updates to that signal's source position and the
   `chaseHoldDuration` countdown does not start.
3. **Given** the monster is `Search`ing, **When** a new detection signal arrives before
   `searchDuration` elapses, **Then** the monster transitions back to `Chase`.

---

### User Story 3 - Monster loses the trail and stands down (Priority: P2)

Once the player is no longer heard, the chasing monster holds its pursuit briefly, then searches
a small area around the last-known point, then gives up and resumes patrol — giving the player a
fair, learnable way to become safe again.

**Why this priority**: Ranked below the two detection-escalation stories because it is the
"relief valve," not the initial threat — but it is still core to fairness: a monster that never
stands down turns tension into pure punishment, which GDD Ch. 6.2's explicit hard rule
(chase speed capped below sprint) exists specifically to avoid.

**Independent Test**: In the debug arena, put the monster into `Chase` via a scripted detection,
then stop sending detection signals and let the clock run. Assert the transition to `Search`
happens at exactly `chaseHoldDuration`, and the transition back to `Patrol` happens at exactly
`chaseHoldDuration + searchDuration`, with no detection input in between.

**Acceptance Scenarios**:

1. **Given** the monster is `Chase`ing, **When** no detection signal arrives for
   `chaseHoldDuration` seconds, **Then** the monster transitions to `Search`, retaining the last
   detected position as the search center.
2. **Given** the monster is `Search`ing, **When** no detection signal arrives for
   `searchDuration` seconds, **Then** the monster transitions to `Patrol`.
3. **Given** the monster re-enters `Patrol` after standing down, **When** ticked with no
   detection, **Then** it resumes its fixed waypoint route from wherever it currently is (no
   teleport back to a "start" waypoint).

---

### User Story 4 - State machine is deterministic and externally freezable (Priority: P3)

The state machine is pure C# (no `MonoBehaviour`/scene dependency), produces identical output for
identical input sequences, and exposes a `Freeze`/`Reset` hook so an external system (the CATCH
outcome, spec 004) can halt it the instant the player is caught, and re-arm it on floor reset.

**Why this priority**: This is enabling infrastructure rather than a player-visible behavior on
its own — it is what makes the other three stories testable in EditMode at all, and what spec 004
will depend on — so it is correctly lower priority than the gameplay-visible escalation/de-escalation
behaviors, but it is not optional.

**Independent Test**: Run the exact same scripted detection-signal sequence through two separate
`MonsterStateMachine` instances and assert both produce an identical resulting state/timer
sequence. Separately, call `Freeze()` mid-sequence and assert every subsequent `Tick()` is a
no-op until `Reset()` is called.

**Acceptance Scenarios**:

1. **Given** two fresh state machine instances constructed with the same tuning values, **When**
   both are fed the identical sequence of `(deltaTime, detectionSignal)` pairs, **Then** both
   report the identical state and elapsed-timer value after every tick.
2. **Given** the state machine is in any state, **When** `Freeze()` is called, **Then**
   `IsFrozen` becomes `true` and every subsequent `Tick()` call has no effect (no state change,
   no timer advance) until `Reset()` is called.
3. **Given** the state machine is frozen, **When** `Reset()` is called, **Then** `IsFrozen`
   becomes `false`, `CurrentState` becomes `Patrol`, and all timers/last-known-position are
   cleared.

---

### Edge Cases

- **Tie-break at exact timer expiry**: if a detection signal arrives on the exact same tick that
  `investigateDuration`/`chaseHoldDuration`/`searchDuration` would otherwise expire, the detection
  MUST win (the monster escalates/re-escalates rather than standing down) — detection always takes
  priority over a simultaneous timeout.
- **Continuous detection never times out**: as long as detection signals keep arriving every tick
  while `Chase`ing, the hold-timer never starts counting — a monster cannot "outlast" a player who
  is continuously making noise right next to it.
- **Single-tick detection blip**: any tick where `IsDetected = true`, however brief, MUST trigger
  the relevant transition — this spec adds no debouncing/minimum-duration filter on top of
  whatever noise-and-detection/002 already reports. If that spec's detection signal is itself
  noisy/flickery, smoothing belongs there, not here.
- **Frozen mid-transition**: calling `Freeze()` takes effect immediately regardless of which state
  the monster is currently in or how close a timer is to expiring; no in-flight transition
  completes after `Freeze()`.
- **External reset without an active catch**: `Reset()` is safe to call even when the machine was
  never frozen (e.g., a defensive floor-reset call) — it always returns the machine to a clean
  `Patrol` state.
- **What "close distance" means for Investigate → Chase**: GDD Ch. 6.1 qualifies the
  Investigate→Chase transition with "player terdeteksi lagi di jarak dekat" (detected again at
  close range), while the Search→Chase transition has no such qualifier. This spec does not layer
  a second, stricter distance threshold on top of the detection signal — see Assumptions.
- **Investigate/Search target math is not this spec's job**: this spec tracks a target *position*
  per state (source point for Investigate, last-known point for Chase/Search-center); how a
  MonoBehaviour adapter actually paths a NavMeshAgent to that position, or generates a circling
  wander pattern around a Search center, is implementation detail for the adapter layer, not the
  pure state machine (see Assumptions and Key Entities).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define exactly four monster states — `Patrol`, `Investigate`, `Chase`,
  `Search` — matching GDD Ch. 6.1's ring. `Caught`/CATCH is explicitly NOT a member of this enum;
  it is a cross-cutting outcome owned by spec 004 (see Related).
- **FR-002**: A newly constructed (or externally `Reset()`) state machine MUST start in `Patrol`.
- **FR-003**: System MUST accept, once per tick, a detection signal shaped as `{ bool IsDetected,
  Vector3 SourcePosition }` — the exact producer type/API is defined by
  `specs/systems/noise-and-detection/002-distance-based-detection-check/spec.md`; this spec only
  fixes the shape of data it needs to receive, and does not redefine that spec's distance/radius
  math.
- **FR-004**: `Patrol` MUST transition to `Investigate` on any tick where `IsDetected = true`,
  capturing `SourcePosition` as the investigate target.
- **FR-005**: `Investigate` MUST transition to `Chase` on any tick where `IsDetected = true`
  arrives again before `investigateDuration` elapses, updating the chase target to the new
  `SourcePosition`.
- **FR-006**: `Investigate` MUST transition back to `Patrol` once `investigateDuration` elapses
  with no further detection.
- **FR-007**: While `Chase`ing, every tick where `IsDetected = true` MUST update the chase target
  to the new `SourcePosition` and MUST NOT start/advance the `chaseHoldDuration` countdown.
- **FR-008**: `Chase` MUST transition to `Search` once `chaseHoldDuration` elapses with no
  detection tick in between, retaining the last detected position as the search center.
- **FR-009**: While `Search`ing, any tick where `IsDetected = true` MUST transition back to
  `Chase`, updating the chase target to the new `SourcePosition`.
- **FR-010**: `Search` MUST transition to `Patrol` once `searchDuration` elapses with no detection.
- **FR-011**: `investigateDuration`, `chaseHoldDuration`, and `searchDuration` (seconds), and the
  speed multipliers for `Patrol`/`Chase`, MUST be supplied to the state machine from outside — this
  spec MUST NOT hardcode any of GDD Ch. 6.2's per-floor numeric values; that is locked exclusively
  by `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`.
- **FR-012**: A detection tick that arrives on the exact tick a timeout would otherwise fire MUST
  be resolved as a detection (escalation/re-escalation), never as a simultaneous timeout — no
  ambiguous/undefined ordering.
- **FR-013**: System MUST expose `Freeze()`, an `IsFrozen` flag, and `Reset()`. While
  `IsFrozen = true`, `Tick()` MUST be a complete no-op (no state change, no timer advance, no
  target update). `Reset()` MUST clear `IsFrozen`, return `CurrentState` to `Patrol`, and clear all
  timers and the last-known/target position.
- **FR-014**: Given an identical sequence of `(deltaTime, detectionSignal)` ticks, the state
  machine MUST produce an identical sequence of `(CurrentState, timer, targetPosition)` outputs on
  every run — no dependency on wall-clock time, frame rate, or any Unity engine state, so the
  logic is fully covered by EditMode tests without a loaded scene.
- **FR-015**: The state machine class itself MUST contain no `MonoBehaviour`, `Component`, or
  scene-graph dependency (constitution Principle III); movement/pathing of the actual `Monster`
  `GameObject` is implemented by a separate `MonoBehaviour` adapter that calls `Tick()` each
  `Update` and translates its output into motion.
- **FR-016**: The adapter layer MUST NOT introduce its own transition logic (e.g., a second
  "is the player close enough" check) — all transition decisions live exclusively in the plain C#
  state machine; the adapter only supplies input (deltaTime, detection signal) and consumes output
  (state, target position, speed multiplier).

### Key Entities

- **`MonsterState`** (enum): `Patrol`, `Investigate`, `Chase`, `Search`. Lives in
  `Assets/Scripts/Systems/MonsterAI/`.
- **`MonsterDetectionSignal`** (plain data struct): `IsDetected` (bool), `SourcePosition`
  (`Vector3`) — the per-tick input this spec consumes from noise-and-detection/002.
- **`MonsterStateTuning`** (plain data struct): `investigateDuration`, `chaseHoldDuration`,
  `searchDuration` (seconds), `patrolSpeedMultiplier`, `chaseSpeedMultiplier` — supplied
  externally per floor by spec 002; this spec treats it as an opaque input, never a literal.
- **`MonsterStateMachine`** (plain C# class): owns `CurrentState`, elapsed timer, last-known/target
  position, `IsFrozen`; exposes `Tick(deltaTime, signal)`, `Freeze()`, `Reset()`. No engine
  dependency.
- **`MonsterController`** (`MonoBehaviour` adapter, `Assets/Scripts/MonoBehaviours/`): owns the
  `Monster` `GameObject`'s `Transform`/`NavMeshAgent`, queries the noise-and-detection runtime API
  each `Update`, calls `Tick()`, and applies the resulting state/target/speed to actual movement.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An automated test suite exercises 100% of the transition-table rows in GDD Ch.
  6.1 (every documented "→" in the table) plus the "stays put" negative case for each state, and
  all pass.
- **SC-002**: A monster continuously detected while `Chase`ing never reverts to `Search`/`Patrol`
  for as long as detection remains true, across an arbitrarily long scripted tick sequence (zero
  unintended reversions).
- **SC-003**: Given identical input tick sequences, the state machine's output is byte-for-byte
  identical across repeated runs (fully deterministic) — verified by an automated equality test,
  not by inspection.
- **SC-004**: Once `Freeze()` is called, 100% of subsequent `Tick()` calls in a test are proven
  to leave `CurrentState`, the timer, and the target position unchanged, until `Reset()` is
  called.
- **SC-005**: Retuning any of the three durations or two speed multipliers for a given floor (via
  spec 002's config) changes this state machine's timing/speed behavior with zero code changes to
  `MonsterStateMachine` itself.

## Assumptions

- **"Jarak dekat" (close range) qualifier**: GDD Ch. 6.1 qualifies the Investigate→Chase
  transition with "detected again at close range," while Search→Chase has no such qualifier. This
  spec treats "detected" as a single boolean already produced by a *distance-based* check
  (noise-and-detection/002 is itself named for exactly that). No second, stricter proximity
  threshold is layered on top here — escalation happens on any re-detection while
  Investigate/Search is active. If a future revision of noise-and-detection/002 needs to expose a
  separate "close" vs. "far" distinction, that is an additive change to `MonsterDetectionSignal`,
  not a redesign of this spec (constitution Principle V).
- **Movement mechanism — NavMesh chosen over hand-rolled waypoint-follow, stated explicitly per
  task guidance and constitution Principle II (Simplicity/YAGNI)**: `Patrol` alone could be solved
  with a trivial `Transform`-to-`Transform` waypoint walk with no pathfinding. But `Investigate`,
  `Chase`, and `Search` all require moving to an *arbitrary* point in the level (a heard noise
  source, a last-known position, a search-wander point) that a designer never pre-authored — on
  an office floor full of walls and desks, a naive straight-line `MoveTowards` to an arbitrary
  point will clip through geometry. Since `com.unity.ai.navigation` (NavMesh) is already an
  installed package, and three of the four states need arbitrary-point pathing, this spec's
  `MonsterController` adapter uses a `NavMeshAgent` for all monster movement — including `Patrol`,
  whose fixed waypoints simply become sequential `SetDestination` calls, at no extra cost over a
  bespoke waypoint walker. Building and maintaining a second, custom pathfinding/steering system
  alongside a built-in one that already solves the harder 3-of-4 case would itself be a YAGNI
  violation. This decision lives entirely in the `MonoBehaviour` adapter (FR-015/FR-016) — the
  pure `MonsterStateMachine` never references `NavMeshAgent` and stays engine-agnostic.
- **`noiseBaseRadius` is out of scope here**: the underlying noise-radius math (including the
  still-unlocked `noiseBaseRadius` value flagged as an open item in GDD Ch. 21, due "sebelum Fase 2
  (Monster)") belongs entirely to noise-and-detection/002. This spec never reads that value
  directly — it only ever sees the already-computed boolean/position signal, so it has nothing to
  fabricate or lock on that front.
- **Search's "circling" visual is an adapter concern**: the state machine only tracks a search
  center point and a duration; the actual small-radius wander/circle pattern the `Monster`
  `GameObject` visibly performs while `Search`ing is generated by `MonsterController` (e.g., picking
  successive NavMesh points within a radius of the center), not by the pure state machine.
- **Initial spawn state and position**: this spec assumes the `Monster`'s starting world position
  on floor load comes from a validated preset spawn point (spec 003) chosen by the floor scene;
  this spec only guarantees the *state* starts at `Patrol` — it does not choose or validate where
  in the world that is.

## Related

- GDD Ch. 6.1 (Monster AI Specification — State Machine) — primary source for the transition
  table this spec implements.
- GDD Ch. 6.2 — the numeric tuning values consumed as opaque input here; locked by spec 002.
- GDD Ch. 9.3 (Fail State Flow) — context for why `Caught` is deliberately excluded from this
  spec's enum (owned by spec 004).
- `specs/ROADMAP.md` §2 "monster-ai" row `001-state-machine-core-transitions`, and §5 dependency
  order (depends on `noise-and-detection/002`).
- `specs/systems/noise-and-detection/002-distance-based-detection-check/spec.md` — upstream
  producer of the detection signal this spec consumes (path cited per that spec's own naming; not
  redefined here).
- `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md` — downstream owner of the
  concrete `investigateDuration`/`chaseHoldDuration`/`searchDuration`/`patrolSpeed`/`chaseSpeed`
  numbers this spec treats as opaque input.
- `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` — governs where a `Monster`
  legally starts in `Patrol`; not this spec's concern.
- `specs/systems/monster-ai/004-catch-outcome-signal/spec.md` — downstream consumer of the
  `Freeze()`/`Reset()` hook exposed by FR-013.
