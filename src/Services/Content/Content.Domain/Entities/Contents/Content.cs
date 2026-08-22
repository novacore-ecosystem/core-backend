namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// Aggregate root of the Content Platform - the reusable content-management core behind WCM,
/// current article/news/bulletin use cases, and future CMS products. Holds identity, lifecycle,
/// and ownership of every content-scoped child (versions, publications, workflow instances,
/// relationships, localizations, taxonomy assignments, audience rules, contributors). A Content's
/// identity is stable across edits - editing never creates a new ContentId, only a new
/// ContentVersion. Article and every future content type are built entirely on this model; there
/// is no Article-specific table or field anywhere in this aggregate.
/// </summary>
public sealed class Content : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentTypeId { get; private set; }
    public ContentType ContentType { get; private set; } = default!;
    public ContentSlug Slug { get; private set; } = null!;
    public ContentStatus Status { get; private set; }
    public ContentVisibility Visibility { get; private set; }
    public Guid? CurrentVersionId { get; private set; }
    public ContentVersion? CurrentVersion { get; private set; }
    public Guid? PublishedVersionId { get; private set; }
    public ContentVersion? PublishedVersion { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public ICollection<ContentVersion> Versions { get; private set; } = [];
    public ICollection<ContentPublication> Publications { get; private set; } = [];
    public ICollection<ContentWorkflowInstance> WorkflowInstances { get; private set; } = [];
    public ICollection<ContentRelationship> Relationships { get; private set; } = [];
    public ICollection<ContentLocalization> Localizations { get; private set; } = [];
    public ICollection<ContentTaxonomyAssignment> TaxonomyAssignments { get; private set; } = [];
    public ICollection<ContentAudience> Audiences { get; private set; } = [];
    public ICollection<ContentContributor> Contributors { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Content() { }

    /// <summary>
    /// Creates a Content together with its required first ContentVersion in one call - a Content
    /// with no version at all would have nothing to edit, publish, or render, so the invariant is
    /// resolved here rather than left to a caller round-trip (same reasoning Product.Create
    /// applies to its required Variant collection).
    /// </summary>
    public static Content Create(
        Guid contentTypeId,
        ContentSlug slug,
        string title,
        string summary,
        string body,
        Guid createdBy,
        ContentVisibility visibility = ContentVisibility.Private,
        ContentMetadata? metadata = null)
    {
        var content = new Content
        {
            Id = Guid.CreateVersion7(),
            ContentTypeId = contentTypeId,
            Slug = slug,
            Status = ContentStatus.Draft,
            Visibility = visibility,
            CreatedBy = createdBy,
        };

        var firstVersion = ContentVersion.Create(content.Id, 1, title, summary, body, createdBy, metadata);
        content.Versions.Add(firstVersion);
        content.CurrentVersionId = firstVersion.Id;

        return content;
    }

    // ============================================================================
    // Versioning
    // Manages the owned ContentVersion collection. Version numbers are ordered and
    // unique within a Content; a draft can exist while another version is published;
    // restoring history creates a new version rather than destroying it.
    // ============================================================================

    #region Versioning

    public ContentVersion CreateDraftVersion(string title, string summary, string body, Guid createdBy, ContentMetadata? metadata = null)
    {
        if (Status == ContentStatus.Archived)
            throw ExceptionFactory.InvalidStatus("Cannot create a new version on an archived content item.");

        var nextVersionNumber = Versions.Count == 0 ? 1 : Versions.Max(v => v.VersionNumber) + 1;
        var version = ContentVersion.Create(Id, nextVersionNumber, title, summary, body, createdBy, metadata);
        Versions.Add(version);
        CurrentVersionId = version.Id;
        UpdatedBy = createdBy;

        return version;
    }

    public void UpdateDraft(Guid versionId, string title, string summary, string body, Guid updatedBy, ContentMetadata? metadata = null)
    {
        var version = Versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw ExceptionFactory.EntityNotFound<ContentVersion>(versionId);

        version.UpdateContent(title, summary, body, metadata);
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Restores a previous version's content into a brand-new current version, preserving every
    /// prior version untouched - history is never overwritten or destroyed by a restore.
    /// </summary>
    public ContentVersion RestoreVersion(Guid versionId, Guid restoredBy)
    {
        var target = Versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw ExceptionFactory.EntityNotFound<ContentVersion>(versionId);

        return CreateDraftVersion(target.Title, target.Summary, target.Body, restoredBy, target.Metadata);
    }

    #endregion

    // ============================================================================
    // Review & Publication
    // Manages the review/approval status and the Publication lifecycle. Publication is
    // always tied to a specific version and never overwrites draft data; scheduling and
    // unpublishing preserve historical ContentPublication rows rather than deleting them.
    // ============================================================================

    #region Review & Publication

    public void SubmitForReview()
    {
        if (Status is not (ContentStatus.Draft or ContentStatus.Rejected))
            throw ExceptionFactory.InvalidStatus("Only draft or rejected content can be submitted for review.");

        Status = ContentStatus.InReview;
    }

    public void Approve()
    {
        if (Status != ContentStatus.InReview)
            throw ExceptionFactory.InvalidStatus("Only content in review can be approved.");

        Status = ContentStatus.Approved;
    }

    public void Reject()
    {
        if (Status != ContentStatus.InReview)
            throw ExceptionFactory.InvalidStatus("Only content in review can be rejected.");

        Status = ContentStatus.Rejected;
    }

    public ContentPublication SchedulePublication(Guid versionId, DateTime scheduledAt, DateTime? expiresAt = null)
    {
        var version = GetOwnedVersion(versionId);

        var publication = ContentPublication.Create(Id, version.Id, scheduledAt, expiresAt);
        Publications.Add(publication);
        Status = ContentStatus.Scheduled;

        return publication;
    }

    /// <summary>
    /// Publishes a specific version. Never touches CurrentVersionId/draft data - a draft may keep
    /// being edited after this call while the published version stays exactly what it was here.
    /// </summary>
    public ContentPublication Publish(Guid versionId, DateTime publishedAt)
    {
        var version = GetOwnedVersion(versionId);

        var publication = Publications
            .Where(p => p.VersionId == versionId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault(p => p.Status is PublicationStatus.Scheduled or PublicationStatus.Unpublished);

        if (publication is null)
        {
            publication = ContentPublication.Create(Id, version.Id);
            Publications.Add(publication);
        }

        publication.MarkPublished(publishedAt);
        version.ChangeStatus(ContentStatus.Published);

        PublishedVersionId = version.Id;
        Status = ContentStatus.Published;
        PublishedAt = publishedAt;

        return publication;
    }

    public void Unpublish(DateTime unpublishedAt)
    {
        if (Status != ContentStatus.Published)
            throw ExceptionFactory.InvalidStatus("Only published content can be unpublished.");

        var activePublication = Publications
            .Where(p => p.VersionId == PublishedVersionId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault(p => p.Status == PublicationStatus.Published);

        activePublication?.MarkUnpublished(unpublishedAt);

        Status = ContentStatus.Unpublished;
    }

    public void Archive()
    {
        if (Status == ContentStatus.Published)
            throw ExceptionFactory.InvalidStatus("Unpublish content before archiving it.");

        Status = ContentStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
    }

    private ContentVersion GetOwnedVersion(Guid versionId)
        => Versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw ExceptionFactory.EntityNotFound<ContentVersion>(versionId);

    #endregion

    // ============================================================================
    // Workflow
    // Manages ContentWorkflowInstance lifecycle. Distinct from Publication/review status -
    // a Content can be mid-workflow while its last published version stays live. Transition
    // legality against the workflow definition's graph is validated by the caller (Application
    // layer), since it requires cross-aggregate data this aggregate cannot see on its own.
    // ============================================================================

    #region Workflow

    public ContentWorkflowInstance StartWorkflow(Guid workflowDefinitionId, string initialState, Guid startedBy)
    {
        var instance = ContentWorkflowInstance.Create(Id, workflowDefinitionId, initialState, startedBy);
        WorkflowInstances.Add(instance);

        return instance;
    }

    public void TransitionWorkflow(Guid instanceId, string newState)
    {
        var instance = WorkflowInstances.FirstOrDefault(i => i.Id == instanceId)
            ?? throw ExceptionFactory.EntityNotFound<ContentWorkflowInstance>(instanceId);

        instance.TransitionTo(newState);
    }

    public void CompleteWorkflow(Guid instanceId)
    {
        var instance = WorkflowInstances.FirstOrDefault(i => i.Id == instanceId)
            ?? throw ExceptionFactory.EntityNotFound<ContentWorkflowInstance>(instanceId);

        instance.Complete();
    }

    #endregion

    // ============================================================================
    // Taxonomy, Audience & Contributors
    // Manages the many-to-many taxonomy membership, audience-targeting rules, and
    // authoring-participation records owned by this Content.
    // ============================================================================

    #region Taxonomy, Audience & Contributors

    public void AssignTaxonomy(Guid taxonomyId)
    {
        if (TaxonomyAssignments.Any(a => a.TaxonomyId == taxonomyId))
            return;

        TaxonomyAssignments.Add(ContentTaxonomyAssignment.Create(Id, taxonomyId));
    }

    public void RemoveTaxonomy(Guid taxonomyId)
    {
        var assignment = TaxonomyAssignments.FirstOrDefault(a => a.TaxonomyId == taxonomyId);
        if (assignment is null)
            return;

        TaxonomyAssignments.Remove(assignment);
    }

    public ContentAudience AddAudience(AudienceType audienceType, Guid? audienceReferenceId = null)
    {
        var audience = ContentAudience.Create(Id, audienceType, audienceReferenceId);
        Audiences.Add(audience);

        return audience;
    }

    public void RemoveAudience(Guid audienceId)
    {
        var audience = Audiences.FirstOrDefault(a => a.Id == audienceId);
        if (audience is null)
            return;

        Audiences.Remove(audience);
    }

    public ContentContributor AddContributor(Guid userId, ContributorRole role)
    {
        var existing = Contributors.FirstOrDefault(c => c.UserId == userId && c.Role == role);
        if (existing is not null)
            return existing;

        var contributor = ContentContributor.Create(Id, userId, role);
        Contributors.Add(contributor);

        return contributor;
    }

    public void RemoveContributor(Guid contributorId)
    {
        var contributor = Contributors.FirstOrDefault(c => c.Id == contributorId);
        if (contributor is null)
            return;

        Contributors.Remove(contributor);
    }

    #endregion

    // ============================================================================
    // Relationships & Localization
    // Manages generic cross-resource relationships (which may target another Content or
    // any external resource type) and per-culture localization records.
    // ============================================================================

    #region Relationships & Localization

    public ContentRelationship AddRelationship(string targetType, Guid targetId, ContentRelationshipType relationshipType, string? metadata = null)
    {
        var relationship = ContentRelationship.Create(Id, targetType, targetId, relationshipType, metadata);
        Relationships.Add(relationship);

        return relationship;
    }

    public void RemoveRelationship(Guid relationshipId)
    {
        var relationship = Relationships.FirstOrDefault(r => r.Id == relationshipId);
        if (relationship is null)
            return;

        Relationships.Remove(relationship);
    }

    public ContentLocalization UpsertLocalization(LanguageCode culture, Guid versionId)
    {
        GetOwnedVersion(versionId);

        var existing = Localizations.FirstOrDefault(l => l.Culture == culture);
        if (existing is not null)
        {
            existing.ChangeVersion(versionId);
            return existing;
        }

        var localization = ContentLocalization.Create(Id, culture, versionId);
        Localizations.Add(localization);

        return localization;
    }

    #endregion

    // ============================================================================
    // Details
    // ============================================================================

    #region Details

    public void ChangeSlug(ContentSlug slug)
    {
        Slug = slug;
    }

    public void ChangeVisibility(ContentVisibility visibility)
    {
        Visibility = visibility;
    }

    #endregion
}
