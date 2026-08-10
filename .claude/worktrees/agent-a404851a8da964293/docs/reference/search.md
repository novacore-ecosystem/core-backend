# Reference: Search (Elasticsearch)

**Scope:** the reusable Search read-model infrastructure introduced by the Product Search feature, the Product-specific implementation built on top of it, and User Search (added 2026-07-28, see [User Search](#user-search) below), the second full implementation of the pattern. Read this before touching `BuildingBlock.Search`, `Product.Persistence/Contexts/Products/Search`, `User.Persistence/Contexts/UserProfiles/Search`, or any future service's search integration.

## Responsibility

Elasticsearch is a **read model, never a source of truth**. PostgreSQL remains authoritative for every service. Elasticsearch exists only to serve search/list queries efficiently — full-text keyword search, faceted filtering, sorting — that Postgres `ILIKE` scans don't do well at scale.

**Hard rules:**
- No command handler ever writes to Elasticsearch directly or synchronously.
- All writes to Elasticsearch originate from PostgreSQL, flow through the existing Outbox → Kafka pipeline, and land asynchronously via a dedicated consumer.
- Elasticsearch is eventually consistent with Postgres, never the reverse.

## Architecture: reusable vs. Product-specific

Search is **not** a new microservice, nor a separate technology-specific project. Each bounded context that adopts Search owns its own index and its own sync pipeline as part of its own `<Service>.Persistence` project, reusing only the technology-agnostic 20% via `BuildingBlock.Search`. Product Search demonstrates the pattern; User Search (2026-07-28) repeats it independently, proving the pattern generalizes; Order Search would follow the same shape.

```
BuildingBlock.Search (reusable — client, generic indexer, config, retry policy)
  ├── used by → Product.Persistence/Contexts/Products/Search (Product-specific: document, mapping, repository, indexer wrapper)
  ├── used by → User.Persistence/Contexts/UserProfiles/Search (User-specific: document, mapping incl. a custom analyzer, repository, indexer wrapper)
  └── used by → (future) Order.Persistence/Contexts/Orders/Search
```

There is deliberately no `*.Persistence.Elasticsearch` project for any service — search is a persistence capability belonging to a bounded context, not an implementation technology, so it lives beside that context's `Read`/`Write`/`Repositories` folders inside the owning `<Service>.Persistence` project. Dependency injection for search is registered from the Persistence composition root (`AddPersistence` → `AddProductSearchServices`), the same place every other persistence capability is registered — never a standalone `Add{Technology}Persistence()` call in `Program.cs`.

### `BuildingBlock.Search` (reusable)

Zero project references (root-level BuildingBlock, like `SharedKernel`), one package: `Elastic.Clients.Elasticsearch`.

- `Configuration/ElasticsearchOptions.cs` — `Url`, `MaxRetries`, `RequestTimeoutSeconds`.
- `Abstractions/IElasticsearchIndexer.cs` — generic `IElasticsearchIndexer<TDocument>`: `EnsureIndexAsync`, `RecreateIndexAsync`, `IndexAsync`, `DeleteAsync`, `BulkIndexAsync`. This is the **only** reusable component allowed to write to Elasticsearch — querying is deliberately not part of this interface, since query DSL is too domain-specific to generalize. `EnsureIndexAsync`/`RecreateIndexAsync` each have an **additive overload** (added 2026-07-28 for User Search) also accepting `Action<IndexSettingsDescriptor<TDocument>> configureSettings`, for services that need index-level settings (e.g. a custom analyzer) beyond field mapping — Product's existing 3-arg calls are unaffected.
- `Indexing/ElasticsearchIndexer.cs` — the implementation, wraps a singleton `ElasticsearchClient`.
- `DependencyInjection/ServiceCollectionExtensions.cs` — `AddElasticsearchClient(configuration)`: binds `ElasticsearchOptions` from the `"Elasticsearch"` config section, registers `ElasticsearchClient` as a singleton (thread-safe/stateless, same lifetime rationale as `Persistence.Mongo`'s `IMongoClient`) with `MaxRetries`/`RequestTimeout` wired from options (the "retry policies" extension point — no Polly needed, the Elastic client's built-in transport retry covers it), and registers `IElasticsearchIndexer<>` as an open-generic singleton.

**Deliberately not included yet** (per the task's "prepare extension points, don't implement unnecessary monitoring" guidance):
- No health check package wired — the extension point is `BuildingBlock.Web/HealthChecks/HealthCheckExtensions.cs`'s `AddHealthCheckServices()` chain; a future `AddElasticsearchHealthCheck()` would slot in there.
- No OpenTelemetry/metrics/tracing — `ElasticsearchClientSettings` supports `OnRequestCompleted`/instrumentation hooks natively when needed.
- No generic query/search abstraction — every service's search criteria and result shape differ enough that a shared interface would be premature; each service's Search Repository talks to `ElasticsearchClient` directly.

### Product-specific (Product.Application + Product.Persistence)

Everything below lives inside Product's own projects, never in a BuildingBlock — per the task's explicit instruction that `ProductSearchDocument`/`ProductProjectionBuilder`/`ProductSearchRepository` remain Product-specific.

**`Product.Application/Abstractions/Search/`** (interfaces + document/criteria, mirrors `Abstractions/Repositories/`):
- `ProductSearchDocument.cs` — the read-model document. Deliberately not the `Product` aggregate: `ProductId`, `Code`, `Name`, `Slug`, `Thumbnail`, `DefaultPrice`, `DefaultVariationId`/`Sku`, `VariationNames`, `VariationIds`, `CategoryIds`/`CategoryNames`, `TagIds`/`TagNames`, `Status`, `UpdatedAt`. Optimized for query/display, not persistence — no metadata blobs, and `VariationNames`/`VariationIds` are just flat lists of each Active variation's `Name`/`Id` (for keyword search and stock aggregation respectively), not a full variation list with price/stock/etc. `SearchProductsHandler` uses `VariationIds` to compute `IsInStock` as "ANY Active variation has stock," not just the Default's.
  - `Status` is a deliberate stand-in: `Product` itself has no lifecycle status field, so the document uses the **Default Variation's** `Status` (Active/Inactive/Discontinued).
  - `Thumbnail` is the Default Variation's first image URL, if any.
- `ProductSearchCriteria.cs` — `Keyword`, `CategoryId`, `TagId`, `Status`, `SortBy`, `SortDescending`, `Page`, `PageSize`. Adding a new filter (price range, brand, attributes) later is just a new optional field here — no redesign of the repository or query pipeline.
- `IProductSearchRepository.cs` — **query-only**: `SearchAsync(ProductSearchCriteria, ct)`. No `Add`/`Update`/`Delete` — indexing is a completely separate interface.
- `IProductSearchIndexer.cs` — **write-only**: `EnsureIndexAsync`, `RecreateIndexAsync`, `IndexAsync`, `DeleteAsync`, `BulkIndexAsync`. Product's thin wrapper around `IElasticsearchIndexer<ProductSearchDocument>`, fixing the index name and mapping.

**`Product.Application/Features/Products/Search/ProductSearchProjectionBuilder.cs`** — the Projection Builder: **Integration Event → Search Document**. `BuildAsync(ProductEntity, ct)` (single) and `BuildManyAsync(IReadOnlyList<ProductEntity>, ct)` (batched, preloads all categories/tags once instead of N+1). This is the **only** place `ProductSearchDocument` is assembled — both the live sync path and the rebuild path call into it, so a future schema change touches exactly one class.

**`Product.Persistence/Contexts/Products/Search/`** (the Product context's Search implementation, living beside its `Read`/`Write`/`Repositories` siblings — not a separate technology-specific project):
- `ProductSearchIndexNames.cs` — the literal index name (`product-search`), the only place it's hardcoded.
- `Mapping/ProductSearchIndexMapping.cs` — the ES field mapping (keyword fields for ids/codes/status, text+keyword-subfield for `Name`, double for price, date for `UpdatedAt`).
- `Indexers/ProductSearchIndexer.cs` — `IProductSearchIndexer` impl, delegates to `IElasticsearchIndexer<ProductSearchDocument>` with the fixed index name/mapping.
- `Repositories/ProductSearchRepository.cs` — `IProductSearchRepository` impl. Builds a `bool` query directly against `ElasticsearchClient`: `must` multi-match on `Keyword` across `name`/`variationNames`/`categoryNames`/`tagNames` when present, `filter` terms on `categoryIds`/`tagIds`/`status` when present, `sort` on `name.keyword`/`defaultPrice`/`updatedAt`, `from`/`size` for paging.

Registration lives in `Product.Persistence/DependencyInjection.cs`'s private `AddProductSearchServices(configuration)`, called from the public `AddPersistence(configuration)` alongside `AddRepositories`/`AddUnitOfWork`/`AddOutbox`/etc. It calls `BuildingBlock.Search`'s `AddElasticsearchClient`, then registers the Product-specific indexer/repository — a business-capability name (`AddProductSearchServices`), not a technology-oriented one (`AddElasticsearchPersistence`). `Program.cs` therefore only calls `.AddPersistence(configuration)`; there is no separate `.AddElasticsearchPersistence(...)` step.

## Synchronization flow

Product Service is **both publisher and consumer** of its own integration events — it self-consumes via its own Outbox → Kafka → its own Kafka consumer, exactly like any other cross-service consumer in this codebase, just looping back to itself. This decouples the write path (Postgres) from the read-model update (Elasticsearch) without a synchronous dependency, and lets the sync retry/backoff independently via the existing Inbox mechanism.

```
CreateProduct / UpdateProduct / DeleteProduct / AddVariation / UpdateVariation / DeleteVariation
AssignProductCategory / RemoveProductCategory / AssignProductTag / RemoveProductTag
  (Command Handlers, Product.Application)
    ↓ IOutboxStore.EnqueueAsync — same transaction as the aggregate write, unchanged pattern
Outbox (Postgres) → OutboxRelayHostedService → Kafka
    ↓
Product.Infrastructure/Messaging/Consumers/*IntegrationEventConsumer
    (10 thin consumers, one per topic — deserialize, log, dispatch an internal event; no business logic)
    ↓ IInternalEventDispatcher.PublishAsync
OnProductSearchSyncRequiredEvent   (9 of the 10 consumers — every event except ProductDeleted)
    ↓ OnProductSearchSyncRequiredHandler:
       productRepo.GetByIdAsync → ProjectionBuilder.BuildAsync → IProductSearchIndexer.IndexAsync (upsert)
OnProductSearchRemovalRequiredEvent   (the ProductDeleted consumer only)
    ↓ OnProductSearchRemovalRequiredHandler: IProductSearchIndexer.DeleteAsync(productId)
```

**Why one handler rebuilds the whole document instead of applying partial updates:** every sync-triggering event (Created/Updated/VariationCreated/VariationUpdated/VariationDeleted/CategoryAssigned/CategoryRemoved/TagAssigned/TagRemoved) funnels into the same `OnProductSearchSyncRequiredEvent` → the handler reloads the current Product from Postgres and rebuilds the full document. This is simpler and strictly more correct than threading 9 different partial-update shapes through the indexer, and it means Postgres is always the source of truth for what gets indexed — the integration event is only a "something changed, go re-sync" signal, never a payload the index trusts directly.

**New integration events added for this feature:** `AssignProductCategory`/`RemoveProductCategory`/`AssignProductTag`/`RemoveProductTag` previously published no event at all. `ProductCategoryAssignedIntegrationEvent`, `ProductCategoryRemovedIntegrationEvent`, `ProductTagAssignedIntegrationEvent`, `ProductTagRemovedIntegrationEvent` (`BuildingBlock.Contract/Events/Product/`) were added purely to keep the Search index in sync — no other consumer needs them today.

**Product now has an Inbox table for the first time** (previously publish-only, see [inbox-outbox-runtime.md](inbox-outbox-runtime.md)) — self-consumption requires the same dedup/retry/dead-letter guarantees any other consumer gets. `AddInboxOutboxCleanupJobs(configuration)` replaces the old Outbox-only cleanup registration.

## Rebuild strategy

`POST /products/search/rebuild` (RequireAdmin) → `RebuildProductSearchIndexCommand` → `RebuildProductSearchIndexHandler`:

```
PostgreSQL → (paged, 200/batch, via IProductRepository.GetAllAsync) →
  ProjectionBuilder.BuildManyAsync → IProductSearchIndexer.BulkIndexAsync → Elasticsearch
```

`RecreateIndexAsync` runs once at the start (drop + recreate with the current mapping), then each batch is bulk-indexed. This reuses the **exact same** `ProductSearchProjectionBuilder` and `IProductSearchIndexer` the live sync path uses — proving the projection/indexing code is shared, not duplicated, between the event-driven and rebuild paths. This is the pattern any future service's rebuild endpoint should follow.

Runs synchronously in the command handler today (no background job) — appropriate for the current catalog scale. If catalogs grow large enough that this blocks a request for too long, the natural extension point is a Hangfire `IRecurringJob`/one-off job wrapping the same handler logic, not a redesign.

## Query flow

`GET /products` → `SearchProductsQuery` → `SearchProductsHandler` → `IProductSearchRepository.SearchAsync` → **Elasticsearch only**, never Postgres. This replaced the previous Postgres `ILIKE`-based `ListProductsQuery`/`ProductRepo.SearchAsync`, which were deleted as dead code once ES became the only Product-list path.

`GET /products/{productId}` (Product Detail) is untouched — still reads Postgres directly via `IProductRepository`. Only the *list/search* surface moved to Elasticsearch, per the task's explicit scope.

## User Search

Added 2026-07-28 (`docs/tasks/2026-07-28/`, Tasks 6-10). Repeats the Product pattern exactly at the architecture level — `User.Application/Abstractions/Search/` (interfaces + document/criteria) and `User.Persistence/Contexts/UserProfiles/Search/` (index name, mapping, indexer, repository), registered via `AddUserSearchServices(configuration)` from `User.Persistence/DependencyInjection.cs`'s `AddPersistence`. Three deliberate differences from Product, each driven by a concrete requirement Product never had:

**Accent/case-insensitive, word-order-independent name search.** Product's `Name`/`CategoryNames`/etc. only ever needed case-insensitivity, which the ES default `standard` analyzer gives for free. User's search needs to match `"Van A"` against `"Nguyen Van A"` regardless of accents, so `UserSearchIndexMapping` defines a **custom analyzer** (`user_search_name_analyzer`: `standard` tokenizer + built-in `lowercase`/`asciifolding` token filters — both ship in core Elasticsearch, no plugin needed) applied only to a dedicated `SearchName` text field (built by concatenating `FirstName`/`MiddleName`/`LastName`, non-empty parts only). Word-order independence isn't analyzer-specific — it falls out of a plain `multi_match` query's own per-term OR semantics once every term is folded/lowercased consistently. `DisplayName` stays a plain, unanalyzed `Keyword` so search results show the exact original name — the two fields deliberately have different analysis settings. This required a small, additive extension to `BuildingBlock.Search` itself (see above) since Product's original indexer signature had no way to pass custom index settings.

**`UserSearchDocument`** carries `UserId`, `FirstName`/`MiddleName`/`LastName` (stored, `index: false` — display-only, no longer individually searchable, see below), `DisplayName`, `SearchName`, `UserName`/`Email` (`Text` + `.keyword` subfield, same pattern as Product's `Name`), `PhoneNumber`/`PhoneSearch`/`PhoneReverse` (`Keyword`, reusing the same normalized-digit prefix/suffix columns the Postgres path already maintained), `Roles` (`Keyword` array), `Status` (`Keyword`), `CreatedAt`/`UpdatedAt` (`Date`).

**Individual `firstName`/`middleName`/`lastName` filters were retired**, in favor of the unified `keyword` search across `searchName`/`userName`/`email` in one `multi_match` — this is a strict improvement (the old Postgres-backed filters were case-sensitive and didn't cover `middleName` at all), not a regression. `UserCriteriaDefinition` (`User.Application/Features/Users/Search/`) is kept, narrowed, and repurposed as a **pure request-shape validator** for `SearchUsersValidator` — `CriteriaRequestValidator<T>` is engine-agnostic, so it still validates field/operator whitelists even though `SearchUsersHandler` no longer executes a Postgres query against it. Allowed operators there are kept in sync by hand with what `SearchUsersHandler.BuildCriteria`/`UserSearchRepository` actually implement (`role` eq/ne, `status` eq, `phone` sw/ew, `userName`/`email`/`createdAt`/`updatedAt` sort-only) — a mismatch here would mean a filter validates successfully and is then silently ignored.

**Sync events fire from four places, not the usual two.** `UserProfileCreatedIntegrationEvent` (already existed) and a new `UserProfileUpdatedIntegrationEvent` (added specifically for this — `UpdateUserHandler` previously published nothing at all, so the index would have gone silently stale on every profile edit) both flow through the standard Outbox → Kafka → self-consumption loop (`UserProfileCreatedSearchSyncConsumer`/`UserProfileUpdatedSearchSyncConsumer` in `User.Infrastructure/Messaging/Consumers/`, raising `OnUserSearchSyncRequiredEvent`). Two other creation/deletion paths dispatch the internal sync/removal event **directly**, skipping the self-consumption hop entirely: `OnUserInitiatedHandler` (Auth's self-registration path, already running in-process off a gRPC call — no cross-service boundary to decouple from) and `OnUserDeletionHandler` (already running off an inbound, Inbox-deduped Kafka message — a second hop would be redundant, not extra-safe). User already had an Inbox table before this feature (it already consumed `UserAccountDeletionIntegrationEvent`), unlike Product, which had to add its first Inbox table specifically for search self-consumption.

Rebuild (`POST /users/search/rebuild`, `RequireAdmin`, documented explicitly in Swagger from day one — closing the exact gap Product's equivalent endpoint initially left undocumented) and query flow (`GET` via `POST /users/search` → `SearchUsersQuery` → `SearchUsersHandler` → `IUserSearchRepository.SearchAsync`, Elasticsearch only) otherwise match Product's shape exactly — same paged-batch rebuild loop, same blocking `RecreateIndexAsync`, same fail-open `EnsureIndexAsync()` bootstrap in `Program.cs`.

**Operational note:** as of 2026-07-28, no Elasticsearch/Docker stack has been run against this code — `RebuildUserSearchIndex` must be run once against real data before `SearchUsers` is exercised in any real environment, or admins will see empty results (see `docs/tasks/2026-07-28/Task16_migration-and-reindex-review.md`).

## Future extension points

- **Order Search**: repeat the Product/User pattern — a `Contexts/<Aggregate>/Search/` folder inside that service's own `<Service>.Persistence` project (referencing `BuildingBlock.Search`), a document/criteria/repository/indexer/mapping quartet, a self-consuming (or cross-service-consuming, if the read model naturally lives in a different service) Kafka sync pipeline, and a rebuild command. Nothing in `BuildingBlock.Search` needs to change (its settings-aware overload, added for User, is already available to reuse), and no new `*.Persistence.Elasticsearch` project should be created.
- **Kibana dashboards**: Kibana is already provisioned in `docker-compose.yml`/`.override.yml` pointing at the shared `elasticsearch` container — any index created here (`product-search`, `user-search`, and future `order-search`) is visible in Kibana with zero additional plumbing.
- **Audit Analytics**: Audit Service could adopt the same `BuildingBlock.Search` indexer for an analytics-oriented index over `AuditLogEntry` data, independent of this feature.
- **Health checks / OpenTelemetry / metrics**: see "Deliberately not included yet" above — the extension points are documented, not implemented.
- **Blue/green reindexing via aliases**: implemented 2026-07-28 (`docs/tasks/2026-07-28/Task20_alias-based-blue-green-reindex.md`). Every index name (`product-search`, `user-search`, and any future service's) is now an ES alias; `EnsureIndexAsync`/`RecreateIndexAsync` manage a versioned concrete index behind it and swap the alias atomically (`Indices.UpdateAliases`, add-new + remove-old in one call) instead of a blocking drop+create. No caller or interface signature changed — Product's and User's `*SearchIndexer`/`*SearchRepository` classes are unaware of the alias indirection.
