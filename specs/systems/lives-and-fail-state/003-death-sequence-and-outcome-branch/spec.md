# Feature Specification: Death Sequence and Outcome Branch

**Feature Branch**: `003-death-sequence-and-outcome-branch`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 9.3 (Fail State Flow — the exact flowchart this spec implements), 9.1 (lives
hidden from HUD, checkpoint = start of floor), 9.2 (referenced only — what a reset restores, owned
by spec 002), 6.1 (CATCH — the state that produces the touch event), 10.4 (Bad Ending trigger)

**Input**: User description: "The exact fail-state flow per GDD 9.3: monster touches player →
death sequence plays (short, no QTE, same for exploration-catch or chase-catch) → lives − 1
(never shown to player) → if lives > 0, trigger 002's floor reset and respawn; if lives == 0,
trigger the Bad Ending. This spec is the single place that decides when a life is lost and which
of the two branches follows — it does not itself define what a reset restores (spec 002) or how
the lives counter is stored (spec 001)."

## Scope Note

This spec owns exactly one thing: the sequence of events between "the monster touches the player"
and "the player is either back at the checkpoint or watching the Bad Ending." It is the single
place in the codebase that decides *when* a life is lost and *which* of the two outcome branches
follows. It explicitly does not:

- Define the CATCH state or produce the touch event itself — that is
  `specs/systems/monster-ai/004-catch-outcome-signal/spec.md`'s job; this spec only consumes the
  signal it emits.
- Define what a floor reset actually restores — that is
  `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`'s job; this spec
  only calls it.
- Define how the lives counter is stored, read, or decremented at the data level, or what a
  checkpoint is — that is `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md`'s
  job; this spec only calls its decrement and "has lives remaining" operations.
- Define the content of the Bad Ending itself (its scene, narrative beats, visuals) — that is
  `specs/scenes/008-bad-ending-scene/spec.md`'s job (not yet written); this spec only triggers the
  transition into it.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Every Catch Plays the Same Short Death Sequence (Priority: P1)

The instant the monster touches the player — whether the player was casually exploring or in the
middle of a full chase — a short death sequence plays: no quick-time event, no chance to escape,
and visually/behaviorally identical regardless of which kind of catch produced it.

**Why this priority**: This is the player-visible moment the entire fail-state chain hinges on,
and GDD 9.3 is explicit that there must be no branching by catch context ("Tidak ada perbedaan
animasi/kamera antara tertangkap saat eksplorasi dan tertangkap saat chase. Satu death sequence
untuk semua kasus."). Getting this wrong — even by accident, e.g. a chase-camera state that lingers
into the death sequence — breaks a rule the GDD states in its most emphatic language in this
chapter.

**Independent Test**: In a test harness, fire the CATCH signal (spec `monster-ai/004`) twice —
once with player/monster state that reflects a plain exploration catch, once with state
reflecting an active chase — and confirm both trigger the identical death-sequence entry point
with no distinguishing parameter reaching it.

**Acceptance Scenarios**:

1. **Given** the player is caught while simply exploring (no active `Chase` state), **When** the
   CATCH signal fires, **Then** the death sequence begins immediately, with no quick-time event
   and no player input accepted to avert it.
2. **Given** the player is caught while the monster is actively `Chase`ing, **When** the CATCH
   signal fires, **Then** the exact same death sequence entry point runs — this spec's outcome
   logic MUST NOT receive or branch on "how" the catch happened, only that it happened.
3. **Given** the death sequence is playing, **When** its duration elapses, **Then** it MUST
   proceed to the lives decrement (User Story 2) exactly once — the sequence cannot be
   interrupted, skipped, or replayed by any player input.

---

### User Story 2 - A Life Is Always Lost, Silently (Priority: P1)

Immediately after the death sequence completes, exactly one life is deducted from the player's
hidden lives counter — an amount the player is never shown, matching the GDD's explicit design
that the counter never surfaces during gameplay.

**Why this priority**: This is the mechanical core of the fail state — without a reliable,
exactly-once decrement, the whole "3 lives, then Bad Ending" design (GDD 9.1) has no floor under
it. Tied at P1 with User Story 1 because a correct-looking death sequence that fails to decrement
(or double-decrements) is just as broken as no death sequence at all.

**Independent Test**: Trigger the death sequence to completion once and confirm the lives counter
(`lives-and-fail-state/001`) decreases by exactly 1, with no lives indicator appearing on any
screen during or after the sequence.

**Acceptance Scenarios**:

1. **Given** the death sequence has just completed, **When** the outcome logic runs, **Then** it
   calls the lives decrement operation (`lives-and-fail-state/001`) exactly once — never zero
   times, never twice.
2. **Given** the decrement has occurred, **When** any UI surface active during or immediately
   after the death sequence is inspected, **Then** no lives count, heart icon, or "lives
   remaining" text is shown (consistent with `lives-and-fail-state/001`'s FR-006).
3. **Given** two separate deaths occur in the same run, **When** each completes its own death
   sequence, **Then** the lives counter reflects exactly two total decrements, not one merged
   decrement or a decrement lost to a race between the two events.

---

### User Story 3 - Lives Remain: Reset and Respawn (Priority: P1)

When the post-decrement lives count is still greater than zero, the player is brought back into
the game: the current floor's entire state resets to its start-of-floor condition and the player
respawns at the floor's checkpoint, ready to try again.

**Why this priority**: This is the "try again" branch that makes up the overwhelming majority of
actual fail-state occurrences in a normal run (at most 2 of a run's up-to-3 catches ever reach the
alternative branch). Tied at P1 with User Stories 1–2 because a death sequence and decrement that
never actually respawn the player leave the game unplayable past the first catch.

**Independent Test**: With the lives counter fixture-configured to have more than one life
remaining, trigger a full death sequence to completion and confirm (a) the floor-reset operation
(`lives-and-fail-state/002`) was called exactly once, and (b) the player ends the sequence at the
floor's checkpoint, not the Bad Ending.

**Acceptance Scenarios**:

1. **Given** the post-decrement lives count is greater than zero, **When** the outcome branch
   runs, **Then** it calls `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`'s
   reset operation exactly once, and the Bad Ending transition is never triggered.
2. **Given** the reset has completed, **When** control returns to the player, **Then** the player
   is positioned at the current floor's checkpoint and normal gameplay resumes (movement, noise
   detection, and the monster's `Patrol` state are all active again).
3. **Given** the lives count is greater than zero after decrement, **When** the branch decision is
   evaluated, **Then** it depends only on the "has lives remaining" query from
   `lives-and-fail-state/001` — never on any other signal (e.g., which floor the player is on,
   how the catch happened).

---

### User Story 4 - Lives Are Exhausted: the Bad Ending (Priority: P1)

When the post-decrement lives count reaches exactly zero, the player is not respawned at all —
instead, the run ends immediately by transitioning to the Bad Ending.

**Why this priority**: This is the run-ending branch and the other half of GDD 9.1's core
design ("Lives habis = Bad Ending"). Equal priority to User Story 3 because both branches of the
same decision must be correct together — a system that only ever resets and never reaches the Bad
Ending (or vice versa) is an incomplete implementation of the same flowchart, not a lower-priority
add-on.

**Independent Test**: With the lives counter fixture-configured to have exactly one life
remaining, trigger a full death sequence to completion and confirm (a) the floor-reset operation
is never called, and (b) the transition to the Bad Ending scene is triggered exactly once.

**Acceptance Scenarios**:

1. **Given** the post-decrement lives count equals zero, **When** the outcome branch runs,
   **Then** it triggers the transition into the Bad Ending (`specs/scenes/008-bad-ending-scene/spec.md`)
   exactly once, and the floor-reset operation (`lives-and-fail-state/002`) is never called.
2. **Given** the Bad Ending transition has been triggered, **When** the current run's state is
   inspected afterward, **Then** the floor the player died on is left in its post-death (not
   reset) state — this spec MUST NOT reset floor state on the path that leads to the Bad Ending,
   since there is no respawn to place the player at.
3. **Given** the lives count reaches exactly zero, **When** the branch decision is evaluated,
   **Then** it depends only on the same "has lives remaining" query used by User Story 3 — the two
   branches are a single if/else over one boolean, never two independently-derived checks that
   could disagree.

---

### Edge Cases

- **Dying mid-hiding**: the CATCH signal this spec consumes only ever fires when the monster
  actually touches the player (per `monster-ai/004` and GDD 4.3's hiding-immunity rule: a hidden
  player generally cannot be found, except when hiding while already in a directly-observed
  `Chase`). If a catch does occur while the player was hiding, this spec's flow is entirely
  unaffected by that fact — the death sequence, decrement, and branch all proceed identically to
  any other catch. This spec adds no "were they hiding" branch of its own.
- **Dying mid-battery-install**: if the CATCH signal fires while an install-from-spare-slot
  interaction was in progress, this spec does not need to know or care — the death sequence begins
  immediately regardless of any other in-progress interaction, and the eventual floor reset (spec
  002) is what defines the post-reset battery state, not this spec.
- **Dying at exactly 0 lives on the very first catch**: if `GameConfig.lives` were ever configured
  to `1` (a non-default/test configuration — the shipped value is locked at 3 per GDD 9.1/17.5),
  the very first catch of the run MUST decrement lives to 0 and MUST correctly route to the Bad
  Ending on that first occurrence — there is no assumption anywhere in this spec's branch logic
  that more than one catch has to occur first. The branch decision is evaluated fresh every time,
  from the current post-decrement value alone.
- **A second CATCH signal fires while a death sequence is already playing**: the outcome logic
  MUST treat the death sequence as already "in progress" and ignore/queue-reject any further CATCH
  signal until the current sequence's decrement and branch have fully resolved — this prevents a
  double-decrement from two touches registering in the same frame or during the brief window
  before the reset repositions the player out of the monster's reach.
- **The floor-reset operation itself fails or cannot complete** (e.g., a sibling system error
  inside spec 002): out of scope for this spec to recover from gracefully beyond calling the
  operation once per FR — this spec's own contract ends at "reset was invoked exactly once when
  lives remain"; the atomicity/correctness of what happens inside that call is spec 002's
  responsibility.
- **The Bad Ending transition itself is retried or the scene fails to load**: out of scope for this
  spec — it is responsible only for triggering the transition exactly once when lives reach zero;
  the receiving scene's own robustness is `specs/scenes/008-bad-ending-scene/spec.md`'s concern.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST begin the death sequence the instant a CATCH signal is received from
  `specs/systems/monster-ai/004-catch-outcome-signal/spec.md`, with no gating condition, no
  quick-time event, and no player input capable of averting, skipping, or delaying it (GDD 9.3:
  "tanpa QTE, tanpa kesempatan lolos").
- **FR-002**: The death sequence MUST be identical regardless of whether the CATCH signal resulted
  from an exploration-context catch or a chase-context catch. This spec's outcome logic MUST NOT
  accept, branch on, or otherwise use any parameter describing how the catch occurred — the CATCH
  signal's payload consumed here is limited to "a catch happened," per `monster-ai/004`'s contract
  (GDD 9.3: "Tidak ada perbedaan animasi/kamera... Satu death sequence untuk semua kasus").
- **FR-003**: The death sequence MUST be short by design intent — this spec does not fabricate an
  exact duration number; the concrete duration is a feel value read from a `GameConfig` field
  (`deathSequenceDuration`, seconds) and validated by playtesting, not hardcoded here (ROADMAP §0
  feel-value convention).
- **FR-004**: Upon the death sequence completing, the system MUST call
  `lives-and-fail-state/001`'s decrement operation exactly once. This call MUST happen exactly
  once per completed death sequence — never zero times (a life must always be lost on a catch)
  and never more than once (no double-decrement).
- **FR-005**: The system MUST NOT expose, render, or otherwise surface the lives count or the fact
  that a decrement occurred to the player through any UI, camera, or audio cue during or after the
  death sequence (consistent with `lives-and-fail-state/001` FR-006).
- **FR-006**: Immediately after the decrement (FR-004), the system MUST query
  `lives-and-fail-state/001`'s "has lives remaining" operation exactly once and use that single
  boolean result to select exactly one of the two branches below — FR-007 or FR-008. No other
  signal (floor identifier, catch context, elapsed run time) may influence this branch selection.
- **FR-007**: If "has lives remaining" is `true`, the system MUST call
  `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`'s
  `ResetCurrentFloor` operation exactly once, and MUST NOT trigger the Bad Ending transition on
  this path. After the reset call returns, normal gameplay MUST resume with the player positioned
  at the checkpoint the reset established.
- **FR-008**: If "has lives remaining" is `false`, the system MUST trigger the transition into the
  Bad Ending (`specs/scenes/008-bad-ending-scene/spec.md`) exactly once, and MUST NOT call
  `lives-and-fail-state/002`'s reset operation on this path — there is no floor to respawn onto.
- **FR-009**: While a death sequence is in progress (from CATCH signal received through branch
  resolution), the system MUST ignore or reject any further CATCH signal that arrives, so that a
  second near-simultaneous touch can never cause a second decrement or a second branch evaluation
  for the same death.
- **FR-010**: The sequencing in FR-001, FR-004, FR-006 MUST always occur in that fixed order —
  death sequence, then decrement, then branch query — for every single catch, with no path that
  skips or reorders a step (GDD 9.3's flowchart is a strict linear sequence with one binary
  branch at the end, not a set of independently triggerable effects).
- **FR-011**: This spec's outcome logic MUST be implemented as plain C# orchestration under
  `Assets/Scripts/Systems/` with no `MonoBehaviour`/`Component`/scene-graph dependency
  (constitution Principle III). The death sequence's actual presentation (animation, camera cut,
  audio) is driven by a thin `MonoBehaviour` adapter under `Assets/Scripts/MonoBehaviours/` that
  the plain C# logic signals to start/stop — the branch decision itself never lives in that
  adapter.
- **FR-012**: A new `GameConfig` field, `deathSequenceDuration` (seconds), is introduced by this
  spec per FR-003. No other new numeric `GameConfig` key is required by this spec — the lives
  counter and checkpoint fields already exist via `lives-and-fail-state/001`.

### Key Entities

- **Death Sequence**: A short, non-interruptible, catch-context-independent presentation beat that
  begins the instant a CATCH signal is received and ends after `GameConfig.deathSequenceDuration`
  seconds, after which the outcome logic proceeds to the decrement.
- **Outcome Branch**: A single evaluation, performed exactly once per completed death sequence,
  of `lives-and-fail-state/001`'s "has lives remaining" boolean immediately after the decrement.
  Not a persisted entity — a one-shot decision that selects exactly one of two calls (floor reset
  vs. Bad Ending transition).
- **DeathSequenceGuard**: A transient in-progress flag preventing a second CATCH signal from
  starting a second death sequence, decrement, or branch evaluation before the current one has
  fully resolved (FR-009).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In automated tests covering both an exploration-context CATCH signal and a
  chase-context CATCH signal, the resulting death-sequence entry point call is identical in both
  cases in 100% of test runs — no distinguishing parameter reaches the outcome logic.
- **SC-002**: In automated tests, every completed death sequence results in exactly one call to
  the lives decrement operation — never zero, never more than one — across 100% of trials,
  including a trial where two CATCH signals fire in immediate succession (FR-009 guard).
- **SC-003**: In automated tests parameterized over "lives remaining after decrement" values of
  {1, 2, 3} (all `> 0`), the floor-reset operation is called exactly once and the Bad Ending
  transition is never triggered, in 100% of trials.
- **SC-004**: In automated tests with "lives remaining after decrement" fixed at exactly 0
  (including the boundary case of the very first catch when `GameConfig.lives` is
  fixture-configured to 1), the Bad Ending transition is triggered exactly once and the
  floor-reset operation is never called, in 100% of trials.
- **SC-005**: In a screen-by-screen audit of a full run containing at least two deaths, no lives
  count or decrement indicator appears on any screen during or immediately after any death
  sequence — consistent with, and re-verifying, `lives-and-fail-state/001`'s SC-003.

## Assumptions

- The CATCH signal's shape and the exact moment it fires are entirely defined by
  `specs/systems/monster-ai/004-catch-outcome-signal/spec.md`; this spec only assumes that signal
  carries enough information to know "a catch happened," and explicitly does not assume (or want)
  it to carry catch-context detail for branching purposes (FR-002).
- The Bad Ending scene's own content, narrative, and loading behavior are entirely owned by
  `specs/scenes/008-bad-ending-scene/spec.md`, which does not yet exist as of this spec's
  authoring — this spec only requires that a callable "trigger the Bad Ending transition"
  operation exists or is stubbed against that future spec's expected contract (mirroring the same
  forward-reference precedent already used elsewhere in this vault, e.g.
  `battery-spawn-system/001` citing `flashlight-and-battery/007` before it existed).
- The floor-reset operation's own atomicity/correctness (whether every piece of floor state is
  actually restored) is entirely owned and tested by
  `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`; this spec only
  asserts that operation is called exactly once on the correct branch.
- `deathSequenceDuration`'s concrete number is left to on-device playtest feel (ROADMAP §0), same
  as other feel-based timings elsewhere in this vault (e.g., `lockedDoorFeedbackCooldown` in
  `keys-and-doors/002`) — this spec only requires the rule ("short, no QTE, config-driven") and
  the field, not a fabricated final value.

## Dependencies

- **Requires**: `specs/systems/monster-ai/004-catch-outcome-signal/spec.md` (the CATCH signal this
  spec consumes), `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md`
  (decrement and "has lives remaining" operations), `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`
  (the reset operation called on the "lives remain" branch), `specs/scenes/008-bad-ending-scene/spec.md`
  (the transition target on the "lives exhausted" branch — cited by path though not yet written,
  per ROADMAP §3 row `008-bad-ending-scene`).
- **Consumed by**: `specs/systems/haptics/001-haptic-trigger-events/spec.md` (per ROADMAP §2, the
  haptics feature fires on this spec's death-sequence/CATCH moment), `specs/systems/audio/002-dynamic-mix-state-machine/spec.md`
  (death-sequence audio cue), and any progression/scene-flow logic that needs to know a run has
  ended via the Bad Ending path.

## Related

- GDD: `specs/_reference/LILO-GDD-v2-Production-Lock.md` — Ch. 9.3 (the exact flowchart this spec
  implements, quoted verbatim in the Input above), 9.1, 9.2, 6.1, 10.4.
- `specs/ROADMAP.md` — §2 `lives-and-fail-state` row `003-death-sequence-and-outcome-branch`
  (depends on `001`, `002`, `monster-ai/004`); §3 scenes row `008-bad-ending-scene` (depends on
  `narrative-content/001`, `lives-and-fail-state/003` — this spec); §5 build order.
- `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md` — the orchestrator
  this spec calls on the "lives remain" branch.
- `specs/scenes/008-bad-ending-scene/spec.md` — the (not-yet-written) transition target on the
  "lives exhausted" branch.
