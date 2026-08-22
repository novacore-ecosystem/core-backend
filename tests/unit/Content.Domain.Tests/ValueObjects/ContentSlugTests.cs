using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.ValueObjects;

public class ContentSlugTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsRequiredField(string? value)
    {
        Action act = () => ContentSlug.Create(value!);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_ExceedsMaxLength_ThrowsValueTooLarge()
    {
        var tooLong = new string('a', 201);

        Action act = () => ContentSlug.Create(tooLong);

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
        Action act = () => ContentSlug.Create(value);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_MixedCaseInput_NormalizesToLowercase()
    {
        var result = ContentSlug.Create("My-Article");

        result.Value.ShouldBe("my-article");
    }

    [Fact]
    public void Create_ValidKebabCase_Succeeds()
    {
        var result = ContentSlug.Create("announcing-nova-core-2026");

        result.Value.ShouldBe("announcing-nova-core-2026");
    }

    [Fact]
    public void TryCreate_InvalidValue_ReturnsFalseWithNullResult()
    {
        var success = ContentSlug.TryCreate("Not Valid", out var result);

        success.ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void IsValid_ValidValue_ReturnsTrue()
    {
        ContentSlug.IsValid("valid-slug").ShouldBeTrue();
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var first = ContentSlug.Create("same-slug");
        var second = ContentSlug.Create("same-slug");

        first.ShouldBe(second);
    }
}
