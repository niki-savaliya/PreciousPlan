namespace MetalSavingsManager.Data.Model;

public class User : Entity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string BankAccountNumber { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "user";
    public virtual ICollection<SavingsPlan> SavingsPlans { get; set; } = new List<SavingsPlan>();
}
