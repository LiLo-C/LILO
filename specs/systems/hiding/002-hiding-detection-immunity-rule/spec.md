# Feature Specification: Hiding Detection Immunity

**Feature Branch**: `002-hiding-detection-immunity-rule`  
**Status**: Draft  
**GDD Sources**: Ch. 4.3, 7.2

## User Stories & Acceptance

### US1 — Hiding is a real tactical choice (P1)
While fully Hidden, the monster cannot detect or catch the player through ordinary detection checks; detection resumes after exit.

**Independent test**: run identical noise/line-of-sight fixtures while Visible, Entering, Hidden, and Exiting.

- Given Hidden, when a detection check runs, then it returns immune and emits no catch.
- Given the player exits, then the next eligible check uses normal rules.
- Given an invalid/interrupted hide transition, then immunity is never granted accidentally.

## Functional Requirements

- FR-001: Immunity MUST apply only during the canonical Hidden state.
- FR-002: It MUST be consumed by the detection system as an explicit result, not a duplicated distance exception.
- FR-003: Catch signals already in flight MUST obey the death-sequence contract.

## Edge Cases

Pause, floor reset, overlapping spots, and scene unload MUST clear immunity consistently.

## Success Criteria

- SC-001: Hidden fixtures produce zero detection/catch events.
- SC-002: Visible and post-exit fixtures match baseline detection behavior.

## Scope

Detection immunity only; hide controls and audio/light dampening remain separate.
