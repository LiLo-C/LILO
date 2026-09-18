---
description: "Task list for Floor 52 Distant Monster Hint Audio"
---

# Tasks: Floor 52 Distant Monster Hint Audio

**Input**: Design documents from
`specs/scenes/004-floor-52-scene/004-distant-monster-hint-audio/`

**Prerequisites**: [spec.md](./spec.md); the `DistantZone` dressing element from
`001-level-layout-and-geometry` (T016) must already exist in `Assets/Scenes/Floor52.unity`.

**Tests**: The scene audit for zero Monster/detection components is structural and
EditMode-checkable (Phase 4). Whether the cue reads as directional/sparse/off-screen rather than
an active chase is level-design and audio judgment that needs a live scene and human listening,
so per constitution Principle IV's exception that is validated by the manual walkthroughs in
Phase 3, not by an automated assertion.

**Organization**: Tasks are grouped by the single user story from spec.md.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/areas, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Review `specs/systems/audio/001-sound-event-taxonomy/spec.md` and select the taxonomy
      event corresponding to GDD Ch. 14.1's Monster "distant sound" asset for use as this floor's
      hint; only add a new taxonomy event if that spec does not yet cover one
- [ ] T002 Create `Assets/Tests/EditMode/Floor52/` folder for this spec's tests if it does not
      already exist (may already exist from a sibling Floor 52 spec)

**Checkpoint**: Taxonomy event selected; ready to build the emitter.

---

## Phase 2: Foundational — Distant Threat Emitter Component

**⚠️ MUST complete before the user story below.**

- [ ] T003 Confirm the `DistantZone` dressing element placed by `001-level-layout-and-geometry`
      T016 exists in `Assets/Scenes/Floor52.unity` — this spec places its audio emitter at or
      near that element, not a new location
- [ ] T004 Create `Assets/Scripts/MonoBehaviours/Floor52/DistantThreatAudioEmitter.cs`: a thin
      `MonoBehaviour` wrapping a spatialized `AudioSource` configured for the taxonomy event
      selected in T001, with a sparse, silence-dominant timing pattern (GDD Ch. 14.2 "PATROL/jauh:
      Nyaris hening") — no detection logic and no reference to any Monster type (constitution
      Principle II)

**Checkpoint**: Emitter component exists; ready to place in the scene.

---

## Phase 3: User Story 1 - Sense Danger Without a Monster (Priority: P2)

**Goal**: Floor 52 foreshadows the threat through one distant, directional audio hint while
containing no physical `Monster`.

**Independent Test**: Inspect scene objects and play from multiple listener positions, verifying
source direction and no chase.

### Implementation for User Story 1

- [ ] T005 [US1] Place a `DistantThreatAudioEmitter` `GameObject` at/near the `DistantZone`
      element in `Assets/Scenes/Floor52.unity`, oriented and positioned so its sound reads
      directionally as coming from elsewhere in the building rather than the player's location
      (spec.md FR-002)
- [ ] T006 [US1] Configure the emitter's spatial blend, min/max distance, and sparse trigger
      interval so the cue never becomes loud or frequent enough to be mistaken for an active
      chase (spec.md FR-002, US1 Independent Test)
- [ ] T007 [US1] Verify the emitter has no hook into `specs/systems/noise-and-detection` and no
      progression/objective dependency — the floor must be completable whether or not the player
      ever hears the cue (spec.md FR-003)
- [ ] T008 [US1] Manual walkthrough: play from at least 3 different listener positions across
      `ExplorationZone` and `ChaseSection`, confirming the cue's perceived direction is consistent
      with the `DistantZone` element's location from each (spec.md Independent Test)
- [ ] T009 [US1] Informal playtest: ask playtesters whether they sensed an off-screen threat
      without believing an active chase was occurring, and record their responses (spec.md
      SC-002)

**Checkpoint**: User Story 1 is independently complete — the hint is placed, directional, sparse,
and free of any Monster/progression coupling.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [ ] T010 [P] Create `Assets/Tests/EditMode/Floor52/Floor52MonsterAudioAuditTests.cs`: an
      EditMode test that opens `Assets/Scenes/Floor52.unity` additively and asserts zero
      components of any Monster/detection type (per `specs/systems/monster-ai`) exist anywhere in
      the scene (spec.md FR-001, SC-001)
- [ ] T011 Log the selected audio asset's CC0/license info (if applicable) in
      `specs/systems/audio/003-audio-asset-sourcing-and-licensing-log`, per GDD Ch. 16's sourcing
      rule, if not already logged by the taxonomy spec
- [ ] T012 Run the scene audit and directionality/timing validation as a single documented pass
      and record the result in this spec's `checklists/requirements.md` Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)** → **User Story 1 (Phase 3)**: sequential.
- **Polish (Phase 4)**: after User Story 1.

```text
Setup (T001-T002)
   ↓
Foundational (T003-T004)
   ↓
US1 (T005-T009)
   ↓
Polish (T010-T012)
```

## Notes

- This spec's only new C# is the thin `DistantThreatAudioEmitter` adapter — the rest is Unity
  Editor audio-source placement and tuning in `Assets/Scenes/Floor52.unity`, validated by the
  manual walkthroughs above and the one structural EditMode test in T010, per constitution
  Principle IV's exception.
- T010's audit and `001`'s Assumptions both establish that Floor 52 contains zero `Monster`
  objects by construction — keep this audit passing as a regression guard against any future
  accidental addition.
