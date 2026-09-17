# Feature Specification: Environmental Storytelling & Foreshadowing

**Feature Branch**: `feat/016-environmental-storytelling`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 6 — Art & Audio Pass (GDD 19.2, 21) · **Proposed owners**: Fathia (placement), Eileen & Salwa (props), audio owner (narrative sounds)

**GDD Sources**: Ch. 1.1, 5.3, 10.1, 10.3, 14.1, 21

**Input**: User description: "Foreshadowing tersirat yang tidak pernah dijelaskan eksplisit, disebar sebagian di ketiga floor: suara monster menyerupai bel lift kantor, langkah kaki monster menyerupai suara atasan, tertawaan rekan kerja samar dari ruangan kosong, meeting room berisi kursi kosong semua, jam dinding selalu menunjuk waktu yang sama, komputer menyala menampilkan task yang belum selesai, printer mencetak surat resign berulang-ulang, office directory menampilkan nama Eddie yang berubah-ubah. Meja Eddie berisi P3K, makanan darurat, lampu emergency. Jumlah hint yang dipasang tergantung kapasitas illustrator (open item)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - The Office Is Wrong (Priority: P1)

A player exploring the floors notices small, unexplained details — an empty meeting room with every chair empty, a wall clock that always shows the same time, a lit computer showing an unfinished task — that make the office feel like more than a building.

**Why this priority**: Visual foreshadowing is what makes the Good Ending's reveal land (GDD 10.1). It needs only props, not systems.

**Independent Test**: Walk each floor and list the foreshadowing details found.

**Acceptance Scenarios**:

1. **Given** each of the three floors, **When** the floor is explored, **Then** at least one foreshadowing item is present on that floor.
2. **Given** any foreshadowing item, **When** the player approaches, **Then** it is not highlighted, not interactable, and no text explains it.
3. **Given** a repeated item (e.g., the frozen wall clock on two floors), **When** seen again, **Then** it shows the same thing each time, so it reads as consistent.

---

### User Story 2 - Sounds That Remind You of Work (Priority: P2)

A player notices that the monster's call sounds like the office elevator bell, its footsteps sound like a boss walking the floor, and faint coworker laughter comes from empty rooms.

**Why this priority**: Audio foreshadowing ties the monster to Eddie's work life without words. Depends on spec 013 assets.

**Independent Test**: Listen to monster cues and walk past laughter triggers.

**Acceptance Scenarios**:

1. **Given** chosen audio foreshadowing items, **When** they play, **Then** they come from the monster sound design (bell-like call, boss-like footsteps) or from authored room triggers (laughter), and never alert the monster.

---

### User Story 3 - Eddie's Desk (Priority: P2)

A player starting Floor 52 finds Eddie's own desk with a first-aid kit, emergency food, and the empty spot in his locker where the lamp was — matching the prologue — and possibly their first battery nearby.

**Why this priority**: Anchors gameplay to the prologue and to Eddie's "always prepared" character (GDD 5.3, 10.1).

**Independent Test**: Look around the Floor 52 start area.

**Acceptance Scenarios**:

1. **Given** Floor 52's start area, **When** the player looks around, **Then** Eddie's desk with first-aid kit, emergency food, and open locker is visible.

---

### Edge Cases

- The printer printing resignation letters repeatedly: it is animated but makes no gameplay noise (it never counts as player noise) and its sound, if any, is ambience-level.
- The office directory with Eddie's name changing: the name change happens only while the directory is off-screen or as a subtle loop, so it is never a text explanation.
- Foreshadowing props placed near a hiding desk or objective: they must not be confused with interactables (no highlight, not on the key or battery-source prop list).
- Hint capacity shortfall (GDD 21): fewer items is acceptable as long as SC-001's minimums hold.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The team MUST choose a subset of the eight GDD 10.3 items and record the choice and floor placement in this spec's plan. Choosing all eight is not required.
- **FR-002**: Every floor MUST contain at least one chosen item, and at least four distinct items MUST appear across the run.
- **FR-003**: Foreshadowing items MUST NOT be interactable, highlighted, or explained by any text, UI, or tutorial (GDD 10.3).
- **FR-004**: Visual items (empty meeting room, frozen clock, unfinished-task computer, resignation-letter printer, changing office directory) MUST be decorative props or simple looping animations placed in floor data (spec 004).
- **FR-005**: Audio items (elevator-bell-like monster call, boss-like monster footsteps, faint laughter) MUST be implemented through spec 013. Laughter uses authored triggers that play once per floor attempt.
- **FR-006**: Floor 52's start area MUST include Eddie's desk with first-aid kit, emergency food, and an open locker (GDD 5.3, 10.1).
- **FR-007**: Foreshadowing props MUST NOT create or affect player noise, monster detection, collision routes required by level validation, or performance (≥60 fps kept).
- **FR-008**: Battery-source prop placement (spec 006) SHOULD support the story, e.g., Eddie's spare flashlight in a desk locker.
- **FR-009**: All props MUST follow the no-AI-asset rule (GDD 11.3).

### Key Entities

- **Foreshadowing Item**: One of the GDD 10.3 list, type (visual or audio), floor placements.
- **Eddie's Desk**: Fixed narrative set piece on Floor 52.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Each floor has ≥1 item and the run has ≥4 distinct items (audit).
- **SC-002**: After a full run, at least 3 of 5 playtesters mention at least one foreshadowing detail unprompted when asked "what did you notice about the office?"
- **SC-003**: 0 playtesters try to interact with a foreshadowing prop more than once (they are not mistaken for interactables).
- **SC-004**: The team records how many hints shipped against illustrator capacity, closing GDD 21's open item.

## Assumptions

- Final prop art and sound come from specs 015 and 013. Placement can happen with placeholders first.
- Localization is out of scope. Any visible in-world text (a task title on a screen, a name on a directory) is short English text as art, not UI.

## Dependencies

- **Requires**: 004 (prop placement), 008–010 (floors), 013 (narrative sounds), 015 (art style).
- **Related**: 011 (Good Ending pays off these hints).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 10.3
