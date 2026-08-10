# AI Framework Foundation

**Scope:** entry point for this framework. Read this first if you're new to it or about to extend it. This document explains how the pieces relate and how to add to each one — it does not restate their content.

## What this is

A foundation layer that makes future AI-driven work on NovaCore deterministic instead of free-form: before writing or changing anything, the relevant Pattern (why), Template (what shape), and Rules (what's binding) are looked up, not re-derived from reading source code cold. It exists because `docs/` already holds nearly everything needed — this framework's job is to index it precisely enough that an AI command loads the minimum necessary and never guesses at something already decided.

11 Skills and 1 non-Skill utility command now exist on top of this foundation — see `INDEX.md` (the Command Registry) for the full list, and "Relationship to Skills" and "Commands & the Command Registry" below.

## How the pieces fit together

```
docs/**                        <- the actual source of truth (architecture, conventions, templates, workflows, ADRs)
        ^
        | indexed by (pointers, not copies)
        |
.claude/framework/
  reading-contracts.md         <- the Required/Optional/Forbidden mechanism itself
  pattern-library.md           <- indexes docs/conventions/* + docs/06 by construct, adds philosophy + Reading Contract per construct
  template-library.md          <- indexes docs/06-implementation-templates.md by construct, flags real gaps
  rules-library.md             <- indexes docs/{02,04,conventions,reference,testing}/* by rule category, flags real gaps
  decisions.md                 <- points to docs/decisions/, states the "check ADRs before architecture changes" enforcement hook
        ^
        | is the spec for
        |
  workflow-contract.md         <- the execution lifecycle every future Skill runs (7 stages)
  command-contract.md          <- the required fields every future SKILL.md must declare
```

Nothing in `.claude/framework/*-library.md` duplicates `docs/` content. If you find yourself about to paste rule text or template code into one of these files, stop — add a link instead, or extend the `docs/` file if the content genuinely doesn't exist yet.

## How to extend each library

**Add a new Pattern** (a construct not yet in `pattern-library.md`):
1. Confirm `docs/conventions/*.md` or `docs/reference/*.md` actually documents the philosophy — if not, that's a `docs/` gap to raise first, not something to invent here.
2. Add a section to `pattern-library.md` in the existing fixed shape (Intent / Reading Contract / Philosophy / Template / Real example).
3. If no template exists yet, point to `template-library.md`'s gap list instead of leaving the field blank.

**Add a new Template** (a literal file shape not yet covered):
1. Add the actual template code to `docs/06-implementation-templates.md` — that file is the one source of template code, never `.claude/framework/`.
2. Add or move its row in `template-library.md`'s Index table; remove it from the Gaps table if it was there.

**Add a new Rule category or fill a gap**:
1. Write the rules doc where it belongs by responsibility — most rule categories already have a natural home (`docs/02-architecture-rules.md`, `docs/04-coding-rules.md`, `docs/conventions/*.md`, `docs/reference/*.md`); a genuinely new category may need a new `docs/reference/*.md` file.
2. Ground it in what the codebase actually does today (cite real files), not in general best practice — the whole point of this framework is stopping invented conventions.
3. Move the category from `rules-library.md`'s Gaps table to its Index table pointing at the new doc.

**Add a new Decision Record**:
1. Follow `docs/decisions/README.md`'s convention directly — `decisions.md` here is just a pointer and needs no edit unless the enforcement hook itself changes.

## Relationship to Skills

11 Skills now exist under `.claude/skills/` — see `INDEX.md` for the full list with triggers and doc links. 10 of them (`commit`, `cleanup`, `prune`, `implement`, `complete`, `align`, `sync`, `review`, `verify`, `inspect`) satisfy `command-contract.md`'s required field list (each as a superset — they add Responsibilities/Rules/Limitations/Examples/Future Extension Notes on top of the required fields, never fewer). `scaffold` still uses the original, pre-`command-contract.md` ad hoc format (Trigger/Context Loading/Execution Workflow/Templates & Docs Used/Validation Checklist/Output Contract/Stop Conditions/Boundaries) and has not yet been reconciled — that's the one remaining piece of this deferred work.

A new Skill:
- Its `SKILL.md` must satisfy `command-contract.md`'s field list.
- Its execution must follow `workflow-contract.md`'s 7 stages.
- Its Reading Contract field must load from the Pattern/Template/Rules libraries here rather than re-deriving equivalents from raw `docs/` exploration.
- It registers in exactly one place: `INDEX.md` — see "Commands & the Command Registry" below.

`clean` (session 1) was archived to `.claude/skills/_archive/clean/`, superseded by `align`; see `INDEX.md`'s "Archived" section.

## Commands & the Command Registry

`.claude/framework/INDEX.md` is the single source of truth for every AI command — both Skills and non-Skill utility commands (`.claude/commands/*.md`, currently just `skills`). The `/skills` command (`.claude/commands/skills.md`) reads only that registry table, plus — for `/skills <command>` — the one target command's own doc. It performs discovery only: no source-code inspection, no repository analysis, no engineering advice, no Skill execution.

- **Register a new command:** add one row to `INDEX.md`'s table (Command, Category, Trigger, Description, Documentation) at the same time you add the underlying `SKILL.md` or `.claude/commands/*.md` file. One registration point — the table row and the file must always exist together.
- **Rename a command:** update the row (name, Trigger, Documentation link) and rename the underlying file/folder in the same change. There's no alias layer — the registry name and the file path always match.
- **Deprecate a command:** Skills move to `.claude/skills/_archive/` and get a line under `INDEX.md`'s "Archived" section (history preserved); non-Skill utility commands are just deleted (no archive — they carry no framework-loading history worth preserving).
- **Categories:** a command's `Category` column value is the only thing that determines its grouping in `/skills`' output — introduce a new category only when a real command needs it; nothing else needs updating for a new category to appear correctly in the help output.
- **How Help uses the registry:** `/skills` (no argument) reads only the registry table and prints it grouped by Category. `/skills <command>` looks up one row, then reads only that row's Documentation file to extract Purpose/Supported syntax/Typical workflow/Examples/Related commands — see `.claude/commands/skills.md` for the exact extraction rules and output shape.
