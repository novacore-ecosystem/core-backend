# Task 1: Add MiddleName — Domain + Persistence

**Status:** Done (2026-07-28)
**Category:** Backend — Domain/Persistence

## What was done

`MiddleName` added to `UserProfile` (default `""`, positional param in `Create`/`UpdateProfile`, no default-optional shim — all call sites updated in the same pass since Tasks 2/3 landed together). `UserProfileConfig` maps it `HasMaxLength(50)` (matching the FluentValidation convention, not the pre-existing 256 used by FirstName/LastName). `UserSeeder` passes `string.Empty` for existing seed accounts. Migration `20260728030503_AddUserProfileMiddleName` generated via `dotnet ef migrations add`: `ADD COLUMN middle_name varchar(50) NOT NULL DEFAULT ''` — no backfill needed, fully reversible `Down`. Verified: `dotnet build` on `User.API`/`Auth.API` succeeds.

## Objective

Add an optional `MiddleName` field to `UserProfile`, the EF mapping, and a migration — the foundation every other name-model task (2, 3) and the search projection (Task 8) builds on.

## Current state (grounded findings)

- `User.Domain/Entities/UserProfile.cs:15-16` has only `FirstName`/`LastName` (both `private set`, default `string.Empty`). `Create(...)` (28-51) and `UpdateProfile(firstName, lastName, phoneNumber)` (53-60) take exactly these two name parts positionally.
- **No validation invariants exist in the Domain entity at all** — no guard clause, no length/null check anywhere in `Create`/`UpdateProfile`. Every constraint lives in FluentValidation validators (`CreateUserValidator.cs:25-31`, `UpdateUserValidator.cs` — note: the class inside is actually named `UpdateUserCommandValidator`, a pre-existing filename/classname mismatch, not something this task needs to fix but worth not copying).
- `User.Persistence/Configs/UserProfileConfig.cs:31-37` maps `FirstName`/`LastName` at **maxlength 256**, required — this already disagrees with the FluentValidation rule of `Length(1, 50)`. Decide once, for `MiddleName`, which convention is authoritative (recommend matching validator length, i.e. 50, and treat the 256 DB columns as pre-existing slack, not a pattern to replicate).
- Latest migrations for reference/shape: `20260721044607_AddUserPhoneSearchFields.cs` (adds columns + backfill SQL + indexes, fully reversible `Down`) and `20260724060832_AddUserProfileRoles.cs` (adds a nullable-safe default-valued column). `MiddleName` should follow the same shape: nullable-safe, default `""`, reversible.
- `UserSeeder.cs:20-21` currently sets `FirstName`/`LastName` both to `account.Username` — decide whether seed data should include a token `MiddleName` (e.g. empty, matching the "optional, default empty" requirement) or omit it; omitting is lower-risk and matches "no data loss" for existing rows.

## Scope

- `UserProfile.cs`: add `MiddleName` (string, default `string.Empty`), add it as an optional parameter to `Create(...)` and `UpdateProfile(...)` (default `""` to avoid breaking every call site in one commit — Task 2/3 update call sites deliberately).
- `UserProfileConfig.cs`: add `HasMaxLength(50)` (recommended, matching the validator convention rather than the pre-existing 256) + `IsRequired()` with default `""`.
- New EF Core migration: add `middle_name` column (`varchar(50)`, `NOT NULL DEFAULT ''`), no backfill needed (default satisfies existing rows), no index (MiddleName is not independently filterable per the request — it only feeds the display-name formatter and search, not direct Postgres criteria filtering).
- `UserSeeder.cs`: decide and apply the seed convention above.

## Dependencies

- **Depends on:** nothing (first task in the chain).
- **Blocks:** Task 2 (Application layer needs the field to exist), Task 8 (search projection needs it as a document input).

## Estimated complexity

Small. Single entity, one migration, no cross-service impact yet (that's Task 3).

## Risks

- If the DB column length and validator length are chosen inconsistently (repeating the existing `FirstName`/`LastName` 256-vs-50 mismatch), it silently allows longer values than the API claims to accept — pick one number and use it in both places for `MiddleName`.
- Making `Create`/`UpdateProfile` parameters optional-with-default avoids a big-bang multi-file change, but leaves a window where the parameter is plumbed at the Domain layer with no caller providing it — track this explicitly as "not yet wired end-to-end" until Task 2/3 close it out, not "done."

## Completion checklist

- [ ] `MiddleName` property added to `UserProfile`, defaulted, optional in `Create`/`UpdateProfile`
- [ ] `UserProfileConfig.cs` mapping added with an explicit, documented length decision
- [ ] Migration created, verified reversible (`Down` drops the column cleanly), applied against a local Postgres instance
- [ ] `UserSeeder.cs` reviewed and updated per the decision above
- [ ] Unit test: `UserProfile.Create`/`UpdateProfile` default `MiddleName` to empty when omitted (feeds Task 17)
