namespace NovaCore.User.Application.Abstractions.Services;

/// <summary>
/// Formats a person's name for display, locale-aware, without ever persisting the formatted result -
/// the database stays FirstName/MiddleName/LastName only. See docs/tasks/2026-07-28/Task5_displayname-formatter.md.
/// </summary>
public interface IUserDisplayNameFormatter
{
    string Format(string firstName, string middleName, string lastName, string locale);
}
