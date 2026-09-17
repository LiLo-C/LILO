# Feature Specification: Floor 50 Battery Spawn Point Placement

**Status**: Draft  
**GDD Sources**: Ch. 3.1

## User Stories & Acceptance

### US1 — Manage scarce light (P1)
Floor 50 provides at most one active battery point with the shared 60-second respawn rule.

**Independent test**: collect the battery, simulate time, and verify cap, respawn, and reset.

## Functional Requirements

- FR-001: The scene MUST define one valid spawn point and no second active point.
- FR-002: The shared system MUST enforce one active battery and 60-second respawn.
- FR-003: Placement MUST be reachable but expose meaningful risk.

## Success Criteria

- SC-001: Count/collision/reachability audit passes.
- SC-002: Runtime cap and timer fixtures pass with scene data.

## Scope

Scene placement only.
