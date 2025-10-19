using MetalSavingsManager.Data.Model;

namespace MetalSavingsManager.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<bool> UpdateUserAsync(Guid id, User updatedUser);
}
