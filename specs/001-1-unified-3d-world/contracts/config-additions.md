# Contract: GameConfig Additions (001-1)

These keys are **added** to the single tunables source defined by 001 FR-015 and documented in
[001's `contracts/game-config.md`](../../001-core-prototype/contracts/game-config.md) — they do
not create a second config source (spec FR-021). `GameConfig.swift` gets one new `extension` /
`enum` section for this feature's keys; every system listed under "Reads" MUST read from here,
none hardcoded at the call site.

| Key | Type | Default | Source / Notes | Reads |
|---|---|---|---|---|
| `flashlightIntensity` | `Double` (SceneKit omni light intensity) | `1200` | Fixed brightness of the flashlight's `SCNLight` itself. Only its *reach* (attenuation, derived from the radii below) changes with light state — this value does not (FR-001). Added during implementation; not itemized separately in the original FR-021 list but required by it ("every new tunable"). | `SceneNodeFactory` |
| `flashlightColor` | color (RGB) | warm white (1.0, 0.91, 0.75) | Flashlight tint (FR-001). Same rationale as `flashlightIntensity`. | `SceneNodeFactory` |
| `flashlightNormalRadius` | `Double` (world units) | `220` | Normal-state lit radius; small enough that darkness is visible on all four landscape screen edges (FR-004). Tuned in-simulator; re-tune on-device (FR-021's "tune by feel" rule). | `LightingController` |
| `flashlightCriticalMinRadius` | `Double` (world units) | `normalRadius * compactDarknessRadiusFraction` (= `22`) | Lit radius at the bottom of Critical and the final Compact Darkness target (FR-005). Computed, not hand-entered, so it can never drift from `flashlightNormalRadius * compactDarknessRadiusFraction`. | `LightingController` |
| `lightRadiusEaseRate` | `Double` (1/s, exponential ease rate) | `4.0` | How fast the displayed radius chases its target (FR-005). Ported technique from the LILO spike's `radiusEaseRate` (spike default `5`); re-tune on-device for this room's scale. | `LightingController` |
| `flickerIntervalMin` | `Double` (seconds) | `0.4` | Shortest gap between flicker dip events while in Flickering (FR-006). | `LightingController` |
| `flickerIntervalMax` | `Double` (seconds) | `1.4` | Longest gap between flicker dip events (FR-006). Must be `≥ flickerIntervalMin`. | `LightingController` |
| `flickerEventDuration` | `Double` (seconds) | `0.15` | How long one dip event lasts, start to recovery (FR-006). | `LightingController` |
| `flickerDipFraction` | `Double` (0–1) | `0.45` | How far the lit radius/intensity dips during an event, as a fraction of the Flickering-state target (FR-006). `0` = no visible dip, `1` = dips to nothing. | `LightingController` |
| `readabilityFillIntensity` | `Double` (SceneKit directional light intensity units) | `220` | The single tunable controlling out-of-flashlight readability (FR-007). `0` MUST produce pure black outside the lit radius (User Story 3 Scenario 2). Implemented as a downward-pointing `.directional` light, not `.ambient` — see research.md §5 addendum; SceneKit's `.ambient` type did not illuminate this project's `.lambert` materials on-device. | `SceneNodeFactory` |
| `cameraTiltDegrees` | `Double` | `45` | Already existed as `TBD` in 001's contract; this spec supplies the default per GDD 13 (FR-012). | `CameraController` |
| `cameraOrthographicScale` | `Double` | `260` | Already existed as `TBD` in 001's contract; fixed zoom, no dynamic zoom (FR-012). Tuned in-simulator; re-tune on-device. | `CameraController` |
| `cameraDistance` | `Double` (world units) | `900` | How far back along the tilt axis the camera sits (needed for an orthographic rig; irrelevant to the visible framing but must not clip room geometry). Ported concept from the LILO spike's `cameraDistance`. | `WorldSceneController` |
| `shadowsEnabled` | `Bool` | `true` | Performance fallback switch (FR-022). Setting `false` disables `SCNLight.castsShadow` with no other code change. | `LightingController` |
| `shadowMapSize` | `Int` (pixels, square) | `512` | Shadow map resolution; lower value is the "reduced quality" half of FR-022's fallback. The flashlight is an omni light (cube shadow map, six faces) — `2048` blew the frame out to solid white on-device; `512` is the smallest tested value that renders shadows correctly. Raise only after re-testing on a physical device. | `SceneNodeFactory` |
| `shadowSampleCount` | `Int` | `8` | Shadow softness sample count; lower value trades softness for performance, same fallback as above. | `SceneNodeFactory` |
| `highlightColor` | color (RGB) | amber (1.0, 0.85, 0.3) | The one color used for both highlight intensity levels (FR-013). | `SceneNodeFactory` |
| `highlightOutOfRangeIntensity` | `Double` (0–1, emission intensity) | `0.18` | Faint level, visible in every light state including Compact Darkness (FR-013). Must be `> 0` (never invisible) and `< highlightInRangeIntensity`. | `HighlightController` |
| `highlightInRangeIntensity` | `Double` (0–1, emission intensity) | `0.6` | Clearly stronger level shown while an object is within interaction range (FR-013). | `HighlightController` |
| `wallHeight` | `Double` (world units) | `110` | Wall/desk solid height (FR-011). Trades shadow reach against how much of the room the walls hide, same tension the LILO spike documents for its own `wallHeightOptions`. | `SceneNodeFactory` |
| `wallThickness` | `Double` (world units) | `16` | Wall solid thickness (FR-011). Added during implementation — needed to build a solid wall box, not itemized separately in the original list. | `SceneNodeFactory` |
| `playerRadius` | `Double` (world units) | `20` | Player collision circle radius (FR-014). Added during implementation — `CollisionResolver` needs a body size to collide with. | `CollisionResolver` |
| `maxFrameDelta` | `Double` (seconds) | `1/20` | Clamp on one frame's delta time, applied to movement/light-easing/flicker/camera alike (FR-015). Ported directly from the LILO spike's own `1.0 / 20` clamp. | `WorldSceneController` |
| `batteryPickupAnimationDuration` | `Double` (seconds) | `0.22` | How long a picked-up battery's grow/fade-out animation takes (FR-016). | `WorldSceneController` |
| `doorOpenAnimationDuration` | `Double` (seconds) | `0.35` | How long the door's closed→open animation takes (FR-017). | `SceneNodeFactory` |
| `joystickControlZoneWidthFraction` | `Double` (0–1, fraction of screen width) | `0.5` | Width of the left-side zone a touch may start in to spawn the floating joystick (FR-019). Replaces 001's never-numerically-supplied fixed `joystickCenterOffset`. | `JoystickView` |
| `joystickControlZoneHeightFraction` | `Double` (0–1, fraction of screen height) | `1.0` | Height of the same zone. | `JoystickView` |

## Keys explicitly *not* added

- `joystickDiameter`, `joystickDeadZone`, `joystickOpacity`, `actionButtonSize`,
  `actionButtonCenterOffset`, `actionButtonTouchRadius` — already exist in 001's contract and are
  unaffected by this spec; the joystick becomes floating (a new *zone*, above) but its own base
  size/opacity/dead-zone are unchanged.
- A `characterRenderMode` switch — out of scope for 001-1 (see spec.md "Impact on Other Specs");
  015 owns that decision.

## Non-negotiable shape rules

- Same rules as 001's contract: one source file, `TBD` keys present with a placeholder value (not
  omitted) so every system is wired up before on-device tuning happens, later specs never create
  a second config source.
- The configured radius boundary MUST be internally consistent:
  `flashlightCriticalMinRadius == flashlightNormalRadius * compactDarknessRadiusFraction`.
