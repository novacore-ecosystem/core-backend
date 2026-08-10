---
name: clean
description: Production-grade layer-scoped refactor of a specific NovaCore domain entity, persistence aggregate, API feature, or cross-service flow
---

## Purpose
Refactor an existing piece of code to match this repo's documented conventions, loading only the docs relevant to the named layer.

## Trigger
`/clean <layer> <target>` where `<layer>` ∈ `api`, `domain`, `persistence`, `flow`. Examples: `/clean api CreateProduct`, `/clean domain Product`, `/clean persistence Inventory`, `/clean flow Checkout`.

## Context Loading
Only the row matching `<layer>` loads — never load another row's docs.

| layer | MUST read | MUST NOT read |
|---|---|---|
| `api` | `docs/02-architecture-rules.md`, `docs/04-coding-rules.md`, `docs/conventions/application-coding-conventions.md`, the target service's `docs/services/*.md`, `docs/workflows/refactor-existing-code.md` | other services, unrelated features' Domain internals |
| `domain` | `docs/02-architecture-rules.md`, `docs/conventions/domain-coding-conventions.md`, `docs/06-implementation-templates.md` (Domain entity section), `docs/workflows/refactor-existing-code.md` | Persistence/API/Infrastructure code beyond the entity's own EF config |
| `persistence` | `docs/conventions/persistence-coding-conventions.md`, `docs/04-coding-rules.md` (Repository/Transaction sections), `docs/workflows/refactor-existing-code.md`, `docs/reference/inbox-outbox-runtime.md` (only if the target touches Outbox) | Domain business rules beyond public surface, API/UI |
| `flow` | `docs/01-architecture-map.md`, each involved service's `docs/services/*.md`, `docs/02-architecture-rules.md` | services outside the named flow |

Also see `../../framework/boundaries.md` for the general per-layer table this specializes.

## Execution Workflow
1. Parse `<layer>` and `<target>` from the trigger. If `<layer>` isn't one of the four, or `<target>` is ambiguous (matches multiple entities/features across services), stop and ask.
2. Locate the target's actual source files via targeted search (grep for the type/feature name) — never a full-repo scan.
3. Load only the docs in the matching row above.
4. Diff the current implementation against the loaded convention doc(s); list every violation found (not just the first one).
5. Refactor to close each violation, preserving external behavior.
6. Run `docs/workflows/refactor-existing-code.md`'s existing checklists as the validation gate: Safety checklist, SOLID checklist, Reuse checklist, Regression checklist — reuse those checkbox lists verbatim, don't recreate them here.
7. Present the diff plus the violation list plus checklist pass/fail.

## Templates & Docs Used
`docs/06-implementation-templates.md` (domain layer only, for the correct target shape) and whichever convention doc matches the layer row above.

## Validation Checklist
`docs/workflows/refactor-existing-code.md`'s Safety / SOLID / Reuse / Regression checklists — run all four regardless of layer.

## Output Contract
Diff (changed code only) + violation list (what was wrong, mapped to the convention doc section it violated) + the four checklist results (pass/fail per item).

## Stop Conditions
- Ambiguous target.
- No source found for the named target.
- The refactor would require touching a layer outside the matched row (e.g. a `domain` clean that seems to require Persistence changes) — stop and report that the target needs a broader `/clean` invocation or a separate one per layer.

## Boundaries
- Never performs architectural redesign — only brings existing code in line with already-documented conventions.
- Never touches layers outside the matched row, even if a violation is "obviously" related.
