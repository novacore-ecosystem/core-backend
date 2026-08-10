using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Product.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Product.Domain.Tests.ValueObjects;

public class DimensionsTests
{
    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(-1, 1, 1)]
    public void Create_AnyDimensionNotGreaterThanZero_ThrowsInvalidRange(decimal length, decimal width, decimal height)
    {
        Action act = () => Dimensions.Create(length, width, height);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_AllPositive_Succeeds()
    {
        var result = Dimensions.Create(10, 20, 30);

        result.Length.ShouldBe(10);
        result.Width.ShouldBe(20);
        result.Height.ShouldBe(30);
    }

    [Fact]
    public void Equals_SameDimensions_AreEqual()
    {
        var left = Dimensions.Create(10, 20, 30);
        var right = Dimensions.Create(10, 20, 30);

        left.ShouldBe(right);
    }

    [Fact]
    public void Equals_DifferentDimensions_AreNotEqual()
    {
        var left = Dimensions.Create(10, 20, 30);
        var right = Dimensions.Create(10, 20, 31);

        left.ShouldNotBe(right);
    }

    [Fact]
    public void TryCreate_InvalidDimensions_ReturnsFalseWithNullResult()
    {
        var success = Dimensions.TryCreate(0, 1, 1, out var result);

        success.ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void IsValid_AllPositive_ReturnsTrue()
    {
        Dimensions.IsValid(10, 20, 30).ShouldBeTrue();
    }
}
