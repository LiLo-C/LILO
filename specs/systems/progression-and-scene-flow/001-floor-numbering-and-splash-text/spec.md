# Feature Specification: Floor Numbering and Splash Text

**Feature Branch**: `001-floor-numbering-and-splash-text`  
**Status**: Draft  
**GDD Sources**: Ch. 2.4, 8.2

## User Stories & Acceptance

### US1 — Know where the run is (P1)
Each floor transition presents the correct floor number and short splash text before gameplay resumes.

**Independent test**: request each floor, including invalid IDs, and compare displayed content and timing.

- Given Floor 52/51/50 loads, then its number and approved text are shown exactly once.
- Given a transition has no valid floor mapping, then it fails visibly rather than showing another floor's text.

## Functional Requirements

- FR-001: Floor IDs and display text MUST come from one ordered progression table.
- FR-002: Splash presentation MUST not mutate gameplay state or skip a transition.
- FR-003: Text MUST be readable, localized-ready, and dismissible only under the configured rule.

## Edge Cases

Reload, duplicate request, rapid scene load, and unsupported floor MUST be deterministic.

## Success Criteria

- SC-001: Every supported floor maps to exactly one correct presentation.
- SC-002: Invalid IDs never display a valid floor's text.

## Scope

Number/text presentation and mapping only.
