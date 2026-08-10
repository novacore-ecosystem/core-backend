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
