using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.BuildingBlock.Domain.ValueObjects;

public sealed class LanguageCode : StringValueObject
{
    private LanguageCode(string val) : base(val) { }

    public static LanguageCode Create(string val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Language code is not valid.");

        return new LanguageCode(val);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && LanguageCodeConstant.SupportedLanguages.Contains(val);
}
