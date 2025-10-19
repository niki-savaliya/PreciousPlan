namespace MetalSavingsManager.Services.Interfaces;

public interface IMetalPriceService
{
    Task<decimal> GetLatestPriceInEuroAsync(string metalType);
}
