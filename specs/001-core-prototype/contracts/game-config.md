# Contract: GameConfig

The single tunables source required by spec FR-015. Every value a system (`MovementController`,
`BatteryController`, `CameraController`, HUD controls) reads MUST come from here — none hardcoded
at the call site. Values below are sourced from GDD Ch. 13, 15.3, 17.1/17.2 and this feature's
Clarifications; a `TBD` value means the GDD explicitly defers it to on-device tuning during this
same phase (spec Assumptions) — the *key* must still exist and be read from `GameConfig`, only the
*number* is pending.

| Key | Type | Default | Source / Notes |
|---|---|---|---|
| `walkSpeed` | `Double` | `1.0` | base unit, GDD 17.1 |
| `sprintMultiplier` | `Double` | `1.6` | × walkSpeed, GDD 17.1 |
| `sprintJoystickThreshold` | `Double` (0–1 deflection) | `TBD` | tuned on-device, spec Assumptions |
| `interactionRadius` | `Double` (world units) | `TBD` | tuned on-device, spec Assumptions |
| `batteryDuration` | `Double` (seconds) | `180` | GDD 17.2, spec FR-004 |
| `lightStateFlickerStart` | `Double` (fraction of duration) | `0.30` | GDD 17.2, spec FR-005 |
| `lightStateCriticalStart` | `Double` (fraction of duration) | `0.10` | GDD 17.2, spec FR-005; also the install-battery gate (FR-009) |
| `compactDarknessRadiusFraction` | `Double` | `0.10` | ~10% of normal radius, GDD 17.2 / spec FR-006 |
| `cameraFollowLerpFactor` | `Double` (0–1 per frame-normalized step) | `TBD` | needed for FR-012's "smooth, non-instant" requirement; not numerically specified by GDD, tune on-device |
| `roomBoundsInset` | `Double` | `TBD` | camera clamp inset from room edge, FR-012 |
| `cameraTiltDegrees` | `Double` | `45` | GDD 13, FR-012 *(added in alignment pass)* |
| `cameraOrthographicScale` | `Double` | `TBD` | fixed zoom, GDD 13, FR-012 *(added)* |
| `joystickDiameter` | `Double` (points) | `TBD` | GDD 15.3 *(added)* |
| `joystickDeadZone` | `Double` (0–1 deflection) | `TBD` | GDD 15.3; must be well below `sprintJoystickThreshold` *(added)* |
| `joystickOpacity` | `Double` (0–1) | `TBD` | GDD 15.3 *(added)* |
| `joystickCenterOffset` | point offset from bottom-left safe area | `TBD` | GDD 15.3 *(added)* |
| `actionButtonSize` | `Double` (points) | `TBD` | GDD 15.3 *(added)* |
| `actionButtonCenterOffset` | point offset from bottom-right safe area | `TBD` | GDD 15.3 *(added)* |
| `actionButtonTouchRadius` | `Double` (points) | `TBD` | GDD 15.3; may exceed the drawn size *(added)* |

## Extensions

- **001-1** (Unified 3D World & Feel Pass) adds light-profile easing/flicker keys, camera tilt/
  scale/distance, shadow settings, interactable highlight color/intensity, wall height, the frame
  delta clamp, pickup/door animation durations, and the floating-joystick control zone — see
  [`001-1-unified-3d-world/contracts/config-additions.md`](../../001-1-unified-3d-world/contracts/config-additions.md)
  for the full table. Same single `GameConfig.swift` source; no second config file was created.

## Non-negotiable shape rules

- One source file (`Game/GameConfig.swift`), no duplicate constants elsewhere (FR-015).
- `TBD` keys MUST still be present with a placeholder value (not omitted) so every system can be
  wired up against the final shape before on-device tuning happens — only the *value* changes
  later, not which system reads from where.
- Later specs (002+) add their own GDD Ch. 17 keys to this same source; they never create a second
  config source.
