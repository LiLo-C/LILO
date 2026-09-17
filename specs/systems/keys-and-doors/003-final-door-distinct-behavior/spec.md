# Feature Specification: Final Door Distinct Behavior

**Feature Branch**: `003-final-door-distinct-behavior`  
**Status**: Draft  
**GDD Sources**: Ch. 8.1, 8.2, 21

## User Stories & Acceptance

### US1 — Know the run is ending (P1)
After all Floor 50 keys are held, the final door offers a distinct completion interaction and routes to the Good Ending.

**Independent test**: vary key count and press the final door; assert locked feedback, unlock state, and exactly one transition.

- Given fewer than required keys, then the final door stays locked and does not transition.
- Given all required keys, when opened, then completion is recorded and Good Ending is requested once.
- Given repeated input or a transition in progress, then no duplicate transition occurs.

## Functional Requirements

- FR-001: Final-door eligibility MUST require the configured Floor 50 key set.
- FR-002: It MUST be visibly and behaviorally distinct from ordinary doors.
- FR-003: Completion MUST be committed before transition and remain valid after reload.

## Edge Cases

Wrong floor, missing key, reset, pause, and duplicate touch MUST not produce a false completion.

## Success Criteria

- SC-001: Eligibility matrix has no false-positive completion.
- SC-002: A valid final-door action creates one completion and one transition request.

## Scope

Final-door gate and completion handoff; ending scene content is separate.
