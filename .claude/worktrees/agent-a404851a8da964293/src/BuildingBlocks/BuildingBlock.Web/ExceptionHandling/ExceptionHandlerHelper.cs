using System.Diagnostics;
using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Domain.Extensions;
using AppException = NovaCore.BuildingBlock.Application.Exceptions.ApplicationException;
using MessageCodeEnum = NovaCore.BuildingBlock.Domain.Enums.MessageCode;
using DomainEx = NovaCore.BuildingBlock.Domain.Exceptions.DomainException;

namespace NovaCore.BuildingBlock.Web.ExceptionHandling;

/// <summary>
/// Shared exception handling utility for all services.
/// Handles both Application and Domain layer exceptions with comprehensive error mapping.
///
/// Supported Exceptions:
///
/// APPLICATION LAYER (HTTP-aware, include status codes):
///   - UnauthorizedException (401) - Authentication failures
///   - ForbiddenException (403) - Authorization failures
///   - NotFoundException (404) - Resource not found
///   - BadRequestException (400) - Invalid input/validation
///   - ConflictException (409) - Duplicate/state conflict
///   - ValidationException (400) - Field-level validation
///   - ApplicationException (500) - Base exception with custom status
///
/// DOMAIN LAYER (business logic, mapped to HTTP codes):
///   - EntityNotFoundException → 404
///   - InvalidArgumentException → 400
///   - InvalidStateException → 400
///   - InvalidStatusException → 400
///   - BusinessRuleException → 400
///   - InsufficientAmountException → 400
///   - EmptyCollectionException → 400
///   - EmptyItemsException → 400
///   - DomainException (base) → 400
///
/// UNEXPECTED EXCEPTIONS:
///   - Any other exception type → 500 Internal Server Error
///
/// All exceptions are converted to standardized API responses with:
///   - Appropriate HTTP status code
///   - MessageCode enum for i18n
///   - Client-friendly message
///   - System message for logging
///   - Stack trace for debugging
/// </summary>
public static class ExceptionHandlerHelper
{
    /// <summary>
    /// Handle any exception and return complete information for API response and logging.
    /// Categorizes exceptions into: Application, Domain, or Unexpected.
    /// </summary>
    /// <param name="exception">The exception to handle</param>
    /// <returns>ExceptionHandlingResult with all information for response and logging</returns>
    public static ExceptionHandlingResult HandleException(Exception exception)
    {
        // IExceptionHandler middleware swallows the exception here rather than letting it
        // propagate to the hosting layer, so OTel's AddAspNetCoreInstrumentation never sees it
        // and its automatic exception recording never fires. Record onto the request's Activity
        // explicitly so it still shows up as an APM error linked to the right trace/transaction.
        Activity.Current?.AddException(exception);
        Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);

        return exception switch
        {
            AppException appEx => HandleApplicationException(appEx),
            DomainEx domainEx => HandleDomainException(domainEx),
            _ => HandleUnexpectedException(exception)
        };
    }

    /// <summary>
    /// Handle Application layer exceptions (HTTP-aware).
    /// These exceptions already include HTTP status codes.
    /// Supported types: UnauthorizedException, ForbiddenException, NotFoundException,
    /// BadRequestException, ConflictException, ValidationException, ApplicationException.
    /// </summary>
    private static ExceptionHandlingResult HandleApplicationException(AppException ex)
    {
        var clientMessage = MessageCodeAttribute.GetMessage(ex.MessageCode);
        var statusCode = ex.StatusCode;

        return new ExceptionHandlingResult
        {
            StatusCode = statusCode,
            MessageCode = ex.MessageCode,
            MessageCodeString = ex.MessageCode.ToStringCode(),
            ClientMessage = clientMessage,
            SystemMessage = ex.SystemMessage,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException,
            ApiResponse = ApiResponse<object?>.Fail(
                clientMessage,
                ex.MessageCode,
                details: ex.ErrorDetails
            ),
            LogMessage = BuildLogMessage(
                clientMessage,
                ex.SystemMessage,
                ex.StackTrace,
                ex.InnerException,
                ex.ErrorDetails)
        };
    }

    /// <summary>
    /// Handle Domain layer exceptions (business logic violations).
    /// Maps domain exceptions to appropriate HTTP status codes.
    /// Supported types: EntityNotFoundException, InvalidArgumentException, InvalidStateException,
    /// InvalidStatusException, BusinessRuleException, InsufficientAmountException,
    /// EmptyCollectionException, EmptyItemsException, DomainException.
    ///
    /// Status Code Mapping:
    ///   - 404 (Not Found): EntityNotFoundException
    ///   - 400 (Bad Request): All other domain exceptions (invalid input, business rules, empty collections)
    /// </summary>
    private static ExceptionHandlingResult HandleDomainException(DomainEx ex)
    {
        var clientMessage = MessageCodeAttribute.GetMessage(ex.MessageCode);

        // Map domain exceptions to appropriate HTTP status codes
        // Domain exceptions are business logic violations, typically client errors (4xx)
        var statusCode = ex switch
        {
            // Entity/Resource not found - 404
            EntityNotFoundException => 404,

            // Invalid arguments/state/status - 400 Bad Request
            InvalidArgumentException => 400,
            InvalidStateException => 400,
            InvalidStatusException => 400,

            // Business rule violations - 400 Bad Request
            BusinessRuleException => 400,

            // Insufficient resources - 400 Bad Request
            InsufficientAmountException => 400,

            // Empty/no items - 400 Bad Request
            EmptyCollectionException => 400,
            EmptyItemsException => 400,

            // Default to 400 for any other domain exception
            // This ensures we never return 5xx for expected business logic errors
            _ => 400
        };

        return new ExceptionHandlingResult
        {
            StatusCode = statusCode,
            MessageCode = ex.MessageCode,
            MessageCodeString = ex.MessageCode.ToStringCode(),
            ClientMessage = clientMessage,
            SystemMessage = ex.SystemMessage,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException,
            ApiResponse = ApiResponse<object?>.Fail(
                clientMessage,
                ex.MessageCode,
                details: ex.ErrorDetails
            ),
            LogMessage = BuildLogMessage(
                clientMessage,
                ex.SystemMessage,
                ex.StackTrace,
                ex.InnerException,
                ex.ErrorDetails)
        };
    }

    /// <summary>
    /// Handle unexpected/unhandled exceptions (not internal exceptions).
    /// Returns 500 Internal Server Error and masks details from client.
    /// System message and stack trace are logged for debugging but not exposed to client.
    /// </summary>
    private static ExceptionHandlingResult HandleUnexpectedException(Exception ex)
    {
        var systemMessage = MessageCodeAttribute.GetMessage(MessageCodeEnum.SystemError);

        return new ExceptionHandlingResult
        {
            StatusCode = 500,
            MessageCode = MessageCodeEnum.SystemError,
            MessageCodeString = MessageCodeEnum.SystemError.ToStringCode(),
            ClientMessage = systemMessage,
            SystemMessage = $"Unhandled exception: {ex.GetType().Name}",
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException,
            ApiResponse = ApiResponse<object?>.Fail(
                systemMessage,
                MessageCodeEnum.SystemError,
                details: null
            ),
            LogMessage = BuildLogMessage(
                systemMessage,
                ex.Message,
                ex.StackTrace,
                ex.InnerException,
                null)
        };
    }

    private static string BuildLogMessage(
        string clientMessage,
        string? systemMessage,
        string? stackTrace,
        Exception? innerException,
        object? detail)
    {
        var logParts = new List<string>
        {
            $"[Client Message] {clientMessage}"
        };

        if (!string.IsNullOrEmpty(systemMessage))
            logParts.Add($"[System Message] {systemMessage}");

        if (innerException != null)
            logParts.Add($"[Inner Exception] {innerException.GetType().Name}: {innerException.Message}");

        if (!string.IsNullOrEmpty(stackTrace))
            logParts.Add($"[Stack Trace] {stackTrace}");

        if (detail is not null)
        {
            if (detail is string || detail.GetType().IsPrimitive)
                logParts.Add($"[Error Detail] {detail}");
            else if (detail.GetType().IsClass)
                logParts.Add($"[Error Detail] {JsonSerializer.Serialize(detail)}");
            else
                logParts.Add($"[Error Detail] {detail}");
        }

        return string.Join(Environment.NewLine, logParts);
    }
}

/// <summary>
/// Complete exception handling result containing all information for API response and logging.
/// </summary>
public class ExceptionHandlingResult
{
    /// <summary>
    /// HTTP status code for response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Message code enum value.
    /// </summary>
    public MessageCodeEnum MessageCode { get; set; }

    /// <summary>
    /// Message code as string (e.g., "001", "100", "304").
    /// </summary>
    public string MessageCodeString { get; set; } = string.Empty;

    /// <summary>
    /// Message for client (from MessageCode).
    /// </summary>
    public string ClientMessage { get; set; } = string.Empty;

    /// <summary>
    /// Detailed system message for logging.
    /// </summary>
    public string? SystemMessage { get; set; }

    /// <summary>
    /// Stack trace of exception.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception if any.
    /// </summary>
    public Exception? InnerException { get; set; }

    /// <summary>
    /// API response to return to client.
    /// </summary>
    public ApiResponse<object?> ApiResponse { get; set; } = null!;

    /// <summary>
    /// Formatted log message for structured logging (Seq).
    /// Includes client message, system message, inner exception, and stack trace.
    /// </summary>
    public string LogMessage { get; set; } = string.Empty;
}
