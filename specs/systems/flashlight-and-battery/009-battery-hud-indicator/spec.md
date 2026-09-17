# Feature Specification: Battery HUD Indicator

**Feature Branch**: `009-battery-hud-indicator`  
**Status**: Draft  
**GDD Sources**: Ch. 15.1

## User Stories & Acceptance

### US1 — Read charge and spare state at a glance (P1)
The HUD communicates current charge, light state, and whether a spare battery is carried without covering the play space.

**Independent test**: feed empty, low, flickering, and full state fixtures and compare icon, fill, and spare-slot output.

- Given charge changes, when the HUD refreshes, then its displayed value matches the state within one frame.
- Given a spare is picked up or installed, then the slot changes exactly once and the charge indicator remains correct.

## Functional Requirements

- FR-001: The indicator MUST expose charge as a normalized fill plus accessible text/value.
- FR-002: It MUST distinguish Normal, Low, and Flickering states using more than color alone.
- FR-003: It MUST show spare-slot occupancy and an install affordance only when the install gate is valid.
- FR-004: It MUST remain legible in dark scenes and respect safe-area margins.

## Edge Cases

Missing state, rapid drain, paused time, screen rotation, and color-vision limitations MUST retain an understandable display.

## Success Criteria

- SC-001: HUD fixture output matches all state boundaries.
- SC-002: A tester identifies charge band and spare presence without entering a menu in 10/10 trials.
- SC-003: HUD remains within safe area on supported iPhone aspect ratios.

## Scope

HUD presentation only; it does not own battery rules or audio.
