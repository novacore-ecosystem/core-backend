using NovaCore.BuildingBlock.SharedKernel.Extensions;
using Shouldly;

namespace NovaCore.BuildingBlock.SharedKernel.Tests.Extensions;

public class ArrayExtensionTests
{
    [Fact]
    public void JoinToString_DefaultSeparator_ConcatenatesWithoutSeparator()
    {
        string[] items = ["a", "b", "c"];

        var result = items.JoinToString();

        result.ShouldBe("abc");
    }

    [Fact]
    public void JoinToString_CustomSeparator_JoinsWithSeparator()
    {
        string[] items = ["a", "b", "c"];

        var result = items.JoinToString(", ");

        result.ShouldBe("a, b, c");
    }

    [Fact]
    public void JoinToString_EmptyList_ReturnsEmptyString()
    {
        var result = Array.Empty<string>().JoinToString(", ");

        result.ShouldBeEmpty();
    }

    [Fact]
    public void JoinToString_SingleItem_ReturnsItemUnchanged()
    {
        string[] items = ["only"];

        var result = items.JoinToString(", ");

        result.ShouldBe("only");
    }
}
