# Feature Specification: Floor 52 Exit and Transition

**Status**: Draft  
**GDD Sources**: Ch. 8.1, 8.2

## User Stories & Acceptance

### US1 — Leave the lesson floor (P1)
After reaching the unkeyed exit door, the player receives clear feedback and transitions to Floor 51 exactly once.

**Independent test**: approach from both sides, press with/without interaction eligibility, and inspect one transition request.

## Functional Requirements

- FR-001: Floor 52 exit MUST require no key.
- FR-002: It MUST use the shared transition manager and preserve run state.
- FR-003: Repeated input and transition-in-progress MUST be idempotent.

## Success Criteria

- SC-001: Valid exit produces one Floor 51 load.
- SC-002: No key prompt or false locked state appears.

## Scope

Exit placement and handoff only.
