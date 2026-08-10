# Reference: gRPC

**Scope:** the gRPC client/server building blocks (`BuildingBlock.Grpc`, `BuildingBlock.Contract`). Condensed and pruned from the former `building-blocks/GRPC.md`, which self-flagged large sections (streaming, retry policy, service-mesh discovery) as unconfirmed against the actual current implementation — those sections are **not** carried forward here; verify against source before relying on anything beyond what's below. Three call chains today: Auth → User `CreateUserProfile`; Order/Product → Inventory (`GetProductStock`/`GetProductsStock` read-only checks, `DeductStock`/`RestockStock` in the CreateOrder saga — see [reference/create-order-saga.md](create-order-saga.md) and [services/inventory-service.md](../services/inventory-service.md#grpc-inventorygrpcservice)); and Audit → User `GetUser` (added 2026-07-28, see below) — User's first read-oriented RPCs and its first read consumer anywhere in the repo (previously `CreateUserProfile` was User's only RPC, write-only).

## Contract-first

`.proto` files live in `BuildingBlock.Contract/Protos/` (currently `user.proto`), compiled with `GrpcServices="Both"` — generates both client and server stubs into `BuildingBlock.Contract.Protos.{X}` namespace. Add a new RPC here first, then implement client/server usage.

## Server side

```csharp
// {Service}.Infrastructure or API DependencyInjection
services.AddGrpcServer();   // BuildingBlock.Grpc.Server — wires LoggingInterceptor + ErrorHandlingInterceptor + health check
// {Service}.API/ApplicationPipeline.cs
app.MapGrpcServices();      // sets health status to Serving
app.MapGrpcService<{X}GrpcServiceImpl>();
```
Implement the generated `{X}GrpcServiceBase`, e.g. `User.API/GrpcServices/UserGrpcServiceImpl.cs` — keep it a thin adapter: parse request, dispatch a Command via `ISender`, no business logic.

## Client side

```csharp
services.AddGrpcClient<{X}GrpcService.{X}GrpcServiceClient>(new Uri(url));
```
10MB max message size + gzip decompression by default (`BuildingBlock.Grpc/Client/GrpcClientExtensions.cs`). Example: `Auth.Infrastructure/GrpcClients/UserProfileServiceClient.cs` wraps the generated client behind a service-specific interface (`IUserProfileService`) so the Application layer never touches gRPC types directly.

## Batch RPCs: never a loop of single calls

`inventory.proto`'s `GetProductsStock` (`repeated string variant_ids` in, `repeated VariantStock items` out — every requested id gets an item back, `total_quantity: 0` if absent, never omitted) is the template every batch RPC in this repo should follow. `user.proto`'s `GetUsers` (added 2026-07-28) repeats the same shape: `repeated string user_ids` in, `repeated UserProfileItem` out, each item carrying a `found` bool instead of omission — a caller never has to guess which requested ids didn't resolve. Server-side (`UserGrpcServiceImpl.GetUsers`), the handler chain (`GetUsersByIdsQuery` → `CachedUserProfileReader.GetManyAsync`, see [reference/caching.md](caching.md#user-detail-cache)) does exactly one DB round trip for whatever wasn't already cached — the whole point of a batch RPC is defeated if the server-side implementation quietly loops single lookups instead.

`Audit.Infrastructure/GrpcClients/UserClientService.cs` is the first (and so far only) consumer of `GetUser` (single, not batch — one actor per `GetAuditLog` call) — a thin adapter, registered via the same `AddGrpcClient<T>()` + per-service `AddGrpcClients()` convention as every other client in this doc. It's also the first gRPC call in this repo used purely for **display enrichment** rather than a business-logic decision: `GetAuditLogHandler` resolves `Metadata.Actor` (a UserId) to a display name for the Audit Trail UI, fail-open (any exception or unparseable Actor just means `ActorDisplayName` stays `null` — the audit-log read itself never fails because of this).

## Interceptors (server-side, applied automatically by `AddGrpcServer()`)

- `ErrorHandlingInterceptor` — maps `ArgumentNullException`→`InvalidArgument`, `InvalidOperationException`→`FailedPrecondition`, `UnauthorizedAccessException`→`Unauthenticated`, else→`Internal`, all as `RpcException`.
- `LoggingInterceptor` — logs method/peer/duration/status.

## When to use gRPC vs an integration event

gRPC: synchronous, same-transaction-adjacent need for a response (e.g. "create this profile and tell me if it worked"). Integration event: fire-and-forget notification another service should eventually react to. Auth's registration flow uses gRPC because it needs to know profile creation succeeded before completing registration — see [services/auth-service.md](../services/auth-service.md).
