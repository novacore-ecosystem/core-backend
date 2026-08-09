using System.Security.Claims;

namespace NovaCore.Auth.Application.Abstractions.Security.Jwt;

public interface IJwtTokenGenerator
{
    /// <summary>tenantId is Guid.Empty for the Root/global identity - no tenant_id claim is emitted
    /// in that case, matching every other entity's Guid.Empty-means-"no tenant" convention (see
    /// docs/reference/tenant-convention.md).</summary>
    string GenerateAccessToken(Guid userId, string email, string username, IEnumerable<string> roles, IEnumerable<string> permissions, Guid tenantId, Guid? jwtId = null);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);
}
