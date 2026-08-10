using NovaCore.BuildingBlock.Domain.Attributes;
using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Base class for domain layer exceptions.
/// Domain exceptions represent business rule violations and are caught at the application layer
/// and converted to ApplicationException for HTTP responses.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(
        MessageCodeEnum messageCode,
        string? systemMessage = null,
        object? detail = null)
        : base(systemMessage ?? MessageCodeAttribute.GetMessage(messageCode))
    {
        MessageCode = messageCode;
        SystemMessage = systemMessage;
        ErrorDetails = detail;
    }

    /// <summary>
    /// Message code for client response (enum value for translation).
    /// </summary>
    public MessageCodeEnum MessageCode { get; }

    /// <summary>
    /// System message for server logging (detailed error info).
    /// </summary>
    public string? SystemMessage { get; }

    public object? ErrorDetails { get; }
}
