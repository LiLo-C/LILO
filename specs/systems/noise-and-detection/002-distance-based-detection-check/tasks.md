---

description: "Task list for 002-distance-based-detection-check"
---

# Tasks: Distance-Based Detection Check

**Input**: Design documents from
`/specs/systems/noise-and-detection/002-distance-based-detection-check/spec.md`

**Prerequisites**: spec.md (this feature's; required)

**Tests**: Explicitly required — constitution Principle IV (Test-Before-Done) mandates EditMode
tests for every pure-logic piece, and this entire feature is pure C# distance/radius comparison
math with no rendering, physics, or input-device dependency, so nothing here qualifies for the
manual-quickstart exception.

**Organization**: Tasks are grouped by user story (US1–US4) to enable independent implementation
and testing of each. Within each story, tests are written first and MUST fail before the matching
implementation task closes them out.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: Which user story this task belongs to (US1–US4), or unlabeled for
  Setup/Foundational/Polish
- File paths are exact and relative to the repository root

## Path Conventions

- Pure C# game-rule class: `Assets/Scripts/Systems/Detection/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Detection/`
- Reused type (owned by monster-ai/001-state-machine-core-transitions, not this feature — this
  feature only consumes it): `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs`
- Reused radius input (owned by noise-and-detection/001-per-action-noise-emission, not this
  feature): `Assets/Scripts/Systems/Noise/NoiseEmitter.cs`

---

## Phase 1: Setup

**Purpose**: Establish the folders this feature's files live in. No production logic yet.

- [ ] T001 Create the `Assets/Scripts/Systems/Detection/` and
  `Assets/Tests/EditMode/Systems/Detection/` directories (via a placeholder file such as the
  first class/test below — Unity does not version empty folders). No `MonoBehaviour`, scene, or
  prefab work belongs in either directory for this feature.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Confirm the two external types/inputs every user story below reads or produces
already exist and are reused, not duplicated. No user story can be implemented before this phase
closes.

**⚠️ CRITICAL**: T002–T004 block every task in Phase 3 onward.

- [ ] T002 [P] Confirm `MonsterDetectionSignal` (`bool IsDetected`, `Vector3 SourcePosition`)
  already exists at `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs` (created by
  monster-ai/001-state-machine-core-transitions's own tasks, T003 in that spec). This feature
  references that exact type and MUST NOT declare a second, duplicate struct anywhere under
  `Assets/Scripts/Systems/Detection/` (per FR-011).
- [ ] T003 [P] Confirm `NoiseEmitter.CurrentNoiseRadius`
  (`Assets/Scripts/Systems/Noise/NoiseEmitter.cs`, from noise-and-detection/001) is the sole
  radius input this feature reads. This spec adds no new `GameConfig` field — the radius is
  entirely owned and computed upstream (per FR-002/FR-006).
- [ ] T004 Create `Assets/Scripts/Systems/Detection/DetectionCheck.cs`: a static class with the
  method signature `public static MonsterDetectionSignal Evaluate(Vector3 monsterPosition,
  Vector3 playerPosition, float noiseRadius)`, returning a not-detected default signal for now —
  no comparison logic yet (depends on T002).

**Checkpoint**: The reused type and the method stub exist — user story work can begin.

---

## Phase 3: User Story 1 - Monster Detects Player Within Its Noise Radius (Priority: P1) 🎯 MVP

**Goal**: `Evaluate` correctly reports detection whenever distance is strictly smaller than the
supplied radius, with `SourcePosition` set to the player's position.

**Independent Test**: Call `Evaluate` directly with a monster position, a player position, and a
radius where the distance between them is smaller than the radius; assert `IsDetected` is true
and `SourcePosition` equals the player's position. No other user story's code needs to exist.

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no comparison logic in `Evaluate` yet) before
> starting implementation.

- [ ] T005 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Detection/DetectionCheckWithinRadiusTests.cs`, write: distance
  smaller than radius → `IsDetected` true, `SourcePosition` equals the player position.
- [ ] T006 [P] [US1] Add: distance just barely inside the radius (radius minus a small epsilon) →
  `IsDetected` true.
- [ ] T007 [US1] Add: two consecutive calls with different monster/player positions and radii
  each produce their own correct, independent result — no state or lag carried between calls
  (depends on T005–T006 sharing the same test fixture).

### Implementation for User Story 1

- [ ] T008 [US1] Implement `Evaluate`'s core comparison: compute `distance =
  Vector3.Distance(monsterPosition, playerPosition)`, set `IsDetected = distance < noiseRadius`,
  and set `SourcePosition = playerPosition` unconditionally (per FR-001/FR-003/FR-012) (depends
  on T004).
- [ ] T009 [US1] Run T005–T007 against T008's implementation; fix `DetectionCheck` until every
  case is green.

**Checkpoint**: User Story 1 is fully functional and independently testable — the positive
detection case is correct.

---

## Phase 4: User Story 2 - No Detection At or Beyond the Radius, the Strict Boundary (Priority:
P1)

**Goal**: The comparison is strictly "less than" — an exactly equal distance, a distance beyond
the radius, and any non-positive radius all resolve to not-detected.

**Independent Test**: Evaluate with distance set to exactly equal the radius, and again with
distance greater than the radius; assert `IsDetected` is false in both cases, reusing the User
Story 1 harness pattern.

### Tests for User Story 2 ⚠️

- [ ] T010 [P] [US2] In
  `Assets/Tests/EditMode/Systems/Detection/DetectionCheckBoundaryTests.cs`, write: distance
  exactly equals the radius → `IsDetected` false (the strictly-less-than boundary test).
- [ ] T011 [P] [US2] Add: distance exceeds the radius → `IsDetected` false.
- [ ] T012 [P] [US2] Add: radius is 0 and distance is 0 (same position, idle/hiding case) →
  `IsDetected` false.
- [ ] T013 [US2] Add: a negative radius (e.g. -5) at any distance, including distance 0 →
  `IsDetected` false in every case — never flips true from a corrupted input.

### Implementation for User Story 2

- [ ] T014 [US2] Harden `Evaluate` so any radius `<= 0` (including negative values) can never
  produce `IsDetected = true` regardless of distance — verify the existing `distance < noiseRadius`
  comparison already satisfies this without a special-case branch (a distance of 0 is never
  `< 0`), and add an explicit defensive guard only if a corrupted negative radius could otherwise
  produce ambiguous behavior (depends on T008).
- [ ] T015 [US2] Run T010–T013 and confirm green.

**Checkpoint**: User Stories 1 and 2 both work independently — the boundary and defensive cases
close out the core comparison logic entirely.

---

## Phase 5: User Story 3 - Detection Ignores Walls and Line-of-Sight (Priority: P2)

**Goal**: Prove, structurally and behaviorally, that no geometry/occlusion input exists or could
ever affect the result.

**Independent Test**: Confirm `Evaluate`'s signature accepts exactly `(Vector3, Vector3, float)`
with no geometry parameter; then confirm two same-distance position pairs — one representing an
unobstructed line, one representing positions that would sit on opposite sides of a wall in a
real level — produce identical results.

### Tests for User Story 3 ⚠️

- [ ] T016 [P] [US3] In
  `Assets/Tests/EditMode/Systems/Detection/DetectionCheckOcclusionTests.cs`, write a structural
  test asserting `DetectionCheck.Evaluate`'s parameter list is exactly
  `(Vector3 monsterPosition, Vector3 playerPosition, float noiseRadius)` — no `Collider`,
  raycast-hit, or NavMesh-path parameter exists to add (compiles against that exact signature, or
  a reflection-based parameter-count/type check).
- [ ] T017 [US3] Add: two position pairs with the identical numeric distance, one chosen to
  represent a clear line and one chosen to represent positions on opposite sides of a wall in
  level-space coordinates, both evaluated with the same radius → identical `IsDetected` result in
  both cases.

### Implementation for User Story 3

- [ ] T018 [US3] No new comparison logic — occlusion-freedom is guaranteed by construction
  (`Evaluate` never queries `Physics` or NavMesh). Add an XML doc comment on
  `DetectionCheck.Evaluate` stating explicitly: "Straight-line distance only. Deliberately ignores
  walls/occlusion per GDD 7.2 — do not add a raycast or NavMesh path check here" (depends on
  T008).
- [ ] T019 [US3] Run T016–T017 and confirm green.

**Checkpoint**: The occlusion-free scope boundary is documented and tested, not just assumed.

---

## Phase 6: User Story 4 - Flashlight and Light State Never Affect Detection (Priority: P2)

**Goal**: Prove, structurally and behaviorally, that no light/illumination input exists or could
ever affect the result.

**Independent Test**: Evaluate the same fixed positions and radius while varying an unrelated
light-state value fed only into the test harness (never into `Evaluate`), across all four GDD 5.2
Light States, and confirm the result never changes.

### Tests for User Story 4 ⚠️

- [ ] T020 [P] [US4] In
  `Assets/Tests/EditMode/Systems/Detection/DetectionCheckLightStateTests.cs`, write: fixed
  monster/player positions and radius, evaluated once per each of the four conceptual Light
  States (Normal/Flickering/Critical/Compact Darkness) represented purely as a test-side loop
  variable never passed into `Evaluate` → identical `IsDetected` result across all four.
- [ ] T021 [US4] Add a structural assertion (mirroring T016's pattern): `DetectionCheck.Evaluate`'s
  parameter list contains no light-state, flashlight-on/off, or illumination/visibility-typed
  parameter.

### Implementation for User Story 4

- [ ] T022 [US4] No new comparison logic — light-independence is guaranteed by construction
  (`Evaluate` has no light-related parameter to read). Add an XML doc comment citing GDD 7.2
  ("Cahaya senter TIDAK memicu deteksi di versi ini") and GDD 18.2's cut-list confirmation
  (depends on T008).
- [ ] T023 [US4] Run T020–T021 and confirm green.

**Checkpoint**: All four user stories are independently proven — this feature's full scope is
functionally complete.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Prove the "no hardcoded radius" and "directly consumable by monster-ai/001"
guarantees the spec's Success Criteria require, and close out documentation.

- [ ] T024 [P] Write
  `Assets/Tests/EditMode/Systems/Detection/DetectionCheckConfigDrivenTests.cs`: feeding different
  `noiseRadius` values (as would come from a `NoiseEmitter` reading a test `GameConfig` with
  varied `noiseBaseRadius`) changes the detected outcome for a fixed monster/player distance —
  proves SC-007 (no hardcoded radius literal inside `DetectionCheck`).
- [ ] T025 [P] Write
  `Assets/Tests/EditMode/Systems/Detection/DetectionCheckSignalIntegrationTests.cs`: construct a
  `MonsterDetectionSignal` from `Evaluate`'s output and confirm its `IsDetected`/`SourcePosition`
  fields satisfy monster-ai/001's consumption contract (FR-003 of that spec) with zero mapping or
  adapter code required — proves this feature's output is directly consumable by monster-ai/001.
- [ ] T026 Add XML doc comments to `DetectionCheck.cs`'s public members citing GDD 7.2/17.3, this
  spec's `noiseBaseRadius` Ch. 21 open-item inheritance from noise-and-detection/001, and the
  `MonsterDetectionSignal` reuse note from Assumptions, so a future contributor does not
  accidentally duplicate the type or "fix" the deliberate scope boundaries in US3/US4.
- [ ] T027 Run the full `Assets/Tests/EditMode/Systems/Detection/` suite and confirm 100% green
  before marking this feature done (SC-006).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS every user story below.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **User Story 2 (Phase 4)**: Depends on Foundational and on `Evaluate`'s core comparison (T008,
  from US1) existing to harden.
- **User Story 3 (Phase 5)**: Depends on Foundational and T008 existing (to document and
  structurally verify) — does not depend on US2.
- **User Story 4 (Phase 6)**: Depends on Foundational and T008 existing — does not depend on US2
  or US3, but reuses the same structural-assertion pattern established in US3 for its own tests.
- **Polish (Phase 7)**: Depends on all four user stories being complete.

### Parallel Opportunities

- T002–T003 (confirming reused external types/inputs) can run in parallel.
- All test-writing tasks marked [P] within a phase can run in parallel with each other (different
  assertions in the same or sibling files, no shared mutable state — `DetectionCheck` is a pure
  static function with no instance state to race on).
- US3 and US4 depend only on Foundational + T008, not on each other or on US2 — once T008 exists,
  US2, US3, and US4 can all be staffed fully in parallel.

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Setup + Foundational.
2. Complete User Story 1 (positive detection) — this alone already gives monster-ai/001 a usable
   signal for the common case.
3. Complete User Story 2 (strict boundary + defensive cases) — required before this feature can
   be trusted not to over-detect at the edge.
4. **STOP and VALIDATE**: run the Phase 3–4 test suites independently; both pass.

### Incremental Delivery

1. Setup + Foundational → the reused type/input contracts confirmed, stub in place.
2. US1 → independently tested → positive detection proven.
3. US2 → independently tested → strict "less than" boundary and defensive-radius guarantees
   proven.
4. US3 → independently tested → occlusion-free scope boundary proven and documented.
5. US4 → independently tested → light-independence scope boundary proven and documented.
6. Polish → config-driven guarantee and monster-ai/001 integration-shape guarantee proven; full
   suite green.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task in Phases 3–6 has a matching test task that must be written and
  failing first, per constitution Principle IV.
- `DetectionCheck` never gains a geometry or light-state parameter during this feature's
  implementation — US3/US4's tasks exist to *prove* their absence, not to eventually add and then
  disable such inputs. Do not add either "for future use."
- The noise radius stays whatever placeholder noise-and-detection/001 currently produces
  throughout this feature's implementation — do not invent a final meters value to make a test
  "feel real" (see this spec's Assumptions and FR-006).
- Commit after each task or logical group; stop at any checkpoint to validate a story
  independently before moving to the next.
