using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is in an invalid state for the requested operation.
/// Example: Cannot cancel an order that is already completed.
/// </summary>
public class InvalidStateException : DomainException
{
    public InvalidStateException(string systemMessage)
        : base(MessageCodeEnum.InvalidInput, systemMessage) { }
}
