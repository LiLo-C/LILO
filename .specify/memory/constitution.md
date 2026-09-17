# LILO Constitution

## Core Principles

### I. Spec-Driven Development

Every feature starts from an approved `spec.md` under `specs/`. No implementation begins before
that feature's `plan.md` and `tasks.md` exist and derive from the spec. Specs describe observable
behavior and success criteria, not implementation — implementation details belong in `plan.md`,
`research.md`, and code.

### II. Simplicity & YAGNI

Build exactly what the current spec's user stories require. Do not add systems, abstractions, or
generalized frameworks ahead of a spec that needs them (no physics engine for a nine-line
push-out, no reactive-state framework for one shared object, no config file format beyond a
single asset). Every dependency — Unity package or third-party — must be justified against a
specific requirement; "might need it later" is not a justification.

### III. Unity Architecture Consistency

Gameplay state lives in exactly one plain C# `GameState` object per running game, owned by a
single root `GameManager` `MonoBehaviour` in the scene. Tunable values live in exactly one
`GameConfig` `ScriptableObject` asset (Principle: single configuration source, below).
Systems that implement game rules (movement, battery drain, interaction, collision, camera,
lighting) are plain C# classes that take `GameState`/`GameConfig` as input and contain no
`UnityEngine.MonoBehaviour`, `Component`, or scene-graph dependency — this is what keeps them
unit-testable in EditMode without a running scene. `MonoBehaviour`s are thin adapters: they own
Unity lifecycle (`Update`, `OnCollisionEnter`, input callbacks), hold references to Unity objects
(`Transform`, `Light`, `Camera`, UI elements), and call into the plain C# systems each frame —
they do not contain gameplay rules themselves. No singletons or static mutable gameplay state
outside `GameConfig` (which is an immutable-at-runtime asset, not mutable global state) — a second
`GameManager` or a static `GameState` is a constitution violation, not a convenience.

### IV. Test-Before-Done (NON-NEGOTIABLE)

A user story is not "done" until its pure-logic systems have EditMode tests (Unity Test
Framework / NUnit) covering the story's acceptance scenarios, and those tests are green. Logic
that depends on rendering, physics callbacks, or input devices needing a live scene is the
exception and is validated instead through the feature's `quickstart.md` manual scenarios on
target hardware — but everything expressible as pure C# (thresholds, timers, state machines,
collision math, config shape) MUST be covered by an EditMode test, per Principle III's
testability requirement.

### V. Versioning & Change Tracking

Each spec amendment that changes a previously-shipped behavior (not just adds a new one) must
name what it supersedes (see the `Relationship to <spec>` pattern used by amendment specs).
`GameConfig` additions are always additive to the single existing asset — no second config
source is ever created, and no key already read by a shipped system is silently repurposed.

## Technology Stack

- **Engine**: Unity 6000.6.1f1 (or the current LTS the project has pinned in
  `ProjectSettings/ProjectVersion.txt` — do not bump without a recorded reason; editor version
  drift between contributors breaks Library caches and serialized scene diffs).
- **Language**: C#, nullable/implicit engine defaults as scaffolded by Unity's project templates.
- **Render pipeline**: Universal Render Pipeline (URP), 3D. The playable world — floor, walls,
  furniture, batteries, doors, player, monster — is one lit 3D scene; there is no 2D/3D hybrid
  render path in this project (see GDD Ch. 11, amended).
- **Input**: Unity Input System package (`com.unity.inputsystem`) — no legacy `Input.GetAxis`
  polling. On-screen joystick and action button are custom UI-driven controls (Principle II: no
  third-party control asset without a justified gap in the built-in package).
- **UI/HUD**: Unity UI (`com.unity.ugui`) `Canvas` overlay, rendered above the 3D world and
  unaffected by world lighting — the HUD equivalent of the old SwiftUI overlay.
- **Navigation/AI**: `com.unity.ai.navigation` (NavMesh) is available for the monster state
  machine (spec 002) — evaluate against Principle II when that spec is planned; do not adopt it
  reflexively if the noise-detection design needs less than a full NavMesh agent.
- **Testing**: Unity Test Framework (`com.unity.test-framework`), EditMode tests as the default
  per Principle IV.
- **Target platform**: iOS (iPhone), landscape orientation only — unchanged from the GDD's
  original platform decision; only the engine/render/language stack changed, not the shipping
  target. Revisit only via an explicit spec, not silently through an engine feature (Unity makes
  Android trivial to add later; that is not authorization to add it).
- **Persistence/haptics/third-party packages**: none adopted by default. A feature spec that
  needs one (e.g., 014's haptic feedback) picks the concrete API during its own planning phase
  and records the choice in that spec's `research.md`, per Principle I — this constitution does
  not pre-select one.

## Governance

This constitution supersedes ad-hoc practice. Amendments require: (1) a stated reason tied to a
concrete problem, (2) an updated version line below, (3) a pass over any spec whose `plan.md`
Constitution Check table cites the changed principle. Complexity that violates Principle II must
be justified in the offending plan's Complexity Tracking table, not silently accepted.

**Version**: 3.0.0 | **Ratified**: 2026-09-17 | **Last Amended**: 2026-09-17

<!--
Sync Impact Report — v3.0.0 (2026-09-17)
Change: MAJOR — full engine migration from native iOS (Swift + SpriteKit + SceneKit + SwiftUI)
to Unity (C# + URP). This is the constitution's first ratified version in this repository; no
prior version existed on disk (the vault's constitution link was broken — see
specs/_reference/constitution.md). Treated as a breaking rewrite rather than a patch because
every principle's technology anchor changed:
  - Principle III renamed "SwiftUI Architecture Consistency" → "Unity Architecture Consistency";
    @Observable GameState/SwiftUI root view → plain C# GameState/GameManager MonoBehaviour.
  - Principle IV: XCTest → Unity Test Framework (EditMode/NUnit).
  - Technology Stack: iOS/Swift/SpriteKit/SceneKit/SwiftUI/Xcode → Unity/C#/URP/Input
    System/uGUI.
Templates requiring updates: specs/001-core-prototype/* and specs/001-1-unified-3d-world/*
(rewritten alongside this constitution — see their plan.md Constitution Check tables).
Specs 002-017: pending a scan pass for engine-specific wording; tracked separately.
-->
