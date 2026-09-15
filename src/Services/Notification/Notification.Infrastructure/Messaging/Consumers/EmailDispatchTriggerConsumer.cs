using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Infrastructure.Mail.Builders;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using MediatR;

using NovaCore.Notification.Application.Features.NotificationDispatches.Commands.CreateNotificationDispatch;
using NovaCore.Notification.Application.Features.NotificationDispatches.DTOs;
using NovaCore.Notification.Domain.Enums;
using NovaCore.Notification.Domain.ValueObjects;

namespace NovaCore.Notification.Infrastructure.Messaging.Consumers;

/// <summary>
/// Fan-in consumer for account-related transactional emails specifically - kept separate from
/// <see cref="NotificationTriggerConsumer"/> because these aren't UserNotification-shaped events
/// (no persisted UserNotification row, no SignalR push): each one renders an HTML email body and
/// dispatches it through the Email channel only. Same thin, keyed-by-"event-type" shape as
/// NotificationTriggerConsumer otherwise.
/// </summary>
public sealed class EmailDispatchTriggerConsumer(
    ISender sender,
    IAppLogger<EmailDispatchTriggerConsumer> logger) : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(PasswordResetRequestedIntegrationEvent).ToLowerInvariant(),
        nameof(EmailVerificationRequestedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        if (!headers.TryGetValue("event-type", out var eventType))
        {
            logger.Warning("Received an email-dispatch-trigger message with no event-type header, skipping");
            return;
        }

        switch (eventType)
        {
            case nameof(PasswordResetRequestedIntegrationEvent):
                await HandlePasswordResetRequestedAsync(message, ct);
                break;

            case nameof(EmailVerificationRequestedIntegrationEvent):
                await HandleEmailVerificationRequestedAsync(message, ct);
                break;

            default:
                logger.Warning("No email mapping registered for event type {EventType}, skipping", eventType);
                return;
        }

        logger.Information("Dispatched email for {EventType}", eventType);
    }

    private async Task HandlePasswordResetRequestedAsync(string message, CancellationToken ct)
    {
        var data = Deserialize<PasswordResetRequestedIntegrationEvent>(message);

        var body = EmailBodyBuilder.Create()
            .Heading("Reset your password")
            .Paragraph("We received a request to reset your NovaCore password. Click the button below to choose a new one.")
            .Button("Reset password", data.ResetLink)
            .SmallText($"This link expires in {data.ExpiresInMinutes} minutes. If you didn't request this, you can safely ignore this email.")
            .Build();

        await SendEmailDispatchAsync(
            nameof(PasswordResetRequestedIntegrationEvent),
            Guid.Parse(data.AccountId),
            data.Email,
            subject: "Reset your NovaCore password",
            htmlBody: EmailTemplate.Default.Wrap(body),
            ct);
    }

    private async Task HandleEmailVerificationRequestedAsync(string message, CancellationToken ct)
    {
        var data = Deserialize<EmailVerificationRequestedIntegrationEvent>(message);

        var body = EmailBodyBuilder.Create()
            .Heading("Verify your email address")
            .Paragraph("Thanks for signing up! Click the button below to confirm your email address.")
            .Button("Verify email", data.VerificationLink)
            .SmallText("If you didn't create a NovaCore account, you can safely ignore this email.")
            .Build();

        await SendEmailDispatchAsync(
            nameof(EmailVerificationRequestedIntegrationEvent),
            Guid.Parse(data.AccountId),
            data.Email,
            subject: "Verify your email address",
            htmlBody: EmailTemplate.Default.Wrap(body),
            ct);
    }

    private async Task SendEmailDispatchAsync(
        string eventType,
        Guid accountId,
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken ct)
    {
        var payload = new NotificationDispatchPayload(
            accountId,
            Category: "Account",
            Type: eventType,
            Title: subject,
            Content: htmlBody,
            RecipientEmail: recipientEmail);

        var command = new CreateNotificationDispatchCommand(
            DispatchReference.Create(eventType, accountId.ToString()),
            [NotificationChannelType.Email],
            JsonSerializer.Serialize(payload));

        await sender.Send(command, ct);
    }

    private static T Deserialize<T>(string message) =>
        JsonSerializer.Deserialize<T>(message)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}");
}
