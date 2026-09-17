# Feature Specification: Eased Radius Transitions

**Feature Branch**: `003-eased-radius-transitions`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The DISPLAYED light radius eases smoothly toward 001's target radius
every frame instead of jumping — an exponential-decay ease, config-driven rate, with continuous
narrowing through the Critical range specifically."

## Why This Spec Exists

GDD Ch. 12.1 states this as a hard rule, not a nice-to-have: "Radius senter harus berubah dengan
easing (menyusut/membesar halus, tidak lompat sekaligus)... Radius senter berubah mengikuti Light
State..., dieased per frame, bukan diset langsung." `001-light-state-thresholds-and-radius`
already computes *what the radius should be right now* (the target) from charge — but a target
is not a displayed value. Setting the actual Unity `Light.range` directly to that target every
frame would make the light visibly snap at every light-state boundary crossing and would make
Critical's continuous narrowing (GDD Ch. 5.2 "Radius menyempit bertahap") look like the very thing
it's trying to avoid — a series of small jumps once per update instead of one smooth, continuous
shrink. This feature is the one place that owns the *displayed* radius: a single number that
chases 001's target, one frame at a time, and never equals the target discontinuously.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - The Displayed Radius Chases Its Target Smoothly, Never Jumps (Priority: P1)

Whenever the target radius changes — because charge crossed a light-state boundary, or because
charge is continuously falling through Critical — the light the player actually sees shrinks or
grows smoothly toward the new target over a short span of frames, rather than instantly becoming
the new value.

**Why this priority**: This is the entire feature. Every acceptance scenario below is a
consequence of this one rule holding at every possible target change, including the two hardest
cases: crossing a state boundary, and a continuously-moving target inside Critical.

**Independent Test**: In an EditMode test with no scene running, start a displayed radius at one
value, set a different target, and step the ease function forward by a fixed `deltaTime`
repeatedly; confirm the displayed value moves monotonically toward the target without ever
equaling it in a single step (unless already within a negligible epsilon) and without ever
overshooting past it.

**Acceptance Scenarios**:

1. **Given** the displayed radius equals the Normal target and charge crosses into Flickering
   (target radius is unchanged per 001 FR-005, since Flickering's base target equals Normal's),
   **When** observed, **Then** the displayed radius does not move at all — there is nothing to
   ease toward, confirming this feature only reacts to an actual target change, not a state-label
   change.
2. **Given** the displayed radius equals the Flickering-band target and charge crosses into
   Critical at the `lightStateCriticalStart` boundary, **When** observed over the following
   frames, **Then** the displayed radius begins shrinking smoothly rather than snapping to the new
   (still-equal-to-normal-at-that-exact-boundary) target.
3. **Given** charge is continuously falling through the Critical band (so 001's target radius is
   itself continuously shrinking, per 001 FR-006), **When** observed across many consecutive
   frames, **Then** the displayed radius shrinks continuously and smoothly, tracking a moving
   target without ever visibly stepping.
4. **Given** the displayed radius is at the Compact Darkness target and the player installs a
   spare battery (charge jumps back to 100%, per `008-battery-install-and-refill`), **When**
   observed, **Then** the displayed radius grows back toward the Normal target smoothly over a
   short span of frames rather than popping instantly to full size.
5. **Given** enough frames have passed for the displayed radius to be within a negligible
   distance of its target, **When** further frames pass with the target unchanged, **Then** the
   displayed radius settles at (or snaps the last negligible distance to) the target exactly,
   rather than asymptotically approaching forever without ever equaling it.

---

### Edge Cases

- **The target radius changes again before the displayed radius has caught up to the previous
  target** (e.g. charge is falling quickly through Critical, or a flicker dip from
  `004-flicker-event-system` is layered on top): the ease function MUST always ease from the
  *current displayed value* toward the *current target*, recomputed fresh each frame — it must
  never assume the previous target was reached, and must never require restarting an animation
  from scratch.
- **An extremely large single-frame `deltaTime`** (e.g. app resume from background): the ease
  function's math (exponential decay, `1 - exp(-rate * dt)`) naturally approaches (but never
  exceeds) 100% convergence to the target as `dt` grows large, so a large `dt` reads as "radius
  is now effectively at target," not an overshoot or a NaN/infinity — this feature's math must
  hold for any non-negative `dt`, including very large values.
- **`lightRadiusEaseRate` configured to `0`**: the displayed radius never moves (a `0` ease rate
  means 0% convergence per frame, regardless of `dt`) — this is a valid, if degenerate, config
  state and must not throw or divide by zero.
- **`deltaTime` of exactly `0`** (e.g. a paused frame that still ticks the update loop once):
  the displayed radius MUST NOT change.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST maintain a displayed light radius as a value distinct from 001's
  target radius, updated once per frame by easing toward whatever the current target is — the
  displayed radius MUST NOT ever be set directly to the target value except as the natural result
  of the ease converging (see FR-002).
- **FR-002**: The easing function MUST be an exponential decay toward the target: each frame, the
  displayed radius moves a fraction of the remaining distance to the target, where that fraction
  is determined by a config-driven ease rate and the frame's elapsed time — formally,
  `displayed += (target - displayed) * (1 - exp(-easeRate * deltaTime))`. This guarantees smooth,
  monotonic, non-overshooting convergence toward any target, moving or fixed, for any non-negative
  `deltaTime` (GDD Ch. 12.1: "tidak lompat sekaligus").
- **FR-003**: `GameConfig` MUST expose `lightRadiusEaseRate` (float, 1/s) controlling how fast the
  displayed radius chases its target. The GDD does not lock an exact number for this rate — it is
  feel content, tuned by playtesting. A starting default of `4.0` is carried over from this
  project's earlier native-engine on-device tuning pass (ROADMAP §0) **to re-confirm on-device**;
  it is not a settled final value.
- **FR-004**: Within the Critical light state, where 001's target radius is itself continuously
  changing as charge falls (001 FR-006), the displayed radius MUST still ease toward whatever the
  *current* target is each frame — producing a continuously narrowing, smooth visual shrink
  through the whole Critical range, not a value that catches up to one target only to immediately
  chase a new one in a visibly separate motion (GDD Ch. 5.2 "Radius menyempit bertahap").
- **FR-005**: The easing calculation MUST be time-based (uses elapsed real seconds since the last
  update), not frame-count-based, so the feel of the ease is the same regardless of frame rate —
  consistent with `002-battery-real-time-drain-timer`'s own real-time requirement.
- **FR-006**: The easing calculation MUST behave correctly (no exception, no `NaN`/`Infinity`, no
  overshoot past the target) for an ease rate of `0`, a `deltaTime` of `0`, and an unusually large
  `deltaTime`.
- **FR-007**: The pure easing math MUST live in a plain C# class under `Assets/Scripts/Systems/`
  with no `UnityEngine.MonoBehaviour`, `Component`, or scene dependency (constitution Principle
  III), taking `(currentDisplayedRadius, targetRadius, easeRate, deltaTime)` and returning the new
  displayed radius — callable from an EditMode test with no running scene, no `Light` component,
  and no `Battery`.
- **FR-008**: A thin `MonoBehaviour` adapter under `Assets/Scripts/MonoBehaviours/` MUST, once per
  frame: read the installed battery's charge fraction (from `002`), derive the current state and
  target radius (from `001`'s `LightStateSystem`), ease the displayed radius toward that target
  (via FR-002's pure function), and apply the result to the actual Unity `Light` component's
  `range` (or equivalent radius-controlling property). This adapter contains no easing math of its
  own — it only wires the pure function to Unity's per-frame update and to a real `Light`.

### Key Entities

- **Displayed Radius**: A single float, owned by this feature, distinct from 001's target radius.
  Not persisted beyond the current session/frame; recomputed continuously from whatever the
  previous displayed value and current target were.
- **GameConfig** *(extended)*: Gains `lightRadiusEaseRate`.
- **Light State / Target Radius**: Consumed from `001-light-state-thresholds-and-radius`, not
  redefined here.
- **Battery charge fraction**: Consumed from `002-battery-real-time-drain-timer`, not redefined
  here.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Across an EditMode simulation of charge draining continuously through the entire
  Critical band in small time steps, the displayed radius sequence is strictly monotonically
  non-increasing (never increases) and never equals the instantaneous target in a single step
  except in the final settling step — i.e., it visibly eases rather than steps.
- **SC-002**: When the target radius changes abruptly (a state-boundary crossing or an install
  refill), the displayed radius reaches within 1% of the new target within a bounded, config-rate
  -dependent number of simulated seconds, and never overshoots past the target at any intermediate
  step.
- **SC-003**: In an observation test with at least 2 people outside the development team watching
  the light shrink from Normal through Compact Darkness on a shortened test battery duration, each
  person describes the transition as "shrinking" or "smooth," not "jumping," "snapping," or
  "flickering" (the last of which would indicate 004's concern leaking into this feature).
- **SC-004**: `lightRadiusEaseRate` can be changed by editing only `GameConfig`, and the visible
  ease speed changes accordingly with no other file requiring a change.
- **SC-005**: The easing calculation produces a finite, non-`NaN` result for ease rate `0`,
  `deltaTime` `0`, and a `deltaTime` of at least 10 simulated seconds in a single step, verified by
  an EditMode test.

## Assumptions

- This feature reads the target radius via `001`'s `LightStateSystem` and the charge fraction via
  `002`'s `Battery`, but does not itself define either — per the category's scope note ("003
  depends on 001"), and per ROADMAP, its direct pure-logic dependency is `001`; the
  `MonoBehaviour` adapter (FR-008) necessarily also touches `002`'s `Battery` and `GameState` at
  the integration layer, which is expected and not a violation of the spec dependency graph.
- `004-flicker-event-system`'s transient dips are layered on top of (or interact with) this
  feature's displayed radius rather than replacing it — the exact composition order (does flicker
  modify the target this feature eases toward, or modify the already-eased displayed value?) is
  decided when `004` is planned, referencing this feature's output as its input.
- No shadow, fill-light, or HUD behavior is defined here — see `005`, `006`, `009`.

## Related

- [[ROADMAP]] — flashlight-and-battery row 3; depends on `001-light-state-thresholds-and-radius`
- [[constitution]] — Principle III (plain C# systems), Principle IV (EditMode tests)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 5.2 (Critical narrowing), Ch. 12/12.1 (Lighting System —
  eased radius requirement)
- `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius` — supplies the
  target radius and `LightState` this feature eases toward; not re-derived here
- `specs/systems/flashlight-and-battery/002-battery-real-time-drain-timer` — supplies the charge
  fraction this feature's target ultimately depends on
