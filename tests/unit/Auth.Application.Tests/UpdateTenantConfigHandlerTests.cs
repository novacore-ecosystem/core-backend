using System.Text.Json;

using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class UpdateTenantConfigHandlerTests
{
    private static (Tenant Tenant, UpdateTenantConfigHandler Handler) BuildHandlerWithTenant()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        tenant.SetLocale(null, """{"theme":"light","brand":"Acme"}""", "{}");

        var writeService = Substitute.For<ITenantWriteService>();
        writeService.UpdateWithLocalesAsync(tenant.Id, Arg.Any<Action<Tenant>>(), Arg.Any<CancellationToken>())
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

        var handler = new UpdateTenantConfigHandler(uow, writeService, Substitute.For<IOutboxStore>());
        return (tenant, handler);
    }

    [Fact]
    public async Task Handle_WithNoLanguage_TargetsTheFallbackLocale_AndPreservesUnrelatedKeys()
    {
        var (tenant, handler) = BuildHandlerWithTenant();

        var payload = JsonDocument.Parse("""{"theme":"dark"}""").RootElement;
        await handler.Handle(new UpdateTenantConfigCommand(tenant.Id, null, payload));

        var fallback = tenant.Locales.Single(l => l.LanguageCode is null);
        var merged = JsonDocument.Parse(fallback.ConfigurationJson).RootElement;
        merged.GetProperty("theme").GetString().ShouldBe("dark");  // updated
        merged.GetProperty("brand").GetString().ShouldBe("Acme");  // preserved
    }
}
