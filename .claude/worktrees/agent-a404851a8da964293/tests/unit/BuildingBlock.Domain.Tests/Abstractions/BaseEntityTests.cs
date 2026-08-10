using NovaCore.BuildingBlock.Domain.Abstractions;
using Shouldly;

namespace NovaCore.BuildingBlock.Domain.Tests.Abstractions;

file sealed class TestAggregate : AggregateRoot<Guid>;

public class BaseEntityTests
{
    // Method is named "Tourch" (typo) in NovaCore.BuildingBlock.Domain.Abstractions.BaseEntity<T> - test
    // matches the actual production API rather than the intended spelling.
    [Fact]
    public void Tourch_UpdatesUpdatedAt_ButNotCreatedAt()
    {
        var entity = new TestAggregate();
        var createdAt = entity.CreatedAt;
        var updatedAtBeforeTouch = entity.UpdatedAt;

        Thread.Sleep(5);
        entity.Touch();

        entity.CreatedAt.ShouldBe(createdAt);
        entity.UpdatedAt.ShouldBeGreaterThan(updatedAtBeforeTouch);
    }
}
