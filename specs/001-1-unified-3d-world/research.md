# Phase 0 Research: LILO Phase 1.1 — Unified 3D World & Feel Pass

No `[NEEDS CLARIFICATION]` markers remain in the Technical Context — the `/speckit-clarify`
session on this feature already resolved the three highest-impact unknowns (camera behaviour,
interactable visibility, joystick style). This document records the remaining technical
decisions needed to move from spec to design.

## 1. Single-scene SceneKit renderer (the phase's central change)

- **Decision**: Drop the SpriteKit `SKScene` (001's `TestRoomScene`) entirely. One `SCNScene`
  holds the floor, walls, desk, batteries, door, player character and placeholder monster, all
  as plain `SCNNode`s with box/capsule/cylinder geometry — following the LILO spike's
  `Spike3DController.buildWorld()`/`buildStaticScene()` split (static nodes built once; per-room
  nodes rebuilt on room change, though this phase only ever has one room so the rebuild path is
  unused but kept as the natural extension point for 004's multi-floor work).
- **Rationale**: Spec FR-001/FR-002/FR-008/FR-009's whole point is that one light source lights
  every surface and casts real shadows. `SCNLight` cannot light an `SKSpriteNode` (it isn't a
  scene-graph member of any `SCNScene`), so a hybrid renderer cannot satisfy those FRs no matter
  how it's tuned — this was the root cause identified when v2 was compared against the LILO
  spike. A single scene is the only way to make FR-008 (characters darken with the same light as
  the floor) automatically true rather than something to keep in sync by hand.
- **Alternatives considered**: Keep the hybrid renderer and fake shadows onto the 2D layer
  (projected blob shadows under nodes) — rejected: still can't light the floor/walls themselves,
  and GDD 12.1's stated failure mode ("karakter akan terlihat seperti tempelan di atas
  background") is exactly what this spec exists to fix.

## 2. Shared game state — unchanged

- **Decision**: Keep 001's `@Observable final class GameState` as the single source of truth,
  owned by the SwiftUI root view. `WorldSceneController` (replacing `TestRoomScene`) holds an
  unowned reference and reads/writes it every frame, same relationship 001's `TestRoomScene` had.
- **Rationale**: Spec FR-023 requires 001's gameplay logic and its tests to keep working
  unmodified. `GameState`'s shape doesn't reference SpriteKit or SceneKit types, so nothing about
  the renderer swap touches it. Re-deciding this would violate constitution Principle III for no
  reason.
- **Alternatives considered**: None seriously — re-litigating an already-settled, renderer-
  independent decision is exactly what Simplicity/YAGNI says not to do.

## 3. World coordinate mapping (2D gameplay math → 3D scene)

- **Decision**: `GameState.player.position` and every other position in `GameState`/`TestRoom`
  stay `CGPoint` in a 2D ground plane, exactly as in 001. `WorldSceneController` maps `(x, y)` to
  `SCNVector3(x, 0, -y)` when placing nodes, the same convention the LILO spike uses
  (`vector(point.x, height, -point.y)`). All gameplay math (`MovementController`,
  `CollisionResolver`, `InteractionController`, distance checks) stays 2D and renderer-agnostic;
  only `WorldSceneController` and `CameraController`'s final transform know about the third axis.
- **Rationale**: This is what let 001's `MovementController`/`BatteryController`/
  `InteractionController` avoid depending on SpriteKit types in the first place (their signatures
  take `CGPoint`/`CGVector`/`GameState`, not scene nodes) — keeping that boundary means those
  systems don't change at all for this spec (FR-023), and unit tests for them stay host-free.
- **Alternatives considered**: Store positions as `SCNVector3` / world-space 3D from the start —
  rejected: would ripple through `GameState`, `MovementController`, `InteractionController`,
  `CollisionResolver` and their existing passing tests for no behavioural benefit, and the ground
  plane is genuinely 2D (no vertical gameplay this phase).

## 4. Camera: locked-follow, orthographic, fixed 45° tilt

- **Decision**: Port the LILO spike's yaw/pitch node rig (`yawNode` → `pitchNode` → `cameraNode`)
  but fix yaw at `0°` (GDD 13's "north facing", no rotation option) and drop the spike's
  look-ahead entirely per the clarified FR-012. `yawNode.position` is set directly to the
  post-`CameraController` player-follow position each frame — no look-ahead offset added to it.
  Pitch is `GameConfig.cameraTiltDegrees` (default `45`, per GDD 13), orthographic scale is
  `GameConfig.cameraOrthographicScale` (tuned on-device, was already `TBD` in 001's contract).
- **Rationale**: FR-012 explicitly rejects look-ahead in favor of GDD 13's "selalu di tengah
  layar". 001's `CameraController.update()` (lerp toward target, then clamp to room bounds) is
  kept verbatim as the *2D* position feed — only what consumes that output changes, from
  positioning an `SKCameraNode` to positioning a 3D rig's root node. This is the smallest change
  that satisfies FR-012.
- **Alternatives considered**: Look-ahead (the spike's original approach) — explicitly rejected
  by the clarification. A hand-authored fixed isometric camera with no follow — rejected, breaks
  001 FR-012's smooth-follow requirement, which 001-1 keeps.

## 5. Lighting: one omnidirectional light, eased radius, event-based flicker, readability fill

- **Decision**:
  - One `SCNLight` (`.omni`), attached to (and following) the player node, is the flashlight —
    a pool of light radiating evenly in every direction around the player, matching GDD 12.1's
    own reference ("acuan visualnya: Among Us saat listrik mati") rather than a forward-facing
    beam. This deliberately diverges from the LILO spike's `.spot` lamp (built for a top-down
    maze where the light needs to point *down*, not around a third-person character) and from
    this feature's own original plan, which had proposed a `.spot` cone — revised after the
    user asked for the Among Us-style radius explicitly. Attenuation distance is still derived
    from a single "lit radius" value, same as the spike's `applyLight()` technique, just without
    a cone angle to compute (omni lights have none).
  - The lit radius **eases** toward a per-light-state target using `radius += (target - radius) *
    (1 - exp(-easeRate * dt))` — ported directly from the spike's `LightSystem`/`Spike3DController.
    updateFuel()` — rather than 001's `LightingController`, which set the vignette scale directly
    from light state with no interpolation.
  - **Flicker** (Flickering state only) is a small state machine — `steady → dipping → steady` —
    driven by config-defined interval/duration/depth, not a new random value every frame (001's
    approach, and the LILO spike's own weakness this spec explicitly avoids per spec FR-006).
  - **Readability fill** is one `SCNLight` (`.ambient`), intensity = `GameConfig.
    readabilityFillIntensity`. Setting it to `0` reproduces pure black outside the flashlight,
    satisfying spec User Story 3 Scenario 2 with exactly one tunable — deliberately simpler than
    the spike's three-light readability rig (ambient + directional "moonlight" + wall
    self-glow), which the spec does not require.
- **Rationale**: Directly implements FR-003 (soft falloff via the spot's inner/outer cone
  angle + attenuation curve, not a hard cutoff), FR-005 (eased radius, no jumps), FR-006
  (discrete flicker events), FR-007 (one fill tunable). Reusing the spike's proven formulas
  keeps this a port, not a fresh derivation, minimizing new failure modes.
  - **Fallback rejected**: matching the spike's 3-light fill rig exactly — rejected per
    Simplicity/YAGNI; the spec only requires "a single config value" for fill, and one light
    satisfies that with the least code. Can be extended later if reviewers want it, per 015.
  - **Implementation addendum**: the readability fill is implemented as a single downward-
    pointing `.directional` `SCNLight`, not `.ambient` as originally planned above. On-device
    testing during implementation found that SceneKit's `.ambient` light type did not visibly
    illuminate this project's `.lambert`-lit materials (floor/walls stayed pure black even at a
    deliberately extreme debug intensity, while `.directional` lit them correctly at normal
    values). A `.directional` light is still direction-only (no position/attenuation), so it
    still lights the whole room evenly from one tunable — the "single value, `0` ⇒ pure black"
    contract is unchanged, only the SceneKit light type differs from the original plan.

## 6. Shadows and the performance fallback

- **Decision**: shadow map size and sample count both read from `GameConfig` (`shadowMapSize`,
  `shadowSampleCount`), `lamp.castsShadow` toggled by `GameConfig.shadowsEnabled`.
  `shadowMode` is left at SceneKit's default (`.forward`) rather than the spike's `.deferred` —
  `.deferred` shadow sampling is documented for spot/directional lights; the flashlight is now
  `.omni` (§5), which renders its shadow via a cube map instead of a single 2D depth map.
  - **On-device (simulator) discovery**: at the previous default `shadowMapSize = 2048`, an
    omni light's cube shadow map blew out the entire frame to solid white — confirmed by
    disabling `castsShadow` (fixed it) and separately confirming the light/geometry math were
    already correct via an on-screen debug readout. Reducing `shadowMapSize` to `512` (still
    configurable, still the FR-1.1-022 fallback lever) resolved it with shadows intact.
- **Rationale**: FR-002 (shadows) and FR-022 (must be switchable off, with reducible quality, as
  the agreed performance fallback) are both satisfied by config alone — no separate code path
  for "shadows off" beyond not calling `castsShadow = true`.
- **Alternatives considered**: Baked/static shadows — rejected, the room's only shadow caster
  (the desk) and the light both move relative to each other as the player walks, so shadows must
  be dynamic; baking would not read as correct per FR-002's "shadows MUST update every frame".

## 7. Frame timing — extends 001's wall-clock decision

- **Decision**: Reuse 001's wall-clock delta-time drain (research.md §3 of 001, unchanged) and
  add a single clamp, `GameConfig.maxFrameDelta` (ported from the LILO spike's `1.0 / 20`), applied
  once per frame to movement, light easing, flicker timing and camera follow alike — not a
  separate clamp per system.
- **Rationale**: FR-015 requires time-based, not frame-count-based, movement/light/camera and a
  single defined maximum step so a long stall (e.g. app resume) can't teleport the player through
  a wall or snap the light/camera. The spike already validates this exact clamp value in
  practice.
- **Alternatives considered**: No clamp — rejected, directly contradicts the Edge Case ("a frame
  takes much longer than normal... must not jump"). A per-system clamp — rejected as needless
  duplication; one shared clamp is simpler and easier to keep consistent.

## 8. Desk collision with sliding

- **Decision**: Port the LILO spike's `GridCollider` push-out technique (`push(_:radius:outOf:)`)
  down to the single case this phase needs: one circle (the player) against 001's existing room
  bounds rect (unchanged: independent per-axis clamp, which already produces slide-along-the-wall
  behavior for the outer boundary) plus one new interior rect (the desk). After movement is
  applied each frame, `CollisionResolver` pushes the player's circle out of the desk rect along
  the shallowest penetration axis if it overlaps, which — applied every frame — reads as sliding
  along the desk's edge when approached at an angle, not a hard stop.
- **Rationale**: FR-014 requires sliding, not a dead stop, and 001 FR-020 (desk collision) was
  never actually implemented in code (001's `TestRoomScene` drew the desk with no physics/
  collision body). This spec is the first to need real geometric collision math, and the spike
  already has a battle-tested, dependency-free implementation to adapt.
- **Alternatives considered**: `SCNPhysicsBody`-based collision — rejected per Simplicity/YAGNI;
  pulls in SceneKit's physics simulation, contact delegates and a physics world tick for a single
  static rectangle, when nine lines of push-out math (already proven in the spike) does the job
  and stays a plain, unit-testable function over `GameState`.

## 9. Interactable highlight as an unlit halo node

- **Decision**: `HighlightController` attaches a thin, unlit (`.constant` lighting model) ring or
  outline `SCNNode` to each interactable (battery, door), sized slightly larger than the object.
  Because it's unlit, its drawn brightness is exactly its material's emission color × the
  controller's chosen intensity — never affected by the flashlight, ambient fill, or shadow.
  Intensity switches between `GameConfig.highlightOutOfRangeIntensity` and
  `GameConfig.highlightInRangeIntensity` based on the same in-range test `InteractionController`
  already computes reusable from 001. Color is `GameConfig.highlightColor`.
- **Rationale**: FR-013 requires the highlight to work identically lit or unlit ("MUST be
  unaffected by shadows... a highlight drawn around the object marks it" — see the spec's
  Clarifications). An unlit material is the direct SceneKit expression of "unaffected by
  lighting"; no custom shader is needed.
- **Alternatives considered**: A screen-space overlay drawn by the HUD SwiftUI layer instead of a
  3D node — rejected: would need to reproject the object's 3D position to screen space every
  frame and wouldn't be occluded by nearer geometry the way GDD 4.2's highlight (drawn "pada
  objek", on the object) implies; a 3D node in the same scene gets correct occlusion for free,
  same as the rest of FR-010.

## 10. Floating joystick — SwiftUI-level, renderer-independent

- **Decision**: `JoystickView` becomes state-driven on a "control zone" (`GeometryReader`-sized
  rect on the left, `GameConfig.joystickControlZoneWidthFraction`/
  `GameConfig.joystickControlZoneHeightFraction` fraction of
  screen or point size). It renders nothing until a `DragGesture(minimumDistance: 0)` starts
  inside that zone, at which point the joystick's base is drawn centered at the touch's start
  location; it clears back to nothing on `.onEnded`. Reuses the same deflection math 001 already
  has (`min(baseRadius, ...)`, angle/deflection → `CGVector`).
- **Rationale**: FR-019 requires floating behaviour; this is purely a SwiftUI overlay concern
  (touch handling was never coupled to SpriteKit in 001 — `JoystickView` already lived in
  SwiftUI, writing straight into `GameState.joystickVector`), so it needs no renderer-side
  change and carries zero risk to the SceneKit migration.
- **Alternatives considered**: Route joystick touches through the SceneKit view's own touch
  handling (like the LILO spike's `GameViewController.touchesBegan` routing into
  `Spike3DController`) — rejected: 001's SwiftUI-native `DragGesture` approach already works and
  is simpler than UIKit touch routing; no reason to introduce that indirection.

## 11. Test target — extends, doesn't replace, 001's

- **Decision**: Add new test files to the existing `v2Tests` target rather than creating a
  second target. New coverage: light-radius easing math, flicker event state machine, and
  `CollisionResolver`'s push-out math — all designed with no `SceneKit`/`SwiftUI` import needed,
  matching 001's precedent for `BatteryController`/`GameConfig` tests.
- **Rationale**: Constitution Principle IV — tests must exist before done, and 001's target
  already exists and is correctly configured (`BUNDLE_LOADER`/`TEST_HOST`/`TEST_TARGET_NAME`,
  built in Debug). No reason to duplicate that setup.
- **Alternatives considered**: None — this is a direct continuation of an already-made decision.
