---
description: "Task list for Shadow Casting & Quality Fallback"
---

# Tasks: Shadow Casting & Quality Fallback

**Input**: Design documents from `specs/systems/flashlight-and-battery/005-shadow-casting-and-quality-fallback/`

**Prerequisites**: [spec.md](./spec.md); `001-light-state-thresholds-and-radius` must already
provide `LightState`/`LightStateSystem` and the Flashlight `Light` component's controlling
adapter (`003`'s `FlashlightRadiusController`)

**Tests**: Included — constitution Principle IV. The config-resolution logic (which value the
light is actually configured with) is pure and EditMode-testable; whether a shadow visibly
renders and at what frame cost is a rendering/perf concern validated on-device per Principle IV's
exception.

**Organization**: Tasks are grouped by user story (P1, P2, P3). User Story 1 is primarily an
on-device/Editor rendering verification since "a shadow visibly appears and moves" has no pure-C#
surface of its own; User Stories 2 and 3 each own one small pure resolution function that IS
testable in EditMode.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on an incomplete task)
- **[Story]**: US1, US2, or US3
- File paths are exact and repo-relative

---

## Phase 1: Setup

- [ ] T001 Confirm `Assets/Scripts/Systems/Flashlight/`, `Assets/Scripts/MonoBehaviours/Flashlight/`,
      and `Assets/Tests/EditMode/Flashlight/` exist (created by `001`/`003`)

---

## Phase 2: Foundational

- [ ] T002 Add `shadowsEnabled` (bool, default `true`) and `shadowResolution` (int, default `512`
      — carried over from prior native-engine tuning, re-confirm on-device per spec.md FR-004) to
      `GameConfig`
- [ ] T003 Create `Assets/Scripts/Systems/Flashlight/ShadowQualitySystem.cs`: a plain C# static
      class with a private, fixed array of supported resolution tiers (`256, 512, 1024, 2048`) —
      no `UnityEngine.MonoBehaviour`, `Component`, or scene dependency

**Checkpoint**: `GameConfig` carries both shadow fields; the resolution system's skeleton exists.

---

## Phase 3: User Story 1 - Walls and Furniture Cast Real-Time Shadows From the Flashlight (Priority: P1)

**Goal**: The single flashlight `Light` (from `001`/`003`/`004`) casts real-time shadows from
solid geometry, updating every frame as the light moves.

**Independent Test**: Manual/Editor scene test per spec.md's own Independent Test — place a solid
object between the flashlight and a surface, move the player, and observe the shadow appear and
track. This is the constitution Principle IV exception (rendering behavior needing a live scene).

### Implementation for User Story 1

- [ ] T004 [US1] In the Unity Editor, on the Flashlight `Light` GameObject (the same object
      `003`'s `FlashlightRadiusController` targets), enable real-time shadows (`Light.shadows` =
      soft shadows) and confirm no second, independently-authored shadow-casting light exists
      anywhere in the test scene (FR-001)
- [ ] T005 [US1] Mark at least one piece of test-scene geometry (e.g. a desk placeholder box) as
      shadow-casting/receiving (default Unity mesh renderer settings), matching GDD Ch. 12's
      "dinding dan furniture solid melempar shadow"

### Manual Validation for User Story 1

- [ ] T006 [US1] On-device or in Editor Play mode: place a solid object between the flashlight and
      a floor/wall area, move the player, and confirm a shadow appears behind the object and its
      position updates continuously as the player moves (spec.md SC-001) — record the observation
      result
- [ ] T007 [US1] Confirm shadow casting behaves correctly with no shadow-casting geometry present
      in an earlier/emptier test scene (no error, simply nothing rendered) — spec.md Edge Cases

**Checkpoint**: User Story 1 is independently complete — real-time shadows are visibly correct in
a running scene.

---

## Phase 4: User Story 2 - Shadows Can Be Disabled Entirely From Config Alone (Priority: P2)

**Goal**: `GameConfig.shadowsEnabled = false` removes all shadow casting from the flashlight with
no other file changed, and all four Light States remain distinguishable without shadows.

**Independent Test**: `ShadowQualitySystemTests` in EditMode for the pure resolve function;
on-device/Editor observation for the legibility claim.

### Tests for User Story 2 (write first, confirm they fail before implementing)

- [ ] T008 [P] [US2] Create `Assets/Tests/EditMode/Flashlight/ShadowQualitySystemTests.cs` with
      cases: `config.shadowsEnabled = true` → `ResolveShadowsEnabled` returns `true`;
      `config.shadowsEnabled = false` → returns `false` (spec.md FR-002, FR-006)

### Implementation for User Story 2

- [ ] T009 [US2] In `ShadowQualitySystem.cs`, add `public static bool
      ResolveShadowsEnabled(GameConfig config) => config.shadowsEnabled;` — a direct, live
      pass-through with no caching, so callers always read the current config value (FR-002,
      FR-006)
- [ ] T010 [US2] Run T008 and confirm all cases pass
- [ ] T011 [US2] Create `Assets/Scripts/MonoBehaviours/Flashlight/FlashlightShadowController.cs`: a
      thin `MonoBehaviour` holding a reference to the Flashlight `Light` component that, once per
      `Update()`, sets `light.shadows = ShadowQualitySystem.ResolveShadowsEnabled(config) ?
      LightShadows.Soft : LightShadows.None` — a live, per-frame read, never a one-time
      initialization value (FR-002, FR-006, FR-007)

### Manual Validation for User Story 2

- [ ] T012 [US2] On-device or in Editor: set `GameConfig.shadowsEnabled` to `false`, run a
      shortened-battery-duration test scene, and confirm all four Light States (`001`) remain
      visually distinguishable by radius/flicker alone with shadows off, then set it back to
      `true` and confirm shadows resume with no other change (spec.md US2 Scenarios 1–3, SC-002,
      SC-003) — record the observation result

**Checkpoint**: User Story 2 independently complete — the fallback switch works and does not
break state legibility.

---

## Phase 5: User Story 3 - Shadow Quality Can Be Reduced From Config Alone Without Disabling Shadows (Priority: P3)

**Goal**: A separate `shadowResolution` config value lowers shadow cost while shadows remain
enabled, snapped to a safe supported tier so an unreasonably large value cannot reproduce the
historical over-expose failure.

**Independent Test**: Extend `ShadowQualitySystemTests` with resolution-resolution cases.

### Tests for User Story 3 (write first, confirm they fail before implementing)

- [ ] T013 [P] [US3] In `ShadowQualitySystemTests.cs`, add cases: `shadowResolution = 512` (the
      shipped default) → resolves to `512` unchanged; `shadowResolution = 300` (between tiers) →
      snaps to the nearest supported tier (`256` or `512`, whichever is closer); `shadowResolution
      = 0` or a negative value (misconfiguration) → resolves to the smallest supported tier
      (`256`), never `0`/negative/crash (spec.md FR-004, Edge Cases)
- [ ] T014 [P] [US3] In the same file, add a case for an unreasonably large input (e.g. `8192`) →
      resolves to the largest supported tier (`2048`), never passed through unclamped — this is
      the concrete guard against the historical over-expose failure this feature exists to prevent
      (spec.md FR-004, US3 Scenario 2)

### Implementation for User Story 3

- [ ] T015 [US3] In `ShadowQualitySystem.cs`, add `public static int
      ResolveShadowResolution(GameConfig config)`: clamps `config.shadowResolution` into the
      supported tier array's `[min, max]` range, then snaps to the nearest tier value (FR-004,
      FR-005)
- [ ] T016 [US3] Run T013–T014 and confirm all cases pass
- [ ] T017 [US3] In `FlashlightShadowController.cs`, once per `Update()` (or only when the config
      value changes, if the adapter chooses to cache it), map
      `ShadowQualitySystem.ResolveShadowResolution(config)` to the closest URP
      `UniversalAdditionalLightData` custom shadow resolution setting and apply it to the
      Flashlight `Light` (FR-004, FR-005)

### Manual Validation for User Story 3

- [ ] T018 [US3] On target hardware, profile frame rate with `shadowResolution` at the shipped
      default (`512`) and again at a lower tier (`256`), confirming a measurable improvement or at
      minimum no regression at the lower tier (spec.md SC-004) — record the profiling result
- [ ] T019 [US3] On target hardware (not Editor/simulator), confirm the scene sustains the
      project's target frame rate with shadows enabled at the shipped default resolution; if not,
      re-test with `shadowsEnabled = false` and/or a lower `shadowResolution` per FR-002/FR-004
      rather than a code change (spec.md SC-005) — record the result

**Checkpoint**: All three user stories independently complete; shadow behavior, its on/off
fallback, and its quality fallback are all config-only levers.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [ ] T020 [P] Grep `Assets/Scripts/` for any hardcoded shadow resolution or shadow-enabled
      literal duplicating a `GameConfig` value from T002 and replace any found with a config read
      (constitution Principle III/V)
- [ ] T021 Add XML-doc comments to `ShadowQualitySystem.ResolveShadowsEnabled` and
      `ResolveShadowResolution` documenting the tier-snapping rule and the over-expose guard
      inline

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → **Foundational (Phase 2)**: sequential.
- **User Story 1** depends only on Foundational and `001`/`003`'s existing Flashlight `Light`; it
  is scene/rendering work, not gated by US2/US3.
- **User Story 2** depends only on Foundational; independently testable via its own pure function.
- **User Story 3** depends only on Foundational; independently testable via its own pure function,
  and shares `ShadowQualitySystem.cs`/`FlashlightShadowController.cs` with US2 (coordinate edits).
- **Polish (Phase 6)**: after all three stories.

```text
Setup (T001)
   ↓
Foundational (T002-T003)
   ↓
   ├──> US1 (T004-T007)
   ├──> US2 (T008-T012)
   └──> US3 (T013-T019)
                           ↓
                      Polish (T020-T021)
```

## Notes

- [P] tasks touch different files, or independent additive regions of the same new test file.
- Unity API type names (`LightShadows`, `UniversalAdditionalLightData`) appear only in the
  `MonoBehaviour` adapter tasks (T011, T017), never in `ShadowQualitySystem.cs`'s pure logic,
  keeping the pure class free of `UnityEngine` scene/component dependencies per constitution
  Principle III.
- The historical over-expose failure this feature guards against (spec.md FR-004) was observed on
  a different render pipeline; T018/T019's on-device profiling is what actually re-verifies
  behavior under URP — the tier-snapping logic (T015) only bounds the *input*, it does not itself
  prove URP is safe at any given tier.
