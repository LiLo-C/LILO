# Feature Specification: Light State Thresholds & Radius

**Feature Branch**: `001-light-state-thresholds-and-radius`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Pure derivation of one of four LightState values (Normal/Flickering/Critical/CompactDarkness) from remaining battery charge fraction, using the GDD's exact boundaries (30%, 10%, 0%), plus the target lit radius per state. No easing, no flicker events, no drain timing — those are later features in this category."

## Why This Spec Exists

LILO's core tension is expressed entirely through how much of the world the player can see (GDD
Ch. 1.3 "Compact Pressure", Ch. 5.2, Ch. 12). Every later feature in this category — the real-time
drain timer (002), the eased radius that chases this feature's target (003), the flicker event
system (004), shadow casting (005), the readability fill (006), and the HUD (009) — needs one
single, unambiguous source of truth for "given how much charge is left, which of the four states
is the flashlight in, and how large should its light be." This feature is that source of truth,
and nothing else. It does not animate anything, does not run every frame on its own, and does not
know about wall-clock time — it is a pure derivation from a charge fraction to a `LightState` and
a target radius, callable from an EditMode test with no scene running.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Identify the Current Light State from Charge (Priority: P1)

Given how much charge the installed battery has left, the game can always say — unambiguously and
without exception — which one of exactly four light states currently applies, so every other
system (radius easing, flicker, HUD, install gating) can trust a single answer instead of each
re-deriving its own threshold logic.

**Why this priority**: This is the single fact every other flashlight-and-battery feature reads.
Get the boundaries wrong here and every dependent feature (002–009) inherits the bug.

**Independent Test**: Call the state derivation with a range of charge fraction values (0.0
through 1.0) with no scene, no `MonoBehaviour`, and no battery object running, and confirm each
value maps to exactly one of the four documented states — fully testable in EditMode alone.

**Acceptance Scenarios**:

1. **Given** charge fraction is anywhere in `(0.30, 1.0]`, **When** the state is derived, **Then**
   the result is `Normal`.
2. **Given** charge fraction is anywhere in `(0.10, 0.30]`, **When** the state is derived, **Then**
   the result is `Flickering`.
3. **Given** charge fraction is anywhere in `(0.0, 0.10]`, **When** the state is derived, **Then**
   the result is `Critical`.
4. **Given** charge fraction is exactly `0.0`, **When** the state is derived, **Then** the result
   is `CompactDarkness`.
5. **Given** charge fraction is exactly `0.30`, **When** the state is derived, **Then** the result
   is `Flickering`, not `Normal` — the upper bound of a band belongs to the lower state.
6. **Given** charge fraction is exactly `0.10`, **When** the state is derived, **Then** the result
   is `Critical`, not `Flickering` — same boundary rule.

---

### User Story 2 - Derive the Target Lit Radius for the Current State (Priority: P1)

Given the same charge fraction, the game can derive the exact target lit radius the flashlight
should be showing right now — the number every other feature eases toward (003), dips from (004),
and never itself animates.

**Why this priority**: Without a target, 003 has nothing to ease toward and 004 has nothing to dip
from. This is exactly as foundational as User Story 1 and ships in the same feature because both
are the same pure function of the same input.

**Independent Test**: Call the target-radius derivation across the same charge fraction sweep used
in User Story 1's test, with no scene running, and confirm the returned radius matches the
expected value at each boundary and within each band, including the continuous narrowing inside
Critical.

**Acceptance Scenarios**:

1. **Given** the state is `Normal` or `Flickering`, **When** the target radius is derived, **Then**
   it equals `GameConfig.flashlightNormalRadius` in both states — Flickering's dips are a
   transient effect layered on top by 004, not a change to this feature's target.
2. **Given** the state is `Critical`, **When** the target radius is derived at charge fraction
   equal to `GameConfig.lightStateCriticalStart` (0.10), **Then** it equals
   `GameConfig.flashlightNormalRadius`.
3. **Given** the state is `Critical`, **When** the target radius is derived at charge fraction
   equal to `0.0`'s boundary approached from above, **Then** it approaches
   `GameConfig.flashlightNormalRadius * GameConfig.compactDarknessRadiusFraction`.
4. **Given** the state is `Critical`, **When** the target radius is derived at charge fraction
   exactly halfway between `0.0` and `GameConfig.lightStateCriticalStart`, **Then** it returns a
   value partway between the two Critical-band endpoints — the narrowing inside Critical MUST be
   continuous with charge, not a second stepped tier (GDD Ch. 5.2 "Radius menyempit bertahap").
5. **Given** the state is `CompactDarkness` (charge fraction `0.0`), **When** the target radius is
   derived, **Then** it equals exactly
   `GameConfig.flashlightNormalRadius * GameConfig.compactDarknessRadiusFraction` (GDD Ch. 12.1:
   "menyusut ke sekitar 10% ukuran normal").

---

### Edge Cases

- **Charge fraction outside `[0.0, 1.0]`** (e.g. a caller passes a raw seconds value instead of a
  fraction, or a floating-point overshoot slightly above `1.0` or below `0.0`): the derivation
  MUST clamp the input to `[0.0, 1.0]` before evaluating bands, rather than throwing or returning
  an undefined state — callers upstream (002) own not feeding it garbage, but this feature must
  not crash a frame if they do.
- **`lightStateCriticalStart` configured at or above `lightStateFlickerStart`** in `GameConfig`
  (a misconfiguration, not a runtime charge value): this feature's derivation still executes
  band comparisons in a fixed order (Normal → Flickering → Critical → CompactDarkness) and
  produces *some* answer for every input; validating that the two config values are sane relative
  to each other is a `GameConfig` validation concern, not this feature's runtime job.
- **Floating point charge fraction extremely close to a boundary** (e.g. `0.30000001` due to
  accumulated float error from the real-time drain in 002): boundary comparisons MUST use the
  documented inequalities exactly (`>` vs `>=`) with no added epsilon/tolerance band — 002 owns
  keeping its charge value numerically clean; this feature is not responsible for absorbing float
  drift with a fudge factor.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST derive exactly one `LightState` value — `Normal`, `Flickering`,
  `Critical`, or `CompactDarkness` — from a single charge fraction input in `[0.0, 1.0]`, with no
  other input (no wall-clock time, no frame count, no player action).
- **FR-002**: The four state bands MUST be non-overlapping and MUST use these exact boundaries,
  read from `GameConfig` (not hardcoded): `Normal` is `lightStateFlickerStart < charge ≤ 1.0`;
  `Flickering` is `lightStateCriticalStart < charge ≤ lightStateFlickerStart`; `Critical` is
  `0.0 < charge ≤ lightStateCriticalStart`; `CompactDarkness` is `charge = 0.0` exactly. A charge
  value exactly on a band's upper boundary belongs to that upper band, never the band above it
  (GDD Ch. 5.2).
- **FR-003**: `GameConfig` MUST expose `lightStateFlickerStart` (fraction, locked default `0.30`
  per GDD Ch. 17.2) and `lightStateCriticalStart` (fraction, locked default `0.10` per GDD Ch.
  17.2) as the only two threshold values this derivation reads. These are locked GDD numbers, not
  feel-tuned placeholders.
- **FR-004**: The system MUST derive a target lit radius (world units) from the same charge
  fraction input, as a pure function with no animation or memory of the previous frame's radius —
  frame-to-frame smoothing toward this value is explicitly out of scope for this feature (see
  `003-eased-radius-transitions`).
- **FR-005**: In `Normal` and `Flickering`, the target radius MUST equal
  `GameConfig.flashlightNormalRadius` unmodified. Flickering's visible dips are a transient effect
  applied on top of this target by a later feature (`004-flicker-event-system`), never a change to
  the target itself.
- **FR-006**: In `Critical`, the target radius MUST narrow continuously as charge falls from
  `lightStateCriticalStart` toward `0.0`, linearly interpolating between
  `GameConfig.flashlightNormalRadius` (at charge `= lightStateCriticalStart`) and
  `GameConfig.flashlightNormalRadius * GameConfig.compactDarknessRadiusFraction` (as charge
  approaches `0.0`) — GDD Ch. 5.2's "Radius menyempit bertahap" describes a gradual narrowing tied
  to remaining charge, not a second fixed tier.
- **FR-007**: In `CompactDarkness`, the target radius MUST equal exactly
  `GameConfig.flashlightNormalRadius * GameConfig.compactDarknessRadiusFraction`. `GameConfig`
  MUST expose `compactDarknessRadiusFraction` (fraction, locked default `0.10` per GDD Ch. 17.2 —
  "±10% radius normal").
- **FR-008**: `GameConfig` MUST expose `flashlightNormalRadius` (world units) as the Normal-state
  lit radius. The GDD does not lock an exact number for this — it is feel content, validated by
  playtesting on target hardware, not derived from a formula. A starting default of `220` world
  units is carried over from this project's earlier native-engine on-device tuning pass (see
  ROADMAP §0) **to re-confirm on-device in Unity/URP** — it is not a settled final value.
- **FR-009**: The derivation MUST clamp any out-of-range input charge fraction to `[0.0, 1.0]`
  before evaluating bands (see Edge Cases), rather than throwing or returning an undefined state.
- **FR-010**: This feature's derivation logic MUST live in a plain C# class under
  `Assets/Scripts/Systems/` with no `UnityEngine.MonoBehaviour`, `Component`, or scene dependency,
  per constitution Principle III — it must be callable from an EditMode test with no running
  scene.

### Key Entities

- **LightState**: An enum with exactly four values — `Normal`, `Flickering`, `Critical`,
  `CompactDarkness`. Derived, never stored independently of charge — nothing persists a
  `LightState` across frames as its own source of truth.
- **Battery / charge fraction**: This feature consumes a charge fraction (`0.0`–`1.0`) as a plain
  `float` input. The `Battery` entity that owns and drains this value in real time is defined by
  `002-battery-real-time-drain-timer` — this feature does not define or store a `Battery`.
- **GameConfig** *(extended)*: Gains `lightStateFlickerStart`, `lightStateCriticalStart`,
  `compactDarknessRadiusFraction`, `flashlightNormalRadius`. See
  `specs/systems/shared-config-and-state/001-game-config-schema` for the config asset's own
  schema-management spec; this feature only adds fields to that single existing asset, per
  constitution Principle V.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every charge fraction value in a fine-grained sweep from `0.0` to `1.0` (including
  the exact boundary values `0.0`, `0.10`, `0.30`, and `1.0`) maps to exactly one of the four
  documented `LightState` values with zero ambiguous or undefined results, verified by an
  automated EditMode test with no scene running.
- **SC-002**: The target radius returned for `Normal` and `Flickering` is identical
  (`flashlightNormalRadius`) across 100% of sampled charge values within those two bands.
- **SC-003**: The target radius returned within `Critical` changes monotonically (never increases)
  as charge fraction decreases from `lightStateCriticalStart` to `0.0`, across a sweep of at least
  20 sample points, with the endpoints matching FR-006 exactly.
- **SC-004**: In an observation test with at least 2 people outside the development team looking
  at the four states rendered at their derived target radii (via the 003 easing feature once
  built), each person can correctly say the light is visibly smaller in `CompactDarkness` than in
  `Normal` without being told what to look for (validates FR-007/FR-008 produce a perceptible
  difference, not just a numeric one).
- **SC-005**: Every threshold and radius value this feature introduces can be changed by editing
  only `GameConfig`, with no other file requiring a change (constitution Principle III/V).

## Assumptions

- `flashlightNormalRadius`'s starting default (`220` world units) is carried over from this
  project's prior native-engine (SceneKit) on-device tuning pass, per ROADMAP §0's note that
  "gameplay math is engine-independent." It is provided so every dependent system has a concrete
  number to build and test against immediately — it is explicitly **not** a locked value and MUST
  be re-tuned on Unity/URP target hardware once a real scene and camera framing exist.
- This feature does not decide *when* or *how often* the state/radius is recomputed (every frame,
  on-demand, etc.) — that is an integration detail for the `MonoBehaviour` adapter that calls into
  it each frame (owned by 003's `FlashlightRadiusController`), not a rule this spec enforces.
- No monster, hiding, noise, or narrative systems are affected by or referenced from this feature.

## Related

- [[ROADMAP]] — flashlight-and-battery row 1; depends on `shared-config-and-state/002`
- [[constitution]] — Principle III (plain C# systems), Principle IV (EditMode tests)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 5.2 (Light State table), Ch. 12/12.1 (Lighting System),
  Ch. 17.2 (GameConfig keys)
- `specs/systems/shared-config-and-state/002-shared-game-state-and-manager` — origin of
  `GameState`/`GameManager`, not redefined here
