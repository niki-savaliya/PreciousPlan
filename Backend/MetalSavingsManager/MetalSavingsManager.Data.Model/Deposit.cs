using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MetalSavingsManager.Data.Model;

[System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
public class Deposit : Entity
{
    public DateTime DepositDate { get; set; }
    public decimal Amount { get; set; }

    [ForeignKey(nameof(SavingsPlan))]
    public Guid SavingsPlanId { get; set; }
    [JsonIgnore]
    public virtual SavingsPlan SavingsPlan { get; set; } = null!;
}
