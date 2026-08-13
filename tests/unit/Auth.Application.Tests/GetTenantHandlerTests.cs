using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Queries.GetTenant;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class GetTenantHandlerTests
{
    private static Tenant CreateTenant()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        tenant.SetLocale(null, """{"theme":"light","brand":"Acme"}""", """{"welcome":"Hello","logout":"Bye"}""");
        tenant.SetLocale(LanguageCode.Create("vi"), """{"theme":"dark"}""", """{"welcome":"Xin chao"}""");
        return tenant;
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTenantDoesNotExist()
    {
        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Tenant?)null);
        var handler = new GetTenantHandler(tenantReadService, Substitute.For<ITenantClientReadService>());

        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(new GetTenantQuery(Guid.NewGuid())));
    }

    [Fact]
    public async Task Handle_MergedTranslations_NeverIncludeTheFallbackLocaleAsItsOwnEntry()
    {
        var tenant = CreateTenant();
        var handler = BuildHandler(tenant);

        var result = await handler.Handle(new GetTenantQuery(tenant.Id));

        // The fallback/default resource (null LanguageCode) has no language code to key it by -
        // it must never appear as an entry in the merged Translations collection, only "en"/"vi".
        result.Translations.Keys.ShouldBe(LanguageCodeConstant.SupportedLanguages, ignoreOrder: true);
    }

    [Fact]
    public async Task Handle_MergedTranslations_LanguageOverrideWinsButUnrelatedFallbackKeysSurvive()
    {
        var tenant = CreateTenant();
        var handler = BuildHandler(tenant);

        var result = await handler.Handle(new GetTenantQuery(tenant.Id));

        var vi = result.Translations["vi"];
        vi.Dictionary.GetProperty("welcome").GetString().ShouldBe("Xin chao"); // override wins
        vi.Dictionary.GetProperty("logout").GetString().ShouldBe("Bye");      // inherited from fallback
        vi.Configuration.GetProperty("theme").GetString().ShouldBe("dark");   // override wins
        vi.Configuration.GetProperty("brand").GetString().ShouldBe("Acme");   // inherited from fallback

        // "en" has no override row at all - effective view is exactly the fallback.
        var en = result.Translations["en"];
        en.Dictionary.GetProperty("welcome").GetString().ShouldBe("Hello");
    }

    [Fact]
    public async Task Handle_IncludesRawLocalesForEditing_IncludingTheFallbackRow()
    {
        var tenant = CreateTenant();
        var handler = BuildHandler(tenant);

        var result = await handler.Handle(new GetTenantQuery(tenant.Id));

        result.Locales.ShouldContain(l => l.LanguageCode == null);
        result.Locales.ShouldContain(l => l.LanguageCode == "vi");
    }

    [Fact]
    public async Task Handle_NeverExposesAnyClientSecret()
    {
        var tenant = CreateTenant();
        var client = TenantClient.Create(tenant.Id, "Web Client");
        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.ListByTenantAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns([client]);
        var handler = new GetTenantHandler(tenantReadService, tenantClientReadService);

        var result = await handler.Handle(new GetTenantQuery(tenant.Id));

        // TenantClientSummaryResponse's own shape is the enforcement - PublicKey is safe to
        // include (not a secret), and there is no secret field on TenantClient to redact.
        result.Clients.ShouldHaveSingleItem();
        result.Clients.Single().PublicKey.ShouldBe(client.PublicKey.Value);
    }

    private static GetTenantHandler BuildHandler(Tenant tenant)
    {
        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.ListByTenantAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns([]);
        return new GetTenantHandler(tenantReadService, tenantClientReadService);
    }
}
