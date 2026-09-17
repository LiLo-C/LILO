# Implementation Plan: LILO Phase 1.1 — Unified 3D World & Feel Pass

**Branch**: `001-1-unified-3d-world` (repo branch: `feat/world-building-unify`) | **Date**: 2026-09-17 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-1-unified-3d-world/spec.md`, amending `/specs/001-core-prototype/spec.md`

## Summary

Replace the hybrid SpriteKit(2D)+SK3DNode(3D) renderer from 001 with one real-time SceneKit
scene, following the approach proven in the `~/Documents/Apple/LILO` spike (`Spike3D.swift`).
Everything playable — floor, walls, desk, batteries, door, player character, placeholder
monster — becomes a solid 3D node in one `SCNScene`, lit by a single spot light attached to the
player. This lets the flashlight actually light the room and cast real shadows, which the
hybrid renderer structurally could not do (a 3D `SCNLight` cannot light a 2D `SKSpriteNode`).

`GameState` and the pure-logic Systems (`MovementController`, `BatteryController`,
`InteractionController`) are kept, since 001-1 changes rendering and feel, not gameplay rules
(spec FR-023). `CameraController` and `LightingController` are substantially rewritten because
their outputs now drive one SceneKit camera/light instead of syncing two renderers.
`InteractionController`'s occlusion concern (001 FR-014) disappears — real depth in one scene
occludes correctly with no code. A new `CollisionResolver` adds desk collision with sliding
(FR-014 of this spec). The SwiftUI HUD overlay (joystick, action button, battery bar) is
unaffected as a renderer choice but `JoystickView` is rewritten to float (FR-019).

## Technical Context

**Language/Version**: Swift (current project `SWIFT_VERSION = 5.0` toolchain default; unchanged)

**Primary Dependencies**: SceneKit (the entire playable world — floor, walls, desk, batteries,
door, player character, placeholder monster, camera, lighting, shadows), SwiftUI (app shell +
HUD overlay, unchanged from 001), Core Haptics (unchanged, still unused this phase). SpriteKit
is dropped from the playable-world path; no other new dependency.

**Storage**: N/A — unchanged from 001.

**Testing**: XCTest, extended from 001's `v2Tests` target. 001's existing logic tests
(`GameConfigTests`, `BatteryControllerTests`, `LightStateTests`) MUST keep passing unmodified
(spec SC-1.1-009) since they test `GameState`/`GameConfig`/`BatteryController`, none of which change
shape. New tests cover the added pure-logic pieces: light-radius easing, flicker event timing,
and desk collision/sliding math — all kept engine-independent so they don't need a rendering
host, per constitution Principle IV.

**Target Platform**: iOS 26+, iPhone only, landscape orientation only — unchanged from 001.
Performance validation (SC-1.1-002) requires a physical iPhone 17, not the Simulator.

**Project Type**: Mobile app (single Xcode target `v2`) — unchanged from 001.

**Performance Goals**: Sustain ≥60 fps on iPhone 17 with shadows enabled and both 3D characters
on screen (SC-1.1-002) — a materially harder bar than 001's, since real shadow mapping is more
expensive than 001's flat compositing. FR-022's shadow on/off and quality switches exist
specifically as the fallback if this isn't met.

**Constraints**: Every new tunable lives in the same single `GameConfig` source as 001 (FR-021)
— extends, does not duplicate, `contracts/game-config.md` from 001. Movement, light easing,
flicker and camera follow are time-based, not frame-count-based (FR-015), continuing 001's
wall-clock drain decision (research.md §3). A single long frame is clamped (`GameConfig.
maxFrameDelta`, ported from the LILO spike's `1/20` clamp — see research.md §7).

**Scale/Scope**: Same one fixed test room, one player character, two batteries, one door, one
placeholder monster figure as 001. No new gameplay content — this is a rendering and feel
rewrite of the same scope.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|---|---|---|
| I. Spec-Driven Development | This plan derives from the approved, clarified `spec.md` (001-1-unified-3d-world); no implementation starts before this plan and its tasks exist. | PASS |
| II. Simplicity & YAGNI | Scope is held to what spec 001-1's FRs require — one flashlight plus one configured readability fill, real shadows, one highlight mechanism, floating joystick, desk sliding collision. No multi-light rigs, no lightmap baking, no physics engine are introduced ahead of need (plain 2D-plane math reused from the LILO spike, not SceneKit physics). | PASS |
| III. SwiftUI Architecture Consistency | `GameState` stays the single `@Observable` source of truth; the new `WorldSceneController` (SceneKit equivalent of `TestRoomScene`) reads/writes it the same way `TestRoomScene` did. HUD stays SwiftUI, reading `GameState` — no new state pattern introduced. | PASS |
| IV. Test-Before-Done | 001's existing logic tests are preserved unmodified (SC-1.1-009); new logic (radius easing, flicker timing, collision/sliding) is designed as pure math so it stays unit-testable without a rendering host. | PASS |
| V. Versioning & Change Tracking | No persisted data model; N/A. | N/A |
| Tech constraint: single `GameConfig` source | All new tunables (FR-021's list) are added to the existing `GameConfig.swift` / its contract doc — no second config source. | PASS |
| Tech constraint: no unjustified dependency | SceneKit is already a used, first-party framework (001 already depends on it for `SK3DNode`'s underlying `SCNScene`); no new dependency, one is actually dropped in scope (2D compositing complexity). | PASS |

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
v2/
├── ContentView.swift               # CHANGED: hosts an SCNView-backed SwiftUI view instead of SpriteView; HUD overlay unchanged
├── Game/
│   ├── GameConfig.swift            # CHANGED: new keys appended (light profile, camera, highlight, joystick zone, animation durations, shadow settings) — same single source
│   ├── GameState.swift             # UNCHANGED — same shape, same @Observable root
│   ├── Scenes/
│   │   ├── TestRoomScene.swift     # REMOVED — SpriteKit scene no longer exists
│   │   └── WorldSceneController.swift  # NEW — SceneKit equivalent: owns SCNScene, builds room/props once, drives per-frame update (was TestRoomScene's job)
│   ├── Views/
│   │   └── SceneContainerView.swift    # NEW — thin SwiftUI wrapper around SCNView (UIViewRepresentable), routes touches to the HUD's joystick/action button same as before
│   ├── Entities/
│   │   ├── PlayerNode.swift        # REMOVED — was an SK3DNode wrapper; player is now a plain SCNNode built by WorldSceneController
│   │   ├── BatteryNode.swift       # REPLACED by SceneNodeFactory.makeBattery() — SCNNode (box) instead of SKShapeNode
│   │   ├── DoorNode.swift          # REPLACED by SceneNodeFactory.makeDoor() — SCNNode (box) instead of SKShapeNode
│   │   └── SceneNodeFactory.swift  # NEW — builds the placeholder SCNNode geometry for floor/walls/desk/battery/door/player/monster, one place per spec's "placeholder materials" baseline (FR-020)
│   ├── Systems/
│   │   ├── MovementController.swift    # CHANGED: same joystick→velocity math (FR-001–003 unchanged), now resolved against CollisionResolver instead of a plain rect clamp
│   │   ├── CollisionResolver.swift     # NEW — room-bounds clamp (ported from MovementController) + desk push-out/slide (ported/simplified from the LILO spike's GridCollider), satisfies FR-014
│   │   ├── BatteryController.swift     # UNCHANGED — pure GameState math, no renderer dependency
│   │   ├── InteractionController.swift # CHANGED: drops the (already-planned, never-needed) manual occlusion concern; nearest-interactable logic itself is UNCHANGED
│   │   ├── CameraController.swift      # REWRITTEN — outputs one SceneKit camera transform (locked to player, tilt, fixed orthographic scale) instead of syncing two renderers' cameras
│   │   ├── LightingController.swift    # REWRITTEN — drives one SCNLight (spot, shadow-casting) with eased radius/flicker-event state instead of a 2D vignette mask + a second unconnected SCNLight
│   │   └── HighlightController.swift   # NEW — two-level (out-of-range/in-range) unlit highlight per interactable, satisfies FR-013
│   └── UI/
│       ├── JoystickView.swift          # REWRITTEN — floating: hidden until touch-down inside a left control zone, then centered on that touch (FR-019)
│       ├── ActionButtonView.swift      # UNCHANGED
│       ├── BatteryIndicatorView.swift  # UNCHANGED
│       └── GameHUDView.swift           # UNCHANGED
└── Assets.xcassets/                 # unchanged

v2Tests/
├── GameConfigTests.swift            # UNCHANGED (SC-1.1-009)
├── BatteryControllerTests.swift     # UNCHANGED (SC-1.1-009)
├── LightStateTests.swift            # UNCHANGED (SC-1.1-009)
├── LightRadiusEasingTests.swift     # NEW — FR-005, FR-006
├── FlickerEventTests.swift          # NEW — FR-006
└── CollisionResolverTests.swift     # NEW — FR-014
```

**Structure Decision**: Keeps 001's `Game/{GameConfig,GameState}` + `Scenes/Systems/UI` layout
(constitution Principle III precedent). `Entities/` becomes a single `SceneNodeFactory` rather
than one class per node type, since every placeholder is now a primitive `SCNNode` built the
same way (geometry + material), not a bespoke `SKShapeNode`/`SK3DNode` subclass each — this is
the Simplicity/YAGNI-driven collapse the renderer change makes possible, not scope creep. A new
`Views/SceneContainerView.swift` is the only new SwiftUI-facing file, mirroring the narrow job
`ContentView.swift` + `SpriteView` did in 001.

## Complexity Tracking

*No violations — table intentionally empty.*
