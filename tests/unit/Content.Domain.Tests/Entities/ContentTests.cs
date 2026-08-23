using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.Content.Domain.Entities.Contents;
using NovaCore.Content.Domain.Enums;
using NovaCore.Content.Domain.ValueObjects;
using Shouldly;
using NovaCore.TestKit.ShouldlyExtensions;

namespace NovaCore.Content.Domain.Tests.Entities;

public class ContentTests
{
    private static readonly LanguageCode English = LanguageCode.Create("en");
    private static readonly LanguageCode Vietnamese = LanguageCode.Create("vi");

    private static ContentEntity CreateValidContent(ContentVisibility visibility = ContentVisibility.Private)
        => ContentEntity.Create(
            Guid.CreateVersion7(),
            ContentSlug.Create("first-article"),
            English,
            "First Article",
            "Summary",
            "{\"blocks\":[]}",
            Guid.CreateVersion7(),
            visibility);

    #region Create

    [Fact]
    public void Create_ValidInput_CreatesContentWithOneDraftVersionAndLocalization()
    {
        var content = CreateValidContent();

        content.Status.ShouldBe(ContentStatus.Draft);
        content.Versions.Count.ShouldBe(1);
        content.CurrentVersionId.ShouldBe(content.Versions.Single().Id);
        content.Versions.Single().VersionNumber.ShouldBe(1);
        content.PublishedVersionId.ShouldBeNull();

        var localization = content.Versions.Single().Localizations.Single();
        localization.Culture.ShouldBe(English);
        localization.Title.ShouldBe("First Article");
        content.Localizations.ShouldContain(localization);
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
            English,
            title!,
            "summary",
            "{}",
            Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_InvalidJsonBody_ThrowsInvalidFormat()
    {
        Action act = () => ContentEntity.Create(
            Guid.CreateVersion7(),
            ContentSlug.Create("slug"),
            English,
            "Title",
            "summary",
            "not-json",
            Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    #endregion

    #region Versioning

    [Fact]
    public void CreateDraftVersion_IncrementsVersionNumber()
    {
        var content = CreateValidContent();

        var second = content.CreateDraftVersion(English, "Second Title", "Summary", "{}", Guid.CreateVersion7());

        second.VersionNumber.ShouldBe(2);
        content.Versions.Count.ShouldBe(2);
        content.CurrentVersionId.ShouldBe(second.Id);
        second.Localizations.Single().Title.ShouldBe("Second Title");
    }

    [Fact]
    public void CreateDraftVersion_OnArchivedContent_ThrowsInvalidStatus()
    {
        var content = CreateValidContent();
        content.Archive();

        Action act = () => content.CreateDraftVersion(English, "Title", "Summary", "{}", Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidStatusException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void RestoreVersion_CreatesNewVersionCopyingEveryLanguage_PreservesHistory()
    {
        var content = CreateValidContent();
        var firstVersionId = content.CurrentVersionId!.Value;
        content.UpsertLocalization(firstVersionId, Vietnamese, "Bai Viet Dau Tien", "Tom tat", "{}", Guid.CreateVersion7());
        content.CreateDraftVersion(English, "Second Title", "Second Summary", "{}", Guid.CreateVersion7());

        var restored = content.RestoreVersion(firstVersionId, Guid.CreateVersion7());

        content.Versions.Count.ShouldBe(3);
        restored.Localizations.Count.ShouldBe(2);
        restored.GetLocalization(English)!.Title.ShouldBe("First Article");
        restored.GetLocalization(Vietnamese)!.Title.ShouldBe("Bai Viet Dau Tien");
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

    #region Localization / Translation

    [Fact]
    public void UpsertLocalization_NewCulture_AddsLocalizationToVersion()
    {
        var content = CreateValidContent();
        var versionId = content.CurrentVersionId!.Value;

        var localization = content.UpsertLocalization(versionId, Vietnamese, "Bai Viet", "Tom tat", "{}", Guid.CreateVersion7());

        content.Versions.Single().Localizations.Count.ShouldBe(2);
        localization.Culture.ShouldBe(Vietnamese);
    }

    [Fact]
    public void UpsertLocalization_ExistingCulture_UpdatesInPlace()
    {
        var content = CreateValidContent();
        var versionId = content.CurrentVersionId!.Value;

        var updated = content.UpsertLocalization(versionId, English, "Updated Title", "Updated Summary", "{}", Guid.CreateVersion7());

        content.Versions.Single().Localizations.Count.ShouldBe(1);
        updated.Title.ShouldBe("Updated Title");
    }

    [Fact]
    public void UpsertLocalization_OnPublishedVersion_ThrowsInvalidStatus()
    {
        var content = CreateValidContent();
        var versionId = content.CurrentVersionId!.Value;
        content.Publish(versionId, DateTime.UtcNow);

        Action act = () => content.UpsertLocalization(versionId, Vietnamese, "Title", "Summary", "{}", Guid.CreateVersion7());

        act.ShouldThrowDomainException<InvalidStatusException>(MessageCode.InvalidInput);
    }

    #endregion

    #region Soft Delete

    [Fact]
    public void Delete_MarksDeletedWithTimestamp()
    {
        var content = CreateValidContent();

        content.Delete();

        content.IsDeleted.ShouldBeTrue();
        content.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Delete_CalledTwice_IsIdempotent()
    {
        var content = CreateValidContent();
        content.Delete();
        var firstDeletedAt = content.DeletedAt;

        content.Delete();

        content.DeletedAt.ShouldBe(firstDeletedAt);
    }

    [Fact]
    public void Restore_DeletedContent_ClearsDeletedState()
    {
        var content = CreateValidContent();
        content.Delete();

        content.Restore();

        content.IsDeleted.ShouldBeFalse();
        content.DeletedAt.ShouldBeNull();
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
        var draft = content.CreateDraftVersion(English, "Draft Title", "Draft Summary", "{}", Guid.CreateVersion7());

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
