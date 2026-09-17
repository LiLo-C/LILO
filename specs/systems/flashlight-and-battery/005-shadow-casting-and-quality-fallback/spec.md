# Feature Specification: Shadow Casting & Quality Fallback

**Feature Branch**: `005-shadow-casting-and-quality-fallback`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Real-time shadow casting from the one flashlight light, toggleable
off and quality-reducible from config alone as a performance fallback — this is the agreed
fallback per GDD Ch. 12/11.2 amendment."

## Why This Spec Exists

GDD Ch. 12 states plainly: "Dinding dan furniture solid melempar real-time shadow (URP shadow map)
yang ikut bergerak saat player bergerak." This is what makes the flashlight read as a real light
in a real 3D room rather than a flat brightness mask — walls and furniture must actually occlude
it. But Ch. 11.2 (amended v2.1) flags this as the phase's primary technical risk: "Overhead
performa real-time shadow (URP) di device target — harus diukur langsung di iPhone... Ukuran
shadow map / kualitas bayangan pada light omnidirectional — nilai terlalu besar historically
membuat frame over-expose... ini WAJIB bisa diturunkan lewat config sebagai fallback performa,
bukan lewat kode," and the chapter's own agreed fallback is explicit: "matikan shadow
(`shadowsEnabled = false` di config) sambil tetap menjaga keempat Light State tetap bisa
dibedakan tanpa bayangan." This feature owns exactly that: real shadows from the one flashlight
light, and the config-only lever(s) to reduce or remove them without touching code.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Walls and Furniture Cast Real-Time Shadows From the Flashlight (Priority: P1)

As the player moves through a room, solid walls and furniture block the flashlight's light and
cast shadows that move and update as the player and light source move, so the room reads as one
real, occluding 3D space rather than a flat lit floor plan.

**Why this priority**: This is the visual payoff the GDD's whole lighting rewrite (Ch. 11.1/12
v2.1 amendment) exists for — a single Unity light illuminating and shadowing everything together.
Every other feature in this category benefits from it existing, but nothing else in this category
requires it to function; it is the top priority because it's the feature's entire reason to
exist, not because anything downstream is blocked without it.

**Independent Test**: Place a solid object (e.g. a desk) between the player's flashlight and a
wall/floor area, move the player, and confirm a shadow is cast behind the object from the
flashlight's current position, and that the shadow's position updates as the player moves — fully
observable in a running scene with shadows enabled, no code change required to verify.

**Acceptance Scenarios**:

1. **Given** shadows are enabled and a solid object sits between the flashlight and the floor
   behind it, **When** the scene is lit, **Then** a shadow appears on the floor/wall behind that
   object, cast from the flashlight's light.
2. **Given** shadows are enabled, **When** the player moves (changing the flashlight's position),
   **Then** the shadow's position updates continuously to match — it is not a static, baked
   shadow.
3. **Given** the flashlight is the only light source affecting the world (per Ch. 12's single-light
   design), **When** shadows render, **Then** they are produced by that one light — no separate,
   independently-authored shadow-casting light exists anywhere in the scene.

---

### User Story 2 - Shadows Can Be Disabled Entirely From Config Alone (Priority: P2)

If real-time shadows prove too expensive on target hardware, a developer can turn them off by
changing a single value in `GameConfig` — no code change, no re-authoring of lights or materials
— and the four Light States (Normal/Flickering/Critical/Compact Darkness) must remain visibly
distinguishable from each other even with shadows off.

**Why this priority**: GDD Ch. 11.2's own agreed fallback is exactly this switch. It is P2, not
P1, because it is a fallback for a risk, not the default experience — but it must exist and work
correctly before this feature can be considered shippable, since the GDD treats it as
non-optional insurance, not a nice-to-have.

**Independent Test**: Set `GameConfig.shadowsEnabled` to `false` with no other change, run the
scene, and confirm no shadows render while the light still illuminates the room and all four Light
States remain visually distinguishable from each other.

**Acceptance Scenarios**:

1. **Given** `GameConfig.shadowsEnabled` is `false`, **When** the scene renders, **Then** no
   object casts a real-time shadow, and this required no change to any script or material asset —
   only the config value.
2. **Given** `GameConfig.shadowsEnabled` is `false`, **When** the flashlight cycles through all
   four Light States (via a shortened test battery duration), **Then** each state remains
   visually distinguishable from the others by radius and/or flicker alone (per `001`, `003`,
   `004`), confirming the fallback does not silently break the game's core signal.
3. **Given** `GameConfig.shadowsEnabled` is `true` again after having been `false`, **When** the
   scene renders, **Then** shadows resume with no other change required.

---

### User Story 3 - Shadow Quality Can Be Reduced From Config Alone Without Disabling Shadows (Priority: P3)

If shadows are affordable but need to be cheaper (rather than removed entirely), a developer can
lower the shadow resolution/quality via a single `GameConfig` value, trading visual crispness for
performance, without disabling shadows outright.

**Why this priority**: GDD Ch. 11.2 explicitly separates this from the on/off switch: "Ukuran
shadow map / kualitas bayangan... WAJIB bisa diturunkan lewat config sebagai fallback performa."
It's P3 because it's a secondary, finer-grained lever than User Story 2's binary switch, useful
only once the team already knows shadows are affordable but want them cheaper.

**Independent Test**: Change only `GameConfig`'s shadow-quality value to a lower setting, run the
scene, and confirm shadows still render (still satisfying User Story 1) but at visibly reduced
resolution/softness, with no other file changed.

**Acceptance Scenarios**:

1. **Given** `GameConfig.shadowsEnabled` is `true`, **When** the shadow-resolution config value
   is lowered, **Then** shadows continue to render (not disabled) but at visibly coarser
   resolution, with the change requiring no code or material edit.
2. **Given** the shadow-resolution config value is set unreasonably high (the specific historical
   failure this chapter warns about), **When** the scene renders, **Then** the system does not
   silently produce an over-exposed/blown-out frame as a result of this feature's own shadow
   settings — the config value itself is the lever to correct that, and the shipped default MUST
   NOT be a value already known to cause it (GDD Ch. 11.2/17 tuning notes).

---

### Edge Cases

- **`GameConfig.shadowsEnabled` toggled at runtime** (not just at scene load, e.g. a debug menu):
  the change MUST take effect without requiring a scene reload or object re-creation — it is a
  live property read, not a one-time initialization value baked into a prefab.
- **A very large single-frame `deltaTime`** or a paused frame: this feature has no timing-based
  logic of its own (shadows are a per-frame render state, not a timed effect) — nothing here needs
  to account for elapsed time; it reacts only to config values and current transforms.
- **No solid shadow-casting geometry exists yet in a scene** (e.g. the earliest test scenes before
  level art lands): shadow casting being enabled with nothing to cast a shadow onto/from MUST NOT
  error — it simply renders no visible shadows because there's nothing to show.
- **Multiple lights accidentally added to a scene** (a level-design or scene-setup mistake, not
  this feature's own design): this feature's contract is that the *flashlight* light casts
  shadows per this spec; a second, unrelated light casting its own shadows is a scene-authoring
  error outside this feature's runtime responsibility to detect or prevent.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST cast real-time shadows from the single flashlight light (the same
  light whose radius `001`/`003`/`004` control) onto solid walls and furniture, updating every
  frame as the player (and therefore the light) moves. No separate, independently-lit shadow
  effect may exist (GDD Ch. 12).
- **FR-002**: `GameConfig` MUST expose `shadowsEnabled` (bool, default `true`) as the single
  switch controlling whether the flashlight casts shadows at all. Setting it to `false` MUST
  disable shadow casting with no other file requiring a change (GDD Ch. 11.2's agreed fallback).
- **FR-003**: With `shadowsEnabled` set to `false`, all four Light States from
  `001-light-state-thresholds-and-radius` MUST remain visually distinguishable from each other by
  radius and/or flicker alone — this feature MUST NOT be a load-bearing part of state
  legibility, only an additional visual enhancement on top of it.
- **FR-004**: `GameConfig` MUST expose a shadow quality/resolution value (independent of the
  on/off switch) that reduces shadow rendering cost while shadows remain enabled — a coarser,
  cheaper shadow, not a binary alternative to FR-002's switch. The GDD does not lock an exact
  resolution number — it is a performance-tuning value validated on target hardware. A starting
  default of `512` (pixels, square shadow map) is carried over from this project's earlier
  native-engine on-device tuning pass (ROADMAP §0) **to re-confirm on-device in Unity/URP** — the
  prior engine's own notes explicitly warn that a too-large value for an omnidirectional/point
  light historically caused the frame to over-expose, so this default MUST NOT regress to a value
  already known to cause that, even though the concrete failure was observed on a different
  render pipeline and must be re-verified under URP specifically.
- **FR-005**: Every shadow-related tunable this feature introduces MUST live in `GameConfig`, with
  no shadow setting hardcoded on a per-object/per-material basis outside it (constitution
  Principle III/V).
- **FR-006**: Changing `shadowsEnabled` MUST take effect as a live, per-frame-read property — not
  a one-time value baked in at scene load that requires a reload to change.
- **FR-007**: Any portion of this feature expressible as pure logic (e.g. resolving a config
  quality value into the concrete setting applied to the light) MUST live in a plain C# class
  under `Assets/Scripts/Systems/` per constitution Principle III; the part that actually assigns
  Unity `Light`/rendering properties is a thin adapter under `Assets/Scripts/MonoBehaviours/`.

### Key Entities

- **GameConfig** *(extended)*: Gains `shadowsEnabled` and a shadow quality/resolution value (e.g.
  `shadowResolution`).
- **Flashlight Light**: The same Unity light object whose radius is controlled by `001`/`003`/
  `004`; this feature only adds shadow-casting behavior to it, it does not introduce a second
  light.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With shadows enabled and a solid object placed between the flashlight and a
  surface, a visible shadow appears and its position updates within the same frame the player's
  position changes, observed across a full walk-through of a test room.
- **SC-002**: Toggling `GameConfig.shadowsEnabled` between `true` and `false` and re-running the
  scene requires editing exactly one file (`GameConfig`) to see the difference, with shadows
  present in the `true` case and absent in the `false` case.
- **SC-003**: With `shadowsEnabled` set to `false`, at least 2 people outside the development team
  can still correctly distinguish all four Light States from each other in an observation test,
  matching the same bar `001`'s SC-004 sets with shadows on.
- **SC-004**: Lowering the shadow-resolution config value produces a measurable frame-rate
  improvement (or at minimum, no regression) on target hardware profiling, without disabling
  shadows.
- **SC-005**: On target hardware (physical device, not Editor/simulator, per GDD Ch. 11.2), the
  scene sustains the project's target frame rate with shadows enabled at the shipped default
  resolution; if it does not, the config-only fallback (FR-002/FR-004) is exercised and re-tested
  rather than a code change being made.

## Assumptions

- The exact Unity/URP mechanism used to implement per-light real-time shadows (shadow type,
  resolution enum vs. raw pixel value, soft-shadow sampling) is a planning-time decision, not
  locked by this spec — this spec locks the *behavior* (shadows exist, are toggleable and
  quality-reducible from config alone) per the category's feel-value handling rule.
- On-device performance measurement (GDD Ch. 11.2, Ch. 20.2) is a manual/on-device validation
  step per constitution Principle IV's exception for anything needing a live scene and real
  hardware — it is not itself an EditMode-testable rule.
- No changes to `001`, `002`, `003`, or `004`'s own logic are made by this feature — it is purely
  additive (shadow casting on top of an existing light), consistent with the category's scope
  note that this feature only adds shadow casting and its fallback.

## Related

- [[ROADMAP]] — flashlight-and-battery row 5; depends on `001-light-state-thresholds-and-radius`
- [[constitution]] — Principle III (plain C# systems / thin adapters), Principle IV (EditMode
  tests where expressible, on-device validation otherwise)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 11.2 (technical risk, agreed fallback), Ch. 12 (real-time
  shadow requirement), Ch. 17.2 (GameConfig keys)
- `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius` — the Light States
  this feature's fallback must not break legibility of
