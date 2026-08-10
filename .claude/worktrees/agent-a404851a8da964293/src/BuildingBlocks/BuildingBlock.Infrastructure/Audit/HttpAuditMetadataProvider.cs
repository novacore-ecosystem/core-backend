using System.Diagnostics;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Audit;

using Microsoft.AspNetCore.Http;

namespace NovaCore.BuildingBlock.Infrastructure.Audit;

/// <summary>
/// HTTP-request-aware <see cref="IAuditMetadataProvider"/> - reuses <see cref="ICurrentUserService"/>
/// for actor/IP/correlation, and reads request path/trace id directly off the current HTTP context.
/// Registered per service via <c>AddHttpAuditMetadataProvider()</c>; services with no HTTP context
/// (background workers) simply don't register this and keep the default no-op provider instead.
/// </summary>
public sealed class HttpAuditMetadataProvider(
    ICurrentUserService currentUser,
    IHttpContextAccessor httpContextAccessor,
    string serviceName) : IAuditMetadataProvider
{
    public AuditMetadata Capture()
    {
        var httpContext = httpContextAccessor.HttpContext;

        return new AuditMetadata
        {
            Actor = currentUser.IsAuthenticated() ? currentUser.GetUserId()?.ToString() ?? currentUser.GetUserName() : null,
            Service = serviceName,
            ClientIp = currentUser.GetIpAddress(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            RequestPath = httpContext?.Request.Path.ToString(),
            TraceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier,
        };
    }
}
