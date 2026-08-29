using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Features.Roles.Commands.UpdateRolePermissions;
using NovaCore.Auth.Application.Features.Roles.DTOs;

using NovaCore.BuildingBlock.Contract.Events.User;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class UpdateRolePermissionsHandlerTests
{
    [Fact]
    public async Task Handle_NoChanges_DoesNotEnqueueOutboxOrSaveChanges()
    {
        var roleId = Guid.NewGuid();
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

        var handler = new UpdateRolePermissionsHandler(roleWriteService, effectivePermissionReadService, outboxStore, unitOfWork);

        await handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"]));

        await outboxStore.DidNotReceive().EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HasChanges_EnqueuesEffectivePermissionsForEveryAffectedAccount()
    {
        var roleId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

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

        var handler = new UpdateRolePermissionsHandler(roleWriteService, effectivePermissionReadService, outboxStore, unitOfWork);

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

        var handler = new UpdateRolePermissionsHandler(roleWriteService, effectivePermissionReadService, outboxStore, unitOfWork);

        await handler.Handle(new UpdateRolePermissionsCommand(roleId, ["product:manage"]));

        await outboxStore.DidNotReceive().EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
