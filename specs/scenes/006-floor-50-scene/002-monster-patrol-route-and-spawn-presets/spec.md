# Feature Specification: Floor 50 Monster Patrol Route & Spawn Presets

**Feature Branch**: `002-monster-patrol-route-and-spawn-presets`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Author the authored (fixed, non-procedural) Monster Spawn Presets
and Patrol Waypoint route for Floor 50 — this floor's most aggressive monster tuning in the game
(GDD Ch. 6.2/6.3) — satisfying the generic spawn-point validity rules from
`specs/systems/monster-ai/003-spawn-point-validation-rules` and confirming this floor correctly
resolves the Floor 50 tuning profile from `specs/systems/monster-ai/002-per-floor-tuning-profile`
(investigateDuration 6s, chaseHoldDuration 5s, searchDuration 8s, patrolSpeed 1.2×, chaseSpeed
1.5×). This spec places content into `Assets/Scenes/Floor50.unity`'s area graph (spec 001); it
does not redefine the state machine, the tuning values, or the validation rules themselves."

## Why This Spec Exists

Floor 50 is the one floor in the game whose monster is explicitly tuned to be more patient, faster,
and more aggressive than Floor 51's (GDD Ch. 6.2: patrol 1.2× vs. 1.0×, chase 1.5× vs. 1.4×,
investigate 6s vs. 4s, chase-hold 5s vs. 3s, search 8s vs. 6s). None of that tuning is decided
here — it is locked verbatim by `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`.
What this spec owns is the floor-specific content that tuning acts on: where the `Monster` is
allowed to start (several validated presets, one chosen at random per GDD Ch. 6.3) and the fixed
route it patrols between encounters. Getting either wrong on this floor is riskier than on Floor
51, because Floor 50's own layout spec (001) is deliberately the most dead-end-heavy, narrow-route
floor in the game — a patrol route or spawn preset placed carelessly here is the likeliest place in
the whole game for an "inescapable trap" regression against 001's User Story 2.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Monster Starts at One of Several Validated Presets, Never the Same Spot Twice in a Row (Priority: P1)

Each time Floor 50 loads (first entry, or a respawn after death per GDD Ch. 9.2), `Monster` begins
in `Patrol` at one of several hand-authored, pre-validated starting positions, chosen uniformly at
random — never a single fixed spot, and never a position picked freely at runtime (GDD Ch. 6.3:
"Posisi awal monster diambil acak dari beberapa preset spawn point yang sudah divalidasi, bukan
acak bebas").

**Why this priority**: Without at least one valid preset, Floor 50 has no monster to speak of; the
GDD's explicit requirement for *multiple* presets (not one) and *random selection among them* (not
a fixed spot) is what keeps a memorized run from trivializing the floor's threat, so both the
existence and the randomness are treated as one P1 story rather than splitting them.

**Independent Test**: With Floor 50's blockout from spec 001 in place, author at least three
`Monster Spawn Preset` markers, each satisfying the five validity rules from
`specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` (reachable by navigation, far
enough from the player's Checkpoint, outside the player's initial sightline, not touching an
objective, and not creating an unavoidable instant-death situation). Run floor load repeatedly in
the Unity Editor and confirm the chosen starting preset varies across runs and every choice is one
of the authored three.

**Acceptance Scenarios**:

1. **Given** Floor 50 has at least three authored Monster Spawn Presets, **When** the floor loads,
   **Then** `Monster` begins in `Patrol` at exactly one of them, chosen uniformly at random.
2. **Given** the floor is reloaded (or reset after a death, GDD Ch. 9.2), **When** the new
   `Monster` spawn is resolved, **Then** the selection is re-rolled independently — it is not
   required to differ from the previous run's choice, but it must not be hardcoded to always
   repeat the same preset.
3. **Given** every authored preset, **When** each is checked against
   `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`'s five rules, **Then** all
   five pass for every preset — none is exempted.

---

### User Story 2 - A Single Fixed Patrol Route Touches Every Loop Without Idling on an Objective (Priority: P1)

While in `Patrol`, `Monster` walks a fixed (hand-authored, non-procedural per GDD Ch. 16.1) sequence
of waypoints that carries it through or near the Exploration Hub and the vicinity of all three Key
Loops from spec 001 — so a player can never assume any one loop is "currently safe" for the whole
run — but the route never stops directly on top of a `Key`, a `Door`, or the `FinalDoorAlcove` (GDD
Ch. 16.3 checklist item 4: "Patrol route monster melewati area objective, tapi tidak berdiri diam di
atasnya").

**Why this priority**: Equal priority to User Story 1 — a monster with nowhere fixed to patrol
cannot meaningfully threaten the three-loop structure that makes Floor 50 different from Floor 51,
and a route that idles on an objective would make that objective's Key or Door effectively
unreachable, which is a correctness failure severe enough to share P1 rather than trail behind it.

**Independent Test**: Author a sequence of Patrol Waypoints inside `Assets/Scenes/Floor50.unity`
covering the Exploration Hub and passing near (not through the innermost point of) each of
`KeyLoopA`/`KeyLoopB`/`KeyLoopC` from spec 001. Run the floor in Play mode, let `Monster` patrol
several full loops of the route, and confirm it never stops on a waypoint that coincides with any
`Key`, `Door`, or the `FinalDoorAlcove`'s reserved area.

**Acceptance Scenarios**:

1. **Given** the full authored patrol route, **When** `Monster` completes one full circuit,
   **Then** it has passed through or near every one of the three Key Loops' vicinity and the
   Exploration Hub at least once.
2. **Given** any single waypoint in the route, **When** its position is checked against every
   `Key`, `Door`, and `FinalDoorAlcove` reserved location, **Then** none coincide.
3. **Given** the patrol route as authored, **When** it is checked against spec 001's `SafeArea`,
   **Then** no waypoint falls inside or requires passing through the `SafeArea` (spec 001 FR-002:
   the Safe Area must sit outside every valid Monster Spawn Point's reach — this spec extends that
   guarantee to the patrol route itself, since a patrolling monster that later drifts into the Safe
   Area would silently violate FR-002's intent).

---

### User Story 3 - Floor 50 Resolves the Aggressive Tuning Profile, Not Floor 51's (Priority: P1)

`Assets/Scenes/Floor50.unity` is wired so that its `MonsterController` resolves
`specs/systems/monster-ai/002-per-floor-tuning-profile`'s Floor 50 values —
`investigateDuration = 6s`, `chaseHoldDuration = 5s`, `searchDuration = 8s`, `patrolSpeed = 1.2×`,
`chaseSpeed = 1.5×` — never Floor 51's, and never a literal hardcoded in this scene's own code.

**Why this priority**: This is the entire reason Floor 50 is called "Mastery" rather than a second
"Pressure" floor (GDD Ch. 3). A correctly-populated area graph (spec 001) and correctly-placed
presets/route (User Stories 1–2) still produce the wrong floor if the tuning wired in is Floor 51's
— this is as foundational to the floor's identity as the geometry itself, so it is P1 alongside the
other two, not a lower-priority polish pass.

**Independent Test**: With `Assets/Scenes/Floor50.unity` loaded and its floor identifier set to
Floor 50, query the resolved `MonsterTuningProfile` and assert it equals the five values above —
and separately, assert it does *not* equal Floor 51's five values
(`investigateDuration = 4s, chaseHoldDuration = 3s, searchDuration = 6s, patrolSpeed = 1.0×,
chaseSpeed = 1.4×`).

**Acceptance Scenarios**:

1. **Given** `Assets/Scenes/Floor50.unity` is loaded, **When** `MonsterController` resolves its
   tuning profile via `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`'s lookup,
   **Then** the resolved values are exactly `investigateDuration = 6s`, `chaseHoldDuration = 5s`,
   `searchDuration = 8s`, `patrolSpeed = 1.2×`, `chaseSpeed = 1.5×`.
2. **Given** the same resolved profile, **When** `chaseSpeed` is compared against
   `GameConfig.sprintMultiplier` (1.6×, GDD Ch. 17.1), **Then** `1.5× < 1.6×` holds — the hard rule
   from GDD Ch. 6.2 is satisfied on this floor specifically, the floor with the smallest margin
   (0.1×) between chase and sprint speed in the entire game.
3. **Given** a hypothetical scene-wiring mistake that pointed Floor 50 at Floor 51's profile
   instead, **When** the same query in Acceptance Scenario 1 runs, **Then** the mismatch is
   immediately detectable by the values not matching — this scenario exists to make clear that
   this spec's own test must assert the *specific* Floor 50 numbers, not merely "a profile
   resolved successfully."

---

### User Story 4 - Every Preset and Waypoint Stays Clear of Spec 001's Audited Dead-Ends (Priority: P2)

Once spec 001's full dead-end audit (its `tasks.md` T016/T017) exists, this spec's spawn presets and
patrol waypoints are cross-checked against it, closing the loop that spec 001's own User Story 2
left as a deferred, forward-looking check ("cross-check the full dead-end list...against spec 002's
Monster Patrol Waypoints once that spec lands").

**Why this priority**: This is a correctness *re-check* on top of content that already exists once
User Stories 1–3 are complete — it cannot be performed before spec 001's audit exists, so it is
necessarily sequenced after the P1 stories, but it is not optional: an un-rechecked patrol route is
exactly the gap GDD Ch. 3's "Banyak dead-end, rute sempit" language warns is riskiest on this floor.

**Why this priority**: Ranked P2 because it is a verification pass over already-built content
(User Stories 1–3), not new player-facing behavior — but it remains required before this feature is
considered complete, since it is the other half of a bidirectional guarantee spec 001 could only
promise, not fully verify, on its own.

**Independent Test**: With spec 001's dead-end list and this spec's presets/waypoints both
finalized, walk the cross-check: for every dead-end sub-area spec 001 lists, confirm no patrol
waypoint or spawn preset sits on its single egress; and for every patrol waypoint, confirm it is not
inside a dead-end spec 001 flagged as requiring distance from patrol.

**Acceptance Scenarios**:

1. **Given** spec 001's finalized dead-end list, **When** every patrol waypoint's position is
   checked against it, **Then** zero waypoints coincide with a dead-end's single egress.
2. **Given** the same finalized list, **When** every Monster Spawn Preset's position is checked,
   **Then** none sits inside or immediately adjacent to a dead-end sub-area (reinforcing spec 001
   FR-002's "far enough from player Checkpoint" guarantee also holds with respect to dead-ends
   specifically, not only the Checkpoint).

---

### Edge Cases

- **Chase speed vs. sprint speed margin**: Floor 50's `chaseSpeed` (1.5×) sits only 0.1× below the
  player's `sprintMultiplier` (1.6×) — the tightest margin of any floor in the game (Floor 51's gap
  is 0.2×). This spec does not change either number (both are locked by monster-ai/002 and
  shared-config-and-state respectively) but treats User Story 3's Acceptance Scenario 2 as
  non-negotiable specifically *because* of how tight this margin is — a scene-wiring regression that
  quietly pointed at the wrong profile would be far more punishing here than on Floor 51.
- **Patrol route dead-end risk**: because Floor 50's area graph (spec 001) is deliberately the most
  dead-end-heavy floor, a naively-drawn patrol route is the single most likely way this floor could
  regress spec 001's User Story 2 ("never become inescapably trapped while chased"). This spec's
  FR-004 and User Story 4 exist specifically to keep patrol content out of the audited dead-ends,
  not merely off of objectives.
- **Final Door reachability if a key is missed**: not a distinct risk introduced by this spec — per
  spec 001's Edge Cases, no key is ever permanently missable within one floor attempt, so a patrol
  route that happens to pass near an unresolved Key Loop does not create a new missed-key state; it
  only needs to avoid idling on the Key/Door itself (User Story 2).
- **Preset selected inside a loop the player hasn't opened yet**: permitted. GDD Ch. 6.3 only
  requires presets be far from the player's spawn, outside initial sightline, off objectives, and
  free of instant-death — it does not forbid a preset from sitting inside an unresolved Key Loop's
  outer area (not its innermost dead-end point, which FR-002 already excludes as a preset location
  via the dead-end/objective rules).
- **All three presets happen to be geographically clustered**: an authoring-quality risk, not a
  functional one — GDD Ch. 6.3 requires "beberapa" (several) validated presets and random selection
  among them, but does not mandate a minimum geographic spread between presets the way GDD Ch. 16.3
  mandates for Battery Spawn Points. This spec's FR-001 requires at least three presets; reviewers
  should still prefer presets spread across different Areas where the layout allows it, but this is
  a quality note, not a blocking FR.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Assets/Scenes/Floor50.unity` MUST contain at least three authored `Monster Spawn
  Preset` markers, each independently satisfying every rule in
  `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` (reachable by navigation, far
  enough from the player's Checkpoint spawn, outside the player's initial sightline, not touching
  any objective — Key, Door, or the Final Door — and not creating an unavoidable instant-death
  situation). This spec MUST NOT invent a different or looser validity rule than that system spec
  defines.
- **FR-002**: At floor load (including every respawn after death, GDD Ch. 9.2), the system MUST
  select exactly one of the authored presets uniformly at random as `Monster`'s starting `Patrol`
  position — never a single fixed preset, and never a position generated outside the authored set.
- **FR-003**: `Assets/Scenes/Floor50.unity` MUST contain a single, fixed (non-procedural, GDD Ch.
  16.1), hand-authored sequence of Patrol Waypoints that, over one full circuit, passes through or
  near spec 001's `ExplorationHub` and the vicinity of all three of `KeyLoopA`/`KeyLoopB`/`KeyLoopC`
  (GDD Ch. 16.3 checklist item 4's "melewati area objective").
- **FR-004**: No Patrol Waypoint MUST coincide with, or require standing directly on, any `Key`,
  `Door`, or the `FinalDoorAlcove`'s reserved location (GDD Ch. 16.3 checklist item 4's "tapi tidak
  berdiri diam di atasnya") — this spec's route must make objectives reachable while the monster is
  patrolling, never blocked by a stationary patrol point.
- **FR-005**: No Patrol Waypoint and no Monster Spawn Preset MUST fall inside, or require passing
  through, spec 001's `SafeArea` — extending spec 001 FR-002's spawn-point guarantee to this spec's
  own patrol content, so the Safe Area's monster-free promise holds for the full patrol cycle, not
  only for the initial spawn moment.
- **FR-006**: `Assets/Scenes/Floor50.unity`'s floor identifier MUST be set such that
  `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`'s lookup resolves the Floor 50
  `MonsterTuningProfile` (`investigateDuration = 6`, `chaseHoldDuration = 5`, `searchDuration = 8`,
  `patrolSpeed = 1.2`, `chaseSpeed = 1.5`, `monsterActive = true`) — never Floor 51's or a default.
  This spec does not redefine those five numbers; it only confirms this scene is wired to the
  correct floor identifier so the existing lookup resolves them correctly.
- **FR-007**: This spec's own scene-wiring code MUST NOT contain a hardcoded literal for any of the
  five tuning numbers in FR-006 — `MonsterController` reads them exclusively through
  monster-ai/002's resolved profile (constitution Principle III/V; ROADMAP §0 single configuration
  source).
- **FR-008**: This spec MUST include an automated check, scoped to Floor 50's own resolved
  configuration, that `chaseSpeed (1.5×) < GameConfig.sprintMultiplier (1.6×)` — reusing
  monster-ai/002's generic hard-rule validator (FR-005/FR-006 of that spec), not a second,
  duplicated implementation of the same check.
- **FR-009**: Once `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` and spec
  001's dead-end audit (`tasks.md` T016/T017) both exist, every Monster Spawn Preset and every
  Patrol Waypoint authored by this spec MUST be re-checked against spec 001's finalized dead-end
  list, with zero coincidences (User Story 4). This is a mandatory forward re-check, not an
  optional nicety — see Notes in this feature's `checklists/requirements.md`.
- **FR-010**: The concrete positions of the presets and waypoints are content authored directly in
  `Assets/Scenes/Floor50.unity` against spec 001's Areas — this spec MUST NOT fabricate exact
  coordinates in `spec.md` itself (ROADMAP §0; constitution Principle I).

### Key Entities

- **Monster Spawn Preset**: One of at least three hand-authored candidate starting positions for
  `Monster` on Floor 50, each validated against
  `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`. Chosen from uniformly at
  random on floor load/reset (FR-002). Defined and placed by this spec; the validity *rules* it
  must satisfy are owned by monster-ai/003.
- **Patrol Waypoint**: One node in Floor 50's single, fixed patrol sequence (FR-003). Ordered;
  `Monster` walks the sequence and loops back to the start. Defined and placed by this spec; the
  state-machine behavior that consumes the sequence (how `Patrol` advances between waypoints) is
  owned by `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`.
- **MonsterTuningProfile (Floor 50 instance)**: Referenced, not redefined — this spec only confirms
  correct floor-identifier wiring so `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md`
  resolves Floor 50's five locked values (FR-006).
- **Areas (from spec 001)**: `SafeArea`, `ExplorationHub`, `KeyLoopA`/`KeyLoopB`/`KeyLoopC`,
  `LoopConfluence`, `ChaseSection`, `FinalDoorAlcove` — this spec places its presets and waypoints
  into these existing Areas; it does not redefine or resize them.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of authored Monster Spawn Presets pass every one of
  `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md`'s validity rules, verified by
  an editor-time or automated check once that spec's rules are implemented.
- **SC-002**: Across at least 100 simulated floor-load trials, every one of the authored presets is
  selected at least once, and no single preset is selected in 100% of trials — confirming the
  random selection (FR-002) is not silently degenerating to a fixed choice (mirroring the
  non-degenerate-distribution pattern already used by
  `specs/systems/battery-spawn-system/003-respawn-timer-and-placement-rule/spec.md`'s SC-005).
- **SC-003**: A level-design walkthrough confirms the patrol route passes through or near all three
  Key Loops' vicinity and the Exploration Hub over one full circuit, and confirms zero waypoints
  coincide with a Key, Door, or the Final Door's reserved location.
- **SC-004**: An automated test querying `Assets/Scenes/Floor50.unity`'s resolved
  `MonsterTuningProfile` returns exactly `investigateDuration = 6s`, `chaseHoldDuration = 5s`,
  `searchDuration = 8s`, `patrolSpeed = 1.2×`, `chaseSpeed = 1.5×` in 100% of test runs, and never
  matches Floor 51's five values.
- **SC-005**: The same automated test confirms `chaseSpeed (1.5×) < sprintMultiplier (1.6×)` holds
  for Floor 50's resolved configuration in 100% of test runs.
- **SC-006**: Once spec 001's dead-end audit is finalized, a documented cross-check confirms zero
  Patrol Waypoints or Monster Spawn Presets coincide with an audited dead-end's single egress.

## Assumptions

- **`specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` is cited as this spec's
  forward dependency for the exact validity rules**, matching the precedent already set by spec
  001's own FR-002/FR-005, which cite the same not-yet-written system spec by path. This spec does
  not restate or invent those five rules independently — GDD Ch. 6.3 lists them, and the system
  spec is their owning source; if that spec's eventual FRs differ in wording from GDD Ch. 6.3's
  list, this spec's presets should be re-validated against whichever is authoritative once it
  ships, not against this spec's own paraphrase.
- **The five Floor 50 tuning numbers are already fully locked** by
  `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md` (GDD Ch. 6.2/17.4 — no `TBD`
  marker on any of the five). This spec treats them as given, cited data, not as something it
  re-derives or is free to adjust.
- **Exact preset and waypoint positions are level-design content**, authored directly in
  `Assets/Scenes/Floor50.unity` against spec 001's Areas, validated by editor review and playtesting
  — not by a fabricated coordinate acceptance test (ROADMAP §0; matches spec 001's own stance).
- **This spec does not decide NavMesh vs. hand-rolled waypoint-follow** for how `Monster` actually
  paths between waypoints — that adapter-layer decision already belongs to
  `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`'s own Assumptions (NavMesh
  chosen). This spec only supplies the waypoint *content*, not the pathing mechanism.

## Related

- [[ROADMAP]] — scenes §3, `006-floor-50-scene` row 2 (`002-monster-patrol-route-and-spawn-presets`,
  depends on `monster-ai/003`)
- [[constitution]] — Principle I (spec before implementation), Principle III (no hardcoded tuning
  literals in scene-specific code), Principle V (GameConfig additions are additive, never
  redefined)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 3 (Floor 50 column: aggressive monster), Ch. 6.2 (per-state
  tuning table and the hard chase-vs-sprint rule), Ch. 6.3 (presence & spawn preset rules), Ch. 16.3
  (validation checklist item 4)
- `specs/systems/monster-ai/002-per-floor-tuning-profile/spec.md` — owns the five Floor 50 numbers
  this spec confirms are correctly wired (FR-006/FR-007)
- `specs/systems/monster-ai/003-spawn-point-validation-rules/spec.md` — owns the validity rules this
  spec's presets must satisfy (FR-001)
- `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` — consumes the patrol
  waypoint sequence and the resolved tuning profile this spec supplies
- `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry` — owns the Areas this spec places
  presets and waypoints into, and the dead-end audit this spec cross-checks against (FR-005,
  FR-009, User Story 4)
- `specs/scenes/006-floor-50-scene/003-three-keys-and-locked-doors-placement` — placed Key/Door
  objects this spec's patrol route must not idle on (FR-004)
- `specs/scenes/006-floor-50-scene/004-final-door-placement-and-trigger` — placed Final Door this
  spec's patrol route must not idle on (FR-004)
