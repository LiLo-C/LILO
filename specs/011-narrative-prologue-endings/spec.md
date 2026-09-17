# Feature Specification: Prologue, Endings & Full-Run Narrative Flow

**Feature Branch**: `feat/011-narrative-prologue-endings`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 5 — Narrative & Ending (GDD 19.2) · **Proposed owner**: Salwa (comic art) with Calzy (flow)

**GDD Sources**: Ch. 1.1, 2.3, 10.1, 10.2, 10.4, 18.1, 18.2, 18.3, 21

**Input**: User description: "Prologue comic-style (Eddie lelah & lembur, tertidur di meja, bangun dalam gelap, ingat lampu emergency di loker, lampu menyala setelah beberapa percobaan, mendengar tawa aneh → masuk gameplay). Good Ending saat keluar lewat Final Door: Eddie benar-benar terbangun di kafe, sadar kantornya toxic, melihat surat resign yang sudah diajukan sambil tersenyum, ada jadwal interview. Bad Ending saat lives habis: Eddie mati dan menjadi salah satu entity, terjebak di kantor selamanya. Alur: Prologue → Floor 52 → 51 → 50 → Final Door → Ending. Kedua ending harus bisa dicapai dari gameplay, bukan debug menu. Secret ending di luar scope."

## Clarifications

- Q: How does the Final Door explain that Eddie finally leaves the building? → A: [NEEDS CLARIFICATION: GDD 21 open item, must be decided before Fase 5. It needs one bridging line in the prologue or the Good Ending. Options: (a) a line in the Good Ending's first panel ("The door opened onto daylight — and then onto a café."); (b) a foreshadowing line in the prologue (e.g., Eddie remembering the building's emergency exit); (c) no line — the Final Door cuts straight to the café wake-up, relying on the dream reveal.]

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Meet Eddie Before the Dark (Priority: P1)

A player starting a new run sees a short comic-style prologue that introduces Eddie as an exhausted, over-prepared office worker, shows him waking in a pitch-black building, fumbling his emergency lamp on, and hearing strange laughter — then hands control to the player on Floor 52.

**Why this priority**: The prologue is must-have scope (GDD 18.1) and gives every mechanic its meaning (the lamp is the flashlight).

**Independent Test**: Start a new run and step through the prologue to the first frame of gameplay.

**Acceptance Scenarios**:

1. **Given** the player starts a new run, **When** the prologue plays, **Then** it shows the 7 beats of GDD 10.2 in order: tired, always working late, very prepared; goes to work; falls asleep at his desk; wakes to a dark building and panics; remembers the emergency lamp in his locker; the lamp turns on after several tries; hears strange laughter down the corridor.
2. **Given** the prologue is showing, **When** the player taps, **Then** it advances to the next panel or beat.
3. **Given** the prologue is showing, **When** the player uses the skip control, **Then** gameplay starts on Floor 52 immediately.
4. **Given** the last prologue beat ends, **When** gameplay begins, **Then** the player is at Floor 52's entry with a full light.

---

### User Story 2 - Escape: The Good Ending (Priority: P1)

A player who opens the Final Door sees Eddie truly wake up in a café, realize what his job has done to him, smile at the resignation letter he already sent, and see an interview waiting.

**Why this priority**: Untouchable scope (GDD 18.3) and the payoff of the run.

**Independent Test**: Complete a run through normal gameplay (no debug menu) and watch the ending.

**Acceptance Scenarios**:

1. **Given** the player uses the Final Door, **When** the run-completed outcome fires, **Then** the Good Ending comic plays with the beats of GDD 10.4.
2. **Given** the Good Ending finishes, **When** the player taps to continue, **Then** they return to the title screen (spec 012).

---

### User Story 3 - Trapped Forever: The Bad Ending (Priority: P1)

A player who loses their last life sees Eddie die and become one of the entities, trapped in the office forever.

**Why this priority**: Untouchable scope (GDD 18.3).

**Independent Test**: Lose all 3 lives through normal gameplay and watch the ending.

**Acceptance Scenarios**:

1. **Given** the player is caught with no lives left, **When** the death sequence ends, **Then** the Bad Ending comic plays with the beats of GDD 10.4.
2. **Given** the Bad Ending finishes, **When** the player taps to continue, **Then** they return to the title screen.

---

### Edge Cases

- The app is backgrounded during a comic: it resumes at the same panel.
- Pause during a comic: comics have their own skip/advance and do not need the gameplay pause menu. The pause button is hidden during comics.
- Rapid tapping: each tap advances at most one panel. A panel cannot be skipped before it is shown for `comicMinPanelDuration`.
- Skip during an ending: allowed, and it goes straight to the title screen.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A full run MUST flow: Prologue → Floor 52 → Floor 51 → Floor 50 → Final Door → Good Ending, with the Bad Ending replacing the rest of the run whenever lives reach 0 (GDD 2.3).
- **FR-002**: The prologue MUST present the GDD 10.2 beats as comic-style panels, advanced by tap, with a skip control always available.
- **FR-003**: The Good Ending MUST be triggered only by the run-completed outcome (Final Door) and MUST present the GDD 10.4 Good Ending content.
- **FR-004**: The Bad Ending MUST be triggered only by lives reaching 0 (spec 007) and MUST present the GDD 10.4 Bad Ending content.
- **FR-005**: Both endings MUST be reachable from normal gameplay in a release build, with no debug menu (Fase 5 DoD).
- **FR-006**: The Final Door explanation MUST follow the decision in Clarifications.
- **FR-007**: Story content MUST never explain foreshadowing or the office-as-trauma theme through explicit text during gameplay (GDD 10.3). The theme is stated only in the Good Ending.
- **FR-008**: No secret ending, branching narrative, or extra endings may be built (GDD 10.4, 18.2).
- **FR-009**: Comic panels MUST be hand-illustrated, with no AI-generated art (GDD 11.3, 19.4).
- **FR-010**: New configuration keys: `comicMinPanelDuration`.

### Key Entities

- **Comic Sequence**: Ordered panels for the prologue, Good Ending, or Bad Ending.
- **Panel**: One illustration with optional short caption text and optional sound cue.
- **Run Outcome**: Completed (Good) or out of lives (Bad).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Both endings are reached through normal gameplay on a release build in 3 of 3 attempts each.
- **SC-002**: The prologue takes 60–120 seconds when read at a normal pace without skipping.
- **SC-003**: After the prologue, at least 4 of 5 first-time players can say why Eddie is alone in the dark and what the lamp is.
- **SC-004**: After the Good Ending, at least 3 of 5 playtesters describe the office as a metaphor for Eddie's work life, without being prompted.

## Assumptions

- Caption text is minimal and in English, matching the "Floor NN" splash text. Localization is out of scope.
- Comics may include simple transitions (fade, slide) and sound cues from spec 013, but no animation beyond that is required.
- Floor splash text ("Floor 51", "Floor 50") is listed in GDD Fase 5 but is implemented in spec 004 because floor transitions need it earlier.
- Until final panels exist, placeholder panels with text descriptions are acceptable for flow testing.

## Dependencies

- **Requires**: 004 (run completed outcome), 007 (lives exhausted outcome), 012 (title screen as the return point).
- **Related**: 013 (comic sound cues), 015 (visual style consistency).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 10
