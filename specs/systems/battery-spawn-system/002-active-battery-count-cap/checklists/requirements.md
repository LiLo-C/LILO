# Specification Quality Checklist: Active Battery Count Cap

**Purpose**: Validate that `spec.md` for `002-active-battery-count-cap` is complete, unambiguous,
and ready for planning/implementation.
**Created**: 2026-09-17
**Feature**: [spec.md](../spec.md)

**Note**: This custom checklist is generated per the `/speckit-checklist` convention based on
this feature's context and requirements.
**Review Ownership**: This checklist is a reviewer-owned requirements-quality review artifact.
Mark an item `[x]` only when the reviewer determines the requirements-quality criterion is
satisfied.
**Marker Semantics**: `[x]` means the criterion has been reviewed and satisfied for requirements
quality. It does not mean implementation work is complete.

## Content Quality

- [x] CHK001 No implementation details leak into `spec.md` (no class names, method signatures,
  or `Dictionary`/data-structure choices in the User Scenarios/FR sections)
- [x] CHK002 Focused on the observable question this system answers ("is this floor at or above
  its cap") rather than how counting is implemented internally
- [x] CHK003 Written for a reviewer who understands the GDD but not the codebase
- [x] CHK004 All mandatory sections (User Scenarios & Testing, Requirements, Success Criteria)
  are present and non-empty

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain in `spec.md`
- [x] CHK006 Every functional requirement (FR-001…FR-010) is testable as written (each maps to
  at least one acceptance scenario or edge case)
- [x] CHK007 Success criteria (SC-001…SC-004) are measurable and technology-agnostic — none name
  a specific class or Unity API
- [x] CHK008 Success criteria describe outcomes only, not implementation approach
- [x] CHK009 All three user stories have an assigned priority (P1/P1/P2) and an independent test
- [x] CHK010 Edge cases explicitly cover: count exceeding the cap, a zero/negative configured
  maximum, an unconfigured floor, and same-frame pickup/check ordering
- [x] CHK011 The GDD's specific numbers (Floor 51 = 2, Floor 50 = 1) are cited as `GameConfig`
  defaults per FR-004, never hardcoded into this system's own logic (FR-004, FR-009)
- [x] CHK012 Scope is clearly bounded: this spec explicitly excludes per-floor value *wiring*
  (which floor scene reads which field) and excludes spawn timing/placement (both deferred, see
  Scope Note and Assumptions)
- [x] CHK013 Dependencies (`Battery` lifecycle signal from
  `flashlight-and-battery/007-battery-pickup-and-spare-slot`; consumed by `003`) are explicitly
  identified

## Feature Readiness

- [x] CHK014 All functional requirements have clear acceptance criteria traceable to a user
  story's acceptance scenario or an edge case
- [x] CHK015 User scenarios cover the primary flow (US1: accurate count, US2: cap check) and a
  config-source-of-truth flow (US3)
- [x] CHK016 The cut-candidate #2 toggle-ability requirement (GDD Ch. 18.3) is stated as an
  explicit Assumption: this system must never be load-bearing for static battery placement to
  work
- [x] CHK017 No contradiction between this spec and the ROADMAP's dependency row for
  `battery-spawn-system/002-active-battery-count-cap` (depends on `001`, feeds `003`)

## Notes

- **Cut-switch design**: this spec's fail-safe defaults (unconfigured floor → "at cap", not
  "unlimited") mean a floor that never calls into this system (because it uses static placement)
  is never at risk of this system's absence being misread as "spawn without limit." Reviewers
  should confirm this asymmetry (fail toward scarcity, not overflow) is preserved if the spec is
  ever amended.
- **Testability of the "in-view" rule**: not applicable to this spec — the in-view
  spawn-avoidance rule belongs to `003-respawn-timer-and-placement-rule`. This spec's own
  testability story is that 100% of its logic (counting, cap comparison, config lookup) is plain
  arithmetic on plain C# state, so every FR here is EditMode-testable with no manual/on-device
  validation needed.
