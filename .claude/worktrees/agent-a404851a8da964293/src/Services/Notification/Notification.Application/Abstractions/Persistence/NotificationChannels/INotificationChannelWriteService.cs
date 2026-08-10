namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;

public interface INotificationChannelWriteService
{
    /// <summary>Persists an already-mutated entity (Mongo ReplaceOneAsync) - the caller applies domain methods (Enable/Disable/UpdateConfiguration) before calling this.</summary>
    Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default);
}
