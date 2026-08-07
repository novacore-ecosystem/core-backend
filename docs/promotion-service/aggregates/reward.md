# Reward Aggregate

**Scope:** Domain-layer facts for the `RewardProgram` aggregate, implemented Phase 2.3. Structural only — no business rule (eligibility, dispatch, claim enforcement) is implemented yet.

## Aggregate boundary

`RewardProgram` (`Promotion.Domain/Entities/Rewards/RewardProgram.cs`) is the aggregate root — `AggregateRoot<Guid>`, `IAuditable`, `ITenantEntity`. It owns, via navigation + internal construction: `RewardDefinition`, `RewardDistribution`.

`RewardExecution`, `RewardReservation`, `RewardClaim`, `RewardHistory` are **not** part of the navigation graph (no Navigation section given for any of them), so all four have a **public** `Create` and are related by id only. None of them reference a `RewardId` pointing at a dedicated "Reward" entity — no such entity exists in this pass (only `RewardProgram`/`RewardDefinition`/`RewardDistribution` do); `RewardId` is kept as a loose `Guid` reference for whatever "issued reward" concept a later phase formalizes.

## `Code` now uses `EntityCode` (Phase 2.6 correction)

Unlike every prior aggregate (Campaign, Promotion, Coupon, Voucher, Loyalty), the original brief gave `RewardProgram` **no `ValueObjects` section**, so `Code` stayed a plain `string` through Phase 2.5. The Phase 2.6 Final Domain Correction closed this gap — `RewardProgram.Code` now uses the shared `EntityCode` Value Object, same as every other aggregate root.

## `RewardDistribution.Status` reuses `DistributionStatus`

`RewardDistribution` has a `Status` field with no enum type specified, but the Distribution aggregate (implemented in this same phase) defines a `DistributionStatus` enum whose semantics match exactly, and `RewardDistribution.DistributionJobId` already ties the two together contextually. Reused directly rather than declaring a duplicate enum.

## Entities

| Entity | Shape | Notes |
|---|---|---|
| `RewardProgram` | `AggregateRoot<Guid>` | Code (`EntityCode`, Phase 2.6 — was plain string)/Name/Description/Status/StartTime/EndTime |
| `RewardDefinition` | `BaseEntity<Guid>` | ProgramId/RewardType/Configuration (opaque string blob) |
| `RewardDistribution` | `BaseEntity<Guid>` | ProgramId/DistributionJobId/Status (`DistributionStatus`, reused)/ScheduledAt/ExecutedAt — `DistributionJob` navigation added Phase 2.6 (forward reference to the independent root, same pattern as `Coupon.Campaign`) |
| `RewardExecution` | `BaseEntity<Guid>` | DistributionId/UserId/ExecutionKey/Status/ExecutedAt — public `Create` |
| `RewardReservation` | `BaseEntity<Guid>` | RewardId/UserId/ReservedAt/ExpiredAt — public `Create` |
| `RewardClaim` | `BaseEntity<Guid>` | RewardId/UserId/ClaimedAt — public `Create` |
| `RewardHistory` | `BaseEntity<Guid>` | RewardId/Action/OperatorId — `CreatedAt` inherited, public `Create` |
| `RewardProgramTranslation` | `BaseEntity<Guid>` | Added Phase 2.5: `Id = RewardProgram.Id`, composite `(Id, LanguageCode)`. Exposed via `RewardProgram.Translate(languageCode, name, description)` — upsert, per [../entities/translation-workflow.md](../entities/translation-workflow.md) |

## Enums

- `ProgramStatus` (shared, consolidated Phase 2.5) — Draft/Active/Paused/Expired/Archived. Was `RewardProgramStatus` until merged with 3 identical siblings — see [../enums/README.md](../enums/README.md).
- `RewardType` — Coupon/Voucher/Point/Gift/Cashback (given explicitly) — also reused by `DistributionItem.RewardType` in the Distribution aggregate.

## Indexes (design only — written in Phase 3)

- `RewardProgram`: `(Code)` unique · `(Status)`
- `RewardExecution`: `(ExecutionKey)` unique · `(UserId)`

## Reconciliation notes

No `EntityData`/`UpdateData`/`RewardProgramTranslationData` wrapper types created (Domain Rule 2).
