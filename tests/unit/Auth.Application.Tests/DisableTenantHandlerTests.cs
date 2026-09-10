using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.DisableTenant;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class DisableTenantHandlerTests
{
    private static ITenantWriteService BuildWriteService(Tenant tenant)
    {
        var writeService = Substitute.For<ITenantWriteService>();
        writeService.DisableAsync(tenant.Id, Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                tenant.Deactivate();
                return Task.CompletedTask;
            });
        return writeService;
    }

    private static IUnitOfWork BuildUnitOfWork()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });
        return uow;
    }

    [Fact]
    public async Task Handle_DeactivatesAnActiveTenant()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var handler = new DisableTenantHandler(BuildUnitOfWork(), BuildWriteService(tenant));

        await handler.Handle(new DisableTenantCommand(tenant.Id));

        tenant.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_IsIdempotent_OnAnAlreadyDisabledTenant()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        tenant.Deactivate();
        var handler = new DisableTenantHandler(BuildUnitOfWork(), BuildWriteService(tenant));

        // Must not throw and must not produce an inconsistent state on a second call.
        await handler.Handle(new DisableTenantCommand(tenant.Id));

        tenant.IsActive.ShouldBeFalse();
    }
}
