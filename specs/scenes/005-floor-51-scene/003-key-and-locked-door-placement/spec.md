# Feature Specification: Floor 51 Key and Locked Door Placement

**Status**: Draft  
**GDD Sources**: Ch. 3, 8.1

## User Stories & Acceptance

### US1 — Solve one readable lock (P1)
Floor 51 places one key and one matching locked door so the player must explore, retreat, and return without a softlock.

**Independent test**: walk the route, collect the key, unlock the door, and reset the floor.

## Functional Requirements

- FR-001: Exactly one key and one locked door MUST exist and have matching stable IDs.
- FR-002: Key MUST be reachable before the door and not spawn inside danger geometry.
- FR-003: Door MUST be resettable to locked and key to its original location.

## Success Criteria

- SC-001: Scene audit finds one matching pair.
- SC-002: First-time playtesters can infer the objective from feedback without external instruction.

## Scope

Placement only.
