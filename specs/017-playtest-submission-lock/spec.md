# Feature Specification: Playtest, Polish & Submission Lock

**Feature Branch**: `feat/017-playtest-submission-lock`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 7 — Playtest & Polish, Fase 8 — Submission Lock (GDD 19.2) · **Proposed owner**: Calzy (build & triage) with whole team

**GDD Sources**: Ch. 1.4, 16.3, 18.2, 18.3, 19.2, 19.3, 19.4, constitution Principle V

**Input**: User description: "Fase 7: playtest eksternal, bug fix berdasarkan prioritas (Crash → Softlock → Impossible state → Bad collision → Monster bug → UI bug → Audio → Visual polish). DoD: minimal 5 orang di luar tim menyelesaikan satu full run. Fase 8: tidak ada fitur baru, build final; DoD: satu full run diuji dari awal sampai ending di build terakhir. Target durasi floor ±5 menit, full run ±15 menit. Memperpanjang durasi hanya lewat config (kurangi battery, perlambat spawn, perbesar map), tidak dengan floor ke-4."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Outsiders Finish the Game (Priority: P1)

At least five people outside the team play a full run on a real device, and the team learns where players get stuck, die, or get bored.

**Why this priority**: The Fase 7 Definition of Done.

**Independent Test**: Run the playtest protocol with 5+ external players and fill in the results sheet.

**Acceptance Scenarios**:

1. **Given** a playtest build, **When** an external player plays, **Then** the team records: completed run yes/no, ending reached, deaths per floor, time per floor, total run time, places they got stuck, and anything they said was confusing.
2. **Given** all sessions, **When** results are reviewed, **Then** at least 5 external players have completed a full run.

---

### User Story 2 - Fix What Matters First (Priority: P1)

Every bug found is triaged into the GDD priority order and fixed from the top down, so the build gets more stable instead of just more polished.

**Why this priority**: Prevents polishing visuals while softlocks remain (GDD 19.3).

**Independent Test**: Inspect the bug list: every item has a category and nothing lower is fixed while a higher-priority item is open without a reason recorded.

**Acceptance Scenarios**:

1. **Given** a new bug, **When** it is logged, **Then** it gets exactly one category from: Crash, Softlock, Impossible state, Bad collision, Monster bug, UI bug, Audio, Visual polish.
2. **Given** open bugs, **When** picking the next fix, **Then** the highest open category goes first, unless a recorded reason says otherwise.

---

### User Story 3 - Tune Duration Without New Content (Priority: P2)

If runs are too short or too long, the team adjusts only configuration and map size — never adds a floor — until floors take about five minutes and a run about fifteen.

**Why this priority**: Balancing lever named by the GDD (1.4).

**Independent Test**: Compare playtest timing before and after a tuning pass and check which files changed.

**Acceptance Scenarios**:

1. **Given** median floor times outside 4–6 minutes, **When** tuned, **Then** changes are limited to configuration (battery counts, respawn times, monster values) and floor layout size, with each change recorded.

---

### User Story 4 - Lock and Ship (Priority: P1)

In the final phase the team stops adding features, produces the final build, and verifies a complete run from launch to each ending on exactly that build.

**Why this priority**: The Fase 8 Definition of Done.

**Independent Test**: Install the final build fresh and play to both endings.

**Acceptance Scenarios**:

1. **Given** Fase 8 has started, **When** a change is proposed, **Then** only bug fixes are accepted — no new features, including anything on the cut list.
2. **Given** the final build, **When** tested, **Then** a full run from launch to the Good Ending and a run to the Bad Ending both pass without crash on iPhone 17.
3. **Given** the final build, **When** audited, **Then** it has no debug menus or overlays, no placeholder visuals, a complete CC0 registry, no AI-generated assets, and a version and build number that follow constitution Principle V.

---

### Edge Cases

- Fewer than 5 external players can complete a run: the DoD is not met. Fix blockers and retest; do not lower the bar.
- A cut becomes necessary late: follow the GDD 18.3 order (hiding → battery respawn → Floor 50 → 3D models), never cutting one complete floor, a working monster, or either ending.
- A fix in Fase 8 introduces a regression: the full-run verification must be repeated on the new build.
- Device heat or memory over long sessions: covered by the back-to-back run check (SC-004).

## Requirements *(mandatory)*

### Functional Requirements

**Fase 7 — Playtest & Polish**

- **FR-001**: The team MUST run external playtests with a written protocol (no coaching during play; observer notes; post-play questions) on physical iPhones.
- **FR-002**: Each session MUST record the fields in US1 scenario 1 in one shared results sheet.
- **FR-003**: All bugs MUST be triaged into the GDD 19.3 order and fixed top-down, with any exception recorded.
- **FR-004**: Duration tuning MUST use only configuration and floor layout size (GDD 1.4). No fourth floor or new mechanic.
- **FR-005**: Each floor MUST get a final GDD 16.3 checklist sign-off using playtest data.
- **FR-006**: If a scope cut is made, it MUST follow the GDD 18.3 order and MUST be recorded with its reason.

**Fase 8 — Submission Lock**

- **FR-007**: After the lock date, only bug fixes may be merged.
- **FR-008**: Release builds MUST exclude every debug-only feature: floor selector, monster/noise overlays, arena, and debug profile switches.
- **FR-009**: The final build MUST have a semantic version aligned with its TestFlight build number (constitution Principle V).
- **FR-010**: The final build MUST pass an audit for: no placeholder visuals (spec 015), complete CC0 registry (spec 013), no AI-generated assets (GDD 19.4), and every AI-assisted code path in main understood and explainable by its owner (GDD 19.4).
- **FR-011**: A full run to each ending MUST be verified on the exact final build.

### Key Entities

- **Playtest Session Record**: Player (anonymous ID), device, outcome, ending, per-floor deaths and times, notes.
- **Bug Record**: Title, category (GDD 19.3), floor or system, status, fix build.
- **Release Checklist**: The Fase 8 audit items.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: ≥5 people outside the team complete a full run (Fase 7 DoD).
- **SC-002**: Median full-run time for those players is 12–18 minutes, with median floor times of 4–6 minutes (GDD 1.4).
- **SC-003**: 0 open Crash or Softlock bugs at submission.
- **SC-004**: 3 full runs back-to-back on iPhone 17 with no crash and ≥60 fps sustained in gameplay.
- **SC-005**: The final build reaches both endings from normal play in the Fase 8 verification (Fase 8 DoD).

## Assumptions

- This is mostly process work. Its tasks will be checklists, sheets, build settings, and fixes rather than new systems.
- Playtesters are recruited informally. No external services or analytics SDKs are added. Timing is recorded by observers or a debug-only in-game timer that is excluded from release.

## Dependencies

- **Requires**: all Must Have specs (001–012) complete. 013–016 for the Fase 6 DoD.

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[constitution]] — Principle V
- [[LILO-GDD-v2-Production-Lock]] — Ch. 19
