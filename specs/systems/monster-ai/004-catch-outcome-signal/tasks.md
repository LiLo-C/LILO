---
description: "Task list for Monster Catch Outcome Signal"
---

# Tasks: Monster Catch Outcome Signal

**Input**: Design documents from `specs/systems/monster-ai/004-catch-outcome-signal/`

**Prerequisites**: `spec.md` (this folder, source of truth). No `plan.md`/`research.md` exist —
tasks are derived directly from `spec.md`'s functional requirements, edge cases, and user story.
Depends on `specs/systems/monster-ai/001-state-machine-core-transitions/` (this feature reads
whether a `Monster`/its `MonsterStateMachine` is active, but does not alter its transition table)
and on `specs/systems/hiding/002-hiding-detection-immunity-rule/` (that spec's FR-002/FR-003
require hiding immunity to be consumed as an explicit boolean result and require catch signals
already in flight to obey this feature's contract — this file is the producer side of that
contract).

**Tests**: EditMode tests are NON-NEGOTIABLE for this feature (constitution Principle IV) — the
signal payload, the idempotency/sequence guard, and the hiding/pause/monster-active contact-rule
guards are all pure C# with zero engine dependency, so every one of them MUST be covered by an
EditMode test. The `MonoBehaviour` collision/trigger callback itself (T023) is the Principle IV
*exception* (physics-callback surface needing a live scene) and is validated instead by the
manual scene check (T024).

**Organization**: `spec.md` defines a single user story (US1). Tasks are still split into the
smallest independently-completable units — by responsibility (payload shape, idempotency gate,
contact-rule guards, `MonoBehaviour` wiring) — rather than left as one giant task. File paths are
exact.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/MonsterAI/`, `Assets/Scripts/MonoBehaviours/MonsterAI/`,
  and `Assets/Tests/EditMode/MonsterAI/` exist (the first and third were created by spec 001);
  this feature adds files alongside them — no new top-level folders needed.

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The data shapes every test and implementation task depends on.

- [ ] T002 [P] Define `CatchSignal` readonly struct (`int MonsterId`, `int Floor`,
  `float EventTime`) in `Assets/Scripts/Systems/MonsterAI/CatchSignal.cs` — the signal payload
  that identifies source, floor, and event time without owning outcome branching (FR-001).
- [ ] T003 [P] Define `ICatchSignalConsumer` interface (`void OnCatchSignal(CatchSignal signal);`)
  in `Assets/Scripts/Systems/MonsterAI/ICatchSignalConsumer.cs` — the consumer seam a future
  `DeathSequenceController` implements; this feature defines the contract only, per Scope (FR-003).
- [ ] T004 [P] Define `CatchContactContext` readonly struct (`bool PlayerHasHidingImmunity`,
  `bool IsGamePaused`, `bool IsMonsterActive`) in
  `Assets/Scripts/Systems/MonsterAI/CatchContactContext.cs` — the three inputs the contact rule
  guards on (Edge Cases: hiding immunity, pause, monster disabled state).
- [ ] T005 Scaffold `CatchSignalEmitter` plain C# class (no `MonoBehaviour`, no engine dependency)
  in `Assets/Scripts/Systems/MonsterAI/CatchSignalEmitter.cs`: holds a nullable
  `ICatchSignalConsumer` subscriber (settable via a `Subscribe` method) and a private
  `bool _sequenceInProgress`; public no-op stub `bool TryEmit(CatchSignal signal)` that currently
  always returns `false`; public no-op stub `void ResetForNextEncounter()` (FR-002, FR-003).
- [ ] T006 [P] Scaffold `MonsterCatchContactRule` static class in
  `Assets/Scripts/Systems/MonsterAI/MonsterCatchContactRule.cs`: public method
  `bool IsValidCatch(CatchContactContext context)` that currently always returns `false` (Edge
  Cases).

**Checkpoint**: Data shapes and emitter/rule scaffolds compile; no guard/idempotency behavior yet.

---

## Phase 3: User Story 1 — A catch starts one death flow (P1) 🎯 MVP

**Goal**: One valid catch produces exactly one `CatchSignal` with floor/context metadata and
exactly one consumer invocation (SC-001); repeated signals during an in-progress sequence never
duplicate that invocation (SC-002); hiding immunity, pause, scene unload, simultaneous contact,
and a disabled monster never produce a false catch (Edge Cases).

**Independent Test**: See `spec.md` User Story 1 — emit one signal, repeated signals during
sequence, and separate signals after reset.

### Tests for User Story 1

- [ ] T007 [P] [US1] EditMode test "TryEmit invokes the subscribed consumer exactly once with a
  CatchSignal whose MonsterId/Floor/EventTime match the values passed in" in
  `Assets/Tests/EditMode/MonsterAI/CatchSignalEmitterTests.cs` (FR-001, SC-001).
- [ ] T008 [P] [US1] EditMode test "TryEmit returns true on the first call and false on a second
  call made before ResetForNextEncounter, with the consumer invoked only once in total" (same
  file, FR-002, SC-001, SC-002, Acceptance Scenario 2, edge case "simultaneous contact").
- [ ] T009 [P] [US1] EditMode test "After ResetForNextEncounter, a subsequent TryEmit returns true
  again and invokes the consumer a second time" (same file, Acceptance Scenario 3).
- [ ] T010 [P] [US1] EditMode test "TryEmit with no subscriber set does not throw and returns
  true" (same file, defensive case covering edge case "scene unload" tearing down a consumer
  before a late signal).
- [ ] T011 [P] [US1] EditMode test "Two rapid TryEmit calls with different CatchSignal payloads
  (simulating two colliders touching the player the same frame) still produce exactly one consumer
  invocation, using the first call's payload" (same file, edge case "simultaneous contact",
  SC-002).
- [ ] T012 [P] [US1] EditMode test "IsValidCatch returns false when PlayerHasHidingImmunity =
  true, with IsGamePaused = false and IsMonsterActive = true" in
  `Assets/Tests/EditMode/MonsterAI/MonsterCatchContactRuleTests.cs` (edge case "hiding immunity";
  hiding/002 FR-002/FR-003).
- [ ] T013 [P] [US1] EditMode test "IsValidCatch returns false when IsGamePaused = true, with the
  other two context fields at their permissive values" (same file, edge case "pause").
- [ ] T014 [P] [US1] EditMode test "IsValidCatch returns false when IsMonsterActive = false, with
  the other two context fields at their permissive values" (same file, edge case "monster disabled
  state").
- [ ] T015 [P] [US1] EditMode test "IsValidCatch returns true when PlayerHasHidingImmunity =
  false, IsGamePaused = false, and IsMonsterActive = true" (same file, Acceptance Scenario 1).
- [ ] T016 [P] [US1] EditMode test "IsValidCatch is a pure function: two calls with an identical
  CatchContactContext produce identical results" (same file, supports reuse from both the detector
  and any future test double).

### Implementation for User Story 1

- [ ] T017 [US1] Implement `CatchSignalEmitter.Subscribe(ICatchSignalConsumer consumer)` to store
  the subscriber reference (FR-003).
- [ ] T018 [US1] Implement `CatchSignalEmitter.TryEmit`: if `_sequenceInProgress` is already
  `true`, return `false` without invoking the subscriber; otherwise set `_sequenceInProgress =
  true`, invoke the subscriber's `OnCatchSignal(signal)` exactly once if one is set, and return
  `true` (FR-002, FR-003, SC-001, SC-002).
- [ ] T019 [US1] Implement `CatchSignalEmitter.ResetForNextEncounter`: set `_sequenceInProgress =
  false` (Acceptance Scenario 3).
- [ ] T020 [US1] Implement the hiding-immunity check in `MonsterCatchContactRule.IsValidCatch`:
  return `false` when `context.PlayerHasHidingImmunity = true` (edge case "hiding immunity";
  consumes hiding/002's explicit boolean result per that spec's FR-002, does not re-derive it).
- [ ] T021 [US1] Implement the pause and monster-active checks in the same method: return `false`
  when `context.IsGamePaused = true` or `context.IsMonsterActive = false`; otherwise return `true`
  (edge cases "pause", "monster disabled state").
- [ ] T022 [US1] Ensure all three `MonsterCatchContactRule.IsValidCatch` checks are evaluated with
  no side effects and no cached/mutable state, so repeated calls with the same input are provably
  pure (Acceptance Scenario 1, T016).
- [ ] T023 [US1] Implement the thin `MonsterCatchContactDetector` `MonoBehaviour` in
  `Assets/Scripts/MonoBehaviours/MonsterAI/MonsterCatchContactDetector.cs`: owns the Unity
  collision/trigger callback (e.g. `OnTriggerEnter`) against the player, builds a
  `CatchContactContext` from the live hiding-immunity query, `GameState`'s pause flag, and this
  monster's active/enabled state; calls `MonsterCatchContactRule.IsValidCatch`; on `true`, builds
  a `CatchSignal` (`MonsterId` from this instance, `Floor` from `GameState`, `EventTime` from
  `Time.time`) and calls a `CatchSignalEmitter` instance's `TryEmit` — contains no gameplay rule
  logic itself (Principle III).
- [ ] T024 [US1] Wire `CatchSignalEmitter.ResetForNextEncounter` to fire on the next
  run/floor-start lifecycle point (e.g. the detector's `OnEnable` for a freshly loaded floor scene,
  per `GameManager`'s floor-load flow) so a new encounter on the next floor can produce a signal
  again (Acceptance Scenario 3, edge case "scene unload" — a fresh scene load naturally yields a
  fresh, non-static `CatchSignalEmitter` instance per Principle III's no-static-mutable-state
  rule, so no cross-scene leakage needs separate handling).

**Checkpoint**: One valid catch yields exactly one signal and one consumer call; hiding immunity,
pause, monster-disabled, simultaneous-contact, and scene-unload edge cases are all provably safe.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Manual scene check (per constitution Principle IV's live-scene exception, recorded
  here since this feature has no `quickstart.md`): in a live scene with one `Monster` and the
  player, confirm contact while Visible produces one signal, contact while Hidden produces none,
  and overlapping the monster twice in one frame still produces only one signal (SC-001, SC-002).
- [ ] T026 [P] XML-doc comments on all public members of `CatchSignal`, `ICatchSignalConsumer`,
  `CatchContactContext`, `CatchSignalEmitter`, and `MonsterCatchContactRule`, since the downstream
  death-sequence and bad-ending features integrate against this public surface (Scope).
- [ ] T027 Code review pass: confirm `CatchSignalEmitter.cs` and `MonsterCatchContactRule.cs`
  contain no `MonoBehaviour`/`Collider`/physics-callback reference anywhere (Principle III
  pure-C# testability).

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: blocks all US1 work; no external `GameConfig`
  fields are needed by this feature (no tunable thresholds in scope).
- **US1 tests (T007–T016)** should be written and failing before **US1 implementation
  (T017–T024)**, per Principle IV.
- Within implementation: T017–T019 (`CatchSignalEmitter`) and T020–T022
  (`MonsterCatchContactRule`) are independent of each other (different files, no shared state) and
  can proceed in either order; T023 (the `MonoBehaviour` detector) depends on both, since it calls
  into each; T024 depends on T023 existing.
- **Phase 4 (Polish)** depends on everything above.

## Notes

- [P] tasks touch different files (or are read-only additions like doc comments) and can be
  parallelized.
- Every implementation task cites the exact `spec.md` FR/SC/edge case it satisfies — keep that
  traceability when this file is updated.
- Per constitution Principle IV, do not mark any Phase 3 implementation task "done" until its
  EditMode tests are written, failing first, then passing; T023's `MonoBehaviour` callback has no
  EditMode surface and is validated by T025 instead.
- This feature does not implement `DeathSequenceController` or bad-ending routing — only the
  `ICatchSignalConsumer` contract it will implement (Scope). A future spec owning that controller
  wires itself in via `CatchSignalEmitter.Subscribe`, unchanged by this file.
- This feature does not re-derive hiding immunity's own detection math — it consumes
  `PlayerHasHidingImmunity` as an already-computed boolean, per hiding/002's FR-002 requirement
  that immunity be "consumed... as an explicit result, not a duplicated distance exception."
