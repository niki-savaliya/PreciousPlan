namespace MetalSavingsManager.Services.Interfaces;

public interface IMetalPriceService
{
    Task<decimal> GetLatestPriceInEuroAsync(string metalType);
    Task<decimal> GetHistoricalPriceInEuroAsync(string metalType, DateTime date);
    Task<SimulationKpi> SimulatePlanAsync(string metalType, decimal monthlySavings, DateTime startDate, DateTime endDate);
}
