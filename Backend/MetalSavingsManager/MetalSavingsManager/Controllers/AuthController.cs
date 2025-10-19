using MetalSavingsManager.Containers;
using MetalSavingsManager.Data;
using MetalSavingsManager.Data.Model;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly BankingDbContext _dbContext;

    public AuthController(IAuthService authService, BankingDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        try
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email already in use.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = user.Name,
                Email = user.Email,
                BankAccountNumber = user.BankAccountNumber,
                CreatedDate = DateTime.UtcNow,
                PasswordHash = _authService.HashPassword(user.PasswordHash),
                Role = user.Role
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User successfully registered." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Username);
            if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return NotFound();
            }

            var token = _authService.GenerateJwtToken(user.Id, user.Role);
            return Ok(new { Token = token, UserId = user.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}