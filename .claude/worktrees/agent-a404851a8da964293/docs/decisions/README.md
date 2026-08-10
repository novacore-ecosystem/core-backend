# Decisions (ADRs)

**Scope:** Architecture Decision Records — *why* a piece of the architecture looks the way it does. Not a binding rule (that's `docs/02-architecture-rules.md` or `docs/conventions/*.md`), not a step-by-step how-to (`docs/workflows/`), not a bug/gap write-up (`docs/tasks/`), not a multi-phase migration tracker (`docs/refactoring/`). If you're explaining a rule, document it elsewhere and link to the ADR for rationale; if you're tracking an in-progress change, that's a `refactoring/` tracker until it's done, then it may earn an ADR for the record.

## Conventions

- One decision = one file, `docs/decisions/<kebab-slug>.md`. No date or number prefix — ADRs are referenced by name, not by when they were written.
- Write an ADR when a decision is non-obvious enough that a future reader (human or AI) would otherwise re-litigate it or accidentally reverse it. Not every change needs one — routine feature work doesn't.
- Section shape, inferred from the two existing ADRs and used consistently:
  - **Problem** — what was wrong or missing before this decision, concretely (cite real files/duplication/bugs, not abstractly).
  - **Decision** — what was actually built/chosen, and the specific mechanism (entry points, where composition happens).
  - **Consequence** (as many subsections as needed) — non-obvious downstream effects, especially anything that looks like it violates a rule in `02-architecture-rules.md` at first glance but isn't, once you understand where the wiring actually happens.
  - **Known tradeoff** — accepted costs, and the condition under which they should be revisited (not "someday," a concrete trigger).
  - **Related follow-up work** (optional) — other changes made in the same effort, briefly, with links.
- Link liberally to the docs whose current shape resulted from this decision (`02-architecture-rules.md`, `03-building-blocks-reference.md`, `services/*.md`) — the ADR explains *why*, those docs state *what is true now*; don't duplicate the latter into the ADR.
- ADRs are not updated after the fact to match later changes — if a decision is later reversed or superseded, write a new ADR that says so and links back to the one it supersedes, rather than editing history.

## Index

- [event-messaging-refactor.md](event-messaging-refactor.md)
- [buildingblock-web-extraction.md](buildingblock-web-extraction.md)
