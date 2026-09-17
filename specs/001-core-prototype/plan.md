# Implementation Plan: LILO Phase 1 — Core Movement & Light System Prototype

**Branch**: `001-core-prototype` | **Date**: 2026-09-17 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-core-prototype/spec.md`

## Summary

Build a single fixed test room proving the core LILO loop end-to-end at a technical level:
joystick movement + sprint, a real-time-draining flashlight with four visible light states,
battery pickup/swap with slot capacity, a context-sensitive action button, and a door —
all rendered through the hybrid SpriteKit (2D environment) + SceneKit-via-SK3DNode (3D
character) pipeline that is this phase's central technical risk. All tunable numbers live in
one `GameConfig` source (constitution Principle I / spec FR-015), and gameplay state lives in
one `@Observable` `GameState` shared between the SpriteKit scene and the SwiftUI HUD overlay
(constitution Principle III).

## Technical Context

**Language/Version**: Swift (current project `SWIFT_VERSION = 5.0` toolchain default; no change needed for this feature)

**Primary Dependencies**: SpriteKit (2D environment/scene), SceneKit via `SK3DNode` (3D player character), SwiftUI (app shell + HUD overlay: joystick, action button, battery indicator), Core Haptics (pickup/critical-battery feedback per constitution). No third-party packages — none justified for this scope.

**Storage**: N/A — no persistence in this phase; all state is in-memory for the session.

**Testing**: XCTest, for the pure-logic pieces that don't require rendering (`GameConfig` loading, battery drain/threshold math, slot-capacity rules) — kept implementation-agnostic from SpriteKit/SceneKit so they're fast and reliable per constitution Principle IV.

**Target Platform**: iOS 26+, iPhone only, landscape orientation only. Performance validation (SC-002) requires a physical iPhone 17 — the team's standardized baseline device for this iOS 26 cycle — not the Simulator.

**Project Type**: Mobile app (single Xcode target `v2`; no separate backend/frontend split).

**Performance Goals**: Sustain ≥60 fps on iPhone 17 during continuous player movement (SC-002).

**Constraints**: Real-time 180s battery drain must hold regardless of frame rate fluctuation (drive off wall-clock time, not frame count); all tunables MUST live in exactly one `GameConfig` source (FR-015); the room's one battery MUST NOT respawn once picked up (FR-016); device orientation locked to landscape.

**Scale/Scope**: One fixed test room, one player character, one battery, one door. No monster, no additional rooms, no narrative content — explicitly out of scope per spec Assumptions.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|---|---|---|
| I. Spec-Driven Development | This plan derives from the approved, clarified `spec.md` (001-core-prototype); no implementation starts before this plan and its tasks exist. | PASS |
| II. Simplicity & YAGNI | Scope is held to exactly the spec's 4 user stories — no multi-room loader, no monster hooks, no inventory system are introduced ahead of need. | PASS |
| III. SwiftUI Architecture Consistency | State lives in one `@Observable GameState` (see Data Model) owned by the SwiftUI root view; the SpriteKit scene and HUD both read/write through it — no singletons or notification-based state introduced. | PASS |
| IV. Test-Before-Done | Drain timing, light-state thresholds, and battery slot-capacity rules are designed as pure logic (`BatteryController`, `GameConfig`) so they're covered by XCTest before the feature is marked done; a new test target is required (none exists yet — tracked as a setup task). | PASS (pending test target creation, tracked in tasks) |
| V. Versioning & Change Tracking | No persisted data model in this phase; N/A. | N/A |
| Tech constraint: single `GameConfig` source | All values in spec FR-015/FR-016 map onto the `GameConfig` contract below. | PASS |
| Tech constraint: no unjustified dependency | Only first-party frameworks (SpriteKit/SceneKit/SwiftUI/Core Haptics) used. | PASS |

No violations — Complexity Tracking table is empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-core-prototype/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md         # Phase 1 output
├── quickstart.md         # Phase 1 output
├── contracts/             # Phase 1 output
│   ├── game-config.md
│   └── action-button-states.md
└── tasks.md              # Phase 2 output (/speckit-tasks — not created here)
```

### Source Code (repository root)

```text
v2/                              # existing single Xcode app target
├── v2App.swift                  # existing entry point (unchanged)
├── ContentView.swift            # becomes the root game view (hosts SpriteView + HUD overlay)
├── Game/
│   ├── GameConfig.swift         # single tunables source — satisfies FR-015
│   ├── GameState.swift          # @Observable shared state — satisfies Principle III
│   ├── Scenes/
│   │   └── TestRoomScene.swift  # SKScene for the Phase 1 prototype room
│   ├── Entities/
│   │   ├── PlayerNode.swift     # SK3DNode wrapper for the placeholder player capsule
│   │   ├── BatteryNode.swift    # loose battery pickup, static/no-respawn per FR-016
│   │   └── DoorNode.swift
│   ├── Systems/
│   │   ├── MovementController.swift   # joystick input → walk/sprint (FR-001, FR-002, FR-003)
│   │   ├── BatteryController.swift    # drain timer, light-state thresholds, slot logic (FR-004–FR-009, FR-016)
│   │   ├── CameraController.swift     # smooth-follow + boundary clamping (FR-012)
│   │   ├── LightingController.swift   # 2D vignette (SKCropNode) + 3D SCNLight sync (FR-005, FR-006)
│   │   └── InteractionController.swift # nearest-interactable detection + tie-break (FR-007, FR-010, FR-011)
│   └── UI/
│       ├── JoystickView.swift         # SwiftUI virtual joystick (left side)
│       ├── ActionButtonView.swift     # context-sensitive button (right side, FR-007/009/010/011)
│       ├── BatteryIndicatorView.swift # persistent charge bar + spare-slot state (FR-017)
│       └── GameHUDView.swift          # composes joystick + action button + battery indicator
└── Assets.xcassets/              # existing, unchanged for this phase (placeholders only)

v2Tests/                          # NEW test target (none exists yet — setup task)
├── GameConfigTests.swift
├── BatteryControllerTests.swift
└── LightStateTests.swift
```

**Structure Decision**: Single Xcode app target (`v2`), organized by responsibility under a new
`Game/` group rather than by MVC layer — `Scenes` (SpriteKit), `Entities` (scene nodes),
`Systems` (pure/near-pure logic controllers), `UI` (SwiftUI HUD). `GameConfig` and `GameState`
sit at the top of `Game/` since every other piece depends on them. A new `v2Tests` target is
added so `Systems` logic can be unit-tested independent of rendering, per constitution
Principle IV.

## Complexity Tracking

*No violations — table intentionally empty.*
