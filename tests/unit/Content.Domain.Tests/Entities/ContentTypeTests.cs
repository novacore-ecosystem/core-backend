using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Content.Domain.Entities.ContentTypes;
using NovaCore.Content.Domain.Enums;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.Entities;

public class ContentTypeTests
{
    private static ContentType CreateValidContentType()
        => ContentType.Create(ContentKey.Create("article"), "Article", "News article");

    [Fact]
    public void Create_ValidInput_DefaultsToActiveWithSchemaVersionOne()
    {
        var contentType = CreateValidContentType();

        contentType.Status.ShouldBe(ContentTypeStatus.Active);
        contentType.SchemaVersion.ShouldBe(1);
        contentType.FieldDefinitions.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidName_ThrowsRequiredField(string? name)
    {
        Action act = () => ContentType.Create(ContentKey.Create("article"), name!, "desc");

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void AddFieldDefinition_BumpsSchemaVersion()
    {
        var contentType = CreateValidContentType();

        contentType.AddFieldDefinition(ContentKey.Create("headline"), "Headline", "", ContentFieldType.Text, isRequired: true);

        contentType.SchemaVersion.ShouldBe(2);
        contentType.FieldDefinitions.Count.ShouldBe(1);
    }

    [Fact]
    public void AddFieldDefinition_DuplicateKey_ThrowsDuplicate()
    {
        var contentType = CreateValidContentType();
        contentType.AddFieldDefinition(ContentKey.Create("headline"), "Headline", "", ContentFieldType.Text);

        Action act = () => contentType.AddFieldDefinition(ContentKey.Create("headline"), "Headline 2", "", ContentFieldType.Text);

        act.ShouldThrowDomainException<BusinessRuleException>(MessageCode.BadRequest);
    }

    [Fact]
    public void RemoveFieldDefinition_UnknownId_ThrowsEntityNotFound()
    {
        var contentType = CreateValidContentType();

        Action act = () => contentType.RemoveFieldDefinition(Guid.CreateVersion7());

        act.ShouldThrowDomainException<EntityNotFoundException>(MessageCode.NotFound);
    }

    [Fact]
    public void Archive_SetsStatusArchived()
    {
        var contentType = CreateValidContentType();

        contentType.Archive();

        contentType.Status.ShouldBe(ContentTypeStatus.Archived);
    }
}
