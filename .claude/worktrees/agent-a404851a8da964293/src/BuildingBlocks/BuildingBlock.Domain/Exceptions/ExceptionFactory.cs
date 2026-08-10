using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Domain.Exceptions;

/// <summary>
/// Factory methods for creating domain exceptions.
/// Details must be explicitly passed - nothing is auto-exposed.
/// </summary>
public static class ExceptionFactory
{
    // ==================== Invalid State/Status ====================

    public static InvalidStateException InvalidState(string systemMessage)
        => new(systemMessage);

    public static InvalidStatusException InvalidStatus(string systemMessage)
        => new(systemMessage);

    // ==================== Empty Collections ====================

    public static EmptyCollectionException EmptyCollection(string systemMessage)
        => new(systemMessage);

    public static EmptyItemsException EmptyItems(string systemMessage)
        => new(systemMessage);

    // ==================== Not Found ====================

    public static EntityNotFoundException EntityNotFound(string systemMessage)
        => new(systemMessage);

    public static EntityNotFoundException EntityNotFound<T>(object id)
        => EntityNotFound($"Related {typeof(T).Name} with id {id} not found");

    // ==================== Insufficient Amount ====================

    public static InsufficientAmountException InsufficientStock(string systemMessage, object? detail = null)
        => new(systemMessage, detail);

    public static InsufficientAmountException InsufficientBalance(string systemMessage)
        => new(systemMessage);

    public static InsufficientAmountException InsufficientQuota(string systemMessage)
        => new(systemMessage);

    // ==================== Duplicate/Conflict ====================

    public static BusinessRuleException Duplicate(string systemMessage)
        => new(systemMessage);

    public static BusinessRuleException UniqueConstraintViolation(string systemMessage)
        => new(systemMessage);

    // ==================== Invalid Value ====================

    public static InvalidArgumentException InvalidEnumValue(string systemMessage)
        => new(systemMessage);

    public static InvalidArgumentException InvalidRange(string systemMessage)
        => new(systemMessage);

    public static InvalidArgumentException ValueTooSmall(string systemMessage)
        => new(systemMessage);

    public static InvalidArgumentException ValueTooLarge(string systemMessage)
        => new(systemMessage);

    public static InvalidArgumentException InvalidFormat(string systemMessage)
        => new(systemMessage);

    // ==================== Required Field ====================

    public static InvalidArgumentException RequiredField(string systemMessage)
        => new(systemMessage);

    public static InvalidArgumentException RequiredNotEmpty(string systemMessage)
        => new(systemMessage);
}
