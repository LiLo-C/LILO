---

description: "Task list for 002-scene-transition-manager"

---

# Tasks: Scene Transition Manager

**Input**: Design documents from
`/specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`

**Prerequisites**: spec.md (this feature's; required)

**Tests**: Explicitly required — constitution Principle IV (Test-Before-Done) mandates EditMode
tests for every pure-logic piece. The scene graph, request serialization/idempotency, and
missing-scene/invalid-request reporting are all pure C# and MUST be covered. Only the actual
`UnityEngine.SceneManagement.SceneManager` load call and app suspend/resume plumbing are the
manual-quickstart exception (they need a live Unity runtime).

**Organization**: This spec defines a single user story (US1). Tasks are still split into the
smallest independently-completable units within that story — tests are written first and MUST
fail before the matching implementation task closes them out.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished
  task in this list)
- **[Story]**: Which user story this task belongs to (US1), or unlabeled for
  Setup/Foundational/Polish
- File paths are exact and relative to the repository root

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Progression/`
- Thin `MonoBehaviour` adapter: `Assets/Scripts/MonoBehaviours/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Progression/`
- Reused type (owned by shared-config-and-state/002, not this feature — this feature only
  triggers loads that it survives): `GameManager`/`GameState` at
  `Assets/Scripts/MonoBehaviours/GameManager.cs` / `Assets/Scripts/State/GameState.cs`
- Deliberately NOT read by this feature: `FloorProgressionTable`
  (progression-and-scene-flow/001) — floor display text and the scene graph are separate tables;
  this manager only needs scene identifiers.
- Consumed by (not owned by this feature): progression-and-scene-flow/001's splash view (its
  per-load hook), progression-and-scene-flow/003's run-completion listener (its final-door/ending
  events)

---

## Phase 1: Setup

- [ ] T001 Confirm/create the `Assets/Scripts/Systems/Progression/`,
  `Assets/Scripts/MonoBehaviours/`, and `Assets/Tests/EditMode/Systems/Progression/` directories.
  These may already exist from a sibling feature in this same folder group (e.g.
  progression-and-scene-flow/001); reuse them, do not create duplicates.

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: T002–T005 block every US1 task in Phase 3.

- [ ] T002 [P] Confirm `GameManager`/`GameState`
  (`Assets/Scripts/MonoBehaviours/GameManager.cs`, `Assets/Scripts/State/GameState.cs`,
  shared-config-and-state/002) is the sole mechanism that carries shared state across a scene
  transition — this feature MUST NOT introduce a second persistence path; it only decides
  *whether and which* scene loads, never re-implements state survival.
- [ ] T003 [P] Confirm this feature does not read `FloorProgressionTable`
  (progression-and-scene-flow/001) for routing decisions — floor display text and the scene
  routing graph are deliberately separate concerns.
- [ ] T004 Define `Assets/Scripts/Systems/Progression/SceneId.cs`: a fixed enum for `Bootstrap`,
  `MainMenu`, `Prologue`, `Floor52`, `Floor51`, `Floor50`, `GoodEnding`, `BadEnding`.
- [ ] T005 Create `Assets/Scripts/Systems/Progression/SceneGraph.cs` stub: a static class
  exposing `IsValidEdge(SceneId from, SceneId to)` that returns `false` for every pair for now
  (depends on T004).

**Checkpoint**: The scene-id enum and graph stub exist — US1 work can begin.

---

## Phase 3: User Story 1 - Progression Is Reliable (Priority: P1) 🎯 MVP

**Goal**: Every valid edge in the scene graph loads exactly once and carries shared state across
the transition; invalid or duplicate requests never produce an unintended load; missing scenes
and load failures are reported clearly, not silently dropped or crashed on.

**Independent Test**: Exercise every valid edge in the scene graph and invalid/repeated requests;
confirm each valid edge produces exactly one load and each invalid/repeated request produces
zero.

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (the graph stub rejects everything and no manager/request
> logic exists yet) before starting implementation.

- [ ] T006 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneGraphEdgesTests.cs`, write: every documented
  valid edge (`Bootstrap→MainMenu`, `MainMenu→Prologue`, `Prologue→Floor52`, `Floor52→Floor51`,
  `Floor51→Floor50`, `Floor50→GoodEnding`, and any floor `→BadEnding` per
  lives-and-fail-state/003) returns `true` from `IsValidEdge`.
- [ ] T007 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneGraphInvalidEdgesTests.cs`, write: edges not in
  the documented graph (`MainMenu→Floor51` skipping Prologue, `Floor50→Floor52` backwards,
  `GoodEnding→` anything) return `false`.
- [ ] T008 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneTransitionManagerSingleLoadTests.cs`, write:
  given no load in progress and a valid edge, requesting a transition produces exactly one "load
  X" instruction targeting the requested scene.
- [ ] T009 [P] [US1] Add to the same file: given a load is already in progress, a second request
  (same or different target) issued while it is active produces zero additional load instructions
  (serialized/idempotent per FR-002).
- [ ] T010 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneTransitionManagerDuplicateRequestTests.cs`,
  write: two identical back-to-back requests for the same target, issued before the first's load
  completes, produce exactly one load instruction total.
- [ ] T011 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneTransitionManagerInvalidRequestTests.cs`,
  write: a request for a target with no valid edge from the current scene produces zero load
  instructions and an explicit reported invalid-request failure (not a silent no-op).
- [ ] T012 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneTransitionManagerMissingSceneTests.cs`, write:
  a request targeting a `SceneId` with no corresponding scene entry produces a clearly reported
  missing-scene failure that names the `SceneId` — not a silent failure or crash (FR-003).
- [ ] T013 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/SceneTransitionManagerLifecycleEdgeCaseTests.cs`,
  write: a simulated scene-unload-before-load-completes, and a request arriving immediately after
  a reported app suspend/resume, both resolve to exactly one net scene change — with a fixture
  state counter (standing in for `GameState`, owned by shared-config-and-state/002) left
  untouched by the manager itself.
- [ ] T014 [P] [US1] Add to the same file: after a request into `GoodEnding`/`BadEnding` resolves,
  a further transition request is rejected unless it is the explicit new-run entry point — an
  ending's recorded state cannot be silently overwritten by a stray late request.

### Implementation for User Story 1

- [ ] T015 [US1] Populate `SceneGraph.IsValidEdge` with the full documented edge set
  (`Bootstrap→MainMenu`, `MainMenu→Prologue`, `Prologue→Floor52`, `Floor52→Floor51`,
  `Floor51→Floor50`, `Floor50→GoodEnding`, any floor `→BadEnding`) and no others (depends on T005;
  closes T006–T007).
- [ ] T016 [US1] Create `Assets/Scripts/Systems/Progression/SceneTransitionRequest.cs`: a pure C#
  struct with a `SceneId Target` field, consumed by the manager below (depends on T004).
- [ ] T017 [US1] Create `Assets/Scripts/Systems/Progression/SceneTransitionManager.cs`: a pure C#
  class holding `SceneId CurrentScene` and an `IsLoadInProgress` flag, exposing
  `TryRequestTransition(SceneId target)` that (a) checks `SceneGraph.IsValidEdge`, (b) checks
  `IsLoadInProgress` for serialization/idempotency, and (c) returns a `TransitionResult`
  (`Accepted` / `RejectedInvalidEdge` / `RejectedLoadInProgress`) with no direct Unity
  `SceneManager` call inside it (depends on T015–T016; closes T008–T010).
- [ ] T018 [US1] Add missing-scene detection to `SceneTransitionManager`: an injected
  `Func<SceneId, bool> sceneExists` seam so a call against a `SceneId` with no backing scene
  returns `RejectedMissingScene` naming the `SceneId`, testable without Unity's real Build
  Settings (depends on T017; closes T011–T012).
- [ ] T019 [US1] Add the suspend/resume and unload-before-complete guard to
  `SceneTransitionManager`: `IsLoadInProgress` is cleared only by an explicit "load completed"
  call from the adapter, never inferred, so a resume or unload event cannot leave it stuck true or
  falsely false (depends on T017; closes T013).
- [ ] T020 [US1] Add the post-ending rejection rule: once `CurrentScene` is `GoodEnding` or
  `BadEnding`, `TryRequestTransition` rejects further requests unless the target is the explicit
  new-run entry point (depends on T017; closes T014).
- [ ] T021 [US1] Create `Assets/Scripts/MonoBehaviours/SceneTransitionController.cs`: a thin
  adapter that owns the actual `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync` call,
  calls `SceneTransitionManager.TryRequestTransition` first, calls back "load completed" when
  Unity's load finishes, and surfaces `RejectedMissingScene`/other failures to a visible dev-log
  per FR-003 (depends on T017–T020).
- [ ] T022 [US1] Run T006–T014 against T015–T020's implementation; fix `SceneGraph`/
  `SceneTransitionManager` until every case is green.

**Checkpoint**: User Story 1 is fully functional and independently testable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T023 [P] Write (or add as an Editor-time check)
  `Assets/Tests/EditMode/Systems/Progression/SceneTransitionManagerAllScenesRegisteredTests.cs`:
  every `SceneId` used in `SceneGraph` has a corresponding entry in Build Settings — proves
  SC-001's "all valid graph edges load the expected scene" precondition holds for the shipped
  build list.
- [ ] T024 [P] Add XML doc comments to `SceneGraph.cs` and `SceneTransitionManager.cs` citing the
  ROADMAP progression table and constitution Principle III, explicitly noting the manager never
  calls `SceneManager` directly — that call lives only in the `MonoBehaviour` adapter.
- [ ] T025 Run the full `Assets/Tests/EditMode/Systems/Progression/` suite covering this feature
  and confirm 100% green before marking this feature done (SC-001/SC-002).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS User Story 1.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **Polish (Phase 4)**: Depends on User Story 1 being complete.

### Parallel Opportunities

- T002–T003 (confirming reused/excluded external inputs) can run in parallel.
- All test-writing tasks marked [P] in Phase 3 can run in parallel with each other (different
  files, no shared mutable state until T017's manager instance exists).

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 in full — this is the entire scope of this feature.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently; all green.

### Incremental Delivery

1. Setup + Foundational → scene-id enum and graph stub in place, reused/excluded inputs
   confirmed.
2. US1 graph (T006–T007, T015) → valid/invalid edge set proven.
3. US1 request handling (T008–T012, T016–T018) → single-load, serialization, duplicate-rejection,
   and missing-scene reporting proven.
4. US1 lifecycle edge cases (T013–T014, T019–T020) → suspend/resume and post-ending guarantees
   proven.
5. US1 adapter (T021) → wired to Unity's real `SceneManager`.
6. Polish → Build Settings audit, documentation, full suite green.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task in Phase 3 has a matching test task that must be written and failing
  first, per constitution Principle IV.
- `SceneTransitionManager` never calls Unity's `SceneManager` directly during this feature's
  implementation — that boundary is what keeps it EditMode-testable; the `MonoBehaviour` adapter
  is the only place that call is allowed to exist.
- Commit after each task or logical group; stop at the Phase 3 checkpoint to validate the story
  independently before moving to Polish.
