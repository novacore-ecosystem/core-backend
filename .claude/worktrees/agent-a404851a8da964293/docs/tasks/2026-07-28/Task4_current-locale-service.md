# Task 4: Build `ICurrentLocaleService` (Locale-from-Header Ambient Context)

**Status:** Done (2026-07-28)
**Category:** Localization

## What was done

`HeaderKeys.Locale = "Accept-Language"` added (reusing the standard header, per the finding that the frontend already sends it — no frontend change needed). `ICurrentLocaleService` (single `GetLocale()` method) added to `BuildingBlock.Application.Abstractions.Services`, mirroring `ICurrentUserService`'s shape exactly. `CurrentLocaleService` implemented in `BuildingBlock.Infrastructure/CurrentUser/` (co-located with `CurrentUserService`, same ambient-context precedent), `IHttpContextAccessor`-backed, parses only the first comma/semicolon-delimited segment of the header (no full RFC 4647 quality-value negotiation — deliberately simple, per the task's own risk note about not over-engineering for a small known locale set), falls back to `"en"` when the header is missing/empty. `AddCurrentLocale()` extension added alongside `AddCurrentUser()`, wired into `User.API/DependencyInjection.cs`'s `AddPresentation` chain (other services not touched — out of this epic's scope, easy to extend later).

## Objective

Make the caller's locale available anywhere in the request pipeline via DI, with zero per-endpoint boilerplate — mirroring the existing `ICurrentUserService` pattern exactly, since that's this repo's only precedent for "read something ambient off the request and expose it as a scoped service."

## Current state (grounded findings)

- **No locale/culture handling of any kind exists anywhere in this repo today.** Confirmed: zero hits for `RequestLocalizationMiddleware`, `IRequestCultureFeature`, `IStringLocalizer`, `Accept-Language`, across all of `src/`. `HeaderKeys` (`BuildingBlock.SharedKernel/Constants/HeaderKeys.cs:3-10`) currently defines only `CorrelationId`, `TenantId`, `ClientVersion`, `DeviceId`, `IdempotencyKey` — no `Locale`/`Language` entry.
- **The frontend already sends the header.** `NovaCoreUI/src/shared/lib/api/client.ts:23` sets `Accept-Language` from `useLocaleStore.getState().locale` on every request, unconditionally, today — value is always `"en"` in practice since no locale switcher exists yet, but the wire mechanism is live. **This means the backend does not need to ask the frontend for anything new** — reuse the standard `Accept-Language` header name rather than inventing a custom one.
- **The precedent to mirror:** `ICurrentUserService` — interface in `BuildingBlock.Application/Abstractions/Services/ICurrentUserService.cs` (33 lines: `GetUserId()`, `GetUserEmail()`, `GetCorrelationId()`, etc.), implementation `BuildingBlock.Infrastructure/CurrentUser/CurrentUserService.cs` (`sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)`, captures `_httpContext` once, reads JWT claims and — for `GetCorrelationId()`, lines 95-102 — reads a header directly off `_httpContextAccessor.HttpContext.Request.Headers`, exactly the pattern a locale reader needs). Registered via `AddCurrentUser()` (`CurrentUserExtensions.cs:11-13`: `services.AddHttpContextAccessor().AddScoped<ICurrentUserService, CurrentUserService>()`), called from every service's `AddPresentation`.
- Consumption is via constructor injection in Application-layer handlers (~20+ call sites), **not** through Carter endpoint code — endpoints only pass `[FromServices] ISender sender`; this is the idiom Task 5's DisplayName-consuming handlers should follow too.
- `BuildingBlock.Web`'s `RequiredHeadersMiddleware` is the *other* existing "read a header, stash it" precedent, but it stashes into `HttpContext.Items` (untyped dictionary), not a typed DI service — worse fit than `ICurrentUserService` for this purpose; don't use it as the template.

## Scope

- `BuildingBlock.SharedKernel/Constants/HeaderKeys.cs`: add `Locale = "Accept-Language"` (reuse the standard header, don't invent `X-Locale`).
- `BuildingBlock.Application/Abstractions/Services/ICurrentLocaleService.cs` (new): `string GetLocale()` (with a documented default, e.g. `"en"`, when the header is absent or unrecognized).
- `BuildingBlock.Infrastructure/CurrentUser/` or a new `CurrentLocale/` folder: `CurrentLocaleService : ICurrentLocaleService`, `IHttpContextAccessor`-backed, reads `HeaderKeys.Locale` off `_httpContextAccessor.HttpContext.Request.Headers`, falls back to a default constant if missing/unparseable. Consider whether `Accept-Language`'s real-world format (`en-US,en;q=0.9`) needs a quality-value parser, or whether this app only ever expects a bare locale tag (`en`, `vi-VN`) since the frontend controls what it sends — recommend a simple first-segment parse (split on `,`), not a full RFC 4647 negotiation engine, since there's no server-side content negotiation need here beyond picking one of a small, known `SUPPORTED_LOCALES` set.
- `AddCurrentLocale()` extension (mirrors `AddCurrentUser()`), called from each service's `AddPresentation` alongside it — start with User service only (this epic's scope), leave it trivially reusable for other services later.

## Dependencies

- **Depends on:** nothing technical (pure new capability) — but coordinate the header-name decision with the frontend team even though no frontend code change is required, so everyone knows why no frontend PR accompanies this backend change.
- **Blocks:** Task 5 (DisplayName formatter consumes `ICurrentLocaleService`).

## Estimated complexity

Small — mirrors an existing, well-understood pattern almost line for line.

## Risks

- If a custom header name were chosen instead of reusing `Accept-Language`, it would require a (currently unnecessary) frontend change — reusing the standard header avoids that entirely; don't deviate without a documented reason.
- Over-engineering the `Accept-Language` parser (full RFC 4647 quality-value negotiation) for a single-consumer, small-known-locale-set use case is wasted complexity — keep it simple per this repo's general "don't build for hypothetical future requirements" convention.

## Completion checklist

- [ ] `HeaderKeys.Locale` added
- [ ] `ICurrentLocaleService` defined in `BuildingBlock.Application`
- [ ] `CurrentLocaleService` implemented in `BuildingBlock.Infrastructure`, `AddCurrentLocale()` registered
- [ ] Wired into User service's `AddPresentation` (or equivalent DI composition)
- [ ] Unit test: header present/absent/malformed → correct locale/default resolution
