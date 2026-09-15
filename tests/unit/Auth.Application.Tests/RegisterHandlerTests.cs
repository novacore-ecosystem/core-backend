using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Commands.Register;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

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
        IRegistrationDefaultsCache? registrationDefaultsCache = null,
        IAccountRoleAssignmentService? accountRoleAssignmentService = null,
        IPermissionGrantService? permissionGrantService = null,
        IAccountAppAssignmentService? accountAppAssignmentService = null,
        IAuthEmailRequestService? authEmailRequestService = null,
        IAppMembershipCache? appMembershipCache = null,
        IOutboxStore? outboxStore = null)
    {
        IRegistrationDefaultsCache defaultsCache;
        if (registrationDefaultsCache is null)
        {
            defaultsCache = Substitute.For<IRegistrationDefaultsCache>();
            defaultsCache.GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(RegistrationDefaultsSnapshot.Empty);
        }
        else
        {
            defaultsCache = registrationDefaultsCache;
        }

        return new RegisterHandler(
            unitOfWork,
            authService,
            appCollectionCache,
            appMembershipCache ?? Substitute.For<IAppMembershipCache>(),
            defaultsCache,
            accountRoleAssignmentService ?? Substitute.For<IAccountRoleAssignmentService>(),
            permissionGrantService ?? Substitute.For<IPermissionGrantService>(),
            accountAppAssignmentService ?? Substitute.For<IAccountAppAssignmentService>(),
            authEmailRequestService ?? Substitute.For<IAuthEmailRequestService>(),
            outboxStore ?? Substitute.For<IOutboxStore>(),
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IInternalEventDispatcher>(),
            Substitute.For<IAppLogger<RegisterHandler>>());
    }

    [Fact]
    public async Task Handle_ValidApp_AssignsUserToAppAndDefaultRole()
    {
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", true);
        var roleId = Guid.NewGuid();
        var account = Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var authService = Substitute.For<IAuthService>();
        authService.GetUserByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((Account?)null);
        authService.CreateUserAsync("test@example.com", "test@example.com", "P@ssw0rd", Arg.Any<CancellationToken>())
            .Returns(account);

        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);

        var registrationDefaultsCache = Substitute.For<IRegistrationDefaultsCache>();
        registrationDefaultsCache.GetAsync(Arg.Any<Guid>(), app.Id, Arg.Any<CancellationToken>())
            .Returns(new RegistrationDefaultsSnapshot([roleId], []));

        var accountRoleAssignmentService = Substitute.For<IAccountRoleAssignmentService>();
        var accountAppAssignmentService = Substitute.For<IAccountAppAssignmentService>();
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        var outboxStore = Substitute.For<IOutboxStore>();

        var handler = BuildHandler(
            BuildUnitOfWork(),
            authService,
            appCollectionCache,
            registrationDefaultsCache,
            accountRoleAssignmentService,
            accountAppAssignmentService: accountAppAssignmentService,
            authEmailRequestService: authEmailRequestService,
            outboxStore: outboxStore);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "storefront_web");

        await handler.Handle(command);

        // Register must claim the resend cooldown and dispatch the verification email itself,
        // synchronously, instead of issuing tokens - the account stays unconfirmed until the
        // link is clicked, and an immediate resend must already see the cooldown established.
        await authEmailRequestService.Received(1).TryDispatchEmailVerificationAsync(account.Email!, Arg.Any<CancellationToken>());

        // Also publishes UserRegisteredIntegrationEvent (in the same transaction as account
        // creation) so the dispatch is retried/observed by a consumer independent of this
        // request's own lifetime.
        await outboxStore.Received(1).EnqueueAsync(
            Arg.Is<UserRegisteredIntegrationEvent>(e => e.AccountId == account.Id.ToString() && e.Email == account.Email),
            Arg.Any<CancellationToken>());

        await accountRoleAssignmentService.Received(1).ReplaceRolesAsync(
            account.Id,
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(roleId)),
            Arg.Any<CancellationToken>());
        await accountAppAssignmentService.Received(1).AssignAsync(account.Id, app.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoDefaultsConfigured_RegistersSuccessfullyWithoutRolesOrPermissions()
    {
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", true);
        var account = Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var authService = Substitute.For<IAuthService>();
        authService.GetUserByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((Account?)null);
        authService.CreateUserAsync("test@example.com", "test@example.com", "P@ssw0rd", Arg.Any<CancellationToken>())
            .Returns(account);

        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);

        // No custom registrationDefaultsCache - BuildHandler's default substitute already
        // returns RegistrationDefaultsSnapshot.Empty for every (tenantId, appId).
        var accountRoleAssignmentService = Substitute.For<IAccountRoleAssignmentService>();
        var permissionGrantService = Substitute.For<IPermissionGrantService>();

        var handler = BuildHandler(
            BuildUnitOfWork(),
            authService,
            appCollectionCache,
            accountRoleAssignmentService: accountRoleAssignmentService,
            permissionGrantService: permissionGrantService);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "storefront_web");

        await handler.Handle(command);

        await accountRoleAssignmentService.DidNotReceive().ReplaceRolesAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await permissionGrantService.DidNotReceive().ReplaceForProviderAsync(
            Arg.Any<PermissionProviderName>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DefaultPermissionsConfigured_GrantsThemToTheNewAccount()
    {
        var app = new CachedApp(Guid.NewGuid(), "storefront_web", "Storefront Web", true);
        var account = Account.Create("test@example.com", Email.Create("test@example.com"), AccountStatus.Active);

        var authService = Substitute.For<IAuthService>();
        authService.GetUserByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((Account?)null);
        authService.CreateUserAsync("test@example.com", "test@example.com", "P@ssw0rd", Arg.Any<CancellationToken>())
            .Returns(account);

        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(app.Code, Arg.Any<CancellationToken>()).Returns(app);

        var registrationDefaultsCache = Substitute.For<IRegistrationDefaultsCache>();
        registrationDefaultsCache.GetAsync(Arg.Any<Guid>(), app.Id, Arg.Any<CancellationToken>())
            .Returns(new RegistrationDefaultsSnapshot([], ["some:permission"]));

        var permissionGrantService = Substitute.For<IPermissionGrantService>();

        var handler = BuildHandler(
            BuildUnitOfWork(),
            authService,
            appCollectionCache,
            registrationDefaultsCache,
            permissionGrantService: permissionGrantService);

        var command = new RegisterCommand(
            "test@example.com",
            "P@ssw0rd",
            "Test",
            "User",
            "0123456789",
            "storefront_web");

        await handler.Handle(command);

        await permissionGrantService.Received(1).ReplaceForProviderAsync(
            PermissionProviderName.User,
            account.Id.ToString(),
            Arg.Is<IReadOnlyCollection<string>>(k => k.Contains("some:permission")),
            account.TenantId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownApp_ThrowsNotFound()
    {
        var authService = Substitute.For<IAuthService>();
        var appCollectionCache = Substitute.For<IAppCollectionCache>();
        appCollectionCache.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((CachedApp?)null);

        var handler = BuildHandler(BuildUnitOfWork(), authService, appCollectionCache);

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

        var handler = BuildHandler(BuildUnitOfWork(), authService, appCollectionCache);

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
