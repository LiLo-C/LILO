# Contract: GameConfig

The single tunables source required by spec FR-015: one `GameConfig` `ScriptableObject` class
with one asset instance (`Assets/Config/GameConfig.asset`). Every system (`MovementController`,
`BatteryController`, `CameraController`, HUD controls) reads its numbers from that asset — none
hardcoded at the call site. Values below are sourced from GDD Ch. 13, 15.3, 17.1/17.2, this
feature's Clarifications, and — where marked *(carried over)* — from on-device tuning already
done for the equivalent build under the project's previous engine; those numbers are a known-good
starting point, not a guarantee, and should be re-confirmed once this feature runs on-device in
Unity, since collision size/joystick feel can shift slightly with the new renderer's touch
input handling. A `TBD` value means no prior tuning exists and it must be set during this
phase's on-device pass (spec Assumptions) — the *key* must still exist and be read from
`GameConfig`, only the *number* is pending.

| Key | Type | Default | Source / Notes |
|---|---|---|---|
| `walkSpeed` | `float` | `1.0` | base unit, GDD 17.1 |
| `sprintMultiplier` | `float` | `1.6` | × walkSpeed, GDD 17.1 |
| `sprintJoystickThreshold` | `float` (0–1 deflection) | `0.9` | *(carried over)* re-confirm on-device, spec Assumptions |
| `interactionRadius` | `float` (world units) | `60.0` | *(carried over)* re-confirm on-device, spec Assumptions |
| `batteryDuration` | `float` (seconds) | `180` | GDD 17.2, spec FR-004 |
| `lightStateFlickerStart` | `float` (fraction of duration) | `0.30` | GDD 17.2, spec FR-005 |
| `lightStateCriticalStart` | `float` (fraction of duration) | `0.10` | GDD 17.2, spec FR-005; also the install-battery gate (FR-009) |
| `compactDarknessRadiusFraction` | `float` | `0.10` | ~10% of normal radius, GDD 17.2 / spec FR-006 |
| `cameraFollowLerpFactor` | `float` (0–1 per frame-normalized step) | `0.12` | *(carried over)* FR-012's "smooth, non-instant" requirement; re-confirm on-device |
| `roomBoundsInset` | `float` | `80.0` | *(carried over)* camera clamp inset from room edge, FR-012 |
| `cameraTiltDegrees` | `float` | `45` | GDD 13, FR-012 *(added in alignment pass)* |
| `cameraOrthographicScale` | `float` | `260` | *(carried over)* fixed zoom, GDD 13, FR-012 *(added)* |
| `joystickDiameter` | `float` (px) | `TBD` | GDD 15.3 *(added)* — Unity UI touch-target sizing differs enough from the prior native build that this needs a fresh on-device pass |
| `joystickDeadZone` | `float` (0–1 deflection) | `TBD` | GDD 15.3; must be well below `sprintJoystickThreshold` *(added)* |
| `joystickOpacity` | `float` (0–1) | `TBD` | GDD 15.3 *(added)* |
| `joystickCenterOffset` | `Vector2` offset from bottom-left safe area | `TBD` | GDD 15.3 *(added)* |
| `actionButtonSize` | `float` (px) | `TBD` | GDD 15.3 *(added)* |
| `actionButtonCenterOffset` | `Vector2` offset from bottom-right safe area | `TBD` | GDD 15.3 *(added)* |
| `actionButtonTouchRadius` | `float` (px) | `TBD` | GDD 15.3; may exceed the drawn size *(added)* |

## Extensions

- **001-1** (Unified 3D World & Feel Pass) adds light-profile easing/flicker keys, camera tilt/
  scale/distance, shadow settings, interactable highlight color/intensity, wall height, the frame
  delta clamp, pickup/door animation durations, and the floating-joystick control zone — see
  [`001-1-unified-3d-world/contracts/config-additions.md`](../../001-1-unified-3d-world/contracts/config-additions.md)
  for the full table. Same single `GameConfig.asset` source; no second config asset was created.

## Non-negotiable shape rules

- One source asset (`Assets/Config/GameConfig.asset`, backed by the `GameConfig`
  `ScriptableObject` class), no duplicate constants elsewhere (FR-015).
- `TBD` keys MUST still be present with a placeholder value (not omitted) so every system can be
  wired up against the final shape before on-device tuning happens — only the *value* changes
  later, not which system reads from where.
- Later specs (002+) add their own GDD Ch. 17 keys to this same source; they never create a second
  config source.
