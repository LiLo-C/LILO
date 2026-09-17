# Feature Specification: Floor 50 — Mastery Floor & Final Door

**Feature Branch**: `feat/010-floor-50-mastery`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 4 — Floor 51 & 50 (GDD 19.2) · **Proposed owner**: Fathia (level design) with Radit (monster tuning)

**GDD Sources**: Ch. 1.4, 3 (Floor 50 column), 3.1, 6.2, 8.1, 8.2, 16.1–16.3, 17.2, 17.4, 17.5, 18.3 (cut candidate #3)

**Input**: User description: "Floor 50 (Hard, peran: Mastery). Monster aktif & agresif (profil Floor 50: patrol lebih cepat, investigate & chase lebih lama). Objective: cari key untuk 3 pintu terkunci, lalu buka Final Door yang jelas berbeda dari pintu turun biasa. Battery maks 1 aktif, respawn tiap 60 detik. Layout banyak dead-end dan rute sempit; hiding spot lebih jarang. Kandidat potong #3: kalau Floor 50 dipotong, game jadi 2 floor dan ending tetap jalan."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Three Locks Under Pressure (Priority: P1)

A player who reaches Floor 50 must find keys and get through three locked doors while a faster, more persistent monster hunts them, with batteries scarcer than ever.

**Why this priority**: The final test of every system, and the path to the Good Ending.

**Independent Test**: Start directly on Floor 50 (debug floor select) and play to the Final Door.

**Acceptance Scenarios**:

1. **Given** Floor 50 loads, **When** it is inspected, **Then** there are 3 locked doors, 3 keys, 1 Final Door, 1 monster in PATROL, and 1 battery.
2. **Given** the monster on Floor 50, **When** its behavior is timed, **Then** it matches the Floor 50 profile (patrol 1.2×, chase 1.5×, investigate 6s, chase hold 5s, search 8s).
3. **Given** the player has opened all 3 locked doors, **When** they reach and use the Final Door, **Then** the run completes and the Good Ending starts (spec 011).

---

### User Story 2 - The Final Door Looks Final (Priority: P1)

A player who sees the Final Door recognizes immediately that it is not just another way down.

**Why this priority**: GDD 8.2 requires it to be clearly distinct. It is the only signal of the end.

**Independent Test**: Show a screenshot of the Final Door and an exit door to someone who hasn't played and ask which is the way out of the building.

**Acceptance Scenarios**:

1. **Given** the Final Door and any Floor 51/52 exit door, **When** compared, **Then** they are visually distinct in color and shape or detail, beyond what separates exit doors from regular doors.

---

### User Story 3 - Cut Floor 50 and Still Finish (Priority: P3)

If the team triggers cut #3, the game becomes two floors and Floor 51's exit acts as the Final Door, so both endings still work.

**Why this priority**: GDD 18.3 pre-agreed fallback. It must be tested before it is needed.

**Independent Test**: Set `floorCount = 2` and play a full run.

**Acceptance Scenarios**:

1. **Given** `floorCount = 2`, **When** the player uses Floor 51's exit, **Then** the run completes and the Good Ending plays.

---

### Edge Cases

- "Many dead ends" (GDD 3) vs "no dead-end traps while chased" (GDD 16.3): dead ends on this floor must either be short enough to double back from before a chase-speed monster closes in, contain a hiding desk, or sit off the main chase paths. Every dead end must be justified in the layout review.
- Only one battery exists and the player has a full spare slot: the battery stays in the world (001 slot rule), and no second battery spawns, because max active is 1.
- A player arrives with 1 life left: no special treatment. The next catch triggers the Bad Ending.
- Keys and doors ordering: with generic keys (spec 005), no pickup order may create a softlock (validated).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Floor 50 MUST be authored in the spec 004 format, drawn on paper and reviewed before being built (GDD 16.1).
- **FR-002**: The layout MUST follow the GDD 16.2 blueprint extended to three locked doors, ending at the Final Door instead of an exit door.
- **FR-003**: The layout MUST have many dead ends and narrow routes, with fewer hiding desks than Floor 51 (GDD 3), while satisfying the dead-end rule in Edge Cases.
- **FR-004**: The floor profile MUST set `monsterActive = true` with Floor 50 monster values, battery mode respawn with max 1 and 60s, and `lockedDoorsFloor50 = 3`.
- **FR-005**: The floor MUST provide at least 3 valid monster spawn presets (spec 002 FR-003).
- **FR-006**: The Final Door MUST be visually distinct from exit doors and regular doors (GDD 8.2) and MUST trigger run completion (spec 004 FR-008, spec 005 FR-008).
- **FR-007**: With `floorCount = 2`, the game MUST treat Floor 51's exit as the Final Door with no layout change (GDD 18.3 cut #3).
- **FR-008**: The floor MUST pass spec 004 automated validation, spec 005's softlock check, and manual GDD 16.3 sign-off.

### Key Entities

- **Floor 50 Definition**: Fixed layout and anchors for this floor.
- **Final Door**: See spec 005.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Median completion time for playtesters who finished Floor 51 is between 4 and 7 minutes (successful attempt), keeping a full run at about 15 minutes (GDD 1.4).
- **SC-002**: At least 2 of 5 such playtesters finish Floor 50 on their first run with lives remaining. This is an initial "hard but fair" target, tuned by configuration.
- **SC-003**: 5 of 5 people shown the Final Door next to an exit door identify the Final Door as the way out.
- **SC-004**: A full run with `floorCount = 2` reaches the Good Ending with no crash in 3 of 3 attempts.
- **SC-005**: ≥60 fps on iPhone 17 during a chase on this floor.

## Assumptions

- Final Door art is spec 015. This spec needs a placeholder that already satisfies SC-003.
- The Final Door's story explanation (how Eddie leaves the building) is an open item owned by spec 011.

## Dependencies

- **Requires**: 002, 004, 005, 006, 007. Optional: 003.
- **Consumed by**: 011 (Good Ending trigger).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3, 8, 18.3
