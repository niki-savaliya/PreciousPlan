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
}