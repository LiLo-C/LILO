# LILO — Spec Roadmap (Unity, one spec per small feature)

## Current integration slice (2026-09-23)

The spec tables below remain the long-term GDD contract. The playable Unity build currently
uses `OfficeLevel1`, `OfficeLevel2`, and `OfficeLevel3` as temporary content copies while the
authored `Floor52`, `Floor51`, and `Floor50` scenes are pending. Their runtime mapping is
OfficeLevel1 → Floor52, OfficeLevel2 → Floor51, OfficeLevel3 → Floor50. MainMenu starts the
first office level; exits advance through all three, then load GoodEnding. Death restarts the
current office level or loads BadEnding when lives reach zero.

| Workstream | Current scope | Owner/status |
|---|---|---|
| 2D visual assets | UI, story, and other 2D assets | Salwa — in progress |
| Battery spawn system | Spawn registry, active caps, respawn and placement | Radit — in progress |
| Three-floor integration | Copy OfficeLevel1 content into OfficeLevel2/3, flow, simple gameplay UI | Implemented; Unity scene validation passed |
| Monster feedback | Patrol silent, Alert 3 short pulses, Chase 3-second pulse; slower patrol speed | Implemented; iPhone feel check pending |
| Remaining authored content | Distinct floor layouts, keys/doors, narrative, audio, release QA | Pending; see sections 2–4 |

`tasks.md` checkboxes are not a reliable implementation status yet: most are unchecked even
where runtime code exists. Reconcile each feature's checklist against the scene and code before
marking it complete. Content copies are temporary and do not complete the individual floor
layout or placement specs.

Source of truth for every spec below: [[LILO-GDD-v2-Production-Lock]] (v2.1) and
[[constitution]] (v3.0.0). This roadmap is itself the shared contract every spec folder is
written against — fixed names below MUST be reused verbatim, never reinvented per-spec.

## 0. Shared naming conventions (fixed — do not rename)

- **Config**: one `GameConfig` `ScriptableObject` class, one asset `Assets/Config/GameConfig.asset`. Every feature spec that needs a tunable adds a field to it — never a second config source (constitution Technology Stack, GDD Ch. 17).
- **State**: one plain C# `GameState` class, owned by one `GameManager` `MonoBehaviour` that persists across scene loads (lives in the Bootstrap scene, `DontDestroyOnLoad`). Plain-C# game-rule classes (no `MonoBehaviour`/scene dependency) live under `Assets/Scripts/Systems/`; thin Unity-lifecycle adapters live under `Assets/Scripts/MonoBehaviours/`; UI under `Assets/Scripts/UI/` (constitution Principle III).
- **Testing**: Unity Test Framework, EditMode by default (constitution Principle IV). Every feature spec's `tasks.md` includes explicit EditMode test tasks for its pure-logic pieces.
- **Scenes**: `Assets/Scenes/Bootstrap.unity`, `MainMenu.unity`, `Prologue.unity`, `Floor52.unity`, `Floor51.unity`, `Floor50.unity`, `GoodEnding.unity`, `BadEnding.unity`.
- **Player character name in code**: `PlayerCharacter`. **Monster**: `Monster`. **Battery**: `Battery`. **Door**: `Door`. **Key**: `Key`.
- **Units**: world units are meters (Unity default); light/shadow numeric defaults inherited from the project's earlier native-engine on-device tuning are marked *(carried over — re-tune)* in each spec's config table, matching the precedent set in the prior 001/001-1 Unity specs (now superseded by this vault, but the same carried-over numbers still apply since gameplay math is engine-independent).
- **Feel-based values (light radius, camera tilt, joystick deadzone, etc.)**: every such FR states the *rule* (e.g. "radius MUST ease toward target, never jump") and requires the value to live in `GameConfig`; it does NOT invent a fake precise final number where none is validated yet. Where a validated starting number exists from prior on-device work, it's given as a default to re-confirm, not as a locked constant — this is a deliberate, honest limit of what a spec can pin down for feel-driven content (see conversation decision 2026-09-17: spec-driven development is scoped to what's actually deterministic/testable; feel is validated by playtesting, not by a fabricated acceptance number).

## 1. Taxonomy

```text
specs/
├── systems/<category>/<NNN>-<feature>/        # reusable engineering, cross-scene
├── scenes/<NNN>-<scene>/<feature>/             # one Unity Scene's own content specs
└── release/<NNN>-<feature>/                    # process/QA gates, not runtime code
```

Every feature folder contains at minimum `spec.md` (user stories, FR, SC — full spec-kit rigor)
and `tasks.md` (broken into the smallest sensible independently-completable units — prefer many
single-responsibility tasks over broad ones). `research.md`/`data-model.md`/`contracts/` are
added where the feature has real technical decisions or a shared data shape to document.

## 2. Systems — full list

| Category | Feature spec | GDD source | Depends on |
|---|---|---|---|
| **shared-config-and-state** | 001-game-config-schema | Ch. 17 (all) | — |
| | 002-shared-game-state-and-manager | (Unity architecture necessity; constitution Principle III) | 001 |
| **movement-and-camera** | 001-joystick-movement-and-sprint | Ch. 4.1 | shared-config-and-state/002 |
| | 002-wall-and-furniture-collision | (feel/physicality requirement, sliding not stopping) | 001 |
| | 003-camera-follow-and-boundary-clamp | Ch. 13 (follow, clamp) | 001 |
| | 004-camera-projection-and-framing | Ch. 13 (orthographic, tilt, zoom) | 003 |
| **flashlight-and-battery** | 001-light-state-thresholds-and-radius | Ch. 5.2 | shared-config-and-state/002 |
| | 002-battery-real-time-drain-timer | Ch. 5.1 | 001 |
| | 003-eased-radius-transitions | Ch. 12.1 (no jump) | 001 |
| | 004-flicker-event-system | Ch. 12.1 (flicker only in Flickering) | 001, 003 |
| | 005-shadow-casting-and-quality-fallback | Ch. 12 (real shadows) | 001 |
| | 006-readability-fill-light | Ch. 12.1 (room stays legible) | 001 |
| | 007-battery-pickup-and-spare-slot | Ch. 5.1 (slot capacity) | shared-config-and-state/002 |
| | 008-battery-install-and-refill | Ch. 4.2, 5.2 (install gate, full refill) | 001, 007 |
| | 009-battery-hud-indicator | Ch. 15.1 | 002, 007 |
| **interaction-and-highlight** | 001-nearest-interactable-detection | Ch. 4.2 | shared-config-and-state/002 |
| | 002-context-sensitive-action-button | Ch. 4.2 | 001 |
| | 003-interactable-highlight-halo | Ch. 4.2 | 001 |
| **hiding** | 001-enter-and-exit-hiding | Ch. 4.3 | movement-and-camera/001, interaction-and-highlight/002 |
| | 002-hiding-detection-immunity-rule | Ch. 4.3 | 001, monster-ai/001 |
| | 003-hiding-audio-and-light-dampening | Ch. 4.3 | 001, audio/002 |
| **monster-ai** | 001-state-machine-core-transitions | Ch. 6.1 | noise-and-detection/002 |
| | 002-per-floor-tuning-profile | Ch. 6.2, 17.4 | 001, shared-config-and-state/001 |
| | 003-spawn-point-validation-rules | Ch. 6.3 | 001 |
| | 004-catch-outcome-signal | Ch. 6.1 (CATCH), 9.3 | 001 |
| **noise-and-detection** | 001-per-action-noise-emission | Ch. 7.1, 17.3 | movement-and-camera/001, flashlight-and-battery/008, hiding/001 |
| | 002-distance-based-detection-check | Ch. 7.2 | 001 |
| **keys-and-doors** | 001-key-pickup-and-inventory | Ch. 8.1 | interaction-and-highlight/002 |
| | 002-locked-door-unlock-logic | Ch. 8.1 | 001, interaction-and-highlight/002 |
| | 003-final-door-distinct-behavior | Ch. 8.1, 8.2, 21 (open item) | 002 |
| | 004-door-visual-and-color-feedback | Ch. 8.2 | 002, 003 |
| **battery-spawn-system** | 001-per-floor-spawn-point-registry | Ch. 3.1 | flashlight-and-battery/007 |
| | 002-active-battery-count-cap | Ch. 3.1 | 001 |
| | 003-respawn-timer-and-placement-rule | Ch. 3.1 | 001, 002 |
| **lives-and-fail-state** | 001-lives-count-and-checkpoint | Ch. 9.1 | shared-config-and-state/002 |
| | 002-floor-state-reset-on-death | Ch. 9.2 | 001, battery-spawn-system/*, keys-and-doors/* |
| | 003-death-sequence-and-outcome-branch | Ch. 9.3 | 001, 002, monster-ai/004 |
| **progression-and-scene-flow** | 001-floor-numbering-and-splash-text | Ch. 2.4, 8.2 | shared-config-and-state/002 |
| | 002-scene-transition-manager | (Unity architecture: which Scene loads next) | 001 |
| | 003-full-run-completion-tracking | Ch. 2.3 | 002 |
| **audio** | 001-sound-event-taxonomy | Ch. 14.1 | — |
| | 002-dynamic-mix-state-machine | Ch. 14.2 | 001, monster-ai/001 |
| | 003-audio-asset-sourcing-and-licensing-log | Ch. 14.3, 19.4 | 001 |
| **haptics** | 001-haptic-trigger-events | Ch. 15.2 | flashlight-and-battery/001, lives-and-fail-state/003, interaction-and-highlight/002 |
| **game-shell-ui** | 001-pause-menu-and-time-freeze | Ch. 15.1, 18.1 | shared-config-and-state/002 |
| | 002-settings-menu-audio-controls | Ch. 18.1 | audio/002 |
| | 003-how-to-play-screen | Ch. 9.1, 15.4 | 001 |
| | 004-hud-composition-and-layout | Ch. 15.1 | flashlight-and-battery/009, interaction-and-highlight/002, movement-and-camera/001 |
| **narrative-content** | 001-eddie-character-bible | Ch. 10.1 | — |
| | 002-foreshadowing-prop-catalog | Ch. 10.3 | 001 |

## 3. Scenes — full list

| Scene | Feature spec | GDD source | Depends on (systems) |
|---|---|---|---|
| **001-bootstrap-scene** | (single spec, no sub-split — its job is one thing: stand up persistent systems then load MainMenu) | Unity architecture necessity | shared-config-and-state/* |
| **002-main-menu-scene** | (single spec — Title screen + navigation wiring to shared How To Play/Settings/Pause systems) | Ch. 15.1 (shell), implied Title screen | game-shell-ui/002, 003 |
| **003-prologue-scene** | (single spec — one linear comic-panel sequence, beats 1–7) | Ch. 10.2, 10.1 | narrative-content/001 |
| **004-floor-52-scene** (Learn) | 001-level-layout-and-geometry | Ch. 16.1, 16.2, 16.3 | — |
| | 002-onboarding-beat-sequencing | Ch. 15.4 | movement-and-camera/001, flashlight-and-battery/*, hiding/001, noise-and-detection/001 |
| | 003-static-battery-and-hiding-placement | Ch. 3 (Floor 52 column) | battery-spawn-system/001, hiding/001 |
| | 004-distant-monster-hint-audio | Ch. 3 ("hanya SFX dari kejauhan") | audio/001 |
| | 005-floor-exit-and-transition | Ch. 8.1, 8.2 | keys-and-doors/004, progression-and-scene-flow/001, 002 |
| **005-floor-51-scene** (Pressure) | 001-level-layout-and-geometry | Ch. 16.1–16.3 | — |
| | 002-monster-patrol-route-and-spawn-presets | Ch. 6.3 | monster-ai/003 |
| | 003-key-and-locked-door-placement | Ch. 8.1 (1 key, 1 door) | keys-and-doors/001, 002 |
| | 004-battery-spawn-point-placement | Ch. 3.1 (max 2, 30s respawn) | battery-spawn-system/* |
| | 005-hiding-spot-placement | Ch. 3 (Floor 51 column) | hiding/001 |
| | 006-floor-exit-and-transition | Ch. 8.2 | progression-and-scene-flow/001, 002 |
| **006-floor-50-scene** (Mastery + Final Door) | 001-level-layout-and-geometry | Ch. 16.1–16.3 | — |
| | 002-monster-patrol-route-and-spawn-presets | Ch. 6.3 (aggressive tuning) | monster-ai/003 |
| | 003-three-keys-and-locked-doors-placement | Ch. 8.1 (3 keys/doors) | keys-and-doors/001, 002 |
| | 004-final-door-placement-and-trigger | Ch. 8.1, 8.2, 21 | keys-and-doors/003 |
| | 005-battery-spawn-point-placement | Ch. 3.1 (max 1, 60s respawn) | battery-spawn-system/* |
| | 006-hiding-spot-placement | Ch. 3 (Floor 50 column, rarer) | hiding/001 |
| **007-good-ending-scene** | (single spec) | Ch. 10.4 (Good) | narrative-content/001, progression-and-scene-flow/003 |
| **008-bad-ending-scene** | (single spec) | Ch. 10.4 (Bad) | narrative-content/001, lives-and-fail-state/003 |

## 4. Release

| Feature spec | GDD source |
|---|---|
| 001-scope-lock-and-cut-order | Ch. 18.1, 18.2, 18.3 |
| 002-playtest-and-bug-priority-process | Ch. 19.3 |
| 003-ai-and-cc0-asset-policy | Ch. 19.4 |

Ch. 1 (Vision & Pillars), 19.1 (roles) and 19.2 (phase plan) are project-management context, not
independently spec-able features — they stay narrative context in the GDD itself; every spec
above must still read as satisfying the four Design Pillars (Ch. 1.3), but that's a review
criterion, not its own spec.

## 5. Dependency-ordered build sequence

```text
shared-config-and-state (001→002)
   │
   ├─ movement-and-camera (001→002, 003→004)
   ├─ flashlight-and-battery (001→002→003→004, 001→005, 001→006, 007→008→009)
   ├─ interaction-and-highlight (001→002→003)
   │
   ├─ hiding (needs movement, interaction, monster-ai/001)
   ├─ noise-and-detection (needs movement, flashlight/008, hiding)
   ├─ monster-ai (001→002/003/004, needs noise-and-detection/002)
   ├─ keys-and-doors (needs interaction)
   ├─ battery-spawn-system (needs flashlight-and-battery/007)
   ├─ lives-and-fail-state (needs battery-spawn-system, keys-and-doors, monster-ai/004)
   ├─ progression-and-scene-flow (001→002→003)
   ├─ audio, haptics (need monster-ai/001, lives-and-fail-state/003, etc. — see table)
   ├─ game-shell-ui (needs flashlight-and-battery/009, interaction/002, movement/001)
   └─ narrative-content (no dependencies — can start anytime)
   │
   ▼
scenes/001-bootstrap → 002-main-menu → 003-prologue
   → 004-floor-52 (all 5 feature specs) → 005-floor-51 (all 6) → 006-floor-50 (all 6)
   → 007-good-ending / 008-bad-ending
   │
   ▼
release/001–003 (final gate before submission)
```

## 6. How an agent picks up a feature

1. Read this ROADMAP's row for the feature (GDD source, dependencies) and the relevant GDD
   chapter(s) in full.
2. Read the constitution for the Unity architecture/testing conventions.
3. Write `spec.md` with full spec-kit rigor: user stories (priority-ordered, each with an
   Independent Test), functional requirements, edge cases, key entities, measurable success
   criteria. For feel-based values, state the rule and the config field, not a fabricated exact
   number (§0 above).
4. Write `tasks.md`, broken into the smallest sensible independently-completable units, grouped
   by user story, each with an exact file path under `Assets/Scripts/...`.
5. Add `research.md`/`data-model.md`/`contracts/` only where the feature has a real technical
   decision or shared data shape worth recording — not as boilerplate for its own sake.
