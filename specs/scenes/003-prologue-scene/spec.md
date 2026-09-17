# Feature Specification: Prologue Scene

**Feature Branch**: `003-prologue-scene`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "One linear comic-panel intro sequence covering exactly GDD Ch.
10.2's 7 beats: Eddie introduced as tired/over-prepared; he goes to work; he naps at his desk; he
wakes to total darkness and panics; he remembers the emergency lamp in his locker; the lamp turns
on after several tries; he hears strange laughter from the hallway, and gameplay begins. This spec
must decide and state whether the sequence is skippable, then own the transition into
`Floor52.unity`. It does not own final comic art, Eddie's characterization rules (that's
`specs/systems/narrative-content/001-eddie-character-bible/spec.md`), or the scene-transition
mechanism itself."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First-time player experiences Eddie's introduction and the horror hook (Priority: P1)

A player who just tapped Start on the Main Menu watches Eddie's story unfold across 7 fixed static
panels — from an ordinary, exhausting workday to waking alone in a pitch-black office — and the
sequence ends by handing off directly into gameplay on Floor 52.

**Why this priority**: This is the entire reason the scene exists. It establishes who Eddie is,
delivers the GDD's core tonal pivot from mundane to horror, and is the fixed entry point into every
run (per `specs/scenes/002-main-menu-scene/spec.md` FR-002). If the beats don't display in the
right order or the handoff to Floor52 doesn't fire, no run can ever begin.

**Independent Test**: From Main Menu, tap Start, advance through all 7 panels, confirm each
panel's content matches its beat and order from GDD Ch. 10.2, and confirm the scene transitions to
`Floor52.unity` immediately after the final panel — no additional input beyond advancing through
panels is required.

**Acceptance Scenarios**:

1. **Given** the Prologue has just loaded, **When** the first panel is shown, **Then** it depicts
   Eddie as tired, chronically over-prepared, and about to head to work (GDD 10.2 beat 1).
2. **Given** the player has advanced past panel 1, **When** panels 2 and 3 are shown in order,
   **Then** they depict Eddie departing for and beginning work (beat 2), then growing tired and
   napping at his desk (beat 3).
3. **Given** the player has advanced past panel 3, **When** panels 4 and 5 are shown in order,
   **Then** they depict Eddie waking to a totally dark building and panicking (beat 4), then
   remembering the emergency lamp in his desk locker (beat 5).
4. **Given** the player has advanced past panel 5, **When** panels 6 and 7 are shown in order,
   **Then** they depict the lamp turning on only after several failed tries (beat 6), then Eddie
   hearing strange laughter from the hallway (beat 7).
5. **Given** panel 7 (the final panel) is showing, **When** the player advances past it, **Then**
   the scene transitions to `Floor52.unity` — there is no 8th panel to advance to.

---

### User Story 2 - Repeat player skips a Prologue they've already seen (Priority: P2)

A player who has already watched the Prologue earlier in the same app session — and per
`specs/scenes/002-main-menu-scene/spec.md`'s Assumptions, Start always routes back through
Prologue, with no menu-level flag to bypass it — taps a persistent Skip control and reaches
gameplay immediately.

**Why this priority**: Secondary to the content existing and playing correctly at all (US1), but
without this, every repeat run in the same session forces the player through the full 7-panel
sequence again, which `002-main-menu-scene`'s spec explicitly relies on this scene to solve.

**Independent Test**: From Main Menu, tap Start, and on the very first panel tap the Skip control;
confirm the scene transitions directly to `Floor52.unity` without displaying any of the remaining
panels.

**Acceptance Scenarios**:

1. **Given** any panel (1 through 7) is currently showing, **When** the player taps the Skip
   control, **Then** the scene immediately transitions to `Floor52.unity`, bypassing all remaining
   panels.
2. **Given** the Skip control has just been tapped, **When** the transition begins, **Then** no
   partially-advanced panel state is carried into Floor52 — Skip and reaching panel 7 both lead to
   the identical Floor52 entry state.

---

### User Story 3 - Player advances the story at their own pace without breaking the sequence (Priority: P3)

A player taps to move from one panel to the next, one panel at a time, and cannot accidentally
skip two panels at once or trigger the Floor52 transition twice by tapping rapidly.

**Why this priority**: A robustness guarantee layered on top of US1/US2 once the core content and
Skip path already work — it protects against a player's own rapid input rather than adding new
content.

**Independent Test**: On any panel before the last, rapidly double-tap the advance input; confirm
exactly one panel advance occurred, not two. On panel 7, rapidly double-tap; confirm exactly one
transition to Floor52 is triggered, not two.

**Acceptance Scenarios**:

1. **Given** panel *N* (where *N* < 7) is showing, **When** the player taps twice in rapid
   succession, **Then** the sequence advances to panel *N+1* exactly once, not to *N+2*.
2. **Given** panel 7 is showing, **When** the player taps twice in rapid succession, **Then**
   exactly one transition to `Floor52.unity` is triggered.

---

### Edge Cases

- What happens if the player rushes through every panel as fast as possible? No minimum display
  time is enforced — this spec does not gate advancing on a caption having been "read"; pacing is
  entirely player-controlled (see Assumptions).
- What happens if the player backgrounds the app mid-Prologue and returns? The current panel index
  is retained for the rest of that same running process (ordinary Unity pause/resume) — there is no
  cross-session save that would restore it after a full app restart, consistent with
  `specs/scenes/002-main-menu-scene/spec.md`'s "no save/resume system" Assumption.
- What happens if the Skip control is tapped on panel 7 at the exact moment its own advance input
  would already trigger the Floor52 transition? Only one transition is ever triggered — both
  inputs route through the same guard described in User Story 3.
- What happens if `GameConfig`/`GameManager` aren't available (e.g., a developer opens
  `Prologue.unity` directly in the Editor, bypassing Bootstrap and Main Menu)? Out of scope for
  this spec — a shipped build always reaches Prologue via Main Menu, per
  `specs/scenes/001-bootstrap-scene/` and `specs/scenes/002-main-menu-scene/`.
- What happens if a caption's wording conflicts with Eddie's locked characterization (e.g., implies
  he's graceful, or financially comfortable)? Rejected during review against
  `specs/systems/narrative-content/001-eddie-character-bible/spec.md`, per that spec's own FR-001–
  FR-002 and its User Story 1 review process — this spec's content must pass that review before
  leaving Draft.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Prologue.unity` MUST present exactly 7 static panels, in this fixed order, matching
  GDD Ch. 10.2 1:1: (1) Eddie introduced as tired and chronically over-prepared, (2) Eddie departs
  for and begins work, (3) Eddie tires and naps at his desk, (4) Eddie wakes to total darkness and
  panics, (5) Eddie remembers the emergency lamp in his desk locker, (6) the lamp turns on only
  after several tries, (7) Eddie hears strange laughter from the hallway.
- **FR-002**: Every panel's content (caption text and any implied action) MUST stay consistent with
  `specs/systems/narrative-content/001-eddie-character-bible/spec.md` — in particular, panel 1 and
  2 MUST portray Eddie as underpaid/overworked (bible FR-001), panel 5 MUST use the emergency lamp
  as one of exactly the three canon locker items (bible FR-003), panel 6 MUST portray the multiple
  failed tries as an instance of Eddie's clumsiness (bible FR-002), and no panel may state the
  dream/reality or shrinking-light/shrinking-life metaphor outright (bible FR-007).
- **FR-003**: Panels MUST be presented one at a time, strictly in the FR-001 order — there is no
  player choice of order, no branching, and no panel is ever shown out of sequence.
- **FR-004**: The player MUST advance from the current panel to the next via a single tap anywhere
  on the panel (a comic-reader-style, player-paced advance) — there is no timed auto-advance (see
  Assumptions for why this interaction model was chosen over a fabricated timing value).
- **FR-005**: A Skip control MUST be visible and available throughout the entire sequence, starting
  from panel 1. Tapping it MUST immediately end the Prologue and trigger the transition to
  `Floor52.unity`, bypassing any remaining panels, regardless of which panel is currently showing.
- **FR-006**: Advancing past panel 7 (the final panel in FR-001) MUST trigger the transition to
  `Floor52.unity` — there is no 8th panel. This is the scene's implementation of GDD 10.2's final
  arrow, "→ masuk gameplay" ("into gameplay").
- **FR-007**: Both the Skip path (FR-005) and the panel-7-completion path (FR-006) MUST result in
  the identical `Floor52.unity` entry state — this spec does not define two different ways of
  entering Floor52.
- **FR-008**: The transition to `Floor52.unity`, however triggered, MUST go through
  `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`'s load mechanism.
  This scene MUST NOT call a separate, ad hoc scene-load path of its own.
- **FR-009**: Rapid repeated taps on the advance input MUST NOT advance more than one panel per
  tap, and MUST NOT queue more than one transition to Floor52, whether triggered via Skip or via
  panel 7's completion.
- **FR-010**: `Prologue.unity` MUST NOT present any gameplay HUD element (joystick, action button,
  battery indicator) or accept any gameplay input beyond the advance tap and the Skip tap — it is
  non-interactive narrative content, not gameplay.
- **FR-011**: `Prologue.unity` MUST render in landscape orientation only, matching the project's
  fixed device-orientation lock (constitution Technology Stack).
- **FR-012**: This spec MUST NOT define or duplicate Eddie's characterization rules, final comic
  art/illustration, or the scene-transition mechanism's own implementation — those remain owned by
  `specs/systems/narrative-content/001-eddie-character-bible/spec.md`, the comic-art production
  process (GDD Ch. 19.1), and `specs/systems/progression-and-scene-flow/002-scene-transition-
  manager/spec.md` respectively.

### Key Entities

- **Prologue Sequence**: The ordered list of exactly 7 `Panel` entries (FR-001) plus the Skip
  control and the advance/transition guard — the core content and behavior this spec owns.
- **Panel**: A single static image placeholder + caption text representing one of the 7 beats;
  advanced one at a time via tap (FR-003, FR-004). Final art is out of scope here (Assumptions).
- **Skip Control**: A persistent, always-visible UI element (FR-005) that ends the sequence
  immediately from any panel.
- **Panel Advance Input**: The tap-to-advance gesture and its rapid-tap guard (FR-004, FR-009) —
  the one piece of pure, extractable decision logic in this scene.
- **Eddie Character Bible** *(referenced, not owned)*: The characterization/tone/through-line
  constraints every panel's content must satisfy; owned by
  `specs/systems/narrative-content/001-eddie-character-bible/spec.md`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of first-time playthroughs display exactly the 7 panels from FR-001, in order,
  with zero skipped, reordered, or duplicated beats.
- **SC-002**: From any panel, tapping Skip reaches `Floor52.unity` in exactly one control
  interaction, on 100% of manual QA attempts.
- **SC-003**: A rapid-tap manual QA pass (User Story 3) shows zero instances of more than one panel
  advance per tap, and zero instances of more than one Floor52 transition triggered from either
  Skip or panel 7.
- **SC-004**: A reviewer audit (using the method described in the Eddie character bible's SC-003)
  confirms zero on-screen caption across all 7 panels states the dream/reality equivalence or the
  shrinking-light/shrinking-life metaphor outright.

## Assumptions

- **Tap-to-advance, not timed auto-advance.** GDD Ch. 10.2 describes the Prologue as a
  "comic-style scene" but does not fix an interaction mechanism or a reading-time value. Per
  `specs/ROADMAP.md` §0 (feel-based values state the rule, not a fabricated number), this spec
  chooses player-paced tap-to-advance: it matches how a reader naturally consumes comic panels, and
  it avoids inventing an unvalidated auto-advance duration that could feel wrong on real hardware.
- **Skip is available from panel 1, not gated behind "has seen it before."** The simplest rule that
  satisfies `specs/scenes/002-main-menu-scene/spec.md`'s Assumption (Start always plays Prologue,
  every time) without adding a new persisted "seen before" flag to `GameState` — consistent with
  constitution Principle II (Simplicity & YAGNI). A first-time player who taps Skip by mistake
  simply reaches gameplay slightly sooner; this spec treats that as an acceptable trade-off over
  adding session-tracking state whose only purpose would be gating the Skip control's visibility.
- **Exactly 7 panels, matching GDD 10.2's bullet list 1:1.** No additional panels, alternate
  branches, or extra beats are invented beyond what the GDD lists.
- **Final comic art/illustration is out of scope.** This spec owns sequencing, interaction, and
  caption-text wiring; the actual illustrated art for each panel is comic-art production's
  ownership (GDD Ch. 19.1) and is placeholder content (static image + caption) until that art
  exists.
- **No voice-over is assumed.** Any ambient SFX during the Prologue (if added later) is out of
  scope for this spec and would be owned by the audio system specs (`specs/systems/audio/`); this
  spec does not require or preclude one.
- **Panel 7's "strange laughter" may reuse the foreshadowing catalog's mocking-coworker-laughter
  entry.** `specs/systems/narrative-content/002-foreshadowing-prop-catalog/spec.md` lists
  "tertawaan rekan kerja yang seolah mengejek Eddie" as a catalog sound; this spec does not mandate
  that reuse, but flags it as the natural, already-validated source for panel 7's laughter rather
  than a new invented sound, should sound design need one before the floor scenes do.
- **Resuming mid-sequence after the app is backgrounded** is ordinary Unity pause/resume behavior
  (the current panel index stays in memory) and is not a persisted, cross-session save — consistent
  with there being no save/resume system anywhere in this project's scope.

## Related

- GDD Ch. 10.2 (Prologue beats, this spec's primary source), Ch. 10.1 (Eddie's characterization),
  Ch. 1.2 (Core Experience/mood — the tonal pivot this sequence delivers).
- `specs/ROADMAP.md` §3 (scenes table, `003-prologue-scene` row: depends on
  `narrative-content/001`) and §5 (build sequence: Prologue follows Main Menu, precedes Floor 52).
- `specs/systems/narrative-content/001-eddie-character-bible/spec.md` — owns the characterization,
  tone, and thematic through-line constraints every panel's caption must satisfy; not redefined
  here.
- `specs/systems/narrative-content/002-foreshadowing-prop-catalog/spec.md` — the candidate source
  for panel 7's hallway laughter (see Assumptions); not redefined here.
- `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md` — owns the actual
  scene-loading mechanism this scene's Skip and panel-7 paths both call into; not redefined here.
- `specs/scenes/002-main-menu-scene/spec.md` — Start always routes here (its FR-002); this scene's
  Skip control (FR-005 above) is what that spec's own Assumptions rely on to keep repeat playthroughs
  tolerable.
- `specs/scenes/004-floor-52-scene/` — the destination this scene hands off to (FR-006, FR-007);
  its own content/onboarding is out of scope here.
