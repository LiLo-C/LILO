# Feature Specification: Flicker Event System

**Feature Branch**: `004-flicker-event-system`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Discrete flicker EVENTS (steady → dip → steady, config-driven
interval/duration/depth) only while in the Flickering state — explicitly NOT a new random value
every frame — and a dip must cancel immediately if the state changes mid-dip."

## Why This Spec Exists

GDD Ch. 12.1 draws a hard line the team has already learned the cost of crossing once (see the
project's own prior feel-pass writeup): "Efek flicker HANYA muncul di state Flickering... sebagai
event diskrit (turun sebentar lalu stabil), bukan nilai random tiap frame. Jangan dipakai
terus-menerus — flicker konstan berubah jadi noise visual yang bikin lelah, bukan sinyal." Flicker
is meant to be read by the player as a warning signal (GDD Ch. 5.2: "SFX kedip. Ini sinyal
peringatan buat player"), not ambient visual noise. That only works if it is a small number of
distinct, separated events, not continuous jitter. This feature owns exactly that: a state machine
that produces occasional, discrete dip events only while the light is in the `Flickering` state,
and nothing outside it.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Flicker Only Happens as Separated, Discrete Events (Priority: P1)

While the flashlight is in the Flickering state, the player occasionally sees the light dip
briefly and then return to steady — a handful of distinct "blips," never a constant flutter.

**Why this priority**: This is the entire point of the feature and the exact failure mode the
GDD calls out by name ("flicker konstan berubah jadi noise visual"). Getting this wrong makes the
warning signal illegible.

**Independent Test**: In an EditMode test with no scene running, drive the flicker state machine
forward through many simulated seconds while forcing the light state to `Flickering`, and confirm
the sequence of dip events it produces has config-bounded gaps between them and config-bounded
durations, never overlapping and never continuous.

**Acceptance Scenarios**:

1. **Given** the light state is `Flickering`, **When** observed over a span of real time, **Then**
   the light alternates between a steady phase and brief dip phases, with each dip separated from
   the next by at least `GameConfig.flickerIntervalMin` seconds of steady light.
2. **Given** the light state is `Flickering` and a dip event begins, **When** observed, **Then**
   the dip lasts approximately `GameConfig.flickerEventDuration` seconds before returning to
   steady — it does not persist indefinitely and does not end instantly with no perceptible
   duration.
3. **Given** the light state is `Flickering` for a long span of time, **When** the sequence of
   dip events is sampled, **Then** no two dip events overlap, and the state machine is always in
   exactly one of two phases at any instant: `Steady` or `Dipping` — never an undefined third
   state.
4. **Given** repeated runs of the same simulated duration, **When** compared, **Then** the flicker
   state machine does not require re-evaluating a fresh random dip depth or duration every single
   frame — a dip's depth and duration are decided once, when that dip begins, not resampled while
   it plays (this is what "discrete event" means as opposed to "random value every frame").

---

### User Story 2 - Flicker Never Happens Outside the Flickering State (Priority: P1)

If the light is in Normal, Critical, or Compact Darkness, no flicker dip ever occurs, no matter
how long time passes.

**Why this priority**: GDD Ch. 12.1 is explicit: "Efek flicker HANYA muncul di state Flickering."
A dip appearing in any other state is not a lesser bug — it directly contradicts the one state
this effect is supposed to signal.

**Independent Test**: In an EditMode test, drive the flicker state machine forward through many
simulated seconds while forcing the light state to `Normal` (and separately, `Critical`, and
`CompactDarkness`), and confirm zero dip events ever start.

**Acceptance Scenarios**:

1. **Given** the light state is `Normal`, **When** any amount of simulated time passes, **Then**
   no dip event ever begins.
2. **Given** the light state is `Critical`, **When** any amount of simulated time passes, **Then**
   no dip event ever begins — Critical has its own continuous-narrowing visual language (003) and
   does not additionally flicker.
3. **Given** the light state is `CompactDarkness`, **When** any amount of simulated time passes,
   **Then** no dip event ever begins.
4. **Given** a dip event is currently in progress and the light state changes away from
   `Flickering` (e.g. charge crossed into `Critical`, or a spare battery was installed and charge
   jumped to `Normal`), **When** that state change is detected, **Then** the in-progress dip is
   cancelled immediately — the light returns to steady (undipped) on the very next update, not
   after the dip's remaining duration plays out.

---

### Edge Cases

- **Light state changes from something else directly into `Flickering` mid-way through what would
  have been a steady interval**: the state machine MUST start its steady-interval countdown fresh
  from the moment `Flickering` begins — it does not "remember" or resume a countdown from a
  previous, unrelated time spent in `Flickering` (e.g. if charge oscillated in and out of the band
  due to an install/drain sequence).
- **The state machine is queried on the very first frame it enters `Flickering`**: it MUST report
  `Steady` (not mid-dip) — a dip is never already "primed" to fire on the first possible frame;
  the first steady interval must elapse first.
- **`flickerIntervalMin` is configured greater than `flickerIntervalMax`**: this is a
  misconfiguration, not a runtime condition this feature must gracefully interpret in a specific
  way beyond not crashing — see FR-006's ordering requirement, which is a `GameConfig` authoring
  concern, not a runtime fallback this feature computes.
- **`flickerDipFraction` is configured at `0`**: dips still occur as discrete events (steady →
  "dip" → steady) but the dip has no visible depth — a valid, if pointless, config state that must
  not crash.
- **`flickerDipFraction` is configured at `1`**: the dip reduces the light to nothing for its
  duration — still a valid config state.
- **The dip's randomized interval/duration/depth values are queried by an external caller
  mid-dip** (e.g. a HUD or audio cue system wants to know "is a dip happening right now, and how
  deep"): the state machine MUST expose the current phase and, while dipping, the current dip's
  depth fraction, as a queryable read, not only as a one-shot event callback — so haptic/audio
  cues (owned by other categories) can react to the same discrete event without this feature
  needing to know about them.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST model flicker as a two-phase state machine — `Steady` and
  `Dipping` — evaluated once per update against elapsed real time, never resampling a new random
  value every single frame while in either phase.
- **FR-002**: Flicker dip events MUST occur only while the current light state (from
  `001-light-state-thresholds-and-radius`) is `Flickering`. In any other light state, the
  flicker state machine MUST remain in `Steady` and MUST NOT begin a new dip, regardless of
  elapsed time.
- **FR-003**: While in `Steady` phase during `Flickering`, the system MUST wait a randomized
  interval before starting the next dip, drawn from
  `[GameConfig.flickerIntervalMin, GameConfig.flickerIntervalMax]` seconds — a new random interval
  drawn once per dip cycle, not once per frame.
- **FR-004**: A dip event, once started, MUST last exactly `GameConfig.flickerEventDuration`
  seconds before automatically returning to `Steady`, with its depth fixed at the moment the dip
  began (drawn from a depth informed by `GameConfig.flickerDipFraction`) — not re-randomized while
  the dip is in progress (GDD Ch. 12.1: "bukan nilai random tiap frame").
- **FR-005**: If the light state transitions away from `Flickering` while a dip is in progress,
  the dip MUST be cancelled immediately: the very next query of the flicker state MUST report
  `Steady` with no residual dip effect, regardless of how much of `flickerEventDuration` had
  elapsed.
- **FR-006**: `GameConfig` MUST expose `flickerIntervalMin`, `flickerIntervalMax`,
  `flickerEventDuration` (all float, seconds), and `flickerDipFraction` (float, 0–1) as the only
  tunables this feature reads. None of these are locked by the GDD as exact numbers — they are
  feel content. Starting defaults of `0.4`, `1.4`, `0.15`, and `0.45` respectively are carried
  over from this project's earlier native-engine on-device tuning pass (ROADMAP §0) **to
  re-confirm on-device**; none are settled final values.
- **FR-007**: The flicker state machine MUST expose its current phase (`Steady`/`Dipping`) and,
  while `Dipping`, the active dip's depth fraction, as a readable value any other system can
  query on demand (e.g. to modulate the displayed radius from `003`, trigger an SFX cue, or drive
  a haptic pulse) — not only as a fire-once event callback.
- **FR-008**: The flicker state machine's logic MUST live in a plain C# class under
  `Assets/Scripts/Systems/` with no `UnityEngine.MonoBehaviour`, `Component`, or scene dependency
  (constitution Principle III), taking the current `LightState` and elapsed time as input and
  requiring no `Light` component or running scene to test.
- **FR-009**: Randomization (the interval draw and the dip-depth draw) MUST be injectable/
  seedable for testing purposes — an EditMode test MUST be able to drive the state machine
  deterministically rather than depending on true randomness, so acceptance scenarios like "no
  two dips overlap" are verifiable, not merely probable.

### Key Entities

- **FlickerPhase**: An enum with two values — `Steady`, `Dipping`. Owned by this feature.
- **Flicker state machine**: Holds the current phase, time remaining in that phase, and (while
  `Dipping`) the current dip's fixed depth fraction. Reset to `Steady` with a freshly-drawn
  interval whenever the light state enters `Flickering` from any other state, and forced to
  `Steady` immediately whenever the light state leaves `Flickering` (FR-005).
- **GameConfig** *(extended)*: Gains `flickerIntervalMin`, `flickerIntervalMax`,
  `flickerEventDuration`, `flickerDipFraction`.
- **Light State**: Consumed from `001-light-state-thresholds-and-radius`, not redefined here.
- **Displayed Radius**: Consumed/modulated in conjunction with `003-eased-radius-transitions`; the
  exact composition (this feature's dip depth is applied as a multiplier on top of `003`'s eased
  displayed radius and/or light intensity) is an integration decision made when this feature is
  planned, referencing `003`'s public output.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Across a simulated 60-second span with the light state forced to `Flickering`
  throughout, the flicker state machine produces a sequence of dip events where every gap between
  consecutive dip start times is at least `flickerIntervalMin` seconds, and every dip's duration
  is exactly `flickerEventDuration` seconds, verified by an automated EditMode test with a seeded
  random source.
- **SC-002**: Across a simulated 60-second span with the light state forced to any of `Normal`,
  `Critical`, or `CompactDarkness` throughout, zero dip events occur.
- **SC-003**: When the light state is forced to change away from `Flickering` at a randomly chosen
  moment strictly inside an in-progress dip's duration, the very next state-machine query reports
  `Steady`, in 100% of sampled cases across at least 20 randomized trials.
- **SC-004**: In an observation test with at least 2 people outside the development team watching
  the Flickering state for at least 30 seconds, each person describes what they see as "flickers"
  or "blips" (distinct events), not "shimmering," "glitching," or "constant noise."
- **SC-005**: All four flicker tunables can be changed by editing only `GameConfig`, with the
  visible flicker rhythm changing accordingly and no other file requiring a change.

## Assumptions

- The exact visual/audio effect a dip produces (does it shrink the displayed radius, dim
  intensity, or both?) is not locked by this spec beyond "a brief dip in brightness and/or reach"
  per GDD Ch. 12.1 — the state machine's public contract is phase + depth fraction; how a
  `MonoBehaviour` adapter applies that depth to the actual `Light` (in combination with `003`'s
  eased displayed radius) is decided at planning time, not fixed here.
- SFX ("kedip" cue, GDD Ch. 14.1) and haptic feedback (GDD Ch. 15.2) triggered by a flicker event
  are owned by the `audio` and `haptics` categories respectively; this feature only exposes the
  phase/depth signal they would read (FR-007), it does not itself play sound or trigger haptics.
- This feature does not decide how often it is polled (once per frame is expected, via the same
  `MonoBehaviour` adapter pattern as `003`, but that integration detail belongs to planning, not
  this spec).

## Related

- [[ROADMAP]] — flashlight-and-battery row 4; depends on `001-light-state-thresholds-and-radius`
  and `003-eased-radius-transitions`
- [[constitution]] — Principle III (plain C# systems), Principle IV (EditMode tests)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 5.2 (Flickering state, warning signal), Ch. 12.1 (discrete
  flicker events, not per-frame randomness), Ch. 14.1 (SFX kedip), Ch. 15.2 (haptic feedback)
- `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius` — supplies the
  `LightState` this feature gates on
- `specs/systems/flashlight-and-battery/003-eased-radius-transitions` — supplies the displayed
  radius this feature's dips are composed with
