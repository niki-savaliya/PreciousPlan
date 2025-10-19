using MetalSavingsManager.Containers;
using MetalSavingsManager.Data.Model;

namespace MetalSavingsManager.Services.Interfaces;

public interface ISavingsPlanService
{
    Task<SavingsPlan> CreateSavingPlanAsync(Guid userId, CreateOrUpdateSavingsPlanRequest request);
    Task<List<SavingsPlan>> GetUserSavingsPlansAsync(Guid userId);
    Task<SavingsPlan?> GetSavingsPlanDetailsAsync(Guid userId, Guid planId);
    Task<decimal?> EstimatePayoutAsync(Guid planId);
    Task<KPIContainer> GetKpisAsync(Guid userId);
}
