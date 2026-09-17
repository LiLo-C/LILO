# Feature Specification: Monster Per-Floor Tuning Profile

**Feature Branch**: `002-per-floor-tuning-profile`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The per-floor tuning profile from GDD Ch. 6.2's table
(investigateDuration, chaseHoldDuration, searchDuration, patrolSpeed, chaseSpeed), different for
Floor 51 vs. Floor 50, read from `GameConfig`, plus the hard rule that chaseSpeed must never
exceed the player's sprint speed (GDD Ch. 6.2's explicit 'aturan keras')."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Monster behaves differently on Floor 51 vs. Floor 50 (Priority: P1)

The same `MonsterStateMachine` (spec 001) drives a noticeably more patient, faster, more
aggressive monster on Floor 50 than on Floor 51, purely because it was constructed/ticked with a
different `MonsterStateTuning` value — no branching `if (floor == Floor50)` logic anywhere in the
state machine or controller.

**Why this priority**: This is the entire point of the feature — GDD Ch. 3's difficulty curve
("Pressure" vs. "Mastery") is expressed almost entirely through these five numbers. Without this,
Floor 51 and Floor 50 are mechanically identical, which contradicts the GDD's core structure.

**Independent Test**: In a debug test arena (not a real floor — real floors are specified
per-scene elsewhere), construct the state machine once with the Floor 51 profile and once with
the Floor 50 profile, feed both the identical scripted detection sequence, and assert the
resulting timer expirations and movement speeds differ by exactly the documented amounts.

**Acceptance Scenarios**:

1. **Given** the active floor is Floor 51, **When** the Monster Tuning Profile is resolved,
   **Then** it yields `investigateDuration = 4s`, `chaseHoldDuration = 3s`, `searchDuration = 6s`,
   `patrolSpeed = 1.0×`, `chaseSpeed = 1.4×` (GDD Ch. 6.2/17.4).
2. **Given** the active floor is Floor 50, **When** the Monster Tuning Profile is resolved,
   **Then** it yields `investigateDuration = 6s`, `chaseHoldDuration = 5s`, `searchDuration = 8s`,
   `patrolSpeed = 1.2×`, `chaseSpeed = 1.5×`.
3. **Given** a designer changes any one of these ten numbers (five values × two floors) in the
   `GameConfig` asset, **When** the game is next run without a code change, **Then** the new
   value takes effect.

---

### User Story 2 - The hard rule against an unbeatable monster is enforced (Priority: P1)

GDD Ch. 6.2 states explicitly, in bold, that chase speed must never reach or exceed the player's
sprint speed — "kalau monster lebih cepat dari sprint, chase berubah dari menegangkan jadi hukuman
yang tidak bisa dihindari" (if the monster is faster than sprint, chase turns from tense into an
inescapable punishment). This story makes that rule impossible to silently violate.

**Why this priority**: Equal priority to User Story 1 — a fast, correctly-varying monster that
occasionally becomes literally unescapable is worse than no variation at all; this is a
correctness gate on top of Story 1's data, not an optional nicety.

**Independent Test**: In an EditMode test (no scene needed), construct a candidate tuning profile
whose `chaseSpeed` is set to equal or exceed the configured `sprintMultiplier`, run it through the
validator, and assert it is rejected. Then set it one increment below and assert it passes.

**Acceptance Scenarios**:

1. **Given** a Monster Tuning Profile whose `chaseSpeed ≥ GameConfig.sprintMultiplier`, **When**
   the hard-rule validator runs, **Then** it reports a failure identifying which floor/value
   violated it.
2. **Given** both shipped profiles (Floor 51's 1.4× and Floor 50's 1.5×) and the shipped
   `sprintMultiplier` (1.6×, GDD Ch. 17.1), **When** the hard-rule validator runs at editor-time
   and as part of the automated test suite, **Then** both profiles pass.
3. **Given** a designer later changes `sprintMultiplier` itself (e.g., down to 1.4× during
   balancing) such that a previously-valid `chaseSpeed` (1.4× or 1.5×) now violates the rule,
   **When** the validator next runs, **Then** it catches the now-invalid cross-field state — the
   check is re-evaluated against the current `sprintMultiplier`, not cached from when the profile
   was authored.

---

### User Story 3 - Floor 52 correctly has no monster tuning to resolve (Priority: P2)

Floor 52 has no monster at all (GDD Ch. 6.3). Querying a Monster Tuning Profile for Floor 52 (or
any undefined floor id) must fail safely and loudly, never silently returning another floor's
numbers.

**Why this priority**: Lower priority than Stories 1–2 because it is a guard rail, not a
gameplay-visible behavior on its own — but still required, because a silent fallback here would be
a uniquely hard-to-notice bug (Floor 52 would appear fine — it has no monster to observe — right
up until someone wires a lookup for it by mistake).

**Independent Test**: In an EditMode test, request the tuning profile for Floor 52 and assert the
lookup throws/returns an explicit "no profile — no monster on this floor" result rather than a
default or another floor's struct.

**Acceptance Scenarios**:

1. **Given** the active floor is Floor 52, **When** `monsterActive` is queried, **Then** it
   reports `false` (GDD Ch. 17.4: "Floor 52 = false").
2. **Given** a lookup for a Monster Tuning Profile is attempted for Floor 52, **When** it
   executes, **Then** it fails explicitly (exception or a clearly-typed "no profile" result) —
   it never returns Floor 51's or Floor 50's values as a fallback.

---

### Edge Cases

- **Zero/negative duration authored by mistake**: any of `investigateDuration`,
  `chaseHoldDuration`, `searchDuration` set to ≤ 0 seconds MUST be treated as an authoring error
  (validation failure), since GDD Ch. 6.2 gives no floor a zero-length state.
- **`chaseSpeed` exactly equal to `sprintMultiplier`**: GDD Ch. 6.2 says chase speed "harus di
  bawah sprint player" (must be *below* sprint) — equality is a violation, not a boundary pass;
  the hard-rule check MUST use a strict `<`, not `≤`.
- **Cross-field revalidation**: because the hard rule spans two different `GameConfig` sections
  (`GameConfig.sprintMultiplier` in Ch. 17.1, `chaseSpeed` in Ch. 17.4), a change to *either* field
  must be able to invalidate the pair — the validator MUST always read both live values, never a
  value cached at profile-construction time.
- **Lookup for an undefined/typo'd floor id**: MUST fail the same explicit way as the Floor 52
  case (User Story 3) — not treated as "assume Floor 51."
- **A future Floor 53+ or a 4th floor**: explicitly out of scope — GDD Ch. 1.4/18.2 forbids adding
  a 4th floor; this spec only ever needs two active profiles (Floor 51, Floor 50).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `MonsterTuningProfile` data structure with exactly five
  tunable numeric fields — `patrolSpeed`, `chaseSpeed` (both multipliers of the player's
  `walkSpeed`), `investigateDuration`, `chaseHoldDuration`, `searchDuration` (all seconds) — and
  one boolean, `monsterActive`.
- **FR-002**: `GameConfig` MUST expose exactly one `MonsterTuningProfile` instance for Floor 51 and
  exactly one for Floor 50, as additive fields on the single existing `GameConfig`
  `ScriptableObject` asset (constitution: single configuration source; no second config asset).
- **FR-003**: Floor 51's profile values MUST be locked to: `investigateDuration = 4`,
  `chaseHoldDuration = 3`, `searchDuration = 6`, `patrolSpeed = 1.0`, `chaseSpeed = 1.4`,
  `monsterActive = true` (GDD Ch. 6.2/17.4 — explicit numbers, not open items).
- **FR-004**: Floor 50's profile values MUST be locked to: `investigateDuration = 6`,
  `chaseHoldDuration = 5`, `searchDuration = 8`, `patrolSpeed = 1.2`, `chaseSpeed = 1.5`,
  `monsterActive = true`.
- **FR-005**: System MUST enforce, for every floor's profile, the hard rule
  `chaseSpeed < GameConfig.sprintMultiplier` (currently 1.6×, GDD Ch. 17.1 — itself already
  locked, so this rule is fully checkable today with no open dependency). Violation MUST be
  reported as an explicit failure naming the offending floor and values, never silently clamped
  or ignored.
- **FR-006**: The hard-rule check (FR-005) MUST be implemented as plain C# logic that reads both
  `chaseSpeed` and `sprintMultiplier` fresh at validation time (no cached copy), so it stays
  correct if either value is later retuned independently.
- **FR-007**: System MUST run the hard-rule check (a) as an automated EditMode test against the
  shipped `GameConfig` values, and (b) as an editor-time validation (e.g., `OnValidate` on the
  `GameConfig` asset) so a designer sees the violation immediately in the Unity Inspector, not
  only in CI.
- **FR-008**: `MonsterStateMachine` (spec 001) and its `MonoBehaviour` adapter MUST read
  `investigateDuration`/`chaseHoldDuration`/`searchDuration`/`patrolSpeed`/`chaseSpeed`
  exclusively via a resolved `MonsterTuningProfile` — never as a literal constant anywhere in
  state-machine or controller code.
- **FR-009**: System MUST expose a single lookup operation, "given a floor identifier, return
  that floor's `MonsterTuningProfile`," with no duplicated floor-branching logic anywhere else in
  the codebase (constitution: single configuration source; DRY).
- **FR-010**: For Floor 52, System MUST report `monsterActive = false` and MUST NOT require a
  `MonsterTuningProfile` to exist for it — no monster instance is ever created on Floor 52 (GDD
  Ch. 6.3).
- **FR-011**: A lookup attempted for Floor 52 or any undefined/unrecognized floor identifier MUST
  fail explicitly (exception or an unambiguous "no profile" result type) — it MUST NOT return
  Floor 51's or Floor 50's profile as an implicit fallback.
- **FR-012**: `patrolSpeed` and `chaseSpeed` MUST be stored and interpreted as dimensionless
  multipliers of the player's `walkSpeed` (never as a literal m/s constant), matching GDD Ch.
  6.2's "Relatif terhadap walk speed player."
- **FR-013**: `investigateDuration`, `chaseHoldDuration`, and `searchDuration` MUST each be
  validated as strictly greater than zero for every floor that has `monsterActive = true`.
- **FR-014**: Changing any of the five tunable values for either floor MUST require editing only
  the `GameConfig` asset — no recompilation, no code edit, per constitution's single
  configuration source principle.

### Key Entities

- **`MonsterTuningProfile`**: the five-number-plus-flag data shape defined in FR-001. Owned by,
  and additive to, the single `GameConfig` asset (constitution Principle V).
- **`GameConfig` (extended)**: gains two new fields — the Floor 51 and Floor 50
  `MonsterTuningProfile` instances — plus reuses its existing `sprintMultiplier` field (GDD Ch.
  17.1) as the other half of the hard-rule check. This spec does not redefine `GameConfig`'s
  overall schema (owned by `shared-config-and-state/001-game-config-schema`), only adds to it.
- **Floor identifier**: this spec assumes an existing `FloorId`-shaped identifier (Floor52 /
  Floor51 / Floor50) already defined by `shared-config-and-state/001-game-config-schema`; it does
  not invent a new one.
- **`MonsterTuningValidator`** (plain C#): the hard-rule (FR-005/FR-006) and positive-duration
  (FR-013) checks, callable both from an EditMode test and from `GameConfig`'s editor-time
  validation hook.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Resolving the Floor 51 profile yields exactly `investigateDuration = 4s`,
  `chaseHoldDuration = 3s`, `searchDuration = 6s`, `patrolSpeed = 1.0×`, `chaseSpeed = 1.4×`, in
  100% of automated test runs.
- **SC-002**: Resolving the Floor 50 profile yields exactly `investigateDuration = 6s`,
  `chaseHoldDuration = 5s`, `searchDuration = 8s`, `patrolSpeed = 1.2×`, `chaseSpeed = 1.5×`, in
  100% of automated test runs.
- **SC-003**: 100% of shipped floor profiles pass the hard-rule check (`chaseSpeed <
  sprintMultiplier`) at all times; any authoring change that would violate it is caught before it
  reaches a build (editor validation error and/or a failing test), never discovered first through
  manual playtesting.
- **SC-004**: A designer can retune any of the ten numbers (five values × two floors) by editing
  only the `GameConfig` asset in the Inspector, with zero code changes and zero recompilation.
- **SC-005**: A lookup for Floor 52's tuning profile fails explicitly in 100% of test runs — it
  never returns a Floor 51 or Floor 50 value.

## Assumptions

- **No open numeric items in this spec's own scope**: unlike `noiseBaseRadius` (noise-and-detection
  system, GDD Ch. 21) or the various UX feel values (joystick, interaction radius), every number
  this spec locks (the five values × two floors) is given explicitly and without qualification in
  GDD Ch. 6.2/17.4 — there is nothing left to fabricate here, per the task's explicit direction to
  use the GDD's real numbers where the GDD actually gives them.
- **`sprintMultiplier` (1.6×) is already locked** (GDD Ch. 17.1, no `TBD` marker), so the hard-rule
  check in FR-005 has no unresolved dependency blocking it — it is fully enforceable today.
- **Floor identifier reuse**: this spec assumes `shared-config-and-state/001-game-config-schema`
  defines the floor-identifier type it looks profiles up by; if that spec ships a different shape
  than assumed, only the lookup signature (FR-009) needs to adapt — the five locked numbers and
  the hard rule are unaffected.
- **This spec does not decide *how* the profile reaches the state machine per-tick** (e.g.,
  resolved once at floor load vs. re-read every tick) — that wiring detail belongs to spec 001's
  `MonsterController` adapter (already covered by its own tasks) and to whichever
  scene-flow/progression spec loads a floor; this spec's job ends at "the correct profile is
  resolvable and correct," not at its exact call-site.

## Related

- GDD Ch. 6.2 (Monster AI Specification — Nilai per state) — the numeric table this spec locks
  verbatim.
- GDD Ch. 17.1 (Player) and 17.4 (Monster, per floor) — the `GameConfig` master tuning table
  entries this spec adds/reuses.
- `specs/ROADMAP.md` §2 "monster-ai" row `002-per-floor-tuning-profile` (depends on `001`,
  `shared-config-and-state/001`), and §0's shared-naming/feel-based-value conventions.
- `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md` — the consumer of
  `MonsterTuningProfile` via its `MonsterStateTuning` input shape.
- `specs/systems/shared-config-and-state/001-game-config-schema/spec.md` — owns `GameConfig`'s
  overall schema and the floor-identifier type this spec's lookup keys off of; this spec only
  adds fields, never redefines that schema.
