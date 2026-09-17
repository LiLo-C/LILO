---

description: "Task list for LILO Phase 1 — Core Movement & Light System Prototype"

---

# Tasks: LILO Phase 1 — Core Movement & Light System Prototype

**Input**: Design documents from `/specs/001-core-prototype/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires unit coverage for the pure-logic systems before a story is considered done.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P1, P2, P3), so each story is independently implementable and testable per its own Independent Test criterion.

**Engine migration note (2026-09-17)**: every task below was previously checked off against a
native iOS/SpriteKit+SceneKit/SwiftUI implementation that lived outside this repository. That
implementation is not part of this Unity project's history, so every task is reset to unchecked
here — there is currently no Unity code for this feature in `Assets/`. The task list itself is
rewritten for Unity paths and APIs; the underlying gameplay logic and its tuned values (see
`contracts/game-config.md`'s *(carried over)* entries) are already validated, which should make
re-implementation faster than the first pass was.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US4)
- File paths are exact and repo-relative

## Path Conventions

Single Unity project per [plan.md](./plan.md#project-structure): source under
`Assets/Scripts/`, tests under `Assets/Tests/EditMode/` (new assembly).

---

## Phase 1: Setup

- [ ] T001 Set the iOS build target's minimum deployment to iOS 26 in Player Settings (`ProjectSettings/ProjectSettings.asset`, iOS section) so the actual project matches the iOS 26 decision in spec.md Clarifications
- [ ] T002 Create the `Assets/Scripts/{Config,State,Systems,MonoBehaviours,UI}` folder structure matching the layout in [plan.md](./plan.md#project-structure)
- [ ] T003 Create the `Assets/Tests/EditMode` folder with a new EditMode assembly definition (`.asmdef`) referencing Unity Test Framework and the `Assets/Scripts` assembly (none exists yet — confirmed in research.md §7)
- [ ] T004 [P] Create `Assets/Scripts/Config/GameConfig.cs` (a `ScriptableObject` class) with every key from [contracts/game-config.md](./contracts/game-config.md) (including `TBD`-value fields, per that contract's "non-negotiable shape rules" — the field must exist even before its final number is tuned), then create the asset instance `Assets/Config/GameConfig.asset`

**Checkpoint**: Project builds on the correct iOS 26 deployment target, with the new folder structure, an empty EditMode test assembly, and a populated `GameConfig` asset.

---

## Phase 2: Foundational (blocking prerequisites)

**⚠️ MUST complete before any user story below — every story reads/writes through `GameState`.**

- [ ] T005 Create `Assets/Scripts/State/GameState.cs`: the plain C# root aggregate plus `PlayerCharacter`, `Battery`, `Door`, `TestRoom` types exactly as specified in [data-model.md](./data-model.md), including the `Battery.location` enum (`World | Spare | Installed`) and its one-way transition rule (no path back to `World`)
- [ ] T006 Create `Assets/Scenes/TestRoom.unity`: a new URP 3D scene with a fixed room boundary matching `TestRoom.bounds`, a `Camera`, and placeholder floor/wall geometry (primitive cubes only, per spec Assumptions — no final art)
- [ ] T007 Create `Assets/Scripts/MonoBehaviours/GameManager.cs`: instantiates one `GameState`, ticks every System each `Update()`, and exposes the shared instance to other `MonoBehaviour`s in the scene; add `Assets/Scripts/UI/GameHUD.cs` with an overlay `Canvas` reading the same `GameState`, per [research.md §2](./research.md#2-shared-game-state)
- [ ] T008 [P] Create `Assets/Tests/EditMode/GameConfigTests.cs` asserting every key from [contracts/game-config.md](./contracts/game-config.md) is present on `GameConfig` with the documented type

**Checkpoint**: App launches into an empty test room with a working camera and shared state — nothing moves yet. All user stories below build on this.

---

## Phase 3: User Story 1 - Move and Explore the Test Room (Priority: P1)

**Goal**: Player can walk/sprint via joystick, camera smooth-follows and clamps to the room, and the placeholder character renders correctly in the URP 3D scene.

**Independent Test**: Run on a physical iPhone 17; walk and sprint around the empty test room per [quickstart.md §1](./quickstart.md#1-movement--camera-user-story-1-sc-001-sc-002); confirm ≥60 fps sustained.

- [ ] T009 [US1] Implement `Assets/Scripts/MonoBehaviours/PlayerRig.cs`: a `GameObject` with a placeholder capsule mesh, positioned from `GameState.player.position` each frame (mapped `(x, y) → (x, 0, y)`)
- [ ] T010 [US1] Implement `Assets/Scripts/Systems/MovementController.cs`: converts joystick input into `GameState.player.movementState` (`Idle | Walking | Sprinting`) and position updates — walking at `GameConfig.walkSpeed`, sprinting at `GameConfig.walkSpeed * GameConfig.sprintMultiplier` when joystick deflection ≥ `GameConfig.sprintJoystickThreshold`, with no separate sprint control (FR-001, FR-002)
- [ ] T011 [US1] In `MovementController.cs`, clamp player position against `TestRoom.bounds` so the character cannot pass through walls (spec Edge Cases: boundary collision handled separately from camera clamping)
- [ ] T012 [P] [US1] Implement `Assets/Scripts/UI/JoystickUI.cs`: an Input System–driven 360° virtual joystick positioned in the left portion of the screen, reporting a normalized direction + deflection to `MovementController`
- [ ] T013 [US1] Implement `Assets/Scripts/Systems/CameraController.cs`: lerp a follow position toward `GameState.player.position` each frame using `GameConfig.cameraFollowLerpFactor`, then clamp to `TestRoom.bounds` inset by `GameConfig.roomBoundsInset` so no space beyond the level is ever visible (FR-012)
- [ ] T014 [US1] Implement `Assets/Scripts/MonoBehaviours/CameraRig.cs`: applies `CameraController`'s output position to the scene's one `Camera` each frame, with `orthographic = true`, tilt `GameConfig.cameraTiltDegrees`, and orthographic size `GameConfig.cameraOrthographicScale` (FR-012, FR-013) — with one scene and one camera there is no second renderer to keep in sync (research.md §5)
- [ ] T015 [US1] Add the placeholder desk as a solid cube `GameObject` in `TestRoom.unity`; confirm Unity's real depth buffer occludes `PlayerRig` correctly with no manual layer/order configuration (FR-014)
- [ ] T016 [P] [US1] Wire `JoystickUI` into `Assets/Scripts/UI/GameHUD.cs` (left side of the overlay `Canvas`)

**Checkpoint**: User Story 1 is independently complete — movement, sprint, camera, and on-device performance are all validated per quickstart.md §1, including the fps check.

---

## Phase 4: User Story 2 - Experience the Flashlight Running Down (Priority: P1)

**Goal**: The installed battery drains in real time, the player can visually distinguish all four light states, and a HUD bar mirrors the charge — provable in isolation without touching pickup.

**Independent Test**: From room start, let the flashlight drain untouched; confirm the four states and the HUD bar both track charge correctly and Compact Darkness never ends the session, per [quickstart.md §2](./quickstart.md#2-flashlight-drain--light-states-user-story-2-sc-003-sc-004).

- [ ] T017 [US2] Implement `Assets/Scripts/Systems/BatteryController.cs`: drain `GameState.installedBattery.charge` using `Time.deltaTime`, reaching 0 at exactly `GameConfig.batteryDuration` (180s) regardless of player action or frame rate (FR-004), per [research.md §3](./research.md#3-real-time-battery-drain-timing)
- [ ] T018 [US2] In `BatteryController.cs`, derive the current `LightState` (`Normal | Flickering | Critical | CompactDarkness`) purely from `installedBattery.charge` against the exact non-overlapping boundaries in spec.md FR-005 and [data-model.md](./data-model.md#lightstate-derived-not-stored) (e.g., exactly 30% charge is `Flickering`, not `Normal`) — never stored independently
- [ ] T019 [US2] Implement `Assets/Scripts/MonoBehaviours/LightingRig.cs`: attach one URP `Light` (Point) to the player, driven by the current `LightState` from `BatteryController` so its range always matches (FR-005, FR-006) — at `CompactDarkness`, range fixes at `GameConfig.compactDarknessRadiusFraction` of normal and the player MUST remain able to move (FR-006)
- [ ] T020 [US2] Implement `Assets/Scripts/UI/BatteryIndicatorUI.cs`: a horizontal bar showing `batteryChargeFraction` (`installedBattery.charge / GameConfig.batteryDuration`) and a separate empty/occupied indicator for the spare slot, per [data-model.md](./data-model.md#derived-hud-values-not-stored) (FR-017)
- [ ] T021 [P] [US2] Wire `BatteryIndicatorUI` into `GameHUD.cs` (per GDD Ch. 15.1 HUD placement)
- [ ] T022 [P] [US2] Create `Assets/Tests/EditMode/BatteryControllerTests.cs`: assert a battery at `charge = 180` reaches `0` after simulating 180s of elapsed time, and that drain rate is identical whether `movementState` is `Idle` or `Sprinting` (FR-003, FR-004)
- [ ] T023 [P] [US2] Create `Assets/Tests/EditMode/LightStateTests.cs`: assert the exact charge→state boundaries from spec.md FR-005 — `30% < charge ≤ 100%` is `Normal`, `10% < charge ≤ 30%` is `Flickering`, `0% < charge ≤ 10%` is `Critical`, `charge = 0%` is `CompactDarkness`, including the exact boundary values (30%, 10%, 0%) themselves

**Checkpoint**: User Story 2 is independently complete and testable without any pickup mechanics existing yet.

---

## Phase 5: User Story 3 - Recover a Spare Battery Before Running Out (Priority: P2)

**Goal**: Player can pick up the room's batteries, carry one as a spare, and install it to refill the flashlight, with slot-capacity enforced and reflected in the HUD.

**Independent Test**: Spawn near the batteries, pick one up, and install it per [quickstart.md §3](./quickstart.md#3-battery-pickup--swap-user-story-3); confirm neither respawns and a second pickup attempt while the spare slot is full is rejected.

- [ ] T024 [US3] Implement `Assets/Scripts/MonoBehaviours/BatteryPickup.cs`: the loose battery `GameObject` component placed at each `TestRoom.batterySpawns` entry, disabled/removed from the scene when its `GameState.worldBatteries` entry transitions out of `World` — never re-added (FR-016)
- [ ] T025 [US3] Implement `Assets/Scripts/Systems/InteractionController.cs`: detects when the player is within `GameConfig.interactionRadius` of a battery and exposes the current interactable to the HUD (battery-only for now; extended for the door in US4)
- [ ] T026 [US3] In `BatteryController.cs`, implement pickup: `World → Spare` succeeds only when `GameState.spareBattery == null`; otherwise reject, leave the battery unchanged, and surface a rejection flag for the UI to read (FR-007, FR-008 — quote: "pickup attempted while both are full MUST be rejected... MUST leave the item in the world, and MUST show the player feedback")
- [ ] T027 [US3] In `BatteryController.cs`, implement install: `Spare → Installed` always sets the new `installedBattery.charge = GameConfig.batteryDuration` (full refill) and discards whatever was previously installed, per [data-model.md](./data-model.md#state-transitions) (FR-009)
- [ ] T028 [US3] Implement `Assets/Scripts/UI/ActionButtonUI.cs`: single context-sensitive button reading `InteractionController`'s current interactable, showing "Pick up" / "Install battery" per [contracts/action-button-states.md](./contracts/action-button-states.md), and no label when nothing is in range (FR-011)
- [ ] T029 [US3] In `ActionButtonUI.cs`, show clear visual feedback (e.g., brief shake + disabled state) when a pickup is rejected per T026's rejection flag (spec Edge Cases)
- [ ] T030 [US3] In `BatteryIndicatorUI.cs`, drive the spare-slot indicator from `spareSlotOccupied` so a rejected pickup has a persistent, at-a-glance explanation ("slot already full") beyond the transient action-button feedback in T029 (FR-008, FR-017)
- [ ] T031 [P] [US3] Wire `ActionButtonUI` into `GameHUD.cs` (right side of the overlay)
- [ ] T032 [P] [US3] Extend `Assets/Tests/EditMode/BatteryControllerTests.cs`: assert pickup is rejected (item stays `World`) when `spareBattery != null`, and assert install always sets charge to exactly `GameConfig.batteryDuration` and discards the prior installed battery's remaining charge (FR-008, FR-009)

**Checkpoint**: User Story 3 is independently complete — the full pickup → carry → install loop works, is reflected in the HUD, and is covered by tests.

---

## Phase 6: User Story 4 - Reach the Exit (Priority: P3)

**Goal**: Player can open the room's one door via the same action button, closing the smallest full loop through the prototype.

**Independent Test**: Walk to the door and press the action button per [quickstart.md §4](./quickstart.md#4-door-user-story-4); confirm it opens with feedback and the button shows nothing when nothing is in range.

- [ ] T033 [US4] Implement `Assets/Scripts/MonoBehaviours/DoorController.cs`: door `GameObject` at `TestRoom.doorPosition`, toggling a visual/animation state when `GameState.door.isOpen` becomes `true` (one-way, no close action — per [data-model.md](./data-model.md#door)) (FR-010)
- [ ] T034 [US4] Extend `InteractionController.cs` to also detect the door within `GameConfig.interactionRadius`, and apply the nearest-object-wins tie-break from [contracts/action-button-states.md](./contracts/action-button-states.md#priority-rule-when-multiple-interactions-are-available) when both a battery-related action and the door are simultaneously in range
- [ ] T035 [US4] In `ActionButtonUI.cs`, add the "Open door" label/action per [contracts/action-button-states.md](./contracts/action-button-states.md), triggering `GameState.door.isOpen = true` with clear success feedback

**Checkpoint**: All 4 user stories are independently functional — the full room can be played start to finish.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T036 [P] Run every scenario in [quickstart.md](./quickstart.md) end-to-end on a physical iPhone 17 running iOS 26; record the sustained fps result for SC-002
- [ ] T037 [P] Recruit and run the light-state blind-observation test from [quickstart.md §2](./quickstart.md#2-flashlight-drain--light-states-user-story-2-sc-003-sc-004) with at least 2 participants outside the development team; record each person's answers against SC-003
- [ ] T038 [P] Tune the `TBD` values in `GameConfig.asset` (`joystickDiameter`, `joystickDeadZone`, `joystickOpacity`, `joystickCenterOffset`, `actionButtonSize`, `actionButtonCenterOffset`, `actionButtonTouchRadius`) by feel on-device, and re-confirm the *(carried over)* values from the previous engine's tuning still feel right in Unity, per [contracts/game-config.md](./contracts/game-config.md) and spec Assumptions — no code changes outside the `GameConfig` asset required
- [ ] T039 Review all new files under `Assets/Scripts/` against constitution Principle II (self-explanatory naming, comments only for non-obvious "why") and Principle III (no state outside `GameState`, no `MonoBehaviour` dependency in `Systems/`); additionally grep `Assets/Scripts/` for any numeric literal duplicating a `GameConfig` value (FR-015's "no such value hardcoded elsewhere") and fix any found
- [ ] T040 Time a cold run of the full loop (spawn → drain begins → pickup → install → door) against SC-006's under-4-minutes target; adjust room layout/spawn distances if it doesn't hold

---

## Dependencies & Execution Order

- **Phase 1 (Setup)** → **Phase 2 (Foundational)**: strictly sequential; Foundational blocks every user story. T001 (deployment target) should land before T002–T004 since it's a project-setting change everything else builds against.
- **User Story 1 (P1)**: depends only on Foundational. No dependency on US2–US4.
- **User Story 2 (P1)**: depends only on Foundational. Independently testable without US1's movement being polished (the room can sit static while the light drains) — but both are P1 because the spec's own priority reflects that a demoable "feel" of the game needs both.
- **User Story 3 (P2)**: depends on Foundational and on US2 existing (there must be an installed battery/flashlight to refill, and a `BatteryIndicatorUI` to extend) — safe to start once T017–T021 (US2) land, does not need US1 finished.
- **User Story 4 (P3)**: depends on Foundational and reuses `InteractionController` introduced in US3 (T025) — start after T025 exists.
- **Polish (Phase 7)**: after all four stories.

```text
Setup (T001-T004)
   ↓
Foundational (T005-T008)
   ↓
   ├──> US1 (T009-T016) ───────────┐
   ├──> US2 (T017-T023) ──┐        │
   │                      ↓        │
   │                   US3 (T024-T032) ──> US4 (T033-T035)
   └──────────────────────┴────────┴──> Polish (T036-T040)
```

## Parallel Execution Examples

Within Phase 2 (Foundational), after T005–T007 land sequentially: `T008` can run in parallel with the start of any user story's non-conflicting files.

Within User Story 1, after T009–T011 land: `T012` (JoystickUI) and `T013`/`T014` (CameraController/CameraRig) touch different files and can run in parallel; `T016` waits on `T012`.

Within User Story 2, after T017–T021 land: `T022` and `T023` (both test files) can run in parallel with each other.

Within User Story 3: `T031` (HUD wiring) can run in parallel with `T032` (tests) once `T028` lands.

## Implementation Strategy

**Suggested MVP**: User Story 1 + User Story 2 (both P1) — together they prove the phase's actual purpose: on-device movement/camera feel (US1) and the core "Lights In, Lights Out" mechanic, including its HUD bar (US2). Either is independently demoable alone if time is tight, per their Independent Test criteria, but the spec's own priority marks both P1 because neither alone represents the game's core loop.

**Incremental delivery**: Setup → Foundational → US1 → US2 → checkpoint (MVP demoable) → US3 → US4 → Polish. Each checkpoint above is a working, playable increment — stop anywhere after US2 and still have something to show.

## Format Validation

All 40 tasks follow `- [ ] T### [P?] [Story?] Description with exact file path`. Setup (T001–T004), Foundational (T005–T008), and Polish (T036–T040) carry no `[Story]` label; every task in Phases 3–6 carries its `[US#]` label.
