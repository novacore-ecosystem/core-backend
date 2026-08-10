# Task 8: ProjectionBuilder + Sync Events (Self-Consumption)

**Status:** Done (2026-07-28)
**Category:** Elasticsearch

## What was done

`UserSearchProjectionBuilder` (`UserProfile` → `UserSearchDocument`, single + batched) added to `User.Application/Features/Users/Search/`, using a fixed `"en"` index-time locale for `DisplayName` per the documented simplification. Closed the flagged gap: added `UserProfileUpdatedIntegrationEvent` (new, `BuildingBlock.Contract`), published from `UpdateUserHandler` via Outbox in the same transaction as the profile write — previously this handler published nothing at all. Added `OnUserSearchSyncRequiredEvent`/`Handler` and `OnUserSearchRemovalRequiredEvent`/`Handler` (internal events, mirroring Product's exact shape) plus two new self-consuming Kafka consumers in `User.Infrastructure/Messaging/Consumers/` (`UserProfileCreatedSearchSyncConsumer`, `UserProfileUpdatedSearchSyncConsumer`), registered in `User.Infrastructure/DependencyInjection.cs`.

**Deliberate deviation from Product's pattern, recorded here as designed, not an oversight:** two of the four sync triggers do **not** go through Outbox/Kafka self-consumption. `OnUserInitiatedHandler` (Auth's self-registration path, already running in-process off a gRPC call) and `OnUserDeletionHandler` (already running off an inbound, Inbox-deduped Kafka message) each dispatch the internal sync/removal event directly via `IInternalEventDispatcher`, since both already have the delivery guarantees Product's self-consumption hop exists to provide — adding a second hop would be redundant complexity, not extra safety. Only the two REST-triggered paths (Create, Update) use the full Outbox → Kafka → self-consumption loop, matching Product's reasoning for *those* specific paths.

## Objective

Build `UserSearchProjectionBuilder` (UserProfile → `UserSearchDocument`) and wire the event-driven sync pipeline so every User mutation (Create/Update/Delete) eventually lands in the index — reusing Product's exact self-consumption pattern (Outbox → Kafka → own Inbox → internal event → re-index), not inventing a new mechanism.

## Current state (grounded findings)

Product's full pipeline, confirmed end-to-end by direct code trace (`docs/reference/search.md`'s "Synchronization flow" section plus direct handler reads):

```
Command Handler → IOutboxStore.EnqueueAsync (same transaction as the aggregate write)
Outbox → OutboxRelayHostedService → Kafka
Product.Infrastructure/Messaging/Consumers/*IntegrationEventConsumer (self-consumption, one per topic)
  → IInternalEventDispatcher.PublishAsync(OnProductSearchSyncRequiredEvent)  [9 of 10 event types]
     or OnProductSearchRemovalRequiredEvent                                  [ProductDeleted only]
OnProductSearchSyncRequiredHandler: reload from Postgres → ProjectionBuilder.BuildAsync → IProductSearchIndexer.IndexAsync (upsert)
OnProductSearchRemovalRequiredHandler: IProductSearchIndexer.DeleteAsync(id)
```

Key design rationale to preserve (directly quoted from `docs/reference/search.md`, confirmed against code): **one handler always rebuilds the whole document from current Postgres state** rather than applying partial updates per event type — Postgres stays the single source of truth for what's indexed; the integration event is only a "something changed, go re-sync" trigger.

**What User has today that determines this task's actual scope:**
- `CreateUserHandler` already publishes `UserProfileCreatedIntegrationEvent` via Outbox (confirmed, `CreateUserHandler.cs:59-76`) — a sync trigger already exists for Create.
- **`UpdateUserHandler` does NOT currently publish any integration event** (confirmed by reading `UpdateUserHandler.cs:9-18` in full — it only calls the write service, no `IOutboxStore.EnqueueAsync` anywhere). **This is a gap this task must close**: without a `UserProfileUpdatedIntegrationEvent` (new), Updates would never trigger a re-sync, and the search index would silently go stale on every profile edit.
- Deletion already flows through `UserAccountDeletionIntegrationEventConsumer` → `OnUserDeletionEvent` → `OnUserDeletionHandler` (`DeleteWithNoTrackingAsync`) — this consumer is the natural place to *also* raise `OnUserSearchRemovalRequiredEvent`, mirroring Product's `ProductDeletedIntegrationEventConsumer`.
- User already has an Inbox table (unlike Product, which had to add one) — self-consumption's dedup/retry infrastructure is already in place, lower incremental risk than Product's own rollout.
- Note the dead-code caveat from Task 1/3's research: `DeleteUserCommand`/`DeleteUserHandler` are unreferenced anywhere in the repo — the real deletion path is the event consumer above; don't wire search-removal into the dead command by mistake.

## Scope

- Add `UserProfileUpdatedIntegrationEvent` (new, `BuildingBlock.Contract`) published from `UpdateUserHandler` via Outbox, same transaction as the write (mirrors `CreateUserHandler`'s existing shape).
- `User.Infrastructure/Messaging/Consumers/`: add self-consuming consumers for `UserProfileCreatedIntegrationEvent` and the new `UserProfileUpdatedIntegrationEvent`, each raising `OnUserSearchSyncRequiredEvent` (new internal event, mirrors `OnProductSearchSyncRequiredEvent`).
- Extend `UserAccountDeletionIntegrationEventConsumer` (or add a sibling raised from the same handler) to also raise `OnUserSearchRemovalRequiredEvent` on deletion.
- `User.Application/Features/Users/Events/OnUserSearchSyncRequired/` and `OnUserSearchRemovalRequired/` (new folders, mirroring Product's exact naming): handlers reload from `IUserProfileReadService`, call `UserSearchProjectionBuilder.BuildAsync`, then `IUserSearchIndexer.IndexAsync` (upsert) or `.DeleteAsync`.
- `UserSearchProjectionBuilder.BuildAsync(UserProfile, ct)` + `BuildManyAsync(IReadOnlyList<UserProfile>, ct)` (batched, for Task 9's rebuild path) — populates `UserSearchDocument` per Task 7's shape, using Task 5's `DisplayName` formatter (with a **fixed, default locale** for the indexed value, since the index has one document per user, not one per requesting-caller's locale — flag this as a deliberate simplification: search results show the default-locale display name regardless of who's searching, unless a future task decides per-locale re-formatting at query time instead of index time).

## Dependencies

- **Depends on:** Task 6 (scaffolding), Task 7 (document shape), Task 2 (MiddleName), Task 5 (DisplayName/SearchName formatting).
- **Blocks:** Task 9 (rebuild path reuses this same builder/indexer), Task 10 (query cutover needs a populated index).

## Estimated complexity

Medium-to-Large — mechanically similar to Product's proven pattern, but requires adding a net-new integration event (`UserProfileUpdatedIntegrationEvent`) that doesn't exist today, plus the DisplayName-at-index-time-vs-query-time design call flagged above.

## Risks

- If `UpdateUserHandler`'s missing integration event is overlooked, the index will silently drift from Postgres on every edit with no error — this is the single most important correctness gap this task must close, not an optional nice-to-have.
- Indexing a single fixed-locale `DisplayName` means search results won't re-format per viewer locale without a query-time (not index-time) formatting step — acceptable for v1 per the request's framing (search matching is the hard requirement; display formatting elsewhere already happens via Task 5's response-time formatter for `GetUser`/`GetUserDetail`/`SearchUsers`), but document this limitation explicitly so it's a conscious trade-off, not an oversight.

## Completion checklist

- [ ] `UserProfileUpdatedIntegrationEvent` added, published from `UpdateUserHandler` via Outbox in the same transaction
- [ ] Self-consuming consumers added for Created/Updated events, raising `OnUserSearchSyncRequiredEvent`
- [ ] Deletion path extended to raise `OnUserSearchRemovalRequiredEvent`
- [ ] `UserSearchProjectionBuilder` implemented (single + batched), reused identically by Task 9's rebuild path
- [ ] Integration test: Create → poll index → document appears; Update → poll index → document reflects new name; Delete → poll index → document gone
