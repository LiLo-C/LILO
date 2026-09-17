# Feature Specification: Eddie Character Bible

**Feature Branch**: `001-eddie-character-bible`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "A canonical, checkable reference for Eddie's characterization (GDD Ch. 10.1) that every other narrative-bearing spec — Prologue (003), the three floor scenes (004/005/006), and both Ending scenes (007/008) — must stay consistent with, instead of re-deriving his personality from the GDD independently each time."

## User Scenarios & Testing *(mandatory)*

<!--
  This feature is a content bible, not runtime code: its "users" are the writers, level
  designers, and reviewers who consult it before adding narrative content, and its
  "independent test" is a documented consistency-review process rather than an automated
  code test — per constitution Principle I, specs describe observable outcomes and success
  criteria, and a content bible's observable outcome is "does new content contradict it."
-->

### User Story 1 - Writer drafts new Eddie dialogue or narration (Priority: P1)

A writer preparing the Prologue comic script (spec 003) or an Ending scene script (spec 007/008)
needs to know exactly which personality traits and biographical facts about Eddie are locked, so
new lines read as the same person the GDD describes rather than a subtly different character.

**Why this priority**: Every narrative-bearing spec in the roadmap depends on this one (ROADMAP.md
§2, `narrative-content` row) — if this bible doesn't exist or is ambiguous, every downstream spec
either stalls or invents its own version of Eddie, and the two would eventually diverge.

**Independent Test**: A reviewer can take any drafted line or beat, check it against each trait
listed in Key Entities below, and produce a pass/fail note per trait — this review can happen
before a single line of code or a single final art asset exists, using only this document and the
draft text.

**Acceptance Scenarios**:

1. **Given** a drafted Prologue line describing Eddie's job, **When** a reviewer checks it against
   this bible, **Then** the reviewer can confirm or reject it using only the trait list below, with
   no need to re-read the GDD to adjudicate.
2. **Given** a drafted Ending line implies Eddie is financially comfortable or has a light
   workload, **When** a reviewer checks it against FR-001, **Then** the reviewer flags a
   contradiction and the line is revised before the owning spec leaves Draft.

---

### User Story 2 - Level designer justifies an environmental-storytelling prop (Priority: P1)

A level designer placing desk-locker contents or other Eddie-owned objects in a floor scene (spec
004/005/006) needs a fixed, short list of what Eddie canonically keeps on hand, so the prop
doesn't have to be re-justified against the GDD each time it appears in a new floor.

**Why this priority**: GDD Ch. 5.3 ties the flashlight's own in-fiction origin to this same
over-prepared trait ("Senter cadangan di loker meja" is a battery-giving object *because* Eddie is
the kind of person who keeps one there) — getting this list right affects a mechanic-facing spec,
not just flavor text.

**Independent Test**: A reviewer can open this bible's Key Entities table, confirm a proposed prop
is one of the three canon locker items (or a clearly-labeled non-canon addition requiring its own
justification), and approve or reject the placement without consulting anyone else.

**Acceptance Scenarios**:

1. **Given** a floor scene spec proposes a first-aid kit, emergency food, or handheld emergency
   lamp in Eddie's desk locker, **When** checked against FR-003, **Then** it passes without
   further justification.
2. **Given** a floor scene spec proposes an unrelated item (e.g., a weapon, a locker full of cash)
   as Eddie's personal effects, **When** checked against FR-003, **Then** it is flagged as a
   contradiction of the "chronically over-prepared, not affluent" trait pair and rejected or
   revised.

---

### User Story 3 - Reviewer audits the central thematic through-line (Priority: P2)

A narrative reviewer auditing a near-final Prologue or Ending script (or an in-progress floor
scene's environmental beats) checks that the GDD's explicitly-called-out through-line — the office
chasing Eddie in the dream is a literal version of the office chasing him in real life, and the
shrinking field of view mirrors his shrinking life — is still legible and not accidentally
contradicted or diluted by newly added content.

**Why this priority**: GDD Ch. 10.1 marks this through-line with "yang harus terasa" ("that must
be felt") — it is the one piece of characterization the GDD elevates above the rest of Eddie's
trait list, so it gets its own explicit checkable rule (FR-006/FR-007) rather than being folded
silently into the general trait check.

**Independent Test**: A reviewer can watch or read a scene script and independently answer "does
this scene support, contradict, or ignore the dream/reality equivalence and the shrinking-life
metaphor" — a yes/no/ignore judgment that doesn't require the full game to be built, only the
scene's draft content.

**Acceptance Scenarios**:

1. **Given** a drafted Ending script for the Good Ending (spec 007), **When** a reviewer checks it
   against FR-006, **Then** the reviewer can confirm Eddie's realization that his dream-office was
   a trauma-metaphor for his real job is present and not replaced with an unrelated resolution.
2. **Given** a proposed new gameplay-adjacent narrative beat wants to explain the shrinking-light
   metaphor via on-screen text or dialogue, **When** checked against FR-007, **Then** it is flagged
   — the through-line must stay felt, not stated (consistent with the implicit-storytelling stance
   GDD Ch. 10.3 states for foreshadowing, applied here to the core metaphor as well).

---

### Edge Cases

- What happens when a spec wants Eddie to display natural grace or organizational skill outside
  of work (e.g., a smoothly-executed non-work action in a comic panel)? → Flagged against FR-002;
  "clumsy" is a general trait, not one that suspends itself conveniently for a single scene.
- What happens when a spec wants to drop the psychologist detail because it's "not needed" for a
  particular scene? → Omission in one scene is fine (this bible does not require every trait to
  appear in every scene); an outright contradiction (e.g., a line stating Eddie has never needed
  therapy) is not (FR-005).
- What happens when a spec wants to use Eddie's overwork/underpay as a joke or punchline played for
  laughs rather than dread? → Flagged against FR-008; GDD Ch. 1.2 defines the game's mood as tense,
  immersive horror driven by limitation and uncertainty, not comic relief — Eddie's hardship is
  played straight, in service of that mood, not mined for comedy.
- What happens when two future specs (e.g., Prologue and Bad Ending) each add a new biographical
  detail about Eddie that isn't in the GDD at all (e.g., a hobby, a hometown)? → Out of this
  spec's scope to invent; if needed, that detail should be proposed as an amendment to this bible
  first (constitution Principle V) so both specs read the same fact, rather than each spec
  minting its own.
- What happens if a future GDD revision changes Eddie's characterization? → This spec must be
  amended first, citing what it supersedes (constitution Principle V), before any dependent spec
  is updated to match.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Any new narrative content (Prologue script, floor-scene environmental storytelling,
  Ending scripts, marketing copy derived from them) MUST portray Eddie as an underpaid, overworked
  salaryman carrying an unreasonable workload — MUST NOT depict him as well-compensated, in a
  comfortable job, or under a light workload.
- **FR-002**: Any new narrative content MUST NOT depict Eddie as generally graceful, organized, or
  physically adept — "clumsy" MUST remain a standing trait, not one suspended for convenience in a
  given scene.
- **FR-003**: Any new narrative content MUST portray Eddie as chronically over-prepared and
  cautious. The concrete evidence of this trait is fixed to exactly three items canonically kept in
  his desk locker — a first-aid kit, an emergency food supply, and a handheld emergency lamp (the
  same lamp that becomes the player's flashlight at gameplay start, per GDD Ch. 1.1 and 5.3) — and
  MUST NOT be replaced, reduced, or contradicted by a differently-characterized locker inventory
  without amending this spec first.
- **FR-004**: Any new narrative content MUST preserve Eddie's sharp, focused competence at his
  actual job as a trait that coexists with his general clumsiness (FR-002) — he is not written as
  incompetent at work itself, only physically/socially clumsy elsewhere.
- **FR-005**: Any new narrative content MUST preserve the established fact that Eddie regularly
  sees a psychologist. This fact MAY be omitted from any single scene (not every trait needs to
  appear everywhere) but MUST NOT be contradicted (e.g., a line stating he has never sought help,
  or that a single session "fixed" him) once it has been introduced elsewhere in the run.
- **FR-006**: Any new Prologue or Ending content MUST preserve the central thematic through-line:
  the office that chases Eddie in the dream (the gameplay's monster/building) is a literal version
  of the office that chases him in real life (his job/workload), and the player's shrinking field
  of view (the flashlight's Light States, GDD Ch. 5.2/12.1) mirrors his shrinking room to live his
  own life. New content MUST NOT introduce an alternate reading that contradicts this equivalence
  (e.g., framing the dream-office as random/unrelated to his job).
- **FR-007**: The through-line in FR-006 MUST stay implicit — conveyed through scene content,
  gameplay pacing, and visual/audio design — and MUST NOT be stated outright via on-screen text,
  narration, or dialogue that names the metaphor directly (e.g., a line literally explaining "the
  shrinking light represents your shrinking life"). This mirrors the implicit-only rule GDD Ch.
  10.3 states for foreshadowing elements, applied here to the core metaphor itself.
- **FR-008**: Any new narrative content involving Eddie's hardship (underpay, overwork, therapy)
  MUST be played straight, in service of the tense/uncertain horror mood defined in GDD Ch. 1.2 —
  MUST NOT be written as a comedic punchline or played for laughs at Eddie's expense.
- **FR-009**: A narrative-bearing spec (Prologue 003, any floor scene 004/005/006, either Ending
  007/008) that introduces new Eddie characterization detail MUST cite this spec by path in its
  own Related section, so the dependency is traceable without re-deriving it from the GDD.

### Key Entities

- **Eddie (Character)**: The player-character. Defined entirely by the trait set and canon facts
  below; this spec does not write his dialogue lines, only the constraints his lines must satisfy.
- **Personality Trait**: A checkable adjective/behavior pattern. Locked set: *clumsy* (FR-002),
  *chronically cautious / over-prepared* (FR-003), *sharply focused on his work* (FR-004).
- **Canon Biographical Fact**: A verifiable statement about Eddie's life circumstances, distinct
  from a personality adjective. Locked set: *underpaid* (FR-001), *overworked to the point of
  feeling he has no free space for himself* (FR-001), *sees a psychologist regularly* (FR-005).
- **Desk Locker Inventory**: The fixed, exhaustive list of personal items Eddie keeps at work as
  concrete evidence of his cautious trait — first-aid kit, emergency food supply, handheld
  emergency lamp (FR-003). The lamp is the same object that becomes the player's flashlight; this
  is the one item in the list with a direct mechanical consequence outside narrative content.
- **Central Thematic Through-Line**: The single-most-load-bearing piece of Eddie's characterization
  (GDD Ch. 10.1's "benang merah yang harus terasa") — the dream-office/real-office equivalence and
  the shrinking-field-of-view/shrinking-life metaphor (FR-006, FR-007).
- **Tone Guardrail**: The mood constraint from GDD Ch. 1.2 (tense, immersive horror from limited
  vision and uncertainty, not jumpscare/violence/comedy) that governs how Eddie's hardship and fear
  may be portrayed (FR-008).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of narrative-bearing specs that introduce new Eddie dialogue, narration, or
  characterization detail (Prologue 003, floor scenes 004/005/006, Endings 007/008) cite this spec
  in their own Related section before that spec's Status leaves Draft.
- **SC-002**: Zero shipped instances, across Prologue, all three floor scenes, and both Endings, of
  content contradicting any of the six locked traits/facts (underpaid, overworked, clumsy,
  cautious/over-prepared, work-focused, sees a psychologist) — verified at each spec's own review
  pass and again at the Release scope-lock gate (`specs/release/001-scope-lock-and-cut-order/`).
- **SC-003**: The central thematic through-line (FR-006) is independently identifiable by at least
  2 people outside the writing team after experiencing the Prologue and one Ending, without being
  told what to look for and without any on-screen text naming the metaphor (mirrors the review
  method used for GDD Ch. 20.2's light-state legibility check, applied to narrative legibility).
- **SC-004**: Zero on-screen text, subtitle, or narrated line across the shipped game states the
  shrinking-light/shrinking-life metaphor outright (FR-007), checked at the same Release gate as
  SC-002.

## Assumptions

- This bible captures backstory, personality, and thematic-consistency constraints only; it does
  not write Eddie's actual dialogue lines or comic-panel script — that remains the job of the
  Prologue (003) and Ending (007/008) scene specs, which cite this document rather than duplicate
  it.
- The GDD's Bahasa Indonesia text in Ch. 10.1 is the ultimate source of truth; the English
  paraphrases and trait names used throughout this spec (e.g., "chronically over-prepared") are
  this vault's working-canon phrasing for that same content, chosen for the same reason the rest of
  this English-language spec vault paraphrases the GDD — if a conflict is ever found between this
  spec's phrasing and the GDD, the GDD Ch. 10.1 text wins and this spec is amended.
- Eddie's visual likeness, comic-panel art direction, and voice performance (if any) are out of
  scope here — that is Fathia's (3D model) and Salwa's (prologue/ending comic art) ownership per
  GDD Ch. 19.1; this spec covers only textual/behavioral characterization a writer or reviewer can
  check without seeing final art.
- This spec introduces no new `GameConfig` fields, no new `MonoBehaviour`, and no runtime system —
  per constitution Principle II, a content bible is documentation reviewers consult, not code that
  loads at runtime. See `tasks.md` for the storage-location decision this implies.
- Every downstream narrative spec (Prologue 003, floor scenes 004/005/006, Endings 007/008) is
  assumed to be written after this spec exists, per the ROADMAP's dependency-ordered build
  sequence (§5: `narrative-content` has "no dependencies — can start anytime").

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[constitution]] — governing principles this spec must comply with (Principle II: no runtime
  system introduced by a content-only spec; Principle V: amendment/supersession process)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 10.1 (Eddie's characterization, the source this spec
  encodes as checkable constraints) and Ch. 1.2 (Core Experience/mood, the tone guardrail in
  FR-008)
- `specs/systems/narrative-content/002-foreshadowing-prop-catalog/` — the sibling content-bible
  spec; every catalog entry there must stay consistent with the through-line defined here (see
  that spec's FR-006)
- Future consumers (not yet written, cited here per ROADMAP.md §2's `narrative-content` row):
  `specs/scenes/003-prologue-scene/`, `specs/scenes/004-floor-52-scene/`,
  `specs/scenes/005-floor-51-scene/`, `specs/scenes/006-floor-50-scene/`,
  `specs/scenes/007-good-ending-scene/`, `specs/scenes/008-bad-ending-scene/`
