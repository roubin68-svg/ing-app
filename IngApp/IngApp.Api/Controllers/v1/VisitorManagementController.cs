using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Users.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/visitor-management")]
    [Authorize]
    public class VisitorManagementController : ControllerBase
    {
        private readonly IVisitorManagementService _visitorManagementService;

        public VisitorManagementController(IVisitorManagementService visitorManagementService)
        {
            _visitorManagementService = visitorManagementService;
        }

        // -------------------- GET Paged List --------------------
        [HttpGet]
        [Authorize(Policy = Permissions.Visitors.View)]
        public async Task<IActionResult> GetPaged([FromQuery] VisitorListQueryDto filter)
        {
            try
            {
                var result = await _visitorManagementService.GetPagedAsync(filter);
                return Ok(ApiResult.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Fail($"خطا در دریافت لیست Visitor ها: {ex.Message}"));
            }
        }

        // -------------------- GET By Id --------------------
        [HttpGet("{visitorProfileId:guid}")]
        [Authorize(Policy = Permissions.Visitors.View)]
        public async Task<IActionResult> GetById(Guid visitorProfileId)
        {
            var visitor = await _visitorManagementService.GetByIdAsync(visitorProfileId);

            if (visitor == null)
                return NotFound(ApiResult.Fail("Visitor مورد نظر یافت نشد."));

            return Ok(ApiResult.Ok(visitor));
        }

        // -------------------- CREATE Visitor --------------------
        [HttpPost]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> Create([FromBody] CreateVisitorDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(ApiResult.Fail("اطلاعات Visitor ارسال نشده است."));

                if (dto.UserId == Guid.Empty)
                    return BadRequest(ApiResult.Fail("شناسه کاربر الزامی است."));

                var created = await _visitorManagementService.CreateAsync(dto);
                return Ok(ApiResult.Ok(created, "Visitor با موفقیت ایجاد شد."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult.Fail($"خطا در ایجاد Visitor: {ex.Message}"));
            }
        }

        // -------------------- UPDATE Visitor --------------------
        [HttpPut("{visitorProfileId:guid}")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> Update(Guid visitorProfileId, [FromBody] UpdateVisitorDto dto)
        {
            var updated = await _visitorManagementService.UpdateAsync(visitorProfileId, dto);
            return Ok(ApiResult.Ok(updated));
        }

        // -------------------- CHANGE STATUS --------------------
        [HttpPut("{visitorProfileId:guid}/status")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> ChangeStatus(Guid visitorProfileId, [FromBody] bool isActive)
        {
            await _visitorManagementService.ChangeStatusAsync(visitorProfileId, isActive);
            return Ok(ApiResult.Ok());
        }

        // -------------------- DELETE Visitor --------------------
        [HttpDelete("{visitorProfileId:guid}")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> Delete(Guid visitorProfileId)
        {
            await _visitorManagementService.DeleteAsync(visitorProfileId);
            return Ok(ApiResult.Ok());
        }

        // -------------------- GET Buyers --------------------
        [HttpGet("{visitorProfileId:guid}/buyers")]
        [Authorize(Policy = Permissions.Visitors.View)]
        public async Task<IActionResult> GetBuyers(Guid visitorProfileId)
        {
            var buyers = await _visitorManagementService.GetBuyersAsync(visitorProfileId);
            return Ok(ApiResult.Ok(buyers));
        }

        // -------------------- ADD Buyer --------------------
        [HttpPost("{visitorProfileId:guid}/buyers")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> AddBuyer(Guid visitorProfileId, [FromBody] AddBuyerToVisitorDto dto)
        {
            var buyer = await _visitorManagementService.AddBuyerAsync(visitorProfileId, dto);
            return Ok(ApiResult.Ok(buyer));
        }

        // -------------------- REMOVE Buyer --------------------
        [HttpDelete("{visitorProfileId:guid}/buyers/{buyerProfileId:guid}")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> RemoveBuyer(Guid visitorProfileId, Guid buyerProfileId)
        {
            await _visitorManagementService.RemoveBuyerAsync(visitorProfileId, buyerProfileId);
            return Ok(ApiResult.Ok());
        }

        // -------------------- GET Commission Rules --------------------
        [HttpGet("{visitorProfileId:guid}/commission-rules")]
        [Authorize(Policy = Permissions.Visitors.View)]
        public async Task<IActionResult> GetCommissionRules(Guid visitorProfileId)
        {
            var rules = await _visitorManagementService.GetCommissionRulesAsync(visitorProfileId);
            return Ok(ApiResult.Ok(rules));
        }

        // -------------------- SET Commission Rule --------------------
        [HttpPost("{visitorProfileId:guid}/commission-rules")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> SetCommissionRule(Guid visitorProfileId, [FromBody] SetVisitorCommissionRuleDto dto)
        {
            var rule = await _visitorManagementService.SetCommissionRuleAsync(visitorProfileId, dto);
            return Ok(ApiResult.Ok(rule));
        }

        // -------------------- REMOVE Commission Rule --------------------
        [HttpDelete("{visitorProfileId:guid}/commission-rules/{commissionRuleCode}")]
        [Authorize(Policy = Permissions.Visitors.Manage)]
        public async Task<IActionResult> RemoveCommissionRule(Guid visitorProfileId, string commissionRuleCode)
        {
            await _visitorManagementService.RemoveCommissionRuleAsync(visitorProfileId, commissionRuleCode);
            return Ok(ApiResult.Ok());
        }
    }
}

