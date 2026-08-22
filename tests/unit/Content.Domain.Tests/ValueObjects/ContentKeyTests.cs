using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.ValueObjects;

public class ContentKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsRequiredField(string? value)
    {
        Action act = () => ContentKey.Create(value!);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_ExceedsMaxLength_ThrowsValueTooLarge()
    {
        var tooLong = new string('a', 101);

        Action act = () => ContentKey.Create(tooLong);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Theory]
    [InlineData("has spaces")]
    [InlineData(".leading-dot")]
    [InlineData("trailing-dot.")]
    [InlineData("Upper Case Not Allowed")]
    public void Create_InvalidFormat_ThrowsInvalidFormat(string value)
    {
        Action act = () => ContentKey.Create(value);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Theory]
    [InlineData("article", "article")]
    [InlineData("News.Bulletin", "news.bulletin")]
    [InlineData("knowledge_article-v2", "knowledge_article-v2")]
    public void Create_ValidKey_NormalizesToLowercase(string input, string expected)
    {
        var result = ContentKey.Create(input);

        result.Value.ShouldBe(expected);
    }

    [Fact]
    public void IsValid_ValidValue_ReturnsTrue()
    {
        ContentKey.IsValid("article").ShouldBeTrue();
    }
}
