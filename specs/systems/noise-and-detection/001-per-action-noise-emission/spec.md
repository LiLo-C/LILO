# Feature Specification: Per-Action Noise Emission

**Feature Branch**: `001-per-action-noise-emission`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Every player action emits a noise radius that is a multiplier of a shared base radius, per GDD 7.1's table: idle/hiding = 0 (undetectable), walk = 1.0× (base), sprint = 3.0×, interact = 2.0×, battery swap = 1.5×. This is a pure emission model — given the player's current action, what is the current noise radius — with no detection logic of its own."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Baseline Movement Noise (Priority: P1)

As Eddie moves through the building, the noise he makes scales with how he is moving: standing still stays silent, walking makes the base amount of noise, and sprinting makes three times as much — so every movement choice carries an audible, calculable risk before any other system reacts to it.

**Why this priority**: This is the floor the entire noise-and-detection system stands on. Every other multiplier in GDD 7.1's table (hiding, interact, battery swap) is defined as a fraction or multiple of the same base radius established here; without a correct baseline, nothing downstream can be tuned or tested meaningfully.

**Independent Test**: Construct a `NoiseEmitter` directly with a fixed `noiseBaseRadius`, feed it idle, walking, and sprinting movement states in turn, and read back `CurrentNoiseRadius` — no scene, no monster, no hiding or interaction system needs to exist for this test to pass.

**Acceptance Scenarios**:

1. **Given** `noiseBaseRadius` is configured to some value, **When** the player's movement state is idle (no directional input, not sprinting), **Then** the current noise radius is 0.
2. **Given** the same base radius, **When** the movement state is walking, **Then** the current noise radius equals `noiseBaseRadius × noiseWalk` (1.0×).
3. **Given** the same base radius, **When** the movement state is sprinting, **Then** the current noise radius equals `noiseBaseRadius × noiseSprint` (3.0×).
4. **Given** the player is walking, **When** they stop moving, **Then** the current radius reflects idle (0) on the very next evaluation — there is no gradual fade or momentum carried over from the movement value.
5. **Given** the player goes straight from idle to sprinting (per movement-and-camera/001's sprint trigger), **When** evaluated, **Then** the radius is the sprint value on the next evaluation; no intermediate walking value is ever emitted.

---

### User Story 2 - Hiding Is Always Silent (Priority: P1)

While Eddie is hidden under a desk, he makes no noise at all, no matter what else is happening — this is an unconditional guarantee, not just "quieter than walking."

**Why this priority**: GDD 4.3 and 7.1 both state hiding is "sama sekali tidak terdeteksi" (not detected at all). That is an absolute override, not a small number — it has to beat every other multiplier this feature defines, so it is proven independently rather than assumed to fall out of the movement math.

**Independent Test**: Drive the emitter with the hiding flag set true while simultaneously feeding it a sprinting movement state and an active pulse (from User Story 3/4); confirm the output radius is exactly 0 regardless of what else is active.

**Acceptance Scenarios**:

1. **Given** the player's hiding flag is true and the movement state is idle, **When** evaluated, **Then** the current radius is 0.
2. **Given** hiding is true and an interaction or battery-swap pulse is simultaneously active, **When** evaluated, **Then** the current radius is still 0 — hiding overrides every pulse.
3. **Given** hiding is true and the movement state reports sprinting (defensive case; hiding/001 disables movement input in practice), **When** evaluated, **Then** the current radius is still 0.
4. **Given** the player exits hiding, **When** evaluated on the next tick, **Then** the current radius returns to whatever the non-hiding movement/pulse state dictates, with no residual suppression.

---

### User Story 3 - Interacting Makes Noise (Priority: P2)

Opening a door or picking something up makes a brief, noticeably louder sound than just walking — enough that doing it near something listening is a real decision, not a free action.

**Why this priority**: Completes GDD 7.1's "Interact objek" row (2.0×) and introduces the one-shot "pulse" concept and the highest-multiplier-wins combination rule that User Story 4 reuses. It is lower priority than US1/US2 because it only matters once a baseline and an override already exist to combine against.

**Independent Test**: Trigger a single interaction pulse on an otherwise-idle emitter; confirm the radius reads `noiseBaseRadius × noiseInteract` for the configured pulse duration, then decays back to whatever the underlying movement state dictates.

**Acceptance Scenarios**:

1. **Given** the player is idle, **When** an interaction pulse is triggered, **Then** the current radius immediately becomes `noiseBaseRadius × noiseInteract` (2.0×).
2. **Given** an interaction pulse is active, **When** `noisePulseDuration` seconds elapse with no retrigger, **Then** the current radius returns to whatever the underlying movement state currently dictates.
3. **Given** the player is walking (1.0×) when an interaction pulse triggers, **When** evaluated during the pulse, **Then** the current radius is base × 2.0 (the higher of the two) — not walking's 1.0× and not an additive sum of both.
4. **Given** the player is sprinting (3.0×) when an interaction pulse triggers, **When** evaluated during the pulse, **Then** the current radius remains base × 3.0 — sprint's multiplier is higher and wins.
5. **Given** a second interaction pulse triggers while the first is still active, **When** evaluated, **Then** the pulse's remaining duration restarts from the new trigger rather than stacking two separate expiries.

---

### User Story 4 - Installing a Battery Makes Noise (Priority: P3)

Swapping in a fresh battery makes a small-to-medium burst of noise — enough to make recharging near danger feel risky, per GDD's own framing of this multiplier.

**Why this priority**: Narrowest trigger surface of the four (only fires from one specific upstream action) and reuses the exact pulse mechanism proven in US3 with a different multiplier — the lowest-risk, most mechanical addition once US3 exists.

**Independent Test**: Trigger a battery-swap pulse from an idle state and confirm the radius reads `noiseBaseRadius × noiseBatterySwap` for the pulse duration, using the same harness as US3.

**Acceptance Scenarios**:

1. **Given** the player is idle, **When** a battery-swap pulse is triggered by flashlight-and-battery/008's battery-install action, **Then** the current radius becomes `noiseBaseRadius × noiseBatterySwap` (1.5×).
2. **Given** a battery-swap pulse is active and an interaction pulse triggers at the same instant, **When** evaluated, **Then** the higher multiplier (interact, 2.0×) wins.
3. **Given** a battery-swap pulse is active while sprinting, **When** evaluated, **Then** the current radius remains sprint's 3.0× — the pulse never lowers an already-higher radius.
4. **Given** the pulse duration elapses, **When** evaluated, **Then** the current radius returns to the underlying movement value.

---

### Edge Cases

- Simultaneous elevated sources (any combination of walking/sprinting/an interact pulse/a battery-swap pulse) always resolve to the single highest multiplier among them — never summed, never averaged.
- Hiding entered mid-pulse cancels the active pulse outright; the pulse does not resume or continue counting down once hiding ends (see FR-010).
- Rapidly repeated triggers of the *same* pulse kind refresh (restart) its remaining duration rather than stacking multiple expiries.
- `noiseBaseRadius` is 0 or an unset placeholder (GDD Ch. 21, still pending on-device lock): every multiplier still resolves correctly — the emitted radius is simply 0 until the value is tuned. The system must never divide by it or fail because of it.
- A negative or otherwise invalid multiplier or base-radius value read from a corrupted config must not propagate into a negative emitted radius — the output is clamped at 0 as a defensive floor.
- Movement state and the hiding flag disagreeing (e.g. "sprinting" reported while hiding is true) should never happen in practice because hiding/001 disables movement input — but User Story 2's unconditional override makes any such inconsistency harmless regardless of how it happened.

## Requirements *(mandatory)*

### Functional Requirements

**Baseline movement noise**

- **FR-001**: The system MUST expose a single current noise radius value at any evaluation instant, computed as `noiseBaseRadius × activeMultiplier`, where `noiseBaseRadius` is one field read from `GameConfig` (GDD 17.3) shared by every action below.
- **FR-002**: When the player is not moving, not sprinting, not hiding, and no pulse is active, `activeMultiplier` MUST equal `noiseHiding` (0) — a stationary player is exactly as undetectable as a hidden one (GDD 7.1, "Diam / hiding" is a single row).
- **FR-003**: While the player's movement state (from movement-and-camera/001-joystick-movement-and-sprint, `specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md`) reports walking, and no higher multiplier is active, `activeMultiplier` MUST equal `noiseWalk` (1.0×).
- **FR-004**: While that same movement state reports sprinting, and no higher multiplier is active, `activeMultiplier` MUST equal `noiseSprint` (3.0×).

**Hiding override**

- **FR-005**: Whenever the player's hiding flag (from hiding/001-enter-and-exit-hiding, `specs/systems/hiding/001-enter-and-exit-hiding/spec.md`) is true, `activeMultiplier` MUST equal `noiseHiding` (0), overriding every other movement or pulse multiplier unconditionally, for as long as hiding remains true.

**One-shot pulses**

- **FR-006**: An interaction pulse — triggered generically by any caller that performs a player interaction (e.g. opening a door, picking up an item) — MUST set `activeMultiplier` to `noiseInteract` (2.0×) for `noisePulseDuration` seconds from the moment it is triggered, unless hiding is true (FR-005 wins). This feature does not require or depend on any specific upstream interaction spec to exist; it only exposes the trigger.
- **FR-007**: A battery-swap pulse — triggered specifically by flashlight-and-battery/008-battery-install-and-refill's battery-install action (`specs/systems/flashlight-and-battery/008-battery-install-and-refill/spec.md`) — MUST set `activeMultiplier` to `noiseBatterySwap` (1.5×) for `noisePulseDuration` seconds from the moment it is triggered, unless hiding is true (FR-005 wins).
- **FR-008**: Re-triggering a pulse of the same kind while it is already active MUST restart that pulse's remaining duration from `noisePulseDuration`, rather than stacking or extending it indefinitely.
- **FR-009**: When more than one of {walking, sprinting, an active interaction pulse, an active battery-swap pulse} would independently apply, `activeMultiplier` MUST equal the single highest of the multipliers that would independently apply. Multipliers MUST NOT be summed, averaged, or otherwise combined.
- **FR-010**: Entering hiding MUST cancel any in-progress pulse outright; a cancelled pulse MUST NOT resume or continue expiring once hiding ends.

**Configuration & architecture**

- **FR-011**: All five GDD 7.1/17.3 multipliers (`noiseHiding`, `noiseWalk`, `noiseSprint`, `noiseInteract`, `noiseBatterySwap`) and the shared `noiseBaseRadius` MUST be read from `GameConfig`. None of these six numbers may be a hardcoded literal anywhere in the emission logic.
- **FR-012**: `noisePulseDuration` MUST be added to `GameConfig` as a new tunable field. It does not appear in GDD 17.3's original table — it exists to make "a one-shot pulse at the moment of the action" (GDD 7.1's own framing) a well-defined, testable window instead of an undefined single-frame instant. Like `sprintJoystickThreshold` and `interactionRadius` in GDD 17.1, it is a feel-based value with no locked number yet; the *rule* (a fixed short window, restarted on retrigger, cancelled by hiding) is normative even before its exact seconds value is tuned on device.
- **FR-013**: The emission logic MUST be implemented as a plain C# class with no `UnityEngine.MonoBehaviour`, `Component`, or scene-graph dependency (constitution Principle III), so it is fully unit-testable in EditMode without a running scene.
- **FR-014**: The emitted noise radius MUST change instantaneously between values — no easing, interpolation, or smoothing. Unlike the flashlight radius (GDD 12.1), noise radius is never rendered on screen (GDD 7.2: "Noise radius tidak pernah digambar di layar") and exists purely as a numeric input to noise-and-detection/002-distance-based-detection-check, so there is nothing to visually smooth.
- **FR-015**: `noiseBaseRadius` MUST remain a `GameConfig` field pending its on-device playtesting lock (GDD Ch. 21: "Nilai noiseBaseRadius … Harus selesai sebelum Fase 2"). This spec, its tasks, and its tests MUST NOT hardcode or fabricate a final number for it anywhere beyond a clearly-labeled placeholder — see Definition of Done below.

### Key Entities

- **Noise Emission Profile**: the subset of `GameConfig` fields this feature reads or adds — `noiseBaseRadius`, `noiseWalk`, `noiseSprint`, `noiseInteract`, `noiseBatterySwap`, `noiseHiding` (GDD 17.3, existing keys) plus `noisePulseDuration` (new key introduced by this spec).
- **Player Noise State**: the plain C# object holding the current inputs (movement state, hiding flag, at most one active pulse) and exposing the single computed current noise radius. It reads movement state from movement-and-camera/001 and the hiding flag from hiding/001 but owns neither.
- **Noise Pulse**: a timed, one-shot elevation of the multiplier (kind: Interact or BatterySwap; remaining duration), started by a trigger call and expiring on its own after `noisePulseDuration` seconds, or cancelled outright by hiding.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Given a fixed `noiseBaseRadius`, 100 idle evaluations and 100 walking evaluations produce exactly two distinct radii — 0 and 1.0× base — with zero deviation.
- **SC-002**: 100 sprinting evaluations produce exactly 3.0× base with zero deviation.
- **SC-003**: In 100/100 trials where hiding is true, the emitted radius is 0 regardless of any simultaneously-fed movement state or active pulse.
- **SC-004**: In 100/100 trials, triggering an interaction pulse from idle produces exactly 2.0× base for the full configured pulse duration and returns to the pre-pulse value within one evaluation of expiry — no overlap frame, no gap frame.
- **SC-005**: In 100/100 trials, triggering a battery-swap pulse from idle produces exactly 1.5× base for the full pulse duration and returns cleanly.
- **SC-006**: In every trial where two elevated sources overlap (any pairing of walk/sprint/interact/battery-swap), the emitted radius equals base × the higher of the two multipliers in 100% of cases — never a sum or an average.
- **SC-007**: Each of the six config-driven numbers (`noiseBaseRadius`, `noiseWalk`, `noiseSprint`, `noiseInteract`, `noiseBatterySwap`, `noiseHiding`) measurably changes the emitted radius when changed in a test `GameConfig` instance — proving none is hardcoded.
- **SC-008**: 100% of the functional requirements above are covered by green EditMode tests before this feature is marked done.

## Definition of Done — Blocking Open Item

`noiseBaseRadius` (GDD Ch. 17.3, Ch. 21) is explicitly listed in the GDD as unresolved: *"Nilai noiseBaseRadius — Bergantung pada skala map yang belum dibuat — Harus selesai sebelum Fase 2 (Monster)."* This feature MUST ship with `noiseBaseRadius` as a `GameConfig` field with a clearly-labeled placeholder value (not a fabricated final number), and every FR/SC above is written to hold for *any* value of that field. This feature is not considered fully done — even once all tests are green — until `noiseBaseRadius` has been locked via on-device playtesting, because until then the multipliers this spec locks in (1.0×/3.0×/2.0×/1.5×/0) have no real-world meaning in meters. Tests and code review MUST verify no test or default asset silently treats a placeholder value as final.

## Assumptions

- "Diam" (standing idle: no directional input, not sprinting) and hiding share the same zero multiplier (`noiseHiding`) per GDD 7.1's single combined row. They are modeled as two independent conditions that happen to produce the same result, not as the same state — idle is a movement-and-camera/001 concept, hiding is a hiding/001 concept.
- "Interact objek" in GDD 7.1 is modeled as a generic pulse trigger with no dependency on any specific upstream interaction spec — any future caller (interaction-and-highlight, keys-and-doors, etc.) invokes it directly. This keeps the emission model from needing to know *which* system caused the interaction, only that one happened, and matches this feature's absence from the ROADMAP's dependency column for anything beyond movement, battery-install, and hiding.
- `noisePulseDuration` is this spec's own addition, needed to turn "a one-shot pulse at the moment of the action" (GDD 7.1) into a well-defined, testable window. No GDD chapter specifies this number, so — like `noiseBaseRadius` — it is left as an unlocked `GameConfig` field per ROADMAP §0's rule for feel-based values, not a fabricated constant.
- The highest-multiplier-wins combination rule for overlapping sources is this spec's own resolution of a case the GDD's per-action table does not address directly (the table lists each action individually, not in combination). It is chosen because it is the simplest rule consistent with every documented single-action value and introduces no new tunable number.
- Movement state and hiding are read, not owned: this spec assumes movement-and-camera/001 and hiding/001 each expose a single, unambiguous instantaneous value (no queued or buffered states) for this feature to read once per evaluation.

## Dependencies

- **Requires** movement-and-camera/001-joystick-movement-and-sprint (`specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md`) for the walking/sprinting movement-state input. Not redefined here.
- **Requires** flashlight-and-battery/008-battery-install-and-refill (`specs/systems/flashlight-and-battery/008-battery-install-and-refill/spec.md`) for the battery-install action that triggers the battery-swap pulse. Not redefined here.
- **Requires** hiding/001-enter-and-exit-hiding (`specs/systems/hiding/001-enter-and-exit-hiding/spec.md`) for the hiding-flag input. Not redefined here.
- **Consumed by** noise-and-detection/002-distance-based-detection-check (`specs/systems/noise-and-detection/002-distance-based-detection-check/spec.md`), which checks a monster's distance against this feature's current noise radius output, and, through it, by monster-ai/001-state-machine-core-transitions (`specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`).

## Related

- GDD Ch. 7.1 (noise radius per action) and Ch. 17.3 (Noise config keys) — `specs/_reference/LILO-GDD-v2-Production-Lock.md`
- `specs/ROADMAP.md` — §0 (shared naming/feel-based-value convention), §2 noise-and-detection row, §5 dependency-ordered build sequence
- `.specify/memory/constitution.md` — Principle III (Unity Architecture Consistency), Principle IV (Test-Before-Done)
