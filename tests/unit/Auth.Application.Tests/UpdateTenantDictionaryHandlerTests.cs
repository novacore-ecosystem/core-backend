using System.Text.Json;

using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Contract.Events.Tenant;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class UpdateTenantDictionaryHandlerTests
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
    public async Task Handle_ValidPayload_UpsertsLocale_AndEnqueuesVersionChangedEvent()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var languageCode = LanguageCode.Create("vi");

        string? capturedConfigurationJson = "unset";
        string? capturedDictionaryJson = null;

        var writeService = Substitute.For<ITenantWriteService>();
        writeService.UpsertLocaleAsync(
                Arg.Any<Guid>(),
                Arg.Any<LanguageCode?>(),
                Arg.Do<string?>(json => capturedConfigurationJson = json),
                Arg.Do<string?>(json => capturedDictionaryJson = json),
                Arg.Any<CancellationToken>())
            .Returns(tenant);

        var outbox = Substitute.For<IOutboxStore>();
        var handler = new UpdateTenantDictionaryHandler(writeService, BuildUnitOfWork(), outbox);

        var payload = JsonDocument.Parse("""{"welcome":"Chao ban"}""").RootElement;
        await handler.Handle(new UpdateTenantDictionaryCommand(tenant.Id, languageCode, payload));

        capturedConfigurationJson.ShouldBeNull();
        capturedDictionaryJson.ShouldNotBeNull();
        capturedDictionaryJson.ShouldContain("Chao ban");
        await outbox.Received(1).EnqueueAsync(
            Arg.Is<TenantVersionChangedIntegrationEvent>(e => e.TenantId == tenant.Id && e.Version == tenant.Version),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RejectsNonObjectPayload()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var writeService = Substitute.For<ITenantWriteService>();
        var handler = new UpdateTenantDictionaryHandler(writeService, BuildUnitOfWork(), Substitute.For<IOutboxStore>());

        var payload = JsonDocument.Parse("[1,2,3]").RootElement;

        await Should.ThrowAsync<BadRequestException>(
            () => handler.Handle(new UpdateTenantDictionaryCommand(tenant.Id, LanguageCode.Create("vi"), payload)));
    }
}
