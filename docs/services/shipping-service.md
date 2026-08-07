# Shipping Service

**Scope:** Shipping-specific facts. This is a brand-new service (added 2026-08-07) — general patterns still live in [conventions/](../conventions/) and are followed as-is; this doc only records what's Shipping-specific: the domain model, the aggregate-vs-child decisions behind it, what's actually wired up in this foundation phase, and what's deliberately postponed.

## Why ShippingService exists, and what it is not

ShippingService is the platform's **logistics execution layer**, reusable by any module that needs goods moved — customer order delivery, warehouse-to-warehouse transfer, supplier import, internal errands, freelance courier runs. It is *not* an "Order shipping module": it never references OrderService (no project reference, no shared database, no direct call). Every shipment is linked to its business context purely through `SourceType` (`Shipping.Domain.Enums.SourceType`) + `SourceReferenceId` (a `Guid` the consuming module owns the meaning of) — the same boundary shape PaymentService already established with `ReferenceType`/`ReferenceId`.

It is also explicitly **not a routing/TMS system**. There is no route optimisation, no vehicle-scheduling solver, no geospatial querying (which is why `GeoCoordinate` is two plain numeric columns, not a PostGIS type). ShippingService records and executes logistics intent; deciding the optimal route is out of scope and stays that way.

## The central architectural idea: Shipment ≠ Transportation

**A Shipment is an intention. A Transportation is an execution attempt. A Shipment is never bound to a provider — a Transportation is.**

This single decision drives the whole model:

- `Shipment` says *"these goods must move from A to B"*. It holds the manifest, the two addresses, the source reference — and no carrier.
- `Transportation` says *"attempt N to actually move them, carried by provider X"*. It holds the provider, the assigned person/vehicle, the tracking pings, the proof, and the cost.

When an attempt fails, a **new** `Transportation` is created against the **same, unchanged** `Shipment` (`Shipment.MarkFailed` is deliberately non-terminal, and `MarkPlanned` accepts `Failed` as an input state). The alternative — one entity carrying both intent and provider — would force either rewriting the shipment on every retry or losing the attempt history. `Transportation.AttemptNo` plus the unique `(ShipmentId, AttemptNo)` index makes the retry chain a first-class, queryable fact.

## Why `ShippingProvider` replaces "Carrier"

"Carrier" implies a third-party logistics company. `ShippingProvider` covers **external** carriers (GHN/VNPost/DHL), the company's own **internal** fleet, and **freelancers** under one model (`ProviderType`). That generalisation is precisely what lets the same execution machinery serve a warehouse transfer or a supplier import, not just a customer delivery — the reusability the service exists for. A `Carrier`-shaped model would have forced internal/freelance movement into a separate parallel implementation.

## Aggregate-to-entity mapping

21 entities: **12 aggregate roots** (own table, repository, `I{X}ReadService`/`I{X}WriteService`) and **9 child entities** (own table + EF config, constructed only by their root). This mapping is a deliberate decision, recorded here rather than left implicit: the originating spec listed 18 names flatly under "Aggregate Roots" while its own relationships section said Transportation *owns* Tracking/Costs/Proof/Assignment — those four are therefore children, and the rule was applied consistently across the model.

| Aggregate root | Owns (child entities) | References (by id, never a FK navigation) |
|---|---|---|
| `Shipment` | `ShipmentItem`, `ShipmentEvent`, `Package` → `PackageItem` | `SourceType` + `SourceReferenceId` |
| `Transportation` | `TransportationAssignment` (1:1), `TransportationTracking` (∗), `TransportationProof` (1:1), `TransportationCost` (∗) | `ShipmentId`, `ProviderId`, `CostRuleId?` |
| `ShippingProvider` | `ShippingProviderProfile` (1:1) | — |
| `TransportationCostRule` | — | `ProviderId?` |
| `TransportationPerson` | — | `ProviderId`, `UserId?` |
| `TransportationVehicle` | — | `ProviderId` |
| `ShippingProfile` | — | `UserId`, `VerifiedAddressId?` |
| `VerifiedShippingAddress` | — | `UserId` |
| `Pickup` | — | `ShipmentId` |
| `Delivery` | — | `TransportationId` |
| `ReturnShipment` | — | `OriginalShipmentId`, `ReturnedShipmentId?` |
| `CarrierIntegration` | — | `ShippingProviderId` |

Two mappings that could reasonably have gone the other way, and why they didn't:

- **`Delivery` is a root, not a child of `Transportation`** — and it is *not* a duplicate of `TransportationProof`. Proof exists for every handover including a warehouse transfer; `Delivery` models the customer-facing outcome only, carrying recipient-specific concerns (attempt count, refusal, COD collection) that are meaningless on an internal transfer. Making it a child would force every transportation to carry delivery semantics it doesn't have.
- **`TransportationCostRule` is a root, not owned by `Transportation`** — it is reusable pricing configuration that outlives any single trip, so it is *referenced* (`Transportation.CostRuleId`), never owned.

`TransportationAssignment`, `TransportationProof` and `ShippingProviderProfile` are strict 1:1 extensions, so their primary key **is** the parent's id — no surrogate `Id`, per [domain-coding-conventions.md](../conventions/domain-coding-conventions.md) rule 5.

## Projects

`Shipping.Domain`, `Shipping.Application`, `Shipping.Persistence`, `Shipping.Infrastructure`, `Shipping.API` — same 5-layer split as every other service, under `src/Services/Shipping/`.

Domain folders mirror the aggregate groups: `Entities/{Shipments,Transportations,Providers,TransportationPeople,TransportationVehicles,ShippingProfiles,VerifiedAddresses,Pickups,Deliveries,ReturnShipments,CarrierIntegrations}/`. `TransportationCostRule` lives under `Entities/Transportations/` (it is transportation pricing, even though it is its own root).

## Value Objects

All ShippingService-local, in `Shipping.Domain/ValueObjects/`:

- `ShipmentNumber` / `TransportationNumber` — `SHP-yyyyMMdd-XXXX` / `TRN-yyyyMMdd-XXXX`, `StringValueObject`-based, mirroring Order's `OrderNumber` (regexes in `Domain/Regexes/ShippingRegexes.cs`).
- `ShippingAddress` — multi-field postal address, mapped via `OwnsOne`. **Deliberately not named `Address`**: this is a bounded-context-local type, following the precedent already set by `User.Domain.ValueObjects.Address` and `Payment.Domain.ValueObjects.BillingAddress` being separate rather than shared.
- `GeoCoordinate` — WGS-84 lat/long pair (`numeric(9,6)` columns).
- `PackageDimensions` — L/W/H in cm, with a derived `VolumeCm3` that is `Ignore`d in EF.

The shared `BuildingBlock.Domain.ValueObjects.Money`, `PhoneNumber`, `Email` and `Quantity` are **reused as-is** — unlike Payment, this service has no currency-aware money requirement, so no local `Money` was introduced and no `GlobalUsings` collision workaround is needed.

## Enums

`ShipmentType`, `ShipmentStatus`, `SourceType`, `TransportationStatus`, `ProviderType`, `CostCategory`, `CostRuleType`, `VerificationStatus`, `PackageType`, `PickupType`, `VehicleStatus`, `PersonStatus` — plus four the model needed that the original spec list didn't name: `PickupStatus`, `DeliveryStatus`, `ReturnShipmentStatus`, `IntegrationStatus`. Each of those four exists because the aggregate has a genuinely different lifecycle from anything already enumerated — a `Pickup` cannot meaningfully be `ShipmentStatus.Delivered`, and reusing a foreign status enum would have been a worse decision than adding a small precise one.

## Exception codes

`MessageCode` **900-999 is reserved for Shipping Service** (`BuildingBlock.Domain/Enums/MessageCode.cs`); 900-915 are defined. They are currently **unused** — the single foundation endpoint throws the generic `NotFoundException(entityName, value)`. Codes get wired in as real business handlers are written, the same way Payment's 800-899 block was reserved ahead of use.

## Persistence

`ShippingDbContext` (Postgres, `shipping_db`), one `IEntityTypeConfiguration<T>` per entity in `Configs/` — **all 21 entities are fully configured**, none stubbed. Migration `InitialCreate` creates 24 tables (21 domain + `outbox_messages`/`inbox_messages`/`inbox_retry_histories`).

- **Relational FKs throughout**, never EF owned *entities* — `ShipmentItem`/`Package`/`TransportationTracking`/… are real tables with their own PK and a FK back to the parent, matching Order's post-refactor convention. Nothing is auto-loaded; every read path `Include`s explicitly.
- **Multi-field Value Objects are `OwnsOne`** (`ShippingAddress`, `GeoCoordinate`, `PackageDimensions`) — a genuine owned-*type* mapping for an identity-less VO is a different thing from the owned-*entity* anti-pattern. The shared column-set mappings live in `Configs/ShippingValueObjectConfigurationExtensions.cs` so they aren't repeated per entity.
- **Single-scalar VOs use `HasConversion`** (`Money` → `numeric(18,2)`, `PhoneNumber`, `Email`, `Quantity`, `ShipmentNumber`, `TransportationNumber`).
- **Enums are `HasConversion<short>()`**.
- **Concurrency**: `ConfigureCommonFields()` (audit timestamps + the Postgres `xmin` shadow row-version) on every mutable entity; append-only rows (`ShipmentEvent`, `TransportationTracking`, `TransportationCost`, `ShipmentItem`, `PackageItem`) use `ConfigureAuditFields()` only, since they are never updated.
- **Tenant filtering is automatic** — every aggregate root implements `ITenantEntity`, and `ModelBuilderExtensions.ApplyEntityConventions` applies the `TenantId` column, index and query filter from the marker interface alone. No query filter is hand-written anywhere in this service.
- **`Shipment` and `Transportation` implement `IIdempotentEntity`** (the two creation entry points a client is most likely to retry); no other aggregate does.
- **Cross-aggregate references are indexed columns, not FKs** — `Transportation.ShipmentId`, `Pickup.ShipmentId`, `Delivery.TransportationId` etc. are plain `Guid` columns with an index, following the codebase's existing rule against FK navigations across aggregate roots.
- **`PackageItem.ShipmentItemId`** is deliberately an indexed column rather than a configured FK: `Package` and `ShipmentItem` both already cascade from `Shipment`, so a second FK would add a second cascade path to the same root for no integrity gain within one aggregate.

### Read/Write services and repositories

All 12 aggregate roots have the full trio — `I{X}Repository` (in Persistence), `I{X}ReadService`/`I{X}WriteService` (ports in `Shipping.Application/Abstractions/Persistence/{X}/`, implemented in `Shipping.Persistence/Contexts/{X}/{Read,Write}/`). Repositories are auto-registered by the Scrutor scan (`AddScopedByInterface(typeof(IRepository<>), typeof(ShippingDbContext))`); Read/Write services are registered explicitly, one pair per aggregate.

Each write service exposes a minimal `CreateAsync` + `DeleteAsync`; each read service a `GetByIdAsync` plus the one lookup that aggregate genuinely needs (`GetByIdempotencyKeyAsync`, `GetByShipmentNumberAsync`, `GetByProviderIdAsync`, …). Richer behaviour arrives with the CQRS handlers that need it.

### Audit hierarchy

Registered in `Shipping.Persistence/DependencyInjection.cs` for all 21 entities: `Shipment`/`Transportation`/`ShippingProvider` as roots with their children mapped via `BelongsTo`, the other nine aggregates as flat roots. `PackageItem` is the one entity with no direct `BelongsTo` mapping — it belongs to `Package`, which belongs to `Shipment`, and the audit hierarchy builder expresses only a single hop.

## Application layer

Foundation only — **no command/query handlers exist yet**. What does exist:

- The 24 Read/Write service **ports** (12 aggregates × 2).
- The `Features/{Aggregate}/{Commands,Queries,DTOs,Events}/` folder skeleton, created up front so the first real feature drops in without restructuring.
- Standard `AddApplication` wiring (MediatR, behaviors, Mapster, FluentValidation).

## Infrastructure

Extension points only, no provider-specific logic:

- `Providers/IShippingProviderClient.cs` — the seam for a future GHN/VNPost/DHL client, **intentionally unimplemented and unregistered**. No carrier contract has been chosen, and inventing a lowest-common-denominator interface shape before one real integration exists is the speculative abstraction this codebase avoids; the interface exists so the seam is visible and Application code can be written against it.
- `Messaging/Consumers/` and `BackgroundJobs/` — empty. ShippingService has nothing to consume in this phase (Order does not yet publish a "shipment requested" trigger, and no carrier webhook pipeline exists).
- `AddInfrastructure` wires the baseline every service gets: `AddAppLogger`, `AddRedisCache`, `AddIdempotency`, `AddApplicationEventDispatcher`, `AddInboxOutboxCleanupJobs`, `AddHttpAuditMetadataProvider("Shipping")`, `AddKafkaMessaging("shipping-service")`, `AddInboxOutboxInfrastructure` — so the Outbox relay and Inbox retry hosted services (and their tables) are live from day one even with no consumers registered.

## API

Internal `8080` (REST) only, no gRPC. Gateway path prefix `/api/shipping/` (`RequireAuth: true`), public debug port `5110` (`SHIPPING_PUBLIC_HTTP_PORT`).

One endpoint exists, and only to verify wiring:

| Method | Route | File | Purpose |
|---|---|---|---|
| GET | `/shipments/{shipmentId}` | `Endpoints/Shipment/GetShipment.cs` | Fetch a shipment with its items/events/packages (RequireAuthenticated) |

It proves Carter discovery, JWT/authorization, DI down to the Persistence read service, and Swagger generation all resolve. It calls `IShipmentReadService` directly rather than going through MediatR — there is no query handler yet, and adding an empty one would be ceremony, not architecture. Its `GetShipmentResponse` record lives beside the endpoint rather than in `Application/Features/Shipments/DTOs` for the same reason: it is the wiring probe's own shape, not a feature contract.

## Messaging

**12 integration event contracts** exist in `BuildingBlock.Contract/Events/Shipping/`, matching `OrderCreatedIntegrationEvent`'s shape (`sealed record … : IIntegrationEvent`, auto-initialized `CorrelationId`/`EventType`/`PublishedAt`):

`ShipmentRequested`, `ShipmentCancelled`, `TransportationCreated`, `TransportationAssigned`, `TransportationStarted`, `TransportationCompleted`, `TransportationFailed`, `TransportationCancelled`, `TransportationDelivered`, `TransportationReturned`, `VerifiedAddressCreated`, `ShippingProfileVerified`.

They are **contracts only — not wired to any `IOutboxStore.EnqueueAsync` call**, because no command handler exists to publish them yet. Publishing happens inside the Application command handler that performs the state change, in the same transaction, per the codebase's existing outbox pattern.

Note the pairing that mirrors the Shipment/Transportation split: `TransportationCompleted` fires for *every* successful attempt (including a warehouse transfer), while `TransportationDelivered` fires specifically when goods reach an end recipient — Order Service waits on the latter.

## Integration boundaries

- **Order Service** — will eventually request a shipment (`SourceType.Order` + the order id) and react to `TransportationDelivered` to complete the order. No project reference exists in either direction, and none should.
- **Inventory Service** — warehouse transfers (`SourceType.WarehouseTransfer`) and supplier imports (`SourceType.SupplierImport`) are ordinary shipments as far as this service is concerned; stock movement remains entirely Inventory's business.
- **User Service** — owns the user's canonical address book. `ShippingProfile`/`VerifiedShippingAddress` are the shipping-side, shipping-shaped records (auto-complete presets and accumulated deliverability knowledge), not a replacement for it. `VerifiedAddressCreated` is the enrichment signal in that direction.
- **Payment Service** — COD amounts are recorded on `Delivery` for collection tracking only; ShippingService never moves money.

## Deployment status

- Registered in `NovaCore.sln`, the Gateway's `appsettings.json` (`Gateway:Services:Shipping`), `.env.template` (`SHIPPING_*`), and `scripts/postgres/init.sql` (`shipping_db`).
- The `shipping-api` blocks in `docker-compose.yml` / `docker-compose.override.yml` are **committed but commented out** — the service has no workflow to serve yet, so starting the container would only add a healthcheck to babysit. Uncomment both blocks together with the `.env` values when the first real endpoints land.
- No Hangfire database and no `UseBackgroundJobsDashboard()`/`UseBackgroundJobsScheduling()` call — same position Payment took; wire it in when a real recurring job (carrier status polling, pickup reminder) exists.

## Planned phases (intentionally postponed)

This phase is architecture + domain model + migration-ready schema + DI wiring only. Explicitly out of scope, to be scoped as their own dated tasks:

- **Phase 2 — Shipment lifecycle CQRS**: create/request/cancel shipment, manage manifest and packages, endpoints and validators.
- **Phase 3 — Transportation execution CQRS**: create attempt, assign person/vehicle, record tracking, capture proof, the retry-on-failure flow end to end.
- **Phase 4 — Provider & capacity management**: `ShippingProvider`/`TransportationPerson`/`TransportationVehicle` admin surfaces.
- **Phase 5 — Cost engine**: `TransportationCostRule` evaluation actually deriving `Transportation.TotalCost`, currently only stored.
- **Phase 6 — Carrier integration**: real `IShippingProviderClient` implementations, `CarrierIntegration` secret-store integration, inbound carrier webhooks.
- **Phase 7 — Address intelligence**: `VerifiedShippingAddress` verification workflow, `ShippingProfile` auto-complete API, geocoding.
- **Phase 8 — Cross-service integration**: outbox-publishing the 12 event contracts, Order consuming `TransportationDelivered`, Inventory-driven transfer shipments.
- **Phase 9 — Returns**: `ReturnShipment` approval automatically creating the reverse `Shipment` (currently `AttachReturnedShipment` expects a caller to supply it).

## Known issues

- No integration or unit tests yet (`tests/…/Shipping.*Tests` don't exist) — add alongside Phase 2 business logic.
- `MessageCode` 900-915 are defined but unreferenced (see "Exception codes").
- The `InitialCreate` migration has been generated but **never applied to a database** — no runtime verification of the schema has happened.
