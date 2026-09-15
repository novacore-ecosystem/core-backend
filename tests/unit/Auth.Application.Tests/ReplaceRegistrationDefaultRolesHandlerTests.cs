using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultRoles;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Exceptions;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ReplaceRegistrationDefaultRolesHandlerTests
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
        var roleIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var writeService = Substitute.For<IRegistrationDefaultsWriteService>();
        var cache = Substitute.For<IRegistrationDefaultsCache>();

        var handler = new ReplaceRegistrationDefaultRolesHandler(AppReadService(appId), writeService, cache);

        await handler.Handle(new ReplaceRegistrationDefaultRolesCommand(appId, roleIds));

        Received.InOrder(() =>
        {
            writeService.ReplaceRolesAsync(Guid.Empty, appId, roleIds, Arg.Any<CancellationToken>());
            cache.RefreshAsync(Guid.Empty, appId, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenAppDoesNotExist()
    {
        var appId = Guid.NewGuid();
        var writeService = Substitute.For<IRegistrationDefaultsWriteService>();
        var cache = Substitute.For<IRegistrationDefaultsCache>();

        var handler = new ReplaceRegistrationDefaultRolesHandler(
            AppReadService(appId, exists: false), writeService, cache);

        await Should.ThrowAsync<NotFoundException>(
            () => handler.Handle(new ReplaceRegistrationDefaultRolesCommand(appId, [Guid.NewGuid()])));

        await writeService.DidNotReceive().ReplaceRolesAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
        await cache.DidNotReceive().RefreshAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
