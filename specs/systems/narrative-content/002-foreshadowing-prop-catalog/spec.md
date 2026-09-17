# Feature Specification: Foreshadowing Prop Catalog

**Feature Branch**: `002-foreshadowing-prop-catalog`

**Created**: 2026-09-17

**Status**: Draft

**Input**: User description: "A fixed catalog of the foreshadowing elements listed in GDD Ch.
10.3 (monster-sound/elevator-bell, monster-footsteps/superior's-footsteps, mocking coworker
laughter, empty meeting room, stuck clock, unfinished-task computer, resignation-letter printer,
changing-name directory sign), plus the GDD's placement rule — not every item needs to be used,
but whatever is used must stay consistent and must never be explained via on-screen text — left
for each floor scene spec (004/005/006, not yet written) to decide actual per-floor placement
against."

## User Scenarios & Testing *(mandatory)*

<!--
  Like its sibling spec (001-eddie-character-bible), this feature is a content bible: its
  "users" are the writers, level/audio designers, and reviewers who select and check foreshadowing
  elements before adding them to a floor, and its "independent test" is a documented
  consistency-review process rather than an automated code test.
-->

### User Story 1 - Level designer selects foreshadowing elements for a floor (Priority: P1)

A level designer working on a floor scene spec (004, 005, or 006) wants to place one or more
foreshadowing elements in that floor without inventing new lore on the spot, so every floor draws
from the same validated list instead of each floor designer improvising independently.

**Why this priority**: This is the catalog's primary reason to exist — ROADMAP.md's `narrative-
content` row lists this spec as a direct dependency of the (not-yet-written) floor scene specs;
without a fixed list to select from, three different floor designers would likely invent three
different, mutually inconsistent sets of foreshadowing props.

**Independent Test**: A reviewer can open this catalog, take a floor scene spec's proposed
placement list, confirm every proposed element traces to one of the entries in Key Entities below,
and confirm no proposed element carries an explanatory label — this can be checked from the two
documents alone, before the floor exists in Unity.

**Acceptance Scenarios**:

1. **Given** a floor scene spec proposes placing "a wall clock stuck at 11:47," **When** checked
   against this catalog, **Then** it matches the Stuck Wall Clock entry and is approved.
2. **Given** a floor scene spec proposes a prop not on this list (e.g., a bloodstained wall), **When**
   checked against FR-001, **Then** it is flagged as outside this catalog and either dropped or
   proposed as a catalog amendment first.
3. **Given** a floor scene spec proposes using zero foreshadowing elements on Floor 52, **When**
   checked against FR-002, **Then** it is approved without issue — the GDD does not require every
   floor, or every entry, to be used.

---

### User Story 2 - Audio designer implements a narrative sound cue (Priority: P1)

An audio designer implementing the "Naratif" sound assets required by GDD Ch. 14.1 (elevator
bell, footsteps resembling a superior, faint mocking coworker laughter) needs the precise
narrative intent behind each cue, so the eventual mix stays consistent with what that sound is
supposed to imply rather than becoming a generic ambient sting.

**Why this priority**: Three of this catalog's eight entries are audio, not level-geometry props —
getting their narrative intent locked matters just as much to `audio/001-sound-event-taxonomy` and
`audio/002-dynamic-mix-state-machine` as the visual entries matter to level design, and audio
implementation typically starts independently of any one floor's layout.

**Independent Test**: A reviewer can take a produced or placeholder sound asset, check its
described trigger and intent against the matching catalog entry, and confirm the cue is used
consistently with that entry's definition wherever it appears.

**Acceptance Scenarios**:

1. **Given** the monster's ambient/patrol sound is implemented, **When** checked against the
   Monster Sound / Elevator Bell entry, **Then** its intent (echoing the office elevator bell) is
   confirmed as the one and only narrative reading assigned to that cue.
2. **Given** a new sound designer proposes reusing the elevator-bell-styled cue for an unrelated
   UI event (e.g., a menu confirmation sound), **When** checked against FR-005, **Then** it is
   flagged — a foreshadowing cue's meaning must stay singular, not repurposed elsewhere.

---

### User Story 3 - Reviewer audits the "never explained" rule at playtest (Priority: P2)

A narrative reviewer during Playtest & Polish (Production Plan Fase 7) checks the shipped build to
confirm that no foreshadowing element was ever explained via on-screen text, subtitle, or dialogue
— that every element that made it into the game stayed implicit end-to-end, from first floor draft
to final build.

**Why this priority**: This is the GDD's explicit, non-negotiable rule for this content ("tidak
pernah dijelaskan lewat teks") — catching a violation here, this late, is expensive, so this
review step exists specifically to catch it before submission rather than relying on it never
happening.

**Independent Test**: A reviewer can play through all three floors, note every foreshadowing
element encountered, and confirm none of them is accompanied by explanatory text/dialogue/UI —
this is checkable by playing the build alone, no source access required.

**Acceptance Scenarios**:

1. **Given** a build with a foreshadowing element implemented (e.g., the resignation-letter
   printer), **When** a reviewer encounters it in play, **Then** no tooltip, subtitle, or dialogue
   line explains what it means.
2. **Given** an accessibility pass wants to add closed captions describing in-world sound, **When**
   checked against the Edge Case below, **Then** captioning the literal sound (e.g., "[elevator
   bell chime]") is allowed, while captioning its narrative meaning (e.g., "[ominous reminder of
   his boss]") is not.

---

### Edge Cases

- What happens if a floor scene spec wants to reuse the same catalog entry more than once within
  one floor, or across two floors (e.g., two stuck clocks, or the same directory sign reappearing
  on Floor 51 and Floor 50)? → Allowed, provided every appearance stays consistent with that
  entry's fixed definition (FR-004) — e.g., a reused stuck clock shows the same frozen time
  everywhere it appears; a reused changing-name directory keeps changing in the same way, not a
  different way each time.
- What happens if zero catalog entries are used on a given floor? → Allowed (FR-002); the GDD is
  explicit that not every item needs to be used.
- What happens if an accessibility feature (captions, colorblind-safe highlighting) needs to
  describe a foreshadowing element for a player who can't perceive it via audio/visual alone? →
  Describing the literal sensory content (what is seen/heard) is allowed; describing or naming its
  narrative meaning is not (FR-006) — this is the same distinction User Story 3's Scenario 2
  checks at playtest.
- What happens if a future audio spec (`audio/002-dynamic-mix-state-machine`) wants the monster's
  real detection-state sound design to closely resemble the elevator-bell foreshadowing cue for
  gameplay-clarity reasons? → Cross-check against FR-005 before finalizing that spec — the
  catalog entry's intent is that the resemblance IS the foreshadowing (the player should half-
  register it as the elevator bell), so this is a case to confirm supports the catalog, not one
  that necessarily conflicts with it.
- What happens if the GDD is amended with new or removed foreshadowing ideas post-launch (see GDD
  Ch. 21, "Berapa banyak hint naratif yang benar-benar dipasang")? → This catalog must be amended
  first, citing what it supersedes (constitution Principle V), before any floor scene spec changes
  its selection.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: This catalog MUST contain exactly the eight entries listed in Key Entities below, all
  sourced from GDD Ch. 10.3. A floor scene spec (004/005/006) selecting a foreshadowing element
  MUST select from this list; introducing an uncataloged foreshadowing element requires amending
  this spec first, not inventing it directly in the floor spec.
- **FR-002**: A floor scene spec MAY select any subset of catalog entries for its floor, including
  none — not every entry needs to appear, and not every floor needs to use any (GDD Ch. 10.3:
  "Tidak perlu semua dipakai").
- **FR-003**: A floor scene spec MUST spread its selected entries (if any) across the three floors
  rather than concentrating the full set on a single floor, per GDD Ch. 10.3's "Sebar sebagian
  dari daftar ini di ketiga floor" — this spec does not mandate which entries go on which floor
  (that decision belongs to each floor scene spec), only that the distribution is spread, not
  bunched.
- **FR-004**: Any catalog entry used on more than one floor, or more than once on the same floor,
  MUST be portrayed identically to its fixed definition in Key Entities every time it appears (same
  frozen clock time, same footstep character, same laughter cue, etc.) — reinforcing the motif, not
  introducing a variant reading of it.
- **FR-005**: A catalog entry's assigned narrative intent MUST remain singular and MUST NOT be
  reused for an unrelated purpose elsewhere in the game (e.g., the elevator-bell-styled cue MUST
  NOT double as a generic UI sound).
- **FR-006**: No catalog entry, wherever placed, MAY be explained, labeled, or annotated via
  on-screen text, subtitle, tooltip, or dialogue that states its narrative meaning — it stays
  implicit, discovered only through play (GDD Ch. 10.3: "tidak pernah dijelaskan lewat teks").
  Captioning the literal sensory content of an entry for accessibility (e.g., "[phone ringing]")
  is not a violation of this rule; captioning or narrating its meaning is.
- **FR-007**: Every catalog entry used MUST remain consistent with, and MUST NOT contradict, the
  central thematic through-line defined in `001-eddie-character-bible` (the dream-office/real-
  office equivalence and the shrinking-life metaphor) — a foreshadowing element is one of the
  mechanisms that through-line is felt through, not a separate, independent piece of lore.
- **FR-008**: The three audio-category entries (Monster Sound / Elevator Bell, Monster Footsteps /
  Superior's Footsteps, Coworkers' Mocking Laughter) draw their sound assets from the "Naratif"
  category already required by GDD Ch. 14.1 — this catalog assigns narrative meaning to those
  assets; it does not introduce a second, separate audio-asset requirement outside that taxonomy
  (see `audio/001-sound-event-taxonomy`).
- **FR-009**: The decision of which specific catalog entries appear on Floor 52, Floor 51, or Floor
  50, and where spatially within each floor, is explicitly OUT of scope for this spec — that
  decision belongs to each floor's own scene spec: `specs/scenes/004-floor-52-scene/`,
  `specs/scenes/005-floor-51-scene/`, `specs/scenes/006-floor-50-scene/` (none written yet). This
  spec defines only the catalog and the rules in FR-001–FR-008 that any such placement must obey.

### Key Entities

Each entry below is a fixed catalog item: an ID, its GDD Ch. 10.3 source line (paraphrased), its
category, and the one-line narrative intent a designer or reviewer checks placements against.

- **FS-01 — Monster Sound / Elevator Bell** *(Audio)*: The monster's ambient/proximity sound
  resembles the office elevator bell. Intent: the building's ordinary machinery and the threat are
  the same sound, so safety and danger become indistinguishable — reinforcing FR-007's
  through-line.
- **FS-02 — Monster Footsteps / Superior's Footsteps** *(Audio)*: The monster's footsteps resemble
  a superior's (a boss's) footsteps. Intent: the thing hunting Eddie sounds like the person who
  makes his waking life unbearable.
- **FS-03 — Mocking Coworker Laughter** *(Audio)*: Coworkers' laughter that sounds mocking, heard
  faintly from empty rooms. Intent: Eddie's social alienation at work persists even when the office
  is otherwise abandoned — the laughter has no visible source.
- **FS-04 — Empty Meeting Room** *(Visual/Environmental)*: A meeting room furnished entirely with
  empty chairs. Intent: the appearance of collaborative work with no actual people present —
  isolation dressed as productivity.
- **FS-05 — Stuck Wall Clock** *(Visual/Environmental)*: A wall clock permanently stuck on one
  time. Intent: time inside the office has stopped for Eddie; his life outside it isn't moving
  forward either.
- **FS-06 — Unfinished Task Computer** *(Visual/Environmental)*: A powered-on computer display
  showing an unfinished task. Intent: work that is never actually done, no matter how much time
  passes — the workload FR-001 of `001-eddie-character-bible` describes, made visible.
- **FS-07 — Resignation Letter Printer** *(Visual/Environmental)*: A printer endlessly printing the
  same resignation letter. Intent: the wish to quit, repeating without ever resolving — foreshadows
  the Good Ending's actual resignation letter (GDD Ch. 10.4) without stating the connection.
- **FS-08 — Changing-Name Directory** *(Visual/Environmental)*: An office directory sign whose
  listed name for Eddie keeps changing. Intent: Eddie's identity is unstable/replaceable within the
  institution — he is not seen as a specific person by the place that consumes his life.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of foreshadowing props and sound cues implemented in any floor scene trace back
  to one of this catalog's eight entries (FS-01–FS-08) — zero ad hoc, uncataloged foreshadowing
  elements ship in the final build.
- **SC-002**: Zero on-screen text, subtitle, tooltip, or dialogue line across the shipped game
  states the narrative meaning of any foreshadowing element (FR-006), verified at each floor scene
  spec's own review and again at the Release scope-lock gate
  (`specs/release/001-scope-lock-and-cut-order/`).
- **SC-003**: Each floor scene spec (004/005/006) that selects one or more catalog entries cites
  this spec by path in its own Related section before that spec's Status leaves Draft.
- **SC-004**: Any catalog entry that appears on more than one floor, or more than once on a floor,
  is portrayed identically in 100% of its appearances (FR-004), confirmed by design review before
  each floor scene spec exits Draft.
- **SC-005**: At least 2 people outside the writing/design team, after playing all three floors in
  a single sitting, can name at least one foreshadowing element they noticed unprompted and
  describe it as feeling deliberate rather than random — without being told the catalog exists.

## Assumptions

- The eight entries are treated as exhaustive per GDD Ch. 10.3 as currently locked (v2.1). If a
  future GDD revision adds, removes, or changes an entry, this catalog is amended first
  (constitution Principle V) before any floor scene spec's selection changes.
- Per-floor selection — which entries, how many, and exactly where within a floor — is
  intentionally left undecided here; FR-009 assigns that decision to each floor scene spec once
  written, consistent with ROADMAP.md's own note that floor scenes are the "consumers" of this
  catalog, not this spec itself.
- The three audio-category entries assume GDD Ch. 14.1's "Naratif" sound asset list is produced by
  `audio/001-sound-event-taxonomy`; this spec assigns meaning to that asset list, it does not
  duplicate or replace its production requirement.
- This spec introduces no new `GameConfig` fields, no new `MonoBehaviour`, and no runtime system,
  matching its sibling `001-eddie-character-bible` — see that spec's tasks.md for the storage-
  location reasoning, which applies identically here (catalog content lives in this spec.md, not a
  separate `Assets/` companion file).
- This spec assumes `001-eddie-character-bible` exists first (ROADMAP.md §2 lists it as this
  spec's dependency) — FR-007 relies on that spec's Central Thematic Through-Line entity already
  being defined.

## Related

- [[Index|Specs Vault Index]] · [[ROADMAP]]
- [[constitution]] — governing principles this spec must comply with (Principle II: no runtime
  system introduced by a content-only spec; Principle V: amendment/supersession process)
- [[LILO-GDD-v2-Production-Lock]] — Ch. 10.3 (the foreshadowing list and placement rule this spec
  encodes as a catalog) and Ch. 14.1 (the "Naratif" audio asset list FS-01–FS-03 draw from)
- `specs/systems/narrative-content/001-eddie-character-bible/` — this spec's declared dependency
  (ROADMAP.md §2); FR-007 requires every catalog entry to stay consistent with that spec's Central
  Thematic Through-Line
- Future consumers (not yet written, cited here per ROADMAP.md §2's `narrative-content` row and
  §3's Scenes table): `specs/scenes/004-floor-52-scene/`, `specs/scenes/005-floor-51-scene/`,
  `specs/scenes/006-floor-50-scene/` — each owns the actual per-floor placement decision this spec
  leaves open (FR-009)
- `audio/001-sound-event-taxonomy`, `audio/002-dynamic-mix-state-machine` (both under
  `specs/systems/audio/`, not yet written) — own production and dynamic mixing of the three
  audio-category entries (FS-01–FS-03)
