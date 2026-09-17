# Feature Specification: Haptic Feedback

**Feature Branch**: `feat/014-haptics`

**Created**: 2026-09-17

**Status**: Draft

**GDD Phase**: Fase 6 — Art & Audio Pass (GDD 19.2) · **Proposed owner**: Eca (feedback/UI)

**GDD Sources**: Ch. 15.2, 18.1

**Input**: User description: "Haptic untuk memperkuat tension memanfaatkan iOS: saat tertangkap monster, saat battery masuk state Critical / habis, saat pickup item. Opsional: haptic mengikuti pola detak jantung saat monster mendekat."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Feel the Important Moments (Priority: P1)

A player feels a strong jolt when caught, a warning pulse when the light becomes critical and again when it dies, and a light tap when picking something up.

**Why this priority**: The three required haptic moments from GDD 15.2.

**Independent Test**: On a physical iPhone, trigger each event once.

**Acceptance Scenarios**:

1. **Given** the monster catches the player, **When** the catch fires, **Then** one strong haptic plays at the start of the death sequence.
2. **Given** the installed charge drops into the Critical state, **When** the state changes, **Then** one warning haptic plays.
3. **Given** the installed charge reaches 0% (Compact Darkness), **When** the state changes, **Then** one distinct haptic plays.
4. **Given** the player picks up a battery or a key, **When** the pickup succeeds, **Then** one light haptic plays. A rejected pickup does not play it.

---

### User Story 2 - Heartbeat When It's Close (Priority: P3)

A player feels a heartbeat-like pulse pattern while the monster is near, getting faster as it closes in.

**Why this priority**: Explicitly optional in the GDD.

**Independent Test**: Enable the heartbeat option, let the monster approach in the arena.

**Acceptance Scenarios**:

1. **Given** `hapticHeartbeatEnabled = true` and the monster is within `monsterNearDistance`, **When** time passes, **Then** a repeating heartbeat pattern plays, with its interval shrinking as distance shrinks.
2. **Given** the monster moves beyond `monsterNearDistance`, **When** the pattern is playing, **Then** it stops.

---

### Edge Cases

- The charge crosses into Critical, then a battery install brings it back to Normal, then it drops into Critical again: the warning plays again. It plays once per entry into the state, never repeatedly while staying in it.
- The device has no haptic hardware or haptics are disabled in iOS settings: nothing plays, and no errors occur.
- Pause: haptics stop, and the heartbeat resumes only when the conditions still hold after resume.
- The player is hiding while the monster searches nearby: the heartbeat (if enabled) still plays, which is intended for tension.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST play a haptic on catch, on entering Critical, on reaching 0% charge, and on successful battery or key pickup (GDD 15.2).
- **FR-002**: Each light-state haptic MUST play once per entry into that state.
- **FR-003**: An optional heartbeat pattern MUST be controlled by `hapticHeartbeatEnabled`, playing while the monster is within `monsterNearDistance`, with interval scaling by distance.
- **FR-004**: Haptics MUST respect the system haptics setting and MUST fail silently on unsupported hardware.
- **FR-005**: Haptics MUST stop while paused (spec 012).
- **FR-006**: Haptic intensities, sharpness, and heartbeat timing MUST come from the single configuration source: `hapticCatchIntensity`, `hapticCriticalIntensity`, `hapticEmptyIntensity`, `hapticPickupIntensity`, `hapticHeartbeatEnabled`, `hapticHeartbeatMinInterval`, `hapticHeartbeatMaxInterval`.
- **FR-007**: Haptic triggers MUST come from the same gameplay events used by audio (spec 013 FR-001), not from duplicated gameplay checks.

### Key Entities

- **Haptic Event**: Catch, critical, empty, pickup, heartbeat tick.
- **Haptic Pattern**: Intensity, sharpness, and timing for each event.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: On iPhone 17, each required event produces its haptic in 10 of 10 triggers, and never on rejected pickups.
- **SC-002**: 4 of 5 playtesters say the haptics made tense moments stronger, and 0 of 5 say they were annoying or constant.
- **SC-003**: The team records a keep/drop decision on the optional heartbeat after playtesting.

## Assumptions

- There is no in-game haptics toggle. Players use the iOS system setting (spec 012 assumption).
- The heartbeat defaults to off until the playtest decision (SC-003).

## Dependencies

- **Requires**: 001 (light states, pickup), 002 (catch, monster distance), 005 (key pickup).
- **Integrates with**: 012 (pause), 013 (shared events).

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[LILO-GDD-v2-Production-Lock]] — Ch. 15.2
