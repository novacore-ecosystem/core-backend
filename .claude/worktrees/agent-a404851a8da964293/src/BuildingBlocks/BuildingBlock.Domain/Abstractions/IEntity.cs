namespace NovaCore.BuildingBlock.Domain.Abstractions;

public interface IEntity<T> : IEntity
{
    T Id { get; init; }
}

public interface IEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }

    void Track();

    void Touch();
}
