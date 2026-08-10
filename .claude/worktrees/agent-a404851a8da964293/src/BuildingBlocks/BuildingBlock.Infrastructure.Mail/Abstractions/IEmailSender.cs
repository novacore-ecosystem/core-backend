using NovaCore.BuildingBlock.Infrastructure.Mail.Models;

namespace NovaCore.BuildingBlock.Infrastructure.Mail.Abstractions;

public interface IEmailSender
{
    Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken ct = default);
}
