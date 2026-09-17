# Feature Specification: Floor 51 Hiding Spot Placement

**Status**: Draft  
**GDD Sources**: Ch. 3, 4.3

## User Stories & Acceptance

### US1 — Find cover during pressure (P1)
Floor 51 places hiding spots where a player can break pursuit without making the route trivial.

**Independent test**: play a chase from nearby route segments and enter/exit each spot safely.

## Functional Requirements

- FR-001: Every spot MUST have valid entry/exit anchors and cover geometry.
- FR-002: Placement MUST support the shared immunity/dampening rules and not create a trap.
- FR-003: Spots MUST be discoverable but not clustered at the checkpoint or exit.

## Success Criteria

- SC-001: Each spot passes anchor, collision, and chase-exit validation.
- SC-002: Playtesters can use cover without bypassing the floor objective.

## Scope

Scene placement only.
