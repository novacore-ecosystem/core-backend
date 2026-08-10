# Task 1: Customer (User) Delete endpoint does not exist

**Status:** Open.

## Source

Full-system business-requirements audit, 2026-07-27 (`docs/2026-07-27-audit-tasks.md` in this repo, now superseded by this per-task breakdown). Requirement: "Customer CRUD."

## Current state

`DeleteUserCommand`/`DeleteUserHandler` exist (`User.Application/Features/Users/Commands/DeleteUser/*.cs`) but have zero callers anywhere in the solution — `grep "DeleteUserCommand("` only finds the definition. There is no `DeleteUser.cs` under `User.API/Endpoints/`, i.e. no `DELETE /profiles/{userId}` route is registered at all. This looks like dead code left over from a saga-rollback compensator path, never wired to a real endpoint.

Confirmed on the frontend side too: `UsersPage.tsx:105-109` renders a disabled `DeleteButton` with tooltip text key `deleteNotAvailable` — the frontend already anticipated this gap.

## Why this matters

"Customer CRUD" is an explicit, named business requirement. Without a working Delete, CRUD is incomplete regardless of how solid Create/Read/Update are.

## Open questions

- Should delete be a hard delete or a soft/status-based delete, given Orders reference `OrderOwner` snapshots (not a live FK to `UserProfile`) — does deleting a user who has placed orders need any special handling, or is it safe because Order already snapshots owner info independently?
- Should this reuse the existing (currently orphaned) `DeleteUserCommand`/`Handler`, or does it need reworking once actually exercised via a real endpoint?

## Suggested acceptance criteria

- `DELETE /profiles/{userId}` exists, admin-only, returns success on delete.
- Behavior with existing Orders referencing the user is explicit and tested (not left ambiguous).

**Cross-ref:** NovaCoreUI `docs/tasks/2026-07-27/Task5_customer-delete-button-disabled.md`.
