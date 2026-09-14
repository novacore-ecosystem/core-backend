using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    ITenantClientReadService tenantClientReadService,
    IAppCollectionCache appCollectionCache,
    IAppMembershipCache appMembershipCache,
    IAccountReadService accountReadService,
    IAuthService authService,
    IEffectivePermissionReadService effectivePermissionReadService,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    ICurrentUserService currentUserService,
    RootSetting rootSetting) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct = default)
    {
        // Check if tenant client is valid
        var tenantClient = await tenantClientReadService.GetByPublicKeyAsync(request.ClientPublicKey, ct);
        if (tenantClient is null || !tenantClient.IsUsable())
            throw new UnauthorizedException("Invalid credentials");

        // Check if email exists
        var tenantId = tenantClient.TenantId ?? Guid.Empty;
        var user = await accountReadService.GetByEmailAsync(request.Email, tenantId, ct)
            ?? throw new UnauthorizedException("Invalid credentials");

        // Check if password is valid
        var isValid = await authService.ValidateCredentialsAsync(user, request.Password, ct);
        if (!isValid)
            throw new UnauthorizedException("Invalid credentials");

        // Root bypasses App resolution/membership entirely; every other account must supply a
        // valid, assigned App
        CachedApp? app = null;
        var isRoot = user.Id == rootSetting.Id;
        if (!isRoot)
        {
            if (request.AppCode.IsNullOrWhiteSpace())
                throw new BadRequestException("This header is missing app code.");

            app = await appCollectionCache.GetByCodeAsync(request.AppCode, ct);
            if (app is null || !app.IsActive)
                throw new UnauthorizedException("Invalid credentials");

            var isAssignedToApp = await appMembershipCache.IsAssignedAsync(app.Id, user.Id, ct);
            if (!isAssignedToApp)
                throw new UnauthorizedException("Invalid credentials");
        }

        // Generate access token and refresh token
        var jwtId = Guid.NewGuid();
        var roles = await accountReadService.GetRoleNamesAsync(user.Id, ct);
        var permissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(user.Id, tenantId, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            userId: user.Id,
            email: user.Email!,
            username: user.UserName!,
            roles: roles,
            permissions: permissions,
            tenantId: tenantId,
            appId: app is null || isRoot ? Guid.Empty : app.Id,
            jwtId: jwtId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        // Set token to client cookie (HTTP Only)
        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        return new LoginResult(accessToken, refreshToken);
    }
}
