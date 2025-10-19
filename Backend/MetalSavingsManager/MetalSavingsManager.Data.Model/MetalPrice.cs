namespace MetalSavingsManager.Data.Model;

[System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
public class MetalPrice : Entity
{
    public string MetalType { get; set; } = null!; // "Gold" or "Silver"
    public DateTime Date { get; set; }
    public decimal PricePerUnit { get; set; }
}
