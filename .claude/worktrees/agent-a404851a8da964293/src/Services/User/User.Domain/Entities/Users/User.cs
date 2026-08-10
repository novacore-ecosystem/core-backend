namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Aggregate root of the identity model. Holds account identity, lifecycle status, and ownership
/// of every user-scoped child (profile, avatar, addresses, contacts, settings, roles/tags
/// assignments). Orders and carts are intentionally absent - those belong to their own services.
/// UserRole and UserTag are independent aggregate roots referenced via join entities
/// (UserRoleAssignment/UserTagMapping), not owned - many users share the same role/tag.
/// </summary>
public sealed class User : AggregateRoot<Guid>, IAuditable, ITenantEntity, ISoftDeleteEntity
{
    public string Username { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }
    public UserType UserType { get; private set; }
    public UserAvatar? Avatar { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public UserMetadata Metadata { get; private set; } = new();
    public UserProfile? Profile { get; private set; }
    public ICollection<UserAddress> Addresses { get; private set; } = [];
    public ICollection<UserContact> Contacts { get; private set; } = [];
    public UserSetting? Setting { get; private set; }
    public UserPreference? Preference { get; private set; }
    public ICollection<UserPaymentMethod> PaymentMethods { get; private set; } = [];
    public UserNotificationSetting? NotificationSetting { get; private set; }
    public UserPrivacySetting? PrivacySetting { get; private set; }
    public UserSecuritySetting? SecuritySetting { get; private set; }
    public ICollection<UserVerification> Verifications { get; private set; } = [];
    public UserActivitySummary? ActivitySummary { get; private set; }
    public ICollection<UserRoleAssignment> RoleAssignments { get; private set; } = [];
    public UserPermissionSnapshot? PermissionSnapshot { get; private set; }
    public ICollection<UserTagMapping> TagMappings { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Framework-facing assignment point for ISoftDeleteEntity - idempotent, same
    /// reasoning as AssignTenant. Called by MarkAsDeleted below rather than directly by callers,
    /// so Status and IsDeleted/DeletedAt never drift out of sync.</summary>
    public void MarkDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private User() { }

    /// <summary>
    /// New accounts start PendingVerification rather than Active - reachability of at least one
    /// contact channel is expected to be confirmed (see UserContact.Verify) before the account
    /// is promoted, matching every other explicit status a User can be Activated into.
    /// </summary>
    public static User Create(
        string username,
        string displayName,
        UserType userType,
        UserStatus status = UserStatus.PendingVerification,
        Guid? id = null)
    {
        ValidateUsername(username);

        return new User
        {
            // Defaults to a fresh id, but SyncFromAccountInitiation needs this User's id to equal
            // the Account's id already minted by Auth - the two rows are correlated by sharing the
            // same id, not by a separate foreign key.
            Id = id ?? Guid.CreateVersion7(),
            Username = username,
            DisplayName = displayName,
            UserType = userType,
            Status = status,
        };
    }

    // ============================================================================
    // Profile
    // Manages the owned 1:1 UserProfile - created on first call, updated in place
    // afterward, so callers never need to branch on whether a profile already
    // exists.
    // ============================================================================

    #region Profile

    public void UpdateProfile(
        PersonalName personalName,
        DateOnly? birthday = null,
        Gender gender = Gender.Unknown,
        string biography = "",
        string? occupation = null,
        string? company = null,
        string? website = null,
        LanguageCode? language = null,
        string? timeZone = null,
        string? countryCode = null)
    {
        if (Profile is null)
        {
            Profile = UserProfile.Create(
                Id,
                personalName,
                birthday,
                gender,
                biography,
                occupation,
                company,
                website,
                language,
                timeZone,
                countryCode);
            return;
        }

        Profile.UpdateDetails(
            personalName,
            birthday,
            gender,
            biography,
            occupation,
            company,
            website,
            language,
            timeZone,
            countryCode);
    }

    #endregion

    // ============================================================================
    // Avatar
    // Manages the owned 1:1 UserAvatar. Each replacement creates a new version
    // (bumping Version) rather than mutating MediaId in place, so cached/CDN
    // copies of the previous avatar are never mistaken for the current one.
    // ============================================================================

    #region Avatar

    public void SetAvatar(
        Guid mediaId,
        Guid? thumbnailMediaId = null,
        AvatarDisplayMode displayMode = AvatarDisplayMode.Original)
    {
        var nextVersion = (Avatar?.Version ?? 0) + 1;
        Avatar = UserAvatar.Create(Id, mediaId, nextVersion, thumbnailMediaId, displayMode);
    }

    public void RemoveAvatar()
    {
        Avatar = null;
    }

    #endregion

    // ============================================================================
    // Addresses
    // Manages the owned UserAddress collection: add/remove and the mutually
    // exclusive IsDefaultShipping/IsDefaultBilling flags, each with at most one
    // winner across the collection.
    // ============================================================================

    #region Addresses

    public UserAddress AddAddress(
        string label,
        Receiver receiver,
        Address address,
        AddressType addressType,
        GeoLocation? geoLocation = null,
        string? building = null,
        string? apartment = null,
        string? floor = null,
        string? deliveryInstruction = null,
        bool isDefaultShipping = false,
        bool isDefaultBilling = false)
    {
        if (isDefaultShipping)
            ClearDefaultShipping();

        if (isDefaultBilling)
            ClearDefaultBilling();

        var userAddress = UserAddress.Create(
            Id,
            label,
            receiver,
            address,
            addressType,
            geoLocation,
            building,
            apartment,
            floor,
            deliveryInstruction,
            isDefaultShipping,
            isDefaultBilling);
        Addresses.Add(userAddress);

        return userAddress;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address is null)
            return;

        Addresses.Remove(address);
    }

    public void SetDefaultShippingAddress(Guid addressId)
    {
        var target = Addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw ExceptionFactory.EntityNotFound<UserAddress>(addressId);

        if (target.IsDefaultShipping)
            return;

        ClearDefaultShipping();
        target.MarkAsDefaultShipping();
    }

    public void SetDefaultBillingAddress(Guid addressId)
    {
        var target = Addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw ExceptionFactory.EntityNotFound<UserAddress>(addressId);

        if (target.IsDefaultBilling)
            return;

        ClearDefaultBilling();
        target.MarkAsDefaultBilling();
    }

    private void ClearDefaultShipping()
    {
        foreach (var address in Addresses.Where(a => a.IsDefaultShipping))
            address.UnmarkAsDefaultShipping();
    }

    private void ClearDefaultBilling()
    {
        foreach (var address in Addresses.Where(a => a.IsDefaultBilling))
            address.UnmarkAsDefaultBilling();
    }

    #endregion

    // ============================================================================
    // Contacts
    // Manages the owned UserContact collection: add/remove and the IsPrimary
    // flag, scoped per ContactType so at most one Email and one Phone (etc.) can
    // be primary at the same time.
    // ============================================================================

    #region Contacts

    public UserContact AddContact(
        ContactType contactType,
        string value,
        string? label = null,
        bool isPrimary = false)
    {
        if (Contacts.Any(c => c.ContactType == contactType && c.Value == value))
            throw ExceptionFactory.Duplicate("This contact value already exists for the given contact type.");

        if (isPrimary)
            ClearPrimaryContact(contactType);

        var contact = UserContact.Create(Id, contactType, value, label, isPrimary);
        Contacts.Add(contact);

        return contact;
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = Contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact is null)
            return;

        Contacts.Remove(contact);
    }

    public void SetPrimaryContact(Guid contactId)
    {
        var target = Contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw ExceptionFactory.EntityNotFound<UserContact>(contactId);

        if (target.IsPrimary)
            return;

        ClearPrimaryContact(target.ContactType);
        target.MarkAsPrimary();
    }

    private void ClearPrimaryContact(ContactType contactType)
    {
        foreach (var contact in Contacts.Where(c => c.ContactType == contactType && c.IsPrimary))
            contact.UnmarkAsPrimary();
    }

    #endregion

    // ============================================================================
    // Settings
    // Manages the owned 1:1 UserSetting (display/locale/UI preferences) -
    // created on first call, updated in place afterward.
    // ============================================================================

    #region Settings

    public void UpdateSettings(
        ThemeMode theme = ThemeMode.System,
        LanguageCode? language = null,
        string? currency = null,
        string? timeZone = null,
        string? dateFormat = null,
        TimeFormat timeFormat = TimeFormat.TwentyFourHours,
        WeekDay firstDayOfWeek = WeekDay.Monday,
        string? dashboardLayout = null,
        bool sidebarCollapsed = false,
        int itemsPerPage = 20)
    {
        if (Setting is null)
        {
            Setting = UserSetting.Create(
                Id, theme, language, currency, timeZone, dateFormat, timeFormat, firstDayOfWeek, dashboardLayout, sidebarCollapsed, itemsPerPage);
            return;
        }

        Setting.UpdateDetails(
            theme, language, currency, timeZone, dateFormat, timeFormat, firstDayOfWeek, dashboardLayout, sidebarCollapsed, itemsPerPage);
    }

    #endregion

    // ============================================================================
    // Preferences
    // Manages the owned 1:1 UserPreference (favorites, recently viewed, search
    // history) - created on first use, then delegated to for every mutation.
    // ============================================================================

    #region Preferences

    public void AddFavoriteCategory(Guid categoryId)
    {
        EnsurePreference().AddFavoriteCategory(categoryId);
    }

    public void RemoveFavoriteCategory(Guid categoryId)
    {
        EnsurePreference().RemoveFavoriteCategory(categoryId);
    }

    public void AddFavoriteBrand(Guid brandId)
    {
        EnsurePreference().AddFavoriteBrand(brandId);
    }

    public void RemoveFavoriteBrand(Guid brandId)
    {
        EnsurePreference().RemoveFavoriteBrand(brandId);
    }

    public void SetPreferredWarehouse(string? preferredWarehouseCode)
    {
        EnsurePreference().SetPreferredWarehouse(preferredWarehouseCode);
    }

    public void RecordProductView(Guid productId)
    {
        EnsurePreference().RecordProductView(productId);
    }

    public void ClearRecentlyViewedProducts()
    {
        EnsurePreference().ClearRecentlyViewedProducts();
    }

    public void RecordSearchTerm(string term)
    {
        EnsurePreference().RecordSearchTerm(term);
    }

    public void ClearSearchHistory()
    {
        EnsurePreference().ClearSearchHistory();
    }

    private UserPreference EnsurePreference()
    {
        Preference ??= UserPreference.Create(Id);
        return Preference;
    }

    #endregion

    // ============================================================================
    // Payment methods
    // Manages the owned UserPaymentMethod collection: add/remove and the
    // IsDefault flag, with at most one default across the collection. Each row is
    // just a reference into Payment Service's own PaymentAccount - see
    // UserPaymentMethod's remarks and docs/reference/payment-ownership-boundaries.md.
    // ============================================================================

    #region Payment methods

    public UserPaymentMethod AddPaymentMethod(
        Guid paymentAccountId,
        string displayName,
        bool isDefault = false)
    {
        if (isDefault)
            ClearDefaultPaymentMethod();

        var paymentMethod = UserPaymentMethod.Create(Id, paymentAccountId, displayName, isDefault);
        PaymentMethods.Add(paymentMethod);

        return paymentMethod;
    }

    public void RemovePaymentMethod(Guid paymentMethodId)
    {
        var paymentMethod = PaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
        if (paymentMethod is null)
            return;

        PaymentMethods.Remove(paymentMethod);
    }

    public void SetDefaultPaymentMethod(Guid paymentMethodId)
    {
        var target = PaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId)
            ?? throw ExceptionFactory.EntityNotFound<UserPaymentMethod>(paymentMethodId);

        if (target.IsDefault)
            return;

        ClearDefaultPaymentMethod();
        target.MarkAsDefault();
    }

    private void ClearDefaultPaymentMethod()
    {
        foreach (var paymentMethod in PaymentMethods.Where(p => p.IsDefault))
            paymentMethod.UnmarkAsDefault();
    }

    #endregion

    // ============================================================================
    // Notification settings
    // Manages the owned 1:1 UserNotificationSetting - created on first call,
    // updated in place afterward.
    // ============================================================================

    #region Notification settings

    public void UpdateNotificationSettings(
        bool emailEnabled = true,
        bool smsEnabled = false,
        bool pushEnabled = true,
        bool signalREnabled = true,
        bool marketingEnabled = false,
        bool orderEnabled = true,
        bool promotionEnabled = false,
        bool securityEnabled = true)
    {
        if (NotificationSetting is null)
        {
            NotificationSetting = UserNotificationSetting.Create(
                Id, emailEnabled, smsEnabled, pushEnabled, signalREnabled, marketingEnabled, orderEnabled, promotionEnabled, securityEnabled);
            return;
        }

        NotificationSetting.UpdateDetails(
            emailEnabled, smsEnabled, pushEnabled, signalREnabled, marketingEnabled, orderEnabled, promotionEnabled, securityEnabled);
    }

    #endregion

    // ============================================================================
    // Privacy settings
    // Manages the owned 1:1 UserPrivacySetting - created on first call, updated
    // in place afterward.
    // ============================================================================

    #region Privacy settings

    public void UpdatePrivacySettings(
        bool showBirthday = false,
        bool showEmail = false,
        bool showPhoneNumber = false,
        bool allowTracking = false,
        bool allowRecommendation = false,
        bool allowPersonalizedAds = false)
    {
        if (PrivacySetting is null)
        {
            PrivacySetting = UserPrivacySetting.Create(
                Id, showBirthday, showEmail, showPhoneNumber, allowTracking, allowRecommendation, allowPersonalizedAds);
            return;
        }

        PrivacySetting.UpdateDetails(
            showBirthday, showEmail, showPhoneNumber, allowTracking, allowRecommendation, allowPersonalizedAds);
    }

    #endregion

    // ============================================================================
    // Security settings
    // Manages the owned 1:1 UserSecuritySetting - created on first call, updated
    // in place afterward - plus dedicated two-factor toggles.
    // ============================================================================

    #region Security settings

    public void UpdateSecuritySettings(
        bool requirePasswordRotation = false,
        bool allowRememberDevice = true,
        bool trustedDevicesOnly = false,
        string? recoveryEmail = null,
        string? recoveryPhone = null)
    {
        EnsureSecuritySetting().UpdateDetails(requirePasswordRotation, allowRememberDevice, trustedDevicesOnly, recoveryEmail, recoveryPhone);
    }

    public void EnableTwoFactorAuthentication()
    {
        EnsureSecuritySetting().EnableTwoFactor();
    }

    public void DisableTwoFactorAuthentication()
    {
        EnsureSecuritySetting().DisableTwoFactor();
    }

    private UserSecuritySetting EnsureSecuritySetting()
    {
        SecuritySetting ??= UserSecuritySetting.Create(Id);
        return SecuritySetting;
    }

    #endregion

    // ============================================================================
    // Verifications
    // Manages the owned UserVerification collection: one Pending record at a
    // time per VerificationType, transitioned to Verified/Rejected/Expired
    // rather than overwritten, preserving history.
    // ============================================================================

    #region Verifications

    public UserVerification RequestVerification(VerificationType verificationType, string? note = null)
    {
        if (Verifications.Any(v => v.VerificationType == verificationType && v.VerificationStatus == VerificationStatus.Pending))
            throw ExceptionFactory.InvalidState("A verification of this type is already pending.");

        var verification = UserVerification.Create(Id, verificationType, note);
        Verifications.Add(verification);

        return verification;
    }

    public void CompleteVerification(Guid verificationId)
    {
        var verification = Verifications.FirstOrDefault(v => v.Id == verificationId)
            ?? throw ExceptionFactory.EntityNotFound<UserVerification>(verificationId);

        verification.Verify();
    }

    public void RejectVerification(Guid verificationId, string? note = null)
    {
        var verification = Verifications.FirstOrDefault(v => v.Id == verificationId)
            ?? throw ExceptionFactory.EntityNotFound<UserVerification>(verificationId);

        verification.Reject(note);
    }

    public void ExpireVerification(Guid verificationId)
    {
        var verification = Verifications.FirstOrDefault(v => v.Id == verificationId)
            ?? throw ExceptionFactory.EntityNotFound<UserVerification>(verificationId);

        verification.Expire();
    }

    #endregion

    // ============================================================================
    // Activity summary
    // Manages the owned 1:1 UserActivitySummary - created on first use, then
    // delegated to for every counter update.
    // ============================================================================

    #region Activity summary

    public void RecordLogin()
    {
        EnsureActivitySummary().RecordLogin();
    }

    public void RecordOrder()
    {
        EnsureActivitySummary().RecordOrder();
    }

    public void RecordPurchase(decimal amount)
    {
        EnsureActivitySummary().RecordPurchase(amount);
    }

    public void SetFavoriteCategory(Guid? categoryId)
    {
        EnsureActivitySummary().SetFavoriteCategory(categoryId);
    }

    private UserActivitySummary EnsureActivitySummary()
    {
        ActivitySummary ??= UserActivitySummary.Create(Id);
        return ActivitySummary;
    }

    #endregion

    // ============================================================================
    // Roles
    // Manages the owned UserRoleAssignment join collection linking this User to
    // independent UserRole aggregates - User never holds a Role object, only
    // this reference. AssignRole is idempotent against any currently-effective
    // grant of the same role; RevokeRole transitions the grant's Status rather
    // than deleting it, preserving history. Neither method touches
    // PermissionSnapshot - see the Permission snapshot region below.
    // ============================================================================

    #region Roles

    public UserRoleAssignment AssignRole(Guid roleId, Guid? assignedBy = null, DateTime? expiredAt = null)
    {
        var existing = RoleAssignments.FirstOrDefault(r => r.RoleId == roleId && r.IsEffective);
        if (existing is not null)
            return existing;

        var assignment = UserRoleAssignment.Create(Id, roleId, assignedBy, expiredAt);
        RoleAssignments.Add(assignment);

        return assignment;
    }

    public void RevokeRole(Guid roleId)
    {
        var assignment = RoleAssignments.FirstOrDefault(r => r.RoleId == roleId && r.IsEffective);
        if (assignment is null)
            return;

        assignment.Revoke();
    }

    #endregion

    // ============================================================================
    // Permission snapshot
    // Manages the owned 1:1 UserPermissionSnapshot read model - created on first
    // call, rebuilt in place afterward. RebuildPermissionSnapshot takes an
    // already-merged PermissionCollection (computed cross-aggregate by the
    // Application layer from every effective UserRoleAssignment's UserRole) and
    // is intentionally never called from AssignRole/RevokeRole above: permission
    // changes propagate asynchronously through events/queue, not synchronously
    // inside a User command.
    // ============================================================================

    #region Permission snapshot

    public void RebuildPermissionSnapshot(PermissionCollection mergedPermissions)
    {
        PermissionSnapshot ??= UserPermissionSnapshot.Create(Id);
        PermissionSnapshot.Rebuild(mergedPermissions);
    }

    #endregion

    // ============================================================================
    // Tags
    // Manages the owned UserTagMapping join collection linking this User to
    // independent UserTag aggregates (segmentation, not membership/loyalty).
    // Add/remove are idempotent - assigning an already-held tag, or removing one
    // not held, is a no-op.
    // ============================================================================

    #region Tags

    public void AssignTag(Guid tagId)
    {
        if (TagMappings.Any(t => t.TagId == tagId))
            return;

        TagMappings.Add(UserTagMapping.Create(Id, tagId));
    }

    public void RemoveTag(Guid tagId)
    {
        var mapping = TagMappings.FirstOrDefault(t => t.TagId == tagId);
        if (mapping is null)
            return;

        TagMappings.Remove(mapping);
    }

    #endregion

    // ============================================================================
    // Metadata
    // Replaces the extensible UserMetadata bag wholesale - callers mutate a copy
    // (or the current instance) and pass it back, matching Product's
    // UpdateMetadata pattern.
    // ============================================================================

    #region Metadata

    public void UpdateMetadata(UserMetadata metadata)
    {
        Metadata = metadata;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core identity fields (username/display name/type), presence tracking, and
    // the UserStatus transitions, plus the shared username-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void Rename(string username, string displayName)
    {
        ValidateUsername(username);

        Username = username;
        DisplayName = displayName;
    }

    public void ChangeUserType(UserType userType)
    {
        UserType = userType;
    }

    public void RecordLastSeen()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    public void Verify()
    {
        Status = UserStatus.Active;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
    }

    public void Lock()
    {
        Status = UserStatus.Locked;
    }

    public void Unlock()
    {
        Status = UserStatus.Active;
    }

    public void MarkAsDeleted()
    {
        Status = UserStatus.Deleted;
        MarkDeleted();
    }

    public static bool IsValidUsername(string? username) => !string.IsNullOrWhiteSpace(username);

    private static void ValidateUsername(string username)
    {
        if (!IsValidUsername(username))
            throw ExceptionFactory.RequiredField("Username cannot be empty.");
    }

    #endregion
}
