using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace NovaCore.Notification.Infrastructure.SignalR.Hubs;

public abstract class HubBase<T>()
    : Hub<T> where T : class, IAppHub
{
    protected HttpContext? HttpContext => Context.GetHttpContext();
    protected string ConnectionId => Context.ConnectionId;
    protected Guid UserId => Guid.TryParse(Context.UserIdentifier, out var id) ? id : default;
    protected string[] Roles => Context.User?
        .FindAll(ClaimTypes.Role)
        .Select(r => r.Value)
        .ToArray() ?? [];

    /// <summary>Same claim Auth's JwtTokenGenerator embeds (AppClaimTypes.TenantId) - null for
    /// the Root client, which carries no tenant. Backend foundation for tenant-group notification,
    /// see ActorGroups.Tenant.</summary>
    protected Guid? TenantId =>
        Guid.TryParse(Context.User?.FindFirst(AppClaimTypes.TenantId)?.Value, out var id) ? id : null;

    protected async Task AddGroupAsync(string groupName)
    {
        await Groups.AddToGroupAsync(this.ConnectionId, groupName);
    }

    #region Public Hub methods
    public async Task KickMe()
    {
        Context.Abort();
        await Task.CompletedTask;
    }
    #endregion
}
