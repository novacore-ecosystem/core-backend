namespace NovaCore.Chat.Application.Common;

/// <summary>
/// Role claim value stamped on a guest's short-lived access token (see GuestTokenGenerator) -
/// local to Chat, not added to the platform-wide AppRoleConstant since no other service has a
/// concept of a guest identity. Lives in Application (not Infrastructure) because both
/// ChatHub.HubBase (Infrastructure) and Application command/query handlers need to distinguish a
/// guest caller from an internal user, and Application cannot depend on Infrastructure.
/// </summary>
public static class ChatRoleConstants
{
    public const string Guest = "Guest";
}
