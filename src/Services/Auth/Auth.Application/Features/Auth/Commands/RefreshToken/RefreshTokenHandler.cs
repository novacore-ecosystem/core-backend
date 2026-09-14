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
        var app = await appCollectionCache.GetByCodeAsync(request.AppCode, ct);
        if (app is null || !app.IsActive)
            throw new UnauthorizedException("Invalid App");

        var refreshToken = currentUserService.GetRefreshToken();
        if (refreshToken.IsNullOrWhiteSpace())
            throw new UnauthorizedException("Refresh token not found in cookies");

        var (userId, isValid) = await refreshTokenService.ValidateAndGetUserIdAsync(refreshToken, ct);
        if (!isValid)
            throw new UnauthorizedException(MessageCode.InvalidToken);

        var user = await authService.GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        // Root intentionally holds no App membership (see RootSetting), so the membership check
        // below would always reject it - Root bypasses it entirely. Its token also gets no
        // app_id claim: Guid.Empty omits the claim the same way it does for tenantId (see
        // IJwtTokenGenerator), since fabricating the header's App as Root's membership would
        // misrepresent a relationship Root doesn't actually have.
        var isRoot = user.Id == rootSetting.Id;
        if (!isRoot)
        {
            var isAssignedToApp = await appMembershipCache.IsAssignedAsync(app.Id, user.Id, ct);
            if (!isAssignedToApp)
                throw new UnauthorizedException("Invalid App");
        }

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
            appId: isRoot ? Guid.Empty : app.Id,
            jwtId: jwtId);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(newRefreshToken);
    }
}
