# Feature Specification: Bad Ending Scene

**Status**: Draft  
**GDD Sources**: Ch. 9.3, 10.4

## User Stories & Acceptance

### US1 — Resolve a failed run (P1)
When lives reach zero, the Bad Ending presents the approved failure beat and does not reset the current floor as if a life remained.

**Independent test**: enter after the final life is lost and compare state, narrative, and return action.

## Functional Requirements

- FR-001: Entry MUST require the authoritative zero-lives outcome.
- FR-002: It MUST never call floor reset or decrement lives again.
- FR-003: Replay/menu actions MUST start a new run explicitly.

## Success Criteria

- SC-001: Zero-life entry produces one Bad Ending and zero floor resets.
- SC-002: Invalid positive-life entry cannot reach the Bad Ending.

## Scope

Bad-ending scene flow and presentation.
