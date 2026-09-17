# Feature Specification: Playtest and Bug Priority Process

**Feature Branch**: `002-playtest-and-bug-priority-process`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 19.3 (Prioritas bug fix); Ch. 19.2 Phase 7 (Playtest & Polish) and Phase 8 (Submission Lock) Definition of Done, which this process spec makes enforceable.

**Input**: User description: "Bugs found during playtest and polish must be fixed in a fixed severity order — Crash, then Softlock, then Impossible state, then Bad collision, then Monster bug, then UI bug, then Audio, then Visual polish — never worked out of order. This process also owns the Phase 7 external-playtest requirement (at least 5 outside testers completing a full run) and the Phase 8 feature freeze, since both gate on the same bug tracker this spec defines."

This spec is a process/QA gate, not runtime code. Its "acceptance scenarios" are audit procedures run against a bug tracker and a playtest log, not gameplay behavior.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Bug Triage Ordering Gate (Priority: P1)

Every bug found during Phase 7 (Playtest & Polish) or Phase 8 (Submission Lock) is classified into exactly one of eight fixed severity tiers, and the team works strictly in tier order: no bug at tier N is fixed while any bug at tier < N (more severe) remains open and undeferred.

**Why this priority**: This is GDD 19.3's entire point — under deadline pressure, the instinct is to fix whatever's easiest or most visible (a flickering light, a wrong SFX), not what's most dangerous (a crash, a softlock). This gate is what keeps the team from spending the last week polishing while a crash ships.

**Independent Test**: At any point during Phase 7/8, an auditor takes a snapshot of the bug tracker, sorts open bugs by tier, and confirms no bug at a numerically-higher (less severe) tier has a more-recent "fixed" timestamp than an open, undeferred bug at a numerically-lower (more severe) tier.

**Acceptance Scenarios**:

1. **Given** the bug tracker has open bugs at tier 1 (Crash) and tier 6 (UI bug), **When** a developer looks for the next bug to work, **Then** they work the tier 1 bug first — the tier 6 bug is not touched until every tier 1–5 bug is fixed or explicitly deferred with documented reasoning.
2. **Given** a bug's symptoms span two tiers (e.g., a monster pathing error that also crashes the game), **When** it is triaged, **Then** it is classified at its single highest-severity applicable tier (tier 1, Crash) — not split into two tracker entries, not classified at the lower tier.
3. **Given** a tier-8 (Visual polish) fix is merged and it introduces a new tier-1 (Crash) regression, **When** this is discovered, **Then** the new crash immediately re-enters the triage queue at tier 1 and blocks all further tier 2–8 work until it is resolved.

---

### User Story 2 - External Playtest Coverage Gate (Priority: P1)

Before Phase 8 (Submission Lock) begins, at least 5 people who are not development team members each complete one full run of the game — Prologue through an ending — on a build with no debug-menu assistance, and every bug they encounter is logged and triaged per User Story 1.

**Why this priority**: GDD 19.2's Phase 7 Definition of Done is explicit: "Minimal 5 orang di luar tim menyelesaikan satu full run." Without this, the bug tracker only reflects issues the team itself noticed — which historically misses exactly the confusion/softlock/onboarding issues that fresh eyes catch.

**Independent Test**: The playtest log shows at least 5 distinct non-team participant sessions, each reaching either the Good or Bad ending, with every bug that session surfaced present in the tracker at a valid tier.

**Acceptance Scenarios**:

1. **Given** Phase 7 is underway, **When** the playtest log is reviewed, **Then** it records at least 5 sessions from people outside the GDD 19.1 role table, each session ending in either the Good Ending or the Bad Ending scene.
2. **Given** a playtest session encounters a softlock and cannot finish the run, **When** the session is logged, **Then** the softlock is entered into the bug tracker at tier 2 (Softlock) and the session is marked as not-completed for the "5 completed runs" count — but the bug itself still counts toward the tracker regardless of session outcome.
3. **Given** all 5+ sessions are logged, **When** Phase 8 begins, **Then** every bug those sessions surfaced has a tier classification in the tracker — no bug is "remembered informally" but absent from the tracker.

---

### User Story 3 - Submission Lock Freeze Gate (Priority: P2)

Once Phase 8 (Submission Lock) begins, no commit introduces a new feature, mechanic, or content item — only fixes to already-logged bugs, worked in tier order, are merged — and the final build is verified with one full run played on the literal submission artifact.

**Why this priority**: GDD 19.2's Phase 8 Definition of Done is "Tidak ada fitur baru. Build final." This is lower priority than the two gates above because it only becomes active once they've already produced a tracker to work from — but it's what stops "just one more feature" from slipping in after the freeze.

**Independent Test**: Diff every commit merged during the Phase 8 window against the bug tracker; any commit that isn't traceable to fixing a logged bug (per User Story 1's tiers) fails the gate. Separately, play one full run on the actual build artifact intended for submission (not the Editor) and confirm it completes to an ending.

**Acceptance Scenarios**:

1. **Given** Phase 8 has begun, **When** a commit is proposed that adds a new mechanic, UI screen, or content item not tied to a logged bug fix, **Then** it is rejected or deferred to a future release — it does not merge during the freeze.
2. **Given** the Phase 8 window closes, **When** the commit log for that window is audited against the bug tracker, **Then** every merged commit maps to a fix for a bug already logged at a valid tier before that commit.
3. **Given** the build intended for submission is produced, **When** it is played start-to-finish as one full run, **Then** it completes to an ending on that literal artifact, not only in the Unity Editor.

---

### Edge Cases

- **Two bugs at the same tier discovered simultaneously**: GDD 19.3 orders tiers, not bugs within a tier — the team may use its own judgment (severity within tier, ease of repro) for same-tier ordering; this spec only enforces cross-tier order.
- **A stakeholder insists a tier-8 (Visual polish) bug is "urgent" for a demo or marketing reason**: it is still triaged and worked at tier 8 — urgency claims do not skip tiers; if a genuine exception is needed, it must be handled the same way spec 001 handles off-list scope cuts (explicit, documented sign-off), not a silent reprioritization.
- **A playtest session ends in a softlock or crash before reaching an ending**: the bug is still logged at its correct tier (Softlock or Crash respectively), but that session does not count toward the "≥5 completed full runs" numerator in User Story 2 — the participant should be invited to replay after the blocking bug is fixed, budget permitting.
- **A tier-5 (Monster bug) fix accidentally also fixes an unrelated tier-3 (Impossible state) bug**: this is fine and not a violation — the ordering rule constrains what a developer *chooses* to work next, not what a fix incidentally also resolves.
- **A regression is discovered that raises a previously-fixed bug's effective tier** (e.g., a tier-8 fix causes a tier-1 crash): per User Story 1's Acceptance Scenario 3, the new issue re-enters the queue at its new, higher tier immediately; already-shipped lower-tier fixes are not reverted, but no further lower-tier work proceeds until the regression is resolved.
- **Fewer than 5 non-team playtesters are available before the Phase 7 deadline**: the Phase 8 entry gate (User Story 2) is not satisfied — Phase 8 must not begin until the minimum is met; this spec does not provide a waiver path, matching the GDD's DoD language ("Minimal 5 orang").

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every bug entered into the tracker MUST be assigned exactly one severity tier from the fixed, ordered list: 1 Crash, 2 Softlock, 3 Impossible state, 4 Bad collision, 5 Monster bug, 6 UI bug, 7 Audio, 8 Visual polish (GDD Ch. 19.3). No bug may remain unclassified once triaged.
- **FR-002**: At any point in Phase 7 or Phase 8, a bug at tier N MUST NOT be actively worked while any bug at tier < N remains open and undeferred, unless that lower-numbered bug carries a documented deferral reason (e.g., blocked pending repro, blocked pending an external dependency).
- **FR-003**: A bug whose symptoms span multiple tiers MUST be classified at its single highest-severity (lowest-numbered) applicable tier — never split across multiple tracker entries for the same underlying defect, and never classified at a lower tier than its worst symptom.
- **FR-004**: Before Phase 8 begins, at least 5 people who are not members of the development team (per the GDD 19.1 role table) MUST each complete one full run — Prologue → Floor 52 → Floor 51 → Floor 50 → an ending — on a build that provides no debug-menu assistance (no skip-floor, no invincibility, no manual battery refill outside normal gameplay).
- **FR-005**: Every bug discovered during an external playtest session MUST be logged in the tracker, classified per FR-001, before Phase 8 sign-off — a bug only mentioned verbally or in chat and never entered in the tracker does not count as "handled."
- **FR-006**: Once Phase 8 (Submission Lock) begins, no commit MAY introduce a new feature, mechanic, or content item; only fixes to bugs already logged in the tracker (per FR-001) MAY be merged during this window.
- **FR-007**: Before submission, the literal build artifact intended for submission (not the Unity Editor, not a debug build) MUST be verified by playing one full run start-to-ending on that artifact.
- **FR-008**: A regression that raises a previously classified or newly discovered bug's effective severity (e.g., a Visual-polish fix causing a new Crash) MUST re-enter the triage queue immediately at its new, higher-severity tier, and MUST block further lower-tier work until it is resolved — this re-affirms FR-002 for issues discovered mid-process, not only at initial triage.
- **FR-009**: The bug tracker MUST retain, for every entry, at minimum: a tier classification (FR-001), the floor/scene where it was found, whether it was found by a team member or an external playtester, and a status (open / fixed / deferred, with a reason if deferred) — this is the minimum data needed to audit FR-002, FR-005, and FR-006.
- **FR-010**: The playtest log MUST retain, for every external session, at minimum: a participant identifier (may be anonymized), confirmation the participant is non-team, which floors were completed, which ending (if any) was reached, and which bug-tracker entries that session produced — this is the minimum data needed to audit FR-004 and FR-005.
- **FR-011**: An exception to work a bug out of tier order (e.g., a stakeholder-urgent tier-8 bug ahead of an open tier-1 bug) MUST carry documented, explicit sign-off from the Tech Lead before it is applied — undocumented reprioritization fails this gate, mirroring the escalation rule in `specs/release/001-scope-lock-and-cut-order/spec.md` FR-029.

### Key Entities

- **Bug Report**: an id, description, tier (1–8, exactly one), floor/scene found, found-by (team or external playtester), and status (open / fixed / deferred-with-reason).
- **Playtest Session**: a participant identifier, a non-team confirmation flag, the floors completed, the ending reached (if any), and the set of Bug Report ids that session produced.
- **Triage Queue**: the current set of open Bug Reports, ordered strictly by tier and, within a tier, by discovery time or team judgment (not specified further by the GDD).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of bug tracker entries carry exactly one tier classification at every point during Phase 7/8.
- **SC-002**: 0 instances, across the full Phase 7/8 audit window, of a lower-severity bug being marked fixed while a higher-severity, undeferred bug remains open.
- **SC-003**: At least 5 external (non-team) playtest sessions are recorded before Phase 8 begins, each completing a full run to an ending, on a build with no debug-menu assistance.
- **SC-004**: 0 non-bugfix commits are found in a post-hoc audit of the Phase 8 merge window against the bug tracker.
- **SC-005**: The literal submission build artifact completes one full run to an ending with 0 open Crash, Softlock, or Impossible-state bugs at the moment of submission.

## Assumptions

- The bug tracker and playtest log are assumed to be a single shared source (e.g., one spreadsheet or issue tracker) accessible to the whole team — this spec defines the required fields (FR-009, FR-010), not the specific tool.
- "Non-debug-assisted build" (FR-004) assumes the project's debug/cheat menu, if any, is a togglable feature distinct from the shipping build configuration; this spec does not itself define that toggle, only requires playtests to run without it.
- Tier-within-tier ordering (same-severity bugs) is left to team judgment per the Edge Cases section — the GDD does not further sub-order same-tier bugs, and this spec does not invent a rule the source document doesn't state.

## Related

- GDD: `specs/_reference/LILO-GDD-v2-Production-Lock.md` — Ch. 19.3 (bug-fix priority order), Ch. 19.2 (Phase 7 and Phase 8 Definition of Done, which this spec's User Stories 2 and 3 make enforceable), Ch. 19.1 (role table used for "non-team" in FR-004 and for Tech Lead sign-off in FR-011).
- `specs/ROADMAP.md` — §4 row 2 (this spec's place in the release gate sequence), §5 (build order — this gate runs after every system/scene spec, during Phase 7/8).
- `specs/release/001-scope-lock-and-cut-order/spec.md` — the sibling release gate whose FR-029 escalation/sign-off pattern this spec's FR-011 mirrors.
