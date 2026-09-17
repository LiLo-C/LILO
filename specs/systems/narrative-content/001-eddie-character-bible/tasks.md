---

description: "Task list for the Eddie Character Bible content spec"
---

# Tasks: Eddie Character Bible

**Input**: Design documents from `/specs/systems/narrative-content/001-eddie-character-bible/`

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

This bible's content lives entirely inside `spec.md` (its Key Entities and Functional
Requirements sections) rather than in a separate companion asset such as
`Assets/Config/NarrativeContentBible.md`. Reasoning: per constitution Principle II (no system
introduced ahead of a driving requirement), nothing in this project's Unity runtime ever needs to
load this bible's text at runtime — it is consulted by people (writers, designers, reviewers)
during authoring and review, not by code during play. A future spec is free to introduce a
runtime-loadable narrative-data asset if one becomes necessary (e.g., for localization), but that
would be its own spec with its own justification, not a default of this one.

---

## Phase 1: Setup

- [X] T001 Record the storage-location decision above in spec.md's Assumptions section (done —
      see "This spec introduces no new `GameConfig` fields...").

**Checkpoint**: Storage approach decided before any trait content is written.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: Extract the raw source material both user stories below draw from.

- [X] T002 [P] Read GDD `LILO-GDD-v2-Production-Lock.md` Ch. 10.1 in full and extract the raw
      trait/fact list (underpaid, overworked, clumsy, cautious/over-prepared, work-focused, sees a
      psychologist, the central through-line) — recorded in spec.md's Key Entities section.
- [X] T003 [P] Read GDD Ch. 1.2 in full and extract the tone guardrail (horror from limited vision
      and uncertainty, not jumpscare/violence) — recorded in spec.md's Key Entities "Tone
      Guardrail" entry and used by FR-008.

**Checkpoint**: Source material extracted; user-story-specific FRs can now be written.

---

## Phase 3: User Story 1 - Writer drafts new Eddie dialogue or narration (Priority: P1)

**Goal**: A writer can check any drafted line against a fixed, short rule set without re-reading
the GDD each time.

**Independent Test**: Take a drafted line, check it against FR-001, FR-002, FR-004, FR-005,
FR-008 in spec.md, and produce a pass/fail note per trait.

### Authoring (complete)

- [X] T004 [P] [US1] Write FR-001 (underpaid/overworked constraint) in spec.md § Functional
      Requirements.
- [X] T005 [P] [US1] Write FR-002 (clumsy-remains-standing-trait constraint) in spec.md §
      Functional Requirements.
- [X] T006 [P] [US1] Write FR-004 (work-focus competence coexists with clumsiness) in spec.md §
      Functional Requirements.
- [X] T007 [P] [US1] Write FR-005 (psychologist fact: omission allowed, contradiction not) in
      spec.md § Functional Requirements.
- [X] T008 [P] [US1] Write FR-008 (tone guardrail: hardship played straight, not for comedy) in
      spec.md § Functional Requirements, cross-referencing GDD Ch. 1.2.

### Downstream cross-checks (pending — blocked on specs that don't exist yet)

- [ ] T009 [US1] Cross-check `specs/scenes/003-prologue-scene/spec.md`'s drafted script against
      FR-001, FR-002, FR-004, FR-005, FR-008 once that spec has a draft. **Blocked**: spec 003 not
      yet written.
- [ ] T010 [US1] Cross-check `specs/scenes/007-good-ending-scene/spec.md` and
      `specs/scenes/008-bad-ending-scene/spec.md`'s drafted scripts against the same FRs once those
      specs have a draft. **Blocked**: specs 007/008 not yet written.

**Checkpoint**: User Story 1's rule set is fully authored; cross-checks activate once consuming
specs exist.

---

## Phase 4: User Story 2 - Level designer justifies an environmental-storytelling prop (Priority: P1)

**Goal**: A level designer can justify Eddie-owned props against a fixed three-item list instead
of re-deriving it from the GDD per floor.

**Independent Test**: Take a proposed prop, check it against FR-003's locked three-item list, and
approve or reject without consulting anyone else.

### Authoring (complete)

- [X] T011 [US2] Write FR-003 (desk locker inventory: exactly first-aid kit, emergency food,
      handheld emergency lamp) in spec.md § Functional Requirements.
- [X] T012 [US2] Record the flashlight-origin cross-reference (GDD Ch. 5.3 / Ch. 1.1 — the locker
      lamp is the same object that becomes the player's flashlight) inside FR-003, so the mechanic
      -facing dependency is visible from this bible, not just the narrative one.

### Downstream cross-checks (pending — blocked on specs that don't exist yet)

- [ ] T013 [US2] Cross-check each floor scene spec's proposed Eddie-owned props —
      `specs/scenes/004-floor-52-scene/`, `specs/scenes/005-floor-51-scene/`,
      `specs/scenes/006-floor-50-scene/` — against FR-003 once those specs have a draft.
      **Blocked**: specs 004/005/006 not yet written.

**Checkpoint**: User Story 2's rule set is fully authored; cross-checks activate once floor scene
specs exist.

---

## Phase 5: User Story 3 - Reviewer audits the central thematic through-line (Priority: P2)

**Goal**: A reviewer can judge whether a scene supports, contradicts, or ignores the dream/reality
equivalence and the shrinking-life metaphor, using only this bible and the scene's draft.

**Independent Test**: Read or watch a drafted scene and answer "supports / contradicts / ignores"
against FR-006 and FR-007, without the full game needing to exist.

### Authoring (complete)

- [X] T014 [US3] Write FR-006 (dream-office/real-office equivalence; shrinking-FOV/shrinking-life
      metaphor) in spec.md § Functional Requirements, citing GDD Ch. 10.1's "benang merah yang
      harus terasa."
- [X] T015 [US3] Write FR-007 (through-line must stay implicit, never stated via on-screen
      text/narration) in spec.md § Functional Requirements, cross-referencing the implicit-only
      rule GDD Ch. 10.3 states for foreshadowing.

### Downstream cross-checks (pending — blocked on specs that don't exist yet)

- [ ] T016 [US3] Cross-check `specs/scenes/003-prologue-scene/`, `specs/scenes/007-good-ending-scene/`,
      and `specs/scenes/008-bad-ending-scene/` for through-line legibility (spec.md SC-003) once
      those specs have a first readable/playable draft. **Blocked**: specs 003/007/008 not yet
      written.

**Checkpoint**: All three user stories' rule sets are fully authored in spec.md; every downstream
cross-check is queued and will activate as each consuming spec is drafted.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T017 [P] Write FR-009 (every consuming spec must cite this one in its own Related section)
      in spec.md § Functional Requirements, so the dependency stays traceable.
- [X] T018 Record SC-002 and SC-004 (the two checks that run at the Release scope-lock gate,
      `specs/release/001-scope-lock-and-cut-order/`) in spec.md § Success Criteria.
- [ ] T019 Re-run T009, T010, T013, T016 whenever a consuming spec's narrative draft changes
      materially, not only once at first draft — ongoing; has no single "done" state, tracked at
      each consuming spec's own review pass instead.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup; blocks all three user stories.
- **User Stories (Phase 3–5)**: Each depends on Foundational. Authoring sub-tasks within a story
  have no cross-story dependency and are already complete. Cross-check sub-tasks depend on the
  named external spec existing, not on each other.
- **Polish (Phase 6)**: T017/T018 depend on Phases 3–5's authoring being complete (they reference
  FR numbers already written). T019 is ongoing and depends on Phase 3–5's cross-check tasks having
  first run at least once.

### Parallel Opportunities

- T002/T003 (Foundational) can run in parallel — different GDD chapters, no shared output.
- T004–T008 (US1 authoring) can run in parallel — each writes a distinct FR.
- T009/T010 (US1 cross-checks) can run in parallel once both target specs exist.
- T011/T012 (US2 authoring) touch the same FR-003 sequentially, not in parallel.
- T014/T015 (US3 authoring) can run in parallel — distinct FRs.

## Notes

- Every "implementation" task in this feature is a writing task against `spec.md`'s own sections
  — there is no separate source file to keep in sync, per the Storage decision above.
- The pending (`[ ]`) tasks are not stalled work on this spec's part; they are correctly blocked on
  specs the ROADMAP places later in the build order (scenes come after all systems, per
  ROADMAP.md §5). Do not check them off until the named spec exists and has been checked.
- If GDD Ch. 10.1 or 1.2 is ever amended, re-open this tasks.md: add a new T0xx under Phase 2 to
  extract the change, then a new T0xx under the affected user story to update the corresponding
  FR, per constitution Principle V.
