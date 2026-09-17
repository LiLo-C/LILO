# Feature Specification: Shared Game State and Manager

**Feature Branch**: `002-shared-game-state-and-manager`  
**Status**: Draft  
**GDD Sources**: Ch. 9, 17; constitution Principle III

## User Stories & Acceptance

### US1 — State survives scene changes (P1)
As a player, my lives, inventory, floor progress, and run outcome remain correct while scenes transition.

**Independent test**: mutate a state fixture, load a second scene, and compare every persisted field.

- Given Bootstrap has started, when a scene loads, then exactly one manager owns the run state.
- Given a scene transition occurs, when the next scene reads state, then it sees the same run values and no duplicate manager exists.

### US2 — Reset boundaries are explicit (P1)
As a system, I can reset only the fields appropriate to a floor restart, death, new run, or completed ending.

**Independent test**: invoke each reset operation and assert its documented field-by-field result.

## Functional Requirements

- FR-001: `GameState` MUST be a plain data object containing lives, current floor, checkpoint, key inventory, battery slot, hiding state, and run outcome.
- FR-002: `GameManager` MUST be the sole owner and persist from Bootstrap across scene loads.
- FR-003: State mutations MUST occur through named operations with deterministic ordering; consumers MUST NOT replace the state object.
- FR-004: New run, floor reset, and ending operations MUST have separate contracts and tests.
- FR-005: A missing or duplicate manager MUST fail visibly during development validation.

## Edge Cases

Scene load failure, repeated Bootstrap initialization, reset before a floor is entered, and transition requested during an ending MUST be rejected or safely ignored according to the operation contract.

## Success Criteria

- SC-001: Cross-scene fixture retains all persistent fields across every listed scene.
- SC-002: Duplicate-manager audit reports zero duplicates in a build.
- SC-003: Reset tests show no unrelated field is changed.

## Scope

This defines ownership and state contracts; it does not implement individual systems or scene content.
