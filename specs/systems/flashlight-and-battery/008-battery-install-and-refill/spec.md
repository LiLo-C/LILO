# Feature Specification: Battery Install and Refill

**Feature Branch**: `008-battery-install-and-refill`  
**Status**: Draft  
**GDD Sources**: Ch. 4.2, 5.2, 17.2

## User Stories & Acceptance

### US1 — Install a carried battery (P1)
When the lamp is empty and a spare battery is carried, the player can install it through the contextual action and immediately receive a full charge.

**Independent test**: run empty/no-spare, charged/spare, and empty/spare fixtures; assert action availability, inventory consumption, and resulting charge.

- Given charge is empty and one spare exists, when Install is pressed, then the spare is consumed exactly once and charge becomes full.
- Given charge is non-empty or no spare exists, then Install is unavailable and state is unchanged.

### US2 — Teach the gate clearly (P2)
The action prompt and feedback explain why installation is unavailable without exposing internal state.

**Independent test**: inspect prompt state for each gate condition and verify no ambiguous enabled button remains.

## Functional Requirements

- FR-001: Install MUST require the lamp charge threshold defined by `GameConfig` and a spare battery.
- FR-002: A successful install MUST atomically consume one spare and refill to the configured maximum.
- FR-003: Repeated input in one interaction window MUST produce at most one install.
- FR-004: UI feedback MUST distinguish “lamp not empty” from “no spare”.

## Edge Cases

Simultaneous pickup/install, input during transition, full charge, and a missing battery reference MUST not duplicate or lose inventory.

## Success Criteria

- SC-001: All gate fixtures pass with exactly one state mutation.
- SC-002: Successful install reaches full charge within one simulation tick.
- SC-003: No invalid install can reduce inventory or alter charge.

## Scope

Install/refill rules and feedback only; battery pickup, drain, light radius, and HUD rendering remain separate specs.
