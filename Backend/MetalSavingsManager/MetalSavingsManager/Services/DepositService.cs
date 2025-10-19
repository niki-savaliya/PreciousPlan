using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services;

public class DepositService : IDepositService
{
    private readonly BankingDbContext _context;

    public DepositService(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Deposit>> GetDepositsForPlanAsync(Guid planId)
    {
        return await _context.Deposits
            .Where(d => d.SavingsPlanId == planId)
            .OrderBy(d => d.DepositDate)
            .ToListAsync();
    }

    public async Task AutoGenerateMonthlyDepositsAsync()
    {
        var today = DateTime.UtcNow.Date;

        var activePlans = await _context.SavingsPlans
            .Where(p => p.IsActive)
            .Include(p => p.Deposits)
            .ToListAsync();

        foreach (var plan in activePlans)
        {
            var lastDepositDate = plan.Deposits
                .OrderByDescending(d => d.DepositDate)
                .FirstOrDefault()?.DepositDate ?? plan.StartDate.AddMonths(-1);

            var nextDepositMonth = new DateTime(lastDepositDate.Year, lastDepositDate.Month, 15).AddMonths(1);

            while (nextDepositMonth <= today)
            {
                var newDeposit = new Deposit
                {
                    Id = Guid.NewGuid(),
                    SavingsPlanId = plan.Id,
                    Amount = plan.MonthlyAmount,
                    DepositDate = nextDepositMonth
                };

                _context.Deposits.Add(newDeposit);
                nextDepositMonth = nextDepositMonth.AddMonths(1);
            }
        }

        await _context.SaveChangesAsync();
    }
}