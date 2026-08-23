# Content Service

**Scope:** Content-specific facts. This service was added 2026-08-22 and its first usable WCM
(Web Content Management) capability landed 2026-08-23 — general patterns still live in
[conventions/](../conventions/) and are followed as-is; this doc only records what's
Content-specific: the domain model, the aggregate-vs-child decisions behind it, what's actually
wired up, and what's deliberately postponed.

## Why ContentService exists, and what it is not

ContentService is the platform's **Content Platform / Content Engine** — reusable content-management capabilities (versioning, publication lifecycle, workflow, taxonomy, localization, audience targeting, authoring participation) for WCM, current article/news/bulletin use cases, and future specialized CMS products. It is **not an Article CRUD service**: there is no `Article` table anywhere in this service. Article is the first concrete use case built entirely on the generic `Content`/`ContentType` model — a future `News`/`Bulletin`/`KnowledgeArticle` content type requires only a new `ContentType` row plus `ContentFieldDefinition` rows, never a schema change here.

It does not own user identity, authentication, notifications, audit storage, reactions/comments/ratings, feed/ranking, search indexing, or binary/media storage — those stay in UserService/AuthService/NotificationService/AuditService/EngagementService/the search infrastructure/an asset service respectively. ContentService references external entities by id only (`ContentContributor.UserId`, `ContentAudience.AudienceReferenceId`, `ContentRelationship.TargetId`) and publishes integration events for cross-service reaction — never a direct call out.

## The central architectural idea: Content ≠ ContentVersion ≠ ContentLocalization

**A Content is a stable identity. A ContentVersion is one editorial edition. A ContentLocalization
is one language's actual payload within that edition.** Editing never creates a new `ContentId` —
it creates a new `ContentVersion` row and repoints `Content.CurrentVersionId`.
`Content.PublishedVersionId` is tracked independently from `CurrentVersionId`, so a draft can keep
being edited after the currently-live version was published — publishing never overwrites draft
data, matching the domain baseline's core invariant.

**Version+Language model (2026-08-23 remodel):** `ContentVersion` itself carries no editorial
content at all anymore (no `Title`/`Summary`/`Body`/`Metadata`) — only `VersionNumber`/`Status`.
Every language's actual `Title`/`Summary`/`Body`(jsonb, Editor.js-compatible JSON)/`Metadata`(SEO)
lives on `ContentLocalization`, keyed uniquely per `(VersionId, Culture)`. One version therefore
groups every language it was edited in together:

```text
Content
 └── Version 1 → en localization, vi localization, ja localization
 └── Version 2 → en localization, vi localization
```

This replaced the original design where `ContentLocalization` was keyed per `(ContentId, Culture)`
and pointed at a whole separate `ContentVersion` per language — i.e. each language had its own
independent version lineage. That shape made "translate this version into another language"
impossible to express (translating meant creating a brand-new, unrelated version chain), which
directly contradicted the WCM translation requirement, so the domain was restructured rather than
worked around.

**`Content.UpsertLocalization(versionId, culture, title, summary, body, updatedBy, metadata?)`** is
the *only* mechanism that writes a language's content into a version — draft editing (Application
command `UpdateContentDraft`) and the Translation API (`TranslateContentVersion`) are both thin
wrappers over this one domain method, never two parallel localization systems. It throws
`InvalidStatusException` if the target version is already `Published`/`Archived` (immutable —
translate into a new version instead), and upserts in place if the culture already exists on that
version.

Restoring history (`Content.RestoreVersion`) never deletes or mutates an old version — it copies
**every language** that version carried into a **new** version (via the same `UpsertLocalization`
mechanism) and makes that the current one, so the full version chain and every localization on it
stay intact.

**Language fallback:** create/update/read/publish/version/translation/landing flows that don't get
an explicit language fall back to `Content.Application.Common.ContentLanguageDefaults.Default`
(`en`). This is a **service-wide** default, not a true per-tenant one — Content Service has no
cross-service lookup into Auth's `Tenant`/`TenantLocale` yet, so every tenant currently shares the
same fallback. Upgrading this to a real per-tenant default is flagged as a `TODO` on
`ContentLanguageDefaults` rather than solved here.

**Soft delete:** `Content` implements `ISoftDeleteEntity` (`IsDeleted`/`DeletedAt`/`MarkDeleted()`,
plus a `Restore()` that clears both) — picked up automatically by the Entity Convention scan (see
[reference/tenant-convention.md](../reference/tenant-convention.md)), no hand-written query filter.
Only the aggregate root is soft-deleted; children stay as-is and simply become unreachable through
the filtered `Content` query. A background job permanently removes rows soft-deleted longer than
the retention window (default 7 days) — see Infrastructure below.

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

Free-text fields (`Content.Slug` aside, `ContentLocalization.Title`/`Summary`, `ContentType.Name`/`Description`, ...) stay plain `string` with `HasMaxLength`/FluentValidation bounds, matching Product's `Name`/`Description` — this codebase does not wrap every field in a Value Object, only genuinely format-validated primitives.

`ContentLocalization.Body` is also plain `string`, not a Value Object or a `JsonDocument` — the domain validates it's syntactically valid JSON (`ContentLocalization.IsValidBody`, via `System.Text.Json.JsonDocument.Parse` in a try/catch) without ever parsing Editor.js-specific internals (block types, etc.). Keeping it a plain string mapped `HasColumnType("jsonb")` is the entire "JSONB migration" — no new domain machinery, no Editor.js-aware C# types, matching how `ContentMetadata` already round-trips through `jsonb` via `MetadataBase.ToJson()`/`FromJson<T>()`.

**No strongly-typed id wrappers** (`ContentId`, `ContentTypeId`, ...). Every `Guid` id is a plain `Guid`, generated via `Guid.CreateVersion7()` inside the owning `Create` factory — the pattern the domain baseline's VO list suggested (`ContentId`, `WorkflowStateId`, ...) is not how any existing NovaCore service models identity, and introducing it here would be a new architecture, not a clone of an established one.

## Enums

`ContentStatus`, `ContentTypeStatus`, `ContentFieldType`, `PublicationStatus`, `WorkflowStatus`, `ContentVisibility`, `AudienceType`, `ContributorRole`, `ContentRelationshipType` — all `enum : byte`, matching `ProductStatus`'s shape. `ContentWorkflowState` uses the boolean `IsInitial`/`IsFinal` flag pair the domain baseline itself offered as the fallback, rather than a separate `WorkflowStateType` enum — no existing service in this codebase models a state-machine node any other way.

## Domain events

**No entity ever raises an event** — `AggregateRoot<TId>` is a plain marker base class with no event-raising mechanism anywhere in this codebase (see [reference/events.md](../reference/events.md)). Content lifecycle events are Application-layer concerns: a Command Handler enqueues an Integration event via `IOutboxStore.EnqueueAsync` in the same transaction as the aggregate mutation. Two exist so far, matching the two commands implemented: `ContentCreatedIntegrationEvent`, `ContentPublishedIntegrationEvent` (`BuildingBlock.Contract/Events/Content/`). The remaining lifecycle events the domain baseline names (`ContentSubmittedForReview`, `ContentApproved`, `ContentScheduled`, `ContentUnpublished`, `ContentArchived`, `WorkflowTransitioned`, `ContentLocalizationCreated`, `ContentRelationshipCreated`, ...) are **not yet defined** — this codebase adds an event contract when a real handler needs to publish it, not speculatively ahead of one (see `reference/events.md`'s note on `UserProfileUpdatedIntegrationEvent`).

**Business audit trail is fully automatic**, not a custom system: every aggregate root implements `IAuditable`, registered in `Content.Persistence/DependencyInjection.cs`'s `ConfigureAuditHierarchy` (`Content` and its 8 owned children as one graph, `ContentType`+`ContentFieldDefinition`, `ContentWorkflowDefinition`+its 2 owned children, `ContentTaxonomy` standalone). `ContentTaxonomyAssignment` is the one entity excluded — pure mapping entities aren't `IAuditable`, same reasoning Product/Chat document for their own mapping entities.

## Persistence

`ContentDbContext` (Postgres, `content_db`), one `IEntityTypeConfiguration<T>` per entity in `Configs/` — **all 15 entities are fully configured**, none stubbed. Two migrations: `InitialCreate`, then `AddContentVersionLanguageAndSoftDelete` (moves `Title`/`Summary`/`Body`/`Metadata` off `content_versions` onto `content_localizations` as `jsonb`, adds `contents.is_deleted`/`deleted_at`). Both greenfield changes — no production data existed to migrate, so the second migration is a straight drop/add rather than a data-preserving backfill.

- **Relational FKs throughout**, never EF owned *entities*.
- **`Content.CurrentVersionId`/`PublishedVersionId` use `DeleteBehavior.NoAction`**, not `Restrict` — they point into the same `Versions` collection that cascades from `Content` itself; `NoAction` (rather than `Restrict`'s immediate check) lets Postgres resolve the whole cascade set within one transaction instead of tripping a circular-constraint error when a `Content` row and its versions are deleted together.
- **`ContentLocalization.VersionId` FK uses `Restrict`** — `ContentLocalization` cascades from `Content` directly (its own `ContentId` FK), the same sibling-cascade reasoning as above; `Restrict` on `VersionId` blocks deleting a single version in isolation while its localizations still exist (never happens in practice - only whole-`Content` cascade deletes remove a version), same pattern as `CurrentVersionId`/`PublishedVersionId`.
- **`ContentWorkflowTransition.FromStateId`/`ToStateId` both use `Restrict`, not `Cascade`** — `ContentWorkflowState` already cascades from `ContentWorkflowDefinition` directly, so a second cascade path through the transition table would be redundant; `Restrict` still lets a whole-definition delete succeed (the transition row is deleted via its own `WorkflowDefinitionId` cascade in the same statement) while blocking an accidental single-state delete that would orphan a transition.
- **`ContentTaxonomy.ParentId` (self-referencing) uses `Restrict`** — deleting a taxonomy node with children requires an explicit reparent/delete of the subtree first.
- **Single-scalar VOs use `HasConversion`** (`ContentSlug`, `ContentKey`, `LanguageCode`); **`ContentLocalization.Body`/`Metadata`, `ContentFieldDefinition.ValidationConfiguration`, `ContentRelationship.Metadata` are `jsonb`**.
- **Enums are `HasConversion<byte>()`**.
- **Concurrency**: `ConfigureCommonFields()` (audit timestamps + Postgres `xmin` row-version) on every entity.
- **Tenant filtering is automatic** — every entity implements `ITenantEntity`; no query filter is hand-written anywhere in this service.
- **Soft-delete filtering is automatic** on `Content` (`ISoftDeleteEntity`) — same Entity Convention scan, no hand-written filter.
- **Indexes for the new query patterns**: `content_localizations (version_id, culture)` unique (replacing the old `(content_id, culture)` unique index — a culture is now unique per version, not per content) plus a non-unique `(content_id, culture)` for the fallback-resolution queries; `contents (is_deleted)` from the soft-delete convention scan; the existing `contents (content_type_id)`/`(status)`/`(visibility)`/`(created_at* via PK/audit)` cover the Admin/Landing cursor keysets (`CreatedAt`/`PublishedAt` + `Id` tie-breaker) combined with the Criteria filters.

### Cursor pagination and read-model projections

`ContentReadService.SearchAdminAsync`/`SearchLandingAsync` are the only two Content queries that
return dedicated read-model records (`ContentAdminListItem`/`ContentLandingItem` in
`Content.Application/Abstractions/Persistence/Contents/ContentReadModels.cs`) instead of the
`ContentEntity` aggregate — both use an EF `.Select()` projection that never materializes
`ContentLocalization.Body`, so list screens genuinely never pull the Editor.js payload over the
wire, not just avoid exposing it in the response DTO. Cursor pagination itself is a fixed keyset
(`CreatedAt`/`PublishedAt` desc, `Id` desc as the tie-breaker), the same shape
`UserNotificationCursor` (Notification Service, Mongo) already established, adapted for EF/Postgres
via `Guid.CompareTo()` (Postgres has no native `<`/`>` operator surfaced to plain C# `Guid`).
Deliberately **not** integrated with arbitrary Criteria `Sorts` — cursor pagination requires the
sort to match the keyset exactly, so `Sorts` on these two queries's `CriteriaRequest` are ignored;
`Filters` still apply normally.

### Read/Write services and repositories

Only the **4 aggregate roots** have the full trio (`I{X}Repository` in Persistence; `I{X}ReadService`/`I{X}WriteService` ports in `Content.Application/Abstractions/Persistence/{X}/`, implemented in `Content.Persistence/Contexts/{X}/{Read,Write}/`) — child entities are reached through their root's navigation `Include`, not a standalone Read/Write service, since no query need for one independently has come up yet (the same reasoning Chat used for entities like `ConversationParticipant`). Repositories are auto-registered by the Scrutor scan; Read/Write services are registered explicitly.

Every `IContentWriteService` method is **non-committing** — the calling Command Handler owns the
transaction via `unitOfWork.ExecuteTransactionAsync`, so the aggregate write and (where one exists)
its Outbox-enqueued integration event commit atomically. `IContentTypeWriteService`/
`IContentWorkflowDefinitionWriteService`/`IContentTaxonomyWriteService.CreateAsync` self-commit
(bare `SaveChangesAsync`) since those creation flows have no accompanying event yet.

Mutations that reach into `Versions`/`Localizations` (`PublishAsync`, `CreateDraftVersionAsync`,
`UpsertLocalizationAsync`, `RestoreVersionAsync`) all go through the repository's
`UpdateAsync(id, includes, action, ct)` overload with an explicit
`.Include(c => c.Versions).ThenInclude(v => v.Localizations)` — the id-only `UpdateAsync(id,
action, ct)` overload queries with **no** includes, which would hand the aggregate method an empty
`Versions` collection and make it fail with a spurious `EntityNotFoundException`. `PublishAsync`
originally used the id-only overload; this was a latent bug (every `Publish` call would have thrown
at runtime) fixed as part of the 2026-08-23 WCM work, not a new pattern introduced by it.

`RestoreAsync`/`GetByIdIncludingDeletedAsync`/`HardDeleteAsync` all use `.IgnoreQueryFilters()`
directly against `ContentDbContext` (not the generic repository) — the only supported way to reach
a soft-deleted `Content` row, since the Entity Convention's automatic query filter excludes
`IsDeleted` rows from every other query path by design.

## Application layer

First usable WCM milestone (2026-08-23) — CRUD, draft, publish, versioning, translation, soft
delete/restore, admin/landing search, and the background retention job all have a real
Application/API surface now:

**Commands** (`Features/Contents/Commands/`): `CreateContent`, `CreateContentVersion` (new draft
version on existing Content), `UpdateContentDraft` (upsert a language onto a specific draft
version), `TranslateContentVersion` (upsert a *new* language onto a version — same
`UpsertLocalization` mechanism as draft editing, reports whether the language already existed),
`PublishContent`, `RestoreContentVersion` (restore a prior version's full language set into a new
version), `DeleteContent` (soft delete), `RestoreContent` (undelete).

**Queries** (`Features/Contents/Queries/`): `GetContentById` (admin detail — every version, every
localization, no body-less trimming), `GetContentVersion` (one version, every language including
`Body`), `SearchContentsAdmin` (Criteria filters + fixed-keyset cursor pagination, lightweight),
`GetLandingContents` (public, published-only, cursor-paginated, language-resolved),
`GetLandingContentCursorPoints` (same filters as Landing, returns only each page's boundary
cursor), `GetPublishedContentBySlug` (public single-item read, language-resolved).

Everything else the domain baseline's business rules cover (workflow transitions, taxonomy/audience/contributor management, scheduling, unpublish/archive, submit-for-review/approve/reject) has full **Domain** support (see `Content.cs`'s public methods) but **no Application/API surface yet** — still deliberately deferred, unrelated to the WCM milestone's scope.

## Infrastructure

- `AddInfrastructure` wires: `AddAppLogger`, `AddHttpAuditMetadataProvider("Content")`, `AddApplicationEventDispatcher`, `AddKafkaMessaging("content-service")`, `AddInboxOutboxInfrastructure`, `AddBackgroundJobs` — the Outbox relay and Inbox retry hosted services (and their tables) are live from day one even with no consumers registered.
- **Background jobs**: `HardDeleteContentService` (`Content.Infrastructure/BackgroundJobs/Jobs/HardDeleteContent/`) — hourly Hangfire job, permanently removes `Content` rows soft-deleted longer than `Jobs:HardDeleteContent:RetentionDays` (default 7, business default per the WCM spec), batched (`BatchSize`, default 100) and transaction-wrapped per batch, same shape as Auth's `RefreshTokenSyncService`. First background job this service needed, so it also added the Hangfire bootstrap (`BackgroundJobsExtensions`, dashboard/scheduling calls in `ApplicationPipeline`) and its own `content_hangfire_db` (provisioned in `scripts/postgres/init.sql`, connection string wired in `docker-compose.override.yml`/`.env.template`).
- No Redis cache, no Idempotency middleware, no gRPC clients — nothing yet needs them (unlike Product/Order's fuller `AddInfrastructure`), and none are wired speculatively.
- `Messaging/Consumers/` is empty — ContentService doesn't consume any integration event yet.
- **Seed data**: `ContentSeeder` (`Content.Persistence/Storage/Seeders/`) runs once at startup after migration (same pattern as `ProductSeeder`) — seeds the `article`/`news`/`blog` `ContentType`s plus 26 `Content` items across them with Editor.js JSON bodies, mixed draft/published status, one soft-deleted item, and en/vi localization on every third item.

## API

Internal `8080` (REST) only, no gRPC. Gateway path prefix `/api/content/` (`RequireAuth: true`), public debug port via `CONTENT_PUBLIC_HTTP_PORT`.

| Method | Route | File | Purpose |
|---|---|---|---|
| POST | `/content-types` | `Endpoints/ContentTypes/CreateContentType.cs` | Create a content type/schema |
| POST | `/contents` | `Endpoints/Contents/CreateContent.cs` | Create a Content + its first draft version + language |
| POST | `/contents/{contentId}/versions` | `Endpoints/Contents/CreateContentVersion.cs` | Create a new draft version |
| GET | `/contents/{contentId}/versions/{versionId}` | `Endpoints/Contents/GetContentVersion.cs` | Get one version, every language |
| PUT | `/contents/{contentId}/versions/{versionId}/draft` | `Endpoints/Contents/UpdateContentDraft.cs` | Update a language's draft content |
| POST | `/contents/{contentId}/versions/{versionId}/translations` | `Endpoints/Contents/TranslateContentVersion.cs` | Add/update a language on a version |
| POST | `/contents/{contentId}/versions/{versionId}/restore` | `Endpoints/Contents/RestoreContentVersion.cs` | Restore a prior version's full language set into a new version |
| POST | `/contents/{contentId}/publish` | `Endpoints/Contents/PublishContent.cs` | Publish a specific version |
| GET | `/contents/{contentId}` | `Endpoints/Contents/GetContent.cs` | Admin detail — every version, every localization |
| DELETE | `/contents/{contentId}` | `Endpoints/Contents/DeleteContent.cs` | Soft delete |
| POST | `/contents/{contentId}/restore` | `Endpoints/Contents/RestoreContent.cs` | Undelete |
| POST | `/contents/admin/search` | `Endpoints/Contents/SearchContentsAdmin.cs` | Admin list — Criteria filters + cursor pagination |
| GET | `/contents/landing` | `Endpoints/Contents/GetLandingContents.cs` | Public landing feed — published, cursor-paginated, language-resolved |
| GET | `/contents/landing/cursor-points` | `Endpoints/Contents/GetLandingContentCursorPoints.cs` | Public — same filters as Landing, boundary cursor per page only |
| GET | `/contents/published/{slug}` | `Endpoints/Contents/GetPublishedContentBySlug.cs` | Public single-item read — published, language-resolved |

Every route requires authentication (`RequireAuthorization()`) except the three explicitly public ones (`GetLandingContents`, `GetLandingContentCursorPoints`, `GetPublishedContentBySlug`, all `AllowAnonymous()`); none use a specific `Permissions.Content.*` policy yet — no `Content` module exists in `BuildingBlock.SharedKernel.Constants.Permissions` yet, add one (following Product's `Permissions.Product.*` shape) before wiring role-scoped access.

## Messaging

**3 integration event contracts** in `BuildingBlock.Contract/Events/Content/`, matching `ProductCreatedIntegrationEvent`'s shape (`sealed record ... : IIntegrationEvent`, auto-initialized `CorrelationId`/`EventType`/`PublishedAt`): `ContentCreatedIntegrationEvent`, `ContentPublishedIntegrationEvent`, `ContentDeletedIntegrationEvent`. All three are wired to a real `IOutboxStore.EnqueueAsync` call inside their owning Command Handler's `ExecuteTransactionAsync`. `CreateContentVersion`/`UpdateContentDraft`/`TranslateContentVersion`/`RestoreContentVersion`/`RestoreContent` deliberately have no accompanying event yet — nothing outside the service needs to react to them yet, and this codebase adds an event contract when a real consumer needs it, not speculatively.

## Deployment status

- Registered in `NovaCore.sln`, the Gateway's `appsettings.json` (`Gateway:Services:Content`), `.env.template` (`CONTENT_*`, now including `CONTENT_HANGFIRE_DB_CONNECTION`), and `docker-compose.yml`/`docker-compose.override.yml` (`content-api`, active — not commented out, since real endpoints exist).
- `Content.API/ContentDbContextFactory.cs` (`IDesignTimeDbContextFactory<ContentDbContext>`) lets `dotnet ef` tooling build the context without booting the full host, same pattern as Order/User.
- `content_db`/`content_hangfire_db` are now provisioned in `scripts/postgres/init.sql` — `content_db` was referenced by `ContentDbContextFactory`'s fallback connection string from day one but had been left out of `init.sql`, a pre-existing gap fixed alongside adding the Hangfire database.

## Planned phases (intentionally postponed)

- **Workflow transition CQRS** — `TransitionWorkflow`/`CompleteWorkflow` commands, validating the requested transition against `ContentWorkflowDefinition.CanTransition` before calling `Content.TransitionWorkflow` (the cross-aggregate check the Domain layer itself cannot perform).
- **Taxonomy/Audience/Contributor CQRS** — assign/remove taxonomy, add/remove audience rule, add/remove/re-role contributor endpoints.
- **Scheduling/Unpublish/Archive CQRS** — `SchedulePublication`, `UnpublishContent`, `ArchiveContent`, `SubmitForReview`/`Approve`/`Reject` commands (all already fully supported on the `Content` aggregate itself).
- **Remaining integration events** — `ContentUnpublished`, `ContentArchived`, `ContentSubmittedForReview`, `ContentApproved`, `ContentRejected`, `ContentScheduled`, `ContentExpired`, `WorkflowTransitioned`, `ContentRestoredIntegrationEvent` — add each alongside the command that needs to publish it.
- **`Permissions.Content.*`** — a real permission module for role-scoped endpoint access, replacing the current bare `RequireAuthorization()`.
- **Real per-tenant default language** — replacing `ContentLanguageDefaults`'s service-wide constant once Content Service can read Auth's tenant configuration.
- **Article-specific WCM surface** — this stays in a consuming product/UI layer, never inside ContentService itself.

## Known issues

- No Testcontainers-backed `Content.Persistence.Tests`/API integration tests yet — this environment had no Docker available while building the WCM milestone, so the JSONB column mapping, `.IgnoreQueryFilters()` soft-delete paths, and the cursor keyset queries (including the `Guid.CompareTo()` translation) are verified by code review and the generated EF migration only, not by a real round-trip against Postgres. `Content.Domain.Tests` (89 tests) and the new `Content.Application.Tests` (22 tests, handler-level with NSubstitute) both run and pass.
- The migrations have been generated and their model validated, but never applied to a running Postgres instance — no runtime round-trip has happened yet.
