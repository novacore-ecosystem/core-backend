# Refactoring

**Scope:** living trackers for framework-level, multi-phase migrations that span multiple services or BuildingBlocks — too large for a single `tasks/` entry, not yet settled enough to be a `conventions/` standard or a `decisions/` ADR. One file per migration, updated continuously while the migration is in flight.

## Conventions

- One migration = one file: `<slug>-refactor-plan.md`.
- Each file states: why (what gap it closes), current architecture, target architecture, phase-by-phase breakdown with a progress checklist, a risk register, and architectural decisions made along the way (including corrections discovered mid-migration).
- Once the migration completes, the file stops being actively maintained and stays as historical record (same convention as [08-migration-plan.md](../08-migration-plan.md)) — the resulting binding standard belongs in `conventions/` instead, cross-linked from the top of the tracker file.
- Update the tracker's checklist and risk register after every phase, in the same commit as that phase's code change — not deferred to a catch-up pass.

## Active

- [persistence-refactor-plan.md](persistence-refactor-plan.md) — Read/Write persistence-service layer migration across all 7 services.
