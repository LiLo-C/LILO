# Feature Specification: Distance-Based Detection Check

**Feature Branch**: `002-distance-based-detection-check`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The monster detects the player when the monster's straight-line
distance to the player is strictly less than the noise radius the player is currently emitting
(GDD 7.2). Detection is distance-only: no line-of-sight/occlusion check — walls do not block it,
a deliberate simplification. Flashlight/light state never triggers or affects detection in this
version (GDD 7.2 states this outright). Feeds monster-ai/001-state-machine-core-transitions's
detection-signal input; does not redefine that state machine."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Monster Detects Player Within Its Noise Radius (Priority: P1)

While patrolling or standing anywhere on the floor, the `Monster` detects the player the instant
their straight-line distance apart is smaller than whatever noise radius the player is currently
emitting — closing the loop that makes every action in noise-and-detection/001's table (walking,
sprinting, interacting, swapping a battery) actually matter to something that reacts.

**Why this priority**: This is the entire payoff of the noise-and-detection category and the only
input `monster-ai/001-state-machine-core-transitions` needs to come alive (ROADMAP §5: monster-ai
depends directly on this feature). Without a correct positive case, noise-and-detection/001's
emission model has no consumer and the monster state machine has no real signal to react to.

**Independent Test**: Call the detection check directly with a monster position, a player
position, and a fixed noise radius where the distance between the two positions is smaller than
that radius; assert the returned signal reports detected with the source position equal to the
player's position. No scene, state machine, or live `NoiseEmitter` needs to exist for this test.

**Acceptance Scenarios**:

1. **Given** a noise radius of `R` and a monster/player distance of `D` where `D < R`, **When**
   the check is evaluated, **Then** `IsDetected` is true and `SourcePosition` equals the player's
   current position.
2. **Given** the distance is just barely inside the radius (`R` minus a small epsilon), **When**
   evaluated, **Then** `IsDetected` is still true.
3. **Given** the player is walking (radius = `noiseBaseRadius × noiseWalk`, per
   noise-and-detection/001) and the monster is closer than that radius, **When** evaluated,
   **Then** `IsDetected` is true — proving this feature correctly consumes 001's output, not a
   fabricated radius.
4. **Given** the monster and player positions change between two consecutive evaluations,
   **When** each is evaluated, **Then** each produces its own correct, independent result with
   no residual state or lag carried from the previous evaluation.

---

### User Story 2 - No Detection At or Beyond the Radius, the Strict Boundary (Priority: P1)

When the monster is exactly at the edge of the player's noise radius, or farther away, it does
not detect the player — the comparison is strictly "less than," never "less than or equal to."

**Why this priority**: GDD 7.2 defines detection as distance "lebih kecil dari" (smaller than)
the radius — an exact numeric boundary that, implemented as `<=` instead of `<`, would make every
single action in the game perceptibly more dangerous than the design intends. This correctness is
exactly as load-bearing as User Story 1's positive case, so it shares P1.

**Independent Test**: Evaluate the check with distance set to exactly equal the radius, and again
with distance greater than the radius; assert `IsDetected` is false in both cases, using the same
harness as User Story 1.

**Acceptance Scenarios**:

1. **Given** distance exactly equals the radius, **When** evaluated, **Then** `IsDetected` is
   false — this is the single most safety-critical numeric case in this feature.
2. **Given** distance exceeds the radius, **When** evaluated, **Then** `IsDetected` is false.
3. **Given** the radius is 0 (the player is idle or hiding, per noise-and-detection/001 User
   Story 2) and the monster happens to occupy the exact same position as the player (distance 0),
   **When** evaluated, **Then** `IsDetected` is still false — `0 < 0` is false, so a hidden/idle
   player remains genuinely undetectable even at zero range, with no special-case branch needed.
4. **Given** a negative or otherwise corrupted radius value reaches this check (e.g. from a bad
   config read), **When** evaluated at any distance, **Then** `IsDetected` is false — invalid
   input degrades safely to "never detected," never to an accidental true.

---

### User Story 3 - Detection Ignores Walls and Line-of-Sight (Priority: P2)

A monster on the other side of a wall from the player, but within noise-radius distance, still
detects them — this feature performs no raycast, no NavMesh path-length query, and no occlusion
check of any kind. This is a deliberate simplification, not an oversight.

**Why this priority**: This is an explicit scope boundary (GDD 7.2 defines detection purely by
distance, with no line-of-sight clause) that protects the codebase from anyone later "fixing"
this into a raycast-based system. Ranked P2 because it guards a scope boundary rather than adding
new player-facing behavior — it doesn't gate the correctness of User Stories 1/2, but it must be
provable so the simplification stays deliberate rather than accidental.

**Independent Test**: Confirm the detection check's inputs are exactly `(monsterPosition,
playerPosition, noiseRadius)` with no geometry, `Collider`, or raycast-hit parameter anywhere in
its signature — there is structurally nothing for a wall to occlude. Then evaluate two positions
placed as if separated by a solid wall in level space and confirm the result matches an
unobstructed pair at the identical numeric distance.

**Acceptance Scenarios**:

1. **Given** the detection check's full input surface, **When** inspected, **Then** it contains
   no geometry, mesh, `Collider`, raycast-hit, or NavMesh path parameter of any kind.
2. **Given** two positions whose straight-line distance is smaller than the radius, chosen so
   that a wall would sit directly between them in a real level layout, **When** evaluated,
   **Then** `IsDetected` is true — identical to the result for an unobstructed pair at the same
   numeric distance.

---

### User Story 4 - Flashlight and Light State Never Affect Detection (Priority: P2)

Whether the player is fully lit by their own flashlight, standing in total Compact Darkness, or
the monster happens to be lit itself, none of it changes whether the monster detects the
player — detection is entirely sound-based, per GDD 7.2's explicit rule.

**Why this priority**: This is a negative-space guarantee the GDD calls out by name, and it is
also an explicit cut-list item (GDD 18.2: "Deteksi lewat cahaya senter" — light-based detection —
is rejected, not deferred). Ranked P2 alongside User Story 3 for the same reason: it protects a
scope boundary rather than adding new positive behavior, but in a game whose entire premise is
lights, it is exactly as important to lock down as the wall guarantee.

**Independent Test**: Evaluate the same fixed monster position, player position, and radius while
varying an unrelated light-state value (representing each of GDD 5.2's four Light States) and
confirm the returned `IsDetected` value never changes. Separately, confirm the detection check's
signature has no light, illumination, or visibility-typed parameter to vary in the first place.

**Acceptance Scenarios**:

1. **Given** fixed positions and radius while the player's light state is Normal, **When**
   evaluated, **Then** the result is some value `R`.
2. **Given** the identical fixed positions and radius while the player's light state is Compact
   Darkness (0% battery), **When** evaluated, **Then** the result is still exactly `R`.
3. **Given** the detection check's full input surface, **When** inspected, **Then** it contains
   no light-state, flashlight-on/off, or illumination/visibility parameter whatsoever — this is
   structural, not merely a passing test outcome.

---

### Edge Cases

- Distance exactly equal to the noise radius resolves to **not detected** — the strict-inequality
  boundary is closed on the "safe" side (see User Story 2).
- Monster and player at the exact same position while the player is idle/hiding (radius 0):
  distance is also 0, and `0 < 0` is false, so detection correctly never fires with no special
  case required.
- A corrupted or negative radius value reaching this feature from a bad config must never flip
  into a false-positive detection — the comparison degrades safely to "never detected."
- Two positions on literally opposite sides of a solid wall, closer together than the radius:
  still detected — not a bug to fix, the deliberate scope boundary from User Story 3.
- The player cycling through all four Light States (GDD 5.2) at a fixed, otherwise-detectable
  distance from the monster: the detection outcome never changes, per User Story 4.
- Rapid consecutive evaluations with positions or radius changing every tick (e.g. the player
  starts sprinting mid-chase): each evaluation is independent and stateless. Any resulting
  tick-to-tick flicker of `IsDetected` is monster-ai/001's concern to smooth or not — this feature
  adds no debouncing or minimum-duration filter of its own.

## Requirements *(mandatory)*

### Functional Requirements

**Core distance comparison**

- **FR-001**: The system MUST compute, once per evaluation, the straight-line (Euclidean)
  distance in world space between the `Monster`'s current position and the `PlayerCharacter`'s
  current position, both supplied as `Vector3` inputs by the calling adapter layer — this
  feature does not itself track, cache, or own either `Transform`.
- **FR-002**: The system MUST read the noise radius to compare against directly from
  noise-and-detection/001-per-action-noise-emission's `NoiseEmitter.CurrentNoiseRadius` output
  (`specs/systems/noise-and-detection/001-per-action-noise-emission/spec.md`) as a single float
  per evaluation. This feature MUST NOT recompute, duplicate, or approximate that emission math.
- **FR-003**: Detection (`IsDetected`) MUST be true if and only if the distance computed per
  FR-001 is strictly less than the radius read per FR-002. An exactly equal distance MUST NOT be
  treated as detected (GDD 7.2: "jarak monster ke player lebih kecil dari noise radius aksi yang
  sedang dilakukan player").

**Boundary and defensive behavior**

- **FR-004**: When distance equals the noise radius exactly, or exceeds it, `IsDetected` MUST be
  false — the boundary is closed on the "not detected" side, never the "detected" side.
- **FR-005**: Any noise radius value of zero or less (including a defensively-clamped or
  corrupted config value) MUST always result in `IsDetected = false`, regardless of distance
  (including a distance of zero) — this holds even for a monster occupying the exact same
  position as a hidden/idle player.
- **FR-006**: This feature MUST NOT hardcode or assume a final numeric value for the noise radius
  anywhere in its logic or tests; every requirement above MUST hold for any non-negative float
  value, since the upstream `noiseBaseRadius` (GDD Ch. 21) remains an unlocked `GameConfig`
  placeholder pending on-device tuning, per noise-and-detection/001's own Definition of Done.

**Occlusion-free scope boundary**

- **FR-007**: The detection check MUST accept no geometry, `Collider`, raycast-hit, or NavMesh
  path-length input of any kind — its only inputs are the monster position, the player position,
  and the current noise radius. Walls, furniture, and floor layout MUST have zero effect on the
  result (deliberate simplification; GDD 7.2 defines detection purely by distance, with no
  line-of-sight clause).
- **FR-008**: This feature MUST NOT perform, trigger, or depend on any `Physics.Raycast`,
  `Physics.Linecast`, or NavMesh path query — occlusion/line-of-sight detection is explicitly out
  of scope for this version and MUST NOT be added speculatively (constitution Principle II,
  Simplicity/YAGNI).

**Light-independence scope boundary**

- **FR-009**: The detection check's inputs MUST NOT include the player's Light State
  (Normal/Flickering/Critical/Compact Darkness, GDD 5.2), flashlight on/off state, or any
  illumination/visibility value. The result of FR-003 MUST be identical for a fixed monster
  position, player position, and noise radius regardless of what the player's light state
  currently is (GDD 7.2: "Cahaya senter TIDAK memicu deteksi di versi ini"; GDD 18.2 confirms
  light-based detection is an explicit cut-list item, not an oversight).
- **FR-010**: Being fully lit by the monster's own light, standing in a bright area, or the
  flashlight pointing directly at the player MUST NOT independently trigger detection outside of
  the distance rule in FR-003 — there is no secondary "seen" detection path in this version.

**Output shape and integration**

- **FR-011**: The detection check's output MUST be shaped as `MonsterDetectionSignal { bool
  IsDetected, Vector3 SourcePosition }` — the exact type already defined by
  monster-ai/001-state-machine-core-transitions
  (`specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` FR-003, implemented at
  `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs`). This feature MUST reuse that
  existing type and MUST NOT declare a second, duplicate struct with the same or a different
  shape.
- **FR-012**: `SourcePosition` MUST be set to the player's current position at the moment of
  evaluation on every call, regardless of whether `IsDetected` is true or false. The value is
  simply unused downstream when `IsDetected` is false (monster-ai/001's FR-004/FR-005 only read
  `SourcePosition` on a detecting tick), so this feature does not need a separate "undefined"
  case to reason about.
- **FR-013**: This feature MUST place no constraint on, and have no awareness of, the monster's
  current AI state (`Patrol`/`Investigate`/`Chase`/`Search`) — the detection check runs
  identically regardless of state. State-dependent reactions to the signal belong exclusively to
  monster-ai/001, not here.

**Architecture and testability**

- **FR-014**: The detection check MUST be implemented as a plain C# static method or class with
  no `UnityEngine.MonoBehaviour`, `Component`, or scene-graph dependency (constitution
  Principle III), taking only primitive/`Vector3`/`float` inputs, so it is fully unit-testable in
  EditMode without a running scene, a NavMesh bake, or any live `GameObject`.
- **FR-015**: Given identical `(monsterPosition, playerPosition, noiseRadius)` inputs, the
  detection check MUST produce identical output on every call — no internal state, caching, or
  per-instance memory carried between evaluations.

### Key Entities

- **Detection Check**: a plain C# static method/class (e.g. `Assets/Scripts/Systems/Detection/`)
  implementing `(Vector3 monsterPosition, Vector3 playerPosition, float noiseRadius) →
  MonsterDetectionSignal`. Owns no state; a pure function of its three inputs.
- **`MonsterDetectionSignal`** (existing type, not redefined here — see
  `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` and
  `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs`): `IsDetected` (bool),
  `SourcePosition` (`Vector3`). This spec documents which fields it populates and how; the type
  itself is authored and owned by monster-ai/001.
- **Noise Radius Input**: the single float this feature reads once per evaluation from
  noise-and-detection/001's `NoiseEmitter.CurrentNoiseRadius`. Not owned or recomputed here.
- **Monster / Player Position Inputs**: `Vector3` world positions of the `Monster` and
  `PlayerCharacter` (fixed naming per ROADMAP §0), supplied externally each evaluation by the
  calling `MonoBehaviour` adapter (e.g. monster-ai/001's `MonsterController`). This feature does
  not track, cache, or own either `Transform`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Across 100 trials spanning varied positive radii and distances strictly less than
  the radius, `IsDetected` is true in 100/100 cases.
- **SC-002**: Across 100 trials where distance exactly equals the radius, plus 100 more trials
  where distance exceeds the radius, `IsDetected` is false in all 200/200 cases — proving the
  strict "less than" boundary is closed correctly on both sides.
- **SC-003**: Across 100 trials with a zero or negative radius at varied distances (including
  distance zero), `IsDetected` is false in 100/100 cases.
- **SC-004**: For every trial where `IsDetected` is true, `SourcePosition` exactly equals the
  player's position at evaluation time, with zero measurable error, in 100/100 cases.
- **SC-005**: For a fixed monster position, player position, and radius, varying an unrelated
  light-state input across all four GDD 5.2 states produces the identical `IsDetected` result in
  100% of combinations tested — proving zero coupling to light/flashlight state.
- **SC-006**: The detection check requires no rendering, physics callback, input device, live
  scene, or NavMesh bake to validate — 100% of this feature's functional requirements are
  covered by green EditMode tests before it is marked done.
- **SC-007**: Changing the upstream noise radius value (via a test `GameConfig`/`NoiseEmitter`
  combination) changes the detection outcome for a fixed monster/player distance — proving no
  hardcoded radius literal exists inside the detection check itself.

## Assumptions

- Distance is full 3D Euclidean distance (`Vector3.Distance`) between the two supplied world
  positions, not a 2D/horizontal-only projection. LILO's floors are flat, single-level scenes with
  no meaningful vertical separation between `Monster` and `PlayerCharacter`, so this is
  equivalent in practice to horizontal distance — but the math does not special-case out the Y
  axis, avoiding an assumption about terrain flatness this spec has no need to make.
- Positions are supplied externally, once per evaluation, by the calling adapter (owned by
  monster-ai/001's `MonsterController` or an equivalent future integration point) — this feature
  does not locate, cache, or subscribe to either `Transform` itself; it is a pure function of
  whatever positions it is handed.
- `MonsterDetectionSignal`'s type/shape is authored by monster-ai/001 (already implemented at
  `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs` per that spec's own tasks.md), even
  though that spec's FR-003 text names this spec as the nominal "producer type" definer. This
  spec treats that existing struct as the single source of truth and reuses it rather than
  creating a second, competing definition — consistent with constitution Principle V (no silently
  repurposed or duplicated shared type).
- `noiseBaseRadius`/`CurrentNoiseRadius`'s unlocked-placeholder status (GDD Ch. 21) is inherited
  from noise-and-detection/001, not re-litigated here; this feature is written so it is already
  fully correct for whatever final value that spec eventually locks, with zero code changes
  required in this feature when that happens.
- GDD's cut list (Ch. 18.2, "Deteksi lewat cahaya senter") confirms light-based detection is a
  deliberately rejected feature, not an unfinished one — this spec's light-independence
  requirements (FR-009/FR-010) are a permanent guarantee for this version, not a placeholder
  awaiting a future toggle.

## Dependencies

- **Requires** noise-and-detection/001-per-action-noise-emission
  (`specs/systems/noise-and-detection/001-per-action-noise-emission/spec.md`) for the current
  noise radius input (`NoiseEmitter.CurrentNoiseRadius`). Not redefined here.
- **Integrates with** monster-ai/001-state-machine-core-transitions
  (`specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`): that spec's FR-003
  names this spec as the nominal definer of the detection-signal producer type, but the concrete
  `MonsterDetectionSignal` struct already exists at
  `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs` (created ahead of this spec by
  monster-ai/001's own tasks). This feature reuses that existing type without modification — see
  Assumptions. Per ROADMAP §5, monster-ai/001 depends on this feature, not the reverse; this
  feature's own build order is unaffected by monster-ai/001's implementation status.
- **Consumed by** monster-ai/001-state-machine-core-transitions, whose `Tick()` accepts this
  feature's output signal once per tick as its sole detection input
  (`specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` FR-003), and,
  transitively, by every downstream monster-ai spec.

## Related

- GDD Ch. 7.2 (Aturan deteksi) — primary source for this feature's exact detection rule.
- GDD Ch. 7.1 (Noise radius per aksi) / Ch. 17.3 (noise config keys) — context for the radius
  input this feature compares against (owned by noise-and-detection/001).
- GDD Ch. 18.2 (Cut List) — confirms "Deteksi lewat cahaya senter" is a deliberate rejection,
  backing FR-009/FR-010.
- GDD Ch. 21 (Open Items) — `noiseBaseRadius`'s on-device lock status, inherited via
  noise-and-detection/001.
- `specs/ROADMAP.md` §2 noise-and-detection row (`002-distance-based-detection-check | Ch. 7.2 |
  001`), §5 dependency-ordered build sequence.
- `specs/systems/noise-and-detection/001-per-action-noise-emission/spec.md` — upstream radius
  producer this feature consumes.
- `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` — downstream consumer of
  this feature's output signal; not redefined here.
- `.specify/memory/constitution.md` — Principle II (Simplicity/YAGNI, backing the no-raycast/
  no-light-input scope boundaries), Principle III (Unity Architecture Consistency), Principle IV
  (Test-Before-Done).
