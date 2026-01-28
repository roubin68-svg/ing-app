using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Users.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/buyer-management")]
[Authorize]
public class BuyerManagementController : ControllerBase
{
    private readonly IBuyerManagementService _buyerManagementService;

    public BuyerManagementController(IBuyerManagementService buyerManagementService)
    {
        _buyerManagementService = buyerManagementService;
    }

    // -------------------- GET Paged List --------------------
    [HttpGet]
    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> GetPaged([FromQuery] BuyerListQueryDto filter)
    {
        try
        {
            var result = await _buyerManagementService.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در دریافت لیست خریداران: {ex.Message}"));
        }
    }

    // -------------------- GET By Id --------------------
    [HttpGet("{buyerProfileId:guid}")]
    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> GetById(Guid buyerProfileId)
    {
        try
        {
            var buyer = await _buyerManagementService.GetByIdAsync(buyerProfileId);

            if (buyer == null)
                return NotFound(ApiResult.Fail("خریدار مورد نظر یافت نشد."));

            return Ok(ApiResult.Ok(buyer));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در دریافت اطلاعات خریدار: {ex.Message}"));
        }
    }

    // -------------------- CREATE Buyer --------------------
    [HttpPost]
    [Authorize(Policy = Permissions.Users.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateBuyerDto dto)
    {
        try
        {
            if (dto == null)
                return BadRequest(ApiResult.Fail("اطلاعات خریدار ارسال نشده است."));

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest(ApiResult.Fail("شماره موبایل الزامی است."));

            var created = await _buyerManagementService.CreateAsync(dto);
            return Ok(ApiResult.Ok(created, "خریدار با موفقیت ایجاد شد."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در ایجاد خریدار: {ex.Message}"));
        }
    }

    // -------------------- UPDATE Buyer --------------------
    [HttpPut("{buyerProfileId:guid}")]
    [Authorize(Policy = Permissions.Users.Manage)]
    public async Task<IActionResult> Update(Guid buyerProfileId, [FromBody] UpdateBuyerDto dto)
    {
        try
        {
            var updated = await _buyerManagementService.UpdateAsync(buyerProfileId, dto);
            return Ok(ApiResult.Ok(updated, "خریدار با موفقیت به‌روزرسانی شد."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در به‌روزرسانی خریدار: {ex.Message}"));
        }
    }

    // -------------------- SET Referral --------------------
    [HttpPut("{buyerProfileId:guid}/referral")]
    [Authorize(Policy = Permissions.Users.Manage)]
    public async Task<IActionResult> SetReferral(Guid buyerProfileId, [FromBody] SetBuyerReferralDto dto)
    {
        // ValidationException و NotFoundException توسط ApiExceptionMiddleware handle می‌شوند
        var updated = await _buyerManagementService.SetReferralAsync(buyerProfileId, dto);
        return Ok(ApiResult.Ok(updated, "بازاریاب با موفقیت تنظیم شد."));
    }

    // -------------------- REMOVE Referral --------------------
    [HttpDelete("{buyerProfileId:guid}/referral")]
    [Authorize(Policy = Permissions.Users.Manage)]
    public async Task<IActionResult> RemoveReferral(Guid buyerProfileId)
    {
        try
        {
            await _buyerManagementService.RemoveReferralAsync(buyerProfileId);
            return Ok(ApiResult.Ok("بازاریاب با موفقیت حذف شد."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در حذف بازاریاب: {ex.Message}"));
        }
    }

    // -------------------- DELETE Buyer --------------------
    [HttpDelete("{buyerProfileId:guid}")]
    [Authorize(Policy = Permissions.Users.Manage)]
    public async Task<IActionResult> Delete(Guid buyerProfileId)
    {
        try
        {
            await _buyerManagementService.DeleteAsync(buyerProfileId);
            return Ok(ApiResult.Ok("خریدار با موفقیت حذف شد."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در حذف خریدار: {ex.Message}"));
        }
    }
}

