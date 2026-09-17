# Feature Specification: Settings Menu Audio Controls

**Feature Branch**: `002-settings-menu-audio-controls`  
**Status**: Draft  
**GDD Sources**: Ch. 18.1

## User Stories & Acceptance

### US1 — Control audio comfortably (P2)
The player can adjust master, music, ambience, and effects volume and mute haptics; settings persist across scenes and runs.

**Independent test**: edit each control, reload a scene, and verify the corresponding output and saved value.

## Functional Requirements

- FR-001: Controls MUST map to named audio buses/settings with bounded values.
- FR-002: Changes MUST apply immediately, persist safely, and not mutate gameplay state.
- FR-003: UI MUST expose accessible labels and reset/default behavior.

## Edge Cases

Corrupt preference, unsupported control, rapid slider input, and app restart MUST use documented defaults.

## Success Criteria

- SC-001: Each control changes only its intended bus.
- SC-002: Values round-trip across scene load and restart.

## Scope

Settings UI and preference contract only.
