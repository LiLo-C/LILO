# Feature Specification: Enter and Exit Hiding

**Feature Branch**: `001-enter-and-exit-hiding`  
**Status**: Draft  
**GDD Sources**: Ch. 4.3, 15.4

## User Stories & Acceptance

### US1 — Hide under a desk (P1)
The player can enter a valid hiding spot through the action button, loses movement control while hidden, and can exit deliberately.

**Independent test**: use a valid spot, invalid distance, and blocked exit fixture; assert transitions and player placement.

- Given the player is near an available spot, when Hide is pressed, then state becomes Hidden and the player is placed at the spot's hide anchor.
- Given Hidden, when Exit is pressed, then state becomes Visible at the exit anchor and movement resumes.
- Given no valid spot or a blocked exit, then the action is unavailable and state does not corrupt.

## Functional Requirements

- FR-001: Entry/exit MUST be explicit state transitions with one active hiding spot.
- FR-002: Hidden state MUST suppress movement and ordinary interaction while retaining exit action.
- FR-003: Entry and exit anchors MUST be validated for collision and navigability.
- FR-004: The system MUST emit events for audio, light, and detection consumers.

## Edge Cases

Spot destroyed, monster overlap, pause/death during transition, and scene reset MUST resolve to a valid visible or reset state.

## Success Criteria

- SC-001: Valid enter/exit fixtures pass with deterministic anchors.
- SC-002: No movement input changes position while Hidden.
- SC-003: Invalid transitions leave state unchanged.

## Scope

Core hiding state and placement; immunity and dampening are separate specs.
