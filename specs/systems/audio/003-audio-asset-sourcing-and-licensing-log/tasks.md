---
description: "Task list for 003-audio-asset-sourcing-and-licensing-log"
---

# Tasks: Audio Asset Sourcing and Licensing Log

**Input**: Design documents from
`specs/systems/audio/003-audio-asset-sourcing-and-licensing-log/spec.md`

**Prerequisites**: spec.md (this folder), plus `001-sound-event-taxonomy` for the `SoundEventId`
enum each log entry links back to. GDD Ch. 14.1 supplies the five asset categories this log is
inventoried by (Ambient, Player, Monster, Sistem, Naratif); GDD Ch. 14.3 and 19.4 supply the
required log columns (nama asset/clip ID, sumber/source, lisensi/license, status
modifikasi/modification status, penanggung jawab integrasi/integration owner, status penggunaan
komersial/commercial-use status) and the "no AI-generated asset" rule this log must enforce.

**Tests**: EditMode tests are mandatory per constitution Principle IV — the log schema and audit
policy are pure C# data/logic with no rendering, physics, or input dependency. This is a
process/log spec, so "tasks" below are split per log column and per GDD asset category rather
than per gameplay mechanic.

**Organization**: Tasks are grouped by user story (US1, the only story in `spec.md`); Setup and
Foundational phases precede it because population and audit both depend on the entry schema
existing first.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on other unfinished tasks)
- **[Story]**: Which user story this task belongs to
- Every task names its exact file path under `Assets/`

## Phase 1: Setup

- [ ] T001 Define the `AudioLicenseType` enum (`Cc0`, `CcBy`, `CcBySa`,
  `RoyaltyFreeCommercial`, `TeamOriginal`, `Unknown` — `AiGenerated` is intentionally omitted per
  GDD 19.4's "no AI-generated asset" rule, so no valid license value can represent one) in
  `Assets/Scripts/Systems/Audio/AudioLicenseType.cs`.
- [ ] T002 [P] Define the `CommercialUseStatus` enum (`Allowed`, `Restricted`, `Unknown`) in
  `Assets/Scripts/Systems/Audio/CommercialUseStatus.cs`.
- [ ] T003 [P] Define the `AudioLicenseApprovalStatus` enum (`Approved`, `PendingReview`,
  `Blocked`) in `Assets/Scripts/Systems/Audio/AudioLicenseApprovalStatus.cs`.

## Phase 2: Foundational — Log Entry Schema, One Task Per Column (blocks Phase 3 and Phase 4)

**Purpose**: Population (Phase 3) and audit (Phase 4) both need the complete entry schema first.

### Tests for the schema (write first, confirm they fail before implementing)

- [ ] T004 [P] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/AudioLicenseEntryTests.cs`:
  `Entry_MissingAnyRequiredField_IsComplete_ReturnsFalse`.
- [ ] T005 [P] Same file: `Entry_AllRequiredFieldsPresent_IsComplete_ReturnsTrue`.

### Implementation — one task per required log column

- [ ] T006 [US1] Add `ClipId` and `EventId` (link to `001-sound-event-taxonomy`'s `SoundEventId`)
  fields to `AudioLicenseEntry` in `Assets/Scripts/Systems/Audio/AudioLicenseEntry.cs`.
- [ ] T007 [P] [US1] Add `SourceUrl` and `Provider` fields, same file.
- [ ] T008 [P] [US1] Add `Creator` field, same file.
- [ ] T009 [P] [US1] Add `License` field (type `AudioLicenseType`, from T001), same file.
- [ ] T010 [P] [US1] Add `AttributionText` field, same file.
- [ ] T011 [P] [US1] Add `ModificationNotes` field, same file.
- [ ] T012 [P] [US1] Add `ProofReference` field (path/URL to archived evidence of the license),
  same file.
- [ ] T013 [P] [US1] Add `IntegrationOwner` field (penanggung jawab integrasi, GDD 19.4), same
  file.
- [ ] T014 [P] [US1] Add `CommercialUseStatus` field (type `CommercialUseStatus`, from T002),
  same file.
- [ ] T015 [US1] Add `ApprovalStatus` field (type `AudioLicenseApprovalStatus`, from T003) and
  implement `IsComplete()` (true only when `ClipId`, `EventId`, `SourceUrl`, `Provider`,
  `Creator`, `License != Unknown`, `ProofReference`, and `IntegrationOwner` are all populated),
  same file.
- [ ] T016 [US1] Run T004–T005 and confirm green.

**Checkpoint**: Entry schema is correct and independently testable.

---

## Phase 3: User Story 1 - Use Audio Safely — Inventory Per GDD Asset Category (Priority: P1)

**Goal**: Every clip referenced by `001-sound-event-taxonomy` has a traceable log entry, filed
under its GDD Ch. 14.1 category.

**Independent Test**: See spec.md US1 — audit a sample of every event category and reject an
entry with missing provenance.

### Tests for User Story 1 (write first, confirm they fail before implementing)

- [ ] T017 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/AudioLicenseLogTests.cs`:
  `Log_ContainsCompleteEntry_ForEveryAmbientEvent` (`AcHum`, `ElectricBuzz`, `EmptyOfficeTone`).
- [ ] T018 [P] [US1] Same file: `Log_ContainsCompleteEntry_ForEveryPlayerEvent` (`FootstepWalk`,
  `FootstepSprint`, `Breathing`, `InteractGeneric`, `BatterySwap`).
- [ ] T019 [P] [US1] Same file: `Log_ContainsCompleteEntry_ForEveryMonsterEvent` (`Patrol`,
  `Distant`, `InvestigationCue`, `Chase`, `AttackCatch`).
- [ ] T020 [P] [US1] Same file: `Log_ContainsCompleteEntry_ForEverySistemEvent` (`LightFlicker`,
  `Pickup`, `DoorLocked`, `DoorUnlock`, `SplashTextFloor`, `UiButton`).
- [ ] T021 [P] [US1] Same file: `Log_ContainsCompleteEntry_ForEveryNaratifEvent` (`LiftBell`,
  `FootstepsBoss`, `CoworkerLaughterDistant`).
- [ ] T022 [P] [US1] Same file: `Audit_EntryWithMissingProvenanceField_IsRejected` — SC-002 (audit
  catches 100% of missing-license fixtures).

### Implementation for User Story 1

- [ ] T023 [US1] Implement `AudioLicenseLog` in `Assets/Scripts/Systems/Audio/AudioLicenseLog.cs`:
  plain C# registry holding `List<AudioLicenseEntry>`, `TryGetEntry(clipId)`,
  `GetEntriesForEvent(SoundEventId)`. Depends on T015.
- [ ] T024 [P] [US1] Inventory and log the **Ambient** category clips (`AcHum`, `ElectricBuzz`,
  `EmptyOfficeTone`) with full provenance in
  `Assets/Config/AudioLicenseLog/AmbientEntries.asset` (or seed data in `AudioLicenseLog.cs` if
  no ScriptableObject asset pipeline exists yet for this feature).
- [ ] T025 [P] [US1] Inventory and log the **Player** category clips (`FootstepWalk`,
  `FootstepSprint`, `Breathing`, `InteractGeneric`, `BatterySwap`), same location pattern as T024.
- [ ] T026 [P] [US1] Inventory and log the **Monster** category clips (`Patrol`, `Distant`,
  `InvestigationCue`, `Chase`, `AttackCatch`), same location pattern as T024.
- [ ] T027 [P] [US1] Inventory and log the **Sistem** category clips (`LightFlicker`, `Pickup`,
  `DoorLocked`, `DoorUnlock`, `SplashTextFloor`, `UiButton`), same location pattern as T024.
- [ ] T028 [P] [US1] Inventory and log the **Naratif** category clips (`LiftBell`,
  `FootstepsBoss`, `CoworkerLaughterDistant`), same location pattern as T024.
- [ ] T029 [US1] Run T017–T022 and confirm green.

**Checkpoint**: Every taxonomy event has a traceable, complete-or-flagged log entry.

---

## Phase 4: User Story 1 continued — Release Audit & Shipment Blocker (FR-002)

**Goal**: Unknown, expired, or incompatible rights block shipment; AI-generated audio is always
blocked (GDD 19.4).

### Tests (write first, confirm they fail before implementing)

- [ ] T030 [P] [US1] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/AudioLicenseAuditPolicyTests.cs`:
  `RunReleaseAudit_UnknownLicense_BlocksShipment`.
- [ ] T031 [P] [US1] Same file: `RunReleaseAudit_ExpiredOrIncompatibleRights_BlocksShipment`.
- [ ] T032 [P] [US1] Same file: `RunReleaseAudit_AllEntriesApproved_PassesCleanForRelease`.
- [ ] T033 [P] [US1] Same file: `RunReleaseAudit_NoValidLicenseCanRepresentAiGenerated_AlwaysBlocked`
  — proves an entry can never carry an "AI-generated" license value and ship (GDD 19.4).

### Implementation

- [ ] T034 [US1] Implement `AudioLicenseAuditPolicy.RunReleaseAudit(AudioLicenseLog log)` in
  `Assets/Scripts/Systems/Audio/AudioLicenseAuditPolicy.cs`: plain C# method returning a result
  listing blocked vs. approved entries. Depends on T023.
- [ ] T035 [US1] Run T030–T033 and confirm green.

**Checkpoint**: A release build can be gated on `RunReleaseAudit` returning zero blocked entries.

---

## Phase 5: Edge Cases — Flagged-for-Review Paths

**Goal**: AI-generated audio, bundled package clips, edited CC0 clips, and inaccessible source
pages are flagged for manual review rather than silently approved or silently blocked.

### Tests (write first, confirm they fail before implementing)

- [ ] T036 [P] EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/AudioLicenseAuditPolicyTests.cs`:
  `RunReleaseAudit_BundledPackageClip_IsFlaggedForManualReview_NotAutoApproved`.
- [ ] T037 [P] Same file: `RunReleaseAudit_EditedCc0ClipWithoutModificationNotes_IsFlaggedIncomplete`.
- [ ] T038 [P] Same file: `RunReleaseAudit_InaccessibleSourcePageMarker_IsFlaggedForReview_NotSilentlyApproved`.

### Implementation

- [ ] T039 Add a `NeedsManualReview` result category (distinct from `Blocked`/`Approved`) to
  `AudioLicenseAuditPolicy.cs` and implement the three flagging branches above.
- [ ] T040 Run T036–T038 and confirm green.

---

## Phase 6: Taxonomy Consistency (FR-003)

**Goal**: A replacement clip either preserves the original event's semantics or the audit flags
that the taxonomy documentation needs updating.

- [ ] T041 EditMode test in
  `Assets/Tests/EditMode/Systems/Audio/AudioLicenseAuditPolicyTests.cs`:
  `RunReleaseAudit_ReplacementChangesSemanticsWithoutTaxonomyDocUpdate_IsFlagged`.
- [ ] T042 Add `ReplacesClipId` and `TaxonomyDocUpdated` fields to `AudioLicenseEntry.cs`, and the
  corresponding check to `AudioLicenseAuditPolicy.cs`.
- [ ] T043 Run T041 and confirm green.

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T044 Code review pass: confirm zero `UnityEngine.MonoBehaviour`/`Component` references in
  `AudioLicenseEntry.cs`, `AudioLicenseLog.cs`, and `AudioLicenseAuditPolicy.cs`.

## Dependencies & Execution Order

- **Setup (T001–T003)**: No dependencies.
- **Foundational (T004–T016)**: Depends on Setup.
- **User Story 1 inventory (T017–T029)**: Depends on Foundational schema (T015) and
  `001-sound-event-taxonomy`'s `SoundEventId`.
- **User Story 1 audit (T030–T035)**: Depends on T023 (the populated log).
- **Edge cases (T036–T040)**: Depends on T034.
- **Taxonomy consistency (T041–T043)**: Depends on T034.
- **Polish (T044)**: Depends on all of the above.

## Notes

- This entire feature is plain C# data/logic — no `MonoBehaviour` adapter is needed; the audit
  runs as an editor/build-time check, not a runtime system.
- Scope is provenance and approval only — do not add event-definition logic (see
  `001-sound-event-taxonomy`) or mix-state logic (see `002-dynamic-mix-state-machine`) here.
- Commit after each checkpoint.
