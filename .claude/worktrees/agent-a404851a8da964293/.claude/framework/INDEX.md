# Command Registry

**Scope:** the single source of truth for every AI command in this project — both Skills (`.claude/skills/*/SKILL.md`) and non-Skill developer utilities (`.claude/commands/*.md`). Every command registers here in exactly one place. The `/skills` help command (`.claude/commands/skills.md`) reads only this file (and, for `/skills <command>`, the one target command's own doc) — nothing else.

Adding a new Skill means adding one row here plus a new `.claude/skills/<name>/SKILL.md` — never edit an existing Skill's file to add another. Adding a non-Skill utility command follows the same rule against `.claude/commands/`.

| Command | Category | Trigger | Description | Documentation |
|---|---|---|---|---|
| commit | Git & Workspace | `/commit [message]` | Create a validated Conventional Commit from staged changes. | [skills/commit/SKILL.md](../skills/commit/SKILL.md) |
| cleanup | Git & Workspace | `/cleanup` | Group uncommitted changes into logical commit sets. | [skills/cleanup/SKILL.md](../skills/cleanup/SKILL.md) |
| prune | Git & Workspace | `/prune` | Detect dead code in current changes; auto-remove only SAFE items. | [skills/prune/SKILL.md](../skills/prune/SKILL.md) |
| implement | Development | `/implement <target> <Name>` | Generate a complete, production-ready implementation of a construct. | [skills/implement/SKILL.md](../skills/implement/SKILL.md) |
| complete | Development | `/complete file\|selection` | Finish partially written code from the matching pattern/template. | [skills/complete/SKILL.md](../skills/complete/SKILL.md) |
| align | Development | `/align <target> [Name]` | Restructure existing code to already-current standards, never behavior. | [skills/align/SKILL.md](../skills/align/SKILL.md) |
| sync | Development | `/sync <target> [Name]` | Catch an implementation up to patterns/rules that changed since it was written. | [skills/sync/SKILL.md](../skills/sync/SKILL.md) |
| scaffold | Development | `/scaffold api <Name>` | Generate feature boilerplate, TODO-marked logic. | [skills/scaffold/SKILL.md](../skills/scaffold/SKILL.md) |
| review | Inspection | `/review` | Engineering-quality review of the current diff — scored, holistic. | [skills/review/SKILL.md](../skills/review/SKILL.md) |
| verify | Inspection | `/verify <target> [Name]` | Production-readiness compliance check, severity-classified. | [skills/verify/SKILL.md](../skills/verify/SKILL.md) |
| inspect | Inspection | `/inspect <aspect> [target]` | Single-aspect deep dive (performance, security, caching, ...). | [skills/inspect/SKILL.md](../skills/inspect/SKILL.md) |
| skills | Utilities | `/skills [command]` | List available AI commands, or show detail for one. | [commands/skills.md](../commands/skills.md) |

`skills` is the one row above that is **not** a Skill (`.claude/skills/`) — it's a plain Claude Code custom command (`.claude/commands/`), a deterministic developer utility with no framework-loading or code-touching behavior. It's registered here anyway because this file is the source of truth for *every* command, not just Skills.

**Knowledge category (not yet implemented):** `/knowledge`, `/pattern`, `/adr` are anticipated but do not exist yet — no rows for them exist here, and `/skills` will not list a category with no commands in it. Adding them is a future task; see "How to register a new command" in `FRAMEWORK.md`.

## Full dependency detail (Skills only)
The table above is the discovery-facing view. Each Skill's own `docs/` dependencies (what it reads, not just what it does) live in its `SKILL.md`'s Reading Contract section — not restated here, to avoid two places drifting out of sync. If you need "what does `X` read," open `skills/X/SKILL.md`, don't infer it from this table.

## Archived
- `clean` — superseded 2026-08-03 by `align`, which covers the same "bring code to standard without changing behavior" responsibility with a fuller target list and the same reuse of `workflows/refactor-existing-code.md`'s checklists. Moved to `.claude/skills/_archive/clean/`.

## Shared config (every Skill above links into these, never copies them)
- [shared-rules.md](shared-rules.md) — cross-skill execution contract: context discipline, pattern-first, stop conditions, output discipline, git rules, selection-mode contract
- [boundaries.md](boundaries.md) — canonical per-layer/service MUST-NOT-read table
- [change-classification.md](change-classification.md) — Feature/Bug Fix/Refactor/... taxonomy (used by `cleanup`, `review`)
- [checklists/commit-message-checklist.md](checklists/commit-message-checklist.md) — the one net-new checklist (used by `commit`)
- [pattern-library.md](pattern-library.md) / [template-library.md](template-library.md) / [rules-library.md](rules-library.md) — the philosophy/shape/rules layers `implement`, `complete`, `align`, `sync`, `verify`, and `review` all load from instead of re-deriving conventions independently
- [inspection-output-standard.md](inspection-output-standard.md) — the shared report structure + Critical/High/Medium/Low/Suggestion severity model for `verify` and `inspect`
- [engineering-scoring.md](engineering-scoring.md) — the shared ten-dimension scoring model, primarily used by `review`

## Suggested composition
These skills compose by sequential invocation, never by one Skill instructing another:
`/scaffold` (skeleton, TODO logic) or `/implement` (full construct) → `/complete` (finish anything left) → `/prune` (catch leftovers) → `/review` (engineering quality) → `/verify` (compliance gate) → `/cleanup` (if changes ended up mixed) → `/commit`.
`/align` (fix drift from unchanged standards) and `/sync` (catch up to standards that moved) are typically standalone, run before starting new work on a module — never both for the same reason on the same code; see each skill's Purpose for which one applies.
`/inspect` is a standalone, single-aspect deep dive — run it any time a specific question ("is the caching here right?") doesn't warrant a full `/verify` sweep.
`/skills` isn't part of this pipeline — it's a lookup tool, run any time.

## Maintenance
- New Skill → new folder + one row here (Category, Trigger, Description, Documentation). Zero edits to existing skill files.
- New non-Skill utility command → new file under `.claude/commands/` + one row here, `Category = Utilities` (or a new category if it doesn't fit an existing one).
- Rename a command → update its row here (name + Trigger + Documentation link) and rename its underlying file/folder in the same change — this table's command name and the actual file path must always match; there is no alias layer.
- Deprecate a command → move it to `.claude/skills/_archive/` (Skills) or delete the file (utility commands, which don't carry the same historical weight), remove its row here, and add one line under "Archived" (Skills only — a deleted utility command just disappears from the table).
- New Category → only introduce one if at least one real command needs it; `/skills`' category grouping in its output is derived directly from this table's Category column, so a new category here appears automatically in the help output with no change needed to `.claude/commands/skills.md` itself.
- New boundary → one row in `boundaries.md`. Consuming skills inherit it automatically via their existing link — no per-skill edits.
- New implementable/completable/alignable/syncable/verifiable construct → add it to `pattern-library.md` (and `template-library.md` if it has a literal shape) — `implement`/`complete`/`align`/`sync`/`verify` pick it up via their Reading Contracts without being edited.
- New `/inspect` aspect → one row in that skill's own Reading Contract table; mark it a gap explicitly if `rules-library.md` doesn't yet cover it.
- If `docs/` gains a new numbered doc/workflow that a skill should depend on, update that skill's own Reading Contract section — this table doesn't restate per-skill `docs/` dependencies (see "Full dependency detail" above), so no edit is needed here for that kind of change.
