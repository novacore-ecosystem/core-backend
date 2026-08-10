namespace NovaCore.Shipping.Domain.Entities.ReturnShipments;

/// <summary>
/// A request to send goods back after an original Shipment. Foundation only: the record and its
/// state machine exist, but nothing yet creates the reverse Shipment automatically - approving a
/// return currently just records the decision and, once a reverse Shipment is created by a later
/// phase, links it via ReturnedShipmentId.
/// </summary>
public sealed class ReturnShipment : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid OriginalShipmentId { get; private set; }

    /// <summary>The reverse Shipment created to carry the goods back - null until a later phase creates it.</summary>
    public Guid? ReturnedShipmentId { get; private set; }
    public ReturnShipmentStatus Status { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime RequestedAt { get; private set; }
    public Guid? RequestedById { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ReturnShipment() { }

    public static ReturnShipment Create(Guid originalShipmentId, string reason, Guid? requestedById = null)
    {
        if (originalShipmentId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Original shipment id is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A return reason is required.");

        return new ReturnShipment
        {
            Id = Guid.CreateVersion7(),
            OriginalShipmentId = originalShipmentId,
            Status = ReturnShipmentStatus.Requested,
            Reason = reason.Trim(),
            RequestedAt = DateTime.UtcNow,
            RequestedById = requestedById,
        };
    }

    public void Approve()
    {
        if (Status != ReturnShipmentStatus.Requested)
            throw ExceptionFactory.InvalidStatus($"Cannot approve a return in {Status} status.");

        Status = ReturnShipmentStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status != ReturnShipmentStatus.Requested)
            throw ExceptionFactory.InvalidStatus($"Cannot reject a return in {Status} status.");

        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A rejection reason is required.");

        Status = ReturnShipmentStatus.Rejected;
        RejectionReason = reason.Trim();
    }

    /// <summary>Links the reverse Shipment that will physically carry the goods back and moves the return into transit.</summary>
    public void AttachReturnedShipment(Guid returnedShipmentId)
    {
        if (Status != ReturnShipmentStatus.Approved)
            throw ExceptionFactory.InvalidStatus($"Cannot attach a return shipment while the return is in {Status} status.");

        if (returnedShipmentId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Returned shipment id is required.");

        ReturnedShipmentId = returnedShipmentId;
        Status = ReturnShipmentStatus.InTransit;
    }

    public void Complete()
    {
        if (Status != ReturnShipmentStatus.InTransit)
            throw ExceptionFactory.InvalidStatus($"Cannot complete a return in {Status} status.");

        Status = ReturnShipmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is ReturnShipmentStatus.Completed or ReturnShipmentStatus.Rejected)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a return in {Status} status.");

        Status = ReturnShipmentStatus.Cancelled;
    }
}
