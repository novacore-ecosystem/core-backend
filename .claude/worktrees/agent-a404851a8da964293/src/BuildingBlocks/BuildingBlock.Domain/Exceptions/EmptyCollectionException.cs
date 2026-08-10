using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when a collection/list is empty but should contain items.
/// Example: OrderItems collection cannot be empty.
/// </summary>
public class EmptyCollectionException : DomainException
{
    public EmptyCollectionException(string systemMessage)
        : base(MessageCodeEnum.InvalidInput, systemMessage) { }
}
