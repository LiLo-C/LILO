# Feature Specification: Active Battery Count Cap

**Feature Branch**: `002-active-battery-count-cap`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 3.1 ("Sistem menjaga jumlah battery aktif di map tidak melebihi batas floor
tersebut (Floor 51 = 2, Floor 50 = 1)"), Ch. 17.2 (`batteryMaxActiveFloor51` = 2,
`batteryMaxActiveFloor50` = 1), Ch. 1.3 (Meaningful Resource pillar), Ch. 18.3 (cut-candidate #2)

**Input**: User description: "The system keeps the count of currently-active loose batteries in
the world at or below that floor's configured maximum (Floor 51 = 2, Floor 50 = 1, per GDD —
these specific numbers are floor config values, cite them as GameConfig fields per-floor, don't
hardcode)."

## Scope Note

This spec answers exactly one question on demand: **"is this floor's currently-active loose
battery count at or above its configured maximum right now?"** It does not decide *when* to
spawn a new battery or *where* — that is `003-respawn-timer-and-placement-rule`'s job, which
calls into this spec's cap check before it is allowed to spawn anything. This spec also does not
decide which numeric value applies to which specific floor scene — the numbers below are cited as
the GDD's own defaults, but the field definitions live in
`shared-config-and-state/001-game-config-schema`, and per-floor wiring is confirmed in each
floor's own scene spec.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - An Always-Accurate Active Count (Priority: P1)

As the battery-spawn-system, I need to always know exactly how many loose `Battery` objects are
currently active in the world for the floor the player is on, so that cap enforcement is never
based on stale or guessed data.

**Why this priority**: Every other story in this spec is a query or config lookup layered on top
of this count. If the count itself drifts from reality, the "Meaningful Resource" design pillar
(GDD Ch. 1.3 — "jumlah battery makin sedikit tiap floor") breaks silently: either the world
over-fills with batteries (destroys scarcity) or under-fills (creates an unfair, unintended
softlock risk).

**Independent Test**: In a test harness, spawn a Battery (count should become 1), spawn another
(count 2), have the player pick one up / despawn it (count 1 again). At every step, assert the
tracked count exactly matches the number of loose Battery instances the harness itself created
and has not yet removed.

**Acceptance Scenarios**:

1. **Given** zero active batteries tracked for a floor, **When** one Battery becomes active in
   the world for that floor, **Then** the tracked count becomes 1.
2. **Given** a tracked count of N for a floor, **When** one of those Battery instances is picked
   up (removed from the world), **Then** the tracked count becomes N-1 immediately — the very
   next query reflects the change, not a later one.
3. **Given** batteries active across two different floors, **When** one floor's count changes,
   **Then** the other floor's tracked count is unaffected.

---

### User Story 2 - A Simple Cap Check (Priority: P1)

As the respawn system (`003`), I need one simple yes/no answer — "is this floor at or above its
cap right now" — so I can gate spawning without re-deriving the per-floor maximum or re-counting
batteries myself every time.

**Why this priority**: This is the actual API surface `003` depends on; without it, `003` would
have to duplicate this spec's config-lookup and counting logic, which is exactly the kind of
scattered-tuning-numbers problem GDD Ch. 17's "SEMUA angka tuning di satu tempat" rule exists to
prevent. Tied at P1 with User Story 1 because the two are only useful together, but User Story 1
is the prerequisite the count check reads from.

**Independent Test**: Configure Floor 51's cap at 2 (its GDD default). Register 2 active
batteries for Floor 51. Assert the cap check reports "at or above cap" = true. Remove one
(simulate pickup). Assert it now reports false.

**Acceptance Scenarios**:

1. **Given** Floor 51's configured maximum is 2 and its tracked active count is 2, **When** the
   cap check runs, **Then** it reports "at or above cap".
2. **Given** the same floor with a tracked count of 1 (below its max of 2), **When** the cap
   check runs, **Then** it reports "below cap".
3. **Given** Floor 50's configured maximum is 1 and its tracked active count is 1, **When** the
   cap check runs, **Then** it reports "at or above cap" (confirms the per-floor maximum, not a
   single global number, drives the result).

---

### User Story 3 - The Cap Number Comes From Config, Not Code (Priority: P2)

As a designer tuning balance during playtesting, I need the per-floor maximum this system checks
against to be read from `GameConfig` (per GDD Ch. 17.2's `batteryMaxActiveFloor51` /
`batteryMaxActiveFloor50` fields) rather than baked into a code literal, so I can change the
number without a code change or rebuild.

**Why this priority**: Functionally, User Stories 1 and 2 already require *some* source for the
maximum — this story exists to make that source explicitly swappable/testable on its own,
matching the constitution's single-configuration-source rule. It is P2 rather than P1 because the
system is still internally correct even before this is proven — it just wouldn't yet be provably
config-driven.

**Independent Test**: In an EditMode test, set a fixture `GameConfig`'s Floor 51 maximum to 2,
run the cap check at count=2 (expect "at cap"), then change the fixture's value to 3 with no
code change and re-run the same check at count=2 (expect "below cap").

**Acceptance Scenarios**:

1. **Given** `GameConfig.batteryMaxActiveFloor51` = 2, **When** the cap lookup runs for Floor 51,
   **Then** the resolved maximum is 2.
2. **Given** the same config value is changed to a different number, **When** the lookup runs
   again with no code change, **Then** the resolved maximum reflects the new number.

---

### Edge Cases

- **Active count already exceeds the configured maximum** at check time (e.g., a bug elsewhere,
  or a debug scene with an extra hand-placed battery pushing the floor over its cap): the system
  MUST report "at or above cap" (blocking further spawns) and MUST NOT despawn or otherwise
  remove any existing active `Battery` instance to force the count back down — silently deleting
  a battery a player can already see would be a worse, more jarring bug than a temporarily
  over-full floor.
- **Configured maximum is zero or negative** (misconfiguration): MUST be treated as "always at
  cap" — i.e., never allows a spawn — rather than throwing an exception or being interpreted as
  "no limit."
- **Cap queried for a floor with no configured maximum at all** (e.g., an unmapped debug/test
  floor, or a floor that intentionally never uses this system such as Floor 52): MUST fail safe
  toward scarcity, reporting "at or above cap" (no spawns), never "unlimited." This mirrors
  `001`'s rule that an unregistered floor is a normal, non-error state.
- **A battery is picked up and a cap check happens in the very same frame/update**: the count
  decrement MUST be applied synchronously with the pickup event, so that cap check sees the
  post-pickup count, not a stale pre-pickup value — otherwise a respawn could be denied for one
  extra frame/tick right after room opened up.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST track the number of currently-active loose `Battery` instances in the
  world, scoped per floor — a battery counts only toward the floor it belongs to.
- **FR-002**: System MUST increment the tracked count for a floor the instant a loose `Battery`
  becomes active in the world on that floor (spawned or otherwise placed), and MUST decrement it
  the instant a `Battery` is removed from the world on that floor (picked up by the player, or
  otherwise despawned) — both synchronously with the triggering event, not on a delay.
- **FR-003**: System MUST expose a query that returns whether the current active count for a
  given floor is at or above that floor's configured maximum.
- **FR-004**: The per-floor maximum MUST be read from `GameConfig` via a per-floor lookup (per
  GDD Ch. 17.2 defaults: `batteryMaxActiveFloor51` = 2, `batteryMaxActiveFloor50` = 1) — this
  system MUST NOT contain a hardcoded numeric literal for any floor's maximum.
- **FR-005**: This spec's lookup MUST be able to resolve a maximum for any floor identifier
  `GameConfig` exposes one for. Deciding exactly which numeric value is assigned to which specific
  floor *scene* is confirmed by each floor scene's own spec and by
  `shared-config-and-state/001-game-config-schema`'s field definitions — not by this spec.
- **FR-006**: If the active count already exceeds the configured maximum at check time, the
  system MUST report "at or above cap" and MUST NOT remove or destroy any existing active
  `Battery` instance as a side effect of that check.
- **FR-007**: A configured maximum of zero or a negative number MUST be treated as "always at
  cap," never as an error or as "no limit."
- **FR-008**: Querying the cap for a floor identifier with no configured maximum MUST fail safe
  by reporting "at or above cap," never "unlimited."
- **FR-009**: The counting and cap-check logic MUST be implemented as plain C# taking
  `GameConfig`-shaped data as input, with no `MonoBehaviour`/`Component`/scene dependency
  (constitution Principle III). A thin `MonoBehaviour` adapter MUST subscribe to `Battery`
  lifecycle events (becomes active / removed) in the scene and forward them into the plain C#
  counter.
- **FR-010**: This spec MUST NOT itself decide when or where a new battery spawns — it only
  answers "is the floor at or over its cap right now." Spawn timing and placement are
  `003-respawn-timer-and-placement-rule`'s responsibility.

### Key Entities

- **ActiveBatteryCountTracker**: Per-floor active count. Exposes increment/decrement operations
  and a current-count read.
- **BatteryCountCapPolicy**: Combines a tracker's current count with a per-floor maximum read
  from `GameConfig` to answer "at or above cap" for a given floor.
- **Battery lifecycle signal**: The "became active" / "was removed" event this system consumes,
  produced by `flashlight-and-battery/007-battery-pickup-and-spare-slot`'s `Battery` object (on
  pickup) and by `003`'s spawner (on spawn) — this spec defines only how it reacts to that
  signal, not the signal's own origin.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Across 100% of EditMode test runs, the tracked active count for a floor exactly
  matches the number of currently-active (not-yet-removed) `Battery` instances the test harness
  registered, after any sequence of spawn/pickup operations.
- **SC-002**: On a floor configured with cap = 2 (Floor 51's default), once exactly 2 batteries
  are simultaneously active, the cap check reports "at or above cap" on the very next check —
  zero false negatives across repeated, randomized spawn/pickup orderings in tests.
- **SC-003**: Changing a floor's configured maximum in `GameConfig` changes the cap check's
  result with zero source-code changes, verified by an EditMode test that swaps the config value
  between two assertions.
- **SC-004**: A floor whose active count exceeds its configured maximum (the over-cap edge case)
  never causes an existing `Battery` instance to be removed by this system — verified by an
  EditMode test that pushes count above max and then asserts the tracked count is unchanged by
  the cap check itself.

## Assumptions

- **This category is Cut-Candidate #2 (GDD Ch. 18.3)**: "Battery respawn → ganti statis." If the
  respawn system is toggled off, this count-cap system simply stops being called by anything —
  it must not be a load-bearing dependency for static battery placement to work. A floor using
  only static, hand-placed batteries (e.g., Floor 52 per GDD Ch. 3.1) never needs this cap check
  to function correctly, and this spec's failure-safe defaults (Edge Cases above) guarantee that
  an unqueried or unconfigured floor never errors.
- **Per-floor value wiring is out of scope here.** This spec assumes `GameConfig` exposes a
  per-floor maximum lookup (fields named per GDD Ch. 17.2:
  `batteryMaxActiveFloor51`/`batteryMaxActiveFloor50`) — defining those fields is
  `shared-config-and-state/001-game-config-schema`'s job; confirming which floor *scene* reads
  which field is each floor scene spec's job
  (`scenes/005-floor-51-scene/004-battery-spawn-point-placement`,
  `scenes/006-floor-50-scene/005-battery-spawn-point-placement`).
- **Floor 52 is out of scope for this cap.** Per GDD Ch. 3.1, Floor 52 does not use the respawn
  system at all (3–5 batteries placed statically). This spec does not need a
  `batteryMaxActiveFloor52` concept — Floor 52's `batteryCountFloor52` (GDD 17.2) is a static
  placement count, not an active-cap ceiling this system enforces.
- Depends on `001-per-floor-spawn-point-registry` only insofar as both are part of the same
  system category and share the floor-identifier assumption; this spec does not directly query
  `001`'s registry (it counts live `Battery` instances, not spawn points).

## Related

- GDD Ch. 3.1 ("Aturan spawn battery"), Ch. 17.2 (GameConfig battery fields), Ch. 1.3 (Meaningful
  Resource pillar), Ch. 18.3 (Scope Lock — cut-candidate #2) —
  `specs/_reference/LILO-GDD-v2-Production-Lock.md`
- `specs/ROADMAP.md` §0 (naming conventions — `GameConfig` single source of truth), §2
  (`battery-spawn-system` row), §5 (build order: depends on `001`, feeds `003`)
- `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/spec.md` — the
  `Battery` object whose lifecycle this spec's count tracker reacts to
- `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md` — sibling spec
  this one shares a floor-identifier assumption with
- `specs/systems/battery-spawn-system/003-respawn-timer-and-placement-rule/spec.md` — the
  consumer that calls this spec's cap check before spawning
