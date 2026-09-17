# Feature Specification: Scope Lock and Cut Order

**Feature Branch**: `001-scope-lock-and-cut-order`

**Created**: 2026-09-17

**Status**: Draft

**GDD Sources**: Ch. 18.1 (Must Have), 18.2 (Cut List), 18.3 (Kandidat potong pertama / Cut Order)

**Input**: User description: "The Must Have list (18.1) is a literal release gate — every item must be traceable to an implemented spec in this vault before ship. The Cut List (18.2) is a set of negative requirements — things that must never exist in the shipped build, auditable by code/asset review. The Cut Order (18.3) is a pre-agreed decision tree for cutting scope under time pressure, in a fixed sequence, with two categories of thing that may never be cut no matter how much pressure the team is under."

This spec is a process/QA gate, not runtime code. Its "acceptance scenarios" are audit procedures the team runs before submission, not gameplay behavior.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Must-Have Traceability Gate (Priority: P1)

Before any build is submitted, the team walks GDD Ch. 18.1 line by line. For every Must Have bullet, at least one spec in this vault is cited by path, and that spec's implementation is confirmed merged into main — not merely planned.

**Why this priority**: This is the actual ship/no-ship gate. Every other release-process spec (002, 003) only matters if the game itself contains everything the GDD locked as mandatory. Without this traceability, "done" has no evidence behind it.

**Independent Test**: An auditor opens this spec's Functional Requirements (FR-001–FR-012 below, one per Must Have bullet), opens every cited spec path, and for each confirms: (a) the spec's `spec.md` exists and its Functional Requirements are implemented in `Assets/Scripts` or the relevant Scene, and (b) the corresponding EditMode tests (constitution Principle IV) are green. A bullet with no cited spec, or a cited spec with no matching code, fails the gate.

**Acceptance Scenarios**:

1. **Given** the Phase 8 (Submission Lock) window has begun, **When** the traceability audit is run against GDD 18.1, **Then** all 12 Must Have bullets have at least one cited, implemented spec with green tests.
2. **Given** a Must Have bullet cites multiple specs (e.g., "battery system lengkap" cites four flashlight-and-battery specs), **When** the audit is run, **Then** every cited spec for that bullet is checked — a bullet is not "done" if only some of its cited specs are implemented.
3. **Given** a cited spec exists but its implementation is only partially merged (e.g., logic exists but is not wired into the Scene), **When** the audit is run, **Then** that Must Have bullet is marked NOT satisfied — a spec's existence on disk is not sufficient evidence.

---

### User Story 2 - Cut List Negative-Requirement Audit (Priority: P1)

Before any build is submitted, the team runs a repo-wide audit — code search plus manual design/asset review — confirming that none of the 13 items on GDD Ch. 18.2's Cut List exist anywhere in the main branch, in code, content, or design documents describing shipped behavior.

**Why this priority**: The Cut List exists precisely because these features will be proposed mid-development by well-meaning contributors. Ch. 18.2 pre-answers that proposal with "no, unless every Must Have is done and stable" — this user story is what makes that "no" enforceable rather than aspirational.

**Independent Test**: An auditor runs the keyword/pattern searches and manual reviews defined in FR-013–FR-025 (one per Cut List item) against the main branch and confirms zero matches. Any match — a stamina field on `PlayerCharacter`, a second monster prefab, a locker hiding spot, a fourth floor scene — fails the gate until removed or the spec is formally amended.

**Acceptance Scenarios**:

1. **Given** the main branch at any point before submission, **When** a code search for stamina/skill/crafting/combat-related identifiers is run, **Then** zero references are found (GDD 18.2: no stamina system, no combat/skill/crafting).
2. **Given** the Scenes list in `specs/ROADMAP.md` §0, **When** the scene inventory is checked, **Then** exactly the eight fixed scenes exist (`Bootstrap`, `MainMenu`, `Prologue`, `Floor52`, `Floor51`, `Floor50`, `GoodEnding`, `BadEnding`) and no `Floor49` or equivalent fourth-floor scene exists.
3. **Given** the hiding system implementation (`specs/systems/hiding/`), **When** its interactable set is reviewed, **Then** the only hiding interactable is the under-desk spot — no locker, bathroom, or cabinet interactable exists.
4. **Given** the narrative content (`specs/systems/narrative-content/`, `specs/scenes/007-good-ending-scene/`, `specs/scenes/008-bad-ending-scene/`), **When** reviewed, **Then** exactly two endings are reachable from gameplay (Good, Bad) and no Secret Ending or branching-narrative content exists.

---

### User Story 3 - Cut-Order Decision Tree Execution (Priority: P2)

If, during Phase 7 or Phase 8, the team determines the schedule requires cutting scope, they follow the pre-agreed four-tier order from GDD 18.3 — never inventing a different cut, and never touching the elements the GDD locks as permanently protected — and record which tier(s) were applied.

**Why this priority**: This decision only fires under schedule pressure, so it is lower priority than the two audits above (which always run). But GDD 18.3's entire premise is that this order is agreed *now*, precisely so nobody has to negotiate it in a panic later — this user story is what makes that pre-agreement binding rather than a suggestion.

**Independent Test**: Simulate a "time is short" scenario during Phase 7. Walk the decision tree in order: confirm the team considers cutting hiding (tier 1) before battery respawn (tier 2), tier 2 before Floor 50 reduction (tier 3), and tier 3 before the 3D→2D sprite fallback (tier 4). Confirm that at every tier, the three protected elements (one complete floor start-to-exit, a working monster, both endings) remain untouched, and that no cut outside this list was applied without documented Tech Lead + Design Lead sign-off.

**Acceptance Scenarios**:

1. **Given** the team decides scope must shrink, **When** the first cut is chosen, **Then** it is Tier 1 (hiding system) — no other tier or unlisted cut is applied first.
2. **Given** Tier 1 alone is insufficient, **When** further scope reduction is needed, **Then** Tier 2 (battery respawn → static) is applied next, in that order, not skipped to Tier 3 or 4.
3. **Given** any tier is applied, **When** the resulting scope is reviewed, **Then** all three protected elements — one fully complete floor playable start-to-exit, a working monster with its full state machine, and both Good and Bad endings reachable from gameplay — are still present and functional.
4. **Given** someone proposes cutting something not on the GDD 18.3 list (e.g., cutting Floor 51 instead of Floor 50, or disabling the monster), **When** that proposal is evaluated, **Then** it is rejected unless it carries documented, explicit Tech Lead + Design Lead sign-off overriding the pre-agreed order — silent deviation fails the gate.

---

### Edge Cases

- **Must Have item cited to a spec that itself doesn't exist yet on disk**: the traceability audit (US1) treats this as NOT satisfied — a ROADMAP.md row is a plan, not evidence of implementation.
- **Cut List item appears only on an unmerged feature branch, never in main**: out of scope for the US2 audit, which is defined over the main branch only; however, if that branch is merged before submission, the audit must be re-run and will then fail.
- **Cutting Tier 3 (Floor 50 → 2-floor game) appears to conflict with "3 floor" in the Must Have list (18.1)**: it does not — Tier 3 is an explicitly pre-agreed *amendment* to the Must Have list under schedule pressure, not a silent violation of it. If Tier 3 is invoked, FR-012 (3 floors) is considered satisfied by the reduced 2-floor scope for that submission, and this must be recorded (FR-029) rather than left as an unexplained discrepancy against 18.1.
- **Applying Tier 3 still leaves two complete floors (Floor 52, Floor 51)**: this does not violate the "one fully complete floor" protection — the protection is a floor, singular, not a specific one, and is satisfied as long as at least one floor is playable start-to-exit.
- **Multiple cut tiers needed in the same crunch window**: tiers are applied strictly in order (1, then 2, then 3, then 4) — the team may not skip ahead to Tier 4 while Tier 1–3 have not yet been tried, per FR-026.
- **A cut candidate would remove a protected element outright** (e.g., someone proposes cutting the monster to save time): this is rejected unconditionally regardless of any sign-off — protected elements (FR-027) have no override path, unlike the ordering rule in FR-026/FR-029.

## Requirements *(mandatory)*

### Functional Requirements

**Must Have traceability (GDD 18.1) — one requirement per bullet:**

- **FR-001**: Movement, sprint, flashlight, and context-sensitive interaction MUST each be traceable to an implemented, tested spec before ship: `specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md` (movement + sprint); `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius/spec.md` (flashlight); `specs/systems/interaction-and-highlight/001-nearest-interactable-detection/spec.md` and `.../002-context-sensitive-action-button/spec.md` (context-sensitive interaction).
- **FR-002**: The complete battery system (1 installed + 1 spare, tiered light state, Compact Darkness) MUST be traceable to: `specs/systems/flashlight-and-battery/001-light-state-thresholds-and-radius/spec.md` (tiers + Compact Darkness), `002-battery-real-time-drain-timer/spec.md`, `007-battery-pickup-and-spare-slot/spec.md`, `008-battery-install-and-refill/spec.md`.
- **FR-003**: The battery respawn system for Floor 51 and Floor 50 MUST be traceable to: `specs/systems/battery-spawn-system/001-per-floor-spawn-point-registry/spec.md`, `002-active-battery-count-cap/spec.md`, `003-respawn-timer-and-placement-rule/spec.md`.
- **FR-004**: Three floors with doors, keys, and a checkpoint MUST be traceable to: `specs/scenes/004-floor-52-scene/001-level-layout-and-geometry/spec.md`, `specs/scenes/005-floor-51-scene/001-level-layout-and-geometry/spec.md`, `specs/scenes/006-floor-50-scene/001-level-layout-and-geometry/spec.md` (three floors); `specs/systems/keys-and-doors/001-key-pickup-and-inventory/spec.md` through `004-door-visual-and-color-feedback/spec.md` (doors + keys); `specs/systems/lives-and-fail-state/001-lives-count-and-checkpoint/spec.md` (checkpoint).
- **FR-005**: One monster with PATROL, INVESTIGATE, CHASE, SEARCH, and CATCH states MUST be traceable to: `specs/systems/monster-ai/001-state-machine-core-transitions/spec.md`, `002-per-floor-tuning-profile/spec.md`, `003-spawn-point-validation-rules/spec.md`, `004-catch-outcome-signal/spec.md`.
- **FR-006**: The noise-radius detection system MUST be traceable to: `specs/systems/noise-and-detection/001-per-action-noise-emission/spec.md`, `002-distance-based-detection-check/spec.md`.
- **FR-007**: Limited hiding (under-desk only) MUST be traceable to: `specs/systems/hiding/001-enter-and-exit-hiding/spec.md`, `002-hiding-detection-immunity-rule/spec.md`, `003-hiding-audio-and-light-dampening/spec.md`.
- **FR-008**: Progression Floor 52 → 51 → 50 → Final Door MUST be traceable to: `specs/systems/progression-and-scene-flow/001-floor-numbering-and-splash-text/spec.md` through `003-full-run-completion-tracking/spec.md`, `specs/systems/keys-and-doors/003-final-door-distinct-behavior/spec.md`, `specs/scenes/006-floor-50-scene/004-final-door-placement-and-trigger/spec.md`.
- **FR-009**: Narrative (prologue, Good ending, Bad ending) MUST be traceable to: `specs/scenes/003-prologue-scene/spec.md`, `specs/systems/narrative-content/001-eddie-character-bible/spec.md`, `specs/scenes/007-good-ending-scene/spec.md`, `specs/scenes/008-bad-ending-scene/spec.md`.
- **FR-010**: UX (pause, restart, audio settings, How To Play, interaction feedback) MUST be traceable to: `specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/spec.md` (pause and restart — the audit MUST confirm this spec's implementation literally includes a restart-run action, not only pause/resume, since GDD 18.1 lists "restart" as its own bullet), `002-settings-menu-audio-controls/spec.md` (audio settings), `003-how-to-play-screen/spec.md` (How To Play), `specs/systems/interaction-and-highlight/002-context-sensitive-action-button/spec.md` and `003-interactable-highlight-halo/spec.md` (interaction feedback).
- **FR-011**: Audio (ambience, player SFX, dynamic monster SFX, chase SFX) MUST be traceable to: `specs/systems/audio/001-sound-event-taxonomy/spec.md`, `002-dynamic-mix-state-machine/spec.md`.
- **FR-012**: Haptic feedback MUST be traceable to: `specs/systems/haptics/001-haptic-trigger-events/spec.md`.

**Cut List negative requirements (GDD 18.2) — one requirement per item, each auditable as "zero references found":**

- **FR-013**: A code review MUST find zero references to a complex inventory system (multi-slot item management beyond the single key/battery-slot model already specced) anywhere in `Assets/Scripts`.
- **FR-014**: A code/design review MUST find zero references to an elaborate puzzle system (any puzzle mechanic beyond key-finds-door) in code, level design docs, or Scene content.
- **FR-015**: An asset and code review MUST find zero references to a second monster type, monster variant, or any enemy other than the single `Monster` defined in `specs/systems/monster-ai/`.
- **FR-016**: A code review MUST find zero procedural level-generation code — every floor's geometry MUST be authored, fixed content (GDD 16.1), matching `specs/scenes/004-floor-52-scene/`, `005-floor-51-scene/`, `006-floor-50-scene/`.
- **FR-017**: A code/asset review of `specs/systems/hiding/` and every floor Scene's hiding-spot placement spec MUST find zero hiding interactables other than the under-desk spot (no locker, bathroom, or cabinet).
- **FR-018**: A code review MUST find zero combat mechanics (attack/damage-to-monster actions), zero skill/ability system, and zero crafting system.
- **FR-019**: A narrative-content and scene review MUST find zero implemented Secret Ending content reachable from gameplay (GDD 10.4 explicitly defers this post-launch).
- **FR-020**: A narrative-content review MUST find zero branching-narrative structures beyond the fixed Good/Bad ending split already specced (no player-choice-driven story forks).
- **FR-021**: A code/asset review MUST find zero references to multiple weapons or tools — the player's only carried item is the flashlight/battery system.
- **FR-022**: A code review of `specs/systems/noise-and-detection/` and `specs/systems/flashlight-and-battery/` MUST find zero logic that triggers monster detection from flashlight illumination — detection MUST be noise-only (GDD 7.2).
- **FR-023**: A code review of `Assets/Scripts` MUST find zero stamina bar, stamina meter, or stamina-depletion field on `PlayerCharacter` or `GameState`.
- **FR-024**: A code review of the sprint and battery-drain logic (`specs/systems/movement-and-camera/001-joystick-movement-and-sprint/spec.md`, `specs/systems/flashlight-and-battery/002-battery-real-time-drain-timer/spec.md`) MUST find zero coupling — sprinting MUST NOT drain battery faster or at all beyond the fixed real-time drain.
- **FR-025**: A Scene-inventory review MUST find exactly the eight fixed scenes named in `specs/ROADMAP.md` §0, with zero fourth-floor scene (no `Floor49` or equivalent) present.

**Cut-order decision tree (GDD 18.3) and protected elements:**

- **FR-026**: When schedule pressure requires cutting scope before submission, cuts MUST be applied strictly in this order, never skipping ahead: Tier 1 (hiding system removed — GDD: "Game tetap utuh. LILO jadi murni chase/evasion."), then Tier 2 (battery respawn replaced with static placement — GDD: "Balancing jadi lebih kasar tapi tetap main."), then Tier 3 (Floor 50 removed, game becomes a 2-floor run — GDD: "Durasi turun ke ±10 menit. Ending tetap jalan."), then Tier 4 (3D character/monster models replaced with 4-direction 2D sprites via the `characterRenderMode` cut switch — GDD Ch. 11.2, 15 — "Visual kurang menarik, tapi gameplay tidak berubah.").
- **FR-027**: The following MUST NEVER be cut, at any tier, under any schedule pressure, with no override path: (a) at least one floor that is fully complete and playable start-to-exit; (b) a working monster with its full PATROL/INVESTIGATE/CHASE/SEARCH/CATCH state machine; (c) both the Good Ending and the Bad Ending, reachable from gameplay.
- **FR-028**: Applying any cut-order tier (FR-026) MUST NOT compromise any protected element (FR-027) — before a tier is applied, the team MUST confirm the resulting scope still satisfies FR-027 in full.
- **FR-029**: Any scope cut not on the GDD 18.3 list, or applied out of the FR-026 order, MUST carry documented, explicit sign-off from both the Tech Lead and the Design Lead (GDD 19.1 roles) before it is applied — undocumented or unilateral deviation fails this gate regardless of outcome.
- **FR-030**: The Must Have traceability audit (FR-001–FR-012) and the Cut List audit (FR-013–FR-025) MUST both be re-run against the literal Phase 8 (Submission Lock) build, not only at an earlier phase — a gate passed once earlier in development does not carry forward automatically.

### Key Entities

- **Must Have Item**: one of the 12 GDD 18.1 bullets; carries an id, the GDD text, one or more cited spec paths, and an implementation status (satisfied / not satisfied).
- **Cut List Item**: one of the 13 GDD 18.2 bullets; carries an id, the GDD text, and an audit method (code search pattern, asset review, or manual design review) that must return zero matches.
- **Cut-Order Tier**: one of the four GDD 18.3 tiers; carries a rank (1–4), the cut it applies, its stated gameplay impact, and a re-check of the three Protected Elements after applying it.
- **Protected Element**: one of the three things GDD 18.3 names as never-cuttable (one complete floor, a working monster, both endings); has no override path, unlike a Cut-Order Tier deviation (which can be signed off).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 12 of 12 Must Have items (FR-001–FR-012) have at least one cited, implemented, tested spec at Phase 8 sign-off.
- **SC-002**: 0 matches found across all 13 Cut List audits (FR-013–FR-025) against the main branch at Phase 8 sign-off.
- **SC-003**: If any scope cut occurs before submission, 100% of applied cuts either match the pre-agreed FR-026 order exactly, or carry a documented dual sign-off per FR-029 — 0 undocumented or out-of-order cuts.
- **SC-004**: 100% of submission candidates retain all three Protected Elements (FR-027), verified independently of which Cut-Order tiers (if any) were applied.
- **SC-005**: The full traceability and Cut List audits (SC-001, SC-002) are re-run and re-pass against the literal Phase 8 build artifact, not only against an earlier development snapshot.

## Assumptions

- Every cited spec path in FR-001–FR-012 is expected to eventually carry its own `spec.md`, `tasks.md`, and `checklists/requirements.md` per `specs/ROADMAP.md` §2/§3 — this gate spec does not author those; it only enforces that they exist and are implemented by ship time.
- "Restart" (GDD 18.1) is assumed to be delivered as an action within the pause menu spec (`specs/systems/game-shell-ui/001-pause-menu-and-time-freeze/spec.md`) rather than its own dedicated spec, since no separate ROADMAP.md row exists for it — the FR-010 audit explicitly checks this assumption holds and fails the gate if that spec ships without a restart action.
- The Cut-Order tiers (FR-026) are assumed to be applied at most once each per submission cycle — GDD 18.3 does not describe a "partial" application (e.g., cutting hiding on only one floor), so this spec treats each tier as an all-or-nothing scope change.

## Related

- GDD: `specs/_reference/LILO-GDD-v2-Production-Lock.md` — Ch. 18.1, 18.2, 18.3 (and Ch. 19.1 for the Tech Lead / Design Lead names used in FR-029's sign-off requirement).
- `specs/ROADMAP.md` — §0 (fixed scene list, used by FR-025), §2 and §3 (every spec path cited in FR-001–FR-012), §4 row 1 (this spec's own place in the release gate sequence), §5 (build order — this gate runs only after every upstream system/scene spec in the dependency graph).
