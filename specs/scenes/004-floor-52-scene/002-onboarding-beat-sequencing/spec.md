# Feature Specification: Floor 52 Onboarding Beat Sequencing

**Status**: Draft  
**GDD Sources**: Ch. 15.4

## User Stories & Acceptance

### US1 — Learn by doing (P1)
Floor 52 introduces movement, light, battery, hiding, noise, and exit in a readable sequence without long tutorial popups.

**Independent test**: play from checkpoint to exit and record each mechanic's first required use in order.

- Given a new player, then each beat is encountered in the approved order and cannot be silently skipped.
- Given a returning player, then the sequence remains playable without disruptive repetition.

## Functional Requirements

- FR-001: Beat order MUST follow the GDD table and the Floor 52 geometry contract.
- FR-002: Each beat MUST have a visible/audio affordance and a completion condition.
- FR-003: Failure/restart MUST return to a valid beat boundary.

## Success Criteria

- SC-001: First-time playtesters complete all beats without external instruction.
- SC-002: No beat requires a system not yet introduced.

## Scope

Onboarding orchestration only.
