---

description: "Task list for Monster State Machine Core Transitions"
---

# Tasks: Monster State Machine Core Transitions

**Input**: Design documents from `specs/systems/monster-ai/001-state-machine-core-transitions/`

**Prerequisites**: `spec.md` (this folder). No `plan.md`/`research.md` exist for this feature —
tasks are derived directly from `spec.md`'s functional requirements and user stories.

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature (constitution Principle IV) — the
state machine is pure C# with zero engine dependency, so every transition-table row MUST have a
covering test. Test tasks are not optional here.

**Organization**: Tasks are grouped by user story (US1–US4, matching `spec.md`), so each story is
independently implementable and testable. File paths are exact.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

## Phase 1: Setup

- [ ] T001 Create folders `Assets/Scripts/Systems/MonsterAI/` and
  `Assets/Scripts/Tests/EditMode/MonsterAI/` → actually place tests under
  `Assets/Tests/EditMode/MonsterAI/` per the project's fixed EditMode test root (ROADMAP §0); no
  code in this task, folder scaffolding only.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The data shapes every user story's tests and implementation depend on.

- [ ] T002 [P] Define `MonsterState` enum (`Patrol`, `Investigate`, `Chase`, `Search`) in
  `Assets/Scripts/Systems/MonsterAI/MonsterState.cs`.
- [ ] T003 [P] Define `MonsterDetectionSignal` readonly struct (`bool IsDetected`,
  `Vector3 SourcePosition`) in `Assets/Scripts/Systems/MonsterAI/MonsterDetectionSignal.cs` — the
  input shape consumed from noise-and-detection/002 (FR-003).
- [ ] T004 [P] Define `MonsterStateTuning` readonly struct (`float investigateDuration`,
  `float chaseHoldDuration`, `float searchDuration`, `float patrolSpeedMultiplier`,
  `float chaseSpeedMultiplier`) in `Assets/Scripts/Systems/MonsterAI/MonsterStateTuning.cs` — no
  default numeric values baked in; always supplied by the caller (FR-011).
- [ ] T005 Scaffold `MonsterStateMachine` class in
  `Assets/Scripts/Systems/MonsterAI/MonsterStateMachine.cs`: constructor takes
  `MonsterStateTuning`; fields for `CurrentState`, elapsed timer, last-known/target `Vector3`,
  `IsFrozen`; public no-op `Tick(float deltaTime, MonsterDetectionSignal signal)` stub;
  `CurrentState` starts at `Patrol` (FR-002).

**Checkpoint**: Foundation compiles; no behavior yet. User story work can begin.

---

## Phase 3: User Story 1 — Monster investigates a noise it hears (P1) 🎯 MVP

**Goal**: `Patrol` → `Investigate` on detection; `Investigate` → `Patrol` on timeout with no
re-detection.

**Independent Test**: Debug test arena / EditMode — see `spec.md` User Story 1.

### Tests for User Story 1

- [ ] T006 [P] [US1] EditMode test "Patrol stays Patrol with no detection" in
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_PatrolTests.cs`.
- [ ] T007 [P] [US1] EditMode test "Patrol → Investigate on detection, target = SourcePosition"
  in the same file (FR-004).
- [ ] T008 [P] [US1] EditMode test "Investigate → Patrol after investigateDuration with no
  re-detection" in `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_InvestigateTests.cs`
  (FR-006).
- [ ] T009 [P] [US1] EditMode test "Investigate does NOT transition to Patrol before
  investigateDuration elapses" (negative case, same file).

### Implementation for User Story 1

- [ ] T010 [US1] Implement `Patrol` branch of `Tick()`: on `IsDetected = true`, set
  `CurrentState = Investigate`, capture `SourcePosition` as target, reset the investigate timer
  (FR-004) — in `MonsterStateMachine.cs`.
- [ ] T011 [US1] Implement `Investigate` timeout branch of `Tick()`: advance timer by
  `deltaTime`; when timer ≥ `investigateDuration` with no detection tick since entering the
  state, transition to `Patrol` (FR-006) — in `MonsterStateMachine.cs`.
- [ ] T012 [US1] Expose a read-only `TargetPosition` property reflecting the current
  investigate/chase/search target, for the adapter to consume — in `MonsterStateMachine.cs`.

**Checkpoint**: Patrol↔Investigate ring fully functional and tested in isolation.

---

## Phase 4: User Story 2 — Monster escalates to a full chase (P1)

**Goal**: `Investigate`/`Search` → `Chase` on re-detection; `Chase` target updates on every
detection tick without starting the hold-timer.

**Independent Test**: See `spec.md` User Story 2.

### Tests for User Story 2

- [ ] T013 [P] [US2] EditMode test "Investigate → Chase on re-detection before
  investigateDuration elapses, target updates to new SourcePosition" in
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_ChaseTests.cs` (FR-005).
- [ ] T014 [P] [US2] EditMode test "Chase target updates on every detection tick; hold-timer
  does not advance while detection continues" (same file, FR-007).
- [ ] T015 [P] [US2] EditMode test "Search → Chase on re-detection, target updates" (same file,
  FR-009).

### Implementation for User Story 2

- [ ] T016 [US2] Implement `Investigate` re-detection branch: on `IsDetected = true` before
  timeout, transition to `Chase`, update target, reset hold-timer to zero (FR-005) — in
  `MonsterStateMachine.cs`.
- [ ] T017 [US2] Implement `Chase` per-tick branch: on `IsDetected = true`, update target to new
  `SourcePosition` and hold hold-timer at zero; on `IsDetected = false`, advance hold-timer by
  `deltaTime` (FR-007) — in `MonsterStateMachine.cs`.
- [ ] T018 [US2] Implement `Search` re-detection branch: on `IsDetected = true`, transition to
  `Chase`, update target, reset hold-timer to zero (FR-009) — in `MonsterStateMachine.cs`.

**Checkpoint**: Full escalation path (Investigate/Search → Chase) works and is tested.

---

## Phase 5: User Story 3 — Monster loses the trail and stands down (P2)

**Goal**: `Chase` → `Search` on hold-timer expiry; `Search` → `Patrol` on search-timer expiry;
`Patrol` resumes from current position (no teleport).

**Independent Test**: See `spec.md` User Story 3.

### Tests for User Story 3

- [ ] T019 [P] [US3] EditMode test "Chase → Search at exactly chaseHoldDuration with no
  detection, search center = last detected position" in
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_SearchTests.cs` (FR-008).
- [ ] T020 [P] [US3] EditMode test "Search → Patrol at exactly searchDuration with no
  detection" (same file, FR-010).
- [ ] T021 [P] [US3] EditMode test "Tie-break: detection arriving on the exact expiry tick wins
  over the timeout" for both Chase→Search and Search→Patrol boundaries (same file, FR-012,
  Edge Case "Tie-break at exact timer expiry").

### Implementation for User Story 3

- [ ] T022 [US3] Implement `Chase` hold-timer expiry branch: when hold-timer ≥
  `chaseHoldDuration`, transition to `Search`, set search center = last target, reset search
  timer (FR-008) — in `MonsterStateMachine.cs`.
- [ ] T023 [US3] Implement `Search` timer expiry branch: when search timer ≥ `searchDuration`
  with no detection, transition to `Patrol`, clear target/search center (FR-010) — in
  `MonsterStateMachine.cs`.
- [ ] T024 [US3] Ensure detection-vs-timeout tie-break resolves in favor of detection at every
  boundary (single shared helper/order-of-checks inside `Tick()`, FR-012) — in
  `MonsterStateMachine.cs`.

**Checkpoint**: Full ring (Patrol→Investigate→Chase→Search→Patrol) works end-to-end and is
tested for every documented transition plus the tie-break edge case.

---

## Phase 6: User Story 4 — Deterministic, externally freezable state machine (P3)

**Goal**: `Freeze()`/`IsFrozen`/`Reset()` hook for spec 004; provable determinism for EditMode
testing.

**Independent Test**: See `spec.md` User Story 4.

### Tests for User Story 4

- [ ] T025 [P] [US4] EditMode test "Freeze() makes every subsequent Tick() a no-op (state,
  timer, target all unchanged) until Reset()" in
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_FreezeResetTests.cs` (FR-013).
- [ ] T026 [P] [US4] EditMode test "Reset() returns CurrentState to Patrol and clears all
  timers/target/search-center, including when called without a prior Freeze()" (same file,
  Edge Case "External reset without an active catch").
- [ ] T027 [P] [US4] EditMode test "Determinism: two fresh instances fed the identical tick
  sequence produce identical (state, timer, target) after every tick" in
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_DeterminismTests.cs` (FR-014).

### Implementation for User Story 4

- [ ] T028 [US4] Implement `Freeze()` (sets `IsFrozen = true`) and an early-return guard at the
  top of `Tick()` that no-ops entirely while `IsFrozen` (FR-013) — in `MonsterStateMachine.cs`.
- [ ] T029 [US4] Implement `Reset()`: `IsFrozen = false`, `CurrentState = Patrol`, all
  timers/target/search-center cleared (FR-013) — in `MonsterStateMachine.cs`.
- [ ] T030 [US4] Audit `MonsterStateMachine.cs` for any hidden non-determinism (e.g., accidental
  use of `UnityEngine.Time`, `Random`, or static state) and remove it — pure function of its
  inputs only (FR-014).

**Checkpoint**: All four user stories complete; state machine is feature-complete, deterministic,
and freezable.

---

## Phase 7: MonoBehaviour Adapter (cross-cutting, enables in-scene/debug-arena verification)

**Purpose**: Give the pure state machine an actual on-screen `Monster` to drive, per constitution
Principle III's Systems/MonoBehaviours split, and per the Assumptions section's NavMesh decision.

- [ ] T031 [P] Create `MonsterController` `MonoBehaviour` in
  `Assets/Scripts/MonoBehaviours/MonsterController.cs`: holds a `MonsterStateMachine` instance, a
  `NavMeshAgent` reference, and an ordered `Transform[]` of patrol waypoints (assigned in the
  Inspector).
- [ ] T032 [MonsterController] `Update()` queries the noise-and-detection runtime API (per
  `specs/systems/noise-and-detection/002-distance-based-detection-check/spec.md`) to build a
  `MonsterDetectionSignal`, calls `Tick(Time.deltaTime, signal)`, and reads back
  `CurrentState`/`TargetPosition`.
- [ ] T033 `MonsterController`: on `Patrol`, advance through the waypoint array via sequential
  `NavMeshAgent.SetDestination` calls (advance to next waypoint when the agent arrives); on
  `Investigate`/`Chase`/`Search`, call `NavMeshAgent.SetDestination(TargetPosition)` (Assumptions:
  NavMesh chosen for all four states).
- [ ] T034 `MonsterController`: set `NavMeshAgent.speed` each tick from
  `patrolSpeedMultiplier`/`chaseSpeedMultiplier` × the player's `walkSpeed` (read from
  `GameConfig` per spec 002 — this task only wires the read, spec 002 owns the values);
  `Investigate`/`Search` speed default to `patrolSpeedMultiplier` unless/until spec 002 says
  otherwise.
- [ ] T035 [P] `MonsterController`: implement a simple `Search`-state wander helper that picks
  successive NavMesh-sampled points within a small radius of the search center (Assumptions:
  "circling" is an adapter concern, not state-machine logic).
- [ ] T036 Set up one debug test arena scene (NavMesh-baked bare room, not a real floor) used to
  manually verify `MonsterController` end-to-end per constitution Principle IV's fallback for
  logic that needs a live scene (movement/pathing itself, as opposed to the pure transition
  logic already covered by EditMode tests in Phases 3–6).

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T037 [P] Full transition-table matrix test: one consolidated EditMode test file,
  `Assets/Tests/EditMode/MonsterAI/MonsterStateMachine_TransitionMatrixTests.cs`, that runs one
  long scripted sequence through every documented transition in GDD Ch. 6.1's table in order
  (Patrol→Investigate→Chase→Search→Patrol, plus the Chase-continuation and Search→Chase
  re-escalation branches) and asserts the state/timer/target at each step — a single
  end-to-end sanity net on top of the per-story unit tests in Phases 3–6.
- [ ] T038 Code review pass: confirm `MonsterStateMachine.cs` never references a literal
  floor-specific number (4/6/3/5/6/8 seconds, 1.0×/1.2×/1.4×/1.5×) — every number must come from
  the injected `MonsterStateTuning` (FR-011).
- [ ] T039 [P] XML-doc comments on all public members of `MonsterState`, `MonsterDetectionSignal`,
  `MonsterStateTuning`, and `MonsterStateMachine`, since three other specs (002, 003, 004) and one
  external spec (noise-and-detection/002) integrate against this public surface.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks all user stories.
- **US1 (Phase 3)** → **US2 (Phase 4)**: US2's Investigate→Chase branch extends US1's Investigate
  timeout branch in the same file; implement in order.
- **US3 (Phase 5)** depends on US2 (Chase must exist before it can time out into Search).
- **US4 (Phase 6)** can start any time after Phase 2 (Freeze/Reset touch every branch, so doing it
  last avoids rework, but it has no hard code dependency on US1–US3's branches).
- **Phase 7 (Adapter)** depends on all of Phases 2–6 (needs the complete, frozen public API).
- **Phase 8 (Polish)** depends on everything above.

## Notes

- [P] tasks touch different files (or are read-only additions like doc comments) and can be
  parallelized.
- Every implementation task cites the exact `spec.md` FR it satisfies — keep that traceability
  when this file is updated.
- Per constitution Principle IV, do not mark any Phase 3–6 task "done" until its EditMode tests
  are written, failing first, then passing.
