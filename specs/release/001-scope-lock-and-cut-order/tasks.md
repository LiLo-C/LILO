---

description: "Task list for release/001-scope-lock-and-cut-order"

---

# Tasks: Scope Lock and Cut Order

**Input**: Design documents from `specs/release/001-scope-lock-and-cut-order/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: This is a process/QA gate, not runtime code — there is no EditMode suite to write. Each task below is itself an auditable check; "passing" means the check returns the stated result against the main branch or a submission build, recorded as evidence for the corresponding Success Criterion in spec.md.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P1, P2). Every Must Have bullet, every Cut List item, and every Cut Order tier gets its own task, per the assignment's instruction to break this maximally small.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (independent audit, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)

---

## Phase 1: Foundational (blocking prerequisites)

- [ ] T001 Read `specs/_reference/LILO-GDD-v2-Production-Lock.md` Ch. 18 (18.1, 18.2, 18.3) and `specs/ROADMAP.md` §0, §2, §3, §4 in full — this gate cannot be audited from memory of a summary.

**Checkpoint**: The GDD and ROADMAP are the confirmed, current source for every task below.

---

## Phase 2: User Story 1 - Must-Have Traceability Gate (Priority: P1)

**Goal**: Every one of the 12 GDD 18.1 Must Have bullets has at least one cited, implemented, tested spec.

**Independent Test**: For each task below, open the cited spec path(s) and confirm the spec exists, its FRs are implemented in `Assets/Scripts`/the relevant Scene, and its EditMode tests are green.

- [ ] T002 [P] [US1] Audit Must Have item 1 (movement + sprint + flashlight + context-sensitive interaction) against `specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md`, `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius/spec.md`, `specs/systems/interaction-and-highlight/001-nearest-interactable-detection/spec.md`, `002-context-sensitive-action-button/spec.md` — record satisfied/not satisfied (spec.md FR-001)
- [ ] T003 [P] [US1] Audit Must Have item 2 (battery system: 1+1 slots, tiered light state, Compact Darkness) against `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius/spec.md`, `002-battery-real-time-drain-timer/spec.md`, `007-battery-pickup-and-spare-slot/spec.md`, `008-battery-install-and-refill/spec.md` (spec.md FR-002)
- [ ] T004 [P] [US1] Audit Must Have item 3 (battery respawn, Floor 51 & 50) against `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md`, `002-active-battery-count-cap/spec.md`, `003-respawn-timer-and-placement-rule/spec.md` (spec.md FR-003)
- [ ] T005 [P] [US1] Audit Must Have item 4 (3 floors + doors + keys + checkpoint) against `specs/scenes/004-floor-52-scene/001-level-layout-and-geometry/spec.md`, `specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/spec.md`, `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry/spec.md`, `specs/systems/keys-and-doors/001-004`, `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md` (spec.md FR-004)
- [ ] T006 [P] [US1] Audit Must Have item 5 (monster with PATROL/INVESTIGATE/CHASE/SEARCH/CATCH) against `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` through `004-catch-outcome-signal/spec.md` (spec.md FR-005)
- [ ] T007 [P] [US1] Audit Must Have item 6 (noise-radius detection) against `specs/systems/noise-and-detection/001-per-action-noise-emission/spec.md`, `002-distance-based-detection-check/spec.md` (spec.md FR-006)
- [ ] T008 [P] [US1] Audit Must Have item 7 (limited hiding, under-desk only) against `specs/systems/hiding/001-enter-and-exit-hiding/spec.md`, `002-hiding-detection-immunity-rule/spec.md`, `003-hiding-audio-and-light-dampening/spec.md` (spec.md FR-007)
- [ ] T009 [P] [US1] Audit Must Have item 8 (progression Floor 52→51→50→Final Door) against `specs/systems/progression-and-scene-flow/001-003`, `specs/systems/keys-and-doors/003-final-door-distinct-behavior/spec.md`, `specs/scenes/006-floor-50-scene/004-final-door-placement-and-trigger/spec.md` (spec.md FR-008)
- [ ] T010 [P] [US1] Audit Must Have item 9 (narrative: prologue + Good ending + Bad ending) against `specs/scenes/003-prologue-scene/spec.md`, `specs/systems/narrative-content/001-eddie-character-bible/spec.md`, `specs/scenes/007-good-ending-scene/spec.md`, `specs/scenes/008-bad-ending-scene/spec.md` (spec.md FR-009)
- [ ] T011 [P] [US1] Audit Must Have item 10 (UX: pause, restart, audio settings, How To Play, interaction feedback) against `specs/systems/game-shell-ui/001-004`, `specs/systems/interaction-and-highlight/002-003` — explicitly confirm the pause-menu spec's implementation includes a restart-run action, not only pause/resume (spec.md FR-010)
- [ ] T012 [P] [US1] Audit Must Have item 11 (audio: ambience, player SFX, dynamic monster SFX, chase SFX) against `specs/systems/audio/001-sound-event-taxonomy/spec.md`, `002-dynamic-mix-state-machine/spec.md` (spec.md FR-011)
- [ ] T013 [P] [US1] Audit Must Have item 12 (haptic feedback) against `specs/systems/haptics/001-haptic-trigger-events/spec.md` (spec.md FR-012)

**Checkpoint**: All 12 Must Have items have a recorded satisfied/not-satisfied status (SC-001).

---

## Phase 3: User Story 2 - Cut List Negative-Requirement Audit (Priority: P1)

**Goal**: Zero references to any of the 13 GDD 18.2 Cut List items exist in the main branch.

**Independent Test**: For each task below, run the stated code/asset/design search against main and confirm zero matches.

- [ ] T014 [P] [US2] Code-search `Assets/Scripts` for a complex multi-slot inventory system beyond the single key/battery-slot model — confirm zero matches (spec.md FR-013)
- [ ] T015 [P] [US2] Review level-design docs and Scene content for any puzzle mechanic beyond key-finds-door — confirm zero matches (spec.md FR-014)
- [ ] T016 [P] [US2] Audit `specs/systems/monster-ai/` and monster prefabs/assets for a second monster type or enemy variant — confirm zero matches (spec.md FR-015)
- [ ] T017 [P] [US2] Code-search for procedural level-generation logic — confirm every floor's geometry is authored fixed content (spec.md FR-016)
- [ ] T018 [P] [US2] Audit `specs/systems/hiding/` and every floor Scene's hiding-spot placement spec for any hiding interactable other than under-desk — confirm zero matches (spec.md FR-017)
- [ ] T019 [P] [US2] Code-search for combat (attack/damage-to-monster), skill/ability, or crafting systems — confirm zero matches (spec.md FR-018)
- [ ] T020 [P] [US2] Review narrative content and Scene list for implemented Secret Ending content reachable from gameplay — confirm zero matches (spec.md FR-019)
- [ ] T021 [P] [US2] Review narrative content for player-choice-driven story forks beyond the fixed Good/Bad split — confirm zero matches (spec.md FR-020)
- [ ] T022 [P] [US2] Code/asset-search for multiple carried weapons or tools beyond the flashlight/battery system — confirm zero matches (spec.md FR-021)
- [ ] T023 [P] [US2] Code-search `specs/systems/noise-and-detection/` and `specs/systems/flashlight-and-battery/` for flashlight-triggered monster detection — confirm zero matches (spec.md FR-022)
- [ ] T024 [P] [US2] Code-search `Assets/Scripts` for a stamina bar/meter/depletion field on `PlayerCharacter` or `GameState` — confirm zero matches (spec.md FR-023)
- [ ] T025 [P] [US2] Review sprint and battery-drain logic for any coupling between sprinting and battery drain rate — confirm zero matches (spec.md FR-024)
- [ ] T026 [P] [US2] Inventory the actual Scene list against `specs/ROADMAP.md` §0's fixed eight scenes — confirm exactly eight scenes and zero fourth-floor scene (spec.md FR-025)

**Checkpoint**: All 13 Cut List audits return zero matches (SC-002).

---

## Phase 4: User Story 3 - Cut-Order Decision Tree Execution (Priority: P2)

**Goal**: If scope must shrink, cuts are applied in the pre-agreed order, protected elements are never touched, and any deviation is documented.

**Independent Test**: Walk the tiers in order during a simulated time-pressure review; confirm protected elements survive every tier; confirm any off-list cut carries dual sign-off.

- [ ] T027 [US3] Confirm/apply Cut-Order Tier 1 (remove hiding system) only if scope reduction is actually required, and only before any later tier is considered (spec.md FR-026)
- [ ] T028 [US3] Confirm/apply Cut-Order Tier 2 (battery respawn → static placement) only after Tier 1 has been considered/applied and found insufficient (spec.md FR-026)
- [ ] T029 [US3] Confirm/apply Cut-Order Tier 3 (remove Floor 50, ship a 2-floor run) only after Tier 2 has been considered/applied and found insufficient — record that this is a pre-agreed, documented amendment to Must Have item 4 (spec.md Edge Cases, FR-004/FR-026)
- [ ] T030 [US3] Confirm/apply Cut-Order Tier 4 (3D character/monster models → 4-direction 2D sprites via `characterRenderMode`) only after Tier 3 has been considered/applied and found insufficient (spec.md FR-026)
- [ ] T031 [US3] After any tier from T027–T030 is applied, re-verify all three Protected Elements are intact: one fully complete floor start-to-exit, a working monster with full state machine, both endings reachable (spec.md FR-027, FR-028)
- [ ] T032 [US3] Confirm any scope cut not on the GDD 18.3 list, or applied out of order, carries documented Tech Lead + Design Lead sign-off before being applied — reject and flag any that doesn't (spec.md FR-029)

**Checkpoint**: Cut-order execution, if invoked, is fully in-order, protected-element-safe, and documented (SC-003, SC-004).

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T033 Re-run T002–T026 (the full Must Have and Cut List audits) against the literal Phase 8 (Submission Lock) build artifact — a pass recorded at an earlier phase does not carry forward automatically (spec.md FR-030, SC-005)
- [ ] T034 [P] Archive the completed audit (T002–T033 results) as submission evidence alongside the build

---

## Dependencies & Execution Order

- **Foundational (Phase 1)**: strictly before all audits — T001 establishes the current GDD/ROADMAP text every other task cites.
- **User Story 1 (P1, T002–T013)** and **User Story 2 (P1, T014–T026)**: both depend only on Foundational; independent of each other and of US3; every task within each is independently parallelizable ([P]).
- **User Story 3 (P2, T027–T032)**: depends on nothing from US1/US2 to *start*, but T031's protected-element re-check implicitly depends on knowing the current Must Have/Cut List status from US1/US2 if a cut changes that status (e.g., Tier 3 changes Must Have item 4's scope).
- **Polish (Phase 5)**: after all three stories; T033 re-runs US1+US2 tasks verbatim against the final build.
