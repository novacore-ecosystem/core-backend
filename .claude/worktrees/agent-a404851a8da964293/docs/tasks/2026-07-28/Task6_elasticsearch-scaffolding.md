# Task 6: Scaffold User Elasticsearch Search (Mirror Product Architecture)

**Status:** Done (2026-07-28)
**Category:** Elasticsearch

## What was done

Scaffolded exactly per plan: `User.Application/Abstractions/Search/{IUserSearchIndexer,IUserSearchRepository,UserSearchCriteria,UserSearchDocument}.cs` and `User.Persistence/Contexts/UserProfiles/Search/{UserSearchIndexNames.cs, Mapping/UserSearchIndexMapping.cs, Indexers/UserSearchIndexer.cs, Repositories/UserSearchRepository.cs}` — flat layout beside `Read/`/`Write/`/`Repositories/`, not Product's `Engine/`/`Reliability/`/`Storage/` regrouping, per the persistence-coding-conventions distinction for single-aggregate services. `AddUserSearchServices(configuration)` added to `User.Persistence/DependencyInjection.cs`'s `AddPersistence` chain. Added the `BuildingBlock.Search` project reference to `User.Persistence.csproj` (previously missing). `BuildingBlock.Search` itself needed one additive change (see Task 7) but no behavior change for its existing Product consumer — verified via a standalone `Product.API` build.

## Objective

Stand up the same layered Elasticsearch architecture Product already has — reused verbatim at the infrastructure level (`BuildingBlock.Search`), replicated at the per-service level (`User.Application/Abstractions/Search`, `User.Persistence/Contexts/UserProfiles/Search/`) — with **zero** new `*.Persistence.Elasticsearch` project, per this repo's explicit, documented rule.

## Current state (grounded findings)

The canonical reference is `docs/reference/search.md` (read in full) plus `docs/conventions/persistence-coding-conventions.md`'s "Search belongs beside Read/Write/Repositories" section — both already document the exact rule to follow, quoted directly:

> "There is **no** `*.Persistence.Elasticsearch` (or `*.Persistence.<Technology>`) peer project for any service: the reusable, technology-specific 20% ... lives in `BuildingBlock.Search`; everything document/mapping/query-shaped is Product-specific and stays inside `Product.Persistence`."

Product's exact file layout to replicate (all confirmed by direct read, one-for-one User equivalents in brackets):

- `Product.Application/Abstractions/Search/{IProductSearchIndexer,IProductSearchRepository,ProductSearchCriteria,ProductSearchDocument}.cs` → `User.Application/Abstractions/Search/{IUserSearchIndexer,IUserSearchRepository,UserSearchCriteria,UserSearchDocument}.cs`
- `Product.Application/Features/Products/Search/ProductSearchProjectionBuilder.cs` → `User.Application/Features/Users/Search/UserSearchProjectionBuilder.cs` (this sits **alongside**, not replacing, the existing `UserCriteriaDefinition.cs` in the same folder — see Task 10 for that file's eventual fate)
- `Product.Persistence/Contexts/Products/Search/{ProductSearchIndexNames.cs, Mapping/ProductSearchIndexMapping.cs, Indexers/ProductSearchIndexer.cs, Repositories/ProductSearchRepository.cs}` → `User.Persistence/Contexts/UserProfiles/Search/{UserSearchIndexNames.cs, Mapping/UserSearchIndexMapping.cs, Indexers/UserSearchIndexer.cs, Repositories/UserSearchRepository.cs}`
- DI: `Product.Persistence/DependencyInjection.cs`'s private `AddProductSearchServices(configuration)` (calls `services.AddElasticsearchClient(configuration)` then registers the two Product-specific indexer/repository types), called from the public `AddPersistence(configuration)` chain — **not** a separate `Program.cs` step. User's `AddPersistence` (`User.Persistence/DependencyInjection.cs`) gets the identical `AddUserSearchServices(configuration)` addition.
- `BuildingBlock.Search` itself needs **zero changes** — its 4 files (`Abstractions/IElasticsearchIndexer.cs`, `Indexing/ElasticsearchIndexer.cs`, `Configuration/ElasticsearchOptions.cs`, `DependencyInjection/ServiceCollectionExtensions.cs`) are already fully generic (open-generic `IElasticsearchIndexer<TDocument>`, singleton `ElasticsearchClient`).

**One structural difference from Product worth noting, not fixing:** Product had to add its **first-ever Inbox table** specifically to support self-consuming its own integration events for search sync (`Product.Persistence/DependencyInjection.cs:129` comment). **User already has an Inbox table** (`User.Persistence/Reliability/Inbox/InboxStore.cs`, since it already consumes `UserAccountDeletionIntegrationEvent`) — so this particular Product-specific migration step does not need repeating for User; lower risk here than Product's own rollout.

## Scope

- Create the four `User.Application/Abstractions/Search/` files (interfaces + criteria/document records — content is Task 7's concern, this task is the scaffolding/wiring).
- Create the `User.Persistence/Contexts/UserProfiles/Search/` folder structure (mapping/indexer/repository — again, content in Task 7).
- Add `AddUserSearchServices(configuration)` to `User.Persistence/DependencyInjection.cs`'s `AddPersistence` chain, following Product's exact method-naming convention (business capability name, not `AddElasticsearchPersistence`).
- No `Program.cs` changes beyond what's already there (`AddPersistence(...)` already gets called; the new capability rides inside it) — except the `EnsureIndexAsync()` bootstrap call, covered in Task 9.

## Dependencies

- **Depends on:** nothing structural (this is pure scaffolding) — but sequence it after Task 2 (MiddleName) so the document shape (Task 7) doesn't need a rework immediately after this lands.
- **Blocks:** Task 7 (document/mapping content), Task 8 (projection builder + sync), Task 9 (rebuild command + config), Task 10 (query cutover).

## Estimated complexity

Small — this task is pure structural mirroring of an already-proven pattern; the actual design decisions (document shape, accent-insensitive mapping, query composition) are deliberately deferred to Tasks 7/8/10 so this task stays a clean, low-risk "make the folders and DI wiring exist" step.

## Risks

- Low risk overall since it's copying a proven pattern — the main risk is copying Product's `Contexts/` grouping convention *incorrectly*: per `persistence-coding-conventions.md`, User (a single-aggregate service) should use the flat layout (`Contexts/UserProfiles/Search/` directly under the project root beside `Read/`/`Write/`/`Repositories/`), matching how User already organizes `Contexts/UserProfiles/{Read,Write,Repositories}/` today — don't additionally copy Product's `Engine/`/`Reliability/`/`Storage/` regrouping, which the docs explicitly call out as "Product-specific... not required for other services."

## Completion checklist

- [ ] `User.Application/Abstractions/Search/` scaffolded (empty/interface-only records acceptable at this stage, filled in Task 7)
- [ ] `User.Persistence/Contexts/UserProfiles/Search/` scaffolded (mapping/indexer/repository stubs)
- [ ] `AddUserSearchServices(configuration)` wired into `AddPersistence`, confirmed via a scoped build that DI resolves without error
- [ ] Confirmed no new `*.Persistence.Elasticsearch` project was created, no changes needed in `BuildingBlock.Search`
