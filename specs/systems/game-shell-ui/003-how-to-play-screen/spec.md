# Feature Specification: How To Play Screen

**Feature Branch**: `003-how-to-play-screen`  
**Status**: Draft  
**GDD Sources**: Ch. 9.1, 15.4

## User Stories & Acceptance

### US1 — Understand the first action (P2)
Players can open a concise How To Play screen explaining movement, light, batteries, hiding, keys, and the objective without a long popup tutorial.

**Independent test**: open from Main Menu and pause, navigate all panels, then return without changing run state.

## Functional Requirements

- FR-001: Content MUST cover the GDD control/objective essentials in short, scannable panels.
- FR-002: It MUST be reachable from Main Menu and pause, and never appear as an unskippable gameplay popup.
- FR-003: Back/close must restore the prior shell context.

## Edge Cases

First launch, reload, missing localization text, rotation, and pause context MUST remain navigable.

## Success Criteria

- SC-001: Every essential mechanic has one readable explanation.
- SC-002: Opening/closing changes no gameplay state.

## Scope

Reference screen only; Floor 52 onboarding remains in its scene spec.
