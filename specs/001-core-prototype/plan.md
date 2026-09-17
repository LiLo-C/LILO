# Implementation Plan: LILO Phase 1 — Core Movement & Light System Prototype

**Branch**: `001-core-prototype` | **Date**: 2026-09-17 (engine migration pass) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-core-prototype/spec.md`

## Summary

Build a single fixed test room proving the core LILO loop end-to-end at a technical level:
joystick movement + sprint, a real-time-draining flashlight with four visible light states,
battery pickup/swap with slot capacity, a context-sensitive action button, and a door — all
rendered as one Unity URP 3D scene (floor, walls, desk, batteries, door, player character, and
placeholder monster all sharing one light and one depth buffer). There is no hybrid 2D/3D
renderer to validate — that was a risk specific to the project's earlier native-iOS engine choice
and does not exist in Unity. All tunable numbers live in one `GameConfig` `ScriptableObject`
asset (constitution Principle III / spec FR-015), and gameplay state lives in one plain C#
`GameState` object owned by a single `GameManager` `MonoBehaviour`, read by the scene's systems
and by the HUD `Canvas`.

## Technical Context

**Language/Version**: C# (Unity 6000.6.1f1's default .NET/IL2CPP toolchain; no change needed for
this feature).

**Primary Dependencies**: Unity URP (the entire playable world — floor, walls, desk, batteries,
door, player character, placeholder monster, camera, lighting), Unity Input System
(`com.unity.inputsystem`, joystick + action button touch input), Unity UI (`com.unity.ugui`, HUD
overlay: joystick, action button, battery indicator). No third-party package — none justified for
this scope (constitution Principle II).

**Storage**: N/A — no persistence in this phase; all state is in-memory for the session.

**Testing**: Unity Test Framework, EditMode (NUnit), for the pure-logic pieces that don't require
a running scene (`GameConfig` shape, battery drain/threshold math, slot-capacity rules) — kept
implementation-agnostic from `MonoBehaviour`/scene-graph types so they're fast and reliable per
constitution Principle IV.

**Target Platform**: iOS 26+, iPhone only, landscape orientation only. Performance validation
(SC-002) requires a physical iPhone 17 — the team's standardized baseline device for this iOS 26
cycle — not the Unity Editor's Game view.

**Project Type**: Mobile app (single Unity project, one iOS build target; no separate
backend/frontend split).

**Performance Goals**: Sustain ≥60 fps on iPhone 17 during continuous player movement (SC-002).

**Constraints**: Real-time 180s battery drain must hold regardless of frame rate fluctuation
(drive off `Time.deltaTime`/unscaled wall-clock time, not a fixed per-frame decrement); all
tunables MUST live in exactly one `GameConfig` asset (FR-015); neither of the room's two
batteries MUST respawn once picked up (FR-016); device orientation locked to landscape.

**Scale/Scope**: One fixed test room, one player character, two batteries, one door, one
placeholder monster figure. No real monster AI, no additional rooms, no narrative content —
explicitly out of scope per spec Assumptions.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Status |
|---|---|---|
| I. Spec-Driven Development | This plan derives from the approved, clarified `spec.md` (001-core-prototype); no implementation starts before this plan and its tasks exist. | PASS |
| II. Simplicity & YAGNI | Scope is held to exactly the spec's 4 user stories — no multi-room loader, no monster hooks, no inventory system, no third-party joystick asset are introduced ahead of need. | PASS |
| III. Unity Architecture Consistency | State lives in one plain C# `GameState` object (see Data Model) owned by a single `GameManager` `MonoBehaviour`; the scene's systems and the HUD `Canvas` both read/write through it — no singletons, no static mutable gameplay state, no second config source. | PASS |
| IV. Test-Before-Done | Drain timing, light-state thresholds, and battery slot-capacity rules are designed as pure C# classes (`BatteryController`, `GameConfig`) with no `MonoBehaviour`/scene dependency, so they're covered by Unity Test Framework EditMode tests before the feature is marked done; a new EditMode test assembly is required (none exists yet — tracked as a setup task). | PASS (pending test assembly creation, tracked in tasks) |
| V. Versioning & Change Tracking | No persisted data model in this phase; N/A. | N/A |
| Tech constraint: single `GameConfig` source | All values in spec FR-015/FR-016 map onto the `GameConfig` contract below. | PASS |
| Tech constraint: no unjustified dependency | Only Unity packages already in `Packages/manifest.json` (URP, Input System, UGUI) are used. | PASS |

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
Assets/
├── Scenes/
│   └── TestRoom.unity            # NEW — the Phase 1 prototype room, one URP 3D scene
├── Config/
│   └── GameConfig.asset          # single tunables source — satisfies FR-015
├── Scripts/
│   ├── Config/
│   │   └── GameConfig.cs         # ScriptableObject definition read by GameConfig.asset
│   ├── State/
│   │   └── GameState.cs          # plain C# root aggregate — satisfies Principle III
│   ├── Systems/                  # plain C# classes, no MonoBehaviour/scene dependency
│   │   ├── MovementController.cs     # joystick input → walk/sprint (FR-001, FR-002, FR-003)
│   │   ├── BatteryController.cs      # drain timer, light-state thresholds, slot logic (FR-004–FR-009, FR-016)
│   │   ├── CameraController.cs       # smooth-follow + boundary clamping (FR-012)
│   │   └── InteractionController.cs  # nearest-interactable detection + tie-break (FR-007, FR-010, FR-011)
│   ├── MonoBehaviours/            # thin Unity-lifecycle adapters over the Systems above
│   │   ├── GameManager.cs             # owns GameState, drives every System's per-frame tick
│   │   ├── PlayerRig.cs               # positions the player GameObject/light from GameState.player
│   │   ├── BatteryPickup.cs           # per-battery component, registers with InteractionController
│   │   ├── DoorController.cs          # door GameObject behavior
│   │   └── CameraRig.cs               # applies CameraController's output to the scene Camera
│   └── UI/
│       ├── JoystickUI.cs              # on-screen virtual joystick (left side), Input System driven
│       ├── ActionButtonUI.cs          # context-sensitive button (right side, FR-007/009/010/011)
│       ├── BatteryIndicatorUI.cs      # persistent charge bar + spare-slot state (FR-017)
│       └── GameHUD.cs                 # composes joystick + action button + battery indicator on one Canvas
└── Prefabs/                      # placeholder primitives (capsule player, box batteries/door/desk)

Assets/Tests/EditMode/             # NEW EditMode test assembly (none exists yet — setup task)
├── GameConfigTests.cs
├── BatteryControllerTests.cs
└── LightStateTests.cs
```

**Structure Decision**: One Unity project, organized by responsibility under `Assets/Scripts/` —
`Systems` (plain C#, unit-testable), `MonoBehaviours` (thin Unity-lifecycle adapters), `UI`
(uGUI HUD). `GameConfig` and `GameState` sit at the top since every other piece depends on them.
A new `Assets/Tests/EditMode` assembly definition is added so `Systems` logic can be unit-tested
independent of a running scene, per constitution Principle IV. This mirrors the responsibility
split the project used under its earlier native-iOS engine (`Scenes`/`Entities`/`Systems`/`UI`)
— only the concrete types change (`SKScene`→Unity Scene, `SK3DNode`→GameObject,
`@Observable`→plain C# class, `XCTest`→Unity Test Framework).

## Complexity Tracking

*No violations — table intentionally empty.*
