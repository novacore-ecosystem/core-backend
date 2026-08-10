using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Tests.ValueObjects;

public class CategoryCodeTests : UppercaseCodeValueObjectTests<CategoryCode>
{
    protected override CategoryCode Create(string value) => CategoryCode.Create(value);
    protected override bool TryCreate(string? value, out CategoryCode? result) => CategoryCode.TryCreate(value, out result);
    protected override bool IsValid(string? value) => CategoryCode.IsValid(value);
}
