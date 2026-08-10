# Task 13: Keyword/filter search has no case-insensitive option (shared infra + User + Order)

**Status:** Resolved 2026-07-27.

## Source

SmartCommerce V3 Search/Cart/Stock checklist audit, 2026-07-27 (read-only, no fixes applied).

## Current state

The shared search builder used by User and Order (`BuildingBlock.Criteria`) has no case-insensitivity lever anywhere in the pipeline:

- `CriteriaPredicateBuilder.Build` (`src/BuildingBlocks/BuildingBlock.Criteria/Building/CriteriaPredicateBuilder.cs:139`) builds keyword matches as plain `Expression.Call(property, ContainsMethod, Expression.Constant(keyword))` — no `StringComparison` overload.
- Per-field `StartsWith`/`EndsWith`/`Contains` operators (same file, lines 82-89) follow the identical pattern.
- `CriteriaFieldMetadata<TEntity>` (`src/BuildingBlocks/BuildingBlock.Criteria/Definition/CriteriaFieldMetadata.cs:8-17`) has no `IgnoreCase` property to opt a field in/out.
- Underlying Postgres columns (`UserProfileConfig.cs:11-17`, `OrderOwnerConfig.cs:17`) are plain `text` with no `citext`/collation override (repo-wide grep for `citext` — zero hits), so `Contains`/`StartsWith`/`EndsWith`/`Eq` are case-sensitive under Postgres' default collation.
- Consumers: `UserCriteriaDefinition` (`src/Services/User/User.Application/Features/Users/Search/UserCriteriaDefinition.cs:11-14`) marks `UserName`/`Email` `KeywordSearchable()`; `OrderCriteriaDefinition` (`src/Services/Order/Order.Application/Features/Orders/Search/OrderCriteriaDefinition.cs:10`) marks `Owner.CustomerName` the same way. Both inherit the case-sensitive behavior with no way to override it today.

Product's keyword search is unaffected — it's Elasticsearch-backed and already case-insensitive via the standard analyzer, so this task is scoped to the Postgres/Criteria-backed services (User, Order).

## Why this matters

Checklist requirement: User search (Email/Username) and Order search (Customer Name) must be case-insensitive. Today a search for `"john"` will not match `"John"`, which is the single most common real-world search miss for name/email fields.

## Suggested acceptance criteria

- Add an `IgnoreCase` toggle to `CriteriaFieldMetadata`/the field-declaration fluent API (e.g. `.String().IgnoreCase().KeywordSearchable()`), reusable by any service, not just User/Order.
- `CriteriaPredicateBuilder` respects the toggle for both keyword-OR and explicit per-field operators (`Eq`/`Contains`/`StartsWith`/`EndsWith`), using a comparison that doesn't depend on DB collation (e.g. `EF.Functions.ILike` for Postgres, or normalize with `.ToLower()` on both sides consistently).
- `UserCriteriaDefinition.Email`/`UserName` and `OrderCriteriaDefinition.Owner.CustomerName` opt into `IgnoreCase`.
- Verify case-insensitivity doesn't silently break existing index usage (check whether `ILike`/`.ToLower()` needs a functional index on affected columns for acceptable query performance at scale — flag as a follow-up if index work is out of scope for this pass).

## What was done

Added `IgnoreCase` to `CriteriaFieldMetadata<TEntity>` (`BuildingBlock.Criteria/Definition/CriteriaFieldMetadata.cs`) and a matching `.IgnoreCase()` fluent modifier on `ConfiguredField<TEntity>`. `CriteriaPredicateBuilder<TEntity>` now wraps both sides of `Contains`/`StartsWith`/`EndsWith`/`Eq`/`Ne` (and each keyword-OR clause) in `.ToLower()` when the field opts in — chosen over `.ToLowerInvariant()` because EF Core's relational providers (Npgsql included) specifically pattern-match `.ToLower()`/`.ToUpper()` for SQL translation (`LOWER()`), not the invariant overloads, which would otherwise force client-side evaluation. `UserCriteriaDefinition.UserName`/`.Email` and `OrderCriteriaDefinition.Owner.CustomerName` now opt in via `.IgnoreCase()`. Doc strings updated on both `SearchUsers.cs` and `SearchOrders.cs`. Scoped builds of `BuildingBlock.Criteria`, `User.API`, and `Order.API` all pass with 0 errors.

## What wasn't done

No functional index (`lower(email)`, `lower(user_name)`, `lower(customer_name)`) was added — the existing Postgres columns are plain `text` with a regular index (if any), so `WHERE LOWER(email) LIKE ...` won't use it and will full-scan at scale. Flagging as a follow-up rather than doing it in this pass, since it's a migration + index-strategy decision (functional index vs. `citext` column type vs. trigram index for `Contains`) that wasn't asked for explicitly and affects write-path cost too.
