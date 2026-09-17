# Specification Quality Checklist: Key Pickup & Inventory

**Purpose**: Validate that `spec.md` for `001-key-pickup-and-inventory` is complete, unambiguous,
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
  or Unity API calls in the User Scenarios/FR sections — those belong in `tasks.md`)
- [x] CHK002 Focused on the pickup/inventory system's observable behavior (what the player
  experiences and what other systems can query) rather than how it is coded internally
- [x] CHK003 Written for a reviewer who understands the GDD but not the codebase
- [x] CHK004 All mandatory sections (User Scenarios & Testing, Requirements, Success Criteria)
  are present and non-empty

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain in `spec.md`
- [x] CHK006 Every functional requirement (FR-001…FR-009) is testable as written (each maps to
  at least one acceptance scenario or edge case)
- [x] CHK007 Success criteria (SC-001…SC-004) are measurable and technology-agnostic — none name
  a specific class, test framework assertion, or Unity API
- [x] CHK008 Success criteria describe outcomes only (pickup correctness, no-duplicate
  guarantees, first-time-tester legibility, structural no-drop guarantee), not implementation
  approach
- [x] CHK009 All three user stories have an assigned priority (P1/P2/P3) and an independent test
- [x] CHK010 Edge cases explicitly cover: unbounded per-floor key count, a key competing with
  another interactable for nearest-object tie-break, pickup during a chase, a duplicated/replayed
  pickup input, and the floor-reset boundary
- [x] CHK011 Scope is clearly bounded: per-floor key counts (Floor 51 = 1, Floor 50 = 3) are
  explicitly out of scope, deferred to the corresponding floor scene specs (see Assumptions)
- [x] CHK012 Dependencies (`interaction-and-highlight/002-context-sensitive-action-button`) and
  consumers (`keys-and-doors/002`, `lives-and-fail-state/002`, Floor 51/50 scene specs) are
  explicitly identified in the Dependencies section

## Feature Readiness

- [x] CHK013 All functional requirements have clear acceptance criteria traceable to a user
  story's acceptance scenario or an edge case
- [x] CHK014 User scenarios cover the primary flow (US1: pickup via the action button) and
  secondary flows (US2: identity-based multi-key tracking, US3: no-drop/no-double-pickup guard)
- [x] CHK015 The "no complex inventory system" scope lock (GDD 18.2) is reflected as an explicit
  requirement (FR-005) rather than left implicit
- [x] CHK016 No contradiction between this spec and the ROADMAP's dependency row for
  `keys-and-doors/001-key-pickup-and-inventory`

## Notes

- **Identity-over-count design**: FR-004's requirement that the inventory track keys by identity
  (not count) is explicitly justified by spec 002's future need to ask "does the player hold the
  key for door X" — reviewers should confirm `tasks.md`'s `KeyInventory` shape
  (`HoldsKeyForDoor`/`TryGetKeyForDoor`) actually satisfies that forward-looking requirement
  rather than only satisfying User Story 1 in isolation.
- **No-drop guarantee is structural, not just behavioral**: per FR-005/SC-004, `tasks.md`
  deliberately ships `KeyInventory` in this feature with no removal method at all (not even an
  internal one) — the only removal path (`ConsumeKeyForDoor`) is added later by spec 002. This
  makes the no-drop guarantee provable by absence, not just by a passing test.
- **Testability**: every FR in this spec is expressible as plain input/output on a plain C# class
  (pick up a key, query the inventory) — 100% EditMode-testable per constitution Principle IV. The
  one exception (the highlight/no-floating-icon visual, FR-002) is owned and tested by
  `interaction-and-highlight/003-interactable-highlight-halo`, not re-tested here.
