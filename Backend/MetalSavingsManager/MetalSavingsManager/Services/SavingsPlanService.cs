using MetalSavingsManager.Containers;
using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services;

public class SavingsPlanService : ISavingsPlanService
{
    private readonly BankingDbContext _dbContext;
    private readonly IMetalPriceService _metalPriceService;

    public SavingsPlanService(BankingDbContext dbContext, IMetalPriceService metalPriceService)
    {
        _dbContext = dbContext;
        _metalPriceService = metalPriceService;
    }

    public async Task<SavingsPlan> CreateSavingPlanAsync(Guid userId, CreateOrUpdateSavingsPlanRequest request)
    {
        var plan = new SavingsPlan
        {
            Id = Guid.NewGuid(),
            PlanType = request.PlanType,
            MonthlyAmount = request.MonthlyAmount,
            StartDate = DateTime.UtcNow,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UserId = userId
        };

        _dbContext.SavingsPlans.Add(plan);
        await _dbContext.SaveChangesAsync();

        return plan;
    }

    public async Task<List<SavingsPlan>> GetUserSavingsPlansAsync(Guid userId)
    {
        return await _dbContext.SavingsPlans
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<SavingsPlan?> GetSavingsPlanDetailsAsync(Guid userId, Guid planId)
    {
        return await _dbContext.SavingsPlans
            .Include(p => p.Deposits)
            .Include(p => p.QuarterlyFees)
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);
    }

    public async Task<decimal?> EstimatePayoutAsync(Guid planId)
    {
        var plan = await _dbContext.SavingsPlans
            .Include(p => p.Deposits)
            .Include(p => p.QuarterlyFees)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
            return null;

        decimal metalPrice = plan.PlanType switch
        {
            PlanType.Gold => await _metalPriceService.GetLatestPriceInEuroAsync("XAU"),
            PlanType.Silver => await _metalPriceService.GetLatestPriceInEuroAsync("XAG"),
            _ => throw new ArgumentException("Unsupported plan type")
        };

        decimal totalDeposits = plan.Deposits?.Sum(d => d.Amount) ?? 0;
        decimal totalFees = plan.QuarterlyFees?.Sum(f => f.FeeAmount) ?? 0;

        decimal metalUnits = totalDeposits / metalPrice;
        decimal estimatedPayout = (metalUnits * metalPrice) - totalFees;

        return estimatedPayout < 0 ? 0 : Math.Round(estimatedPayout, 2);
    }

    public async Task<KPIContainer> GetKpisAsync(Guid userId)
    {
        var activePlans = await _dbContext.SavingsPlans
            .Where(sp => sp.UserId == userId && sp.IsActive)
            .ToListAsync();

        decimal totalDeposited = 0m;
        decimal totalPortfolioValue = 0m;
        decimal totalFeesPaid = 0m;
        decimal totalMetalUnits = 0m;
        decimal totalGoldUnits = 0m;
        decimal totalSilverUnits = 0m;

        var goldPriceTask = _metalPriceService.GetLatestPriceInEuroAsync("XAU");
        var silverPriceTask = _metalPriceService.GetLatestPriceInEuroAsync("XAG");
        await Task.WhenAll(goldPriceTask, silverPriceTask);

        decimal goldPrice = goldPriceTask.Result;
        decimal silverPrice = silverPriceTask.Result;

        foreach (var plan in activePlans)
        {
            var deposits = await _dbContext.Deposits
                .Where(d => d.SavingsPlanId == plan.Id)
                .Select(d => d.Amount)
                .ToListAsync();

            decimal depositsSum = deposits.Sum();
            totalDeposited += depositsSum;

            decimal metalPrice = plan.PlanType == PlanType.Gold ? goldPrice : silverPrice;
            decimal metalUnits = depositsSum / metalPrice;

            if (plan.PlanType == PlanType.Gold)
                totalGoldUnits += metalUnits;
            else if (plan.PlanType == PlanType.Silver)
                totalSilverUnits += metalUnits;

            totalMetalUnits += metalUnits;
            totalPortfolioValue += metalUnits * metalPrice;

            var fees = await _dbContext.QuarterlyFees
                .Where(f => f.SavingsPlanId == plan.Id)
                .Select(f => f.FeeAmount)
                .ToListAsync();

            totalFeesPaid += fees.Sum();
        }

        decimal adjustedValue = totalPortfolioValue - totalFeesPaid;
        decimal profitLoss = adjustedValue - totalDeposited;

        return new KPIContainer
        {
            TotalDeposited = Math.Round(totalDeposited, 2),
            PortfolioValue = Math.Round(adjustedValue, 2),
            ProfitLoss = Math.Round(profitLoss, 2),
            FullyPurchasedBars = Math.Round(totalMetalUnits, 2),
            TotalMetalUnits = Math.Round(totalMetalUnits, 2),
            TotalGoldUnits = Math.Round(totalGoldUnits, 2),
            TotalSilverUnits = Math.Round(totalSilverUnits, 2),
            QuarterlyFeesPaid = Math.Round(totalFeesPaid, 2),
            ActivePlansCount = activePlans.Count
        };
    }
}