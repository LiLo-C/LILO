# Feature Specification: Wall & Furniture Collision (Sliding Resolution)

**Feature Branch**: `002-wall-and-furniture-collision`

**Created**: 2026-09-17

**Status**: Implemented; target-hardware feel validation pending

**GDD Sources**: Ch. 16.3 (level validation checklist implies solid geometry the player cannot
pass through); feel/physicality requirement carried by this vault's ROADMAP.md (sliding, not
stopping dead, and never clipping through)

**Input**: User description: "Sliding collision against walls/solid furniture — the character must
slide along a surface it's pushed into at an angle, never stop dead or clip through. A plain C#
collision-resolution system, deliberately NOT using Unity's physics engine/Rigidbody, kept as pure,
EditMode-testable math (e.g. circle-vs-rect push-out)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sliding Along a Wall Hit at an Angle (Priority: P1)

A player walks or sprints toward a wall or a piece of solid furniture at an angle, and instead of
stopping dead or getting stuck, their character keeps moving — sliding smoothly along the surface
in whichever direction their input still has a component pointing along it.

**Why this priority**: This is the entire reason this spec exists (per the feature's explicit
scope): a survival-horror chase only feels fair if grazing a wall while fleeing doesn't halt the
player outright. Every other scenario in this spec is a variation or edge case of the same
push-out math.

**Independent Test**: Feed the collision resolver a player position overlapping one rectangular
obstacle at a 45° approach angle and confirm the corrected position preserves movement along the
wall's tangent while removing only the penetrating component.

**Acceptance Scenarios**:

1. **Given** the player's collision circle is overlapping a single rectangular obstacle after a
   movement step, **When** the position is resolved, **Then** the corrected position has zero
   penetration into the obstacle.
2. **Given** the player approaches a flat wall at an oblique angle (neither head-on nor parallel),
   **When** the position is resolved, **Then** the component of motion parallel to the wall's
   surface is preserved — the character continues moving along the wall rather than stopping.
3. **Given** the player approaches a wall exactly parallel to it, **When** the position is
   resolved, **Then** no push-out occurs at all (no overlap exists) and full speed along the wall
   is preserved.

---

### User Story 2 - Stopping Cleanly on a Head-On Wall (Priority: P1)

A player walks or sprints straight into a wall or solid furniture face-on, and their character
stops right at the surface — it does not clip through even a single frame, and it does not
vibrate or jitter in place.

**Why this priority**: The direct counterpart to sliding: a system that only slides but lets
head-on collisions tunnel through would be worse than no collision system at all. Tied at P1 with
User Story 1 because both describe the same underlying push-out formula from different approach
angles.

**Independent Test**: Feed the resolver a straight-on (perpendicular) approach into a single
obstacle across a range of approach speeds and confirm the corrected position always sits flush
with the obstacle's face, never inside it and never oscillating between calls.

**Acceptance Scenarios**:

1. **Given** the player's movement step would fully overlap an obstacle head-on, **When** the
   position is resolved, **Then** the corrected position has the player's collision circle exactly
   touching (not overlapping, not gapped by a visible margin) the obstacle's face.
2. **Given** the same head-on scenario is resolved repeatedly with the same held input, **When**
   comparing consecutive resolved positions, **Then** they are identical — no jitter or drift frame
   to frame.

---

### User Story 3 - Resolving Cleanly in a Corner (Priority: P2)

A player gets pushed into the inside corner formed by two walls (or a wall and a furniture piece)
meeting at roughly a right angle, and their character settles at the corner without vibrating
between the two surfaces or being ejected out of either one.

**Why this priority**: Corners are the classic failure mode for simple push-out collision (each
obstacle's resolution can re-trigger the other's). It is a correctness requirement for any level
with intersecting walls or furniture placed against a wall — common in an office layout (GDD Ch.
16, desks against walls) — but it builds on, rather than gates, User Stories 1–2.

**Independent Test**: Feed the resolver a player position overlapping two perpendicular obstacles
simultaneously and confirm the resolved position converges to a stable point at or near the corner
within a small, fixed number of resolution passes, with no further movement on repeated identical
calls.

**Acceptance Scenarios**:

1. **Given** the player's collision circle overlaps two obstacles meeting at a corner, **When**
   the position is resolved, **Then** the final position has zero penetration into both obstacles
   simultaneously.
2. **Given** the corner scenario is resolved repeatedly with the same held input, **When** comparing
   consecutive resolved positions, **Then** they converge and stop changing — no infinite
   back-and-forth between the two obstacles' push-out vectors.

---

### User Story 4 - Recovering From an Overlapping Spawn (Priority: P3)

If a character's starting position (checkpoint respawn, floor entry) happens to sit exactly on or
just inside an obstacle's edge due to level-authoring imprecision, the character is pushed out to
the nearest free point smoothly rather than being flung across the room.

**Why this priority**: A defensive correctness case rather than a moment-to-moment feel
requirement — it protects level designers and the lives-and-fail-state respawn flow (GDD Ch. 9.2)
from a rare authoring mistake turning into a visible bug. Lower priority because it only matters
when an upstream authoring error already exists.

**Independent Test**: Place the player's collision circle so it fully or partially overlaps an
obstacle with zero initial velocity and confirm the resolved position is the nearest point outside
the obstacle, at a distance no greater than the original penetration depth.

**Acceptance Scenarios**:

1. **Given** the player's collision circle starts already overlapping an obstacle with no input
   velocity, **When** the position is resolved, **Then** the corrected position is the nearest
   point on the obstacle's boundary plus the collision radius — not an arbitrarily distant or
   randomly chosen point.

---

### Edge Cases

- Two obstacles' push-out vectors directly oppose each other (player wedged in a gap narrower than
  their collision diameter): the resolver MUST still return a deterministic position (e.g., the
  average or the smaller-penetration axis) rather than NaN or an unbounded value; this is a
  level-design error to avoid (minimum gap width should exceed player diameter) but the math must
  degrade safely if it happens.
- An obstacle with zero width or height, or a degenerate/malformed bounds (min ≥ max on an axis):
  MUST NOT produce NaN, infinity, or a divide-by-zero — defensively rejected or treated as no
  obstacle.
- Very high sprint speed against a thin obstacle: this system resolves overlap after a movement
  step is tentatively applied; if an obstacle's thickness is smaller than the maximum distance the
  player can travel in one frame at sprint speed, the player can tunnel through before any overlap
  is ever detected. This is a level-authoring constraint (minimum obstacle thickness relative to
  the frame-delta-clamped maximum travel distance — see Assumptions), not something this spec's
  discrete push-out math is asked to solve with swept collision (constitution Principle II:
  YAGNI — a full swept-collision system is not justified by this feature's scenarios).
- Multiple obstacles overlapping the player at once (three or more): resolution MUST be applied
  against each overlapping obstacle in a stable, repeatable order so the same input always
  produces the same output (no per-frame order-dependent flicker).
- The player's collision circle exactly touches an obstacle's face with zero penetration (boundary
  case): MUST NOT be treated as an overlap requiring push-out (avoids a permanently-active
  "resolving" state that never actually moves anything).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST detect overlap between the player's collision circle (position +
  radius) and a static rectangular obstacle (axis-aligned bounds), and, when overlap exists,
  compute a corrected player position with zero penetration.
- **FR-002**: The correction MUST be a minimum push-out — the smallest displacement along the axis
  of least penetration that eliminates overlap — so that any component of the player's motion not
  aligned with that axis is preserved (this is what produces sliding: FR-002 is the single
  mechanism behind both User Story 1's sliding and User Story 2's clean head-on stop, which differ
  only in approach angle, not in formula).
- **FR-003**: The system MUST NOT ever leave the player's corrected position overlapping the
  obstacle it just resolved against (zero residual penetration, within floating-point tolerance).
- **FR-004**: When the player's collision circle overlaps more than one obstacle in the same
  resolution step, the system MUST resolve against each in a fixed, repeatable order and MUST
  converge to a stable position with no penetration remaining against any of them, without
  oscillating indefinitely (bounds the corner case in User Story 3 to a small, fixed number of
  passes).
- **FR-005**: The system MUST operate purely on plain position/radius/bounds data — it MUST NOT
  depend on `UnityEngine.Rigidbody`, `Collider` overlap/trigger callbacks, or any PhysX simulation
  step (constitution Principle II: no physics engine for this scope; Principle IV: must be
  EditMode-testable without a running scene).
- **FR-006**: The player's collision circle radius MUST be a `GameConfig` field (`playerRadius`),
  not a hardcoded literal in the resolver or its caller.
- **FR-007**: Obstacle bounds (walls, solid furniture) are level-authored data — each obstacle's
  rectangular bounds are defined per level/scene, not a single global tunable; this spec's math
  MUST accept any set of axis-aligned bounds as input, generic to whatever a level provides.
- **FR-008**: The resolver MUST be re-run every frame, after the player's tentative movement step
  for that frame has been applied and before that position is treated as final (so the corrected
  position, not the pre-collision one, is what the camera, noise system, and rendering all see).
- **FR-009**: Starting (or being teleported to, e.g. on respawn) at a position already overlapping
  an obstacle MUST resolve to the nearest valid point outside it, bounded by the original
  penetration depth — never an arbitrarily large correction.
- **FR-010**: The collision math MUST be implemented as a plain C# system taking positions, radii,
  and bounds as parameters and returning a corrected position — with no `MonoBehaviour`,
  `Component`, or scene-graph dependency (constitution Principle III), so it is fully unit-testable
  in EditMode.

### Key Entities

- **CollisionResolver**: The plain C# system that computes a corrected position given the player's
  circle (position + `playerRadius`) and one or more static obstacle bounds. Pure math, no engine
  dependency.
- **Obstacle**: A static, axis-aligned rectangular bounds value representing one wall segment or
  one piece of solid furniture. Authored per level/scene; this spec does not define where that
  authoring data lives beyond "a set of bounds the resolver consumes."
- **CollisionResult**: The output of a resolution pass — the corrected position (and, where useful
  for debugging/tests, which obstacle(s) it resolved against).
- **PlayerCharacter**: The player's on-screen avatar (fixed name, per project convention). Its
  `Transform` is what the resolved position is ultimately applied to, via a thin adapter.
- **GameConfig fields added by this spec**: `playerRadius`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Across a wide sweep of approach angles into a single obstacle, the corrected position
  always has zero measurable penetration into that obstacle.
- **SC-002**: For every tested oblique approach angle (neither head-on nor parallel), the
  character's resulting movement retains a non-zero component along the obstacle's surface — the
  character visibly continues moving rather than halting.
- **SC-003**: For a purely head-on approach, the character's resulting velocity perpendicular to
  the surface is fully removed at the moment of contact, across every tested approach speed —
  never observed clipping through.
- **SC-004**: In a two-obstacle corner scenario, repeated resolution against the same held input
  converges to one stable position within a small, fixed number of passes and does not continue
  changing on further identical calls.
- **SC-005**: A character whose starting position overlaps an obstacle is moved no farther than
  the original penetration depth to reach a valid position — never visibly "launched" across the
  room.
- **SC-006**: Every scenario above is verified without needing a live scene, a physics step, or any
  engine collision callback — the underlying math is exercised entirely through direct, isolated
  calls.

## Assumptions

- Collision is resolved on the horizontal (ground) plane only — obstacle height and the player's
  vertical position are not part of this system's math, consistent with the game's fixed,
  non-rotating tilted-orthographic camera (see movement-and-camera/004) presenting what is
  functionally top-down movement on a 3D-rendered set.
- The player's collision shape is approximated as a circle (radius `playerRadius`); obstacles are
  approximated as axis-aligned rectangles. This is the simplest shape pairing that satisfies the
  sliding requirement (constitution Principle II) — more complex shapes (capsules, rotated
  obstacles) are not justified by any current user story and are not built ahead of need.
- `playerRadius` uses a provisional value of `0.5` world units, matching the current
  `PlayerCharacter` capsule and the current 20×20 prototype room. The earlier carried-over value
  of `20` belongs to a different world scale and is not suitable for this scene. `0.5` remains a
  feel value to re-confirm on target hardware, not a locked constant.
- Preventing tunneling through obstacles thinner than one frame's maximum travel distance at sprint
  speed is treated as a level-authoring constraint (minimum wall/furniture thickness), not a
  runtime concern for this spec's discrete resolution math — consistent with keeping this system a
  small, YAGNI-justified piece of pure math rather than a full swept-collision engine.
- Frame-delta spikes are assumed to be clamped upstream by `GameManager` (constitution Principle
  III), the same assumption made in movement-and-camera/001; this spec's math takes a tentative
  post-movement position as a given input, whatever produced it.
- This spec covers static obstacles only (walls, fixed furniture). Moving obstacles (e.g., a
  monster's own collision, if any) are out of scope and belong to their owning spec
  (`monster-ai/*`) if ever needed.

## Related

- GDD: Ch. 16.3 (level validation checklist — implies solid, non-passable geometry); the sliding
  vs. stop-dead vs. clip-through distinction is a feel/physicality requirement recorded directly in
  ROADMAP.md's `movement-and-camera` row for this feature (no single numbered GDD chapter states it
  verbatim, since the GDD does not separately spec collision math).
- ROADMAP.md: `systems` table, `movement-and-camera` row `002-wall-and-furniture-collision`; see §0
  for shared config conventions and §5 for build order (depends on
  `movement-and-camera/001-joystick-movement-and-sprint`, which supplies the tentative movement
  step this system resolves each frame).
- Depends on: `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`
  for where `GameState`/`GameManager` are defined (not redefined here); and
  `movement-and-camera/001-joystick-movement-and-sprint/spec.md` for the movement step this
  resolver runs against each frame.
- Constitution: Principle II (Simplicity & YAGNI — explicitly why this is plain math, not
  Rigidbody/PhysX); Principle IV (Test-Before-Done — why this must be EditMode-testable).
