# Layer & Service Boundaries

**Scope:** the canonical MUST-NOT-read table for every code-touching Skill (`clean`, `complete`, `scaffold`, `sync`). Skills link here instead of restating boundaries — add a row here, and every skill that references this file inherits it without being edited.

Full rationale for these boundaries lives in `docs/02-architecture-rules.md`; this file only restates them as a lookup table for skill execution, it does not redefine them.

| Working in... | May read | Must NOT read |
|---|---|---|
| `*.Domain` | The entity/aggregate itself, its Value Objects, `docs/conventions/domain-coding-conventions.md` | Persistence (`*.Persistence`), API, Infrastructure, other services |
| `*.Persistence` | Its own DbContext, entity configs, repositories, `docs/conventions/persistence-coding-conventions.md` | Domain business-rule internals beyond the entity's public surface, API layer, UI, other services |
| `*.Application` (CQRS handlers) | Its own Feature folder, the Read/Write service interfaces it depends on, `docs/conventions/application-coding-conventions.md` | Other Features' internals unless explicitly composing them, other services |
| `*.API` (endpoints) | Its own endpoint + the command/query it dispatches, `docs/04-coding-rules.md` (Endpoints section) | Other services' endpoints, unrelated Features |
| `*.Infrastructure` (consumers/jobs) | Its own consumer/job + the command it dispatches, `docs/reference/events.md` or `docs/reference/saga.md` as applicable | Business logic — infrastructure adapters translate and dispatch, they never decide |
| Cross-service `flow` work | Only the services actually named in the flow, `docs/01-architecture-map.md` | Any service not part of the named flow |
| Frontend (`NovaCoreUI`) | Never, from any backend skill in this framework | N/A — out of scope entirely; see `novacoreui_companion_project` memory for cross-repo contract changes |

## Rule of thumb
If a skill's Context Loading section for a given invocation would require a row not covered above, that's a doc gap — report it, don't expand scope silently (see `shared-rules.md` §1).
