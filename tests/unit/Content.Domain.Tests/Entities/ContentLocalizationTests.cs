using NovaCore.Content.Domain.Entities.Contents;
using Shouldly;

namespace NovaCore.Content.Domain.Tests.Entities;

public class ContentLocalizationTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"time\":123,\"blocks\":[{\"type\":\"paragraph\",\"data\":{\"text\":\"hi\"}}]}")]
    [InlineData("[]")]
    public void IsValidBody_SyntacticallyValidJson_ReturnsTrue(string body)
    {
        ContentLocalization.IsValidBody(body).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{unterminated")]
    public void IsValidBody_InvalidJson_ReturnsFalse(string? body)
    {
        ContentLocalization.IsValidBody(body).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidTitle_BlankTitle_ReturnsFalse(string? title)
    {
        ContentLocalization.IsValidTitle(title).ShouldBeFalse();
    }

    [Fact]
    public void IsValidTitle_NonBlankTitle_ReturnsTrue()
    {
        ContentLocalization.IsValidTitle("Title").ShouldBeTrue();
    }
}
