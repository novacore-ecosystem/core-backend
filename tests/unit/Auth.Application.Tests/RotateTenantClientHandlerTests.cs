using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.RotateTenantClient;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class RotateTenantClientHandlerTests
{
    [Fact]
    public async Task Handle_RevokesEveryActiveClient_AndIssuesANewOne_ReturningOnlyTheNewKey()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var oldClient = TenantClient.Create(tenant.Id, "Web Client");
        var oldPublicKey = oldClient.PublicKey.Value;

        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);

        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.ListByTenantAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns([oldClient]);

        var tenantClientWriteService = Substitute.For<ITenantClientWriteService>();
        tenantClientWriteService.UpdateAsync(oldClient.Id, Arg.Any<Action<TenantClient>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                ci.ArgAt<Action<TenantClient>>(1)(oldClient);
                return Task.CompletedTask;
            });
        TenantClient? newClient = null;
        tenantClientWriteService.CreateAsync(Arg.Do<TenantClient>(c => newClient = c), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        var handler = new RotateTenantClientHandler(tenantReadService, tenantClientReadService, tenantClientWriteService, uow);

        var result = await handler.Handle(new RotateTenantClientCommand(tenant.Id));

        oldClient.Status.ShouldBe(TenantClientStatus.Revoked);
        oldClient.RevokedReason.ShouldBe(RevocationReason.Superseded);

        newClient.ShouldNotBeNull();
        newClient!.TenantId.ShouldBe(tenant.Id);
        result.PublicKey.ShouldBe(newClient.PublicKey.Value);
        result.PublicKey.ShouldNotBe(oldPublicKey);
        result.ClientId.ShouldBe(newClient.Id);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTenantDoesNotExist()
    {
        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tenant?)null);

        var handler = new RotateTenantClientHandler(
            tenantReadService,
            Substitute.For<ITenantClientReadService>(),
            Substitute.For<ITenantClientWriteService>(),
            Substitute.For<IUnitOfWork>());

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new RotateTenantClientCommand(Guid.NewGuid())));
    }
}
