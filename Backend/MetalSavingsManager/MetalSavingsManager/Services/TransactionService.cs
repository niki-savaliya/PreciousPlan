using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using MetalSavingsManager.Utils;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services;

public class TransactionService : ITransactionService
{
    private readonly BankingDbContext _context;
    private readonly IMetalPriceService _metalPriceService;

    public TransactionService(BankingDbContext context, IMetalPriceService metalPriceService)
    {
        _context = context;
        _metalPriceService = metalPriceService;
    }

    public async Task<List<Transaction>> GetUserTransactionsAsync(Guid userId)
    {
        return await _context.Transactions
            .Include(t => t.SavingsPlan)
            .Where(t => t.SavingsPlan.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<decimal?> ProcessPayoutAsync(Guid planId)
    {
        var plan = await _context.SavingsPlans
            .Include(p => p.Deposits)
            .Include(p => p.QuarterlyFees)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
            return null;

        if (!plan.IsActive)
            throw new InvalidOperationException("Savings plan already ended.");

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

        plan.EndDate = DateTime.UtcNow;
        plan.IsActive = false;

        var payoutTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            SavingsPlanId = planId,
            TransactionType = Constants.Payout,
            Amount = Math.Round(payoutAmount, 2),
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(payoutTransaction);
        await _context.SaveChangesAsync();

        return Math.Round(payoutAmount, 2);
    }
}