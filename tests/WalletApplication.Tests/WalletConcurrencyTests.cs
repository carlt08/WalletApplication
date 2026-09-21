using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WalletApplication.API.Data;
using WalletApplication.API.Models;
using WalletApplication.API.Repositories;
using WalletApplication.API.Services;

namespace WalletApplication.Tests;

/// <summary>
/// Concurrency tests. Each "request" gets its own DbContext (as it would in the API,
/// where the context is scoped) while sharing a single in-memory SQLite database.
/// </summary>
public class WalletConcurrencyTests : IDisposable
{
    private static readonly Guid WalletId = WalletDbContext.SeedWalletId;

    private readonly SqliteConnection _connection;

    public WalletConcurrencyTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var setup = CreateDbContext();
        setup.Database.EnsureCreated();
    }

    private WalletDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<WalletDbContext>().UseSqlite(_connection).Options);

    private static WalletService CreateService(WalletDbContext dbContext) =>
        new(new WalletRepository(dbContext), NullLogger<WalletService>.Instance);

    [Fact]
    public async Task TwoWithdrawalsFromSameOriginalBalance_SecondIsRejectedWithConcurrencyError()
    {
        using var contextA = CreateDbContext();
        using var contextB = CreateDbContext();

        var serviceA = CreateService(contextA);
        var serviceB = CreateService(contextB);

        // Both "requests" read the wallet at Balance = 1000, Version = 0.
        await serviceA.GetBalanceAsync(WalletId);
        await serviceB.GetBalanceAsync(WalletId);

        // A saves first and wins.
        var balanceAfterA = await serviceA.WithdrawAsync(WalletId, 600m);
        Assert.Equal(400m, balanceAfterA);

        // B is still working from Version 0, so its save matches 0 rows.
        await Assert.ThrowsAsync<ConcurrentWithdrawalException>(
            () => serviceB.WithdrawAsync(WalletId, 600m));
    }

    [Fact]
    public async Task ConcurrentWithdrawal_LeavesBalanceReflectingOnlyTheWinner()
    {
        using var contextA = CreateDbContext();
        using var contextB = CreateDbContext();

        await CreateService(contextA).GetBalanceAsync(WalletId);
        await CreateService(contextB).GetBalanceAsync(WalletId);

        await CreateService(contextA).WithdrawAsync(WalletId, 600m);

        await Assert.ThrowsAsync<ConcurrentWithdrawalException>(
            () => CreateService(contextB).WithdrawAsync(WalletId, 600m));

        // Only one R600 withdrawal was applied; the balance never went negative.
        using var verifyContext = CreateDbContext();
        var balance = await CreateService(verifyContext).GetBalanceAsync(WalletId);

        Assert.Equal(400m, balance);
        Assert.True(balance >= 0m);
    }

    [Fact]
    public async Task ConcurrentWithdrawal_DoesNotRaiseWithdrawalEventForTheLoser()
    {
        using var contextA = CreateDbContext();
        using var contextB = CreateDbContext();

        var serviceA = CreateService(contextA);
        var serviceB = CreateService(contextB);

        await serviceA.GetBalanceAsync(WalletId);
        await serviceB.GetBalanceAsync(WalletId);

        var winnerEventCount = 0;
        var loserEventCount = 0;
        serviceA.WithdrawalCompleted += (_, _) => winnerEventCount++;
        serviceB.WithdrawalCompleted += (_, _) => loserEventCount++;

        await serviceA.WithdrawAsync(WalletId, 600m);

        await Assert.ThrowsAsync<ConcurrentWithdrawalException>(
            () => serviceB.WithdrawAsync(WalletId, 600m));

        Assert.Equal(1, winnerEventCount);
        Assert.Equal(0, loserEventCount);
    }

    [Fact]
    public async Task SequentialWithdrawals_BothSucceedAndIncrementVersion()
    {
        using var context = CreateDbContext();
        var service = CreateService(context);

        await service.WithdrawAsync(WalletId, 100m);
        var balance = await service.WithdrawAsync(WalletId, 100m);

        Assert.Equal(800m, balance);

        var wallet = await context.Wallets.AsNoTracking().SingleAsync(w => w.Id == WalletId);
        Assert.Equal(2, wallet.Version);
    }

    public void Dispose() => _connection.Dispose();
}
