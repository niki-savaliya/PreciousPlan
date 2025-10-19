namespace MetalSavingsManager.Services.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string storedHash);
    string GenerateJwtToken(Guid userId, string role);
}
