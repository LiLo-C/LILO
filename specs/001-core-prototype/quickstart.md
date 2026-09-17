# Quickstart: Validating LILO Phase 1 — Core Movement & Light System Prototype

This is a manual validation guide — it proves the feature works end-to-end against the spec's
Success Criteria and Definition of Done. It is not a test suite; automated coverage for the pure
logic pieces is tracked separately (see [research.md §7](./research.md#7-test-assembly)).

## Prerequisites

- Unity 6000.6.1f1 (or the editor version pinned in `ProjectSettings/ProjectVersion.txt`) with
  the iOS build module installed.
- A physical iPhone 17 (the team's standardized baseline device for this iOS 26 cycle) connected
  and trusted, for the fps checks (SC-002) — the Unity Editor's Game view cannot substitute for
  this step.
- The project building to iOS with no other feature branches merged ahead of
  `001-core-prototype`.

## Setup

1. Open the project in Unity, open `Assets/Scenes/TestRoom.unity`, and build to the connected
   iPhone (File → Build Settings → iOS → Build and Run) for the performance-sensitive checks
   below — not Play mode in the Editor.
2. Build & run. The app should launch directly into the test room (no menu required this phase).

## Validation scenarios

Each scenario maps to one or more spec Acceptance Scenarios / Success Criteria — see
[spec.md](./spec.md) for the authoritative wording.

### 1. Movement & camera (User Story 1, SC-001, SC-002)

- Push the joystick partway in several directions → character walks, camera follows smoothly.
- Push the joystick to full deflection → character visibly moves faster (sprint), no separate
  button needed.
- Walk to each wall of the room → camera stops panning at the boundary; no empty space beyond
  the level is ever visible.
- **On the physical iPhone 17**: move continuously for at least 30 seconds while watching the
  Unity Profiler connected to the device build (Window → Analysis → Profiler, Autoconnect Profile)
  → sustained ≥60 fps. This is the go/no-go check for the phase's on-device performance target.

### 2. Flashlight drain & light states (User Story 2, SC-003, SC-004)

- From room start, do not pick up the battery. Time the flashlight with a stopwatch → reaches
  0% at 180 seconds (±1s tolerance for manual timing).
- Observe the light visibly change at the 30% and 10% charge marks (Flickering, then Critical).
- Let it reach 0% → Compact Darkness (small fixed radius), and confirm the player can still move
  and the app has not crashed (SC-004).
- Watch the HUD's battery bar the whole time → it visibly empties in sync with the light,
  reaching empty at exactly 0% charge (FR-017).
- Hand the device to 2 people outside the dev team; ask each, without prompting, "what state is
  the light in right now?" at a few random points during a drain run → both can answer correctly
  without being told what to look for (SC-003).

### 3. Battery pickup & swap (User Story 3)

- Walk to the loose battery, press the action button → it's picked up (disappears from world,
  action button no longer shows "Pick up" for that spot).
- With a spare battery held, press the action button again away from the battery's old spot →
  flashlight resets to 100%; the previous installed battery's remaining charge is gone.
- Restart the drain, let the flashlight run low, then try to walk back to where the battery
  used to be → confirm it does not reappear (FR-016 — no respawn).
- With both slots full (installed + spare), attempt to interact with the (already collected)
  spot — not applicable since it's gone; instead verify by code review or a debug spawn that a
  second pickup attempt while `spareBattery != null` is rejected with visible feedback, per
  [contracts/action-button-states.md](./contracts/action-button-states.md).
- While carrying a spare battery, check the HUD's spare-slot indicator shows "occupied"; after
  installing it, confirm it switches back to "empty" (FR-017).

### 4. Door (User Story 4)

- Walk to the door, press the action button → it opens with clear feedback.
- Stand somewhere with nothing in range → action button shows no label/prompt (FR-011).

### 5. Config & full loop (FR-015, SC-005, SC-006)

- Select `Assets/Config/GameConfig.asset` in the Unity Inspector, change `walkSpeed`, rebuild →
  movement speed changes with no other file touched (SC-005).
- Time a first-time player (or yourself, cold) going from spawn → battery pickup → swap → door
  open → under 2 minutes without being given instructions (SC-006).

## Expected end state

All items in spec.md's `checklists/requirements.md` remain checked, and every scenario above
passes on a physical iPhone 17 running iOS 26. Any scenario that fails should be filed as a gap
against the relevant FR/SC before `/speckit-tasks` work in that area is marked done.
