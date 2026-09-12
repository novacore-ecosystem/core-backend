using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Roles.Commands.UpdateRolePermissions;
using NovaCore.Auth.Application.Features.Roles.DTOs;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.Metadata;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Domain.Metadata;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

/// <summary>
/// The actor is a Root snapshot in every test here unless a test's own name says otherwise - Root
/// bypasses every AccountAuthorizationGuard check this handler now runs (can-grant-what-you-hold,
/// target-does-not-grant-Root, within-tenant-boundary - see AccountAuthorizationGuardTests for
/// those rules in isolation), keeping these tests focused on the outbox/unit-of-work behavior they
/// were written for.
/// </summary>
public sealed class UpdateRolePermissionsHandlerTests
{
    private static ICurrentUserService CurrentUser(Guid actorId)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns(actorId);
        return currentUserService;
    }

    private static IEffectiveAuthorizationCache RootAuthorizationCache(Guid actorId, Guid tenantId)
    {
        var authorizationCache = Substitute.For<IEffectiveAuthorizationCache>();
        authorizationCache
            .GetAsync(actorId, tenantId, Arg.Any<CancellationToken>())
            .Returns(new AccountAuthorizationSnapshot(int.MaxValue, new HashSet<string> { Permissions.Root }));
        return authorizationCache;
    }

    private static IRoleReadService RoleReadService(Guid roleId, params string[] currentKeys)
    {
        var roleReadService = Substitute.For<IRoleReadService>();
        roleReadService.GetPermissionKeysAsync(roleId, Arg.Any<CancellationToken>()).Returns((IReadOnlyList<string>)currentKeys);
        return roleReadService;
    }

    [Fact]
    public async Task Handle_NoChanges_DoesNotEnqueueOutboxOrSaveChanges()
    {
        var roleId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var effectivePermissionReadService = Substitute.For<IEffectivePermissionReadService>();
        effectivePermissionReadService
            .GetAccountIdsForRoleAsync(roleId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<Guid>)new HashSet<Guid> { Guid.NewGuid() });

        var roleWriteService = Substitute.For<IRoleWriteService>();
        roleWriteService
            .UpdatePermissionsAsync(roleId, Arg.Any<IReadOnlyCollection<string>>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new RolePermissionUpdateResult(HasChanges: false, PermissionKeys: []));

        var outboxStore = Substitute.For<IOutboxStore>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new UpdateRolePermissionsHandler(
            RoleReadService(roleId),
            roleWriteService,
            Substitute.For<IPermissionGrantService>(),
            Substitute.For<ITenantReadService>(),
            RootAuthorizationCache(actorId, Guid.Empty),
            effectivePermissionReadService,
            CurrentUser(actorId),
            outboxStore,
            unitOfWork);

        await handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"]));

        await outboxStore.DidNotReceive().EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HasChanges_EnqueuesEffectivePermissionsForEveryAffectedAccount()
    {
        var roleId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var effectivePermissionReadService = Substitute.For<IEffectivePermissionReadService>();
        effectivePermissionReadService
            .GetAccountIdsForRoleAsync(roleId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<Guid>)new HashSet<Guid> { accountId });
        effectivePermissionReadService
            .GetEffectivePermissionsForAccountsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, IReadOnlySet<string>> { [accountId] = new HashSet<string> { "product:manage" } });

        var roleWriteService = Substitute.For<IRoleWriteService>();
        roleWriteService
            .UpdatePermissionsAsync(roleId, Arg.Any<IReadOnlyCollection<string>>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new RolePermissionUpdateResult(HasChanges: true, PermissionKeys: ["product:manage"]));

        var outboxStore = Substitute.For<IOutboxStore>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new UpdateRolePermissionsHandler(
            RoleReadService(roleId),
            roleWriteService,
            Substitute.For<IPermissionGrantService>(),
            Substitute.For<ITenantReadService>(),
            RootAuthorizationCache(actorId, Guid.Empty),
            effectivePermissionReadService,
            CurrentUser(actorId),
            outboxStore,
            unitOfWork);

        await handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"]));

        await outboxStore.Received(1).EnqueueAsync(
            Arg.Is<AccountEffectivePermissionsChangedIntegrationEvent>(e =>
                e.TenantId == Guid.Empty
                && e.Accounts.Count == 1
                && e.Accounts[0].AccountId == accountId
                && e.Accounts[0].Permissions.SequenceEqual(new[] { "product:manage" })),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoAccountsHoldTheRole_SkipsOutboxEvenIfPermissionsChanged()
    {
        var roleId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var effectivePermissionReadService = Substitute.For<IEffectivePermissionReadService>();
        effectivePermissionReadService
            .GetAccountIdsForRoleAsync(roleId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<Guid>)new HashSet<Guid>());

        var roleWriteService = Substitute.For<IRoleWriteService>();
        roleWriteService
            .UpdatePermissionsAsync(roleId, Arg.Any<IReadOnlyCollection<string>>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new RolePermissionUpdateResult(HasChanges: true, PermissionKeys: ["product:manage"]));

        var outboxStore = Substitute.For<IOutboxStore>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new UpdateRolePermissionsHandler(
            RoleReadService(roleId),
            roleWriteService,
            Substitute.For<IPermissionGrantService>(),
            Substitute.For<ITenantReadService>(),
            RootAuthorizationCache(actorId, Guid.Empty),
            effectivePermissionReadService,
            CurrentUser(actorId),
            outboxStore,
            unitOfWork);

        await handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"]));

        await outboxStore.DidNotReceive().EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActorMissingAPermissionBeingGranted_ThrowsAndNeverWrites()
    {
        var roleId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var authorizationCache = Substitute.For<IEffectiveAuthorizationCache>();
        authorizationCache
            .GetAsync(actorId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new AccountAuthorizationSnapshot(0, new HashSet<string> { "product:view" }));

        var roleWriteService = Substitute.For<IRoleWriteService>();

        var handler = new UpdateRolePermissionsHandler(
            RoleReadService(roleId),
            roleWriteService,
            Substitute.For<IPermissionGrantService>(),
            Substitute.For<ITenantReadService>(),
            authorizationCache,
            Substitute.For<IEffectivePermissionReadService>(),
            CurrentUser(actorId),
            Substitute.For<IOutboxStore>(),
            Substitute.For<IUnitOfWork>());

        await Should.ThrowAsync<ForbiddenException>(
            () => handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"])));

        await roleWriteService.DidNotReceive().UpdatePermissionsAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RoleAlreadyGrantsRoot_NonRootActorCannotModifyIt()
    {
        var roleId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var authorizationCache = Substitute.For<IEffectiveAuthorizationCache>();
        authorizationCache
            .GetAsync(actorId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new AccountAuthorizationSnapshot(0, new HashSet<string> { "product:manage" }));

        var roleWriteService = Substitute.For<IRoleWriteService>();

        var handler = new UpdateRolePermissionsHandler(
            RoleReadService(roleId, Permissions.Root),
            roleWriteService,
            Substitute.For<IPermissionGrantService>(),
            Substitute.For<ITenantReadService>(),
            authorizationCache,
            Substitute.For<IEffectivePermissionReadService>(),
            CurrentUser(actorId),
            Substitute.For<IOutboxStore>(),
            Substitute.For<IUnitOfWork>());

        await Should.ThrowAsync<ForbiddenException>(
            () => handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"])));

        await roleWriteService.DidNotReceive().UpdatePermissionsAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantBoundaryEnabled_ActorCannotGrantKeyOutsideIt()
    {
        // RequestContext.Current.TenantId is unset (ambient AsyncLocal, no HTTP request in a unit
        // test) - the handler resolves tenantId as Guid.Empty in that case, same as every other
        // test in this file, so every tenant-scoped stub below is keyed on Guid.Empty too.
        var roleId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var tenantId = Guid.Empty;

        var authorizationCache = Substitute.For<IEffectiveAuthorizationCache>();
        authorizationCache
            .GetAsync(actorId, tenantId, Arg.Any<CancellationToken>())
            .Returns(new AccountAuthorizationSnapshot(0, new HashSet<string> { "product:manage" }));

        var metadata = MetadataBase.Create<TenantMetadata>();
        metadata.PermissionBoundaryEnabled = true;
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme", metadata: metadata);

        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var permissionGrantService = Substitute.For<IPermissionGrantService>();
        permissionGrantService
            .GetGrantedKeysAsync(PermissionProviderName.Tenant, tenantId.ToString(), tenantId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<string>)new HashSet<string> { "order:view" });

        var roleWriteService = Substitute.For<IRoleWriteService>();

        var handler = new UpdateRolePermissionsHandler(
            RoleReadService(roleId),
            roleWriteService,
            permissionGrantService,
            tenantReadService,
            authorizationCache,
            Substitute.For<IEffectivePermissionReadService>(),
            CurrentUser(actorId),
            Substitute.For<IOutboxStore>(),
            Substitute.For<IUnitOfWork>());

        await Should.ThrowAsync<ForbiddenException>(
            () => handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"])));

        await roleWriteService.DidNotReceive().UpdatePermissionsAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
