---

description: "Task list for release/003-ai-and-cc0-asset-policy"

---

# Tasks: AI and CC0 Asset Policy

**Input**: Design documents from `specs/release/003-ai-and-cc0-asset-policy/`

**Prerequisites**: [spec.md](./spec.md)

**Tests**: This is a process/QA gate, not runtime code — there is no EditMode suite to write. Each task below is itself an auditable check; "passing" means the check returns the stated result against the asset register and build manifest, recorded as evidence for the corresponding Success Criterion in spec.md.

**Organization**: Tasks are grouped by user story from spec.md (only US1 is defined). Every required register field (FR-001), every policy rule (FR-002–FR-004), and every Edge Case bullet gets its own task, per the assignment's instruction to break this maximally small.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (independent audit, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1)

---

## Phase 1: Foundational (blocking prerequisites)

- [ ] T001 Read `specs/_reference/LILO-GDD-v2-Production-Lock.md` Ch. 19.4 in full — this gate cannot be audited from memory of a summary.

**Checkpoint**: Ch. 19.4 is the confirmed, current source for every task below.

---

## Phase 2: User Story 1 - Ship Assets With Traceable Rights (Priority: P1)

**Goal**: The team can prove every shipped visual, audio, and generated asset has an allowed license and a recorded source.

**Independent Test**: Audit the build manifest against the asset register and attempt to approve an unlicensed fixture.

### Register field completeness (FR-001)

- [ ] T002 [P] [US1] Confirm every shipped third-party or AI-assisted asset's register entry records its **source** (spec.md FR-001)
- [ ] T003 [P] [US1] Confirm every shipped third-party or AI-assisted asset's register entry records its **license** (spec.md FR-001)
- [ ] T004 [P] [US1] Confirm every shipped third-party or AI-assisted asset's register entry records its **creator/tool** (spec.md FR-001)
- [ ] T005 [P] [US1] Confirm every shipped third-party or AI-assisted asset's register entry records its **date** (spec.md FR-001)
- [ ] T006 [P] [US1] Confirm every shipped third-party or AI-assisted asset's register entry records its **permitted-use** (spec.md FR-001)

### License verification and blocking (FR-002)

- [ ] T007 [P] [US1] Independently check each CC0/public-domain claim in the register against its original source where practical (spec.md FR-002)
- [ ] T008 [US1] Attempt to approve a known-unlicensed fixture asset and confirm the approval is rejected — every asset with an incompatible license MUST block release (spec.md FR-002, Independent Test)

### AI-output review (FR-003)

- [ ] T009 [P] [US1] Review AI-assisted asset output for unwanted resemblance to identifiable real people, copyrighted characters, or existing trademarked works (spec.md FR-003)
- [ ] T010 [P] [US1] Review AI-assisted asset output for unsafe or inappropriate content (spec.md FR-003)
- [ ] T011 [P] [US1] Review AI-assisted asset output for consistency with the established art direction (spec.md FR-003)

### Register/manifest sync on change (FR-004)

- [ ] T012 [P] [US1] Confirm every asset removal updates the asset register (spec.md FR-004)
- [ ] T013 [P] [US1] Confirm every asset removal updates the build manifest (spec.md FR-004)
- [ ] T014 [P] [US1] Confirm every asset replacement updates the asset register (spec.md FR-004)
- [ ] T015 [P] [US1] Confirm every asset replacement updates the build manifest (spec.md FR-004)

### Build-manifest-to-register audit

- [ ] T016 [US1] Audit the full build manifest against the asset register: confirm every shipped non-original asset maps to an approved register entry, with zero unmapped assets (spec.md SC-001)

### Edge-case release-blocker audits

- [ ] T017 [P] [US1] Confirm any asset with an unclear license is treated as a release blocker until resolved (spec.md Edge Cases)
- [ ] T018 [P] [US1] Confirm any asset with a modified source (altered from its original licensed form) is treated as a release blocker until resolved (spec.md Edge Cases)
- [ ] T019 [P] [US1] Confirm any bundled package asset (e.g., from an imported Unity package or asset-store asset) is treated as a release blocker until its own license is separately verified and resolved (spec.md Edge Cases)
- [ ] T020 [P] [US1] Confirm any generated variation (a derivative of an AI-generated or licensed source asset) is treated as a release blocker until resolved (spec.md Edge Cases)
- [ ] T021 [P] [US1] Confirm any asset with missing metadata (any FR-001 field absent) is treated as a release blocker until resolved (spec.md Edge Cases)

**Checkpoint**: All register-field, license, AI-review, sync, and edge-case audits pass (SC-001, SC-002).

---

## Phase 3: Polish & Cross-Cutting Concerns

- [ ] T022 [P] Archive the completed asset-register and license audit (T002–T021 results) as submission evidence alongside the build
- [ ] T023 Re-run T002–T021 (the full register/license/edge-case audit) against the literal submission build manifest — a pass recorded earlier does not carry forward automatically if assets changed since
- [ ] T024 Confirm a clean final audit contains zero unresolved license blockers across the entire register (spec.md SC-002)

---

## Dependencies & Execution Order

- **Foundational (Phase 1)**: strictly before all audits — T001 establishes the current GDD 19.4 text every other task cites.
- **User Story 1 (P1, T002–T021)**: depends only on Foundational; the field-completeness (T002–T006), sync (T012–T015), and edge-case (T017–T021) task groups are each independently parallelizable ([P]); T008 depends on the register existing (T002–T006) to have a fixture to test against; T016 depends on T002–T015 for a complete register to audit against the manifest.
- **Polish (Phase 3)**: after User Story 1; T023 re-runs T002–T021 verbatim against the final submission artifact.
