# Feature Specification: LILO Phase 1 — Core Movement & Light System Prototype

**Feature Branch**: `001-core-prototype`

**Created**: 2026-09-17

**Status**: Draft — rendering requirements amended by [[001-1-unified-3d-world/spec|001-1]] (FR-013, FR-014, FR-021, SC-002 superseded)

**Input**: User description: "Fase 1 — Core Prototype untuk LILO (Lights In, Lights Out), horror survival iOS game. Referensi lengkap: LILO GDD v2 Production Lock. Scope (sesuai GDD Bab 20): satu ruangan kotak sebagai prototype level, tanpa monster, tanpa art asli (placeholder kotak/kapsul), tanpa audio. Player bisa bergerak + sprint via joystick, senter yang menguras battery real-time selama 180 detik dengan 4 light state bertingkat, battery pickup + slot cadangan, tombol aksi context-sensitive, satu pintu, kamera orthographic smooth-follow, karakter 3D di atas background 2D (SK3DNode risk validation), vignette 2D + spotlight 3D menyatu. Semua angka tuning wajib dari satu GameConfig. Definition of Done sesuai GDD Bab 20.3."

## Clarifications

### Session 2026-09-17

- Q: Device iPhone spesifik mana yang jadi baseline minimum buat validasi 60fps di SC-002? → A: iPhone 17 — superseded from the initial iPhone SE (3rd gen) recommendation once the iOS 26 minimum was set: the team is standardizing on iPhone 17 as everyone's development/testing baseline for this iOS 26 cycle, not the lowest-end device in the abstract.
- Q: Should the battery/flashlight charge have a persistent HUD indicator, beyond the diegetic light itself? → A: Yes — a simple bar is sufficient (per GDD Ch. 15.1's "indikator battery + slot cadangan"); see FR-017.
- Q: Minimum iOS deployment target buat project ini? → A: iOS 26. *(Amended 2026-09-17 — engine migration: the project now ships from a Unity/URP build rather than a native SpriteKit/SceneKit one; iOS 26 remains the deployment floor for the exported Xcode project, chosen for device-support parity with the iPhone 17 baseline below, not for any specific native API.)*
- Q: Battery satu-satunya di test room itu respawn setelah diambil, atau statis (sekali diambil hilang)? → A: Static — it does not respawn once picked up.

### GDD Alignment Pass 2026-09-17

A re-read of this spec against LILO GDD v2 Production Lock (Ch. 4.2, 5.2, 13, 15.3, 20.2, 20.3) found requirements the GDD states for Phase 1 that this spec had missed or contradicted. They are folded in below. Because `tasks.md` was generated before this pass, run `/speckit-analyze` then `/speckit-converge` on this feature to turn the delta into tasks.

### Engine Migration Pass 2026-09-17

The project's engine changed from native iOS (Swift + SpriteKit + SceneKit + SwiftUI) to Unity
(C#, URP). This is a technology-layer change only — every functional requirement, success
criterion, and numeric value below is unchanged. Two places that named the old architecture as a
locked assumption are updated: the rendering line in Assumptions, and FR-014's wording (see
below). FR-013, FR-014 and FR-021 were already marked superseded by
[[001-1-unified-3d-world/spec|001-1]] before this pass; that relationship is unchanged — 001-1's
"unified 3D world" decision is now simply what Unity does by default from the start, rather than
a later fix to a hybrid renderer. See `plan.md`/`research.md` for this feature's now-current
Unity technical decisions.

- Install-battery gating → GDD 4.2 ("Senter < 10% / kosong → Pasang battery dari slot") and 5.2 ("10–0%: Tombol aksi mulai menampilkan opsi pasang battery"): the install action is only offered once charge is at or below the Critical threshold. Replaces the previous "available at any charge" behavior. See FR-009, FR-011, US3.
- Interactable highlight → GDD 4.2 ("Feedback interactable: highlight warna pada objek… Tidak pakai ikon prompt melayang"). See FR-018.
- Placeholder monster figure → GDD 20.2 row 1 requires "1 karakter 3D + 1 monster 3D di scene" for the on-device performance test. A static, behavior-less placeholder is added; no AI. See FR-019, SC-002.
- Second loose battery → GDD 20.3 DoD requires "Slot cadangan menolak battery kedua saat sudah penuh, dan player paham kenapa", which cannot happen naturally with one battery. The room now holds two static batteries. This intentionally deviates from GDD 20.1's literal "satu battery" to satisfy the DoD. See FR-016, US3.
- On-screen control layout values → GDD 15.3 requires joystick diameter/dead zone/opacity/position and action-button size/position/touch radius to come from config. See FR-015.
- Camera framing → GDD 13 (orthographic, north-facing, 45° tilt, fixed zoom). See FR-012.
- Solid furniture collision → GDD 19.2 Phase 1 target lists "collision". See FR-020.
- Missing Phase 1 test questions from GDD 20.2 (vignette/spotlight unity, joystick feel, 180s drain feel) added as FR-021, SC-007, SC-008, SC-009. SC-006 is re-timed because install gating makes the full loop take at least one near-full battery cycle.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Move and Explore the Test Room (Priority: P1)

A player spawns in a single test room and can walk around freely and sprint when they need to move faster, with the camera smoothly keeping them in view, so that the core feel of moving through LILO's world can be validated on a real device before anything else is built on top of it.

**Why this priority**: Every other system in the game (light, battery, interaction, eventually the monster) is experienced through this movement and camera loop. If it doesn't feel right or doesn't run correctly on device, nothing built after it matters.

**Independent Test**: Can be fully tested by placing a player character in the test room with nothing else present and walking/sprinting around it — delivers a working, feelable movement loop entirely on its own.

**Acceptance Scenarios**:

1. **Given** the player is idle at the spawn point, **When** the player pushes the joystick partway in any direction, **Then** the character walks in that direction at base speed and the camera smoothly follows.
2. **Given** the player is walking, **When** the player pushes the joystick to full deflection, **Then** the character moves at sprint speed (1.6× base speed) with no separate control needed to trigger it.
3. **Given** the player moves toward the edge of the room, **When** the character approaches the room boundary, **Then** the camera stops panning further so the player never sees empty space beyond the level.

---

### User Story 2 - Experience the Flashlight Running Down (Priority: P1)

A player carries a flashlight that drains in real time, and can tell — purely by looking at it — how much charge is left through four visibly distinct states, so the tension at the heart of "Lights In, Lights Out" is provable before any other system is added.

**Why this priority**: The light/battery system is the game's core mechanic, not a supporting feature — the GDD's own title refers to it directly. It must be validated as early and in isolation as movement.

**Independent Test**: Can be fully tested by starting with a fully charged flashlight and letting time pass without touching any pickup — delivers an observable, self-contained light-decay experience.

**Acceptance Scenarios**:

1. **Given** the flashlight is freshly installed at 100% charge, **When** no battery is swapped in, **Then** the charge depletes continuously in real time and reaches 0% at exactly 180 seconds.
2. **Given** the flashlight charge drops below 30%, **When** the player observes the light, **Then** it visibly flickers, signalling the Flickering state.
3. **Given** the flashlight charge drops below 10%, **When** the player observes the light, **Then** its radius visibly narrows, signalling the Critical state.
4. **Given** the flashlight charge reaches 0%, **When** the player continues playing, **Then** the light shrinks to a small fixed radius (Compact Darkness) and the player can still move and act — the game does not end.
5. **Given** the player is standing still or hiding-equivalent idle, **When** time passes, **Then** the charge drains at the same rate as while moving.

---

### User Story 3 - Recover a Spare Battery Before Running Out (Priority: P2)

A player finds a loose battery in the room, picks it up, and swaps it into the flashlight once the light is running critically low, so the resource-management decision at the center of the game can be tested independently of movement polish or light-state tuning.

**Why this priority**: This depends on User Story 2 existing (a battery to manage) but is a distinct, separately testable interaction loop — pickup, carry, install — that can be validated once the light system works.

**Independent Test**: Can be fully tested by spawning a player near the two loose batteries, picking one up, confirming the second is rejected, letting the light fall to Critical, and installing the spare — delivers a complete pickup-and-refill loop including the slot-full rule.

**Acceptance Scenarios**:

1. **Given** the player is near a battery lying in the room, **When** the object is within interaction range, **Then** the battery is visibly highlighted, and **When** the player presses the context-sensitive action button, **Then** the battery is picked up into the spare slot and disappears from the world.
2. **Given** the player is carrying a spare battery and the installed charge is above 10%, **When** the player looks at the action button away from any other interactable, **Then** no install option is offered.
3. **Given** the player is carrying a spare battery and the installed charge is at or below 10% (Critical or Compact Darkness), **When** the player presses the action button to install it, **Then** the flashlight's charge is set to exactly 100% and the previously installed battery's remaining charge is discarded.
4. **Given** the player already has an installed battery and a full spare slot, **When** the player tries to pick up the second loose battery, **Then** the pickup is rejected, the battery remains in the world, and the player is shown clear feedback explaining why it couldn't be picked up.

---

### User Story 4 - Reach the Exit (Priority: P3)

A player locates the room's single door and opens it using the same action button used for every other interaction, completing the smallest possible full loop through the prototype.

**Why this priority**: This closes the loop end-to-end but depends on nothing new being built — it reuses the same interaction button as User Stories 2 and 3, so it's the lowest-risk, lowest-priority piece to validate last.

**Independent Test**: Can be fully tested by walking a player character to the door and pressing the action button — delivers a demonstrable "start to finish" run through the prototype room.

**Acceptance Scenarios**:

1. **Given** the player is near the door, **When** the player presses the context-sensitive action button, **Then** the door opens with clear feedback that it succeeded.
2. **Given** the player is not near any interactable object, **When** the player looks at the action button, **Then** it shows no label or prompt.

---

### Edge Cases

- What happens when the flashlight reaches exactly 0% while the player is sprinting? The transition to Compact Darkness MUST occur without interrupting movement or causing an error state.
- What happens when the player tries to pick up a battery while both the installed and spare slots are full? The pickup MUST be rejected with feedback, per User Story 3, Scenario 3 — the item is never silently lost.
- What happens when the player is near the door while carrying a spare battery? The action button MUST reflect whichever interactable is nearest/relevant, not conflate the two prompts.
- What happens if the player sprints directly into the room boundary? The camera MUST stop panning (per US1) while the character's own collision with the wall is handled separately — the character does not pass through geometry.
- What happens if the player sprints into the placeholder desk or other solid furniture? The character MUST stop against it (FR-020) rather than walking through it, while the desk still occludes the character when the character is behind it (FR-014).
- What happens if the installed charge is refilled by an install while the light is in Compact Darkness? The light MUST return to Normal immediately, and the install option MUST disappear because charge is now above 10%.
- What happens if the player walks into the placeholder monster figure? It is solid scenery for this phase only — no catch, no damage, no behavior.
- What happens if the device is rotated out of landscape? Out of scope for this feature — the app is landscape-only; no portrait behavior is defined.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST let the player move the character in any direction using a virtual joystick control positioned in the left portion of the screen.
- **FR-002**: System MUST let the player sprint at 1.6× walking speed automatically when the joystick is pushed to full deflection, without a separate sprint control.
- **FR-003**: System MUST NOT drain the flashlight's charge any faster while the player is sprinting than while walking or standing still — drain is time-based only.
- **FR-004**: System MUST continuously drain the installed battery's charge in real time such that a fully charged (100%) battery reaches 0% at exactly 180 seconds, regardless of player action.
- **FR-005**: System MUST present four visibly distinct light states driven by remaining charge, with each charge value mapping to exactly one state (boundaries are not shared): Normal is `30% < charge ≤ 100%` (full radius), Flickering is `10% < charge ≤ 30%` (visible flicker), Critical is `0% < charge ≤ 10%` (visibly narrowing radius), and Compact Darkness is `charge = 0%` (radius fixed at roughly 10% of normal). E.g., exactly 30% charge is Flickering, not Normal; exactly 10% charge is Critical, not Flickering.
- **FR-006**: At 0% charge, system MUST keep the player able to move and act normally — reaching Compact Darkness MUST NOT end the session or block input.
- **FR-007**: System MUST let the player pick up a battery item in the room through a single context-sensitive action button that appears when an interactable object is within range.
- **FR-008**: System MUST cap the player's carried batteries at one installed plus one spare; a pickup attempted while both are full MUST be rejected, MUST leave the item in the world, and MUST show the player feedback explaining why.
- **FR-009**: System MUST let the player install the spare battery via the action button only while the installed charge is at or below the Critical threshold (10%, i.e. Critical or Compact Darkness, per GDD 4.2/5.2). Installing MUST always set the flashlight's charge to exactly 100% and discard the remaining charge of whatever was previously installed.
- **FR-010**: System MUST let the player open the room's single door via the same context-sensitive action button when standing near it.
- **FR-011**: System MUST update the action button's label/icon to match the currently available interaction (pick up battery, install battery, open door) and MUST show no prompt when no interaction is available. The install option counts as available only under FR-009's charge condition.
- **FR-012**: System MUST keep the camera centered on the player with smooth, non-instant follow motion, and MUST stop the camera from panning past the test room's boundaries. The view MUST use a fixed-zoom, orthographic (no perspective distortion) north-facing framing tilted roughly 45°, per GDD Ch. 13; no dynamic zoom exists.
- **FR-013**: System MUST keep the player character's apparent position, scale, and grounding on the environment visually consistent while the player moves and the camera follows (no drifting, scaling, or floating artifacts).
- **FR-014**: System MUST let solid environment geometry (e.g., a piece of furniture) visually occlude the player character when that geometry is positioned in front of the character from the camera's viewpoint, based on actual scene depth rather than a manually-authored draw order.
- **FR-015**: Every tunable numeric value used by this feature's systems (including but not limited to walk speed, sprint multiplier, battery duration, light-state thresholds, and the on-screen control layout values from GDD 15.3 — joystick diameter, dead zone, opacity and position; action button size, position and touch radius — see `contracts/game-config.md` for the authoritative full list) MUST be defined in exactly one configuration source, with no such value hardcoded elsewhere.
- **FR-016**: The test room MUST contain exactly two loose batteries at fixed positions. System MUST NOT respawn either battery once it has been picked up — each remains removed from the world for the rest of the session.
- **FR-017**: System MUST display the installed flashlight's current charge as a persistent bar-style indicator in the HUD, and MUST show whether the spare battery slot is empty or occupied, so the player has an at-a-glance view of both without any menu (per GDD Ch. 15.1's HUD spec; decided in Clarifications above).
- **FR-018**: System MUST visually highlight (color highlight on the object itself) any interactable object — battery, door — while it is within interaction range, and MUST NOT use floating prompt icons over objects (GDD 4.2).
- **FR-019**: The test room MUST contain one static placeholder monster figure rendered through the same 3D-over-2D pipeline as the player character. It has no AI, no movement, and no gameplay effect; it exists only so performance is measured with two 3D characters on screen (GDD 20.2).
- **FR-020**: System MUST prevent the player character from passing through solid furniture placed in the room (at minimum the placeholder desk), not only the room's outer walls.
- **FR-021**: As the light radius shrinks through Critical into Compact Darkness, the 3D characters (player and placeholder monster) MUST darken consistently with the 2D environment. No 3D character may remain brightly lit inside an otherwise dark screen.

### Key Entities

- **Player Character**: The in-scene avatar (Eddie), placeholder-rendered for this phase. Tracks position, movement state, and which battery is currently installed.
- **Battery**: A pickup with a charge level. Exists either lying in the world at a fixed spawn point, installed in the flashlight, or carried as a spare. The room has exactly two loose batteries, neither of which respawns once collected.
- **Flashlight (Light Source)**: Attached to the player; its visible radius and flicker behavior are driven entirely by the installed battery's charge state, and its charge is additionally mirrored as a persistent HUD bar (FR-017).
- **Door**: The room's single interactable exit object.
- **Placeholder Monster Figure**: A static, behavior-less 3D stand-in used only for the performance and lighting checks (FR-019, FR-021). Replaced by the real monster in spec 002.
- **Test Room**: The fixed, single-room environment containing the player spawn point, two batteries, one door, one placeholder desk, and one placeholder monster figure — no other rooms are in scope.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A first-time player can walk and sprint around the test room on the target iPhone hardware and describe the movement as responsive, with no noticeable camera stutter.
- **SC-002**: The game sustains at least 60 frames per second on an iPhone 17 — the project's baseline target device for this iOS 26 development cycle — during continuous player movement with both the player character and the placeholder monster figure on screen, measured on physical hardware, not a simulator.
- **SC-003**: In an observation test with at least 2 people outside the development team, each person can correctly identify which of the four light states they're currently seeing without being told what to look for.
- **SC-004**: A player can let the flashlight drain from 100% to 0% and keep playing afterward without the game crashing or becoming unresponsive.
- **SC-005**: Every tunable value used in this prototype (speed, drain duration, thresholds) can be changed by editing a single configuration source, with no other file requiring a change.
- **SC-006**: A new player can complete the full loop — spawn, pick up a battery, see the second battery rejected, wait for the light to reach Critical, install the spare, and exit through the door — within 4 minutes without being given instructions. (The install gate means the loop cannot be shorter than about 162 seconds at the default 180-second battery.)
- **SC-007**: In a joystick test with 2–3 people outside the development team, each asked to walk slowly and then suddenly run, none of them sprints unintentionally while trying to walk slowly (GDD 20.2).
- **SC-008**: After at least one full uninterrupted 180-second drain walkthrough, the team records a decision on whether 180 seconds feels pressing but not too loose. Any change is made only in the configuration source (GDD 20.2).
- **SC-009**: When the light is shrunk to Compact Darkness, observers see no 3D character that stays brightly lit in the middle of an otherwise dark screen (GDD 20.2).

## Assumptions

- Per the project's technical direction (GDD v2.1 Ch. 11 and the constitution's Technology Stack), the test room's environment, the player character, and every other scene object are rendered together in one Unity URP 3D scene — there is no separate 2D environment layer. This spec's rendering-related requirements (FR-013, FR-014) describe the observable outcome that setup must produce (consistent grounding, correct occlusion), not a mandated implementation; both are automatically satisfied by ordinary 3D depth rather than requiring extra work, since Unity has no hybrid renderer to reconcile.
- Performance validation (SC-002) is measured on physical iPhone hardware — specifically an iPhone 17, the team's standardized baseline device for this iOS 26 cycle — not the Unity Editor's Game view or a simulator, per the GDD's own Phase 1 test plan.
- Minimum iOS deployment target is iOS 26, matching the iPhone 17 baseline device. This resolves the constitution's previously open `TODO(IOS_DEPLOYMENT_TARGET)`.
- All room geometry, the player character, the placeholder monster figure, and the battery/door objects are placeholder primitive shapes for this feature — no final art is in scope.
- Haptics are not required in this phase (GDD assigns them to Phase 6, spec 014). Hooks may exist, but no FR here depends on them.
- No audio assets are in scope for this feature; any sound-trigger points needed later are not required to produce actual sound yet.
- No monster/AI behavior, no narrative content (prologue/ending), and no multi-room or procedural level design are in scope — this feature is a single fixed test room only.
- Device orientation is locked to landscape; no portrait-mode behavior is defined or required.
- Exact values for a few parameters (e.g., joystick sprint threshold, interaction radius) are intentionally left to be tuned by feel on-device during this phase rather than fixed in advance, per the source GDD's own open items — FR-015 requires wherever they land to live in the single configuration source, not the specific numbers themselves.

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[constitution]] — governing principles this spec must comply with
- [[LILO-GDD-v2-Production-Lock]] — Ch. 4, 5, 11–13, 15, 17, 20 cover this feature directly
