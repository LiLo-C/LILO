# Feature Specification: Floor 51 — Pressure Floor

**Feature Branch**: `feat/009-floor-51-pressure`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 4 — Floor 51 & 50 (GDD 19.2) · **Proposed owner**: Fathia (level design) with Radit (monster tuning)

**GDD Sources**: Ch. 1.4, 2.2, 3 (Floor 51 column), 3.1, 6.2, 6.3, 8.1, 16.1–16.3, 17.2, 17.4, 17.5

**Input**: User description: "Floor 51 (Medium, peran: Pressure). Monster aktif: patrol + investigate + chase normal (profil Floor 51). Objective: cari 1 key untuk membuka 1 pintu turun terkunci. Battery maks 2 aktif, respawn tiap 30 detik. Layout banyak persimpangan, ada hiding spot. Ikuti blueprint Bab 16.2 dan checklist 16.3. Target ±5 menit."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First Real Encounter (Priority: P1)

A player arrives on Floor 51 in a safe area, then explores an office full of intersections while the monster patrols. They must find the one key, unlock the way down, and use sound and hiding to avoid being caught.

**Why this priority**: The first floor where the full loop (explore → hear → hide/evade → objective) exists. Required for the Fase 4 DoD.

**Independent Test**: Start directly on Floor 51 (debug floor select) and play to the exit door.

**Acceptance Scenarios**:

1. **Given** the player arrives on Floor 51, **When** they stand in the safe area, **Then** the monster already exists elsewhere on the floor, is in PATROL, and cannot reach the player within `monsterSpawnSafeSeconds` if the player waits.
2. **Given** the floor, **When** the player explores, **Then** there is exactly one key and exactly one locked door, and the locked door stands between the player and the exit door.
3. **Given** the player unlocks the door and reaches the exit door, **When** they use it, **Then** they descend to Floor 50.

---

### User Story 2 - The Monster Guards the Objective (Priority: P1)

The monster's patrol route passes near the key and the locked door without standing on them, so reaching the objective always involves some risk, but never a guaranteed catch.

**Why this priority**: The core pressure of this floor comes from patrol design, not from new systems.

**Independent Test**: Watch the patrol route with a debug overlay and check its distance to the key and doors.

**Acceptance Scenarios**:

1. **Given** the patrol route, **When** it is checked against objectives, **Then** it passes within `patrolObjectiveProximity` of the key and the locked door, and no waypoint is on either.
2. **Given** any monster spawn preset on this floor, **When** the floor starts, **Then** the preset passes all spec 002 FR-003 validity checks.

---

### User Story 3 - Scarcity Starts (Priority: P2)

The player notices batteries are no longer lying everywhere: only two exist at a time, and a replacement shows up somewhere else later.

**Why this priority**: Difficulty step from Floor 52. The mechanism is spec 006; this story is about placement and tuning on this floor.

**Independent Test**: Count batteries at load, pick one up, and verify respawn behavior on this floor's spawn points.

**Acceptance Scenarios**:

1. **Given** Floor 51 loads, **When** batteries are counted, **Then** there are 2 at the authored initial positions and the floor has more spawn points than that, spread across the map.

---

### Edge Cases

- The monster's random preset puts it on the far side of the floor from the key: still valid. Validation guarantees reachability, not proximity.
- A player finds the exit door before the key: the locked door is in front of it, so "locked" feedback plays (spec 005).
- A chase into a corridor with no exit: not allowed by design — any dead end on this floor must be short enough to leave or contain a hiding desk (GDD 16.3).
- Death on this floor: reset per spec 007. Lives lost here carry to Floor 50.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Floor 51 MUST be authored in the spec 004 format, drawn on paper and reviewed before being built (GDD 16.1).
- **FR-002**: The layout MUST follow the GDD 16.2 blueprint: Start/checkpoint → Safe Area → Exploration Zone (battery spawn points + patrol route) → Locked Door → Key Area → long chase-style corridor → exit door. The key MUST be reachable without passing the locked door.
- **FR-003**: The layout MUST have many intersections (GDD 3) and hiding desks.
- **FR-004**: The floor profile MUST set `monsterActive = true` with the Floor 51 monster values (patrol 1.0×, chase 1.4×, investigate 4s, chase hold 3s, search 6s), battery mode respawn with max 2 and 30s, and `lockedDoorsFloor51 = 1`.
- **FR-005**: The floor MUST provide at least 3 monster spawn presets that pass spec 002 FR-003, so the start position genuinely varies.
- **FR-006**: The patrol route MUST pass near the objective area without any waypoint on the key or doors (GDD 16.3).
- **FR-007**: The floor MUST have more battery spawn points than its max-active count, spread across the map, each on a battery-source prop.
- **FR-008**: The floor MUST pass spec 004 automated validation and manual GDD 16.3 sign-off, including the ±5 minute completion check and "no dead-end trap while chased".

### Key Entities

- **Floor 51 Definition**: Fixed layout and anchors for this floor.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Median completion time for playtesters who have finished Floor 52 is between 4 and 6 minutes (successful attempt only, excluding retries).
- **SC-002**: At least 3 of 5 such playtesters finish Floor 51 without losing all 3 lives. This is an initial tuning target, adjusted by configuration only.
- **SC-003**: At least 4 of 5 playtesters report at least one moment where they heard the monster and changed what they were doing (hid, stopped, or ran).
- **SC-004**: 0 catches within `monsterSpawnSafeSeconds` of arrival across 20 automated floor starts with an idle player.
- **SC-005**: ≥60 fps on iPhone 17 during a chase on this floor.

## Assumptions

- Monster, noise, keys, battery respawn, and reset behavior are defined in specs 002, 005, 006, and 007. This spec only authors and tunes content.
- If hiding is cut (spec 003), hiding desks become furniture and the dead-end rule must hold without them — the layout review must check this case.

## Dependencies

- **Requires**: 002, 004, 005, 006, 007. Optional: 003.
- **Enriched later by**: 013 (audio), 015 (art), 016 (foreshadowing).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3, 8.1, 16
