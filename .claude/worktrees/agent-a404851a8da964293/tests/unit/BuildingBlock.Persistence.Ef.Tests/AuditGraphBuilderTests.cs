using NovaCore.BuildingBlock.Contract.Events.Audit;
using NovaCore.BuildingBlock.Persistence.Audit;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

/// <summary>Exercises the provider-agnostic grouping/tree algorithm directly - no EF involved - against the exact scenario from the audit-graph requirements: two Orders (one with two items, one with one item) plus an unrelated User in a single batch.</summary>
public sealed class AuditGraphBuilderTests
{
    [Fact]
    public void Build_GroupsByAggregateRoot_NeverMergingDifferentRoots()
    {
        var registry = AuditTestFixture.BuildRegistry();
        var change = new AuditFieldChange("Status", "Pending", "Confirmed");

        AuditTrackedEntity[] entities =
        [
            new(typeof(TestOrder), "order-1", null, true, AuditAction.Updated, [change]),
            new(typeof(TestOrderItem), "item-1a", "order-1", true, AuditAction.Created, [change]),
            new(typeof(TestOrderItem), "item-1b", "order-1", true, AuditAction.Created, [change]),
            new(typeof(TestOrder), "order-2", null, true, AuditAction.Created, [change]),
            new(typeof(TestOrderItem), "item-2a", "order-2", true, AuditAction.Created, [change]),
            new(typeof(TestUser), "user-10", null, true, AuditAction.Updated, [change]),
        ];

        var graphs = AuditGraphBuilder.Build(entities, registry);

        Assert.Equal(3, graphs.Count);

        var order1 = graphs.Single(g => g.RootEntityId == "order-1");
        Assert.Equal(2, order1.Root.Children.Count);
        Assert.All(order1.Root.Children, c => Assert.Equal(order1.Root.NodeId, c.ParentNodeId));

        var order2 = graphs.Single(g => g.RootEntityId == "order-2");
        Assert.Single(order2.Root.Children);

        var user = graphs.Single(g => g.RootEntityId == "user-10");
        Assert.Empty(user.Root.Children);
    }

    [Fact]
    public void Build_ReturnsNothing_WhenNoEntityHasChanges()
    {
        var registry = AuditTestFixture.BuildRegistry();

        AuditTrackedEntity[] entities =
        [
            new(typeof(TestOrder), "order-1", null, false, AuditAction.Updated, []),
            new(typeof(TestOrderItem), "item-1a", "order-1", false, AuditAction.Updated, []),
        ];

        var graphs = AuditGraphBuilder.Build(entities, registry);

        Assert.Empty(graphs);
    }
}
