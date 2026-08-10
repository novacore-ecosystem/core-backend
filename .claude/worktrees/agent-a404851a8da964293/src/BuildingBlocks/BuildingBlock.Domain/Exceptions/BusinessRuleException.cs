using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// Use for domain-level business logic violations that should not proceed.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string? systemMessage = null)
        : base(MessageCodeEnum.BadRequest, systemMessage) { }

    public BusinessRuleException(string ruleName, string? systemMessage = null)
        : base(
            MessageCodeEnum.BadRequest,
            systemMessage ?? $"Business rule violation: {ruleName}")
    {
        RuleName = ruleName;
    }

    public BusinessRuleException(MessageCodeEnum messageCode, string? systemMessage = null)
        : base(messageCode, systemMessage) { }

    public string? RuleName { get; }
}
