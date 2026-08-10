# Task 16: `ProductVariation.Name` is a dead property — never set, never returned

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

`ProductVariation.Name` (`src/Services/Product/Product.Domain/Entities/ProductVariation.cs:7`) exists as a domain property, but:

- The `Create()` factory (same file, lines 28-67) has no `name` parameter — it is always constructed as `""`.
- There is no `UpdateName`/`Rename` method on the entity at all (no parity with `Product.ValidateName`/rename support).
- `ProductVariationConfig.cs` never explicitly configures the column; it exists in the DB by EF convention only (migration `20260724103020_InitialCreate.cs:173`), and is always empty in practice.
- No command DTO sets it: `CreateProductVariationRequest` (`Product.API/Endpoints/CreateProduct.cs:13-23`), `ProductVariationInputDto` (`Product.Application/.../DTOs/ProductVariationInputDto.cs:3-14`), `AddVariationRequest` (`Product.API/Endpoints/AddVariation.cs:10-20`), and `UpdateVariationCommand`/`UpdateVariationRequest` (`UpdateVariation.cs:9-19`, `UpdateVariationCommand.cs:8-20`) all lack a `Name` field, and `UpdateVariationHandler.cs:32-43` never touches it.
- No query DTO returns it: `ProductVariationResponse.cs:4-33` has no `Name` field; `GetProductHandler.cs:15-25` and the search handler never surface it.
- Cross-service confirmation: `ProductVariationCreatedIntegrationEvent` (`BuildingBlock.Contract/Events/Product/ProductVariationCreatedIntegrationEvent.cs:11-18`) carries a `ProductName` explicitly populated from `Product.Name` (`CreateProductHandler.cs:140`, `AddVariationHandler.cs:54`) — so Order Service's `OrderProductCatalog.ProductName` (`Order.Domain/Entities/OrderProductCatalog.cs:17`) is always the parent Product's name, never variation-specific, by design of the current event contract.

## Why this matters

Checklist requirement: "Variation Name should become the source of truth instead of always displaying Product Name." Right now there is no code path — create, update, or read — through which a variation name can be set or retrieved at all. This blocks Task 15 (variation search) and every frontend variation-name display item.

## Suggested acceptance criteria

- `ProductVariation.Create()` accepts and validates a required `name` parameter; add an `UpdateName`/`Rename` method.
- `CreateProductVariationRequest`/`ProductVariationInputDto`, `AddVariationRequest`, and `UpdateVariationCommand`/`UpdateVariationRequest` all carry `Name`, wired through their respective handlers and mappings.
- `ProductVariationResponse` returns `Name`; `GetProduct`/`SearchProducts` responses surface it per-variation.
- Decide and document whether `ProductVariationCreatedIntegrationEvent`/`OrderProductCatalog.ProductName` should be renamed or supplemented with a `VariationName` field so Order-side displays (order line items) can eventually show variation name instead of/alongside product name — flagging as a design decision rather than assuming scope; the checklist's frontend order-picker item depends on this choice.

## What was done

- **Domain:** `ProductVariation.Create()` now takes a required `name` (validated via new `ValidateName`/`IsValidName`, mirroring `Product`'s pattern); added `UpdateName(string name)`.
- **Application:** `ProductVariationInputDto` gained a required `Name`; `ProductVariationMapping.MapInputToEntity` passes it through; `ProductVariationResponse`/`From()` now returns `Name`; `IProductWriteService.UpdateVariationInformationAsync` gained a `name` parameter, implemented in `ProductWriteService` via `variation.UpdateName(name)`.
- **API:** `CreateProductVariationRequest`, `AddVariationRequest`, `UpdateVariationRequest` (+ `UpdateVariationCommand`) all carry a required `Name`, wired end-to-end through their handlers. Added FluentValidation rules (`IsValidName`, max 200 chars) to `CreateProductValidator`, `AddVariationValidator`, `UpdateVariationValidator`. Doc strings updated on `CreateProduct.cs`/`UpdateVariation.cs`.
- **Read path:** `GetProductHandler` already used `ProductVariationResponse.From` for every variation, so it now surfaces `Name` with no handler change needed.
- **Persistence:** Added an explicit `HasMaxLength(200).IsRequired()` mapping for `ProductVariation.Name` (previously convention-only, unbounded) in `ProductVariationConfig`. Generated migration `AddVariationName` (`ALTER COLUMN name TYPE character varying(200)` - EF flagged it as a possible-data-loss scaffold, but existing rows are all `""`, well under 200 chars, so no actual loss). Added `ProductDbContextFactory` (`Product.API`) since none existed - `dotnet ef migrations add` needed a design-time factory to avoid booting the full app host, matching the existing User/Order pattern.
- **Cross-service (additive only):** Added `VariationName` to `ProductVariationCreatedIntegrationEvent` and `ProductVariationUpdatedIntegrationEvent` (`BuildingBlock.Contract`), populated from `variation.Name` at all three publish sites (`CreateProductHandler`, `AddVariationHandler`, `UpdateVariationHandler`). This is additive/non-breaking: Order's and Inventory's local `On*Event` record shapes are separate types deserialized from the same JSON by property name, so they simply don't pick up the new field until a consumer chooses to declare it.
- Scoped builds of `Product.API`, `Order.API`, and `Inventory.API` all pass with 0 errors.

## What wasn't done

`OrderProductCatalog`/`OnProductVariationCreatedHandler`/`OnProductVariationUpdatedHandler` (Order service) were deliberately left untouched - they still snapshot `ProductName` only. Making Order actually consume the new `VariationName` field (new column, migration, handler updates, and a decision on whether it replaces or supplements `ProductName` for cart/order-line display) is real additional scope belonging to whatever task acts on NovaCoreUI Task 14's blocked order-picker requirement, not this one.

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task14_variation-name-ui-missing.md`. Enables Task 15.
