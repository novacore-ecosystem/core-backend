namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>
/// A user's stored-value wallet balance - independent lifecycle, not owned by any single Voucher
/// (many Vouchers can reference the same user's wallet via Voucher.WalletId), so construction is
/// public, not internal, unlike Voucher's owned children.
/// </summary>
public sealed class VoucherWallet : BaseEntity<Guid>, IAuditable
{
    public Guid UserId { get; private set; }
    public Money TotalBalance { get; private set; } = default!;
    public Money AvailableBalance { get; private set; } = default!;
    public Money ReservedBalance { get; private set; } = default!;

    public ICollection<Voucher> Vouchers { get; private set; } = [];

    private VoucherWallet() { }

    public static VoucherWallet Create(Guid userId)
    {
        var zero = Money.Create(0);

        return new VoucherWallet
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TotalBalance = zero,
            AvailableBalance = zero,
            ReservedBalance = zero,
        };
    }

    /// <summary>Structural balance setters only - no debit/credit/reservation calculation lives here.</summary>
    public void UpdateBalances(Money totalBalance, Money availableBalance, Money reservedBalance)
    {
        TotalBalance = totalBalance;
        AvailableBalance = availableBalance;
        ReservedBalance = reservedBalance;
    }
}
