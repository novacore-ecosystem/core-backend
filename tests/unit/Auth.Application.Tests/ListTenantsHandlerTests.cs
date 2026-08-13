using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ListTenantsHandlerTests
{
    private static Tenant CreateTenant(string code, string name) =>
        Tenant.Create(TenantCode.Create(code), name);

    [Fact]
    public async Task Handle_PassesSearchAndPagingThroughToReadService_AndWrapsResultAsPaginatedResult()
    {
        var tenants = new[] { CreateTenant("acme", "Acme Corp") };
        var readService = Substitute.For<ITenantReadService>();
        readService.SearchAsync("acme", 2, 10, Arg.Any<CancellationToken>())
            .Returns((tenants, 21));

        var handler = new ListTenantsHandler(readService);

        var result = await handler.Handle(new ListTenantsQuery("acme", 2, 10));

        result.TotalCount.ShouldBe(21);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.Items.ShouldHaveSingleItem();
        result.Items.Single().Code.ShouldBe("acme");

        await readService.Received(1).SearchAsync("acme", 2, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NeverIncludesLargeConfigurationOrClientFields_ListStaysLightweight()
    {
        var readService = Substitute.For<ITenantReadService>();
        readService.SearchAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(([CreateTenant("acme", "Acme Corp")], 1));

        var handler = new ListTenantsHandler(readService);

        var result = await handler.Handle(new ListTenantsQuery());

        // TenantSummaryResponse's own shape is the enforcement mechanism - it has no Metadata,
        // Version, Locales, or client fields to leak. This test documents that intent.
        var item = result.Items.Single();
        item.Id.ShouldNotBe(Guid.Empty);
        item.Code.ShouldBe("acme");
        item.Name.ShouldBe("Acme Corp");
        item.IsActive.ShouldBeTrue();
    }
}
