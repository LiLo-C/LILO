---

description: "Task list for Monster Spawn Point Validation Rules"
---

# Tasks: Monster Spawn Point Validation Rules

**Input**: Design documents from `specs/systems/monster-ai/003-spawn-point-validation-rules/`

**Prerequisites**: `spec.md` (this folder). No `plan.md`/`research.md` exist for this feature —
tasks are derived directly from `spec.md`'s functional requirements and user stories. Depends on
`specs/systems/monster-ai/001-state-machine-core-transitions/` (this validator governs where a
`Monster` legally starts in `Patrol`) and, for `GameConfig` field additions, on
`specs/systems/shared-config-and-state/001-game-config-schema/`.

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature (constitution Principle IV) — every
purely-geometric rule and the aggregation/reporting logic are pure C# with zero engine
dependency, so every rule from FR-004 through FR-010 MUST have a covering test. Test tasks are
not optional here.

**Organization**: Tasks are grouped by user story (US1–US4, matching `spec.md`), so each story is
independently implementable and testable. File paths are exact.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/MonsterAI/` and `Assets/Tests/EditMode/MonsterAI/`
  exist (created by spec 001); this feature adds files alongside them — no new top-level folders
  needed.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The data shapes every user story's tests and implementation depend on.

- [ ] T002 [P] Define `MonsterSpawnCandidate` readonly struct (`Vector3 Position`,
  `bool IsNavigable`, `bool IsOutsideInitialPlayerSightline`, `bool IsSafeFromInstantDeath`) in
  `Assets/Scripts/Systems/MonsterAI/MonsterSpawnCandidate.cs`, with a constructor that requires
  all three booleans explicitly (no defaulted-to-`true` parameterless construction path) (FR-001,
  FR-016).
- [ ] T003 [P] Define `MonsterSpawnRuleViolation` flags enum
  (`TooCloseToPlayerSpawn`, `TooCloseToObjective`, `NotNavigable`,
  `WithinInitialPlayerSightline`, `UnsafeInstantDeath`) in
  `Assets/Scripts/Systems/MonsterAI/MonsterSpawnRuleViolation.cs` (FR-009, Key Entities).
- [ ] T004 [P] Define `MonsterSpawnValidationResult` readonly struct (`bool IsValid`,
  `MonsterSpawnRuleViolation Violations`) in
  `Assets/Scripts/Systems/MonsterAI/MonsterSpawnValidationResult.cs` (FR-010).
- [ ] T005 Add two additive `float` fields to the existing `GameConfig` `ScriptableObject`
  (`Assets/Scripts/Systems/Config/GameConfig.cs`, owned by
  `shared-config-and-state/001-game-config-schema`): `monsterSpawnMinDistanceFromPlayer` and
  `monsterSpawnMinDistanceFromObjective` (FR-003).
- [ ] T006 Scaffold `MonsterSpawnPointValidator` static class in
  `Assets/Scripts/Systems/MonsterAI/MonsterSpawnPointValidator.cs`: public method
  `Validate(MonsterSpawnCandidate candidate, GameConfig config, Vector3 playerSpawnPosition,
  IReadOnlyList<Vector3> objectivePositions)` returning `MonsterSpawnValidationResult`; no-op stub
  that currently always returns valid (FR-011).

**Checkpoint**: Data shapes and `GameConfig` fields compile; no rule logic yet.

---

## Phase 3: User Story 1 — Reject candidates too close to player/objective (P1) 🎯 MVP

**Goal**: The two purely geometric rules (player-distance, objective-distance) are enforced with
live-read `GameConfig` thresholds and strict-less-than boundary semantics.

**Independent Test**: See `spec.md` User Story 1.

### Tests for User Story 1

- [ ] T007 [P] [US1] EditMode test "Candidate closer than monsterSpawnMinDistanceFromPlayer
  reports TooCloseToPlayerSpawn" in
  `Assets/Tests/EditMode/MonsterAI/MonsterSpawnPointValidator_DistanceTests.cs` (FR-004).
- [ ] T008 [P] [US1] EditMode test "Candidate exactly at monsterSpawnMinDistanceFromPlayer does
  NOT report TooCloseToPlayerSpawn" (same file, FR-004, Edge Case "Boundary distance is a pass").
- [ ] T009 [P] [US1] EditMode test "Candidate closer than monsterSpawnMinDistanceFromObjective to
  the nearest of several objective positions reports TooCloseToObjective, regardless of which
  entry is nearest" (same file, FR-005).
- [ ] T010 [P] [US1] EditMode test "Empty objectivePositions list never reports
  TooCloseToObjective" (same file, FR-005, SC-005, Edge Case "Empty objective list is not an
  error").
- [ ] T011 [P] [US1] EditMode test "Changing monsterSpawnMinDistanceFromPlayer or
  monsterSpawnMinDistanceFromObjective on a GameConfig instance between two calls changes the
  result for the same candidate with no code change" in the same file (FR-015, SC-003).

### Implementation for User Story 1

- [ ] T012 [US1] Implement the player-distance check in `MonsterSpawnPointValidator.Validate`:
  read `config.monsterSpawnMinDistanceFromPlayer` fresh on every call; report
  `TooCloseToPlayerSpawn` when `Vector3.Distance(candidate.Position, playerSpawnPosition) <
  threshold` (FR-004, FR-015).
- [ ] T013 [US1] Implement the objective-distance check: compute the minimum distance from
  `candidate.Position` to any entry in `objectivePositions` (skip the check entirely, reporting
  no violation, when the list is empty); report `TooCloseToObjective` when that minimum is
  strictly less than `config.monsterSpawnMinDistanceFromObjective` (FR-005).

**Checkpoint**: The two geometric rules are fully functional and tested in isolation.

---

## Phase 4: User Story 2 — Reject unreachable or in-plain-view candidates (P1)

**Goal**: The two caller-supplied boolean rules (`IsNavigable`,
`IsOutsideInitialPlayerSightline`) are enforced identically to the geometric rules.

**Independent Test**: See `spec.md` User Story 2.

### Tests for User Story 2

- [ ] T014 [P] [US2] EditMode test "IsNavigable = false reports NotNavigable" in
  `Assets/Tests/EditMode/MonsterAI/MonsterSpawnPointValidator_SignalTests.cs` (FR-006).
- [ ] T015 [P] [US2] EditMode test "IsOutsideInitialPlayerSightline = false reports
  WithinInitialPlayerSightline" (same file, FR-007).
- [ ] T016 [P] [US2] EditMode test "Both signals true, with both User Story 1 distance checks
  passing, yields IsValid = true with an empty violation set" (same file, matching Acceptance
  Scenario 3).

### Implementation for User Story 2

- [ ] T017 [US2] Implement the navigability check in `MonsterSpawnPointValidator.Validate`:
  report `NotNavigable` when `candidate.IsNavigable = false` (FR-006).
- [ ] T018 [US2] Implement the sightline check: report `WithinInitialPlayerSightline` when
  `candidate.IsOutsideInitialPlayerSightline = false` (FR-007).

**Checkpoint**: All four of the five rules implemented (player-distance, objective-distance,
navigability, sightline); instant-death remains.

---

## Phase 5: User Story 3 — Report every violated rule in a single pass (P2)

**Goal**: The instant-death rule is added, and the aggregation guarantee (report ALL applicable
violations, never short-circuit) is proven across multi-violation cases.

**Independent Test**: See `spec.md` User Story 3.

### Tests for User Story 3

- [ ] T019 [P] [US3] EditMode test "IsSafeFromInstantDeath = false reports UnsafeInstantDeath" in
  `Assets/Tests/EditMode/MonsterAI/MonsterSpawnPointValidator_AggregationTests.cs` (FR-008).
- [ ] T020 [P] [US3] EditMode test "Candidate violating exactly 1 rule reports exactly that one
  violation" (same file, SC-002).
- [ ] T021 [P] [US3] EditMode test "Candidate violating exactly 3 rules (player-distance,
  navigability, instant-death) reports exactly those three, no fewer, no more" (same file,
  FR-009, SC-002).
- [ ] T022 [P] [US3] EditMode test "Candidate violating all 5 rules reports all 5 violations"
  (same file, FR-009, SC-002, Acceptance Scenario 1).
- [ ] T023 [P] [US3] EditMode test "Candidate violating 0 rules yields IsValid = true with an
  empty violation set — never a partial/warning-only state" (same file, FR-010, Acceptance
  Scenario 3).

### Implementation for User Story 3

- [ ] T024 [US3] Implement the instant-death check in `MonsterSpawnPointValidator.Validate`:
  report `UnsafeInstantDeath` when `candidate.IsSafeFromInstantDeath = false` (FR-008).
- [ ] T025 [US3] Ensure all five checks (T012, T013, T017, T018, T024) are evaluated
  unconditionally in `Validate` and their violations combined with bitwise-OR into a single
  `MonsterSpawnRuleViolation` value — no early return after the first violation is found (FR-009).
- [ ] T026 [US3] Set `MonsterSpawnValidationResult.IsValid = (Violations == default)` (zero flags
  set) as the sole pass condition — no intermediate "valid with warnings" branch (FR-010).

**Checkpoint**: All five GDD Ch. 6.3 rules implemented and independently/jointly tested; full
diagnostic reporting proven.

---

## Phase 6: User Story 4 — Reusable from tests and an editor tool (P3)

**Goal**: The exact same `MonsterSpawnPointValidator.Validate` function backs both the EditMode
test suite and a minimal editor-only Scene-view visualization tool, with zero duplicated rule
logic.

**Independent Test**: See `spec.md` User Story 4.

### Tests for User Story 4

- [ ] T027 [P] [US4] EditMode test "Validate is a pure function: two calls with identical inputs
  produce identical MonsterSpawnValidationResult values" in
  `Assets/Tests/EditMode/MonsterAI/MonsterSpawnPointValidator_PurityTests.cs` (supports SC-004).

### Implementation for User Story 4

- [ ] T028 [P] [US4] Create a minimal custom Editor script,
  `Assets/Scripts/Editor/MonsterAI/MonsterSpawnCandidateGizmoDrawer.cs`, that calls
  `MonsterSpawnPointValidator.Validate` for a candidate marker `Component` in the Scene view and
  draws a pass/fail-colored gizmo — containing no threshold/rule logic of its own, only a call
  into the shared validator and a color choice based on `IsValid` (FR-014, Acceptance Scenario
  2).
- [ ] T029 [US4] Manual spot check (per constitution Principle IV's live-scene exception,
  recorded here rather than in a separate `quickstart.md` since this feature has none): open a
  scene with one placed candidate marker, confirm the gizmo's pass/fail color matches the
  EditMode test suite's result for the same numeric inputs (SC-004).

**Checkpoint**: Validator is proven reusable with zero logic duplication between automated tests
and the editor tool.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T030 [P] Full success-criteria matrix test: one consolidated EditMode test file,
  `Assets/Tests/EditMode/MonsterAI/MonsterSpawnPointValidator_SuccessCriteriaTests.cs`, asserting
  SC-001 through SC-005 in one pass.
- [ ] T031 Code review pass: confirm `MonsterSpawnPointValidator.cs` contains no
  `MonoBehaviour`/`Collider`/`NavMeshAgent`/physics-callback reference anywhere (FR-011).
- [ ] T032 [P] XML-doc comments on all public members of `MonsterSpawnCandidate`,
  `MonsterSpawnRuleViolation`, `MonsterSpawnValidationResult`, and `MonsterSpawnPointValidator`,
  since the floor-scene spawn-preset specs and the editor tool both integrate against this public
  surface.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks all user stories; Phase 2's `GameConfig`
  field addition depends on `shared-config-and-state/001-game-config-schema` having created the
  base `GameConfig` class.
- **US1 (Phase 3)** and **US2 (Phase 4)** are independent of each other (different checks, same
  `Validate` method) and can be implemented in either order; both must land before US3's
  multi-violation aggregation tests are meaningful.
- **US3 (Phase 5)** depends on US1 and US2 (needs all five checks present to test 1/3/5-violation
  combinations) and adds the fifth check itself (instant-death).
- **US4 (Phase 6)** depends on Phases 2–5 (needs the complete, correct public API before wrapping
  it in an editor tool).
- **Phase 7 (Polish)** depends on everything above.

## Notes

- [P] tasks touch different files (or are read-only additions like doc comments) and can be
  parallelized.
- Every implementation task cites the exact `spec.md` FR it satisfies — keep that traceability
  when this file is updated.
- Per constitution Principle IV, do not mark any Phase 3–6 task "done" until its EditMode tests
  are written, failing first, then passing.
- This feature does not include level-design authoring tasks (placing actual candidate points on
  Floor 51/50) — those belong to `scenes/005-floor-51-scene/002-monster-patrol-route-and-spawn-presets`
  and `scenes/006-floor-50-scene/002-monster-patrol-route-and-spawn-presets`.
