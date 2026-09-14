using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Commands.Register;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class RegisterHandlerTests
{
    private static IUnitOfWork BuildUnitOfWork()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });
        return uow;
    }

    private static RegisterHandler BuildHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IAppCollectionCache appCollectionCache,
        IRoleReadService roleReadService,
        IAccountRoleAssignmentService? accountRoleAssignmentService = null,
        IAccountAppAssignmentService? accountAppAssignmentService = null,
        IAccountReadService? accountReadService = null,
        IJwtTokenGenerator? tokenGenerator = null,
        IAppMembershipCache? appMembershipCache = null)
    {
        return new RegisterHandler(
            unitOfWork,
            authService,
            appCollectionCache,
            appMembershipCache ?? Substitute.For<IAppMembershipCache>(),
            roleReadService,
            accountRoleAssignmentService ?? Substitute.For<IAccountRoleAssignmentService>(),
            accountAppAssignmentService ?? Substitute.For<IAccountAppAssignmentService>(),
            accountReadService ?? Substitute.For<IAccountReadService>(),
            Substitute.For<IEffectivePermissionReadService>(),
            tokenGenerator ?? Substitute.For<IJwtTokenGenerator>(),
            Substitute.For<IRefreshTokenService>(),
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IInternalEventDispatcher>(),
            Substitute.For<IAppLogger<RegisterHandler>>());
    }

    [Fact]
    public async Task Handle_ValidApp_AssignsUserToAppAndDefaultRole()
    {
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", true);
        var role = Role.Create("User", RoleCode.Create("User"));
        var account = Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var authService = Substitute.For<IAuthService>();
        authService.GetUserByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((Account?)null);
        authService.CreateUserAsync("test@example.com", "test@example.com", "P@ssw0rd", Arg.Any<CancellationToken>())
            .Returns(account);

        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);

        var roleReadService = Substitute.For<IRoleReadService>();
        roleReadService.GetByCodeAsync(Arg.Any<RoleCode>(), Arg.Any<CancellationToken>()).Returns(role);

        var accountRoleAssignmentService = Substitute.For<IAccountRoleAssignmentService>();
        var accountAppAssignmentService = Substitute.For<IAccountAppAssignmentService>();
        var tokenGenerator = Substitute.For<IJwtTokenGenerator>();

        var handler = BuildHandler(
            BuildUnitOfWork(),
            authService,
            appCollectionCache,
            roleReadService,
            accountRoleAssignmentService,
            accountAppAssignmentService,
            tokenGenerator: tokenGenerator);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "storefront_web");

        await handler.Handle(command);

        // The resolved App must travel as the token's appId claim - so authenticated requests
        // never need to re-supply an App identifier.
        tokenGenerator.Received(1).GenerateAccessToken(
            account.Id, account.Email!, account.UserName!,
            Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>(),
            Guid.Empty, app.Id, Arg.Any<Guid?>());

        await accountRoleAssignmentService.Received(1).ReplaceRolesAsync(
            account.Id,
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(role.Id)),
            Arg.Any<CancellationToken>());
        await accountAppAssignmentService.Received(1).AssignAsync(account.Id, app.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownApp_ThrowsNotFound()
    {
        var authService = Substitute.For<IAuthService>();
        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((CachedApp?)null);
        var roleReadService = Substitute.For<IRoleReadService>();

        var handler = BuildHandler(BuildUnitOfWork(), authService, appCollectionCache, roleReadService);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "unknown_app");

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command));

        await authService.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InactiveApp_ThrowsBadRequest()
    {
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", false);

        var authService = Substitute.For<IAuthService>();
        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);
        var roleReadService = Substitute.For<IRoleReadService>();

        var handler = BuildHandler(BuildUnitOfWork(), authService, appCollectionCache, roleReadService);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "storefront_web");

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command));

        await authService.DidNotReceive().CreateUserAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
