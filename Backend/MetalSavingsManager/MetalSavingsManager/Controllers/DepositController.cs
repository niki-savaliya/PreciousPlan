using MetalSavingsManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetalSavingsManager.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DepositController : ControllerBase
    {
        private readonly IDepositService _depositService;

        public DepositController(IDepositService depositService)
        {
            _depositService = depositService;
        }

        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetDepositsForPlan(Guid planId)
        {
            try
            {

                var deposits = await _depositService.GetDepositsForPlanAsync(planId);
                return Ok(deposits);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("auto-generate")]
        public async Task<IActionResult> AutoGenerateMonthlyDeposits()
        {
            try
            {
                await _depositService.AutoGenerateMonthlyDepositsAsync();
                return Ok("Monthly deposits generated for all active savings plans.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}