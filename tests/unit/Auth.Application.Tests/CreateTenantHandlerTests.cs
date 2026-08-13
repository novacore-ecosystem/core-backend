using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.CreateTenant;
using NovaCore.Auth.Domain.Entities.Tenants;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class CreateTenantHandlerTests
{
    [Fact]
    public async Task Handle_CreatesTenant_WhenCodeIsUnique()
    {
        var readService = Substitute.For<ITenantReadService>();
        readService.ExistsByCodeAsync("acme", Arg.Any<CancellationToken>()).Returns(false);
        var writeService = Substitute.For<ITenantWriteService>();
        Tenant? created = null;
        writeService.CreateAsync(Arg.Do<Tenant>(t => created = t), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new CreateTenantHandler(readService, writeService);

        var id = await handler.Handle(new CreateTenantCommand("acme", "Acme Corp", null, null));

        id.ShouldNotBe(Guid.Empty);
        created.ShouldNotBeNull();
        created!.Code.Value.ShouldBe("acme");
        created.Name.ShouldBe("Acme Corp");
        created.Version.ShouldBe(1);
        created.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenCodeAlreadyExists()
    {
        var readService = Substitute.For<ITenantReadService>();
        readService.ExistsByCodeAsync("acme", Arg.Any<CancellationToken>()).Returns(true);
        var writeService = Substitute.For<ITenantWriteService>();
        var handler = new CreateTenantHandler(readService, writeService);

        await Should.ThrowAsync<ConflictException>(
            () => handler.Handle(new CreateTenantCommand("acme", "Acme Corp", null, null)));

        await writeService.DidNotReceive().CreateAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
    }
}
