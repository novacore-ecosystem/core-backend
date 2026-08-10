using NovaCore.BuildingBlock.SharedKernel.Extensions;
using Shouldly;

namespace NovaCore.BuildingBlock.SharedKernel.Tests.Extensions;

public class StringExtensionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsNullOrWhiteSpace_NullEmptyOrWhitespace_ReturnsTrue(string? input)
    {
        input.IsNullOrWhiteSpace().ShouldBeTrue();
    }

    [Fact]
    public void IsNullOrWhiteSpace_NonBlankString_ReturnsFalse()
    {
        "value".IsNullOrWhiteSpace().ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsNotNullOrWhiteSpace_NullEmptyOrWhitespace_ReturnsFalse(string? input)
    {
        input.IsNotNullOrWhiteSpace().ShouldBeFalse();
    }

    [Fact]
    public void IsNotNullOrWhiteSpace_NonBlankString_ReturnsTrue()
    {
        "value".IsNotNullOrWhiteSpace().ShouldBeTrue();
    }

    [Fact]
    public void ToStringOrEmpty_NullString_ReturnsEmpty()
    {
        string? input = null;

        input.ToStringOrEmpty().ShouldBeEmpty();
    }

    [Fact]
    public void ToStringOrEmpty_NonNullString_ReturnsSameValue()
    {
        "value".ToStringOrEmpty().ShouldBe("value");
    }

    [Fact]
    public void GetFileName_GenericType_ReturnsTypeName()
    {
        42.GetFileName().ShouldBe(nameof(Int32));
    }

    [Fact]
    public void GetUpperName_GenericType_ReturnsUppercaseTypeName()
    {
        42.GetUpperName().ShouldBe(nameof(Int32).ToUpperInvariant());
    }

    [Fact]
    public void GetLowerName_GenericType_ReturnsLowercaseTypeName()
    {
        42.GetLowerName().ShouldBe(nameof(Int32).ToLowerInvariant());
    }
}
