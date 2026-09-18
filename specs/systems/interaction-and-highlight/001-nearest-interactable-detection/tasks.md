---
description: "Task list for Nearest Interactable Detection"
---

# Tasks: Nearest Interactable Detection

**Input**: Design documents from
`specs/systems/interaction-and-highlight/001-nearest-interactable-detection/spec.md`

**Prerequisites**: `spec.md` (this feature; required). No dependency on another
interaction-and-highlight feature — this is the base contract that
`002-context-sensitive-action-button` and `003-interactable-highlight-halo` both consume.

**Tests**: Included per constitution Principle IV — selection, filtering, and tie-break are pure
math/logic and MUST be fully covered by EditMode tests. Only the occlusion raycast and the
in-scene registry wiring need a live scene, per the Principle IV exception.

**Organization**: Tasks are grouped by user story from `spec.md` (this feature has one story,
US1), in priority order.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, or independent assertions in the same file, with
  no ordering dependency on another unfinished task in this list)
- **[Story]**: Maps the task to `spec.md`'s user story (US1), or unlabeled for
  Setup/Foundational/Polish

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Interaction/`
- MonoBehaviour adapters: `Assets/Scripts/MonoBehaviours/Interaction/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Interaction/`

---

## Phase 1: Setup

- [ ] T001 Create the `Assets/Scripts/Systems/Interaction/`, `Assets/Scripts/MonoBehaviours/Interaction/`,
  and `Assets/Tests/EditMode/Systems/Interaction/` directories (via the first class/test below —
  Unity does not version empty folders).
- [ ] T002 Add `interactionRadius` (`float`, a sensible default such as `1.5`, documented as the
  world-unit distance within which a candidate can be selected) to the existing `GameConfig`
  `ScriptableObject` in `Assets/Scripts/Config/GameConfig.cs`. Do not create a second config
  asset/class (constitution Principle III).

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story task below can start until this phase is complete.

- [ ] T003 [P] Define the `IInteractionTarget` interface in
  `Assets/Scripts/Systems/Interaction/IInteractionTarget.cs`: `bool IsInteractable { get; }`
  (false when disabled, occluded-by-design, or already completed), `int Priority { get; }`
  (tie-break priority, higher wins), `Vector3 InteractionPoint { get; }` (world position used for
  distance). This is a plain interface with no `MonoBehaviour` requirement on the contract itself
  (constitution Principle III); concrete interactables (keys, doors, batteries, hiding spots —
  out of scope here) implement it on their own `MonoBehaviour`.
- [ ] T004 [P] Define the `InteractableCandidate` readonly struct value type in
  `Assets/Scripts/Systems/Interaction/InteractableCandidate.cs`: `Vector3 Position`,
  `bool IsInteractable`, `bool IsOccluded`, `int Priority`, `int StableId` (a caller-assigned
  deterministic tie-break key, e.g. the target's instance id). Pure data snapshot — no `Component`
  reference stored.
- [ ] T005 Define the `NearestInteractableSystem` plain C# static class with a stub
  `int SelectNearest(Vector3 origin, float radius, IReadOnlyList<InteractableCandidate> candidates)`
  (returns `-1` for "no selection") in
  `Assets/Scripts/Systems/Interaction/NearestInteractableSystem.cs` (depends on T004). No
  `MonoBehaviour`/`Component`/scene dependency (constitution Principle III).

**Checkpoint**: `NearestInteractableSystem.SelectNearest` compiles and is ready for story-specific
tests.

---

## Phase 3: User Story 1 - Action Targets Are Predictable (Priority: P1) 🎯 MVP

**Goal**: The player sees one nearest valid target within the interaction radius, chosen
deterministically, with no frame-to-frame flicker.

**Independent Test**: Place zero, one, and multiple candidates at boundary distances and assert
the selected index (spec.md's own Independent Test).

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no selection logic yet) before implementing.

- [ ] T006 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/NearestInteractableSystemEmptyTests.cs`,
  write: `SelectNearest` with zero candidates returns `-1` (FR-003's "empty" result).
- [ ] T007 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/NearestInteractableSystemBoundaryTests.cs`,
  write: a single valid candidate exactly at the configured radius is selected; the same candidate
  moved just beyond the radius is not (Edge Cases: boundary distance).
- [ ] T008 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/NearestInteractableSystemNearestTests.cs`,
  write: given several in-range valid candidates at different distances, the geometrically closest
  one's index is returned (US1 Acceptance Scenario 1).
- [ ] T009 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/NearestInteractableSystemTieBreakTests.cs`,
  write: two candidates at exactly equal distance resolve via `Priority` (higher wins); when
  `Priority` also ties, resolve via `StableId` — and calling `SelectNearest` repeatedly with the
  same unchanged candidate list returns the identical index every time (US1 Acceptance Scenario 2:
  "does not flicker frame to frame").
- [ ] T010 [P] [US1] In `Assets/Tests/EditMode/Systems/Interaction/NearestInteractableSystemValidityTests.cs`,
  write: a candidate with `IsInteractable = false` or `IsOccluded = true` is excluded even when it
  is the geometrically nearest one (FR-002; US1 Acceptance Scenario 3, "blocked candidate").
- [ ] T011 [P] [US1] In the same file as T009, write: shuffling the order of an otherwise-identical
  candidate list never changes which candidate is selected — selection depends only on distance,
  `Priority`, and `StableId`, never list position (Edge Cases: "simultaneous enable/disable" and
  moving-candidate churn must stay deterministic).

### Implementation for User Story 1

- [ ] T012 [US1] Implement `NearestInteractableSystem.SelectNearest`: filter to candidates with
  `IsInteractable && !IsOccluded` and distance `<= radius`, select the minimum-distance candidate,
  and resolve ties by highest `Priority` then lowest `StableId` (extends T005; makes T006–T011
  pass).

**Checkpoint**: User Story 1 is fully functional and independently testable via pure C# EditMode
tests — nearest, boundary, tie-break, and validity filtering all covered.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T013 [P] EditMode test: `SelectNearest` with a zero or negative `radius` returns `-1` for any
  input (never throws, never returns `NaN`-adjacent garbage), and a candidate placed exactly at
  the origin (zero distance) is still handled correctly, in
  `Assets/Tests/EditMode/Systems/Interaction/NearestInteractableSystemDegenerateTests.cs` (Edge
  Cases coverage).
- [ ] T014 Implement `NearestInteractableDetector` `MonoBehaviour` in
  `Assets/Scripts/MonoBehaviours/Interaction/NearestInteractableDetector.cs`: maintains a registry
  of in-scene `IInteractionTarget` components (targets register/unregister on enable/disable),
  each relevant update builds an `InteractableCandidate` list — performing the occlusion check
  (e.g. a raycast between the player and each in-range candidate; camera obstruction per Edge
  Cases) here, since that Unity API call cannot live in the pure system — calls
  `NearestInteractableSystem.SelectNearest`, and exposes the current selection
  (`GameObject CurrentSelection`, `event Action<GameObject> SelectionChanged`) for
  `002-context-sensitive-action-button` and `003-interactable-highlight-halo` to consume (depends
  on T003, T005, T012).
- [ ] T015 Manual quickstart-style validation pass on target hardware/test scene: place multiple
  interactables at varied distances, including one behind a wall, and confirm the nearest
  *visible* one is always selected with no flicker (documents the constitution Principle IV
  live-scene exception for the raycast/occlusion behavior — the selection math itself is already
  fully covered by EditMode tests above; operationalizes SC-001/SC-002).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** has no dependencies.
- **Foundational (Phase 2)** depends on Setup; blocks the user story.
- **User Story 1 (Phase 3)** depends only on Foundational.
- **Polish (Phase 4)** depends on User Story 1 being complete; T014 is the first point this
  feature is wired into a live scene, and the first point downstream features (`002`, `003`) have
  something to consume.

## Parallel Opportunities

- T003 and T004 can run in parallel; T005 depends on T004.
- All [P] test-writing tasks within Phase 3 can run in parallel with each other.

## Notes

- [P] tasks touch different files, or independent assertions with no ordering dependency on
  another unfinished task in this list.
- Write each story's tests before its implementation task and confirm they fail first, per
  constitution Principle IV.
- This feature introduces exactly one new `GameConfig` field (`interactionRadius`, T002) — no
  other config surface is touched.
- Commit after each task or logical group.
