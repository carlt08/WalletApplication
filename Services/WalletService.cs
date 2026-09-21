using Microsoft.EntityFrameworkCore;
using WalletApplication.API.Events;
using WalletApplication.API.Models;
using WalletApplication.API.Repositories;

namespace WalletApplication.API.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _repository;

    // ILogger is used ONLY for operational logging (diagnostics/troubleshooting).
    // The WithdrawalCompleted event below is the actual notification mechanism.
    private readonly ILogger<WalletService> _logger;

    public event EventHandler<WithdrawalEvent>? WithdrawalCompleted;

    public WalletService(IWalletRepository repository, ILogger<WalletService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<decimal> GetBalanceAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var wallet = await _repository.GetByIdAsync(walletId, cancellationToken)
                     ?? throw new WalletNotFoundException(walletId);

        return wallet.Balance;
    }

    public async Task<decimal> WithdrawAsync(Guid walletId, decimal amount, CancellationToken cancellationToken = default)
    {
        var wallet = await _repository.GetByIdAsync(walletId, cancellationToken)
                     ?? throw new WalletNotFoundException(walletId);

        // The entity enforces the business rules and throws on invalid withdrawals,
        // so nothing is saved and no event is raised when a withdrawal fails.
        wallet.Withdraw(amount);

        try
        {
            await _repository.SaveAsync(wallet, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another withdrawal changed the wallet between our read and our save.
            // Nothing was persisted, so no event is raised.
            _logger.LogWarning("Concurrent withdrawal detected on wallet {WalletId}.", walletId);
            throw new ConcurrentWithdrawalException(walletId);
        }

        _logger.LogInformation(
            "Withdrawal of {Amount} from wallet {WalletId} succeeded. New balance: {Balance}.",
            amount, walletId, wallet.Balance);

        WithdrawalCompleted?.Invoke(this, new WithdrawalEvent(walletId, amount, DateTime.UtcNow));

        return wallet.Balance;
    }
}
