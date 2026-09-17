# Feature Specification: Art, Character Models & Lighting Pass

**Feature Branch**: `feat/015-art-lighting-pass`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 6 — Art & Audio Pass (GDD 19.2) · **Proposed owners**: Fathia (3D models), Eileen (environment art 2D, office tileset), Salwa (UI art), Eca (HUD integration), Calzy (SK3DNode integration)

**GDD Sources**: Ch. 1.3, 4.2, 5.2, 8.2, 11.1, 11.2, 11.3, 12, 12.1, 13, 18.3 (cut candidate #4), 19.4

**Input**: User description: "Ganti semua placeholder dengan art final: model 3D low-poly Eddie + monster (tanpa AI-generated asset), environment 2D kantor (tileset) untuk 3 floor, UI art (HUD, tombol, menu), visual pintu (biasa / turun lantai warna beda / terkunci / Final Door), highlight interactable, props battery-source. Lighting polish: radius vignette 2D dan spotlight 3D harus menyatu dan menyusut bersamaan sesuai light state, flicker hanya di state Flickering, Compact Darkness ±10%. Referensi mood Inside, gaya kamera & karakter Sneaky Sasquatch. Fallback kandidat potong #4: model 3D → sprite 2D minimal 4 arah. DoD: tidak ada lagi placeholder yang terlihat player."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Eddie and the Monster Look Real (Priority: P1)

A player sees a low-poly Eddie whose facing is always obvious, and a monster that is readable when it enters the light, both animated for how they move, and both sitting naturally on the 2D office floor.

**Why this priority**: Characters are what the player watches every second. Facing clarity is why the 3D pipeline was chosen (GDD 11.1).

**Independent Test**: Replace the placeholder capsules with final models in the arena and walk, sprint, hide, and get caught.

**Acceptance Scenarios**:

1. **Given** final Eddie, **When** the player moves in any direction, **Then** Eddie faces the movement direction and plays idle, walk, and sprint animations that match speed.
2. **Given** the final monster, **When** it patrols, investigates, searches, chases, and catches, **Then** each state has a readable animation (walk, look-around, run, catch).
3. **Given** either model moves across the floor, **When** the camera follows, **Then** it never floats, slides, or changes scale relative to the background (same check as 001 FR-013).

---

### User Story 2 - A Dark Office Worth Exploring (Priority: P1)

A player explores office floors drawn as a consistent 2D environment — desks, corridors, meeting rooms, electronics — where doors, hiding desks, keys, and batteries are recognizable at a glance in a small circle of light.

**Why this priority**: Readability of interactables in near-darkness is what makes the floors playable without UI markers.

**Independent Test**: Show screenshots at each light state to someone new and ask them to point out doors, the exit door, a hiding desk, a key, and a battery source.

**Acceptance Scenarios**:

1. **Given** any floor, **When** it renders, **Then** it uses final environment art and no placeholder primitives are visible.
2. **Given** the door types, **When** compared, **Then** regular, locked, exit (distinct color), and Final Door (distinct from exit) are all visually different.
3. **Given** an interactable in range, **When** highlighted, **Then** the highlight is a color effect on the object itself, readable in every light state including Compact Darkness, with no floating icons.
4. **Given** battery-source props and non-source electronics (spec 006), **When** seen, **Then** each prop type is recognizable.

---

### User Story 3 - Light That Belongs to the World (Priority: P1)

A player's light looks like one light: the 2D darkness circle and the light on the 3D characters shrink, flicker, and fade together, and characters cast real shadows.

**Why this priority**: GDD 12.1 warns that a mismatch makes characters look pasted on. It is the central visual risk of the hybrid pipeline.

**Independent Test**: Step through all four light states in the arena with both characters in and near the light circle.

**Acceptance Scenarios**:

1. **Given** any light state, **When** a character stands at the edge of the 2D light circle, **Then** the character's lighting falls off at the same edge.
2. **Given** the light state is Flickering, **When** observed, **Then** flicker happens in both the 2D and 3D light together. In any other state, no flicker happens.
3. **Given** Compact Darkness, **When** observed, **Then** the light circle is about 10% of normal size and characters outside it are dark.
4. **Given** the light hits a character, **When** observed, **Then** the character casts a shadow.

---

### User Story 4 - Menus and HUD Match the Game (Priority: P2)

A player sees a HUD, buttons, and menus drawn in the same style as the game.

**Why this priority**: Required for the "no placeholder visible" DoD, but lower gameplay risk.

**Independent Test**: Audit every screen and HUD state.

**Acceptance Scenarios**:

1. **Given** the title, How To Play, pause, settings screens, and HUD (joystick, action button with each context icon or label, battery bar and spare slot, pause button), **When** audited, **Then** each uses final UI art.

---

### User Story 5 - Sprite Fallback Ready (Priority: P3)

If 3D proves too expensive (cut #4), the team can switch characters to 2D sprites with the minimum number of directions without changing gameplay.

**Why this priority**: Pre-agreed fallback (GDD 11.2, 18.3). Only needed if performance fails.

**Independent Test**: Switch `characterRenderMode` to sprite and play a floor.

**Acceptance Scenarios**:

1. **Given** `characterRenderMode = sprite`, **When** the game runs, **Then** Eddie and the monster render as 2D sprites with at least 4 facing directions (or 1 sprite with rotate/flip), and all gameplay behaves identically.

---

### Edge Cases

- A 2D prop in front of a 3D character (desk, wall top): the prop covers the character correctly (001 FR-014) with final art on every floor.
- A character at the map edge with the clamped camera: model alignment still holds.
- Highlight on an object inside Compact Darkness: must still be visible enough to find, without lighting up the surroundings.
- The monster outside the light circle: it must not be visible at all (GDD 1.3 Limited Vision), including its shadow.

## Requirements *(mandatory)*

### Functional Requirements

**Characters**

- **FR-001**: Final Eddie and monster models MUST be low-poly 3D models, hand-made by the team, with no AI-generated assets (GDD 11.3, 19.4).
- **FR-002**: Eddie MUST have idle, walk, sprint, hide enter/exit (if hiding is not cut), and caught animations. The monster MUST have patrol walk, investigate/search look-around, chase run, and catch animations.
- **FR-003**: Character facing MUST be readable from the fixed camera angle in every direction.
- **FR-004**: A config switch `characterRenderMode` MUST allow a 2D sprite fallback with at least 4 directions or a single rotate/flip sprite, with no gameplay change (GDD 11.2, 18.3 cut #4).

**Environment & objects**

- **FR-005**: Every floor MUST use final 2D office environment art (tileset or equivalent) with no visible placeholder.
- **FR-006**: Doors MUST have four distinct looks: regular, locked, exit (different color from regular), and Final Door (clearly distinct from exit) (GDD 8.2).
- **FR-007**: The interactable highlight MUST be a color effect on the object, readable in all light states, with no floating prompt icons (GDD 4.2).
- **FR-008**: Battery-source props (emergency radio, smoke detector, remote control, spare flashlight in desk locker) and non-source electronics (keyboard, dead monitor, printer, desk phone) MUST each have recognizable art (GDD 5.3).

**Lighting**

- **FR-009**: The 2D darkness mask radius and the 3D character light range MUST match in every light state and shrink together (GDD 12.1).
- **FR-010**: Flicker MUST appear only in the Flickering state, in both 2D and 3D lighting together (GDD 12.1).
- **FR-011**: Compact Darkness MUST shrink the light to about `compactDarknessRadiusFraction` (10%) of normal (GDD 12.1).
- **FR-012**: 3D characters MUST receive real lighting and cast shadows from the player's light (GDD 12).
- **FR-013**: Nothing outside the light circle (including the monster and its shadow) may be visible, apart from interactable highlights (FR-007).

**UI**

- **FR-014**: The HUD, action button contexts, pause button, and all menus MUST use final UI art.

**Process & performance**

- **FR-015**: Every visual asset MUST have a recorded creator (team member) to support the no-AI-asset rule.
- **FR-016**: With all final assets, gameplay MUST hold ≥60 fps on iPhone 17 on every floor.

### Key Entities

- **Character Model**: Eddie or monster, with animation set and render mode.
- **Environment Tileset**: Office floor, wall, and furniture art shared by floors.
- **Door Visual Type**: Regular, locked, exit, Final.
- **Light Profile**: Radius and flicker values per light state, applied to both 2D and 3D.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A screen-by-screen audit of a full run finds 0 placeholder visuals (Fase 6 DoD).
- **SC-002**: ≥60 fps sustained on iPhone 17 on the heaviest floor during a chase with final assets.
- **SC-003**: 5 of 5 new viewers correctly identify Eddie's facing direction from 8 screenshots of different directions.
- **SC-004**: 4 of 5 new viewers correctly point out the exit door, a hiding desk, a key, and a battery source in in-game screenshots at Normal light.
- **SC-005**: In a side-by-side review, 0 of 3 reviewers say the characters look "pasted on" the background in any light state.

## Assumptions

- The art direction references are Playdead's Inside (dark, vignette, single light source) for mood and Sneaky Sasquatch for camera and character style (GDD 11.1).
- The three floors may share one tileset with small variations. Distinct per-floor art is nice to have, not required.
- The death sequence visuals (spec 007) and comic art (spec 011) are covered by their own specs, but they must match this spec's style.

## Dependencies

- **Requires**: 001 (lighting pipeline), 002 (monster states), 004 (floors), 005 (door types), 006 (prop types), 012 (screens).
- **Enables**: 016 (story props drawn in the same style), 017 (placeholder-free build).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 11, 12
