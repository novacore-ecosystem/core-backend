namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

public interface IAppLogger<T>
{
    void Trace(string message, params object?[] args);

    void Debug(string message, params object?[] args);

    void Information(string message, params object?[] args);

    void Warning(string message, params object?[] args);

    void Error(
        Exception? exception,
        string message,
        params object?[] args);

    void Critical(
        Exception? exception,
        string message,
        params object?[] args);
}
