using MetalSavingsManager.Data.Model;

namespace MetalSavingsManager.Services.Interfaces;

public interface IDepositService
{
    Task<List<Deposit>> GetDepositsForPlanAsync(Guid planId);
}
