using System.Reflection;

using NovaCore.BuildingBlock.Domain.Enums;

namespace NovaCore.BuildingBlock.Domain.Attributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class MessageCodeAttribute(string code, string message) : Attribute
{
    public string Code { get; } = code;
    public string Message { get; } = message;

    public override string ToString() => $"{Code}: {Message}";

    public static string GetMessage(MessageCode messageCode)
    {
        var fieldInfo = typeof(MessageCode).GetField(messageCode.ToString());
        var attribute = fieldInfo?.GetCustomAttribute<MessageCodeAttribute>();
        return attribute?.Message ?? messageCode.ToString();
    }

    public static string GetCode(MessageCode messageCode)
    {
        var fieldInfo = typeof(MessageCode).GetField(messageCode.ToString());
        var attribute = fieldInfo?.GetCustomAttribute<MessageCodeAttribute>();
        return attribute?.Code ?? string.Empty;
    }
}
