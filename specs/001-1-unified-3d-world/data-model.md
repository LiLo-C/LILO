# Phase 1 Data Model: LILO Phase 1.1 — Unified 3D World & Feel Pass

`GameState` and its existing entities (`PlayerCharacter`, `Battery`, `Door`, `TestRoom`,
`PlaceholderMonsterFigure`, derived `LightState`) are **unchanged** — see [001's
data-model.md](../001-core-prototype/data-model.md), which remains authoritative for all of
them (spec FR-023). This document covers only what 001-1 adds: state needed to drive the
extended `LightingRig`, `CameraController`, `CollisionResolver` and `HighlightController`. None
of it is stored on `GameState` itself — it is owned by the plain-C#-backed systems/`MonoBehaviour`
adapters that read `GameState`, keeping `GameState` free of render-specific fields (constitution
Principle III: gameplay state and rendering-facing state are different things — here, the
render-facing state below is not gameplay state, so it does not belong on the shared root).

## Light Profile (derived from `GameState.lightState`, not stored)

Computed each frame from the existing `LightState` enum (001, unchanged) plus new `GameConfig`
values. Owned by the `LightingRig`/`LightRadiusEasing` system's internal easing state (see
below), not `GameState`.

| Light state | Target lit radius | Notes |
|---|---|---|
| `Normal` | `GameConfig.flashlightNormalRadius` | full radius, FR-004 |
| `Flickering` | `GameConfig.flashlightNormalRadius`, momentarily dipped by a flicker event | steady between events, FR-006 |
| `Critical` | interpolated between `flashlightNormalRadius` and `flashlightCriticalMinRadius` by remaining fraction of the Critical range | continuous narrowing, FR-005 |
| `CompactDarkness` | `GameConfig.flashlightCriticalMinRadius` | Must equal `flashlightNormalRadius * compactDarknessRadiusFraction`, preserving 001's 10% target and making the Critical → Compact transition continuous. |

## LightRadiusEasing State (owned by the lighting system, not `GameState`)

| Field | Type | Notes |
|---|---|---|
| `currentRadius` | `float` | the displayed, eased radius — approaches the target above each frame per FR-005 |
| `flickerPhase` | `enum { Steady, Dipping }` (+ `dippingElapsed: float` when `Dipping`) | drives FR-006's discrete flicker events; only advances while `lightState == Flickering` |
| `nextFlickerDelay` | `float` | re-rolled (within `flickerIntervalMin...flickerIntervalMax`) each time a dip event ends |

This is intentionally *not* on `GameState`: it is display interpolation over a value
(`lightState`) that `GameState` already derives authoritatively. Keeping it local to the lighting
system means `GameState`'s meaning (installed battery charge → light state, per 001 FR-005) is
never ambiguous with how that state is *rendered*.

## Camera Follow State (owned by `CameraController`, not `GameState`)

| Field | Type | Notes |
|---|---|---|
| `currentPosition` | `Vector2` | the eased 2D follow position — identical role to 001's `CameraController`, now feeding a tilted 3D rig instead of a placeholder transform |

Unchanged in shape from 001; documented here only because 001-1's plan renames its consumer
(`CameraRig`).

## Highlight State (derived from `InteractionController`'s existing computation, not stored)

| Value | Formula | Notes |
|---|---|---|
| Battery highlight intensity | `inRange ? highlightInRangeIntensity : highlightOutOfRangeIntensity` | one per loose battery still in `World`; `inRange` reuses 001's existing distance test |
| Door highlight intensity | same formula, using the door's existing distance test | only while `door.isOpen == false` — an opened door has nothing left to highlight toward |

No new `GameState` field: `HighlightController` recomputes this every frame from
`GameState.player.position`, `GameState.room`, `GameState.worldBatteries` and `GameState.door`,
exactly the same inputs `InteractionController`'s nearest-interactable logic already reads.

## Collision Shapes (static, derived from `TestRoom`, not stored)

| Shape | Source | Notes |
|---|---|---|
| Room bounds | `TestRoom.bounds` (001, unchanged) | outer clamp, same per-axis independent clamp as 001's `MovementController` (already slide-like at the boundary) |
| Desk rect | `TestRoom.deskFrame` (001, unchanged) | the one interior solid `CollisionResolver` pushes the player circle out of (FR-014); 001 declared this field but had no collision component wired to it |

`CollisionResolver` takes these as plain `Rect`s read from `GameState.room` — no new entity, no
new `GameState` field.

## New `GameConfig` Fields

See [contracts/config-additions.md](./contracts/config-additions.md) for the full table with
types, defaults and sourcing — reproduced in summary here:

- Light profile: `flashlightNormalRadius`, `flashlightCriticalMinRadius`, `lightRadiusEaseRate`,
  `flickerIntervalMin`, `flickerIntervalMax`, `flickerEventDuration`, `flickerDipFraction`,
  `readabilityFillIntensity`
- Camera: `cameraTiltDegrees` (already existed in 001; value now supplied),
  `cameraOrthographicScale` (already existed; value now supplied), `cameraDistance`
- Shadows: `shadowsEnabled`, `shadowMapSize`, `shadowSampleCount`
- Highlight: `highlightColor`, `highlightOutOfRangeIntensity`, `highlightInRangeIntensity`
- World: `wallHeight`, `wallThickness`, `playerRadius`, `maxFrameDelta`
- Feedback: `batteryPickupAnimationDuration`, `doorOpenAnimationDuration`
- Joystick: `joystickControlZoneWidthFraction`, `joystickControlZoneHeightFraction` (replaces
  001's fixed on-screen joystick position field, which was never numerically supplied)

## Validation Rules Summary (new in 001-1)

- `currentRadius` (Lighting easing state) MUST only move toward its target — it is never set
  directly except at scene setup (spec FR-005: "MUST NOT jump between sizes").
- `flashlightCriticalMinRadius` MUST equal `flashlightNormalRadius *
  compactDarknessRadiusFraction`; this shared boundary prevents a radius jump when the derived
  `LightState` changes from Critical to Compact Darkness.
- A flicker event (`flickerPhase == Dipping`) MUST only begin while `GameState.lightState ==
  Flickering`; if charge crosses out of Flickering mid-dip, the dip MUST end immediately rather
  than finish playing (spec Edge Case: the easing added here must not make the light show a
  different state than the one in effect).
- `CollisionResolver` MUST NOT move the player's position outside `TestRoom.bounds` even after
  resolving desk overlap (desk push-out runs after, and is itself clamped by, the existing room
  clamp) — prevents a corner case where pushing out of the desk could theoretically eject the
  player through the room boundary if the desk were adjacent to a wall (it isn't, in this room,
  but the rule holds generally).
- Highlight intensity for a battery MUST drop to "not rendered" (`HighlightView` disabled/
  destroyed) at the same moment `GameState.worldBatteries` loses that entry (001 FR-016,
  unchanged) — no orphaned highlight objects.
