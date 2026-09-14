using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Commands.RefreshToken;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Apps;
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
        IJwtTokenGenerator TokenGenerator,
        App App,
        Account Account) BuildScenario(bool appIsActive = true, bool refreshTokenValid = true)
    {
        var appCode = AppCode.Create("storefront_web");
        var app = App.Create(appCode, "Storefront Web");
        if (!appIsActive)
            app.Deactivate();
        var account = Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var appReadService = Substitute.For<IAppReadService>();
        appReadService.GetByCodeAsync(appCode, Arg.Any<CancellationToken>()).Returns(app);

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
            appReadService,
            accountReadService,
            Substitute.For<IEffectivePermissionReadService>(),
            currentUserService);

        return (handler, tokenGenerator, app, account);
    }

    [Fact]
    public async Task Handle_ValidAppAndRefreshToken_IssuesNewTokenWithAppClaim()
    {
        var (handler, tokenGenerator, app, account) = BuildScenario();

        await handler.Handle(new RefreshTokenCommand("storefront_web"));

        tokenGenerator.Received(1).GenerateAccessToken(
            account.Id, account.Email!, account.UserName!,
            Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(),
            Guid.Empty, app.Id, Arg.Any<Guid?>());
    }

    [Fact]
    public async Task Handle_InactiveApp_ThrowsUnauthorized()
    {
        var (handler, _, _, _) = BuildScenario(appIsActive: false);

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new RefreshTokenCommand("storefront_web")));
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ThrowsUnauthorized()
    {
        var (handler, _, _, _) = BuildScenario(refreshTokenValid: false);

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new RefreshTokenCommand("storefront_web")));
    }
}
