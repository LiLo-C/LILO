# Feature Specification: Floor 51 Battery Spawn Point Placement

**Status**: Draft  
**GDD Sources**: Ch. 3.1

## User Stories & Acceptance

### US1 — Recover light under pressure (P1)
Floor 51 provides up to two valid battery spawn points that work with the shared 30-second respawn rule.

**Independent test**: inspect points, collect batteries, and simulate respawn/cap behavior.

## Functional Requirements

- FR-001: Points MUST be reachable, separated, and outside walls/doors/unsafe spawn zones.
- FR-002: Placement MUST support a maximum of two active batteries and 30-second respawn through the shared system.
- FR-003: Reset MUST restore the floor's initial battery state.

## Success Criteria

- SC-001: Placement validation passes count, reachability, and collision checks.
- SC-002: Runtime cap/timer tests pass with scene points.

## Scope

Scene anchors only.
