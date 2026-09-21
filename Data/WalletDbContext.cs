using Microsoft.EntityFrameworkCore;
using WalletApplication.API.Models;

namespace WalletApplication.API.Data;

public class WalletDbContext : DbContext
{
    public static readonly Guid SeedWalletId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var wallet = modelBuilder.Entity<Wallet>();

        wallet.HasKey(w => w.Id);
        wallet.Property(w => w.Balance).HasColumnType("decimal(18,2)");

        // Optimistic concurrency: EF adds the original Version to the UPDATE's WHERE
        // clause, so a save based on a stale read matches 0 rows and throws.
        wallet.Property(w => w.Version).IsConcurrencyToken();

        // Seed a known wallet so the API is usable immediately.
        wallet.HasData(new { Id = SeedWalletId, Balance = 1000.00m, Version = 0 });
    }
}
