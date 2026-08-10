using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NovaCore.Order.Application;
using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.DTOs;
using NovaCore.Order.Domain.Entities.Catalogs;
using NovaCore.Order.Domain.Enums;
using NovaCore.Order.Domain.ValueObjects;
using NovaCore.Order.Persistence;
using NovaCore.Order.Persistence.Engine;

using NovaCore.TestKit.Fakes;

using Testcontainers.PostgreSql;

using Xunit;

using OrderEntity = NovaCore.Order.Domain.Entities.Orders.Order;

namespace NovaCore.Order.IntegrationTests.Infrastructure;

/// <summary>
/// Shared harness for NovaCore.Order.IntegrationTests: a real Postgres (Testcontainers), wired through the
/// exact same <c>NovaCore.Order.Persistence</c>/<c>NovaCore.Order.Application</c> DI extensions NovaCore.Order.API itself
/// uses (<see cref="NovaCore.Order.Persistence.DependencyInjection.AddPersistence"/> and
/// <see cref="NovaCore.Order.Application.DependencyInjection.AddApplication"/>) - so concurrency behavior
/// (xmin optimistic concurrency, transaction/rollback semantics, MediatR pipeline) is exactly
/// what production runs, not a simplified stand-in.
///
/// Deliberately NOT a WebApplicationFactory/real-HTTP host: <c>NovaCore.Order.Infrastructure.AddInfrastructure</c>
/// eagerly wires Kafka, Redis, and a gRPC channel to Inventory Service, none of which are relevant
/// to whether two concurrent UpdateOrder requests corrupt the same Order aggregate - that's purely
/// an EF Core + Postgres question. <see cref="IInventoryClientService"/> (real impl needs a live
/// Inventory gRPC endpoint) is substituted with an ample-stock fake; <see cref="ICurrentUserService"/>
/// (real impl reads HttpContext) with the TestKit's <see cref="FakeCurrentUserService"/>. Every other
/// moving part - OrderDbContext, OrderRepo, OrderWriteService, EfUnitOfWork, the real
/// UpdateOrderHandler/OrderItemPreparationService - is exactly what NovaCore.Order.API runs.
///
/// One Postgres container for the whole test class (expensive to start); callers are responsible
/// for keeping test data isolated per test/iteration (a fresh Order per iteration, not a fresh
/// database).
///
/// NOTE: while building the race-condition suite, every single UpdateOrder call - even with zero
/// concurrency - turned out to fail with the same ConflictException/DbUpdateConcurrencyException
/// the race test was designed to detect. That's a separate, more fundamental bug in
/// Order.UpdateItems/OrderRepo.UpdateAsync (new OrderItem entities discovered via collection
/// navigation fix-up, rather than an explicit context.Add(), get misclassified as Modified
/// instead of Added once their client-generated Guid key is non-default) - see
/// docs/tasks/2026-07-27/Task23_updateorder-always-fails-not-a-race-condition.md. It is NOT fixed
/// here per this task's "don't modify production code" instruction; it just means the race test
/// below can't yet observe the "one 200, one 409" steady state it was built to distinguish from
/// corruption - today it only ever sees "409, 409".
/// </summary>
public abstract class OrderIntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private ServiceProvider _rootProvider = null!;

    public async Task InitializeAsync()
    {
        // Same major version as docker-compose.yml's POSTGRES_IMAGE (see .env) - keeps xmin/EF
        // Core behavior identical to what actually runs in dev/prod, not just "some Postgres".
        _postgres = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("order_race_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await _postgres.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddPersistence(configuration);
        services.AddApplication();

        // Real IInventoryClientService needs a live Inventory gRPC endpoint - substituted with an
        // ample-stock fake so OrderItemPreparationService's stock check never rejects a test
        // request; that check is not what this test suite is about.
        var inventoryClient = Substitute.For<IInventoryClientService>();
        inventoryClient
            .GetAvailableStockBatchAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var ids = callInfo.Arg<IReadOnlyCollection<Guid>>() ?? [];
                IReadOnlyDictionary<Guid, int> stock = ids.ToDictionary(id => id, _ => 1_000);
                return Task.FromResult(stock);
            });
        services.AddSingleton(inventoryClient);

        // Real ICurrentUserService reads HttpContext, which doesn't exist here - EfOutboxStore
        // (enqueued by every Order command handler) needs one to stamp actor metadata.
        services.AddScoped<ICurrentUserService>(_ => new FakeCurrentUserService());

        _rootProvider = services.BuildServiceProvider(validateScopes: true);

        await using var scope = _rootProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _rootProvider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>A fresh DI scope - the equivalent of a brand-new incoming HTTP request, with its own OrderDbContext instance. Never share a scope between two "concurrent requests" in a test, or the race being tested is fake (same DbContext can't race with itself).</summary>
    protected AsyncServiceScope CreateScope() => _rootProvider.CreateAsyncScope();

    protected static ISender GetSender(AsyncServiceScope scope) => scope.ServiceProvider.GetRequiredService<ISender>();

    /// <summary>Seeds ProductCatalog rows directly (bypassing the Product->Kafka->consumer sync path, which isn't running here) so OrderItemPreparationService's catalog lookup resolves the variation ids a test uses.</summary>
    protected async Task SeedCatalogAsync(params (Guid ProductId, Guid VariationId, string ProductName, string VariationName, string Sku, decimal Price)[] variations)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        foreach (var v in variations)
        {
            db.ProductCatalogs.Add(
                ProductCatalog.Create(
                    v.ProductId,
                    v.VariationId,
                    v.ProductName,
                    v.VariationName,
                    Sku.Create(v.Sku),
                    Money.Create(v.Price),
                    ProductCatalogStatus.Active));
        }

        await db.SaveChangesAsync();
    }

    /// <summary>Creates a Pending order directly via the domain factory + write service, bypassing CreateOrderHandler's saga/idempotency machinery - this suite is about UpdateOrder, not order creation.</summary>
    protected async Task<Guid> CreateOrderAsync(
        Guid customerId,
        string customerName,
        string customerPhone,
        string shippingAddress,
        params (
            Guid productId,
            Guid variationId,
            string productName,
            string variationName,
            decimal unitPrice,
            int quantity
        )[] items)
    {
        await using var scope = CreateScope();
        var writeService = scope.ServiceProvider.GetRequiredService<IOrderWriteService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var request = new CreateOrderRequest(
            Guid.NewGuid().ToString(),
            null,
            new OrderOwnerRequestDto(
                customerId,
                customerName,
                Email.Create("abc1234@gmai.com"),
                PhoneNumber.Create(customerPhone)),
            new OrderShippingInfoRequestDto(
                ShippingMethod.Standard,
                customerName,
                PhoneNumber.Create(customerPhone),
                shippingAddress,
                string.Empty),
            items
                .Select(item => new PreparedOrderItem(
                    item.productId,
                    item.variationId,
                    item.productName,
                    item.variationName,
                    item.unitPrice,
                    item.quantity))
                .ToArray());

        var order = await writeService.CreateAsync(request);
        await uow.SaveChangesAsync();

        return order.Id;
    }

    /// <summary>
    /// Moves a Pending order to Confirmed via IOrderWriteService.ConfirmAsync directly - the same
    /// saga-bypassing shortcut CreateOrderAsync takes for creation. Needed before
    /// UpdateOrderOwnerInfo will accept the order (OrderWriteService.UpdateOwnerInfoAsync only
    /// allows Status == Confirmed, unlike UpdateItems' Pending-only window).
    /// </summary>
    protected async Task ConfirmOrderAsync(Guid orderId)
    {
        await using var scope = CreateScope();
        var writeService = scope.ServiceProvider.GetRequiredService<IOrderWriteService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await writeService.ConfirmAsync(orderId);
        await uow.SaveChangesAsync();
    }

    /// <summary>
    /// Reloads the order from a brand-new DbContext/scope - never the tracked entity a handler
    /// under test just mutated. <c>Version</c> is the Postgres <c>xmin</c> system column EF maps
    /// as the concurrency token (see OrderConfig.cs) - read via a raw shadow-property projection
    /// since AsNoTracking() results can't use ChangeTracker-based Entry() access.
    /// </summary>
    protected async Task<OrderSnapshot?> ReloadOrderAsync(Guid orderId)
    {
        await using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Owner)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return null;

        var version = await db.Orders
            .Where(o => o.Id == orderId)
            .Select(o => EF.Property<uint>(o, "xmin"))
            .FirstAsync();

        return new OrderSnapshot(order, version);
    }
}

public sealed record OrderSnapshot(OrderEntity Order, uint Version);
