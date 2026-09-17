# Contract: GameConfig Additions (001-1)

These fields are **added** to the single tunables source defined by 001 FR-015 and documented in
[001's `contracts/game-config.md`](../../001-core-prototype/contracts/game-config.md) — they do
not create a second config source (spec FR-021). `GameConfig.cs` gets one new nested class/section
for this feature's fields; every system listed under "Reads" MUST read from the same
`GameConfig.asset`, none hardcoded at the call site. Defaults marked *(carried over)* come from
on-device tuning already done for the equivalent build under the project's previous engine — a
known-good starting point to re-confirm on-device in Unity, not a guarantee (URP's light/shadow
units and the previous engine's aren't numerically identical even where the visual target is).

| Key | Type | Default | Source / Notes | Reads |
|---|---|---|---|---|
| `flashlightIntensity` | `float` (URP Point light intensity) | `1200` *(carried over, re-tune — light-unit systems differ between engines)* | Fixed brightness of the flashlight's `Light` itself. Only its *reach* (`Light.range`, derived from the radii below) changes with light state — this value does not (FR-001). | `LightingRig` |
| `flashlightColor` | color (RGB) | warm white (1.0, 0.91, 0.75) | Flashlight tint (FR-001). | `LightingRig` |
| `flashlightNormalRadius` | `float` (world units) | `220` *(carried over)* | Normal-state lit radius; small enough that darkness is visible on all four landscape screen edges (FR-004). Re-tune on-device. | `LightRadiusEasing` |
| `flashlightCriticalMinRadius` | `float` (world units) | `normalRadius * compactDarknessRadiusFraction` (= `22`) | Lit radius at the bottom of Critical and the final Compact Darkness target (FR-005). Computed, not hand-entered, so it can never drift from `flashlightNormalRadius * compactDarknessRadiusFraction`. | `LightRadiusEasing` |
| `lightRadiusEaseRate` | `float` (1/s, exponential ease rate) | `4.0` *(carried over)* | How fast the displayed radius chases its target (FR-005). Re-tune on-device for this room's scale. | `LightRadiusEasing` |
| `flickerIntervalMin` | `float` (seconds) | `0.4` *(carried over)* | Shortest gap between flicker dip events while in Flickering (FR-006). | `LightRadiusEasing` |
| `flickerIntervalMax` | `float` (seconds) | `1.4` *(carried over)* | Longest gap between flicker dip events (FR-006). Must be `≥ flickerIntervalMin`. | `LightRadiusEasing` |
| `flickerEventDuration` | `float` (seconds) | `0.15` *(carried over)* | How long one dip event lasts, start to recovery (FR-006). | `LightRadiusEasing` |
| `flickerDipFraction` | `float` (0–1) | `0.45` *(carried over)* | How far the lit radius/intensity dips during an event, as a fraction of the Flickering-state target (FR-006). `0` = no visible dip, `1` = dips to nothing. | `LightRadiusEasing` |
| `readabilityFillIntensity` | `float` (URP Directional light intensity) | `220` *(carried over, re-tune — light-unit systems differ between engines)* | The single tunable controlling out-of-flashlight readability (FR-007). `0` MUST produce pure black outside the lit radius (User Story 3 Scenario 2). Implemented as a downward-pointing `Directional` light, not global ambient — see research.md §5. | `LightingRig` |
| `cameraTiltDegrees` | `float` | `45` | Already existed as a placeholder in 001's contract; this spec supplies the default per GDD 13 (FR-012). | `CameraRig` |
| `cameraOrthographicScale` | `float` | `260` *(carried over)* | Already existed as a placeholder in 001's contract; fixed zoom, no dynamic zoom (FR-012). Re-tune on-device. | `CameraRig` |
| `cameraDistance` | `float` (world units) | `900` *(carried over)* | How far back along the tilt axis the camera sits (needed for an orthographic rig; irrelevant to the visible framing but must not clip room geometry). | `CameraRig` |
| `shadowsEnabled` | `bool` | `true` | Performance fallback switch (FR-022). Setting `false` sets `Light.shadows = LightShadows.None` with no other code change. | `LightingRig` |
| `shadowMapSize` | `int` (pixels, square) | `512` *(carried over)* | Shadow map resolution (maps to URP's shadow resolution tier for this light); lower value is the "reduced quality" half of FR-022's fallback. The flashlight is a `Point` light (cube shadow map, six faces) — the equivalent native build found `2048` blew the frame out to solid white on-device; `512` was the smallest tested value that rendered shadows correctly there. Start here and re-test upward only if perf allows; Unity's cube-shadow behavior at high resolution should still be re-verified on-device since it isn't guaranteed to fail identically. | `LightingRig` |
| `shadowSampleCount` | `int` | `8` *(carried over)* | Shadow softness sample count; lower value trades softness for performance, same fallback as above. | `LightingRig` |
| `highlightColor` | color (RGB) | amber (1.0, 0.85, 0.3) | The one color used for both highlight intensity levels (FR-013). | `HighlightView` |
| `highlightOutOfRangeIntensity` | `float` (0–1, emission intensity) | `0.18` | Faint level, visible in every light state including Compact Darkness (FR-013). Must be `> 0` (never invisible) and `< highlightInRangeIntensity`. | `HighlightController` |
| `highlightInRangeIntensity` | `float` (0–1, emission intensity) | `0.6` | Clearly stronger level shown while an object is within interaction range (FR-013). | `HighlightController` |
| `wallHeight` | `float` (world units) | `110` *(carried over)* | Wall/desk solid height (FR-011). Trades shadow reach against how much of the room the walls hide. | `SceneNodeFactory`-equivalent scene setup |
| `wallThickness` | `float` (world units) | `16` *(carried over)* | Wall solid thickness (FR-011). | Scene setup |
| `playerRadius` | `float` (world units) | `20` *(carried over)* | Player collision circle radius (FR-014). `CollisionResolver` needs a body size to collide with. | `CollisionResolver` |
| `maxFrameDelta` | `float` (seconds) | `1/20` *(carried over)* | Clamp on one frame's delta time, applied to movement/light-easing/flicker/camera alike (FR-015). | `GameManager` (applies the clamp once, shares it with every system) |
| `batteryPickupAnimationDuration` | `float` (seconds) | `0.22` *(carried over)* | How long a picked-up battery's grow/fade-out animation takes (FR-016). | `BatteryPickup` |
| `doorOpenAnimationDuration` | `float` (seconds) | `0.35` *(carried over)* | How long the door's closed→open animation takes (FR-017). | `DoorController` |
| `joystickControlZoneWidthFraction` | `float` (0–1, fraction of screen width) | `0.5` *(carried over)* | Width of the left-side zone a touch may start in to spawn the floating joystick (FR-019). Replaces 001's never-numerically-supplied fixed `joystickCenterOffset`. | `JoystickUI` |
| `joystickControlZoneHeightFraction` | `float` (0–1, fraction of screen height) | `1.0` *(carried over)* | Height of the same zone. | `JoystickUI` |

## Fields explicitly *not* added

- `joystickDiameter`, `joystickDeadZone`, `joystickOpacity`, `actionButtonSize`,
  `actionButtonCenterOffset`, `actionButtonTouchRadius` — already exist in 001's contract and are
  unaffected by this spec; the joystick becomes floating (a new *zone*, above) but its own base
  size/opacity/dead-zone are unchanged.
- A `characterRenderMode` switch — out of scope for 001-1 (see spec.md "Impact on Other Specs");
  015 owns that decision.

## Non-negotiable shape rules

- Same rules as 001's contract: one source asset, placeholder fields present with a documented
  value (not omitted) so every system is wired up before on-device tuning happens, later specs
  never create a second config source.
- The configured radius boundary MUST be internally consistent:
  `flashlightCriticalMinRadius == flashlightNormalRadius * compactDarknessRadiusFraction`.
