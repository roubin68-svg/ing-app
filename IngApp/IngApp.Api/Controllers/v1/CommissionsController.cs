using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/commissions")]
[Authorize]
public class CommissionsController : ControllerBase
{
    private readonly ICommissionService _commissionService;

    public CommissionsController(ICommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        return Guid.Parse(claim.Value);
    }

    // GET: دریافت لیست پورسانت‌های بازاریاب
    [HttpGet("my")]
    public async Task<IActionResult> GetMyCommissions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var commissions = await _commissionService.GetVisitorCommissionsAsync(userId, page, pageSize);
        return Ok(ApiResult.Ok(commissions));
    }

    // GET: دریافت مجموع پورسانت‌های بازاریاب
    [HttpGet("my/total")]
    public async Task<IActionResult> GetMyTotalCommission()
    {
        var userId = GetCurrentUserId();
        var totalRial = await _commissionService.GetTotalCommissionAmountAsync(userId);
        return Ok(ApiResult.Ok(new
        {
            TotalAmountRial = totalRial,
            TotalAmountToman = totalRial / 10m
        }));
    }

    // GET: دریافت لیست پورسانت‌های یک بازاریاب (Admin)
    [HttpGet("visitors/{visitorUserId:guid}")]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> GetVisitorCommissions(
        Guid visitorUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var commissions = await _commissionService.GetVisitorCommissionsForAdminAsync(visitorUserId, page, pageSize);
        return Ok(ApiResult.Ok(commissions));
    }

    // GET: دریافت مجموع پورسانت‌های یک بازاریاب (Admin)
    [HttpGet("visitors/{visitorUserId:guid}/total")]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> GetVisitorTotalCommission(Guid visitorUserId)
    {
        var totalRial = await _commissionService.GetTotalCommissionAmountForAdminAsync(visitorUserId);
        return Ok(ApiResult.Ok(new
        {
            TotalAmountRial = totalRial,
            TotalAmountToman = totalRial / 10m
        }));
    }
}










