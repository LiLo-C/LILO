# Feature Specification: Locked Door Unlock Logic

**Feature Branch**: `002-locked-door-unlock-logic`  
**Status**: Draft  
**GDD Sources**: Ch. 8.1

## User Stories & Acceptance

### US1 — Keys open only their matching doors (P1)
The player can unlock a locked door when carrying its key; the key is consumed once and the door remains open.

**Independent test**: test missing, wrong, matching, repeated, and already-open states.

- Given a matching key, when Unlock is pressed, then the door unlocks/opens and exactly that key is removed.
- Given no matching key, then the door remains locked and feedback explains the requirement.
- Given an open door, repeated action is a no-op.

## Functional Requirements

- FR-001: Door/key matching MUST use stable IDs, never display labels.
- FR-002: Unlock MUST be atomic and idempotent.
- FR-003: Locked-door state MUST persist through valid scene state and reset on floor reset.

## Edge Cases

Wrong key, simultaneous input, destroyed door, reset during animation, and final-door subtype MUST be handled explicitly.

## Success Criteria

- SC-001: State matrix passes with one or zero inventory mutations as expected.
- SC-002: No wrong key opens any door.

## Scope

Unlock rules only; key pickup, door visuals, and final-door outcome are separate.
