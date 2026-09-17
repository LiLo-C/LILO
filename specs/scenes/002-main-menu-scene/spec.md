# Feature Specification: Main Menu Scene

**Feature Branch**: `002-main-menu-scene`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The Title screen: a Start button routing into the game, and
navigation entry points into the shared How To Play and Settings overlays. This scene spec owns
only the Title screen's own layout and the button wiring into those shared systems, not their
content. Must decide and state explicitly whether Start always plays the Prologue once per app
session, or whether a returning player can skip straight to Floor52."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start a run from the Title screen (Priority: P1)

A player looking at the Title screen taps Start and is taken into the Prologue, the game's fixed
entry point into a run.

**Why this priority**: This is the single action that gets a player from "app is open" into
"game is being played." Nothing else on this screen matters if this doesn't work.

**Independent Test**: From a freshly-loaded Main Menu, tap Start and confirm the scene transitions
to Prologue with no other input required.

**Acceptance Scenarios**:

1. **Given** the Main Menu has finished loading, **When** the player taps Start, **Then** the
   scene transitions to `Prologue.unity`.
2. **Given** the player has already completed a full run earlier in the same app session (either
   ending) and returned to Main Menu, **When** the player taps Start again, **Then** the scene
   transitions to `Prologue.unity` again, identically to the first time (see Assumptions).
3. **Given** a transition away from Main Menu is already in progress, **When** the player taps
   Start again before it completes, **Then** no second transition is triggered.

---

### User Story 2 - Learn the rules before playing (Priority: P2)

A player unsure how the game works taps a How To Play entry point from the Title screen and sees
the shared How To Play content — including the one place the game states the player's life
count — without leaving the Title screen underneath it.

**Why this priority**: GDD Ch. 15.4/9.1 make How To Play the only place lives are ever
communicated to the player; it needs a clear, reliable entry point, but it's secondary to Start
existing at all.

**Independent Test**: From Main Menu, tap the How To Play entry point and confirm the shared How
To Play overlay opens; dismiss it and confirm Main Menu is unchanged underneath.

**Acceptance Scenarios**:

1. **Given** the Main Menu is showing, **When** the player taps the How To Play entry point,
   **Then** the shared How To Play overlay
   (`specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`) opens.
2. **Given** the How To Play overlay is open, **When** the player dismisses it, **Then** the Main
   Menu is showing again with Start, How To Play, and Settings all still functional.

---

### User Story 3 - Adjust audio before playing (Priority: P3)

A player wanting to change volume/audio settings taps a Settings entry point from the Title
screen and sees the shared Settings/audio overlay.

**Why this priority**: Useful and expected, but the least critical of the three entry points —
a player can start and enjoy a run without ever touching Settings first.

**Independent Test**: From Main Menu, tap the Settings entry point and confirm the shared
Settings overlay opens; dismiss it and confirm Main Menu is unchanged underneath.

**Acceptance Scenarios**:

1. **Given** the Main Menu is showing, **When** the player taps the Settings entry point,
   **Then** the shared Settings/audio overlay
   (`specs/systems/game-shell-ui/002-settings-menu-audio-controls/spec.md`) opens.
2. **Given** the Settings overlay is open, **When** the player dismisses it, **Then** the Main
   Menu is showing again with Start, How To Play, and Settings all still functional.

---

### Edge Cases

- What happens if the player rapidly double-taps Start? Only one transition to Prologue is ever
  triggered; the control is disabled/ignored while a transition is already in flight (see User
  Story 1, Acceptance Scenario 3).
- What happens if the player opens How To Play and Settings back-to-back without dismissing the
  first? Only one overlay is ever open at a time — opening the second while the first is open is
  treated as owned by the shared overlay system itself (`game-shell-ui/002`, `.../003`), not by
  this scene, since Main Menu only exposes the entry points, not the overlay stacking rule.
- What happens on the very first app launch of a session versus a later Start press after
  finishing a run? Per this spec's Assumption, both behave identically — Start always leads to
  Prologue. There is no different "returning player" path.
- What happens if a player reaches Main Menu from `GoodEnding.unity` or `BadEnding.unity` (owned
  by `specs/scenes/007-good-ending-scene/` and `.../008-bad-ending-scene/`)? Main Menu MUST look
  and behave identically to a fresh Bootstrap-to-MainMenu launch — no ending-specific state
  lingers on this screen.
- What happens if `GameConfig`/`GameManager` aren't available (e.g., a developer opens
  `MainMenu.unity` directly in the Editor, bypassing Bootstrap)? Out of scope for this spec — a
  shipped build always reaches Main Menu via Bootstrap, per `specs/scenes/001-bootstrap-scene/`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `MainMenu.unity` MUST present exactly one primary call-to-action: a Start control.
- **FR-002**: Tapping Start MUST trigger a scene transition, via
  `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`, to
  `Prologue.unity`, every time it is pressed — regardless of how many times Prologue has already
  been viewed earlier in the same app session (see Assumptions).
- **FR-003**: `MainMenu.unity` MUST present a How To Play entry point that opens the shared How To
  Play overlay (`specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`) without unloading
  `MainMenu.unity` underneath it.
- **FR-004**: `MainMenu.unity` MUST present a Settings entry point that opens the shared
  Settings/audio overlay (`specs/systems/game-shell-ui/002-settings-menu-audio-controls/spec.md`)
  without unloading `MainMenu.unity` underneath it.
- **FR-005**: This spec MUST NOT define the internal layout, content, or dismiss behavior of the
  How To Play or Settings overlays — only that Main Menu exposes one entry point into each.
- **FR-006**: While a scene transition away from Main Menu is in progress, the Start, How To Play,
  and Settings controls MUST be disabled or otherwise made non-triggering, so repeated taps cannot
  queue up a second transition.
- **FR-007**: Reaching Main Menu MUST NOT require any input beyond the normal app-launch/return
  flow — no login, account creation, or network call gates any of Start, How To Play, or Settings.
- **FR-008**: `MainMenu.unity` MUST be the scene both `GoodEnding.unity` and `BadEnding.unity`
  return to when their own ending sequence completes (per
  `specs/scenes/007-good-ending-scene/spec.md` and `.../008-bad-ending-scene/spec.md`), and Start
  MUST behave identically to FR-002 on that return — there is no separate "post-run" menu state.
- **FR-009**: `MainMenu.unity` MUST NOT display any in-run gameplay HUD element (battery
  indicator, spare slot, or any lives readout) — those are confined to floor scenes per GDD
  Ch. 15.1.
- **FR-010**: `MainMenu.unity` MUST render in landscape orientation only, matching the project's
  fixed device-orientation lock (constitution Technology Stack).

### Key Entities

- **Title Screen**: The `MainMenu.unity` scene's own owned content — the Start control, the How
  To Play entry point, and the Settings entry point, plus the in-flight-transition guard from
  FR-006.
- **How To Play Overlay** *(referenced, not owned)*: Its content and behavior are defined by
  `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md`.
- **Settings/Audio Overlay** *(referenced, not owned)*: Its content and behavior are defined by
  `specs/systems/game-shell-ui/002-settings-menu-audio-controls/spec.md`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: From Main Menu's first interactive frame, a player reaches the start of the
  Prologue in exactly one tap, on 100% of attempts.
- **SC-002**: How To Play and Settings are each reachable from Main Menu in exactly one tap, and
  each returns the player to a fully-functional Main Menu on dismissal.
- **SC-003**: Across a manual QA pass of rapid repeated Start taps, zero duplicate Prologue-load
  transitions are observed.
- **SC-004**: After completing a full run (either ending) and returning to Main Menu, Start,
  How To Play, and Settings are all confirmed functional again with no stale/disabled state
  carried over from the previous run.

## Assumptions

- **Start always plays the Prologue — no session-scoped skip.** This spec deliberately does not
  add a "Prologue already seen this session" flag to `GameState` purely to let Start route
  straight to `Floor52`. Reasoning: (1) constitution Principle II (Simplicity & YAGNI) — a new
  persisted flag whose only purpose is menu routing is exactly the kind of speculative state the
  principle rules out when a simpler option satisfies the same need; (2) the repeat-viewing
  tedium this would exist to solve is instead solved one layer down, inside the Prologue scene
  itself: `specs/scenes/003-prologue-scene/spec.md` gives the Prologue sequence its own Skip
  control, so a player who has already seen it can bypass it in one tap without Main Menu ever
  needing to know whether they have. This keeps Start's behavior fixed and trivially testable
  (it always leads to the same place) while still giving repeat players a fast path.
- How To Play and Settings are shared overlay systems invoked additively on top of `MainMenu`
  (not scene swaps) — consistent with them also being reachable from the in-run Pause menu
  (`specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/spec.md`), which this scene does
  not otherwise depend on.
- Main Menu offers no "Continue"/"Resume run" option. GDD Ch. 9.1's checkpoints are per-floor and
  scoped to a single run in progress; there is no cross-session save/resume system anywhere in
  this project's scope, so Main Menu only ever offers a fresh Start.
- Main Menu does not implement an in-app "Quit" control — not required on the target mobile
  platform.

## Related

- GDD Ch. 15.1 (control/shell layout context), Ch. 18.1 (Must Have: "UX: pause, restart, audio
  settings, How To Play, feedback interaksi"), Ch. 9.1 (How To Play is where lives are stated).
- `specs/ROADMAP.md` §3 (scenes table, `002-main-menu-scene` row: depends on
  `game-shell-ui/002, 003`) and §5 (build sequence).
- `specs/systems/game-shell-ui/002-settings-menu-audio-controls/spec.md` — owns the Settings
  overlay's content; not redefined here.
- `specs/systems/game-shell-ui/003-how-to-play-screen/spec.md` — owns the How To Play overlay's
  content; not redefined here.
- `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md` — owns the
  scene-loading mechanism Start calls into; not redefined here.
- `specs/scenes/003-prologue-scene/spec.md` — owns the Skip control that makes the "Start always
  plays Prologue" Assumption above tolerable for repeat players within a session.
- `specs/scenes/007-good-ending-scene/spec.md` and `specs/scenes/008-bad-ending-scene/spec.md` —
  both return here per FR-008.
