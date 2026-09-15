using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultPermissions;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Exceptions;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ReplaceRegistrationDefaultPermissionsHandlerTests
{
    private static IAppReadService AppReadService(Guid appId, bool exists = true)
    {
        var readService = Substitute.For<IAppReadService>();
        readService.GetByIdAsync(appId, Arg.Any<CancellationToken>())
            .Returns(exists ? App.Create(AppCode.Create("storefront_web"), "Storefront Web") : null);
        return readService;
    }

    [Fact]
    public async Task Handle_ReplacesDefaultsThenRefreshesCache_InThatOrder()
    {
        var appId = Guid.NewGuid();
        string[] permissionKeys = ["product:view"];

        var writeService = Substitute.For<IRegistrationDefaultsWriteService>();
        var cache = Substitute.For<IRegistrationDefaultsCache>();

        var handler = new ReplaceRegistrationDefaultPermissionsHandler(AppReadService(appId), writeService, cache);

        await handler.Handle(new ReplaceRegistrationDefaultPermissionsCommand(appId, permissionKeys));

        Received.InOrder(() =>
        {
            writeService.ReplacePermissionsAsync(Guid.Empty, appId, permissionKeys, Arg.Any<CancellationToken>());
            cache.RefreshAsync(Guid.Empty, appId, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenAppDoesNotExist()
    {
        var appId = Guid.NewGuid();
        var writeService = Substitute.For<IRegistrationDefaultsWriteService>();
        var cache = Substitute.For<IRegistrationDefaultsCache>();

        var handler = new ReplaceRegistrationDefaultPermissionsHandler(
            AppReadService(appId, exists: false), writeService, cache);

        await Should.ThrowAsync<NotFoundException>(
            () => handler.Handle(new ReplaceRegistrationDefaultPermissionsCommand(appId, ["product:view"])));

        await writeService.DidNotReceive().ReplacePermissionsAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
        await cache.DidNotReceive().RefreshAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
