# Task 20 — Alias-based blue/green reindex infra (Product + User)

Status: **Implemented.** Prerequisite for Task 19's mapping-changing tasks (21-24).

## Why

`Task19_search-relevance-audit-and-plan.md` recommends this land first: every later task
(21, 22, 23, 24) changes a field's ES type (e.g. `Text` → `search_as_you_type`), which ES
rejects as an in-place mapping update - each requires a full reindex. The existing
`RecreateIndexAsync` did a blocking `Delete` + `Create`, leaving the index (and therefore
search) briefly nonexistent during every future reindex. This task removes that gap without
changing any caller's code.

## What changed

Only `BuildingBlock.Search` changed - no mapping, no query, no interface signature, no
caller (`ProductSearchIndexer`/`UserSearchIndexer`/the Rebuild handlers) touched:

- `Indexing/ElasticsearchIndexer.cs` - the literal string every caller passes as
  `indexName` (`"product-search"`, `"user-search"`) is now treated as an **ES alias**, not
  a concrete index name:
  - `EnsureIndexAsync`: no-op if the alias already exists (assumes its backing index is
    fine). Otherwise creates a new versioned index (`{alias}-{yyyyMMddHHmmssfff}`) with the
    alias attached atomically at creation (`CreateIndexRequestDescriptor.AddAlias`).
  - `RecreateIndexAsync`: creates a new versioned index, then does one atomic
    `Indices.UpdateAliases` call (`Add` the new index + `Remove` the old index(es) from the
    alias in a single request - ES applies both halves atomically, so the alias is never
    briefly unbound), then best-effort-deletes the old concrete index(es).
  - `IndexAsync`/`DeleteAsync`/`BulkIndexAsync` - unchanged; ES resolves single-doc and bulk
    writes against an alias exactly like a concrete index, as long as exactly one index is
    behind it (guaranteed by the swap logic above).
  - A one-time migration guard (`MigrateLegacyConcreteIndexIfPresentAsync`): if a **plain**
    index already exists with the exact alias name (the pre-Task-20 shape), it's dropped
    before creating the alias infra, since ES forbids an alias and a concrete index sharing
    a name. No environment has ever run this code against live Elasticsearch data (see
    `docs/reference/search.md`'s "Operational note"), so dropping rather than building
    reindex-migration machinery for a state this repo's history never reached is the
    correct amount of handling, not a shortcut around real data loss.
- `IElasticsearchIndexer<TDocument>` - doc comments only, clarifying `indexName` is an
  alias and describing the new `RecreateIndexAsync` swap behavior. No signature change.
- `ProductSearchIndexNames`/`UserSearchIndexNames`, `ProductSearchIndexer`/
  `UserSearchIndexer` - doc comments only, same clarification. No code change - these
  classes were already unaware of *how* the index name resolves, so nothing to touch.

## Verified

`dotnet build` succeeds with zero warnings for `BuildingBlock.Search`, and zero *new*
warnings for `Product.API`/`User.API` (both pre-existing MSB4011/MSB3026 warnings are
unrelated parallel-build races, confirmed present before this change too). No live
Elasticsearch instance was run against this change - same operational caveat as the rest
of the User Search epic (`docs/tasks/2026-07-28/Task16_migration-and-reindex-review.md`).
Live verification (alias created correctly, swap leaves zero read-unavailable window,
old-generation cleanup) is deferred to Task 27 (testing), same as every other task in
this epic that needs a running ES container.

## Bug found and fixed via live Docker run (post-merge)

`POST /products/search/rebuild` against a real cluster failed with a 400
`illegal_argument_exception: "The provided expression [product-search] matches an alias,
specify the corresponding concrete indices instead"` on every call after the first. Root
cause: `Indices.ExistsAsync(name)` resolves aliases too - it returns `Exists: true` once
`product-search` is already alias-managed (from the prior `EnsureIndexAsync` call at
startup), not only for a genuine pre-Task-20 concrete index. `MigrateLegacyConcreteIndexIfPresentAsync`
was checking only `ExistsAsync`, so it mis-detected the already-migrated alias as
"legacy" and tried to `DELETE` it as a concrete index - which ES rejects.

Fixed by checking `Indices.ExistsAliasAsync` first and returning immediately (nothing to
migrate) whenever the name is already alias-managed; only falls through to the
concrete-index check/delete when it's genuinely not an alias. Verified via `dotnet build`
across `BuildingBlock.Search`/`Product.API`; re-verify live via the same
`/products/search/rebuild` call before considering this task closed.

## Next

Task 21 (User: `SearchName`/`UserName` → `search_as_you_type` + tiered query) can now
proceed - it will trigger `RecreateIndexAsync`'s new swap path the first time it ships.
