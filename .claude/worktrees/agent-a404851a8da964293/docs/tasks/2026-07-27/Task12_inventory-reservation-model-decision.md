# Task 12: Confirm whether Inventory's deduct-immediately + saga-compensate pattern satisfies the "reservation" requirement

**Status:** Open — decision needed, not necessarily a defect.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Inventory — verify "reservation (if any)."

## Current state

There is no `ReservedQuantity` field or three-state reserve→commit/release model. `DeductStockHandler.cs:64-129` decrements the actual `Quantity` synchronously (via gRPC) during the Create-Order saga (`RunCreateOrderSagaHandler.cs:34`), and `RestockStockHandler.cs` reverses it as saga compensation on failure, or on order cancel (`CancelOrderHandler.cs:31`). This is guarded by an xmin optimistic-concurrency retry loop (`MaxConcurrencyRetries = 3`) that functionally prevents overselling under concurrent orders today.

## Why this matters

This works, but it's architecturally different from a "reservation" model (temporarily hold stock, then commit or release). The audit couldn't determine whether the current model was an intentional simplification or an assumed shortcut.

## Suggested next step

Record a decision (ADR) on whether the current deduct-then-compensate model is sufficient long-term, or whether a true reserved-quantity state is required — e.g. if there's ever a need to show "N reserved for pending orders" separately from "N available," the current model can't represent that distinction.
