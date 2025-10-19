using MetalSavingsManager.Containers;
using MetalSavingsManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MetalSavingsManager.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SavingsPlanController : ControllerBase
{
    private readonly ISavingsPlanService _savingsPlanService;

    public SavingsPlanController(ISavingsPlanService savingsPlanService)
    {
        _savingsPlanService = savingsPlanService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSavingPlan([FromBody] CreateOrUpdateSavingsPlanRequest request)
    {
        try
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var plan = await _savingsPlanService.CreateSavingPlanAsync(Guid.Parse(userId), request);
            return Ok(new { Message = "Saving plan created successfully", PlanId = plan.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user-plans")]
    public async Task<IActionResult> GetUserSavingsPlans()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var plans = await _savingsPlanService.GetUserSavingsPlansAsync(Guid.Parse(userId));
            return Ok(plans);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{planId}")]
    public async Task<IActionResult> GetSavingsPlanDetails(Guid planId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var plan = await _savingsPlanService.GetSavingsPlanDetailsAsync(Guid.Parse(userId), planId);
            if (plan == null)
                return NotFound();

            return Ok(plan);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{planId}/estimate-payout")]
    public async Task<IActionResult> EstimatePayout(Guid planId)
    {
        try
        {
            var payout = await _savingsPlanService.EstimatePayoutAsync(planId);
            if (payout == null)
                return NotFound();

            return Ok(new { amount = payout });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetAllKPIs()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var result = await _savingsPlanService.GetKpisAsync(Guid.Parse(userId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}