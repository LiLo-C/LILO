# Feature Specification: Floor 51 Level Layout & Geometry

**Feature Branch**: `001-level-layout-and-geometry`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Apply the GDD Ch. 16.2 blueprint template to Floor 51 (Pressure),
LILO's middle floor — one Locked Door → Key Area loop, an Exploration Zone deliberately built with
many intersections (GDD Ch. 3 Floor 51 column: 'Banyak persimpangan'), and a monster that is active
from floor start (patrol + investigate + normal-speed chase, GDD Ch. 3/6). This spec owns the
fixed, hand-drawn (non-procedural) geometry and area graph for `Assets/Scenes/Floor51.unity` that
every other Floor 51 content spec (monster patrol/spawn, key/door, battery spawns, hiding spot,
exit/transition) places its objects into."

## Why This Spec Exists

Floor 51 is the "Pressure" floor (GDD Ch. 3): the monster is active for the first time in the run
(Floor 52 has none), the objective introduces the game's first locked door, and the layout style is
explicitly "many intersections" rather than Floor 52's wide, low-branching corridors or Floor 50's
dead-end-heavy narrow routes. Every other Floor 51 scene spec — monster patrol/spawn presets (002),
the key/locked-door pair (003), battery spawn points (004), the hiding spot (005), and the floor
exit/transition (006) — places its objects *into* a floor geometry that must already exist and
already satisfy the GDD Ch. 16.3 validation checklist. This spec is that geometry: the fixed area
graph for `Assets/Scenes/Floor51.unity`, expressed as content requirements (named areas, their
connectivity, and the spatial guarantees every downstream spec can rely on) rather than fabricated
coordinates. Getting the area graph and its escape-route guarantees wrong here means every
downstream spec inherits an unsafe or unsatisfiable floor — and because this is the first floor
where a monster can actually catch the player, that risk is no longer theoretical the way it was on
Floor 52.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reach the Key Without Ever Needing the Locked Door First (Priority: P1)

A player entering Floor 51's Exploration Zone can reach the Key Area directly, through the many
intersections that make up this floor's exploration space, without ever needing to pass through or
open the floor's one Locked Door first — the key is always obtainable before the door it opens is
even attempted.

**Why this priority**: This is the structural backbone every downstream Floor 51 spec depends on.
If the Key Area were only reachable *through* the Locked Door, the floor would be unsolvable from
the very first attempt — a hard soft-lock, not a difficulty choice. Get this ordering right in the
geometry before spec 003 places the actual `Key`/`Door` objects into it.

**Independent Test**: Block out the Exploration Zone, the Key Area, and the Locked Door Approach in
`Assets/Scenes/Floor51.unity` using placeholder geometry (no final art, no functional `Door`/`Key`
components required yet), then walk `PlayerCharacter` from the floor checkpoint to the Key Area and
confirm it is reachable without passing through the Locked Door Approach's gated boundary.

**Acceptance Scenarios**:

1. **Given** the player has just entered the Exploration Zone from the Safe Area, **When** they
   head toward the Key Area, **Then** they reach it via a route that never requires crossing the
   Locked Door's position.
2. **Given** the player has picked up the key at the Key Area, **When** they backtrack toward the
   Locked Door Approach, **Then** at least one of the many intersecting routes in the Exploration
   Zone gets them there without retracing the exact path they used to reach the Key Area.

---

### User Story 2 - Never Become Inescapably Trapped While Chased (Priority: P1)

**This is a non-negotiable requirement for this floor, and it is more critical here than on Floor
52 because Floor 51 is the first floor where a real, active `Monster` can catch the player.** Every
dead-end or narrow sub-area the floor contains — even though Floor 51's layout style favors
intersections over dead-ends (GDD Ch. 3) — MUST still give a player being actively chased by
`Monster` a way out that does not require passing back through the monster's current blocking
position.

**Why this priority**: GDD Ch. 16.3's second checklist item ("Tidak ada dead-end yang bisa membuat
player terjebak tanpa jalan keluar saat dikejar") applies to every floor, but Floor 51 is the first
floor it can actually be violated in a way that costs the player a life — Floor 52 has no monster to
chase them into a corner. Treated as a P1 blocking requirement, called out on its own rather than
folded into the general validation pass in User Story 4.

**Independent Test**: With placeholder blockout geometry in place and `Monster` either absent or
represented by a simple chase-simulating stand-in, walk every narrow or single-approach sub-area in
the floor (the Key Area's innermost point, the Locked Door Approach, any narrow intersection spur,
the Chase Section's margins) and confirm each one has either (a) a second egress distinct from its
entrance, or (b) is explicitly documented in this spec's Assumptions as an intentional
single-entrance area whose distance from every Monster Patrol Waypoint (spec 002) makes it
implausible for a chase to corner a player there.

**Acceptance Scenarios**:

1. **Given** a player is standing at the innermost point of the Key Area and `Monster` enters CHASE
   state and approaches, **When** the player attempts to leave, **Then** a second route out of the
   Key Area exists that does not require passing the position `Monster` currently occupies.
2. **Given** a player is anywhere inside the Chase Section (the corridor leading to the Floor Exit
   Door), **When** they are being chased, **Then** the Chase Section's margins contain no unmarked
   single-entrance alcove that a fleeing player could mistake for an escape route and become
   trapped in.
3. **Given** the full area graph is reviewed end to end, **When** every sub-area is checked against
   Monster Patrol Waypoints and Spawn Presets from spec 002, **Then** zero sub-areas are found
   where the only physical egress coincides with a waypoint or connecting corridor `Monster` is
   known to occupy during PATROL, INVESTIGATE, or CHASE.

---

### User Story 3 - Explore an Intersection-Heavy Zone That Still Funnels to One Locked Door and Chase Section (Priority: P2)

A player experiences an Exploration Zone genuinely built around branching intersections (GDD Ch. 3:
"Banyak persimpangan" — many intersections), not a single corridor with a side room, and that zone
still funnels into exactly one Locked Door Approach, one Key Area, and — once the door is unlocked —
one shared Chase Section leading to the Floor Exit Door.

**Why this priority**: This is Floor 51's headline structural identity versus Floor 52 (wide,
low-branching corridors, zero locked doors) and Floor 50 (dead-end-heavy, three locked doors). It is
P2 rather than P1 because the concrete `Key`/`Door` objects and the exit trigger are out of this
spec's scope (owned by 003 and 006) — this spec only owns that the *geometry* for an
intersection-rich zone exists and converges correctly.

**Independent Test**: Block out the Exploration Zone with at least three distinct intersections
(points where three or more corridors meet), then walk `PlayerCharacter` from the Checkpoint to the
Key Area and to the Locked Door Approach by at least two different routes each, confirming both
converge into the same Chase Section once the (placeholder) door is open.

**Acceptance Scenarios**:

1. **Given** the Exploration Zone's blockout, **When** its intersections are counted, **Then** at
   least three points exist where three or more distinct corridors meet.
2. **Given** the Locked Door has been unlocked (per spec 003), **When** the player walks from either
   the Key Area or the Locked Door Approach toward the floor's remaining objective, **Then** both
   routes lead into the same single Chase Section — this spec does not build two parallel chase
   corridors.

---

### User Story 4 - Complete the Floor in About Five Minutes on a First Try (Priority: P3)

A player who has never played Floor 51 before can complete it — checkpoint to Floor Exit Door — in
approximately five minutes (GDD Ch. 1.4 target floor duration, Ch. 16.3 checklist item 6).

**Why this priority**: Pacing validation is a review/playtest outcome, not a structural
prerequisite for the other user stories — it can only be confirmed once the other Floor 51 specs'
content (monster, key/door, batteries, hiding spot) is placed into this geometry, so it is ordered
last.

**Independent Test**: Once all six Floor 51 specs are implemented, have a person with no prior
exposure to this floor play a full run from the checkpoint to the Floor Exit Door and time it.

**Acceptance Scenarios**:

1. **Given** a first-time player starts at the Floor 51 checkpoint, **When** they play without
   external guidance, **Then** they reach the Floor Exit Door in approximately five minutes (target
   `GameConfig.targetFloorDuration` = 300 seconds, GDD Ch. 17.5).

---

### User Story 5 - Never See Outside the Level (Priority: P3)

From any point a player can stand in Floor 51, looking in any direction never reveals empty space
beyond the level's built geometry (GDD Ch. 16.3 checklist item 7).

**Why this priority**: A visual-polish/immersion check, dependent on the camera's boundary clamp
(owned by `movement-and-camera/003-camera-follow-and-boundary-clamp`) and only fully verifiable once
the floor's outer boundary is fully blocked out — ordered last as a closing validation pass.

**Independent Test**: Walk the full perimeter of the completed blockout, including every
intersection spur and the Key Area's innermost point, and confirm no camera angle exposes area
outside the built floor geometry.

**Acceptance Scenarios**:

1. **Given** the player stands at any reachable point in Floor 51, **When** the camera follows per
   its standard framing, **Then** no unbuilt/empty space is visible at any edge of the frame.

---

### Edge Cases

- **Key-before-door ordering**: this spec does not gate the Key Area behind the Locked Door (User
  Story 1) — the Exploration Zone geometry MUST present the Key Area as reachable without first
  passing the Locked Door. Spec 003 inherits this and MUST NOT introduce an item order-dependency
  that contradicts it without re-opening this spec's topology decision (see Assumptions).
- **Trapped in a dead-end while chased**: addressed as its own P1 story (User Story 2) rather than
  left implicit in the general validation pass, because Floor 51 is the first floor where this can
  actually happen to a player — it is the single highest-risk edge case this spec must close before
  any other Floor 51 content is considered safe to build on top of it.
- **Locked Door reachability if the key pickup is missed on a first pass**: because a `Key` is never
  consumed or lost once picked up and a `Door` once unlocked stays unlocked for the remainder of the
  floor attempt (GDD Ch. 8.1 objective model; reset-on-death is a full floor reset per GDD Ch. 9.2,
  not a partial one), there is no reachable state where a player has "missed" the key permanently
  within a single floor attempt — the Key Area remains open and its `Key` remains collectible until
  the Locked Door is unlocked.
- **Player revisits the Key Area or Locked Door Approach after unlocking the door**: the geometry
  MUST NOT block backtracking into either area purely because the Chase Section is now reachable
  (e.g., to reach a Battery Spawn Point placed there by spec 004) — unlocking only adds a new route,
  it does not remove existing ones.
- **Intersection count vs. dead-end risk**: an intersection-heavy layout (User Story 3) is not
  automatically safer than a dead-end-heavy one — a corridor spur off an intersection that leads
  nowhere is exactly as dangerous as a Floor-50-style dead-end if it has only one egress. This spec
  treats every corridor spur the same as a dead-end for the purposes of User Story 2's audit,
  regardless of how many intersections lead into it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Assets/Scenes/Floor51.unity` MUST implement the GDD Ch. 16.2 blueprint order —
  Checkpoint/START → Safe Area → Exploration Zone → Locked Door Approach ↔ Key Area (both reachable
  directly from the Exploration Zone, neither gated behind the other) → Chase Section → Floor Exit
  Door — as its top-level area graph. This is Floor 51's floor-specific instantiation of the generic
  blueprint; it does not redefine the blueprint itself.
- **FR-002**: The Safe Area MUST guarantee no `Monster` presence at floor start not by disabling the
  monster (GDD Ch. 6.3: the monster always exists on the map from floor start on Floor 51/50, it
  never "appears out of nowhere") but by geometry alone — the Safe Area MUST be far enough from, and
  outside the initial sightline of, every valid Monster Spawn Preset defined by spec 002, per the
  spawn-point validity rules in `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`.
- **FR-003**: The Exploration Zone MUST connect to both the Key Area and the Locked Door Approach
  directly, with neither gated behind the other — satisfying the order-independence decision
  recorded in this spec's Assumptions and consumed by spec 003.
- **FR-004**: The Exploration Zone MUST contain at least three distinct intersections (points where
  three or more corridors meet), matching GDD Ch. 3's "Banyak persimpangan" layout style for this
  floor and providing the alternate-route guarantee required by GDD Ch. 16.3 checklist item 1 — no
  portion of the explorable floor (excluding the Key Area's innermost point, which is intentionally
  single-entrance per FR-005) may be reachable by exactly one corridor with no branch.
- **FR-005**: Every dead-end or single-entrance sub-area in the floor — including the Key Area's
  innermost point, the Locked Door Approach's innermost point, and any narrow intersection spur —
  MUST either (a) provide a second, distinct egress, or (b) be explicitly listed in this spec's
  Assumptions as an intentional single-entrance dead-end whose distance from every Monster Patrol
  Waypoint/Spawn Preset (spec 002) makes it implausible for a chase to corner a player there. This
  is a non-negotiable, floor-wide gate — no dead-end may ship without satisfying (a) or being
  explicitly justified under (b) (GDD Ch. 16.3 checklist item 2; see User Story 2).
- **FR-006**: The Chase Section MUST be a single corridor-style area (GDD Ch. 16.2: "lorong panjang,
  mendorong sprint") positioned after the Locked Door is unlocked, and MUST NOT contain any unmarked
  single-entrance alcove along its margins that could be mistaken for an escape route during a chase
  (User Story 2, Acceptance Scenario 2).
- **FR-007**: The route from the Key Area and the Locked Door Approach into the Chase Section MUST
  have exactly one lock on it — the Locked Door itself (owned by spec 003, citing
  `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md`). This spec MUST NOT place any
  additional physical barrier or redundant gate on that route (constitution Principle II).
- **FR-008**: The Floor Exit Door's area (its immediate approach and alcove at the end of the Chase
  Section) is reserved by this spec's area graph but its concrete placement, color feedback, and
  transition trigger are owned entirely by spec 006 — this spec only guarantees the Chase Section
  terminates in a dedicated Floor Exit Alcove, not a shared/ambiguous space.
- **FR-009**: The area graph MUST reserve spatially distinct locations across the Exploration Zone,
  Key Area, and Chase Section suitable for Battery Spawn Point placement (spec 004) such that
  spread-out placement (GDD Ch. 16.3 checklist item 3) is achievable without spec 004 needing to
  alter this spec's geometry.
- **FR-010**: The area graph MUST reserve at least two office-appropriate enclosed nooks
  (under-desk-sized spaces, GDD Ch. 4.3) in spatially distinct areas along plausible traversal
  paths, for Hiding Spot placement by spec 005 — Floor 51 has a hiding spot present (GDD Ch. 3 Floor
  51 column), and this spec reserves more than one slot so spec 005 can place hiding spots fairly
  across the floor rather than clustering them near a single chase route.
- **FR-011**: The Exploration Zone, Key Area, and Locked Door Approach MUST be laid out so that
  Monster Patrol Waypoints (spec 002) can pass through objective-adjacent areas without idling
  directly on top of the Key, the Locked Door, or the Floor Exit Door (GDD Ch. 16.3 checklist item
  4) — this spec provides the geometry that makes such a route possible; spec 002 owns the actual
  waypoint placement.
- **FR-012**: The full built floor MUST contain no camera-visible point at which empty space outside
  the level geometry is revealed, at any reachable player position (GDD Ch. 16.3 checklist item 7).
- **FR-013**: The floor's geometry MUST be fixed and hand-authored, never procedurally generated,
  per GDD Ch. 16.1 ("Map FIXED, bukan procedural").
- **FR-014**: The complete area graph, once implemented, MUST be reviewable end to end against every
  item in the GDD Ch. 16.3 validation checklist as a single pass, with each item traceable to a
  specific FR in this spec or an explicitly named downstream spec (see checklist Notes).

### Key Entities

- **Area**: A named region of the floor's fixed geometry. This spec defines exactly these Areas:
  `Checkpoint` (floor entry/respawn point, GDD Ch. 9.1), `SafeArea`, `ExplorationZone` (the
  intersection-heavy hub, FR-004), `KeyArea`, `LockedDoorApproach` (the door's blocking position,
  gating the route onward), `ChaseSection`, and `FloorExitAlcove`. Areas are a design/documentation
  concept for this spec's content requirements, not necessarily a distinct `MonoBehaviour` type —
  later specs may implement waypoints, spawn points, and triggers as children of these Areas' scene
  hierarchy without this spec mandating a specific component.
- **Door / Key**: Referenced structurally (one Locked Door gates the Chase Section; the Floor Exit
  Door ends it) but defined and placed by specs 003 and 006, not this spec.
- **Monster / Monster Spawn Preset / Monster Patrol Waypoint**: Referenced structurally (Safe Area
  must sit outside their reach, dead-ends must sit outside plausible chase-cornering distance) but
  defined and placed by spec 002, not this spec.
- **Battery Spawn Point / Hiding Spot**: This spec only reserves spatial slots for them (FR-009,
  FR-010); concrete placement is owned by specs 004 and 005 respectively.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A level-design walkthrough of the completed blockout confirms the Key Area is
  reachable from the Exploration Zone without ever crossing the Locked Door's position, in 100% of
  route attempts.
- **SC-002**: A level-design walkthrough confirms zero dead-end or single-entrance sub-areas exist
  that both (a) lack a second egress and (b) sit within plausible chase-cornering distance of a
  Monster Patrol Waypoint or Spawn Preset — the floor-wide pass/fail gate for User Story 2, reviewed
  and signed off before this spec is considered implementation-complete.
- **SC-003**: A level-design walkthrough confirms at least three distinct intersections exist in the
  Exploration Zone, and at least one alternate route exists between the Exploration Zone and every
  previously-visited area, excluding the Key Area's and Locked Door Approach's documented
  single-entrance innermost points.
- **SC-004**: A level-design walkthrough confirms the route from the Key Area and the Locked Door
  Approach into the Chase Section carries exactly one lock (the Locked Door itself, owned by spec
  003) — zero redundant gates exist anywhere on that path.
- **SC-005**: A first-time playtester (per User Story 4) completes checkpoint-to-Floor-Exit-Door in
  a time the team judges consistent with the ±5-minute target, recorded as a playtest observation
  rather than an automated pass/fail (feel-driven pacing content, per ROADMAP §0).
- **SC-006**: A full perimeter walkthrough (User Story 5) finds zero camera angles exposing space
  outside the built level.
- **SC-007**: Every one of the seven GDD Ch. 16.3 checklist items is traceable to a specific FR in
  this spec or to a named downstream spec (002/003/004/005/006), with none left unaddressed by
  either.

## Assumptions

- **Topology decision**: the Key Area and the Locked Door Approach both branch directly off the
  Exploration Zone (neither nested behind the other), matching Floor 50's precedent of a
  hub-and-spoke topology for its own key loops. This is a deliberate choice to satisfy "the key is
  reachable without needing the locked door open first" and to minimize soft-lock risk, per this
  spec's User Story 1 and FR-003. If a future design pass wants an intentional order dependency, it
  must re-open this spec's topology, not be layered on top silently by spec 003.
- **Intentional single-entrance dead-ends**: none are currently designated. The Key Area's and the
  Locked Door Approach's innermost points are both treated as dead-ends that MUST get a second
  egress under FR-005(a) rather than being justified under FR-005(b), because these are exactly
  where a chase is most likely to corner a player (a `Monster` investigating noise from a Key
  pickup or a door-unlock interaction, GDD Ch. 7.1 noise radius for "Interact objek"). Any exception
  discovered during blockout must be added here by name before it ships, with the FR-005(b) distance
  justification spelled out.
- **"Chase Section" is singular**: per GDD Ch. 16.2's blueprint (one Chase Section per floor), this
  spec builds exactly one, positioned after the Locked Door.
- **"Many intersections" is a floor-identity requirement, not decoration**: FR-004's minimum of three
  distinct intersections is treated as load-bearing for GDD Ch. 3's Floor 51 layout description, the
  same way Floor 50's dead-end density is load-bearing for that floor — it is not an optional
  flourish a future pass may quietly drop.
- This spec does not fabricate exact coordinates, room dimensions, or a floor plan image — per
  ROADMAP §0 and the constitution's Principle I (specs describe observable behavior, not
  implementation), the actual blockout geometry is authored directly in
  `Assets/Scenes/Floor51.unity` by the level designer against these content requirements, and is
  validated by playtesting/review, not by a fabricated coordinate acceptance test.

## Related

- [[ROADMAP]] — scenes §3, `005-floor-51-scene` row 1 (`001-level-layout-and-geometry`); no systems
  dependency (matches Floor 52/50's equivalent row)
- [[constitution]] — Principle I (spec before implementation), Principle II (fixed/manual map, no
  speculative procedural-generation framework)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3 (Floor 51 column: Pressure, active monster at normal
  chase speed, max-2 battery respawn, intersection-heavy layout, hiding spot present), Ch. 3.1
  (battery respawn rules, consumed by spec 004), Ch. 6.2/6.3 (monster tuning and spawn rules,
  consumed by spec 002), Ch. 8.1 (1 key + 1 locked door objective), Ch. 8.2 (progression feedback,
  consumed by spec 006), Ch. 16.1 (fixed, hand-drawn map), Ch. 16.2 (blueprint template), Ch. 16.3
  (validation checklist)
- `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` — Safe Area distance/sightline
  rule (FR-002) and dead-end/patrol-proximity rule (FR-005) both depend on this system's spawn point
  validity rules
- `specs/scenes/005-floor-51-scene/002-monster-patrol-route-and-spawn-presets` — places Monster
  Spawn Presets and Patrol Waypoints into this spec's area graph
- `specs/scenes/005-floor-51-scene/003-key-and-locked-door-placement` — places the Key and Locked
  Door into this spec's Key Area and Locked Door Approach
- `specs/scenes/005-floor-51-scene/004-battery-spawn-point-placement` — places Battery Spawn Points
  into this spec's reserved slots (FR-009)
- `specs/scenes/005-floor-51-scene/005-hiding-spot-placement` — places the Hiding Spot(s) into this
  spec's reserved nooks (FR-010)
- `specs/scenes/005-floor-51-scene/006-floor-exit-and-transition` — places the Floor Exit Door into
  this spec's `FloorExitAlcove`
