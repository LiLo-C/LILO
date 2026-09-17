# Feature Specification: Battery Pickup & Spare Slot

**Feature Branch**: `007-battery-pickup-and-spare-slot`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "Picking up a loose battery into a single spare slot; a pickup
while the spare slot is already full must be rejected with visible feedback and the battery must
remain in the world."

## Why This Spec Exists

GDD Ch. 5.1 locks the resource shape this feature implements: "Slot cadangan: 1 slot. Total
bawaan: 1 terpasang + 1 cadangan," "Slot penuh + nemu battery: Tidak bisa diambil. UI slot
menunjukkan kapasitas 1, jadi player paham tanpa perlu dijelaskan," "Bisa dibuang/ditaruh ulang:
TIDAK," and "Ambil saat dikejar: BOLEH. Tanpa syarat tambahan." Ch. 4.2 adds the interaction
contract: picking up a battery highlights it first and plays a pickup SFX. This feature owns the
world-battery-to-spare-slot transition and its one hard capacity limit — it does not touch the
installed battery's drain (`002`) or the install/refill action (`008`), and it does not define
where world batteries spawn or how many exist at once (`battery-spawn-system`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Player Picks Up a Loose Battery Into an Empty Spare Slot (Priority: P1)

A player who has no spare battery walks up to a loose battery in the world and picks it up; it
disappears from the world and now occupies the player's one spare slot.

**Why this priority**: This is the feature's entire reason to exist — without a working pickup,
there is no resource loop for `008`'s install action to ever have anything to install.

**Independent Test**: In an EditMode test with no scene running, construct an empty spare slot,
attempt a pickup, and confirm the slot now holds exactly one battery and the pickup reports
success — fully testable without a running scene or world object.

**Acceptance Scenarios**:

1. **Given** the spare slot is empty, **When** the player picks up a loose battery, **Then** the
   slot now holds exactly one battery, and the pickup result reports success.
2. **Given** a successful pickup, **When** the picked-up world battery object is checked
   afterward, **Then** it no longer exists as an available pickup in the world (GDD Ch. 4.2:
   "Objek di-highlight sebelum diambil + SFX pickup" implies removal on take).
3. **Given** the player is being actively chased by the monster, **When** they pick up a battery,
   **Then** the pickup succeeds exactly as it would with no monster nearby — this feature adds no
   additional gating condition beyond spare-slot capacity (GDD Ch. 5.1: "Ambil saat dikejar: BOLEH.
   Tanpa syarat tambahan").

---

### User Story 2 - Pickup Is Rejected When the Spare Slot Is Already Full, and the Battery Stays in the World (Priority: P1)

A player who already has a spare battery attempts to pick up another loose battery; the attempt
is rejected, the new battery remains exactly where it was in the world, and the outcome is
distinguishable from a successful pickup so calling systems can give the player visible feedback.

**Why this priority**: GDD Ch. 5.1 states this outcome as plainly as the success case ("Slot
penuh + nemu battery: Tidak bisa diambil"), and the task description that scoped this feature
calls it out explicitly as a required, tested negative-space behavior — not an incidental
side-effect of the capacity check.

**Independent Test**: In an EditMode test, construct a spare slot already holding one battery,
attempt a second pickup, and confirm the slot still holds exactly the original battery, no second
battery was created, and the pickup result reports rejection — fully testable without a running
scene.

**Acceptance Scenarios**:

1. **Given** the spare slot already holds one battery, **When** the player attempts to pick up
   another loose battery, **Then** the attempt is rejected, the slot's existing contents are
   unchanged, and the pickup result reports rejection distinctly from success.
2. **Given** a rejected pickup, **When** the world battery object that was just declined is
   checked afterward, **Then** it still exists in the world, unmodified, available for a future
   pickup once the spare slot frees up (`008`).
3. **Given** a rejected pickup, **When** the same battery is attempted again immediately (e.g. the
   player mashes the interact button while the slot is still full), **Then** every additional
   attempt is rejected the same way, with no side effect accumulating from repeated attempts
   (idempotent rejection).

---

### User Story 3 - A Rejected Pickup Produces a Distinguishable Outcome for Visible Feedback (Priority: P2)

When a pickup is rejected because the spare slot is full, the underlying system exposes a result
that is clearly different from a successful pickup, so whatever presents feedback to the player
(an SFX cue, the action button's state, or the HUD's slot indicator) can react differently to
"picked up" versus "couldn't pick up, slot full" without re-deriving the capacity check itself.

**Why this priority**: The task scope for this feature explicitly requires the rejection to be
observable, not merely a silent no-op — GDD Ch. 4.2's general interaction feedback rule ("SFX
hanya saat objek benar-benar diambil/dipakai") implies success and failure must already read
differently to the player, and this feature is the one place that can guarantee a caller has
enough information to make that distinction. It is P2 because it is a contract requirement on top
of User Story 2's rejection behavior, not a new behavior of its own.

**Independent Test**: In an EditMode test, trigger both a successful and a rejected pickup and
assert their reported outcomes are distinct, checkable values (not both simply "nothing
happened").

**Acceptance Scenarios**:

1. **Given** a successful pickup, **When** its result is inspected, **Then** it is a specific,
   named success outcome.
2. **Given** a rejected pickup (slot full), **When** its result is inspected, **Then** it is a
   specific, named rejection outcome distinguishable in code from the success outcome — this
   feature does not itself play an SFX or render UI (that belongs to `audio`, `interaction-and-
   highlight`, and `009-battery-hud-indicator` respectively); it only guarantees the outcome each
   of those systems would need to react correctly is available.

---

### Edge Cases

- **Two pickup attempts land in the same update/frame while the slot is empty** (e.g. a
  double-triggered input event): exactly one attempt MUST succeed and occupy the slot; any
  attempt evaluated after the slot becomes occupied (even within the same frame) MUST be rejected
  — the capacity check MUST be evaluated against the slot's actual current occupancy at the moment
  each attempt is processed, not against a stale "was empty this frame" snapshot.
- **The player interacts with the same already-declined world battery repeatedly while the slot
  stays full**: every attempt is rejected identically, with no accumulating state and no
  duplicate battery ever created (User Story 2 Scenario 3).
- **`GameConfig.batterySlots` is misconfigured to `0` or a negative value** (authoring error):
  every pickup attempt MUST be rejected (the slot is always "full" relative to a non-positive
  capacity) rather than crashing or behaving as unlimited.
- **A world battery is picked up while the player is hiding** (per `hiding` category): whether
  interaction is even reachable while hiding is decided by the `hiding` and `interaction-and-
  highlight` categories, not this feature — this feature only defines what happens once a pickup
  attempt is actually made, regardless of what allowed it to be made.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST model a spare battery slot with a fixed capacity read from
  `GameConfig.batterySlots` (locked default `1`, per GDD Ch. 17.2 "Slot cadangan: 1 slot") —
  never a hardcoded `1` literal duplicating that config value (constitution Principle III/V).
- **FR-002**: When a pickup is attempted and the spare slot's current occupancy is below its
  capacity, the system MUST accept the pickup: a new, full-charge battery is placed into the slot,
  and the result reports success (GDD Ch. 5.1, Ch. 4.2).
- **FR-003**: When a pickup is attempted and the spare slot's current occupancy already equals its
  capacity, the system MUST reject the pickup: the slot's existing contents remain completely
  unchanged, no new battery is created, and the result reports rejection (GDD Ch. 5.1 "Slot penuh
  + nemu battery: Tidak bisa diambil").
- **FR-004**: A pickup's result MUST be a specific, distinguishable value (e.g. an enum with at
  least `Success` and `RejectedSlotFull` cases) — never a bare boolean or void return that leaves
  callers unable to tell success from rejection, and never itself triggering an SFX or UI update
  (those are owned by other categories/features that consume this result).
- **FR-005**: A world battery object that was declined by a rejected pickup MUST remain fully
  present and unmodified in the world, available for a future pickup attempt once the spare slot
  frees up (GDD Ch. 5.1; User Story 2).
- **FR-006**: A battery already placed in the spare slot MUST NOT be removable back into the
  world by any player action defined in this feature — the only way the spare slot empties is
  through installation (`008-battery-install-and-refill`), never a "drop" action (GDD Ch. 5.1:
  "Bisa dibuang/ditaruh ulang: TIDAK").
- **FR-007**: Pickup eligibility MUST NOT depend on monster proximity, monster state, or any
  condition beyond spare-slot capacity — this feature introduces no additional gate (GDD Ch. 5.1:
  "Ambil saat dikejar: BOLEH. Tanpa syarat tambahan").
- **FR-008**: The capacity check and slot-occupancy transition (accept/reject, slot contents)
  MUST live in a plain C# class under `Assets/Scripts/Systems/` with no `UnityEngine.
  MonoBehaviour`, `Component`, or scene dependency (constitution Principle III), callable from an
  EditMode test with no running scene and no world battery `GameObject`. The `MonoBehaviour`
  adapter that detects a world battery object and the player's interaction input, and calls into
  this logic, is a separate thin class under `Assets/Scripts/MonoBehaviours/`.

### Key Entities

- **SpareBatterySlot** *(new)*: Holds zero or more `Battery` instances up to `GameConfig.
  batterySlots`'s capacity (locked at `1` today). Exposes whether it currently has room, and the
  accept/reject transition this feature defines. Consumed (not redefined) by
  `008-battery-install-and-refill`, which is the only feature allowed to empty it.
- **Battery** *(consumed, not redefined)*: The entity and its charge behavior are owned by
  `002-battery-real-time-drain-timer`. A successful pickup here creates a new `Battery` instance
  at full charge (`ChargeSeconds == GameConfig.batteryDuration`) and places it in the slot — this
  feature does not define draining or any other charge behavior.
- **World battery pickup object**: The scene-placed object the player interacts with. Its
  existence/placement/respawn rules belong to `battery-spawn-system`, not this feature — this
  feature only defines what happens to a *given* pickup attempt against the spare slot, and that a
  declined one remains present.
- **GameConfig** *(extended)*: Gains `batterySlots` (int, locked default `1`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With an empty spare slot, a pickup attempt succeeds in 100% of sampled trials in an
  automated EditMode test, leaving the slot occupied by exactly one battery.
- **SC-002**: With a full spare slot, a pickup attempt is rejected in 100% of sampled trials, and
  the slot's pre-existing contents are byte-for-byte unchanged after the attempt.
- **SC-003**: A successful pickup's result value and a rejected pickup's result value are
  distinguishable by a direct equality/type check in an EditMode test — never requiring the test
  to infer the outcome from a side effect alone.
- **SC-004**: `batterySlots` can be changed by editing only `GameConfig`, with the capacity check
  picking up the new value with no other file requiring a change (validated at the GDD-locked
  value of `1` for shipped behavior).
- **SC-005**: Across 20 randomized-order trials mixing successful and rejected pickup attempts
  against the same slot, the slot never ends up holding more batteries than its configured
  capacity.

## Assumptions

- This feature does not decide how the player's interaction input reaches a specific world battery
  object (nearest-interactable detection and the context-sensitive action button are owned by
  `interaction-and-highlight/001`/`002`) — it only defines the outcome of an attempt once made.
- The visual highlight shown on an interactable battery before pickup (GDD Ch. 4.2) is owned by
  `interaction-and-highlight/003`, not redefined here. The pickup SFX and any HUD reaction to a
  rejection (e.g. a "slot full" cue) are owned by the `audio` category and `009-battery-hud-
  indicator` respectively — this feature only guarantees FR-004's distinguishable result exists
  for them to react to.
- Placement, count, and respawn timing of world battery objects belong to `battery-spawn-system`
  and each scene's own level-layout specs — this feature is agnostic to how many loose batteries
  exist or where.

## Related

- [[ROADMAP]] — flashlight-and-battery row 7; depends on `shared-config-and-state/002`
- [[constitution]] — Principle III (plain C# systems / thin adapters), Principle IV (EditMode
  tests)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 4.2 (pickup interaction/feedback), Ch. 5.1 (spare slot
  capacity, no-drop rule, chase-pickup rule), Ch. 17.2 (GameConfig keys)
- `specs/systems/flashlight-and-battery/002-battery-real-time-drain-timer` — origin of the
  `Battery` entity a successful pickup instantiates, not redefined here
- `specs/systems/shared-config-and-state/002-shared-game-state-and-manager` — origin of
  `GameState`/`GameManager`, not redefined here
