using MetalSavingsManager.Containers;
using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using MetalSavingsManager.Utils;
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

        var metalType = plan.PlanType == PlanType.Gold ? Constants.Gold : Constants.Silver;
        var currentPrice = await _metalPriceService.GetLatestPriceInEuroAsync(metalType);

        var deposits = plan.Deposits.OrderBy(d => d.DepositDate).ToList();
        var fees = plan.QuarterlyFees.OrderBy(f => f.FeeDate).ToList();

        decimal currentMetalUnits = 0m;

        foreach (var deposit in deposits)
        {
            var priceAtDeposit = await _metalPriceService.GetHistoricalPriceInEuroAsync(metalType, deposit.DepositDate);
            decimal unitsPurchased = deposit.Amount / priceAtDeposit;
            currentMetalUnits += unitsPurchased;
        }

        foreach (var fee in fees)
        {
            var priceAtFee = await _metalPriceService.GetHistoricalPriceInEuroAsync(metalType, fee.FeeDate);
            decimal unitsDeducted = fee.FeeAmount / priceAtFee;
            currentMetalUnits -= unitsDeducted;
        }

        decimal payoutAmount = currentMetalUnits * currentPrice;
        if (payoutAmount < 0) payoutAmount = 0;

        return payoutAmount;
    }

    public async Task<KPIContainer> GetKpisAsync(Guid userId)
    {
        var activePlans = await _dbContext.SavingsPlans
            .Where(sp => sp.UserId == userId && sp.IsActive)
            .Include(sp => sp.Deposits)
            .Include(sp => sp.QuarterlyFees)
            .ToListAsync();

        if (!activePlans.Any())
        {
            return new KPIContainer
            {
                TotalDeposited = 0,
                PortfolioValue = 0,
                ProfitLoss = 0,
                FullyPurchasedBars = 0,
                TotalMetalUnits = 0,
                TotalGoldUnits = 0,
                TotalSilverUnits = 0,
                QuarterlyFeesPaid = 0,
                ActivePlansCount = 0
            };
        }

        var goldPriceTask = _metalPriceService.GetLatestPriceInEuroAsync(Constants.Gold);
        var silverPriceTask = _metalPriceService.GetLatestPriceInEuroAsync(Constants.Silver);
        await Task.WhenAll(goldPriceTask, silverPriceTask);

        decimal currentGoldPricePerBar = goldPriceTask.Result;
        decimal currentSilverPricePerBar = silverPriceTask.Result;

        decimal totalDeposited = 0m;
        decimal totalPortfolioValue = 0m;
        decimal totalFeesPaid = 0m;
        decimal totalGoldUnits = 0m;
        decimal totalSilverUnits = 0m;

        foreach (var plan in activePlans)
        {
            var planKpis = await CalculatePlanKpisAsync(plan, currentGoldPricePerBar, currentSilverPricePerBar);

            totalDeposited += planKpis.TotalDeposited;
            totalPortfolioValue += planKpis.CurrentPortfolioValue;
            totalFeesPaid += planKpis.TotalFeesPaid;

            if (plan.PlanType == PlanType.Gold)
                totalGoldUnits += planKpis.CurrentMetalUnits;
            else if (plan.PlanType == PlanType.Silver)
                totalSilverUnits += planKpis.CurrentMetalUnits;
        }

        decimal totalMetalUnits = totalGoldUnits + totalSilverUnits;
        decimal profitLoss = totalPortfolioValue - totalDeposited;

        return new KPIContainer
        {
            TotalDeposited = Math.Round(totalDeposited, 2),
            PortfolioValue = Math.Round(totalPortfolioValue, 2),
            ProfitLoss = Math.Round(profitLoss, 2),
            FullyPurchasedBars = Math.Round(Math.Floor(totalMetalUnits), 0),
            TotalMetalUnits = Math.Round(totalMetalUnits, 6),
            TotalGoldUnits = Math.Round(totalGoldUnits, 6),
            TotalSilverUnits = Math.Round(totalSilverUnits, 6),
            QuarterlyFeesPaid = Math.Round(totalFeesPaid, 2),
            ActivePlansCount = activePlans.Count
        };
    }

    private async Task<PlanKpiResult> CalculatePlanKpisAsync(SavingsPlan plan, decimal currentGoldPricePerBar, decimal currentSilverPricePerBar)
    {
        var deposits = plan.Deposits.OrderBy(d => d.DepositDate).ToList();
        var fees = plan.QuarterlyFees.OrderBy(f => f.FeeDate).ToList();

        decimal totalDeposited = 0m;
        decimal totalFeesPaid = 0m;
        decimal currentMetalUnits = 0m;

        var metalType = plan.PlanType == PlanType.Gold ? Constants.Gold : Constants.Silver;

        foreach (var deposit in deposits)
        {
            totalDeposited += deposit.Amount;

            decimal pricePerBarAtDeposit = await _metalPriceService.GetHistoricalPriceInEuroAsync(metalType, deposit.DepositDate);
            decimal unitsPurchased = deposit.Amount / pricePerBarAtDeposit;
            currentMetalUnits += unitsPurchased;
        }

        foreach (var fee in fees)
        {
            totalFeesPaid += fee.FeeAmount;

            decimal pricePerBarAtFee = await _metalPriceService.GetHistoricalPriceInEuroAsync(metalType, fee.FeeDate);
            decimal unitsDeducted = fee.FeeAmount / pricePerBarAtFee;
            currentMetalUnits -= unitsDeducted;
        }

        decimal currentPricePerBar = plan.PlanType == PlanType.Gold ? currentGoldPricePerBar : currentSilverPricePerBar;
        decimal currentPortfolioValue = currentMetalUnits * currentPricePerBar;

        return new PlanKpiResult
        {
            TotalDeposited = totalDeposited,
            TotalFeesPaid = totalFeesPaid,
            CurrentMetalUnits = currentMetalUnits,
            CurrentPortfolioValue = currentPortfolioValue
        };
    }

    internal class PlanKpiResult
    {
        public decimal TotalDeposited { get; set; }
        public decimal TotalFeesPaid { get; set; }
        public decimal CurrentMetalUnits { get; set; }
        public decimal CurrentPortfolioValue { get; set; }
    }
}