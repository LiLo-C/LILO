---

description: "Task list for LILO Phase 1 — Core Movement & Light System Prototype"
---

# Tasks: LILO Phase 1 — Core Movement & Light System Prototype

**Input**: Design documents from `/specs/001-core-prototype/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Included — constitution Principle IV (Test-Before-Done) requires unit coverage for the pure-logic systems before a story is considered done.

**Organization**: Tasks are grouped by user story from spec.md, in priority order (P1, P1, P2, P3), so each story is independently implementable and testable per its own Independent Test criterion.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: Which user story this task belongs to (US1–US4)
- File paths are exact and repo-relative

## Path Conventions

Single Xcode app target per [plan.md](./plan.md#project-structure): source under `v2/Game/`, tests under `v2Tests/` (new target).

---

## Phase 1: Setup

- [X] T001 Set `IPHONEOS_DEPLOYMENT_TARGET = 26.0` uniformly across all build configurations in `v2.xcodeproj/project.pbxproj` (project + target level, Debug and Release) so the actual project matches the iOS 26 decision in spec.md Clarifications
- [X] T002 Create the `Game/` group and its `Scenes/`, `Entities/`, `Systems/`, `UI/` subfolders under `v2/` matching the layout in [plan.md](./plan.md#project-structure)
- [X] T003 Add a new `v2Tests` XCTest target to `v2.xcodeproj`, linked against the `v2` app target (none exists yet — confirmed in research.md §7)
- [X] T004 [P] Create `v2/Game/GameConfig.swift` with every key from [contracts/game-config.md](./contracts/game-config.md) (including `TBD`-value keys, per that contract's "non-negotiable shape rules" — the key must exist even before its final number is tuned)

**Checkpoint**: Project builds on the correct iOS 26 deployment target, with the new group structure, empty test target, and a populated `GameConfig`.

---

## Phase 2: Foundational (blocking prerequisites)

**⚠️ MUST complete before any user story below — every story reads/writes through `GameState`.**

- [X] T005 Create `v2/Game/GameState.swift`: the `@Observable` root aggregate plus `PlayerCharacter`, `Battery`, `Door`, `TestRoom` types exactly as specified in [data-model.md](./data-model.md), including the `Battery.location` enum (`.world | .spare | .installed`) and its one-way transition rule (no path back to `.world`)
- [X] T006 Create `v2/Game/Scenes/TestRoomScene.swift`: an `SKScene` skeleton with a fixed room boundary matching `TestRoom.bounds`, an `SKCameraNode`, and placeholder floor/wall geometry (primitive shapes only, per spec Assumptions — no final art)
- [X] T007 Wire `v2/ContentView.swift` to own a single `GameState` instance and host `SpriteView(scene:)` (the `TestRoomScene`) with a `GameHUDView` overlay on top, per [research.md §2](./research.md#2-shared-game-state)
- [X] T008 [P] Create `v2Tests/GameConfigTests.swift` asserting every key from [contracts/game-config.md](./contracts/game-config.md) is present on `GameConfig` with the documented type

**Checkpoint**: App launches into an empty test room with a working camera node and shared state — nothing moves yet. All user stories below build on this.

---

## Phase 3: User Story 1 - Move and Explore the Test Room (Priority: P1)

**Goal**: Player can walk/sprint via joystick, camera smooth-follows and clamps to the room, and the 3D placeholder character renders correctly over the 2D room — the phase's core technical-risk validation.

**Independent Test**: Run on a physical iPhone 17; walk and sprint around the empty test room per [quickstart.md §1](./quickstart.md#1-movement--camera-user-story-1-sc-001-sc-002); confirm ≥60 fps sustained.

- [X] T009 [US1] Implement `v2/Game/Entities/PlayerNode.swift`: an `SK3DNode` wrapping a placeholder capsule `SCNGeometry`, positioned from `GameState.player.position` each frame
- [X] T010 [US1] Implement `v2/Game/Systems/MovementController.swift`: converts joystick input into `GameState.player.movementState` (`idle | walking | sprinting`) and position updates — walking at `GameConfig.walkSpeed`, sprinting at `GameConfig.walkSpeed * GameConfig.sprintMultiplier` when joystick deflection ≥ `GameConfig.sprintJoystickThreshold`, with no separate sprint control (FR-001, FR-002)
- [X] T011 [US1] In `MovementController.swift`, clamp player position against `TestRoom.bounds` so the character cannot pass through walls (spec Edge Cases: boundary collision handled separately from camera clamping)
- [X] T012 [P] [US1] Implement `v2/Game/UI/JoystickView.swift`: a `DragGesture`-based 360° virtual joystick positioned in the left portion of the screen, reporting a normalized direction + deflection to `MovementController`
- [X] T013 [US1] Implement `v2/Game/Systems/CameraController.swift`: lerp the `SKCameraNode` toward `GameState.player.position` each frame using `GameConfig.cameraFollowLerpFactor`, then clamp to `TestRoom.bounds` inset by `GameConfig.roomBoundsInset` so no space beyond the level is ever visible (FR-012)
- [X] T014 [US1] In `CameraController.swift`, derive the 3D `SCNCamera`'s transform from the same 2D `SKCameraNode` each frame (not a second independently-tuned camera) per [research.md §5](./research.md#5-camera-smooth-follow-and-boundary-clamping), so `PlayerNode` never drifts, scales, or floats relative to the background (FR-013)
- [X] T015 [US1] In `TestRoomScene.swift`, add one placeholder 2D sprite (e.g., a desk) with `zPosition` set so it visually occludes `PlayerNode` when positioned in front of the character from the camera's viewpoint (FR-014)
- [X] T016 [P] [US1] Wire `JoystickView` into `v2/Game/UI/GameHUDView.swift` (left side of the overlay)

**Checkpoint**: User Story 1 is independently complete — movement, sprint, camera, and the hybrid-rendering technical risk are all validated per quickstart.md §1, including the on-device fps check.

---

## Phase 4: User Story 2 - Experience the Flashlight Running Down (Priority: P1)

**Goal**: The installed battery drains in real time, the player can visually distinguish all four light states, and a HUD bar mirrors the charge — provable in isolation without touching pickup.

**Independent Test**: From room start, let the flashlight drain untouched; confirm the four states and the HUD bar both track charge correctly and Compact Darkness never ends the session, per [quickstart.md §2](./quickstart.md#2-flashlight-drain--light-states-user-story-2-sc-003-sc-004).

- [X] T017 [US2] Implement `v2/Game/Systems/BatteryController.swift`: drain `GameState.installedBattery.charge` using wall-clock delta time (`CACurrentMediaTime()`), reaching 0 at exactly `GameConfig.batteryDuration` (180s) regardless of player action or frame rate (FR-004), per [research.md §3](./research.md#3-real-time-battery-drain-timing)
- [X] T018 [US2] In `BatteryController.swift`, derive the current `LightState` (`.normal | .flickering | .critical | .compactDarkness`) purely from `installedBattery.charge` against the exact non-overlapping boundaries in spec.md FR-005 and [data-model.md](./data-model.md#lightstate-derived-not-stored) (e.g., exactly 30% charge is `.flickering`, not `.normal`) — never stored independently
- [X] T019 [US2] Implement `v2/Game/Systems/LightingController.swift`: a 2D vignette via `SKCropNode` + hole texture, and a 3D `SCNLight` (spot type), both driven by the same `LightState` from `BatteryController` so their radius always matches (FR-005, FR-006) — at `.compactDarkness`, radius fixes at `GameConfig.compactDarknessRadiusFraction` of normal and the player MUST remain able to move (FR-006)
- [X] T020 [US2] Implement `v2/Game/UI/BatteryIndicatorView.swift`: a horizontal bar showing `batteryChargeFraction` (`installedBattery.charge / GameConfig.batteryDuration`) and a separate empty/occupied indicator for the spare slot, per [data-model.md](./data-model.md#derived-hud-values-not-stored) (FR-017)
- [X] T021 [P] [US2] Wire `BatteryIndicatorView` into `GameHUDView.swift` (per GDD Ch. 15.1 HUD placement)
- [X] T022 [P] [US2] Create `v2Tests/BatteryControllerTests.swift`: assert a battery at `charge = 180` reaches `0` after simulating 180s of wall-clock time, and that drain rate is identical whether `movementState` is `.idle` or `.sprinting` (FR-003, FR-004)
- [X] T023 [P] [US2] Create `v2Tests/LightStateTests.swift`: assert the exact charge→state boundaries from spec.md FR-005 — `30% < charge ≤ 100%` is `.normal`, `10% < charge ≤ 30%` is `.flickering`, `0% < charge ≤ 10%` is `.critical`, `charge = 0%` is `.compactDarkness`, including the exact boundary values (30%, 10%, 0%) themselves

**Checkpoint**: User Story 2 is independently complete and testable without any pickup mechanics existing yet.

---

## Phase 5: User Story 3 - Recover a Spare Battery Before Running Out (Priority: P2)

**Goal**: Player can pick up the room's one battery, carry it as a spare, and install it to refill the flashlight, with slot-capacity enforced and reflected in the HUD.

**Independent Test**: Spawn near the battery, pick it up, and install it per [quickstart.md §3](./quickstart.md#3-battery-pickup--swap-user-story-3); confirm it never respawns and a second pickup attempt while the spare slot is full is rejected.

- [X] T024 [US3] Implement `v2/Game/Entities/BatteryNode.swift`: the loose battery scene node placed at `TestRoom.batterySpawn`, removed from the scene when `GameState.worldBattery` transitions out of `.world` — never re-added (FR-016)
- [X] T025 [US3] Implement `v2/Game/Systems/InteractionController.swift`: detects when the player is within `GameConfig.interactionRadius` of the battery and exposes the current interactable to the HUD (battery-only for now; extended for the door in US4)
- [X] T026 [US3] In `BatteryController.swift`, implement pickup: `worldBattery → .spare` succeeds only when `GameState.spareBattery == nil`; otherwise reject, leave `worldBattery` unchanged, and surface a rejection flag for the UI to read (FR-007, FR-008 — quote: "pickup attempted while both are full MUST be rejected... MUST leave the item in the world, and MUST show the player feedback")
- [X] T027 [US3] In `BatteryController.swift`, implement install: `spareBattery → .installed` always sets the new `installedBattery.charge = GameConfig.batteryDuration` (full refill) and discards whatever was previously installed, per [data-model.md](./data-model.md#state-transitions) (FR-009)
- [X] T028 [US3] Implement `v2/Game/UI/ActionButtonView.swift`: single context-sensitive button reading `InteractionController`'s current interactable, showing "Pick up" / "Install battery" per [contracts/action-button-states.md](./contracts/action-button-states.md), and no label when nothing is in range (FR-011)
- [X] T029 [US3] In `ActionButtonView.swift`, show clear visual feedback (e.g., brief shake + disabled state) when a pickup is rejected per T026's rejection flag (spec Edge Cases)
- [X] T030 [US3] In `BatteryIndicatorView.swift`, drive the spare-slot indicator from `spareSlotOccupied` so a rejected pickup has a persistent, at-a-glance explanation ("slot already full") beyond the transient action-button feedback in T029 (FR-008, FR-017)
- [X] T031 [P] [US3] Wire `ActionButtonView` into `GameHUDView.swift` (right side of the overlay)
- [X] T032 [P] [US3] Extend `v2Tests/BatteryControllerTests.swift`: assert pickup is rejected (item stays in `.world`) when `spareBattery != nil`, and assert install always sets charge to exactly `GameConfig.batteryDuration` and discards the prior installed battery's remaining charge (FR-008, FR-009)

**Checkpoint**: User Story 3 is independently complete — the full pickup → carry → install loop works, is reflected in the HUD, and is covered by tests.

---

## Phase 6: User Story 4 - Reach the Exit (Priority: P3)

**Goal**: Player can open the room's one door via the same action button, closing the smallest full loop through the prototype.

**Independent Test**: Walk to the door and press the action button per [quickstart.md §4](./quickstart.md#4-door-user-story-4); confirm it opens with feedback and the button shows nothing when nothing is in range.

- [X] T033 [US4] Implement `v2/Game/Entities/DoorNode.swift`: door scene node at `TestRoom.doorPosition`, toggling a visual/animation state when `GameState.door.isOpen` becomes `true` (one-way, no close action — per [data-model.md](./data-model.md#door)) (FR-010)
- [X] T034 [US4] Extend `InteractionController.swift` to also detect the door within `GameConfig.interactionRadius`, and apply the nearest-object-wins tie-break from [contracts/action-button-states.md](./contracts/action-button-states.md#priority-rule-when-multiple-interactables-are-in-range) when both a battery-related action and the door are simultaneously in range
- [X] T035 [US4] In `ActionButtonView.swift`, add the "Open door" label/action per [contracts/action-button-states.md](./contracts/action-button-states.md), triggering `GameState.door.isOpen = true` with clear success feedback

**Checkpoint**: All 4 user stories are independently functional — the full room can be played start to finish.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T036 [P] Run every scenario in [quickstart.md](./quickstart.md) end-to-end on a physical iPhone 17 running iOS 26; record the sustained fps result for SC-002
- [ ] T037 [P] Recruit and run the light-state blind-observation test from [quickstart.md §2](./quickstart.md#2-flashlight-drain--light-states-user-story-2-sc-003-sc-004) with at least 2 participants outside the development team; record each person's answers against SC-003
- [ ] T038 [P] Tune the `TBD` values in `GameConfig.swift` (`sprintJoystickThreshold`, `interactionRadius`, `cameraFollowLerpFactor`, `roomBoundsInset`) by feel on-device, per [contracts/game-config.md](./contracts/game-config.md) and spec Assumptions — no code changes outside `GameConfig.swift` required
- [X] T039 Review all new files under `v2/Game/` against constitution Principle II (self-explanatory naming, comments only for non-obvious "why") and Principle III (no state outside `GameState`); additionally grep `v2/Game/` for any numeric literal duplicating a `GameConfig` value (FR-015's "no such value hardcoded elsewhere") and fix any found
- [ ] T040 Time a cold run of the full loop (spawn → drain begins → pickup → install → door) against SC-006's under-2-minutes target; adjust room layout/spawn distances if it doesn't hold

---

## Dependencies & Execution Order

- **Phase 1 (Setup)** → **Phase 2 (Foundational)**: strictly sequential; Foundational blocks every user story. T001 (deployment target) should land before T002–T004 since it's a project-file change everything else builds against.
- **User Story 1 (P1)**: depends only on Foundational. No dependency on US2–US4.
- **User Story 2 (P1)**: depends only on Foundational. Independently testable without US1's movement being polished (the room can sit static while the light drains) — but both are P1 because the spec's own priority reflects that a demoable "feel" of the game needs both.
- **User Story 3 (P2)**: depends on Foundational and on US2 existing (there must be an installed battery/flashlight to refill, and a `BatteryIndicatorView` to extend) — safe to start once T017–T021 (US2) land, does not need US1 finished.
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

Within User Story 1, after T009–T011 land: `T012` (JoystickView) and `T013`/`T014` (CameraController) touch different files and can run in parallel; `T016` waits on `T012`.

Within User Story 2, after T017–T021 land: `T022` and `T023` (both test files) can run in parallel with each other.

Within User Story 3: `T031` (HUD wiring) can run in parallel with `T032` (tests) once `T028` lands.

## Implementation Strategy

**Suggested MVP**: User Story 1 + User Story 2 (both P1) — together they prove the phase's actual purpose: the hybrid-rendering technical risk (US1) and the core "Lights In, Lights Out" mechanic, including its HUD bar (US2). Either is independently demoable alone if time is tight, per their Independent Test criteria, but the spec's own priority marks both P1 because neither alone represents the game's core loop.

**Incremental delivery**: Setup → Foundational → US1 → US2 → checkpoint (MVP demoable) → US3 → US4 → Polish. Each checkpoint above is a working, playable increment — stop anywhere after US2 and still have something to show.

## Format Validation

All 40 tasks follow `- [ ] T### [P?] [Story?] Description with exact file path`. Setup (T001–T004), Foundational (T005–T008), and Polish (T036–T040) carry no `[Story]` label; every task in Phases 3–6 carries its `[US#]` label.
