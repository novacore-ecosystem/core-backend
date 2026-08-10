using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Product.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Product.Domain.Tests.ValueObjects;

public class BarcodeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsRequiredField(string? value)
    {
        Action act = () => Barcode.Create(value!);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Theory]
    [InlineData("1234567")]      // 7 digits - below the 8-digit minimum
    [InlineData("123456789012345")] // 15 digits - above the 14-digit maximum
    [InlineData("1234567A")]     // contains a non-digit
    public void Create_InvalidFormat_ThrowsInvalidFormat(string value)
    {
        Action act = () => Barcode.Create(value);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Theory]
    [InlineData("12345678")]         // 8 digits - EAN-8 minimum
    [InlineData("12345678901234")]   // 14 digits - GTIN-14 maximum
    public void Create_ValidLength_Succeeds(string value)
    {
        var result = Barcode.Create(value);

        result.Value.ShouldBe(value);
    }

    [Fact]
    public void Create_SurroundingWhitespace_Trims()
    {
        var result = Barcode.Create("  12345678  ");

        result.Value.ShouldBe("12345678");
    }

    [Fact]
    public void TryCreate_InvalidValue_ReturnsFalseWithNullResult()
    {
        var success = Barcode.TryCreate("abc", out var result);

        success.ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void IsValid_ValidValue_ReturnsTrue()
    {
        Barcode.IsValid("12345678").ShouldBeTrue();
    }
}
