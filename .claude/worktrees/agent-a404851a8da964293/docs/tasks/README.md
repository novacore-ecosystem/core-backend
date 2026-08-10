# Tasks

**Scope:** Dated, per-task tracking for cross-cutting work items (bugs, gaps, feature requests) raised during a session — not architectural decisions (those belong in `decisions/`) and not step-by-step how-tos (those belong in `workflows/`). A task file is a grounded briefing: what was reported, what the code actually does today (file:line citations), and what's confirmed vs. still open. It does not need to contain a finished fix — investigation-only is a complete, valid task file.

## Structure

```
docs/tasks/
  README.md              <- this file
  PROGRESS.md            <- overall status across every date folder; short, one line per open task
  2026-07-22/
    Task1_<slug>.md       <- one file per task, numbered in the order raised that day
    Task2_<slug>.md
    ...
    PROGRESS.md          <- todo-list status for just this date's tasks (status / issue / caution)
  2026-07-23/
    ...
```

## Conventions

- One task = one file. Filename: `Task<N>_<kebab-slug>.md`; `N` restarts at 1 in each new date folder.
- A task file should state: the report as given (verbatim request/response payloads if any were provided), the grounded investigation (exact file:line citations, not paraphrase), and an explicit "open questions / not yet confirmed" section rather than papering over gaps.
- Cross-reference the paired frontend task (NovaCoreUI's `docs/tasks/<date>/TaskN_*.md`) when a task originates from or affects the other repo — link it by relative description, both repos are siblings under `workspace/projects/`.
- Update the date folder's `PROGRESS.md` whenever a task's status changes. Keep it a todo list, not prose.
- Update the top-level `PROGRESS.md` at the end of a session: one line per still-open task across all dates. No per-task detail — that belongs in the task file itself.
- Once every task in a date folder is Done, leave the folder in place as a historical record; mark it closed in the top-level `PROGRESS.md` rather than deleting it.
