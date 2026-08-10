using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation cannot be completed due to insufficient amount/stock/balance.
/// Use for domain-level constraints (inventory, balance, quota, etc.).
/// </summary>
public class InsufficientAmountException : DomainException
{
    public InsufficientAmountException(string? systemMessage = null, object? detail = null)
        : base(MessageCodeEnum.InsufficientStock, systemMessage, detail) { }

    public InsufficientAmountException(
        string resourceName,
        decimal available,
        decimal required,
        string? systemMessage = null)
        : base(
            MessageCodeEnum.InsufficientStock,
            systemMessage ?? $"Insufficient {resourceName}: available {available}, required {required}")
    {
        ResourceName = resourceName;
        AvailableAmount = available;
        RequiredAmount = required;
    }

    public InsufficientAmountException(MessageCodeEnum messageCode, string? systemMessage = null, object? detail = null)
        : base(messageCode, systemMessage, detail) { }

    public string? ResourceName { get; }
    public decimal AvailableAmount { get; }
    public decimal RequiredAmount { get; }
}
