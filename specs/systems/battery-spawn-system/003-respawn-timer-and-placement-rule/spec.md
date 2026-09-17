# Feature Specification: Respawn Timer and Placement Rule

**Feature Branch**: `003-respawn-timer-and-placement-rule`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 3.1 ("Kalau jumlah battery aktif di bawah batas, timer respawn berjalan (30
detik / 60 detik). Saat timer habis, satu battery muncul di spawn point acak yang saat itu kosong
dan tidak berada di dalam radius pandang player... Battery tidak pernah muncul di depan mata
player... Floor 52 tidak memakai respawn sama sekali"), Ch. 12 (flashlight is a single
omnidirectional point light — radius spreads evenly around the player, not a directional beam),
Ch. 17.2 (`batteryRespawnFloor51` = 30, `batteryRespawnFloor50` = 60), Ch. 18.3 (cut-candidate
#2)

**Input**: User description: "When active count is below the cap, a respawn timer runs (30s
Floor 51 / 60s Floor 50 per GDD); when it elapses, one battery spawns at a random currently-EMPTY
registered spawn point that is NOT within the player's current view/light radius. Floor 52
explicitly does NOT use this respawn system at all — it uses static battery placement only."

## Scope Note

This spec is the orchestrator that ties `001-per-floor-spawn-point-registry` and
`002-active-battery-count-cap` together into the actual respawn behavior: run a timer, and when
it elapses, pick a valid point and spawn. It owns the timer and the "which point is a valid
target right now" placement rule; it does not own the spawn point data (`001`) or the cap check
(`002`). It also explicitly does not apply to Floor 52, which never runs this timer at all.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A New Battery Eventually Appears (Priority: P1)

As a player exploring Floor 51 or Floor 50, after I've picked up enough batteries to bring the
active count below the floor's cap, I want a new battery to eventually appear somewhere else in
the world, so scarcity is temporary pressure, not permanent depletion (GDD Ch. 1.3, "Meaningful
Resource").

**Why this priority**: This is the core behavior the entire `battery-spawn-system` category
exists to deliver. Without it, a floor could be picked clean and become uncompletable, which
directly contradicts GDD Ch. 3's design (Floor 51/50 are meant to stay resource-pressured
indefinitely, not run dry).

**Independent Test**: In a test floor configured with cap = 1 and exactly one empty, out-of-view
spawn point, start with the active count already at 0 (below cap). Advance simulated time by the
floor's configured respawn duration. Assert exactly one battery spawn decision fires, targeting
that spawn point.

**Acceptance Scenarios**:

1. **Given** a floor's active count is below its configured cap, **When** no time has yet
   passed, **Then** the respawn timer is running (counting toward the floor's configured
   duration), not idle.
2. **Given** the timer has been running below-cap for less than the floor's configured duration,
   **When** a spawn-decision check happens, **Then** no battery spawns yet.
3. **Given** the timer reaches the floor's full configured duration (30s for Floor 51, 60s for
   Floor 50, by GDD Ch. 17.2 default) while at least one empty, out-of-view spawn point exists,
   **When** the timer elapses, **Then** exactly one battery spawns at a valid spawn point.
4. **Given** the active count reaches or exceeds the floor's cap while the timer is still
   running, **When** that happens, **Then** the timer stops counting toward a spawn (per `002`'s
   cap check) until the count drops below cap again.

---

### User Story 2 - Never In Front of the Player's Eyes (Priority: P2)

As a player, I never want to watch a battery pop into existence inside my flashlight's visible
radius, because the GDD is explicit that this "cuts the illusion and feels artificial" (Ch. 3.1:
"Battery tidak pernah muncul di depan mata player — ini memutus ilusi dan bikin terasa
artifisial").

**Why this priority**: A design rule the GDD calls out by name as important to preserve, but it
is layered on top of User Story 1's basic timer-and-spawn mechanism existing at all — hence P2,
not P1.

**Independent Test**: Configure two empty spawn points on a floor: one inside the player's
current view radius, one outside it. Run the respawn selection many times (each trial resets the
timer/count to a spawn-eligible state). Assert that across all trials, the in-view point is never
chosen and the out-of-view point is chosen every time (with only one valid candidate, selection
is deterministic — see User Story 3's Independent Test for the multi-candidate random case).

**Acceptance Scenarios**:

1. **Given** two empty spawn points, one within the player's current view radius and one
   outside it, **When** the respawn timer elapses, **Then** the battery spawns only at the
   out-of-view point.
2. **Given** an empty spawn point exactly at the boundary of the player's view radius (distance
   equal to the radius), **When** eligibility is evaluated, **Then** the point is treated as
   in-view (not eligible) — the boundary itself counts as visible, erring toward the stricter,
   GDD-mandated reading of "not within radius."
3. **Given** the "in view" test, **When** it is evaluated in an EditMode test, **Then** it
   requires only a player position, a view radius, and a spawn point position as plain inputs —
   no live `Camera`, `Light`, or rendering step is needed to get a correct answer (this follows
   directly from the flashlight being a single omnidirectional point light per GDD Ch. 12: "radius
   memancar rata ke segala arah dari posisi player, bukan beam terarah" — so "in view" reduces to
   a simple distance check, not a frustum/facing-direction check).

---

### User Story 3 - Every Empty Spawn Point Is Currently In View (Priority: P3)

As the respawn system, when the timer elapses but every registered, currently-empty spawn point
on the floor happens to be within the player's current view radius, I need well-defined,
non-broken behavior — not a crash, not a frozen timer that never recovers, and absolutely not a
forced in-view spawn that violates User Story 2's rule.

**Why this priority**: An edge-case robustness story for a real but comparatively rare
condition (small/compact maps, or a player standing in a spot with an unusually wide view of the
remaining empty points). It matters for stability, but the game is already functionally correct
for the vast majority of play sessions without this exact scenario occurring.

**Decision** (resolving the "must the system wait, or is there a fallback?" question this
category's Edge Cases are required to answer): **the system waits.** It never spawns in view as
a fallback. Once the timer has elapsed, the system re-evaluates "is there now a valid (empty AND
out-of-view) spawn point" on every subsequent update, and spawns at the first update where the
answer is yes. The main respawn duration is not restarted from zero while waiting — only the
"has a valid point appeared yet" check repeats. This keeps the GDD's absolute rule ("tidak pernah
muncul di depan mata player") intact under all conditions, at the cost of an occasional
longer-than-configured wait in the rare case where the player is parked somewhere that sees every
empty point.

**Independent Test**: Configure every empty spawn point on a floor as within the player's current
view radius. Let the respawn timer elapse. Assert no battery spawns and no exception/error
occurs. Then move the player (or shrink the fixture's simulated view radius) so one point becomes
out-of-view, run one more update, and assert the battery spawns at that point on the very next
check.

**Acceptance Scenarios**:

1. **Given** every empty spawn point is within view when the timer elapses, **When** a spawn
   check runs, **Then** no battery spawns and no error/exception occurs.
2. **Given** the same held-elapsed state, **When** the player's position (or the configuration
   used for eligibility) changes such that at least one empty spawn point becomes out-of-view,
   **Then** a battery spawns at that point on the next check, without the respawn duration having
   restarted from zero in the meantime.
3. **Given** the waiting state, **When** an additional check runs while still no valid point
   exists, **Then** the system remains in the same, calm "waiting" state indefinitely — it does
   not error, log a growing number of duplicate warnings, or degrade performance over time.

---

### Edge Cases

- **Every registered empty spawn point is currently in view when the timer elapses**: resolved
  above in User Story 3 — the system waits and retries, never spawns in view, never loses its
  already-elapsed timer progress.
- **Zero empty spawn points exist at all** even though the active count is below cap (implies an
  inconsistency between `001`'s registry and `002`'s counter — e.g., a bug where a picked-up
  battery's spawn point was never marked empty again): the system MUST NOT spawn and MUST NOT
  crash; this is the same "no valid candidate yet" state as the all-in-view case, structurally.
- **Multiple valid (empty AND out-of-view) spawn points exist at spawn time**: selection MUST be
  uniformly random among them — never deterministic (e.g., always the first in the registry's
  list) — matching GDD Ch. 3.1's "spawn point **acak**" (random).
- **The player repeatedly enters and leaves view of the only currently-valid empty point**
  (thrashing near a single candidate): the system MUST NOT spawn while that point is in view, and
  MUST NOT restart the elapsed respawn duration back to full just because eligibility
  fluctuated — only the moment of an actual successful spawn resets the timer (FR-008).
- **Floor 52 is loaded**: this entire timer/placement system MUST NOT run at all. Floor 52 uses
  only static battery placement (its own scene spec, per GDD Ch. 3.1: "Floor 52 tidak memakai
  respawn sama sekali: 3–5 battery ditaruh statis sejak awal"). This is an explicit **non-goal**
  for this spec, not an oversight — see FR-009 and Assumptions. Concretely: no respawn timer
  instance MUST exist for Floor 52, and no `GameConfig` respawn-duration value MUST be assumed
  to exist for it.
- **The whole battery-spawn-system category is toggled off** (GDD Ch. 18.3, cut-candidate #2):
  this is the same shape of non-goal as the Floor 52 case, just applied to every floor at once —
  see Assumptions.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST run a respawn timer for a floor whenever that floor's active battery
  count is below its configured maximum (per `002-active-battery-count-cap`'s check); the timer
  MUST NOT advance toward a spawn while the floor is at or above its cap.
- **FR-002**: The respawn timer's full duration MUST be read from `GameConfig` per floor (per GDD
  Ch. 17.2 defaults: `batteryRespawnFloor51` = 30 seconds, `batteryRespawnFloor50` = 60 seconds)
  — never a hardcoded literal in this system's code. Deciding exactly which floor *scene* uses
  which duration value is confirmed by each floor scene's own spec, not this one.
- **FR-003**: When the timer elapses, the system MUST attempt to select one candidate spawn
  point that is simultaneously (a) currently empty per `001`'s registry, and (b) outside the
  player's current view radius, and spawn exactly one new `Battery` there.
- **FR-004**: "Outside the player's current view radius" MUST be expressed as a pure geometric
  distance test: a candidate spawn point is out-of-view if the distance between its position and
  a given player position is strictly greater than a given view radius; a distance exactly equal
  to the radius counts as in-view (not eligible — see User Story 2, Acceptance Scenario 2). This
  test MUST be fully expressible and testable with plain position/radius values, requiring no
  live Unity `Camera`, `Light`, or rendering step to produce a correct answer.
- **FR-005**: If, at the moment the timer elapses, no spawn point is simultaneously empty and
  out-of-view, the system MUST NOT spawn a battery anywhere as a fallback, and MUST NOT treat
  this as an error. It MUST keep re-checking eligibility on subsequent updates and spawn at the
  first update where a valid point exists. The already-elapsed respawn timer MUST NOT restart
  from its full duration while waiting in this state (see User Story 3, FR-008 for when it does
  restart).
- **FR-006**: When multiple candidate spawn points are simultaneously empty and out-of-view at
  spawn time, the system MUST choose exactly one of them using a uniform random selection — never
  a deterministic rule such as "first in list" or "closest to player."
- **FR-007**: Immediately after a successful spawn, the system MUST mark the chosen spawn point
  occupied in `001`'s registry and increment the active count in `002`'s tracker for that floor —
  both synchronously with the spawn, so the very next cap check and empty-point query reflect it.
- **FR-008**: After a successful spawn, if the floor is still below its cap, the respawn timer
  MUST restart counting from its full per-floor duration (FR-002) for the next battery. The
  timer only restarts from full duration on an actual successful spawn — not merely because
  spawn-point eligibility changed while waiting (FR-005).
- **FR-009**: Floor 52 MUST NOT have any respawn timer instance running at all — this system is
  entirely absent for Floor 52, not merely configured to a very long duration or zero max. More
  generally, this system MUST be inert for any floor that has no configured respawn duration and
  cap, rather than requiring a scattered "is this Floor 52" special case anywhere in this
  system's logic — the absence of config for a floor is itself the off-switch.
- **FR-010**: The whole `battery-spawn-system` category (this spec plus `001` and `002`) MUST be
  toggle-able off entirely in favor of a fully static battery-placement fallback (GDD Ch. 18.3,
  cut-candidate #2) without requiring code changes in any system that merely consumes `Battery`
  objects — in particular, `flashlight-and-battery/007-battery-pickup-and-spare-slot`'s pickup
  and install logic MUST have zero dependency on this timer/placement rule existing or running.
- **FR-011**: The timer, the in-view geometric test, and the candidate-selection logic MUST be
  implemented as plain C# with no `MonoBehaviour`/`Component`/scene dependency (constitution
  Principle III/IV). A thin `MonoBehaviour` adapter MUST own the per-frame tick, supply the live
  player position and the live current view radius (sourced from
  `flashlight-and-battery/003-eased-radius-transitions`'s current light radius value) as plain
  inputs each check, and instantiate the actual `Battery` prefab in the scene only when the
  plain-C# logic signals a spawn decision.

### Key Entities

- **BatteryRespawnTimer**: Per-floor countdown state — full duration (from config), elapsed time,
  an "elapsed" flag, and a reset-to-full operation.
- **SpawnPointVisibilityRule**: A pure function of (spawn point position, player position, view
  radius) → in-view or out-of-view, per the strictly-greater-than distance rule in FR-004.
- **RespawnCandidateSelector**: Given a floor's currently-empty spawn points (from `001`) and the
  player's current position/view radius, filters to the eligible (empty AND out-of-view) subset
  and picks one uniformly at random, or reports "no eligible candidate yet."
- **BatteryRespawnCoordinator**: Orchestrates the timer, the cap check (`002`), the candidate
  selector, and the registry/counter mutations (`001`/`002`) into a single per-update decision:
  "spawn now at point X" or "not yet."

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In EditMode tests simulating Floor 51's defaults (cap = 2, respawn duration = 30
  configured seconds) with at least one eligible candidate available throughout, a spawn decision
  fires exactly once per elapsed duration, in 100% of trials — never early, never skipped.
- **SC-002**: Across 100+ randomized trials mixing in-view and out-of-view empty spawn points,
  the system selects an in-view point 0% of the time.
- **SC-003**: When 100% of currently-empty spawn points are in-view at the moment the timer
  elapses, zero spawns occur until at least one point becomes out-of-view, and zero
  exceptions/crashes occur across the entire waiting period across all tests exercising this
  path.
- **SC-004**: Disabling this feature's `BatteryRespawnCoordinator`/`MonoBehaviour` adapter (the
  Ch. 18.3 cut, substituting static placement) requires touching only this feature's own files —
  verified by zero inbound references from
  `flashlight-and-battery/007-battery-pickup-and-spare-slot`'s code to any class defined by this
  spec; that consuming system only ever calls generic `Battery` pickup/install APIs, never
  anything timer- or spawn-point-specific.
- **SC-005**: Over 500 simulated respawn cycles with multiple eligible candidates present each
  time, the distribution of chosen spawn points shows no candidate chosen 0% or 100% of the time
  — confirming the random selection (FR-006) is not silently degenerating to a deterministic
  pick.

## Assumptions

- **This category is Cut-Candidate #2 (GDD Ch. 18.3)**: "Battery respawn → ganti statis." This
  spec's timer and placement rule are designed to be removable as a unit — see FR-010 and
  SC-004. The static-placement fallback is expected to place hand-authored `Battery` instances
  directly in a floor scene (as Floor 52 already does per GDD Ch. 3.1) with no involvement from
  this spec's classes at all.
- **Floor 52's non-use of this system is a permanent design decision, not a temporary gap** — per
  GDD Ch. 3.1 it is stated as the floor's actual design, independent of the Ch. 18.3 cut
  consideration for Floor 51/50. This spec treats "no config for this floor" and "the whole
  category is cut" as the same underlying off-switch (FR-009), which is why both cases are safe
  by construction rather than needing separate code paths.
- **"Player's current view/light radius" is supplied, not computed, by this spec.** The actual
  live radius value (which changes per Light State — Normal/Flickering/Critical/Compact
  Darkness, per GDD Ch. 5.2/12.1, eased over time) is owned by
  `flashlight-and-battery/003-eased-radius-transitions`. This spec's `MonoBehaviour` adapter
  reads that current value each check and passes it into the plain geometric test as a number —
  this spec does not duplicate or reimplement the easing/light-state logic.
- **Per-floor respawn duration/cap wiring is out of scope here**, same as in `001` and `002` —
  which specific floor scene uses which configured number is confirmed in each floor scene's own
  spec (`scenes/005-floor-51-scene/004-battery-spawn-point-placement`,
  `scenes/006-floor-50-scene/005-battery-spawn-point-placement`).
- **Map is fixed, not procedural** (GDD Ch. 16.1) — spawn point positions never change at
  runtime, only which ones are currently empty/occupied and which happen to be in view.

## Related

- GDD Ch. 3.1 ("Aturan spawn battery"), Ch. 12 (single omnidirectional flashlight — basis for
  treating "in view" as a distance check), Ch. 18.3 (Scope Lock — cut-candidate #2) —
  `specs/_reference/LILO-GDD-v2-Production-Lock.md`
- `specs/ROADMAP.md` §0 (naming conventions), §2 (`battery-spawn-system` row), §5 (build order:
  depends on `001` and `002`)
- `specs/systems/flashlight-and-battery/007-battery-pickup-and-spare-slot/spec.md` — the
  `Battery` object this spec instantiates; its pickup/install logic has zero dependency on this
  spec (FR-010, SC-004)
- `specs/systems/flashlight-and-battery/003-eased-radius-transitions/spec.md` — source of the
  live "current view radius" value this spec's adapter consumes as a plain input
- `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md` — source of
  empty spawn points this spec selects from and mutates on spawn
- `specs/systems/battery-spawn-system/002-active-battery-count-cap/spec.md` — the cap check this
  spec's timer gates against
