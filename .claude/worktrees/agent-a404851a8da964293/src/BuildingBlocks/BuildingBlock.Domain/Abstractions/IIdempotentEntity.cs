namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-in capability for entities that carry a client-supplied dedupe key (a retried write should
/// resolve to the same row instead of creating a duplicate). Implementing this interface makes
/// the Entity Convention map IdempotencyKey and index it automatically. Uniqueness scope varies
/// per entity (globally unique, or unique within some other column) and stays a per-entity
/// IEntityTypeConfiguration concern - the convention only guarantees the column exists and is
/// indexed for lookup, it never assumes a uniqueness shape.
/// </summary>
public interface IIdempotentEntity
{
    string? IdempotencyKey { get; }
}
