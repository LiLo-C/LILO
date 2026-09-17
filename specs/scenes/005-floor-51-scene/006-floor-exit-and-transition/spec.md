# Feature Specification: Floor 51 Exit and Transition

**Status**: Draft  
**GDD Sources**: Ch. 8.1, 8.2

## User Stories & Acceptance

### US1 — Advance after the lock (P1)
After the Floor 51 door is unlocked, the player reaches the exit and transitions to Floor 50 without losing run state.

**Independent test**: attempt exit before and after unlock, then reload Floor 50.

## Functional Requirements

- FR-001: Exit MUST require the Floor 51 objective/door state.
- FR-002: It MUST issue one shared transition request to Floor 50.
- FR-003: Reset/death MUST return the exit to its initial locked/objective state.

## Success Criteria

- SC-001: Precondition matrix has no false-positive exit.
- SC-002: Valid exit loads Floor 50 once and preserves lives/run state.

## Scope

Scene exit and transition handoff only.
