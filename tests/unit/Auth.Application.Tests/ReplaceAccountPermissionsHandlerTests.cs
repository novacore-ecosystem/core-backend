using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ReplaceAccountPermissionsHandlerTests
{
    private readonly Guid _actorId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAccountAuthorizationGuard _authorizationGuard = Substitute.For<IAccountAuthorizationGuard>();
    private readonly IPermissionGrantService _permissionGrantService = Substitute.For<IPermissionGrantService>();
    private readonly IEffectivePermissionReadService _effectivePermissionReadService = Substitute.For<IEffectivePermissionReadService>();
    private readonly IOutboxStore _outboxStore = Substitute.For<IOutboxStore>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ReplaceAccountPermissionsHandler CreateHandler()
        => new(_currentUserService, _authorizationGuard, _permissionGrantService, _effectivePermissionReadService, _outboxStore, _unitOfWork);

    [Fact]
    public async Task Handle_NewPermissionAdded_ChecksGrantAuthorityOnlyForTheAddedKey()
    {
        var providerKey = _accountId.ToString();

        _currentUserService.GetUserId().Returns(_actorId);
        _permissionGrantService
            .GetGrantedKeysAsync(PermissionProviderName.User, providerKey, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<string>)new HashSet<string> { "order:view" });
        _permissionGrantService
            .ReplaceForProviderAsync(PermissionProviderName.User, providerKey, Arg.Any<IReadOnlyCollection<string>>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new PermissionGrantReplaceResult(true, new HashSet<string> { "order:view", "order:manage" }));
        _effectivePermissionReadService
            .GetEffectivePermissionsAsync(_accountId, Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<string>)new HashSet<string> { "order:view", "order:manage" });

        await CreateHandler().Handle(new ReplaceAccountPermissionsCommand(_accountId, ["order:view", "order:manage"]));

        await _authorizationGuard.Received(1).EnsureCanManageAccountAsync(_actorId, _accountId, Guid.Empty, Arg.Any<CancellationToken>());
        await _authorizationGuard.Received(1).EnsureCanGrantPermissionsAsync(
            _actorId,
            Arg.Is<IReadOnlyCollection<string>>(keys => keys.SequenceEqual(new[] { "order:manage" })),
            Guid.Empty,
            Arg.Any<CancellationToken>());
        await _outboxStore.Received(1).EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoChanges_DoesNotEnqueueOutboxOrSaveChanges()
    {
        _currentUserService.GetUserId().Returns(_actorId);
        _permissionGrantService
            .GetGrantedKeysAsync(PermissionProviderName.User, Arg.Any<string>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns((IReadOnlySet<string>)new HashSet<string>());
        _permissionGrantService
            .ReplaceForProviderAsync(PermissionProviderName.User, Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Guid.Empty, Arg.Any<CancellationToken>())
            .Returns(new PermissionGrantReplaceResult(false, new HashSet<string>()));

        await CreateHandler().Handle(new ReplaceAccountPermissionsCommand(_accountId, []));

        await _outboxStore.DidNotReceive().EnqueueAsync(Arg.Any<AccountEffectivePermissionsChangedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoCurrentUser_ThrowsUnauthorized()
    {
        _currentUserService.GetUserId().Returns((Guid?)null);

        await Should.ThrowAsync<UnauthorizedException>(
            () => CreateHandler().Handle(new ReplaceAccountPermissionsCommand(_accountId, [])));
    }
}
