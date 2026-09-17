# Feature Specification: Floor 51 Monster Patrol Route and Spawn Presets

**Status**: Draft  
**GDD Sources**: Ch. 3, 6.3

## User Stories & Acceptance

### US1 — Pressure begins predictably (P1)
Floor 51 contains one monster with a validated patrol route and safe spawn that creates pressure without an unfair instant catch.

**Independent test**: inspect spawn and walk the patrol in a blocked-out scene, including reset.

## Functional Requirements

- FR-001: Exactly one `Monster` uses the Floor 51 tuning profile.
- FR-002: Spawn MUST be reachable, separated from the player checkpoint, and not inside forbidden geometry.
- FR-003: Route MUST have multiple waypoints, no trap dead-end, and deterministic reset behavior.

## Success Criteria

- SC-001: Spawn validation passes all minimum-distance/visibility rules.
- SC-002: Reset returns the monster to a valid preset waypoint.

## Scope

Scene route and spawn data only.
