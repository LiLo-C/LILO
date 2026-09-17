# Feature Specification: Floor 52 Static Battery and Hiding Placement

**Status**: Draft  
**GDD Sources**: Ch. 3, 15.4, 16.3

## User Stories & Acceptance

### US1 — Find the lesson objects (P1)
Floor 52 contains 3–5 static batteries and exactly one on-path hiding spot placed to teach, not gate, the run.

**Independent test**: inspect the scene and walk the critical path, counting reachable objects and verifying no respawn occurs.

## Functional Requirements

- FR-001: Battery count MUST be 3–5, static, reachable, and spread across the approved geometry.
- FR-002: Exactly one hiding spot MUST be on the unavoidable path.
- FR-003: Placement MUST not create a key/door dependency or unsafe trap.

## Success Criteria

- SC-001: Placement audit passes count, reachability, and spread checks.
- SC-002: Batteries never respawn during a floor run.

## Scope

Scene placement only; runtime pickup/spawn rules are separate.
