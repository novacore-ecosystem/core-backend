namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 pointer to the media asset used as the user's avatar. Version increments on every
/// replacement so downstream consumers (CDN/cache) can bust cached copies without a MediaId
/// change forcing a full re-upload.
/// </summary>
public sealed class UserAvatar : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public Guid MediaId { get; private set; }
    public Guid? ThumbnailMediaId { get; private set; }
    public AvatarDisplayMode DisplayMode { get; private set; }
    public int Version { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserAvatar() { }

    public static UserAvatar Create(
        Guid userId,
        Guid mediaId,
        int version,
        Guid? thumbnailMediaId = null,
        AvatarDisplayMode displayMode = AvatarDisplayMode.Original)
    {
        ValidateMediaId(mediaId);

        return new UserAvatar
        {
            UserId = userId,
            MediaId = mediaId,
            ThumbnailMediaId = thumbnailMediaId,
            DisplayMode = displayMode,
            Version = version,
        };
    }

    internal void UpdateDetails(
        Guid mediaId,
        Guid? thumbnailMediaId,
        AvatarDisplayMode displayMode,
        int version)
    {
        ValidateMediaId(mediaId);

        MediaId = mediaId;
        ThumbnailMediaId = thumbnailMediaId;
        DisplayMode = displayMode;
        Version = version;
    }

    private static void ValidateMediaId(Guid mediaId)
    {
        if (mediaId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Avatar media id cannot be empty.");
    }
}
