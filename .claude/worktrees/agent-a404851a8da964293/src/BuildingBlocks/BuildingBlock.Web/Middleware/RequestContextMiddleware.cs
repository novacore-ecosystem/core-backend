using System.Security.Claims;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Context;

using Microsoft.AspNetCore.Http;

namespace NovaCore.BuildingBlock.Web.Middleware;

/// <summary>
/// The only component allowed to read request identity (JWT claims, correlation/idempotency/locale
/// headers) off HttpContext. Parses it exactly once per request and hands the result to
/// RequestContext.Initialize - every downstream component (handlers, EF interceptors, model
/// conventions, ...) reads RequestContext.Current instead of touching HttpContext itself.
/// Must run after UseAuthentication/UseAuthorization (so User claims are populated) and before
/// everything else in the custom pipeline. Anonymous requests (login, refresh token, health
/// checks, public endpoints) simply get null UserId/TenantId and an empty ScopeIds - this
/// middleware never rejects a request for missing identity, that is an authorization concern, not
/// this one's.
///
/// Explicitly clears the request context in a `finally` block once the pipeline finishes, rather
/// than relying on AsyncLocal's own per-call-chain scoping alone - see RequestContext.Clear.
/// </summary>
public sealed class RequestContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        var data = new RequestContextData
        {
            UserId = TryParseGuid(user.FindFirst(ClaimTypes.NameIdentifier)?.Value),
            TenantId = TryParseGuid(user.FindFirst(AppClaimTypes.TenantId)?.Value),
            ScopeIds = ParseScopeIds(user),
            CorrelationId = ResolveCorrelationId(context),
            IdempotencyKey = context.Request.Headers.TryGetValue(HeaderKeyConstant.IdempotencyKey, out var idempotencyKey)
                ? idempotencyKey.ToString()
                : null,
            Locale = context.Request.Headers.TryGetValue(HeaderKeyConstant.Locale, out var locale)
                ? locale.ToString()
                : null,
        };

        RequestContext.Initialize(data);

        try
        {
            await _next(context);
        }
        finally
        {
            RequestContext.Clear();
        }
    }

    /// <summary>Scopes are already expanded (descendants included) by the token issuer, so this is
    /// a plain read of every AppClaimTypes.ScopeIds claim present - one claim per accessible
    /// Scope, the standard inbound-JWT shape for an array claim. No tree traversal, no recursion,
    /// no call back to Auth Service - see docs/reference/tenant-convention.md.</summary>
    private static IReadOnlyCollection<Guid> ParseScopeIds(ClaimsPrincipal user) =>
        user.FindAll(AppClaimTypes.ScopeIds)
            .Select(claim => TryParseGuid(claim.Value))
            .Where(scopeId => scopeId.HasValue)
            .Select(scopeId => scopeId!.Value)
            .ToArray();

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderKeyConstant.CorrelationId, out var value)
            && !string.IsNullOrWhiteSpace(value))
            return value.ToString();

        return Guid.NewGuid().ToString();
    }

    private static Guid? TryParseGuid(string? value) =>
        Guid.TryParse(value, out var parsed) ? parsed : null;
}
