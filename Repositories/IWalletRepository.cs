using WalletApplication.API.Models;

namespace WalletApplication.API.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task SaveAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
