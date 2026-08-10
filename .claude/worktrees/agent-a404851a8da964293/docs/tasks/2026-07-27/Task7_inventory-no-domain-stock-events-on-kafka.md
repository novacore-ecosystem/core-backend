# Task 7: Inventory publishes no domain-specific stock events over Kafka

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27. Requirement: Inventory — verify "Kafka integration."

## Current state

Inventory only *consumes* Product-domain events: `ProductVariationCreatedIntegrationEventConsumer.cs`, `ProductVariationDeletedIntegrationEventConsumer.cs`, `ProductDeletedIntegrationEventConsumer.cs` (registered in `Inventory.Infrastructure/DependencyInjection.cs:45-47`).

Order↔Inventory integration does **not** go through Kafka at all — it's a direct gRPC call inside the Create-Order saga (`Order.Application/.../RunCreateOrderSagaHandler.cs:34`, `Order.Infrastructure/GrpcClients/InventoryClientService.cs`, received by `InventoryGrpcServiceImpl.cs:51-78`).

Inventory does not publish any domain integration events — zero hits anywhere in the repo for `StockAdjusted`, `StockReserved`, `LowStock`, or similar, and zero `EnqueueAsync` call sites in `Inventory.Application` outside of the generic audit-trail outbox usage.

## Why this matters

The requirement names Kafka integration specifically for Inventory. Today that's one-directional (consume-only from Product) and there's no async fan-out when stock changes — e.g. a future Notification "low stock" alert, or any other service wanting to react to stock movements, has nothing to subscribe to.

## Open questions

- Is the current gRPC-based saga integration between Order and Inventory an intentional design choice (synchronous stock check/deduct needed for the saga to work correctly) that should stay as-is, with Kafka only added *additionally* for fan-out/notification purposes — or is full Kafka-based integration expected to replace gRPC here? This is a scope decision, not just an implementation task.

## Suggested acceptance criteria

- A decision recorded on whether gRPC stays for the synchronous saga path.
- If additional fan-out is wanted: stock mutations produce a durable outbox row and Kafka message consumable by other services (e.g. `StockAdjusted`), following the same outbox pattern already proven for audit events.
