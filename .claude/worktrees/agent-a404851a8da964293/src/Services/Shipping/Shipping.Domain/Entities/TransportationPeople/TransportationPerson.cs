namespace NovaCore.Shipping.Domain.Entities.TransportationPeople;

/// <summary>
/// A person who can be assigned to a Transportation (staff driver, freelance shipper, a
/// carrier's own courier). Standalone aggregate root referencing its ShippingProvider by id - it
/// has its own lifecycle (hired, suspended, left) independent of any single trip.
/// </summary>
public sealed class TransportationPerson : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ProviderId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public PhoneNumber PhoneNumber { get; private set; } = default!;
    public Email? Email { get; private set; }
    public string? LicenseNumber { get; private set; }
    public PersonStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }

    /// <summary>Set when this person is also a platform user (freelancer with an app login) - null for a carrier's own staff.</summary>
    public Guid? UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private TransportationPerson() { }

    public static TransportationPerson Create(
        Guid providerId,
        string fullName,
        PhoneNumber phoneNumber,
        Email? email = null,
        string? licenseNumber = null,
        Guid? userId = null,
        DateTime? joinedAt = null)
    {
        if (providerId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Provider id is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw ExceptionFactory.RequiredField("Person full name cannot be empty.");

        return new TransportationPerson
        {
            Id = Guid.CreateVersion7(),
            ProviderId = providerId,
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber,
            Email = email,
            LicenseNumber = licenseNumber?.Trim(),
            UserId = userId,
            Status = PersonStatus.Active,
            JoinedAt = joinedAt ?? DateTime.UtcNow,
        };
    }

    public void Suspend() => Status = PersonStatus.Suspended;

    public void Reactivate() => Status = PersonStatus.Active;

    public void Deactivate() => Status = PersonStatus.Inactive;
}
