# Feature Specification: Monster AI State Machine & Noise Detection

**Feature Branch**: `feat/002-monster-ai-noise`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 2 — Monster Prototype (GDD 19.2) · **Proposed owner**: Radit (GDD 19.1)

**GDD Sources**: Ch. 1.3 (pillars), 6 (Monster AI), 7 (Noise & Detection), 17.3, 17.4, 18.1, 18.2

**Input**: User description: "Fase 2 — Monster Prototype. Satu monster dengan state machine PATROL → INVESTIGATE → CHASE → SEARCH → PATROL plus CATCH, nilai per floor (Floor 51 / Floor 50) dari GameConfig, spawn dari preset spawn point yang tervalidasi, dan sistem deteksi berbasis noise radius per aksi player (diam/hiding 0, jalan 1.0×, sprint 3.0×, interact 2.0×, pasang battery 1.5×). Tidak ada radar, minimap, atau indikator posisi monster. Cahaya senter tidak memicu deteksi. Diuji di satu map kecil (test arena). DoD: monster bisa patrol, mendengar sprint, investigate, chase, kehilangan jejak, lalu kembali patrol."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A Monster Roams the Arena (Priority: P1)

A player enters a small test arena and a monster is already present, walking a fixed patrol route between waypoints at a steady pace and making almost no sound, so the player learns something shares the space without ever being told where it is.

**Why this priority**: Every other monster behavior starts from PATROL. Without a monster that exists, moves, and navigates the map, no detection or chase can be tested.

**Independent Test**: Load the test arena with detection disabled and watch the monster loop its patrol route for several minutes without getting stuck, leaving the map, or teleporting.

**Acceptance Scenarios**:

1. **Given** the arena has loaded, **When** the first frame is shown, **Then** the monster already exists at one of the arena's preset spawn points — it never appears later or near the player.
2. **Given** the monster is in PATROL, **When** time passes with no player noise, **Then** it walks between its fixed waypoints in order at `patrolSpeed × walkSpeed`, routing around walls and furniture.
3. **Given** the arena is loaded several times, **When** the starting position is observed each time, **Then** it is always one of the preset spawn points and varies between loads when more than one valid preset exists.

---

### User Story 2 - Noise Draws the Monster (Priority: P1)

A player who walks, sprints, or interacts near the monster creates noise, and a monster within that noise radius goes to where the sound came from without knowing where the player actually is, so sound becomes the player's main risk.

**Why this priority**: Detection by noise is the GDD's only detection channel and is what makes movement choices matter. It is the core of the Fase 2 DoD ("mendengar sprint").

**Independent Test**: With the monster in PATROL, stand still inside what would be the sprint radius but outside the walk radius, then walk (no reaction), then sprint (monster reacts) — delivers a verifiable noise ladder.

**Acceptance Scenarios**:

1. **Given** the player is standing still, **When** the monster passes at any distance without touching the player, **Then** the monster is not alerted (noise 0).
2. **Given** the monster is in PATROL, **When** the player walks and the monster is closer than `noiseBaseRadius × noiseWalk`, **Then** the monster switches to INVESTIGATE and moves toward the point where the noise was made.
3. **Given** the monster is farther than the walk radius but closer than `noiseBaseRadius × noiseSprint`, **When** the player sprints, **Then** the monster switches to INVESTIGATE.
4. **Given** the monster is in INVESTIGATE and heading to a noise point, **When** the player walks away while staying outside the monster's walk-noise radius (so no new detection happens), **Then** the monster still goes to the original noise point, not the player's current position.
5. **Given** the monster reaches the noise point and hears nothing new for `investigateDuration`, **When** the timer ends, **Then** it returns to PATROL.
6. **Given** the monster is in PATROL within `noiseBaseRadius × noiseInteract`, **When** the player performs an interaction (pick up, open door) or within `noiseBaseRadius × noiseBatterySwap` installs a battery, **Then** the monster switches to INVESTIGATE toward that point.
7. **Given** the player's flashlight is shining directly on the monster, **When** the player makes no noise, **Then** the monster is not alerted (light never triggers detection).

---

### User Story 3 - Chase, Lose, Search, Give Up (Priority: P1)

When the monster hears the player again at close range it chases the player's last known position at a speed slower than the player's sprint; if the player breaks contact, the monster keeps pushing for a short hold time, searches the nearby area, and eventually returns to patrol.

**Why this priority**: This is the tension loop of the game and the explicit Fase 2 DoD ("chase, kehilangan jejak, lalu kembali patrol"). Chase speed below sprint is a hard GDD rule.

**Independent Test**: Deliberately provoke a chase, sprint away out of noise range, then stand still — observe CHASE → SEARCH → PATROL with durations matching config.

**Acceptance Scenarios**:

1. **Given** the monster is in INVESTIGATE, **When** it detects player noise whose source is within `chaseTriggerDistance` of the monster, **Then** it switches to CHASE targeting that noise point as the last known position.
2. **Given** the monster is in INVESTIGATE, **When** it detects noise farther than `chaseTriggerDistance`, **Then** it stays in INVESTIGATE, retargets to the new noise point, and restarts its investigate timer.
3. **Given** the monster is in CHASE, **When** the player keeps making detected noise, **Then** the last known position updates to each new noise point and the monster keeps chasing at `chaseSpeed × walkSpeed`.
4. **Given** the monster is in CHASE, **When** no new detection happens for `chaseHoldDuration`, **Then** it switches to SEARCH around the last known position.
5. **Given** the monster is in SEARCH, **When** it detects the player's noise, **Then** it switches back to CHASE.
6. **Given** the monster is in SEARCH, **When** `searchDuration` passes without detection, **Then** it returns to PATROL, resuming from the nearest patrol waypoint.
7. **Given** the player is sprinting directly away from a chasing monster on open ground, **When** 5 seconds pass, **Then** the distance between them has grown.

---

### User Story 4 - Getting Caught (Priority: P2)

If the monster touches the player, the player is caught immediately — no quick-time event and no escape — and the game raises a single "player caught" outcome that later specs turn into death, life loss, and floor reset.

**Why this priority**: The catch closes the loop but its consequences (lives, reset, bad ending) belong to spec 007. Here it only needs to fire reliably and once.

**Independent Test**: Stand still in the monster's patrol path; when it walks into the player, the catch outcome fires exactly once and the arena resets to its start state.

**Acceptance Scenarios**:

1. **Given** the monster is in any state, **When** its body comes within `catchRadius` of the player, **Then** the catch outcome fires exactly once, player input stops, and the monster stops moving.
2. **Given** spec 007 is not implemented yet, **When** a catch fires, **Then** the arena reloads to its start state with the monster placed at a preset spawn point in PATROL.

---

### User Story 5 - Floor-Specific Aggression (Priority: P2)

The same monster behaves more aggressively on Floor 50 than on Floor 51 — faster patrol and chase, longer investigate, chase hold, and search — using only configuration, so difficulty scales without new code.

**Why this priority**: Required for Floors 51 and 50 (specs 009, 010), but the arena test in US1–US4 already proves the behavior with one profile.

**Independent Test**: Switch the arena's monster profile between Floor 51 and Floor 50 (debug build only) and time each state and speed.

**Acceptance Scenarios**:

1. **Given** the Floor 51 profile, **When** timings are measured, **Then** investigate lasts 4s, chase hold 3s, search 6s, patrol speed 1.0×, chase speed 1.4×.
2. **Given** the Floor 50 profile, **When** timings are measured, **Then** investigate lasts 6s, chase hold 5s, search 8s, patrol speed 1.2×, chase speed 1.5×.
3. **Given** a floor profile where `monsterActive` is false (Floor 52), **When** the floor loads, **Then** no monster exists and no detection runs.

---

### Edge Cases

- A noise event happens exactly on the radius boundary: detection requires distance strictly less than the radius (GDD 7.2: "lebih kecil dari").
- The noise point is unreachable (e.g., on the other side of a wall with no path): the monster moves to the nearest reachable point and then continues its INVESTIGATE/SEARCH timers normally — it never stalls forever.
- The player makes a noise while the monster is walking back to PATROL: it is treated as a PATROL-state detection.
- Multiple noises in the same frame (e.g., sprinting while picking up a battery): the largest radius applies; one detection per frame.
- The player is caught during the same frame they reach an exit door: the catch wins only if spec 004's transition has not already started; once a floor transition begins, catches are ignored.
- The monster gets physically stuck for longer than `monsterStuckTimeout`: it warps to the nearest navigation point it can reach, out of the player's view — a safeguard, not a behavior.
- Config sets `chaseSpeed ≥ sprintMultiplier`: invalid; debug builds MUST fail loudly at load and tests MUST catch it (hard rule, GDD 6.2).
- Pause (spec 012): all monster timers and movement freeze.

## Requirements *(mandatory)*

### Functional Requirements

**Monster presence & spawn**

- **FR-001**: On any floor whose profile has `monsterActive = true`, exactly one monster MUST exist from the first frame of the floor. It MUST never be created, hidden, or teleported near the player during play (except the stuck safeguard, out of view).
- **FR-002**: The monster's starting position MUST be chosen at random from the floor's preset monster spawn points that pass validation (FR-003). Only preset points may be used, never a free random position.
- **FR-003**: A monster spawn point is valid only if it is (a) reachable by navigation from the player's floor entry point, (b) at least `monsterSpawnMinDistance` from the player's entry point, (c) outside the player's initial line of sight, (d) at least `monsterSpawnObjectiveClearance` from keys and the exit door, and (e) cannot lead to a catch within `monsterSpawnSafeSeconds` of floor start if the player does nothing. Checks (a)–(d) MUST be automated in tests against every floor's data. Check (e) MUST be verified by playtest per floor.
- **FR-004**: Exactly one monster type exists. No second monster or enemy type may be introduced (GDD 18.2).

**State machine**

- **FR-005**: The monster MUST be in exactly one of PATROL, INVESTIGATE, CHASE, SEARCH, or CATCH at any time, and MUST only change state through the transitions in US1–US4. No other transitions are allowed.
- **FR-006**: PATROL — the monster MUST walk the floor's authored patrol waypoints in order, looping, at `patrolSpeed × walkSpeed`.
- **FR-007**: INVESTIGATE — the monster MUST move toward the most recent detected noise point at patrol speed, wait at the point, and return to PATROL after `investigateDuration` seconds without a new detection (timer starts on arrival or when the point is unreachable).
- **FR-008**: CHASE — the monster MUST move toward the last known position (the most recent detected noise point) at `chaseSpeed × walkSpeed`. It MUST NOT use the player's real-time position. It switches to SEARCH after `chaseHoldDuration` seconds without a new detection.
- **FR-009**: SEARCH — the monster MUST wander within `searchRadius` of the last known position and return to PATROL after `searchDuration` seconds without detection, resuming at the nearest patrol waypoint.
- **FR-010**: CATCH — when the monster comes within `catchRadius` of the player in any state, the system MUST raise one "player caught" outcome, freeze player input and monster movement, and hand off to the fail-state flow (spec 007, or an arena reload until 007 exists). There is no QTE and no escape.
- **FR-011**: Chase speed MUST be strictly lower than the player's sprint speed on every floor. The configuration MUST be validated at load; an invalid value is a build/test failure, not a runtime clamp.

**Noise & detection**

- **FR-012**: The player MUST emit noise with radius `noiseBaseRadius × multiplier`, where the multiplier is 0 when idle or hiding, `noiseWalk` (1.0) when walking, `noiseSprint` (3.0) when sprinting, `noiseInteract` (2.0) for an interaction (pickup, door open or open attempt), and `noiseBatterySwap` (1.5) for installing a battery. Movement noise is continuous. Interaction and swap noise are one-shot pulses at the moment of the action.
- **FR-013**: A detection MUST occur when the straight-line distance from the monster to the noise source is strictly less than the current noise radius. When several noises happen at once, the largest radius wins.
- **FR-014**: A detection MUST give the monster only the world position of the noise. The monster MUST NOT receive the player's identity, facing, or later real-time position.
- **FR-015**: Flashlight light MUST NOT contribute to detection in any way (GDD 7.2, 18.2).
- **FR-016**: The noise radius, the monster's position, and its state MUST never be drawn or indicated on screen in release builds — no radar, minimap, outline through walls, or direction indicator (GDD 1.3, 7). Debug builds MAY show a developer overlay behind a debug-only toggle.
- **FR-017**: The monster's current state and its distance to the player MUST be readable by other systems (audio mixing in spec 013, haptics in spec 014) without those systems changing monster behavior.

**Configuration**

- **FR-018**: All values in this spec MUST live in the single configuration source (001 FR-015), keyed per floor where the GDD gives per-floor values: `monsterActive`, `patrolSpeed`, `chaseSpeed`, `investigateDuration`, `chaseHoldDuration`, `searchDuration` (GDD 17.4), and `noiseBaseRadius`, `noiseWalk`, `noiseSprint`, `noiseInteract`, `noiseBatterySwap`, `noiseHiding` (GDD 17.3). New keys introduced here: `chaseTriggerDistance`, `searchRadius`, `catchRadius`, `monsterSpawnMinDistance`, `monsterSpawnObjectiveClearance`, `monsterSpawnSafeSeconds`, `monsterStuckTimeout`.
- **FR-019**: `noiseBaseRadius` MUST have a locked, playtested value before this spec is marked done (GDD 21 Open Item: "Harus selesai sebelum Fase 2").

**Test arena**

- **FR-020**: A debug-only test arena MUST exist: one small fixed map with walls, at least one obstacle loop the player can circle, a patrol route of at least 4 waypoints, at least 2 monster spawn presets, a door, a battery, and a placeholder 3D monster rendered in the same Unity URP 3D scene as the player.

### Key Entities

- **Monster**: The single enemy. Has a state (PATROL/INVESTIGATE/CHASE/SEARCH/CATCH), world position, current target point, last known position, state timer, and the active floor profile.
- **Monster Floor Profile**: Per-floor tuning set (active flag, speeds, durations) from GDD 17.4.
- **Patrol Route**: Ordered, looping list of waypoints authored in the floor's level data (spec 004).
- **Monster Spawn Point**: Authored candidate start position with the validity rules in FR-003.
- **Noise Event**: A world position plus a radius, emitted continuously (movement) or as a pulse (interaction, battery swap).
- **Player Caught Outcome**: One-time signal consumed by the fail-state flow (spec 007).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a 10-minute unattended arena run with detection off, the monster completes its patrol loop continuously with zero stuck events longer than 2 seconds.
- **SC-002**: A tester can reproduce the full Fase 2 DoD sequence — patrol → hears sprint → investigate → chase → loses track → search → patrol — on 5 of 5 attempts.
- **SC-003**: Measured state durations and speeds match the active floor profile within ±10% on both the Floor 51 and Floor 50 profiles.
- **SC-004**: A player who sprints directly away from a chasing monster on open ground always increases the distance between them (0 catches in 10 straight-line sprint trials).
- **SC-005**: With a monster and player both on screen and the monster in CHASE, the game holds ≥60 fps on iPhone 17.
- **SC-006**: In a blind test, 3 of 3 testers say they could not tell the monster's exact position from anything on screen, and could only guess from sound or seeing it in their light.
- **SC-007**: Automated tests cover every state transition in FR-005–FR-010, the strict-less-than detection boundary, the largest-radius rule, and config validation that rejects `chaseSpeed ≥ sprintMultiplier`.

## Assumptions

- Noise distance is straight-line and ignores walls, per the GDD's plain distance rule (7.2). Occlusion-based hearing is out of scope. If playtests show sound "through walls" feels unfair, the fix is tuning `noiseBaseRadius`, not a new system.
- GDD 6.1 says INVESTIGATE → CHASE when the player is "terdeteksi lagi di jarak dekat". "Close range" is defined here as a new detection whose source is within `chaseTriggerDistance` of the monster (tuned on device).
- The monster has no vision-based detection. The only sight-related rule is in hiding (spec 003: entering a hiding spot in view of a chasing monster).
- Monster audio (patrol, investigate cue, chase) is produced by spec 013. This spec only exposes the state. Arena testing may use placeholder sounds.
- The final monster model and animations come from spec 015. This spec uses a placeholder 3D figure.
- Death sequence, lives, floor reset, and bad ending are spec 007. Until then, a catch reloads the arena.
- Regular (non-locked) doors do not block the monster's navigation, and locked doors do (see spec 004 assumption). The test arena's door follows the same rule.

## Dependencies

- **Requires**: 001-core-prototype (movement states, interaction events, battery install event, the unified Unity 3D scene, GameConfig).
- **Consumed by**: 003 (hiding), 007 (catch → death), 009/010 (floors), 013 (audio mix), 014 (haptics).
- **Floor data format**: spec 004 defines where patrol routes and spawn presets live. The test arena may hardcode its own until 004 exists, but MUST migrate to 004's format when 004 lands.

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[constitution]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 6, 7, 17.3, 17.4
