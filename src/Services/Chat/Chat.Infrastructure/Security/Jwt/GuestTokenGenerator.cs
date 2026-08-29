using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using NovaCore.Chat.Application.Common;
using NovaCore.Chat.Infrastructure.Configurations.Settings;

namespace NovaCore.Chat.Infrastructure.Security.Jwt;

/// <summary>
/// Mints a short-lived access token for a guest Contact, signing with the same "Jwt" config
/// section every service's AddJwtBearerAuthentication already validates against - a guest
/// connecting with this token is authenticated by the exact same pipeline a real user's Auth
/// token goes through, no parallel auth mechanism needed. See IGuestTokenGenerator's remarks for
/// why this can't reuse Auth's own IJwtTokenGenerator (a guest has no Auth account).
/// </summary>
public sealed class GuestTokenGenerator(ChatJwtSetting settings) : IGuestTokenGenerator
{
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(settings.SecretKey));

    /// <summary>Deliberately short-lived (2 hours) - a guest conversation is a Session-lifecycle conversation (spec section 11), not a durable account.</summary>
    public string GenerateAccessToken(Guid contactId, string displayName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, contactId.ToString()),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Role, ChatRoleConstants.Guest),
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
