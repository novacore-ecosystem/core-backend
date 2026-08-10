using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Product.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Product.Domain.Tests.ValueObjects;

public class SlugTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsRequiredField(string? value)
    {
        Action act = () => Slug.Create(value!);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_ExceedsMaxLength_ThrowsValueTooLarge()
    {
        var tooLong = new string('a', 201);

        Action act = () => Slug.Create(tooLong);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Theory]
    [InlineData("has spaces")]
    [InlineData("-leading-hyphen")]
    [InlineData("trailing-hyphen-")]
    [InlineData("double--hyphen")]
    [InlineData("under_score")]
    public void Create_NotKebabCase_ThrowsInvalidFormat(string value)
    {
        Action act = () => Slug.Create(value);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_MixedCaseInput_NormalizesToLowercase()
    {
        var result = Slug.Create("My-Product");

        result.Value.ShouldBe("my-product");
    }

    [Fact]
    public void Create_ValidKebabCase_Succeeds()
    {
        var result = Slug.Create("wireless-mouse-2024");

        result.Value.ShouldBe("wireless-mouse-2024");
    }

    [Fact]
    public void TryCreate_InvalidValue_ReturnsFalseWithNullResult()
    {
        var success = Slug.TryCreate("Not Valid", out var result);

        success.ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void IsValid_ValidValue_ReturnsTrue()
    {
        Slug.IsValid("valid-slug").ShouldBeTrue();
    }
}
