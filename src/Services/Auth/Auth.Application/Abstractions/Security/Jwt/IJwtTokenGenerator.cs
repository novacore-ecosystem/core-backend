using System.Security.Claims;

namespace NovaCore.Auth.Application.Abstractions.Security.Jwt;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// tenantId is Guid.Empty for the Root/global identity - no tenant_id claim is emitted
    /// in that case, matching every other entity's Guid.Empty-means-"no tenant" convention (see
    /// docs/reference/tenant-convention.md). appId is the App (see Auth.Domain's App entity) the
    /// caller already resolved and validated for this session - Guid.Empty omits the app_id claim
    /// the same way tenantId does, though in practice Login/Register/RefreshToken always resolve
    /// a real App before calling this.
    /// </summary>
    string GenerateAccessToken(
        Guid userId,
        string email,
        string username,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Guid tenantId,
        Guid appId,
        Guid? jwtId = null);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);
}
