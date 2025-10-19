using MetalSavingsManager.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Data;
public class BankingDbContext : DbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<SavingsPlan> SavingsPlans { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    public DbSet<MetalPrice> MetalPrices { get; set; }
    public DbSet<QuarterlyFee> QuarterlyFees { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints if needed
        base.OnModelCreating(modelBuilder);
    }
}
