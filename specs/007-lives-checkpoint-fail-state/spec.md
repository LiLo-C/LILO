# Feature Specification: Lives, Checkpoint & Fail State

**Feature Branch**: `feat/007-lives-checkpoint-fail-state`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 4 — Floor 51 & 50 (GDD 19.2) · **Proposed owner**: Calzy (state management)

**GDD Sources**: Ch. 2.3, 6.1 (CATCH), 9.1, 9.2, 9.3, 15.1, 15.4, 17.5

**Input**: User description: "Player punya 3 lives yang disembunyikan dari HUD (hanya disebut sekali di How To Play). Checkpoint di awal tiap floor. Tertangkap monster → death sequence singkat tanpa QTE → lives −1 (tidak ditampilkan) → kalau masih ada lives: reset seluruh state floor dan respawn di awal floor; kalau habis: Bad Ending. Reset mencakup posisi player, battery terpasang 100% dan slot cadangan kosong, battery di map, key, pintu, dan monster kembali PATROL di salah satu preset spawn point. Satu death sequence untuk semua kasus."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Caught, Then Back to the Start of the Floor (Priority: P1)

A player caught by the monster sees one short death sequence and then finds themselves back at the start of the current floor with everything on that floor exactly as it was when they first arrived.

**Why this priority**: Without this, a catch has no consequence and the game cannot be played past Floor 52.

**Independent Test**: On a monster floor, pick up a key, use a battery, open a door, get caught — then compare the floor state to a fresh load.

**Acceptance Scenarios**:

1. **Given** the monster catches the player, **When** the catch fires, **Then** player input is disabled and a single death sequence plays for `deathSequenceDuration`, with no QTE and no way to escape.
2. **Given** the player had lives remaining before the catch, **When** the death sequence ends, **Then** the floor is reset and the player stands at the floor's entry point with control returned.
3. **Given** a floor reset, **When** the state is compared to a fresh load of the same floor, **Then** player position, installed battery (100%), spare slot (empty), world batteries and respawn timers, keys, doors, and hiding state all match. The monster is in PATROL at a valid preset spawn point.

---

### User Story 2 - The Last Death Ends the Run (Priority: P1)

A player who is caught with no lives left goes straight into the Bad Ending instead of respawning.

**Why this priority**: The Bad Ending is untouchable scope (GDD 18.3), and this is its only trigger.

**Independent Test**: Get caught three times in one run.

**Acceptance Scenarios**:

1. **Given** a new run, **When** the player is caught the first and second time, **Then** they respawn at the current floor's entry each time.
2. **Given** the player has been caught twice this run, **When** they are caught a third time, **Then** after the death sequence the Bad Ending starts (spec 011) instead of a respawn.

---

### User Story 3 - Lives Stay Hidden (Priority: P2)

A player never sees how many lives they have during play. The number is mentioned once, on the How To Play screen before the game starts.

**Why this priority**: A deliberate tension choice (GDD 9.1), easy to break by accident with a HUD element or a death message.

**Independent Test**: Play a run with two deaths and inspect every screen for a lives count.

**Acceptance Scenarios**:

1. **Given** any gameplay moment, death sequence, pause menu, or floor transition, **When** the screen is inspected, **Then** no lives count, heart icon, or "lives remaining" text appears.

---

### Edge Cases

- A catch during the descend transition: ignored (specs 002, 004).
- A catch while the player is mid-install or mid-pickup: the catch wins; the action is cancelled and the reset restores the floor anyway.
- A catch while hiding "seen" (spec 003): same single death sequence.
- Floor 52 has no monster: lives cannot be lost there.
- Lives carry across floors: two deaths on Floor 51 leave one life for Floor 50.
- Pause during the death sequence: the sequence freezes and resumes (spec 012).
- The app is backgrounded during a death sequence: on return the game is paused (spec 012), then continues the flow.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Each new run MUST start with `lives` = 3 (GDD 17.5). Lives MUST persist across floors within the run and MUST only reset when a new run starts.
- **FR-002**: The checkpoint MUST be the entry point of the current floor, set on floor arrival (spec 004). There MUST be no mid-floor checkpoints (GDD 9.1).
- **FR-003**: On the "player caught" outcome (spec 002), the system MUST disable input and play one death sequence of `deathSequenceDuration`. The sequence MUST be identical regardless of monster state, player action, or location (GDD 9.3).
- **FR-004**: After the death sequence, the system MUST decrement lives by one. If lives > 0 it MUST reset the floor and respawn the player at the checkpoint. If lives = 0 it MUST start the Bad Ending (spec 011).
- **FR-005**: A floor reset MUST restore, without exception (GDD 9.2): player position to the entry point; installed battery to 100%; spare slot to empty; world batteries and respawn timers to the floor's initial state (spec 006); keys to authored positions and held keys cleared (spec 005); all doors to their initial locked or closed state (spec 005); monster to PATROL at a randomly chosen valid preset spawn point (spec 002); hiding state cleared (spec 003); light state recomputed from the refilled battery.
- **FR-006**: Lives MUST NOT appear anywhere during gameplay, the death sequence, pause, or transitions. The only place the count may appear is the How To Play screen shown before a run (spec 012).
- **FR-007**: The reset MUST be implemented as a single operation that every stateful floor system participates in, so a system added later cannot be forgotten. Tests MUST compare the full post-reset state to a fresh floor load.
- **FR-008**: New configuration keys: `deathSequenceDuration`, plus `lives` and `checkpointPerFloor` from GDD 17.5.

### Key Entities

- **Run State**: Lives remaining, current floor, checkpoint.
- **Death Sequence**: One short, non-interactive presentation after a catch.
- **Floor Reset**: The operation restoring every floor system to its initial state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Automated tests confirm post-reset floor state equals a fresh load (except the monster's preset choice) for every floor system, in 100% of test cases.
- **SC-002**: From catch to regaining control after a respawn takes no longer than `deathSequenceDuration` + 2 seconds.
- **SC-003**: In 5 of 5 test runs, the third catch leads to the Bad Ending and never to a respawn.
- **SC-004**: A worst-case retry (death near the end of a floor) costs at most about 5 minutes of replay (GDD 1.4), confirmed during floor playtests.
- **SC-005**: 0 lives indicators found in a screen-by-screen audit of a run with deaths.

## Assumptions

- The death sequence content (animation, camera, sound, haptic) is a short placeholder here. Final visuals come from spec 015, sound from 013, haptic from 014.
- Lives are not saved between app launches because runs are not saved (spec 012 assumption).
- Until spec 011 exists, "Bad Ending" can be a placeholder screen that returns to the start.

## Dependencies

- **Requires**: 002 (catch outcome, spawn presets), 004 (checkpoint, floor load), 005 (key and door reset), 006 (battery reset), 003 (hiding reset, if not cut).
- **Consumed by**: 011 (Bad Ending trigger), 012 (How To Play lives mention), 009/010 (floors with a monster).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 9
