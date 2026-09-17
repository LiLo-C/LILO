# Specification Quality Checklist: Monster Per-Floor Tuning Profile

**Purpose**: Validate `spec.md`'s completeness and quality before implementation planning begins.
**Created**: 2026-09-17
**Feature**: [spec.md](../spec.md)

**Note**: This custom checklist is generated based on feature context and requirements.
**Review Ownership**: This checklist is a reviewer-owned requirements-quality review artifact.
Mark an item `[x]` only when the reviewer determines the requirements-quality criterion is
satisfied.
**Marker Semantics**: `[x]` means the criterion has been reviewed and satisfied for requirements
quality. It does not mean implementation work is complete.

## Content Quality

- [x] CHK001 No implementation leaks into `spec.md`'s requirements themselves — `GameConfig`
  field names, `OnValidate`, and the editor-hook mechanism are confined to FR-007/Key Entities as
  the *where* the rule must run, not stated as the "what" of any user story.
- [x] CHK002 Focused on observable behavior (which numbers resolve per floor, whether the hard
  rule catches a violation, whether Floor 52 fails loudly) and measurable outcomes, not on class
  internals.
- [x] CHK003 Written for a reader who knows the GDD but not the eventual code — every locked
  number traces back to GDD Ch. 6.2/17.4 verbatim, and the hard rule quotes GDD Ch. 6.2's bolded
  Indonesian rule directly.
- [x] CHK004 All mandatory sections present: User Scenarios & Testing, Requirements (FR + Key
  Entities), Success Criteria, Assumptions, Related.

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain — every one of the ten locked numbers (five
  values × two floors) is given explicitly and without qualification in the GDD; nothing here is
  a feel-based/TBD value.
- [x] CHK006 Every FR is testable via EditMode: resolved-value equality checks (FR-003/FR-004),
  the hard-rule validator (FR-005/FR-006), and the explicit-failure lookup (FR-011) all avoid any
  live-scene dependency.
- [x] CHK007 Success criteria (SC-001…SC-005) are measurable and technology-agnostic (exact
  resolved values, 100% hard-rule pass rate, zero-code retuning, explicit-failure guarantee) —
  none mention a specific class or API name.
- [x] CHK008 Every user story has an explicit priority (P1–P2) and an Independent Test naming a
  debug test arena or EditMode test, consistent with the project's "independent test = arena, not
  a real floor" convention.
- [x] CHK009 Edge cases explicitly cover zero/negative durations, exact-equality at the hard-rule
  boundary, cross-field revalidation when either side of the rule changes independently, lookup
  for an undefined/typo'd floor id, and the explicit non-goal of a 4th floor.
- [x] CHK010 Scope is explicitly bounded: this spec does not redefine `GameConfig`'s overall
  schema (owned by `shared-config-and-state/001`), does not choose how/when the profile is
  re-read per tick (owned by spec 001's adapter), and does not invent a floor-identifier type —
  each is named with its owning spec.
- [x] CHK011 Dependencies and assumptions are identified: `monster-ai/001`'s consumption of
  `MonsterStateTuning`, `shared-config-and-state/001`'s ownership of `GameConfig`'s base schema
  and floor-identifier type, and the already-locked `sprintMultiplier` (GDD Ch. 17.1) the hard
  rule depends on are all named by path in Related/Assumptions.

## Feature Readiness

- [x] CHK012 All functional requirements (FR-001…FR-014) have at least one corresponding
  acceptance scenario or edge case in User Scenarios & Testing.
- [x] CHK013 User stories are independently testable and delivery-ordered (P1 per-floor variation
  and P1 hard-rule safety, both foundational and equally weighted, then P2 Floor 52 guard rail)
  matching the stated "why this priority" reasoning.
- [x] CHK014 Success criteria (SC-001…SC-005) cover every user story, including the Floor 52
  explicit-failure story (US3 → SC-005).
- [x] CHK015 No speculative/unrequested capability is specified (no runtime difficulty scaling
  beyond the two locked floors, no designer-facing difficulty slider, no interpolation between
  Floor 51 and Floor 50 values) — consistent with constitution Principle II (Simplicity/YAGNI).

## Notes

- Unlike sibling spec 001 (which had one genuine open interpretive question — the "jarak dekat"
  qualifier), this spec has no equivalent ambiguity: every number it locks is stated verbatim and
  unconditionally in GDD Ch. 6.2/17.4, so Assumptions here documents scope boundaries and
  dependency ordering rather than resolving an ambiguity.
- The hard-rule check (FR-005/FR-006) is the one place this spec must stay vigilant against
  "silent regression" — reviewers should specifically confirm the shipped test suite (not just
  editor-time `OnValidate`) exercises it, since CI has no Unity Inspector to surface a validation
  error in.
