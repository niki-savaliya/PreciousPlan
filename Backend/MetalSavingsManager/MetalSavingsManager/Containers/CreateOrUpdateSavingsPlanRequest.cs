using MetalSavingsManager.Data.Model;

namespace MetalSavingsManager.Containers;


public class CreateOrUpdateSavingsPlanRequest
{
    public Guid? Id { get; set; }
    public bool IsActive { get; set; }
    public PlanType PlanType { get; set; }
    public decimal MonthlyAmount { get; set; }
}

public class SimulationRequest
{
    public string PlanType { get; set; }
    public decimal MonthlyAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}