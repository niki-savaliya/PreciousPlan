using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services
{
    public class QuarterlyFeeWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;

        public QuarterlyFeeWorker(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var today = DateTime.UtcNow.Date;

                // Only run on quarter end
                if (IsQuarterEnd(today))
                {
                    using var scope = _provider.CreateScope();
                    var feeService = scope.ServiceProvider.GetRequiredService<IQuarterlyFeeService>();
                    var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

                    var activePlans = await db.SavingsPlans.Where(p => p.IsActive).ToListAsync(stoppingToken);

                    foreach (var plan in activePlans)
                    {
                        var quarterEndDate = GetQuarterEndDate(today);
                        var metalPriceService = scope.ServiceProvider.GetRequiredService<IMetalPriceService>();
                        decimal metalPrice = await metalPriceService.GetLatestPriceInEuroAsync(plan.PlanType.ToString());

                        await InsertQuarterlyFee(plan, quarterEndDate, metalPrice, db, CancellationToken.None);
                    }
                }

                // Wait 24 hours before next check
                try
                {
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore cancellation
                }
            }
        }

        private async Task InsertQuarterlyFee(SavingsPlan plan, DateTime quarterEndDate, decimal metalPrice, BankingDbContext dbContext, CancellationToken cancellationToken)
        {
            var depositAmounts = await dbContext.Deposits
                .Where(d => d.SavingsPlanId == plan.Id && d.DepositDate <= quarterEndDate)
                .Select(d => d.Amount)
                .ToListAsync(cancellationToken);

            decimal totalDeposits = depositAmounts.Sum();
            decimal metalUnits = totalDeposits / metalPrice;
            decimal portfolioValue = metalUnits * metalPrice;
            decimal feeAmount = portfolioValue * 0.005m;

            bool feeExists = await dbContext.QuarterlyFees
                .AnyAsync(qf => qf.SavingsPlanId == plan.Id && qf.FeeDate == quarterEndDate, cancellationToken);

            if (!feeExists)
            {
                var fee = new QuarterlyFee
                {
                    Id = Guid.NewGuid(),
                    SavingsPlanId = plan.Id,
                    FeeAmount = feeAmount,
                    FeeDate = quarterEndDate
                };
                dbContext.QuarterlyFees.Add(fee);

                var feeTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    SavingsPlanId = plan.Id,
                    TransactionType = "Fee",
                    TransactionDate = quarterEndDate,
                    Amount = -feeAmount
                };
                dbContext.Transactions.Add(feeTransaction);

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private bool IsQuarterEnd(DateTime date)
        {
            return date == GetQuarterEndDate(date);
        }

        private DateTime GetQuarterEndDate(DateTime date)
        {
            int quarterMonth = ((date.Month - 1) / 3 + 1) * 3;
            return new DateTime(date.Year, quarterMonth, DateTime.DaysInMonth(date.Year, quarterMonth));
        }
    }
}
