using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Exceptions;

public class NotFoundException : ApplicationException
{
    public NotFoundException(string? systemMessage = null)
        : base(MessageCodeEnum.NotFound, systemMessage, statusCode: 404)
    {
    }

    /// <summary>
    /// Create not found exception with entity name and value.
    /// Example: new NotFoundException("Product", 123) -> "The Product (123) is not found."
    /// </summary>
    public NotFoundException(string entityName, object value, string? systemMessage = null)
        : base(
            MessageCodeEnum.NotFound,
            systemMessage ?? $"The {entityName} ({value}) is not found.",
            statusCode: 404)
    {
        EntityName = entityName;
        Value = value;
    }

    public string? EntityName { get; }
    public object? Value { get; }
}
