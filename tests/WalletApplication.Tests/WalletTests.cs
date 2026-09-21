using WalletApplication.API.Models;

namespace WalletApplication.Tests;

/// <summary>Tests for the wallet business rules (the domain entity).</summary>
public class WalletTests
{
    [Fact]
    public void Withdraw_WithValidAmount_DecreasesBalance()
    {
        var wallet = new Wallet(Guid.NewGuid(), 1000m);

        wallet.Withdraw(250m);

        Assert.Equal(750m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_MoreThanBalance_IsRejectedAndBalanceUnchanged()
    {
        var wallet = new Wallet(Guid.NewGuid(), 100m);

        Assert.Throws<InvalidWithdrawalException>(() => wallet.Withdraw(100.01m));
        Assert.Equal(100m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_ZeroAmount_IsRejected()
    {
        var wallet = new Wallet(Guid.NewGuid(), 100m);

        Assert.Throws<InvalidWithdrawalException>(() => wallet.Withdraw(0m));
        Assert.Equal(100m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_NegativeAmount_IsRejected()
    {
        var wallet = new Wallet(Guid.NewGuid(), 100m);

        Assert.Throws<InvalidWithdrawalException>(() => wallet.Withdraw(-50m));
        Assert.Equal(100m, wallet.Balance);
    }

    [Fact]
    public void Withdraw_EntireBalance_LeavesZeroAndNeverGoesNegative()
    {
        var wallet = new Wallet(Guid.NewGuid(), 100m);

        wallet.Withdraw(100m);

        Assert.Equal(0m, wallet.Balance);
    }
}
