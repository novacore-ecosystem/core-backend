using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity requires items but none are provided.
/// Example: Order must have at least one item.
/// </summary>
public class EmptyItemsException : DomainException
{
    public EmptyItemsException(string systemMessage)
        : base(MessageCodeEnum.InvalidInput, systemMessage) { }
}
