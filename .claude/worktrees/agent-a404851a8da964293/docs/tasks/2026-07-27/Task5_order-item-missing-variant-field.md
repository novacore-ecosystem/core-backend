# Task 5: OrderItem has no distinct Variant field

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: "Order Detail: Product, Variant, Quantity, Price, Discount, Line Total."

## Current state

`OrderItem.cs:16-22` has `ProductId`, `ProductName`, `UnitPrice`, `Quantity`, `Discount`, `LineTotal`. There is no separate Variant field — `ProductId` is semantically the *variation* id already (per the comment in `OrderItemPreparationService.cs:20`), but only `ProductName` is stored/returned for display; no SKU or variant name is captured or surfaced anywhere in the Order model.

`GetOrderItemResponse` (`GetOrderQuery.cs:5-11`) and `GetOrderHandler.cs:32` correctly map Discount/LineTotal through, but there is nothing to map for Variant because the field doesn't exist.

## Why this matters

The requirement lists Product and Variant as two distinct fields on Order Detail. Today there is exactly one product/variant identifier with one display name — a customer's order can't show "T-Shirt — Size L / Red" distinctly from "T-Shirt."

## Open questions

- Should this store a denormalized variant name/SKU snapshot at order-creation time (matching how `CustomerName`/`CustomerPhone` are already snapshotted on `OrderOwner`), or look it up live from Product? Snapshotting is more consistent with the rest of the Order model and avoids a cross-service call on read.

## Suggested acceptance criteria

- `OrderItem` captures a variant identifier/name distinct from the product name (e.g. `VariantSku`/`VariantName`), snapshotted at order time.
- `GetOrderItemResponse` exposes it.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task10_order-item-variant-display.md` (blocked on this task).
