---

description: "Task list for release/002-playtest-and-bug-priority-process"

---

# Tasks: Playtest and Bug Priority Process

**Input**: Design documents from `specs/release/002-playtest-and-bug-priority-process/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: This is a process/QA gate, not runtime code — there is no EditMode suite to write. Each task below is itself an auditable check; "passing" means the check returns the stated result against the bug tracker and playtest log, recorded as evidence for the corresponding Success Criterion in spec.md.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P1, P2). Every severity tier, every FR, and every Key Entity/field requirement gets its own task, per the assignment's instruction to break this maximally small.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (independent audit, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US3)

---

## Phase 1: Foundational (blocking prerequisites)

- [ ] T001 Read `specs/_reference/LILO-GDD-v2-Production-Lock.md` Ch. 19.3 (bug-fix priority order), Ch. 19.2 (Phase 7 and Phase 8 Definition of Done), and Ch. 19.1 (role table) in full, plus `specs/release/001-scope-lock-and-cut-order/spec.md` FR-029 (the sign-off pattern FR-011 mirrors) — this gate cannot be audited from memory of a summary.

**Checkpoint**: The GDD chapters and the FR-029 sign-off pattern are the confirmed, current source for every task below.

---

## Phase 2: User Story 1 - Bug Triage Ordering Gate (Priority: P1)

**Goal**: Every bug is classified into exactly one of eight fixed severity tiers, and the team works strictly in tier order.

**Independent Test**: Take a snapshot of the bug tracker, sort open bugs by tier, and confirm no bug at a numerically-higher (less severe) tier has a more-recent "fixed" timestamp than an open, undeferred bug at a numerically-lower (more severe) tier.

- [ ] T002 [P] [US1] Tier 1 (Crash) classification audit: confirm every tracker entry tagged tier-1 genuinely causes a crash, and no known crash-class defect is missing from the tracker (spec.md FR-001)
- [ ] T003 [P] [US1] Tier 2 (Softlock) ordering audit: confirm no tier-2 bug was marked fixed while any tier-1 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T004 [P] [US1] Tier 3 (Impossible state) ordering audit: confirm no tier-3 bug was marked fixed while any tier-1 or tier-2 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T005 [P] [US1] Tier 4 (Bad collision) ordering audit: confirm no tier-4 bug was marked fixed while any tier-1–3 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T006 [P] [US1] Tier 5 (Monster bug) ordering audit: confirm no tier-5 bug was marked fixed while any tier-1–4 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T007 [P] [US1] Tier 6 (UI bug) ordering audit: confirm no tier-6 bug was marked fixed while any tier-1–5 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T008 [P] [US1] Tier 7 (Audio) ordering audit: confirm no tier-7 bug was marked fixed while any tier-1–6 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T009 [P] [US1] Tier 8 (Visual polish) ordering audit: confirm no tier-8 bug was marked fixed while any tier-1–7 bug remained open and undeferred (spec.md FR-001, FR-002)
- [ ] T010 [US1] Multi-tier symptom classification audit: confirm every tracker entry whose symptoms span multiple tiers is filed once at its single highest-severity (lowest-numbered) tier — never split across entries, never under-classified (spec.md FR-003)
- [ ] T011 [US1] Deferred-bug documentation audit: confirm every open, lower-numbered-tier bug being bypassed by in-progress higher-numbered-tier work carries a documented deferral reason (spec.md FR-002 exception clause)
- [ ] T012 [US1] Regression re-entry audit: confirm any regression that raised a bug's effective tier (e.g., a tier-8 fix causing a new tier-1 crash) re-entered the triage queue immediately at its new, higher-severity tier and blocked further lower-tier work until resolved (spec.md FR-008)
- [ ] T013 [US1] Tier-order exception sign-off audit: confirm any bug worked out of tier order (e.g., a stakeholder-urgent tier-8 bug ahead of an open tier-1 bug) carries documented, explicit Tech Lead sign-off before being applied (spec.md FR-011)

**Checkpoint**: All 8 tier-ordering audits and the classification/deferral/regression/sign-off audits pass (SC-001, SC-002).

---

## Phase 3: User Story 2 - External Playtest Coverage Gate (Priority: P1)

**Goal**: At least 5 non-team people each complete one full run on a debug-free build before Phase 8 begins, and every bug they surface is logged and triaged.

**Independent Test**: The playtest log shows at least 5 distinct non-team participant sessions, each reaching either the Good or Bad ending, with every bug that session surfaced present in the tracker at a valid tier.

- [ ] T014 [P] [US2] Audit playtest participant eligibility and count: confirm at least 5 distinct sessions are recorded from people outside the GDD 19.1 role table (spec.md FR-004)
- [ ] T015 [P] [US2] Audit playtest build configuration: confirm the build used for counted sessions provided no debug-menu assistance — no skip-floor, no invincibility, no manual battery refill outside normal gameplay (spec.md FR-004)
- [ ] T016 [P] [US2] Audit playtest run completion: confirm each counted session reached either the Good Ending or the Bad Ending scene (spec.md FR-004)
- [ ] T017 [P] [US2] Audit playtest-sourced bug logging: confirm every bug surfaced during an external session is entered in the tracker and classified per FR-001 before Phase 8 sign-off — not merely mentioned verbally or in chat (spec.md FR-005)
- [ ] T018 [P] [US2] Audit non-completed session handling: confirm any session ending in a softlock or crash before an ending is marked not-completed in the "≥5 completed runs" numerator, while its surfaced bug is still logged at its correct tier (spec.md Edge Cases, FR-005)
- [ ] T019 [P] [US2] Audit bug-tracker required fields: confirm every tracker entry retains tier, floor/scene found, found-by (team or external playtester), and status (open/fixed/deferred-with-reason) (spec.md FR-009, Key Entities)
- [ ] T020 [P] [US2] Audit playtest-log required fields: confirm every session record retains a participant identifier, a non-team confirmation flag, floors completed, ending reached (if any), and the set of bug-tracker ids that session produced (spec.md FR-010, Key Entities)

**Checkpoint**: At least 5 qualifying external sessions are recorded and every field/logging requirement passes (SC-003).

---

## Phase 4: User Story 3 - Submission Lock Freeze Gate (Priority: P2)

**Goal**: Once Phase 8 begins, only fixes to already-logged bugs merge, worked in tier order, and the final build is verified on the literal submission artifact.

**Independent Test**: Diff every commit merged during the Phase 8 window against the bug tracker; any commit not traceable to a logged bug fails the gate. Separately, play one full run on the actual submission build artifact and confirm it completes to an ending.

- [ ] T021 [US3] Feature-freeze commit audit: diff every commit merged during the Phase 8 window against the bug tracker and confirm each one maps to a fix for a bug already logged at a valid tier before that commit (spec.md FR-006)
- [ ] T022 [US3] Freeze-violation rejection audit: confirm any commit proposed during Phase 8 that adds a new mechanic, UI screen, or content item not tied to a logged bug fix was rejected or deferred, not merged (spec.md Acceptance Scenario 1, FR-006)
- [ ] T023 [US3] Submission-artifact full-run verification: play one full run start-to-ending on the literal build artifact intended for submission — not the Unity Editor, not a debug build (spec.md FR-007)
- [ ] T024 [US3] Zero-blocker confirmation: confirm the submission build has 0 open Crash, Softlock, or Impossible-state bugs at the moment of submission (spec.md SC-005)

**Checkpoint**: The Phase 8 merge window is 100% bugfix-traceable and the submission artifact completes a full run with zero severe open bugs (SC-004, SC-005).

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Archive the completed triage and playtest audit (T002–T024 results) as submission evidence alongside the build
- [ ] T026 Re-run T002–T020 (the full tier-ordering and playtest-coverage audits) against the bug tracker's state at the moment Phase 8 actually begins — a pass recorded earlier in Phase 7 does not carry forward automatically if the tracker changed since

---

## Dependencies & Execution Order

- **Foundational (Phase 1)**: strictly before all audits — T001 establishes the current GDD text and the FR-029 sign-off pattern every other task cites.
- **User Story 1 (P1, T002–T013)** and **User Story 2 (P1, T014–T020)**: both depend only on Foundational; independent of each other and of US3; every task within each is independently parallelizable ([P]) except T010–T013, which read the whole tracker rather than one tier slice.
- **User Story 3 (P2, T021–T024)**: depends on US1's tier-classification audits being current, since T021's "already logged at a valid tier" check reuses US1's FR-001/FR-002 findings.
- **Polish (Phase 5)**: after all three stories; T026 re-runs the US1+US2 tasks verbatim at the actual Phase 8 entry point.
