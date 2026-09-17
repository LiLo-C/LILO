# Feature Specification: Sound Event Taxonomy

**Feature Branch**: `001-sound-event-taxonomy`  
**Status**: Draft  
**GDD Sources**: Ch. 14.1

## User Stories & Acceptance

### US1 — Audio responds consistently (P1)
All gameplay audio requests use named event categories with predictable mix priority and variation rules.

**Independent test**: enumerate GDD Ch. 14.1 rows and dispatch each event through the taxonomy.

## Functional Requirements

- FR-001: Events MUST be named by gameplay meaning, not file path.
- FR-002: Taxonomy MUST cover player, monster, light/battery, interaction, ambience, narrative, and UI events from the GDD.
- FR-003: Each event MUST define priority, bus/category, positional behavior, and whether it may overlap.
- FR-004: Missing assets MUST produce a visible development warning and safe silence.

## Edge Cases

Duplicate event, missing clip, rapid repeated noise, paused game, and accessibility mute MUST be deterministic.

## Success Criteria

- SC-001: Every GDD audio row maps to one event definition.
- SC-002: Unknown events never crash or play an arbitrary clip.

## Scope

Event contract only; mixing and sourcing are separate.
