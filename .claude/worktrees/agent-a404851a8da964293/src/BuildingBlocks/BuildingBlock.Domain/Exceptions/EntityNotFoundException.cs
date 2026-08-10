using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when a related entity is not found during domain operation.
/// Example: Referenced product not found when creating order item.
/// Different from API NotFoundException (which is application-level).
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string? systemMessage = null)
        : base(MessageCodeEnum.NotFound, systemMessage) { }

    public EntityNotFoundException(string entityName, object id, string? systemMessage = null)
        : base(
            MessageCodeEnum.NotFound,
            systemMessage ?? $"Related {entityName} with id {id} not found")
    {
        EntityName = entityName;
        EntityId = id;
    }

    public string? EntityName { get; }
    public object? EntityId { get; }
}
