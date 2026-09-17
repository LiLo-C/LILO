# Feature Specification: HUD Composition and Layout

**Feature Branch**: `004-hud-composition-and-layout`  
**Status**: Draft  
**GDD Sources**: Ch. 15.1

## User Stories & Acceptance

### US1 — See only vital information (P1)
The HUD shows charge/spare, current objective/floor, interaction prompt, and pause access without obscuring the horror space.

**Independent test**: render every gameplay state and supported aspect ratio, including hidden, chase, and low battery.

## Functional Requirements

- FR-001: Composition MUST reserve safe areas and maintain touch target sizes.
- FR-002: Vital indicators MUST remain legible in darkness and use non-color cues.
- FR-003: HUD visibility MUST follow pause, hiding, death, and ending modes.
- FR-004: Each component MUST read authoritative state and have one owner.

## Edge Cases

Notch, rotation, long text, missing state, and simultaneous prompts MUST degrade without overlap.

## Success Criteria

- SC-001: No overlap/cutoff occurs across supported aspect ratios.
- SC-002: Testers identify charge, objective, and available action without menu navigation.

## Scope

HUD composition and layout; component-specific rules remain in their feature specs.
