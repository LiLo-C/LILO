# Specification Quality Checklist: Distance-Based Detection Check

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

- [x] CHK001 No implementation leaks into `spec.md`'s requirements themselves — `Vector3`,
  `MonoBehaviour`, `Physics.Raycast`, and NavMesh are named only where a requirement exists
  specifically to *forbid* that dependency (FR-007, FR-008, FR-014) or in Key
  Entities/Assumptions, never stated as the "what" of a user story.
- [x] CHK002 Focused on observable behavior (detected/not-detected outcomes, the exact boundary
  distance, source-position correctness) and measurable outcomes, not on class internals.
- [x] CHK003 Written for a reader who knows the GDD but not the eventual code — every FR traces
  back to a specific GDD Ch. 7.2 clause or an explicitly reasoned scope decision (Assumptions).
- [x] CHK004 All mandatory sections present: User Scenarios & Testing, Requirements (FR + Key
  Entities), Success Criteria, Assumptions, Dependencies, Related.

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain — the one inherited open item
  (`noiseBaseRadius`, GDD Ch. 21) is resolved by requiring every FR/SC to hold for any
  non-negative radius value (FR-006), not left as a blocking question.
- [x] CHK006 Every FR is testable via a direct `(monsterPosition, playerPosition, noiseRadius)`
  call with no live Unity scene required (FR-014 exists specifically to guarantee this).
- [x] CHK007 Success criteria (SC-001…SC-007) are measurable and technology-agnostic (trial
  counts, percentage coverage, "zero measurable error") — none mention a specific class or API
  name.
- [x] CHK008 Every user story has an explicit priority (P1–P2) and an Independent Test naming a
  concrete direct-call harness, not a real floor or scene.
- [x] CHK009 Edge cases explicitly cover the exact-equality boundary, the zero-radius/
  zero-distance case, a corrupted negative radius, wall-occlusion, all four light states, and
  rapid per-tick re-evaluation — not just the happy path of a single detection.
- [x] CHK010 Scope is explicitly bounded in both directions this feature must not creep into:
  occlusion/line-of-sight math is explicitly excluded (FR-007/FR-008) and light-state coupling is
  explicitly excluded (FR-009/FR-010), each tied to a named GDD chapter so neither reads as an
  accidental omission.
- [x] CHK011 Dependencies and assumptions are identified: the noise-and-detection/001 radius
  input, the monster-ai/001-owned `MonsterDetectionSignal` type and its actual file location, and
  the externally-supplied position inputs are all named by path in Dependencies/Assumptions.

## Feature Readiness

- [x] CHK012 All functional requirements (FR-001…FR-015) have at least one corresponding
  acceptance scenario or edge case in User Scenarios & Testing.
- [x] CHK013 User stories are independently testable and delivery-ordered (P1 positive detection
  → P1 strict boundary → P2 occlusion-free guarantee → P2 light-independence guarantee) matching
  the stated "why this priority" reasoning.
- [x] CHK014 Success criteria (SC-001…SC-007) cover every user story, including the two
  scope-boundary stories (SC-005 for light-independence; US3's occlusion-freedom is covered
  structurally per FR-007 rather than a numeric SC, which is appropriate since there is no
  numeric outcome to measure — only an absent input to prove absent).
- [x] CHK015 No speculative/unrequested capability is specified (no partial/graduated detection
  by light level, no per-monster hearing variance, no multi-monster aggregation) — consistent
  with constitution Principle II (Simplicity/YAGNI).

## Notes

- **`noiseBaseRadius` open item (GDD Ch. 21)**: this spec does NOT lock a final meters value for
  the noise radius, and does not need to — FR-002/FR-006 require every requirement to hold for
  any non-negative value produced by noise-and-detection/001's `NoiseEmitter.CurrentNoiseRadius`,
  and the Assumptions section states explicitly that this feature is already correct for whatever
  final number that spec eventually locks, with zero code changes required here. Reviewers should
  treat any implementation that hardcodes a meters literal inside `DetectionCheck` as a spec
  violation.
- **`MonsterDetectionSignal` type reuse**: monster-ai/001's own spec text (its FR-003) names this
  spec, 002, as the nominal "producer type" definer — but that struct was already implemented at
  `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs` ahead of this spec being written.
  This spec resolves that ordering nuance explicitly in Assumptions and Dependencies: 002 reuses
  the existing type rather than declaring a second, competing one, and the ROADMAP build-order
  (monster-ai/001 depends on noise-and-detection/002, not the reverse) is preserved.
- Reviewed against constitution Principle II (no raycast/light-state input added speculatively),
  Principle III (plain C# under `Assets/Scripts/Systems/Detection/`, no `MonoBehaviour`
  dependency), and Principle IV (EditMode test coverage for every functional requirement,
  including the two structural "this input does not exist" assertions in US3/US4).
