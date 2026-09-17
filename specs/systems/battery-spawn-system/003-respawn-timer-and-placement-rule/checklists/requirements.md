# Specification Quality Checklist: Respawn Timer and Placement Rule

**Purpose**: Validate that `spec.md` for `003-respawn-timer-and-placement-rule` is complete,
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

- [x] CHK001 No implementation details leak into `spec.md` (no class internals, method
  signatures, or Unity API calls in the User Scenarios/FR sections — those live in `tasks.md`
  instead; the Key Entities section names classes only as the shared contract for `tasks.md`, not
  as an implementation spec)
- [x] CHK002 Focused on the observable timer/placement behavior (when a spawn decision fires and
  where) rather than how the timer or selection logic is coded internally
- [x] CHK003 Written for a reviewer who understands the GDD but not the codebase
- [x] CHK004 All mandatory sections (User Scenarios & Testing, Requirements, Success Criteria) are
  present and non-empty

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain in `spec.md` (the one open design question
  — "must the system wait, or is there a fallback?" for the all-in-view edge case — is resolved
  in-line under User Story 3's explicit **Decision** subsection, not left as an open marker)
- [x] CHK006 Every functional requirement (FR-001…FR-011) is testable as written (each maps to at
  least one acceptance scenario or edge case)
- [x] CHK007 Success criteria (SC-001…SC-005) are measurable and technology-agnostic — none name a
  specific class, test framework assertion, or Unity API
- [x] CHK008 Success criteria describe outcomes only (spawn timing, in-view exclusion rate,
  randomness distribution, decoupling from the pickup consumer), not implementation approach
- [x] CHK009 All three user stories have an assigned priority (P1/P2/P3) and an independent test
- [x] CHK010 Edge cases explicitly cover: every empty spawn point in view at elapse, zero empty
  spawn points despite being below cap, multiple simultaneously valid candidates (randomness
  requirement), eligibility thrashing near the only candidate, Floor 52 never running this system,
  and the whole category being toggled off (Ch. 18.3)
- [x] CHK011 Scope is clearly bounded: this spec is explicitly the orchestrator only — it does not
  own spawn point data (`001`'s job) or the cap check (`002`'s job), and does not decide per-floor
  duration/cap *wiring* (deferred to each floor scene's own spec) — see Scope Note and Assumptions
- [x] CHK012 Dependencies (`001`'s registry, `002`'s cap policy,
  `flashlight-and-battery/003-eased-radius-transitions`'s live view-radius value) and the one
  explicit non-dependency (`flashlight-and-battery/007-battery-pickup-and-spare-slot`'s pickup/
  install logic must have zero dependency on this spec, FR-010/SC-004) are all explicitly
  identified

## Feature Readiness

- [x] CHK013 All functional requirements have clear acceptance criteria traceable to a user
  story's acceptance scenario or an edge case
- [x] CHK014 User scenarios cover the primary flow (US1: a battery eventually respawns), the
  GDD-mandated placement constraint (US2: never in the player's view), and the edge-case
  robustness flow (US3: every candidate currently in view — the system waits, it never forces an
  in-view spawn)
- [x] CHK015 The cut-candidate #2 toggle-ability requirement (GDD Ch. 18.3) is stated as an
  explicit FR/SC pair (FR-010/SC-004), naming the specific consumer
  (`flashlight-and-battery/007-battery-pickup-and-spare-slot`) that must have zero dependency on
  this spec's classes
- [x] CHK016 Floor 52's permanent non-use of this system (distinct from, but structurally
  identical to, the Ch. 18.3 cut) is stated as its own FR (FR-009) and Assumption, so both cases
  share one code path rather than needing separate special-casing
- [x] CHK017 No contradiction between this spec and the ROADMAP's dependency row for
  `battery-spawn-system/003-respawn-timer-and-placement-rule` (depends on `001` and `002`, is the
  final feature in the category, and feeds the floor-51/floor-50 scene specs' battery-spawn-point
  placement)

## Notes

- **Geometric simplicity is deliberate, not an oversight**: FR-004 and User Story 2's Acceptance
  Scenario 3 both call out that "in view" reduces to a plain distance check because the GDD (Ch.
  12) specifies a single omnidirectional point-light flashlight, not a directional beam. Reviewers
  should confirm this reasoning still holds if the flashlight's design ever changes to a directed
  beam or frustum — at that point this spec's FR-004 would need a genuine amendment, not a
  reinterpretation.
- **The "wait, never force an in-view spawn" decision (User Story 3) is the single most
  safety-critical rule in this spec** — it is what keeps the GDD's absolute "Battery tidak pernah
  muncul di depan mata player" rule intact under every edge case, including ones that look like
  they'd otherwise justify a fallback. Reviewers should treat any future amendment proposing an
  in-view fallback spawn as a breaking change requiring the `Relationship to <spec>` amendment
  pattern (constitution Principle V), not a routine tweak.
- **Cut-switch design carries over from `001`/`002`**: FR-009/FR-010 and their Assumptions treat
  "no config for a floor" and "the whole category is cut" as the same underlying off-switch,
  consistent with how `001` and `002` already handle an unregistered/unconfigured floor as a
  normal, non-error state. Reviewers should confirm this spec did not introduce a Floor-52-specific
  code path that the other two specs avoided.
