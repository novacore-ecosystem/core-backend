using System.Text.Json;

using NovaCore.BuildingBlock.Contract.Events.Audit;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

/// <summary>End-to-end through a real DbContext + AuditInterceptor + Outbox, InMemory-provider-backed.</summary>
public sealed class AuditInterceptorTests
{
    [Fact]
    public async Task SaveChanges_ProducesExactlyOneOutboxMessagePerAggregateRoot()
    {
        var registry = AuditTestFixture.BuildRegistry();
        await using var context = AuditTestFixture.CreateContext(registry);

        var order1 = new TestOrder { Id = Guid.NewGuid() };
        var order1Item1 = new TestOrderItem { Id = Guid.NewGuid(), OrderId = order1.Id, ProductName = "Widget" };
        var order1Item2 = new TestOrderItem { Id = Guid.NewGuid(), OrderId = order1.Id, ProductName = "Gadget" };

        var order2 = new TestOrder { Id = Guid.NewGuid() };
        var order2Item1 = new TestOrderItem { Id = Guid.NewGuid(), OrderId = order2.Id, ProductName = "Gizmo" };

        var user = new TestUser { Id = Guid.NewGuid(), Email = "a@b.com" };

        context.AddRange(order1, order1Item1, order1Item2, order2, order2Item1, user);
        await context.SaveChangesAsync();

        var outboxMessages = await context.OutboxMessages.ToListAsync();

        Assert.Equal(3, outboxMessages.Count);
        Assert.All(outboxMessages, m => Assert.Equal(nameof(AuditIntegrationEvent), m.EventType));

        var events = outboxMessages
            .Select(m => JsonSerializer.Deserialize<AuditIntegrationEvent>(m.Payload)!)
            .ToList();

        var order1Event = events.Single(e => e.RootEntityId == order1.Id.ToString());
        Assert.Equal(nameof(TestOrder), order1Event.RootEntityType);
        Assert.Equal(2, order1Event.Root.Children.Count);

        var order2Event = events.Single(e => e.RootEntityId == order2.Id.ToString());
        Assert.Single(order2Event.Root.Children);

        var userEvent = events.Single(e => e.RootEntityId == user.Id.ToString());
        Assert.Equal(nameof(TestUser), userEvent.RootEntityType);
        Assert.Empty(userEvent.Root.Children);
    }

    [Fact]
    public async Task SaveChanges_SkipsEvent_WhenOnlyAuditIgnoredPropertyChanged()
    {
        var registry = AuditTestFixture.BuildRegistry();
        await using var context = AuditTestFixture.CreateContext(registry);

        var user = new TestUser { Id = Guid.NewGuid(), Email = "a@b.com" };
        context.Add(user);
        await context.SaveChangesAsync();

        user.Touch(); // touches only [AuditIgnore] UpdatedAt
        await context.SaveChangesAsync();

        var outboxMessages = await context.OutboxMessages.ToListAsync();

        Assert.Single(outboxMessages); // only the Created event from the first save
    }

    [Fact]
    public async Task SaveChanges_PreservesFullAncestorPath_WhenOnlyADeepDescendantChanges()
    {
        var registry = AuditTestFixture.BuildRegistry();
        await using var context = AuditTestFixture.CreateContext(registry);

        var order = new TestOrder { Id = Guid.NewGuid() };
        var item = new TestOrderItem { Id = Guid.NewGuid(), OrderId = order.Id, ProductName = "Widget" };
        var tax = new TestOrderTax { Id = Guid.NewGuid(), OrderItemId = item.Id, Amount = 1.0m };

        context.AddRange(order, item, tax);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Simulates a repository loading the whole aggregate before mutating one deep entity -
        // Order and OrderItem are tracked (Unchanged) purely so the path back to the root resolves.
        var trackedOrder = await context.Orders.SingleAsync(o => o.Id == order.Id);
        var trackedItem = await context.OrderItems.SingleAsync(i => i.Id == item.Id);
        var trackedTax = await context.OrderTaxes.SingleAsync(t => t.Id == tax.Id);
        trackedTax.Amount = 2.5m;

        await context.SaveChangesAsync();

        var outboxMessages = await context.OutboxMessages.OrderBy(m => m.CreatedAt).ToListAsync();
        Assert.Equal(2, outboxMessages.Count); // [0] = initial Created graph, [1] = this Updated graph

        var updateEvent = JsonSerializer.Deserialize<AuditIntegrationEvent>(outboxMessages[1].Payload)!;
        Assert.Equal(nameof(TestOrder), updateEvent.RootEntityType);
        Assert.Equal(order.Id.ToString(), updateEvent.RootEntityId);

        // Root -> child -> grandchild
        var rootNode = updateEvent.Root;
        Assert.Empty(rootNode.Changes); // Order itself untouched - structural pass-through
        var itemNode = Assert.Single(rootNode.Children);
        Assert.Equal(nameof(TestOrderItem), itemNode.EntityType);
        Assert.Empty(itemNode.Changes); // untouched
        var taxNode = Assert.Single(itemNode.Children);
        Assert.Equal(nameof(TestOrderTax), taxNode.EntityType);
        Assert.NotEmpty(taxNode.Changes); // the actual change

        // Grandchild -> child -> root, via ParentNodeId - never losing the hierarchy
        Assert.Equal(itemNode.NodeId, taxNode.ParentNodeId);
        Assert.Equal(rootNode.NodeId, itemNode.ParentNodeId);
        Assert.Null(rootNode.ParentNodeId);
        Assert.Equal(0, rootNode.Depth);
        Assert.Equal(1, itemNode.Depth);
        Assert.Equal(2, taxNode.Depth);
    }
}
