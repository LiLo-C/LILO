# Feature Specification: Floor 50 Final Door Placement and Trigger

**Status**: Draft  
**GDD Sources**: Ch. 8.1, 8.2, 21

## User Stories & Acceptance

### US1 — Reach the escape (P1)
The final door is clearly distinct, reachable after the three objectives, and triggers the Good Ending once.

**Independent test**: attempt with 0–2 keys, then with all keys, including repeated input and reset.

## Functional Requirements

- FR-001: Door MUST be outside the three ordinary door interactions and use final-door behavior.
- FR-002: Trigger MUST require all three matching objectives and route through progression.
- FR-003: Placement MUST provide a readable approach and no unsafe dead-end.

## Success Criteria

- SC-001: Invalid attempts produce no completion/transition.
- SC-002: Valid attempt produces one Good Ending request.

## Scope

Scene placement and trigger only.
