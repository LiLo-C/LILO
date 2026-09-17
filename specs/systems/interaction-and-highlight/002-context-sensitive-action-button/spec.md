# Feature Specification: Context-Sensitive Action Button

**Feature Branch**: `002-context-sensitive-action-button`  
**Status**: Draft  
**GDD Sources**: Ch. 4.2, 15.1

## User Stories & Acceptance

### US1 — One obvious action (P1)
The player receives a single action button whose label and enabled state match the nearest target.

**Independent test**: feed key, door, hiding spot, battery, and empty-target fixtures; compare label and command.

- Given a valid target, when it becomes nearest, then the button shows the target's action label.
- Given no valid target, then the button is hidden/disabled and cannot execute an action.
- Given the target changes, then the old action cannot fire.

## Functional Requirements

- FR-001: Labels MUST come from the target action contract, not object-name string matching.
- FR-002: Press MUST execute at most one action and provide success/failure feedback.
- FR-003: Disabled/paused/death states MUST suppress execution.

## Edge Cases

Target swap during press, repeated touch, missing label, and accessibility text MUST be handled safely.

## Success Criteria

- SC-001: Fixture matrix maps every target to the expected label/action.
- SC-002: Repeated touch causes one mutation.

## Scope

Button state and dispatch only; target detection and target-specific rules remain separate.
