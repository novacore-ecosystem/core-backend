using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Content.Domain.Entities.Taxonomies;
using NovaCore.Content.Domain.Enums;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.Entities;

public class ContentTaxonomyTests
{
    [Fact]
    public void Create_WithoutParent_IsRootNode()
    {
        var taxonomy = ContentTaxonomy.Create(ContentKey.Create("technology"), "Technology", "");

        taxonomy.ParentId.ShouldBeNull();
        taxonomy.Status.ShouldBe(ContentTypeStatus.Active);
    }

    [Fact]
    public void Create_WithSelfAsParentId_ThrowsInvalidState()
    {
        var id = Guid.CreateVersion7();

        // ParentId is only assignable via ChangeParent post-construction for a self-reference
        // check to trigger meaningfully - construct then attempt to self-parent.
        var taxonomy = ContentTaxonomy.Create(ContentKey.Create("technology"), "Technology", "");

        Action act = () => taxonomy.ChangeParent(taxonomy.Id);

        act.ShouldThrowDomainException<InvalidStateException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void ChangeParent_ToDifferentParent_UpdatesParentId()
    {
        var taxonomy = ContentTaxonomy.Create(ContentKey.Create("technology"), "Technology", "");
        var newParentId = Guid.CreateVersion7();

        taxonomy.ChangeParent(newParentId);

        taxonomy.ParentId.ShouldBe(newParentId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_InvalidName_ThrowsRequiredField(string? name)
    {
        Action act = () => ContentTaxonomy.Create(ContentKey.Create("technology"), name!, "");

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }
}
