using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;

using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenHandler(
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IAuthService authService,
    IAppCollectionCache appCollectionCache,
    IAppMembershipCache appMembershipCache,
    IAccountReadService accountReadService,
    IEffectivePermissionReadService effectivePermissionReadService,
    ICurrentUserService currentUserService,
    RootSetting rootSetting) : ICommandHandler<RefreshTokenCommand>
{
    public async Task Handle(RefreshTokenCommand request, CancellationToken ct = default)
    {
        // Check if refresh token cookie is present
        var refreshToken = currentUserService.GetRefreshToken();
        if (refreshToken.IsNullOrWhiteSpace())
            throw new UnauthorizedException("Refresh token not found in cookies");

        // Check if refresh token is valid
        var (userId, isValid) = await refreshTokenService.ValidateAndGetUserIdAsync(refreshToken, ct);
        if (!isValid)
            throw new UnauthorizedException(MessageCode.InvalidToken);

        var user = await authService.GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

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
                throw new UnauthorizedException("Invalid App");

            var isAssignedToApp = await appMembershipCache.IsAssignedAsync(app.Id, user.Id, ct);
            if (!isAssignedToApp)
                throw new UnauthorizedException("Invalid App");
        }

        // Generate access token and refresh token
        var jwtId = Guid.NewGuid();
        var roles = await accountReadService.GetRoleNamesAsync(user.Id, ct);
        var permissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(user.Id, user.TenantId, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            userId: user.Id,
            email: user.Email!,
            username: user.UserName!,
            roles: roles,
            permissions: permissions,
            tenantId: user.TenantId,
            appId: app is null || isRoot ? Guid.Empty : app.Id,
            jwtId: jwtId);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        // Set token to client cookie (HTTP Only)
        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(newRefreshToken);
    }
}
