using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Exceptions;

public class ConflictException : ApplicationException
{
    public ConflictException(string? systemMessage = null, object? detail = null)
        : base(MessageCodeEnum.Conflict, systemMessage, statusCode: 409, detail) { }

    public ConflictException(MessageCodeEnum messageCode, string? systemMessage = null, object? detail = null)
        : base(messageCode, systemMessage, statusCode: 409, detail) { }
}
