namespace NovaCore.User.Domain.Enums;

/// <summary>Named distinctly from System.DayOfWeek - a project-owned enum keeps its ordinal
/// values (Monday-first) stable independently of the BCL's Sunday-first ordering.</summary>
public enum WeekDay : byte
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}
