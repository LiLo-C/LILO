---
description: "Task list for Floor 52 Static Battery and Hiding Placement"
---

# Tasks: Floor 52 Static Battery and Hiding Placement

**Input**: Design documents from
`specs/scenes/004-floor-52-scene/003-static-battery-and-hiding-placement/`

**Prerequisites**: [spec.md](./spec.md); the `BatteryCandidate_1`–`BatteryCandidate_N` and
`HidingSpotCandidate` placeholder anchors from `001-level-layout-and-geometry` must already exist
in `Assets/Scenes/Floor52.unity`.

**Tests**: Object counts and component presence are structural and EditMode-checkable (Phase 4).
Reachability, spread, and "no trap" judgment need a live scene and a human walkthrough, so per
constitution Principle IV's exception those are validated by the manual walkthroughs in Phase 2,
not by an automated assertion.

**Organization**: Tasks are grouped by the single user story from spec.md, with a separate task
per placed `Battery` object and the one `HidingSpot` object.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files/objects, no dependency on an incomplete task)
- **[Story]**: US1
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Confirm the `BatteryCandidate_1`…`BatteryCandidate_4` and `HidingSpotCandidate`
      placeholder `GameObject`s from `001-level-layout-and-geometry` exist in
      `Assets/Scenes/Floor52.unity`; this spec instantiates real objects at those exact anchors
      only and does not add or remove candidate slots (spec.md Scope)
- [ ] T002 Create `Assets/Tests/EditMode/Floor52/` folder for this spec's tests if it does not
      already exist (may already exist from a sibling Floor 52 spec)

**Checkpoint**: Candidate anchors confirmed; ready to instantiate real objects.

---

## Phase 2: User Story 1 - Find the Lesson Objects (Priority: P1)

**Goal**: Floor 52 contains 3–5 static batteries and exactly one on-path hiding spot placed to
teach, not gate, the run.

**Independent Test**: Inspect the scene and walk the critical path, counting reachable objects
and verifying no respawn occurs.

### Implementation for User Story 1

- [ ] T003 [P] [US1] Instantiate a static `Battery` object (per
      `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot`'s contract) at the
      `BatteryCandidate_1` anchor in `Assets/Scenes/Floor52.unity`, configured with no
      respawn/despawn behavior (spec.md FR-001)
- [ ] T004 [P] [US1] Instantiate a static `Battery` object at the `BatteryCandidate_2` anchor in
      `Assets/Scenes/Floor52.unity`, same static/no-respawn configuration as T003
- [ ] T005 [P] [US1] Instantiate a static `Battery` object at the `BatteryCandidate_3` anchor in
      `Assets/Scenes/Floor52.unity`, same static/no-respawn configuration as T003
- [ ] T006 [P] [US1] Instantiate a static `Battery` object at the `BatteryCandidate_4` anchor in
      `Assets/Scenes/Floor52.unity`, same static/no-respawn configuration as T003, settling the
      3–5 range at 4 total (spec.md FR-001) — record the chosen count (4) in this spec's spec.md
      Assumptions if not already noted
- [ ] T007 [US1] Instantiate the `HidingSpot` object (per
      `specs/systems/hiding/001-enter-and-exit-hiding`'s contract) at the `HidingSpotCandidate`
      anchor in `Assets/Scenes/Floor52.unity`, confirming it sits on the unavoidable critical path
      and not inside any optional side loop (spec.md FR-002)
- [ ] T008 [US1] Verify none of the 4 `Battery` objects or the `HidingSpot` object create a
      key/door dependency or an unsafe trap — no locked-door reference, no dead-end without
      egress (spec.md FR-003)
- [ ] T009 [US1] Manual walkthrough: inspect the scene and walk the critical path once, counting
      reachable `Battery` objects (must total 4, matching T003–T006) and confirming the
      `HidingSpot` is reachable without detour (spec.md Independent Test)
- [ ] T010 [US1] Manual walkthrough: verify placement spread — confirm the 4 `Battery` objects sit
      across more than one side of the map and are not clustered on one side (spec.md FR-001,
      SC-001)
- [ ] T011 [US1] Manual walkthrough: play through the floor across two separate runs (including
      one death/restart) and confirm no `Battery` object ever respawns or duplicates during a
      single floor run (spec.md SC-002)

**Checkpoint**: User Story 1 is independently complete — all lesson objects are placed, counted,
reachable, spread, and trap-free.

---

## Phase 3: Polish & Cross-Cutting Concerns

- [ ] T012 [P] Create `Assets/Tests/EditMode/Floor52/Floor52BatteryAndHidingPlacementTests.cs`: an
      EditMode test that opens `Assets/Scenes/Floor52.unity` additively and asserts exactly 4
      static `Battery` components and exactly 1 `HidingSpot` component exist in the scene, as a
      structural regression guard for FR-001/FR-002 counts
- [ ] T013 Run the placement audit (count, reachability, spread checks per spec.md SC-001) as a
      single documented pass and record the result in this spec's `checklists/requirements.md`
      Notes section

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **User Story 1 (Phase 2)**: sequential.
- Within User Story 1, T003–T006 are `[P]` (different `Battery` instances); T007 is independent of
  those four; T008–T011 depend on all five objects (T003–T007) existing.
- **Polish (Phase 3)**: after User Story 1.

```text
Setup (T001-T002)
   ↓
US1 objects (T003-T007, parallel)
   ↓
US1 validation (T008-T011)
   ↓
Polish (T012-T013)
```

## Notes

- [P] tasks (T003–T006) touch different `Battery` instances with no dependency on an incomplete
  task.
- This spec has no new pure-logic C# — it consumes the existing `Battery`/`HidingSpot` contracts
  from `flashlight-and-battery` and `hiding` and is purely Unity Editor placement in
  `Assets/Scenes/Floor52.unity`, validated by the manual walkthroughs above and the one
  structural EditMode test in T012, per constitution Principle IV's exception.
