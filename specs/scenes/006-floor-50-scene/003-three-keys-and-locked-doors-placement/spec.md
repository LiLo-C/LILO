# Feature Specification: Floor 50 Three Keys and Locked Doors Placement

**Status**: Draft  
**GDD Sources**: Ch. 3, 8.1

## User Stories & Acceptance

### US1 — Master the full objective (P1)
Floor 50 places three distinct matching key/door pairs that require exploration under the aggressive monster profile.

**Independent test**: audit counts/IDs, collect each key, unlock its door, and reset.

## Functional Requirements

- FR-001: Exactly three keys and three matching locked doors MUST exist.
- FR-002: Each pair MUST use stable IDs and be spatially distributed with no impossible ordering.
- FR-003: Placement MUST preserve escape routes and reset to initial state.

## Success Criteria

- SC-001: Count/ID/reachability audit passes.
- SC-002: All three pairs can be completed without a softlock in playtest.

## Scope

Scene placement only.
