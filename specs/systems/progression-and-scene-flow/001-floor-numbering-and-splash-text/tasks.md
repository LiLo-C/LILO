---

description: "Task list for 001-floor-numbering-and-splash-text"

---

# Tasks: Floor Numbering and Splash Text

**Input**: Design documents from
`/specs/systems/progression-and-scene-flow/001-floor-numbering-and-splash-text/spec.md`

**Prerequisites**: spec.md (this feature's; required)

**Tests**: Explicitly required — constitution Principle IV (Test-Before-Done) mandates EditMode
tests for every pure-logic piece. The floor→number/text mapping, invalid-ID rejection, "exactly
once" lifecycle, and dismiss-rule timing are all pure C# and MUST be covered. Only the actual
on-screen timing/safe-area rendering of the splash view is the manual-quickstart exception
(rendering + live scene).

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
- Reused type (owned by shared-config-and-state/002, not this feature): `GameState`
  at `Assets/Scripts/State/GameState.cs`
- Reused type (owned by shared-config-and-state/001, not this feature — this feature only adds
  new fields to it): `GameConfig` schema
- Consumed by (not owned by this feature): `SceneTransitionManager`'s per-load hook
  (progression-and-scene-flow/002)

---

## Phase 1: Setup

- [ ] T001 Confirm/create the `Assets/Scripts/Systems/Progression/`,
  `Assets/Scripts/MonoBehaviours/`, and `Assets/Tests/EditMode/Systems/Progression/` directories
  (via a placeholder file such as the first class/test below — Unity does not version empty
  folders). These may already exist from a sibling feature in this same folder group; reuse them,
  do not create duplicates.

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: T002–T005 block every US1 task in Phase 3.

- [ ] T002 [P] Confirm `GameState.CurrentFloor` (`Assets/Scripts/State/GameState.cs`,
  shared-config-and-state/002) is the sole floor-id input this feature reads to decide which
  splash to show — this feature MUST NOT introduce a second floor-tracking field.
- [ ] T003 [P] Add two new additive `GameConfig` fields per constitution Principle V:
  `floorSplashDisplayDuration` (seconds, auto-advance point) and
  `floorSplashMinimumHoldSeconds` (seconds, earliest a manual dismiss is honored) — confirm the
  addition is purely additive to the existing schema type/asset (shared-config-and-state/001), no
  existing field renamed or repurposed.
- [ ] T004 Define `Assets/Scripts/Systems/Progression/FloorProgressionEntry.cs`: a readonly
  struct with `FloorId` (int), `DisplayNumber` (string), and `SplashTextKey` (string) fields.
- [ ] T005 Create `Assets/Scripts/Systems/Progression/FloorProgressionTable.cs` stub: a static
  class exposing `TryGetEntry(int floorId, out FloorProgressionEntry entry)` that returns `false`
  for every id for now (depends on T004).

**Checkpoint**: The entry type and table stub exist — US1 work can begin.

---

## Phase 3: User Story 1 - Know Where the Run Is (Priority: P1) 🎯 MVP

**Goal**: Floor 52/51/50 each map to exactly one correct, ordered display; unsupported IDs are
structurally rejected rather than falling back to another floor's text; the splash presents
exactly once per load without mutating gameplay state or skipping the transition; and the
dismiss rule plus reload/duplicate/rapid-load edge cases are deterministic.

**Independent Test**: Request each floor, including invalid IDs, and compare displayed content
and timing; separately, fire reload/duplicate/rapid-load sequences and confirm identical,
single-shot output every time.

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (the table stub returns `false` for everything and no
> resolver/dismiss logic exists yet) before starting implementation.

- [ ] T006 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/FloorProgressionTableMappingTests.cs`, write:
  `TryGetEntry(52)`, `(51)`, and `(50)` each return `true` with the correct `DisplayNumber` and
  `SplashTextKey` per the ordered table, in strictly descending floor order.
- [ ] T007 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/FloorProgressionTableInvalidIdTests.cs`, write:
  `TryGetEntry` for an unsupported id (`0`, `53`, `-1`) returns `false` with a default `out`
  entry — never another floor's entry.
- [ ] T008 [US1] Add to the same fixture: two different invalid ids requested in sequence never
  return the same cached "last valid" entry — there is no fallback/sticky state (depends on
  T006–T007 sharing the fixture).
- [ ] T009 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/FloorSplashResolverLifecycleTests.cs`, write:
  resolving a valid floor id for a fresh load produces exactly one "show" decision; resolving the
  same floor id again for the same load (not a new load) does not produce a second "show" and
  does not report "skip the transition."
- [ ] T010 [P] [US1] Add to the same file: resolving does not read or write any `GameState`
  field — pass a `GameState` snapshot fixture before/after resolution and assert byte-for-byte
  equality (proves FR-002's "no gameplay-state mutation").
- [ ] T011 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/FloorSplashResolverDismissRuleTests.cs`, write:
  the splash cannot be manually dismissed before `GameConfig.floorSplashMinimumHoldSeconds` has
  elapsed, and auto-advances at/after `GameConfig.floorSplashDisplayDuration` — covers FR-003's
  "dismissible only under the configured rule."
- [ ] T012 [P] [US1] In
  `Assets/Tests/EditMode/Systems/Progression/FloorSplashResolverDeterminismTests.cs`, write:
  reloading the same floor, a duplicate resolve request in the same frame, and a simulated rapid
  back-to-back scene load each produce the same single deterministic outcome (one show, correct
  text) — covers the spec's Edge Cases list.

### Implementation for User Story 1

- [ ] T013 [US1] Populate `FloorProgressionTable` with the ordered entries for Floor 52, 51, and
  50 — their approved display numbers and splash text keys per GDD Ch. 2.4/8.2 (depends on T005;
  closes T006).
- [ ] T014 [US1] Harden `TryGetEntry` so any id not explicitly in the table returns `false`/
  default — no partial match, no nearest-floor fallback (depends on T013; closes T007–T008).
- [ ] T015 [US1] Create `Assets/Scripts/Systems/Progression/FloorSplashResolver.cs`: a pure C#
  class with `Resolve(int floorId, bool alreadyShownThisLoad)` returning a `FloorSplashDecision`
  (`ShouldShow`, `DisplayNumber`, `SplashTextKey`, `IsValidFloor`) — reads only
  `FloorProgressionTable` and the caller-supplied "already shown" flag, never a `GameState`
  reference (depends on T014; closes T009–T010).
- [ ] T016 [US1] Add the dismiss-rule check to `FloorSplashResolver` (a companion pure method
  reading elapsed display time against `GameConfig.floorSplashMinimumHoldSeconds` and
  `floorSplashDisplayDuration`) to produce `CanDismiss`/`ShouldAutoAdvance` (depends on T015,
  T003; closes T011).
- [ ] T017 [US1] Verify/adjust the "already shown this load" contract in `Resolve` so
  repeated/duplicate/rapid calls for the same load are idempotent, and a genuine reload always
  re-evaluates cleanly from a fresh "not yet shown" state (depends on T015–T016; closes T012).
- [ ] T018 [US1] Create `Assets/Scripts/MonoBehaviours/FloorSplashView.cs`: a thin adapter owning
  the Canvas/Text UI references, calling `FloorSplashResolver.Resolve` from
  `SceneTransitionManager`'s per-load hook (progression-and-scene-flow/002), starting/stopping
  display timing, and forwarding dismiss input — it contains no mapping or lifecycle-decision
  logic of its own (depends on T015–T016).
- [ ] T019 [US1] Run T006–T012 against T013–T017's implementation; fix `FloorProgressionTable`/
  `FloorSplashResolver` until every case is green.

**Checkpoint**: User Story 1 is fully functional and independently testable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T020 [P] Write
  `Assets/Tests/EditMode/Systems/Progression/FloorSplashResolverConfigDrivenTests.cs`: changing
  `GameConfig.floorSplashDisplayDuration`/`floorSplashMinimumHoldSeconds` changes the
  dismiss/auto-advance outcome for a fixed elapsed-time fixture — proves no hardcoded duration
  literal inside the resolver.
- [ ] T021 [P] Add XML doc comments to `FloorProgressionTable.cs` and `FloorSplashResolver.cs`
  citing GDD Ch. 2.4/8.2, and explicitly stating "do not add a fallback/nearest-floor lookup" so a
  future contributor doesn't silently soften the invalid-ID rule.
- [ ] T022 Manually validate `FloorSplashView`'s actual on-screen timing and safe-area layout on
  target hardware (iPhone, landscape) — the one piece of this feature needing a live scene/
  rendering per constitution Principle IV's exception.
- [ ] T023 Run the full `Assets/Tests/EditMode/Systems/Progression/` suite covering this feature
  and confirm 100% green before marking this feature done (SC-001/SC-002).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS User Story 1.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **Polish (Phase 4)**: Depends on User Story 1 being complete.

### Parallel Opportunities

- T002–T003 (confirming/adding reused external inputs) can run in parallel.
- All test-writing tasks marked [P] in Phase 3 can run in parallel with each other (different
  files or independent assertions, no shared mutable state — `FloorProgressionTable` and
  `FloorSplashResolver` are pure static/stateless functions).

---

## Implementation Strategy

### MVP First (User Story 1)

1. Complete Setup + Foundational.
2. Complete User Story 1 in full — this is the entire scope of this feature.
3. **STOP and VALIDATE**: run the Phase 3 test suite independently; all green.

### Incremental Delivery

1. Setup + Foundational → reused inputs confirmed, new `GameConfig` fields added, entry/table
   stub in place.
2. US1 mapping (T006–T008, T013–T014) → valid-floor and invalid-ID guarantees proven.
3. US1 lifecycle (T009–T010, T015) → exactly-once, no-state-mutation guarantee proven.
4. US1 dismiss rule + determinism (T011–T012, T016–T017) → configured-rule and edge-case
   guarantees proven.
5. US1 view adapter (T018) → wired to `SceneTransitionManager`.
6. Polish → config-driven guarantee proven, on-device timing/layout verified, full suite green.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on
  another *unfinished* task in this list.
- Every implementation task in Phase 3 has a matching test task that must be written and failing
  first, per constitution Principle IV.
- `FloorProgressionTable` and `FloorSplashResolver` never gain a fallback/nearest-floor lookup
  during this feature's implementation — T007–T008/T014's tasks exist to *prove* its absence, not
  to add and later disable one.
- Commit after each task or logical group; stop at the Phase 3 checkpoint to validate the story
  independently before moving to Polish.
