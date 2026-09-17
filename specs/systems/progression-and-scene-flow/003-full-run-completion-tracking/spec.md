# Feature Specification: Full Run Completion Tracking

**Feature Branch**: `003-full-run-completion-tracking`  
**Status**: Draft  
**GDD Sources**: Ch. 2.3, 10.4

## User Stories & Acceptance

### US1 — Complete one coherent run (P1)
The game records completion only after the final door routes to Good Ending, while a Bad Ending records failure without falsely claiming success.

**Independent test**: execute successful, failed, reset, and reload fixtures and inspect the run record.

## Functional Requirements

- FR-001: Completion MUST require Floor 52→51→50 progression and final-door success.
- FR-002: Completion/failure MUST be mutually exclusive and idempotent.
- FR-003: A new run MUST clear prior run completion while preserving any intended settings.

## Edge Cases

Reload during ending, duplicate final-door input, abandoned run, and corrupted progress MUST not create a false success.

## Success Criteria

- SC-001: Only the valid full path records completion.
- SC-002: Repeated ending signals change the record zero additional times.

## Scope

Run-level tracking only; ending presentation is separate.
