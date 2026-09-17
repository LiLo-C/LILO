# LILO — Specs Vault (Unity, scene-based, one spec per small feature)

**Reset 2026-09-17.** The previous vault (17 GDD-phase-driven features) is scrapped. This vault
splits every small feature — system-level or scene-level — into its own spec folder. See
[ROADMAP.md](./ROADMAP.md) for the full GDD → spec traceability table, dependency graph, and
shared naming conventions every spec below follows.

## Governance & source of truth

- [[constitution]] — project principles (spec-driven development, simplicity, Unity architecture, testing, versioning)
- [[LILO-GDD-v2-Production-Lock]] — full Game Design Document, source of truth for every spec below

## Systems (`specs/systems/<category>/<NNN>-<feature>/`)

Reusable engineering, grouped by category folder. No single Scene owns these; scenes reference
and configure them. See ROADMAP.md §2 for the full list (16 categories, ~47 feature specs).

## Scenes (`specs/scenes/<NNN>-<scene>/<feature>/`)

One category folder per Unity Scene asset; each contains the small-feature specs that make up
that scene's content. See ROADMAP.md §3 for the full list (8 scenes, ~23 feature specs).

## Release (`specs/release/<NNN>-<feature>/`)

Process/QA gates that aren't runtime code. See ROADMAP.md §4 (3 feature specs).
