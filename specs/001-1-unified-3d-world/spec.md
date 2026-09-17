# Feature Specification: LILO Phase 1.1 — Unified 3D World & Feel Pass

**Feature Branch**: `001-1-unified-3d-world`

**Created**: 2026-09-17

**Status**: Draft

**Amends**: [[001-core-prototype/spec|001 — Core Movement & Light System Prototype]]

**Input**: User description: "The current v2 build looks dumb compared to the LILO spike (~/Documents/Apple/LILO). Switch to the LILO approach — render the whole world in one 3D scene so the flashlight really lights the room and walls cast shadows — and write it as 001-1. Write all the spec requirements first."

## Why This Spec Exists

The 001 build meets its checklist but does not read as a horror game. A side-by-side review against
the LILO spike found five causes:

1. **No visible darkness.** In the Normal state the lit area is larger than the landscape screen, so the whole room is bright.
2. **The flashlight lights nothing.** The light exists only around the character. The floor, walls and furniture are flat images it cannot reach, so it casts no shadows.
3. **Mismatched viewpoints.** The room is seen from above, but the character is seen from the front. The character looks pasted on the floor plan.
4. **Harsh light-state changes.**
   - Flicker is per-frame noise.
   - Critical jumps to a smaller radius instantly.
   - The darkness edge is hard.
5. **An empty, bright world.** Walls are thin outlines, the floor is mid-grey, and the desk has no collision.

Causes 2 and 3 come from the render architecture in GDD Ch. 11.1 and 12: a 2D environment with 3D characters composited on top. A 3D light in that setup cannot light 2D surfaces. The LILO spike renders everything in one 3D scene and does not have these problems.

This spec changes the render approach and sets the look-and-feel bar Phase 1 must meet. **The gameplay rules of 001 are not changed.**

## Relationship to 001

| 001 item | Status under 001-1 |
|---|---|
| FR-001–FR-012, FR-015–FR-020 | **Kept as-is.** Gameplay, controls, battery, interaction and camera rules are unchanged except where FR-013 and FR-014 below add to them. |
| FR-013 (character grounding across two renderers) | **Superseded** by FR-009 below. |
| FR-014 (2D element occludes 3D character) | **Superseded** by FR-010 below. |
| FR-021 (3D characters darken with the 2D environment) | **Superseded** by FR-008 below. |
| SC-002 (60 fps, 2 characters) | **Superseded** by SC-1.1-002 below (adds shadows). |
| FR-018 (color highlight on interactable objects) | **Extended** by FR-013 below: the highlight is drawn around the object and also marks it outside the lit area. |
| SC-001, SC-003–SC-008 | **Kept as-is in 001.** They retain their 001 identifiers and remain authoritative for the original gameplay checks. |
| SC-009 (no brightly lit character in a dark screen) | **Kept in 001.** The related 001-1 visual check is SC-1.1-005 below. |
| New 001-1 outcomes | Use the `SC-1.1-###` namespace below to avoid colliding with 001's criteria. |
| Assumption "environment in 2D, character in 3D" | **Replaced** by the render decision in Assumptions below. |

## Clarifications

### Session 2026-09-17

- Q: Should the camera lead the player in the direction they are moving (LILO spike look-ahead), or keep the player exactly centered as GDD Ch. 13 states? → A: Lock the camera on the player. No look-ahead; GDD Ch. 13 "selalu di tengah layar" stands. See FR-012.
- Q: Outside the lit area, should batteries and the door glow as navigation beacons, or stay as dark as the rest of the room? → A: Neither. The object itself is not self-lit; a highlight drawn around the object marks it. See FR-013, FR-020.
- Q: Should the joystick float (appear where the thumb first touches in the left control zone) or stay at a fixed position? → A: Floating. See FR-019.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - The Flashlight Lights the Room (Priority: P1)

A player walks through the test room and sees their flashlight light the floor, walls, desk,
batteries, door, their own character and the monster figure, all from one light. Walls and
furniture throw shadows that swing as the player moves. Beyond the light the room falls into darkness.

**Why this priority**: This is the core of "Lights In, Lights Out". Without it the 001 build reads as a lit floor plan, not a horror game. Every later spec (monster reveal, hiding, level design) depends on light and shadow meaning something.

**Independent Test**: Launch the test room at full charge. Walk the player past the desk and along a wall. Confirm that the light lands on the surfaces, that shadows move with the player, and that darkness is visible on every screen edge.

**Acceptance Scenarios**:

1. **Given** the flashlight is at full charge, **When** the player stands still, **Then** the floor, nearby walls, nearby furniture and the player character are all lit by the same light, and the screen edges are dark.
2. **Given** the player walks past the desk, **When** the desk is between the flashlight and the floor behind it, **Then** a shadow falls behind the desk and moves as the player moves.
3. **Given** the player stands near a wall, **When** the player walks along it, **Then** the space on the far side of the wall stays unlit.
4. **Given** the placeholder monster figure is outside the light, **When** the player walks toward it, **Then** it becomes lit only as the light reaches it, the same way the floor around it does.

---

### User Story 2 - Light States Read as One Light Fading (Priority: P1)

As the battery runs down, the player sees one light. It flickers in short, distinct bursts. It
narrows smoothly into Critical and settles into a small pool of light at Compact Darkness. The
light never jumps or shimmers every frame.

**Why this priority**: 001 US2 is the game's core mechanic, and 001 SC-003 needs people to tell the states apart. The current build fails on feel even though the logic is correct.

**Independent Test**: Start at full charge with a shortened battery duration set in config. Watch the light go through all four states without touching anything.

**Acceptance Scenarios**:

1. **Given** the charge is in the Normal state, **When** observed, **Then** the light is steady, with no flicker and no size change.
2. **Given** the charge enters Flickering, **When** observed, **Then** the light dips in brief, separate flicker events with steady light between them, and never shimmers every frame.
3. **Given** the charge falls through Critical, **When** observed, **Then** the lit area shrinks gradually as charge falls, never in a single jump.
4. **Given** the charge reaches 0%, **When** observed, **Then** the lit area settles smoothly at the Compact Darkness size, and the player can still move and act (001 FR-006).
5. **Given** the player installs a spare battery during Compact Darkness, **When** observed, **Then** the light grows back to its Normal size over a short, visible transition rather than popping.

---

### User Story 3 - The Room Stays Navigable in the Dark (Priority: P2)

A player with a weak or dead flashlight can still make out the room's layout well enough to find their way. Walls and furniture show as faint shapes against the dark, never bright.

**Why this priority**: The LILO spike showed that pure black beyond the light makes players lost, not scared (spike note: "running dry while lost is not interesting"). This is a feel requirement layered on US1.

**Independent Test**: Set charge to 0% and walk from the spawn point to the door.

**Acceptance Scenarios**:

1. **Given** the flashlight is in Compact Darkness, **When** the player looks around, **Then** the walls and the desk are faintly visible as shapes outside the lit area.
2. **Given** the readability fill is set to zero in config, **When** the player looks around, **Then** everything outside the lit area is black. This proves the fill is one tunable, not baked in.
3. **Given** a battery or the door is outside the lit area, **When** the player looks for it, **Then** the object itself stays as dark as its surroundings, but a faint highlight around it shows where it is (FR-013).
4. **Given** the player walks within interaction range of a battery or the door, **When** observed, **Then** the highlight around it becomes clearly stronger than the out-of-range highlight.

---

### User Story 4 - Movement and Controls Feel Physical (Priority: P2)

A player moves the character through a room with solid walls and a solid desk. The character slides along surfaces instead of sticking. The camera frames the world at a fixed angle. Picking up a battery and opening the door each give visible feedback.

**Why this priority**: These are the smaller "juice" gaps from the review. None blocks the prototype on its own, but together they separate "a test" from "a game".

**Independent Test**: Walk diagonally into a wall and into the desk, pick up a battery, and open the door.

**Acceptance Scenarios**:

1. **Given** the player pushes diagonally into a wall, **When** movement continues, **Then** the character slides along the wall in the free direction instead of stopping dead.
2. **Given** the player picks up a battery, **When** the pickup succeeds, **Then** the battery visibly animates out of the world over a short duration instead of vanishing in one frame.
3. **Given** the player opens the door, **When** the action succeeds, **Then** the door visibly moves to its open state over a short duration.
4. **Given** no joystick is shown, **When** the player's thumb touches anywhere inside the left control zone, **Then** the joystick appears centered under that touch and movement follows the drag from that point.
5. **Given** the player lifts their thumb, **When** the touch ends, **Then** the joystick disappears and the character stops.
6. **Given** a touch starts outside the left control zone, **When** the player drags, **Then** no joystick appears and the character does not move.

---

### Edge Cases

- **The player stands in the corner between two walls.** Shadows from both walls must render without flicker or gaps at the corner seam.
- **The flashlight is at Compact Darkness and the player walks behind the desk.** The desk still covers the character from the camera's viewpoint, and the character stays as dark as the floor around it.
- **A frame takes much longer than normal** (for example, the app resumes from background). Movement, light easing and camera follow must not jump, and the character must not pass through a wall.
- **Shadows are turned off in config** (performance fallback). The game must remain fully playable, and all four light states must still be distinguishable.
- **The player stands exactly on a light-state boundary.** 001 FR-005 decides the state. The easing added here must not make the light show a different state than the one in effect.
- **The door is opened while it is in a wall's shadow.** The FR-013 highlight still marks it, and its open state must still be visible.
- **The player drags the joystick thumb out of the left control zone.** The joystick keeps tracking that touch until it ends; the zone only limits where a touch may start.
- **Two thumbs land in the left control zone.** Only the first creates the joystick; the second is ignored until the first ends.
- **The device is in landscape.** The lit-area size is set so darkness stays visible on the short screen axis (height). The LILO spike was portrait, so its values do not carry over directly.

## Requirements *(mandatory)*

### Functional Requirements

**Light and shadow**

- **FR-001**: The flashlight MUST be one light source that lights every visible part of the playable world from the player's position: floor, walls, furniture, batteries, door, the player character and the placeholder monster figure. No object may use a separate light meant to fake the flashlight.
- **FR-002**: Walls and solid furniture MUST block the flashlight and cast shadows onto the floor and other objects. Shadows MUST update every frame as the player moves.
- **FR-003**: The edge of the lit area MUST fall off softly from lit to dark, with no hard binary edge.
- **FR-004**: In every light state, including Normal, the lit area MUST be small enough that darkness is visible along all four screen edges in landscape. The Normal lit radius MUST be a config value.
- **FR-005**: The lit area MUST change size by easing toward the size that the current light state and charge call for (001 FR-005). It MUST NOT jump between sizes. Within Critical it MUST narrow continuously as charge falls from the Critical threshold to 0%. The easing rate MUST be a config value.
- **FR-006**: Flicker MUST appear only in the Flickering state (GDD 12.1). It MUST take the form of discrete flicker events: a brief dip in brightness and/or reach, separated by steady intervals. It MUST NOT produce a new random value every frame. Event duration, interval range and dip depth MUST be config values.
- **FR-007**: Outside the flashlight's reach the world MUST remain faintly readable. Wall and furniture shapes stay distinguishable from the floor, and nothing outside the light may look brighter than the dimmest lit surface. A single config value MUST control this readability fill, and setting it to zero MUST produce pure black outside the light.

**One world, one viewpoint**

- **FR-008**: The player character and the placeholder monster figure MUST be lit, shadowed and darkened by exactly the same light as the environment around them. A character may never be brighter than the floor it stands on. This supersedes 001 FR-021.
- **FR-009**: Characters, furniture and the environment MUST share one world scale and one viewpoint. A character standing on the floor MUST appear grounded, with no floating, sliding against the floor, or scale drift while the camera follows. This supersedes 001 FR-013.
- **FR-010**: Any object nearer to the camera MUST cover characters behind it, based on actual depth rather than manual layer ordering. This supersedes 001 FR-014.
- **FR-011**: Walls and furniture MUST have visible height and thickness as solid shapes, not outlines or flat rectangles. Wall height MUST be a config value, because it trades shadow length against how much of the room is hidden behind walls.
- **FR-012**: The camera MUST keep 001 FR-012's rules: orthographic, north-facing, fixed zoom, smooth follow, and clamped at the room boundary. Tilt angle and zoom MUST be config values, with tilt defaulting to 45° per GDD Ch. 13. The camera MUST stay locked on the player: it follows the player's position only, with no look-ahead or offset in the direction of movement, so the player is at the screen center whenever the camera is not clamped at a room boundary (GDD Ch. 13).

**Readability of important objects**

- **FR-013**: Interactable objects (batteries, door) MUST NOT light themselves or light their surroundings; the object's own surface is lit only by the flashlight and readability fill like everything else. Instead, a highlight MUST be drawn around each interactable object (an outline or halo following its shape, not a floating icon, per GDD 4.2). The highlight MUST have two levels:
  - **Out of range**: faint, visible in every light state including Compact Darkness, so the object can be located in the dark.
  - **In interaction range**: clearly stronger, replacing 001 FR-018's in-range highlight.

  The highlight MUST be unaffected by shadows, so an object in a wall's shadow is still marked. Its color and both intensity levels MUST be config values. The highlight disappears when the object leaves the world (picked-up battery) and stays on an opened door only while the door is still interactable.

**Movement and controls**

- **FR-014**: The player character MUST collide with walls and solid furniture using its body size. It MUST slide along a surface when pushed into it at an angle, rather than stopping entirely. This extends 001 FR-020.
- **FR-015**: Movement, light easing, flicker timing and camera follow MUST be driven by elapsed time, so feel is the same at any frame rate. A single long frame MUST be clamped to a config-defined maximum step, so nothing jumps or passes through geometry.
- **FR-016**: A picked-up battery MUST visibly animate out of the world (for example, grow and fade) over a short config-defined duration.
- **FR-017**: Opening the door MUST visibly animate from closed to open over a short config-defined duration.
- **FR-018**: The HUD (battery bar, spare slot indicator, action button, joystick) MUST draw above the world and MUST NOT be affected by world lighting or darkness.
- **FR-019**: The joystick MUST be floating. It is hidden until a touch begins inside the left control zone, then appears centered on that touch point and reads deflection relative to it. It hides and reports zero input when that touch ends or is cancelled. Touches that begin outside the zone MUST NOT create a joystick, and only one joystick touch is tracked at a time, so the action button can be pressed with the other thumb while moving. The control zone's extent replaces 001's fixed joystick position (GDD 15.3 "posisi di layar") and MUST be a config value; diameter, dead zone, opacity and sprint threshold stay config values as in 001.

**Visual baseline**

- **FR-020**: Placeholder materials MUST use a dark palette. The unlit background MUST be near-black, and the lit floor and walls MUST stay mid-to-low in brightness, so the flashlight is the brightest thing on screen apart from the HUD and the FR-013 highlights.

**Configuration and continuity**

- **FR-021**: Every new tunable introduced by this spec MUST live in the single configuration source defined by 001 FR-015, with no second config source. This covers:
  - lit radius per state, easing rate and flicker parameters
  - readability fill, wall height, camera tilt and zoom
  - interactable highlight color and out-of-range / in-range intensities
  - joystick control zone extent
  - animation durations, maximum frame step
  - shadow on/off and shadow quality
- **FR-022**: Shadow rendering MUST be switchable off, and its quality reducible, from config alone. This is the agreed performance fallback for this render approach and replaces the GDD 11.2 fallback for the environment.
- **FR-023**: The gameplay behaviour of 001 FR-001–FR-012 and FR-015–FR-020 MUST remain unchanged, and the existing automated checks for that logic MUST keep passing. That logic covers:
  - movement and sprint
  - battery drain, light-state thresholds and install gating
  - the pickup slot rules
  - interaction priority and door opening

### Key Entities

- **World Scene**: The single space that holds the floor, walls, furniture, batteries, door, player character and monster figure, all at one scale and lit together. It replaces 001's separate "2D environment" and "3D character" layers.
- **Flashlight Light**: The one light attached to the player. Its reach, brightness and flicker come from the current light state and charge (001 FR-005), eased over time (FR-005, FR-006).
- **Readability Fill**: The dim, shadowless light that keeps room shapes legible outside the flashlight (FR-007). It is one tunable.
- **Light Profile** *(config)*: The lit radius per light state, easing rate and flicker parameters. It is the shape that 015's "Light Profile" later extends with final values.
- **Test Room, Player Character, Battery, Door, Placeholder Monster Figure**: Unchanged from 001 except that each is now a solid object in the World Scene.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-1.1-001**: In screenshots taken in each of the four light states on an iPhone 17 in landscape, darkness is visible along all four screen edges in every screenshot.
- **SC-1.1-002**: The game sustains at least 60 frames per second on a physical iPhone 17 while the player moves continuously through the test room. Shadows are on, and both the player character and the placeholder monster figure are on screen. This supersedes 001 SC-002.
- **SC-1.1-003**: In a side-by-side viewing of the 001 build and the 001-1 build, at least 2 of 3 people outside the development team pick the 001-1 build as "looks and feels more like a horror game", without being told which is newer.
- **SC-1.1-004**: In an observation test with at least 2 people outside the development team, each person describes the Flickering state as the light "flickering" or "failing", not as "shimmering", "glitching" or "noise".
- **SC-1.1-005**: In Compact Darkness, no observer reports a character or object that looks brighter than the floor around it (extends 001 SC-009).
- **SC-1.1-006**: With the flashlight fixed at 0% charge, a person outside the development team can walk from the spawn point to the door within 60 seconds without instructions.
- **SC-1.1-007**: A player walking diagonally into any wall or into the desk keeps moving along it in 100% of attempts across 10 tries, never stopping dead or passing through.
- **SC-1.1-008**: Every new tunable listed in FR-021 can be changed by editing only the single configuration source.
- **SC-1.1-009**: All automated checks that passed on the 001 build still pass after the change.

## Assumptions

- **Render decision.** The whole playable world is rendered as one real-time 3D scene with dynamic lighting and shadows, following the approach proven in the LILO spike (`~/Documents/Apple/LILO`, `Spike3D.swift`). This deliberately amends GDD Ch. 11.1 (hybrid 2D environment + 3D characters) and Ch. 12 (fake 2D vignette plus separate 3D spotlight). The GDD's own goals for those chapters still apply:
  - characters readable in every facing
  - one visually unified light
  - Inside-like single-light mood

  Only the technique changes. Record the change in the GDD's Ch. 21 open items.
- The 2D HUD and on-screen controls remain a flat overlay above the world. Only the playable world changes renderer.
- GDD Ch. 13 camera rules are unchanged; the LILO spike's look-ahead is intentionally not adopted.
- The LILO spike's tuning (portrait, 70° pitch, lit radius 185, wall height 112) is evidence the approach works, not a set of final values. Landscape and the GDD's 45° tilt need fresh on-device tuning.
- Baseline device (iPhone 17), minimum iOS (26) and landscape-only are unchanged from 001.
- Haptics, audio, real monster AI, final art and multiple rooms remain out of scope, exactly as in 001.
- The placeholder primitives (capsule character, box furniture, box batteries, slab door) stay placeholders. This spec sets the lighting and material baseline they sit in, not final art.

## Impact on Other Specs

These are not changed by this spec. They need a follow-up alignment pass once 001-1 is clarified:

- **015 Art, Models & Lighting Pass.** This spec is most affected.
  - FR-004 (`characterRenderMode` 2D sprite fallback, cut #4) needs rethinking. A sprite fallback would now be a flat character inside the 3D world rather than instead of it.
  - FR-005 ("final 2D office environment art / tileset") would become office art applied to 3D floor, wall and furniture shapes. Eileen's environment-art lane changes from sprite tiles to textures and low-poly props.
  - FR-009 and FR-010 ("2D darkness mask radius and 3D character light range must match") are automatically satisfied and can be simplified.
- **004 Floor & Level Framework.** The Assumptions line naming SpriteKit scene files and tilemaps as an authoring option should allow 3D level data instead.
- **ROADMAP.md.** Section 1 needs a row for 001-1. The build order puts 001-1 before 002. The contracts table is unaffected.
- **Constitution.** No principle changes. Its iOS 26 rationale mentions SpriteKit/Sprite3D APIs. This is a wording refresh only (PATCH), not required to proceed.
- **001 plan.md, research.md and tasks.md.** The rendering tasks and research §5 (camera sync between renderers) become obsolete. The logic-layer tasks and tests stay.

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[001-core-prototype/spec|001 — Core Movement & Light System Prototype]] — the spec this amends
- [[constitution]] — governing principles this spec must comply with
- [[LILO-GDD-v2-Production-Lock]] — Ch. 5.2, 11, 12, 13, 15, 20 are relevant; Ch. 11.1 and 12 are amended here
