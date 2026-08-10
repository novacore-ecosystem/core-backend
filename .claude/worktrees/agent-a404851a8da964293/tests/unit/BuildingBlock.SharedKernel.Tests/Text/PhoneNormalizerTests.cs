using NovaCore.BuildingBlock.SharedKernel.Text;

using Shouldly;

namespace NovaCore.BuildingBlock.SharedKernel.Tests.Text;

public class PhoneNormalizerTests
{
    [Theory]
    [InlineData("0901234567", "0901234567")]
    [InlineData("(090) 123-4567", "0901234567")]
    [InlineData("+84 90 123 4567", "84901234567")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Normalize_StripsNonDigits(string? input, string expected)
    {
        PhoneNormalizer.Normalize(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("0901234567", "7654321090")]
    [InlineData("", "")]
    public void Reverse_ReversesDigitString(string input, string expected)
    {
        PhoneNormalizer.Reverse(input).ShouldBe(expected);
    }
}
