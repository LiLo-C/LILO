# Feature Specification: Dynamic Audio Mix State Machine

**Feature Branch**: `002-dynamic-mix-state-machine`  
**Status**: Draft  
**GDD Sources**: Ch. 14.2

## User Stories & Acceptance

### US1 — Tension changes with danger (P1)
The mix shifts between exploration, investigation, chase, hiding, pause, and ending states without abrupt or contradictory transitions.

**Independent test**: feed the state sequence and assert active mix, priority, and restoration after each transition.

## Functional Requirements

- FR-001: Mix states MUST be driven by authoritative gameplay state.
- FR-002: Transitions MUST be deterministic, eased, and reversible.
- FR-003: Pause/settings MUST preserve underlying gameplay state and restore the prior mix.
- FR-004: Hiding and ending states MUST have explicit precedence.

## Edge Cases

Rapid transitions, missing mixer snapshots, app suspend, duplicate state, and mute settings MUST fail safely.

## Success Criteria

- SC-001: State-matrix tests produce the expected snapshot and precedence.
- SC-002: No transition leaves a stale snapshot active.

## Scope

Mix state selection and transitions; event taxonomy and asset sourcing are separate.
