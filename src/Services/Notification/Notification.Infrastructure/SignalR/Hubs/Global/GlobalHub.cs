using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using MediatR;

using Microsoft.AspNetCore.Authorization;

using NovaCore.Notification.Application.Features.UserNotifications.Commands.MarkUserNotificationAsRead;
using NovaCore.Notification.Application.Features.UserNotifications.DTOs;
using NovaCore.Notification.Infrastructure.SignalR.Groups;

namespace NovaCore.Notification.Infrastructure.SignalR.Hubs.Global;

public interface IGlobalHubBase : IAppHub
{
    Task ReceiveNotification(NotificationDto message);

    /// <summary>Backend foundation for the future Hub version-check flow (see docs/services/
    /// auth-service.md, "Notification Hub Version Check") - informs a connection that its
    /// tenant's bootstrap Version changed, so it knows a refetch is needed. Client-side refetch
    /// orchestration is deliberately not implemented yet (see "Important Scope Boundary").</summary>
    Task BootstrapVersionChanged(int version);
}

public interface IGlobalHubClient
    : IAdminSiteActions, IClientSiteActions, IGlobalHubBase
{
}

[Authorize]
public partial class GlobalHub(
    ISender sender,
    IAppLogger<GlobalHub> logger) : HubBase<IGlobalHubClient>
{
    public const string Path = "/hubs/global";

    public override async Task OnConnectedAsync()
    {
        logger.Information($"User connected ({nameof(GlobalHub)}): {this.UserId}");

        var roleList = this.Roles;
        if (roleList.Contains(AppRoleConstant.Root))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Root(this.UserId));
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Broadcast(AppRoleConstant.Root));
        }

        if (roleList.Contains(AppRoleConstant.Admin))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Admin(this.UserId));
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Broadcast(AppRoleConstant.Admin));
        }

        var isUser = roleList.Contains(AppRoleConstant.User);
        if (isUser)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Member(this.UserId));
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Broadcast(AppRoleConstant.User));
        }

        // Backend foundation for tenant-wide notification (BootstrapVersionChanged, ...) - see
        // ActorGroups.Tenant and docs/services/auth-service.md, "Tenant Group Notification".
        if (this.TenantId is { } tenantId)
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Tenant(tenantId));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.Information($"User disconnected: {Context.UserIdentifier}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        var command = new MarkUserNotificationAsReadCommand(notificationId);
        await sender.Send(command);
    }
}
