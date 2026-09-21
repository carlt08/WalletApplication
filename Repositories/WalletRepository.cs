using Microsoft.EntityFrameworkCore;
using WalletApplication.API.Data;
using WalletApplication.API.Models;

namespace WalletApplication.API.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _dbContext;

    public WalletRepository(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default) =>
        _dbContext.Wallets.FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);

    public async Task SaveAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        // The wallet is already tracked when loaded via GetByIdAsync.
        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
