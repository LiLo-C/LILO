# Feature Specification: Keys, Locked Doors & Final Door

**Feature Branch**: `feat/005-keys-locked-doors`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 4 — Floor 51 & 50 (GDD 19.2) · **Proposed owner**: Calzy (state) with Eca (UI feedback)

**GDD Sources**: Ch. 2.2, 4.2, 7.1, 8.1, 8.2, 9.2, 17.5, 18.1, 18.2

**Input**: User description: "Sistem key dan pintu terkunci: ambil key lewat tombol aksi (highlight + SFX pickup), buka pintu terkunci kalau punya key (SFX unlock + animasi), gagal kalau tidak punya key (SFX 'terkunci' pendek). Floor 52: 0 pintu terkunci; Floor 51: 1 key + 1 pintu terkunci; Floor 50: key untuk 3 pintu terkunci lalu Final Door. Tanpa objective tracker, tanpa inventory kompleks. Reset saat mati: key kembali ke posisi semula, pintu terkunci lagi."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Find a Key, Open the Way (Priority: P1)

A player finds a key lying in the office, picks it up with the action button, walks to a locked door, and opens it, so floors can gate progress behind exploration.

**Why this priority**: This is Floor 51's entire objective (GDD 8.1) and the core of Floor 50.

**Independent Test**: On a sample floor with one key and one locked door, pick up the key and open the door.

**Acceptance Scenarios**:

1. **Given** a key is within interaction range, **When** the player looks at it, **Then** it is highlighted and the action button offers "Pick up".
2. **Given** the player presses "Pick up" on a key, **When** the action completes, **Then** the key leaves the world, the player holds it, pickup feedback plays, and an interaction noise pulse is emitted (spec 002).
3. **Given** the player holds a key and stands at a locked door, **When** they press the action button ("Unlock"), **Then** the door unlocks and opens with unlock feedback, one key is used up, and an interaction noise pulse is emitted.

---

### User Story 2 - Try a Locked Door Without a Key (Priority: P1)

A player who reaches a locked door without a key tries it and gets a short "locked" response, so they understand they need to find something without any text tutorial.

**Why this priority**: The only teaching tool for locks, given there is no objective UI (GDD 8.2).

**Independent Test**: Walk to a locked door with no key and press the action button.

**Acceptance Scenarios**:

1. **Given** the player holds no key and stands at a locked door, **When** they press the action button, **Then** the door stays shut, a short "locked" feedback plays, and an interaction noise pulse is emitted.
2. **Given** a locked door is within range, **When** the player has no key, **Then** the door is still highlighted and the action button still offers the interaction, so the "locked" feedback is discoverable.

---

### User Story 3 - Three Locks and the Final Door (Priority: P2)

On the last floor, a player finds keys for three locked doors that stand between them and the Final Door, then opens the Final Door to escape.

**Why this priority**: Needed only for Floor 50 (spec 010), which is the third cut candidate.

**Independent Test**: On a sample floor with three keys, three locked doors, and a Final Door, complete the sequence.

**Acceptance Scenarios**:

1. **Given** a floor with `lockedDoorsFloor50 = 3`, **When** it loads, **Then** it contains exactly 3 locked doors and 3 keys.
2. **Given** the player reaches the Final Door, **When** they press the action button, **Then** it opens without a key and raises the "run completed" outcome (spec 004 FR-008).

---

### User Story 4 - Death Undoes Progress (Priority: P2)

When the player dies, every key they picked up returns to where it was and every door they unlocked is locked again, so the floor restarts truly fresh.

**Why this priority**: Required by GDD 9.2 and consumed by spec 007's reset flow.

**Independent Test**: Pick up a key, open a locked door, trigger a floor reset, and check both.

**Acceptance Scenarios**:

1. **Given** the player holds a key and has opened one locked door, **When** a floor reset happens, **Then** every key is back at its authored position, every locked door is locked and closed, and the player holds no keys.

---

### Edge Cases

- A key and a locked door are both in range: nearest object wins (001 action-button priority rule).
- The player holds more keys than there are remaining locked doors on that floor: impossible by data (key count equals locked-door count), and validation in spec 004 MUST flag a mismatch.
- The player descends with an unused key: keys do not carry between floors; the key is discarded on descend.
- The monster is chasing while the player unlocks a door: unlocking is allowed at any time; the noise pulse applies.
- The player spams the action button on a locked door: each press plays "locked" feedback but feedback and noise pulses are rate-limited to one per `lockedFeedbackCooldown`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Keys MUST be picked up with the single action button, with highlight while in range and pickup feedback on success (GDD 4.2).
- **FR-002**: Keys MUST be generic: any held key opens any locked door on the same floor, and each unlock uses exactly one key.
- **FR-003**: Pressing the action button at a locked door while holding a key MUST unlock and open it with unlock feedback (sound + door animation). Without a key it MUST stay locked with short "locked" feedback.
- **FR-004**: Picking up a key, unlocking a door, and attempting a locked door MUST each emit an interaction noise pulse (`noiseInteract`, spec 002).
- **FR-005**: Once unlocked, a locked door MUST stay open until a floor reset.
- **FR-006**: Locked doors and the Final Door MUST block the monster's navigation while closed (spec 004 assumption).
- **FR-007**: Number of locked doors per floor MUST come from configuration: `lockedDoorsFloor52 = 0`, `lockedDoorsFloor51 = 1`, `lockedDoorsFloor50 = 3` (GDD 17.5). Each floor MUST contain exactly as many keys as locked doors, verified by floor validation.
- **FR-008**: The Final Door MUST open without a key and raise the "run completed" outcome.
- **FR-009**: On floor reset, all keys MUST return to their authored positions, all locked doors MUST relock and close, and held keys MUST be cleared (GDD 9.2).
- **FR-010**: Held keys MUST NOT carry over to the next floor.
- **FR-011**: No objective tracker, key counter, quest log, or map marker may be added. All objective types use the same interaction feedback (GDD 8.2).
- **FR-012**: New configuration keys: `lockedFeedbackCooldown`, plus the `lockedDoorsFloorNN` keys from GDD 17.5.

### Key Entities

- **Key**: Authored pickup with a home position. States: in world / held / used.
- **Locked Door**: Authored door with locked / unlocked-open states.
- **Final Door**: Special last-floor exit, keyless, triggers run completion.
- **Held Keys**: Count of keys the player currently carries on this floor.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 5 of 5 playtesters who reach a locked door without a key understand, without being told, that they need to find something to open it.
- **SC-002**: After a floor reset, automated tests confirm key positions, lock states, and held keys equal a freshly loaded floor in 100% of cases.
- **SC-003**: No combination of key pickup order on Floor 50 can leave the player unable to reach the Final Door (checked by automated reachability with each door locked/unlocked combination).

## Assumptions

- GDD 8.1 does not say whether keys are door-specific. Generic keys are chosen because they are the simplest option that matches "no complex inventory" (GDD 18.2) and cannot softlock the player.
- The HUD defined in GDD 15.1 shows only battery and spare slot, so held keys are not shown. Pickup feedback is the only confirmation. If playtests show players forget whether they hold a key, a minimal key indicator is a follow-up decision, not part of this spec.
- The Final Door needs no key because GDD 8.1 describes the keys as being "for 3 locked doors, then open Final Door".
- Final door and key visuals and sounds come from specs 013 and 015. Placeholders are fine here.

## Dependencies

- **Requires**: 001 (action button, highlight), 002 (noise pulses), 004 (floor data, door types, run completed).
- **Consumed by**: 007 (reset), 009 (Floor 51), 010 (Floor 50), 011 (Final Door → Good Ending).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 4.2, 8
