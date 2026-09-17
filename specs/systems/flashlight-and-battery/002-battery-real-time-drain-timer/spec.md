# Feature Specification: Battery Real-Time Drain Timer

**Feature Branch**: `002-battery-real-time-drain-timer`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The installed battery's charge drains in real wall-clock time
(Time.deltaTime-based, NOT frame-count-based), reaching 0 at exactly the GDD's 180 seconds
regardless of frame rate or player action — sprinting must not drain faster."

## Why This Spec Exists

GDD Ch. 5.1 is explicit and non-negotiable on this point: "Durasi satu battery: 180 detik.
Real-time, bukan game-time" and "Drain saat idle: Tetap berjalan. Waktu berjalan absolut. Diam
tidak menghemat battery" — plus the deliberately-rejected "Sprint drain battery: TIDAK ADA.
Sengaja ditolak" (Ch. 4.1). This feature is the one place that owns the `Battery` entity's charge
value and the rule for how it decreases: by elapsed wall-clock seconds only, never by frame count,
and never modified by what the player is doing. `001-light-state-thresholds-and-radius` already
owns turning a charge fraction into a state and target radius — this feature owns producing that
charge fraction in the first place, in real time.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A Full Battery Drains to Empty in Exactly 180 Seconds (Priority: P1)

A player installs a fresh battery and, without ever swapping it, watches (or simply lets time
pass) as its charge counts down from full to empty in exactly 180 real-world seconds — not 180
frames, not "about" 180 seconds depending on device performance.

**Why this priority**: This is the entire premise of the game's title. If the timer is
frame-rate-dependent, the same 180-second battery would empty faster on a low-end device (fewer,
larger `Time.deltaTime` steps accumulating error) or slower/faster inconsistently across runs on
the same device — breaking the tuned pressure curve the whole GDD is built around (Ch. 20.2's own
Phase 1 test: "Tim merasa 180 detik terasa masuk akal").

**Independent Test**: Instantiate a `Battery` at full charge in an EditMode test, advance it with a
fixed sequence of `deltaTime` values that sum to exactly 180 seconds (regardless of how many
individual steps make up that sum), and confirm the resulting charge is exactly `0` — fully
testable without a running scene or real wall-clock wait.

**Acceptance Scenarios**:

1. **Given** a battery is installed at 100% charge, **When** 180 seconds of wall-clock time pass
   with no other action taken, **Then** its charge reaches exactly `0%`.
2. **Given** the same 180 seconds are simulated as many small steps (e.g. 10,800 steps of
   1/60s, representing 60 FPS), **When** compared against the same 180 seconds simulated as few
   large steps (e.g. 600 steps of 0.3s, representing a stuttering 3–4 FPS), **Then** both
   sequences reach exactly the same final charge — the result depends on the sum of elapsed time,
   not on how many update steps carried it.
3. **Given** a battery is installed at 100% charge, **When** the game is paused (zero elapsed
   time passed to the drain, e.g. during the pause menu) for some duration, **Then** charge does
   not decrease during that duration — pausing is the one legitimate way to stop the real-time
   clock, and it is out of this feature's scope to define (owned by `game-shell-ui`); this
   feature only guarantees drain is proportional to time actually passed to it.

---

### User Story 2 - Drain Rate Is Identical Regardless of Player Action (Priority: P1)

A player who stands still the entire time drains exactly as much charge as a player who sprints,
walks, interacts with objects, or hides — for the same elapsed wall-clock time.

**Why this priority**: GDD Ch. 4.1 explicitly and deliberately rejects a sprint-drains-battery
rule ("tidak ada hubungan logis antara lari dan baterai senter") and Ch. 4.3 explicitly keeps
drain running during hiding ("Battery tetap berkurang saat hiding — supaya hiding bukan tempat
aman tak terbatas"). Getting this wrong either makes sprinting a hidden trap or makes hiding a
free, unlimited-duration safe room — both contradict the GDD directly.

**Independent Test**: Advance a `Battery` by the same elapsed time twice in an EditMode test —
once tagged as "idle" and once as "sprinting" (the drain method takes only elapsed time, with no
movement-state parameter at all) — and confirm the resulting charge is identical in both cases.

**Acceptance Scenarios**:

1. **Given** a battery is installed and the player is standing still, **When** 30 seconds pass,
   **Then** charge decreases by exactly the same amount as if the player had been sprinting for
   those same 30 seconds.
2. **Given** a battery is installed and the player is hiding under a desk, **When** time passes,
   **Then** charge continues to decrease at the same rate as when not hiding (GDD Ch. 4.3).
3. **Given** a battery is installed and the player interacts with an object (opens a door, picks
   up an item), **When** that interaction takes some amount of time, **Then** charge decreases
   only by that elapsed wall-clock amount — the interaction itself has no separate drain cost or
   bonus.

---

### Edge Cases

- **A single frame's `deltaTime` is unusually large** (e.g. the app resumes from background after
  minutes away, or the Editor hits a breakpoint): draining the full elapsed time in one step would
  instantly zero out or deeply overdraw the charge in a way that reads as a bug rather than
  "time passed while suspended." This feature's own drain math MUST still be correct for whatever
  elapsed time it is given (it is a pure function of `(currentCharge, deltaTime, batteryDuration)`
  with no frame-rate assumption) — clamping an unreasonably large single-frame `deltaTime` before
  it reaches this feature, if desired, is a `MonoBehaviour` adapter integration concern, not a
  rule this feature enforces itself.
- **Charge would go below zero from a single drain step** (elapsed time in that step exceeds
  remaining charge): charge MUST clamp at exactly `0`, never negative — a negative charge would
  break `001`'s clamped-but-still-defined charge fraction contract.
- **No battery is currently installed** (e.g. before the first battery is ever installed, if that
  state is reachable): draining MUST be a no-op with no exception — there is nothing to drain.
  Whether an "no battery installed" state is reachable in practice is decided by
  `007-battery-pickup-and-spare-slot`/`008-battery-install-and-refill`, not this feature.
- **`GameConfig.batteryDuration` is changed at runtime** (e.g. a debug/tuning tool): this feature
  always computes drain rate as `1 second of charge lost per 1 second of elapsed time` — i.e. the
  charge value itself is stored and drained in seconds, not as a pre-computed fraction — so a
  runtime change to `batteryDuration` affects the *fraction* derived from the current seconds
  value on the next read, without needing to rescale already-drained charge.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST drain the installed `Battery`'s remaining charge by exactly the
  amount of real elapsed wall-clock time passed to it, in seconds — never by a fixed amount per
  frame/tick regardless of that frame's duration (i.e. NOT `charge -= 1 per frame`).
- **FR-002**: A battery starting at full charge MUST reach exactly `0` after exactly
  `GameConfig.batteryDuration` (locked default `180`, seconds, per GDD Ch. 17.2) seconds of
  cumulative elapsed time have been passed to it, regardless of how that cumulative time is split
  across individual update steps (one call summing to 180s and many calls summing to 180s MUST
  produce the identical final result).
- **FR-003**: `GameConfig` MUST expose `batteryDuration` (float, seconds, locked default `180`) as
  the single source for full-battery duration — no other file may hardcode `180` or any derived
  drain-rate constant.
- **FR-004**: The drain calculation MUST take no parameter describing player action, movement
  state, or which system/interaction triggered the update — its only inputs are the battery's
  current charge and the elapsed time to apply. This is what guarantees FR-005/FR-006 below by
  construction rather than by convention.
- **FR-005**: Sprinting MUST NOT increase drain rate relative to walking or standing still, for
  the same elapsed wall-clock time (GDD Ch. 4.1 — sprint-drains-battery is explicitly rejected).
- **FR-006**: Drain MUST continue at the same rate while the player is idle, moving, interacting,
  or hiding — no gameplay action pauses, accelerates, or decelerates it (GDD Ch. 5.1 "Drain saat
  idle: Tetap berjalan"; Ch. 4.3 "Battery tetap berkurang saat hiding").
- **FR-007**: Charge MUST clamp at exactly `0` and never go negative, even if a single drain step's
  elapsed time exceeds the remaining charge.
- **FR-008**: Draining MUST be a no-op (no exception, no charge change) when there is no installed
  battery to drain.
- **FR-009**: The `Battery` entity's charge MUST be stored and drained in seconds (`0` to
  `GameConfig.batteryDuration`), with the charge *fraction* consumed by
  `001-light-state-thresholds-and-radius` (and any other reader) derived on demand as
  `chargeSeconds / GameConfig.batteryDuration` — never stored as a separately-drained fraction
  that could drift out of sync with the seconds value.
- **FR-010**: The pure drain calculation MUST live in a plain C# class under
  `Assets/Scripts/Systems/` with no `UnityEngine.MonoBehaviour`, `Component`, or scene dependency
  (constitution Principle III), callable from an EditMode test with an arbitrary sequence of
  elapsed-time values and no running scene. The `MonoBehaviour` adapter that reads `Time.deltaTime`
  each frame and calls into it is a separate, thin class under `Assets/Scripts/MonoBehaviours/`.

### Key Entities

- **Battery** *(fixed name — first full definition; extended by 007/008)*: Owns `ChargeSeconds`
  (float, `0`..`GameConfig.batteryDuration`) and exposes a derived `ChargeFraction` property
  (`ChargeSeconds / GameConfig.batteryDuration`, consumed by `001`'s `LightStateSystem`). This
  feature defines the entity and its drain behavior only; the world-battery/spare-slot/installed
  distinction is added by `007-battery-pickup-and-spare-slot` and
  `008-battery-install-and-refill`.
- **GameConfig** *(extended)*: Gains `batteryDuration`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A battery drained by a sequence of elapsed-time steps summing to exactly 180 seconds
  reaches exactly `0` charge, verified across at least two different step-count decompositions of
  the same 180-second total (e.g. one large step vs. thousands of small steps), with identical
  final results.
- **SC-002**: Draining the same battery by the same total elapsed time produces the identical
  resulting charge value regardless of what movement-state or interaction label is associated
  with that elapsed time in the test — because the drain function has no such parameter to vary.
- **SC-003**: No sequence of elapsed-time inputs, however large a single step is, ever produces a
  negative charge value.
- **SC-004**: In a hands-on playtest, the team lets a battery drain fully from 100% without
  touching a pickup, and records whether 180 seconds felt "pressing but not too loose" (GDD Ch.
  20.2) — a feel judgment, not a pass/fail number, tracked as a decision log entry rather than a
  numeric criterion.
- **SC-005**: `batteryDuration` can be changed by editing only `GameConfig`, with every dependent
  drain/fraction calculation picking up the new value with no other file requiring a change.

## Assumptions

- The real-time clock source (`Time.deltaTime` vs. `Time.unscaledDeltaTime`) is an implementation
  choice made when this feature is planned, not a rule this spec locks — what this spec locks is
  that the source MUST represent actual elapsed wall-clock time, not a frame count, and MUST NOT
  itself be affected by gameplay `Time.timeScale` changes intended for other purposes (e.g. a
  hitstop effect) unless a future spec explicitly says the battery should also freeze then.
  Pause-menu behavior (whether the clock freezes while paused) belongs to `game-shell-ui`, per
  User Story 1 Scenario 3.
- This feature does not decide how often the `MonoBehaviour` adapter runs (every `Update()` frame
  is the obvious choice and is expected, but not itself a testable rule of this spec beyond "the
  math is correct for whatever elapsed time it's given").
- No UI, HUD, or visual representation of charge is in this feature's scope — see
  `009-battery-hud-indicator`.

## Related

- [[ROADMAP]] — flashlight-and-battery row 2; depends on `001-light-state-thresholds-and-radius`
- [[constitution]] — Principle III (plain C# systems), Principle IV (EditMode tests)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 4.1 (no sprint drain), Ch. 4.3 (hiding drain continues),
  Ch. 5.1 (180s real-time duration), Ch. 17.2 (GameConfig keys)
- `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius` — consumes this
  feature's charge fraction; not re-derived here
- `specs/systems/shared-config-and-state/002-shared-game-state-and-manager` — origin of
  `GameState`/`GameManager`, not redefined here
