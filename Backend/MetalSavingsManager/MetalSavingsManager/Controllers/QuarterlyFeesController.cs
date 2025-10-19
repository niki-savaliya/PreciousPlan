using MetalSavingsManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetalSavingsManager.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QuarterlyFeesController : ControllerBase
{
    private readonly IQuarterlyFeeService _quarterlyFeeService;

    public QuarterlyFeesController(IQuarterlyFeeService quarterlyFeeService)
    {
        _quarterlyFeeService = quarterlyFeeService;
    }

    [HttpGet("plan/{planId}")]
    public async Task<IActionResult> GetFeesForPlan(Guid planId)
    {
        try
        {
            var fees = await _quarterlyFeeService.GetFeesForPlanAsync(planId);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}