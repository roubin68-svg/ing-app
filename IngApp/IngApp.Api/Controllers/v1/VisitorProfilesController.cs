using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/visitor-profiles")]
[Authorize]
public class VisitorProfilesController : ControllerBase
{
    private readonly IVisitorProfileService _service;
    private readonly IVisitorManagementService _visitorManagementService;

    public VisitorProfilesController(
        IVisitorProfileService service,
        IVisitorManagementService visitorManagementService)
    {
        _service = service;
        _visitorManagementService = visitorManagementService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        return Guid.Parse(claim.Value);
    }

    // GET: دریافت پروفایل Visitor کاربر فعلی
    [HttpGet("my")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _service.GetMyProfileAsync(userId);
        
        if (profile == null)
            return NotFound(ApiResult.Fail("پروفایل Visitor یافت نشد."));
        
        return Ok(ApiResult.Ok(profile));
    }

    // PUT: ایجاد یا به‌روزرسانی پروفایل Visitor کاربر فعلی
    [HttpPut("my")]
    public async Task<IActionResult> UpsertMyProfile([FromBody] UpsertVisitorProfileDto dto)
    {
        var userId = GetCurrentUserId();
        var profile = await _service.UpsertMyProfileAsync(userId, dto);
        return Ok(ApiResult.Ok(profile));
    }

    // GET: دریافت پروفایل Visitor بر اساس ReferralCode (برای استفاده عمومی)
    [HttpGet("by-code/{referralCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByReferralCode(string referralCode)
    {
        var profile = await _service.GetByReferralCodeAsync(referralCode);
        
        if (profile == null)
            return NotFound(ApiResult.Fail("کد معرف یافت نشد."));
        
        return Ok(ApiResult.Ok(profile));
    }

    // GET: دریافت لیست Buyer های Visitor فعلی
    [HttpGet("my/buyers")]
    public async Task<IActionResult> GetMyBuyers()
    {
        var userId = GetCurrentUserId();
        var profile = await _service.GetMyProfileAsync(userId);
        
        if (profile == null)
            return NotFound(ApiResult.Fail("پروفایل Visitor یافت نشد."));
        
        var buyers = await _visitorManagementService.GetBuyersAsync(profile.Id);
        return Ok(ApiResult.Ok(buyers));
    }

    // POST: اضافه کردن Buyer به Visitor فعلی
    [HttpPost("my/buyers")]
    public async Task<IActionResult> AddMyBuyer([FromBody] AddBuyerToVisitorDto dto)
    {
        var userId = GetCurrentUserId();
        var profile = await _service.GetMyProfileAsync(userId);
        
        if (profile == null)
            return NotFound(ApiResult.Fail("پروفایل Visitor یافت نشد."));
        
        var buyer = await _visitorManagementService.AddBuyerAsync(profile.Id, dto);
        return Ok(ApiResult.Ok(buyer));
    }
}









