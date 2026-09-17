# Feature Specification: Game Shell — Title, How To Play, Pause, Restart & Audio Settings

**Feature Branch**: `feat/012-game-shell-ux`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 5 (needed for playtests in Fase 7) · **Proposed owner**: Eca (UI/HUD) with Salwa (UI art)

**GDD Sources**: Ch. 1.3, 9.1, 15.1, 15.4, 18.1

**Input**: User description: "UX must-have dari Scope Lock: pause, restart, audio settings, How To Play, feedback interaksi. Tombol pause di pojok atas membuka menu pause. Layar How To Play singkat sebelum game dimulai — satu-satunya tempat jumlah lives disebut. HUD hanya indikator battery + slot cadangan; tidak ada indikator lives, tidak ada minimap. Landscape only."

## Clarifications

- Q: What does "Restart" in the pause menu do? → A: [NEEDS CLARIFICATION: GDD 18.1 lists "restart" without defining it. Recommended: "Restart run" — start a new run at Floor 52 with 3 lives (prologue skippable). Alternative: "Restart floor" — return to the current floor's checkpoint; this must then decide whether it costs a life, because a free floor restart undermines hidden lives.]

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start a Run (Priority: P1)

A player opens the app, lands on a simple title screen, reads a short How To Play screen before their first run, and starts the game.

**Why this priority**: Every playtest and the final build start here.

**Independent Test**: Fresh install → launch → title → How To Play → prologue.

**Acceptance Scenarios**:

1. **Given** the app launches, **When** loading finishes, **Then** a title screen shows Start, How To Play, and Settings, in landscape.
2. **Given** the player taps Start for the first time on this install, **When** the run begins, **Then** the How To Play screen is shown first, then the prologue (spec 011).
3. **Given** the How To Play screen, **When** it is read, **Then** it explains the joystick (full push = sprint), the action button, the flashlight and batteries, that noise attracts danger, and that the player has 3 lives. This is the only place lives are mentioned.
4. **Given** the player has seen How To Play before, **When** they tap Start, **Then** the run starts at the prologue, and How To Play stays available from the title screen.

---

### User Story 2 - Pause Anywhere in Gameplay (Priority: P1)

A player taps the pause button in the top corner, and the whole game world freezes until they resume.

**Why this priority**: Needed for real-world play and for playtests. Timing systems (battery, respawn, monster) must respect it.

**Independent Test**: Pause with the battery at a known value during a chase for 60 seconds, resume, and compare.

**Acceptance Scenarios**:

1. **Given** gameplay is running, **When** the player taps pause, **Then** battery drain, respawn timers, monster movement and timers, death sequence, splash text, and gameplay audio all freeze, and a pause menu appears.
2. **Given** the game is paused, **When** the player taps Resume, **Then** everything continues from exactly where it stopped.
3. **Given** gameplay is running, **When** the app goes to the background or is interrupted (call, Control Center), **Then** the game is paused automatically and stays paused on return.

---

### User Story 3 - Restart and Quit (Priority: P2)

A player who wants to start over can restart from the pause menu, or quit back to the title screen.

**Why this priority**: Scope-lock must-have, but less critical than pause itself.

**Independent Test**: Pause mid-Floor 51 → Restart; pause → Quit to title.

**Acceptance Scenarios**:

1. **Given** the pause menu, **When** the player chooses Restart and confirms, **Then** the behavior matches the Clarifications decision.
2. **Given** the pause menu, **When** the player chooses Quit to Title and confirms, **Then** the run ends without saving and the title screen shows.

---

### User Story 4 - Adjust Audio (Priority: P2)

A player can change volume levels from the title screen or pause menu, and the setting is remembered next time they open the app.

**Why this priority**: Scope-lock must-have. Audio is a core mechanic, so players must be able to set it comfortably.

**Independent Test**: Lower effects volume, relaunch the app, confirm the setting stuck and applies in gameplay.

**Acceptance Scenarios**:

1. **Given** the Settings screen, **When** the player adjusts Master, Effects, or Ambience volume, **Then** the change is audible immediately in a preview or on return to gameplay.
2. **Given** the player changed audio settings, **When** the app is relaunched, **Then** the settings are the same.

---

### Edge Cases

- Pause tapped during a descend transition or death sequence: allowed; both freeze and continue on resume.
- Pause during a comic (prologue or ending): the pause button is hidden; comics have their own skip (spec 011).
- Restart or Quit confirmation dismissed: return to the pause menu with nothing changed.
- The How To Play screen opened from the title screen mid-install: it still mentions 3 lives, which is allowed because it is before a run.
- Accidental touches: the pause button's touch area must not overlap the joystick or action button areas.
- Devices with a Dynamic Island or rounded corners: all HUD and menu controls stay inside the safe area.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST open to a title screen with Start, How To Play, and Settings, and MUST run in landscape only.
- **FR-002**: How To Play MUST be shown automatically before the first run on an install and MUST be available from the title screen. It MUST mention the lives count (3). The lives count MUST NOT appear anywhere else (GDD 9.1).
- **FR-003**: The gameplay HUD MUST contain only: the joystick (left), action button (right), pause button (top corner), and battery indicator with spare slot (from 001). It MUST NOT contain a lives indicator, minimap, objective tracker, or monster indicator (GDD 1.3, 8.2, 15.1).
- **FR-004**: Pause MUST freeze all gameplay time: battery drain, respawn timers, monster movement and timers, hiding transitions, death sequence, splash text, and gameplay audio and haptics.
- **FR-005**: The game MUST auto-pause when the app leaves the foreground or its audio is interrupted.
- **FR-006**: The pause menu MUST offer Resume, Restart, Settings, and Quit to Title. It MUST NOT offer How To Play, because that would show the lives count mid-run.
- **FR-007**: Restart and Quit to Title MUST require confirmation. Restart MUST follow the Clarifications decision.
- **FR-008**: Settings MUST provide Master, Effects, and Ambience volume, applied immediately and persisted across launches.
- **FR-009**: Run progress MUST NOT be saved. Quitting the app or Quit to Title ends the run.
- **FR-010**: Menu and HUD layout values (pause button size, position, touch radius) MUST come from the single configuration source, like the 001 controls (GDD 15.3).
- **FR-011**: All interactive feedback (highlight, pickup, locked, unlock, install) MUST stay consistent across object types, as defined in 001 and 005 (GDD 18.1 "feedback interaksi").

### Key Entities

- **Title Screen**, **How To Play Screen**, **Pause Menu**, **Settings Screen**.
- **Audio Settings**: Master, Effects, and Ambience volume values, persisted locally.
- **Onboarding Flag**: Whether How To Play has been shown on this install, persisted locally.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After a 60-second pause at any gameplay moment, battery charge, respawn timers, and monster state match their pre-pause values exactly (automated test).
- **SC-002**: 5 of 5 first-time players get from launch to controlling Eddie on Floor 52 without help.
- **SC-003**: A screen-by-screen audit finds the lives count only on How To Play.
- **SC-004**: Audio settings survive app relaunch in 5 of 5 tries.
- **SC-005**: 0 accidental pauses reported in playtests caused by the pause button overlapping other controls.

## Assumptions

- Persisted data is limited to audio settings and the onboarding flag. Per constitution Principle V, the plan MUST document the storage format and a migration note, even if trivial.
- A music volume slider is not included because the GDD audio list (14.1) contains no music. "Ambience" covers AC hum, electric buzz, and empty-office tone.
- A haptics on/off toggle is not included. The game follows the system haptics setting (spec 014).
- Menu visual design comes from spec 015. Placeholder styling is acceptable here.

## Dependencies

- **Requires**: 001 (HUD), 004 (run start), 007 (lives), 011 (prologue flow).
- **Affects**: every system with timers (002, 003, 006, 007) must honor pause; 013 (audio settings apply to mix).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 15, 18.1
