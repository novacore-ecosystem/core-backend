# SOLID Recommendations

**Scope:** documentation-only review of current architecture against SOLID. Per the constraints of this audit, **no code was changed** — this is analysis and suggested future direction, not a refactor.

## Single Responsibility

**Good:** Layer separation is clean — Domain has no framework concerns, Application has no infrastructure concerns, endpoints are pure adapters (bind → send → return). `IntegrationEventConsumerRegistry` isolates per-consumer failures so one broken consumer's responsibility doesn't leak into another's.

**Recommendation — split `BuildingBlock.Web`'s composition role from its individual extensions.** `AddBuildingBlockWeb`/`UseBuildingBlockWeb` bundle seven unrelated concerns (current-user, exceptions, JWT, Swagger, CORS, Carter, health checks) behind one call. Each piece is independently well-scoped; the *bundler* is doing orchestration, which is fine, but a consumer that wants five of the seven has no way to opt out without unbundling by hand. Recommend: keep the bundler for the common case (what both Auth and User use today), but document (as this docset now does) that the individual `Add*`/`Use*` extensions are first-class, so a future minimal consumer (e.g. a third service that doesn't need Swagger) can compose only what it needs — same principle already correctly applied for the Gateway, which uses `BuildingBlock.Web`'s `RefreshTokenCacheExtensions` in isolation rather than the full bundle.

## Open/Closed

**Good:** `ExceptionFactory` and the `MessageCode` enum let new exception types/codes be added without modifying `ExceptionHandlerHelper`'s Application-exception branch (it dispatches on the exception's own `StatusCode`/`MessageCode`, not a hardcoded switch). `IIntegrationEventConsumer` fan-out means new consumers don't require touching the dispatch registry.

**Gap:** `ExceptionHandlerHelper.HandleException`'s **Domain**-exception branch is a hardcoded `switch` on concrete types (`EntityNotFoundException` → 404, everything else → 400). Adding a new domain exception that should map to a different status code (e.g. a future `ConcurrencyConflictException` → 409) requires modifying this switch — a closed-for-extension point today. **Recommendation:** consider giving `DomainException` itself an optional status-code hint (defaulting to 400) the way `ApplicationException` already does, so new domain exceptions can opt into a specific status without touching the central mapper. Do not do this reactively per-exception — it's a one-time structural change worth planning deliberately.

## Liskov Substitution

**Good:** `CachedAuthServiceDecorator : IAuthService` is a textbook LSP-compliant decorator — callers can't tell the difference between the cached and uncached implementation.

**Watch:** the two User Service handlers that throw raw `InvalidOperationException` instead of `NotFoundException` (see [services/user-service.md](services/user-service.md#known-issues)) are not an LSP violation per se, but they violate the *implicit contract* every other handler upholds ("failures throw Application/Domain exceptions, nothing else") — from a caller's perspective, substituting one handler for another currently changes error-handling behavior in a way it shouldn't. Fixing this restores the substitutability the rest of the codebase already has.

## Interface Segregation

**Good:** `ICacheService`, `IAppLogger<T>`, `ICurrentUserService` are each narrowly scoped to one concern. `IRepository<T>` is intentionally minimal (Get/Add/Update/Delete) rather than a fat generic repository — per-aggregate interfaces extend it only when needed (Auth's pattern).

**Gap:** `ICurrentUserService` mixes two concerns that don't always travel together — *identity/claims reading* (GetUserId/GetRoles/IsAuthenticated) and *cookie management* (SetAccessToken/SetRefreshToken/GetAccessToken/RemoveAccessToken/GetIpAddress). Every consumer that only needs "who is the current user" (the vast majority — most query handlers) is coupled to an interface that also exposes token-cookie mutation, which only auth-flow code (Login/Refresh/Logout) actually needs. **Recommendation:** consider splitting into `ICurrentUserService` (read-only identity) and `ITokenCookieService` (cookie read/write), implemented by the same concrete `CurrentUserService` if convenient, so most handlers depend on a narrower contract. Not urgent — no current pain reported — but worth doing before a third service starts consuming `ICurrentUserService` broadly.

## Dependency Inversion

**Good:** this is the architecture's strongest area. Domain never depends downward; Application depends only on its own abstractions (`IRepository<T>`, `ICacheService`, etc.), never on Infrastructure/Persistence concretes; DI composition roots are the only place concretes get wired to abstractions; `BuildingBlock.Messaging` (abstraction) vs `BuildingBlock.Messaging.Kafka` (adapter) is a clean hexagonal boundary that would let a future broker swap happen without touching any service's Application/Domain code.

**Gap:** `BuildingBlock.Web`'s dependency on `BuildingBlock.Infrastructure` (for `GlobalExceptionHandler` → `ExceptionHandlerHelper`) means anything that references `BuildingBlock.Web` transitively pulls in all of Infrastructure's packages (Redis, Authorization, Scrutor, etc.), even a minimal consumer like the Gateway that only wants the refresh-token Redis lookup. This was a deliberate, documented tradeoff (see [decisions/buildingblock-web-extraction.md](decisions/buildingblock-web-extraction.md)) made to avoid a bigger restructuring mid-task — flagging it here as the honest DIP gap it is. **Recommendation for future work:** if a third consumer of `BuildingBlock.Web` emerges with even tighter dependency constraints than the Gateway, consider extracting `ExceptionHandlerHelper`'s pure mapping logic (it has no ASP.NET Core dependency itself) into `BuildingBlock.Application` or a new minimal project, so `BuildingBlock.Web` doesn't need a full `BuildingBlock.Infrastructure` reference just for exception formatting.

## Cross-cutting observation: Mapster is dead code, not a pattern

Both Auth and User register Mapster (`AddMapster()`) but never call `.Adapt<T>()` anywhere — every handler/endpoint hand-maps DTOs. This isn't a SOLID violation, but documenting it as "the current pattern" (which this docset does, deliberately, in [04-coding-rules.md](04-coding-rules.md#mapping)) rather than silently treating unused registered infrastructure as authoritative avoids a future implementer introducing Mapster usage in one feature and not others, creating an inconsistency. **Recommendation:** either commit to Mapster (introduce it consistently, likely starting with the highest-boilerplate mapping in each service) or remove the registration — leaving it half-wired is the actual smell, not the choice of manual mapping itself.
