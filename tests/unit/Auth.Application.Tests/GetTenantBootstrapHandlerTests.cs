using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantBootstrap;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class GetTenantBootstrapHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCurrentVersion_ForAUsableClientOfAnActiveTenant()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        tenant.IncrementVersion();
        var client = TenantClient.Create(tenant.Id, "Web Client");

        var handler = BuildHandler(client, tenant);

        var result = await handler.Handle(new GetTenantBootstrapQuery(client.PublicKey.Value));

        result.Version.ShouldBe(tenant.Version);
        result.Tenant.Id.ShouldBe(tenant.Id);
        result.SupportedLanguages.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_ForAnUnknownClientKey()
    {
        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.GetByPublicKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((TenantClient?)null);
        var handler = new GetTenantBootstrapHandler(tenantClientReadService, Substitute.For<ITenantReadService>());

        await Should.ThrowAsync<UnauthorizedException>(() => handler.Handle(new GetTenantBootstrapQuery("unknown")));
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_ForARevokedClientKey()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        var client = TenantClient.Create(tenant.Id, "Web Client");
        client.Revoke(RevocationReason.AdminForced);

        var handler = BuildHandler(client, tenant);

        await Should.ThrowAsync<UnauthorizedException>(() => handler.Handle(new GetTenantBootstrapQuery(client.PublicKey.Value)));
    }

    [Fact]
    public async Task Handle_RejectsTheRootClient()
    {
        var rootClient = TenantClient.Create(null, "Root Client");
        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.GetByPublicKeyAsync(rootClient.PublicKey.Value, Arg.Any<CancellationToken>()).Returns(rootClient);
        var handler = new GetTenantBootstrapHandler(tenantClientReadService, Substitute.For<ITenantReadService>());

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(new GetTenantBootstrapQuery(rootClient.PublicKey.Value)));
    }

    [Fact]
    public async Task Handle_ThrowsConflict_ForADisabledTenant()
    {
        var tenant = Tenant.Create(TenantCode.Create("acme"), "Acme Corp");
        tenant.Deactivate();
        var client = TenantClient.Create(tenant.Id, "Web Client");

        var handler = BuildHandler(client, tenant);

        await Should.ThrowAsync<ConflictException>(() => handler.Handle(new GetTenantBootstrapQuery(client.PublicKey.Value)));
    }

    private static GetTenantBootstrapHandler BuildHandler(TenantClient client, Tenant tenant)
    {
        var tenantClientReadService = Substitute.For<ITenantClientReadService>();
        tenantClientReadService.GetByPublicKeyAsync(client.PublicKey.Value, Arg.Any<CancellationToken>()).Returns(client);
        var tenantReadService = Substitute.For<ITenantReadService>();
        tenantReadService.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        return new GetTenantBootstrapHandler(tenantClientReadService, tenantReadService);
    }
}
