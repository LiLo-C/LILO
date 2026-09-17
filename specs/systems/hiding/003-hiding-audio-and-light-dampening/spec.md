# Feature Specification: Hiding Audio and Light Dampening

**Feature Branch**: `003-hiding-audio-and-light-dampening`  
**Status**: Draft  
**GDD Sources**: Ch. 4.3, 5.2, 14.2

## User Stories & Acceptance

### US1 — Hiding changes the player's presence (P2)
Entering a hiding spot dampens the player's emitted noise and lamp presentation, then restores both on exit.

**Independent test**: compare the same movement/light fixture before, during, and after hiding.

- Given Hidden, when an action would emit player noise, then the configured dampening rule is applied.
- Given Hidden, when the lamp is evaluated, then the configured visual/audio dampening is applied without changing stored battery charge.
- Given exit, then the exact pre-hide presentation and emission rules return.

## Functional Requirements

- FR-001: Dampening MUST be a reversible view/modifier over existing systems.
- FR-002: It MUST not alter battery quantity, light-state thresholds, or inventory.
- FR-003: Entry/exit events MUST apply and clear modifiers exactly once.

## Edge Cases

Pause, death, floor reset, low battery, and interrupted transitions MUST clear temporary modifiers.

## Success Criteria

- SC-001: Before/during/after fixtures produce expected noise and presentation outputs.
- SC-002: Stored battery state is bit-for-bit unchanged by hiding.

## Scope

Temporary audio/light presentation and noise modifiers only.
