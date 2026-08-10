# Task 2: Notification list returns null `category`/`type`/`title`

**Scope:** `ListMyUserNotifications` returns items where `priority`/`status` are populated but `category`/`type`/`title` are `null`, even though these are set at creation time.

## Reported response

```json
{
  "items": [
    {
      "id": "019f890d-612e-7725-9725-ff200645de41",
      "category": null,
      "type": null,
      "title": null,
      "priority": 2,
      "status": 2,
      "createdAt": "2026-07-22T08:59:43.278Z"
    }
  ],
  "nextCursor": null,
  "hasMore": false
}
```

## Investigation

Files: `Notification.Domain/Entities/UserNotification.cs`, `Notification.Domain/ValueObjects/{NotificationCategory,NotificationType,NotificationContent}.cs`, `Notification.Application/Features/UserNotifications/Commands/CreateUserNotification/CreateUserNotificationHandler.cs`, `Notification.Application/Features/UserNotifications/Queries/ListMyUserNotifications/ListMyUserNotificationsHandler.cs`, `Notification.Persistence/Repository/UserNotificationRepo.cs`, `BuildingBlock.Persistence.Mongo/DependencyInjection/ServiceCollectionExtensions.cs`.

**Ruled out: not a missing join.** `Category`/`Type`/`Content` are persisted directly on `UserNotification` (`UserNotification.cs:12-15`), not derived from a `NotificationTemplate` lookup — no such lookup exists anywhere in the create or list path.

**Ruled out: not a mapping bug.** Both handlers are written correctly:

- `CreateUserNotificationHandler.cs:14-27` builds `NotificationContent.Create(request.Title, request.Body)`, `NotificationCategory.Create(request.Category)`, `NotificationType.Create(request.Type)` and passes them into `UserNotification.Create(...)`.
- `ListMyUserNotificationsHandler.cs:26-27` projects `x.Category.Value, x.Type.Value, x.Content.Title, x.Priority, x.Status, x.CreatedAt` — correct C# expression.

**Actual root cause: MongoDB BSON serialization silently drops get-only value-object fields.**

`Notification.Persistence` is MongoDB-backed. The only Mongo serialization configuration anywhere in the codebase is `BuildingBlock.Persistence.Mongo/DependencyInjection/ServiceCollectionExtensions.cs:49-61` — a camelCase naming convention pack + `Guid` serializers. There is **no** `[BsonConstructor]`, `[BsonElement]`, custom `IBsonSerializer`, or manual `BsonClassMap` anywhere in `Notification.Domain`/`Notification.Persistence` (confirmed via full-project grep — zero hits).

`NotificationCategory.Value`, `NotificationType.Value`, `NotificationContent.Title`/`Body` are all **get-only properties backed by a private constructor** (e.g. `NotificationCategory.cs:15,17`). The MongoDB C# driver's default `BsonClassMap.AutoMap()` only maps members with both a getter *and* a setter (or an attributed constructor mapping) — get-only properties with no matching mapped constructor are silently skipped on serialization.

`UserNotification.Category`/`.Type` themselves *do* round-trip (they have `private set`), so a non-null wrapper object comes back on read — but its inner `Value`/`Title`/`Body` were never written, so they deserialize as `null`. `Priority`/`Status` are plain enums with private setters directly on the aggregate (no nested value object), which is exactly why they're unaffected.

**Verified empirically**: a standalone repro using the same `MongoDB.Driver`/`MongoDB.Bson` version (`3.10.0`) and identically-shaped classes (get-only `Value`/`Title`, private ctor, outer private-setter wrapper, same conventions registered) reproduces `"category": {}` / `"content": {}` on write, and `Category.Value` / `Content.Title` coming back `null` on read — matching the reported output exactly.

## Impact

**This is a write-time data-loss bug, not a read-time bug.** Every `UserNotification` document ever written by `CreateUserNotificationHandler` already has empty `category`/`content` subdocuments stored in MongoDB. A query-side fix alone cannot recover already-written documents — their original category/type/title values are gone unless recoverable from elsewhere (e.g. the originating `OrderConfirmedIntegrationEvent`/audit trail, on a case-by-case basis, not from this collection).

## Fix applied 2026-07-22

Registered explicit `BsonClassMap`s from the Persistence layer (not attributes on Domain — Domain has zero MongoDB.Bson reference and stays that way, per this repo's Clean Architecture layering rules). New shared, reusable helper:

- `BuildingBlock.Persistence.Mongo/Serialization/BsonImmutableValueObjectRegistrar.cs` — `Register<T>(params string[] memberNames)`. Uses reflection to bind the value object's private constructor (`GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)`), `AutoMap()` + explicit `MapMember` for each get-only property, then `MapConstructor` to wire deserialization. Guarded by `BsonClassMap.IsClassMapRegistered` for idempotency.
- Called once per value object from `Notification.Persistence/DependencyInjection.cs`'s new `RegisterValueObjectClassMaps()` — covers **all 8** value objects in `Notification.Domain.ValueObjects`, not just the 3 originally reported (`NotificationCategory`/`NotificationType`/`NotificationContent`): also `AudienceSelector`, `ChannelConfiguration`, `DispatchReference`, `NotificationSchedule`, `TemplateContent` — all share the exact same private-constructor + get-only-properties shape and were equally affected (confirmed by inspection, not just inference).
- Verified fixed empirically against the real `MongoDB.Driver` 3.10.0 package (not a stand-in repro) — before the fix, `NotificationCategory.Create("Order").ToBsonDocument()` serialized to `{ }`; after, `{ "value": "Order" }`, and a full `UserNotification` round-trip preserves `Category`/`Type`/`Content` correctly.
- Regression tests: `tests/unit/Notification.Persistence.Tests/BsonValueObjectSerializationTests.cs` (9 tests, all passing) — one round-trip test per value object plus a full-entity `UserNotification` round-trip.

**Also checked:** `Audit.Domain`'s Mongo-persisted entities (`AuditTrailFieldChange`, `AuditTrailMetadata`) use a *different*, safe shape (`private set` properties + parameterless constructor, not get-only + private constructor with args) — confirmed NOT affected by this bug class, no fix needed there.

## Historical data — decision made 2026-07-22

**Left as-is, by user decision.** This is a dev/test environment, not production with real user data — not worth writing a backfill script to recover already-corrupted `category`/`type`/`title` on pre-fix documents. Every `UserNotification` written from now on is correct.

## Status

**Fixed and tested.** No frontend action needed — this was backend-only; the API response shape (`category`/`type`/`title` string fields) is unchanged, just no longer null for newly-created notifications.
