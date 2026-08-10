using NovaCore.BuildingBlock.SharedKernel.Text;

namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Point-in-time snapshot of who placed an order and where it ships - captured once at Create
/// time, never resynced from the User service afterward (same convention OrderItem.ProductName/
/// UnitPrice already follow). Split out from Order itself so the core order/status/items data
/// isn't coupled to this snapshot's columns. 1:1 with Order, sharing its primary key (OrderId) -
/// see OrderOwnerConfig.
/// </summary>
public sealed class OrderOwner : BaseEntity, ITenantEntity, IIdempotentEntity
{
    public Guid OrderId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;
    public Email OwnerEmail { get; private set; } = default!;
    public PhoneNumber OwnerPhone { get; private set; } = default!;
    public string OwnerPhoneSearch { get; private set; } = string.Empty;
    public string OwnerPhoneReverse { get; private set; } = string.Empty;

    public string? IdempotencyKey { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderOwner() { }

    /// <summary>Only Order may construct/mutate its Owner - same reasoning as OrderItem.Create being internal.</summary>
    internal static OrderOwner Create(
        Guid orderId,
        Guid customerId,
        string name,
        Email email,
        PhoneNumber phone,
        string? idempotencyKey = null)
    {
        var owner = new OrderOwner
        {
            OrderId = orderId,
            OwnerId = customerId,
            OwnerName = name,
            OwnerEmail = email,
            OwnerPhone = phone,
            IdempotencyKey = idempotencyKey,
        };
        owner.SyncCustomerSearchFields();

        return owner;
    }

    public void UpdateContact(
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey)
    {
        if (ownerName.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Owner name cannot be empty.");

        OwnerName = ownerName;
        OwnerEmail = ownerEmail;
        OwnerPhone = ownerPhone;
        IdempotencyKey = idempotencyKey;
        SyncCustomerSearchFields();
    }

    private void SyncCustomerSearchFields()
    {
        OwnerPhoneSearch = PhoneNormalizer.Normalize(OwnerPhone.Value);
        OwnerPhoneReverse = PhoneNormalizer.Reverse(OwnerPhoneSearch);
    }
}
