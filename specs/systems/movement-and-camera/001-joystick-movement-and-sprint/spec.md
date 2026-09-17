# Feature Specification: Joystick Movement & Sprint

**Feature Branch**: `001-joystick-movement-and-sprint`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 4.1 (Movement), Ch. 15.1 (Controls layout), Ch. 15.3 (control parameters must
live in config), Ch. 17.1 (Player config keys)

**Input**: User description: "Virtual joystick input drives walk/sprint speed. Sprint triggers
automatically at full joystick deflection — there is no separate sprint button. This is generic
movement logic reusable in every gameplay scene (Floor 52/51/50), not tied to a specific room."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Walking Around With the Joystick (Priority: P1)

A player pushes the on-screen virtual joystick in a direction and their character moves that way
at a steady walking pace, so exploring the office at a controlled, quiet pace feels natural on a
touch screen.

**Why this priority**: This is the minimum viable interaction for the entire game — nothing else
(camera follow, collision, noise detection, hiding) has anything to consume until a character can
move at all. Every other user story in this spec is a refinement on top of this one.

**Independent Test**: In an empty scene with one `PlayerCharacter` and no obstacles, drive the
joystick input directly (via a test harness or Play Mode) in each of the four cardinal directions
and confirm the character's position changes at `walkSpeed` in the matching direction, and stops
the instant the joystick is released.

**Acceptance Scenarios**:

1. **Given** the joystick is at rest, **When** the player pushes it partway in any direction (above
   the dead zone, below the sprint threshold), **Then** the character moves in that direction at
   `walkSpeed`.
2. **Given** the character is moving, **When** the player releases the joystick back to center,
   **Then** the character stops moving on the same frame the joystick returns to rest.
3. **Given** the joystick is pushed in a diagonal direction, **When** movement is computed,
   **Then** the character moves in that diagonal direction at no more than `walkSpeed` (diagonal
   input is not faster than cardinal input at the same push strength).

---

### User Story 2 - Sprinting Without a Button (Priority: P1)

A player pushes the joystick all the way to its edge and their character automatically breaks into
a sprint — faster, but noisier (noise is handled by a separate spec) — with no separate button to
press, so a sudden decision to run never costs a fumbled extra tap.

**Why this priority**: Sprint-by-full-deflection is an explicit, non-negotiable GDD decision (Ch.
4.1, Ch. 15.1: "Tanpa tombol terpisah") and is the other half of the core moment-to-moment movement
loop (GDD Ch. 2.1: "... dengar cue monster → hide/evade ..." depends on being able to sprint away
on demand). Tied with User Story 1 as P1 because a movement spec without sprint does not satisfy
the GDD's core loop.

**Independent Test**: Push the joystick to full deflection and confirm the character's speed equals
`walkSpeed × sprintMultiplier`; ease the joystick back under the sprint threshold and confirm the
character returns to plain `walkSpeed` on the same frame, with no separate input required at any
point.

**Acceptance Scenarios**:

1. **Given** the joystick deflection reaches or exceeds `sprintJoystickThreshold`, **When**
   movement is computed that frame, **Then** the character's speed is `walkSpeed × sprintMultiplier`.
2. **Given** the character is sprinting, **When** the player eases the joystick back below
   `sprintJoystickThreshold` (but still above the dead zone), **Then** the character immediately
   returns to `walkSpeed` — sprint never "sticks" past the threshold.
3. **Given** any player input state, **When** checking what triggered sprint, **Then** no button,
   tap, or hold gesture other than joystick deflection is involved — there is no sprint button
   anywhere in the control scheme.
4. **Given** the character is sprinting, **When** other systems need to know about it (noise
   emission, audio mixing, footstep sound), **Then** an `IsSprinting` flag for the current frame is
   available for those systems to read.

---

### User Story 3 - Small, Accidental Nudges Don't Move the Character (Priority: P2)

A player's thumb rests near the joystick's center without meaning to move, or a UI/touch reporting
quirk produces a tiny non-zero deflection, and the character stays perfectly still — so idle
players are never quietly repositioned or made to make noise they didn't intend.

**Why this priority**: Directly required by the Fase 1 test plan (GDD Ch. 20.2: "Tidak ada yang
tidak sengaja sprint saat mau jalan pelan" — the inverse problem, unintended input at either end of
the range, is the same dead-zone mechanism). It refines User Story 1 rather than replacing it, so it
is P2.

**Independent Test**: Feed the movement system a sequence of very small, jittering deflection
values (below the configured dead zone) over several seconds and confirm zero net displacement.

**Acceptance Scenarios**:

1. **Given** the joystick reports a deflection magnitude below `joystickDeadZone`, **When**
   movement is computed, **Then** the resulting velocity is exactly zero, not merely small.
2. **Given** deflection jitters back and forth within the dead zone for several seconds, **When**
   the character's net displacement is measured, **Then** it is zero.

---

### User Story 4 - Diagonal Input Never Out-Runs Straight Input (Priority: P3)

A player pushing the joystick diagonally at maximum deflection moves at the same top speed as
pushing it straight up, down, left, or right — not faster — so no direction is a secret speed
exploit.

**Why this priority**: A correctness/fairness detail that matters for feel and for any future
speed-based balancing (chase sequences, GDD Ch. 6.2's hard rule that monster chase speed must stay
below player sprint speed depends on player sprint speed being a single, predictable number, not
one that varies by input angle). Lower priority than US1–US3 because it is a refinement of an
already-working movement system, not a precondition for one.

**Independent Test**: Drive the joystick to full deflection along a cardinal axis and measure top
speed; drive it to full deflection along a diagonal; confirm both produce the same speed.

**Acceptance Scenarios**:

1. **Given** the joystick is pushed to full deflection along a 45° diagonal, **When** speed is
   measured, **Then** it equals the same top speed produced by full deflection along a cardinal
   axis — never higher.

---

### Edge Cases

- Joystick deflection sits exactly at `sprintJoystickThreshold`: treated as sprint (inclusive lower
  bound), for a single, unambiguous rule.
- Joystick deflection sits exactly at `joystickDeadZone`: treated as movement (inclusive upper bound
  of the dead zone is the last value ignored; the boundary value itself starts producing movement),
  mirroring the sprint boundary rule for consistency.
- `joystickDeadZone` configured greater than or equal to `sprintJoystickThreshold`: an invalid
  configuration (no usable walking band remains) — MUST be caught by a config validation check, not
  discovered as a silent runtime dead zone.
- Player releases the joystick mid-sprint on the exact frame a scene pauses or unloads: movement
  computation simply stops being called; no special-cased "sprint interrupted" state is needed
  since sprint is derived fresh from input every frame, never stored.
- A very large or spiking frame time (e.g., the app resuming from background) could otherwise move
  the character an unreasonably large distance in one step; this spec assumes `GameManager` (see
  Related) clamps frame delta upstream before any system — including this one — reads it, so this
  spec does not add its own clamp.
- Touch input is lost mid-gesture (finger lifts without a clean "released" event reaching center):
  the input adapter MUST treat "no active touch" as center/zero deflection, never as "last known
  deflection persists."

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST read a per-frame joystick deflection as a 2D direction plus a
  magnitude normalized to the 0–1 range (0 = centered, 1 = fully pushed to the joystick's edge).
- **FR-002**: A deflection magnitude below the configured dead zone MUST produce zero movement
  output — not merely a small value. The dead zone value MUST live in `GameConfig`
  (`joystickDeadZone`); the GDD does not specify a number for it (Ch. 15.3), so no value is
  fabricated here — it is a config field pending on-device tuning (see Assumptions).
- **FR-003**: For a deflection magnitude at or above the dead zone and below the sprint threshold,
  the character's speed MUST be `walkSpeed` (GDD Ch. 4.1, Ch. 17.1: base unit, default `1.0`),
  scaled by the deflection's direction.
- **FR-004**: For a deflection magnitude at or above the configured sprint threshold, the
  character's speed MUST be `walkSpeed × sprintMultiplier` (GDD Ch. 17.1: `sprintMultiplier`
  default `1.6`). No separate button, tap, or hold gesture may trigger or gate sprint — deflection
  magnitude is the only signal (GDD Ch. 4.1, Ch. 15.1).
- **FR-005**: The sprint threshold MUST live in `GameConfig` (`sprintJoystickThreshold`). The GDD
  explicitly defers this number to on-device tuning (Ch. 17.1: "TBD di device") — this spec does
  not lock a final number; see Assumptions for the interim default to re-confirm.
- **FR-006**: Sprint MUST activate and deactivate purely based on the current frame's deflection —
  it MUST NOT be a sticky/latched state that persists once the joystick eases back below threshold,
  and MUST NOT require the joystick to return to center first.
- **FR-007**: Diagonal deflection MUST NOT produce a resultant speed greater than the same
  magnitude of deflection along a cardinal axis — the direction vector feeding into speed
  calculation MUST be magnitude-normalized before being scaled by walk/sprint speed.
- **FR-008**: The system MUST NOT implement any stamina, energy, or exertion meter tied to
  sprinting — the GDD explicitly rejects this (Ch. 4.1: "Stamina TIDAK ADA").
- **FR-009**: The system MUST NOT couple sprinting to flashlight battery drain in any way — the GDD
  explicitly rejects this coupling (Ch. 4.1: "Sprint drain battery TIDAK ADA").
- **FR-010**: The movement calculation (deflection + config in, resultant velocity and an
  `IsSprinting` flag out) MUST be implemented as a plain, engine-lifecycle-independent unit so it
  is fully testable without a running scene (constitution Principle III/IV). Reading the actual
  touch/joystick hardware input is a separate, thin adapter responsibility.
- **FR-011**: The current frame's `IsSprinting` flag and resultant velocity MUST be exposed for
  other systems to consume without those systems recomputing movement themselves — specifically the
  camera follow system (movement-and-camera/003), per-action noise emission
  (noise-and-detection/001), and dynamic audio mixing (audio/002).
- **FR-012**: The joystick's on-screen presentation values (diameter, opacity, and its fixed
  screen-space position/anchor) MUST live in `GameConfig` rather than being hardcoded in UI layout
  code (GDD Ch. 15.3). The GDD does not specify numbers for these (Ch. 15.3: "belum dikunci ...
  ditentukan sambil dicoba langsung di device") — this spec does not fabricate them; see
  Assumptions. Rendering the joystick's visuals is not this spec's concern (see game-shell-ui/004);
  this spec owns only that the position/size values used to interpret touch input into a deflection
  vector are config-driven, not hardcoded.
- **FR-013**: An invalid configuration where `joystickDeadZone` is greater than or equal to
  `sprintJoystickThreshold` (leaving no walking band) MUST be detectable by a validation check
  rather than only discovered by feel during playtesting.

### Key Entities

- **MovementInput**: A per-frame data value (direction, magnitude 0–1) produced by reading the
  on-screen joystick's current touch state. Owned by a thin input adapter, not gameplay logic.
- **MovementSystem**: The plain C# system that turns a `MovementInput` plus the relevant
  `GameConfig` fields into a resultant world-space velocity and an `IsSprinting` flag for the
  current frame. Contains no engine lifecycle or component dependency.
- **PlayerCharacter**: The player's on-screen avatar (fixed name, per project convention). Owns the
  `Transform` that the movement adapter moves each frame using `MovementSystem`'s output.
- **GameConfig fields added by this spec**: `walkSpeed`, `sprintMultiplier`,
  `sprintJoystickThreshold`, `joystickDeadZone`, `joystickDiameter`, `joystickOpacity`,
  `joystickCenterOffset`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Holding the joystick at full deflection for a fixed duration covers noticeably more
  ground than holding it at a partial, sub-threshold deflection for the same duration, by exactly
  the configured sprint multiplier's ratio.
- **SC-002**: Small, unintentional joystick jitter within the dead-zone range produces zero
  character displacement over a sustained multi-second period, in 100% of sampled jitter sequences.
- **SC-003**: Every tunable movement number this spec introduces (walk speed, sprint multiplier,
  sprint-trigger threshold, dead zone, joystick size/opacity/position) can be changed by editing one
  configuration asset alone — no code change or recompile — and the change is observable in
  gameplay on the next run.
- **SC-004**: Sprinting starts and stops within a single frame of the joystick crossing the
  configured threshold in either direction, with zero additional input beyond joystick deflection.
- **SC-005**: Pushing the joystick diagonally at maximum deflection never produces a higher top
  speed than pushing it along a single cardinal direction at maximum deflection, across all tested
  angles.
- **SC-006**: An external playtester who has never seen the control scheme can walk at a controlled
  pace and then sprint on demand within their first minute of play without being told sprint exists
  as a separate mechanic (validates GDD Ch. 20.2's Fase 1 test plan item).

## Assumptions

- `sprintJoystickThreshold` has no GDD-locked number (Ch. 17.1 marks it "TBD di device"). Following
  this vault's carried-over-value convention (see ROADMAP.md §0), the interim default to re-confirm
  on-device is `0.9` (0–1 deflection scale), inherited from this project's prior on-device tuning
  pass on an equivalent build — a starting point, not a locked constant.
- `joystickDeadZone`, `joystickDiameter`, `joystickOpacity`, and `joystickCenterOffset` have no
  prior on-device tuning value anywhere in this project's history and are left as pending
  `GameConfig` fields with no fabricated default; they must be set during this feature's on-device
  tuning pass before the feature is considered feel-complete, per GDD Ch. 15.3 and Ch. 21 ("Parameter
  joystick ... Harus selesai sebelum: Akhir Fase 1").
- Because the game's camera never rotates relative to the world (GDD Ch. 13: north-facing, no
  rotation — see movement-and-camera/004), the joystick's direction vector maps directly to
  world-space movement without any camera-relative transform. If a rotating camera were ever
  introduced, this mapping would need revisiting.
- Frame-delta spikes (e.g., app resume from background) are assumed to be clamped upstream by
  `GameManager` before any per-frame system reads them (constitution Principle III); this spec's
  movement math takes `deltaTime` as a given input and does not implement its own clamp.
- The action button, HUD, and joystick visual rendering are out of scope of this spec (see
  interaction-and-highlight/002 and game-shell-ui/004); this spec owns only the joystick's input
  interpretation and the resulting walk/sprint speed logic.

## Related

- GDD: Ch. 4.1 (Movement), Ch. 15.1 (Controls layout — joystick position/role), Ch. 15.3 (control
  parameters must be config-driven), Ch. 17.1 (Player `GameConfig` keys).
- ROADMAP.md: `systems` table, `movement-and-camera` row `001-joystick-movement-and-sprint`; see
  §0 for the shared naming/config conventions this spec follows, and §5 for build order (this
  feature has no dependency other than `shared-config-and-state/002`).
- Depends on: `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`
  for where `GameState`/`GameManager` are defined (this spec does not redefine them).
- Consumed by (per ROADMAP.md §2/§5): `movement-and-camera/002` (collision), `003` (camera follow),
  `hiding/001`, `noise-and-detection/001`, `audio/002`, `game-shell-ui/004`.
