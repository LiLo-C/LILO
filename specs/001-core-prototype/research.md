# Phase 0 Research: LILO Phase 1 — Core Movement & Light System Prototype

No `[NEEDS CLARIFICATION]` markers remain in the Technical Context — the `/speckit-clarify`
session on this feature already resolved the highest-impact unknowns (target device, iOS
version, battery respawn behavior). This document records the technical decisions needed to move
from spec to design, each with rationale and the alternative considered.

*Engine migration note (2026-09-17): this feature was previously planned against a native
iOS/SpriteKit+SceneKit stack. That plan is superseded by this one. Every decision below is
re-derived for Unity; none of the original SpriteKit/SceneKit/SwiftUI decisions carry over as
written, though several rationales (single config source, time-based drain, no third-party
control library) are unchanged in substance.*

## 1. One Unity URP 3D scene (no hybrid renderer to validate)

- **Decision**: The playable world — floor, walls, desk, batteries, door, player character,
  placeholder monster — is one `Scene` with every object as a `GameObject` (primitive mesh:
  cube for floor/walls/desk/batteries/door, capsule for characters), lit by URP's real-time
  lighting.
- **Rationale**: The project's earlier native-iOS engine required validating a hybrid 2D
  (SpriteKit) + 3D (SceneKit via `SK3DNode`) compositing risk in this exact phase (GDD Ch. 11,
  pre-migration). Unity has no such split — a `Light` component lights every `GameObject` in its
  scene by construction, and Unity's depth buffer occludes correctly with no manual layering.
  There is nothing left to "validate" about renderer compositing; SC-002's 60fps target instead
  validates ordinary URP performance on device with real-time lighting/shadows switched on from
  the start (the concern spec 001-1 would otherwise have introduced later).
- **Alternatives considered**: None — this isn't a choice between rendering approaches, it's the
  removal of a problem that was specific to the previous engine.

## 2. Shared game state

- **Decision**: One plain C# `GameState` class owns player position/movement state, installed +
  spare battery charge, and door state. A single `GameManager` `MonoBehaviour` in the scene
  creates and owns it; every other `MonoBehaviour` (player rig, HUD, interactables) holds a
  reference to the same instance and reads/writes it each frame.
- **Rationale**: Constitution Principle III requires state to live in one plain object with no
  Unity-lifecycle dependency, named before implementation starts, and requires justifying any
  ad-hoc state pattern. A single shared object avoids duplicating state between scene objects and
  UI, and keeps `GameState` unit-testable without a running scene.
- **Alternatives considered**: `UnityEvent`/C# event bridging between scene objects and UI —
  rejected, adds indirection with no benefit when everything already runs in the same process and
  can hold a direct reference. Per-system singletons — rejected as directly against Principle III.

## 3. Real-time battery drain timing

- **Decision**: Drain is computed from `Time.deltaTime` accumulated into `GameState`, not from a
  fixed per-frame decrement.
- **Rationale**: FR-004 requires exactly 180 real-time seconds regardless of frame rate; a
  frame-count-based drain would run faster or slower if fps drops (which is explicitly a risk
  this phase is testing for), corrupting the very measurement SC-002 depends on. `Time.deltaTime`
  is Unity's standard per-frame elapsed time and needs no custom clock.
- **Alternatives considered**: Per-frame fixed decrement assuming 60fps — rejected, breaks
  correctness exactly when fps drops, which is the scenario most worth testing correctly.

## 4. Virtual joystick & action button

- **Decision**: Custom `uGUI` components driven by the Unity Input System's pointer/touch
  actions, no third-party control library.
- **Rationale**: Constitution requires justifying any new dependency; a 360° joystick and a
  single context-sensitive button are both small enough to build directly, and doing so keeps
  full control over the tunable parameters (deadzone, sprint threshold) that FR-015 requires to
  live in `GameConfig`.
- **Alternatives considered**: Third-party joystick packages (e.g. Asset Store on-screen control
  kits) — rejected, no justified need.

## 5. Camera smooth-follow and boundary clamping

- **Decision**: `CameraController` (plain C#) lerps a target position toward the player position
  each frame using `GameConfig.cameraFollowLerpFactor`, then clamps the result to the room's
  bounds (inset by `GameConfig.roomBoundsInset`) so the camera never shows area outside the
  level. A `CameraRig` `MonoBehaviour` applies that position to the scene's one `Camera`, which is
  set to `orthographic = true`, tilted per GDD Ch. 13 (`GameConfig.cameraTiltDegrees`, default
  `45`), with a fixed `GameConfig.cameraOrthographicScale` — no FOV-based perspective and no
  dynamic zoom.
- **Rationale**: FR-012 and FR-013 require the camera and the player to never visually drift
  apart. With one scene and one camera there is nothing to keep in sync — this removes the
  original two-camera synchronization problem (GDD Ch. 11.2, pre-migration: "posisi, sudut, dan
  skala harus dijaga cocok secara manual") entirely, rather than solving it.
- **Alternatives considered**: Unity's Cinemachine package for the follow/clamp behavior —
  rejected per Principle II; the follow-and-clamp math is a few lines and doesn't need a
  dedicated camera package for a single fixed room.

## 6. Configuration source format

- **Decision**: `GameConfig` is a single `ScriptableObject` class, with one asset instance
  (`Assets/Config/GameConfig.asset`) as the actual single source of truth.
- **Rationale**: FR-015 only requires one source of truth, not a specific file format. A
  `ScriptableObject` asset is Unity's idiomatic mechanism for exactly this — designer-editable in
  the Inspector without opening code, serialized as a single asset, and referenced by every
  system through one field. This is the direct Unity equivalent of the project's previous
  Swift-`enum`-of-constants decision, adapted because Unity doesn't compile per-build the way a
  native app's source constants do — an asset is the idiomatic single-source pattern here.
- **Alternatives considered**: A plain C# static class of constants (mirroring the original
  Swift `enum`) — rejected: it would need a recompile to tune any value and gets no Inspector
  editing, losing the exact "balancing without touching code" property GDD Ch. 17 asks for. A
  JSON/plist-style external file — rejected for now per Simplicity/YAGNI; no need beyond what a
  `ScriptableObject` already gives, and it would need custom (de)serialization code the
  `ScriptableObject` gets for free.

## 7. Test assembly

- **Decision**: Add a new `Assets/Tests/EditMode` assembly definition (`.asmdef`) referencing an
  EditMode test assembly, as a setup task.
- **Rationale**: None exists yet (confirmed: the project currently has no test assemblies).
  Constitution Principle IV requires tests to exist before a feature is done; the logic-only
  pieces (`GameConfig`, `BatteryController` math, light-state thresholds) are designed to not
  depend on `MonoBehaviour`/scene types specifically so they're testable in EditMode with no
  scene load.
- **Alternatives considered**: PlayMode tests only — rejected as the primary approach; slower and
  requiring a loaded scene for pure threshold/timing math that doesn't need one. PlayMode tests
  remain available later for interaction flows that do need a live scene, if needed.
