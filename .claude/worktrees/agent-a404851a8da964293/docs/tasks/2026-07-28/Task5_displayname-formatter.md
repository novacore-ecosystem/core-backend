# Task 5: Build Locale-Aware DisplayName Formatter

**Status:** Done (2026-07-28)
**Category:** Localization

## What was done

`IUserDisplayNameFormatter`/`UserDisplayNameFormatter` added to `User.Application` (per the task's recommendation — only User needs it today, not promoted to a BuildingBlock). `Format(firstName, middleName, lastName, locale)` orders parts `vi`-prefixed locales as Last-Middle-First, everything else (including the `"en"` default) as First-Middle-Last, joining only non-empty/trimmed parts so an empty `MiddleName` never produces a double space. Registered as a DI singleton (stateless, pure function) in `User.Application/DependencyInjection.cs`. Wired into all three response-building handlers: `GetUserHandler`, `GetUserDetailHandler`, `SearchUsersHandler` (batch case: locale resolved once per request, not per row). `NotificationTriggerConsumer`'s greeting adoption explicitly deferred (see Task 3's note) — recorded as a decision, not an oversight. Unit tests for the formatter itself (en/vi ordering, empty-middle-name, unknown-locale fallback) and the locale-header integration test are still open — tracked under Task 17 (Testing), not duplicated here.

## Objective

A single, reusable service that turns `(FirstName, MiddleName, LastName, locale)` into a display string per-locale (e.g. `vi-VN`: `LastName MiddleName FirstName`; `en-US`/default: `FirstName MiddleName LastName`), consumed by every response model that currently prints a user's name — without persisting a formatted string in the database.

## Current state (grounded findings)

- **Nothing like this exists anywhere in the backend today.** Grep for `DisplayName` as a person-name concept returns zero hits (the only matches are unrelated: ASP.NET `.WithDisplayName("...")` Swagger metadata, and `NotificationChannel.DisplayName`). `CultureInfo` is used exactly twice in the whole repo, both for value-conversion parsing (`BuildingBlock.Criteria/Building/CriteriaValueConverter.cs:44`, a Mongo cursor date parser), unrelated to name formatting.
- **Closest existing analog is on the frontend, not backend**: `NovaCoreUI/src/features/auth/api/auth.queries.ts:15` — `toSessionUser()`'s `[detail.firstName, detail.lastName].filter(Boolean).join(' ').trim() || detail.userName || detail.email || 'User'`. This is a hardcoded, locale-blind, two-part concatenation — it's the thing this backend formatter should make obsolete once it ships (Frontend Task F3 switches to consuming the server-provided `DisplayName` instead of recomputing it client-side).
- `UsersPage.tsx:73` has the identical hand-rolled concatenation for the admin user-list name column — same story.
- Order's `OrderOwner.CustomerName` (`Order.Domain/Entities/OrderOwner.cs:17`) is a free-text, user-typed snapshot at checkout, not derived from `UserProfile` — it is **not** a candidate for this formatter to touch in this epic's scope (a future decision, not this task's).

## Scope

- New service, e.g. `IUserDisplayNameFormatter` (or a static, pure-function helper if no DI-injected state is actually needed beyond the locale itself — prefer the simplest shape that's still unit-testable). Recommended home: `User.Application` (only User needs it today; promote to a `BuildingBlock` only if/when a second consumer materializes — per this repo's "don't design for hypothetical future requirements" convention, do not pre-emptively generalize).
- Input: `FirstName`, `MiddleName` (optional/empty), `LastName`, `locale` (string, from Task 4's `ICurrentLocaleService`). Output: a single formatted string, whitespace-collapsed (so an empty `MiddleName` doesn't leave a double space).
- Locale → order mapping: a small, extensible lookup (e.g. a dictionary or switch keyed on locale prefix) — start with exactly the two orderings the request specifies (`vi-VN` → Last Middle First; default/anything else → First Middle Last), structured so adding a third locale later is a one-line addition, not a redesign.
- Wire into: `GetUserResponse`, `GetUserDetailResponse`, `SearchUsersItemResponse` (add a `DisplayName` field to each, computed at response-build time using the current request's locale via Task 4 — **never stored**, per the explicit "do NOT persist localized FullName" requirement).
- Optional, lower-priority wiring (flag for the team, not required for this task's completion): `NotificationTriggerConsumer.cs:99`'s greeting — switching `$"Hi {data.FirstName}"` to use the formatter would need `MiddleName`/`LastName` threaded through the same event (already true after Task 3) and a locale to format with (Notification has no request-scoped locale — would need the user's stored/preferred locale from somewhere, which doesn't exist yet; treat as a separate follow-up, don't block this task on it).

## Dependencies

- **Depends on:** Task 2 (needs `MiddleName` on the entity/DTOs), Task 4 (needs `ICurrentLocaleService`).
- **Blocks:** Task 8 (search projection's `SearchName`/`DisplayName` document fields should reuse this formatter's name-composition logic, or at least stay consistent with it), Frontend F3 (UI switches from client-side concatenation to server-provided `DisplayName`).

## Estimated complexity

Small-to-Medium — the formatting logic itself is small; the medium part is deciding the right seam (pure function vs. service, where it lives) and threading the locale through three response builders consistently.

## Risks

- If `SearchName` (Task 7, for Elasticsearch) and `DisplayName` (this task) are designed independently by different people/sessions, they risk drifting (e.g. different whitespace-collapsing rules) — this task and Task 7 should share the same underlying name-token-cleaning helper even though `SearchName` additionally needs accent-folding/lowercasing that `DisplayName` must **not** apply (a display name should keep original casing/accents; only the search index needs the folded version).
- Locale defaulting behavior (Task 4's fallback) directly determines what most real traffic sees today, since almost all traffic will resolve to `"en"` in practice (no locale switcher UI yet) — test the non-default (`vi-VN`) path explicitly rather than trusting it'll be exercised organically.

## Completion checklist

- [ ] Formatter service/function implemented with the two specified orderings, extensible for more
- [ ] Whitespace-collapse verified for empty `MiddleName` (no double spaces, no leading/trailing space)
- [ ] `DisplayName` added to `GetUserResponse`, `GetUserDetailResponse`, `SearchUsersItemResponse`, populated via `ICurrentLocaleService`
- [ ] Unit tests: en-US ordering, vi-VN ordering, empty MiddleName, unknown/unsupported locale falls back sensibly
- [ ] Explicit decision recorded on whether `NotificationTriggerConsumer`'s greeting adopts this formatter now or is deferred
