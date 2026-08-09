using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    ITenantClientReadService tenantClientReadService,
    IAccountReadService accountReadService,
    IAuthService authService,
    IEffectivePermissionReadService effectivePermissionReadService,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    ICurrentUserService currentUserService) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct = default)
    {
        // A single generic message covers every pre-token failure (unknown/invalid/revoked
        // client key, unknown user, wrong password) - distinguishing them in the response would
        // let a caller enumerate valid tenants/clients/users (see docs/services/auth-service.md).
        var tenantClient = await tenantClientReadService.GetByPublicKeyAsync(request.ClientPublicKey, ct);
        if (tenantClient is null || !tenantClient.IsUsable())
            throw new UnauthorizedException("Invalid credentials");

        var tenantId = tenantClient.TenantId ?? Guid.Empty;

    var user = await accountReadService.GetByEmailAsync(request.Email, tenantId, ct)
        ?? throw new UnauthorizedException("Invalid credentials");
        
    var isValid = await authService.ValidateCredentialsAsync(user, request.Password, ct);
        if (!isValid)
            throw new UnauthorizedException("Invalid credentials");

        var jwtId = Guid.NewGuid();
        var roles = await authService.GetUserRolesAsync(user.Id, ct);
        var permissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(user.Id, tenantId, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            userId: user.Id,
            email: user.Email!,
            username: user.UserName!,
            roles: roles,
            permissions: permissions,
            tenantId: tenantId,
            jwtId: jwtId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        return new LoginResult(accessToken, refreshToken);
    }
}
