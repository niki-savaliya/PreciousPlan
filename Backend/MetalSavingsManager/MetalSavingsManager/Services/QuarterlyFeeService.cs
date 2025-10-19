using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class QuarterlyFeeService : IQuarterlyFeeService
{
    private readonly BankingDbContext _dbContext;
    public QuarterlyFeeService(BankingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<QuarterlyFee>> GetFeesForPlanAsync(Guid planId)
    {
        return await _dbContext.QuarterlyFees
            .Where(f => f.SavingsPlanId == planId)
            .OrderBy(f => f.FeeDate)
            .ToListAsync();
    }
}
