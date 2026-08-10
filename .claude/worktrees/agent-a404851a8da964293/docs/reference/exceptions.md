# Reference: Exception Handling

**Scope:** the exception hierarchy, `MessageCode` catalogue, and the central mapping to HTTP responses. Merges and supersedes the old `EXCEPTIONS.md` + `EXCEPTION_PATTERNS.md` (archived — see [08-migration-plan.md](../08-migration-plan.md)). The rule ("what must you throw") lives in [02-architecture-rules.md](../02-architecture-rules.md#exception-rule) — this doc is the lookup table behind that rule.

## Two layers

### Domain exceptions (`BuildingBlock.Domain.Exceptions`)
Business-rule violations, no HTTP awareness. Base: `DomainException` (abstract, carries `MessageCode`). Concrete types, created via `ExceptionFactory` (never `new` them directly — the factory method names are self-documenting and the only way `SystemMessage` gets attached consistently):

| Factory method | Exception | HTTP status (via `ExceptionHandlerHelper`) |
|---|---|---|
| `ExceptionFactory.EntityNotFound(msg)` / `EntityNotFound<T>(id)` | `EntityNotFoundException` | 404 |
| `ExceptionFactory.InvalidState(msg)` | `InvalidStateException` | 400 |
| `ExceptionFactory.InvalidStatus(msg)` | `InvalidStatusException` | 400 |
| `ExceptionFactory.EmptyCollection(msg)` | `EmptyCollectionException` | 400 |
| `ExceptionFactory.EmptyItems(msg)` | `EmptyItemsException` | 400 |
| `ExceptionFactory.InsufficientStock/Balance/Quota(msg)` | `InsufficientAmountException` | 400 |
| `ExceptionFactory.Duplicate(msg)` / `UniqueConstraintViolation(msg)` | `BusinessRuleException` | 400 |
| `ExceptionFactory.InvalidEnumValue/InvalidRange/ValueTooSmall/ValueTooLarge/InvalidFormat(msg)` | `InvalidArgumentException` | 400 |
| `ExceptionFactory.RequiredField/RequiredNotEmpty(msg)` | `InvalidArgumentException` | 400 |

Every domain exception status is hardcoded in `ExceptionHandlerHelper`'s switch except `EntityNotFoundException` (404) — see [07-solid-recommendations.md](../07-solid-recommendations.md#openclosed) for the extensibility gap this creates if you need a domain exception with a different status.

### Application exceptions (`BuildingBlock.Application.Exceptions`)
HTTP-aware, thrown directly by handlers. Base: `ApplicationException` (abstract, carries its own `StatusCode`):

| Exception | Status | Use for |
|---|---|---|
| `BadRequestException` | 400 | Generic bad request |
| `ValidationException` | 400 | Field-level validation (also thrown automatically by `ValidationBehavior<,>` on FluentValidation failure — don't throw this yourself for that case) |
| `UnauthorizedException` | 401 | Not authenticated |
| `ForbiddenException` | 403 | Authenticated but not permitted |
| `NotFoundException` | 404 | Resource not found (`new NotFoundException("Entity", id)` convenience overload) |
| `ConflictException` | 409 | Duplicate/state conflict |

## Central mapping

`BuildingBlock.Infrastructure/ExceptionHandling/ExceptionHandlerHelper.cs` — static `HandleException(Exception)`, returns `ExceptionHandlingResult { StatusCode, MessageCode, ApiResponse, LogMessage, StackTrace, InnerException }`. Dispatch: `ApplicationException` → own `StatusCode`; `DomainException` → hardcoded switch (table above); anything else → 500, masked client message, full detail only in `LogMessage`.

Consumed by `BuildingBlock.Web/ExceptionHandling/GlobalExceptionHandler.cs` (`IExceptionHandler`), wired for every service via `AddBuildingBlockWeb`/`UseBuildingBlockWeb` — **never write a per-service exception handler.** Behavior differs by environment: `Development` uses `UseDeveloperExceptionPage()` (full framework error page, `GlobalExceptionHandler` is *not* invoked); everything else uses `UseExceptionHandler("/error")`, which does invoke it.

Log format (Seq): `[{StatusCode}] [Client Message] ... [System Message] ... [Inner Exception] ... [Stack Trace] ...` — search Seq by status code or message text when triaging, see [troubleshooting/seq.md](../troubleshooting/seq.md).

## MessageCode ranges (`BuildingBlock.Domain/Enums/MessageCode.cs`)

| Range | Category |
|---|---|
| 001-099 | System messages (success/error/timeout) |
| 100-199 | Validation errors |
| 200-299 | General client errors |
| 300-399 | Authentication & authorization |
| 400-499 | Product Service |
| 500-599 | Inventory Service |
| 600-699 | Order Service |
| 700-799 | User Service |
| 800-899 | Payment Service |

Pick the correct per-service range when adding a new code. Full enum (~800 entries) is not reproduced here — open the file directly when you need a specific code, it's organized by these same range comments.

## Response shape

```json
{ "success": false, "message": "Client-facing message from MessageCode", "messageCode": "102", "data": null, "details": null }
```
No auto-exposed internals — only what's explicitly passed via `details` reaches the client. `ApiResponse<T>.Ok(...)`/`.Fail(...)` (`BuildingBlock.Application/Abstractions/Common/ApiResponse.cs`) is the only response wrapper used across all endpoints.
