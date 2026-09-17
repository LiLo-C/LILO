# Specification Quality Checklist: Per-Action Noise Emission

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

- [x] CHK001 No implementation leaks into `spec.md`'s requirements themselves —
  `UnityEngine.MonoBehaviour`/`Component`/`GameConfig` are named only where a requirement exists
  specifically to require testability or a single config source (FR-011, FR-013), never stated
  as the "what" of a user story.
- [x] CHK002 Focused on observable behavior (which multiplier is active, pulse timing, override
  and combination outcomes) and measurable outcomes, not on class internals.
- [x] CHK003 Written for a reader who knows the GDD but not the eventual code — every FR traces
  back to a specific GDD Ch. 7.1/17.3 table row or an explicitly reasoned design decision
  (Assumptions).
- [x] CHK004 All mandatory sections present: User Scenarios & Testing, Requirements (FR + Key
  Entities), Success Criteria, Definition of Done, Assumptions, Dependencies, Related.

## Requirement Completeness

- [x] CHK005 No `[NEEDS CLARIFICATION]` markers remain — the one genuinely open GDD item
  (`noiseBaseRadius`, Ch. 21) is resolved with a dedicated "Definition of Done — Blocking Open
  Item" section requiring every FR/SC to hold for any value of that field, rather than left as a
  blocking question.
- [x] CHK006 Every FR is testable via a scripted `GameConfig` + movement-state/pulse-trigger
  sequence with no live Unity scene required (FR-013 exists specifically to guarantee this).
- [x] CHK007 Success criteria (SC-001…SC-008) are measurable and technology-agnostic (trial
  counts, deviation checks, percentage coverage) — none mention a specific class or API name.
- [x] CHK008 Every user story has an explicit priority (P1–P3) and an Independent Test naming a
  concrete direct-construction harness (a `NoiseEmitter` built and fed inputs directly), not a
  real floor or scene.
- [x] CHK009 Edge cases explicitly cover simultaneous elevated sources resolving to a single
  highest multiplier, hiding cancelling an in-progress pulse outright, rapid retrigger of the
  same pulse kind, the unlocked `noiseBaseRadius` placeholder, and defensive clamping of an
  invalid/negative config value — not just the happy path of each individual action.
- [x] CHK010 Scope is explicitly bounded: distance/detection math against a monster is explicitly
  excluded and named as noise-and-detection/002's job (Dependencies "Consumed by"), and visual
  rendering of the noise radius is explicitly excluded (FR-014, citing GDD 7.2's "tidak pernah
  digambar di layar") — scope creep in either direction is pre-empted.
- [x] CHK011 Dependencies and assumptions are identified: movement-and-camera/001's movement-state
  input, flashlight-and-battery/008's battery-install trigger, and hiding/001's hiding-flag input
  are all named by path in both the Functional Requirements text and the Dependencies section.

## Feature Readiness

- [x] CHK012 All functional requirements (FR-001…FR-015) have at least one corresponding
  acceptance scenario or edge case in User Scenarios & Testing.
- [x] CHK013 User stories are independently testable and delivery-ordered (P1 baseline movement →
  P1 hiding override → P2 interact pulse → P3 battery-swap pulse) matching the stated "why this
  priority" reasoning.
- [x] CHK014 Success criteria (SC-001…SC-008) cover every user story, including the config-driven/
  no-hardcode guarantee (SC-007) and the full-suite-green gate (SC-008).
- [x] CHK015 No speculative/unrequested capability is specified (no noise sources beyond GDD
  7.1's five documented rows, no visual radius rendering, no per-monster hearing variance) —
  consistent with constitution Principle II (Simplicity/YAGNI).

## Notes

- **`noiseBaseRadius` open item (GDD Ch. 21)**: explicitly NOT locked by this spec.
  `spec.md`'s "Definition of Done — Blocking Open Item" section requires it ship as a
  clearly-labeled `GameConfig` placeholder, and every FR/SC is written to hold for any value of
  that field. Reviewers should treat any PR that hardcodes a final meters value for
  `noiseBaseRadius` as a spec violation, not an implementation improvement — this feature is not
  considered fully done, even with all tests green, until that value is locked via on-device
  playtesting (per the GDD's own "harus selesai sebelum Fase 2" framing).
- `noisePulseDuration` (FR-012) is a second, similarly unlocked feel-based value this spec itself
  introduces (it has no GDD-sourced number) — flagged here so it receives the same reviewer
  scrutiny as `noiseBaseRadius` and is not mistaken for a locked constant.
- Reviewed against constitution Principle III (plain C# under `Assets/Scripts/Systems/Noise/`,
  no `MonoBehaviour` dependency) and Principle IV (EditMode test coverage for every functional
  requirement) — both are satisfied by FR-013 and the Success Criteria, and by this feature's
  existing `tasks.md`, which pairs every implementation task with a failing-first test task.
