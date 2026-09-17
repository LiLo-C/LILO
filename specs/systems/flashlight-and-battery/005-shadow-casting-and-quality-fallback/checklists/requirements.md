# Specification Quality Checklist: Shadow Casting & Quality Fallback

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-09-17
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Notes

- This feature's core claim ("a shadow visibly appears and tracks the light") has no pure-C#
  surface of its own — it is a rendering behavior of a single Unity `Light`'s built-in shadow
  system, so User Story 1's verification is manual/on-device per constitution Principle IV's
  explicit exception, not an EditMode gap. What IS pure and tested is the config-to-applied-value
  *resolution* logic (User Stories 2/3): whether `shadowsEnabled` is read live, and whether an
  out-of-range `shadowResolution` is safely clamped/snapped rather than passed through raw.
- FR-004's `shadowResolution` default (`512`) is carried over from the project's prior
  native-engine on-device tuning pass per ROADMAP §0, explicitly flagged "to re-confirm on-device
  in Unity/URP" rather than as a locked number — the historical over-expose failure it warns
  against was observed on a different render pipeline, so the re-verification (tasks.md T018/
  T019) is load-bearing, not a formality.
- User Story 2's fallback (shadows off) is treated as non-optional insurance per GDD Ch. 11.2's
  own wording ("WAJIB bisa diturunkan lewat config"), so FR-002/FR-003 make it a first-class,
  independently tested requirement rather than a nice-to-have toggle.
- Reviewed against constitution Principle III (the tier-snapping/enabled-resolution logic is
  isolated in `ShadowQualitySystem`, a plain C# class with no `MonoBehaviour`/scene dependency;
  the `Light.shadows`/URP shadow-resolution assignment lives in a separate thin
  `MonoBehaviour` adapter) and Principle IV (every pure-logic branch has an EditMode test; the
  rendering/performance claims are the documented manual-validation exception).
