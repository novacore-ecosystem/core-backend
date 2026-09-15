using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.User;
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
    IEffectivePermissionReadService effectivePermRead,
    ICurrentUserService currentUserService,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    RootSetting rootSetting) : ICommandHandler<RefreshTokenCommand>
{
    public async Task Handle(RefreshTokenCommand request, CancellationToken ct = default)
    {
        var user = await ValidateRefreshTokenAsync(ct);
        var app = await ResolveAppAsync(request.AppCode, user, ct);

        var (accessToken, refreshToken, jwtId) = await IssueTokensAsync(user, app, ct);

        // Set token to client cookie (HTTP Only)
        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        await PublishAuthenticationEventAsync(user.Id, user.TenantId, app, jwtId, ct);
    }

    #region Validation
    /// <summary>
    /// Validates the refresh token cookie and loads the account it was issued to.
    /// </summary>
    /// <returns>The account the refresh token belongs to.</returns>
    private async Task<Account> ValidateRefreshTokenAsync(CancellationToken ct = default)
    {
        var refreshToken = currentUserService.GetRefreshToken();
        if (refreshToken.IsNullOrWhiteSpace())
            throw new UnauthorizedException("Refresh token not found in cookies");

        var (userId, isValid) = await refreshTokenService.ValidateAndGetUserIdAsync(
            refreshToken,
            ct);
        if (!isValid)
            throw new UnauthorizedException(MessageCode.InvalidToken);

        return await authService.GetUserByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);
    }

    /// <summary>
    /// Resolves and validates the requested App, unless the account is the configured Root
    /// account, which bypasses App resolution/membership entirely.
    /// </summary>
    /// <param name="appCode">The App code supplied via header.</param>
    /// <param name="user">The account the refresh token belongs to.</param>
    /// <returns>The resolved App, or <see langword="null"/> for the Root account.</returns>
    private async Task<CachedApp?> ResolveAppAsync(
        string appCode,
        Account user,
        CancellationToken ct = default)
    {
        if (user.Id == rootSetting.Id)
            return null;

        if (appCode.IsNullOrWhiteSpace())
            throw new BadRequestException("This header is missing app code.");

        var app = await appCollectionCache.GetByCodeAsync(appCode, ct);
        if (app is null || !app.IsActive)
            throw new UnauthorizedException("Invalid App");

        var isAssignedToApp = await appMembershipCache.IsAssignedAsync(
            app.Id,
            user.Id,
            ct);
        if (!isAssignedToApp)
            throw new UnauthorizedException("Invalid App");

        return app;
    }
    #endregion

    #region Tokens
    /// <summary>
    /// Generates a new access token and refresh token, rotating the account's session identity.
    /// </summary>
    /// <param name="user">The account the refresh token belongs to.</param>
    /// <param name="app">The resolved App, or <see langword="null"/> for the Root account.</param>
    /// <returns>The issued access token, refresh token, and the access token's JWT id.</returns>
    private async Task<(string AccessToken, string RefreshToken, Guid JwtId)> IssueTokensAsync(
        Account user,
        CachedApp? app,
        CancellationToken ct = default)
    {
        var jwtId = Guid.NewGuid();
        var roles = await accountReadService.GetRoleNamesAsync(user.Id, ct);
        var permissions = await effectivePermRead.GetEffectivePermissionsAsync(
            user.Id,
            user.TenantId,
            ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            userId: user.Id,
            email: user.Email!,
            username: user.UserName!,
            roles: roles,
            permissions: permissions,
            tenantId: user.TenantId,
            appId: app?.Id ?? Guid.Empty,
            jwtId: jwtId);
        var newRefreshToken = await refreshTokenService.GenerateRefreshTokenAsync(
            user.Id,
            jwtId,
            ct);

        return (accessToken, newRefreshToken, jwtId);
    }
    #endregion

    #region Events
    /// <summary>
    /// Enqueues the authentication event so Session/Device/LoginHistory bookkeeping happens
    /// asynchronously, off the request path.
    /// </summary>
    private async Task PublishAuthenticationEventAsync(
        Guid userId,
        Guid tenantId,
        CachedApp? app,
        Guid jwtId,
        CancellationToken ct = default)
    {
        var authenticationEvent = new AuthenticationSucceededIntegrationEvent(
            userId.ToString(),
            app?.Id.ToString(),
            tenantId.ToString(),
            AuthenticationType.RefreshToken,
            jwtId.ToString(),
            currentUserService.GetIpAddress(),
            currentUserService.GetCorrelationId());

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await outboxStore.EnqueueAsync(authenticationEvent, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, ct: ct);
    }
    #endregion
}
