using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalSavingsManager.Services;

public class UserService : IUserService
{
    private readonly BankingDbContext _context;

    public UserService(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<bool> UpdateUserAsync(Guid id, User updatedUser)
    {
        if (id != updatedUser.Id)
            return false;

        _context.Entry(updatedUser).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == id))
                return false;

            throw;
        }
    }
}
