using MetalSavingsManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetalSavingsManager.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var transactions = await _transactionService.GetUserTransactionsAsync(Guid.Parse(userId));
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("plan/{planId}/payout")]
    public async Task<IActionResult> ProcessPayout(Guid planId)
    {
        try
        {
            var payout = await _transactionService.ProcessPayoutAsync(planId);
            if (payout == null)
                return NotFound("Savings plan not found.");

            return Ok(new { Message = "Payout processed", Payout = payout });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}