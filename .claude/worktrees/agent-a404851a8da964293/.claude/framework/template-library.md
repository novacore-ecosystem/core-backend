# Template Library

**Scope:** the file-shape layer — literal copy-paste starting points. Distinct from `pattern-library.md` (why a construct is shaped this way). All real templates already live in `docs/06-implementation-templates.md`; this file is only an index into it plus an honest gap list. Do not duplicate the template code here — link to it.

## Index

| Template | Location |
|---|---|
| Command + Handler + Validator | `docs/06-implementation-templates.md` — "Command + Handler + Validator" |
| Query + Handler | `docs/06-implementation-templates.md` — "Query + Handler" |
| Carter Endpoint | `docs/06-implementation-templates.md` — "Carter Endpoint" |
| Repository + Read/Write persistence service | `docs/06-implementation-templates.md` — "Repository + Read/Write persistence service" |
| Domain entity | `docs/06-implementation-templates.md` — "Domain entity" |
| Integration event (publish side) | `docs/06-implementation-templates.md` — "Integration event (publish side)" |
| Integration event (consume side) | `docs/06-implementation-templates.md` — "Integration event (consume side)" |
| Background job | `docs/06-implementation-templates.md` — "Background job" |

## Gaps (no literal template exists yet — do not invent one silently)

| Template | Nearest existing coverage | Note |
|---|---|---|
| DTO | Inline `{Verb}Result` / `{Verb}Request` records shown inside the Command/Query/Endpoint templates | No standalone DTO shape exists separate from a specific Command/Query/Endpoint — that may be intentional (DTOs are always feature-specific) rather than a real gap; flag for confirmation if a Skill ever needs a DTO with no owning Command/Query |
| Configuration (`IEntityTypeConfiguration<T>`) | Referenced by path only (`{Service}.Persistence/Config/{Entity}Config.cs`) in the Domain entity section, no literal code shown | Open a real `*Config.cs` file for the target service before writing one |
| Mapping | Prose policy in `docs/04-coding-rules.md` (Mapping section) — hand-map, Mapster registered but unused | No literal template needed beyond what's already inline in the Query + Handler template |
| Saga | `docs/reference/saga.md` (building block description) + `docs/reference/create-order-saga.md` (one real worked example) | No copy-paste skeleton — the one real usage is the closest thing to a template; read it directly rather than abstracting a template from a single instance |
| Value Object | Described in `docs/conventions/domain-coding-conventions.md`, no standalone template | Same shape as a Domain entity's private-constructor + static factory pattern, without identity/`BaseEntity<Guid>` |
| Caching decorator | Described in `docs/reference/caching.md` + `docs/04-coding-rules.md`, no literal template | Read the cited example in `reference/caching.md` directly |
| Search | `docs/reference/search.md` | No copy-paste skeleton — read the cited example directly; the pattern's binding constraint (search stays inside the owning service's Persistence layer, per-aggregate) matters more than any literal shape |
| Domain Service | `docs/conventions/domain-coding-conventions.md` | No example exists yet in this codebase — before templating one, confirm the logic can't live on an aggregate/entity method instead (see `pattern-library.md`) |

## Maintenance
When a genuine gap above gets filled with a real template, add it to `docs/06-implementation-templates.md` (the single source of truth for template code) and move its row from Gaps to Index here — never add template code directly to this file.
