# Feature Specification: Floor 50 Level Layout & Geometry

**Feature Branch**: `001-level-layout-and-geometry`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Apply the GDD Ch. 16.2 blueprint template to Floor 50 (Mastery),
LILO's hardest and final floor, with THREE Locked Door → Key Area loops (not one) branching off a
shared exploration hub before a single closing Chase Section → Final Door. Floor 50's layout is
explicitly the most dead-end-heavy, narrow-route floor in the game (GDD Ch. 3 Floor 50 column) —
this spec owns the fixed, hand-drawn (non-procedural) geometry and area graph that every other
Floor 50 content spec (monster patrol/spawn, keys/doors, Final Door, battery spawns, hiding spots)
places its objects into."

## Why This Spec Exists

Floor 50 is the "Mastery" floor (GDD Ch. 3): the last floor of the run, the hardest, and the one
whose layout is explicitly described as having more dead-ends and narrower routes than Floor 51 or
Floor 52. Every other Floor 50 scene spec — monster patrol/spawn presets (002), the three
keys/locked doors (003), the Final Door (004), battery spawn points (005), and hiding spots (006)
— places its objects *into* a floor geometry that must already exist and already satisfy the GDD
Ch. 16.3 validation checklist. This spec is that geometry: the fixed (non-procedural, GDD Ch. 16.1)
area graph for `Assets/Scenes/Floor50.unity`, expressed as content requirements (named areas, their
connectivity, and the spatial guarantees every downstream spec can rely on) rather than fabricated
coordinates. Getting the area graph and its escape-route guarantees wrong here means every
downstream spec inherits an unsafe or unsatisfiable floor.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reach Every Objective Loop Without a Single Forced Path (Priority: P1)

A player entering Floor 50's Exploration Hub can reach the entrance of any of the three
Locked-Door-to-Key-Area loops without first passing through another loop's Locked Door, and can
always choose more than one route between the Hub and any point they've already explored — Floor
50 is not one straight corridor with three side-rooms bolted on.

**Why this priority**: This is the structural backbone every other Floor 50 spec depends on. If
the three loops are nested or gated behind each other, spec 003's key/door placement inherits an
artificial order-dependency and a soft-lock risk the task explicitly asks it to avoid. Get the hub
topology right first; everything else is content placed inside it.

**Independent Test**: Block out the Exploration Hub and the entrances to all three loops in
`Assets/Scenes/Floor50.unity` using placeholder geometry (no final art, no functional Door/Key
components required yet), then walk `PlayerCharacter` from the floor checkpoint to each of the
three loop entrances in turn and confirm each is reachable directly from the Hub, and that at
least one alternate corridor exists between the Hub and each previously-visited area (GDD Ch. 16.3
checklist item 1).

**Acceptance Scenarios**:

1. **Given** the player has just entered the Exploration Hub from the Safe Area, **When** they
   choose to head toward any one of the three loop entrances first, **Then** they reach it without
   passing through a Locked Door belonging to either of the other two loops.
2. **Given** the player has explored the Hub and one loop, **When** they backtrack toward the Hub,
   **Then** at least one route back exists that is not the exact reverse of the path they used to
   arrive (an alternate corridor, not a single there-and-back corridor).

---

### User Story 2 - Never Become Inescapably Trapped While Chased (Priority: P1)

**This is the single non-negotiable requirement for this floor.** Because Floor 50's GDD-mandated
layout style is dead-end-heavy and narrow (GDD Ch. 3 Floor 50 column: "Banyak dead-end, rute
sempit"), every dead-end sub-area the floor contains MUST still give a player being actively
chased by `Monster` a way out that does not require passing back through the monster's current
blocking position — a dead-end is allowed to exist for exploration tension, but it is never allowed
to be an inescapable trap.

**Why this priority**: GDD Ch. 16.3's second checklist item ("Tidak ada dead-end yang bisa membuat
player terjebak tanpa jalan keluar saat dikejar") is a hard floor-wide validation gate applied to
every floor, but it carries the most risk specifically on Floor 50, precisely because this is the
one floor the GDD explicitly instructs to be built with more dead-ends and narrower routes than the
other two (GDD Ch. 3). A layout that satisfies "has lots of dead-ends" while violating "no
inescapable dead-end while chased" is not a stricter version of Floor 50's design intent — it is a
broken floor, indistinguishable from a softlock generator. This is treated as a P1 blocking
requirement, not a nice-to-have polish pass, and is called out on its own rather than folded into
the general validation checklist pass in User Story 5.

**Independent Test**: With placeholder blockout geometry in place and `Monster` either absent or
represented by a simple chase-simulating stand-in, walk every dead-end sub-area in the floor (each
Key Area's innermost point, any narrow side-route, the Chase Section's margins) and confirm each
one has either (a) a second egress distinct from its single entrance, or (b) is explicitly
documented as intentionally single-entrance and is far enough from any Monster Patrol Waypoint
(per 002) that a chase cannot plausibly corner a player there — this second option requires
sign-off recorded in this spec's Assumptions, not a silent exception.

**Acceptance Scenarios**:

1. **Given** a player is standing at the innermost point of any Key Area (A, B, or C) and
   `Monster` enters CHASE state and approaches the loop's only Locked Door opening, **When** the
   player attempts to leave, **Then** a second route out of that Key Area exists that does not
   require passing the position `Monster` currently occupies.
2. **Given** a player is anywhere inside the Chase Section (the long corridor before the Final
   Door), **When** they are being chased, **Then** the Chase Section's margins contain no
   unmarked single-entrance alcove that a fleeing player could mistake for an escape route and
   become trapped in.
3. **Given** the full area graph is reviewed end to end, **When** every sub-area is checked against
   Monster Patrol Waypoints from spec 002, **Then** zero sub-areas are found where the only
   physical egress coincides with a waypoint or connecting corridor `Monster` is known to occupy
   during PATROL, INVESTIGATE, or CHASE.

---

### User Story 3 - Flow Through Three Independent Key Loops Into a Shared Chase Section (Priority: P2)

A player experiences exactly three Locked-Door-to-Key-Area loops branching off the Exploration Hub
(GDD Ch. 8.1: "Cari key untuk 3 pintu terkunci"), and the geometry funnels back to a single shared
area downstream of all three loops that leads into the Chase Section — the floor's remaining
objective — the Final Door at the far end of that Chase Section — is what actually stays closed
until all three loops are resolved, enforced by spec 004's lock condition, not by a physical
barrier this spec places in front of the Chase Section itself.

**Why this priority**: This is Floor 50's headline structural difference from Floor 51 (one loop)
and Floor 52 (zero loops), per the GDD Ch. 3 difficulty table. It is P2 rather than P1 because the
concrete key/door objects and the actual lock condition are out of this spec's scope (owned by 003
and 004) — this spec only owns that the *geometry* for three independent loops exists and that
they converge into one shared route toward the Chase Section, with no invented secondary gate
mechanism duplicating the Final Door's own lock (constitution Principle II — no redundant
mechanism where one lock, owned by 004, already does the job).

**Independent Test**: Block out the three loop geometries and the shared downstream area, then
walk `PlayerCharacter` from each loop's Key Area back to that shared area and confirm all three
converge into one route toward the Chase Section, with no additional physical blocker along that
route — only the Final Door itself (spec 004) is ever locked.

**Acceptance Scenarios**:

1. **Given** a player has resolved zero, one, two, or three of the loops, **When** they walk from
   the Exploration Hub toward the Chase Section, **Then** the physical route is open in every case
   — this spec places no barrier there.
2. **Given** a player walks the full Chase Section to the Final Door alcove having resolved fewer
   than three loops, **When** they reach the Final Door, **Then** the door itself is locked (per
   spec 004's lock condition, citing `specs/systems/keys-and-doors/003-final-door-distinct-behavior`)
   — the player can physically stand in front of it, but cannot open it.

---

### User Story 4 - Complete the Floor in About Five Minutes on a First Try (Priority: P3)

A player who has never played Floor 50 before can complete it — checkpoint to Final Door — in
approximately five minutes (GDD Ch. 1.4 target floor duration, Ch. 16.3 checklist item 6),
consistent with the game's per-floor pacing target even though this is the hardest floor.

**Why this priority**: Pacing validation is a review/playtest outcome, not a structural
prerequisite for the other user stories — it can only be confirmed once the other Floor 50 specs'
content (keys, monster, batteries) is placed into this geometry, so it is ordered last.

**Independent Test**: Once all six Floor 50 specs are implemented, have a person with no prior
exposure to this floor play a full run from the checkpoint to the Final Door and time it.

**Acceptance Scenarios**:

1. **Given** a first-time player starts at the Floor 50 checkpoint, **When** they play without
   external guidance, **Then** they reach the Final Door in approximately five minutes (target
   `GameConfig.targetFloorDuration` = 300 seconds, GDD Ch. 17.5), acknowledging Floor 50 is allowed
   to run longer than Floor 51/52 given its difficulty but should not grossly exceed the target.

---

### User Story 5 - Never See Outside the Level (Priority: P3)

From any point a player can stand in Floor 50, looking in any direction never reveals empty space
beyond the level's built geometry (GDD Ch. 16.3 checklist item 7).

**Why this priority**: A visual-polish/immersion check, dependent on the camera's boundary clamp
(owned by `movement-and-camera/003-camera-follow-and-boundary-clamp`) and only fully verifiable
once the floor's outer boundary is fully blocked out — ordered last as a closing validation pass.

**Independent Test**: Walk the full perimeter of the completed blockout, including every dead-end
and loop's innermost point, and confirm no camera angle exposes area outside the built floor
geometry.

**Acceptance Scenarios**:

1. **Given** the player stands at any reachable point in Floor 50, **When** the camera follows per
   its standard framing, **Then** no unbuilt/empty space is visible at any edge of the frame.

---

### Edge Cases

- **Three-key ordering**: this spec does not gate any one loop behind another (User Story 1) — the
  Exploration Hub geometry MUST present all three loop entrances as reachable in any order. Spec
  003 inherits this and MUST NOT introduce an item-based order-dependency across loops without
  re-opening this spec's topology decision (see Assumptions).
- **Dead-end-while-chased risk given this floor's mandated layout style**: addressed as its own P1
  story (User Story 2) rather than left implicit in the general validation pass, because this is
  the floor where the risk is highest by design (GDD Ch. 3: "Banyak dead-end, rute sempit").
- **Final Door reachability if a key is missed**: because a `Key` is never consumed or lost once
  picked up and a `Door` once unlocked stays unlocked for the remainder of the floor attempt (GDD
  Ch. 8.1 objective model; reset-on-death is a full floor reset per GDD Ch. 9.2, not a partial
  one), there is no reachable state where a player has "missed" a key permanently within a single
  floor attempt — every Key Area remains open and its Key remains collectible until that loop's
  Door is unlocked. This spec's gating area (User Story 3) therefore only needs to check "all three
  resolved," never a case of a permanently unreachable fourth path.
- **Player revisits a resolved loop after opening the gate to the Chase Section**: the geometry
  MUST NOT block backtracking into an already-resolved loop (e.g., to reach a Battery Spawn Point
  placed there by spec 005) purely because the Chase Section gate is open — the gate only adds a
  new route, it does not remove existing ones.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Assets/Scenes/Floor50.unity` MUST implement the GDD Ch. 16.2 blueprint order —
  Checkpoint/START → Safe Area → Exploration Hub → three parallel Locked-Door-to-Key-Area loops →
  a single Chase Section → Final Door — as its top-level area graph. This is Floor 50's
  floor-specific instantiation of the generic blueprint; it does not redefine the blueprint itself.
- **FR-002**: The Safe Area MUST guarantee no `Monster` presence at floor start not by disabling
  the monster (GDD Ch. 6.3: the monster always exists on the map from floor start on Floor 50/51,
  it never "appears out of nowhere") but by geometry alone — the Safe Area MUST be far enough from,
  and outside the initial sightline of, every valid Monster Spawn Point defined by spec 002, per
  the spawn-point validity rules in
  `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`.
- **FR-003**: The Exploration Hub MUST connect to the entrance of all three Locked-Door loops
  directly, with no loop entrance gated behind another loop's Locked Door — satisfying the
  order-independence decision recorded in this spec's Assumptions and consumed by spec 003.
- **FR-004**: The area graph MUST provide at least one alternate route between the Exploration Hub
  and any area the player has already visited (GDD Ch. 16.3 checklist item 1) — no portion of the
  explorable floor (excluding the three Key Areas' innermost dead-end points, which are
  intentionally single-entrance per FR-005) may be reachable by exactly one corridor with no loop
  or branch.
- **FR-005**: Every dead-end sub-area in the floor — including each Key Area's innermost point and
  any narrow side-route — MUST either (a) provide a second, distinct egress, or (b) be explicitly
  listed in this spec's Assumptions as an intentional single-entrance dead-end whose distance from
  every Monster Patrol Waypoint (spec 002) makes it implausible for a chase to corner a player
  there. This is a non-negotiable, floor-wide gate — no dead-end may ship without satisfying (a) or
  being explicitly justified under (b) (GDD Ch. 16.3 checklist item 2; see User Story 2).
- **FR-006**: The Chase Section MUST be a single long corridor-style area (GDD Ch. 16.2: "lorong
  panjang, mendorong sprint") positioned after the point where all three loops are resolved, and
  MUST NOT contain any unmarked single-entrance alcove along its margins that could be mistaken
  for an escape route during a chase (User Story 2, Acceptance Scenario 2).
- **FR-007**: A single shared area MUST sit between the three loops and the Chase Section, giving
  all three loops one common route into the Chase Section. This spec MUST NOT place any physical
  barrier or resolved-flag gate on that route — the requirement that all three loops be resolved
  before progression completes is enforced entirely by the Final Door's own lock condition at the
  far end of the Chase Section (owned by spec 004, citing
  `specs/systems/keys-and-doors/003-final-door-distinct-behavior/spec.md`), not by a second,
  redundant gate this spec would otherwise need to keep in sync with it (User Story 3).
- **FR-008**: The Final Door's area (its immediate approach and alcove at the end of the Chase
  Section) is reserved by this spec's area graph but its concrete placement, trigger, and
  narrative-bridge dependency are owned entirely by spec 004 — this spec only guarantees the
  Chase Section terminates in a dedicated Final Door alcove, not a shared/ambiguous space.
- **FR-009**: The area graph MUST reserve spatially distinct locations across the Exploration Hub
  and all three loops suitable for Battery Spawn Point placement (spec 005) such that spread-out
  placement (GDD Ch. 16.3 checklist item 3) is achievable without spec 005 needing to alter this
  spec's geometry.
- **FR-010**: The area graph MUST reserve at least one office-appropriate enclosed nook (an
  under-desk-sized space, GDD Ch. 4.3) along a plausible traversal path, for Hiding Spot placement
  by spec 006 — Floor 50 has fewer hiding spots than Floor 51/52 (GDD Ch. 3 Floor 50 column: "Ada,
  tapi lebih jarang"), so this reservation is deliberately sparser than earlier floors, not absent.
- **FR-011**: The Exploration Hub and all three loops MUST be laid out so that Monster Patrol
  Waypoints (spec 002) can pass through objective-adjacent areas without idling directly on top of
  a Key, Door, or the Final Door (GDD Ch. 16.3 checklist item 4) — this spec provides the
  geometry that makes such a route possible; spec 002 owns the actual waypoint placement.
- **FR-012**: The full built floor MUST contain no camera-visible point at which empty space
  outside the level geometry is revealed, at any reachable player position (GDD Ch. 16.3 checklist
  item 7).
- **FR-013**: The floor's geometry MUST be fixed and hand-authored, never procedurally generated,
  per GDD Ch. 16.1 ("Map FIXED, bukan procedural").
- **FR-014**: The complete area graph, once implemented, MUST be reviewable end to end against
  every item in the GDD Ch. 16.3 validation checklist as a single pass, with each item traceable
  to a specific FR in this spec or an explicitly named downstream spec (see checklist Notes).

### Key Entities

- **Area**: A named region of the floor's fixed geometry. This spec defines exactly these Areas:
  `Checkpoint` (floor entry/respawn point, GDD Ch. 9.1), `SafeArea`, `ExplorationHub`,
  `KeyLoopA` / `KeyLoopB` / `KeyLoopC` (each containing a Key Area and its Locked Door opening),
  `LoopConfluence` (the single shared area downstream of all three loops, FR-007), `ChaseSection`,
  and `FinalDoorAlcove`. Areas are a design/documentation concept for this spec's content
  requirements, not necessarily a distinct `MonoBehaviour` type — later specs may implement
  waypoints, spawn points, and triggers as children of these Areas' scene hierarchy without this
  spec mandating a specific component.
- **Door / Key**: Referenced structurally (three Locked Doors gate the three loops; the Final Door
  gates the floor's end) but defined and placed by specs 003 and 004, not this spec.
- **Monster / Monster Spawn Point / Monster Patrol Waypoint**: Referenced structurally (Safe Area
  must sit outside their reach, dead-ends must sit outside plausible chase-cornering distance) but
  defined and placed by spec 002, not this spec.
- **Battery Spawn Point / Hiding Spot**: This spec only reserves spatial slots for them (FR-009,
  FR-010); concrete placement is owned by specs 005 and 006 respectively.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A level-design walkthrough of the completed blockout confirms all three Key Loop
  entrances are reachable from the Exploration Hub in any order, with zero cases of one loop's
  Locked Door blocking access to another loop's entrance.
- **SC-002**: A level-design walkthrough confirms zero dead-end sub-areas exist that both (a) lack
  a second egress and (b) sit within plausible chase-cornering distance of a Monster Patrol
  Waypoint — the floor-wide pass/fail gate for User Story 2, reviewed and signed off before this
  spec is considered implementation-complete.
- **SC-003**: A level-design walkthrough confirms at least one alternate route exists between the
  Exploration Hub and every previously-visited area, excluding the three Key Areas' documented
  single-entrance innermost points.
- **SC-004**: A level-design walkthrough confirms the route from every one of the three loops
  through the shared `LoopConfluence` area into the Chase Section is physically open regardless of
  how many loops are resolved (zero through three), and that the only locked element anywhere on
  that path is the Final Door itself at the end (owned by spec 004) — i.e., zero redundant gates
  exist between the loops and the Chase Section.
- **SC-005**: A first-time playtester (per User Story 4) completes checkpoint-to-Final-Door in a
  time that the team judges consistent with the ±5-minute target given Floor 50's difficulty,
  recorded as a playtest observation rather than an automated pass/fail (feel-driven pacing content,
  per ROADMAP §0).
- **SC-006**: A full perimeter walkthrough (User Story 5) finds zero camera angles exposing space
  outside the built level.
- **SC-007**: Every one of the seven GDD Ch. 16.3 checklist items is traceable to a specific FR in
  this spec or to a named downstream spec (003/004/005/006), with none left unaddressed by either.

## Assumptions

- **Loop topology decision**: the three Key Loops branch off the Exploration Hub in parallel
  (hub-and-spoke), not nested or chained. This is a deliberate choice to satisfy "each key
  reachable without needing another locked door open first" and to minimize soft-lock risk, per
  this spec's User Story 1 and FR-003. If a future design pass wants an intentional order
  dependency between loops, it must re-open this spec's topology, not be layered on top silently
  by spec 003.
- **Intentional single-entrance dead-ends**: none are currently designated. Each Key Loop's
  innermost Key Area point is treated as a dead-end that MUST get a second egress under FR-005(a)
  rather than being justified under FR-005(b), because Key Areas are exactly where a chase is most
  likely to corner a player (a `Monster` investigating noise from a Key pickup, GDD Ch. 7.1 noise
  radius for "Interact objek"). Any exception discovered during blockout must be added here by
  name before it ships, with the FR-005(b) distance justification spelled out.
- **"Chase Section" is singular**: per GDD Ch. 16.2's blueprint (one Chase Section per floor), this
  spec builds exactly one, positioned after all three loops, not one per loop.
- This spec does not fabricate exact coordinates, room dimensions, or a floor plan image — per
  ROADMAP §0 and the constitution's Principle I (specs describe observable behavior, not
  implementation), the actual blockout geometry is authored directly in
  `Assets/Scenes/Floor50.unity` by the level designer against these content requirements, and is
  validated by playtesting/review, not by a fabricated coordinate acceptance test.

## Related

- [[ROADMAP]] — scenes §3, `006-floor-50-scene` row 1 (`001-level-layout-and-geometry`); no
  systems dependency (matches Floor 51/52's equivalent row)
- [[constitution]] — Principle I (spec before implementation), Principle II (fixed/manual map, no
  speculative procedural-generation framework)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3 (Floor 50 column: Mastery, aggressive monster, scarce
  battery, dead-end/narrow-route layout, rarer hiding), Ch. 3.1 (battery respawn rules, consumed by
  spec 005), Ch. 6.2/6.3 (monster tuning and spawn rules, consumed by spec 002), Ch. 8.1 (3 keys +
  Final Door objective), Ch. 8.2 (progression feedback), Ch. 16.1 (fixed, hand-drawn map), Ch. 16.2
  (blueprint template), Ch. 16.3 (validation checklist)
- `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` — Safe Area distance/sightline
  rule (FR-002) and dead-end/patrol-proximity rule (FR-005) both depend on this system's spawn
  point validity rules
- `specs/scenes/006-floor-50-scene/002-monster-patrol-route-and-spawn-presets` — places Monster
  Spawn Presets and Patrol Waypoints into this spec's area graph
- `specs/scenes/006-floor-50-scene/003-three-keys-and-locked-doors-placement` — places the three
  Keys and Locked Doors into this spec's three loops
- `specs/scenes/006-floor-50-scene/004-final-door-placement-and-trigger` — places the Final Door
  into this spec's `FinalDoorAlcove`
- `specs/scenes/006-floor-50-scene/005-battery-spawn-point-placement` — places Battery Spawn Points
  into this spec's reserved slots (FR-009)
- `specs/scenes/006-floor-50-scene/006-hiding-spot-placement` — places Hiding Spot(s) into this
  spec's reserved nook(s) (FR-010)
