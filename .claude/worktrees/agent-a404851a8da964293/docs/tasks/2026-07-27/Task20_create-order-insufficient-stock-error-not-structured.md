# Task 20: Create Order's insufficient-stock error has no structured per-item detail

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`OrderItemPreparationService.EnsureStockAvailableAsync` (`src/Services/Order/Order.Application/Features/Orders/Common/OrderItemPreparationService.cs:53-65`) does a real, batched, per-variation Inventory check and builds a human-readable itemized string, but:

```csharp
throw ExceptionFactory.InsufficientStock($"Insufficient stock for: {string.Join(", ", insufficient)}");
```

`ExceptionFactory.InsufficientStock(string)` → `InsufficientAmountException(string? systemMessage)` (`BuildingBlock.Domain/Exceptions/InsufficientAmountException.cs:11-12`) → base `DomainException(MessageCodeEnum.InsufficientStock, systemMessage)` with `detail: null` by default (`DomainException.cs:13-22`). The itemized string goes only into `SystemMessage` (server log, via `LogMessage`), never into a client-visible structured field. `ExceptionHandlerHelper.HandleDomainException` (`BuildingBlock.Infrastructure/ExceptionHandling/ExceptionHandlerHelper.cs:143-156`) returns `ApiResponse<object?>.Fail(clientMessage, ex.MessageCode, details: ex.ErrorDetails)` where `details` is always `null` for this path, and `clientMessage` is the generic string from `MessageCode.cs:106-107` ("Insufficient stock available"). Actual client-visible body today:

```json
{"success": false, "message": "Insufficient stock available", "messageCode": "500", "data": null, "details": null}
```

## Why this matters

Checklist requirement: on insufficient stock, the response should carry structured detail (e.g. `{"detail":{"insufficients":["variation-id-1","variation-id-2"]}}`) so the frontend can mark/disable the specific failing line items. Today the client gets a generic message with no way to know *which* item(s) failed without out-of-band information.

## Suggested acceptance criteria

- `InsufficientAmountException` (or a new subclass) carries a structured `ErrorDetails` payload — e.g. a list of `{ variationId, requested, available }` — populated from the same data `EnsureStockAvailableAsync` already computes for its log string (don't compute it twice).
- `ExceptionHandlerHelper`'s existing `details: ex.ErrorDetails` passthrough already supports this once the exception actually sets it — confirm no additional handler changes are needed, or note them if the `object?` shape needs a concrete DTO for consistent JSON serialization.
- Reuse this same structured shape for Task 18 (Add Cart)'s rejection, so the frontend has exactly one error contract to parse for "insufficient stock," not two.
- Update `EnsureStockAvailableAsync` to build the shared service call from Task 17 instead of its own private inline logic, once Task 17 lands (sequencing note, not a blocker for this task alone since the check itself already works — only the response shape is missing).

## What was done

- `InsufficientAmountException` gained an optional `detail` parameter on its two general-purpose constructors, passed through to the `DomainException` base's existing `ErrorDetails`; `ExceptionFactory.InsufficientStock(string, object?)` exposes it.
- `OrderItemPreparationService.EnsureStockAvailableAsync` (now built on top of Task 17's `IStockAvailabilityService`) builds `new { insufficients = insufficientItems.Select(i => i.VariationId).ToArray() }` and passes it as `detail`. Actual client-visible body is now `{"success": false, "message": "...", "messageCode": "...", "data": null, "details": {"insufficients": ["<guid>", ...]}}` - `ExceptionHandlerHelper.HandleDomainException` already forwarded `ex.ErrorDetails` into the response's `details` field, so no handler change was needed once the exception actually populated it.
- The per-item log message (requested/available quantities) is preserved in `SystemMessage` as before - `detail` only carries the IDs, matching the checklist's example shape, not the full diagnostic string (which stays server-log-only, consistent with the original design intent).
- Scoped build of `Order.API` passes.

## What wasn't done

`detail` is an anonymous type (`object?` on the exception, matching the existing `ConflictException` convention elsewhere in the codebase of passing anonymous objects) rather than a named DTO - consistent with how `ErrorDetails` is used elsewhere, but means there's no compile-time-checked contract for consumers. Not introducing a named type since nothing else in the codebase's exception `detail` payloads does either.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task17_add-to-cart-checkout-stock-error-handling-missing.md`.
