using NovaCore.BuildingBlock.Domain.Abstractions;
using Shouldly;

namespace NovaCore.BuildingBlock.Domain.Tests.Abstractions;

file sealed class TestValueObject(string a, int b) : ValueObject
{
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return a;
        yield return b;
    }
}

public class ValueObjectTests
{
    [Fact]
    public void Equals_SameComponents_ReturnsTrue()
    {
        var left = new TestValueObject("x", 1);
        var right = new TestValueObject("x", 1);

        left.Equals(right).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentComponents_ReturnsFalse()
    {
        var left = new TestValueObject("x", 1);
        var right = new TestValueObject("x", 2);

        left.Equals(right).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var left = new TestValueObject("x", 1);

        left.Equals("not a value object").ShouldBeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var left = new TestValueObject("x", 1);

        left.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_SameComponents_ReturnsSameHash()
    {
        var left = new TestValueObject("x", 1);
        var right = new TestValueObject("x", 1);

        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        TestValueObject? left = null;
        TestValueObject? right = null;

        (left == right).ShouldBeTrue();
    }

    [Fact]
    public void EqualityOperator_OneNull_ReturnsFalse()
    {
        var left = new TestValueObject("x", 1);
        TestValueObject? right = null;

        (left == right).ShouldBeFalse();
    }

    [Fact]
    public void InequalityOperator_DifferentComponents_ReturnsTrue()
    {
        var left = new TestValueObject("x", 1);
        var right = new TestValueObject("y", 1);

        (left != right).ShouldBeTrue();
    }
}
