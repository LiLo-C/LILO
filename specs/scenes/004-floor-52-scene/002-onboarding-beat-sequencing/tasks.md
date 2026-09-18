---
description: "Task list for Floor 52 Onboarding Beat Sequencing"
---

# Tasks: Floor 52 Onboarding Beat Sequencing

**Input**: Design documents from
`specs/scenes/004-floor-52-scene/002-onboarding-beat-sequencing/`

**Prerequisites**: [spec.md](./spec.md); geometry from
`001-level-layout-and-geometry` (Checkpoint, SafeArea, ExplorationZone, DistantZone,
ChaseSection, ExitDoorAlcove Areas and the `BatteryCandidate_<n>` / `HidingSpotCandidate`
anchors) must already exist in `Assets/Scenes/Floor52.unity`.

**Tests**: The beat-order/state-machine logic is plain C# and EditMode-testable (Phase 5). Whether
the sequence *feels* readable without tutorial popups (spec.md SC-001, SC-002) is level-design
judgment that needs a live scene and a human player, so per constitution Principle IV's exception
it is validated by the manual playtests in Phase 3, not by an automated assertion.

**Organization**: Tasks are grouped by user story from spec.md. This spec has a single P1 user
story (US1), broken into the five onboarding beats from GDD Ch. 15.4 as separate, independently
placeable trigger tasks.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Create `Assets/Scripts/MonoBehaviours/Floor52/OnboardingBeat.cs`: a plain C# enum
      `OnboardingBeat { MovementAndLight, BatteryAndResource, DistantThreatAwareness, Hiding,
      NoiseAndSprint }` matching the GDD Ch. 15.4 onboarding table order exactly
- [ ] T002 Create `Assets/Scripts/Systems/Floor52/OnboardingBeatSequencer.cs`: a plain C# class
      (no `MonoBehaviour`, no scene dependency — constitution Principle III) that tracks the
      current/completed `OnboardingBeat`s and exposes `TryCompleteBeat(OnboardingBeat)`, which
      only succeeds when the beat is the next one in sequence (spec.md FR-001, FR-003), plus
      `IsBeatUnlocked`/`CurrentBeat` queries
- [ ] T003 Create `Assets/Tests/EditMode/Floor52/` folder for this spec's tests if it does not
      already exist

**Checkpoint**: The beat enum and sequencer logic exist for scene triggers to reference.

---

## Phase 2: Foundational — Trigger Adapter

**⚠️ MUST complete before any user story below.**

- [ ] T004 Create `Assets/Scripts/MonoBehaviours/Floor52/OnboardingBeatTrigger.cs`: a thin
      `MonoBehaviour` with `public OnboardingBeat beat;`, attached to a trigger-volume
      `GameObject`, that calls `OnboardingBeatSequencer.TryCompleteBeat(beat)` on the qualifying
      player action and fires that beat's visible/audio affordance event (spec.md FR-002)

**Checkpoint**: The adapter component exists for each beat's trigger volume to use.

---

## Phase 3: User Story 1 - Learn by Doing (Priority: P1)

**Goal**: Floor 52 introduces movement, light, battery, hiding, noise, and exit in the GDD Ch.
15.4 order, without long tutorial popups, and the sequence cannot be silently skipped.

**Independent Test**: Play from checkpoint to exit and record each mechanic's first required use
in order.

### Implementation for User Story 1

- [ ] T005 [P] [US1] Place and tag the Beat 1 (`MovementAndLight`) trigger volume covering the
      `SafeArea` Area in `Assets/Scenes/Floor52.unity` with `OnboardingBeatTrigger(beat:
      MovementAndLight)`, so basic movement + flashlight is the first beat encountered (GDD Ch.
      15.4 row 1)
- [ ] T006 [P] [US1] Place and tag the Beat 2 (`BatteryAndResource`) trigger volume at the
      entrance to `ExplorationZone` in `Assets/Scenes/Floor52.unity` with
      `OnboardingBeatTrigger(beat: BatteryAndResource)`, positioned so it fires once the player
      reaches the first `BatteryCandidate_<n>` area (coordinate with
      `003-static-battery-and-hiding-placement`'s actual `Battery` instances)
- [ ] T007 [P] [US1] Place and tag the Beat 3 (`DistantThreatAwareness`) trigger volume at the
      sightline point toward the `DistantZone` element in `Assets/Scenes/Floor52.unity` with
      `OnboardingBeatTrigger(beat: DistantThreatAwareness)` (coordinate with
      `004-distant-monster-hint-audio`'s emitter placement so the audio cue and this trigger read
      as the same event)
- [ ] T008 [P] [US1] Place and tag the Beat 4 (`Hiding`) trigger volume around the
      `HidingSpotCandidate` nook in `Assets/Scenes/Floor52.unity` with `OnboardingBeatTrigger(beat:
      Hiding)`, firing on a successful Hide action (per
      `specs/systems/hiding/001-enter-and-exit-hiding`'s Hidden-state event) rather than mere
      proximity
- [ ] T009 [P] [US1] Place and tag the Beat 5 (`NoiseAndSprint`) trigger volume at the entrance to
      `ChaseSection` in `Assets/Scenes/Floor52.unity` with `OnboardingBeatTrigger(beat:
      NoiseAndSprint)`, positioned to fire once the player crosses into the long corridor that
      encourages sprinting (GDD Ch. 15.4 row 5)
- [ ] T010 [US1] Verify `OnboardingBeatSequencer.TryCompleteBeat` rejects a beat fired
      out-of-order (e.g., Beat 3 firing before Beat 2 completes), so the sequence cannot be
      silently skipped (spec.md FR-001, FR-003)
- [ ] T011 [US1] Wire a restart/returning-player guard so that on death/restart the sequencer
      resets to the last *completed* beat boundary (not Beat 1) and does not replay
      already-completed beats' affordances (spec.md FR-003, US1 Acceptance Scenario 2) —
      coordinate with `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death`
- [ ] T012 [US1] Manual playtest (new-player persona): play checkpoint → exit, recording each
      mechanic's first required use in order, and confirm the recorded order matches GDD Ch. 15.4
      exactly with no external instruction needed (spec.md Independent Test, SC-001)
- [ ] T013 [US1] Manual playtest (returning/restarted-player persona): trigger a death/restart
      mid-floor and confirm the beat sequence resumes without disruptive repetition of
      already-completed beats (spec.md US1 Acceptance Scenario 2, SC-002)

**Checkpoint**: User Story 1 is independently complete — all five beats are placed, ordered,
skip-proof, and restart-safe.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T014 [P] Create `Assets/Tests/EditMode/Floor52/OnboardingBeatSequencerTests.cs`: EditMode
      tests for `OnboardingBeatSequencer` covering in-order completion succeeding, out-of-order
      completion being rejected, and restart resetting to the last completed boundary — pure C#,
      no scene load required
- [ ] T015 Review the completed beat sequence against spec.md's Functional Requirements and
      Success Criteria; record the GDD Ch. 15.4 table mapping and playtest results (T012–T013) in
      this spec's `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1 (Phase 3)** depends only on Foundational; its five beat-placement tasks
  (T005–T009) are `[P]` (different trigger volumes) but T010–T013 depend on all five existing.
- **Polish (Phase 4)**: after User Story 1.

```text
Setup (T001-T003)
   ↓
Foundational (T004)
   ↓
US1 beats (T005-T009, parallel)
   ↓
US1 order/restart guards (T010-T011)
   ↓
US1 playtests (T012-T013)
   ↓
Polish (T014-T015)
```

## Notes

- [P] tasks (T005–T009) touch different trigger volumes with no dependency on an incomplete task.
- T006–T009 are explicitly cross-spec coordination points with 003/004/005 — mark them done once
  the corresponding sibling spec's content is placed at the same anchor, not before.
- This spec's only pure-logic C# is `OnboardingBeat` and `OnboardingBeatSequencer` — the rest is
  Unity Editor trigger-volume placement in `Assets/Scenes/Floor52.unity`, validated by the manual
  playtests above per constitution Principle IV's exception.
