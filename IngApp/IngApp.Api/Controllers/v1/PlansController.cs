using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/plans")]
[Authorize] // TODO: باید Permission مناسب اضافه شود
public class PlansController : ControllerBase
{
    private readonly IPlanManagementService _planService;

    public PlansController(IPlanManagementService planService)
    {
        _planService = planService;
    }

    // GET: دریافت لیست Plan ها (با Pagination)
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _planService.GetPagedPlansAsync(page, pageSize);
        return Ok(ApiResult.Ok(result));
    }

    // GET: دریافت تمام Plan ها
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var plans = await _planService.GetAllPlansAsync();
        return Ok(ApiResult.Ok(plans));
    }

    // GET: دریافت Plan بر اساس Id
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _planService.GetPlanByIdAsync(id);
        
        if (plan == null)
            return NotFound(ApiResult.Fail("پلن مورد نظر یافت نشد."));

        return Ok(ApiResult.Ok(plan));
    }

    // POST: ایجاد Plan جدید
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanDto dto)
    {
        var planId = await _planService.CreatePlanAsync(dto);
        return Ok(ApiResult.Ok(planId));
    }

    // PUT: به‌روزرسانی Plan
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanDto dto)
    {
        await _planService.UpdatePlanAsync(id, dto);
        return Ok(ApiResult.Ok("پلن با موفقیت به‌روزرسانی شد."));
    }

    // PUT: تغییر وضعیت فعال/غیرفعال
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ToggleStatus(int id, [FromBody] TogglePlanStatusRequest request)
    {
        await _planService.TogglePlanStatusAsync(id, request.IsActive);
        return Ok(ApiResult.Ok("وضعیت پلن با موفقیت تغییر کرد."));
    }

    // DELETE: حذف Plan
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _planService.DeletePlanAsync(id);
        return Ok(ApiResult.Ok("پلن با موفقیت حذف شد."));
    }
}

public class TogglePlanStatusRequest
{
    public bool IsActive { get; set; }
}











