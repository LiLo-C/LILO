# Feature Specification: Nearest Interactable Detection

**Feature Branch**: `001-nearest-interactable-detection`  
**Status**: Draft  
**GDD Sources**: Ch. 4.2

## User Stories & Acceptance

### US1 — Action targets are predictable (P1)
The player sees one nearest valid target within the interaction radius, chosen deterministically.

**Independent test**: place zero, one, and multiple candidates at boundary distances and assert the selected target.

- Given candidates are in range, when detection runs, then the nearest visible/valid candidate is selected.
- Given equal distance candidates, then stable priority ordering resolves the tie and does not flicker frame to frame.
- Given no candidate or a blocked candidate, then the result is empty.

## Functional Requirements

- FR-001: Detection MUST use the configured radius and validity filters.
- FR-002: It MUST exclude disabled, occluded, already-completed, and non-interactable objects.
- FR-003: It MUST return a stable result and metadata sufficient for the action button.

## Edge Cases

Boundary distance, moving candidates, simultaneous enable/disable, and camera obstruction MUST be deterministic.

## Success Criteria

- SC-001: Candidate fixture matrix returns the expected target in every case.
- SC-002: Selection does not oscillate for static equal-distance candidates.

## Scope

Detection only; action execution and halo presentation are separate.
