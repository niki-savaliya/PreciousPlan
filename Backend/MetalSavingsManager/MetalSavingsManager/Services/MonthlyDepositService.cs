using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services;

public class MonthlyDepositService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MonthlyDepositService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime today = DateTime.UtcNow.Date;

            // Check if today is the 15th
            if (today.Day == 15)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

                    // Get active plans without a deposit for this month
                    var plans = await dbContext.SavingsPlans
                        .Where(p => p.IsActive)
                        .ToListAsync();

                    foreach (var plan in plans)
                    {
                        // Check if deposit for this month already exists to avoid duplicates
                        bool depositExists = await dbContext.Deposits
                            .AnyAsync(d => d.SavingsPlanId == plan.Id &&
                                d.DepositDate.Year == today.Year &&
                                d.DepositDate.Month == today.Month);

                        if (!depositExists)
                        {
                            // Create deposit record dated 15th of current month
                            var deposit = new Deposit
                            {
                                Id = Guid.NewGuid(),
                                SavingsPlanId = plan.Id,
                                DepositDate = today,
                                Amount = plan.MonthlyAmount     // assuming Amount is stored as string
                            };
                            await dbContext.Deposits.AddAsync(deposit);

                            // Create corresponding transaction record
                            var transaction = new Transaction
                            {
                                Id = Guid.NewGuid(),
                                SavingsPlanId = plan.Id,
                                TransactionType = "Deposit",
                                TransactionDate = today,
                                Amount = plan.MonthlyAmount
                            };
                            await dbContext.Transactions.AddAsync(transaction);

                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            }

            // Wait until next day
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}