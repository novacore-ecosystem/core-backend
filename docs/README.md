# NovaCore Documentation Index

This is the entry point. Docs are organized so a task only requires reading a **minimal, deterministic subset** of files — read [05-context-loading-map.md](05-context-loading-map.md) before anything else if you're about to implement something.

> This tree was reorganized to reduce redundant reading. The previous `/docs` layout (architecture/, building-blocks/, guides/, services/, setup/, troubleshooting/, decisions/) was audited and consolidated — see [08-migration-plan.md](08-migration-plan.md) for what changed and why. Old files are preserved under `docs/_archive/` for history; do not read them for current guidance, several were stale.

## Start here

| If you are... | Read |
|---|---|
| New to this repo | [01-architecture-map.md](01-architecture-map.md) → [02-architecture-rules.md](02-architecture-rules.md) → [04-coding-rules.md](04-coding-rules.md) |
| About to write Domain/Application/Persistence code | [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md) / [conventions/application-coding-conventions.md](conventions/application-coding-conventions.md) / [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md) |
| About to implement a task | [05-context-loading-map.md](05-context-loading-map.md) — tells you exactly which files to read for your task type, nothing else |
| Looking for a copy-paste starting point | [06-implementation-templates.md](06-implementation-templates.md) |

## Document map

Every document below belongs to exactly one of four responsibilities. Don't mix them: a fact about layer boundaries belongs in Architecture, a fact about how a class is shaped belongs in Coding Convention, a step-by-step task belongs in Implementation Guidelines, and a "where do I find X" fact belongs in Project Maps.

### Architecture — system shape, layer responsibilities, dependency direction

| # | Document | Purpose |
|---|---|---|
| 01 | [architecture-map.md](01-architecture-map.md) | System-level picture: services, BuildingBlocks, dependency graph, request/event flow |
| 02 | [architecture-rules.md](02-architecture-rules.md) | Binding rules: layer responsibilities, dependency direction, what must never happen |
| 03 | [building-blocks-reference.md](03-building-blocks-reference.md) | One reference entry per `BuildingBlock.*` project: purpose, key types, DI extensions |
| — | [decisions/](#decisions-adrs) | *Why* the architecture looks the way it does — the ADRs below |

### Coding Convention — how a class/method is shaped, per layer

| Document | Purpose |
|---|---|
| [conventions/domain-coding-conventions.md](conventions/domain-coding-conventions.md) | Domain-layer style rules — aggregate creation shape, no Spec objects, plain navigation collections, many-to-many via mapping entities, reusable Value Object validation. Binding for every `*.Domain` project. |
| [conventions/application-coding-conventions.md](conventions/application-coding-conventions.md) | Application-layer style rules — Feature-First folder shape, Handler Philosophy, responsibility-based extraction, Mapster policy, validation/constants/regex placement. Binding for every `*.Application` project. |
| [conventions/persistence-coding-conventions.md](conventions/persistence-coding-conventions.md) | Persistence-layer style rules — the Read/Write persistence-service pattern (repository vs. Read Service vs. Write Service responsibilities, transaction ownership, naming). Binding for every `*.Persistence` project. |
| [04-coding-rules.md](04-coding-rules.md) | Naming, CQRS shape, endpoints, DI registration, caching decorator pattern, exceptions, async — cross-layer conventions not owned by either doc above |

### Implementation Guidelines — how to build a feature end to end

| # | Document | Purpose |
|---|---|---|
| 06 | [implementation-templates.md](06-implementation-templates.md) | Copy-paste templates: command, query, validator, repository, endpoint, integration event, background job |
| — | [workflows/](#workflows) | Step-by-step checklists per task type (add an API, add a domain entity, add an integration event, fix a bug, ...) |

### Project Maps — navigation, what to read for a given task

| # | Document | Purpose |
|---|---|---|
| 05 | [context-loading-map.md](05-context-loading-map.md) | **Read this before starting any task.** Maps task type → minimal file set |
| 07 | [solid-recommendations.md](07-solid-recommendations.md) | Documentation-only SOLID review of current architecture; no code changed |
| 08 | [migration-plan.md](08-migration-plan.md) | What happened to the old `/docs` tree during the first reorganization; see also the 2026-07-17 convention-consolidation pass this same doc set went through since |

## Services

- [services/auth-service.md](services/auth-service.md) — Auth Service (the reference implementation)
- [services/user-service.md](services/user-service.md) — User Service
- [services/product-service.md](services/product-service.md) — Product Service
- [services/inventory-service.md](services/inventory-service.md) — Inventory Service
- [services/order-service.md](services/order-service.md) — Order Service
- [services/payment-service.md](services/payment-service.md) — Payment Service (foundation phase)
- [services/shipping-service.md](services/shipping-service.md) — Shipping Service (foundation phase)
- [services/audit-service.md](services/audit-service.md) — Audit Service (MongoDB-backed)
- [services/gateway.md](services/gateway.md) — YARP API Gateway

### Promotion Service (planning)

Not yet built — [promotion-service/README.md](promotion-service/README.md) is the Phase 0 planning freeze for a brand-new, platform-wide Promotion Engine (7-phase roadmap: Bootstrap → Domain Model → Persistence → Search → CQRS → Infrastructure → Migration Prep). Check [promotion-service/planning/PROGRESS.md](promotion-service/planning/PROGRESS.md) for current phase before starting any Promotion Service work. Once Phase 7 completes, this entry moves up into the list above as `services/promotion-service.md`.

## Workflows

Step-by-step, minimal-context checklists for common tasks — see [workflows/](workflows/):
project-initialization, add-new-api, add-new-domain-entity, add-new-repository, add-integration-event, add-background-job, fix-bug, refactor-existing-code, performance-optimization, production-incident, new-service-scaffold.

## Reference (deep-dive — load only when your task actually needs it)

- [reference/exceptions.md](reference/exceptions.md) — full exception hierarchy + `ExceptionFactory` catalogue
- [reference/caching.md](reference/caching.md) — `ICacheService`/Redis, including the role-caching decorator pattern
- [reference/events.md](reference/events.md) — two-tier event system (internal/integration) and the direct-Outbox-enqueue publishing pattern
- [reference/inbox-outbox-runtime.md](reference/inbox-outbox-runtime.md) — full Outbox relay / Inbox dedup+retry+dead-letter runtime detail
- [reference/grpc.md](reference/grpc.md) — gRPC client/server building blocks
- [reference/saga.md](reference/saga.md) — saga orchestration building block
- [reference/create-order-saga.md](reference/create-order-saga.md) — the CreateOrder saga: flow, events, compensation, idempotency, failure scenarios (the building block's first real usage)
- [reference/serialization.md](reference/serialization.md) — shared JSON settings
- [reference/authorization.md](reference/authorization.md) — role/claims-based authorization
- [reference/audit-trail.md](reference/audit-trail.md) — opt-in audit tracking: `IAuditable`, `[AuditIgnore]`, `AuditInterceptor`
- [reference/payment-ownership-boundaries.md](reference/payment-ownership-boundaries.md) — Payment/Order/User responsibility matrix, `ReferenceType`/`ReferenceId` convention, payment integration strategy

## Refactoring

Living trackers for framework-level, multi-phase migrations spanning multiple services — see [refactoring/README.md](refactoring/README.md) for the convention.

- [refactoring/persistence-refactor-plan.md](refactoring/persistence-refactor-plan.md) — Read/Write persistence-service layer migration across all 7 services (in progress).

## Tasks

Dated, per-task bug/gap tracking (not architecture, not workflows) — see [tasks/README.md](tasks/README.md) for the convention. Check [tasks/PROGRESS.md](tasks/PROGRESS.md) for what's currently open before starting unrelated work that might overlap.

## Testing

- [testing/TestingArchitecture.md](testing/TestingArchitecture.md) — `/tests` project layout, central package management, `NovaCore.TestKit` shared infrastructure, library choices (xUnit/Shouldly/NSubstitute)
- [testing/TestingGuidelines.md](testing/TestingGuidelines.md) — how to write a test: AAA structure, naming, mocking rules, when to use a `TestDataBuilder`, what triggers a new test per workflow
- [testing/TestingRoadmap.md](testing/TestingRoadmap.md) — the 6-phase long-term plan (SharedKernel → BuildingBlocks → Domain → Application → Infrastructure → API), mapped to NovaCore's actual projects
- [testing/TestingProgress.md](testing/TestingProgress.md) — living checkpoint: what's done, what's next, technical debt, known limitations — read this first when resuming the testing initiative

## Setup & operations

- [setup/docker.md](setup/docker.md) — Docker Compose layering, day-to-day commands
- [setup/environment-config.md](setup/environment-config.md) — `.env`/`.env.template` workflow
- [setup/database-split.md](setup/database-split.md) — splitting the shared Postgres container per service
- [setup/credentials.md](setup/credentials.md) — default dev credentials and access points
- [setup/observability.md](setup/observability.md) — Elasticsearch log shipping + Elastic APM tracing (Order.API vertical slice)
- [troubleshooting/seq.md](troubleshooting/seq.md) — Seq logging troubleshooting

## Decisions (ADRs)

See [decisions/README.md](decisions/README.md) for the convention (when to write one, section shape).

- [decisions/event-messaging-refactor.md](decisions/event-messaging-refactor.md)
- [decisions/buildingblock-web-extraction.md](decisions/buildingblock-web-extraction.md)

## Rules for maintaining this doc system

1. **Don't duplicate.** If a fact belongs in one doc, link to it from others — never restate it.
2. **Don't let docs drift.** When you change a DI registration, endpoint route, config key, or shared pattern, update the doc that owns that fact (check [05-context-loading-map.md](05-context-loading-map.md) to find it) in the same change.
3. **New workflow docs go in `workflows/`.** New deep-reference docs go in `reference/`. New layer-wide style rules go in `conventions/`. New multi-phase migration trackers go in `refactoring/`. Nothing new goes directly under `docs/` root except the numbered core docs above — if you think you need a new root-level doc, you probably need a workflow, reference, convention, or refactoring doc instead.
4. **Every doc states its own scope in the first paragraph** so a reader (human or AI) can decide in one sentence whether to keep reading.
