# Feature Specification: Camera Projection & Framing

**Feature Branch**: `004-camera-projection-and-framing`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 13 (Camera Specification — orthographic projection, north-facing tilt, fixed
zoom), Ch. 15.3 (feel-based/device-tuned values must be read from config, never hardcoded)

**Input**: User description: "The camera's static setup: orthographic projection, north-facing
with a fixed tilt angle, and a fixed orthographic size/zoom with no dynamic zoom — config-driven
values distinct from the per-frame follow-and-clamp math owned by
movement-and-camera/003-camera-follow-and-boundary-clamp."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - The World Always Reads as Orthographic, North-Facing, and Tilted (Priority: P1)

As a player looks at the game world at any moment, the camera always presents it through the same
fixed orthographic projection, facing a constant "north" direction, tilted at a constant angle — it
never behaves like a rotating or perspective-distorted view, regardless of which way the player
character is facing or moving.

**Why this priority**: This is the GDD's explicit, non-negotiable camera identity (Ch. 13:
"Proyeksi: Orthographic"; "Sudut: North facing, tilt 45 derajat"). It is the visual foundation every
other camera behavior (follow, clamp, zoom) sits on top of; getting this wrong would make the whole
game read differently from its stated Alien Shooter / Sneaky Sasquatch reference.

**Independent Test**: Query the camera's projection mode and rotation at scene start — before any
player input — and confirm orthographic projection is enabled, yaw is fixed at the project's defined
north-facing orientation, and tilt/pitch equals the configured angle; repeat the query after driving
the player through movement in every direction and confirm none of these three values changed.

**Acceptance Scenarios**:

1. **Given** the camera has been initialized by this feature, **When** its projection mode is
   queried, **Then** it reports orthographic, never perspective.
2. **Given** the camera has been initialized, **When** its yaw (rotation about the world-up axis) is
   queried, **Then** it matches the fixed "north facing" orientation, with no rotation applied.
3. **Given** the camera has been initialized, **When** its tilt/pitch is queried, **Then** it equals
   the `GameConfig` tilt value (defaulting to the GDD-locked 45°).
4. **Given** the player moves in any direction (including turning to face south, east, or west),
   **When** the camera's yaw and tilt are queried again, **Then** both are unchanged from Scenario
   2 and 3 — the camera never rotates to track the player's facing.

---

### User Story 2 - Zoom Never Changes During Play (Priority: P1)

As a player experiences the flashlight's radius shrinking under low battery, sprints, or hides, the
camera's zoom level (how much of the world is visible) stays exactly the same throughout — only the
flashlight's own radius changes, never the camera's framing.

**Why this priority**: An explicit, load-bearing GDD rule (Ch. 13: "Zoom: Tetap, tidak ada zoom
dinamis di versi ini") that protects a core horror mechanic: the player's sense of scale and
distance-to-monster must stay stable so Light State changes (GDD Ch. 5.2/12.1) read as the light
shrinking, not the world zooming. Tied at P1 with User Story 1 because a correct projection with a
drifting zoom would still misrepresent the GDD's camera spec.

**Independent Test**: Sample the camera's orthographic size across a sequence of simulated gameplay
states (all four Light States, sprinting, hiding) and confirm the sampled value never changes from
its configured constant.

**Acceptance Scenarios**:

1. **Given** the flashlight transitions through Normal → Flickering → Critical → Compact Darkness
   (GDD Ch. 5.2), **When** the camera's orthographic size is sampled at each transition, **Then** it
   is identical at every sample — light-radius easing (owned by flashlight-and-battery/003) is a
   fully separate system from camera zoom.
2. **Given** the player alternates between walking, sprinting, and hiding, **When** the camera's
   orthographic size is sampled during each, **Then** it is identical across all three.
3. **Given** the configured `GameConfig` orthographic size value, **When** the camera is queried at
   any point during play, **Then** its actual orthographic size equals that configured value exactly
   — no other system in this feature's scope applies a runtime modifier to it.

---

### User Story 3 - Other Systems Can Get a Correct Visible Ground Footprint (Priority: P2)

As the boundary-clamp system (movement-and-camera/003) needs to know how much of the level is
currently visible so it can stop the camera at a level's edge, this feature provides that visible
ground-plane footprint (half-extent), correctly accounting for the camera's fixed tilt, so the clamp
never lets empty space show and never clamps too aggressively either.

**Why this priority**: 003's own boundary-clamp FR-006 explicitly names this feature as the owner of
the half-extent it consumes; without a correct value here, 003's clamp is either wrong on every
device aspect ratio or wrong because it ignores how a 45°-tilted orthographic view foreshortens the
ground footprint compared to a top-down view. P2 because it is a derived value consumed by another
system, not something a player directly perceives on its own — its correctness is only observable
through 003's behavior.

**Independent Test**: For a fixed orthographic size and tilt angle, compute the ground-plane
half-extent at two or more different runtime screen aspect ratios (representing different iPhone
models) and confirm the horizontal extent scales with aspect ratio while the tilt-foreshortened axis
is computed from the tilt angle rather than passed through unmodified.

**Acceptance Scenarios**:

1. **Given** a configured orthographic size and a runtime aspect ratio, **When** the ground-plane
   half-extent is computed, **Then** its horizontal (non-tilted) component scales proportionally with
   the aspect ratio.
2. **Given** the same orthographic size but a different aspect ratio, **When** the half-extent is
   recomputed, **Then** only the horizontal component changes — the vertical/tilt-axis component
   depends on orthographic size and tilt angle, not aspect ratio.
3. **Given** the fixed 45° tilt, **When** the ground-plane half-extent's tilt-axis component is
   computed, **Then** it reflects the tilt's foreshortening (it is not simply equal to the raw,
   untilted orthographic size).

---

### User Story 4 - Tilt and Zoom Are Tunable Without Touching Code (Priority: P3)

As the team playtests on real devices, they can change how tilted the camera is or how much of the
room is visible by editing the `GameConfig` asset alone, with no script changes, so tuning "the feel
of the view" doesn't require a programmer or a rebuild of game logic.

**Why this priority**: Enforces GDD Ch. 15.3/17's "every tunable number lives in config" rule and
constitution Principle III (single configuration source). P3 because it is a workflow/process
guarantee rather than new player-facing behavior — the player never observes "config-editability"
directly, only its downstream effects (already covered by User Stories 1–3).

**Independent Test**: Edit `cameraTiltAngle` and `cameraOrthographicSize` on the `GameConfig` asset
with no code changes, reload the scene, and confirm the running camera's actual tilt and orthographic
size match the newly edited values.

**Acceptance Scenarios**:

1. **Given** a new value written to `GameConfig`'s tilt field, **When** the scene next loads,
   **Then** the camera's actual pitch matches the new value.
2. **Given** a new value written to `GameConfig`'s orthographic size field, **When** the scene next
   loads, **Then** the camera's actual orthographic size matches the new value.
3. **Given** no code was modified, only the asset, **When** the values are checked, **Then** no other
   file or system holds a second, conflicting hardcoded copy of either value.

---

### Edge Cases

- A degenerate tilt angle (0°, which collapses to a pure top-down view, or 90°+, which collapses the
  ground-plane footprint toward a razor-thin sliver) would make the User Story 3 half-extent
  computation degenerate (divide-by-near-zero or an unbounded footprint): MUST be caught by config
  validation rather than silently producing a broken or infinite clamp for movement-and-camera/003.
- A runtime aspect ratio at the extreme ends of the supported iPhone landscape range (e.g., a very
  short/wide vs. a taller/narrower landscape screen) MUST still produce a finite, sane half-extent —
  no NaN or negative extent.
- A scene is loaded with a `Camera` component left in perspective mode by an Editor default or a
  designer's manual scene edit: this feature's initialization MUST force orthographic mode rather
  than assuming it was already set correctly by whoever last touched the scene.
- This feature's setup runs more than once across repeated scene loads (e.g., floor-to-floor
  transitions, which reload a new floor Scene per GDD Ch. 2.4): re-running the setup MUST be
  idempotent — the resulting projection, yaw, and tilt MUST NOT drift from repeated application.
- Device orientation lock (landscape-only, GDD Ch. 13's "Orientasi device: Landscape" row) is a
  project/build platform setting (Unity Player Settings), not a per-frame or per-scene runtime
  computation — this spec's aspect-ratio math is correct for whatever landscape resolution the
  runtime reports, but enforcing the lock itself is out of this feature's scope (constitution
  Principle II: do not build a runtime system for what a platform setting already guarantees).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The camera MUST be set to orthographic projection (`Camera.orthographic = true`) by
  this feature's own initialization, not left to whatever a scene's Editor-authored default happens
  to be (GDD Ch. 13: "Proyeksi: Orthographic").
- **FR-002**: The camera's yaw (rotation about the world-up axis) MUST be fixed at the project's
  defined north-facing orientation and MUST NOT change in response to player facing or movement
  direction (GDD Ch. 13: "Sudut: North facing").
- **FR-003**: The camera's tilt/pitch angle MUST be a `GameConfig` field (`cameraTiltAngle`),
  defaulting to 45 degrees — the one feel-adjacent camera value the GDD gives an explicit locked
  number for (GDD Ch. 13: "tilt 45 derajat"), so this default is used as-is, not left pending.
- **FR-004**: The camera's orthographic size (world units visible on the vertical screen axis) MUST
  be a `GameConfig` field (`cameraOrthographicSize`). The GDD states only the rule ("Tetap, tidak
  ada zoom dinamis") and gives no explicit number — this spec MUST NOT fabricate a locked default; the
  value is left pending on-device tuning per GDD Ch. 15.3's convention (see Assumptions).
- **FR-005**: Once applied, the camera's orthographic size MUST NOT be modified at runtime by any
  system within this feature's scope — no dynamic zoom exists in this version (GDD Ch. 13). Other
  systems that appear visually similar (e.g., flashlight radius easing, owned by
  flashlight-and-battery/003) MUST NOT be implemented by substituting a camera zoom change.
- **FR-006**: This feature MUST compute and expose a ground-plane visible half-extent, derived from
  `cameraOrthographicSize`, the runtime screen aspect ratio, and `cameraTiltAngle`, for consumption by
  movement-and-camera/003's boundary-clamp math (003's own FR-006 names this feature as that value's
  owner). The tilt-axis component of this half-extent MUST reflect the tilt's foreshortening of the
  ground footprint, not the raw, untilted orthographic size.
- **FR-007**: This feature's camera setup (projection mode, yaw, tilt) MUST be applied once during
  camera initialization and MUST be idempotent — re-applying it across repeated scene loads MUST NOT
  cause the resulting values to drift.
- **FR-008**: `GameConfig` MUST include a validation check that flags a degenerate `cameraTiltAngle`
  (at or below 0°, or at or above 90°) as invalid, since such values make the FR-006 half-extent
  computation degenerate or undefined (mirrors the validation pattern already used for movement
  config in movement-and-camera/001).
- **FR-009**: The ground-plane half-extent computation (orthographic size, aspect ratio, and tilt
  angle in; world-space half-extent out) MUST be implemented as a plain, engine-lifecycle-independent
  unit with no `MonoBehaviour`, `Component`, or scene-graph dependency, so it is fully testable in
  EditMode (constitution Principle III/IV) — mirrors movement-and-camera/003's FR-008 for its own
  math.
- **FR-010**: This feature's scope MUST NOT include per-frame camera-follow lerp or boundary
  clamping (owned entirely by movement-and-camera/003) — it owns only the camera's static
  projection/yaw/tilt/zoom setup and the ground-plane half-extent derived from them.

### Key Entities

- **CameraProjectionSystem**: The plain C# system that computes the ground-plane visible half-extent
  from `cameraOrthographicSize`, a runtime aspect ratio, and `cameraTiltAngle`. Contains no Unity
  scene dependency; consumed once per frame (or once per resolution change) by the adapter below and
  by movement-and-camera/003's follow controller.
- **CameraRigController**: The thin `MonoBehaviour` adapter that owns the actual `Camera` component;
  on initialization, sets `orthographic = true`, applies the fixed north-facing yaw, and applies
  `cameraTiltAngle` from `GameConfig` to the camera's transform; exposes the current ground-plane
  half-extent (via `CameraProjectionSystem`) for movement-and-camera/003's `CameraFollowController` to
  read each frame.
- **PlayerCharacter**: The player's on-screen avatar (fixed name, per project convention); referenced
  here only to state that this feature's camera orientation is independent of the player's facing.
- **GameConfig fields added by this spec**: `cameraTiltAngle` (default `45`, GDD-locked),
  `cameraOrthographicSize` (no default — pending on-device tuning).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At scene start, before any player input, an automated check confirms the camera reports
  orthographic projection and a tilt equal to the configured value (default 45°), in 100% of runs.
- **SC-002**: Across a simulated play pass exercising all four Light States, sprinting, and hiding,
  the camera's orthographic size never changes from its configured value at any sampled point.
- **SC-003**: For at least two distinct simulated aspect ratios spanning the supported iPhone
  landscape range, the computed ground-plane half-extent's horizontal component scales correctly with
  aspect ratio and its tilt-axis component is derived from the tilt angle (not equal to the raw
  orthographic size), verified by direct computation.
- **SC-004**: Editing `cameraTiltAngle` or `cameraOrthographicSize` in the `GameConfig` asset with no
  code changes is reflected in the camera's actual values on the next scene load, in 100% of manual
  verification passes.
- **SC-005**: Attempting to set `cameraTiltAngle` to a degenerate value (≤0° or ≥90°) is caught by
  config validation before it reaches the half-extent computation, in 100% of automated checks.
- **SC-006**: Across every floor scene, re-loading the scene any number of times never changes the
  camera's resulting projection, yaw, or tilt from their first-load values (idempotency).

## Assumptions

- `cameraTiltAngle` uses the GDD-locked value of `45` degrees (GDD Ch. 13 gives this number
  explicitly, unlike most other feel values in this vault) — this is used as the shipped default, not
  merely a starting point to re-confirm.
- `cameraOrthographicSize` has no GDD-locked or prior carried-over number (unlike, e.g.,
  movement-and-camera/003's `cameraFollowLerpFactor`, which had a prior on-device value to carry
  over). Per ROADMAP.md §0's honesty rule, this spec does not invent one — it is left genuinely
  pending on-device tuning, to be set once a floor's layout and typical viewing distance exist to
  tune against (GDD Ch. 15.3, Ch. 21).
- The "north facing" yaw is treated as a single fixed constant (effectively `0` rotation about the
  world-up axis, matching whatever axis the level geometry is authored against) rather than a
  `GameConfig` field, because the GDD states it as a fixed directional decision ("North facing"), not
  a value the team expects to re-tune per Ch. 15.3's tunable-value list.
- The exact trigonometric relationship used to foreshorten the tilt-axis half-extent (FR-006) is an
  implementation detail for this feature's `plan.md`/`research.md`, not specified here — this spec
  only requires that the foreshortening happen and be verifiable, per constitution Principle I
  (specs describe observable behavior, not implementation).
- Device orientation lock (landscape-only) is assumed to be enforced by Unity Player Settings at the
  project level, not by any runtime system this feature owns (see Edge Cases).

## Related

- GDD: Ch. 13 (Camera Specification — orthographic, north-facing tilt, fixed zoom), Ch. 15.3
  (feel/device-tuned values must live in `GameConfig`, not hardcoded).
- ROADMAP.md: `systems` table, `movement-and-camera` row `004-camera-projection-and-framing`; its
  listed dependency is `003-camera-follow-and-boundary-clamp` (build-order sequencing — see §5's
  `003→004` ordering). Functionally, the relationship also runs the other way for one value: 003's
  own `spec.md` (FR-006, Assumptions) already documents that its boundary-clamp math consumes the
  ground-plane half-extent this feature (004) computes, and ships against an interim placeholder
  until 004 lands — once this feature exists, 003's `CameraFollowController` wires in the real value
  from this feature's `CameraRigController` in place of that placeholder. See ROADMAP.md §0 for
  shared config conventions.
- Depends on: `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md` for
  where `GameState`/`GameManager` are defined (not redefined here).
- Constitution: Principle II (Simplicity — device orientation lock is a platform setting, not a
  system this feature builds); Principle III (`GameConfig` as single configuration source; plain C#
  systems vs. thin `MonoBehaviour` adapters); Principle IV (why the half-extent math must be
  EditMode-testable).
