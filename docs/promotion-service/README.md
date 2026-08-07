# Promotion Service — Planning

**Scope:** Phase-by-phase implementation roadmap and internal documentation for `SmartEcommerce.PromotionService`, a brand-new microservice that will become the central Promotion Engine for the entire NovaCore platform (not an Order Service module). The architecture and domain model have already been designed by the system architect. As of Phase 2, the Domain layer is complete and frozen — 103 entities across 13 aggregate groups, `Promotion.Domain` builds clean — but **no migration, repository, or endpoint exists yet.** See [planning/PROGRESS.md](planning/PROGRESS.md) for exactly what's done.

## Implementation-mode rules (binding for every future phase)

- Never redesign existing architecture. Never optimize. Never extend beyond what a phase's prompt explicitly requests.
- Never create additional entities unless explicitly requested. Never infer missing business logic.
- Never merge multiple phases together. Never implement a future phase early.
- If a dependency belongs to a future phase, add only a minimal placeholder or `TODO` comment.
- Each phase gets its own prompt and must be followed exactly — see [phases/](phases/).
- This service reuses NovaCore's existing platform conventions and BuildingBlocks as-is (5-layer Clean Architecture split, MediatR CQRS, Outbox/Inbox, Read/Write persistence services, FluentValidation, `BuildingBlock.Web`) — same precedent Payment Service's foundation phase set (see [../services/payment-service.md](../services/payment-service.md)). Nothing here invents a new platform pattern.
- **Commit granularity for Domain-implementation prompts (added 2026-08-06):** when a prompt implements one or more aggregate roots, each aggregate root's entities/enums/Value Objects/aggregate doc land in their own commit — never one commit spanning multiple aggregate roots. Cross-cutting structure (folder scaffolding, GlobalUsings, the entity-implementation-strategy doc) and cross-cutting documentation (catalogues, progress trackers, task records) each get their own commit too, separate from any aggregate's commit. This keeps `git log` readable per aggregate as the ~100+-entity roadmap plays out.

## Structure

| Path | Purpose | Populated |
|---|---|---|
| [planning/PROGRESS.md](planning/PROGRESS.md) | Current phase, completion percentage, status | Every phase |
| [planning/roadmap.md](planning/roadmap.md) | The 7-phase roadmap overview + dependency order | Phase 0 (this freeze) |
| [phases/](phases/) | One file per phase: Purpose / Expected Output / Build Verification / Completion Criteria / Blocked Items / Dependencies | Phase 0 (this freeze) |
| [architecture/](architecture/) | Service-level architecture map, aggregate boundary diagram, integration topology | Phase 2+ |
| [entities/](entities/) | Entity implementation strategy (frozen) + [translation-workflow.md](entities/translation-workflow.md) (frozen Phase 2.5) | Phase 0-2.5, done |
| [aggregates/](aggregates/) | Per-aggregate boundary docs | Phase 2+ |
| [value-objects/](value-objects/) | Value Object docs | Phase 2+ |
| [enums/](enums/) | Enum catalogue | Phase 2+ |
| [indexes/](indexes/) | Index / unique-constraint catalogue | Phase 3+ |
| [persistence/entity-configuration-conventions.md](persistence/entity-configuration-conventions.md) | EF Core entity configuration policy (Translation/mapping composite keys, enum underlying type, PK strategy, navigation policy) | Frozen Phase 3.1 |
| [search/search-strategy.md](search/search-strategy.md) | Elasticsearch integration strategy (frozen now) | Strategy: Phase 0. Implementation: Phase 4 |
| [cqrs/cqrs-strategy.md](cqrs/cqrs-strategy.md) | CQRS strategy (frozen now) | Strategy: Phase 0. Implementation: Phase 5 |
| [persistence/persistence-strategy.md](persistence/persistence-strategy.md) | Repository / persistence-service strategy (frozen now) | Strategy: Phase 0. Implementation: Phase 3/5 |
| [integration/](integration/) | Cross-service integration event docs | Phase 6+ |
| [tasks/](tasks/) | Pointer only — actual dated task tracking uses the repo-wide [../tasks/](../tasks/) convention | N/A |

## Current status

See [planning/PROGRESS.md](planning/PROGRESS.md). **Phase 0, 1, and 2 (Domain Model) complete, 3/7. Phase 3 (Persistence) is in progress — 3.1 (Entity Configuration + Domain Correction) is done: all 103 entities have an `IEntityTypeConfiguration<T>` under `Promotion.Persistence/Configs/`, `PromotionDbContext` is fully wired, and `Promotion.Domain`/`Promotion.Persistence` both build clean. No repository, persistence service, CQRS, or migration exists yet.**
