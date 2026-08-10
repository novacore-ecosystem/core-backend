namespace NovaCore.BuildingBlock.Domain.Abstractions;

public abstract class AggregateRoot<TId> : BaseEntity<TId>
{
}

/// <summary>
/// Base class for aggregate roots without specific ID type.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
}
