# Gift Aggregate

**Scope:** Domain-layer facts for the `GiftProgram` aggregate, implemented Phase 2.4. Structural only — no business rule (stock deduction, eligibility, fulfillment) is implemented yet.

## Aggregate boundary

`GiftProgram` (`Promotion.Domain/Entities/Gifts/GiftProgram.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `GiftItem`.

`GiftInventory`, `GiftReservation`, `GiftClaim`, `GiftUsage` are **not** part of the navigation graph (no Navigation section given for any of them) — all four get a public `Create`, related by `GiftItemId` (or, for `GiftClaim`, `ReservationId`) only.

## `Code` now uses `EntityCode` (Phase 2.6 correction)

Same original gap as Reward/Distribution (Phase 2.3) — no `ValueObjects` section was given, so `GiftProgram.Code` stayed a plain `string` through Phase 2.5. Now uses the shared `EntityCode` Value Object, same as every other aggregate root.

## `Quantity` reused for every stock/reservation count

`GiftItem.Quantity`, `GiftInventory.AvailableQuantity`, `GiftReservation.ReservedQuantity` all use the shared `BuildingBlock.Domain.ValueObjects.Quantity` — same as `ProductSetItem`/`BundleGift` in the Product Set aggregate.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `GiftProgram` | `AggregateRoot<Guid>` | Code (`EntityCode`, Phase 2.6 — was plain string)/Name/Description/Status/StartTime/EndTime |
| `GiftItem` | `BaseEntity<Guid>` | ProgramId/ProductId/VariantId/Quantity (shared VO) |
| `GiftInventory` | `BaseEntity<Guid>` | GiftItemId/WarehouseId/AvailableQuantity (shared VO) — public `Create` |
| `GiftReservation` | `BaseEntity<Guid>` | GiftItemId/UserId/OrderId/ReservedQuantity (shared VO)/ReservedAt — public `Create` |
| `GiftClaim` | `BaseEntity<Guid>` | ReservationId/ClaimedAt — public `Create` |
| `GiftUsage` | `BaseEntity<Guid>` | GiftItemId/UserId/OrderId/UsedAt — public `Create` |
| `GiftProgramTranslation` | `BaseEntity<Guid>` | Added Phase 2.5: `Id = GiftProgram.Id`, composite `(Id, LanguageCode)`. Exposed via `GiftProgram.Translate(languageCode, name, description)` — upsert, per [../entities/translation-workflow.md](../entities/translation-workflow.md) |

## Enums

- `ProgramStatus` (shared, consolidated Phase 2.5) — Draft/Active/Paused/Expired/Archived. Was `GiftProgramStatus` until merged with 3 identical siblings — see [../enums/README.md](../enums/README.md).

## Indexes (design only — written in Phase 3)

`(Code)` unique · `(Status)`

## Reconciliation notes

No `EntityData`/`UpdateData`/`GiftProgramTranslationData` wrapper types created (Domain Rule 2).
