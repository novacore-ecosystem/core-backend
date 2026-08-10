#pragma warning disable CS1591

using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Infrastructure.Logging;

public sealed class AppLogger<T>(ILogger<T> logger) : IAppLogger<T>
{
    public void Trace(string message, params object?[] args)
        => logger.LogTrace(message, args);

    public void Debug(string message, params object?[] args)
        => logger.LogDebug(message, args);

    public void Information(string message, params object?[] args)
        => logger.LogInformation(message, args);

    public void Warning(string message, params object?[] args)
        => logger.LogWarning(message, args);

    public void Error(
        Exception? exception,
        string message,
        params object?[] args)
        => logger.LogError(exception, message, args);

    public void Critical(
        Exception? exception,
        string message,
        params object?[] args)
        => logger.LogCritical(exception, message, args);
}
