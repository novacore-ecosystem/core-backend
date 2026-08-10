using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Product.Domain.Tests.ValueObjects;

/// <summary>
/// Shared contract test for NovaCore.Product.Domain's uppercase business-code Value Objects
/// (<c>Sku</c>, <c>ProductCode</c>, <c>CategoryCode</c>, <c>TagCode</c>) - all four have the
/// identical validation shape: required, max 50 chars, <c>^[A-Z0-9-]+$</c> format, normalized
/// via <c>Trim().ToUpperInvariant()</c>. One shared test base instead of four near-duplicate
/// test classes; each concrete class only wires up the three static factory calls.
/// </summary>
public abstract class UppercaseCodeValueObjectTests<T> where T : StringValueObject
{
    private const int MaxLength = 50;

    protected abstract T Create(string value);
    protected abstract bool TryCreate(string? value, out T? result);
    protected abstract bool IsValid(string? value);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ThrowsRequiredField(string? value)
    {
        Action act = () => Create(value!);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_ExceedsMaxLength_ThrowsValueTooLarge()
    {
        var tooLong = new string('A', MaxLength + 1);

        Action act = () => Create(tooLong);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_AtMaxLength_Succeeds()
    {
        var exactLength = new string('A', MaxLength);

        var result = Create(exactLength);

        result.Value.ShouldBe(exactLength);
    }

    [Theory]
    [InlineData("abc def")]
    [InlineData("abc_def")]
    [InlineData("abc.def")]
    [InlineData("abc/def")]
    public void Create_InvalidCharacters_ThrowsInvalidFormat(string value)
    {
        Action act = () => Create(value);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_LowercaseInput_NormalizesToUppercase()
    {
        var result = Create("abc-123");

        result.Value.ShouldBe("ABC-123");
    }

    [Fact]
    public void Create_ValueWithSurroundingWhitespace_Trims()
    {
        var result = Create("  ABC-123  ");

        result.Value.ShouldBe("ABC-123");
    }

    [Fact]
    public void TryCreate_ValidValue_ReturnsTrueWithResult()
    {
        var success = TryCreate("ABC-123", out var result);

        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        result!.Value.ShouldBe("ABC-123");
    }

    [Fact]
    public void TryCreate_InvalidValue_ReturnsFalseWithNullResult()
    {
        var success = TryCreate("", out var result);

        success.ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void IsValid_ValidValue_ReturnsTrue()
    {
        IsValid("ABC-123").ShouldBeTrue();
    }

    [Fact]
    public void IsValid_NullValue_ReturnsFalse()
    {
        IsValid(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_SameValueDifferentCasing_AreEqualAfterNormalization()
    {
        var left = Create("abc-123");
        var right = Create("ABC-123");

        left.ShouldBe(right);
    }
}
