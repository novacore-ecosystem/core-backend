using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Content.Domain.Entities.Contents;
using NovaCore.Content.Domain.Enums;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.Entities;

public class ContentTests
{
    private static ContentEntity CreateValidContent(ContentVisibility visibility = ContentVisibility.Private)
        => ContentEntity.Create(
            Guid.CreateVersion7(),
            ContentSlug.Create("first-article"),
            "First Article",
            "Summary",
            "Body",
            Guid.CreateVersion7(),
            visibility);

    #region Create

    [Fact]
    public void Create_ValidInput_CreatesContentWithOneDraftVersion()
    {
        var content = CreateValidContent();

        content.Status.ShouldBe(ContentStatus.Draft);
        content.Versions.Count.ShouldBe(1);
        content.CurrentVersionId.ShouldBe(content.Versions.Single().Id);
        content.Versions.Single().VersionNumber.ShouldBe(1);
        content.PublishedVersionId.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidTitle_ThrowsRequiredField(string? title)
    {
        Action act = () => ContentEntity.Create(
            Guid.CreateVersion7(),
            ContentSlug.Create("slug"),
            title!,
            "summary",
            "body",
            Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    #endregion

    #region Versioning

    [Fact]
    public void CreateDraftVersion_IncrementsVersionNumber()
    {
        var content = CreateValidContent();

        var second = content.CreateDraftVersion("Second Title", "Summary", "Body", Guid.CreateVersion7());

        second.VersionNumber.ShouldBe(2);
        content.Versions.Count.ShouldBe(2);
        content.CurrentVersionId.ShouldBe(second.Id);
    }

    [Fact]
    public void CreateDraftVersion_OnArchivedContent_ThrowsInvalidStatus()
    {
        var content = CreateValidContent();
        content.Archive();

        Action act = () => content.CreateDraftVersion("Title", "Summary", "Body", Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidStatusException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void RestoreVersion_CreatesNewVersionCopyingTargetContent_PreservesHistory()
    {
        var content = CreateValidContent();
        var firstVersionId = content.CurrentVersionId!.Value;
        content.CreateDraftVersion("Second Title", "Second Summary", "Second Body", Guid.CreateVersion7());

        var restored = content.RestoreVersion(firstVersionId, Guid.CreateVersion7());

        content.Versions.Count.ShouldBe(3);
        restored.Title.ShouldBe("First Article");
        content.CurrentVersionId.ShouldBe(restored.Id);
        content.Versions.ShouldContain(v => v.Id == firstVersionId);
    }

    [Fact]
    public void RestoreVersion_UnknownVersion_ThrowsEntityNotFound()
    {
        var content = CreateValidContent();

        Action act = () => content.RestoreVersion(Guid.CreateVersion7(), Guid.CreateVersion7());

        act.ShouldThrowDomainException<EntityNotFoundException>(MessageCode.NotFound);
    }

    #endregion

    #region Review & Publication

    [Fact]
    public void SubmitForReview_FromDraft_TransitionsToInReview()
    {
        var content = CreateValidContent();

        content.SubmitForReview();

        content.Status.ShouldBe(ContentStatus.InReview);
    }

    [Fact]
    public void SubmitForReview_FromPublished_ThrowsInvalidStatus()
    {
        var content = CreateValidContent();
        content.Publish(content.CurrentVersionId!.Value, DateTime.UtcNow);

        Action act = () => content.SubmitForReview();

        act.ShouldThrowDomainException<InvalidStatusException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Approve_FromInReview_TransitionsToApproved()
    {
        var content = CreateValidContent();
        content.SubmitForReview();

        content.Approve();

        content.Status.ShouldBe(ContentStatus.Approved);
    }

    [Fact]
    public void Reject_FromInReview_TransitionsToRejected()
    {
        var content = CreateValidContent();
        content.SubmitForReview();

        content.Reject();

        content.Status.ShouldBe(ContentStatus.Rejected);
    }

    [Fact]
    public void Publish_UnknownVersion_ThrowsEntityNotFound()
    {
        var content = CreateValidContent();

        Action act = () => content.Publish(Guid.CreateVersion7(), DateTime.UtcNow);

        act.ShouldThrowDomainException<EntityNotFoundException>(MessageCode.NotFound);
    }

    [Fact]
    public void Publish_ValidVersion_SetsPublishedVersionAndStatus()
    {
        var content = CreateValidContent();
        var versionId = content.CurrentVersionId!.Value;
        var publishedAt = DateTime.UtcNow;

        content.Publish(versionId, publishedAt);

        content.Status.ShouldBe(ContentStatus.Published);
        content.PublishedVersionId.ShouldBe(versionId);
        content.PublishedAt.ShouldBe(publishedAt);
        content.Versions.Single(v => v.Id == versionId).Status.ShouldBe(ContentStatus.Published);
    }

    [Fact]
    public void Publish_DoesNotOverwriteNewerDraft()
    {
        var content = CreateValidContent();
        var firstVersionId = content.CurrentVersionId!.Value;
        var draft = content.CreateDraftVersion("Draft Title", "Draft Summary", "Draft Body", Guid.CreateVersion7());

        content.Publish(firstVersionId, DateTime.UtcNow);

        content.PublishedVersionId.ShouldBe(firstVersionId);
        content.CurrentVersionId.ShouldBe(draft.Id);
        draft.Status.ShouldBe(ContentStatus.Draft);
    }

    [Fact]
    public void Unpublish_PublishedContent_TransitionsToUnpublished()
    {
        var content = CreateValidContent();
        content.Publish(content.CurrentVersionId!.Value, DateTime.UtcNow);

        content.Unpublish(DateTime.UtcNow);

        content.Status.ShouldBe(ContentStatus.Unpublished);
    }

    [Fact]
    public void Unpublish_NotPublished_ThrowsInvalidStatus()
    {
        var content = CreateValidContent();

        Action act = () => content.Unpublish(DateTime.UtcNow);

        act.ShouldThrowDomainException<InvalidStatusException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Archive_WhilePublished_ThrowsInvalidStatus()
    {
        var content = CreateValidContent();
        content.Publish(content.CurrentVersionId!.Value, DateTime.UtcNow);

        Action act = () => content.Archive();

        act.ShouldThrowDomainException<InvalidStatusException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Archive_FromDraft_SetsArchivedAtAndStatus()
    {
        var content = CreateValidContent();

        content.Archive();

        content.Status.ShouldBe(ContentStatus.Archived);
        content.ArchivedAt.ShouldNotBeNull();
    }

    #endregion

    #region Taxonomy, Audience & Contributors

    [Fact]
    public void AssignTaxonomy_CalledTwice_DoesNotDuplicate()
    {
        var content = CreateValidContent();
        var taxonomyId = Guid.CreateVersion7();

        content.AssignTaxonomy(taxonomyId);
        content.AssignTaxonomy(taxonomyId);

        content.TaxonomyAssignments.Count(a => a.TaxonomyId == taxonomyId).ShouldBe(1);
    }

    [Fact]
    public void RemoveTaxonomy_NotAssigned_IsNoOp()
    {
        var content = CreateValidContent();

        Should.NotThrow(() => content.RemoveTaxonomy(Guid.CreateVersion7()));
    }

    [Fact]
    public void AddContributor_SameUserAndRoleTwice_ReturnsExistingContributor()
    {
        var content = CreateValidContent();
        var userId = Guid.CreateVersion7();

        var first = content.AddContributor(userId, ContributorRole.Author);
        var second = content.AddContributor(userId, ContributorRole.Author);

        second.ShouldBe(first);
        content.Contributors.Count.ShouldBe(1);
    }

    [Fact]
    public void AddAudience_OrganizationWithoutReferenceId_ThrowsRequiredField()
    {
        var content = CreateValidContent();

        Action act = () => content.AddAudience(AudienceType.Organization);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void AddAudience_AllUsersWithReferenceId_ThrowsInvalidState()
    {
        var content = CreateValidContent();

        Action act = () => content.AddAudience(AudienceType.AllUsers, Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidStateException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void AddAudience_AllUsersWithoutReferenceId_Succeeds()
    {
        var content = CreateValidContent();

        var audience = content.AddAudience(AudienceType.AllUsers);

        audience.AudienceReferenceId.ShouldBeNull();
        content.Audiences.ShouldContain(audience);
    }

    #endregion

    #region Workflow

    [Fact]
    public void StartWorkflow_CreatesInstanceInInitialState()
    {
        var content = CreateValidContent();
        var workflowDefinitionId = Guid.CreateVersion7();

        var instance = content.StartWorkflow(workflowDefinitionId, "draft", Guid.CreateVersion7());

        instance.CurrentState.ShouldBe("draft");
        instance.CompletedAt.ShouldBeNull();
        content.WorkflowInstances.ShouldContain(instance);
    }

    [Fact]
    public void TransitionWorkflow_UnknownInstance_ThrowsEntityNotFound()
    {
        var content = CreateValidContent();

        Action act = () => content.TransitionWorkflow(Guid.CreateVersion7(), "in_review");

        act.ShouldThrowDomainException<EntityNotFoundException>(MessageCode.NotFound);
    }

    [Fact]
    public void CompleteWorkflow_ActiveInstance_SetsCompletedAt()
    {
        var content = CreateValidContent();
        var instance = content.StartWorkflow(Guid.CreateVersion7(), "draft", Guid.CreateVersion7());

        content.CompleteWorkflow(instance.Id);

        instance.CompletedAt.ShouldNotBeNull();
    }

    #endregion
}
