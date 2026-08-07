namespace NovaCore.Shipping.Domain.Entities.Transportations;

/// <summary>
/// One *execution attempt* against a Shipment - and the only place a ShippingProvider is ever
/// bound. Several Transportations can exist for the same Shipment (attempt 1 fails, attempt 2 is
/// created with a different provider/person/vehicle), which is exactly why the provider lives
/// here and not on Shipment. Owns its Assignment (who/what carries it), Tracking pings, delivery
/// Proof, and Cost breakdown.
/// </summary>
public sealed class Transportation : AggregateRoot<Guid>, IAuditable, ITenantEntity, IIdempotentEntity
{
    public TransportationNumber TransportationNumber { get; private set; } = default!;
    public Guid ShipmentId { get; private set; }
    public Guid ProviderId { get; private set; }

    /// <summary>1-based attempt counter within its Shipment - attempt N+1 is a brand-new Transportation, never a mutation of attempt N.</summary>
    public int AttemptNo { get; private set; }
    public TransportationStatus Status { get; private set; }

    /// <summary>Optional reference to the reusable TransportationCostRule the cost was derived from - referenced, never owned.</summary>
    public Guid? CostRuleId { get; private set; }
    public Money TotalCost { get; private set; } = default!;
    public decimal? DistanceKm { get; private set; }

    public DateTime? ScheduledPickupAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? Note { get; private set; }

    public TransportationAssignment? Assignment { get; private set; }
    public TransportationProof? Proof { get; private set; }
    public ICollection<TransportationTracking> Trackings { get; private set; } = [];
    public ICollection<TransportationCost> Costs { get; private set; } = [];

    public string? IdempotencyKey { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    // ============================================================================
    // Construction
    // Creates one attempt against a Shipment, bound to the provider that will
    // actually carry it. Cost starts at zero and accumulates from its own
    // TransportationCost lines.
    // ============================================================================

    #region Construction
    private Transportation() { }

    public static Transportation Create(
        Guid shipmentId,
        Guid providerId,
        int attemptNo,
        Guid? costRuleId = null,
        DateTime? scheduledPickupAt = null,
        decimal? distanceKm = null,
        string? note = null,
        string? idempotencyKey = null)
    {
        if (shipmentId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Shipment id is required.");

        if (providerId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Provider id is required.");

        if (attemptNo < 1)
            throw ExceptionFactory.InvalidRange("Attempt number must be at least 1.");

        if (distanceKm is < 0)
            throw ExceptionFactory.InvalidRange("Distance cannot be negative.");

        return new Transportation
        {
            Id = Guid.CreateVersion7(),
            TransportationNumber = TransportationNumber.Create(),
            ShipmentId = shipmentId,
            ProviderId = providerId,
            AttemptNo = attemptNo,
            Status = TransportationStatus.Created,
            CostRuleId = costRuleId,
            TotalCost = Money.Create(0),
            DistanceKm = distanceKm,
            ScheduledPickupAt = scheduledPickupAt,
            Note = note?.Trim(),
            IdempotencyKey = idempotencyKey,
        };
    }
    #endregion

    // ============================================================================
    // Assignment
    // Binds the concrete carrier resources (person and/or vehicle) to this attempt.
    // 1:1 with the Transportation and only replaceable while the attempt has not
    // physically started.
    // ============================================================================

    #region Assignment
    public void Assign(Guid? personId, Guid? vehicleId, Guid? assignedById = null, string? note = null)
    {
        if (Status is not (TransportationStatus.Created or TransportationStatus.Assigned))
            throw ExceptionFactory.InvalidStatus($"Cannot assign a transportation in {Status} status.");

        if (personId is null && vehicleId is null)
            throw ExceptionFactory.RequiredField("An assignment needs at least a person or a vehicle.");

        Assignment = TransportationAssignment.Create(Id, personId, vehicleId, assignedById, note);
        Status = TransportationStatus.Assigned;
    }
    #endregion

    // ============================================================================
    // Tracking
    // Append-only physical progress pings for this attempt. Distinct from
    // ShipmentEvent, which records the intention's own status timeline.
    // ============================================================================

    #region Tracking
    public void RecordTracking(GeoCoordinate? coordinate, string description)
    {
        if (Status is TransportationStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Cannot record tracking on a cancelled transportation.");

        Trackings.Add(TransportationTracking.Record(Id, Status, coordinate, description));
    }
    #endregion

    // ============================================================================
    // Proof
    // The delivery evidence (signature/photo/receiver name) captured at completion.
    // 1:1 and write-once - a redelivery produces a new Transportation with its own
    // proof, never an overwrite of this one.
    // ============================================================================

    #region Proof
    public void CaptureProof(string receivedByName, string? signatureUrl, string? photoUrl, string? note = null)
    {
        if (Proof is not null)
            throw ExceptionFactory.InvalidState("Proof has already been captured for this transportation.");

        if (Status is not (TransportationStatus.InTransit or TransportationStatus.PickedUp))
            throw ExceptionFactory.InvalidStatus($"Cannot capture proof for a transportation in {Status} status.");

        Proof = TransportationProof.Create(Id, receivedByName, signatureUrl, photoUrl, note);
    }
    #endregion

    // ============================================================================
    // Costs
    // Accumulates the per-category cost breakdown of this attempt and keeps
    // TotalCost as their derived sum - never set directly.
    // ============================================================================

    #region Costs
    public void AddCost(CostCategory category, Money amount, string? description = null)
    {
        Costs.Add(TransportationCost.Create(Id, category, amount, description));
        TotalCost = Money.Create(Costs.Sum(c => c.Amount.Value));
    }

    public void RemoveCost(long costId)
    {
        var cost = Costs.FirstOrDefault(c => c.Id == costId)
            ?? throw ExceptionFactory.EntityNotFound<TransportationCost>(costId);

        Costs.Remove(cost);
        TotalCost = Money.Create(Costs.Sum(c => c.Amount.Value));
    }
    #endregion

    // ============================================================================
    // Status & lifecycle
    // The attempt's own state machine. Reaching Failed/Cancelled ends this attempt
    // only - the parent Shipment decides whether another attempt follows.
    // ============================================================================

    #region Status & lifecycle
    public void MarkPickedUp()
    {
        if (Status != TransportationStatus.Assigned)
            throw ExceptionFactory.InvalidStatus($"Cannot pick up a transportation in {Status} status.");

        Status = TransportationStatus.PickedUp;
        StartedAt = DateTime.UtcNow;
    }

    public void MarkInTransit()
    {
        if (Status != TransportationStatus.PickedUp)
            throw ExceptionFactory.InvalidStatus($"Cannot move a transportation in {Status} status to transit.");

        Status = TransportationStatus.InTransit;
    }

    public void MarkDelivered()
    {
        if (Status != TransportationStatus.InTransit)
            throw ExceptionFactory.InvalidStatus($"Cannot deliver a transportation in {Status} status.");

        Status = TransportationStatus.Delivered;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A failure reason is required.");

        if (Status is TransportationStatus.Delivered or TransportationStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a transportation in {Status} status.");

        Status = TransportationStatus.Failed;
        FailureReason = reason.Trim();
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkReturned(string reason)
    {
        if (Status is not (TransportationStatus.InTransit or TransportationStatus.Failed))
            throw ExceptionFactory.InvalidStatus($"Cannot return a transportation in {Status} status.");

        Status = TransportationStatus.Returned;
        FailureReason = reason?.Trim();
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A cancellation reason is required.");

        if (Status is TransportationStatus.Delivered or TransportationStatus.Returned)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a transportation in {Status} status.");

        Status = TransportationStatus.Cancelled;
        FailureReason = reason.Trim();
        CompletedAt = DateTime.UtcNow;
    }
    #endregion
}
