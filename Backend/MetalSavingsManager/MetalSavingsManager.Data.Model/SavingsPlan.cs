using System.ComponentModel.DataAnnotations.Schema;

namespace MetalSavingsManager.Data.Model;

[System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
public class SavingsPlan : Entity
{
    public PlanType PlanType { get; set; }
    public decimal MonthlyAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
    public virtual ICollection<QuarterlyFee> QuarterlyFees { get; set; } = new List<QuarterlyFee>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}


public enum PlanType
{
    Gold = 1,
    Silver = 2
}