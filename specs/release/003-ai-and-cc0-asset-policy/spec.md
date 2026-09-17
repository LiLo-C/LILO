# Feature Specification: AI and CC0 Asset Policy

**Feature Branch**: `003-ai-and-cc0-asset-policy`  
**Status**: Draft  
**GDD Sources**: Ch. 19.4

## User Stories & Acceptance

### US1 — Ship assets with traceable rights (P1)
The team can prove every shipped visual, audio, and generated asset has an allowed license and a recorded source.

**Independent test**: audit the build manifest against the asset register and attempt to approve an unlicensed fixture.

## Functional Requirements

- FR-001: Every shipped third-party or AI-assisted asset MUST have source, license, creator/tool, date, and permitted-use record.
- FR-002: CC0/public-domain claims MUST be independently checked where practical; incompatible licenses MUST block release.
- FR-003: AI output MUST be reviewed for unwanted resemblance, unsafe content, and consistency with the art direction.
- FR-004: Asset removals/replacements MUST update the register and build manifest.

## Edge Cases

Unclear license, modified source, bundled package asset, generated variation, and missing metadata MUST be treated as release blockers until resolved.

## Success Criteria

- SC-001: 100% of shipped non-original assets map to an approved register entry.
- SC-002: A clean audit contains zero unresolved license blockers.

## Scope

Release provenance and review process; it does not prescribe a particular generation tool or art style.
