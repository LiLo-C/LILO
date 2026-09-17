# Feature Specification: Camera Follow & Boundary Clamp

**Feature Branch**: `003-camera-follow-and-boundary-clamp`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 13 (Camera Specification — follow, player-centering, boundary), Ch. 16.3
(level validation checklist item on never seeing outside the level)

**Input**: User description: "The camera lerps toward the player's position each frame and clamps
to the current level's bounds so no out-of-level space is ever visible. Per-frame follow math,
distinct from the static projection/tilt/zoom setup."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - The Camera Smoothly Tracks the Player (Priority: P1)

As a player moves around a floor, the camera eases toward their position every frame rather than
snapping instantly or lagging indefinitely, so movement feels grounded and cinematic instead of
jerky.

**Why this priority**: This is the GDD's explicit, non-negotiable camera decision (Ch. 13: "Smooth
follow (lerp), bukan fixed-snap") and the foundation every other scenario in this spec refines.
Without it, boundary clamping would just be clamping a snapped camera, which is a different (and
explicitly rejected) feel.

**Independent Test**: Move a simulated player position by a large, sudden amount and confirm the
camera's position moves toward it gradually over multiple frames rather than jumping there in one
frame, converging within a bounded number of frames.

**Acceptance Scenarios**:

1. **Given** the camera is at rest at the player's position, **When** the player's position moves,
   **Then** the camera's position moves toward the new player position gradually across
   subsequent frames, not instantly.
2. **Given** the player stops moving after a burst of motion, **When** enough frames pass,
   **Then** the camera's position converges to the player's position and stays there.
3. **Given** the player moves at a constant, ongoing pace, **When** camera position is sampled
   over many frames, **Then** it never overshoots past the player's position.

---

### User Story 2 - The Camera Never Shows Outside the Level (Priority: P1)

As a player walks toward the edge of a floor, the camera stops following past a certain point,
keeping the frame filled with the level's own geometry instead of empty space beyond it.

**Why this priority**: An explicit, testable level-validation requirement (GDD Ch. 13: "Boundary...
supaya player tidak melihat area kosong di luar level"; Ch. 16.3's checklist: "Tidak ada titik di
mana player bisa melihat area kosong di luar level"). Tied at P1 with User Story 1 because a smooth
follow that ignores level bounds is not shippable on its own.

**Independent Test**: Move the simulated player position past the level's authored bounds in each
direction and confirm the camera's resulting position never lets the visible frame extend beyond
those bounds.

**Acceptance Scenarios**:

1. **Given** the player is near a level boundary, **When** the player continues moving toward
   that edge and past where an unclamped camera would follow, **Then** the camera's position stops
   advancing at the point where its visible frame's edge reaches the level boundary.
2. **Given** the player stands exactly at a level's authored boundary corner, **When** the camera
   is resolved, **Then** the visible frame shows only the level's own geometry on both boundary
   axes at once.
3. **Given** the player walks away from the boundary back toward the level's interior, **When** the
   camera is resolved, **Then** it resumes following normally (the clamp is not sticky past the
   point where it's no longer needed).

---

### User Story 3 - The Player Stays Centered When Not Clamped (Priority: P2)

Whenever the camera isn't being held back by a level boundary, the player's character sits at the
exact center of the screen, so aiming, reading the room, and reacting to threats always happen
from the same, predictable framing.

**Why this priority**: An explicit GDD requirement (Ch. 13: "Posisi player selalu di tengah
layar") that is simple to state but easy to silently break while tuning the lerp or the clamp; it
is P2 because it is effectively the steady-state guarantee of User Story 1, not a new mechanism.

**Independent Test**: Hold the player still at the center of a level larger than the viewport for
long enough for the lerp to settle, and confirm the resulting camera position keeps the player
exactly centered.

**Acceptance Scenarios**:

1. **Given** the player is well inside a level's bounds (away from any edge), **When** the camera
   has had enough frames to settle, **Then** the player's position is exactly at the center of the
   visible frame.

---

### User Story 4 - The Camera Handles a Level Smaller Than the Viewport (Priority: P3)

If a floor (or a portion of one) is narrower or shorter than what the camera can see at once, the
camera centers on that axis instead of trying to clamp between two bounds that don't leave room
for it, so the level never appears to jitter or push the player off-center.

**Why this priority**: A defensive correctness case for an authoring situation (a small room) that
may not occur on every floor but must not silently break the clamp when it does. Lower priority
because it is a boundary condition on User Story 2's mechanism, not new player-facing behavior in
the common case.

**Independent Test**: Set a level's bounds narrower than the camera's visible width and confirm
the camera's horizontal position stays centered on that axis regardless of where the player stands
within it.

**Acceptance Scenarios**:

1. **Given** a level's bounds on one axis are narrower than the camera's visible extent on that
   axis, **When** the player moves anywhere within those bounds, **Then** the camera's position on
   that axis remains centered on the level rather than trying to clamp to a range that doesn't
   exist.

---

### Edge Cases

- The player is teleported a large distance in a single frame (checkpoint respawn, floor-to-floor
  transition — GDD Ch. 9.2): a continuous lerp alone would show a visibly wrong, lagging frame
  immediately after load. This spec's continuous follow is not expected to look correct across a
  scene load; an explicit "snap to" bypass for load/respawn moments is a distinct capability this
  spec provides but does not gate the moment-to-moment lerp behind.
- A level's bounds are narrower than the camera's viewport on both axes at once (a very small
  room): both axes center per User Story 4's rule simultaneously.
- Level bounds data is malformed (minimum greater than maximum on an axis): MUST NOT produce a
  camera position that is NaN, infinite, or wildly outside the level — defensively rejected or
  treated as "no clamp" rather than crashing.
- The player stands exactly on a level boundary line (not past it): the clamp's boundary is
  inclusive — standing exactly on the line does not show any out-of-level space, but also does not
  needlessly pull the camera further inward than necessary.
- The camera's visible half-extent changes because of a runtime aspect ratio difference between
  devices: the clamp MUST use the camera's actual current visible half-extent for that frame, not
  a value assumed at authoring time (ties to movement-and-camera/004, which owns the camera's
  actual projection settings).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The camera's tracked position MUST move toward the player's current position every
  frame using a smoothing interpolation (lerp), never an instant snap, consistent with GDD Ch. 13.
- **FR-002**: The smoothing rate MUST live in `GameConfig` (`cameraFollowLerpFactor`). The GDD
  specifies the rule ("smooth follow") but not a locked number — this spec does not fabricate one;
  see Assumptions for the interim default to re-confirm on-device.
- **FR-003**: When unclamped by a level boundary, the camera's tracked position MUST place the
  player's position at the exact center of the visible frame (GDD Ch. 13: "Posisi player selalu di
  tengah layar").
- **FR-004**: The camera's tracked position MUST be clamped so that its visible frame's edges never
  extend past the current level's authored bounds — no out-of-level empty space may ever be
  visible (GDD Ch. 13; Ch. 16.3 checklist item).
- **FR-005**: Level bounds MUST be authored data per level/scene (each floor has its own bounds),
  not a single shared `GameConfig` value, since floor footprints differ (GDD Ch. 3, differing
  layouts per floor).
- **FR-006**: The clamp MUST derive the camera's visible half-extent from the camera's actual
  current orthographic size and aspect ratio for that frame (owned by movement-and-camera/004),
  not from a value assumed once at authoring time — so the clamp remains correct across different
  device aspect ratios.
- **FR-007**: When a level's bounds on a given axis are narrower than the camera's visible extent
  on that axis, the camera MUST center itself on that axis for that level rather than attempting an
  inverted or degenerate clamp range.
- **FR-008**: The follow-and-clamp calculation (current camera position, target/player position,
  lerp factor, level bounds, camera half-extent, and elapsed time in; corrected camera position
  out) MUST be implemented as a plain, engine-lifecycle-independent unit with no `MonoBehaviour`,
  `Component`, or scene-graph dependency, so it is fully testable in EditMode (constitution
  Principle III/IV).
- **FR-009**: A distinct, explicit "snap to" operation MUST be available for moments where a
  continuous lerp would show an incorrect transient frame (checkpoint respawn, floor load) — this
  bypasses the per-frame lerp for that one frame only and does not change FR-001's rule for every
  other frame.
- **FR-010**: The follow-and-clamp calculation MUST run after that frame's player movement has been
  applied (constitution Principle III: `MonoBehaviour` adapters own Unity lifecycle ordering; the
  camera adapter runs its update after the player-movement adapter's), so the camera never lags an
  extra frame behind the player's true position.

### Key Entities

- **CameraFollowSystem**: The plain C# system that computes the next camera position from the
  current position, the player's (target) position, the configured lerp factor, elapsed time, the
  active level's bounds, and the camera's current visible half-extent. Contains both the lerp step
  and the boundary clamp as two composable, independently testable operations.
- **LevelBounds**: Per-level/per-scene authored data describing a floor's playable rectangular
  extent, against which the camera is clamped. Distinct from `GameConfig` because it varies per
  floor.
- **CameraFollowController**: The thin `MonoBehaviour` adapter that owns the actual `Camera`
  component and the reference to the `PlayerCharacter`'s `Transform` and the active scene's
  `LevelBounds`, calling `CameraFollowSystem` each `LateUpdate` and exposing the "snap to"
  operation for respawn/load moments.
- **PlayerCharacter**: The player's on-screen avatar (fixed name, per project convention); its
  position is this system's per-frame follow target.
- **GameConfig fields added by this spec**: `cameraFollowLerpFactor`, `cameraBoundsInset`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After a sudden, large change in the player's position, the camera reaches within a
  small distance of the player's position within a bounded number of frames, without ever
  overshooting past it.
- **SC-002**: Across a full sweep of player positions from well inside a level to past its
  authored edges, the visible camera frame never extends beyond that level's bounds at any sampled
  position.
- **SC-003**: With the player standing still, well inside a level larger than the viewport, the
  camera settles to keep the player exactly centered in the frame within a bounded number of
  frames.
- **SC-004**: In a level (or room) narrower than the viewport on one axis, the camera remains
  centered on that axis regardless of where the player stands within it, with no observed jitter.
- **SC-005**: Immediately after a checkpoint respawn or floor transition, the camera shows the new
  player position correctly on the very first rendered frame — no stale or incorrect intermediate
  frame is ever visible to a human observer.
- **SC-006**: On every floor in the game, a full walkthrough of its perimeter never reveals any
  area outside that floor's level geometry (validates GDD Ch. 16.3's checklist item directly).

## Assumptions

- `cameraFollowLerpFactor` has no GDD-locked number (Ch. 13 states the rule, not a value).
  Following this vault's carried-over-value convention (ROADMAP.md §0), the interim default to
  re-confirm on-device is `0.12` (per-frame-normalized smoothing step), inherited from this
  project's prior on-device tuning pass on an equivalent build.
- `cameraBoundsInset` (an inward margin applied when clamping the camera's visible frame against a
  level's raw authored bounds, so the frame's edge doesn't sit exactly flush with wall geometry) is
  not a GDD-specified value either; the interim default to re-confirm on-device is `80` world
  units, carried over from the same prior tuning pass, under the same "starting point, not a locked
  constant" caveat.
- Level bounds are assumed rectangular and axis-aligned, matching this project's fixed,
  non-procedural, manually-authored level layouts (GDD Ch. 16.1).
- The camera's visible half-extent (needed for FR-006) is supplied by movement-and-camera/004's
  projection/zoom setup; this spec consumes that value each frame rather than owning it.
- A snap-to-target bypass (FR-009) is assumed sufficient for checkpoint/floor-load moments; this
  spec does not design a separate "catch-up" easing curve for those moments beyond an instant
  snap, since the GDD does not ask for one and no user story currently needs it (constitution
  Principle II).

## Related

- GDD: Ch. 13 (Camera Specification — smooth follow, player-centering, boundary clamping), Ch.
  16.3 (level validation checklist — "no out-of-level space visible").
- ROADMAP.md: `systems` table, `movement-and-camera` row `003-camera-follow-and-boundary-clamp`;
  see §0 for shared config conventions and §5 for build order (depends on
  `movement-and-camera/001-joystick-movement-and-sprint` for the player position it follows;
  feeds into `movement-and-camera/004-camera-projection-and-framing`, which owns the camera's
  static tilt/projection and supplies the half-extent this spec's clamp needs).
- Depends on: `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`
  for where `GameState`/`GameManager` are defined (not redefined here).
- Constitution: Principle III (`MonoBehaviour` adapters own Unity lifecycle ordering — why the
  camera update runs in `LateUpdate` after player movement); Principle IV (why the follow/clamp
  math must be EditMode-testable).
