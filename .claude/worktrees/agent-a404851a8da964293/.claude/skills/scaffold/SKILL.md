---
name: scaffold
description: Generate the boilerplate skeleton for a new NovaCore feature (endpoint through persistence), business logic left as TODOs
---

## Purpose
Generate only the boilerplate required for a new feature — command/query, handler, validator, DTO, endpoint, mapping, persistence interface/implementation, DI registration. Business logic is left as TODO markers, never invented.

## Trigger
`/scaffold api <Name>` — e.g. `/scaffold api CreateWarehouse`. (Other scopes — `domain`, `repository`, `integration-event`, `background-job` — follow the same mechanism against the matching `workflows/add-*.md` doc; `api` is the primary/default scope since it typically composes the others.)

## Context Loading
- **MUST read:** `docs/06-implementation-templates.md`, the matching `docs/workflows/add-*.md` for the requested scope (`add-new-api.md` for `api`, plus `add-new-domain-entity.md` / `add-new-repository.md` / `add-integration-event.md` / `add-background-job.md` only if the feature genuinely needs a new entity/repo/event/job, not by default), the target service's `docs/services/*.md`
- **MUST NOT read:** other services, unrelated existing features

## Execution Workflow
1. Parse `<Name>` and the target service (ask if not inferable from context).
2. Load `docs/workflows/add-new-api.md` — treat its checklist as the literal generation sequence, in order.
3. For each artifact the checklist calls for (Command/Query, Handler, Validator, Endpoint, Request/Result records, Read/Write service interfaces if new, EF config if new), pull the exact starting shape from `06-implementation-templates.md`.
4. Open the template doc's cited real reference file for this service (e.g. `Register.cs`, `CreateUser.cs`) and mirror its current actual shape, not the possibly-stale template prose.
5. Replace placeholders (`{Service}`, `{Feature}`, `{Entity}`, `{Verb}`) with real names.
6. Leave every business-rule decision as `// TODO: <what needs deciding>` — never fabricate validation rules, field lists, or business logic.
7. List the DI registration this requires (Scrutor auto-scan vs. explicit `AddScoped`, per the templates doc's notes) as a reminder, and add it if the target pattern requires explicit registration.
8. If no template exists for something the checklist calls for, stop and report the doc gap rather than freehanding a new shape.

## Templates & Docs Used
`docs/06-implementation-templates.md` (literal starting text for every artifact), `docs/workflows/add-new-api.md` (sequence/checklist), target `docs/services/*.md` (routes/ports/conventions specific to that service).

## Validation Checklist
- [ ] Every artifact the workflow checklist calls for was created.
- [ ] No business logic was invented — every decision point is a TODO.
- [ ] Placeholders fully replaced, no `{Service}`/`{Verb}` literals left in generated code.
- [ ] DI registration reminder given, and applied where the pattern requires explicit registration.

## Output Contract
List of files created, each annotated with its TODO markers, plus the DI registration note.

## Stop Conditions
- Target service ambiguous or unspecified.
- The workflow checklist calls for an artifact type with no matching template — report the gap, don't invent the shape.

## Boundaries
- Never writes business logic — TODO markers only.
- Never touches existing features — additive only.
