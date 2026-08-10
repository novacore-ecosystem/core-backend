using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Extensions;
using Shouldly;

namespace NovaCore.BuildingBlock.Domain.Tests.Extensions;

public class MessageCodeExtensionTests
{
    [Fact]
    public void ToMessage_KnownCode_ReturnsAttributeMessage()
    {
        MessageCode.Success.ToMessage().ShouldBe("Request success");
    }

    [Fact]
    public void ToStringCode_KnownCode_ReturnsAttributeCode()
    {
        MessageCode.Success.ToStringCode().ShouldBe("001");
    }

    [Fact]
    public void ToValue_AnyCode_ReturnsUnderlyingIntAsString()
    {
        MessageCode.Success.ToValue().ShouldBe(((int)MessageCode.Success).ToString());
    }
}
