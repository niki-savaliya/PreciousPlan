using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using MetalSavingsManager.Utils;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services;

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

            if (IsQuarterEnd(today))
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
                var metalPriceService = scope.ServiceProvider.GetRequiredService<IMetalPriceService>();

                var activePlans = await db.SavingsPlans
                    .Where(p => p.IsActive)
                    .Include(p => p.Deposits)
                    .Include(p => p.QuarterlyFees)
                    .ToListAsync(stoppingToken);

                foreach (var plan in activePlans)
                {
                    var quarterEndDate = GetQuarterEndDate(today);

                    bool feeExists = await db.QuarterlyFees
                        .AnyAsync(qf => qf.SavingsPlanId == plan.Id && qf.FeeDate == quarterEndDate, stoppingToken);

                    if (!feeExists)
                    {
                        await CalculateAndInsertQuarterlyFee(plan, quarterEndDate, metalPriceService, db, stoppingToken);
                    }
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task CalculateAndInsertQuarterlyFee(
        SavingsPlan plan,
        DateTime quarterEndDate,
        IMetalPriceService metalPriceService,
        BankingDbContext db,
        CancellationToken cancellationToken)
    {
        var metalType = plan.PlanType == PlanType.Gold ? Constants.Gold : Constants.Silver;

        var deposits = plan.Deposits
            .Where(d => d.DepositDate <= quarterEndDate)
            .OrderBy(d => d.DepositDate)
            .ToList();

        var previousFees = plan.QuarterlyFees
            .Where(f => f.FeeDate < quarterEndDate)
            .OrderBy(f => f.FeeDate)
            .ToList();

        decimal currentMetalUnits = 0m;

        foreach (var deposit in deposits)
        {
            var priceAtDeposit = await metalPriceService.GetHistoricalPriceInEuroAsync(metalType, deposit.DepositDate);
            decimal unitsPurchased = deposit.Amount / priceAtDeposit;
            currentMetalUnits += unitsPurchased;
        }

        foreach (var fee in previousFees)
        {
            var priceAtFee = await metalPriceService.GetHistoricalPriceInEuroAsync(metalType, fee.FeeDate);
            decimal unitsDeducted = fee.FeeAmount / priceAtFee;
            currentMetalUnits -= unitsDeducted;
        }

        var priceAtQuarterEnd = await metalPriceService.GetHistoricalPriceInEuroAsync(metalType, quarterEndDate);
        decimal portfolioValue = currentMetalUnits * priceAtQuarterEnd;
        decimal feeAmount = portfolioValue * 0.005m;

        if (feeAmount > 0)
        {
            var fee = new QuarterlyFee
            {
                Id = Guid.NewGuid(),
                SavingsPlanId = plan.Id,
                FeeAmount = Math.Round(feeAmount, 2),
                FeeDate = quarterEndDate
            };
            db.QuarterlyFees.Add(fee);

            var feeTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                SavingsPlanId = plan.Id,
                TransactionType = Constants.Fee,
                TransactionDate = quarterEndDate,
                Amount = Math.Round(feeAmount, 2)
            };
            db.Transactions.Add(feeTransaction);

            await db.SaveChangesAsync(cancellationToken);
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
