namespace NovaCore.BuildingBlock.Infrastructure.Mail.Exceptions;

public sealed class MailException : Exception
{
    public MailException(string message) : base(message)
    {
    }

    public MailException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
