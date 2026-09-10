using System.Text.Json;

using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class UpdateTenantConfigHandlerTests
{
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
    public async Task Handle_WithNoLanguage_TargetsTheFallbackLocale_AndEnqueuesVersionChangedEvent()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");

        var writeService = Substitute.For<ITenantWriteService>();
        writeService.UpsertLocaleAsync(
                Arg.Any<Guid>(), Arg.Any<LanguageCode?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(tenant);

        var outbox = Substitute.For<IOutboxStore>();
        var handler = new UpdateTenantConfigHandler(BuildUnitOfWork(), writeService, outbox);

        var payload = JsonDocument.Parse("""{"theme":"dark"}""").RootElement;
        await handler.Handle(new UpdateTenantConfigCommand(tenant.Id, null, payload));

        await writeService.Received(1).UpsertLocaleAsync(
            tenant.Id, null, Arg.Is<string>(json => json.Contains("dark")), null, Arg.Any<CancellationToken>());
        await outbox.Received(1).EnqueueAsync(
            Arg.Is<TenantVersionChangedIntegrationEvent>(e => e.TenantId == tenant.Id && e.Version == tenant.Version),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RejectsNonObjectPayload()
    {
        var handler = new UpdateTenantConfigHandler(BuildUnitOfWork(), Substitute.For<ITenantWriteService>(), Substitute.For<IOutboxStore>());

        var payload = JsonDocument.Parse("[1,2,3]").RootElement;

        await Should.ThrowAsync<BadRequestException>(
            () => handler.Handle(new UpdateTenantConfigCommand(Guid.NewGuid(), null, payload)));
    }
}
