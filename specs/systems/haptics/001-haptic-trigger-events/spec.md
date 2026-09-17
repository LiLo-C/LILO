# Feature Specification: Haptic Trigger Events

**Feature Branch**: `001-haptic-trigger-events`  
**Status**: Draft  
**GDD Sources**: Ch. 15.2

## User Stories & Acceptance

### US1 — Feel important events (P2)
Supported devices provide restrained haptic feedback for interaction success, low battery, catch, and outcome events.

**Independent test**: emit each event with haptics enabled/disabled and verify one mapped pulse or no pulse.

## Functional Requirements

- FR-001: Event-to-pattern mapping MUST be centralized and documented.
- FR-002: Haptics MUST respect settings, device capability, pause, and accessibility preference.
- FR-003: Repeated source events MUST be debounced where specified and never block gameplay.

## Edge Cases

Unsupported device, disabled setting, app backgrounding, duplicate event, and low-power mode MUST be safe.

## Success Criteria

- SC-001: Every supported event maps to one expected pattern.
- SC-002: Disabled/unsupported fixtures produce zero haptic calls.

## Scope

Haptic event mapping only; source systems own event semantics.
