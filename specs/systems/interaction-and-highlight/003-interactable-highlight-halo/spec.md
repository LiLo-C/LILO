# Feature Specification: Interactable Highlight Halo

**Feature Branch**: `003-interactable-highlight-halo`  
**Status**: Draft  
**GDD Sources**: Ch. 4.2

## User Stories & Acceptance

### US1 — Understand what can be acted on (P2)
The currently selected interactable receives a restrained halo that disappears when selection is lost.

**Independent test**: move selection across candidates and assert exactly one halo is active at each step.

- Given a valid target, then its halo is visible and the prior target's halo is cleared.
- Given no target, pause, or hiding/death, then no halo remains active.

## Functional Requirements

- FR-001: Halo ownership MUST follow the detector's selected target.
- FR-002: It MUST not reveal hidden targets through walls or exceed configured readability limits.
- FR-003: It MUST support non-color contrast and avoid distracting flicker during stable selection.

## Edge Cases

Destroyed target, disabled renderer, rapid swaps, and low-light scenes MUST leave no orphaned halo.

## Success Criteria

- SC-001: Selection transition tests show zero orphan or duplicate halos.
- SC-002: A first-time player identifies the actionable object in usability testing.

## Scope

Highlight presentation only.
