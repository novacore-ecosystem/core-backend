# Promotion Service — Value Objects

**Scope:** Catalogue of every `Promotion.Domain` Value Object, updated as each is introduced (step 7 of [../entities/entity-implementation-strategy.md](../entities/entity-implementation-strategy.md)).

## Phase 2.5 consolidation

The Domain Standardization Review merged two sets of structurally-identical, per-aggregate Value Objects that had accumulated across Phases 2.1-2.4:

- **7 Code VOs → `EntityCode`** — `CampaignCode`, `PromotionCode`, `CouponCode`, `VoucherCode`, `LoyaltyProgramCode`, `RecommendationCode`, `ProductSetCode` were all byte-for-byte identical (uppercase alphanumeric + `_`/`-`, max 50 chars) and are now one shared `EntityCode`, used by every aggregate root's own `Code` property. This also **removed the `CouponCode` entity/Value Object name collision** documented in Phase 2.2 — the alias workaround (`CouponCodeVo`/`CouponCodeEntity`) is gone, since the VO no longer shares the entity's name.
- **6 Period VOs → `Period`** — `CampaignPeriod`, `PromotionPeriod`, `CouponPeriod`, `VoucherPeriod`, `LoyaltyPeriod`, `RecommendationPeriod` were all identical (`StartTime`/`EndTime`/optional `TimeZone`) and are now one shared `Period`, currently consumed by `CampaignSchedule.Period` (every other usage remains reserved).

This is a genuine intra-project deduplication (all 13 VOs lived in the same `Promotion.Domain` project/namespace), not a cross-service-boundary merge — it does not compromise "keep every aggregate independent" any more than reusing the shared `BuildingBlock.Domain.ValueObjects.Money`/`Quantity` across many entities already does in this same codebase.

## Phase 2.6 correction

- **`RewardProgram.Code`/`DistributionJob.Code`/`GiftProgram.Code` now use `EntityCode`**, closing the gap the Phase 2.5 review had flagged but deliberately left open pending this final pass. Every aggregate root's `Code` property is now consistently `EntityCode`.
- **New local `Currency` Value Object** (mirrors `EntityCode`'s shape — 3-letter uppercase ISO-4217, `IsValid`/`TryCreate`/`Create`) replaces the bare `string` previously duplicated across `Promotion.Currency`, `Voucher.Currency`, `CampaignBudget.CurrencyCode`, `BundlePrice.Currency`. Local to this project since Payment.Domain's own `Currency` lives in a different microservice and cannot be referenced across a service boundary.
- **`PointAccount`'s five balance fields** (`AvailablePoints`, `PendingPoints`, `ExpiredPoints`, `LifetimeEarned`, `LifetimeSpent`) now use the shared `Quantity` VO — non-negative running counts, same shape `GiftInventory.AvailableQuantity` already used.
- `Period`/`CouponUsageLimit` remain **not** adopted by `Coupon`/`Voucher` root-level `StartTime`/`EndTime`/`MaxUsage` fields — re-confirmed during this pass as a deliberate, not accidental, choice: `Campaign`/`Promotion` (the aggregates `Period` itself was consolidated from) also keep their own root-level window as flat scalars and reserve `Period` for sub-entities needing a genuinely reusable/multi-window shape (`CampaignSchedule.Period`). Forcing adoption at the aggregate-root level would make `Coupon`/`Voucher` *inconsistent* with `Campaign`/`Promotion`, not more consistent.

## Current catalogue

| Value Object | Base | Shape | Used by |
|---|---|---|---|
| `EntityCode` | `StringValueObject` | Uppercase alphanumeric + `_`/`-`, max 50 chars | `Campaign.Code`, `Promotion.Code`, `Coupon.Code`, `CouponCode` entity's own `Code`, `Voucher.Code`, `LoyaltyProgram.Code`, `RecommendationProgram.Code`, `ProductSet.Code`, `RewardProgram.Code`, `DistributionJob.Code`, `GiftProgram.Code` |
| `Currency` | `StringValueObject` | 3-letter uppercase ISO-4217 | `Promotion.Currency`, `Voucher.Currency`, `CampaignBudget.CurrencyCode`, `BundlePrice.Currency` |
| `Period` | `ValueObject` | `StartTime`/`EndTime`/optional `TimeZone`, `EndTime > StartTime` enforced | `CampaignSchedule.Period`; reserved for reuse anywhere else a bundled period shape is needed |
| `PromotionPriorityValue` | `ValueObject` | Validated `int` 0-100 (mirrors `BuildingBlock.Domain.ValueObjects.Percentage`) | `PromotionPriority.Value` |
| `CouponUsageLimit` | `ValueObject` | Bundled `MaxUsage`/`MaxUsagePerUser` pair | reserved — not yet consumed (`Coupon` keeps them as flat scalars per its literal Properties list, re-confirmed Phase 2.6) |

`ApprovalWorkflow` has no `Code` field at all, so it has no Value Object to adopt here.

## Reused shared Value Objects (not duplicated locally)

- `LanguageCode` (`BuildingBlock.Domain.ValueObjects`) — used by every `{Entity}Translation` entity (`CampaignTranslation`, `PromotionTranslation`, `CouponTranslation`, `VoucherTranslation`, `LoyaltyProgramTranslation`, `RewardProgramTranslation`, `GiftProgramTranslation`, `RecommendationProgramTranslation`, `ProductSetTranslation`, `ProductBundleTranslation`).
- `Money` (`BuildingBlock.Domain.ValueObjects`) — used by `Voucher.Amount`/`Balance`, `VoucherWallet`'s three balances, `VoucherReservation.ReservedAmount`, `VoucherRedemption.RedeemedAmount`, `VoucherTransfer.Amount`, `VoucherExpiration.ExpiredAmount`, `CampaignBudget.AllocatedAmount`/`SpentAmount`, `BundlePrice.Price`. No PromotionService-local Money VO was created — the shared one (deliberately currency-less, per Payment Service's own precedent) fits exactly; the new local `Currency` VO stays a separate scalar wherever Money is used.
- `Quantity` (`BuildingBlock.Domain.ValueObjects`) — used by `ProductSetItem.Quantity`, `BundleGift.Quantity`, `GiftItem.Quantity`, `GiftInventory.AvailableQuantity`, `GiftReservation.ReservedQuantity`, `PointAccount`'s five balance fields (Phase 2.6) — same Value Object `Order.Domain`'s `OrderItem` already uses.

## Reconciliation: root entities keep flat scalar period/limit fields

`Campaign`/`Promotion`/`Coupon`/`Voucher` all keep `StartTime`/`EndTime`/`TimeZone` (and `Coupon`'s `MaxUsage`/`MaxUsagePerUser`) as plain scalar properties, not wrapped in `Period`/`CouponUsageLimit` — each phase brief's Properties list specified them as separate flat fields, and Phase 2.6 re-confirmed this is consistent (not an omission) since `Campaign`/`Promotion` set the precedent of never wrapping their own root-level window. The VOs exist and are used wherever a *reusable* bundled shape is actually needed elsewhere (`CampaignSchedule.Period`), or reserved for a future need if none exists yet.
