using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WalletApplication.API.Data;
using WalletApplication.API.Models;
using WalletApplication.API.Repositories;
using WalletApplication.API.Services;

namespace WalletApplication.Tests;

/// <summary>
/// Tests for the service flow, using a real (in-memory) SQLite database
/// so the repository and EF mapping are exercised too.
/// </summary>
public class WalletServiceTests : IDisposable
{
    private static readonly Guid WalletId = WalletDbContext.SeedWalletId;

    private readonly SqliteConnection _connection;
    private readonly WalletDbContext _dbContext;
    private readonly WalletService _service;

    public WalletServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new WalletDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new WalletService(
            new WalletRepository(_dbContext),
            NullLogger<WalletService>.Instance);
    }

    [Fact]
    public async Task GetBalanceAsync_ReturnsSeededBalance()
    {
        var balance = await _service.GetBalanceAsync(WalletId);

        Assert.Equal(1000.00m, balance);
    }

    [Fact]
    public async Task WithdrawAsync_Successful_DecreasesBalanceAndPersists()
    {
        var balance = await _service.WithdrawAsync(WalletId, 250m);

        Assert.Equal(750m, balance);
        Assert.Equal(750m, await _service.GetBalanceAsync(WalletId));
    }

    [Fact]
    public async Task WithdrawAsync_Successful_RaisesWithdrawalEvent()
    {
        WithdrawalEventCapture? captured = null;
        _service.WithdrawalCompleted += (_, e) =>
            captured = new WithdrawalEventCapture(e.WalletId, e.Amount, e.OccurredAtUtc);

        await _service.WithdrawAsync(WalletId, 100m);

        Assert.NotNull(captured);
        Assert.Equal(WalletId, captured!.WalletId);
        Assert.Equal(100m, captured.Amount);
        Assert.NotEqual(default, captured.OccurredAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(1000.01)]
    public async Task WithdrawAsync_Invalid_IsRejectedAndDoesNotRaiseEventOrChangeBalance(decimal amount)
    {
        var eventRaised = false;
        _service.WithdrawalCompleted += (_, _) => eventRaised = true;

        await Assert.ThrowsAsync<InvalidWithdrawalException>(
            () => _service.WithdrawAsync(WalletId, amount));

        Assert.False(eventRaised);
        Assert.Equal(1000.00m, await _service.GetBalanceAsync(WalletId));
    }

    [Fact]
    public async Task WithdrawAsync_WalletNotFound_ThrowsWalletNotFound()
    {
        await Assert.ThrowsAsync<WalletNotFoundException>(
            () => _service.WithdrawAsync(Guid.NewGuid(), 10m));
    }

    [Fact]
    public async Task GetBalanceAsync_WalletNotFound_ThrowsWalletNotFound()
    {
        await Assert.ThrowsAsync<WalletNotFoundException>(
            () => _service.GetBalanceAsync(Guid.NewGuid()));
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private record WithdrawalEventCapture(Guid WalletId, decimal Amount, DateTime OccurredAtUtc);
}
