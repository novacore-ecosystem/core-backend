using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Contract.Events.User;
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
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    RootSetting rootSetting) : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct = default)
    {
        var (user, tenantId) = await ValidateCredentialsAsync(request, ct);
        var app = await ResolveAppAsync(request.AppCode, user, ct);

        var (accessToken, refreshToken, jwtId) = await IssueTokensAsync(user, tenantId, app, ct);

        // Set token to client cookie (HTTP Only)
        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        await PublishAuthenticationEventAsync(user.Id, tenantId, app, jwtId, ct);

        return new LoginResult(accessToken, refreshToken);
    }

    #region Validation
    /// <summary>
    /// Validates the tenant client, the account, and the account's password, in that order.
    /// </summary>
    /// <param name="request">The login command.</param>
    /// <returns>The authenticated account and the tenant it was resolved against.</returns>
    private async Task<(Account User, Guid TenantId)> ValidateCredentialsAsync(LoginCommand request, CancellationToken ct = default)
    {
        var tenantClient = await tenantClientReadService.GetByPublicKeyAsync(request.ClientPublicKey, ct);
        if (tenantClient is null || !tenantClient.IsUsable())
            throw new UnauthorizedException("Invalid credentials");

        var tenantId = tenantClient.TenantId ?? Guid.Empty;
        var user = await accountReadService.GetByEmailAsync(request.Email, tenantId, ct)
            ?? throw new UnauthorizedException("Invalid credentials");

        var isValid = await authService.ValidateCredentialsAsync(user, request.Password, ct);
        if (!isValid)
            throw new UnauthorizedException("Invalid credentials");

        if (!user.EmailConfirmed)
            throw new BadRequestException(MessageCode.EmailNotVerified, "Email is not confirmed.");

        return (user, tenantId);
    }

    /// <summary>
    /// Resolves and validates the requested App, unless the account is the configured Root
    /// account, which bypasses App resolution/membership entirely.
    /// </summary>
    /// <param name="appCode">The App code supplied via header.</param>
    /// <param name="user">The already-authenticated account.</param>
    /// <returns>The resolved App, or <see langword="null"/> for the Root account.</returns>
    private async Task<CachedApp?> ResolveAppAsync(string appCode, Account user, CancellationToken ct = default)
    {
        if (user.Id == rootSetting.Id)
            return null;

        if (appCode.IsNullOrWhiteSpace())
            throw new BadRequestException("This header is missing app code.");

        var app = await appCollectionCache.GetByCodeAsync(appCode, ct);
        if (app is null || !app.IsActive)
            throw new UnauthorizedException("Invalid credentials");

        var isAssignedToApp = await appMembershipCache.IsAssignedAsync(app.Id, user.Id, ct);
        if (!isAssignedToApp)
            throw new UnauthorizedException("Invalid credentials");

        return app;
    }
    #endregion

    #region Tokens
    /// <summary>
    /// Generates the access token and refresh token for an authenticated account.
    /// </summary>
    /// <param name="user">The authenticated account.</param>
    /// <param name="tenantId">The resolved tenant.</param>
    /// <param name="app">The resolved App, or <see langword="null"/> for the Root account.</param>
    /// <returns>The issued access token, refresh token, and the access token's JWT id.</returns>
    private async Task<(string AccessToken, string RefreshToken, Guid JwtId)> IssueTokensAsync(
        Account user,
        Guid tenantId,
        CachedApp? app,
        CancellationToken ct = default)
    {
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
            appId: app?.Id ?? Guid.Empty,
            jwtId: jwtId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(
            user.Id,
            jwtId,
            ct);

        return (accessToken, refreshToken, jwtId);
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
            AuthenticationType.Login,
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
