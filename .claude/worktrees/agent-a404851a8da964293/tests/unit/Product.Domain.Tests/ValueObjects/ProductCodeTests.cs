using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Tests.ValueObjects;

public class ProductCodeTests : UppercaseCodeValueObjectTests<ProductCode>
{
    protected override ProductCode Create(string value) => ProductCode.Create(value);
    protected override bool TryCreate(string? value, out ProductCode? result) => ProductCode.TryCreate(value, out result);
    protected override bool IsValid(string? value) => ProductCode.IsValid(value);
}
