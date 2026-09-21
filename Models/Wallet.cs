namespace WalletApplication.API.Models;

/// <summary>
/// Wallet entity. All withdrawal business rules live here so the balance
/// can never be changed into an invalid state from outside.
/// </summary>
public class Wallet
{
    public Guid Id { get; private set; }

    public decimal Balance { get; private set; }

    /// <summary>
    /// Optimistic concurrency token. Incremented on every successful withdrawal so
    /// EF Core can detect a save based on a stale read.
    /// </summary>
    public int Version { get; private set; }

    private Wallet()
    {
    }

    public Wallet(Guid id, decimal balance)
    {
        Id = id;
        Balance = balance;
    }

    /// <summary>
    /// Withdraws the given amount. Throws <see cref="InvalidWithdrawalException"/>
    /// when the amount is not positive or exceeds the available balance.
    /// </summary>
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidWithdrawalException("Withdrawal amount must be greater than zero.");
        }

        if (amount > Balance)
        {
            throw new InvalidWithdrawalException("Insufficient funds for this withdrawal.");
        }

        Balance -= amount;
        Version++;
    }
}
