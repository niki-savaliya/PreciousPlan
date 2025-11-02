using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using MetalSavingsManager.Utils;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MetalSavingsManager.Services;

public class MetalPriceService : IMetalPriceService
{
    private readonly HttpClient _httpClient;
    private readonly BankingDbContext _dbContext;
    private const decimal SILVER_OZ_PER_KG = 32.1507m;
    private const decimal QUARTERLY_FEE_RATE = 0.005m;

    public MetalPriceService(HttpClient httpClient, BankingDbContext dbContext)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
    }

    public async Task<decimal> GetLatestPriceInEuroAsync(string metalType)
    {
        var usdToEurRate = await GetUsdToEurRateAsync();
        var pricePerOzUsd = await GetLatestMetalPriceInUsdAsync(metalType);
        var pricePerOzEur = ConvertUsdToEur(pricePerOzUsd, usdToEurRate);

        if (metalType == Constants.Silver)
        {
            pricePerOzEur *= SILVER_OZ_PER_KG;
        }

        return pricePerOzEur;
    }

    public async Task<decimal> GetHistoricalPriceInEuroAsync(string metalType, DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");

        var cachedPrice = await _dbContext.MetalPrices
            .Where(mp => mp.MetalType == metalType && mp.Date == date)
            .FirstOrDefaultAsync();

        if (cachedPrice != null)
        {
            return cachedPrice.PricePerUnit;
        }

        var usdToEurRate = await GetHistoricalUsdToEurRateAsync(date);
        var pricePerOzUsd = await GetHistoricalMetalPriceInUsdAsync(metalType, date);
        var pricePerOzEur = ConvertUsdToEur(pricePerOzUsd, usdToEurRate);

        decimal pricePerBarEur = metalType == Constants.Gold
            ? pricePerOzEur
            : pricePerOzEur * SILVER_OZ_PER_KG;

        await CachePriceAsync(metalType, date, pricePerBarEur);

        return pricePerBarEur;
    }

    public async Task<SimulationKpi> SimulatePlanAsync(string metalType, decimal monthlySavings, DateTime startDate, DateTime endDate)
    {
        var months = GetMonthsBetween(startDate, endDate);
        var state = new SimulationState();
        var yearlyData = new Dictionary<int, YearlySnapshot>();

        foreach (var month in months)
        {
            var usdToEurRate = await GetHistoricalUsdToEurRateAsync(month);
            await ProcessMonthlyTransaction(metalType, monthlySavings, month, usdToEurRate, state);
            UpdateYearlySnapshot(month, state, yearlyData);
        }

        return BuildSimulationResult(state, yearlyData);
    }

    private async Task ProcessMonthlyTransaction(string metalType, decimal deposit, DateTime month, decimal usdToEurRate, SimulationState state)
    {
        state.TotalDeposits += deposit;

        var pricePerOzUsd = await GetHistoricalMetalPriceInUsdAsync(metalType, month);
        var pricePerOzEur = ConvertUsdToEur(pricePerOzUsd, usdToEurRate);
        var pricePerBarEur = CalculatePricePerBar(metalType, pricePerOzEur);

        var barUnitsPurchased = deposit / pricePerBarEur;
        state.TotalUnits += barUnitsPurchased;

        if (IsQuarterEnd(month))
        {
            ApplyQuarterlyFee(state, pricePerBarEur);
        }

        state.CurrentPortfolioValue = state.TotalUnits * pricePerBarEur;
    }

    private void ApplyQuarterlyFee(SimulationState state, decimal pricePerBarEur)
    {
        var portfolioValueBeforeFee = state.TotalUnits * pricePerBarEur;
        var feeAmount = portfolioValueBeforeFee * QUARTERLY_FEE_RATE;
        var feeInUnits = feeAmount / pricePerBarEur;

        state.TotalUnits -= feeInUnits;
    }

    private void UpdateYearlySnapshot(DateTime month, SimulationState state, Dictionary<int, YearlySnapshot> yearlyData)
    {
        var year = month.Year;
        yearlyData[year] = new YearlySnapshot
        {
            CumulativeDeposits = state.TotalDeposits,
            PortfolioValue = state.CurrentPortfolioValue
        };
    }

    private SimulationKpi BuildSimulationResult(SimulationState state, Dictionary<int, YearlySnapshot> yearlyData)
    {
        var years = yearlyData.Keys.OrderBy(y => y).ToList();
        var cumulativeDeposits = years.Select(y => decimal.Round(yearlyData[y].CumulativeDeposits, 2)).ToList();
        var portfolioValues = years.Select(y => decimal.Round(yearlyData[y].PortfolioValue, 2)).ToList();
        var profitLossHistory = portfolioValues.Select((pv, i) => decimal.Round(pv - cumulativeDeposits[i], 2)).ToList();

        var finalProfitLoss = state.CurrentPortfolioValue - state.TotalDeposits;
        var returnRate = state.TotalDeposits == 0 ? 0 : (finalProfitLoss / state.TotalDeposits) * 100;

        return new SimulationKpi
        {
            TotalDeposits = decimal.Round(state.TotalDeposits, 2),
            PortfolioValue = decimal.Round(state.CurrentPortfolioValue, 2),
            ProfitLoss = decimal.Round(finalProfitLoss, 2),
            ReturnRatePercent = decimal.Round(returnRate, 2),
            Years = years.Select(y => y.ToString()).ToList(),
            CumulativeDeposits = cumulativeDeposits,
            PortfolioValues = portfolioValues,
            ProfitLossHistory = profitLossHistory
        };
    }

    private async Task<decimal> GetUsdToEurRateAsync()
    {
        var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/latest.json?app_id={Constants.APP_ID}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("rates").GetProperty("EUR").GetDecimal();
    }

    private async Task<decimal> GetHistoricalUsdToEurRateAsync(DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");

        var cachedRate = await _dbContext.MetalPrices
            .Where(mp => mp.MetalType == "EUR" && mp.Date == date)
            .FirstOrDefaultAsync();

        if (cachedRate != null)
        {
            return cachedRate.PricePerUnit;
        }

        var url = $"{Constants.BaseUrl}/historical/{dateStr}.json?app_id={Constants.APP_ID}&symbols=EUR";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var rate = doc.RootElement.GetProperty("rates").GetProperty("EUR").GetDecimal();

        await CachePriceAsync("EUR", date, rate);

        return rate;
    }

    private async Task<decimal> GetLatestMetalPriceInUsdAsync(string metalType)
    {
        var response = await _httpClient.GetAsync($"{Constants.BaseUrl}/latest.json?app_id={Constants.APP_ID}&symbols={metalType}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var inverseRate = doc.RootElement.GetProperty("rates").GetProperty(metalType).GetDecimal();

        return 1m / inverseRate;
    }

    private async Task<decimal> GetHistoricalMetalPriceInUsdAsync(string metalType, DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        var cacheKey = $"{metalType}_USD";

        var cachedPrice = await _dbContext.MetalPrices
            .Where(mp => mp.MetalType == cacheKey && mp.Date == date)
            .FirstOrDefaultAsync();

        if (cachedPrice != null)
        {
            return cachedPrice.PricePerUnit;
        }

        var url = $"{Constants.BaseUrl}/historical/{dateStr}.json?app_id={Constants.APP_ID}&symbols={metalType}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var inverseRate = doc.RootElement.GetProperty("rates").GetProperty(metalType).GetDecimal();
        var priceUsdPerOz = 1m / inverseRate;

        await CachePriceAsync(cacheKey, date, priceUsdPerOz);

        return priceUsdPerOz;
    }

    private async Task CachePriceAsync(string metalType, DateTime date, decimal price)
    {
        var existingPrice = await _dbContext.MetalPrices
            .FirstOrDefaultAsync(mp => mp.MetalType == metalType && mp.Date == date);

        if (existingPrice == null)
        {
            _dbContext.MetalPrices.Add(new MetalPrice
            {
                Id = Guid.NewGuid(),
                MetalType = metalType,
                Date = date,
                PricePerUnit = price
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    private decimal ConvertUsdToEur(decimal usdAmount, decimal usdToEurRate)
        => usdAmount * usdToEurRate;

    private decimal CalculatePricePerBar(string metalType, decimal pricePerOzEur)
    {
        return metalType switch
        {
            "XAU" => pricePerOzEur,
            "XAG" => pricePerOzEur * SILVER_OZ_PER_KG,
            _ => throw new ArgumentException($"Metal type unsupported: {metalType}")
        };
    }

    private List<DateTime> GetMonthsBetween(DateTime start, DateTime end)
    {
        var months = new List<DateTime>();
        var current = new DateTime(start.Year, start.Month, 15);

        while (current <= end)
        {
            months.Add(current);
            current = current.AddMonths(1);
        }

        return months;
    }

    private bool IsQuarterEnd(DateTime date)
        => date.Month % 3 == 0;
}

internal class SimulationState
{
    public decimal TotalDeposits { get; set; }
    public decimal TotalUnits { get; set; }
    public decimal CurrentPortfolioValue { get; set; }
}

internal class YearlySnapshot
{
    public decimal CumulativeDeposits { get; set; }
    public decimal PortfolioValue { get; set; }
}

public class SimulationKpi
{
    public decimal TotalDeposits { get; set; }
    public decimal PortfolioValue { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal ReturnRatePercent { get; set; }

    public List<string> Years { get; set; } = new();
    public List<decimal> CumulativeDeposits { get; set; } = new();
    public List<decimal> PortfolioValues { get; set; } = new();
    public List<decimal> ProfitLossHistory { get; set; } = new();
}