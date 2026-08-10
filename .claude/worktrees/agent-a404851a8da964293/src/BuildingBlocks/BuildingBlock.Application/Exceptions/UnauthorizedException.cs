using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Exceptions;

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string? systemMessage = null)
        : base(MessageCodeEnum.Unauthorized, systemMessage, statusCode: 401) { }

    public UnauthorizedException(MessageCodeEnum messageCode, string? systemMessage = null)
        : base(messageCode, systemMessage, statusCode: 401) { }
}
