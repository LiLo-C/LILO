# Feature Specification: Hiding Under Desks

**Feature Branch**: `feat/003-hiding-under-desk`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 2 — Monster Prototype (GDD 19.2: "Hiding sudah bisa diuji") · **Proposed owner**: Radit (monster interaction) with Eca (light/HUD)

**GDD Sources**: Ch. 4.2, 4.3, 5.1, 7.1, 14.2, 15.4, 18.1, 18.2, 18.3 (cut candidate #1), 21

**Input**: User description: "Hiding terbatas: player hanya bisa sembunyi di kolong meja (bukan locker/kamar mandi/lemari). Masuk/keluar lewat tombol aksi yang sama. Saat hiding: tidak bisa bergerak, noise 0, audio teredam (low-pass), senter otomatis diredupkan, battery tetap berkurang. Monster SEARCH tidak bisa menemukan player yang hiding, kecuali player masuk hiding saat monster sudah CHASE dan melihat langsung. Fitur ini kandidat potong #1 — harus bisa dimatikan tanpa merusak game."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Duck Under a Desk (Priority: P1)

A player near a desk that is a hiding spot presses the action button to crawl under it. While hidden they cannot move, make no noise, their flashlight dims because Eddie covers it, and the world sounds muffled. Pressing the button again brings them back out.

**Why this priority**: This is the whole mechanic. Without entering and leaving hiding there is nothing to balance against the monster.

**Independent Test**: In the test arena with no monster, walk to a hiding desk, enter, try to move, check the light and audio, leave — delivers the full enter/stay/exit loop.

**Acceptance Scenarios**:

1. **Given** the player is within interaction range of a hiding desk and no nearer interactable exists, **When** they look at the action button, **Then** it offers "Hide" and the desk is highlighted.
2. **Given** the player presses "Hide", **When** the transition finishes, **Then** the player is under the desk, joystick input does not move them, and the action button offers "Leave".
3. **Given** the player is hiding, **When** time passes, **Then** the installed battery keeps draining at the normal rate and the light state still changes by charge.
4. **Given** the player is hiding, **When** they look at the scene, **Then** the flashlight is visibly dimmed or covered, and game audio is muffled.
5. **Given** the player presses "Leave", **When** the transition finishes, **Then** the player is standing next to the desk with movement, full light radius for the current light state, and normal audio restored.

---

### User Story 2 - Hiding Beats a Search (Priority: P1)

A player who hides before the monster has them in a chase is safe from a searching or patrolling monster, even if it walks right past the desk.

**Why this priority**: This is what gives hiding its value. It depends on spec 002's SEARCH state being solid, which is exactly the GDD 21 open item ("tergantung seberapa rapi state SEARCH").

**Independent Test**: Make noise to start an INVESTIGATE, hide before it becomes a chase, and let the monster search next to the desk until it returns to PATROL.

**Acceptance Scenarios**:

1. **Given** the player is hiding, **When** the monster is in PATROL, INVESTIGATE, or SEARCH and passes within touching distance of the desk, **Then** the player is not caught and the monster is not alerted.
2. **Given** the player hid while the monster was not in CHASE, **When** the monster's SEARCH timer runs out, **Then** the monster returns to PATROL and the player can leave safely.

---

### User Story 3 - Hiding in Plain Sight Fails (Priority: P2)

A player who dives under a desk while the monster is actively chasing and can see them is found anyway, so hiding is not a panic button.

**Why this priority**: The GDD exception keeps hiding honest, but US1–US2 already deliver a usable mechanic without it.

**Independent Test**: Provoke a chase, let the monster close to within `hideSpottedDistance` with a clear line between them, hide — the monster still reaches and catches the player.

**Acceptance Scenarios**:

1. **Given** the monster is in CHASE, within `hideSpottedDistance` of the player, with no wall or solid furniture blocking the straight line between them, **When** the player enters hiding, **Then** the monster keeps chasing the hiding spot and catches the player on contact.
2. **Given** the monster is in CHASE but farther than `hideSpottedDistance` or with the line blocked, **When** the player enters hiding, **Then** the hide counts as unseen: the monster continues to the last known position and then searches (US2 rules apply).

---

### User Story 4 - Cut Without Breaking the Game (Priority: P3)

If the team triggers cut #1, hiding is switched off in configuration and the game still plays correctly: desks become ordinary furniture and nothing references hiding.

**Why this priority**: GDD 18.3 makes hiding the first thing cut when time runs short. The switch must exist before the pressure does.

**Independent Test**: Set `hidingEnabled = false`, play a floor — no "Hide" prompt anywhere, desks block movement as normal, no errors.

**Acceptance Scenarios**:

1. **Given** `hidingEnabled` is false, **When** the player stands at any desk, **Then** no hide option appears and the desk behaves as solid furniture.

---

### Edge Cases

- The player is caught during the enter-hiding transition: if the monster touches the player before the transition finishes, the catch wins.
- The monster is standing where the player would exit: "Leave" is still allowed and the player exits into the catch. No auto-shifting exit position.
- The battery reaches 0% while hiding: light state becomes Compact Darkness as usual; hiding continues.
- The player has a spare battery and charge falls to ≤10% while hiding: install is allowed from hiding (the install noise is 1.5× and, being a pulse, can alert the monster — consistent with FR-012 in spec 002).
- Two hiding desks are within range: nearest one wins (001 action-button priority rule).
- A floor reset (spec 007) happens while hiding: the player respawns standing, not hiding.
- Pause (spec 012) while hiding: hiding state is kept after resume.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Only authored hiding desks (level data, spec 004) MUST be hideable. No lockers, bathrooms, cabinets, or other hiding types may exist (GDD 4.3, 18.2).
- **FR-002**: Entering and leaving hiding MUST use the single context-sensitive action button with "Hide" / "Leave" states, following the nearest-object priority rule from 001's action-button contract.
- **FR-003**: While hiding, the player MUST be unable to move, and the player's movement noise multiplier MUST be `noiseHiding` (0).
- **FR-004**: While hiding, battery drain MUST continue unchanged (GDD 4.3, 5.1).
- **FR-005**: While hiding, the flashlight MUST be visibly dimmed or covered, and all non-UI game audio MUST be muffled (low-pass). Leaving hiding MUST restore both within `hideTransitionDuration`.
- **FR-006**: A hiding player MUST NOT be caught or detected by the monster in PATROL, INVESTIGATE, or SEARCH, including when the monster's body overlaps the desk.
- **FR-007**: If the player enters hiding while the monster is in CHASE, within `hideSpottedDistance`, and with an unobstructed straight line between monster and player, the hide MUST count as "seen": the monster keeps chasing the hiding position and the catch rule from spec 002 applies. Otherwise the hide is "unseen" and FR-006 applies.
- **FR-008**: Entering and leaving hiding MUST each take a short transition (`hideTransitionDuration`) during which the player cannot move and can still be caught.
- **FR-009**: A config switch `hidingEnabled` MUST disable the entire mechanic (no prompts, desks act as normal solid furniture) with no other code or data change (GDD 18.3 cut #1).
- **FR-010**: New configuration keys MUST live in the single configuration source: `hidingEnabled`, `hideSpottedDistance`, `hideTransitionDuration`, `hideLightDimFraction`, `hideAudioMuffleAmount`.

### Key Entities

- **Hiding Spot (Desk)**: Authored desk position with an entry side, flagged hideable in floor data.
- **Player Hiding State**: not hiding / entering / hiding / leaving, plus whether the current hide was "seen".

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 10 trials where the player hides before a chase starts, the monster never catches the hidden player (0/10 catches).
- **SC-002**: In 10 trials where the player hides in clear view of a chasing monster within `hideSpottedDistance`, the player is caught 10/10 times.
- **SC-003**: With `hidingEnabled = false`, a full playthrough of the test arena shows no hide prompt and no errors.
- **SC-004**: 3 of 3 playtesters describe hiding as useful but "not a place to stay forever", because the battery keeps draining.
- **SC-005**: The team records the GDD 21 decision "does hiding stay in the final build?" by the end of Fase 2.

## Assumptions

- Final muffled-audio sound design belongs to spec 013. This spec requires the muffle to be audible, using any placeholder.
- The "camera transition" named in GDD 4.2 is a short visual transition. The camera stays orthographic with fixed zoom (GDD 13) — no zoom-in.
- "Sees directly" is modeled as distance plus unobstructed line (FR-007), because the monster otherwise has no vision system.
- Hiding does not refill, pause, or otherwise protect the battery.

## Dependencies

- **Requires**: 001-core-prototype (action button, light system), 002-monster-ai-noise (states, catch).
- **Integrates with**: 004 (desk placement in floor data), 007 (reset clears hiding), 008 (Floor 52 teaches hiding), 013 (muffled mix).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 4.3, 18.3
