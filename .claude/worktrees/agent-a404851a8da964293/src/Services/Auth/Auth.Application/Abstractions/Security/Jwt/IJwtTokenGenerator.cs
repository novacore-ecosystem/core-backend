using System.Security.Claims;

namespace NovaCore.Auth.Application.Abstractions.Security.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, string username, IEnumerable<string> roles, IEnumerable<string> permissions, Guid? jwtId = null);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);
}
