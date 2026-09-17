# Phase 0 Research: LILO Phase 1.1 — Unified 3D World & Feel Pass

No `[NEEDS CLARIFICATION]` markers remain in the Technical Context — the `/speckit-clarify`
session on this feature already resolved the three highest-impact unknowns (camera behaviour,
interactable visibility, joystick style). This document records the remaining technical
decisions needed to move from spec to design.

*Engine migration note: this feature was previously planned as a renderer migration (SpriteKit
2D + SceneKit 3D → one SceneKit scene). Under Unity, 001 already has one URP 3D scene from the
start, so §1–§3 below (which used to be about *building* a unified scene) are replaced with
notes on what's already true and needs no further work; the real remaining decisions are §4
onward (shadows, lighting feel, collision, highlights, joystick), which are unchanged in
substance from the original feature's intent.*

## 1. One scene — already true, nothing to build

- **Status**: 001's `TestRoom.unity` already holds the floor, walls, desk, batteries, door,
  player character and placeholder monster as plain `GameObject`s in one scene, lit by one
  `Light`. There is no second scene/renderer to merge.
- **Rationale**: Spec FR-001/FR-002/FR-008/FR-009's whole point — one light source lights every
  surface and casts real shadows, and characters darken with the same light as the floor — is
  automatically true of any Unity scene with one `Light` component. Nothing here is this spec's
  job to implement.
- **Alternatives considered**: N/A — not a decision point anymore.

## 2. Shared game state — unchanged

- **Decision**: Keep 001's plain C# `GameState` as the single source of truth, owned by
  `GameManager`. `LightingRig`/`CollisionResolver`/`HighlightController` hold a reference and
  read/write it every frame, same relationship 001's systems already have.
- **Rationale**: Spec FR-023 requires 001's gameplay logic and its tests to keep working
  unmodified. `GameState`'s shape doesn't reference any rendering type, so nothing about this
  feel pass touches it. Re-deciding this would violate constitution Principle III for no reason.
- **Alternatives considered**: None seriously — re-litigating an already-settled,
  render-independent decision is exactly what Simplicity/YAGNI says not to do.

## 3. World coordinate mapping — unchanged

- **Decision**: `GameState.player.position` and every other position in `GameState`/`TestRoom`
  stay `Vector2` on a 2D ground plane, exactly as in 001. `PlayerRig`/`CameraRig`/`LightingRig`
  map `(x, y) → (x, 0, y)` when placing/orienting Unity objects. All gameplay math
  (`MovementController`, `CollisionResolver`, `InteractionController`, distance checks) stays 2D
  and engine-agnostic; only the `MonoBehaviour` adapters and `CameraController`'s final transform
  know about the third axis.
- **Rationale**: This is what already lets 001's `MovementController`/`BatteryController`/
  `InteractionController` avoid depending on `UnityEngine` types in the first place (their
  signatures take `Vector2`/`GameState`, not `Transform`s) — keeping that boundary means those
  systems don't change at all for this spec (FR-023), and EditMode tests for them stay
  scene-free.
- **Alternatives considered**: Store positions as `Vector3` world-space from the start —
  rejected: would ripple through `GameState`, `MovementController`, `InteractionController`,
  `CollisionResolver` and their existing passing tests for no behavioural benefit; the ground
  plane is genuinely 2D (no vertical gameplay this phase).

## 4. Camera: locked-follow, orthographic, fixed 45° tilt — supplying the tuned values

- **Decision**: `CameraRig` (a yaw/pitch empty-`GameObject` rig → `Camera`) fixes yaw at `0°`
  (GDD 13's "north facing", no rotation option) and applies no look-ahead, per the clarified
  FR-012. The rig's position is set directly to `CameraController`'s player-follow output each
  frame. Pitch is `GameConfig.cameraTiltDegrees` (`45`, per GDD 13), orthographic size is
  `GameConfig.cameraOrthographicScale` (`260`, tuned during on-device work on the project's
  earlier engine and carried over as a starting value — re-confirm once this runs in Unity).
- **Rationale**: FR-012 explicitly rejects look-ahead in favor of GDD 13's "selalu di tengah
  layar". 001's `CameraController.Update()` (lerp toward target, then clamp to room bounds) is
  kept verbatim as the *2D* position feed — only what consumes that output changes, from
  positioning a placeholder transform to positioning a tilted rig. This is the smallest change
  that satisfies FR-012.
- **Alternatives considered**: Cinemachine's virtual camera follow/framing — rejected per
  Simplicity/YAGNI; a fixed-tilt orthographic rig with a lerp-and-clamp feed doesn't need a
  dedicated camera package.

## 5. Lighting: one point light, eased radius, event-based flicker, readability fill

- **Decision**:
  - One URP `Light` (`Point`), attached to (and following) the player `GameObject`, is the
    flashlight — a pool of light radiating evenly in every direction around the player, matching
    GDD 12.1's own reference ("acuan visualnya: Among Us saat listrik mati") rather than a
    forward-facing beam. `Point` is Unity's omnidirectional light type — the direct equivalent
    of the omni light the project's earlier engine settled on after first trying a directional
    beam. Range is derived from a single "lit radius" value via `Light.range`.
  - The lit radius **eases** toward a per-light-state target using
    `radius += (target - radius) * (1 - exp(-easeRate * dt))` — a standard exponential-decay
    ease — rather than 001's `LightingRig`, which sets the light's range directly from
    `LightState` with no interpolation.
  - **Flicker** (Flickering state only) is a small state machine — `steady → dipping → steady` —
    driven by config-defined interval/duration/depth, not a new random value every frame.
  - **Readability fill** is one URP `Light` (`Directional`), intensity =
    `GameConfig.readabilityFillIntensity`. Setting it to `0` reproduces pure black outside the
    flashlight, satisfying spec User Story 3 Scenario 2 with exactly one tunable. A dedicated
    light is used instead of Unity's global ambient/environment lighting (`RenderSettings`) so
    the "one tunable, `0` ⇒ pure black" contract stays explicit and isn't entangled with any
    skybox/reflection-probe contribution to ambient — deliberately simpler than a multi-light
    readability rig, which the spec does not require.
- **Rationale**: Directly implements FR-003 (soft falloff via the light's range/attenuation
  curve, not a hard cutoff), FR-005 (eased radius, no jumps), FR-006 (discrete flicker events),
  FR-007 (one fill tunable).
  - **Fallback rejected**: a 3-light fill rig (ambient + directional "moonlight" + wall
    self-glow) — rejected per Simplicity/YAGNI; the spec only requires "a single config value"
    for fill, and one light satisfies that with the least code. Can be extended later if
    reviewers want it, per 015.

## 6. Shadows and the performance fallback

- **Decision**: shadow resolution and softness both read from `GameConfig`
  (`shadowMapSize`/`shadowSampleCount`, mapped to URP's per-light shadow resolution tier and
  `Light.shadowStrength`/soft-shadow settings), `Light.shadows` toggled between `LightShadows.
  Soft` and `LightShadows.None` by `GameConfig.shadowsEnabled`.
  - **Known risk carried over from the equivalent native build's on-device testing**: an
    omnidirectional light's cube shadow map at a large resolution (`2048`) blew out the entire
    frame to solid white on-device; dropping to a much smaller resolution (`512`) resolved it
    with shadows intact. Unity's `Point` light shadows are also cube-map-based, so budget for the
    same failure mode during this feature's own on-device pass and start from a low shadow
    resolution tier rather than the pipeline default, re-testing upward only if perf allows.
- **Rationale**: FR-002 (shadows) and FR-022 (must be switchable off, with reducible quality, as
  the agreed performance fallback) are both satisfied by config alone.
- **Alternatives considered**: Baked/static shadows — rejected, the room's only shadow caster
  (the desk) and the light both move relative to each other as the player walks, so shadows must
  be dynamic; baking would not satisfy FR-002's "shadows MUST update every frame".

## 7. Frame timing — extends 001's wall-clock decision

- **Decision**: Reuse 001's `Time.deltaTime`-based drain (research.md §3 of 001, unchanged) and
  add a single clamp, `GameConfig.maxFrameDelta` (`1/20`), applied once per frame to movement,
  light easing, flicker timing and camera follow alike — not a separate clamp per system.
- **Rationale**: FR-015 requires time-based, not frame-count-based, movement/light/camera and a
  single defined maximum step so a long stall (e.g. app resume) can't teleport the player through
  a wall or snap the light/camera.
- **Alternatives considered**: No clamp — rejected, directly contradicts the Edge Case ("a frame
  takes much longer than normal... must not jump"). A per-system clamp — rejected as needless
  duplication.

## 8. Desk collision with sliding

- **Decision**: A plain C# `CollisionResolver.Push(circle, radius, rects)` push-out function
  against one circle (the player) and 001's existing room bounds `Rect` (unchanged: independent
  per-axis clamp, already slide-like at the outer boundary) plus one new interior `Rect` (the
  desk). After movement is applied each frame, `CollisionResolver` pushes the player's circle out
  of the desk rect along the shallowest penetration axis if it overlaps, which — applied every
  frame — reads as sliding along the desk's edge when approached at an angle, not a hard stop.
- **Rationale**: FR-014 requires sliding, not a dead stop, and 001's desk `GameObject` had no
  collision component. This is the first spec to need real geometric collision math.
- **Alternatives considered**: `Rigidbody` + `CapsuleCollider`/`BoxCollider` with Unity's built-in
  physics, or `CharacterController.Move()` — rejected per Simplicity/YAGNI *and* Principle IV: a
  physics-engine or `CharacterController` dependency pulls in collision callbacks and a physics
  tick for one static rectangle, and is harder to unit-test without a loaded scene, when a dozen
  lines of push-out math does the job and stays a plain, EditMode-testable function over
  `GameState`.

## 9. Interactable highlight as an unlit halo GameObject

- **Decision**: `HighlightController` (plain C#) computes an intensity per interactable
  (battery, door); `HighlightView` (a thin `MonoBehaviour`) attaches a thin, unlit outline/halo
  `GameObject` to each, sized slightly larger than the object, using an unlit shader so its drawn
  brightness is exactly its material's emission color × the controller's chosen intensity — never
  affected by the flashlight, ambient fill, or shadow. Intensity switches between
  `GameConfig.highlightOutOfRangeIntensity` and `GameConfig.highlightInRangeIntensity` based on
  the same in-range test `InteractionController` already computes, reused from 001. Color is
  `GameConfig.highlightColor`.
- **Rationale**: FR-013 requires the highlight to work identically lit or unlit ("MUST be
  unaffected by shadows... a highlight drawn around the object marks it" — see the spec's
  Clarifications). An unlit material is the direct Unity expression of "unaffected by lighting";
  no custom shader logic beyond an unlit/emission material is needed.
- **Alternatives considered**: A screen-space overlay drawn by the HUD `Canvas` instead of a 3D
  `GameObject` — rejected: would need to reproject the object's 3D position to screen space
  every frame and wouldn't be occluded by nearer geometry the way GDD 4.2's highlight (drawn "pada
  objek", on the object) implies; a 3D `GameObject` in the same scene gets correct occlusion for
  free, same as the rest of FR-010.

## 10. Floating joystick — UI-level, render-independent

- **Decision**: `JoystickUI` becomes state-driven on a "control zone" (a `RectTransform`-sized
  rect on the left, `GameConfig.joystickControlZoneWidthFraction`/
  `GameConfig.joystickControlZoneHeightFraction` fraction of screen size). It renders nothing
  until a pointer-down (Input System) starts inside that zone, at which point the joystick's base
  is drawn centered at the touch's start location; it clears back to nothing on pointer-up.
  Reuses the same deflection math 001 already has (`min(baseRadius, ...)`, angle/deflection →
  `Vector2`).
- **Rationale**: FR-019 requires floating behaviour; this is purely a `uGUI`/Input System
  overlay concern (touch handling was never coupled to the 3D scene in 001 — `JoystickUI` already
  lived on the HUD `Canvas`, writing straight into `GameState`), so it needs no scene-side change
  and carries zero risk to anything else in this feature.
- **Alternatives considered**: Routing joystick touches through the 3D scene's own raycast/touch
  handling — rejected: 001's UI-native pointer-event approach already works and is simpler; no
  reason to introduce that indirection.

## 11. Test assembly — extends, doesn't replace, 001's

- **Decision**: Add new test files to the existing `Assets/Tests/EditMode` assembly rather than
  creating a second one. New coverage: light-radius easing math, flicker event state machine, and
  `CollisionResolver`'s push-out math — all designed with no `UnityEngine.MonoBehaviour`/scene
  dependency, matching 001's precedent for `BatteryController`/`GameConfig` tests.
- **Rationale**: Constitution Principle IV — tests must exist before done, and 001's assembly
  already exists and is correctly configured. No reason to duplicate that setup.
- **Alternatives considered**: None — this is a direct continuation of an already-made decision.
