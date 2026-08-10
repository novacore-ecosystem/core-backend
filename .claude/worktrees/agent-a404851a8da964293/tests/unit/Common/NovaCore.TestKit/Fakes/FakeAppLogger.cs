using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.TestKit.Fakes;

/// <summary>
/// No-op <see cref="IAppLogger{T}"/> that records every call so a handler test can assert
/// "an error was logged" without depending on a real logging provider.
/// </summary>
public sealed class FakeAppLogger<T> : IAppLogger<T>
{
    public List<(string Level, Exception? Exception, string Message)> Entries { get; } = [];

    public void Trace(string message, params object?[] args) => Entries.Add(("Trace", null, message));
    public void Debug(string message, params object?[] args) => Entries.Add(("Debug", null, message));
    public void Information(string message, params object?[] args) => Entries.Add(("Information", null, message));
    public void Warning(string message, params object?[] args) => Entries.Add(("Warning", null, message));
    public void Error(Exception? exception, string message, params object?[] args) => Entries.Add(("Error", exception, message));
    public void Critical(Exception? exception, string message, params object?[] args) => Entries.Add(("Critical", exception, message));
}
