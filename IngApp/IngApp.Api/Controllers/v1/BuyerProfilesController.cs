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
[Route("api/v1/buyer-profiles")]
[Authorize]
public class BuyerProfilesController : ControllerBase
{
    private readonly IBuyerProfileService _service;

    public BuyerProfilesController(IBuyerProfileService service)
    {
        _service = service;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        return Guid.Parse(claim.Value);
    }

    // GET: دریافت پروفایل Buyer کاربر فعلی
    [HttpGet("my")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _service.GetMyProfileAsync(userId);
        
        // اگر پروفایل وجود نداشت، null برمی‌گردانیم (نه NotFound)
        // چون این یک وضعیت طبیعی است و کاربر می‌تواند پروفایل ایجاد کند
        return Ok(ApiResult.Ok(profile));
    }

    // PUT: ایجاد یا به‌روزرسانی پروفایل Buyer کاربر فعلی
    [HttpPut("my")]
    public async Task<IActionResult> UpsertMyProfile([FromBody] UpsertBuyerProfileDto dto)
    {
        var userId = GetCurrentUserId();
        var profile = await _service.UpsertMyProfileAsync(userId, dto);
        return Ok(ApiResult.Ok(profile));
    }
}










