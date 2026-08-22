# Content Service

**Scope:** Content-specific facts. This is a brand-new service (added 2026-08-22) — general patterns still live in [conventions/](../conventions/) and are followed as-is; this doc only records what's Content-specific: the domain model, the aggregate-vs-child decisions behind it, what's actually wired up in this first phase, and what's deliberately postponed.

## Why ContentService exists, and what it is not

ContentService is the platform's **Content Platform / Content Engine** — reusable content-management capabilities (versioning, publication lifecycle, workflow, taxonomy, localization, audience targeting, authoring participation) for WCM, current article/news/bulletin use cases, and future specialized CMS products. It is **not an Article CRUD service**: there is no `Article` table anywhere in this service. Article is the first concrete use case built entirely on the generic `Content`/`ContentType` model — a future `News`/`Bulletin`/`KnowledgeArticle` content type requires only a new `ContentType` row plus `ContentFieldDefinition` rows, never a schema change here.

It does not own user identity, authentication, notifications, audit storage, reactions/comments/ratings, feed/ranking, search indexing, or binary/media storage — those stay in UserService/AuthService/NotificationService/AuditService/EngagementService/the search infrastructure/an asset service respectively. ContentService references external entities by id only (`ContentContributor.UserId`, `ContentAudience.AudienceReferenceId`, `ContentRelationship.TargetId`) and publishes integration events for cross-service reaction — never a direct call out.

## The central architectural idea: Content ≠ ContentVersion

**A Content is a stable identity. A ContentVersion is one editorial snapshot.** Editing never creates a new `ContentId` — it creates a new `ContentVersion` row and repoints `Content.CurrentVersionId`. `Content.PublishedVersionId` is tracked independently from `CurrentVersionId`, so a draft can keep being edited after the currently-live version was published — publishing never overwrites draft data, matching the domain baseline's core invariant.

Restoring history (`Content.RestoreVersion`) never deletes or mutates an old version — it copies that version's content into a **new** version and makes that the current one, so the full version chain is always intact.

## Aggregate-to-entity mapping

15 entities: **4 aggregate roots** (own table, repository, `I{X}ReadService`/`I{X}WriteService`) and **11 child entities** (own table + EF config, constructed only by their root, most via an `internal static Create`).

| Aggregate root | Owns (child entities) | References (by id, never a FK navigation into another root's data) |
|---|---|---|
| `Content` | `ContentVersion`, `ContentPublication`, `ContentWorkflowInstance`, `ContentRelationship`, `ContentLocalization`, `ContentTaxonomyAssignment`, `ContentAudience`, `ContentContributor` | `ContentTypeId`, `ContentVersion.WorkflowDefinitionId` (via `ContentWorkflowInstance`), `ContentTaxonomyAssignment.TaxonomyId`, `ContentContributor.UserId`, `ContentRelationship.TargetType`+`TargetId` |
| `ContentType` | `ContentFieldDefinition` | — |
| `ContentWorkflowDefinition` | `ContentWorkflowState`, `ContentWorkflowTransition` | — |
| `ContentTaxonomy` | — (self-referencing `ParentId`/`Children`) | — |

`ContentTaxonomyAssignment` is the one pure many-to-many mapping entity (`Content` ↔ `ContentTaxonomy`) — per [domain-coding-conventions.md](../conventions/domain-coding-conventions.md) rule 4, it extends the non-generic `BaseEntity` with no surrogate `Id`; the composite key is `(ContentId, TaxonomyId)`.

## Projects

`Content.Domain`, `Content.Application`, `Content.Persistence`, `Content.Infrastructure`, `Content.API` — same 5-layer split as every other service, under `src/Services/Content/`. Domain folders mirror the aggregate groups: `Entities/{Contents,ContentTypes,Workflows,Taxonomies}/`.

**Naming collision note:** the service name and its primary aggregate are both "Content", which collides with the `NovaCore.Content.*` root namespace when referenced unqualified from Application/Persistence/API. Every layer above Domain defines `global using ContentEntity = NovaCore.Content.Domain.Entities.Contents.Content;` in its `GlobalUsings.cs` and uses `ContentEntity` instead of the bare type name — the same workaround Product's `ProductEntity` alias already established for the identical Product/Product collision.

## Value Objects

Content-local, in `Content.Domain/ValueObjects/`:

- `ContentSlug` — lowercase kebab-case URL slug, `StringValueObject`-based, mirrors Product's `Slug` exactly.
- `ContentKey` — lowercase, dot/underscore/hyphen-segmented machine key. Deliberately **shared** across `ContentType.Key`, `ContentWorkflowDefinition.Key`, and `ContentTaxonomy.Key` rather than three near-identical VOs, since all three are the same concept (a stable, human-assigned key distinct from the row's surrogate `Id`) with three real usages up front — not speculative sharing ahead of need.
- **Culture reuses the existing shared `BuildingBlock.Domain.ValueObjects.LanguageCode`** — no local Culture VO was introduced.

Free-text fields (`Content.Slug` aside, `ContentVersion.Title`/`Summary`/`Body`, `ContentType.Name`/`Description`, ...) stay plain `string` with `HasMaxLength`/FluentValidation bounds, matching Product's `Name`/`Description` — this codebase does not wrap every field in a Value Object, only genuinely format-validated primitives.

**No strongly-typed id wrappers** (`ContentId`, `ContentTypeId`, ...). Every `Guid` id is a plain `Guid`, generated via `Guid.CreateVersion7()` inside the owning `Create` factory — the pattern the domain baseline's VO list suggested (`ContentId`, `WorkflowStateId`, ...) is not how any existing NovaCore service models identity, and introducing it here would be a new architecture, not a clone of an established one.

## Enums

`ContentStatus`, `ContentTypeStatus`, `ContentFieldType`, `PublicationStatus`, `WorkflowStatus`, `ContentVisibility`, `AudienceType`, `ContributorRole`, `ContentRelationshipType` — all `enum : byte`, matching `ProductStatus`'s shape. `ContentWorkflowState` uses the boolean `IsInitial`/`IsFinal` flag pair the domain baseline itself offered as the fallback, rather than a separate `WorkflowStateType` enum — no existing service in this codebase models a state-machine node any other way.

## Domain events

**No entity ever raises an event** — `AggregateRoot<TId>` is a plain marker base class with no event-raising mechanism anywhere in this codebase (see [reference/events.md](../reference/events.md)). Content lifecycle events are Application-layer concerns: a Command Handler enqueues an Integration event via `IOutboxStore.EnqueueAsync` in the same transaction as the aggregate mutation. Two exist so far, matching the two commands implemented: `ContentCreatedIntegrationEvent`, `ContentPublishedIntegrationEvent` (`BuildingBlock.Contract/Events/Content/`). The remaining lifecycle events the domain baseline names (`ContentSubmittedForReview`, `ContentApproved`, `ContentScheduled`, `ContentUnpublished`, `ContentArchived`, `WorkflowTransitioned`, `ContentLocalizationCreated`, `ContentRelationshipCreated`, ...) are **not yet defined** — this codebase adds an event contract when a real handler needs to publish it, not speculatively ahead of one (see `reference/events.md`'s note on `UserProfileUpdatedIntegrationEvent`).

**Business audit trail is fully automatic**, not a custom system: every aggregate root implements `IAuditable`, registered in `Content.Persistence/DependencyInjection.cs`'s `ConfigureAuditHierarchy` (`Content` and its 8 owned children as one graph, `ContentType`+`ContentFieldDefinition`, `ContentWorkflowDefinition`+its 2 owned children, `ContentTaxonomy` standalone). `ContentTaxonomyAssignment` is the one entity excluded — pure mapping entities aren't `IAuditable`, same reasoning Product/Chat document for their own mapping entities.

## Persistence

`ContentDbContext` (Postgres, `content_db`), one `IEntityTypeConfiguration<T>` per entity in `Configs/` — **all 15 entities are fully configured**, none stubbed. Migration `InitialCreate` creates 18 tables (15 domain + `outbox_messages`/`inbox_messages`/`inbox_retry_histories`) and has been generated and verified against the model (`dotnet ef migrations add` succeeded with the full relationship graph).

- **Relational FKs throughout**, never EF owned *entities*.
- **`Content.CurrentVersionId`/`PublishedVersionId` use `DeleteBehavior.NoAction`**, not `Restrict` — they point into the same `Versions` collection that cascades from `Content` itself; `NoAction` (rather than `Restrict`'s immediate check) lets Postgres resolve the whole cascade set within one transaction instead of tripping a circular-constraint error when a `Content` row and its versions are deleted together.
- **`ContentWorkflowTransition.FromStateId`/`ToStateId` both use `Restrict`, not `Cascade`** — `ContentWorkflowState` already cascades from `ContentWorkflowDefinition` directly, so a second cascade path through the transition table would be redundant; `Restrict` still lets a whole-definition delete succeed (the transition row is deleted via its own `WorkflowDefinitionId` cascade in the same statement) while blocking an accidental single-state delete that would orphan a transition.
- **`ContentTaxonomy.ParentId` (self-referencing) uses `Restrict`** — deleting a taxonomy node with children requires an explicit reparent/delete of the subtree first.
- **Single-scalar VOs use `HasConversion`** (`ContentSlug`, `ContentKey`, `LanguageCode`); **`ContentVersion.Metadata`/`ContentFieldDefinition.ValidationConfiguration`/`ContentRelationship.Metadata` are `jsonb`**.
- **Enums are `HasConversion<byte>()`**.
- **Concurrency**: `ConfigureCommonFields()` (audit timestamps + Postgres `xmin` row-version) on every entity.
- **Tenant filtering is automatic** — every entity implements `ITenantEntity`; no query filter is hand-written anywhere in this service.

### Read/Write services and repositories

Only the **4 aggregate roots** have the full trio (`I{X}Repository` in Persistence; `I{X}ReadService`/`I{X}WriteService` ports in `Content.Application/Abstractions/Persistence/{X}/`, implemented in `Content.Persistence/Contexts/{X}/{Read,Write}/`) — child entities are reached through their root's navigation `Include`, not a standalone Read/Write service, since no query need for one independently has come up yet (the same reasoning Chat used for entities like `ConversationParticipant`). Repositories are auto-registered by the Scrutor scan; Read/Write services are registered explicitly.

`IContentWriteService.CreateAsync`/`PublishAsync` are both **non-committing** — the calling Command Handler owns the transaction via `unitOfWork.ExecuteTransactionAsync`, so the aggregate write and its Outbox-enqueued integration event commit atomically. `IContentTypeWriteService`/`IContentWorkflowDefinitionWriteService`/`IContentTaxonomyWriteService.CreateAsync` self-commit (bare `SaveChangesAsync`) since those creation flows have no accompanying event yet.

## Application layer

Representative CQRS slice, not full CRUD — the template for implementing the remaining commands/queries:

- `CreateContentType` (Commands) — bootstraps a schema.
- `CreateContent` (Commands) — creates the aggregate + its required first `ContentVersion`, enqueues `ContentCreatedIntegrationEvent`.
- `PublishContent` (Commands) — publishes a specific version without touching draft data, enqueues `ContentPublishedIntegrationEvent`.
- `GetContentById` (Queries) — read flow demonstrating `Include`/navigation across `ContentType` + `Versions`.

Everything else the domain baseline's business rules cover (workflow transitions, taxonomy/audience/contributor management, localization, scheduling, unpublish/archive, version restore) has full **Domain** support (see `Content.cs`'s public methods) but **no Application/API surface yet** — deliberately deferred per this task's "small representative skeleton" scope.

## Infrastructure

- `AddInfrastructure` wires: `AddAppLogger`, `AddHttpAuditMetadataProvider("Content")`, `AddApplicationEventDispatcher`, `AddKafkaMessaging("content-service")`, `AddInboxOutboxInfrastructure` — the Outbox relay and Inbox retry hosted services (and their tables) are live from day one even with no consumers registered.
- No Redis cache, no Idempotency middleware, no background jobs, no gRPC clients — nothing in this phase needs them (unlike Product/Order's fuller `AddInfrastructure`), and none are wired speculatively.
- `Messaging/Consumers/` is empty — ContentService doesn't consume any integration event yet.

## API

Internal `8080` (REST) only, no gRPC. Gateway path prefix `/api/content/` (`RequireAuth: true`), public debug port via `CONTENT_PUBLIC_HTTP_PORT`.

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/content-types` | `Endpoints/ContentTypes/CreateContentType.cs` | Create a content type/schema |
| POST | `/contents` | `Endpoints/Contents/CreateContent.cs` | Create a Content + its first draft version |
| POST | `/contents/{contentId}/publish` | `Endpoints/Contents/PublishContent.cs` | Publish a specific version |
| GET | `/contents/{contentId}` | `Endpoints/Contents/GetContent.cs` | Fetch a Content with its full version history |

All four require authentication (`RequireAuthorization()`); none use a specific `Permissions.Content.*` policy yet — no `Content` module exists in `BuildingBlock.SharedKernel.Constants.Permissions` yet, add one (following Product's `Permissions.Product.*` shape) before wiring role-scoped access.

## Messaging

**2 integration event contracts** in `BuildingBlock.Contract/Events/Content/`, matching `ProductCreatedIntegrationEvent`'s shape (`sealed record ... : IIntegrationEvent`, auto-initialized `CorrelationId`/`EventType`/`PublishedAt`): `ContentCreatedIntegrationEvent`, `ContentPublishedIntegrationEvent`. Both are wired to a real `IOutboxStore.EnqueueAsync` call inside their owning Command Handler's `ExecuteTransactionAsync`, unlike a foundation-phase service's unwired contracts.

## Deployment status

- Registered in `NovaCore.sln`, the Gateway's `appsettings.json` (`Gateway:Services:Content`), `.env.template` (`CONTENT_*`), and `docker-compose.yml`/`docker-compose.override.yml` (`content-api`, active — not commented out, since real endpoints exist).
- `Content.API/ContentDbContextFactory.cs` (`IDesignTimeDbContextFactory<ContentDbContext>`) lets `dotnet ef` tooling build the context without booting the full host, same pattern as Order/User.

## Planned phases (intentionally postponed)

- **Workflow transition CQRS** — `TransitionWorkflow`/`CompleteWorkflow` commands, validating the requested transition against `ContentWorkflowDefinition.CanTransition` before calling `Content.TransitionWorkflow` (the cross-aggregate check the Domain layer itself cannot perform).
- **Taxonomy/Audience/Contributor CQRS** — assign/remove taxonomy, add/remove audience rule, add/remove/re-role contributor endpoints.
- **Localization CQRS** — `UpsertLocalization` command, per-culture content retrieval.
- **Scheduling/Unpublish/Archive CQRS** — `SchedulePublication`, `UnpublishContent`, `ArchiveContent`, `SubmitForReview`/`Approve`/`Reject` commands (all already fully supported on the `Content` aggregate itself).
- **Remaining integration events** — `ContentUnpublished`, `ContentArchived`, `ContentSubmittedForReview`, `ContentApproved`, `ContentRejected`, `ContentScheduled`, `ContentExpired`, `WorkflowTransitioned`, `ContentLocalizationCreated`, `ContentRelationshipCreated`/`Removed` — add each alongside the command that needs to publish it.
- **`Permissions.Content.*`** — a real permission module for role-scoped endpoint access, replacing the current bare `RequireAuthorization()`.
- **Article-specific WCM surface** — this stays in a consuming product/UI layer, never inside ContentService itself.

## Known issues

- No `Content.Application.Tests`/`Content.Persistence.Tests`/integration tests yet — only `Content.Domain.Tests` exists (71 tests across the 4 aggregate roots + the 2 Value Objects).
- The `InitialCreate` migration has been generated and its model validated, but never applied to a running Postgres instance — no runtime round-trip has happened yet.
