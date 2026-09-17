# Feature Specification: Per-Floor Battery Spawn Point Registry

**Feature Branch**: `001-per-floor-spawn-point-registry`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 3.1 ("Aturan spawn battery" — "Setiap floor punya daftar Battery Spawn Point yang sudah ditentukan manual di level design"), Ch. 16.2 (blueprint template — Battery spawn points sit inside the Exploration Zone), Ch. 16.3 (level validation checklist — "Battery spawn point tersebar, tidak menumpuk di satu sisi map"), Ch. 18.3 (cut-candidate #2)

**Input**: User description: "A registry of Battery Spawn Points authored per floor in level data (hand-placed, not random) that the battery-spawn-system reads from. This spec defines the data shape/contract — a list of spawn points per floor — not the actual Floor 51/Floor 50 coordinates, which belong to each floor scene's own spec."

## Scope Note

This spec defines the **data shape and query contract** for a per-floor collection of hand-authored
Battery Spawn Points. It intentionally does **not** decide how many spawn points Floor 51 or
Floor 50 have, or where they sit in the world — those are level-design decisions authored in
`scenes/005-floor-51-scene/004-battery-spawn-point-placement` and
`scenes/006-floor-50-scene/005-battery-spawn-point-placement` respectively. This spec only has to
be correct for *any* number of hand-placed points on *any* floor, including zero.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A Reliable List of Candidate Locations (Priority: P1)

As the battery-spawn-system, when a floor loads, I need a per-floor list of hand-authored Battery
Spawn Points, exactly as level design placed them — never randomly generated — so that
`002-active-battery-count-cap` and `003-respawn-timer-and-placement-rule` always have a fixed,
valid set of candidate locations to choose from.

**Why this priority**: This is the foundational data contract. Neither the count cap nor the
respawn timer/placement rule can function without a source of truth for "where batteries are
allowed to appear." Every other story in this feature category depends on this one existing
first (see ROADMAP.md §2, §5).

**Independent Test**: Author 3 spawn point markers in a test scene, each tagged to the same floor
identifier. Query the registry for that floor identifier and confirm it returns exactly 3 entries
whose positions match the authored transforms. Query the registry for a different, unrelated
floor identifier and confirm it returns an empty list.

**Acceptance Scenarios**:

1. **Given** a floor scene with N hand-placed spawn point markers tagged to floor F, **When** the
   scene loads and the registry is queried for floor F, **Then** the registry returns exactly N
   spawn points matching the authored positions.
2. **Given** the same registry, **When** it is queried for a floor identifier that has no
   authored markers, **Then** it returns an empty list, not an error.
3. **Given** a registry already populated for floor F, **When** a different floor F2 loads,
   **Then** querying floor F afterward reflects only F's own markers — F2's load does not leak
   entries into F's list or vice versa.

---

### User Story 2 - Tracking Which Points Are Currently Free (Priority: P2)

As the battery-spawn-system, I need each registered spawn point to carry an occupancy state
(empty vs. currently holding an active battery) so that respawn logic can ask "which spawn points
are free right now" without re-scanning the scene for battery objects on every check.

**Why this priority**: Required by `002` and `003` to avoid double-placing a battery on an
already-occupied point, but the registry could in principle exist as a read-only list without
this — it is the second layer of the same contract, not a separate foundation.

**Independent Test**: Register 2 spawn points for a floor (both default to empty). Mark one
occupied. Query "empty spawn points" for that floor and confirm only the unmarked one is
returned. Mark it empty again and confirm it reappears.

**Acceptance Scenarios**:

1. **Given** a newly registered spawn point, **When** no occupancy operation has been performed
   on it yet, **Then** it reports as empty by default.
2. **Given** a spawn point marked occupied, **When** the "empty spawn points for floor F" query
   runs, **Then** that point is excluded from the result.
3. **Given** a spawn point previously marked occupied, **When** it is marked empty again,
   **Then** it reappears in the "empty spawn points" query result.

---

### User Story 3 - Authoring Mistakes Fail Loudly (Priority: P3)

As a developer running EditMode tests, I need the registry to reject an obviously malformed
authoring input — such as two spawn points sharing the same identifier on the same floor — so a
level-design mistake is caught in a fast test, not discovered as a silent duplicate at runtime on
a device.

**Why this priority**: Robustness/quality-of-life. It does not gate the core respawn loop working
(P1/P2 already deliver a playable system), but it materially reduces debugging time for the level
designers who hand-author these points (GDD Ch. 16.1: "Setiap floor digambar manual di kertas
dulu, sebelum dibangun di engine" — manual authoring is expected to be error-prone without a
guard rail).

**Independent Test**: Register two spawn points with the same identifier for the same floor in an
EditMode test and confirm the registry rejects/flags the second registration (logs an error and
does not silently create a duplicate live entry) rather than accepting it.

**Acceptance Scenarios**:

1. **Given** a spawn point with identifier "A" already registered for floor F, **When** a second
   spawn point also identifier "A" is registered for floor F, **Then** the registry rejects the
   second registration and the floor's list still contains exactly one entry for "A".
2. **Given** the same identifier used on two *different* floors, **When** both are registered,
   **Then** this is valid — identifiers only need to be unique within a floor, not globally.

---

### Edge Cases

- **A floor has zero authored spawn points** (e.g., a misconfigured or in-progress level): the
  registry MUST return an empty list for that floor without throwing. `003`'s respawn logic must
  be able to treat "no candidates" as "never spawns on this floor" rather than crashing — this is
  the same code path that makes Floor 52's deliberate non-use of this system (see Assumptions and
  `003`'s Edge Cases) safe by construction.
- **The same spawn point identifier registered twice for the same floor**: treated as an
  authoring error (User Story 3) — rejected, not silently duplicated.
- **A floor identifier that was never registered at all is queried** (e.g., a floor that
  legitimately has no respawn registry entries because it uses static placement, like Floor 52):
  the registry MUST return an empty list, exactly like the "zero spawn points" case above — there
  is no distinct "unknown floor" error state, because from this registry's point of view an
  unregistered floor and a floor with zero authored points are indistinguishable and both are
  valid, non-error conditions.
- **The registry is queried before a floor scene has finished loading its spawn point markers**:
  it MUST return whatever has been registered so far (consistently empty at that point), never
  stale data left over from a previously loaded floor — the registry MUST be rebuilt (its
  per-floor contents cleared) on floor load, not accumulated across floor transitions.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST define a `BatterySpawnPoint` data shape with, at minimum: a stable
  identifier (unique within its floor), the floor it belongs to, a world position, and a current
  occupancy state (empty or occupied).
- **FR-002**: System MUST provide a per-floor registry that stores an arbitrary number of
  hand-authored `BatterySpawnPoint` entries. Entries MUST come only from level data (scene-placed
  markers) — the registry itself MUST NOT generate, randomize, or infer spawn point positions.
- **FR-003**: The registry MUST expose a query that returns all spawn points currently registered
  for a given floor identifier.
- **FR-004**: The registry MUST expose a query that returns only the currently-empty spawn points
  for a given floor identifier.
- **FR-005**: The registry MUST expose an operation to mark a specific spawn point occupied and a
  separate operation to mark it empty. A spawn point's occupancy MUST default to empty at the
  moment it is first registered.
- **FR-006**: The registry MUST clear and rebuild its per-floor contents when a new floor loads,
  so no stale spawn points or occupancy state from a previous floor persist. This also supports
  GDD Ch. 9.2's requirement that battery-in-map state fully resets to the floor's starting
  condition on player death — the registry's occupancy state MUST be resettable to "all empty"
  for that purpose (the reset trigger itself belongs to
  `lives-and-fail-state/002-floor-state-reset-on-death`; this spec only guarantees the registry
  exposes a reset operation for that consumer to call).
- **FR-007**: Registering a duplicate spawn point identifier for the same floor MUST be treated
  as an authoring error: the registry MUST reject the duplicate (log the conflict, keep the
  original entry, do not add a second live entry) rather than silently overwriting or duplicating
  it.
- **FR-008**: The data shape and registry MUST be plain C# with no `MonoBehaviour`, `Component`,
  or scene-graph dependency (constitution Principle III), so they are usable and testable in
  EditMode without a running scene.
- **FR-009**: A thin `MonoBehaviour` adapter MUST be responsible for reading scene-placed marker
  `Transform`s at floor-load time and feeding their positions/identifiers into the plain C#
  registry — the registry itself never reads a `Transform` directly.
- **FR-010**: This spec MUST NOT define actual spawn point coordinates, counts, or layout for any
  specific floor. Those values are authored per floor scene spec (see Related).
- **FR-011**: A floor MAY have zero registered spawn points (e.g., Floor 52, which per GDD Ch.
  3.1 uses static battery placement instead of this respawn-driven registry). The registry MUST
  support this without error, as covered in Edge Cases.

### Key Entities

- **BatterySpawnPoint**: A hand-authored candidate location for a battery to appear. Attributes:
  identifier (unique within its floor), floor identifier, world position, occupancy state
  (empty/occupied).
- **BatterySpawnPointRegistry**: The per-floor collection of `BatterySpawnPoint` entries. Exposes
  queries (all points for a floor, empty points for a floor) and mutations (mark occupied, mark
  empty, clear/rebuild for a floor).
- **Floor identifier**: A value identifying which floor (Floor 52 / Floor 51 / Floor 50) a spawn
  point and registry query belong to. This spec consumes such an identifier but does not own its
  definition — see Assumptions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Given N hand-placed spawn point markers authored for a floor, the registry returns
  exactly N entries for that floor and 0 entries for every other floor, across 100% of EditMode
  test runs.
- **SC-002**: Marking any spawn point occupied removes it from that floor's "empty spawn points"
  query result on the very next call, with zero flaky failures across repeated runs of the same
  EditMode test.
- **SC-003**: Querying a floor with zero authored spawn points produces zero exceptions/errors —
  this is the same guarantee that lets Floor 52 coexist with this system unused (Ch. 18.3
  toggle-off case), verified directly by an EditMode test that never registers any point for a
  given floor identifier and only calls the query methods.
- **SC-004**: Registering a duplicate identifier on the same floor is rejected in 100% of trials,
  and the floor's registered-point count never exceeds the number of *distinct* identifiers
  registered for it.

## Assumptions

- **This category is Cut-Candidate #2 (GDD Ch. 18.3)**: "Battery respawn → ganti statis." The
  registry defined here MUST remain safely inert if the whole battery-spawn-system is toggled off
  in favor of static battery placement — a floor with the respawn system disabled simply never
  queries this registry (or queries it and gets/uses an empty list harmlessly, per FR-011 and the
  Edge Cases above). Nothing outside `battery-spawn-system` (in particular
  `flashlight-and-battery/007-battery-pickup-and-spare-slot`'s `Battery` object) may take a
  dependency on this registry existing or being populated — Battery pickup/install logic must
  work identically whether a given Battery came from this respawn registry or from a hand-placed
  static Battery instance.
- **Floor identifier ownership**: this spec assumes a shared floor identifier type (however it is
  ultimately represented — enum or equivalent) is available from `shared-config-and-state` and/or
  `progression-and-scene-flow/001-floor-numbering-and-splash-text`, which own floor numbering.
  This spec consumes that identifier to scope registry entries per floor; it does not define or
  own the identifier type itself. Implementers picking up this spec before that shared type
  exists should coordinate rather than inventing a second, competing floor-identifier type.
- **Actual spawn point positions/counts per floor are out of scope here.** Floor 51's "max 2
  active, spread across the map" and Floor 50's "max 1 active, rarer" placements (GDD Ch. 3
  table) are authored in `scenes/005-floor-51-scene/004-battery-spawn-point-placement` and
  `scenes/006-floor-50-scene/005-battery-spawn-point-placement`.
- **Map is fixed, not procedural** (GDD Ch. 16.1), so spawn points are authored once per floor and
  do not change shape at runtime — only their occupancy state changes.

## Related

- GDD Ch. 3.1 ("Aturan spawn battery") and Ch. 18.3 (Scope Lock — cut-candidate #2) —
  `specs/_reference/LILO-GDD-v2-Production-Lock.md`
- `specs/ROADMAP.md` §0 (naming conventions), §2 (`battery-spawn-system` row), §5 (build order:
  depends on `flashlight-and-battery/007-battery-pickup-and-spare-slot`; feeds `002` and `003`)
- `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/spec.md` — defines the
  `Battery` object this registry's spawn points ultimately host; this spec does not redefine it
- `specs/systems/battery-spawn-system/002-active-battery-count-cap/spec.md` — reads this
  registry's occupancy state and per-floor point counts
- `specs/systems/battery-spawn-system/003-respawn-timer-and-placement-rule/spec.md` — reads this
  registry's empty spawn points and mutates their occupancy on spawn
