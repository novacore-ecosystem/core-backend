using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Commands.Register;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Apps;
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
        IAppReadService appReadService,
        IRoleReadService roleReadService,
        IAccountRoleAssignmentService? accountRoleAssignmentService = null,
        IAccountAppAssignmentService? accountAppAssignmentService = null,
        IAccountReadService? accountReadService = null)
    {
        return new RegisterHandler(
            unitOfWork,
            authService,
            appReadService,
            roleReadService,
            accountRoleAssignmentService ?? Substitute.For<IAccountRoleAssignmentService>(),
            accountAppAssignmentService ?? Substitute.For<IAccountAppAssignmentService>(),
            accountReadService ?? Substitute.For<IAccountReadService>(),
            Substitute.For<IEffectivePermissionReadService>(),
            Substitute.For<IJwtTokenGenerator>(),
            Substitute.For<IRefreshTokenService>(),
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IInternalEventDispatcher>(),
            Substitute.For<IAppLogger<RegisterHandler>>());
    }

    [Fact]
    public async Task Handle_ValidApp_AssignsUserToAppAndDefaultRole()
    {
        var appCode = AppCode.Create("storefront_web");
        var app = App.Create(appCode, "Storefront Web");
        var role = Role.Create("User", RoleCode.Create("User"));
        var account = Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var authService = Substitute.For<IAuthService>();
        authService.GetUserByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((Account?)null);
        authService.CreateUserAsync("test@example.com", "test@example.com", "P@ssw0rd", Arg.Any<CancellationToken>())
            .Returns(account);

        var appReadService = Substitute.For<IAppReadService>();
        appReadService.GetByCodeAsync(appCode, Arg.Any<CancellationToken>()).Returns(app);

        var roleReadService = Substitute.For<IRoleReadService>();
        roleReadService.GetByCodeAsync(Arg.Any<RoleCode>(), Arg.Any<CancellationToken>()).Returns(role);

        var accountRoleAssignmentService = Substitute.For<IAccountRoleAssignmentService>();
        var accountAppAssignmentService = Substitute.For<IAccountAppAssignmentService>();

        var handler = BuildHandler(
            BuildUnitOfWork(),
            authService,
            appReadService,
            roleReadService,
            accountRoleAssignmentService,
            accountAppAssignmentService);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "storefront_web");

        await handler.Handle(command);

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
        var appReadService = Substitute.For<IAppReadService>();
        appReadService.GetByCodeAsync(Arg.Any<AppCode>(), Arg.Any<CancellationToken>()).Returns((App?)null);
        var roleReadService = Substitute.For<IRoleReadService>();

        var handler = BuildHandler(BuildUnitOfWork(), authService, appReadService, roleReadService);

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
        var appCode = AppCode.Create("storefront_web");
        var app = App.Create(appCode, "Storefront Web");
        app.Deactivate();

        var authService = Substitute.For<IAuthService>();
        var appReadService = Substitute.For<IAppReadService>();
        appReadService.GetByCodeAsync(appCode, Arg.Any<CancellationToken>()).Returns(app);
        var roleReadService = Substitute.For<IRoleReadService>();

        var handler = BuildHandler(BuildUnitOfWork(), authService, appReadService, roleReadService);

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
