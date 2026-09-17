---
description: "Task list for the Bootstrap scene"
---

# Tasks: Bootstrap Scene

**Input**: Design documents from `/specs/scenes/001-bootstrap-scene/` (`spec.md` only — this
feature has no `plan.md`/`data-model.md`/`contracts/`, per ROADMAP.md's note that scene specs
marked "single, no sub-split" own just `spec.md`/`tasks.md`/`checklists/`)

**Prerequisites**: `spec.md`. Also assumes, per the dependency order in
`specs/ROADMAP.md` §5, that `specs/systems/shared-config-and-state/001-game-config-schema` and
`.../002-shared-game-state-and-manager` and
`specs/systems/progression-and-scene-flow/002-scene-transition-manager` have already been
implemented — this scene only wires into their public surface, it does not implement them. Exact
method/class names on `GameManager` and the scene-transition mechanism are owned by those specs;
where this task list needs to call into them, it names the call in terms of *what it must do*
(create/reuse the manager, load MainMenu) rather than guessing an exact signature that spec
hasn't fixed yet.

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires EditMode coverage for
this scene's one piece of pure, extractable decision logic (the init-order/idempotency gate).

**Organization**: Tasks are grouped by user story from `spec.md`, in priority order (P1, P2, P3).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project: scene under `Assets/Scenes/`, scripts under `Assets/Scripts/MonoBehaviours/`
and `Assets/Scripts/Systems/`, EditMode tests under `Assets/Tests/EditMode/`.

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scenes/Bootstrap.unity` as a new, minimal Unity scene (default Camera +
  nothing else rendered)
- [ ] T002 Add `Assets/Scenes/Bootstrap.unity` to Build Settings as scene index 0 (File > Build
  Settings > Scenes In Build), reordering any existing entries so it is first
- [ ] T003 [P] Confirm/create the `Assets/Scripts/Systems/` and `Assets/Scripts/MonoBehaviours/`
  folders exist (per `specs/ROADMAP.md` §0 naming conventions: pure C# logic under `Systems/`,
  thin Unity-lifecycle adapters under `MonoBehaviours/`)

**Checkpoint**: Bootstrap scene exists, is empty, and is first in the build order.

---

## Phase 2: User Story 1 — Cold launch reaches the Main Menu unattended (Priority: P1) 🎯 MVP

**Goal**: Bootstrap creates `GameManager` (with `GameConfig` assigned), then transitions to
MainMenu with no player input required.

**Independent Test**: Launch the app from a cold process start; confirm Main Menu's Start button
is interactive with no crash and no visible intermediate screen.

### Tests for User Story 1

- [ ] T004 [P] [US1] Create `Assets/Tests/EditMode/BootstrapInitSequenceTests.cs`: a pure C#
  EditMode test asserting that, given a non-null `GameConfig` reference and no pre-existing
  `GameManager`, the init-sequence decision logic (T006) reports "proceed" through every step in
  order (config assigned → manager ready → audio root ready → transition) — write this test first
  and confirm it fails before T006 exists

### Implementation for User Story 1

- [ ] T005 [US1] Add a single `Bootstrap` GameObject to `Assets/Scenes/Bootstrap.unity` as the
  scene's one initialization entry point (FR-002 — no other GameObject in this scene performs
  independent `Awake()`/`Start()` initialization)
- [ ] T006 [US1] Create `Assets/Scripts/Systems/BootstrapInitSequence.cs`: a plain C# class (no
  `MonoBehaviour`/scene dependency, per constitution Principle III) exposing the ordered
  init-sequence decision described in spec.md FR-006 as pure logic — e.g. a small state/result
  type covering "config missing", "ready to create/reuse manager", "ready to init audio root",
  "ready to transition" — so `BootstrapInitializer` (T007) has nothing to decide, only to execute
- [ ] T007 [US1] Create `Assets/Scripts/MonoBehaviours/BootstrapInitializer.cs`: attach to the
  `Bootstrap` GameObject; holds the serialized `GameConfig` field (FR-004); in `Start()`, drives
  `BootstrapInitSequence` step by step: assign `GameConfig` to `GameManager`, create `GameManager`
  if one does not already exist (FR-003, FR-008), initialize the persistent audio mixer root
  (T009), then call into the scene-transition-manager system to load `MainMenu` (FR-007)
- [ ] T008 [US1] In the Inspector for `Assets/Scenes/Bootstrap.unity`, assign
  `Assets/Config/GameConfig.asset` to `BootstrapInitializer`'s serialized `GameConfig` field
- [ ] T009 [US1] Create `Assets/Scripts/MonoBehaviours/PersistentAudioRoot.cs`: a `MonoBehaviour`
  that, on creation, marks its GameObject `DontDestroyOnLoad` and hosts a reference to the
  project's shared `AudioMixer` (internal mixing behavior out of scope here — owned by
  `specs/systems/audio/`); add one `PersistentAudioRoot` GameObject to
  `Assets/Scenes/Bootstrap.unity`, wired so `BootstrapInitializer` can confirm it initialized
  before transitioning (FR-005, FR-006)

**Checkpoint**: A cold launch reaches MainMenu with a live `GameManager` (holding `GameConfig`)
and a live persistent audio root, with no manual step. US1 is independently testable end to end.

---

## Phase 3: User Story 2 — Persistent systems are live before gameplay ever needs them (Priority: P2)

**Goal**: The transition to MainMenu never fires until `GameConfig` assignment, `GameManager`
creation, and audio-root initialization have all genuinely completed — not merely been attempted.

**Independent Test**: Temporarily log, at the very start of MainMenu's own initialization, that
`GameConfig` is non-null and the persistent audio root already exists; confirm this holds on every
cold launch.

### Tests for User Story 2

- [ ] T010 [P] [US2] Extend `Assets/Tests/EditMode/BootstrapInitSequenceTests.cs` with cases for
  "config missing" and "audio root failed" inputs, asserting `BootstrapInitSequence` reports a
  blocking failure state rather than a "ready to transition" state for either (covers spec.md
  FR-009 and the two matching Edge Cases)

### Implementation for User Story 2

- [ ] T011 [US2] In `BootstrapInitializer.cs`, gate each step behind the previous step's actual
  completion (not a fire-and-forget call) — e.g. only create/reuse `GameManager` after `GameConfig`
  is confirmed non-null, only initialize `PersistentAudioRoot` after `GameManager` exists, only
  call the scene-transition-manager after `PersistentAudioRoot` reports ready (FR-006)
- [ ] T012 [US2] In `BootstrapInitializer.cs`, when `BootstrapInitSequence` reports a blocking
  failure (missing `GameConfig`, or the audio root failing to initialize), log a clear,
  identifiable `Debug.LogError` naming which step failed and return without calling the
  scene-transition-manager (FR-009)

**Checkpoint**: Bootstrap never hands off to MainMenu with a partially-initialized persistent
world, and a broken `GameConfig` reference fails loudly instead of silently.

---

## Phase 4: User Story 3 — Re-entering Bootstrap never duplicates persistent systems (Priority: P3)

**Goal**: If `Bootstrap.unity` is loaded a second time in the same process, the existing
`GameManager`/audio root are reused, not duplicated, and Bootstrap still ends by transitioning to
MainMenu.

**Independent Test**: In the Editor, load Bootstrap, let it transition to MainMenu, then manually
load Bootstrap a second time in the same Play session; confirm only one `GameManager` exists
afterward.

### Tests for User Story 3

- [ ] T013 [P] [US3] Extend `Assets/Tests/EditMode/BootstrapInitSequenceTests.cs` with a case for
  "GameManager already exists" input, asserting `BootstrapInitSequence` reports "reuse existing,
  still proceed to transition" rather than "create new" (covers FR-008)

### Implementation for User Story 3

- [ ] T014 [US3] In `BootstrapInitializer.cs`'s manager-creation step, check whether a persistent
  `GameManager` already exists (per the existing-instance check owned by
  `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`) before
  creating one; when one exists, reuse it and still continue through the remaining steps (FR-008)
- [ ] T015 [US3] Manual QA pass in the Unity Editor: Play from Bootstrap, let it reach MainMenu,
  stop, re-enter Play and manually load `Bootstrap.unity` a second time; confirm via the Hierarchy
  (or a temporary debug count) that exactly one `GameManager` and one `PersistentAudioRoot` exist

**Checkpoint**: All three user stories are independently verified; Bootstrap is safe to build the
rest of the scene layer on top of.

---

## Dependencies & Execution Order

- **Setup (Phase 1)**: No dependencies — start immediately.
- **User Story 1 (Phase 2)**: Depends on Setup. Delivers the MVP (cold launch → MainMenu).
- **User Story 2 (Phase 3)**: Depends on US1's `BootstrapInitializer`/`BootstrapInitSequence`
  existing; hardens the gating between steps.
- **User Story 3 (Phase 4)**: Depends on US1's manager-creation step existing; adds the
  reuse-instead-of-duplicate check on top of it.

## Notes

- `BootstrapInitSequence.cs` is deliberately the only place decision *logic* lives, so it can be
  unit-tested in EditMode without a running scene, per constitution Principle IV.
  `BootstrapInitializer.cs` stays a thin adapter that only calls Unity APIs (`DontDestroyOnLoad`,
  the scene-transition-manager call) — it does not re-implement the ordering decisions itself.
- Do not add a loading-screen UI, a second `GameManager`, or any gameplay-facing content to this
  scene under any task above — those are explicitly out of scope per `spec.md` Assumptions/FR-010.
