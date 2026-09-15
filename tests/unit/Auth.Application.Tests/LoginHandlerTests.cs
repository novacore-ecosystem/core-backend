using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Application.Features.Auth.Commands.Login;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class LoginHandlerTests
{
    private static (
        LoginHandler Handler,
        IAuthService AuthService,
        IAppMembershipCache AppMembershipCache,
        IJwtTokenGenerator TokenGenerator,
        CachedApp App,
        Account Account) BuildScenario(bool isAssignedToApp, bool credentialsValid = true, bool isRootAccount = false, bool emailConfirmed = true)
    {
        var rootSetting = new RootSetting();
        var tenantClient = TenantClient.Create(null, "Root Client");
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", true);
        var account = isRootAccount
            ? Account.Create(rootSetting.Id, "test@example.com", Email.Create("test@example.com"), AccountStatus.Active)
            : Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);
        if (emailConfirmed)
            account.ConfirmEmail();

        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.GetByPublicKeyAsync("client-key", Arg.Any<CancellationToken>()).Returns(tenantClient);

        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);

        var accountReadService = Substitute.For<IAccountReadService>();
        accountReadService.GetByEmailAsync("test@example.com", Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        accountReadService.GetRoleNamesAsync(account.Id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<string>)["User"]);

        var appMembershipCache = Substitute.For<IAppMembershipCache>();
        appMembershipCache.IsAssignedAsync(app.Id, account.Id, Arg.Any<CancellationToken>())
            .Returns(isAssignedToApp);

        var authService = Substitute.For<IAuthService>();
        authService.ValidateCredentialsAsync(account, "P@ssw0rd", Arg.Any<CancellationToken>())
            .Returns(credentialsValid);

        var tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        tokenGenerator.GenerateAccessToken(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<string>>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid?>())
            .Returns("access-token");

        var refreshTokenService = Substitute.For<IRefreshTokenService>();
        refreshTokenService.GenerateRefreshTokenAsync(account.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns("refresh-token");

        var handler = new LoginHandler(
            tenantClientReadService,
            appCollectionCache,
            appMembershipCache,
            accountReadService,
            authService,
            Substitute.For<IEffectivePermissionReadService>(),
            tokenGenerator,
            refreshTokenService,
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IOutboxStore>(),
            Substitute.For<IUnitOfWork>(),
            rootSetting);

        return (handler, authService, appMembershipCache, tokenGenerator, app, account);
    }

    [Fact]
    public async Task Handle_ValidCredentialsAndAppMembership_Succeeds()
    {
        var (handler, _, _, tokenGenerator, app, account) = BuildScenario(isAssignedToApp: true);

        var result = await handler.Handle(new LoginCommand("test@example.com", "P@ssw0rd", "client-key", "storefront_web"));

        result.AccessToken.ShouldBe("access-token");
        result.RefreshToken.ShouldBe("refresh-token");

        // The resolved App must travel as the token's appId claim - so authenticated requests
        // never need to re-supply an App identifier.
        tokenGenerator.Received(1).GenerateAccessToken(
            account.Id, account.Email!, account.UserName!,
            Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(),
            Guid.Empty, app.Id, Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Handle_RootAccount_BypassesAppCheckEntirely_EvenWithNoAppCodeSupplied()
    {
        var (handler, _, appMembershipCache, tokenGenerator, _, account) =
            BuildScenario(isAssignedToApp: false, isRootAccount: true);

        // Root never sends X-App-Key (see nova-console's env.appCode) - the bypass must not
        // depend on one being supplied.
        var result = await handler.Handle(new LoginCommand("test@example.com", "P@ssw0rd", "client-key", ""));

        result.AccessToken.ShouldBe("access-token");

        // Root holds no App membership by design - resolution/membership must never even be
        // attempted for it, and its token gets no app_id claim (Guid.Empty), same as
        // RefreshTokenHandler's bypass.
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
        var (handler, _, _, _, _, _) = BuildScenario(isAssignedToApp: true);

        await Should.ThrowAsync<BadRequestException>(
            () => handler.Handle(new LoginCommand("test@example.com", "P@ssw0rd", "client-key", "")));
    }

    [Fact]
    public async Task Handle_ValidCredentialsButNoAppMembership_ThrowsUnauthorized()
    {
        var (handler, _, _, _, _, _) = BuildScenario(isAssignedToApp: false);

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("test@example.com", "P@ssw0rd", "client-key", "storefront_web")));
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsUnauthorized_RegardlessOfAppMembership()
    {
        var (handler, _, _, _, _, _) = BuildScenario(isAssignedToApp: true, credentialsValid: false);

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("test@example.com", "P@ssw0rd", "client-key", "storefront_web")));
    }

    [Fact]
    public async Task Handle_UnconfirmedEmail_ThrowsBadRequest()
    {
        var (handler, _, _, _, _, _) = BuildScenario(isAssignedToApp: true, emailConfirmed: false);

        await Should.ThrowAsync<BadRequestException>(
            () => handler.Handle(new LoginCommand("test@example.com", "P@ssw0rd", "client-key", "storefront_web")));
    }
}
