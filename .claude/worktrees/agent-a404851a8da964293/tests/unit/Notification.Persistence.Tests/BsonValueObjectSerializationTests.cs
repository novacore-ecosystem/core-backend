using NovaCore.BuildingBlock.Persistence.Mongo.Serialization;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

using NovaCore.Notification.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Notification.Persistence.Tests;

/// <summary>
/// Regression coverage for a write-time data-loss bug (docs/tasks/2026-07-22/Task2_notification-list-null-fields.md):
/// MongoDB's default BsonClassMap.AutoMap() does not serialize get-only properties at all, so
/// every value object in NovaCore.Notification.Domain (private constructor + get-only properties)
/// round-tripped as an empty subdocument - confirmed against the real MongoDB.Driver 3.10.0
/// package, not a stand-in repro. Fixed via BsonImmutableValueObjectRegistrar, registered once
/// per type here to mirror NovaCore.Notification.Persistence.DependencyInjection's production registration.
/// </summary>
public sealed class BsonValueObjectSerializationTests
{
    static BsonValueObjectSerializationTests()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        BsonImmutableValueObjectRegistrar.Register<NotificationCategory>("Value");
        BsonImmutableValueObjectRegistrar.Register<NotificationType>("Value");
        BsonImmutableValueObjectRegistrar.Register<NotificationContent>("Title", "Body");
        BsonImmutableValueObjectRegistrar.Register<AudienceSelector>("Type", "ConfigJson");
        BsonImmutableValueObjectRegistrar.Register<ChannelConfiguration>("ConfigJson");
        BsonImmutableValueObjectRegistrar.Register<DispatchReference>("ReferenceType", "ReferenceId");
        BsonImmutableValueObjectRegistrar.Register<NotificationSchedule>("ExecutionType", "StartAt", "EndAt", "CronExpression");
        BsonImmutableValueObjectRegistrar.Register<TemplateContent>("Subject", "Body", "Variables");
    }

    private static T RoundTrip<T>(T value)
    {
        var doc = value.ToBsonDocument();
        return BsonSerializer.Deserialize<T>(doc);
    }

    [Fact]
    public void NotificationCategory_RoundTrips_ThroughBson()
    {
        var category = NotificationCategory.Create("Order");

        RoundTrip(category).Value.ShouldBe("Order");
    }

    [Fact]
    public void NotificationType_RoundTrips_ThroughBson()
    {
        var type = NotificationType.Create("OrderConfirmed");

        RoundTrip(type).Value.ShouldBe("OrderConfirmed");
    }

    [Fact]
    public void NotificationContent_RoundTrips_ThroughBson()
    {
        var content = NotificationContent.Create("Order confirmed", "Your order has shipped");

        var result = RoundTrip(content);

        result.Title.ShouldBe("Order confirmed");
        result.Body.ShouldBe("Your order has shipped");
    }

    [Fact]
    public void AudienceSelector_RoundTrips_ThroughBson()
    {
        var selector = AudienceSelector.Create(AudienceType.Segment, """{"role":"admin"}""");

        var result = RoundTrip(selector);

        result.Type.ShouldBe(AudienceType.Segment);
        result.ConfigJson.ShouldBe("""{"role":"admin"}""");
    }

    [Fact]
    public void ChannelConfiguration_RoundTrips_ThroughBson()
    {
        var configuration = ChannelConfiguration.Create("""{"host":"smtp.example.com"}""");

        RoundTrip(configuration).ConfigJson.ShouldBe("""{"host":"smtp.example.com"}""");
    }

    [Fact]
    public void DispatchReference_RoundTrips_ThroughBson()
    {
        var reference = DispatchReference.Create("Order", "019f88d9-e51c-7c48-a0d9-43366cd49739");

        var result = RoundTrip(reference);

        result.ReferenceType.ShouldBe("Order");
        result.ReferenceId.ShouldBe("019f88d9-e51c-7c48-a0d9-43366cd49739");
    }

    [Fact]
    public void NotificationSchedule_RoundTrips_ThroughBson()
    {
        var schedule = NotificationSchedule.Create(CampaignExecutionType.Recurring, DateTime.UtcNow.Date, cronExpression: "0 9 * * *");

        var result = RoundTrip(schedule);

        result.ExecutionType.ShouldBe(CampaignExecutionType.Recurring);
        result.CronExpression.ShouldBe("0 9 * * *");
    }

    [Fact]
    public void TemplateContent_RoundTrips_ThroughBson()
    {
        var content = TemplateContent.Create("Welcome", "Hi {{customerName}}", ["customerName"]);

        var result = RoundTrip(content);

        result.Subject.ShouldBe("Welcome");
        result.Body.ShouldBe("Hi {{customerName}}");
        result.Variables.ShouldBe(["customerName"]);
    }

    [Fact]
    public void UserNotification_RoundTrips_WithAllNestedValueObjectFieldsIntact()
    {
        var notification = NovaCore.Notification.Domain.Entities.UserNotification.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            NotificationCategory.Create("Order"),
            NotificationType.Create("OrderConfirmed"),
            NotificationContent.Create("Order confirmed", "Your order has shipped"));

        var result = RoundTrip(notification);

        result.Category.Value.ShouldBe("Order");
        result.Type.Value.ShouldBe("OrderConfirmed");
        result.Content.Title.ShouldBe("Order confirmed");
        result.Content.Body.ShouldBe("Your order has shipped");
    }
}
