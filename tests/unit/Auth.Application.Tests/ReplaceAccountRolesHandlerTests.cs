using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;
using NovaCore.Auth.Application.Features.Accounts.DTOs;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ReplaceAccountRolesHandlerTests
{
    private readonly Guid _actorId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAccountAuthorizationGuard _authorizationGuard = Substitute.For<IAccountAuthorizationGuard>();
    private readonly IAccountReadService _accountReadService = Substitute.For<IAccountReadService>();
    private readonly IAccountRoleAssignmentService _accountRoleAssignmentService = Substitute.For<IAccountRoleAssignmentService>();
    private readonly IEffectivePermissionReadService _effectivePermissionReadService = Substitute.For<IEffectivePermissionReadService>();
    private readonly IOutboxStore _outboxStore = Substitute.For<IOutboxStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ReplaceAccountRolesHandler CreateHandler()
        => new(_currentUserService, _authorizationGuard, _accountReadService, _accountRoleAssignmentService, _effectivePermissionReadService, _outboxStore, _unitOfWork);

    [Fact]
    public async Task Handle_NoChanges_DoesNotEnqueueOutboxOrSaveChanges()
    {
        _currentUserService.GetUserId().Returns(_actorId);
        _accountReadService.GetRoleIdsAsync(_accountId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<Guid>)new HashSet<Guid>());
        _accountRoleAssignmentService
            .ReplaceRolesAsync(_accountId, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new AccountRoleReplaceResult(HasChanges: false, ResultingRoleIds: new HashSet<Guid>()));

        await CreateHandler().Handle(new ReplaceAccountRolesCommand(_accountId, []));

        await _outboxStore.DidNotReceive().EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewRoleAdded_ChecksGrantAuthorityOnlyForTheAddedRole()
    {
        var existingRoleId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();

        _currentUserService.GetUserId().Returns(_actorId);
        _accountReadService.GetRoleIdsAsync(_accountId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<Guid>)new HashSet<Guid> { existingRoleId });
        _accountRoleAssignmentService
            .ReplaceRolesAsync(_accountId, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new AccountRoleReplaceResult(HasChanges: true, ResultingRoleIds: new HashSet<Guid> { existingRoleId, newRoleId }));
        _effectivePermissionReadService
            .GetEffectivePermissionsAsync(_accountId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<string>)new HashSet<string> { "product:manage" });

        await CreateHandler().Handle(new ReplaceAccountRolesCommand(_accountId, [existingRoleId, newRoleId]));

        await _authorizationGuard.Received(1).EnsureCanManageAccountAsync(_actorId, _accountId, Guid.Empty, Arg.Any<CancellationToken>());
        await _authorizationGuard.Received(1).EnsureCanGrantRoleAsync(_actorId, newRoleId, Guid.Empty, Arg.Any<CancellationToken>());
        await _authorizationGuard.DidNotReceive().EnsureCanGrantRoleAsync(_actorId, existingRoleId, Guid.Empty, Arg.Any<CancellationToken>());
        await _outboxStore.Received(1).EnqueueAsync(
            Arg.Is<AccountEffectivePermissionsChangedIntegrationEvent>(e => e.Accounts.Single().AccountId == _accountId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoCurrentUser_ThrowsUnauthorized()
    {
        _currentUserService.GetUserId().Returns((Guid?)null);

        await Should.ThrowAsync<UnauthorizedException>(
            () => CreateHandler().Handle(new ReplaceAccountRolesCommand(_accountId, [])));
    }
}
