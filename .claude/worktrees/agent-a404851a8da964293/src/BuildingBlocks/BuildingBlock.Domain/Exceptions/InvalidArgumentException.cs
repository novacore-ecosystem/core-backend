using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid argument/parameter is provided to a domain operation.
/// Use for domain method validation and business constraints.
/// </summary>
public class InvalidArgumentException : DomainException
{
    public InvalidArgumentException(string? systemMessage = null)
        : base(MessageCodeEnum.InvalidInput, systemMessage) { }

    public InvalidArgumentException(string argumentName, object? value, string? systemMessage = null)
        : base(
            MessageCodeEnum.InvalidInput,
            systemMessage ?? $"Invalid argument: {argumentName} = '{value}'")
    {
        ArgumentName = argumentName;
        Value = value;
    }

    public InvalidArgumentException(MessageCodeEnum messageCode, string? systemMessage = null)
        : base(messageCode, systemMessage) { }

    public string? ArgumentName { get; }
    public object? Value { get; }
}
