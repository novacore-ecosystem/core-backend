using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Application.Features.Auth.Commands.RefreshToken;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class RefreshTokenHandlerTests
{
    private static (
        RefreshTokenHandler Handler,
        IAppMembershipCache AppMembershipCache,
        IJwtTokenGenerator TokenGenerator,
        CachedApp App,
        Account Account) BuildScenario(
            bool appIsActive = true, bool refreshTokenValid = true, bool isRootAccount = false)
    {
        var rootSetting = new RootSetting();
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", appIsActive);
        var account = isRootAccount
            ? Account.Create(rootSetting.Id, "test@example.com", Email.Create("test@example.com"), AccountStatus.Active)
            : Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);

        var appMembershipCache = Substitute.For<IAppMembershipCache>();
        appMembershipCache.IsAssignedAsync(app.Id, account.Id, Arg.Any<CancellationToken>()).Returns(true);

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetRefreshToken().Returns("existing-refresh-token");

        var refreshTokenService = Substitute.For<IRefreshTokenService>();
        refreshTokenService.ValidateAndGetUserIdAsync("existing-refresh-token", Arg.Any<CancellationToken>())
            .Returns((account.Id, refreshTokenValid));
        refreshTokenService.GenerateRefreshTokenAsync(account.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("new-refresh-token");

        var authService = Substitute.For<IAuthService>();
        authService.GetUserByIdAsync(account.Id, Arg.Any<CancellationToken>()).Returns(account);

        var accountReadService = Substitute.For<IAccountReadService>();
        accountReadService.GetRoleNamesAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<string>)["User"]);

        var tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        tokenGenerator.GenerateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>())
            .Returns("new-access-token");

        var handler = new RefreshTokenHandler(
            tokenGenerator,
            refreshTokenService,
            authService,
            appCollectionCache,
            appMembershipCache,
            accountReadService,
            Substitute.For<IEffectivePermissionReadService>(),
            currentUserService,
            rootSetting);

        return (handler, appMembershipCache, tokenGenerator, app, account);
    }

    [Fact]
    public async Task Handle_ValidAppAndRefreshToken_IssuesNewTokenWithAppClaim()
    {
        var (handler, _, tokenGenerator, app, account) = BuildScenario();

        await handler.Handle(new RefreshTokenCommand("storefront_web"));

        tokenGenerator.Received(1).GenerateAccessToken(
            account.Id, account.Email!, account.UserName!,
            Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(),
            Guid.Empty, app.Id, Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Handle_RootAccount_BypassesAppCheckEntirely_EvenWithNoAppCodeSupplied()
    {
        var (handler, appMembershipCache, tokenGenerator, _, account) = BuildScenario(isRootAccount: true);

        // Root never sends X-App-Key (see nova-console's env.appCode) - the bypass must not
        // depend on one being supplied.
        await handler.Handle(new RefreshTokenCommand(""));

        // Root holds no App membership by design - resolution/membership must never even be
        // attempted for it, and its token gets no app_id claim (Guid.Empty).
        await appMembershipCache.DidNotReceive().IsAssignedAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        tokenGenerator.Received(1).GenerateAccessToken(
            account.Id, account.Email!, account.UserName!,
            Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(),
            Guid.Empty, Guid.Empty, Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Handle_NonRootWithoutAppCode_ThrowsBadRequest()
    {
        var (handler, _, _, _, _) = BuildScenario();

        await Should.ThrowAsync<BadRequestException>(
            () => handler.Handle(new RefreshTokenCommand("")));
    }

    [Fact]
    public async Task Handle_InactiveApp_ThrowsUnauthorized()
    {
        var (handler, _, _, _, _) = BuildScenario(appIsActive: false);

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new RefreshTokenCommand("storefront_web")));
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ThrowsUnauthorized()
    {
        var (handler, _, _, _, _) = BuildScenario(refreshTokenValid: false);

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new RefreshTokenCommand("storefront_web")));
    }
}
