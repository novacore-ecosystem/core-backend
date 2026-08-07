# Domain — DONE (Phase 2 complete, frozen after Phase 2.6 final correction)

**Phase 2 (Domain Model) is complete as of Phase 2.6 (Final Domain Correction & Freeze).** `Promotion.Domain` builds clean across 13 aggregate groups. See [../../../docs/promotion-service/aggregates/](../../../docs/promotion-service/aggregates/) for the full per-aggregate breakdown and [../../../docs/promotion-service/entities/translation-workflow.md](../../../docs/promotion-service/entities/translation-workflow.md) for the frozen Translation feature order.

**The Domain model is now frozen** — do not modify it in future phases unless a design defect is discovered. Phase 3 (Persistence) is current; no EF Core, no MediatR, no repository types belong in this project — see [../../../docs/promotion-service/phases/phase-3-persistence.md](../../../docs/promotion-service/phases/phase-3-persistence.md).

## What Phase 2.5 changed (standardization)

- Consolidated 7 duplicate Code Value Objects → `EntityCode`; 6 duplicate Period Value Objects → `Period`; 4 duplicate Program status enums → `ProgramStatus`.
- Renamed `CampaignLocalization` → `CampaignTranslation` for naming consistency.
- Added Translation support (`{Entity}Translation` entity + upsert `Translate(...)`) to Promotion, Coupon, Voucher, LoyaltyProgram, RewardProgram, GiftProgram, RecommendationProgram, ProductSet, and `ProductBundle` (10 aggregates translatable in total, counting Campaign).

## What Phase 2.6 changed (final correction pass)

- **Fixed 5 uninstantiable entities** — `CampaignBudget`, `CampaignApproval`, `PromotionRuleGroup`, `PromotionPriority`, `PromotionExclusion` all had `internal static Create` with zero callers anywhere in the codebase (none is owned via an `ICollection<T>`). All five are now `public static Create`.
- **Completed the `PromotionRule` → `PromotionCondition` ownership path** — `PromotionCondition`'s own doc comment always said only `PromotionRule` could construct it, but `PromotionRule` had no `Conditions` collection or `AddCondition` method. Both are now added; `PromotionCondition.Create` stays `internal`.
- **Added 3 navigation properties**: `Campaign.Budget` (→ `CampaignBudget`), `PromotionRule.RuleGroup` (→ `PromotionRuleGroup`), `RewardDistribution.DistributionJob` (→ `DistributionJob`, cross-group) — all forward references to independent, related-but-not-owned entities, matching the existing `Coupon.Campaign`/`Coupon.Promotion` pattern. Deliberately **not** extended to every owned child across the service (e.g. `CampaignSchedule.Campaign`, `CouponUsage.Coupon`) — that would reverse the established, consistent, one-directional-navigation convention verified across 9 aggregate groups, not fix a defect.
- **11 string fields converted to enums**: `PromotionRuleGroup.LogicOperator`, `PromotionCondition.Operator`, `PromotionTarget.TargetType`, `PromotionBenefit.BenefitType`, `PromotionConstraint.ConstraintType`, `PromotionUsageLimit.Scope`, `CouponReservation.Status`, `ApprovalStep.Status`, `ApprovalDecision.Decision`, `PromotionValidationResult.Status`, `PromotionSimulationResult.Status`. See [../../../docs/promotion-service/enums/README.md](../../../docs/promotion-service/enums/README.md) for the full list including which are architect-confirmed vs. structural placeholders.
- **Closed the `EntityCode` gap** — `RewardProgram.Code`, `DistributionJob.Code`, `GiftProgram.Code` now use the shared `EntityCode` Value Object, same as every other aggregate root.
- **New local `Currency` Value Object** — replaces the bare `string` previously duplicated across `Promotion.Currency`, `Voucher.Currency`, `CampaignBudget.CurrencyCode`, `BundlePrice.Currency`.
- **`PointAccount`'s five balance fields** now use the shared `Quantity` Value Object instead of plain `int`.
- Re-confirmed (not changed): `Period`/`CouponUsageLimit` stay unused at the `Coupon`/`Voucher` root level (consistent with `Campaign`/`Promotion`'s own precedent); `CouponStatus`/`VoucherStatus` were not given a `Rejected` value (`Cancel()` already provides a working exit path); the Validation/Audit groups' missing `ITenantEntity` was re-flagged, more strongly, rather than silently fixed (see below).

## Known open items — flag for architect confirmation, do not fix silently

- `CampaignType`/`PromotionType`/`PromotionTargetType`/`PromotionBenefitType`/`PromotionConstraintType` enum values are structural placeholders, not architect-specified.
- **The Validation and Audit entity groups (9 entities total) still have no aggregate root and no `TenantId`/`ITenantEntity`** — re-confirmed twice now (Phase 2.4 and Phase 2.6) as a literal reading of an omission in both briefs, not an oversight. This is the single highest-priority item to resolve before Phase 3 (Persistence) locks the schema: without `ITenantEntity`, queries against these tables bypass the platform's automatic tenant-filter mechanism entirely.
- `CampaignApproval`/`CouponApproval` remain unwired to the `ApprovalWorkflow` structure (Phase 2.4) — "do not modify previous aggregates" was respected each time this came up, including Phase 2.6.
- `LoyaltyProgram`/`RecommendationProgram` have no `TimeZone` property, unlike Campaign/Promotion/Coupon/Voucher.
- `CampaignStatus` has a `Scheduled` value that `PromotionStatus` doesn't — a lifecycle asymmetry between two very similar aggregates, flagged but not resolved (adding a status would invent an unrequested business state).
- `CampaignChannel.Channel` remains a plain `string` — a `ChannelType` enum was already flagged in `campaign.md`, not invented here.
