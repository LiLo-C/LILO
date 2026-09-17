# Quickstart: Validating LILO Phase 1.1 — Unified 3D World & Feel Pass

Manual validation guide against this spec's Success Criteria and Definition of Done. Not a test
suite — automated coverage for the pure-logic pieces (radius easing, flicker timing, desk
collision) is tracked separately (research.md §11). Run this *after* 001's own quickstart
scenarios still pass unmodified (001 SC-001–SC-009) — this guide only adds what changed.

## Prerequisites

- Same as [001's quickstart](../001-core-prototype/quickstart.md): Xcode with iOS 26 SDK, a
  physical iPhone 17 for fps checks (SC-1.1-002), the `v2` scheme.
- Before starting: confirm 001's existing `v2Tests` suite (`GameConfigTests`,
  `BatteryControllerTests`, `LightStateTests`) still builds and passes green — this is SC-1.1-009's
  regression gate and should be checked before spending time on manual visual checks.

## Setup

1. Open `v2.xcodeproj`, select the `v2` scheme, choose the connected iPhone as the run
   destination.
2. Build & run. The app launches directly into the test room, now rendered as one SceneKit
   scene instead of a SpriteKit scene with a composited 3D character.

## Validation scenarios

### 1. The flashlight lights the room (User Story 1, SC-1.1-001)

- Stand still at spawn with a full charge → floor, nearby walls, the desk, and the player
  character are all visibly lit by the same light; darkness is visible past the lit area on
  every screen edge (SC-1.1-001 — take a screenshot here for the record).
- Walk past the desk with the light behind you → a shadow falls on the far side of the desk and
  swings as you move around it.
- Walk toward the placeholder monster figure from outside the lit area → it lights up only once
  the flashlight reaches it, matching the floor around it.

### 2. Light states read as one light (User Story 2, SC-1.1-004)

- Set a short `batteryDuration` in `GameConfig` for faster iteration, then let charge run down
  uninterrupted from 100%.
- **Normal**: steady, no flicker, no size change.
- **Flickering**: watch for distinct dip-and-recover events with steady light between them — not
  continuous shimmer. Ask 2 people outside the dev team what they see; both should say
  "flickering" or "failing", not "glitching"/"noise" (SC-1.1-004).
- **Critical**: confirm the lit area visibly shrinks as charge falls, with no single jump at
  entry to Critical.
- **Compact Darkness**: confirm the lit area settles smoothly at its small fixed size, and you
  can still move and act (001 FR-006, unchanged).
- Install a spare battery while in Compact Darkness → the light grows back to Normal over a
  short, visible transition, not a pop.

### 3. The room stays navigable in the dark (User Story 3, SC-1.1-006)

- Set charge to 0% via a debug hook or by waiting it out. Hand the device to someone outside the
  dev team with no instructions → they reach the door within 60 seconds (SC-1.1-006).
- While there, confirm walls/the desk are faintly visible as shapes, never bright, and that a
  battery or the door — even out of the lit area — shows a faint highlight marking where it is.
- Set `readabilityFillIntensity` to `0` and relaunch → confirm everything outside the lit area is
  now pure black (proves the single-tunable requirement).

### 4. Movement and controls feel physical (User Story 4)

- Walk diagonally into a wall, then into the desk, at several angles → the character slides
  along the surface every time rather than stopping dead or clipping through (SC-1.1-007 — repeat at
  least 10 times across both surfaces and count any failures).
- Touch anywhere inside the configured left control zone with no joystick visible → it appears
  centered on that touch point; lift the finger → it disappears and the character stops. Try a
  touch starting outside the left zone → no joystick appears, character doesn't move.
- Pick up a battery → it visibly grows/fades out over a short duration, not an instant vanish.
- Open the door → it visibly animates from closed to open.

### 5. Side-by-side feel check (SC-1.1-003, SC-1.1-005)

- Build 001's commit (`c982b79`) and this feature's build on two devices, or record short clips
  of each. Show both, unlabeled, to at least 3 people outside the dev team and ask which "looks
  and feels more like a horror game" → at least 2 of 3 pick the 001-1 build (SC-1.1-003).
- During the Compact Darkness scenario above, ask observers if anything on screen looks brighter
  than the floor around it → none should say yes (SC-1.1-005).

### 6. Performance with shadows (SC-1.1-002)

- On the physical iPhone 17, move continuously for at least 30 seconds with shadows enabled and
  the placeholder monster figure on screen, watching Xcode's FPS gauge or Instruments →
  sustained ≥60 fps. If this fails, reduce `shadowMapSize`/`shadowSampleCount` while keeping
  shadows enabled and re-measure. Setting `GameConfig.shadowsEnabled = false` is a separate
  FR-022 playability fallback and does not satisfy SC-1.1-002's shadows-on measurement.

### 7. Config-only tuning (SC-1.1-008)

- Change `flashlightNormalRadius`, `lightRadiusEaseRate`, `wallHeight`, and
  `joystickControlZoneWidthFraction` one at a time in `GameConfig.swift`, rebuilding after each
  → each visibly changes behavior with no other file touched (SC-1.1-008).

### 8. Regression (SC-1.1-009)

- Run the full `v2Tests` target → 001's pre-existing tests plus this feature's new tests
  (`LightRadiusEasingTests`, `FlickerEventTests`, `CollisionResolverTests`) all pass.
- Re-run 001's own quickstart scenarios 3 (battery pickup/swap) and 4 (door) verbatim → still
  pass exactly as before; only the *look* of the room and the *floating* joystick behaviour
  should differ, not the gameplay rules.

## Expected end state

Every scenario above passes on a physical iPhone 17 running iOS 26, `v2Tests` is fully green
(001's tests unmodified, 001-1's new tests passing), and 001's own quickstart still passes
end-to-end. Any scenario that fails should be filed as a gap against the relevant FR/SC before
`/speckit-tasks` work in that area is marked done.
