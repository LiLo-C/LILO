# Feature Specification: Bootstrap Scene

**Feature Branch**: `001-bootstrap-scene`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "The very first Unity Scene loaded on app launch. Its one job: instantiate the persistent GameManager (which owns GameState and survives every later scene load via DontDestroyOnLoad) and the GameConfig asset reference, initialize any other persistent systems (audio mixer root, e.g.), then immediately load MainMenu. This spec owns only what exists in the Bootstrap scene and the order it initializes in — it does not redefine GameConfig's schema, GameState's shape, or the scene-transition mechanism itself."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Cold launch reaches the Main Menu unattended (Priority: P1)

A player opens the app for the first time in a session. Without seeing any loading screen, debug
output, or intermediate screen they can act on, the app initializes its persistent systems and
lands them on the Main Menu, ready to tap Start.

**Why this priority**: Every other scene and system in the game depends on `GameManager` and
`GameConfig` already existing before it runs. If this doesn't work, nothing else in the game is
reachable at all — this is the single most upstream story in the entire project.

**Independent Test**: Launch the app from a cold process start (not resumed from background) and
observe that the Main Menu's Start button is interactive within a normal launch time, with no
crash, no visible second scene, and no manual step required.

**Acceptance Scenarios**:

1. **Given** the app process has just started, **When** Bootstrap is the first scene to run,
   **Then** exactly one persistent `GameManager` is created, holding a reference to the single
   `GameConfig` asset.
2. **Given** `GameManager` and the persistent audio mixer root have finished initializing,
   **When** Bootstrap has nothing further to do, **Then** the app transitions to MainMenu without
   any player input.
3. **Given** the transition to MainMenu has completed, **When** the player looks at the screen,
   **Then** Bootstrap's own scene is no longer visible or active.

---

### User Story 2 - Persistent systems are live before gameplay ever needs them (Priority: P2)

Systems that must outlive every scene transition for the rest of the app's life — the shared game
state and the persistent audio mixer root — are fully initialized during Bootstrap, so that no
later scene (MainMenu, Prologue, any floor, either ending) ever has to check whether they exist
yet.

**Why this priority**: Getting to MainMenu (User Story 1) is necessary but not sufficient — if
`GameManager` or the audio root only *partially* initialize before the transition fires, later
scenes inherit an inconsistent world. This story is what makes User Story 1 safe to build on.

**Independent Test**: Instrument (temporarily, for QA) a log line at the start of MainMenu's own
initialization confirming `GameConfig` is non-null and the persistent audio root already exists;
confirm this holds on every cold launch.

**Acceptance Scenarios**:

1. **Given** Bootstrap has started running, **When** persistent systems initialize, **Then** the
   `GameConfig` asset reference is assigned to `GameManager` before anything else in Bootstrap
   proceeds.
2. **Given** the `GameConfig` reference is assigned, **When** Bootstrap continues, **Then** the
   persistent audio mixer root is created and marked to survive scene loads before the transition
   to MainMenu is triggered.
3. **Given** any persistent system fails to initialize (see Edge Cases), **When** Bootstrap
   detects this, **Then** it does not proceed to MainMenu with an incompletely-initialized world.

---

### User Story 3 - Re-entering Bootstrap never duplicates persistent systems (Priority: P3)

If Bootstrap's scene is ever loaded a second time during the same app process (for example, a
developer navigating back to it during testing, or a future flow this spec doesn't otherwise
anticipate), the existing persistent `GameManager` and audio root are reused rather than
duplicated.

**Why this priority**: This is a correctness safety net, not a flow the shipped game is expected
to exercise under normal play (Main Menu never routes back to Bootstrap) — but constitution
Principle III makes a second `GameManager` an explicit violation, so Bootstrap must not be the
place that creates one.

**Independent Test**: In the Editor, load Bootstrap, let it transition to MainMenu, then manually
load Bootstrap a second time in the same Play session; confirm only one `GameManager` instance
exists afterward and Bootstrap still ends by transitioning to MainMenu.

**Acceptance Scenarios**:

1. **Given** a `GameManager` already exists from a prior Bootstrap run in this process, **When**
   Bootstrap runs again, **Then** it does not create a second `GameManager`.
2. **Given** Bootstrap detects an existing `GameManager`, **When** it finishes its check, **Then**
   it still transitions to MainMenu exactly as it would on a true cold launch.

---

### Edge Cases

- What happens if the `GameConfig` asset reference is unassigned in the Bootstrap scene (e.g., a
  broken prefab link after a merge)? Bootstrap MUST fail loudly (a logged error identifying the
  missing reference) and MUST NOT transition to MainMenu with an unconfigured `GameManager` — a
  silent proceed would only surface as a confusing null-reference failure much later, deep in
  gameplay.
- What happens if Bootstrap is loaded a second time in the same process (User Story 3)? Persistent
  systems are detected as already existing and reused; no duplicate is created.
- What happens if device asset loading is slow enough that persistent systems aren't ready the
  instant Bootstrap's first frame runs? The transition to MainMenu MUST wait for initialization to
  actually complete — Bootstrap does not fire the transition on a fixed timer or on the first
  frame unconditionally.
- What happens if a developer opens MainMenu.unity directly in the Editor without going through
  Bootstrap first (a common Unity iteration habit)? This is a development-workflow concern, not a
  shipped-app flow; this spec does not require Bootstrap or MainMenu to guard against it (that
  behavior, if any, belongs to whichever spec defines `GameManager`'s own safety checks).
- What happens if the persistent audio mixer root fails to initialize but `GameConfig`/
  `GameManager` succeeded? Bootstrap MUST treat this the same as a `GameConfig` failure — it is
  one of the "other persistent systems" this scene is responsible for standing up before handing
  off, per User Story 2.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: `Bootstrap.unity` MUST be the first scene in the project's Build Settings scene list
  (index 0), so it is what the app runs on cold launch.
- **FR-002**: Bootstrap MUST contain exactly one initialization entry point responsible for
  standing up persistent systems in order; it MUST NOT rely on multiple independent, unordered
  `Awake()`/`Start()` methods racing each other across separate GameObjects.
- **FR-003**: Bootstrap MUST create the single persistent `GameManager` (per
  `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`) and mark it
  to survive scene loads, unless one already exists in the process (see FR-008).
- **FR-004**: Bootstrap MUST hold the serialized reference to the one `GameConfig` asset
  (`Assets/Config/GameConfig.asset`, schema owned by
  `specs/systems/shared-config-and-state/001-game-config-schema/spec.md`) and assign it to
  `GameManager` before any later step in Bootstrap's own sequence runs.
- **FR-005**: Bootstrap MUST initialize a persistent audio mixer root (a `DontDestroyOnLoad`
  GameObject hosting the project's shared `AudioMixer`) as part of its "other persistent systems"
  responsibility, before transitioning to MainMenu. This spec owns only that this initialization
  happens and where in the order it falls — the audio mixer root's own internal structure and
  runtime behavior belong to the audio system specs (`specs/systems/audio/`).
- **FR-006**: Bootstrap's initialization order MUST be deterministic and MUST NOT proceed past a
  step until its predecessor has completed: (1) assign the `GameConfig` reference, (2) create or
  reuse `GameManager`, (3) initialize the persistent audio mixer root, (4) trigger the transition
  to MainMenu.
- **FR-007**: Once all persistent systems in FR-006 have completed, Bootstrap MUST trigger the
  scene transition to `MainMenu.unity` using the shared mechanism defined by
  `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md`. Bootstrap MUST
  NOT call a separate, ad hoc scene-load path of its own.
- **FR-008**: If a persistent `GameManager` already exists when Bootstrap runs (see User Story 3),
  Bootstrap MUST reuse it instead of creating a second one, and MUST still proceed through the
  remaining steps of FR-006 (verifying/assigning `GameConfig`, ensuring the audio root exists,
  then transitioning) rather than skipping straight to MainMenu unchecked.
- **FR-009**: If the `GameConfig` reference is missing/unassigned, or the persistent audio mixer
  root fails to initialize, Bootstrap MUST log a clear, identifiable error and MUST NOT trigger
  the transition to MainMenu.
- **FR-010**: Bootstrap MUST NOT render playable content, present gameplay UI (joystick, action
  button, battery indicator), or accept gameplay input. A minimal, static loading visual (e.g., a
  logo or blank frame) is permitted but not required.
- **FR-011**: Bootstrap MUST NOT define or duplicate `GameConfig`'s schema, `GameState`'s shape, or
  the scene-transition mechanism's own implementation — those remain owned entirely by the specs
  cited in FR-003, FR-004, and FR-007.

### Key Entities

- **Bootstrap Scene**: The `Bootstrap.unity` scene itself — holds the initialization entry point,
  the serialized `GameConfig` reference, and the GameObjects for `GameManager` and the persistent
  audio mixer root. This is the one entity this spec actually owns.
- **Bootstrap Initialization Sequence**: The ordered, gate-checked list of steps (assign config →
  create/reuse `GameManager` → initialize audio root → transition) described in FR-006 — the core
  behavior this spec specifies.
- **GameManager** *(referenced, not owned)*: The persistent `MonoBehaviour` created here; its own
  shape and API are defined by `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md`.
- **GameConfig** *(referenced, not owned)*: The `ScriptableObject` asset referenced here; its
  schema is defined by `specs/systems/shared-config-and-state/001-game-config-schema/spec.md`.
- **Persistent Audio Mixer Root**: A `DontDestroyOnLoad` GameObject created here to host the
  shared `AudioMixer`; its internal audio behavior is defined by `specs/systems/audio/`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: From a cold app launch, the Main Menu's Start button becomes interactive without any
  manual step beyond opening the app, in 100% of launches during manual QA.
- **SC-002**: At no point during or after any QA pass does more than one persistent `GameManager`
  instance exist simultaneously in the running process, including after a Bootstrap re-entry.
- **SC-003**: A deliberately-broken `GameConfig` reference is caught with a logged error before
  MainMenu ever loads, in 100% of test launches that exercise this condition — it never instead
  surfaces later as a null-reference error inside gameplay.
- **SC-004**: Bootstrap's own on-screen presence is imperceptible under normal conditions on
  target hardware — manual QA reports no visible hang, spinner, or stall beyond a momentary frame
  before MainMenu appears.

## Assumptions

- Bootstrap.unity has no player-facing gameplay content; any Camera/EventSystem present exists
  only to satisfy Unity's technical requirements for a scene to run, not to render anything the
  player is meant to look at.
- "Other persistent systems" is scoped, for this spec, to the persistent audio mixer root — the
  one concrete example given in this feature's brief. Future persistent systems Bootstrap must
  also stand up are added to this spec's requirements additively (constitution Principle V), not
  invented speculatively here ahead of a concrete need (Principle II).
- No loading-screen UI is required. The GDD does not call for one, and Bootstrap's own work
  (creating a few persistent objects and assigning a config reference) is not expected to take
  long enough on target hardware to need one; if that assumption proves wrong during on-device
  testing, a loading indicator is a future amendment, not part of this spec.
- Bootstrap is never re-entered by the player during normal play (Main Menu, once reached, never
  routes back to Bootstrap). The re-entry behavior in User Story 3 exists purely as a correctness
  guarantee against Editor/testing workflows and future flows this spec does not anticipate, not
  because a shipped flow currently exercises it.

## Related

- GDD: no single narrative chapter — Bootstrap is a Unity architecture necessity (see ROADMAP.md
  §3, "001-bootstrap-scene" row), consistent with GDD Ch. 17 ("SEMUA angka tuning wajib berada di
  satu sumber config") and the constitution's Unity Architecture Consistency principle.
- `specs/ROADMAP.md` §3 (scenes table, `001-bootstrap-scene` row) and §5 (build sequence: this
  scene is the first scene-layer spec, built after every `shared-config-and-state` system spec).
- `specs/systems/shared-config-and-state/001-game-config-schema/spec.md` — owns `GameConfig`'s
  schema; not redefined here.
- `specs/systems/shared-config-and-state/002-shared-game-state-and-manager/spec.md` — owns
  `GameState`'s shape and `GameManager`'s API/persistence mechanism; not redefined here.
- `specs/systems/progression-and-scene-flow/002-scene-transition-manager/spec.md` — owns the
  actual scene-loading mechanism Bootstrap calls into; not redefined here.
- `.specify/memory/constitution.md` Principle III (Unity Architecture Consistency — one
  `GameManager`, no singletons/static mutable gameplay state) and Principle IV (Test-Before-Done).
