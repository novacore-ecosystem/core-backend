using NovaCore.BuildingBlock.Domain.Attributes;
using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Exceptions;

public abstract class ApplicationException : Exception
{
    protected ApplicationException(
        MessageCodeEnum messageCode,
        string? systemMessage = null,
        int statusCode = 400,
        object? errorDetails = null)
        : base(systemMessage ?? MessageCodeAttribute.GetMessage(messageCode))
    {
        MessageCode = messageCode;
        SystemMessage = systemMessage;
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
    }

    /// <summary>
    /// Message code for client response (enum value for translation).
    /// </summary>
    public MessageCodeEnum MessageCode { get; }

    /// <summary>
    /// System message for server logging (detailed error info).
    /// </summary>
    public string? SystemMessage { get; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; }

    public object? ErrorDetails { get; }
}
