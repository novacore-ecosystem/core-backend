# Task 13: Extend `user.proto` with GetUser/GetUsers RPCs

**Status:** Done (2026-07-28)
**Category:** gRPC

## What was done

Added `GetUser`/`GetUsers` to `UserGrpcService`, additive (field 8 onward, existing `CreateUserProfile`/`CreateUserProfileRequest` untouched). `GetUserResponse`/`UserProfileItem` both carry a `found` bool (never omit a requested id from a batch response — mirrors `inventory.proto`'s `GetProductsStock` convention exactly) plus `display_name`, formatted at a fixed default locale (`"en"`) since a gRPC caller is a service, not an authenticated end-user request with its own `Accept-Language` — the same simplification the search index already uses. Verified via a scoped `BuildingBlock.Contract` build (proto stubs regenerate automatically at build time, no manual codegen step).

## Objective

Add single (`GetUser`) and batch (`GetUsers`) read RPCs to User's gRPC contract — today it has **none** — following the exact single/batch shape already proven by `inventory.proto`, so other services can eventually fetch User data efficiently without inventing a new contract style.

## Current state (grounded findings)

- **The entire current contract** (`BuildingBlock.Contract/Protos/user.proto`, 27 lines, quoted in full by the research agent):
  ```protobuf
  service UserGrpcService {
    rpc CreateUserProfile (CreateUserProfileRequest) returns (CreateUserProfileResponse);
  }
  ```
  One RPC, write-only, no `repeated` fields anywhere. **There is no read/lookup RPC of any kind today, single or batch.** This confirms the original request's framing ("many services retrieve User information... optimize gRPC") doesn't match reality — see `00-architecture-and-plan.md`'s finding #1. This task is pure greenfield addition.
- **The exact template to replicate** — `inventory.proto`'s already-proven batch pattern (confirmed live in production, consumed by both Order and Product):
  ```protobuf
  // Batch rollup across every warehouse for each requested variation - one round trip instead of N.
  // A variation id with no inventory rows at all comes back as total_quantity 0, not omitted.
  message GetProductsStockRequest {
    repeated string product_variation_ids = 1;
  }
  message ProductVariationStock {
    string product_variation_id = 1;
    int32 total_quantity = 2;
  }
  message GetProductsStockResponse {
    repeated ProductVariationStock items = 1;
  }
  ```
  Two conventions worth copying verbatim: **(a)** plural RPC name (`GetProductsStock`, not `GetProductStockByIds`) and **(b)** every requested id gets a response item even when not found — filled with a default/empty value, never silently omitted. This second point directly satisfies the original request's explicit requirement: "Even if some users no longer exist, return all successfully resolved users instead of failing the entire request" — the *existence* signal should be a field on each item (e.g. `found: bool`), not omission, so client-side merge logic never has to guess which ids vanished.
- `auth.proto`'s `GetUserRoles` is single-user only — no batch variant exists there either. Among the three existing contracts (User, Auth, Inventory), **Inventory is the only one with a real batch RPC today** — the concrete precedent to copy.
- Server-side today: `UserGrpcServiceImpl.cs:10-32` implements exactly `CreateUserProfile` as a thin adapter dispatching an internal *event* (not a Query) — there is no existing "gRPC read RPC dispatches a Query via `ISender`" example anywhere on this server. `docs/reference/grpc.md:18`'s stated convention ("parse request, dispatch a Command via `ISender`, no business logic") needs to be read as "Command **or Query**" for this task — the doc predates any read RPC existing.

## Scope

- `GetUserRequest { string user_id = 1; }` / `GetUserResponse { bool found = 1; string user_id = 2; string email = 3; string user_name = 4; string first_name = 5; string middle_name = 6; string last_name = 7; string display_name = 8; string phone_number = 9; repeated string roles = 10; string status = 11; }` — include `display_name` (Task 5's formatter output, using... whose locale? See risk below) since consuming services likely want a ready-to-use name, not just raw parts.
- `GetUsersRequest { repeated string user_ids = 1; }` / `UserProfileItem` (same fields as `GetUserResponse` minus the wrapper) / `GetUsersResponse { repeated UserProfileItem items = 1; }` — one item per requested id, `found = false` + empty fields for any id that doesn't resolve, mirroring `inventory.proto`'s "never omit" convention exactly.
- Add both RPCs to the `UserGrpcService` service definition alongside the existing `CreateUserProfile` (proto is additive — old clients unaffected).

## Dependencies

- **Depends on:** Task 2 (MiddleName must exist on the DTOs this mirrors), Task 5 (if `display_name` is included, needs the formatter — see risk below on locale).
- **Blocks:** Task 14 (server implementation), Task 15 (any consumer needs the contract to exist first).

## Estimated complexity

Small — proto definition is the easy part; the design decisions (field list, "never omit" semantics) are what this task file should lock in before Task 14 starts.

## Risks

- **`display_name` on a gRPC response has no request-scoped "current caller's locale" to format against** (unlike the REST `GetUser`/`GetUserDetail`/`SearchUsers` responses, which use `ICurrentLocaleService` off the *caller's* HTTP request) — a gRPC caller (e.g. Order) isn't a human with a locale preference, it's a service. Decide: format at a fixed default locale for gRPC responses (simplest, consistent with Task 8's index-time decision for the same reason), or add a `locale` field to the request so the calling service can pass through its own end-user's locale if it has one. Recommend the fixed-default approach for v1, matching Task 8's precedent, and revisit only if a real consumer needs otherwise.
- Proto field-numbering: this task adds fields to *new* messages, not existing ones, so there's no risk of breaking `CreateUserProfileRequest`'s existing field numbers — keep it that way, never renumber existing fields.

## Completion checklist

- [ ] `GetUserRequest`/`GetUserResponse` added, `found` semantics documented
- [ ] `GetUsersRequest`/`UserProfileItem`/`GetUsersResponse` added, "never omit, `found=false` instead" documented and matched to `inventory.proto`'s exact convention
- [ ] `display_name`-at-fixed-locale decision recorded explicitly
- [ ] Proto regenerated, confirmed additive (existing `CreateUserProfile` clients unaffected)
