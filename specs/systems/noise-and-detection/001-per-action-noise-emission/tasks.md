---

description: "Task list for 001-per-action-noise-emission"
---

# Tasks: Per-Action Noise Emission

**Input**: Design documents from `/specs/systems/noise-and-detection/001-per-action-noise-emission/spec.md`

**Prerequisites**: spec.md (this feature's; required)

**Tests**: Explicitly required — constitution Principle IV (Test-Before-Done) mandates EditMode tests for every pure-logic piece, and this entire feature is pure C# distance/radius math with no rendering, physics, or input-device dependency, so nothing here qualifies for the manual-quickstart exception.

**Organization**: Tasks are grouped by user story (US1–US4) to enable independent implementation and testing of each. Within each story, tests are written first and MUST fail before the matching implementation task closes them out.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no ordering dependency on another unfinished task in this list)
- **[Story]**: Which user story this task belongs to (US1–US4), or unlabeled for Setup/Foundational/Polish
- File paths are exact and relative to the repository root

## Path Conventions

- Pure C# game-rule classes: `Assets/Scripts/Systems/Noise/`
- EditMode tests: `Assets/Tests/EditMode/Systems/Noise/`
- Shared config asset class (owned by shared-config-and-state/001-game-config-schema, not this feature — this feature only adds fields to it): `Assets/Scripts/Config/GameConfig.cs`

---

## Phase 1: Setup

**Purpose**: Establish the folders this feature's files live in. No production logic yet.

- [ ] T001 Create the `Assets/Scripts/Systems/Noise/` and `Assets/Tests/EditMode/Systems/Noise/` directories (via a placeholder file such as the first class/test below — Unity does not version empty folders). No `MonoBehaviour`, scene, or prefab work belongs in either directory for this feature.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The shared config fields and small value types every user story below reads or produces. No user story can be implemented before this phase closes.

**⚠️ CRITICAL**: T002–T005 block every task in Phase 3 onward.

- [ ] T002 [P] In `Assets/Scripts/Config/GameConfig.cs` (owned by shared-config-and-state/001-game-config-schema — this task only adds fields, per ROADMAP §0's "every feature spec that needs a tunable adds a field to it"), confirm or add the six GDD 17.3 fields: `noiseBaseRadius` (float, unlocked placeholder — see this feature's Definition of Done), `noiseWalk` (float, default 1.0), `noiseSprint` (float, default 3.0), `noiseInteract` (float, default 2.0), `noiseBatterySwap` (float, default 1.5), `noiseHiding` (float, default 0).
- [ ] T003 [P] In the same file, add the new field `noisePulseDuration` (float, unlocked placeholder per FR-012 — a feel-based value with no GDD-sourced number).
- [ ] T004 [P] Create `Assets/Scripts/Systems/Noise/NoiseAction.cs` defining `enum PlayerMovementNoiseState { Idle, Walking, Sprinting }` (the movement-and-camera/001 signal this feature consumes) and `enum NoisePulseKind { Interact, BatterySwap }`.
- [ ] T005 Create `Assets/Scripts/Systems/Noise/NoisePulse.cs`: a small plain C# type holding `NoisePulseKind Kind` and a remaining-duration counter, exposing `bool IsActive`, a way to (re)start it at `noisePulseDuration`, and a `Tick(float deltaSeconds)` that counts down and clamps at zero (depends on T004).
- [ ] T006 [P] Write `Assets/Tests/EditMode/Systems/Noise/NoisePulseTests.cs` covering: a freshly-started pulse is active with full duration; `Tick` counts down; the pulse becomes inactive at/after its duration elapses; repeated `Tick` calls past expiry never go negative or re-activate (depends on T005).

**Checkpoint**: Config fields and the pulse primitive exist and are tested in isolation — user story work can begin.

---

## Phase 3: User Story 1 - Baseline Movement Noise (Priority: P1) 🎯 MVP

**Goal**: `CurrentNoiseRadius` correctly reflects idle (0×), walking (1.0×), and sprinting (3.0×) against a configurable base radius.

**Independent Test**: Construct a `NoiseEmitter` with a fixed `GameConfig`, feed it each movement state in turn, and assert the returned radius — no other user story's code needs to exist.

### Tests for User Story 1 ⚠️

> Write these first; confirm they fail (there is no `NoiseEmitter` yet) before starting implementation.

- [ ] T007 [P] [US1] In `Assets/Tests/EditMode/Systems/Noise/NoiseEmitterMovementTests.cs`, write the idle case: no movement, not hiding, no pulse → radius is 0.
- [ ] T008 [P] [US1] Add the walking case to the same file: radius equals `noiseBaseRadius × noiseWalk`.
- [ ] T009 [P] [US1] Add the sprinting case: radius equals `noiseBaseRadius × noiseSprint`.
- [ ] T010 [US1] Add the transition case: switching directly from idle to sprinting yields the sprint value on the very next evaluation, with no intermediate walking value ever observed (depends on T007–T009 sharing the same test fixture).

### Implementation for User Story 1

- [ ] T011 [US1] Create `Assets/Scripts/Systems/Noise/NoiseEmitter.cs`: a plain C# class (no `MonoBehaviour`, no scene dependency) constructed with a `GameConfig` reference, exposing `SetMovementState(PlayerMovementNoiseState state)` and a `CurrentNoiseRadius` property computing `noiseBaseRadius × multiplier` for Idle(`noiseHiding`)/Walking(`noiseWalk`)/Sprinting(`noiseSprint`) — hiding and pulses are not implemented yet (depends on T002, T004).
- [ ] T012 [US1] Run T007–T010 against T011's implementation; fix `NoiseEmitter` until every case is green.

**Checkpoint**: User Story 1 is fully functional and independently testable — the baseline noise ladder (0/1.0×/3.0×) is correct.

---

## Phase 4: User Story 2 - Hiding Is Always Silent (Priority: P1)

**Goal**: The hiding flag forces the radius to 0 unconditionally, beating movement and any pulse.

**Independent Test**: Feed the emitter hiding=true simultaneously with sprinting and (once available) an active pulse; the radius must be 0 in every combination.

### Tests for User Story 2 ⚠️

- [ ] T013 [P] [US2] In `Assets/Tests/EditMode/Systems/Noise/NoiseEmitterHidingTests.cs`, write: hiding=true + idle movement → radius 0.
- [ ] T014 [P] [US2] Add: hiding=true + sprinting movement → radius still 0 (defensive-override case).
- [ ] T015 [US2] Add: hiding transitions from true to false → on the next evaluation the radius reflects the current non-hiding movement state, with no residual suppression.

### Implementation for User Story 2

- [ ] T016 [US2] Extend `NoiseEmitter.cs` with `SetHiding(bool isHiding)`, and reorder `CurrentNoiseRadius` so the hiding flag is checked first and, if true, short-circuits to `noiseHiding` (0) before any movement or pulse logic runs (depends on T011).
- [ ] T017 [US2] Run T013–T015 and confirm green.

**Checkpoint**: User Stories 1 and 2 both work independently — hiding's override is proven, including against movement.

---

## Phase 5: User Story 3 - Interacting Makes Noise (Priority: P2)

**Goal**: A one-shot interaction pulse elevates the radius to 2.0× for a configured duration, combining with movement via highest-wins.

**Independent Test**: Trigger an interaction pulse on an otherwise-idle emitter and confirm the radius, duration, and decay behavior; then re-run against walking/sprinting to confirm the combination rule.

### Tests for User Story 3 ⚠️

- [ ] T018 [P] [US3] In `Assets/Tests/EditMode/Systems/Noise/NoiseEmitterInteractPulseTests.cs`, write: triggering an interact pulse from idle → radius immediately equals `noiseBaseRadius × noiseInteract`.
- [ ] T019 [P] [US3] Add: the pulse value persists for `noisePulseDuration` seconds of simulated `Tick`s, then the radius reverts to the underlying movement value on the next tick after expiry.
- [ ] T020 [P] [US3] Add: triggering an interact pulse while walking → radius is the pulse's higher value (2.0×), not walking's 1.0× and not their sum.
- [ ] T021 [P] [US3] Add: triggering an interact pulse while sprinting → radius remains sprint's higher value (3.0×) — the pulse never lowers an already-higher radius.
- [ ] T022 [US3] Add: retriggering the interact pulse while one is already active restarts its remaining duration instead of stacking two expiries.

### Implementation for User Story 3

- [ ] T023 [US3] Extend `NoiseEmitter.cs` with `TriggerInteractPulse()`, an internal `NoisePulse` slot, and a `Tick(float deltaSeconds)` method that advances any active pulse (depends on T005, T016).
- [ ] T024 [US3] Update `CurrentNoiseRadius` to take `max(movementMultiplier, activePulseMultiplier)` whenever hiding is false, and make retriggering the same pulse kind restart its duration (FR-008/FR-009) (depends on T023).
- [ ] T025 [US3] Run T018–T022 and confirm green.

**Checkpoint**: User Stories 1–3 work independently and together — interaction pulses and the highest-wins combination rule are implemented and tested.

---

## Phase 6: User Story 4 - Installing a Battery Makes Noise (Priority: P3)

**Goal**: A battery-swap pulse elevates the radius to 1.5× using the same pulse mechanism as US3.

**Independent Test**: Trigger a battery-swap pulse from idle and confirm the radius and combination behavior, reusing the US3 harness pattern.

### Tests for User Story 4 ⚠️

- [ ] T026 [P] [US4] In `Assets/Tests/EditMode/Systems/Noise/NoiseEmitterBatterySwapPulseTests.cs`, write: triggering a battery-swap pulse from idle → radius equals `noiseBaseRadius × noiseBatterySwap`.
- [ ] T027 [P] [US4] Add: a battery-swap pulse and an interact pulse active at the same instant → the higher (interact, 2.0×) value wins.
- [ ] T028 [US4] Add: a battery-swap pulse active while sprinting → sprint's higher value wins.

### Implementation for User Story 4

- [ ] T029 [US4] Extend `NoiseEmitter.cs` with `TriggerBatterySwapPulse()`, reusing the same pulse slot and combination logic as US3 (depends on T023, T024).
- [ ] T030 [US4] Document (XML doc comment on `TriggerBatterySwapPulse`) that flashlight-and-battery/008-battery-install-and-refill's battery-install action is the intended sole caller of this method — wiring the actual `MonoBehaviour` adapter that calls it is out of scope here and belongs to 008's own tasks.
- [ ] T031 [US4] Run T026–T028 and confirm green.

**Checkpoint**: All four action multipliers plus hiding's override are implemented and independently tested — this feature's full scope is functionally complete.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Prove the "config-driven, not hardcoded" and defensive-clamp guarantees the spec's Success Criteria require, and close out documentation.

- [ ] T032 [P] Write `Assets/Tests/EditMode/Systems/Noise/NoiseEmitterConfigDrivenTests.cs`: for each of the six fields (`noiseBaseRadius`, `noiseWalk`, `noiseSprint`, `noiseInteract`, `noiseBatterySwap`, `noiseHiding`), changing its value in a test `GameConfig` instance changes `NoiseEmitter`'s corresponding output — proves SC-007 (no hardcoded literal).
- [ ] T033 [P] Add the hiding-cancels-pulse edge case to `NoiseEmitterHidingTests.cs`: a pulse active when hiding begins does not resume or continue expiring once hiding ends (FR-010).
- [ ] T034 [P] Add a defensive-clamp test (new or appended to `NoiseEmitterMovementTests.cs`): a negative or otherwise invalid config multiplier never produces a negative emitted radius.
- [ ] T035 Add XML doc comments to `NoiseEmitter.cs`'s public members citing GDD 7.1/17.3 and this spec's `noiseBaseRadius` Ch. 21 open-item caveat, so a future contributor does not silently "finish" the value without checking the lock status first.
- [ ] T036 Run the full `Assets/Tests/EditMode/Systems/Noise/` suite and confirm 100% green before marking this feature done (SC-008).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS every user story below.
- **User Story 1 (Phase 3)**: Depends on Foundational only.
- **User Story 2 (Phase 4)**: Depends on Foundational and on `NoiseEmitter` existing (T011, from US1) to extend.
- **User Story 3 (Phase 5)**: Depends on Foundational (specifically `NoisePulse`, T005) and on US2's hiding short-circuit (T016) being in place so the combination rule has something to short-circuit against.
- **User Story 4 (Phase 6)**: Depends on US3's pulse/combination machinery (T023, T024) — it reuses rather than reimplements it.
- **Polish (Phase 7)**: Depends on all four user stories being complete.

### Parallel Opportunities

- T002–T004 (config fields, enum) can run in parallel; T005/T006 depend on T004.
- All test-writing tasks marked [P] within a phase can run in parallel with each other (different assertions in the same or sibling files, no shared mutable state).
- Because US4 depends on US3's implementation, US3 and US4 cannot be staffed fully in parallel — but their *test-writing* tasks can be drafted in parallel once US3's public method signatures are known.

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Setup + Foundational.
2. Complete User Story 1 (baseline movement noise) — this alone already gives monster-ai a usable signal for walk/sprint.
3. Complete User Story 2 (hiding override) — required before hiding/002's detection-immunity rule can be trusted.
4. **STOP and VALIDATE**: run the Phase 3–4 test suites independently; both pass with zero knowledge of pulses.

### Incremental Delivery

1. Setup + Foundational → Foundation ready.
2. US1 → independently tested → baseline noise ladder proven.
3. US2 → independently tested → hiding guarantee proven.
4. US3 → independently tested → interaction pulses + combination rule proven.
5. US4 → independently tested → battery-swap pulse proven, reusing US3's machinery.
6. Polish → config-driven and defensive-clamp guarantees proven; full suite green.

---

## Notes

- [P] tasks touch different files or independent assertions with no ordering dependency on another *unfinished* task in this list.
- Every implementation task in Phases 3–6 has a matching test task that must be written and failing first, per constitution Principle IV.
- `noiseBaseRadius` and `noisePulseDuration` stay as clearly-labeled placeholders in `GameConfig` throughout this feature's implementation — do not invent a final number to make a test "feel real" (see spec.md's Definition of Done).
- Commit after each task or logical group; stop at any checkpoint to validate a story independently before moving to the next.
