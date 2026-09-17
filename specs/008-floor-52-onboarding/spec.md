# Feature Specification: Floor 52 — Onboarding Floor

**Feature Branch**: `feat/008-floor-52-onboarding`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 3 — Floor 52 Lengkap (GDD 19.2) · **Proposed owner**: Fathia (level design) with Eileen (environment art, later)

**GDD Sources**: Ch. 1.4, 2.1, 2.2, 3 (Floor 52 column), 6.3, 8.1, 15.4, 16.1–16.3, 17.2, 17.5

**Input**: User description: "Floor 52 (Easy, peran: Learn) lengkap dari start sampai pintu turun. Tanpa monster — hanya SFX monster dari kejauhan sebagai hint. Objective: survive & navigate, tanpa key. 3–5 battery statis. Lorong lebar, sedikit percabangan. Floor 52 adalah tutorialnya lewat level design, tanpa popup tutorial: awal ruangan aman (gerak + senter) → battery pertama saat senter mulai Flickering → SFX monster dari jauh → kolong meja di jalur wajib (hiding) → lorong panjang yang mendorong lari (noise & sprint) → pintu turun. DoD: orang yang belum pernah main bisa menyelesaikan Floor 52 tanpa dijelaskan apa pun, ±5 menit."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Learn by Walking Through It (Priority: P1)

A first-time player starts in a safe, quiet room, finds their way through wide corridors, and reaches the exit door in about five minutes having learned to move, watch their light, and collect a battery without reading a single tutorial message.

**Why this priority**: This is the Fase 3 Definition of Done, and the floor every player sees first.

**Independent Test**: Give the build to someone who has never played, say nothing, and time them from spawn to the exit door.

**Acceptance Scenarios**:

1. **Given** a new player spawns on Floor 52, **When** they look around, **Then** they are in a safe room with no threat, a full light, and at least one visible way out.
2. **Given** the player follows the main route at a normal pace, **When** their light first reaches the Flickering state (~126s at default settings), **Then** they are at or near the area where the first battery is placed.
3. **Given** the player reaches the exit door (distinct color), **When** they use it, **Then** they descend to Floor 51 (spec 004).

---

### User Story 2 - Something Else Is in the Building (Priority: P1)

Partway through the floor, the player hears the monster far away, although it never appears on this floor, so they learn to fear sound before they ever meet the monster.

**Why this priority**: Sets up the core threat and the audio-driven pillar (GDD 1.3) without punishing a beginner.

**Independent Test**: Walk the main route and note where distant monster sounds play; confirm no monster entity exists.

**Acceptance Scenarios**:

1. **Given** the player passes the authored trigger points after the battery segment, **When** each trigger fires, **Then** a distant monster sound plays once.
2. **Given** Floor 52 at any time, **When** the game state is inspected, **Then** no monster exists, nothing can catch the player, and no lives can be lost.

---

### User Story 3 - A Desk on the Path, a Corridor to Run (Priority: P2)

Before the end, the player's route passes a hiding desk they cannot miss, then opens into a long corridor that tempts them to sprint, so both hiding and sprinting are introduced before Floor 51 makes them matter.

**Why this priority**: Completes the GDD 15.4 teaching order. Hiding is cut candidate #1, so this beat must degrade cleanly.

**Independent Test**: Walk the route and confirm the desk is on the mandatory path and the long corridor comes near the end.

**Acceptance Scenarios**:

1. **Given** the main route, **When** a player walks it, **Then** they pass within interaction range of a hiding desk that sits on a path they must take.
2. **Given** the final segment before the exit, **When** the player enters it, **Then** it is a long, straight-ish corridor noticeably longer than any earlier one.
3. **Given** `hidingEnabled = false` (cut #1), **When** the floor is played, **Then** the desk is ordinary furniture and the floor is still completable.

---

### Edge Cases

- A player sprints everywhere and reaches the first battery area before the light flickers: fine. The battery is static and still there when needed.
- A player explores every corner slowly: with 3–5 batteries they still cannot run out completely before the exit, and reaching Compact Darkness never blocks progress.
- A player walks backward toward the spawn room: distant sound triggers fire at most once each per floor attempt.
- The player picks up every battery: extra pickups are rejected by the slot rule (001), which also teaches the slot limit.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Floor 52 MUST be authored in the spec 004 floor format as a fixed layout, and MUST first be drawn on paper and reviewed by the design lead before being built (GDD 16.1).
- **FR-002**: The layout MUST follow the GDD 16.2 blueprint without the locked door and key area: Start/checkpoint → Safe Area → Exploration Zone (battery placements) → Chase-style long corridor → exit door.
- **FR-003**: The layout MUST use wide corridors with few branches (GDD 3), while still providing at least one alternate route (GDD 16.3).
- **FR-004**: The floor profile MUST set `monsterActive = false`, battery mode static with `batteryCountFloor52` between 3 and 5, and `lockedDoorsFloor52 = 0`.
- **FR-005**: Onboarding MUST follow the GDD 15.4 order using only level design, with no tutorial popups or instruction text: (1) safe start room — movement and light; (2) battery segment — first battery reached around the time the light begins Flickering; (3) distant monster sound triggers; (4) hiding desk on a mandatory path; (5) long corridor near the end encouraging sprint.
- **FR-006**: Distant monster sounds MUST be authored trigger points that each play a distant monster sound once per floor attempt. No monster entity may exist on the floor (GDD 3, 6.3).
- **FR-007**: The floor MUST pass all automated validation from spec 004 and a manual sign-off of the GDD 16.3 checklist: alternate route, no dead-end traps, battery placements spread out, hiding desks where an office would have them, ±5 minute completion by a new player, nothing outside the level visible.
- **FR-008**: Battery placements MUST sit on battery-source props (spec 006), preferably including Eddie's own desk locker near the start (GDD 5.3, 10.1).

### Key Entities

- **Floor 52 Definition**: The fixed layout and anchors for this floor.
- **Onboarding Beat**: An authored segment in the teaching order (FR-005).
- **Distant Sound Trigger**: Authored point that plays a distant monster sound once.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 4 of 5 people who have never played complete Floor 52 without any explanation (Fase 3 DoD).
- **SC-002**: Median completion time for those first-time players is between 4 and 6 minutes (GDD 1.4 target ±5 min).
- **SC-003**: At least 4 of 5 first-time players pick up and install a battery on this floor without help.
- **SC-004**: When asked afterward, at least 4 of 5 first-time players say they believed something dangerous was in the building.
- **SC-005**: 0 softlocks, 0 places where the player can see outside the level, across 5 full playthroughs.

## Assumptions

- The distant monster sound uses placeholder audio until spec 013 delivers final assets. The trigger system itself only needs to play a sound.
- Final environment art is spec 015. This spec is complete with placeholder visuals as long as rooms, desks, and doors are readable.
- Floor size is tuned to hit the ±5 minute target. If too short, add distance or reduce batteries — never add a floor (GDD 1.4).

## Dependencies

- **Requires**: 001, 004 (floor framework), 006 (static batteries). Optional: 003 (hiding beat).
- **Soft dependency**: 013 (distant sounds — placeholder acceptable).
- **Enriched later by**: 015 (art), 016 (foreshadowing props).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3, 15.4, 16
