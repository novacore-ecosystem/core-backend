using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenHandler(
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IAuthService authService,
    IAppReadService appReadService,
    IAccountReadService accountReadService,
    IEffectivePermissionReadService effectivePermissionReadService,
    ICurrentUserService currentUserService) : ICommandHandler<RefreshTokenCommand>
{
    public async Task Handle(RefreshTokenCommand request, CancellationToken ct = default)
    {
        var app = await appReadService.GetByCodeAsync(AppCode.Create(request.AppCode), ct);
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
            appId: app.Id,
            jwtId: jwtId);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ct);

        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(newRefreshToken);
    }
}
