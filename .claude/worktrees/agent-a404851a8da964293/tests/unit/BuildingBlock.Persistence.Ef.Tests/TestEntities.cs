using NovaCore.BuildingBlock.Domain.Abstractions;

namespace NovaCore.BuildingBlock.Persistence.Ef.Tests;

public sealed class TestOrder : BaseEntity<Guid>, IAuditable
{
    public string Status { get; set; } = "Pending";
}

public sealed class TestOrderItem : BaseEntity<Guid>, IAuditable
{
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
}

public sealed class TestOrderTax : BaseEntity<Guid>, IAuditable
{
    public Guid OrderItemId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class TestUser : BaseEntity<Guid>, IAuditable
{
    public string Email { get; set; } = string.Empty;
}
