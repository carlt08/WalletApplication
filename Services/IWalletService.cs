using WalletApplication.API.Events;

namespace WalletApplication.API.Services;

public interface IWalletService
{
    /// <summary>Notification mechanism: raised after a withdrawal succeeds.</summary>
    event EventHandler<WithdrawalEvent>? WithdrawalCompleted;

    Task<decimal> GetBalanceAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<decimal> WithdrawAsync(Guid walletId, decimal amount, CancellationToken cancellationToken = default);
}
