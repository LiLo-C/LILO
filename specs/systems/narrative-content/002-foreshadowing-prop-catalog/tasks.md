---

description: "Task list for the Foreshadowing Prop Catalog content spec"

---

# Tasks: Foreshadowing Prop Catalog

**Input**: Design documents from `/specs/systems/narrative-content/002-foreshadowing-prop-catalog/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: Not applicable — this feature is a content bible (constitution Principle IV's
EditMode-test requirement applies to pure-logic systems; a content bible has none). Its
"acceptance scenarios" are consistency-review checks, executed as the cross-check tasks below.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P1, P2).
Each story splits into (a) authoring tasks that produced this spec's content — already complete,
marked `[X]` — and (b) downstream cross-check tasks against consuming specs that don't exist yet,
left `[ ]` pending until those specs have a draft to check against.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different sections/files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)
- File paths and section names are exact

## Storage decision

This catalog's content lives entirely inside `spec.md` (its Key Entities and Functional
Requirements sections) rather than in a separate companion asset such as
`Assets/Config/ForeshadowingCatalog.md`. Reasoning: identical to `001-eddie-character-bible`'s
storage decision (see that spec's `tasks.md`) — per constitution Principle II, nothing in this
project's Unity runtime ever needs to load this catalog's text at runtime; it is consulted by
people (writers, level/audio designers, reviewers) during authoring and review, not by code during
play. A future spec is free to introduce a runtime-loadable narrative-data asset if localization
or an in-game archive ever needs one, but that would be its own spec with its own justification,
not a default of this one.

---

## Phase 1: Setup

- [X] T001 Record the storage-location decision above in spec.md's Assumptions section (done — see
      "This spec introduces no new `GameConfig` fields...").

**Checkpoint**: Storage approach decided before any catalog content is written.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: Extract the raw source material both P1 user stories draw from.

- [X] T002 [P] Read GDD `LILO-GDD-v2-Production-Lock.md` Ch. 10.3 in full and extract the eight-item
      foreshadowing list verbatim plus its placement rule ("Sebar sebagian dari daftar ini di
      ketiga floor. Tidak perlu semua dipakai --- yang penting konsisten dan tidak pernah
      dijelaskan lewat teks.") — recorded in spec.md's Key Entities (FS-01–FS-08) and FR-001–FR-003,
      FR-006.
- [X] T003 [P] Read GDD Ch. 14.1 in full and extract the "Naratif" audio asset row (bel lift,
      langkah kaki menyerupai atasan, tertawaan rekan kerja samar) that cites Ch. 10.3 — recorded in
      spec.md's FR-008 as the audio-asset-sourcing cross-reference for FS-01–FS-03.

**Checkpoint**: Source material extracted; user-story-specific FRs can now be written.

---

## Phase 3: User Story 1 - Level designer selects foreshadowing elements for a floor (Priority: P1)

**Goal**: A level designer working on a floor scene spec can select from a fixed, validated list
of foreshadowing elements without inventing new lore on the spot.

**Independent Test**: Take a floor scene spec's proposed placement list, confirm every proposed
element traces to one of the entries in Key Entities, and confirm no proposed element carries an
explanatory label — checkable from spec.md and the floor draft alone.

### Authoring (complete)

- [X] T004 [P] [US1] Write FR-001 (catalog contains exactly the eight FS-01–FS-08 entries; an
      uncataloged element requires amending this spec first) in spec.md § Functional Requirements.
- [X] T005 [P] [US1] Write FR-002 (a floor MAY select any subset, including none) in spec.md §
      Functional Requirements, citing GDD Ch. 10.3's "Tidak perlu semua dipakai."
- [X] T006 [P] [US1] Write FR-003 (selected entries MUST be spread across the three floors, not
      concentrated on one) in spec.md § Functional Requirements, citing GDD Ch. 10.3's "Sebar
      sebagian dari daftar ini di ketiga floor."
- [X] T007 [P] [US1] Write FR-004 (any entry reused across floors or within one floor MUST be
      portrayed identically to its fixed Key Entities definition every time) in spec.md §
      Functional Requirements.
- [X] T008 [P] [US1] Write the eight Key Entities catalog rows (FS-01–FS-08: ID, GDD source line,
      category, one-line narrative intent) in spec.md § Key Entities.
- [X] T009 [P] [US1] Write the "reused entry across/within floors" and "zero entries used on a
      floor" Edge Cases in spec.md § Edge Cases, cross-referencing FR-004 and FR-002 respectively.

### Downstream cross-checks (pending — blocked on specs that don't exist yet)

- [ ] T010 [US1] Cross-check `specs/scenes/004-floor-52-scene/`'s proposed foreshadowing placement
      list against FR-001–FR-004 and FS-01–FS-08 once that spec (any of its 001–005 sub-specs
      proposing environmental/audio dressing) has a draft. **Blocked**: scene 004 not yet written.
- [ ] T011 [US1] Cross-check `specs/scenes/005-floor-51-scene/`'s proposed foreshadowing placement
      list against FR-001–FR-004 and FS-01–FS-08 once that spec has a draft. **Blocked**: scene 005
      not yet written.
- [ ] T012 [US1] Cross-check `specs/scenes/006-floor-50-scene/`'s proposed foreshadowing placement
      list against FR-001–FR-004 and FS-01–FS-08 once that spec has a draft. **Blocked**: scene 006
      not yet written.
- [ ] T013 [US1] Confirm each of the three floor scene specs above spreads its selected entries
      (if any) across the three floors per FR-003, once all three drafts exist to compare side by
      side (a single floor's draft alone cannot confirm "spread," only "not internally
      contradictory"). **Blocked**: scenes 004/005/006 not yet written.

**Checkpoint**: User Story 1's rule set is fully authored; cross-checks activate once consuming
floor scene specs exist.

---

## Phase 4: User Story 2 - Audio designer implements a narrative sound cue (Priority: P1)

**Goal**: An audio designer implementing the "Naratif" sound assets (GDD Ch. 14.1) can look up the
precise narrative intent behind each of the three audio-category entries.

**Independent Test**: Take a produced or placeholder sound asset, check its described trigger and
intent against the matching catalog entry, and confirm the cue is used consistently with that
entry's definition wherever it appears.

### Authoring (complete)

- [X] T014 [P] [US2] Write FR-005 (a catalog entry's narrative intent MUST remain singular and MUST
      NOT be reused for an unrelated purpose, e.g. a generic UI sound) in spec.md § Functional
      Requirements.
- [X] T015 [P] [US2] Write FR-008 (the three audio-category entries draw their assets from the
      GDD Ch. 14.1 "Naratif" taxonomy; this catalog assigns meaning, it does not duplicate asset
      production) in spec.md § Functional Requirements, cross-referencing
      `audio/001-sound-event-taxonomy`.
- [X] T016 [US2] Write the "future audio spec wants the monster's real detection-state sound to
      closely resemble the elevator-bell cue" Edge Case in spec.md § Edge Cases, cross-referencing
      FR-005 and `audio/002-dynamic-mix-state-machine`.

### Downstream cross-checks (pending — blocked on specs that don't exist yet)

- [ ] T017 [US2] Cross-check `specs/systems/audio/001-sound-event-taxonomy/`'s produced "Naratif"
      asset list against FS-01, FS-02, FS-03's defined intents once that spec has a draft.
      **Blocked**: audio/001 not yet written.
- [ ] T018 [US2] Cross-check `specs/systems/audio/002-dynamic-mix-state-machine/`'s monster
      detection-state sound design against the Edge Case from T016 (resemblance to FS-01 should
      support, not conflict with, the catalog) once that spec has a draft. **Blocked**: audio/002
      not yet written.

**Checkpoint**: User Story 2's rule set is fully authored; cross-checks activate once the audio
specs exist.

---

## Phase 5: User Story 3 - Reviewer audits the "never explained" rule at playtest (Priority: P2)

**Goal**: A narrative reviewer can confirm, from playing the shipped build alone, that no
foreshadowing element was ever explained via on-screen text, subtitle, or dialogue.

**Independent Test**: Play through all three floors, note every foreshadowing element encountered,
and confirm none is accompanied by explanatory text/dialogue/UI.

### Authoring (complete)

- [X] T019 [US3] Write FR-006 (no catalog entry may be explained/labeled/annotated via on-screen
      text, subtitle, tooltip, or dialogue stating its meaning; literal-sensory captioning for
      accessibility is not a violation) in spec.md § Functional Requirements, citing GDD Ch. 10.3's
      "tidak pernah dijelaskan lewat teks."
- [X] T020 [US3] Write the accessibility-captioning Edge Case (captioning literal sound/sight is
      allowed; captioning or naming narrative meaning is not) in spec.md § Edge Cases,
      cross-referencing FR-006.

### Downstream cross-checks (pending — blocked on a build that doesn't exist yet)

- [ ] T021 [US3] Play through all three floors of a build with foreshadowing elements implemented
      and confirm zero instances of on-screen text/subtitle/tooltip/dialogue stating any entry's
      narrative meaning (SC-002), per `specs/release/002-playtest-and-bug-priority-process/`.
      **Blocked**: floor scenes and a playable build do not exist yet.
- [ ] T022 [US3] Re-run T021 at the Release scope-lock gate
      (`specs/release/001-scope-lock-and-cut-order/`) as the final pre-submission check (SC-002).
      **Blocked**: same as T021, plus the release spec itself not yet written.
- [ ] T023 [US3] Recruit at least 2 people outside the writing/design team to play all three floors
      in one sitting and confirm at least one can name a foreshadowing element they noticed
      unprompted, describing it as deliberate rather than random, without being told the catalog
      exists (SC-005). **Blocked**: floor scenes not yet written.

**Checkpoint**: User Story 3's rule set is fully authored; cross-checks activate once a playable
build with foreshadowing elements exists.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T024 [P] Write FR-007 (every catalog entry used MUST remain consistent with, and MUST NOT
      contradict, `001-eddie-character-bible`'s Central Thematic Through-Line) in spec.md §
      Functional Requirements.
- [X] T025 [P] Write FR-009 (per-floor placement decision is explicitly OUT of scope for this spec;
      it belongs to each floor scene spec) in spec.md § Functional Requirements, naming
      `specs/scenes/004-floor-52-scene/`, `005-floor-51-scene/`, `006-floor-50-scene/` by path.
- [X] T026 Write the "GDD amended with new/removed foreshadowing ideas post-launch" Edge Case in
      spec.md § Edge Cases, citing GDD Ch. 21 and constitution Principle V.
- [X] T027 Record SC-001, SC-003, SC-004 (the traceability, identical-portrayal, and catalog-
      exhaustiveness checks that run at each floor scene spec's own design review) in spec.md §
      Success Criteria.
- [X] T028 Record SC-002 and SC-005 (the two checks that run at Release/playtest —
      `specs/release/001-scope-lock-and-cut-order/` and `specs/release/002-playtest-and-bug-priority-
      process/`) in spec.md § Success Criteria.
- [ ] T029 Re-run T010–T013, T017, T018, T021–T023 whenever a consuming spec's narrative or audio
      draft changes materially, not only once at first draft — ongoing; has no single "done" state,
      tracked at each consuming spec's own review pass instead.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup; blocks all three user stories.
- **User Stories (Phase 3–5)**: Each depends on Foundational. Authoring sub-tasks within a story
  have no cross-story dependency and are already complete. Cross-check sub-tasks depend on the
  named external spec (or build) existing, not on each other.
- **Polish (Phase 6)**: T024–T028 depend on Phases 3–5's authoring being complete (they reference
  FR/SC numbers already written, and T024 depends on `001-eddie-character-bible`'s Central
  Thematic Through-Line already being defined, per spec.md's Assumptions). T029 is ongoing and
  depends on Phase 3–5's cross-check tasks having first run at least once.

### Parallel Opportunities

- T002/T003 (Foundational) can run in parallel — different GDD chapters, no shared output.
- T004–T009 (US1 authoring) can run in parallel — each writes a distinct FR, the Key Entities
  table, or a distinct Edge Case.
- T010/T011/T012 (US1 cross-checks) can run in parallel once each named floor scene spec exists;
  T013 depends on all three having landed first.
- T014/T015 (US2 authoring) can run in parallel — distinct FRs; T016 depends on FR-005 (T014)
  existing to cross-reference.
- T017/T018 (US2 cross-checks) can run in parallel once both target specs exist.
- T024/T025 (Polish authoring) can run in parallel — distinct FRs.

## Notes

- Every "implementation" task in this feature is a writing task against `spec.md`'s own sections
  — there is no separate source file to keep in sync, per the Storage decision above.
- The pending (`[ ]`) tasks are not stalled work on this spec's part; they are correctly blocked on
  specs the ROADMAP places later in the build order (scenes come after all systems, and audio's own
  sub-specs are peers, not predecessors, of this one — per ROADMAP.md §5). Do not check them off
  until the named spec or build exists and has been checked.
- If GDD Ch. 10.3 or 14.1 is ever amended (e.g., an entry added, removed, or reworded), re-open
  this tasks.md: add a new T0xx under Phase 2 to extract the change, then a new T0xx under the
  affected user story to update the corresponding FR/Key Entity, per constitution Principle V —
  matching the pattern already used by `001-eddie-character-bible`'s tasks.md.
