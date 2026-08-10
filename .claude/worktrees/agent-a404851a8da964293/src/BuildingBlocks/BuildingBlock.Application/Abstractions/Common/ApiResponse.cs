using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.Extensions;
using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Abstractions.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageCode { get; set; }
    public T? Data { get; set; }
    public object? Details { get; set; }

    public static ApiResponse<T> Ok(T data)
    {
        return CreateSuccess(data, MessageCodeEnum.Success);
    }
    public static ApiResponse<T> Ok(T data, MessageCodeEnum code)
    {
        return CreateSuccess(data, code);
    }
    public static ApiResponse<T> Ok()
    {
        return CreateSuccess(default!, MessageCodeEnum.Success);
    }
    public static ApiResponse<T> Ok(MessageCodeEnum code)
    {
        return CreateSuccess(default!, code);
    }

    public static ApiResponse<T> Accepted()
    {
        return CreateSuccess(default!, MessageCodeEnum.Accepted);
    }

    public static ApiResponse<T> Accepted(T data)
    {
        return CreateSuccess(data, MessageCodeEnum.Accepted);
    }

    public static ApiResponse<T> NoContent()
    {
        return CreateSuccess(default!, MessageCodeEnum.NoContent);
    }

    public static ApiResponse<T> Fail(
        string message,
        MessageCodeEnum code,
        object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            MessageCode = code.ToStringCode(),
            Data = default,
            Details = details
        };
    }

    private static ApiResponse<T> CreateSuccess(T data, MessageCodeEnum code)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = MessageCodeAttribute.GetMessage(code),
            MessageCode = code.ToStringCode(),
            Data = data
        };
    }
}
