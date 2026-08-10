# Task 2: Add MiddleName — Application Layer (User service)

**Status:** Done (2026-07-28)
**Category:** Backend — Application

## What was done

`MiddleName` threaded through `CreateUserCommand`/`CreateUserValidator` (optional, `MaximumLength(50)`, no `NotEmpty`), `UpdateUserCommand`/`UpdateUserCommandValidator`, `IUserProfileWriteService.UpdateProfileDetailsAsync`/`UserProfileWriteService`, `CreateUserRequest`/`UpdateUserRequest` (Carter endpoints, Swagger `API_DESC` updated). `UserCriteriaDefinition` decision: `MiddleName` added as its own `KeywordSearchable()` field, matching `FirstName`/`LastName`'s existing (case-sensitive) treatment for consistency — not given `.IgnoreCase()` since neither sibling field has it today; revisit once Task 10 (ES cutover) supersedes this Postgres path. `GetUserResponse`/`GetUserDetailResponse`/`SearchUsersItemResponse` gained both `MiddleName` and `DisplayName` (the latter from Task 5, landed together since they're the same response records). Mapster's convention-based mapping needed no explicit config — `MiddleName` populates automatically since it's now on `UserProfile`; `DisplayName` (not on the entity) is set via `with { DisplayName = ... }` after `.Adapt<T>()` in `GetUserHandler`/`SearchUsersHandler`.

## Objective

Wire `MiddleName` (from Task 1) through every User-service-internal Command/Query/DTO/validator, so User's own API surface fully supports it before cross-service propagation (Task 3).

## Current state (grounded findings)

Exact files that assume only two name parts today, all under `User.Application`/`User.API`:

- `Commands/CreateUser/CreateUserCommand.cs:3-10`, `CreateUserHandler.cs:19-48` (trims `FirstName`/`LastName` at 30-37, builds `UserProfile.Create(...)`), `CreateUserValidator.cs:25-31` (`NotEmpty()`, `.Length(1,50)` each).
- `Commands/UpdateUser/UpdateUserCommand.cs:3-9`, `UpdateUserHandler.cs:9-18` (delegates to `IUserProfileWriteService.UpdateProfileDetailsAsync(userId, firstName, lastName, phoneNumber, ct)` — positional string params, no slot for a third name part), `UpdateUserValidator.cs` (class `UpdateUserCommandValidator`, rules at lines 14-20).
- `Queries/GetUser/GetUserQuery.cs:3-14` — `GetUserResponse` has no `Roles` field (unlike the other two) and no `MiddleName` yet.
- `Queries/GetUserDetail/GetUserDetailQuery.cs:3-15` — **takes no parameters**, scoped to `ICurrentUserService.GetUserId()` internally (`GetUserDetailHandler.cs:16-17`) — note this when adding fields, this query returns "my own profile," not an arbitrary id.
- `Queries/SearchUsers/SearchUsersQuery.cs`, `SearchUsersItemResponse` — Mapster-adapted from `UserProfile` (`SearchUsersHandler.cs:12-19`, uses `.Adapt<SearchUsersItemResponse>()`), so adding `MiddleName` to the response record is enough for this path — **no explicit mapping config exists anywhere in User service** (`IRegister` classes: none found), Mapster matches by property name only. Same for `GetUserHandler.cs:12-18`.
- `Features/Users/Search/UserCriteriaDefinition.cs:10-21` — `FirstName`/`LastName` are `KeywordSearchable()` only (no `.Sortable()`, no `.IgnoreCase()` — inconsistent with `UserName`/`Email` which have both). Decide whether `MiddleName` joins the same keyword-search set with the same (currently case-sensitive) behavior, or whether this whole endpoint is superseded by Task 10's ES cutover before it matters.
- `Abstractions/Persistence/UserProfiles/IUserProfileWriteService.cs:11` — `UpdateProfileDetailsAsync(Guid id, string firstName, string lastName, string phoneNumber, ct)` needs a `middleName` parameter.
- `User.API/Endpoints/CreateUser.cs:12-19` (`CreateUserRequest`) and `UpdateUser.cs:9-12` (`UpdateUserRequest`) — REST request DTOs.

## Scope

- Add `MiddleName` (optional, default `""`) to: `CreateUserCommand`, `CreateUserRequest`, `CreateUserValidator` (optional — `MaximumLength(50)`, no `NotEmpty()`), `UpdateUserCommand`, `UpdateUserRequest`, `UpdateUserValidator`/`UpdateUserCommandValidator`, `IUserProfileWriteService.UpdateProfileDetailsAsync`, `UserProfileWriteService` implementation, `GetUserResponse`, `GetUserDetailResponse`, `SearchUsersItemResponse`.
- Decide and apply `UserCriteriaDefinition`'s treatment of `MiddleName` (own field vs. folded into a combined name search) — coordinate with Task 5 (DisplayName/SearchName) so this isn't designed twice.
- Update Swagger `API_DESC` prose in each affected Carter endpoint file (`CreateUser.cs`, `UpdateUser.cs`, `SearchUsers.cs`) to mention `MiddleName` so docs don't drift, per this repo's existing convention of hand-written per-endpoint description arrays.

## Dependencies

- **Depends on:** Task 1 (needs `UserProfile.MiddleName` to exist).
- **Blocks:** Task 3 (cross-service propagation reuses these DTOs' shape as the template), Task 5 (formatter needs `MiddleName` on the entity/response to consume), Task 8 (search projection input).

## Estimated complexity

Medium — mechanical but touches ~10 files; the only judgment call is the `UserCriteriaDefinition` search-field decision.

## Risks

- Mapster's convention-based mapping means a typo'd property name silently fails to map rather than erroring at compile time — verify each new DTO field actually populates via a runtime check, not just a compile pass.
- `GetUserResponse` (no Roles) vs. `GetUserDetailResponse`/`SearchUsersItemResponse` (have Roles) already diverge in shape — don't "fix" that divergence as a drive-by in this task; scope creep here risks breaking `GetUser`'s existing (Admin-only, single-user) contract for API consumers unrelated to this refactor.

## Completion checklist

- [ ] `MiddleName` added to all Commands/Requests/Validators listed above, optional with sensible max length
- [ ] `IUserProfileWriteService`/`UserProfileWriteService` updated, `UpdateProfile` domain call now passes `MiddleName` through
- [ ] `MiddleName` added to `GetUserResponse`, `GetUserDetailResponse`, `SearchUsersItemResponse`
- [ ] `UserCriteriaDefinition` decision made and documented (own field / combined / deferred to ES cutover)
- [ ] Swagger `API_DESC` text updated in each touched endpoint
- [ ] Existing User integration/unit tests (if any target these handlers) updated to pass/assert `MiddleName`
