# Feature Specification: Floor 52 Level Layout & Geometry

**Feature Branch**: `001-level-layout-and-geometry`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 3 (Floor 52 column — Learn role, no monster, wide corridors/few branches, 3–5
static batteries, hiding spot present), Ch. 8.1 (Floor 52 objective: survive & navigate, find
battery, find the exit — no key), Ch. 15.4 (onboarding beat table — this floor's content order),
Ch. 16.1 (fixed, hand-drawn map), Ch. 16.2 (blueprint template), Ch. 16.3 (level validation
checklist)

**Input**: User description: "Apply the GDD Ch. 16.2 blueprint template to Floor 52 (Learn), the
easiest and first floor of the run, omitting the Locked Door → Key Area section entirely because
Floor 52 has zero locked doors (GDD Ch. 8.1). Floor 52 has no monster physically present — only
distant SFX hints — wide corridors with few branches, 3–5 static (non-respawning) batteries, and
one hiding spot used to teach the mechanic. The level design itself IS the tutorial (GDD Ch. 15.4).
This spec owns the fixed, hand-authored geometry and area graph that every other Floor 52 content
spec (onboarding beat sequencing, battery/hiding placement, distant monster audio, exit/transition)
places its objects into."

## Why This Spec Exists

Floor 52 is the "Learn" floor (GDD Ch. 3): the first floor of the run, the only one with no
monster physically present, and the floor whose entire job is to teach the game's systems through
level design rather than through a tutorial popup (GDD Ch. 15: "Tidak ada tutorial popup panjang.
Floor 52 adalah tutorialnya, lewat level design."). Every other Floor 52 scene spec — onboarding
beat sequencing (002), static battery/hiding placement (003), the distant monster audio hint
(004), and the exit/transition (005) — places its content *into* a floor geometry that must
already exist, already be a single legible critical path, and already satisfy the GDD Ch. 16.3
validation checklist. This spec is that geometry: the fixed (non-procedural, GDD Ch. 16.1) area
graph for `Assets/Scenes/Floor52.unity`, expressed as content requirements (named areas, their
connectivity, and the spatial guarantees every downstream spec can rely on) rather than fabricated
coordinates.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Walk the Full Blueprint With No Locked Door or Key Area (Priority: P1)

A player entering Floor 52 moves through a Safe Area, into an Exploration Zone, through a single
long Chase Section, and reaches an Exit Door — with no Locked Door and no Key Area anywhere on the
floor, because Floor 52 has zero locked doors (GDD Ch. 8.1: "Tanpa key").

**Why this priority**: This is the GDD Ch. 16.2 blueprint applied to this specific floor's known
constraint (0 locked doors, per Ch. 17.5 `lockedDoorsFloor52 = 0`). Every downstream Floor 52 spec
places its content into this backbone; getting the backbone's shape wrong (e.g., accidentally
leaving room for a locked door) contradicts the floor's defining design decision.

**Independent Test**: Block out the Checkpoint, Safe Area, Exploration Zone, Chase Section, and
Exit Door Alcove in `Assets/Scenes/Floor52.unity` with placeholder geometry (no final art, no
functional `Door`/`Battery` components required yet), then walk `PlayerCharacter` from the
checkpoint to the exit alcove and confirm the path never requires a key or passes through a
locked-door object.

**Acceptance Scenarios**:

1. **Given** a player has just entered Floor 52 at the Checkpoint, **When** they walk forward
   through the floor's single critical path, **Then** they pass through exactly these areas in
   order — Safe Area, Exploration Zone, Chase Section, Exit Door Alcove — with no Locked Door or
   Key Area area anywhere in the graph.
2. **Given** the full area graph is reviewed, **When** it is compared against the generic GDD Ch.
   16.2 blueprint, **Then** it matches exactly except for the omission of the Locked
   Door → Key Area section, which this spec deliberately does not build (GDD Ch. 8.1: 0 locked
   doors on Floor 52).

---

### User Story 2 - Wide, Low-Branching Corridors That Still Offer One Real Choice (Priority: P1)

A player exploring Floor 52 experiences wide corridors with few branches (GDD Ch. 3 Floor 52
column: "Lorong lebar, sedikit percabangan") — this is not the same floor-feel as Floor 51 or
Floor 50's maze-like layouts — while the floor still provides at least one alternate route
somewhere in its Exploration Zone, satisfying the GDD Ch. 16.3 checklist's first item ("jangan
cuma satu jalur lurus").

**Why this priority**: This reconciles an apparent tension the task explicitly calls out: GDD Ch.
3 asks for *few* branches on Floor 52, while Ch. 16.3's checklist item 1 asks every floor to have
*at least one* alternate route. "Few" is not "zero" — this spec resolves that tension once, so
every downstream spec (and reviewer) can rely on a single, unambiguous answer instead of
re-deriving it. It is P1 because it is a structural decision, not a polish pass.

**Independent Test**: Walk the blocked-out Exploration Zone end to end and confirm (a) the overall
branching factor is visibly lower than a maze — most of the zone is a small number of wide,
direct corridors — while (b) at least one point offers a genuine second route between two already
-visited areas, satisfying Ch. 16.3 item 1 without turning Floor 52 into Floor 51.

**Acceptance Scenarios**:

1. **Given** the Exploration Zone's full layout, **When** its branch points are counted, **Then**
   the count is deliberately small (a handful, not "many"), consistent with GDD Ch. 3's "sedikit
   percabangan."
2. **Given** the same layout, **When** a player who has explored part of the zone tries to
   backtrack, **Then** at least one segment offers a route that is not the exact reverse of the
   path they used to arrive — the floor is never reducible to one single unbroken corridor from
   Checkpoint to Exit.
3. **Given** the required alternate route from Acceptance Scenario 2, **When** it is walked,
   **Then** it does not let the player skip ahead of any onboarding beat's content (see spec 002)
   — the alternate route is a short loop within the current beat's area, never a shortcut past a
   later beat.

---

### User Story 3 - Reserve Every Slot the Onboarding Beats and Content Specs Need (Priority: P1)

The Exploration Zone and Chase Section contain enough distinct, spatially separated locations for
every piece of downstream content this floor needs: 3–5 static Battery placements spread across
the map (not stacked on one side), exactly one hiding spot on the floor's single critical path, and
a plausible "distant" zone or direction from which the monster audio hint can be perceived as
coming from elsewhere in the building — all without any of those specs needing to alter this
spec's geometry to fit.

**Why this priority**: Specs 002–005 cannot place their content until this spec's geometry gives
them somewhere valid to place it. This is the geometry-side half of the contract those specs
depend on (ROADMAP.md §3, `004-floor-52-scene` row); it is P1 alongside User Stories 1–2 because a
backbone with no reserved content slots is not usable by any downstream spec.

**Independent Test**: Walk the blocked-out floor and identify, by inspection, at least 3 and at
most 5 distinct locations suitable for a static Battery, spread across more than one side of the
map; exactly one enclosed under-desk-sized nook along the unavoidable critical path suitable for a
hiding spot; and a directionally plausible "elsewhere in the building" zone (an unseen corridor,
a door to an unreachable area, or similar office-appropriate dressing) that a distant sound could
be perceived as coming from.

**Acceptance Scenarios**:

1. **Given** the blocked-out Exploration Zone, **When** candidate Battery locations are counted,
   **Then** between 3 and 5 distinct, non-adjacent locations exist, spread across more than one
   side of the map (GDD Ch. 16.3 checklist item 3; Ch. 17.2 `batteryCountFloor52`).
2. **Given** the same zone, **When** the floor's single critical path is walked start to end,
   **Then** exactly one under-desk-sized nook exists directly on that path — not down an optional
   side branch a player could choose to skip (GDD Ch. 15.4 Beat 4: "kolong meja ditempatkan di
   jalur yang pasti dilewati").
3. **Given** the floor's office-themed dressing, **When** a location for the distant monster audio
   hint (spec 004) is chosen, **Then** it reads spatially as "somewhere else in this building" —
   e.g., behind a door the player cannot open, down an unlit side corridor, or through a vent/duct
   — rather than requiring an actual `Monster` GameObject anywhere in the scene (GDD Ch. 3: "TIDAK
   ADA. Hanya SFX dari kejauhan").

---

### User Story 4 - No Dead-End Ever Reads as a Trap, Even Though Nothing Chases You Here (Priority: P3)

A player exploring any dead-end pocket of Floor 52 — a side alcove, the innermost point of a small
loop — never feels cornered or stuck, even though Floor 52 has no monster capable of actually
cornering them.

**Why this priority**: GDD Ch. 16.3's checklist item 2 ("Tidak ada dead-end yang bisa membuat
player terjebak tanpa jalan keluar saat dikejar") applies to every floor as a validation gate.
On Floor 52 specifically, the literal risk it guards against — being cornered mid-chase — cannot
occur, because Ch. 3 confirms this floor has no monster at all. This is P3, not P1 (contrast Floor
50's equivalent, which is P1), because the consequence of getting it wrong here is a design-hygiene
and future-proofing concern (a level layout that would fail this checklist the moment a monster
were ever added), not an active softlock risk for the shipped Floor 52.

**Independent Test**: Walk every dead-end pocket in the blocked-out floor (any side alcove, the
innermost point of the alternate-route loop from User Story 2) and confirm each either has a
second egress or is small/shallow enough that a player is never more than a few steps from the
main path — checked as a design-hygiene pass, not against an active monster.

**Acceptance Scenarios**:

1. **Given** the full area graph, **When** every dead-end pocket is listed, **Then** none of them
   requires more than a short backtrack to rejoin the main critical path.
2. **Given** the checklist item's literal wording ("saat dikejar" / "while chased"), **When** this
   spec's validation pass is recorded, **Then** it is explicitly noted as satisfied vacuously on
   Floor 52 (no monster exists to chase the player here) while still holding the layout to the
   same no-trap hygiene standard as every other floor (see Assumptions).

---

### User Story 5 - Finish in About Five Minutes on a First Try (Priority: P3)

A player who has never played Floor 52 before — and has never played LILO before — completes the
floor, checkpoint to exit door, in approximately five minutes, having learned every mechanic the
floor teaches along the way with zero external instruction (GDD Ch. 1.4 target floor duration; Ch.
16.3 checklist item 6).

**Why this priority**: A pacing validation outcome that depends on all of this floor's content
specs (002–005) being in place, not a structural prerequisite for the other stories — ordered
last, same as the equivalent story on Floor 50/51's layout specs.

**Independent Test**: Once specs 002–005 have placed their content into this geometry, have a
person with no prior exposure to LILO play a full run from the checkpoint to the exit door and
time it, with no verbal or written instruction beyond the pre-game How To Play screen's lives
count (GDD Ch. 15.4, Ch. 9.1).

**Acceptance Scenarios**:

1. **Given** a first-time player starts at the Floor 52 checkpoint, **When** they play using only
   what the level design has taught them, **Then** they reach the exit door in approximately five
   minutes (target `GameConfig.targetFloorDuration` = 300 seconds, GDD Ch. 17.5).

---

### User Story 6 - Never See Outside the Level (Priority: P3)

From any point a player can stand on Floor 52, looking in any direction never reveals empty space
beyond the level's built geometry (GDD Ch. 16.3 checklist item 7).

**Why this priority**: A visual-polish/immersion check, dependent on the camera's boundary clamp
(owned by `movement-and-camera/003-camera-follow-and-boundary-clamp`) and only fully verifiable
once the floor's outer boundary is fully blocked out — ordered last as a closing validation pass,
matching the equivalent story on the Floor 50 layout spec.

**Independent Test**: Walk the full perimeter of the completed blockout, including every dead-end
and the alternate-route loop's innermost point, and confirm no camera angle exposes area outside
the built floor geometry, citing
`specs/systems/movement-and-camera/003-camera-follow-and-boundary-clamp/spec.md`'s boundary-clamp
guarantee (that spec's SC-002/SC-006 validate the clamp mechanism itself; this story validates that
*this floor's* authored bounds actually match its built geometry).

**Acceptance Scenarios**:

1. **Given** the player stands at any reachable point in Floor 52, **When** the camera follows per
   its standard framing and boundary clamp, **Then** no unbuilt/empty space is visible at any edge
   of the frame.

---

### Edge Cases

- **"Few branches" vs. "at least one alternate route"**: resolved explicitly by User Story 2 — a
  small number of branch points, not zero. Downstream specs (002's beat sequencing in particular)
  MUST NOT treat this floor's single required alternate route as license to reorder or bypass
  onboarding beats (see User Story 2, Acceptance Scenario 3).
- **Ch. 16.3 checklist item 4 (monster patrol route through objective areas) has no literal
  referent on Floor 52**: because Ch. 3 states the monster is not physically present on this floor,
  there is no Monster Patrol Waypoint to check this item against. This spec instead reserves a
  directionally plausible "elsewhere in the building" zone for spec 004's distant audio hint (User
  Story 3, Acceptance Scenario 3) as the closest floor-appropriate equivalent, and the checklist
  Notes record this item as N/A-by-design rather than silently skipped (see checklist
  `requirements.md`).
- **A future revision adds a monster to Floor 52** (explicitly out of scope — GDD Ch. 3 and Ch. 6.3
  lock this as a Learn-floor property): this spec's dead-end hygiene pass (User Story 4) is
  deliberately kept to the same standard as chase-capable floors specifically so that such a future
  change would not immediately fail Ch. 16.3 item 2 — but this spec does not itself build for that
  hypothetical, per constitution Principle II (Simplicity & YAGNI).
- **Level bounds data is malformed or the floor is smaller than the camera's viewport on one axis**:
  owned entirely by `movement-and-camera/003-camera-follow-and-boundary-clamp`'s own FR-007;
  this spec's User Story 6 only requires that Floor 52's authored bounds match its built geometry,
  not that it re-implement the clamp's degenerate-input handling.
- **A player attempts to reach the Exit Door Alcove by leaving the critical path early (cutting
  through a dead-end pocket)**: the area graph MUST NOT expose a shortcut that skips the
  Exploration Zone or Chase Section entirely — the alternate route from User Story 2 stays local to
  the Exploration Zone, per the beat-order guarantee in Edge Case 1.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Assets/Scenes/Floor52.unity` MUST implement the GDD Ch. 16.2 blueprint order —
  Checkpoint/START → Safe Area → Exploration Zone → Chase Section → Exit Door (color-differentiated
  floor-exit door) — as its top-level area graph, **omitting the Locked Door → Key Area section
  entirely**, because Floor 52 has zero locked doors (GDD Ch. 8.1, Ch. 17.5
  `lockedDoorsFloor52 = 0`).
- **FR-002**: The Safe Area MUST be a threat-free space at floor start with full ambient light and
  no monster-related audio of any kind (see spec 004's Edge Cases), matching GDD Ch. 15.4 Beat 1's
  description ("Ruangan aman, tidak ada ancaman, cahaya masih penuh").
- **FR-003**: The Exploration Zone MUST have a visibly low branching factor overall (GDD Ch. 3:
  "sedikit percabangan") while still providing at least one genuine alternate route between two
  already-visited areas (GDD Ch. 16.3 checklist item 1) — reconciling these two requirements is
  this spec's explicit responsibility, not left to downstream specs (User Story 2).
- **FR-004**: The Exploration Zone MUST reserve between 3 and 5 distinct, spatially separated
  candidate locations for static Battery placement, spread across more than one side of the map
  (GDD Ch. 16.3 checklist item 3; Ch. 17.2 `batteryCountFloor52`), for spec 003 to place concrete
  `Battery` instances into.
- **FR-005**: The floor's single critical path (the route every player must walk regardless of
  which optional alternate route they take) MUST pass through exactly one under-desk-sized nook
  suitable for a hiding spot — never down an optional branch a player could choose never to visit
  (GDD Ch. 15.4 Beat 4), for spec 003 to place the concrete hiding spot into.
- **FR-006**: The area graph MUST include a directionally plausible "elsewhere in the building"
  zone or dressing element (an unopenable door, an unlit side corridor, a duct/vent, or similar
  office-appropriate set dressing) that spec 004's distant monster audio hint can be perceived as
  emanating from, without requiring an actual `Monster` GameObject to exist anywhere in
  `Assets/Scenes/Floor52.unity` (GDD Ch. 3, Ch. 6.3: monster is not physically present on this
  floor).
- **FR-007**: The Chase Section MUST be a single long corridor-style area (GDD Ch. 16.2: "lorong
  panjang, mendorong sprint"; Ch. 15.4 Beat 5) positioned as the floor's final segment before the
  Exit Door Alcove, free of side branches that could be mistaken for an escape route (there is
  nothing to escape from on this floor, but the corridor's shape still primes the sprint-encourage
  design intent carried forward to Floor 51/50).
- **FR-008**: The area graph MUST reserve a dedicated Exit Door Alcove at the end of the Chase
  Section; this spec reserves the space only — the concrete `Door` object, its distinct color
  feedback, and the floor-transition trigger are owned entirely by spec 005 (citing
  `specs/systems/keys-and-doors/004-door-visual-and-color-feedback/spec.md`).
- **FR-009**: This spec MUST NOT place, reserve space for, or reference any Locked Door or Key Area
  anywhere in the graph — Floor 52 has none (GDD Ch. 8.1, Ch. 17.5).
- **FR-010**: Every dead-end pocket in the area graph (any side alcove, the innermost point of the
  User Story 2 alternate-route loop) MUST either have a second egress or be shallow enough that no
  more than a short backtrack is ever required to rejoin the critical path — a design-hygiene
  standard applied even though no monster exists on this floor to enforce it through chase risk
  (GDD Ch. 16.3 checklist item 2; User Story 4).
- **FR-011**: The complete area graph MUST NOT allow any route that reaches Chase Section or Exit
  Door Alcove content before the Exploration Zone's onboarding content (spec 002's beats) has been
  encountered — the required alternate route (FR-003) stays local to the Exploration Zone and never
  shortcuts past a later beat.
- **FR-012**: The full built floor MUST contain no camera-visible point at which empty space
  outside the level geometry is revealed, at any reachable player position (GDD Ch. 16.3 checklist
  item 7), consistent with the boundary-clamp mechanism defined in
  `specs/systems/movement-and-camera/003-camera-follow-and-boundary-clamp/spec.md`.
- **FR-013**: The floor's geometry MUST be fixed and hand-authored, never procedurally generated,
  per GDD Ch. 16.1 ("Map FIXED, bukan procedural").
- **FR-014**: The complete area graph, once implemented, MUST be reviewable end to end against
  every item in the GDD Ch. 16.3 validation checklist as a single pass, with each item traceable to
  a specific FR in this spec or an explicitly named downstream spec, and with item 4 (monster
  patrol route) explicitly recorded as N/A-by-design per this spec's Edge Cases (see checklist
  Notes).

### Key Entities

- **Area**: A named region of the floor's fixed geometry. This spec defines exactly these Areas:
  `Checkpoint` (floor entry/respawn point, GDD Ch. 9.1), `SafeArea`, `ExplorationZone`
  (containing the Battery candidate slots, the hiding-spot nook, and the required alternate-route
  loop), `DistantZone` (the off-limits/unseen dressing element spec 004 anchors its audio hint to),
  `ChaseSection`, and `ExitDoorAlcove`. Areas are a design/documentation concept for this spec's
  content requirements, not necessarily a distinct `MonoBehaviour` type — later specs may implement
  spawn points, triggers, and the exit `Door` as children of these Areas' scene hierarchy without
  this spec mandating a specific component.
- **Battery / Door**: Referenced structurally (the Exploration Zone reserves Battery slots; the
  Exit Door Alcove reserves the exit door's space) but defined and placed by specs 003 and 005
  respectively, not this spec.
- **Hiding Spot**: This spec only reserves the one under-desk-sized nook on the critical path
  (FR-005); concrete placement is owned by spec 003.
- **Monster**: Explicitly absent from this floor's scene graph (GDD Ch. 3, Ch. 6.3). Referenced
  only as the reason FR-006/FR-010 are framed the way they are — this spec never instantiates one.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A level-design walkthrough of the completed blockout confirms the area graph matches
  the GDD Ch. 16.2 blueprint with the Locked Door → Key Area section omitted, and contains zero
  Locked Door or Key Area elements.
- **SC-002**: A level-design walkthrough confirms the Exploration Zone's branch-point count is
  visibly low (a handful) while at least one genuine alternate route exists between two
  already-visited areas, and that route never bypasses a later onboarding beat's content.
- **SC-003**: A level-design walkthrough confirms between 3 and 5 spatially separated Battery
  candidate locations exist, spread across more than one side of the map, and exactly one
  under-desk-sized nook exists directly on the floor's critical path.
- **SC-004**: A level-design walkthrough confirms zero dead-end pockets exist that require more
  than a short backtrack to rejoin the critical path.
- **SC-005**: A first-time playtester (per User Story 5) completes checkpoint-to-exit-door in a
  time the team judges consistent with the ±5-minute target (`GameConfig.targetFloorDuration` =
  300s), recorded as a playtest observation rather than an automated pass/fail (feel-driven pacing
  content, per ROADMAP.md §0).
- **SC-006**: A full perimeter walkthrough (User Story 6) finds zero camera angles exposing space
  outside the built level.
- **SC-007**: Every one of the seven GDD Ch. 16.3 checklist items is traceable to a specific FR in
  this spec, to a named downstream spec (002/003/004/005), or is explicitly recorded as N/A-by-
  design (item 4), with none left unaddressed.

## Assumptions

- **Loop shape for the required alternate route (User Story 2)**: implemented as a single short
  loop within the Exploration Zone (e.g., two parallel corridors around one central room), not a
  second independent path across the whole floor — this keeps the "few branches" feel intact while
  still satisfying Ch. 16.3 item 1. If a future design pass wants a larger branching structure, it
  must re-open this spec's topology decision rather than being layered on silently by a downstream
  spec.
- **Ch. 16.3 checklist item 4 is treated as N/A-by-design for Floor 52**, recorded explicitly rather
  than silently dropped, because the item's literal subject (a Monster Patrol Waypoint) does not
  exist on this floor by GDD design (Ch. 3, Ch. 6.3). The `DistantZone` reservation (FR-006) is
  this spec's best-effort structural equivalent, feeding spec 004.
- **Dead-end hygiene (User Story 4, FR-010) is applied as a standard, not because an active monster
  requires it on Floor 52 today** — this is a deliberate, low-cost consistency choice (constitution
  Principle II still permits it because it costs nothing extra to hold the same layout standard
  the team already uses on every other floor) rather than new speculative infrastructure.
- This spec does not fabricate exact coordinates, room dimensions, or a floor plan image — per
  ROADMAP.md §0 and the constitution's Principle I (specs describe observable behavior, not
  implementation), the actual blockout geometry is authored directly in
  `Assets/Scenes/Floor52.unity` by the level designer against these content requirements, and is
  validated by playtesting/review, not by a fabricated coordinate acceptance test.
- Floor 52's checkpoint-to-exit duration target (~5 minutes) is the same
  `GameConfig.targetFloorDuration` value (300s) shared across all three floors (GDD Ch. 17.5) — this
  spec does not introduce a Floor-52-specific timing config field.

## Related

- [[ROADMAP]] — scenes §3, `004-floor-52-scene` row 1 (`001-level-layout-and-geometry`); no systems
  dependency (matches the equivalent row on Floor 51/50), §5 dependency-ordered build sequence
- [[constitution]] — Principle I (spec before implementation), Principle II (fixed/manual map, no
  speculative procedural-generation framework; no monster-hygiene infrastructure built beyond what
  this floor needs today)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3 (Floor 52 column: Learn role, no monster, wide/low-branch
  corridors, 3–5 static batteries, hiding spot present), Ch. 8.1 (0 locked doors, no-key objective),
  Ch. 15.4 (onboarding beat table, consumed by spec 002), Ch. 16.1 (fixed, hand-drawn map), Ch. 16.2
  (blueprint template), Ch. 16.3 (validation checklist)
- `specs/systems/movement-and-camera/003-camera-follow-and-boundary-clamp/spec.md` — the camera
  boundary-clamp mechanism this spec's User Story 6 / FR-012 relies on; that spec owns the clamp
  math, this spec owns making Floor 52's authored bounds match its built geometry
- `specs/scenes/004-floor-52-scene/002-onboarding-beat-sequencing` — sequences its five teaching
  beats into this spec's Checkpoint → Safe Area → Exploration Zone → Chase Section order
- `specs/scenes/004-floor-52-scene/003-static-battery-and-hiding-placement` — places concrete
  `Battery` instances into this spec's reserved Exploration Zone slots (FR-004) and the concrete
  hiding spot into this spec's reserved nook (FR-005)
- `specs/scenes/004-floor-52-scene/004-distant-monster-hint-audio` — anchors its audio hint to this
  spec's `DistantZone` reservation (FR-006)
- `specs/scenes/004-floor-52-scene/005-floor-exit-and-transition` — places the concrete Exit `Door`
  and its transition trigger into this spec's `ExitDoorAlcove` reservation (FR-008)
