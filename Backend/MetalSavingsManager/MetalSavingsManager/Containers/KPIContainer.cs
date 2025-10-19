namespace MetalSavingsManager.Containers;

public class KPIContainer
{
    public decimal TotalDeposited { get; set; }
    public decimal PortfolioValue { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal FullyPurchasedBars { get; set; }
    public decimal TotalMetalUnits { get; set; }
    public decimal TotalGoldUnits { get; set; }
    public decimal TotalSilverUnits { get; set; }
    public decimal QuarterlyFeesPaid { get; set; }
    public int ActivePlansCount { get; set; }
}
