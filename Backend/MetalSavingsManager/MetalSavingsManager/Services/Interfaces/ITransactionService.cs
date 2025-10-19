using MetalSavingsManager.Data.Model;

namespace MetalSavingsManager.Services.Interfaces;

public interface ITransactionService
{
    Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);
    Task<decimal?> ProcessPayoutAsync(Guid planId);
}
