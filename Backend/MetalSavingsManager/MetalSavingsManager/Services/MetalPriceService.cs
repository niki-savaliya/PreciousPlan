using MetalSavingsManager.Services.Interfaces;
using MetalSavingsManager.Utils;
using System.Text.Json;

namespace MetalSavingsManager.Services;

public class MetalPriceService : IMetalPriceService
{
    private readonly HttpClient _httpClient;

    public MetalPriceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetLatestPriceInEuroAsync(string metalType)
    {
        // Call the external API
        var response = await _httpClient.GetAsync("https://openexchangerates.org/api/latest.json?app_id=" + Constants.APP_ID);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();

        // Parse JSON
        var jsonDoc = JsonDocument.Parse(jsonResponse);
        var rates = jsonDoc.RootElement.GetProperty("rates");
        decimal usdToEurRate = rates.GetProperty("EUR").GetDecimal();

        // Placeholder for fetching the actual metal price in USD, e.g., via another API
        decimal metalPriceInUsd = await FetchMetalPriceInUsd(metalType);

        // Convert USD price to EUR
        var priceInEuro = metalPriceInUsd * usdToEurRate;
        return priceInEuro;
    }

    private Task<decimal> FetchMetalPriceInUsd(string metalType)
    {
        return Task.FromResult(metalType == "XAU" ? 1800m : 25m); // Gold or Silver approximate
    }
}
