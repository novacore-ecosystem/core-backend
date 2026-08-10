using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.Enums;

namespace NovaCore.BuildingBlock.Domain.Extensions;

public static class MessageCodeExtension
{
    public static string ToMessage(this MessageCode code)
        => MessageCodeAttribute.GetMessage(code);

    public static string ToStringCode(this MessageCode code)
        => MessageCodeAttribute.GetCode(code);

    public static string ToValue(this MessageCode code)
        => ((int)code).ToString();
}
