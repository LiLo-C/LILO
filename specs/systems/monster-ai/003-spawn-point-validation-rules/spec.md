# Feature Specification: Monster Spawn Point Validation Rules

**Feature Branch**: `003-spawn-point-validation-rules`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The validity rules for a Monster Spawn Point (navigable/not
trapped, far enough from player spawn, outside player's initial line of sight, not touching an
objective, no instant-death situations) as a reusable VALIDATOR — actual floor-specific spawn
point authoring happens in each floor scene's own spec, this system only defines and enforces the
validation rule."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validator rejects candidates that are too close to the player or an objective (Priority: P1)

Given a candidate world position for a `Monster` spawn point, the validator flags it invalid when
it is too close to the player's floor-entry position, or too close to any objective (a key, the
floor-exit door, or — on Floor 50 — the Final Door), using purely geometric distance checks
against tunable thresholds.

**Why this priority**: These are the two rules in GDD Ch. 6.3 that are fully expressible as plain
position math with no dependency on level geometry, physics, or a live scene — they are the
cheapest, most foundational slice of the validator, and directly protect the "Uncertainty over
Information" pillar (GDD Ch. 1.3): a monster spawning on top of the player or guarding a key would
make the encounter unfair on the very first tick, before any AI behavior even runs.

**Independent Test**: In an EditMode test (no scene needed), construct a candidate position at a
known distance from a scripted player-spawn position and from a scripted list of objective
positions; assert the validator reports the correct violation(s) at, above, and below each
threshold, using values read from a test `GameConfig`-shaped input, not literals baked into the
validator.

**Acceptance Scenarios**:

1. **Given** a candidate position closer to the player's spawn position than
   `monsterSpawnMinDistanceFromPlayer`, **When** the validator runs, **Then** it reports the
   "too close to player spawn" violation.
2. **Given** a candidate position closer to any single entry in the objective-position list than
   `monsterSpawnMinDistanceFromObjective`, **When** the validator runs, **Then** it reports the
   "too close to objective" violation, regardless of which objective in the list it is closest
   to.
3. **Given** a candidate position exactly at or beyond both thresholds, **When** the validator
   runs, **Then** neither distance-based violation is reported.
4. **Given** an empty objective-position list (e.g., a floor with no objectives resolved yet),
   **When** the validator runs, **Then** the objective-distance rule is vacuously satisfied — it
   is never reported as a violation merely because the list is empty.

---

### User Story 2 - Validator rejects candidates that are unreachable or start in plain view (Priority: P1)

Given a candidate position plus caller-supplied signals describing whether it is reachable
without being sealed in an enclosed space, and whether it lies within the player's initial line of
sight, the validator flags a candidate invalid on either condition.

**Why this priority**: Equal to User Story 1 — GDD Ch. 6.3 lists all five rules flatly, with no
rule marked more important than another, and a monster that starts either unreachable/trapped or
visibly in front of the player breaks the "Limited Vision" and "Uncertainty over Information"
pillars just as badly as spawning too close. It is a separate story from User Story 1 only because
these two checks consume signals that require live-scene geometry (NavMesh reachability, sightline
occlusion) rather than pure position math, and are supplied by the caller rather than computed
here (see Assumptions).

**Independent Test**: In an EditMode test, construct candidates with every combination of
`IsNavigable`/`IsOutsideInitialPlayerSightline` set to `true`/`false` (the two caller-supplied
booleans this story owns) while holding the User Story 1 distance checks fixed at "pass"; assert
the validator reports exactly the violations matching the `false` flags, and none when both are
`true`.

**Acceptance Scenarios**:

1. **Given** a candidate whose `IsNavigable = false`, **When** the validator runs, **Then** it
   reports the "not navigable / trapped" violation.
2. **Given** a candidate whose `IsOutsideInitialPlayerSightline = false`, **When** the validator
   runs, **Then** it reports the "within player's initial line of sight" violation.
3. **Given** a candidate with both flags `true` and both User Story 1 distance checks passing,
   **When** the validator runs, **Then** the overall result is valid (zero violations).

---

### User Story 3 - Validator reports every violated rule in a single pass (Priority: P2)

A candidate that fails more than one rule at once (for example, too close to the player **and**
not navigable **and** unsafe from instant death) has every one of its violations named in the
result, not just the first one the validator happens to check.

**Why this priority**: Ranked below Stories 1–2 because it is a quality-of-diagnosis behavior
layered on top of rules that already work individually — but it is still required, because a
level designer iterating on hand-placed candidate points in a floor scene (owned elsewhere, per
Related) needs to fix every problem in one pass instead of playing whack-a-mole with a validator
that only ever reports its first complaint.

**Independent Test**: In an EditMode test, construct a candidate that simultaneously violates the
player-distance rule, the navigability rule, and the instant-death rule (three of five), and
assert the result names exactly those three violations — no fewer, no more, and not merely a
single aggregate boolean.

**Acceptance Scenarios**:

1. **Given** a candidate violating all five rules simultaneously, **When** the validator runs,
   **Then** the result names all five violations.
2. **Given** a candidate violating exactly one rule, **When** the validator runs, **Then** the
   result names exactly that one violation and no others.
3. **Given** a candidate violating zero rules, **When** the validator runs, **Then** the result
   is valid with an empty violation set — never a "valid with warnings" partial state.

---

### User Story 4 - Validator is reusable and callable identically from tests and an editor tool (Priority: P3)

The same validator function used by the automated EditMode test suite is the one an editor-only
Scene-view tool calls to visualize pass/fail for hand-placed candidate points while a level
designer is authoring a floor's spawn presets elsewhere — no second copy of the rule logic exists
anywhere.

**Why this priority**: Enabling infrastructure rather than a rule of its own — lower priority than
Stories 1–3 because it doesn't change *what* is validated, only *how reusable* that validation is
— but it is the reason this spec is written as a standalone, engine-agnostic validator in the
first place, per the task's explicit framing ("reusable VALIDATOR," with actual spawn point
authoring happening elsewhere).

**Independent Test**: Call the validator function directly from an EditMode test with scripted
inputs (no scene) and, separately, from a minimal custom Editor script against an example
candidate in an open scene; assert both call sites produce identical results for identical inputs,
and that the Editor script contains no rule logic of its own — only visualization.

**Acceptance Scenarios**:

1. **Given** identical candidate data and identical `GameConfig`/context inputs, **When** the
   validator is called once from an EditMode test and once from the editor tool, **Then** both
   report the identical result (same violation set).
2. **Given** the editor tool's source code, **When** it is inspected, **Then** it contains no
   distance/navigability/sightline/objective/instant-death threshold logic of its own — it only
   calls the shared validator and renders the result (e.g., as a Scene-view gizmo color).

---

### Edge Cases

- **Boundary distance is a pass, not a violation**: a candidate exactly at
  `monsterSpawnMinDistanceFromPlayer` or `monsterSpawnMinDistanceFromObjective` (not strictly
  less than) satisfies "far enough" and MUST NOT be reported as a violation — matching GDD Ch.
  6.3's "cukup jauh" (far enough) framing, and spec 002's precedent of stating the comparison
  operator explicitly rather than leaving equality ambiguous.
- **Empty objective list is not an error**: a floor context that resolves zero objective
  positions (see User Story 1, Acceptance Scenario 4) must never produce a false violation — this
  matters because a candidate might be validated before all of a floor's keys/doors are placed.
- **No implicit default-to-valid on missing signals**: constructing a candidate MUST require
  `IsNavigable`, `IsOutsideInitialPlayerSightline`, and `IsSafeFromInstantDeath` to be explicitly
  supplied — a caller that has not actually computed one of these yet cannot accidentally get a
  passing result by omission (this is a data-shape requirement, not just a convention — see
  FR-016).
- **Live re-read of thresholds**: `monsterSpawnMinDistanceFromPlayer` and
  `monsterSpawnMinDistanceFromObjective` MUST be read from `GameConfig` fresh on every validator
  call, never cached, so a designer retuning either value in the Inspector takes effect on the
  very next validation without a code change — matching spec 002's precedent for its own
  cross-field hard rule.
- **"Not trapped" is one rule, not two**: GDD Ch. 6.3 phrases navigability and enclosure as a
  single bullet ("Terjangkau secara navigasi (tidak terjebak di ruang tertutup)"); this spec
  treats both as one boolean signal (`IsNavigable`), not two separately-reported violations (see
  Assumptions).
- **Instant-death rule has no fabricated automatable definition**: GDD Ch. 6.3 gives no concrete,
  checkable definition of "instant-death atau situasi yang mustahil dihindari" beyond the phrase
  itself; this spec does not invent one. It only fixes that such a signal exists, is required, and
  gates validity exactly like the other four (see Assumptions, and ROADMAP §0's rule against
  fabricating feel-based/undefined-precision values).
- **A floor with no monster (Floor 52) never calls this validator**: per spec 002 User Story 3,
  Floor 52 has `monsterActive = false` and no `MonsterTuningProfile`; by the same logic, no code
  path ever needs to validate a spawn point for it. This spec does not add a special case for
  Floor 52 — it simply is never invoked for that floor.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define a `MonsterSpawnCandidate` data shape carrying a candidate
  `Position` (`Vector3`) plus three caller-supplied boolean signals — `IsNavigable`,
  `IsOutsideInitialPlayerSightline`, `IsSafeFromInstantDeath` — corresponding to GDD Ch. 6.3's
  navigability, line-of-sight, and instant-death rules, since computing these three requires
  live-scene geometry (NavMesh reachability, sightline occlusion, level-specific hazard analysis)
  that cannot be expressed as pure C# math (constitution Principle IV's stated exception).
- **FR-002**: The validator MUST additionally accept, as call-time context rather than
  per-candidate data, a `PlayerSpawnPosition` (`Vector3`) and an `ObjectivePositions`
  (`IReadOnlyList<Vector3>`) — shared context for every candidate evaluated on a given floor, not
  duplicated per candidate.
- **FR-003**: `GameConfig` MUST expose two new additive tunable `float` fields:
  `monsterSpawnMinDistanceFromPlayer` and `monsterSpawnMinDistanceFromObjective` (meters) — no
  second config source (constitution: single configuration source).
- **FR-004**: The validator MUST report the "too close to player spawn" violation when
  `Distance(candidate.Position, PlayerSpawnPosition) < monsterSpawnMinDistanceFromPlayer`, and
  MUST NOT report it when the distance is greater than or equal to the threshold.
- **FR-005**: The validator MUST report the "too close to objective" violation when the distance
  from `candidate.Position` to the *nearest* entry in `ObjectivePositions` is strictly less than
  `monsterSpawnMinDistanceFromObjective`. An empty `ObjectivePositions` list MUST NOT be treated
  as an error or as an automatic violation — the rule is vacuously satisfied.
- **FR-006**: The validator MUST report the "not navigable / trapped" violation when
  `candidate.IsNavigable = false`.
- **FR-007**: The validator MUST report the "within player's initial line of sight" violation
  when `candidate.IsOutsideInitialPlayerSightline = false`.
- **FR-008**: The validator MUST report the "unsafe from instant death" violation when
  `candidate.IsSafeFromInstantDeath = false`.
- **FR-009**: The validator's result MUST name every violated rule from FR-004 through FR-008
  that applies to a given candidate in one pass — it MUST NOT short-circuit and report only the
  first violation found.
- **FR-010**: A candidate is valid if and only if zero rules from FR-004 through FR-008 are
  violated. There is no partial-pass, warning-only, or "valid with caveats" result state.
- **FR-011**: The validator itself MUST be implemented as a plain C# class/function with no
  `MonoBehaviour`, `Component`, or scene-graph dependency (constitution Principle III), taking
  `MonsterSpawnCandidate`, the two `GameConfig` thresholds, `PlayerSpawnPosition`, and
  `ObjectivePositions` as pure inputs and returning a pure result. FR-004/FR-005 (purely
  geometric) and FR-009/FR-010 (aggregation/reporting) MUST be fully EditMode-testable
  regardless of how the three FR-001 boolean signals were computed upstream.
- **FR-012**: This spec MUST NOT define how `IsNavigable`, `IsOutsideInitialPlayerSightline`, or
  `IsSafeFromInstantDeath` are computed (NavMesh queries, raycast occlusion, hazard analysis) —
  that responsibility belongs to an editor-time tool/adapter and to each floor scene's own
  spawn-preset authoring spec (see Related). This spec fixes only the shape of the signal it
  consumes and the pass/fail rule built on it.
- **FR-013**: This spec MUST NOT select, store, register, or randomize among valid spawn points
  at runtime. The "posisi awal monster diambil acak dari beberapa preset spawn point yang sudah
  divalidasi" (GDD Ch. 6.3) selection/registry behavior belongs to
  `monster-ai/001-state-machine-core-transitions`'s Assumptions (initial spawn position comes
  from a validated preset chosen by the floor scene) and to each floor scene's own spec (see
  Related). This spec only defines what makes one candidate point valid.
- **FR-014**: The validator function MUST be callable identically from an automated EditMode test
  and from an editor-only tool/context (e.g., a custom Editor script visualizing candidates in
  the Scene view) — zero rule logic duplicated between the two call sites; the editor tool calls
  the exact same function.
- **FR-015**: `monsterSpawnMinDistanceFromPlayer` and `monsterSpawnMinDistanceFromObjective` MUST
  be read from `GameConfig` fresh at validation time on every call — never cached — so a designer
  retuning either value takes effect on the very next validator call with zero code changes.
- **FR-016**: Constructing a `MonsterSpawnCandidate` MUST require all three boolean signals
  (`IsNavigable`, `IsOutsideInitialPlayerSightline`, `IsSafeFromInstantDeath`) to be explicitly
  supplied by the caller — the data shape MUST NOT provide a default value that silently resolves
  to "valid" if a caller has not actually computed one of them yet.

### Key Entities

- **`MonsterSpawnCandidate`**: the position-plus-three-booleans data shape defined in FR-001.
  Lives in `Assets/Scripts/Systems/MonsterAI/`.
- **`MonsterSpawnRuleViolation`** (flags enum or equivalent set type): one member per rule from
  FR-004 through FR-008 — `TooCloseToPlayerSpawn`, `TooCloseToObjective`, `NotNavigable`,
  `WithinInitialPlayerSightline`, `UnsafeInstantDeath` — used to report every violated rule
  (FR-009).
- **`MonsterSpawnValidationResult`**: `IsValid` (bool) plus the set of `MonsterSpawnRuleViolation`
  values present (empty when valid).
- **`MonsterSpawnPointValidator`** (plain C# class/static function): implements FR-004 through
  FR-010, taking `MonsterSpawnCandidate`, `GameConfig`, `PlayerSpawnPosition`, and
  `ObjectivePositions` and returning `MonsterSpawnValidationResult`. No engine dependency
  (FR-011).
- **`GameConfig` (extended)**: gains the two new additive threshold fields from FR-003. This spec
  does not redefine `GameConfig`'s overall schema (owned by
  `shared-config-and-state/001-game-config-schema`), only adds to it.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every one of the five rules documented in GDD Ch. 6.3 has at least one automated
  test proving both a violating case and a passing case, for a total of at least ten scenarios
  covered.
- **SC-002**: For candidates violating exactly 1, exactly 3, and all 5 rules simultaneously, the
  validator's reported violation set matches the expected set exactly, in 100% of automated test
  runs — proving the "report all, never short-circuit" guarantee (FR-009) at more than one
  violation count.
- **SC-003**: Retuning `monsterSpawnMinDistanceFromPlayer` or
  `monsterSpawnMinDistanceFromObjective` in `GameConfig` changes the validator's pass/fail outcome
  for an unchanged candidate, with zero code changes to `MonsterSpawnPointValidator` — verified by
  an automated test, not by inspection.
- **SC-004**: The validator function, called with identical inputs from an EditMode test and from
  the editor-only visualization tool, produces identical results in 100% of a manually-verified
  spot check (per constitution Principle IV's live-scene-tooling exception) — zero divergent
  logic paths.
- **SC-005**: An empty `ObjectivePositions` list never produces a "too close to objective"
  violation, verified by an automated test, across at least one candidate position that would
  otherwise be flagged if the list were non-empty and included a coincident point.

## Assumptions

- **Three of five rules are caller-supplied booleans, not computed here** — consistent with
  `monster-ai/001`'s precedent of treating engine-geometry-dependent input (there, the noise
  detection signal; here, NavMesh reachability, sightline occlusion, and instant-death hazard
  analysis) as an opaque signal this layer consumes, per constitution Principle IV's explicit
  exception for logic needing a live scene, physics, or rendering. This keeps the actual rule
  (thresholds, aggregation, reporting) deterministic and EditMode-testable independent of how
  expensive or engine-specific the upstream geometry check is.
- **"Not trapped" folds into a single `IsNavigable` signal** rather than being split into a
  separate rule, because GDD Ch. 6.3 phrases reachability and enclosure as one bullet point. If a
  future revision needs to distinguish "reachable but sealed in with no second exit" from "simply
  unreachable," that is an additive change to `MonsterSpawnCandidate` (a new boolean or a richer
  enum), not a redesign of this spec (constitution Principle V).
- **The instant-death rule is deliberately left without a fabricated automatable definition.**
  GDD Ch. 6.3 states the requirement ("tidak menciptakan instant-death atau situasi yang mustahil
  dihindari") without specifying how to detect it mechanically. Per ROADMAP §0's explicit
  direction not to invent a fake precise rule where none is validated, this spec only requires
  that *some* signal (`IsSafeFromInstantDeath`) be supplied and gates on it identically to the
  other four rules; the editor tool and/or a level designer's manual review are expected to be
  the actual source of that signal until/unless a future spec defines an automatable heuristic.
- **"Player's initial line of sight" is a single fixed snapshot**, taken at the player's
  floor-entry position and facing, not continuously recalculated — matching GDD Ch. 6.3's phrase
  "garis pandang awal player" (the player's *initial* line of sight, not an ongoing check).
- **Objective positions include keys and the floor's locked/exit doors**, matching GDD Ch. 6.3's
  explicit examples ("key, pintu turun"); on Floor 50 this includes the Final Door (GDD Ch. 8.1).
  This spec does not itself resolve that list — it is supplied by the caller as
  `ObjectivePositions`, sourced from `keys-and-doors/*` at whatever point in floor load the
  caller chooses.
- **This spec never runs for Floor 52** — Floor 52 has no monster (GDD Ch. 6.3, spec 002 User
  Story 3), so no candidate spawn point is ever validated for it; this spec adds no explicit
  Floor 52 special case because none is needed.

## Related

- GDD Ch. 6.3 (Monster AI Specification — Presence & Spawn) — the five validity-rule bullets this
  spec implements verbatim.
- `specs/ROADMAP.md` §2 "monster-ai" row `003-spawn-point-validation-rules` (depends on `001`),
  and §0's shared-naming/feel-based-value conventions (the instant-death rule's deliberate lack of
  a fabricated definition follows this section's explicit guidance).
- `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` — its Assumptions section
  states the `Monster`'s initial world position on floor load comes from a validated preset
  spawn point chosen by the floor scene; this spec is what makes a preset "validated" in the
  first place, but does not itself choose or store presets.
- `specs/scenes/005-floor-51-scene/002-monster-patrol-route-and-spawn-presets` and
  `specs/scenes/006-floor-50-scene/002-monster-patrol-route-and-spawn-presets` — the
  floor-specific specs where actual candidate spawn points are hand-authored and run through this
  validator; this spec defines the rule, they do the authoring.
- `specs/systems/keys-and-doors/*` — the eventual source of the `ObjectivePositions` list this
  validator's objective-distance rule (FR-005) consumes.
