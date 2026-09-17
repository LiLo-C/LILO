# Specification Quality Checklist: Per-Floor Battery Spawn Point Registry

**Purpose**: Validate that `spec.md` for `001-per-floor-spawn-point-registry` is complete,
unambiguous, and ready for planning/implementation.
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
  or Unity API calls in the User Scenarios/FR sections — those live in `tasks.md` instead)
- [x] CHK002 Focused on the data contract's observable behavior (what the registry returns/does)
  rather than how it is coded internally
- [x] CHK003 Written for a reviewer who understands the GDD but not the codebase
- [x] CHK004 All mandatory sections (User Scenarios & Testing, Requirements, Success Criteria)
  are present and non-empty

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain in `spec.md`
- [x] CHK006 Every functional requirement (FR-001…FR-011) is testable as written (each maps to
  at least one acceptance scenario or edge case)
- [x] CHK007 Success criteria (SC-001…SC-004) are measurable and technology-agnostic — none name
  a specific class, test framework assertion, or Unity API
- [x] CHK008 Success criteria describe outcomes only (what the registry does), not
  implementation approach
- [x] CHK009 All three user stories have an assigned priority (P1/P2/P3) and an independent test
- [x] CHK010 Edge cases explicitly cover: a floor with zero spawn points, duplicate identifiers,
  querying an unregistered floor, and stale-data leakage across floor loads
- [x] CHK011 Scope is clearly bounded: this spec explicitly excludes per-floor spawn point
  coordinates/counts (deferred to each floor scene's own spec) — see Scope Note and FR-010
- [x] CHK012 Dependencies (this feature depends on
  `flashlight-and-battery/007-battery-pickup-and-spare-slot`; is depended on by `002` and `003`)
  and assumptions (shared floor identifier type ownership) are explicitly identified

## Feature Readiness

- [x] CHK013 All functional requirements have clear acceptance criteria traceable to a user
  story's acceptance scenario or an edge case
- [x] CHK014 User scenarios cover the primary flow (US1: list candidates) and secondary flows
  (US2: occupancy tracking, US3: duplicate-authoring guard rail)
- [x] CHK015 The cut-candidate #2 toggle-ability requirement (GDD Ch. 18.3) is stated as an
  explicit Assumption, with the specific consumer (`Battery` pickup/install logic) named as not
  allowed to depend on this registry existing
- [x] CHK016 No contradiction between this spec and the ROADMAP's dependency row for
  `battery-spawn-system/001-per-floor-spawn-point-registry`

## Notes

- **Cut-switch design**: the registry is designed so that "not populated" and "not used" are the
  same, ordinary, zero-error state (see Edge Cases and FR-011). This is deliberate — it means the
  Ch. 18.3 cut (respawn → static placement) never requires touching this registry's code, only
  requires floor scenes to stop feeding it spawn points and to place static `Battery` instances
  instead. Reviewers should confirm no FR was written in a way that *requires* a non-empty
  registry to function correctly.
- **Testability of the data contract**: every FR in this spec is expressible as plain
  input/output on a plain C# class (register a point, query a list, mark occupancy) — there is no
  "in view" or timing logic in this spec (that is `003`'s job), so 100% of this feature's
  requirements are EditMode-testable per constitution Principle IV with no manual/on-device
  validation needed.
