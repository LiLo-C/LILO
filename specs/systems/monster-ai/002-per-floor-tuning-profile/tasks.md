---

description: "Task list for Monster Per-Floor Tuning Profile"
---

# Tasks: Monster Per-Floor Tuning Profile

**Input**: Design documents from `specs/systems/monster-ai/002-per-floor-tuning-profile/`

**Prerequisites**: `spec.md` (this folder). No `plan.md`/`research.md` exist for this feature —
tasks are derived directly from `spec.md`'s functional requirements and user stories. Depends on
`specs/systems/monster-ai/001-state-machine-core-transitions/` (consumes `MonsterStateTuning`) and
`specs/systems/shared-config-and-state/001-game-config-schema/` (owns the base `GameConfig` class
and the floor-identifier type this feature adds fields to and looks profiles up by).

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature (constitution Principle IV) — every
locked numeric value, the hard-rule validator, and the explicit-failure lookup behavior are pure
C# and MUST be covered. Test tasks are not optional here.

**Organization**: Tasks are grouped by user story (US1–US3, matching `spec.md`), so each story is
independently implementable and testable. File paths are exact.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/MonsterAI/` and `Assets/Tests/EditMode/MonsterAI/`
  exist (created by spec 001) — this feature adds files alongside them; no new top-level folders
  needed.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The data shape and `GameConfig` additions every user story depends on.

- [ ] T002 [P] Define `MonsterTuningProfile` readonly struct (`bool monsterActive`,
  `float patrolSpeed`, `float chaseSpeed`, `float investigateDuration`,
  `float chaseHoldDuration`, `float searchDuration`) in
  `Assets/Scripts/Systems/MonsterAI/MonsterTuningProfile.cs` (FR-001). `patrolSpeed`/`chaseSpeed`
  are documented as dimensionless multipliers of the player's `walkSpeed`, never a literal m/s
  constant (FR-012).
- [ ] T003 [P] Define a `MonsterTuningLookupResult` type (success-with-profile /
  explicit-failure-with-reason) in `Assets/Scripts/Systems/MonsterAI/MonsterTuningLookupResult.cs`
  — the return shape for FR-009/FR-011's "no silent fallback" guarantee.
- [ ] T004 Add two additive serialized fields to the existing `GameConfig`
  `ScriptableObject` (`Assets/Scripts/Systems/Config/GameConfig.cs`, owned by
  `shared-config-and-state/001-game-config-schema`): `monsterTuningFloor51` and
  `monsterTuningFloor50`, both `MonsterTuningProfile` (FR-002). No second config source; reuses
  the existing `sprintMultiplier` field already on `GameConfig` (GDD Ch. 17.1) for the hard-rule
  check — do not duplicate it.
- [ ] T005 [P] Set the `GameConfig` asset's Floor 51 default values: `investigateDuration = 4`,
  `chaseHoldDuration = 3`, `searchDuration = 6`, `patrolSpeed = 1.0`, `chaseSpeed = 1.4`,
  `monsterActive = true` (FR-003).
- [ ] T006 [P] Set the `GameConfig` asset's Floor 50 default values: `investigateDuration = 6`,
  `chaseHoldDuration = 5`, `searchDuration = 8`, `patrolSpeed = 1.2`, `chaseSpeed = 1.5`,
  `monsterActive = true` (FR-004).

**Checkpoint**: `GameConfig` carries both locked profiles; no lookup/validation logic yet.

---

## Phase 3: User Story 1 — Monster behaves differently on Floor 51 vs. Floor 50 (P1) 🎯 MVP

**Goal**: A single lookup resolves the correct, GDD-locked profile per floor, with zero
floor-branching logic duplicated elsewhere.

**Independent Test**: See `spec.md` User Story 1.

### Tests for User Story 1

- [ ] T007 [P] [US1] EditMode test "Floor 51 lookup resolves to investigateDuration=4,
  chaseHoldDuration=3, searchDuration=6, patrolSpeed=1.0, chaseSpeed=1.4" in
  `Assets/Tests/EditMode/MonsterAI/MonsterTuningProfileLookupTests.cs` (FR-003, SC-001).
- [ ] T008 [P] [US1] EditMode test "Floor 50 lookup resolves to investigateDuration=6,
  chaseHoldDuration=5, searchDuration=8, patrolSpeed=1.2, chaseSpeed=1.5" (same file, FR-004,
  SC-002).
- [ ] T009 [P] [US1] EditMode test "Identical scripted detection sequence run through a
  Floor-51-tuned `MonsterStateMachine` vs. a Floor-50-tuned one produces differing timer
  expirations and speed multipliers by exactly the documented deltas" in
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_PerFloorTuningTests.cs` (User Story 1
  Independent Test; exercises spec 001's `MonsterStateMachine` as the consumer).
- [ ] T010 [P] [US1] EditMode test "Editing a value on the `GameConfig` asset changes the
  resolved profile with no code change" — construct the profile from asset-like data rather than
  a hardcoded literal in the test itself, proving the value is read, not baked in (FR-014,
  SC-004).

### Implementation for User Story 1

- [ ] T011 [US1] Implement `MonsterTuningProfileLookup.Resolve(GameConfig config, FloorId floor)`
  returning the Floor 51/Floor 50 profile for those two floors in
  `Assets/Scripts/Systems/MonsterAI/MonsterTuningProfileLookup.cs` (FR-009) — the single lookup
  operation; no floor-branching logic exists anywhere else.
- [ ] T012 [US1] Wire spec 001's `MonsterController` (`Assets/Scripts/MonoBehaviours/
  MonsterController.cs`) to construct its `MonsterStateTuning`/speed inputs exclusively from a
  `MonsterTuningProfileLookup.Resolve` result for the active floor — remove any placeholder
  literal numbers left over from spec 001 (FR-008).

**Checkpoint**: Floor 51 and Floor 50 drive visibly different monster timing/speed purely via
config data.

---

## Phase 4: User Story 2 — The hard rule against an unbeatable monster is enforced (P1)

**Goal**: `chaseSpeed < sprintMultiplier` is checked live (never cached) for every profile, at
both test time and Unity editor time, with a named, actionable failure on violation.

**Independent Test**: See `spec.md` User Story 2.

### Tests for User Story 2

- [ ] T013 [P] [US2] EditMode test "chaseSpeed equal to sprintMultiplier is rejected, failure
  names the offending floor and both values" in
  `Assets/Tests/EditMode/MonsterAI/MonsterTuningValidatorTests.cs` (FR-005, Edge Case "chaseSpeed
  exactly equal to sprintMultiplier").
- [ ] T014 [P] [US2] EditMode test "chaseSpeed exceeding sprintMultiplier is rejected" (same
  file, FR-005).
- [ ] T015 [P] [US2] EditMode test "chaseSpeed one increment below sprintMultiplier passes" (same
  file, FR-005).
- [ ] T016 [P] [US2] EditMode test "Both shipped profiles (1.4× Floor 51, 1.5× Floor 50) pass
  against the shipped sprintMultiplier (1.6×)" (same file, FR-007, SC-003).
- [ ] T017 [P] [US2] EditMode test "Lowering sprintMultiplier below a previously-valid chaseSpeed
  causes the validator to fail on its next run, with no cached copy from profile-construction
  time" (same file, FR-006, Edge Case "Cross-field revalidation").
- [ ] T018 [P] [US2] EditMode test "investigateDuration/chaseHoldDuration/searchDuration ≤ 0 is
  rejected for a `monsterActive = true` profile" (same file, FR-013, Edge Case "Zero/negative
  duration authored by mistake").

### Implementation for User Story 2

- [ ] T019 [US2] Implement `MonsterTuningValidator.ValidateChaseSpeed(MonsterTuningProfile
  profile, float sprintMultiplier, FloorId floor)` in
  `Assets/Scripts/Systems/MonsterAI/MonsterTuningValidator.cs`, reading both values fresh on
  every call (no cached field), using strict `<` (FR-005, FR-006).
- [ ] T020 [US2] Implement `MonsterTuningValidator.ValidatePositiveDurations(MonsterTuningProfile
  profile, FloorId floor)` for FR-013 in the same file.
- [ ] T021 [US2] Add a `GameConfig.OnValidate()` editor-time hook (Unity `ScriptableObject`
  callback) that calls both validator methods against the Floor 51 and Floor 50 profiles and
  surfaces any failure as an Inspector-visible error/log (FR-007b).
- [ ] T022 [US2] Add an EditMode test runner entry point that calls the same two validator
  methods against the shipped `GameConfig` asset as part of the automated suite (FR-007a) — same
  file as T016.

**Checkpoint**: The hard rule cannot silently regress, whether a designer edits `chaseSpeed` or
`sprintMultiplier`.

---

## Phase 5: User Story 3 — Floor 52 correctly has no monster tuning to resolve (P2)

**Goal**: Lookups for Floor 52 or any unrecognized floor id fail explicitly; `monsterActive`
correctly reports `false` for Floor 52 without requiring a profile to exist.

**Independent Test**: See `spec.md` User Story 3.

### Tests for User Story 3

- [ ] T023 [P] [US3] EditMode test "monsterActive reports false for Floor 52" in
  `Assets/Tests/EditMode/MonsterAI/MonsterTuningProfileLookupTests.cs` (FR-010).
- [ ] T024 [P] [US3] EditMode test "Lookup for Floor 52 fails explicitly and never returns Floor
  51's or Floor 50's profile as a fallback" (same file, FR-011, SC-005).
- [ ] T025 [P] [US3] EditMode test "Lookup for an undefined/typo'd floor id fails the same
  explicit way as Floor 52" (same file, FR-011, Edge Case "Lookup for an undefined/typo'd floor
  id").

### Implementation for User Story 3

- [ ] T026 [US3] Implement the `MonsterTuningProfileLookup.Resolve` branch for Floor 52 and any
  unrecognized floor id, returning the explicit-failure case of `MonsterTuningLookupResult`
  (never a default struct or another floor's values) (FR-011) — in
  `MonsterTuningProfileLookup.cs`.
- [ ] T027 [US3] Confirm (via test, not just code inspection) that no call site constructs a
  `Monster` instance or invokes `MonsterTuningProfileLookup.Resolve` for Floor 52 during normal
  floor load — cite `scenes/004-floor-52-scene/*` as the consumer that must never do so; this
  task only adds the guarding test here, not the scene wiring itself (FR-010).

**Checkpoint**: All three user stories complete; profile resolution is correct, validated, and
fails loudly rather than silently for the no-monster floor.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T028 [P] Full success-criteria matrix test: one consolidated EditMode test file,
  `Assets/Tests/EditMode/MonsterAI/MonsterTuningProfile_SuccessCriteriaTests.cs`, asserting
  SC-001 through SC-005 in one pass.
- [ ] T029 Code review pass: confirm no literal `4`/`6`/`3`/`5`/`6`/`8`/`1.0`/`1.2`/`1.4`/`1.5`
  values exist anywhere in `MonsterStateMachine.cs` or `MonsterController.cs` outside of
  `GameConfig`'s two profile fields (FR-008, FR-014).
- [ ] T030 [P] XML-doc comments on all public members of `MonsterTuningProfile`,
  `MonsterTuningProfileLookup`, `MonsterTuningLookupResult`, and `MonsterTuningValidator`, since
  spec 001's adapter and `GameConfig`'s editor hook both integrate against this public surface.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks all user stories; Phase 2 also depends
  on `shared-config-and-state/001-game-config-schema` having created the base `GameConfig` class
  and floor-identifier type.
- **US1 (Phase 3)** has no dependency on US2/US3 — it only needs the locked values from Phase 2.
- **US2 (Phase 4)** can start any time after Phase 2 (the hard rule only needs `chaseSpeed` and
  `sprintMultiplier`, both already present); independent of US1's lookup wiring.
- **US3 (Phase 5)** depends on US1's lookup existing (Phase 3, T011) so it has a function whose
  failure branch it can implement/test.
- **Phase 6 (Polish)** depends on everything above.

## Notes

- [P] tasks touch different files (or are read-only additions like doc comments) and can be
  parallelized.
- Every implementation task cites the exact `spec.md` FR it satisfies — keep that traceability
  when this file is updated.
- Per constitution Principle IV, do not mark any Phase 3–5 task "done" until its EditMode tests
  are written, failing first, then passing.
