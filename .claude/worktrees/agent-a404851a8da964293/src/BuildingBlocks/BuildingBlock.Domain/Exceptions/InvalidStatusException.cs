using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity status is invalid or cannot be changed to the requested status.
/// Example: Cannot change order status from "completed" to "pending".
/// </summary>
public class InvalidStatusException : DomainException
{
    public InvalidStatusException(string systemMessage)
        : base(MessageCodeEnum.InvalidInput, systemMessage) { }
}
