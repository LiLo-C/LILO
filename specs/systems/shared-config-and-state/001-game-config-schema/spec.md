# Feature Specification: Shared GameConfig Schema

**Feature Branch**: `001-game-config-schema`  
**Status**: Draft  
**GDD Sources**: Ch. 17; constitution Principle III

## User Stories & Acceptance

### US1 — One authoritative tuning source (P1)
As a designer, I can tune gameplay values in one `GameConfig` asset so every scene uses the same values.

**Independent test**: load the asset, change one documented value, and verify all consuming systems read the changed value after reload.

- Given any tunable gameplay value, when it is needed at runtime, then it is read from the shared config asset rather than a second constant or scene-local copy.
- Given the asset is missing or invalid, when a build starts, then startup reports a clear validation failure and does not silently invent defaults.

### US2 — Complete, validated schema (P1)
The schema covers movement, light/battery, monster, noise, objectives, lives, camera, audio and UI values named by the roadmap.

**Independent test**: enumerate the schema against the GDD Ch. 17 table and run validation with boundary-invalid values.

## Functional Requirements

- FR-001: Exactly one `GameConfig` type and one production asset at `Assets/Config/GameConfig.asset` MUST exist.
- FR-002: Fields MUST use the fixed names and units from GDD Ch. 17; each field MUST have a safe range validation rule.
- FR-003: Validation MUST identify the field, invalid value, and correction rule; it MUST reject missing required references.
- FR-004: Runtime systems MUST receive the config explicitly or through the shared manager; no system may own a competing tuning source.
- FR-005: Defaults MUST preserve the GDD production-lock values and mark feel-based values for later playtest retuning.

## Edge Cases

Duplicate assets, null config, zero/negative durations, dead-zone ordering errors, and values changed while a run is active MUST be handled deterministically (reject at validation or apply only at the next run).

## Success Criteria

- SC-001: A schema audit finds one type and one asset, with zero duplicate tuning fields.
- SC-002: Every GDD Ch. 17 gameplay value maps to a named field and validation rule.
- SC-003: Invalid-fixture tests produce actionable failures without silently coercing values.

## Scope

This feature defines the shared schema and validation contract. It does not implement gameplay systems, editor UI beyond normal field validation, or playtest tuning.
