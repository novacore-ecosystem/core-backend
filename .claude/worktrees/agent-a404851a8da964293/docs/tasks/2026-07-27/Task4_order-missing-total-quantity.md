# Task 4: Order aggregate/DTOs have no `TotalQuantity` field

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: "Order information: Customer, Total Quantity, Total Amount."

## Current state

`Order.cs:14` computes `TotalAmount` (sum of item `LineTotal`s). A repo-wide grep for `TotalQuantity` finds **zero** hits on the Order aggregate or any Order DTO — the only `TotalQuantity` in the solution belongs to the unrelated Inventory service. `GetOrderResponse` (`GetOrderQuery.cs:13-24`) and `SearchOrdersItemResponse` (`SearchOrdersQuery.cs:8-16`) both expose `CustomerId/CustomerName/CustomerPhone/TotalAmount`, but nothing for total quantity.

## Why this matters

Total Quantity is one of three explicitly named fields for "Order information," alongside Customer and Total Amount. It is simply absent, not just unsurfaced on the frontend.

## Suggested acceptance criteria

- `TotalQuantity` (sum of item quantities) computed on the `Order` aggregate, alongside `TotalAmount`.
- Exposed on both `GetOrderResponse` and `SearchOrdersItemResponse`.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task9_order-total-quantity-display.md` (blocked on this task).
