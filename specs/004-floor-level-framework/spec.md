# Feature Specification: Floor & Level Framework

**Feature Branch**: `feat/004-floor-level-framework`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Prerequisite for Fase 3 — Floor 52 Lengkap (GDD 19.2) · **Proposed owner**: Calzy (architecture) with Fathia (level design)

**GDD Sources**: Ch. 2.3, 2.4, 3, 8.2, 13, 16.1, 16.2, 16.3, 17.5

**Input**: User description: "Framework untuk floor yang fixed (bukan procedural): definisi data per floor (batas map, dinding & furnitur solid, titik masuk/checkpoint, safe area, battery spawn point, patrol waypoint monster, preset spawn monster, kolong meja untuk hiding, key, pintu terkunci, pintu turun lantai dengan warna berbeda, Final Door), urutan run Floor 52 → 51 → 50 dengan hitungan mundur, splash text nomor lantai saat turun, kamera clamp per floor, dan validasi otomatis checklist level design (Bab 16.3) sejauh bisa diotomatisasi. Menggantikan test room hardcoded dari 001."

## Clarifications

- Q: When the player descends to the next floor, do they keep their current installed charge and spare battery, or does every floor start with a fresh 100% battery and an empty spare slot? → A: [NEEDS CLARIFICATION: GDD 9.2 says death resets the battery to 100% with an empty spare, but it does not say what happens on a normal descend. Recommended: every floor starts at 100% with an empty spare, so the first entry and every checkpoint reset are identical. Alternative: carry over, so saving a spare battery pays off — but then a checkpoint reset would be easier than the first entry.]

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Play a Hand-Built Floor (Priority: P1)

A player spawns at a floor's entry point inside a real multi-room office layout — walls, corridors, furniture — and can explore it with the camera never showing space outside the level, so floors can be built as content instead of code.

**Why this priority**: Every floor spec (008–010) builds on this. Without a loadable floor, nothing past the Phase 1 test room exists.

**Independent Test**: Load a small sample floor made only of data (a few rooms, doors, furniture) and walk every corridor.

**Acceptance Scenarios**:

1. **Given** a floor definition, **When** the floor loads, **Then** walls and solid furniture block the player, the player stands at the entry point, and the light and battery systems from 001 work unchanged.
2. **Given** the player walks to any edge of the floor, **When** the camera would show outside the level, **Then** the camera stops at the floor's bounds.
3. **Given** the same floor is loaded twice, **When** layout, keys, doors, batteries, and hiding desks are compared, **Then** they are identical; only the monster's starting preset may differ (GDD 16.1).
4. **Given** a regular (unlocked) door in the floor, **When** the player uses the action button next to it, **Then** it opens as in 001.

---

### User Story 2 - Descend to the Next Floor (Priority: P1)

A player who reaches the floor's exit door — colored differently from ordinary doors — goes down to the next floor, sees a short splash with the new floor number, and starts at that floor's entry point, which becomes their checkpoint.

**Why this priority**: The run structure (52 → 51 → 50) is the backbone of progression and is needed for the full-run DoD in Fase 4.

**Independent Test**: Chain three sample floors and walk through each exit door.

**Acceptance Scenarios**:

1. **Given** the player is on Floor 52 at the exit door, **When** they use the action button, **Then** the screen transitions, "Floor 51" splash text shows for `floorSplashDuration`, and the player is at Floor 51's entry point.
2. **Given** the player looks at doors on any floor, **When** they compare the exit door to ordinary doors, **Then** the exit door is clearly a different color, and it is the only visual marker of the way down (no objective tracker, no quest log).
3. **Given** the player arrives on a new floor, **When** the arrival completes, **Then** that floor's entry point is recorded as the current checkpoint.
4. **Given** the exit door on the last floor (`floorCount`), **When** the level is inspected, **Then** it is the Final Door, visually distinct from exit doors, and using it ends the run (spec 011).

---

### User Story 3 - Floor Data Carries All Gameplay Anchors (Priority: P1)

A level designer can place every gameplay anchor the GDD blueprint needs — safe area, battery spawn points, patrol waypoints, monster spawn presets, hiding desks, keys, locked doors, exit door, Final Door — in the floor data, and other systems read them from there.

**Why this priority**: Specs 002, 003, 005, and 006 all read their positions from floor data. Defining one format prevents each system inventing its own.

**Independent Test**: Author a sample floor with one of each anchor and check that each system finds its anchors.

**Acceptance Scenarios**:

1. **Given** a floor definition with every anchor type, **When** it loads, **Then** each anchor is available to its consuming system with the authored position and properties.
2. **Given** a floor profile, **When** it loads, **Then** per-floor settings (monster active, battery mode and counts, locked-door count) come from configuration keyed by floor number.

---

### User Story 4 - Automated Level Validation (Priority: P2)

A level designer runs the test suite and gets an automatic report of every Chapter 16.3 checklist rule that can be checked by a machine, so broken floors are caught before playtest.

**Why this priority**: Prevents softlocks (second in the GDD bug priority list). Subjective checks still need a human (floor specs).

**Independent Test**: Deliberately break a sample floor (unreachable key, exit behind a wall, all batteries in one corner) and confirm the tests fail with clear messages.

**Acceptance Scenarios**:

1. **Given** a floor where the exit is unreachable from the entry, **When** validation runs, **Then** it fails naming the unreachable anchor.
2. **Given** a floor whose battery spawn points all sit in one quadrant of the map, **When** validation runs, **Then** it fails on distribution.
3. **Given** a valid floor, **When** validation runs, **Then** all automated checks pass.

---

### Edge Cases

- The player reaches the exit door while the monster is touching them: once the descend transition has started, catches are ignored (matches spec 002).
- The player opens the exit door with a spare battery in hand: see Clarifications for battery carry-over.
- A floor with `floorCount = 2` (GDD 18.3 cut #3): Floor 51 is the last floor, so its exit door must act as the Final Door without a new layout.
- The camera near a map corner with a very small floor: clamping must still center the player as much as bounds allow and never show outside the level.
- Pausing during the splash text: the splash resumes after unpause.

## Requirements *(mandatory)*

### Functional Requirements

**Floor data**

- **FR-001**: Each floor MUST be defined as fixed, hand-authored data. No procedural generation (GDD 16.1).
- **FR-002**: A floor definition MUST support: floor number, map bounds, wall and solid-furniture collision shapes, regular doors, entry point (checkpoint), safe area region, battery spawn points (each optionally tagged with its battery-source prop type, GDD 5.3), patrol waypoints (ordered loop), monster spawn presets, hiding desks, keys, locked doors, one exit door (or Final Door on the last floor), and decorative props (non-interactive).
- **FR-003**: The only randomness allowed at floor load MUST be the monster's starting preset (GDD 16.1). Battery respawn position randomness belongs to spec 006 during play, not load.
- **FR-004**: Per-floor settings MUST come from the single configuration source, keyed by floor number: `monsterActive`, battery mode and counts (spec 006), locked-door count (`lockedDoorsFloor52/51/50`), `floorCount` (3), and `targetFloorDuration` (300s, GDD 17.5). New keys introduced here: `floorSplashDuration`, `patrolObjectiveProximity`.

**Run order & transitions**

- **FR-005**: A run MUST progress through floors in descending order, starting at Floor 52, for `floorCount` floors (52 → 51 → 50). Adding a fourth floor is out of scope (GDD 1.4, 18.2).
- **FR-006**: Using the exit door MUST start a descend transition that loads the next floor, shows splash text "Floor NN" for `floorSplashDuration`, places the player at the new entry point, and records that entry as the checkpoint.
- **FR-007**: The exit door MUST be visually distinct from regular doors by color. The Final Door MUST be visually distinct from exit doors. No other objective marker, tracker, or quest log may exist (GDD 8.2).
- **FR-008**: The last floor's exit MUST be a Final Door whose use raises a single "run completed" outcome for spec 011. With `floorCount = 2`, Floor 51's exit door MUST act as the Final Door.
- **FR-009**: Player battery state on descend MUST follow the Clarifications decision.

**Camera**

- **FR-010**: The camera MUST clamp to each floor's own bounds so no area outside the level is ever visible (GDD 13, 16.3), keeping 001's orthographic, fixed-zoom framing.

**Validation**

- **FR-011**: Automated tests MUST validate every floor definition for: all anchors inside bounds and not inside solid collision; entry → exit reachable; entry → every key, locked door, battery spawn point, and hiding desk reachable; at least two distinct routes between entry and exit that do not share every corridor (GDD 16.3 "minimal satu rute alternatif"); battery spawn points spread across at least 3 of 4 map quadrants (only when the floor has ≥4 spawn points); patrol route passes within `patrolObjectiveProximity` of each key and the exit door without any waypoint placed on them; camera clamping never shows outside the bounds at any reachable player position.
- **FR-012**: Checklist items that cannot be automated (no dead-end trap during a chase, hiding desks placed where an office would have them, ±5 minute completion) MUST be listed in each floor spec's quickstart as manual sign-off items.

**Development aids**

- **FR-013**: Debug builds MUST offer a floor selector to start directly on any floor. Release builds MUST NOT expose it (GDD 19.2 Fase 5 DoD: endings reachable "bukan lewat debug menu").

### Key Entities

- **Floor Definition**: Fixed layout data for one floor plus its anchor lists (FR-002).
- **Floor Profile**: Per-floor configuration values (FR-004).
- **Run Progress**: Current floor number and current checkpoint. Lives are added in spec 007.
- **Door**: Regular, locked (spec 005), exit, or Final. Each type has a distinct visual.
- **Anchor**: Typed, positioned element in floor data consumed by one system.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new floor layout can be added or changed by editing floor data only, with no gameplay-code change (verified by adding a sample floor).
- **SC-002**: A player can go 52 → 51 → 50 → Final Door across three placeholder floors without a crash or softlock in 5 of 5 runs.
- **SC-003**: The largest floor holds ≥60 fps on iPhone 17 while walking its longest corridor.
- **SC-004**: Floor-to-floor transition, including splash text, completes in under 4 seconds.
- **SC-005**: Automated validation catches 100% of a prepared set of deliberately broken sample floors (unreachable exit, unreachable key, anchor inside a wall, clustered batteries, camera seeing outside bounds).

## Assumptions

- Regular (unlocked) doors, once opened, stay open until a floor reset. The monster's navigation passes through regular doors (open or closed) but never through locked doors or the Final Door. If playtests show the monster "walking through" closed doors looks wrong, spec 015 can add an opening animation — behavior stays the same.
- The authoring tool is whatever is simplest for the team (e.g., Unity scenes/Tilemap with marker GameObjects, ScriptableObject data assets, or plain data files). The plan picks one. The GDD rule that every floor is drawn on paper first is a process step owned by the floor specs.
- The safe area is a design region (no patrol waypoints or monster spawn presets inside it). Validation checks that, but nothing prevents a chasing monster from following the player into it.
- Splash text is plain "Floor NN". Final typography comes from spec 015.

## Dependencies

- **Requires**: 001-core-prototype.
- **Consumed by**: 002 (patrol, spawn presets), 003 (hiding desks), 005 (keys, locked doors), 006 (battery spawn points), 007 (checkpoint), 008–010 (floor content), 011 (run completed), 012 (pause during transition).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 2.4, 3, 8.2, 16
