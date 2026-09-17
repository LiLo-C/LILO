# Feature Specification: Readability Fill Light

**Feature Branch**: `006-readability-fill-light`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "A single config-driven fill light so the room stays faintly
readable outside the flashlight's reach; setting its intensity to 0 must reproduce pure black."

## Why This Spec Exists

GDD Ch. 12 states the rule directly: "Di luar radius senter, satu light fill (intensitas dari
config, `0` = gelap total) menjaga bentuk ruangan tetap samar terlihat supaya player tidak
tersesat total — ini menggantikan kebutuhan vignette 2D yang dulu murni kosmetik." In the v2.0
hybrid architecture this readability floor was a cosmetic 2D vignette layered outside the 3D
scene entirely; in the Unity/URP single-scene architecture (Ch. 11.1/12, amended v2.1) it must
instead be a real, config-driven light contribution that coexists with the single shadow-casting
flashlight (`001`–`005`) without becoming a second thing the player mistakes for the flashlight
itself. This feature owns exactly that one fill light and the config lever that controls it —
nothing about the flashlight's own radius, shadows, or flicker changes here.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - The Room Stays Dimly Visible Outside the Flashlight's Reach (Priority: P1)

As the player explores, areas outside the flashlight's currently lit radius remain faintly
visible — walls, floor, and furniture silhouettes are readable enough to navigate by — rather than
being pure black everywhere the flashlight doesn't directly reach.

**Why this priority**: This is the feature's entire reason to exist and the direct successor to
the design goal the GDD explicitly names ("supaya player tidak tersesat total"). Every other
story in this feature is a control or edge-case guarantee layered on top of this base behavior.

**Independent Test**: With the fill light at a non-zero configured intensity and no other change,
walk the player through a test room and confirm areas outside the flashlight's current radius are
faintly visible (non-zero brightness) rather than fully black — observable in a running scene with
no code change required to verify.

**Acceptance Scenarios**:

1. **Given** `GameConfig.fillLightIntensity` is set to some positive value, **When** the player
   stands so that part of the room lies outside the flashlight's lit radius, **Then** that area is
   rendered at a low but non-zero brightness — visible enough to make out room shape, not merely
   theoretically non-zero.
2. **Given** the fill light is active, **When** the flashlight's own radius changes (state
   transitions, easing, flicker dips from `001`/`003`/`004`), **Then** the fill light's own
   contribution is unaffected by those changes — it is a separate, independent light source, not a
   second copy of the flashlight's radius logic.
3. **Given** the fill light is active, **When** a solid object would normally cast a shadow from
   the flashlight (`005`), **Then** the fill light does not itself cast a competing shadow — only
   the flashlight is a shadow-casting light in this scene (GDD Ch. 12's single-shadow-source
   design is preserved).

---

### User Story 2 - Fill Brightness Is a Single Config Value, Changeable With No Other Edit (Priority: P2)

A developer can raise or lower how visible the room is outside the flashlight's reach by changing
one `GameConfig` value, with no scene, material, or code change required.

**Why this priority**: GDD Ch. 12 names this explicitly as a config-driven value ("intensitas
dari config"), and it is exactly the kind of feel-tuning lever the whole GameConfig discipline
(constitution Principle III/V) exists to support. It is P2 rather than P1 because the base
behavior (User Story 1) must exist before there is anything to tune.

**Independent Test**: Change only `GameConfig.fillLightIntensity` to a different positive value,
run the scene, and confirm the outside-of-flashlight brightness visibly changes with no other file
edited.

**Acceptance Scenarios**:

1. **Given** `GameConfig.fillLightIntensity` is raised to a higher value, **When** the scene
   renders, **Then** the areas outside the flashlight's radius appear visibly brighter than before,
   with the change requiring no code or material edit.
2. **Given** `GameConfig.fillLightIntensity` is lowered (but kept above `0`), **When** the scene
   renders, **Then** those areas appear visibly dimmer, again with no other file changed.

---

### User Story 3 - Setting Fill Intensity to Exactly Zero Reproduces Pure Black (Priority: P2)

If a developer sets `GameConfig.fillLightIntensity` to exactly `0`, every area outside the
flashlight's current lit radius renders as true black — not a dim residual glow left over from
some other, uncontrolled ambient source.

**Why this priority**: GDD Ch. 12 states this as the value's defining boundary condition
("`0` = gelap total"), and it is the guarantee that makes this feature trustworthy as the *only*
source of outside-the-flashlight visibility — if some other stray ambient/skybox contribution
could still leak light through at `0`, the config value would lie about what it controls. It is
P2 because it is a boundary-condition guarantee on top of User Story 1's base behavior, not a
separate visible feature in its own right.

**Independent Test**: Set `GameConfig.fillLightIntensity` to exactly `0` with no other change, run
the scene, position the player so part of the room is outside the flashlight's radius, and confirm
that area renders as pure black (no visible detail, no residual glow) — no code change required to
verify.

**Acceptance Scenarios**:

1. **Given** `GameConfig.fillLightIntensity` is exactly `0`, **When** the scene renders, **Then**
   every point outside the flashlight's current lit radius is pure black — no walls, furniture
   silhouettes, or floor detail are discernible there.
2. **Given** `GameConfig.fillLightIntensity` is exactly `0`, **When** the scene is inspected for
   any other ambient/environment/skybox light contribution independent of this feature's fill
   light, **Then** none exists — the flashlight and this one fill light are the only two light
   sources affecting the playable world (GDD Ch. 11.1/12's single-lighting-system design), so `0`
   on this value truly means total darkness outside the flashlight, not "total darkness minus
   whatever else happens to be lit."
3. **Given** `GameConfig.fillLightIntensity` is raised back above `0` after having been `0`,
   **When** the scene renders, **Then** the faint readability from User Story 1 resumes with no
   other change required.

---

### Edge Cases

- **`GameConfig.fillLightIntensity` configured as a negative number** (authoring error): the
  system MUST clamp the effective value to `0` rather than producing an undefined negative-light
  result or a rendering error.
- **`GameConfig.fillLightIntensity` changed at runtime** (e.g. a debug menu, or during on-device
  tuning): the change MUST be reflected without requiring a scene reload, consistent with `005`'s
  `shadowsEnabled` live-read precedent.
- **The player is standing in the flashlight's Compact Darkness state** (radius shrunk to ~10% of
  normal, per `001`): the fill light's own contribution to areas outside that tiny radius is
  unaffected by the flashlight being in its dimmest state — the two light sources are independent,
  so a non-zero fill still keeps the room faintly legible even when the flashlight itself is
  nearly useless.
- **No solid geometry exists yet in a scene** (earliest test scenes before level art lands): the
  fill light illuminating nothing but an empty room MUST NOT error — it simply has less to
  illuminate.
- **A second, unrelated ambient/skybox/environment light contribution is accidentally left enabled
  in a scene** (a scene-authoring mistake, not this feature's own design): this feature's contract
  is that *it* is the only non-flashlight light source; a stray second source leaking brightness at
  `fillLightIntensity = 0` is a scene-authoring error outside this feature's runtime responsibility
  to detect, though User Story 3 Scenario 2 exists specifically so this class of mistake is caught
  by review/testing before it ships.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide exactly one fill light source that illuminates the playable
  scene broadly and uniformly (not following the player, not targeting anything), in addition to
  and independent of the single shadow-casting flashlight light owned by `001`/`003`/`004`/`005`
  (GDD Ch. 12: "satu light fill").
- **FR-002**: `GameConfig` MUST expose `fillLightIntensity` (float, `>= 0`) as the single value
  controlling this fill light's brightness. Unlike this category's other tunables (`001`'s
  `flashlightNormalRadius`, `003`'s `lightRadiusEaseRate`, `004`'s flicker values, `005`'s shadow
  fields), no carried-over starting value exists from this project's prior native-engine tuning
  pass — a fill light of this kind did not exist in the v2.0 hybrid 2D-vignette architecture, so
  this is genuinely new content in the Unity migration. Implementers MUST choose a placeholder
  default when first wiring the scene, documented explicitly as a placeholder (not a carried-over
  or locked value) to be replaced entirely by on-device playtesting — never treated as settled in
  any later spec or plan.
- **FR-003**: When `fillLightIntensity` is exactly `0`, every point outside the flashlight's
  current lit radius MUST render as pure black — this requires that no other ambient, skybox, or
  environment light contribution independent of this feature affects the playable scene. The
  flashlight (`001`–`005`) and this feature's one fill light MUST be the only two light sources in
  the playable world (GDD Ch. 11.1/12).
- **FR-004**: The fill light MUST NOT cast shadows and MUST NOT be the shadow-casting light that
  `005` configures — only the flashlight light casts real-time shadows in this scene, preserving
  the GDD's single-shadow-source design.
- **FR-005**: The fill light's illumination MUST be independent of the flashlight's own state,
  radius, easing, or flicker (`001`/`003`/`004`) — it is a flat, separate contribution that does
  not read or react to any of those systems' outputs.
- **FR-006**: `GameConfig.fillLightIntensity` MUST be read as a live value — a runtime change MUST
  be reflected without requiring a scene reload, consistent with `005`'s `shadowsEnabled` precedent
  (FR-002/FR-006 there).
- **FR-007**: The effective intensity value applied to the light MUST be clamped to `>= 0` before
  being applied, regardless of what raw value is configured (Edge Cases).
- **FR-008**: Any portion of this feature expressible as pure logic (resolving/clamping the
  configured intensity value into the concrete value applied to the light) MUST live in a plain
  C# class under `Assets/Scripts/Systems/` with no `UnityEngine.MonoBehaviour`, `Component`, or
  scene dependency (constitution Principle III); the part that actually assigns a Unity `Light`
  property is a thin adapter under `Assets/Scripts/MonoBehaviours/`.

### Key Entities

- **Fill Light**: A single additional Unity `Light` present in each playable scene, distinct from
  the Flashlight Light (`001`–`005`). Illuminates broadly and does not follow the player or cast
  shadows. New to this feature.
- **GameConfig** *(extended)*: Gains `fillLightIntensity`.
- **Flashlight Light**: Consumed from `001`/`003`/`004`/`005`, not redefined or modified here —
  this feature only adds a second, independent light alongside it.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With `fillLightIntensity` at a positive value chosen for on-device tuning, a full
  walk-through of a test room shows every area outside the flashlight's current radius at a
  non-zero, discernible brightness — confirmed by at least 2 people outside the development team
  being able to describe the room's basic shape while standing somewhere the flashlight does not
  directly reach.
- **SC-002**: With `fillLightIntensity` set to exactly `0`, a rendered frame sampled at a point
  outside the flashlight's current radius reads as pure black (0,0,0 / no visible detail),
  verified by direct visual inspection (Editor Game view or on-device screenshot) with no other
  light source contributing.
- **SC-003**: Changing `fillLightIntensity` to any value requires editing exactly one file
  (`GameConfig`) to see the difference, with no scene, material, or code change needed.
- **SC-004**: A scene audit confirms exactly one shadow-casting light (the flashlight) and exactly
  one non-shadow-casting fill light exist in each playable scene — no third, stray ambient
  contribution is present.

## Assumptions

- This feature does not lock a final shipped `fillLightIntensity` value — per the category's
  feel-value handling rule (ROADMAP §0), the FR states the rule (config-driven, `0` = pure black)
  and requires the value live in `GameConfig`, without inventing a fake precise number where no
  validated or carried-over one exists.
- The exact Unity/URP mechanism for the fill light (a low-intensity directional light, a
  broad-range point/area light, or Unity's `RenderSettings` environment ambient term) is a
  planning-time decision, not locked by this spec — this spec locks the *behavior* (one
  independent, non-shadow-casting fill contribution, config-driven, `0` = true black), consistent
  with how `005` left its shadow *mechanism* to planning while locking its behavior.
- No changes to `001`–`005`'s own logic are made by this feature — it is purely additive (a second,
  independent light alongside the existing flashlight), consistent with the category's scope note
  that this feature only adds the readability fill.

## Related

- [[ROADMAP]] — flashlight-and-battery row 6; depends on `001-light-state-thresholds-and-radius`
- [[constitution]] — Principle III (plain C# systems / thin adapters), Principle IV (EditMode
  tests where expressible, on-device validation otherwise)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 12 (fill light requirement, `0` = gelap total), Ch. 12.1
  (single lighting system), Ch. 17.2 (GameConfig keys)
- `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius` — the Flashlight
  Light this feature's fill coexists with, not redefined here
- `specs/systems/flashlight-and-battery/005-shadow-casting-and-quality-fallback` — the single
  shadow-casting light this feature's fill deliberately does not duplicate
