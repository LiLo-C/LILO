# Feature Specification: Key Pickup & Inventory

**Feature Branch**: `001-key-pickup-and-inventory`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 4 — Floor 51 & 50 (GDD 19.2) · **Proposed owner**: Calzy (state management, GDD 19.1)

**GDD Sources**: Ch. 4.2 (context-sensitive action button, interaction feedback table), 8.1 (objective types & key counts), 17.5 (progression config), 18.2 (no complex inventory system)

**Input**: User description: "Generic, reusable key pickup and inventory system: the player picks up a key through the same context-sensitive action button used for every other interaction (highlight + SFX pickup, per GDD 4.2), and holds it in a simple inventory (a set of held keys) until it is later consumed unlocking its matching door. Keys are never dropped, never discarded, and never picked up twice. Per-floor key counts (Floor 51 = 1, Floor 50 = 3) are floor-scene concerns and are explicitly out of scope here."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Pick Up a Key With the Action Button (Priority: P1)

A player walking through the office notices a key on a desk, approaches it, and presses the same action button used for every other interaction; the key is highlighted while in range, disappears from the world on pickup, and a pickup sound plays — with no separate "key" button and no floating prompt icon.

**Why this priority**: Without pickup, no locked door can ever be unlocked. This is the entry point of the entire keys-and-doors chain and is Floor 51's whole objective (GDD 8.1).

**Independent Test**: In a test scene containing a single key, walk into interaction range, press the action button, and confirm the key leaves the world, appears in the player's inventory, and pickup feedback plays.

**Acceptance Scenarios**:

1. **Given** a key at rest in the world, **When** the player is within interaction range, **Then** the key is highlighted and the action button offers a "pick up" affordance (per interaction-and-highlight/001 and 002), with no floating prompt icon (GDD 4.2).
2. **Given** the action button is pressed while a key is the nearest interactable, **When** the press completes, **Then** the key is removed from the world, added to the player's held-key inventory, and a pickup feedback hook fires exactly once.
3. **Given** a key has just been picked up, **When** its original world position is checked again, **Then** no key exists there — it does not respawn and does not linger as an interactable.

---

### User Story 2 - Keys Are Tracked by Identity, Not Just a Count (Priority: P2)

Because a later spec (002) requires that each key opens one specific door and not any door, the inventory must remember *which* keys the player holds, not merely *how many* — so a player who has picked up two different keys can be asked "do you hold the key for this specific door" and get a correct answer.

**Why this priority**: This generality only matters once more than one key exists on a floor, and unlocking itself is spec 002's concern — so a single-key demo (User Story 1) is already a viable slice without it. It is still P2, not P3, because getting the inventory's shape right now avoids a rework once 002 is built on top of it.

**Independent Test**: In a test scene with two keys, each authored with a different target-door identity, pick up both and query the inventory for each door's specific key.

**Acceptance Scenarios**:

1. **Given** two keys with different target-door identities exist in the world, **When** both are picked up, **Then** the inventory holds both distinct key identities simultaneously and neither overwrites the other.
2. **Given** the player holds one key, **When** another system asks "does the player hold the key for door X", **Then** the answer reflects that exact key's identity, not just a nonzero count of held keys.

---

### User Story 3 - Keys Can Never Be Dropped, Discarded, or Picked Up Twice (Priority: P3)

A player cannot drop a key back into the world, discard it, or accidentally pick up the same key a second time — the inventory only ever grows through valid pickups and only ever shrinks through consumption (spec 002) or a floor reset, protecting the game's deliberately minimal inventory design.

**Why this priority**: This is a safety/scope-lock constraint (GDD 18.2: no complex inventory) on a system that already works correctly per User Stories 1–2, rather than new player-facing behavior — it guards against future scope creep and bugs, so it is lowest priority to build though still required before the feature is considered done.

**Independent Test**: With a test harness, confirm no drop/discard action exists for a held key, and confirm re-attempting a pickup at an already-collected key's former position finds nothing there.

**Acceptance Scenarios**:

1. **Given** the player holds a key, **When** the game's available interactions are inspected, **Then** no "drop" or "discard" action exists for keys anywhere.
2. **Given** a key has already been picked up, **When** the pickup system is invoked again for that same key (e.g., a duplicated input event in the same frame), **Then** the second call is a no-op — no duplicate inventory entry, no error.

---

### Edge Cases

- The floor has many keys: there is no upper bound on inventory size in this generic spec — per-floor key counts are entirely out of scope here, and the inventory MUST accept every distinct key present on the floor.
- A key and another interactable (e.g., a battery) are in range at the same time: the nearest-object-wins tie-break already defined by interaction-and-highlight/002 applies unchanged; this spec adds no competing rule.
- The player picks up a key while being chased: pickup succeeds instantly with no additional gating, mirroring the GDD 5.1 precedent that resource pickups are never blocked by threat state.
- A duplicated or replayed pickup input fires for a key already removed from the world: pickup MUST be idempotent (see User Story 3, Acceptance Scenario 2).
- A floor reset occurs (out of scope here — see [keys-and-doors/lives-and-fail-state's floor reset](../../lives-and-fail-state/002-floor-state-reset-on-death/spec.md)): the inventory is cleared and world keys are restored to their authored positions. This spec only defines the inventory's shape; the reset trigger and its full effects belong to `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let the player pick up a key using the same context-sensitive action button defined by `specs/systems/interaction-and-highlight/002-context-sensitive-action-button/spec.md`. No separate "key" button or gesture may exist.
- **FR-002**: A key MUST be visually highlighted while within interaction range and MUST NOT display any floating prompt icon (GDD 4.2: "Tidak pakai ikon prompt melayang").
- **FR-003**: On a successful pickup, the key MUST be permanently removed from the world for the remainder of the current floor attempt, MUST be added to the player's held-key inventory, and MUST fire a pickup feedback hook exactly once. The concrete sound asset is out of scope (owned by the audio system); this spec only requires the hook call site exists.
- **FR-004**: The held-key inventory MUST track each held key by a distinct identity (its associated door, established in floor/level data), not merely as a count, so that a later system (spec 002) can determine whether the player holds the one specific key required by a specific door.
- **FR-005**: Keys MUST NOT be droppable, discardable, or re-placeable into the world by the player. The only ways a key leaves the inventory are (a) consumption during a successful unlock (spec 002) or (b) a floor reset (`specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md`).
- **FR-006**: A key MUST NOT be pickable more than once. Once removed from the world it MUST NOT reappear, respawn, or become interactable again until a floor reset restores it, and a repeated pickup attempt on an already-collected key MUST be a safe no-op.
- **FR-007**: Picking up a key MUST NOT be gated by any player state (e.g., being chased, sprinting, current light state) beyond the standard interaction-range check already defined by interaction-and-highlight/001.
- **FR-008**: The system MUST support an arbitrary number of distinct keys per floor. This spec MUST NOT hardcode or assume any specific per-floor key count — those counts are defined by the corresponding floor scene spec (e.g., Floor 51 = 1, Floor 50 = 3, per GDD 8.1).
- **FR-009**: All numeric/tunable values this spec depends on (interaction range, action-button behavior) MUST be read from the GameConfig fields already introduced by the interaction-and-highlight system; this spec introduces no new GameConfig keys of its own, since pickup itself has no feel-tunable timing beyond the existing interaction range.

### Key Entities

- **Key**: An authored pickup object with a stable identity, a reference to the one specific Door it will eventually unlock (established in floor/level data — out of scope here), and a state of `InWorld` or `Held` in this spec (a further `Consumed` state is added by spec 002).
- **Held-Key Inventory**: The set of keys currently carried by the player during the current floor attempt. Membership-based only — no ordering, no stacking, no duplicate entries for the same key identity.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In automated tests, picking up N distinct keys (verified for at least N = 1 and N = 5) always results in exactly N distinct entries in the inventory, in 100% of test runs.
- **SC-002**: In automated tests, attempting to pick up the same key a second time after its first successful pickup never adds a duplicate entry and never errors, in 100% of test runs.
- **SC-003**: In a manual playtest, 5 of 5 first-time testers recognize the highlight-plus-pickup-sound feedback as "I picked something up" without any on-screen text explanation.
- **SC-004**: A code/design review confirms zero drop or discard actions are reachable from input mapping or UI for keys (a structural check, since no such action exists to exercise at runtime).

## Assumptions

- The specific Door each Key maps to is authored in floor/level data, owned by the corresponding floor scene spec; this spec only requires that the mapping exists and is stable once a floor has loaded.
- Pickup feedback content (the SFX asset itself, the highlight color) comes from `specs/systems/audio/001-sound-event-taxonomy` and `specs/systems/interaction-and-highlight/003-interactable-highlight-halo`; this spec only requires that the hook and highlight fire, not the final asset.
- Per-floor key counts (Floor 51 = 1, Floor 50 = 3, per GDD 8.1 and 17.5) belong to the Floor 51 and Floor 50 scene specs, not here.

## Dependencies

- **Requires**: `specs/systems/interaction-and-highlight/002-context-sensitive-action-button/spec.md` (the action button itself, and transitively 001's nearest-interactable detection).
- **Consumed by**: `specs/systems/keys-and-doors/002-locked-door-unlock-logic/spec.md` (consumption on unlock), `specs/systems/lives-and-fail-state/002-floor-state-reset-on-death/spec.md` (inventory reset), and the Floor 51 / Floor 50 scene specs (key placement and per-floor counts).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[constitution]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 4.2, 8.1, 17.5, 18.2
