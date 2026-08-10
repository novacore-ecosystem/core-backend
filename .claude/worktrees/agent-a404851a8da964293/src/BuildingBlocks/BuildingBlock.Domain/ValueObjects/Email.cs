using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.SharedKernel.RegexPatterns;

namespace NovaCore.BuildingBlock.Domain.ValueObjects;

public sealed class Email : StringValueObject
{
    private Email(string val) : base(val) { }

    public static Email Create(string val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Email is not valid.");

        return new Email(val);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && val.Length <= 100
            && RegexPatterns.Email().IsMatch(val);
}
