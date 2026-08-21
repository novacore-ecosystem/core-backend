using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

using NovaCore.Chat.Application.Common;

namespace NovaCore.Chat.Infrastructure.SignalR.Hubs;

/// <summary>Mirrors Notification.Infrastructure's own HubBase&lt;T&gt; - same shape, kept per-service rather than shared since each service's Hub set differs.</summary>
public abstract class HubBase<T>() : Hub<T> where T : class
{
    protected HttpContext? HttpContext => Context.GetHttpContext();
    protected string ConnectionId => Context.ConnectionId;
    protected Guid UserId => Guid.TryParse(Context.UserIdentifier, out var id) ? id : default;
    protected string[] Roles => Context.User?
        .FindAll(ClaimTypes.Role)
        .Select(r => r.Value)
        .ToArray() ?? [];

    protected bool IsGuest => Roles.Contains(ChatRoleConstants.Guest);

    protected async Task AddGroupAsync(string groupName)
    {
        await Groups.AddToGroupAsync(ConnectionId, groupName);
    }

    protected async Task RemoveGroupAsync(string groupName)
    {
        await Groups.RemoveFromGroupAsync(ConnectionId, groupName);
    }
}
