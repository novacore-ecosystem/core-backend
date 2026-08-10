using NovaCore.BuildingBlock.Domain.Abstractions;
using Shouldly;

namespace NovaCore.BuildingBlock.Domain.Tests.Abstractions;

file sealed class TestStringValueObject(string value) : StringValueObject(value);

public class StringValueObjectTests
{
    [Fact]
    public void Value_ReturnsConstructedValue()
    {
        var vo = new TestStringValueObject("abc");

        vo.Value.ShouldBe("abc");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var vo = new TestStringValueObject("abc");

        vo.ToString().ShouldBe("abc");
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var left = new TestStringValueObject("abc");
        var right = new TestStringValueObject("abc");

        left.ShouldBe(right);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var left = new TestStringValueObject("abc");
        var right = new TestStringValueObject("xyz");

        left.ShouldNotBe(right);
    }
}
