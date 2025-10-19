using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
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

        decimal metalPrice = plan.PlanType switch
        {
            PlanType.Gold => await _metalPriceService.GetLatestPriceInEuroAsync("XAU"),
            PlanType.Silver => await _metalPriceService.GetLatestPriceInEuroAsync("XAG"),
            _ => throw new ArgumentException("Unsupported plan type")
        };

        decimal totalDeposits = plan.Deposits?.Sum(d => d.Amount) ?? 0;
        decimal totalFees = plan.QuarterlyFees?.Sum(f => f.FeeAmount) ?? 0;

        decimal metalUnits = totalDeposits / metalPrice;
        decimal payoutAmount = (metalUnits * metalPrice) - totalFees;
        if (payoutAmount < 0) payoutAmount = 0;

        // Mark plan as ended
        plan.EndDate = DateTime.UtcNow;
        plan.IsActive = false;

        var payoutTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            SavingsPlanId = planId,
            TransactionType = "Payout",
            Amount = Math.Round(payoutAmount, 2),
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(payoutTransaction);
        await _context.SaveChangesAsync();

        return Math.Round(payoutAmount, 2);
    }
}