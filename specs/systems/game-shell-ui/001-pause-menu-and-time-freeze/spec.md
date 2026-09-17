# Feature Specification: Pause Menu and Time Freeze

**Feature Branch**: `001-pause-menu-and-time-freeze`  
**Status**: Draft  
**GDD Sources**: Ch. 15.1, 18.1

## User Stories & Acceptance

### US1 — Pause safely (P1)
The player can pause gameplay, see a clear menu, and resume without the monster, timers, or input advancing behind the menu.

**Independent test**: pause during movement, chase, battery drain, and transition; advance simulation and assert gameplay state is frozen.

## Functional Requirements

- FR-001: Pause MUST freeze gameplay simulation and input while leaving menu input active.
- FR-002: Resume, settings, and quit/restart actions MUST have explicit outcomes and no duplicate dispatch.
- FR-003: Audio and haptics MUST follow the pause policy.

## Edge Cases

Repeated pause, app backgrounding, death/ending, and transition in progress MUST resolve to one valid mode.

## Success Criteria

- SC-001: All frozen systems remain unchanged while paused.
- SC-002: Resume restores the exact prior gameplay mode.

## Scope

Pause shell and time-freeze contract only.
