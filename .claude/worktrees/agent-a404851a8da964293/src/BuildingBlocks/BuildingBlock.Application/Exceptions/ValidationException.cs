using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;

namespace NovaCore.BuildingBlock.Application.Exceptions;

public sealed class ValidationException(
    List<ValidationError> validationErrors,
    string? systemMessage = null) : ApplicationException(
        MessageCodeEnum.ValidationFailed,
        systemMessage ?? BuildErrorMessage(validationErrors!),
        statusCode: 400)
{
    public ValidationException(
        string propertyName,
        string errorMessage,
        string? systemMessage = null)
        : this([new(propertyName, errorMessage)], systemMessage)
    {
    }

    /// <summary>
    /// List of validation errors (for logging/debugging).
    /// </summary>
    public List<ValidationError> ValidationErrors { get; } = validationErrors ?? [];

    private static string BuildErrorMessage(List<ValidationError> errors)
    {
        var messages = errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
        return string.Join("; ", messages);
    }
}

public class ValidationError(string propertyName, string errorMessage)
{
    public string PropertyName { get; } = propertyName;
    public string ErrorMessage { get; } = errorMessage;
}
