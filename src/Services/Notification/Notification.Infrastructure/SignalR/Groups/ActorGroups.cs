namespace NovaCore.Notification.Infrastructure.SignalR.Groups;

internal static class ActorGroups
{
    public static string Root(Guid userId) => $"root:{userId}";
    public static string Admin(Guid userId) => $"admin:{userId}";
    public static string Member(Guid userId) => $"member:{userId}";

    public static string Broadcast(string roleName) => $"role:{roleName}";

    /// <summary>Every connection whose access token carries this TenantId - backend foundation
    /// for tenant-wide notification (e.g. BootstrapVersionChanged), see GlobalHub.OnConnectedAsync
    /// and docs/services/auth-service.md, "Tenant Group Notification".</summary>
    public static string Tenant(Guid tenantId) => $"tenant:{tenantId}";
}
