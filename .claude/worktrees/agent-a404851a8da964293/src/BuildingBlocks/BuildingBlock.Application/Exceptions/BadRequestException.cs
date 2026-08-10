using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Exceptions;

public class BadRequestException : ApplicationException
{
    public BadRequestException(string? systemMessage = null)
        : base(MessageCodeEnum.BadRequest, systemMessage, statusCode: 400) { }

    public BadRequestException(MessageCodeEnum messageCode, string? systemMessage = null)
        : base(messageCode, systemMessage, statusCode: 400) { }
}
