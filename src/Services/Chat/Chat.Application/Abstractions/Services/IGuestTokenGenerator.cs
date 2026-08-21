namespace NovaCore.Chat.Application.Abstractions.Services;

/// <summary>
/// Mints a short-lived access token for a guest Contact (spec section 39's guest flow: a guest has
/// no User/Auth account, so this is not IJwtTokenGenerator/Auth's login flow) - the resulting token
/// carries just enough identity (ContactId as the standard NameIdentifier claim, a Guest role
/// claim) for the guest to be recognized by the same JWT Bearer pipeline and ChatHub every
/// authenticated caller already goes through. Implemented in Chat.Infrastructure.
/// </summary>
public interface IGuestTokenGenerator
{
    string GenerateAccessToken(Guid contactId, string displayName);
}
