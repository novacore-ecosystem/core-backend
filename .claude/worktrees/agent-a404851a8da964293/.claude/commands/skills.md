---
description: List available AI commands, or show detail for one — a discovery utility, not a Skill
argument-hint: [command]
---

# /skills — Command Help

This is a developer utility, not an AI Skill: it performs command discovery only. It never inspects source code, never analyzes the repository, never loads documentation unrelated to the one thing it's showing, and never executes a Skill. Its entire job is reading `.claude/framework/INDEX.md` (the Command Registry) and, only when a specific command is named, that one command's own doc file.

Argument passed: `$ARGUMENTS`

## Behavior

**If `$ARGUMENTS` is empty** (`/skills`):

1. Read only `.claude/framework/INDEX.md`'s registry table (the `| Command | Category | Trigger | Description | Documentation |` table). Do not read anything else — not the "Full dependency detail," "Shared config," "Suggested composition," or "Maintenance" sections, none of that is discovery output.
2. Group rows by their `Category` column, preserving the category order they first appear in the table (currently: Git & Workspace, Development, Inspection, Utilities). Skip any category with zero rows — never print an empty category heading. Sort commands alphabetically by name within each category.
3. Map each category to an emoji (fixed list — a category not in this list gets no emoji, just the heading text):

   | Category | Emoji |
   |---|---|
   | Git & Workspace | 🛠 |
   | Development | 💻 |
   | Inspection | 🔍 |
   | Knowledge | 📚 |
   | Utilities | ⚙️ |

4. Print in exactly this shape, nothing added — one `##` heading per non-empty category, one `-` bullet per command, description condensed to a single concise sentence:

```md
# Available AI Commands

Quick reference for all supported project commands.

---

## <emoji> <Category>

- `/<command>` — <Description, condensed to one sentence>
- `/<command>` — <Description, condensed to one sentence>

## <emoji> <Category>

- `/<command>` — <Description, condensed to one sentence>

---

Tip:
Run `/skills <command>` to display detailed documentation and examples for a specific command.
```

5. Do not add commentary, explanations, or a summary beyond the Tip line. This output is the entire response.

**If `$ARGUMENTS` is a command name** (`/skills implement`, `/skills commit`, ...):

1. Look up `$ARGUMENTS` (strip a leading `/` if present) against the registry table's `Command` column. Exact match only — no fuzzy matching.
2. **If not found:** respond with exactly: `"<name>" is not a registered command. Run /skills to list all available commands.` — do not guess a close match, do not print the full list automatically.
3. **If found:** open only that one row's `Documentation` file (a `SKILL.md` or, for `skills` itself, this file) — no other file. Extract and present:

```
/<command>

Purpose
<one or two sentences — from the doc's own Purpose section>

Supported syntax
<the Trigger column value, plus the doc's Supported Commands/Input section if it lists more
than one invocation shape>

Description
<the registry's Description column, verbatim>

Typical workflow
<condensed from the doc's Execution/Inspection Workflow section — the key steps only, not a
verbatim reproduction of every sub-bullet>

Examples
<one or two representative examples from the doc's own Examples section, condensed>

Related commands
<same-category siblings from the registry, plus anything named adjacent to this command in
INDEX.md's "Suggested composition" section>
```

4. Never print the target doc's internal sections verbatim beyond what's needed for the fields above — no Reading Contract tables, no Boundaries/Limitations/Failure Conditions/Success Criteria, no Rules. Those are implementation detail for the Skill itself to follow, not discovery output for a developer.
5. Keep the whole response concise — this is a lookup, not a tutorial.

## Constraints (always, both modes)
- Never read any file under `docs/`, `src/`, or `tests/`.
- Never read more than: the registry table (`INDEX.md`), and — only in detail mode — the one target command's own doc file.
- Never generate engineering advice, opinions, or recommendations about the codebase.
- Never invoke a Skill, even the one being described.
- Never modify any file.
