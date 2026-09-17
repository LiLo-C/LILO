# Phase 0 Research: LILO Phase 1 — Core Movement & Light System Prototype

No `[NEEDS CLARIFICATION]` markers remain in the Technical Context — the `/speckit-clarify`
session on this feature already resolved the highest-impact unknowns (target device, iOS
version, battery respawn behavior). This document instead records the technical decisions
needed to move from spec to design, each with rationale and the alternative considered.

## 1. Hybrid 2D/3D rendering (the phase's core technical risk)

- **Decision**: Player character (and later the monster) renders as an `SK3DNode` embedding a
  `SCNScene`, layered above an `SKScene` that holds the 2D environment (`SKSpriteNode`/tilemap).
- **Rationale**: Already a locked decision in the source GDD (Ch. 11) and the constitution's
  platform constraints — 3D solves character facing/direction without per-direction sprite art,
  while 2D environment art stays cheap to iterate. Phase 1's job is to *validate*, not
  *reconsider*, this choice.
- **Alternatives considered**: Full 2D (sprite-direction set) — rejected for this phase because
  it's the GDD's own documented fallback, only to be taken if `SK3DNode` proves too expensive
  on-device (that measurement is what SC-002 exists to produce).

## 2. Shared game state

- **Decision**: One `@Observable final class GameState` owns player position/movement state,
  installed + spare battery charge, and door state. The root SwiftUI view creates and owns it;
  the `SKScene` holds an unowned reference and mutates it each frame; SwiftUI HUD views
  (joystick, action button, battery indicator) read it via `@Bindable`/environment.
- **Rationale**: Constitution Principle III requires state to live in an observable model type,
  named before implementation starts, and requires justifying any ad-hoc state pattern. A single
  shared observable avoids the common SpriteKit/SwiftUI integration trap of duplicating state in
  both the scene and the view layer.
- **Alternatives considered**: `NotificationCenter` bridging between `SKScene` and SwiftUI —
  rejected, constitution explicitly calls this out as needing justification and a shared
  observable is simpler here. Per-system singletons — rejected as directly against Principle III.

## 3. Real-time battery drain timing

- **Decision**: Drain is computed from wall-clock delta time (`CACurrentMediaTime()` diff between
  scene updates), accumulated into `GameState`, not from a fixed per-frame decrement.
- **Rationale**: FR-004 requires exactly 180 real-time seconds regardless of frame rate; a
  frame-count-based drain would run faster or slower if fps drops (which is explicitly a risk
  this phase is testing for), corrupting the very measurement SC-002 depends on.
- **Alternatives considered**: Per-frame fixed decrement assuming 60fps — rejected, breaks
  correctness exactly when fps drops, which is the scenario most worth testing correctly.

## 4. Virtual joystick & action button

- **Decision**: Custom SwiftUI views built on `DragGesture`, no third-party control library.
- **Rationale**: Constitution requires justifying any new dependency; a 360° joystick and a
  single context-sensitive button are both small enough to build directly, and doing so keeps
  full control over the tunable parameters (deadzone, sprint threshold) that FR-015 requires to
  live in `GameConfig`.
- **Alternatives considered**: Third-party joystick packages — rejected, no justified need.

## 5. Camera smooth-follow and boundary clamping

- **Decision**: `CameraController` lerps the `SKCameraNode` position toward the player position
  each frame, then clamps the result to the room's bounds (inset by half the visible viewport) so
  the camera never shows area outside the level. The `SCNCamera` used for the 3D player layer is
  kept in sync (position/orthographic scale) by deriving its transform from the same 2D camera each frame rather
  than maintaining two independently-tuned cameras.
- **Framing (alignment pass)**: per GDD Ch. 13 the 3D camera is orthographic (`usesOrthographicProjection`),
  north-facing with a ~45° tilt (`GameConfig.cameraTiltDegrees`) and a fixed `GameConfig.cameraOrthographicScale`;
  no FOV-based perspective and no dynamic zoom.
- **Rationale**: FR-012 and FR-013 both fail if the two cameras drift independently — deriving
  one from the other is the only way to guarantee they can't disagree.
- **Alternatives considered**: Two independently-configured cameras kept in sync by matching
  tuned constants — rejected, GDD Ch. 11.2 calls this out as the exact historical pain point
  ("posisi, sudut, dan skala harus dijaga cocok secara manual").

## 6. Configuration source format

- **Decision**: `GameConfig` is a single Swift `enum`/`struct` namespace of `static let` constants
  (not a `.plist`).
- **Rationale**: FR-015 only requires one source of truth, not a specific file format; a Swift
  source file gets compile-time type safety and autocomplete, and is simpler than wiring up
  `.plist` decoding for a single-developer/small-team prototype phase. Revisit as a `.plist` only
  if non-programmer teammates need to tune values without opening Xcode — not needed for Phase 1.
- **Alternatives considered**: `.plist` — rejected for now per Simplicity/YAGNI; can be swapped in
  later without changing any call site if `GameConfig`'s public shape stays stable.

## 7. Test target

- **Decision**: Add a new `v2Tests` XCTest target to the Xcode project as a setup task.
- **Rationale**: None exists yet (confirmed: the project currently has exactly one native
  target). Constitution Principle IV requires tests to exist before a feature is done; the
  logic-only pieces (`GameConfig`, `BatteryController` math, light-state thresholds) are designed
  to not depend on `SKScene`/`SK3DNode` specifically so they're testable without a UI test host.
- **Alternatives considered**: UI tests only via `XCUITest` — rejected as the primary approach;
  slower and flakier for pure threshold/timing math than plain XCTest, though UI tests remain
  available later for the interaction flows if needed.
