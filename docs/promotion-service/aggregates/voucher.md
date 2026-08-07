# Voucher Aggregate

**Scope:** Domain-layer facts for the `Voucher` aggregate, implemented Phase 2.2. Structural only — no business rule (balance calculation, redemption enforcement, fraud/freeze logic) is implemented yet.

## Aggregate boundary

`Voucher` (`Promotion.Domain/Entities/Vouchers/Voucher.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It always belongs to a `Promotion` (`PromotionId`), optionally to a `Campaign` (`CampaignId`) — unlike Coupon, the phase brief did **not** request `Promotion`/`Campaign` navigation *objects* for Voucher, only the FK ids, so those stay plain `Guid`/`Guid?` scalars here (a deliberate difference from Coupon, not an oversight). It owns, via `ICollection<T>` navigation + internal construction (Rule 3): `VoucherIssue`, `VoucherReservation`, `VoucherRedemption`, `VoucherTransfer`, `VoucherHistory`. `Wallet` is a nullable single-object navigation to `VoucherWallet` (FK `WalletId`).

`VoucherBatch`, `VoucherExpiration`, `VoucherFreeze` are **not** part of this aggregate's owned navigation graph — the brief's Navigation list (`Wallet`, `Issues`, `Reservations`, `Redemptions`, `Transfers`, `History`) omitted all three, so each has a public `Create` and is related by `VoucherId` only (same reconciliation as Coupon's `CouponBatch`/`CouponCode`/`CouponApproval`).

## `VoucherWallet` is a shared, independent entity

`VoucherWallet` is keyed by `UserId`, not `VoucherId` — a user's wallet balance is independent of any single voucher (many vouchers can reference the same wallet via `Voucher.WalletId`). Its `Create` is public, same reasoning as `CouponBatch`.

## `VoucherBatch` fields were inferred

The phase brief listed `VoucherBatch` under Voucher's Entities but — unlike every other entity in this phase — gave it **no Fields subsection**. Its shape was inferred from `CouponBatch`'s parallel role (both are generation/import batches for their respective aggregate): `Name`/`Source`/`ImportedAt`/`TotalCount`/`ActivatedCount`/`UsedCount`/`FailedCount`. Flagging this explicitly since it's the one field-set not given directly.

## `Money` reused, not duplicated

`Voucher.Amount`/`Balance`, and every Money-valued child field (`VoucherWallet`'s three balances, `VoucherReservation.ReservedAmount`, `VoucherRedemption.RedeemedAmount`, `VoucherTransfer.Amount`, `VoucherExpiration.ExpiredAmount`) use the shared `BuildingBlock.Domain.ValueObjects.Money` directly — no new Voucher-local Money VO was created, since the shared one (which deliberately has no currency field, per Payment Service's own precedent) fits exactly. `Voucher.Currency` stays a separate scalar (now the local `Currency` Value Object, Phase 2.6 — was a bare `string`), same split Payment Service already uses.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `Voucher` | `AggregateRoot<Guid>` | PromotionId/CampaignId/Code/Name/Description/VoucherType/Status/Currency/Amount/Balance/StartTime/EndTime/TimeZone/OwnerId/WalletId |
| `VoucherWallet` | `BaseEntity<Guid>` | UserId/TotalBalance/AvailableBalance/ReservedBalance — public `Create` (independent lifecycle) |
| `VoucherIssue` | `BaseEntity<Guid>` | VoucherId/UserId/DistributionId/IssuedAt |
| `VoucherBatch` | `BaseEntity<Guid>` | Fields inferred from `CouponBatch` (see above) — public `Create` |
| `VoucherReservation` | `BaseEntity<Guid>` | VoucherId/OrderId/UserId/ReservedAmount/ReservedAt/ExpiredAt |
| `VoucherRedemption` | `BaseEntity<Guid>` | VoucherId/OrderId/RedeemedAmount/RedeemedAt |
| `VoucherTransfer` | `BaseEntity<Guid>` | VoucherId/FromUserId/ToUserId/Amount/TransferredAt |
| `VoucherHistory` | `BaseEntity<Guid>` | VoucherId/Action/OperatorId — `CreatedAt` inherited from `BaseEntity`, not redeclared |
| `VoucherExpiration` | `BaseEntity<Guid>` | VoucherId/ExpiredAmount/ExpiredAt — public `Create` |
| `VoucherFreeze` | `BaseEntity<Guid>` | VoucherId/Reason/FrozenAt/ReleasedAt — public `Create` |
| `VoucherTranslation` | `BaseEntity<Guid>` | Added Phase 2.5: `Id = Voucher.Id`, composite `(Id, LanguageCode)`. Exposed via `Voucher.Translate(languageCode, name, description)` — upsert, per [../entities/translation-workflow.md](../entities/translation-workflow.md) |

## Enums

- `VoucherStatus` — Draft/PendingApproval/Approved/Issued/Active/Reserved/Redeemed/Expired/Cancelled/Archived (all 10 values given explicitly).
- `VoucherType` — Cash/Balance/Gift/Compensation/Refund (given explicitly).

## Value Objects

- `EntityCode` (shared, consolidated Phase 2.5) — used by `Voucher.Code`.
- `Period` (shared, consolidated Phase 2.5, reserved).
- `Money` — reused from `BuildingBlock.Domain.ValueObjects`, not duplicated (see above).

## Indexes (design only — written in Phase 3)

`(Code)` unique · `(OwnerId)` · `(Status)` · `(PromotionId)` · `(CampaignId)` · `(WalletId)`

## Phase 2.6 correction

`Voucher.Currency` converted from `string` to the new local `Currency` Value Object. No other changes — `VoucherReservation` correctly has no `Status` field (release is modeled by row removal via `Voucher.ReleaseReservation`), which is why `CouponReservation.Status` was converted to an enum rather than removed for parity: the two aggregates model reservation release differently on purpose.

## Reconciliation notes

Same as every aggregate so far — no `EntityData`/`UpdateData`/`VoucherTranslationData` wrapper types created; every `Create`/update/translate method takes flat parameters (Domain Rule 2). Status-transition method names (`MarkIssued`, `MarkReserved`, `MarkRedeemed`) were chosen to avoid colliding with the collection-add methods (`AddIssue`, `AddReservation`, `AddRedemption`) that share the same underlying verb.
