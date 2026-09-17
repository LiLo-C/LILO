# Feature Specification: Scene Transition Manager

**Feature Branch**: `002-scene-transition-manager`  
**Status**: Draft  
**GDD Sources**: ROADMAP progression table; constitution Principle III

## User Stories & Acceptance

### US1 — Progression is reliable (P1)
The game loads the next approved scene exactly once and carries shared state across the transition.

**Independent test**: exercise every valid edge in the scene graph and invalid/repeated requests.

- Given a valid transition request, then the mapped scene loads once and state remains available.
- Given an invalid or duplicate request, then no unintended scene load occurs.

## Functional Requirements

- FR-001: The manager MUST use an explicit scene graph for Bootstrap, MainMenu, Prologue, floors, and endings.
- FR-002: Requests MUST be serialized and idempotent while a load is active.
- FR-003: It MUST report missing scenes and load failures clearly.

## Edge Cases

Back-to-back requests, scene unload, app suspend/resume, and ending completion MUST not lose or duplicate state.

## Success Criteria

- SC-001: All valid graph edges load the expected scene once.
- SC-002: Invalid/repeated requests produce zero unintended loads.

## Scope

Scene routing only; floor rules and splash content are separate.
