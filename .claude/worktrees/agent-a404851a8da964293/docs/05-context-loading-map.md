# Context Loading Map

**Scope:** read this before starting any implementation task. It tells you the exact, minimal set of documents to read for your task type — nothing else. Do not explore the repository beyond what's listed unless the docs below explicitly point you to a source file. If a workflow doc names a "target service README," that means the relevant file in `docs/services/`, not a search of the source tree.

**Rule of thumb:** every row below is a ceiling, not a floor — if the task is trivial (e.g. a one-line copy fix), read less. If you find yourself needing to open files not listed here, the gap is a documentation bug — note it, then proceed.

## By task type

| Task | Read (in order) | Do NOT read |
|---|---|---|
| **New API endpoint on existing service** | [02-architecture-rules.md](02-architecture-rules.md), [04-coding-rules.md](04-coding-rules.md), [conventions/application-coding-conventions.md](conventions/application-coding-conventions.md), target service doc (`services/auth-service.md` or `services/user-service.md`), [workflows/add-new-api.md](workflows/add-new-api.md), [06-implementation-templates.md](06-implementation-templates.md) (command/query + endpoint templates) | Other services' docs, `reference/*` unless the feature touches caching/events/grpc |
| **New domain entity** | [02-architecture-rules.md](02-architecture-rules.md), [04-coding-rules.md](04-coding-rules.md), [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md), [workflows/add-new-domain-entity.md](workflows/add-new-domain-entity.md), [06-implementation-templates.md](06-implementation-templates.md) | Workflow library beyond this one workflow |
| **New repository** | [04-coding-rules.md](04-coding-rules.md#repository--unit-of-work), [workflows/add-new-repository.md](workflows/add-new-repository.md), [06-implementation-templates.md](06-implementation-templates.md) (repository template) | Full building-blocks reference — you only need the Application/Persistence rows |
| **New integration event (publish or consume)** | [reference/events.md](reference/events.md), [workflows/add-integration-event.md](workflows/add-integration-event.md), [06-implementation-templates.md](06-implementation-templates.md) (integration event template) | `reference/saga.md`, `reference/grpc.md` |
| **New background job** | [workflows/add-background-job.md](workflows/add-background-job.md), [06-implementation-templates.md](06-implementation-templates.md) (background job template), `services/auth-service.md` (only if you need the Hangfire dashboard/queue example — Auth is the only current consumer) | Everything else |
| **Fix a bug** | [workflows/fix-bug.md](workflows/fix-bug.md), [02-architecture-rules.md](02-architecture-rules.md#exception-rule), affected service doc | Unrelated services, workflow library beyond fix-bug |
| **Refactor existing code** | [workflows/refactor-existing-code.md](workflows/refactor-existing-code.md), [04-coding-rules.md](04-coding-rules.md), [07-solid-recommendations.md](07-solid-recommendations.md) (only the section matching the code area) | New-feature workflows |
| **Performance investigation** | [workflows/performance-optimization.md](workflows/performance-optimization.md), [reference/caching.md](reference/caching.md) (if caching is a candidate fix) | Coding-rules/templates — this is investigation, not new code |
| **Production incident** | [workflows/production-incident.md](workflows/production-incident.md), [troubleshooting/seq.md](troubleshooting/seq.md), affected service doc | Architecture docs unless root cause turns out to be architectural |
| **Onboarding / understand a service quickly** | [workflows/project-initialization.md](workflows/project-initialization.md) → [01-architecture-map.md](01-architecture-map.md) → target service doc | Reference docs (load on demand as the service doc links to them) |
| **Scaffold a brand-new service** | [workflows/new-service-scaffold.md](workflows/new-service-scaffold.md) (this workflow itself sequences everything else you need) | Nothing extra — the workflow is self-contained |
| **Add caching to a service/feature** | [reference/caching.md](reference/caching.md), [04-coding-rules.md](04-coding-rules.md#caching--decorator-pattern) | — |
| **Add/modify authorization on an endpoint** | [reference/authorization.md](reference/authorization.md) | — |
| **Docker/env/deployment change** | [setup/docker.md](setup/docker.md), [setup/environment-config.md](setup/environment-config.md) | Architecture/coding docs |
| **Writing/updating tests** | [testing/TestingGuidelines.md](testing/TestingGuidelines.md), [testing/TestingArchitecture.md](testing/TestingArchitecture.md), [testing/TestingRoadmap.md](testing/TestingRoadmap.md) (only if deciding what to test next, not how) | `testing/TestingProgress.md` unless resuming the testing initiative itself |

## By document (what triggers reading it)

Use this if you already know which fact you need rather than which task you're doing.

| Document | Load when you need to know... |
|---|---|
| `01-architecture-map.md` | The big picture — services, BuildingBlocks, request/event flow |
| `02-architecture-rules.md` | Whether a dependency direction or layer placement is allowed |
| `03-building-blocks-reference.md` | What a specific `BuildingBlock.*` project exposes |
| `04-coding-rules.md` | Naming, folder placement, or shape of a specific construct |
| `conventions/domain-coding-conventions.md` | How to shape an aggregate/entity/Value Object — creation signature, collections, many-to-many, validation reuse |
| `conventions/application-coding-conventions.md` | How to shape a handler/consumer/job — folder structure, Handler Philosophy, extraction, mapping/validation/constants/regex placement |
| `06-implementation-templates.md` | You're about to write a command/query/repo/endpoint/event/job and want the exact starting shape |
| `07-solid-recommendations.md` | You're evaluating whether an existing pattern should change |
| `services/*.md` | Service-specific routes, ports, config keys, known issues |
| `reference/exceptions.md` | Which exception type/status code to use, or the `MessageCode` ranges |
| `reference/caching.md` | `ICacheService` usage or the role-cache decorator pattern |
| `reference/events.md` | Internal vs integration event choice, and the direct-Outbox-enqueue publishing pattern |
| `reference/inbox-outbox-runtime.md` | Outbox relay / Inbox dedup+retry+dead-letter mechanics and configuration |
| `reference/grpc.md` | Adding a gRPC client/server call |
| `reference/saga.md` | Whether to use saga orchestration (currently: don't, unless you have a real multi-step compensable workflow) |
| `reference/authorization.md` | Policy names, `[Authorize]`/`[AuthorizeRole]` usage, claims helpers |
| `testing/TestingGuidelines.md` | How to write/name a test, mocking rules, when to use a `TestDataBuilder` |
| `testing/TestingArchitecture.md` | The `/tests` project layout, central package management, `NovaCore.TestKit` contents |
| `testing/TestingRoadmap.md` / `testing/TestingProgress.md` | What's tested, what's next, and why something was skipped |
| `setup/*.md`, `troubleshooting/seq.md` | Local environment / Docker / logging operational questions |
| `decisions/*.md` | *Why* something is architected the way it is, before changing it |

## Anti-patterns to avoid (the reason this map exists)

- Do not `grep`/explore the whole `src/` tree "just to be sure" before implementing a documented pattern — if the pattern isn't in the docs above, that's a doc gap to report, not a license to freehand.
- Do not read a service's *entire* source tree to add one endpoint — the workflow + template + coding rules are sufficient; only open the specific existing files the workflow tells you to mirror.
- Do not re-derive architecture from source when [01-architecture-map.md](01-architecture-map.md)/[02-architecture-rules.md](02-architecture-rules.md) already state it.
