using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.Auth.Application.Features.Registrations.Queries.GetRegistrationDefaults;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;

using NSubstitute;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class GetRegistrationDefaultsHandlerTests
{
    private static IAppReadService AppReadService(Guid appId, bool exists = true)
    {
        var readService = Substitute.For<IAppReadService>();
        readService.GetByIdAsync(appId, Arg.Any<CancellationToken>())
            .Returns(exists ? App.Create(AppCode.Create("storefront_web"), "Storefront Web") : null);
        return readService;
    }

    [Fact]
    public async Task Handle_ReturnsSnapshotFromCache_ForCallersTenant()
    {
        var appId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var snapshot = new RegistrationDefaultsSnapshot([roleId], ["product:view"]);

        var cache = Substitute.For<IRegistrationDefaultsCache>();
        cache.GetAsync(Guid.Empty, appId, Arg.Any<CancellationToken>()).Returns(snapshot);

        var handler = new GetRegistrationDefaultsHandler(AppReadService(appId), cache);

        var response = await handler.Handle(new GetRegistrationDefaultsQuery(appId));

        response.RoleIds.ShouldBe([roleId]);
        response.PermissionKeys.ShouldBe(["product:view"]);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenAppDoesNotExist()
    {
        var appId = Guid.NewGuid();
        var cache = Substitute.For<IRegistrationDefaultsCache>();

        var handler = new GetRegistrationDefaultsHandler(AppReadService(appId, exists: false), cache);

        await Should.ThrowAsync<NotFoundException>(
            () => handler.Handle(new GetRegistrationDefaultsQuery(appId)));

        await cache.DidNotReceive().GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
