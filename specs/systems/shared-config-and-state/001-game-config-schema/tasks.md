---
description: "Task list for 001-game-config-schema"
---

# Tasks: Shared GameConfig Schema

**Input**: Design documents from `specs/systems/shared-config-and-state/001-game-config-schema/spec.md`

**Prerequisites**: spec.md (this folder). No `plan.md`/`research.md`/`data-model.md` exist for
this feature — GDD Ch. 17's table is itself the data model, restated field-by-field below.

**Tests**: EditMode tests are mandatory per constitution Principle IV for every pure-logic piece
below — this feature is 100% pure C# data/validation, so it is 100% covered. This spec is
foundational (every other systems spec reads `GameConfig`), so every field gets its own addition
task and its own validation-rule task/test — coarse "add all fields" tasks are deliberately
avoided.

**Organization**: Tasks are grouped by user story (US1/US2 from `spec.md`), then by GDD Ch. 17
subsection within US2, so each field is independently addable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, or same file but non-overlapping fields/tests)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Create the folders `Assets/Scripts/Config/` and `Assets/Tests/EditMode/Config/` (no
  content yet — establishes the location every task below writes into).
- [ ] T002 Define the canonical shared floor identifier `Assets/Scripts/Config/FloorId.cs`: enum
  `FloorId { Floor52, Floor51, Floor50 }`, plain C# (no `MonoBehaviour`). This is the single
  definition every other spec (`battery-spawn-system/001`, `monster-ai/002`, etc.) is written to
  assume exists here — if a placeholder already exists elsewhere per those specs' own notes,
  this task supersedes it as the real one.

## Phase 2: Foundational — Base Type, Asset, and Validation Plumbing (blocks all user stories)

**Purpose**: The `GameConfig` `ScriptableObject` skeleton and its validation-result shape are
extended by every field task below; they must exist first.

- [ ] T003 [P] Create `Assets/Scripts/Config/GameConfig.cs`: empty `ScriptableObject` skeleton
  with `[CreateAssetMenu]`, no fields yet — the sole config type per FR-001. No
  `MonoBehaviour`/`Component` reference (constitution Principle III).
- [ ] T004 [P] Define `Assets/Scripts/Config/ConfigValidationFailure.cs`: plain C# type carrying
  the field name, the invalid value, and the correction rule/message (FR-003 — validation "MUST
  identify the field, invalid value, and correction rule").
- [ ] T005 Add a `GameConfig.Validate()` method (returns a list/collection of
  `ConfigValidationFailure`) in `GameConfig.cs` — the single aggregator every per-field validation
  rule below plugs into; empty/no-op until fields exist (depends on T003, T004).
- [ ] T006 Create the production asset `Assets/Config/GameConfig.asset` from the (currently empty)
  `GameConfig` type (FR-001's "one production asset").

**Checkpoint**: `GameConfig` compiles, has one asset, and has an aggregator ready to receive
per-field validation rules.

---

## Phase 3: User Story 1 - One Authoritative Tuning Source (Priority: P1) 🎯 MVP

**Goal**: Exactly one type, one asset; startup fails loudly and specifically on a missing/invalid
config instead of inventing a default.

**Independent Test**: See spec.md User Story 1 — load the asset, change one documented value,
verify all consuming systems read the changed value after reload; and confirm a missing/invalid
asset fails startup with a clear report.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T007 [P] [US1] EditMode test in `Assets/Tests/EditMode/Config/GameConfigSchemaAuditTests.cs`:
  `SchemaAudit_ExactlyOneGameConfigTypeAndAsset` — asserts a project-level audit finds exactly one
  `GameConfig` type and resolves exactly one asset at `Assets/Config/GameConfig.asset` (SC-001).
- [ ] T008 [P] [US1] EditMode test, same file: `Validate_NullConfigReference_ReportsFailureNotDefault`
  — passing a null/missing `GameConfig` reference to the startup guard produces an explicit
  validation failure, never a silently-invented default (FR-001 edge case).
- [ ] T009 [P] [US1] EditMode test, same file: `Validate_FailureIdentifiesFieldValueAndCorrection`
  — for a fixture with one deliberately invalid field, assert the returned
  `ConfigValidationFailure` names that field, its invalid value, and a correction rule (FR-003).
- [ ] T010 [P] [US1] EditMode test, same file: `ConfigChangedMidRun_AppliesOnlyAtNextRun` — mutate
  a loaded config-like fixture after a run has started, assert the currently-running consumer
  still sees the value it was constructed with (Edge Case: "values changed while a run is active
  MUST be handled deterministically... apply only at the next run").

### Implementation for User Story 1

- [ ] T011 [US1] Implement a startup validation guard (e.g. `GameConfigLoader.LoadAndValidate()`)
  in `Assets/Scripts/Config/GameConfig.cs` (or a sibling `GameConfigLoader.cs` in the same folder)
  that calls `Validate()` at Bootstrap load and fails visibly (thrown exception or equivalent
  hard stop) on any failure — no silent default invention (FR-001, FR-004). Depends on T005.
- [ ] T012 [US1] Run T007–T010 and confirm green.

**Checkpoint**: User Story 1 fully functional and independently testable — the empty schema's
loading/validation contract is proven before any field exists.

---

## Phase 4: User Story 2 - Complete, Validated Schema (Priority: P1)

**Goal**: Every GDD Ch. 17 value maps to a named `GameConfig` field with a safe-range validation
rule, enumerated section-by-section.

**Independent Test**: See spec.md User Story 2 — enumerate the schema against the GDD Ch. 17
table and run validation with boundary-invalid values.

### 17.1 Player

- [ ] T013 [P] [US2] Add `walkSpeed` (`float`, default `1.0`, base unit) to `GameConfig.cs`.
- [ ] T014 [P] [US2] Add `sprintMultiplier` (`float`, default `1.6`, × `walkSpeed`) to
  `GameConfig.cs`.
- [ ] T015 [P] [US2] Add `sprintJoystickThreshold` (`float`, unlocked — "TBD di device" per GDD
  17.1, range `0`–`1`) to `GameConfig.cs`, clearly commented as a feel value to re-confirm on
  device, not a fabricated final number (ROADMAP §0 feel-value convention).
- [ ] T016 [P] [US2] Add `interactionRadius` (`float`, unlocked — "TBD di device", must be `> 0`)
  to `GameConfig.cs`, same feel-value comment convention as T015.
- [ ] T017 [US2] Add validation rules for T013–T016 to `Validate()`: `walkSpeed > 0`,
  `sprintMultiplier > 1.0` (must be faster than walk), `sprintJoystickThreshold` in `[0, 1]`,
  `interactionRadius > 0`. Depends on T013–T016.
- [ ] T018 [P] [US2] EditMode tests in `Assets/Tests/EditMode/Config/GameConfigPlayerTests.cs`:
  one valid-fixture pass and one boundary-invalid fixture per field in T013–T016 (8 cases).

### 17.2 Light & Battery

- [ ] T019 [P] [US2] Add `batteryDuration` (`float`, default `180`, seconds real-time) to
  `GameConfig.cs`.
- [ ] T020 [P] [US2] Add `batterySlots` (`int`, default `1`, spare slots — total carried `2`) to
  `GameConfig.cs`.
- [ ] T021 [P] [US2] Add `lightStateFlickerStart` (`float`, default `0.30`, fraction of charge) to
  `GameConfig.cs`.
- [ ] T022 [P] [US2] Add `lightStateCriticalStart` (`float`, default `0.10`, fraction of charge)
  to `GameConfig.cs`.
- [ ] T023 [P] [US2] Add `compactDarknessRadius` (`float`, default `0.10`, ±10% radius adjustment
  at 0% charge — does not disable the player) to `GameConfig.cs`.
- [ ] T024 [P] [US2] Add `batteryCountFloor52Min` (`int`, default `3`) and
  `batteryCountFloor52Max` (`int`, default `5`) to `GameConfig.cs` (static count range, no
  respawn on Floor 52).
- [ ] T025 [P] [US2] Add `batteryMaxActiveFloor51` (`int`, default `2`) to `GameConfig.cs`.
- [ ] T026 [P] [US2] Add `batteryRespawnFloor51` (`float`, default `30`, seconds) to
  `GameConfig.cs`.
- [ ] T027 [P] [US2] Add `batteryMaxActiveFloor50` (`int`, default `1`) to `GameConfig.cs`.
- [ ] T028 [P] [US2] Add `batteryRespawnFloor50` (`float`, default `60`, seconds) to
  `GameConfig.cs`.
- [ ] T029 [US2] Add validation rules for T019–T028 to `Validate()`: `batteryDuration > 0`,
  `batterySlots >= 0`, `lightStateCriticalStart > 0`, `lightStateFlickerStart < 1`, **and the
  dead-zone ordering rule** `lightStateCriticalStart < lightStateFlickerStart` (Edge Case:
  "dead-zone ordering errors"), `compactDarknessRadius` in `(0, 0.5]`,
  `batteryCountFloor52Min <= batteryCountFloor52Max` and both `> 0`,
  `batteryMaxActiveFloor51 > 0`, `batteryRespawnFloor51 > 0`, `batteryMaxActiveFloor50 > 0`,
  `batteryRespawnFloor50 > 0`. Depends on T019–T028.
- [ ] T030 [P] [US2] EditMode tests in
  `Assets/Tests/EditMode/Config/GameConfigLightBatteryTests.cs`: valid-fixture pass and
  boundary-invalid fixture per field in T019–T028, plus a dedicated
  `Validate_CriticalStartAboveFlickerStart_RejectsDeadZoneOrdering` case for the ordering rule.

### 17.3 Noise

- [ ] T031 [P] [US2] Add `noiseBaseRadius` (`float`, unlocked — "TBD di device", must be `> 0`) to
  `GameConfig.cs`, same feel-value comment convention as T015/T016.
- [ ] T032 [P] [US2] Add `noiseWalk` (`float`, default `1.0`, multiplier) to `GameConfig.cs`.
- [ ] T033 [P] [US2] Add `noiseSprint` (`float`, default `3.0`, multiplier) to `GameConfig.cs`.
- [ ] T034 [P] [US2] Add `noiseInteract` (`float`, default `2.0`, multiplier) to `GameConfig.cs`.
- [ ] T035 [P] [US2] Add `noiseBatterySwap` (`float`, default `1.5`, multiplier) to
  `GameConfig.cs`.
- [ ] T036 [P] [US2] Add `noiseHiding` (`float`, default `0`, multiplier — "not detected") to
  `GameConfig.cs`.
- [ ] T037 [US2] Add validation rules for T031–T036 to `Validate()`: `noiseBaseRadius > 0`,
  `noiseWalk > 0`, `noiseSprint > noiseWalk`, `noiseInteract > 0`, `noiseBatterySwap > 0`,
  `noiseHiding >= 0`. Depends on T031–T036.
- [ ] T038 [P] [US2] EditMode tests in `Assets/Tests/EditMode/Config/GameConfigNoiseTests.cs`:
  valid-fixture pass and boundary-invalid fixture per field in T031–T036.

### 17.4 Monster (per floor)

- [ ] T039 [US2] Define `Assets/Scripts/Config/MonsterTuningProfile.cs`: plain C# readonly struct
  with `bool monsterActive`, `float patrolSpeed`, `float chaseSpeed`,
  `float investigateDuration`, `float chaseHoldDuration`, `float searchDuration` — no
  `MonoBehaviour`/`Component`. Depends on T003.
- [ ] T040 [P] [US2] Add `monsterTuningFloor51` (`MonsterTuningProfile`, defaults:
  `monsterActive=true`, `patrolSpeed=1.0`, `chaseSpeed=1.4`, `investigateDuration=4`,
  `chaseHoldDuration=3`, `searchDuration=6`) to `GameConfig.cs`.
- [ ] T041 [P] [US2] Add `monsterTuningFloor50` (`MonsterTuningProfile`, defaults:
  `monsterActive=true`, `patrolSpeed=1.2`, `chaseSpeed=1.5`, `investigateDuration=6`,
  `chaseHoldDuration=5`, `searchDuration=8`) to `GameConfig.cs`.
- [ ] T042 [P] [US2] Add `monsterActiveFloor52` (`bool`, locked `false` — Floor 52 has no monster)
  to `GameConfig.cs`.
- [ ] T043 [US2] Add the cross-field hard-rule validation to `Validate()`: for every floor profile,
  `chaseSpeed < sprintMultiplier`, read live (not cached) so retuning either field re-triggers the
  check. Depends on T040, T041, T017 (sprintMultiplier).
- [ ] T044 [US2] Add positive-duration validation to `Validate()`: for each floor profile with
  `monsterActive == true`, `investigateDuration > 0`, `chaseHoldDuration > 0`,
  `searchDuration > 0`. Depends on T040, T041.
- [ ] T045 [P] [US2] EditMode tests in
  `Assets/Tests/EditMode/Config/GameConfigMonsterTuningTests.cs`: locked-default assertions for
  Floor 51 and Floor 50 profiles, `monsterActiveFloor52 == false`, the hard-rule boundary
  (`chaseSpeed == sprintMultiplier` rejected, one increment below accepted), and the
  zero/negative-duration rejection for T044.

### 17.5 Progression

- [ ] T046 [P] [US2] Add `lives` (`int`, default `3`, hidden from HUD) to `GameConfig.cs`.
- [ ] T047 [P] [US2] Add `floorCount` (`int`, default `3` — Floor 52/51/50) to `GameConfig.cs`.
- [ ] T048 [P] [US2] Add `checkpointPerFloor` (`bool`, default `true`) to `GameConfig.cs`.
- [ ] T049 [P] [US2] Add `lockedDoorsFloor52` (`int`, default `0`) to `GameConfig.cs`.
- [ ] T050 [P] [US2] Add `lockedDoorsFloor51` (`int`, default `1`) to `GameConfig.cs`.
- [ ] T051 [P] [US2] Add `lockedDoorsFloor50` (`int`, default `3` — "+ Final Door" noted
  separately, not counted in this int) to `GameConfig.cs`.
- [ ] T052 [P] [US2] Add `targetFloorDuration` (`float`, default `300`, seconds, ±5 min target) to
  `GameConfig.cs`.
- [ ] T053 [US2] Add validation rules for T046–T052 to `Validate()`: `lives > 0`,
  `floorCount > 0`, `lockedDoorsFloor52 >= 0`, `lockedDoorsFloor51 >= 0`,
  `lockedDoorsFloor50 >= 0`, `targetFloorDuration > 0`. Depends on T046–T052.
- [ ] T054 [P] [US2] EditMode tests in
  `Assets/Tests/EditMode/Config/GameConfigProgressionTests.cs`: valid-fixture pass and
  boundary-invalid fixture per field in T046–T052.

### Checkpoint

- [ ] T055 [US2] Run T018, T030, T038, T045, T054 and confirm all green; cross-check the field
  list against the GDD Ch. 17 table line-by-line (SC-002 schema audit — zero missing, zero extra
  fields).

**Checkpoint**: Both user stories independently functional — the schema is complete, every field
has a validation rule, and startup fails loudly on any invalid fixture.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T056 [P] Audit all existing consumer code (any Unity project code outside
  `Assets/Scripts/Config/`) for duplicate literals matching a shipped `GameConfig` value; document
  any intentional presentation-only constant (e.g. a UI label's decorative-only number) that is
  NOT a duplicate (FR-005/FR-006, original T006).
- [ ] T057 Code review pass: confirm zero `UnityEngine.MonoBehaviour`/`Component` references in
  `GameConfig.cs`, `MonsterTuningProfile.cs`, `ConfigValidationFailure.cs`, or `FloorId.cs`
  (constitution Principle III compliance check).
- [ ] T058 Confirm the production asset `Assets/Config/GameConfig.asset` has zero duplicate
  tuning fields and every default matches its locked GDD Ch. 17 value where one is given (SC-001,
  SC-003).
- [ ] T059 Run the full Unity EditMode test suite under `Assets/Tests/EditMode/Config/` and record
  the result in the release gate (original T007).

## Dependencies & Execution Order

- **Setup (T001–T002)**: No dependencies.
- **Foundational (T003–T006)**: Depends on Setup — blocks every user story.
- **User Story 1 (T007–T012)**: Depends on Foundational. Deliberately implemented against the
  still-empty schema so the loading/validation contract is proven before fields pile on top.
- **User Story 2 (T013–T055)**: Depends on Foundational; independent of US1's guard
  implementation, though both read the same `Validate()` aggregator. Field-addition tasks marked
  `[P]` within the same GDD subsection touch the same file (`GameConfig.cs`) but add disjoint
  members, so they are still safe to parallelize across contributors before a single merge; each
  subsection's validation-rule task depends on that subsection's field tasks.
- **Polish (T056–T059)**: Depends on all of Phase 3 and Phase 4 being complete.

## Notes

- This entire feature is plain C# — there is no MonoBehaviour-only logic exempt from EditMode
  testing per constitution Principle IV.
- Fields marked "TBD di device" (T015, T016, T031) are feel-based values per ROADMAP §0: the FR
  states the rule/range, not a fabricated final number — do not invent precision the GDD doesn't
  have yet.
- `MonsterTuningProfile`'s two floor instances (T040, T041) are intentionally defined here even
  though `monster-ai/002-per-floor-tuning-profile` also references them — this spec owns the base
  `GameConfig` schema per FR-002/SC-002 ("every GDD Ch. 17 value maps to a named field"); that
  spec's own tasks should treat these fields as already present, not redefine them.
- Camera (Ch. 13), audio (Ch. 14), and UI/control feel-values (Ch. 15.3) are explicitly out of
  this spec's field list — the ROADMAP assigns those as additive `GameConfig` fields to
  `movement-and-camera`, `audio`, and `game-shell-ui` respectively; this spec's FR-002/SC-002
  scope is GDD Ch. 17 only.
- Commit after each checkpoint (T012, T055), not after every single task.
