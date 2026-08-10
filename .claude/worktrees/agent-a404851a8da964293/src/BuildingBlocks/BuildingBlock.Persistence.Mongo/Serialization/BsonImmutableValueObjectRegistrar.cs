using System.Reflection;

using MongoDB.Bson.Serialization;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Serialization;

/// <summary>
/// Registers a BsonClassMap for a Domain value object shaped like this codebase's convention:
/// public get-only properties, populated only through a private constructor (see e.g.
/// NovaCore.Notification.Domain.ValueObjects.NotificationCategory). Without this, MongoDB's default
/// BsonClassMap.AutoMap() does not map get-only properties at all - the value object round-trips
/// as an empty subdocument, silently dropping every field on write (confirmed empirically against
/// MongoDB.Driver 3.10.0 - see docs/tasks/2026-07-22/Task2_notification-list-null-fields.md).
///
/// Uses reflection to bind the private constructor so Domain itself stays free of any
/// MongoDB.Bson reference - only Persistence knows about BSON mapping, per this repo's Clean
/// Architecture layering rules.
/// </summary>
public static class BsonImmutableValueObjectRegistrar
{
    /// <summary>
    /// <paramref name="memberNames"/> must list the value object's public properties in the exact
    /// order its private constructor declares the matching parameters - BsonClassMap.MapConstructor
    /// binds constructor parameters to members positionally, not by name.
    /// </summary>
    public static void Register<T>(params string[] memberNames)
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(T)))
            return;

        var ctor = typeof(T)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .SingleOrDefault(c => c.GetParameters().Length == memberNames.Length)
            ?? throw new InvalidOperationException(
                $"{typeof(T).Name} has no private constructor with {memberNames.Length} parameter(s) to match: {string.Join(", ", memberNames)}.");

        BsonClassMap.RegisterClassMap<T>(cm =>
        {
            cm.AutoMap();
            foreach (var name in memberNames)
                cm.MapMember(typeof(T).GetProperty(name)!);
            cm.MapConstructor(ctor, memberNames);
        });
    }
}
