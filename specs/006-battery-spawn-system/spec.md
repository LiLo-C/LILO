# Feature Specification: Battery Placement & Respawn System

**Feature Branch**: `feat/006-battery-spawn-system`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Static mode for Fase 3 (Floor 52), respawn mode for Fase 4 (Floors 51 & 50) (GDD 19.2) · **Proposed owner**: Eca (GDD 19.1)

**GDD Sources**: Ch. 1.3 (Meaningful Resource), 3, 3.1, 5.1, 5.3, 7.1, 9.2, 17.2, 18.1, 18.3 (cut candidate #2)

**Input**: User description: "Sistem battery per floor. Floor 52: 3–5 battery statis sejak awal, tanpa respawn. Floor 51: maks 2 battery aktif, respawn tiap 30 detik. Floor 50: maks 1 battery aktif, respawn tiap 60 detik. Battery muncul di spawn point acak yang kosong dan tidak berada di pandangan player — tidak pernah muncul di depan mata. Spawn point ditentukan manual di level design, sebaiknya di barang elektronik yang masuk akal (radio darurat, smoke detector, remote, senter cadangan di loker). Mengambil battery menghasilkan suara. Saat mati, battery di map kembali ke kondisi awal floor. Kandidat potong #2: respawn → statis."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plenty of Batteries on the First Floor (Priority: P1)

A player on Floor 52 finds a fixed handful of batteries placed around the office from the start, so they learn how batteries work without scarcity pressure.

**Why this priority**: Floor 52 (Fase 3) needs this first, and it is also the fallback mode for every floor if cut #2 happens.

**Independent Test**: Load a static-mode floor and count batteries; pick all up across a session and confirm none come back.

**Acceptance Scenarios**:

1. **Given** Floor 52 loads, **When** the map is inspected, **Then** exactly `batteryCountFloor52` batteries (between 3 and 5) are present at their authored positions.
2. **Given** a battery on a static floor has been picked up, **When** any amount of time passes, **Then** it never reappears unless the floor is reset.

---

### User Story 2 - Scarce Batteries That Come Back (Priority: P1)

A player on Floor 51 or 50 finds that only a few batteries exist at any time; after one is taken, a new one appears somewhere else they are not looking once a timer passes, so each battery is a real decision.

**Why this priority**: This is the difficulty lever for Floors 51 and 50 and a GDD must-have (18.1).

**Independent Test**: On a respawn floor, pick up a battery, wait out the timer while watching the screen, then search for the new battery.

**Acceptance Scenarios**:

1. **Given** Floor 51 loads, **When** the map is inspected, **Then** exactly `batteryMaxActiveFloor51` (2) batteries exist at the floor's authored initial positions.
2. **Given** Floor 51 has 2 active batteries, **When** the player picks one up, **Then** a `batteryRespawnFloor51` (30s) timer starts.
3. **Given** the respawn timer ends, **When** a spawn point is chosen, **Then** it is an empty spawn point, picked at random from the eligible ones, and not visible on the player's screen at that moment.
4. **Given** Floor 50, **When** the same flow runs, **Then** at most `batteryMaxActiveFloor50` (1) battery exists and the timer is `batteryRespawnFloor50` (60s).
5. **Given** active batteries are already at the floor's maximum, **When** time passes, **Then** no timer runs and no new battery appears.

---

### User Story 3 - Batteries Come From Believable Places (Priority: P2)

A player learns that batteries turn up in electronics that would hold them — emergency radios, smoke detectors, remote controls, spare flashlights in desk lockers — while keyboards, dead monitors, printers, and desk phones never hold one, so exploring feels like searching instead of sweeping.

**Why this priority**: Supports narrative and level readability, but US1–US2 already deliver the mechanic.

**Independent Test**: Inspect floor data and in-game props: every battery spawn point sits on a battery-source prop, and no non-source prop ever holds one.

**Acceptance Scenarios**:

1. **Given** any floor, **When** battery spawn points are checked, **Then** each one is attached to a battery-source prop type (emergency radio, smoke detector, remote control, spare flashlight in desk locker).
2. **Given** a non-source electronic prop (keyboard, dead monitor, printer, desk phone), **When** the floor is played, **Then** it never holds a battery and is not interactable.

---

### User Story 4 - Cut Respawn Without Rewriting (Priority: P3)

If the team triggers cut #2, every floor switches to static batteries via configuration only.

**Why this priority**: GDD 18.3 pre-agrees this cut. The switch must exist before it is needed.

**Independent Test**: Set Floor 51 and 50 battery mode to static, play both floors — fixed batteries, no timers, no errors.

**Acceptance Scenarios**:

1. **Given** a floor's battery mode is set to static in configuration, **When** it loads, **Then** it uses its authored static placements and no respawn timer ever runs.

---

### Edge Cases

- The respawn timer ends but every empty spawn point is on screen: no battery spawns yet. The system retries each tick and spawns at the first moment an eligible point exists. The timer does not restart.
- The player picks up a battery but the slot is full (001 FR-008): the pickup is rejected, the battery stays, and it still counts as active, so no timer starts.
- The player picks up two batteries quickly on Floor 51: one timer runs. When it spawns a battery and active is still below max, the timer restarts for the next one.
- A floor reset during a running timer: the timer is cancelled and batteries return to the floor's initial state (GDD 9.2).
- Pause: respawn timers freeze (spec 012).
- A battery pickup noise alerts the monster (spec 002): intended (GDD 1.3 "Mengambil battery menghasilkan suara").

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Each floor MUST have a battery mode from configuration: static or respawn.
- **FR-002**: Static mode MUST place exactly the floor's configured count of batteries at authored positions at floor load, with no respawn. For Floor 52, `batteryCountFloor52` MUST be between 3 and 5 inclusive, and the number of authored static positions MUST equal it (validated in tests).
- **FR-003**: Respawn mode MUST start the floor with exactly the floor's max-active count of batteries at authored initial positions.
- **FR-004**: In respawn mode, while active batteries in the world are fewer than the floor max, one respawn timer of the floor's duration MUST run. When it ends, one battery MUST spawn; if active is still below max, the timer MUST restart.
- **FR-005**: A respawned battery MUST appear only at a spawn point that (a) holds no battery and (b) is not inside the player's visible screen area plus `batterySpawnOffscreenMargin` at that moment. The point MUST be picked at random from all eligible points. If none is eligible, spawning MUST wait until one is.
- **FR-006**: A battery MUST never visibly pop into existence on screen (GDD 3.1).
- **FR-007**: Per-floor values MUST come from the single configuration source: `batteryCountFloor52` (3–5), `batteryMaxActiveFloor51` (2), `batteryRespawnFloor51` (30s), `batteryMaxActiveFloor50` (1), `batteryRespawnFloor50` (60s), plus new keys `batteryModeFloorNN` and `batterySpawnOffscreenMargin`.
- **FR-008**: Every battery spawn point MUST be attached to a battery-source prop type (emergency radio, smoke detector, remote control, spare flashlight in desk locker). Non-source electronics (keyboard, dead monitor, printer, desk phone) MUST never hold a battery and MUST NOT be interactable (GDD 5.3).
- **FR-009**: Picking up a battery MUST emit an interaction noise pulse (spec 002).
- **FR-010**: On floor reset, all world batteries and respawn timers MUST return to the floor's initial state (GDD 9.2).
- **FR-011**: Pickup, carry, install, and slot rules MUST remain exactly as in 001. This spec only controls where and when batteries exist in the world. Batteries cannot be dropped or placed back (GDD 5.1).
- **FR-012**: Switching a floor from respawn to static MUST need only configuration and, if absent, authored static positions — no code change (GDD 18.3 cut #2).

### Key Entities

- **Battery Spawn Point**: Authored position with a battery-source prop type, and whether it is an initial or static placement.
- **World Battery**: A battery currently lying at a spawn point.
- **Respawn Timer**: One per floor, running only while active < max.
- **Battery Floor Profile**: Mode, count/max, respawn duration.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Across 50 automated respawn events, 0 batteries spawn inside the player's visible screen area.
- **SC-002**: Active battery count never exceeds the floor max in automated tests covering rapid pickups, rejected pickups, resets, and pause.
- **SC-003**: 5 of 5 playtesters, asked afterward, did not see a battery appear out of nowhere.
- **SC-004**: Switching Floor 51 to static mode requires changes in configuration and level data only (verified by doing it once).

## Assumptions

- "Tidak berada di dalam radius pandang player" (GDD 3.1) is interpreted strictly as "not anywhere on the player's screen", not just outside the flashlight circle. This is the safest reading of "never appear in front of the player's eyes".
- Initial battery positions on respawn floors are fixed like all other layout (GDD 16.1). Only respawn locations are random.
- Battery-source props are placed in floor data (spec 004) and drawn in spec 015. Placeholders are fine here.
- Picking up from a source prop uses the same "Pick up" interaction as 001. There is no separate "search" action (GDD 18.2 cuts elaborate interaction systems).

## Dependencies

- **Requires**: 001 (battery pickup rules), 002 (noise pulse), 004 (spawn points in floor data, floor profiles).
- **Consumed by**: 007 (reset), 008 (Floor 52 static), 009/010 (respawn), 016 (battery-source props as storytelling).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3.1, 5.3, 17.2
