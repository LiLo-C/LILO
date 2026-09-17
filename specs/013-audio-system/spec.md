# Feature Specification: Audio System & Dynamic Mixing

**Feature Branch**: `feat/013-audio-system`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Engine + placeholder sounds should land early (before Fase 3/4 playtests); final assets in Fase 6 — Art & Audio Pass (GDD 19.2) · **Proposed owner**: to be assigned at kickoff (GDD 19.1 lists no audio owner)

**GDD Sources**: Ch. 1.3, 4.2, 4.3, 5.2, 6.1, 6.3, 7.2, 10.3, 14.1, 14.2, 14.3, 18.1, 19.4

**Input**: User description: "Audio adalah core mechanic karena deteksi monster berbasis suara. Silence adalah senjata utama horror. Daftar asset: ambient (AC hum, electric buzz, nada kantor kosong), player (footsteps walk & sprint yang jelas berbeda, breathing, SFX interaksi, SFX ganti battery), monster (patrol, distant, investigation cue, chase, attack/catch), sistem (pickup, pintu terkunci, unlock, kedip lampu, splash text floor, UI button), naratif (bel lift, langkah kaki menyerupai atasan, tertawaan rekan kerja samar). Dynamic mixing per state monster: PATROL/jauh nyaris hening, INVESTIGATE cue halus, dekat player heartbeat/breathing, CHASE intensitas penuh. Hiding = audio teredam. Sumber: CC0 + buatan tim, semua CC0 wajib dicatat."

## Clarifications

- Q: Should monster sounds tell the player which direction the monster is in (stereo panning), or only how close it is (volume)? → A: [NEEDS CLARIFICATION: GDD 1.3 says the player only knows "something is near" through audio, and 14.2 says the INVESTIGATE cue should not reveal position. That suggests distance-only loudness with no panning. Directional audio would make evasion more skill-based but weaker on uncertainty. Recommended: distance-only by default, with `monsterAudioPanning` as a config switch for playtest comparison.]

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Hearing the Threat Change (Priority: P1)

A player hears near-silence while the monster patrols far away, a subtle cue when it starts investigating, their own heartbeat and breathing when it is close, and full-intensity sound in a chase, so they can read danger by ear alone.

**Why this priority**: Audio is a core mechanic, not decoration (GDD 14). Monster floors are unplayable as designed without it.

**Independent Test**: In the monster test arena with placeholder sounds, drive the monster through each state and listen with eyes closed.

**Acceptance Scenarios**:

1. **Given** the monster is in PATROL and far from the player, **When** the player listens, **Then** only ambience is audible, plus at most a barely audible patrol sound.
2. **Given** the monster switches to INVESTIGATE, **When** the player listens, **Then** a subtle cue plays that signals "something changed" but does not reveal the monster's position.
3. **Given** the monster is within `monsterNearDistance` of the player in any non-chase state, **When** the player listens, **Then** a heartbeat or breathing layer fades in.
4. **Given** the monster is in CHASE, **When** the player listens, **Then** the chase sound plays at full intensity.
5. **Given** the monster changes state, **When** the mix changes, **Then** layers crossfade over `audioStateCrossfadeDuration` instead of cutting abruptly.

---

### User Story 2 - My Own Sounds (Priority: P1)

A player hears their own footsteps, and sprinting clearly sounds louder and different from walking, so they understand that moving fast is noisy.

**Why this priority**: The player needs audible feedback for noise radius, which is never drawn on screen (GDD 7.2).

**Independent Test**: Walk, then sprint, then stop, in a quiet area.

**Acceptance Scenarios**:

1. **Given** the player walks, **When** they listen, **Then** walking footsteps play in time with movement.
2. **Given** the player sprints, **When** they listen, **Then** sprint footsteps are clearly distinct (louder, faster, heavier) from walking.
3. **Given** the player stops or hides, **When** they listen, **Then** footsteps stop.
4. **Given** the player picks up, installs a battery, opens a door, or tries a locked door, **When** the action happens, **Then** the matching sound plays exactly once.

---

### User Story 3 - The Building Sounds Empty (Priority: P2)

A player hears the quiet hum of an empty office — air conditioning, electric buzz, and a low empty-room tone — so the silence feels heavy rather than broken.

**Why this priority**: Builds atmosphere. The game works without it, but the pillar of silence does not.

**Independent Test**: Stand still on each floor for 30 seconds.

**Acceptance Scenarios**:

1. **Given** any floor, **When** the player stands still, **Then** ambience loops seamlessly with no audible gap or click at loop points.

---

### User Story 4 - Muffled Under a Desk (Priority: P2)

A player who hides hears the whole world go muffled, so hiding feels physical.

**Why this priority**: Required by spec 003 but only if hiding is not cut.

**Independent Test**: Hide during an active chase sound.

**Acceptance Scenarios**:

1. **Given** the player enters hiding, **When** the transition completes, **Then** all game audio except UI is low-pass muffled by `hideAudioMuffleAmount`, and it clears on leaving.

---

### User Story 5 - Story in Sound (Priority: P3)

A player occasionally hears sounds that don't belong: a monster call that resembles an office elevator bell, footsteps like a boss walking the floor, faint coworker laughter from empty rooms.

**Why this priority**: Narrative foreshadowing (GDD 10.3), placed by spec 016. This spec provides the sounds and trigger support.

**Independent Test**: Trigger each narrative sound in the arena.

**Acceptance Scenarios**:

1. **Given** a narrative sound trigger fires, **When** the sound plays, **Then** it never alerts the monster and never uses the player's noise system.

---

### Edge Cases

- Many one-shot sounds at once (pickup during a chase): the mix limits simultaneous voices so chase and heartbeat layers are never cut off.
- Flicker sound: plays only while in the Flickering light state (GDD 12.1), not in Normal, Critical, or Compact Darkness.
- Pause: gameplay audio pauses; UI sounds still play.
- Audio interruption (phone call): game auto-pauses (spec 012), and audio resumes cleanly after.
- Floor 52: no monster exists, but distant monster sounds play from triggers (spec 008).
- Volume settings at 0: mechanics still work. Audio is never required for the game to be technically completable, only for it to be fair.

## Requirements *(mandatory)*

### Functional Requirements

**Engine & mixing**

- **FR-001**: Gameplay systems MUST emit sound events (player movement, interactions, light-state changes, monster state and distance, floor transitions, UI, narrative triggers), and the audio system MUST map events to sounds without the gameplay systems depending on audio.
- **FR-002**: The mix MUST have at least three volume groups — Ambience, Effects (player, monster, system, narrative), and UI — controlled by the Master, Effects, and Ambience settings (spec 012).
- **FR-003**: Monster audio MUST follow GDD 14.2: PATROL/far → near-silent (ambience only); INVESTIGATE → subtle cue; near player (`monsterNearDistance`) → heartbeat/breathing layer; CHASE → full intensity. Transitions MUST crossfade over `audioStateCrossfadeDuration`.
- **FR-004**: Monster audio loudness MUST scale with distance to the player. Directional panning MUST follow the Clarifications decision.
- **FR-005**: While hiding, all non-UI audio MUST be muffled (spec 003).
- **FR-006**: The flicker sound MUST play only in the Flickering light state.
- **FR-007**: Audio MUST pause with the game and handle system interruptions (spec 012).
- **FR-008**: Audio output MUST NOT affect detection. Only the player noise system in spec 002 does. Narrative sounds and monster sounds are never "heard" by the monster.
- **FR-009**: The game's audio MUST play even when the device's silent switch is on, and MUST stop other apps' audio while playing, because audio is required to play fairly.

**Assets**

- **FR-010**: The asset set MUST cover every item in GDD 14.1: Ambient (AC hum, electric buzz, empty office tone); Player (walk footsteps, sprint footsteps, breathing, interaction, battery swap); Monster (patrol, distant, investigation cue, chase, attack/catch); System (pickup, locked door, unlock, light flicker, floor splash, UI button); Narrative (elevator bell, boss-like footsteps, faint coworker laughter).
- **FR-011**: Placeholder sounds MUST be replaceable by final sounds without code changes.
- **FR-012**: Every external CC0 asset MUST be recorded in the team's asset license registry with: asset name, source, license, modification status, integration owner, and commercial-use status (GDD 19.4). No asset may ship without a registry entry.
- **FR-013**: New configuration keys: `monsterNearDistance`, `audioStateCrossfadeDuration`, `monsterAudioPanning`, `maxSimultaneousEffects`, and per-group default volumes.

### Key Entities

- **Sound Event**: A typed event from gameplay with optional position and intensity.
- **Sound Asset**: A file mapped to an event, with group, looping flag, and source (team-made or CC0).
- **Mix State**: Current monster audio layer levels and muffle amount.
- **Asset License Registry Entry**: The six GDD 19.4 fields per external asset.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a blindfolded test in the arena, 4 of 5 testers correctly say whether the monster is patrolling far away, investigating, close, or chasing, from audio alone.
- **SC-002**: 5 of 5 testers can tell walking footsteps from sprint footsteps with eyes closed.
- **SC-003**: 100% of GDD 14.1 asset items exist in the build (placeholder or final) before Fase 6 starts, and 100% are final by the Fase 6 DoD.
- **SC-004**: 100% of CC0 assets in the build have a complete registry entry (audit at Fase 8).
- **SC-005**: Audio adds no frame drops below 60 fps on iPhone 17 during a chase with all layers active.

## Assumptions

- The GDD lists no music, so there is no music system. Tension comes from ambience and dynamic layers.
- The asset license registry is the team spreadsheet named in GDD 19.4. The plan may mirror it in the repo for audit.
- Recording and sourcing sounds is team work outside code. This spec's engine tasks can be finished with placeholders.

## Dependencies

- **Requires**: 001 (player events, light states), 002 (monster state and distance).
- **Integrates with**: 003 (muffle), 005 (lock and unlock), 008 (distant triggers), 011 (comic cues), 012 (settings, pause), 016 (narrative triggers).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 14, 19.4
