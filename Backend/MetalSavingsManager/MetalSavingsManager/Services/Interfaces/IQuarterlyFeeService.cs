using MetalSavingsManager.Data.Model;

namespace MetalSavingsManager.Services.Interfaces;

public interface IQuarterlyFeeService
{
    Task<List<QuarterlyFee>> GetFeesForPlanAsync(Guid planId);
}