# Implementation Plan: LILO Phase 1.1 — Unified 3D World & Feel Pass

**Branch**: `001-1-unified-3d-world` | **Date**: 2026-09-17 (engine migration pass) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-1-unified-3d-world/spec.md`, amending `/specs/001-core-prototype/spec.md`

## Summary

Add the feel/polish layer on top of 001's already-unified Unity URP 3D scene: real-time shadows
from the flashlight, eased light-radius transitions with discrete flicker events (instead of
001's direct radius jump), a config-driven readability fill so the dark stays navigable, halo
highlights around batteries and the door, sliding collision against walls/desk, a floating
joystick, and visible pickup/door animations. None of this requires a second renderer or a
render-architecture change — 001 already has one scene, one light, and Unity's real depth buffer
occluding everything correctly, so the FRs below extend `LightingRig`/`MovementController`/
`JoystickUI` rather than replacing a broken hybrid pipeline (see spec.md's engine migration
note).

`GameState` and the pure-logic Systems (`MovementController`, `BatteryController`,
`InteractionController`) are kept, since 001-1 changes feel and polish, not gameplay rules (spec
FR-023). `CameraController` gains tilt/scale/distance config already anticipated by 001's
contract; `LightingRig` gains easing, flicker, shadows and the readability fill. A new
`CollisionResolver` (plain C#) adds desk collision with sliding (FR-014 of this spec) — kept as
pure math rather than Unity physics, per constitution Principle II/IV (a `Rigidbody`/
`CharacterController` would work too, but a nine-line push-out function is simpler and stays
unit-testable without a running scene). `JoystickUI` is rewritten to float (FR-019).

## Technical Context

**Language/Version**: C# — unchanged from 001.

**Primary Dependencies**: Unity URP (real-time shadows, on top of 001's existing lighting), Unity
UI (HUD overlay, unchanged from 001). No new package — no physics package is added; collision
stays plain C# math per Principle II.

**Storage**: N/A — unchanged from 001.

**Testing**: Unity Test Framework EditMode tests, extended from 001's `Assets/Tests/EditMode`
assembly. 001's existing logic tests (`GameConfigTests`, `BatteryControllerTests`,
`LightStateTests`) MUST keep passing unmodified (spec SC-1.1-009) since they test
`GameState`/`GameConfig`/`BatteryController`, none of which change shape. New tests cover the
added pure-logic pieces: light-radius easing, flicker event timing, and desk collision/sliding
math — all kept engine-independent (no `MonoBehaviour`/scene dependency) so they don't need a
loaded scene, per constitution Principle IV.

**Target Platform**: iOS 26+, iPhone only, landscape orientation only — unchanged from 001.
Performance validation (SC-1.1-002) requires a physical iPhone 17, not the Unity Editor's Game
view.

**Project Type**: Mobile app (single Unity project, one iOS build target) — unchanged from 001.

**Performance Goals**: Sustain ≥60 fps on iPhone 17 with real-time shadows enabled and both
characters on screen (SC-1.1-002) — a materially harder bar than 001's, since shadow mapping is
more expensive than an unshadowed light. FR-022's shadow on/off and quality switches exist
specifically as the fallback if this isn't met.

**Constraints**: Every new tunable lives in the same single `GameConfig` asset as 001 (FR-021)
— extends, does not duplicate, `contracts/game-config.md` from 001. Movement, light easing,
flicker and camera follow are time-based (`Time.deltaTime`), not frame-count-based (FR-015),
continuing 001's wall-clock drain decision (research.md §3). A single long frame is clamped
(`GameConfig.maxFrameDelta`).

**Scale/Scope**: Same one fixed test room, one player character, two batteries, one door, one
placeholder monster figure as 001. No new gameplay content — this is a feel/polish pass on the
same scope.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|---|---|---|
| I. Spec-Driven Development | This plan derives from the approved, clarified `spec.md` (001-1-unified-3d-world); no implementation starts before this plan and its tasks exist. | PASS |
| II. Simplicity & YAGNI | Scope is held to what spec 001-1's FRs require — eased flashlight, one configured readability fill, real shadows, one highlight mechanism, floating joystick, desk sliding collision via plain math. No physics package, no lightmap baking, no multi-light rigs are introduced ahead of need. | PASS |
| III. Unity Architecture Consistency | `GameState` stays the single plain C# source of truth; `LightingRig`/`CollisionResolver`/`HighlightController` are added as plain-C#-backed systems with thin `MonoBehaviour` adapters, matching 001's split. HUD stays uGUI, reading `GameState` — no new state pattern introduced. | PASS |
| IV. Test-Before-Done | 001's existing logic tests are preserved unmodified (SC-1.1-009); new logic (radius easing, flicker timing, collision/sliding) is designed as pure C# so it stays EditMode-testable without a loaded scene. | PASS |
| V. Versioning & Change Tracking | No persisted data model; N/A. | N/A |
| Tech constraint: single `GameConfig` source | All new tunables (FR-021's list) are added to the existing `GameConfig` class/asset — no second config source. | PASS |
| Tech constraint: no unjustified dependency | URP's built-in real-time shadows are already part of the render pipeline in use; no new package added, and Unity physics is deliberately *not* added (scope reduction, not an addition). | PASS |

No violations — Complexity Tracking table is empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-1-unified-3d-world/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md         # Phase 1 output
├── quickstart.md         # Phase 1 output
├── contracts/             # Phase 1 output
│   └── config-additions.md   # new GameConfig keys, appended to 001's contract too
└── tasks.md              # Phase 2 output (/speckit-tasks — not created here)
```

### Source Code (repository root)

```text
Assets/
├── Config/
│   └── GameConfig.asset                # CHANGED: new fields appended (light profile, camera, highlight, joystick zone, animation durations, shadow settings) — same single asset
├── Scripts/
│   ├── Config/
│   │   └── GameConfig.cs               # CHANGED: new fields appended, same single-source class
│   ├── State/
│   │   └── GameState.cs                # UNCHANGED — same shape
│   ├── Systems/
│   │   ├── MovementController.cs       # CHANGED: same joystick→velocity math (FR-001–003 unchanged), now resolved against CollisionResolver instead of a plain rect clamp
│   │   ├── CollisionResolver.cs        # NEW — room-bounds clamp (ported from MovementController) + desk push-out/slide, satisfies FR-014
│   │   ├── BatteryController.cs        # UNCHANGED — pure GameState math, no engine dependency
│   │   ├── InteractionController.cs    # UNCHANGED — nearest-interactable logic itself doesn't change; occlusion was never its concern (Unity depth handles it)
│   │   ├── CameraController.cs         # CHANGED: reads the now-supplied tilt/scale/distance config values (were placeholders in 001's contract)
│   │   ├── LightRadiusEasing.cs        # NEW — pure easing/flicker-event state machine, satisfies FR-005, FR-006
│   │   └── HighlightController.cs      # NEW — two-level (out-of-range/in-range) highlight intensity per interactable, satisfies FR-013
│   ├── MonoBehaviours/
│   │   ├── LightingRig.cs              # REWRITTEN — drives one URP `Light` (Point) with eased radius/flicker-event state instead of a direct per-state jump; configures shadows and adds the readability fill light
│   │   ├── CameraRig.cs                # CHANGED — applies the now-tuned tilt/scale/distance
│   │   ├── PlayerRig.cs                # UNCHANGED shape; movement now flows through CollisionResolver
│   │   └── HighlightView.cs            # NEW — per-interactable unlit outline/halo GameObject, intensity driven by HighlightController
│   └── UI/
│       ├── JoystickUI.cs               # REWRITTEN — floating: hidden until touch-down inside a left control zone, then centered on that touch (FR-019)
│       ├── ActionButtonUI.cs           # UNCHANGED
│       ├── BatteryIndicatorUI.cs       # UNCHANGED
│       └── GameHUD.cs                  # UNCHANGED

Assets/Tests/EditMode/
├── GameConfigTests.cs               # UNCHANGED (SC-1.1-009)
├── BatteryControllerTests.cs        # UNCHANGED (SC-1.1-009)
├── LightStateTests.cs               # UNCHANGED (SC-1.1-009)
├── LightRadiusEasingTests.cs        # NEW — FR-005, FR-006
├── FlickerEventTests.cs             # NEW — FR-006
└── CollisionResolverTests.cs        # NEW — FR-014
```

**Structure Decision**: Keeps 001's `Assets/Scripts/{Config,State,Systems,MonoBehaviours,UI}`
layout (constitution Principle III precedent) — this spec adds files to existing folders rather
than introducing a new top-level structure, since it is additive polish on an already-correct
scene, not a renderer migration.

## Complexity Tracking

*No violations — table intentionally empty.*
