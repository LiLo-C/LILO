# Feature Specification: Monster Catch Outcome Signal

**Feature Branch**: `004-catch-outcome-signal`  
**Status**: Draft  
**GDD Sources**: Ch. 6.1, 9.3

## User Stories & Acceptance

### US1 — A catch starts one death flow (P1)
When the monster catches the player, one authoritative signal starts the death sequence and no duplicate catch can decrement lives twice.

**Independent test**: emit one signal, repeated signals during sequence, and separate signals after reset.

- Given a valid visible player, when catch occurs, then one `CatchSignal` is emitted with floor/context metadata.
- Given a sequence is in progress, further signals are ignored.
- Given the next run/floor begins, a new catch can be accepted.

## Functional Requirements

- FR-001: Signal payload MUST identify source, floor, and event time without owning outcome branching.
- FR-002: Emission MUST be edge-triggered and idempotent for one encounter.
- FR-003: Death/outcome orchestration MUST consume the signal exactly once.

## Edge Cases

Hiding immunity, pause, scene unload, simultaneous contact, and monster disabled state MUST not create false catches.

## Success Criteria

- SC-001: One encounter yields one signal and one consumer invocation.
- SC-002: Guard tests show repeated signals do not duplicate outcomes.

## Scope

Catch event contract only; death sequence and bad-ending routing are downstream.
