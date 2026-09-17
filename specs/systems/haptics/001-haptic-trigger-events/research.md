---
description: "Phase 0 research for Haptic Trigger Events"
---

# Research: Haptic Trigger Events

**Input**: `spec.md` in this folder. This document records the one real technical decision this
feature needs before planning/tasks: which concrete Unity/iOS API actually fires a haptic pulse.
The constitution's Technology Stack section explicitly defers this choice to the owning feature
spec ("a feature spec that needs one... picks the concrete API during its own planning phase and
records the choice in that spec's `research.md`") — this is that record.

## Decision: Platform haptic API

**Decision**: Implement the platform-specific haptic call as a **thin custom native iOS plugin
wrapping Apple's `UIFeedbackGenerator` family** —
`UIImpactFeedbackGenerator` (styles `.heavy`/`.rigid` for `PlayerCaught`, `.light` for
`ItemPickup`) and `UINotificationFeedbackGenerator` (`.warning` for `BatteryCritical`, `.error` for
`BatteryDepleted`) — bridged into Unity via a small Objective-C (`.mm`) file under
`Assets/Plugins/iOS/` and `[DllImport("__Internal")]` extern declarations on the C# side. On any
runtime where that native bridge is not the shipping target (Unity Editor, or any hypothetical
future non-iOS platform), the `HapticFeedbackController` adapter falls back to Unity's built-in
`Handheld.Vibrate()` — a generic, engine-provided call that is always safe to invoke and requires
no native symbols — rather than attempting the native call and catching a failure.

**Rationale**:

- **The GDD explicitly frames haptics as leveraging the iOS platform, not as a generic
  cross-platform afterthought.** Ch. 15.2's own wording — *"memanfaatkan kekuatan platform iOS"*
  ("leveraging the power of the iOS platform") — is a direct textual signal toward the native
  Apple API, not the lowest-common-denominator engine call. The constitution's Technology Stack
  confirms the target platform is "iOS (iPhone), landscape orientation only" with no Android
  target — there is no cross-platform requirement pulling toward a generic solution.
- **`UIFeedbackGenerator`'s built-in semantic styles are enough for the three mandatory events.**
  This feature needs exactly four *fixed, one-shot* patterns (`PlayerCaught`, `BatteryCritical`,
  `BatteryDepleted`, `ItemPickup`), each of which maps cleanly onto one of `UIFeedbackGenerator`'s
  already-designed, Apple-tuned built-in styles. Reaching for the lower-level `CHHapticEngine`/
  `CHHapticPattern` custom-waveform API — which is what a *fully* custom haptic language would
  need — is more native surface than the mandatory scope requires, and would be a Principle II
  (Simplicity/YAGNI) violation for MVP.
- **A thin bridge is small enough to own outright, and avoids a paid dependency for something
  Apple's own frameworks already expose directly.** `UIFeedbackGenerator` is a handful of
  Objective-C calls; the plugin bridge is on the order of tens of lines, not a framework to
  maintain. Every third-party asset considered (see Alternatives) is itself a wrapper *around*
  this same OS API on iOS — choosing one would add a paid Asset Store dependency and an external
  maintenance risk for zero additional capability on an iOS-only target, which fails the
  constitution's bar that "every dependency... must be justified against a specific requirement."
- **This choice keeps the door open for the optional heartbeat stretch (User Story 4) without a
  rewrite.** If `MonsterProximityPulse` is ever picked up, extending the *same* native plugin file
  to also expose `CHHapticEngine`/`CHHapticPattern` for a custom repeating/dynamic waveform is an
  additive change to one file — it does not require replacing an already-shipped mechanism, since
  the mandatory events keep using `UIFeedbackGenerator` regardless.
- **Silent degradation is inherited for free.** Apple's `UIFeedbackGenerator` and `CHHapticEngine`
  APIs already no-op safely when the device has no Taptic Engine or when the user has System
  Haptics turned off in Settings — exactly the "no in-game toggle, must degrade silently" behavior
  the spec requires (FR-009, FR-010). No custom capability-detection code has to be written or
  maintained to get this property; writing one would itself violate Principle II.

**Alternatives Considered**:

1. **Third-party asset (e.g., Lofelt Studio, or "Nice Vibrations" by More Mountains)**. These
   packages wrap native haptics (Core Haptics on iOS, an Android equivalent) behind a simplified
   Unity-side API and support authoring custom `.haptic` clips. Rejected for this feature because:
   (a) on an iOS-only target, the cross-platform parity these packages sell is worth nothing; (b)
   on iOS specifically, they call the exact same `UIFeedbackGenerator`/`CHHapticEngine` APIs a
   ~50-line custom bridge already reaches directly, so no unique capability is gained; (c) it is a
   paid Asset Store dependency with its own update/compatibility risk, which the constitution's
   "none adopted by default... must be justified against a specific requirement" bar does not
   clear when a smaller, equally capable option exists; (d) Lofelt Studio specifically was
   discontinued by its maker in 2022, making it a live risk to depend on for a new project.
2. **Unity's `Handheld.Vibrate()` as the primary (not just fallback) mechanism**. Rejected as the
   *primary* path because on iOS it triggers a single generic system-level vibration with no
   amplitude/sharpness/style control — all three mandatory events would produce the identical felt
   buzz, which directly undermines GDD Ch. 15.2's intent to make haptics reinforce *distinct*
   tension moments (and fails SC-006's "each person can correctly say the three feel noticeably
   different" bar). It remains valuable as the automatic, zero-cost fallback for any non-iOS
   runtime (Editor, future platforms) where writing/maintaining a native bridge would have no
   payoff.
3. **Full custom `CHHapticEngine`/`CHHapticPattern` authoring for all four events now.** This is
   the most powerful option (arbitrary custom waveforms, exactly what the optional heartbeat pulse
   would eventually need) but is more implementation surface than four fixed, one-shot patterns
   require today. Deferred: the native plugin bridge can be extended to this API specifically if
   and when User Story 4 is picked up, without disturbing the mandatory events' already-working
   `UIFeedbackGenerator` calls.

## Non-decisions (explicitly out of scope for this document)

- The exact retrigger-cooldown duration (`GameConfig.hapticCooldownSeconds`) is a feel/engineering
  value with no GDD-locked number (GDD Ch. 15.2 does not specify one). Per `ROADMAP.md` §0's rule
  for feel-based values, `tasks.md` provides an initial default to make the feature function and
  be testable, explicitly flagged as needing on-device re-confirmation — it is not re-litigated
  here since it is a tuning number, not an API/architecture decision.
- Whether User Story 4 (heartbeat pulse) is ever built at all is a scheduling decision for a later
  production phase (GDD Ch. 19.2 Fase 6), not a research question this document needs to resolve.
