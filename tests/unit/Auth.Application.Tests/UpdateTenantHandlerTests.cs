using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenant;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Contract.Events.Tenant;

using NovaCore.TestKit.Fakes;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class UpdateTenantHandlerTests
{
    [Fact]
    public async Task Handle_RenamesTenant_AndBumpsVersion_AndEnqueuesVersionChangedEvent()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var originalVersion = tenant.Version;

        var writeService = Substitute.For<ITenantWriteService>();
        writeService.UpdateAsync(tenant.Id, Arg.Any<Action<Tenant>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                ci.ArgAt<Action<Tenant>>(1)(tenant);
                return Task.CompletedTask;
            });

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var outbox = Substitute.For<IOutboxStore>();
        var handler = new UpdateTenantHandler(writeService, uow, outbox, new FakeCurrentUserService());

        await handler.Handle(new UpdateTenantCommand(tenant.Id, "Acme Corporation", "https://logo", null));

        tenant.Name.ShouldBe("Acme Corporation");
        tenant.LogoUrl.ShouldBe("https://logo");
        tenant.Version.ShouldBe(originalVersion + 1);

        await outbox.Received(1).EnqueueAsync(
            Arg.Is<TenantVersionChangedIntegrationEvent>(e => e.TenantId == tenant.Id && e.Version == tenant.Version),
            Arg.Any<CancellationToken>());
    }
}
