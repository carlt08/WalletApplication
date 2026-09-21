namespace WalletApplication.API.Events;

/// <summary>
/// Raised (via a native C# event) after a withdrawal has succeeded and been saved.
/// </summary>
public class WithdrawalEvent : EventArgs
{
    public Guid WalletId { get; }

    public decimal Amount { get; }

    public DateTime OccurredAtUtc { get; }

    public WithdrawalEvent(Guid walletId, decimal amount, DateTime occurredAtUtc)
    {
        WalletId = walletId;
        Amount = amount;
        OccurredAtUtc = occurredAtUtc;
    }
}
