# LILO — Spec Roadmap

The whole of [[LILO-GDD-v2-Production-Lock]] is split into 17 Spec Kit features. Each one is
already **specified** (`spec.md` + `checklists/requirements.md`). An implementing agent does
**not** re-split anything. It picks the next spec, clarifies it if needed, then runs plan → tasks
→ implement.

## 1. Spec list

| # | Spec | GDD phase | Proposed owner (GDD 19.1) | Must-have (GDD 18.1) | Cut switch (GDD 18.3) | Open clarification |
|---|---|---|---|---|---|---|
| 001 | [[001-core-prototype/spec\|Core Movement & Light Prototype]] | Fase 1 | Calzy, Eca | movement, sprint, flashlight, interaction, battery system | — | — (GDD alignment pass added; run analyze + converge) |
| 001-1 | [[001-1-unified-3d-world/spec\|Unified 3D World & Feel Pass]] | Fase 1 | Calzy, Eca | flashlight (feel), movement (feel) | shadows off (`FR-022`) | — (clarified 2026-09-17) |
| 002 | [[002-monster-ai-noise/spec\|Monster AI & Noise Detection]] | Fase 2 | Radit | 1 monster, 5 states; noise detection | — | — |
| 003 | [[003-hiding-under-desk/spec\|Hiding Under Desks]] | Fase 2 | Radit, Eca | limited hiding | **#1** `hidingEnabled` | — |
| 004 | [[004-floor-level-framework/spec\|Floor & Level Framework]] | Fase 3 prerequisite | Calzy, Fathia | 3 floors, progression | #3 `floorCount` | battery carry-over on descend |
| 005 | [[005-keys-locked-doors/spec\|Keys, Locked Doors & Final Door]] | Fase 4 | Calzy, Eca | doors + keys | — | — |
| 006 | [[006-battery-spawn-system/spec\|Battery Placement & Respawn]] | Fase 3 (static) / Fase 4 (respawn) | Eca | battery respawn | **#2** `batteryModeFloorNN` | — |
| 007 | [[007-lives-checkpoint-fail-state/spec\|Lives, Checkpoint & Fail State]] | Fase 4 | Calzy | checkpoint | — | — |
| 008 | [[008-floor-52-onboarding/spec\|Floor 52 — Onboarding]] | Fase 3 | Fathia | 3 floors | — | — |
| 009 | [[009-floor-51-pressure/spec\|Floor 51 — Pressure]] | Fase 4 | Fathia, Radit | 3 floors | — | — |
| 010 | [[010-floor-50-mastery/spec\|Floor 50 — Mastery & Final Door]] | Fase 4 | Fathia, Radit | 3 floors, Final Door | **#3** `floorCount = 2` | — |
| 011 | [[011-narrative-prologue-endings/spec\|Prologue & Endings]] | Fase 5 | Salwa, Calzy | prologue, Good + Bad ending | never cut | Final Door explanation (GDD 21) |
| 012 | [[012-game-shell-ux/spec\|Game Shell UX]] | Fase 5 | Eca, Salwa | pause, restart, audio settings, How To Play | — | meaning of "Restart" |
| 013 | [[013-audio-system/spec\|Audio System & Dynamic Mixing]] | engine early, assets Fase 6 | *unassigned* | ambience, player/monster/chase SFX | — | directional vs distance-only monster audio |
| 014 | [[014-haptics/spec\|Haptics]] | Fase 6 | Eca | haptic feedback | — | — |
| 015 | [[015-art-lighting-pass/spec\|Art, Models & Lighting Pass]] | Fase 6 | Fathia, Eileen, Salwa | — (DoD: no placeholders) | **#4** `characterRenderMode` | — |
| 016 | [[016-environmental-storytelling/spec\|Environmental Storytelling]] | Fase 6 | Fathia, Eileen, Salwa | — | reduce hint count | — |
| 017 | [[017-playtest-submission-lock/spec\|Playtest & Submission Lock]] | Fase 7–8 | Calzy + team | — | enforces cut order | — |

**Never cut (GDD 18.3):** one complete floor from start to exit, a working monster, and both endings.

## 2. Build order

```text
Fase 1  001 ──(analyze + converge the alignment delta) ──► 001-1 (unified 3D world)
          │
Fase 2  002 ─────────────► 003
          │
Fase 3  004 ──► 006 (static) ──► 008 ◄── 013 engine + placeholder sounds (pull forward)
          │
Fase 4  ├──► 005 ─┐
        ├──► 007 ─┼──► 006 (respawn) ──► 009 ──► 010
          │
Fase 5  012 ──► 011
          │
Fase 6  013 (final assets) · 014 · 015 · 016     (parallel, by owner)
          │
Fase 7–8  017
```

Parallel lanes that do not touch each other's files much:

- **Radit**: 002 → 003 → tuning in 009/010
- **Calzy**: 004 → 007 → 005 → 012 → 011 flow
- **Eca**: 006 → 012 HUD/menus → 014
- **Fathia**: paper blueprints for 008/009/010 while 004 is built, then models for 015
- **Eileen / Salwa**: environment tileset and comic panels can start immediately — they only need the GDD, not code

## 3. Contracts between specs

Names used in several specs. The first spec to implement one owns its shape; later specs extend it rather than duplicating it.

| Shared thing | Defined in | Used by |
|---|---|---|
| Single configuration source (all GDD Ch. 17 keys) | 001 FR-015, `001/contracts/game-config.md` | every spec adds keys here, never a second config |
| Action button + nearest-object priority | `001/contracts/action-button-states.md` | 003 (Hide/Leave), 005 (Pick up key / Unlock / Final Door) |
| Interactable highlight | 001 FR-018, extended by 001-1 FR-013 (drawn around the object, two levels) | 003, 005, 006; final look 015 |
| Floor definition + anchors | 004 FR-002 | 002, 003, 005, 006, 008–010, 016 |
| Noise event / "player caught" outcome | 002 FR-010, FR-012 | 003, 005, 006, 007 |
| Floor reset operation | 007 FR-005, FR-007 | 002, 003, 005, 006 participate |
| "Run completed" outcome | 004 FR-008 / 005 FR-008 | 011 Good Ending |
| "Lives exhausted" outcome | 007 FR-004 | 011 Bad Ending |
| Pause freezes gameplay time | 012 FR-004 | 002, 003, 006, 007, 013, 014 |
| Sound events | 013 FR-001 | 014 reuses the same events |
| Debug-only tools (floor select, arena, overlays) | 002 FR-020, 004 FR-013 | stripped in 017 FR-008 |

## 4. GDD → spec traceability

| GDD section | Spec(s) |
|---|---|
| 1.1 Premis | 011 |
| 1.3 Design pillars | 002 FR-016, 006, 012 FR-003, 015 FR-013 |
| 1.4 Target durasi | 008–010 SCs, 017 SC-002 |
| 2.1–2.2 Loops | 008, 009, 010 |
| 2.3 Full run | 004, 007, 011 |
| 2.4 Floor numbering | 004 |
| 3 Floor table | 008, 009, 010 |
| 3.1 Battery spawn rules | 006 |
| 4.1 Movement | 001 |
| 4.2 Interaction | 001, 003, 005 |
| 4.3 Hiding | 003 |
| 5.1–5.2 Battery & light states | 001 (+ 003 drain while hiding) |
| 5.3 Battery sources | 006, 016 |
| 6 Monster AI | 002 (+ 008 distant SFX) |
| 7 Noise & detection | 002 |
| 8.1 Objectives | 005, 008, 009, 010 |
| 8.2 Progression feedback | 004, 005, 010, 015 |
| 9 Lives, checkpoint, fail | 007 |
| 10.1–10.2, 10.4 Eddie, prologue, endings | 011 |
| 10.3 Foreshadowing | 016 (+ 013 sounds) |
| 11 Visual pipeline | 001 (risk validation), 001-1 (amends 11.1: unified 3D world), 015 (final + fallback) |
| 12 Lighting | 001, 001-1 (amends 12: one real light + shadows), 015 |
| 13 Camera | 001, 001-1, 004 |
| 14 Audio | 013 |
| 15.1 Layout / HUD | 001, 012 |
| 15.2 Haptics | 014 |
| 15.3 Control parameters in config | 001, 012 |
| 15.4 Onboarding | 008, 012 (How To Play) |
| 16 Level design | 004 (framework + validation), 008–010 (content), 017 (final sign-off) |
| 17.1–17.2 Player, light & battery config | 001, 006 |
| 17.3–17.4 Noise, monster config | 002 |
| 17.5 Progression config | 004, 005, 007 |
| 18.1 Must have | see Section 1 column |
| 18.2 Cut list | guard-rail FRs in 002, 003, 004, 005, 011, 012, 017 |
| 18.3 Cut order | 003, 006, 010, 015 switches; 017 FR-006 |
| 19.1 Roles | owner column above |
| 19.2 Phases | phase column above |
| 19.3 Bug priority | 017 |
| 19.4 AI & CC0 policy | 013 FR-012, 015 FR-015, 017 FR-010 |
| 20 Fase 1 test plan | 001 |
| 21 Open items | joystick params → 001 SC-007 / T038; `noiseBaseRadius` → 002 FR-019; keep hiding? → 003 SC-005; hint count → 016 SC-004; Final Door line → 011 clarification; secret ending → out of scope (011 FR-008) |

## 5. How an agent picks up the next spec

Specs 002–017 were written **without** switching branches or touching `.specify/feature.json`,
so the agent working on 001 was not disturbed. To start one:

1. Create the branch from an up-to-date `main`: `feat/0NN-<slug>` (constitution: one branch per spec).
2. Point Spec Kit at it: set `.specify/feature.json` → `{"feature_directory": "specs/0NN-<slug>"}`.
3. If `checklists/requirements.md` has an unchecked "No [NEEDS CLARIFICATION]" item (004, 011, 012, 013): run `/speckit-clarify`.
4. `/speckit-plan` → `/speckit-tasks` → `/speckit-analyze` → `/speckit-implement`.
5. Do **not** run `/speckit-specify` for these — it would create a duplicate numbered folder.

For **001**: after the current implementation pass, run `/speckit-analyze` then `/speckit-converge`
so the GDD alignment delta (install gate ≤10%, highlight, placeholder monster, second battery,
control layout config, camera framing, furniture collision, new SCs) becomes tasks.
